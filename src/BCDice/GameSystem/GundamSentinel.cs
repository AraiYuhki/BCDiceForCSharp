using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガンダム・センチネルRPG
    /// </summary>
    public sealed class GundamSentinel : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly GundamSentinel Instance = new GundamSentinel();

        private static readonly Regex BasicBattleRegex = new Regex(
            @"^BB(M)?([-+][-+\d]+)?(>([-+\d]+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GeneralSkillRegex = new Regex(
            @"^GS([-+][-+\d]+)?(>([-+\d]+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "GundamSentinel";

        /// <inheritdoc/>
        public override string Name => "ガンダム・センチネルRPG";

        /// <inheritdoc/>
        public override string SortKey => "かんたむせんちねる";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・基本戦闘(BB, BBM)
        　BB[+修正][>回避値]で基本戦闘を判定します。回避値を指定すると、命中・回避も表示します。
        　BBM[+修正][>回避値]でモブ用の基本戦闘を判定します。クリティカルを判定します。回避値を指定すると、命中・回避も表示します。

        　例）BB BBM BB+5>14 BBM+5>15

        ・一般技能(GS)
        　GS[+修正][>目標値]で一般技能を判定します。目標値を指定しない場合は、目標値10で判定します。

        　例）GS GS+5 GS+5>10


        ・各種表
        　敵MSクリティカルヒットチャート　(ECHC)
        　PC用脱出判定チャート　　　　　　(PEJC[+m] m:修正)
        　艦船追加ダメージ決定チャート　　(ASDC)
        　対空砲結果チャート　　　　　　　(AARC[+m]=t m:修正, t:対空防御力)
        　リハビリ判定チャート　　　　　　(RTJC[+m] m:修正)
        　二次被害判定チャート　　　　　　(SDDC)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollBasicBattle(command, randomizer)
                ?? RollGeneralSkill(command, randomizer)
                ?? RollAntiAircraftGunResultChart(command, randomizer)
                ?? RollEscapeChart(command, randomizer)
                ?? RollRehabilitationChart(command, randomizer)
                ?? RollTables(command, TABLES);
        }

        private Result? RollBasicBattle(string command, IRandomizer randomizer)
        {
            var m = BasicBattleRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var mob = m.Groups[1].Success;
            var haveModify = m.Groups[2].Success;
            var modify = haveModify ? EvalSimpleArithmetic(m.Groups[2].Value) : 0;
            var haveAvoid = m.Groups[4].Success;
            var avoid = haveAvoid ? EvalSimpleArithmetic(m.Groups[4].Value) : 0;

            var d60 = randomizer.RollOnce(6);
            var d06 = randomizer.RollOnce(6);
            var totalD = d60 * 10 + d06;

            // Ruby: d60 += (d06 + modify - 1).div(6)
            // Ruby: d06 = (d06 + modify - 1).modulo(6) + 1
            var combined = d06 + modify - 1;
            d60 += (int)Math.Floor((double)combined / 6);
            d06 = ((combined % 6) + 6) % 6 + 1;
            var total = d60 * 10 + d06;
            if (total < 11)
            {
                total = 11;
            }

            var success = false;
            var failure = false;
            var critical = false;

            string? modifyLabel = null;
            if (haveModify)
            {
                modifyLabel = modify >= 0 ? $"{totalD}+{modify}" : $"{totalD}{modify}";
            }

            string? criticalLabel = null;
            if (mob && total >= 66)
            {
                criticalLabel = "クリティカル";
                critical = true;
            }

            string? resultText = null;
            if (haveAvoid)
            {
                if (total > avoid)
                {
                    resultText = "命中(+" + CountSuccess(total, avoid).ToString() + ")";
                    success = true;
                }
                else
                {
                    resultText = "回避";
                    failure = true;
                }
            }

            var sequence = new List<string?> { $"({command})", modifyLabel, total.ToString(), resultText, criticalLabel }
                .Where(x => x != null)
                .ToList();

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(success)
                .SetFailure(failure)
                .SetCritical(critical)
                .Build();
        }

        private int CountSuccess(int dice, int avoid)
        {
            var d60 = dice / 10;
            var d06 = dice % 10;
            var a60 = avoid / 10;
            var a06 = avoid % 10;
            return (d60 * 6 + d06) - (a60 * 6 + a06);
        }

        private Result? RollGeneralSkill(string command, IRandomizer randomizer)
        {
            var m = GeneralSkillRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var haveModify = m.Groups[1].Success;
            var modify = haveModify ? EvalSimpleArithmetic(m.Groups[1].Value) : 0;
            int target;
            if (m.Groups[3].Success)
            {
                target = EvalSimpleArithmetic(m.Groups[3].Value);
            }
            else
            {
                target = 10;
            }

            var success = false;
            var failure = false;

            var dice = randomizer.RollSum(2, 6);

            string? modifyLabel = null;
            if (haveModify)
            {
                modifyLabel = modify >= 0 ? $"{dice}+{modify}" : $"{dice}{modify}";
            }

            var total = dice + modify;
            string resultText;
            if (total > target)
            {
                resultText = "成功";
                success = true;
            }
            else
            {
                resultText = "失敗";
                failure = true;
            }

            var sequence = new List<string?> { $"({command})", modifyLabel, total.ToString(), resultText }
                .Where(x => x != null)
                .ToList();

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(success)
                .SetFailure(failure)
                .Build();
        }

        // 対空砲結果チャート
        private static readonly object[][] GUN_RESULT_CHART = new object[][]
        {
            new object[] { "D", "H", "H", "H", 10, 8, 6, 5, 4, 2, 1, "-", "-" },
            new object[] { "D", "H", "H", "H", 12, 10, 9, 8, 6, 5, 3, 2, "-" },
            new object[] { "D", "D", "H", "H", "H", 12, 10, 9, 7, 6, 4, 3, 1 },
            new object[] { "D", "D", "H", "H", "H", 14, 13, 12, 10, 8, 6, 5, 3 },
            new object[] { "D", "D", "D", "H", "H", "H", 14, 13, 11, 9, 7, 6, 4 },
            new object[] { "D", "D", "D", "H", "H", "H", "H", 16, 14, 12, 11, 8, 6 },
        };

        private Result? RollAntiAircraftGunResultChart(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^AARC([-+]\d+)?=(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modifyNumber = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var targetNumber = Clamp(int.Parse(m.Groups[2].Value), 1, 6);
            var dice = randomizer.RollSum(2, 6);
            var total = Clamp(dice + modifyNumber, 1, 13);

            string cmd;
            if (modifyNumber != 0)
            {
                var modSign = modifyNumber >= 0 ? $"+{modifyNumber}" : modifyNumber.ToString();
                cmd = $"({dice}{modSign}={total})";
            }
            else
            {
                cmd = total.ToString();
            }

            var result = GUN_RESULT_CHART[targetNumber - 1][total - 1];
            var isInteger = result is int;

            return Result.CreateBuilder($"対空砲結果チャート({cmd}vs{targetNumber}).Build() ＞ 結果「{result}」")
                .SetCondition(isInteger)
                .Build();
        }

        // PC用脱出判定チャート
        private static readonly string[] ESCAPE_CHART = new string[]
        {
            "*",
            "*",
            "無傷で脱出",
            "無傷で脱出",
            "無傷で脱出",
            "軽傷で脱出「１Ｄ６ダメージ。」",
            "中傷で脱出「２Ｄ６ダメージ。」",
            "重傷で脱出「３Ｄ６ダメージ。」",
            "重体で脱出「１Ｄ３の耐久力が残る。」",
            "戦死「二階級特進。」",
            "戦死「二階級特進。」",
            "戦死「二階級特進。」",
            "戦死「二階級特進。」",
        };

        private Result? RollEscapeChart(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^PEJC([-+]\d+)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modifyNumber = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var dice = randomizer.RollSum(2, 6);
            var total = Clamp(dice + modifyNumber, 2, 12);

            string cmd;
            if (modifyNumber != 0)
            {
                var modSign = modifyNumber >= 0 ? $"+{modifyNumber}" : modifyNumber.ToString();
                cmd = $"{dice}{modSign}={total}";
            }
            else
            {
                cmd = total.ToString();
            }

            var result = ESCAPE_CHART[total];
            return Result.CreateBuilder($"PC用脱出判定チャート({cmd}).Build() ＞ {result}").Build();
        }

        // リハビリ判定チャート
        private static readonly string[] REHABILITATION_CHART = new string[]
        {
            "*",
            "*",
            "なし",
            "１ヶ月",
            "２ヶ月",
            "３ヶ月",
            "４ヶ月",
            "５ヶ月",
            "６ヶ月",
            "１０ヶ月",
            "１年",
            "１年６ヶ月",
            "１年と、もう一度このチャートで振った結果分を足した期間",
        };

        private Result? RollRehabilitationChart(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^RTJC([-+]\d+)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var modifyNumber = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var dice = randomizer.RollSum(2, 6);
            var total = Clamp(dice + modifyNumber, 2, 12);

            string cmd;
            if (modifyNumber != 0)
            {
                var modSign = modifyNumber >= 0 ? $"+{modifyNumber}" : modifyNumber.ToString();
                cmd = $"{dice}{modSign}={total}";
            }
            else
            {
                cmd = total.ToString();
            }

            var result = REHABILITATION_CHART[total];
            return Result.CreateBuilder($"リハビリ判定チャート({cmd}).Build() ＞ {result}").Build();
        }

        // TABLES は Ruby 版のテーブル定義に対応（省略 - 別途テーブル実装が必要）
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        /// <summary>
        /// .NET Standard 2.0 互換のClamp実装
        /// </summary>
        private static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        /// <summary>
        /// 簡易的な加減算の評価（"+3+5-2" のような文字列）
        /// </summary>
        private static int EvalSimpleArithmetic(string expr)
        {
            var result = 0;
            var current = "";
            foreach (var ch in expr)
            {
                if (ch == '+' || ch == '-')
                {
                    if (current.Length > 0)
                    {
                        result += int.Parse(current);
                    }
                    current = ch.ToString();
                }
                else
                {
                    current += ch;
                }
            }
            if (current.Length > 0)
            {
                result += int.Parse(current);
            }
            return result;
        }

    }
}
