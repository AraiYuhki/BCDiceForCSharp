using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Fate Core System
    /// </summary>
    public sealed class FateCoreSystem : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly FateCoreSystem Instance = new FateCoreSystem();

        /// <inheritdoc/>
        public override string Id => "FateCoreSystem";

        /// <inheritdoc/>
        public override string Name => "Fate Core System";

        /// <inheritdoc/>
        public override string SortKey => "ふえいとこあしすてむ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
■ ファッジダイスによる判定 (xDF+y>=t)
  ファッジダイスをx個ダイスロールし、結果を判定します。
  x: ダイス数(省略時4)
  y: 修正値（省略可）
  t: 目標値（省略可）
  例）4DF, 4DF>=3, 4DF+1>=3, DF, DF>=3, DF+1>=3
";

        private static readonly Regex DfCommand = new Regex(
            @"^(\d*)DF([+\-]\d+)?(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private FateCoreSystem() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollDf(command, randomizer);
        }

        private Result? RollDf(string command, IRandomizer randomizer)
        {
            var match = DfCommand.Match(command.ToUpperInvariant());
            if (!match.Success)
            {
                return null;
            }

            // ダイス数（省略時は4）
            int diceCount = 4;
            if (!string.IsNullOrEmpty(match.Groups[1].Value))
            {
                diceCount = int.Parse(match.Groups[1].Value);
            }

            // 修正値（省略時は0）
            int modifier = 0;
            if (match.Groups[2].Success)
            {
                modifier = int.Parse(match.Groups[2].Value);
            }

            // 目標値（省略可）
            int? targetNumber = null;
            if (match.Groups[4].Success)
            {
                targetNumber = int.Parse(match.Groups[4].Value);
            }

            // Fateダイスを振る（1-3を-1,0,+1に変換）
            var diceList = new List<int>();
            for (int i = 0; i < diceCount; i++)
            {
                int roll = randomizer.RollOnce(3) - 2; // 1->-1, 2->0, 3->+1
                diceList.Add(roll);
            }

            int diceTotal = 0;
            foreach (int d in diceList)
            {
                diceTotal += d;
            }
            int total = diceTotal + modifier;

            // ダイス表示を作成
            var fateDiceDisplay = new StringBuilder();
            foreach (int d in diceList)
            {
                if (d == 0)
                {
                    fateDiceDisplay.Append("[ ]");
                }
                else if (d > 0)
                {
                    fateDiceDisplay.Append("[+]");
                }
                else
                {
                    fateDiceDisplay.Append("[-]");
                }
            }

            // 修正値表示
            string modifierDisplay = "";
            if (modifier > 0)
            {
                modifierDisplay = $"+{modifier}";
            }
            else if (modifier < 0)
            {
                modifierDisplay = modifier.ToString();
            }

            // コマンド表示
            string commandDisplay = $"{diceCount}DF";
            if (modifier != 0)
            {
                commandDisplay += modifierDisplay;
            }
            if (targetNumber.HasValue)
            {
                commandDisplay += $">={targetNumber.Value}";
            }

            // 結果を構築
            var sequence = new List<string>
            {
                $"({commandDisplay})",
                $"{fateDiceDisplay}{modifierDisplay}",
                ResultLadder(total)
            };

            // 成功判定
            bool isSuccess = false;
            bool isFailure = false;
            bool isCritical = false;

            if (targetNumber.HasValue)
            {
                var (outcomeText, success, failure, critical) = Outcome(total, targetNumber.Value);
                sequence.Add(outcomeText);
                isSuccess = success;
                isFailure = failure;
                isCritical = critical;
            }

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string ResultLadder(int total)
        {
            int clamped = Math.Max(-2, Math.Min(8, total));
            string ladder = clamped switch
            {
                8 => "Legendary",
                7 => "Epic",
                6 => "Fantastic",
                5 => "Superb",
                4 => "Great",
                3 => "Good",
                2 => "Fair",
                1 => "Average",
                0 => "Mediocre",
                -1 => "Poor",
                _ => "Terrible"
            };

            string sign = total >= 0 ? "+" : "";
            return $"{ladder}({sign}{total})";
        }

        private static (string Text, bool IsSuccess, bool IsFailure, bool IsCritical) Outcome(int total, int target)
        {
            if (total >= target + 3)
            {
                return ("Succeed with Style", true, false, true);
            }
            else if (total == target + 1)
            {
                return ("Succeed(+1)", true, false, false);
            }
            else if (total == target)
            {
                return ("Tie(+0)", true, false, false);
            }
            else if (total >= target)
            {
                return ("Succeed", true, false, false);
            }
            else
            {
                return ("Fail", false, true, false);
            }
        }
    }
}
