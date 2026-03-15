using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// セラフィザイン
    /// </summary>
    public sealed class TherapieSein : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TherapieSein Instance = new TherapieSein();

        private static readonly Regex TsOpRegex = new Regex(
            @"(TS|OP)(\d+)?(([+-]\d+)*)(@(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "TherapieSein";

        /// <inheritdoc/>
        public override string Name => "セラフィザイン";

        /// <inheritdoc/>
        public override string SortKey => "せらふいさいん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・一般判定：TS[n][±m][@t]　　[]内のコマンドは省略可能。クリティカル無。
        ・戦闘判定：OP[n][±m][@t]　　[]内のコマンドは省略可能。クリティカル有。

        「n」で能力値修正などを指定。
        「±m」で達成値への修正値を追加指定。+5+1-3のように、複数指定も可能です。
        「@t」で目標値を指定。省略時は達成値のみ表示、指定時は判定の正否を追加表示。

        【書式例】
        ・TS → ダイスの合計値を達成値として表示。
        ・TS4 → ダイス合計+4を達成値表示。
        ・TS4-1 → ダイス合計+4-1（計+3）を達成値表示。
        ・TS2+1@10 → ダイス合計+2+1（計+3）の達成値と、判定の成否を表示。
        ・OP4+3+1 → ダイス合計+4+3+1（計+8）を達成値＆クリティカル表示。
        ・OP3@12 → ダイス合計+3の達成値＆クリティカル、判定の成否を表示。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = TsOpRegex.Match(command.ToUpper());
            if (!m.Success)
            {
                return null;
            }

            var hasCritical = m.Groups[1].Value == "OP";
            var target = m.Groups[6].Success && !string.IsNullOrEmpty(m.Groups[6].Value) ? int.Parse(m.Groups[6].Value) : 0;
            var modify = m.Groups[2].Success && !string.IsNullOrEmpty(m.Groups[2].Value) ? int.Parse(m.Groups[2].Value) : 0;
            var modifyAddString = m.Groups[3].Value;

            // Parse additional modifiers like +5+1-3
            var modifyMatches = Regex.Matches(modifyAddString, @"[+-]\d+");
            foreach (Match mm in modifyMatches)
            {
                modify += int.Parse(mm.Value);
            }

            return CheckRoll(hasCritical, modify, target, randomizer);
        }

        private Result? CheckRoll(bool hasCritical, int modify, int target, IRandomizer randomizer)
        {
            var diceList = randomizer.RollBarabara(2, 6);
            var dice = diceList.Sum();
            var diceText = string.Join(",", diceList);
            var successValue = dice + modify;

            var modifyText = GetValueText(modify);
            var targetText = (target == 0) ? "" : $">={target}";

            var result = $"(2D6{modifyText}{targetText})";
            result += $" ＞ {dice}({diceText}){modifyText}";

            if (hasCritical && dice == 12)
            {
                result += " ＞ クリティカル！";
                return Result.CreateBuilder(result)
                    .SetCritical(true)
                    .SetSuccess(true)
                    .Build();
            }

            result += $" ＞ {successValue}{targetText}";

            if (target == 0)
            {
                return Result.CreateBuilder(result).Build();
            }

            if (successValue >= target)
            {
                result += " ＞ 【成功】";
                return Result.CreateBuilder(result)
                    .SetSuccess(true)
                    .Build();
            }
            else
            {
                result += " ＞ 【失敗】";
                return Result.CreateBuilder(result)
                    .SetFailure(true)
                    .Build();
            }
        }

        private string GetValueText(int value)
        {
            if (value == 0) return "";
            if (value < 0) return value.ToString();
            return $"+{value}";
        }
    }
}
