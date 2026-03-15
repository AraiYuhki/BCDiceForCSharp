using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// マモノスクランブル
    /// </summary>
    public sealed class MamonoScramble : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MamonoScramble Instance = new MamonoScramble();

        /// <inheritdoc/>
        public override string Id => "MamonoScramble";

        /// <inheritdoc/>
        public override string Name => "マモノスクランブル";

        /// <inheritdoc/>
        public override string SortKey => "まものすくらんふる";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override int SidesImplicitD => 12;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 xMS<=t
        　[判定]を行う。成否と[マリョク]の上昇量を表示する。
        　x: ダイス数
        　t: 能力値（目標値）

        ・アクシデント表 ACC
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollAbility(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollAbility(string command, IRandomizer randomizer)
        {
            // xMS<=t: prefix number = dice count, no modifier, target with <=
            var m = Regex.Match(command, @"^(\d+)MS<=(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }
            var prefixNumber = int.Parse(m.Groups[1].Value);
            var targetNumber = int.Parse(m.Groups[2].Value);

            var dice_list = randomizer.RollBarabara(prefixNumber, 12).OrderBy(x => x).ToList();
            var count_success = dice_list.Count(value => value <= targetNumber);
            var count_one = dice_list.Count(x => x == 1);
            var is_critical = count_one > 0;
            var has_twelve = dice_list.Contains(12);

            var maryoku = (has_twelve && !is_critical) ? 0 : count_success + count_one;

            var sequence = new[]
            {
                $"({prefixNumber}MS<={targetNumber})",
                $"[{string.Join(",", dice_list)}]",
                count_success > 0 ? $"成功, [マリョク]が{maryoku}上がる" : "失敗"
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCondition(count_success > 0)
                .SetCritical(count_success > 0 && is_critical)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private static readonly Dictionary<string, SimpleTable> TABLES = new Dictionary<string, SimpleTable>
        {
            {
                "ACC", new SimpleTable(
                    "アクシデント表",
                    "1D12",
                    new[]
                    {
                        "思わぬ対立：[判定]で10〜12の出目を1個でも出した場合、【耐久値】を2点減らす。",
                        "都市の迷宮化：[判定]に【社会】を使用できない。",
                        "不穏な天気：特別な効果は発生しない。",
                        "突然の雷雨：エリアの[特性]に[雨]や[水たまり]などを足してもいい。",
                        "関係ない危機：[判定]に失敗したPCの【耐久値】を2点減らす。",
                        "からりと晴天：エリアの[特性]に[強い日光]や[日だまり]などを足してもいい。",
                        "謎のお祭り：[判定]で1〜3の出目を1個でも出した場合、【耐久値】を2点回復する。",
                        "すごい人ごみ：エリアの[特性]に[野次馬]や[観光客]などを足してもいい。",
                        "マリョク乱気流：[判定]に【異質】を使用できない。",
                        "魔術テロ事件：GMが1Dをロールする。出目が1〜3なら【身体】、出目が4〜6なら【異質】、出目が7〜9なら【社会】が[判定]で使えない。10〜12は何も起きない。",
                        "マリョク低気圧：[判定]に【身体】を使用できない。",
                        "平穏な時間：特別な効果は発生しない。",
                    }
                )
            }
        };

    }
}
