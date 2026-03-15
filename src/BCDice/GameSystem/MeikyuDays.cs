using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 迷宮デイズ
    /// </summary>
    public sealed class MeikyuDays : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MeikyuDays Instance = new MeikyuDays();

        /// <inheritdoc/>
        public override string Id => "MeikyuDays";

        /// <inheritdoc/>
        public override string Name => "迷宮デイズ";

        /// <inheritdoc/>
        public override string SortKey => "めいきゆうていす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　(nMD+m)
        　n個のD6を振って大きい物二つだけみて達成値を算出します。修正mも可能です。
        　絶対成功と絶対失敗も自動判定します。
        ・各種表
        　・散策表　　　　　　DRT
        　・交渉表　　　　　　DNT
        　・休憩表　　　　　　DBT
        　・ハプニング表　　　DHT
        　・カーネル停止表　　KST
        　・痛打表　CAT／戦闘ファンブル表　CFT／致命傷表　FWT
        　・おたから表１／２／３／４　T1T/T2T/T3T/T4T
        　・相場表　　　　　　MPT
        　・登場表　　　　　　APT
        　・因縁表　DCT／怪物因縁表　MCT／PC因縁表　PCT／ラブ因縁表　LCT
        ・D66ダイスあり
        ";

        private static readonly Regex MdRegex = new Regex(
            @"(^|\s)S?((\d+)[rR]6([+\-\d]*)(([>=]+)(\d+))?)(\s|$)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckRoll(command, randomizer);
        }

        /// <summary>
        /// Ruby の replace_text 相当。nMD6 / nMD → nR6 に変換。
        /// </summary>
        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"(\d+)MD6", m => $"{m.Groups[1].Value}R6", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"(\d+)MD", m => $"{m.Groups[1].Value}R6", RegexOptions.IgnoreCase);
            return str;
        }

        /// <summary>
        /// 2D6の判定結果を返す (Ruby の result_2d6 相当)
        /// </summary>
        private Result? Result2d6(int total, int diceTotal, List<int> diceList, CompareOperator? cmpOp, int target)
        {
            if (cmpOp != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }

            if (diceTotal <= 2)
            {
                return Result.CreateBuilder("絶対失敗").SetFumble(true).SetFailure(true).Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder("絶対成功").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total >= target)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var str = ReplaceText(command);

            var m = MdRegex.Match(str);
            if (!m.Success)
            {
                return null;
            }

            str = m.Groups[2].Value;
            var diceC = int.Parse(m.Groups[3].Value);
            var bonus = 0;
            var signOfInequality = "";
            var diff = 0;

            var bonusText = m.Groups[4].Value;
            if (!string.IsNullOrEmpty(bonusText))
            {
                bonus = ArithmeticEvaluator.Eval(bonusText, RoundType.Floor) ?? 0;
            }

            if (m.Groups[6].Success && !string.IsNullOrEmpty(m.Groups[6].Value))
            {
                signOfInequality = m.Groups[6].Value;
            }
            if (m.Groups[7].Success && !string.IsNullOrEmpty(m.Groups[7].Value))
            {
                diff = int.Parse(m.Groups[7].Value);
            }

            var diceNum = randomizer.RollBarabara(diceC, 6).OrderBy(x => x).ToList();
            var diceStr = string.Join(",", diceNum);

            var diceNow = diceNum[diceC - 2] + diceNum[diceC - 1];
            var totalN = diceNow + bonus;

            diceStr = $"[{diceStr}]";

            var output = $"{diceNow}{diceStr}";

            if (bonus > 0)
            {
                output += $"+{bonus}";
            }
            else if (bonus < 0)
            {
                output += bonus.ToString();
            }

            if (Regex.IsMatch(output, @"[^\d\[\]]+"))
            {
                output = $"({str}) ＞ {output} ＞ {totalN}";
            }
            else
            {
                output = $"({str}) ＞ {totalN}";
            }

            if (signOfInequality != "")
            {
                var cmpOp = Command.Normalize.ComparisonOperator(signOfInequality);
                var result = Result2d6(totalN, diceNow, diceNum, cmpOp, diff);
                if (result != null)
                {
                    output += $" ＞ {result.Text}";
                }
            }

            return Result.CreateBuilder(output).Build();
        }
    }
}
