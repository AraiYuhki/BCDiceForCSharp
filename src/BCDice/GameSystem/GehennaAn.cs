using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゲヘナ・アナスタシス
    /// </summary>
    public sealed class GehennaAn : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly GehennaAn Instance = new GehennaAn();

        /// <inheritdoc/>
        public override string Id => "GehennaAn";

        /// <inheritdoc/>
        public override string Name => "ゲヘナ・アナスタシス";

        /// <inheritdoc/>
        public override string SortKey => "けへなあなすたしす";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        戦闘判定と通常判定に対応。幸運の助け、連撃増加値(戦闘判定)、闘技チット(戦闘判定)を自動表示します。
        ・戦闘判定　(nGAt+m)
        　ダイス数n、目標値t、修正値mで戦闘判定を行います。
        　幸運の助け、連撃増加値、闘技チットを自動処理します。
        ・通常判定　(nGt+m)
        　ダイス数n、目標値t、修正値mで通常判定を行います。
        　幸運の助けを自動処理します。(連撃増加値、闘技チットを表示抑制します)
        ";

        private static readonly Regex CommandReg = new Regex(
            @"(^|\s)S?((\d+)[rR]6([+\-\d]+)?([>=]+(\d+))(\[(\d)\]))(\s|$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string replaced = ReplaceText(command);

            var m = CommandReg.Match(replaced);
            if (!m.Success)
                return null;

            string cmdStr = m.Groups[2].Value;
            int diceCount = int.Parse(m.Groups[3].Value);
            string? modText = m.Groups[4].Success ? m.Groups[4].Value : null;
            int diff = int.Parse(m.Groups[6].Value);
            int mode = int.Parse(m.Groups[8].Value);

            int mod = modText != null ? (ArithmeticEvaluator.Eval(modText, RoundType.Floor) ?? 0) : 0;

            var diceArray = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            string diceText = string.Join(",", diceArray);

            int dice1st = -1;
            bool isLuck = true;
            int diceValue = 0;

            // 幸運の助けチェック
            foreach (int i in diceArray)
            {
                if (dice1st >= 0)
                {
                    if (dice1st != i || i < diff)
                    {
                        isLuck = false;
                    }
                }
                else
                {
                    dice1st = i;
                }

                if (i >= diff)
                {
                    diceValue += 1;
                }
            }

            if (isLuck && diceCount > 1)
            {
                diceValue *= 2;
            }

            string output = diceValue + "[" + diceText + "]";
            int success = diceValue + mod;
            if (success < 0) success = 0;

            int failed = diceCount - diceValue;
            if (failed < 0) failed = 0;

            if (mod > 0)
                output += "+" + mod;
            else if (mod < 0)
                output += mod.ToString();

            // If output has non-numeric characters beyond the brackets, show full result
            if (Regex.IsMatch(output, @"[^\d\[\]]+"))
                output = "(" + cmdStr + ") ＞ " + output + " ＞ 成功" + success + "、失敗" + failed;
            else
                output = "(" + cmdStr + ") ＞ " + output;

            // 連撃増加値と闘技チット
            output += GetAnastasisBonusText(mode, success);

            return Result.Success(output);
        }

        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"(\d+)GA(\d+)([+-][+\-\d]+)", m => $"{m.Groups[1].Value}R6{m.Groups[3].Value}>={m.Groups[2].Value}[1]");
            str = Regex.Replace(str, @"(\d+)GA(\d+)", m => $"{m.Groups[1].Value}R6>={m.Groups[2].Value}[1]");
            str = Regex.Replace(str, @"(\d+)G(\d+)([+-][+\-\d]+)", m => $"{m.Groups[1].Value}R6{m.Groups[3].Value}>={m.Groups[2].Value}[0]");
            str = Regex.Replace(str, @"(\d+)G(\d+)", m => $"{m.Groups[1].Value}R6>={m.Groups[2].Value}[0]");
            return str;
        }

        private string GetAnastasisBonusText(int mode, int success)
        {
            if (mode == 0)
                return "";

            int maBonus = (success - 1) / 2;
            if (maBonus > 7) maBonus = 7;

            string bonusStr = "";
            if (maBonus > 0)
                bonusStr += "連撃[+" + maBonus + "]/";
            bonusStr += "闘技[" + GetTougiBonus(success) + "]";
            return " ＞ " + bonusStr;
        }

        private static string GetTougiBonus(int success)
        {
            var table = new (int Max, string Value)[]
            {
                (6, "1"),
                (13, "2"),
                (18, "3"),
                (22, "4"),
                (99, "5"),
            };

            foreach (var (max, value) in table)
            {
                if (success <= max)
                    return value;
            }
            return "5";
        }
    }
}
