using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// モノトーンミュージアムRPG
    /// nD6+m>=t[c,f] 形式の判定コマンドを実装。
    /// </summary>
    public sealed class MonotoneMuseum : GameSystemBase
    {
        public static readonly MonotoneMuseum Instance = new MonotoneMuseum();

        private static readonly Regex CheckRollRegex = new Regex(
            @"^(\d+)D6([+\-\d]*)>=(\?|\d+)(\[(\d+)?(,(\d+))?\])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "MonotoneMuseum";
        public override string Name => "モノトーンミュージアムRPG";
        public override string SortKey => "ものとおんみゆうしあむRPG";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"・判定
　・通常判定　　　　　　2D6+m>=t[c,f]
　　修正値m,目標値t,クリティカル値c,ファンブル値fで判定ロールを行います。
　　クリティカル値、ファンブル値は省略可能です。([]ごと省略できます)
　　自動成功、自動失敗、成功、失敗を自動表示します。
・D66ダイスあり
";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckRoll(command, randomizer);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = CheckRollRegex.Match(command);
            if (!m.Success) return null;

            int diceCount = int.Parse(m.Groups[1].Value);
            int modifyNumber = 0;
            if (!string.IsNullOrEmpty(m.Groups[2].Value))
            {
                modifyNumber = EvalModifier(m.Groups[2].Value);
            }

            bool isTargetUnknown = m.Groups[3].Value == "?";
            int target = isTargetUnknown ? 0 : int.Parse(m.Groups[3].Value);
            int critical = (m.Groups[5].Success && !string.IsNullOrEmpty(m.Groups[5].Value))
                ? int.Parse(m.Groups[5].Value) : 12;
            int fumble = (m.Groups[7].Success && !string.IsNullOrEmpty(m.Groups[7].Value))
                ? int.Parse(m.Groups[7].Value) : 2;

            var diceList = randomizer.RollBarabara(diceCount, 6);
            Array.Sort(diceList);
            int diceValue = diceList.Sum();
            string diceStr = string.Join(",", diceList);
            int total = diceValue + modifyNumber;

            string? resultText;
            bool isSuccess = false;
            bool isFailure = false;
            bool isCritical = false;
            bool isFumble = false;

            if (diceValue <= fumble)
            {
                resultText = "自動失敗";
                isFumble = true;
                isFailure = true;
            }
            else if (diceValue >= critical)
            {
                resultText = "自動成功";
                isCritical = true;
                isSuccess = true;
            }
            else if (isTargetUnknown)
            {
                resultText = null;
                isSuccess = true;
            }
            else if (total >= target)
            {
                resultText = "成功";
                isSuccess = true;
            }
            else
            {
                resultText = "失敗";
                isFailure = true;
            }

            string modStr = FormatModifier(modifyNumber);
            var parts = new List<string>
            {
                $"({command})",
                $"{diceValue}[{diceStr}]{modStr}",
                total.ToString()
            };

            if (resultText != null)
            {
                parts.Add(resultText);
            }

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            int result = 0;
            foreach (Match m in Regex.Matches(expr, @"[+\-]\d+"))
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        private static string FormatModifier(int value)
        {
            if (value == 0) return "";
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}