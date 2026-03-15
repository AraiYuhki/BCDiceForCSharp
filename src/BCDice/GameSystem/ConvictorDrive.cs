using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// コンヴィクター・ドライブ
    /// </summary>
    public sealed class ConvictorDrive : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ConvictorDrive Instance = new ConvictorDrive();

        /// <inheritdoc/>
        public override string Id => "ConvictorDrive";

        /// <inheritdoc/>
        public override string Name => "コンヴィクター・ドライブ";

        /// <inheritdoc/>
        public override string SortKey => "こんういくたあとらいふ";

        /// <inheritdoc/>
        public override int SidesImplicitD => 10;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        xCD@z>=y: x個の10面ダイスで目標値y（省略時5）、クリティカルラインz（省略時10）の判定を行う。
        SLT: 技能レベル表を振る
        DCT: 遅延イベント表を振る
        ";

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>
        {
            {
                "SLT", new SimpleTable(
                    "技能ランク表",
                    "2D10",
                    new[]
                    {
                        "ランク外",
                        "E-",
                        "E",
                        "E+",
                        "D-",
                        "D",
                        "D+",
                        "C-",
                        "C",
                        "C+",
                        "B-",
                        "B",
                        "B+",
                        "A-",
                        "A",
                        "A+",
                        "S-",
                        "S",
                        "S+",
                    }
                )
            },
            {
                "DCT", new SimpleTable(
                    "遅延イベント表",
                    "1D10",
                    new[]
                    {
                        "状況遅延Ⅰ（全員の初期リソースを-1する）",
                        "状況遅延Ⅱ（全員の初期リソースを-1する）",
                        "状況遅延Ⅲ（全員の初期リソースを-2する）",
                        "武装を許すⅠ（ボスの攻撃ダイスを+1dする）",
                        "武装を許すⅡ（脅威度4以下のエネミーの攻撃ダイスを2体まで+1dする）",
                        "武装を許すⅢ（脅威度3以下のエネミーの攻撃ダイスを1体+2dする）",
                        "緊急出撃Ⅰ（ランダムなPCのHPを-1する）",
                        "緊急出撃Ⅱ（ランダムなPCのHPを-1する）",
                        "緊急出撃Ⅲ（ランダムなPC2人のHPを-1する）",
                        "絶望（ダイスを二度振り、二つ適用する）",
                    }
                )
            }
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollCommand(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)CD(?:@(\d+))?(?:>=(\d+))?$");
            if (!m.Success)
            {
                return null;
            }

            var prefixNumber = int.Parse(m.Groups[1].Value);
            var critical = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 10;
            var targetNum = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 5;

            critical = critical < targetNum ? targetNum : critical > 10 ? 10 : critical;

            var diceList = randomizer.RollBarabara(prefixNumber, 10);
            var succeedNum = diceList.Count(x => x >= targetNum);
            var criticalNum = diceList.Count(x => x >= critical);

            var parts = new List<string>
            {
                $"({prefixNumber}CD@{critical}>={targetNum})",
                string.Join(",", diceList),
            };

            if (criticalNum > 0)
            {
                parts.Add($"クリティカル数{criticalNum}");
            }

            parts.Add($"成功数{succeedNum + criticalNum}");

            var text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetCondition(succeedNum > 0)
                .SetCritical(criticalNum > 0)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
