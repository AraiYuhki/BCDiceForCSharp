using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// カオスフレア
    /// CFコマンドによる判定と因縁表（FT）を実装。
    /// </summary>
    public sealed class ChaosFlare : GameSystemBase
    {
        public static readonly ChaosFlare Instance = new ChaosFlare();

        private static readonly Regex CfRegex = new Regex(
            @"^(\d*)CF((?:[+\-]\d+)+)?(?:@(\d+))?(?:#(\d+))?((?:[+\-]\d+)+)?(?:>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FateRegex = new Regex(
            @"^FT(\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "ChaosFlare";
        public override string Name => "カオスフレア";
        public override string SortKey => "かおすふれあ";

        public override string HelpMessage => @"判定
CF
  書式: [ダイスの数]CF[修正値][@クリティカル値][#ファンブル値][>=目標値]
    CF以外は全て省略可能
  例:
  - CF 2D6,クリティカル値12,ファンブル値2で判定
  - CF+10@10 修正値+10,クリティカル値10で判定
  - CF+10#3 修正値+10,ファンブル値3で判定
  - CF+10>=10 目標値を指定した場合、差分値も出力する
  - 3CF+10@10#3>=10 3D6での判定
  - CF@9#3+8>=10

2D6
  ファンブル値2で判定する。クリティカルの判定は行われない。
  目標値が設定された場合、差分値を出力する。
  - 2D6+4>=10

各種表
  FT: 因縁表
  FTx: 数値を指定すると因果表の値を出力する
  - FT -> 11から66の間でランダム決定
  - FT23 -> 23の項目を出力
  - FT0
  - FT7
";

        // 因果表データ
        // index 0: 腐れ縁(特殊)、1-6: D66テーブル、7: 任意(特殊)
        private static readonly string[][] FateTable = new[]
        {
            new[] { "腐れ縁" },
            new[] { "純愛", "親近感", "庇護" },
            new[] { "信頼", "感服", "共感" },
            new[] { "友情", "尊敬", "慕情" },
            new[] { "好敵手", "期待", "借り" },
            new[] { "興味", "憎悪", "悲しみ" },
            new[] { "恐怖", "執着", "利用" },
            new[] { "任意" },
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (command.StartsWith("FT", StringComparison.OrdinalIgnoreCase))
            {
                return RollFateTable(command, randomizer);
            }

            return CfRoll(command, randomizer);
        }

        private Result? RollFateTable(string command, IRandomizer randomizer)
        {
            var m = FateRegex.Match(command);
            if (!m.Success) return null;

            int dice1, dice2;

            if (m.Groups[1].Success)
            {
                int num = int.Parse(m.Groups[1].Value);

                // 特殊値: 0 と 7
                if (num == 0 || num == 7)
                {
                    return Result.CreateBuilder($"因果表({num}).Build() ＞ {FateTable[num][0]}")
                        .Build();
                }

                dice1 = num / 10;
                dice2 = num % 10;

                if (dice1 < 1 || dice1 > 6 || dice2 < 1 || dice2 > 6)
                {
                    return null;
                }
            }
            else
            {
                dice1 = randomizer.RollOnce(6);
                dice2 = randomizer.RollOnce(6);
            }

            int index1 = dice1;
            int index2 = (dice2 / 2) - 1;

            if (index2 < 0) index2 = FateTable[index1].Length - 1;
            if (index1 < 0 || index1 >= FateTable.Length) return null;
            if (index2 >= FateTable[index1].Length) index2 = FateTable[index1].Length - 1;

            string resultText = FateTable[index1][index2];

            return Result.CreateBuilder($"因果表({dice1}{dice2}).Build() ＞ {resultText}")
                .Build();
        }

        private Result? CfRoll(string command, IRandomizer randomizer)
        {
            var m = CfRegex.Match(command);
            if (!m.Success) return null;

            int times = string.IsNullOrEmpty(m.Groups[1].Value) ? 2 : int.Parse(m.Groups[1].Value);
            int modifier = EvalModifier(m.Groups[2].Value) + EvalModifier(m.Groups[5].Value);
            int critical = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 12;
            int fumble = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 2;
            int? targetNumber = m.Groups[6].Success ? int.Parse(m.Groups[6].Value) : (int?)null;

            if (times < 1) return null;

            var diceList = randomizer.RollBarabara(times, 6);
            int diceTotal = diceList.Sum();
            string diceListText = string.Join(",", diceList);

            bool isCritical = diceTotal >= critical;
            bool isFumble = diceTotal <= fumble;

            int total;
            if (isCritical) total = 30;
            else if (isFumble) total = -20;
            else total = diceTotal;

            total += modifier;

            string modStr = FormatModifier(modifier);
            string cmdStr = $"{(times != 2 ? times.ToString() : "")}CF{modStr}@{critical}#{fumble}";
            if (targetNumber.HasValue) cmdStr += $">={targetNumber}";

            var parts = new List<string>
            {
                $"({cmdStr})",
                $"{diceTotal}[{diceListText}]",
                total.ToString()
            };

            if (total < 0) parts.Add("0");
            if (isCritical) parts.Add("クリティカル");
            if (isFumble) parts.Add("ファンブル");
            if (targetNumber.HasValue)
            {
                int diff = total < 0 ? -targetNumber.Value : total - targetNumber.Value;
                parts.Add($"差分値 {diff}");
            }

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetSuccess(isCritical || (targetNumber.HasValue && total >= targetNumber.Value))
                .SetFailure(isFumble || (targetNumber.HasValue && total < targetNumber.Value))
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
