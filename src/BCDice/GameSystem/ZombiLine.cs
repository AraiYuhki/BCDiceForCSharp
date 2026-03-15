using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゾンビライン
    /// </summary>
    public sealed class ZombiLine : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ZombiLine Instance = new ZombiLine();

        /// <inheritdoc/>
        public override string Id => "ZombiLine";

        /// <inheritdoc/>
        public override string Name => "ゾンビライン";

        /// <inheritdoc/>
        public override string SortKey => "そんひらいん";

        /// <inheritdoc/>
        public override int SidesImplicitD => 10;

        /// <inheritdoc/>
        public override string HelpMessage => @"
■ 判定 (xZL<=y)
　x：ダイス数(省略時は1)
　y：成功率

■ 各種表
　ストレス症状表 SST
　食材表 IT
";

        private static readonly Regex ZlRegex = new Regex(
            @"^(\d+)?ZL<=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer);
            // TODO: || RollTables(command, TABLES)
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = ZlRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
            var targetNum = int.Parse(m.Groups[2].Value);

            var diceList = randomizer.RollBarabara(diceCount, 100).OrderBy(x => x).ToList();
            var isSuccess = diceList.Any(i => i <= targetNum);
            var isCritical = diceList.Any(i => i <= 5);
            var isFumble = diceList.Any(i => i >= 96 && i > targetNum);

            if (isCritical && isFumble)
            {
                isCritical = false;
                isFumble = false;
            }

            string successMessage;
            if (isSuccess && isCritical)
            {
                successMessage = "成功(クリティカル)";
            }
            else if (isSuccess && isFumble)
            {
                successMessage = "成功(ファンブル)";
            }
            else if (isSuccess)
            {
                successMessage = "成功";
            }
            else if (isFumble)
            {
                successMessage = "失敗(ファンブル)";
            }
            else
            {
                successMessage = "失敗";
            }

            var sequence = new[]
            {
                $"({(m.Groups[1].Success ? m.Groups[1].Value : "1")}ZL<={targetNum})",
                $"[{string.Join(",", diceList)}]",
                successMessage
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }
    }
}
