using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アマデウス
    /// </summary>
    public sealed class Amadeus : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Amadeus Instance = new Amadeus();

        /// <inheritdoc/>
        public override string Id => "Amadeus";

        /// <inheritdoc/>
        public override string Name => "アマデウス";

        /// <inheritdoc/>
        public override string SortKey => "あまてうす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定(Rx±y@z>=t)
        　能力値のダイスごとに成功・失敗の判定を行います。
        　x：能力ランク(S,A～D)　y：修正値（省略可）
        　z：スペシャル最低値（省略：6）　t：目標値（省略：4）
        　　例） RA　RB-1　RC>=5　RD+2　RS-1@5>=6
        　出力書式は
        　　(達成値)_(判定結果)[(出目)(対応するインガ)]
        　C,Dランクでは対応するインガは出力されません。
        　　出力例)　2_ファンブル！[1黒] / 3_失敗[3青]
        ・各種表
        　境遇表 ECT／関係表 RT／親心表 PRT／戦場表 BST／休憩表 BT／
        　ファンブル表 FT／致命傷表 FWT／戦果表 BRT／ランダムアイテム表 RIT／
        　損傷表 WT／悪夢表 NMT／目標表 TGT／制約表 CST／
        　ランダムギフト表 RGT／決戦戦果表 FBT／
        　店内雰囲気表 SAT／特殊メニュー表 SMT
        ・試練表（～VT）
        　ギリシャ神群 GCVT／ヤマト神群 YCVT／エジプト神群 ECVT／
        　クトゥルフ神群 CCVT／北欧神群 NCVT／中華神群 CHVT／
          ラストクロニクル神群 LCVT／ケルト神群 KCVT／ダンジョン DGVT／日常 DAVT
        ・挑戦テーマ表（～CT）
        　武勇 PRCT／技術 TCCT／頭脳 INCT／霊力 PSCT／愛 LVCT／日常 DACT
        ";

        // ランク毎のダイス数
        private static readonly Dictionary<string, int> CHECK_DICE_COUNT = new Dictionary<string, int>
        {
            { "S", 4 }, { "A", 3 }, { "B", 2 }, { "C", 1 }, { "D", 2 }
        };

        // インガテーブル（ダイス目1～6に対応）
        private static readonly string[] INGA_TABLE = new[]
        {
            "黒", "赤", "青", "緑", "白", "金"
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollAmadeus(command, randomizer);
        }

        private Result? RollAmadeus(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^R([A-DS])([+\-\d]*)(@(\d))?((>=?)([+\-\d]*))?(@(\d))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var rank = m.Groups[1].Value.ToUpperInvariant();
            var modifier = EvalModifier(m.Groups[2].Value);
            var cmpOp = !string.IsNullOrEmpty(m.Groups[6].Value) ? m.Groups[6].Value : ">=";
            var target = !string.IsNullOrEmpty(m.Groups[7].Value) ? EvalModifier(m.Groups[7].Value) : 4;
            int special;
            if (m.Groups[4].Success)
            {
                special = int.Parse(m.Groups[4].Value);
            }
            else if (m.Groups[9].Success)
            {
                special = int.Parse(m.Groups[9].Value);
            }
            else
            {
                special = 6;
            }

            if (!CHECK_DICE_COUNT.TryGetValue(rank, out int diceCount))
            {
                return null;
            }

            var diceList = randomizer.RollBarabara(diceCount, 6);
            var diceText = string.Join(",", diceList);
            var specialText = (special == 6) ? "" : $"@{special}";

            // Dランクは最小値のみ使用
            int[] usedDiceList;
            if (rank == "D")
            {
                usedDiceList = new[] { diceList.Min() };
            }
            else
            {
                usedDiceList = diceList;
            }

            var availableInga = usedDiceList.Length > 1;

            var success = false;
            var critical = false;
            var fumble = false;

            var results = usedDiceList.Select(dice =>
            {
                var total = dice + modifier;
                string result;

                if (dice == 1)
                {
                    fumble = true;
                    result = "ファンブル！";
                }
                else if (dice >= special)
                {
                    critical = true;
                    success = true;
                    result = "スペシャル";
                }
                else if (CompareOperator(total, cmpOp, target))
                {
                    success = true;
                    result = "成功";
                }
                else
                {
                    result = "失敗";
                }

                if (availableInga)
                {
                    var inga = INGA_TABLE[dice - 1];
                    return $"{total}_{result}[{dice}{inga}]";
                }
                else
                {
                    return $"{total}_{result}[{dice}]";
                }
            }).ToArray();

            var sequence = new[]
            {
                $"(R{rank}{FormatModifier(modifier)}{specialText}{cmpOp}{target})",
                $"[{diceText}]{FormatModifier(modifier)}",
                string.Join(" / ", results)
            };

            var builder = Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (success)
            {
                builder.SetSuccess(true);
                builder.SetCritical(critical);
            }
            else
            {
                builder.SetFailure(true);
                builder.SetFumble(fumble);
            }

            return builder.Build();
        }

        private static bool CompareOperator(int left, string op, int right) => op switch
        {
            ">=" => left >= right,
            "<=" => left <= right,
            ">" => left > right,
            "<" => left < right,
            "==" => left == right,
            "!=" => left != right,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr))
            {
                return 0;
            }

            // 簡易的な四則演算（+と-のみ）
            int result = 0;
            int sign = 1;
            int current = 0;
            bool hasDigit = false;

            foreach (char c in expr)
            {
                if (c >= '0' && c <= '9')
                {
                    current = current * 10 + (c - '0');
                    hasDigit = true;
                }
                else if (c == '+' || c == '-')
                {
                    if (hasDigit)
                    {
                        result += sign * current;
                        current = 0;
                        hasDigit = false;
                    }
                    sign = c == '+' ? 1 : -1;
                }
            }

            if (hasDigit)
            {
                result += sign * current;
            }

            return result;
        }

        private static string FormatModifier(int value)
        {
            if (value > 0) return $"+{value}";
            if (value < 0) return value.ToString();
            return "";
        }
    }
}
