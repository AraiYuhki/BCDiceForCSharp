using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ネクロニカ
    /// </summary>
    public sealed class Nechronica : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Nechronica Instance = new Nechronica();

        private static readonly Regex NcRegex = new Regex(
            @"^S?(\d+)NC$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AttackRegex = new Regex(
            @"^S?NA(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Nechronica";

        /// <inheritdoc/>
        public override string Name => "ネクロニカ";

        /// <inheritdoc/>
        public override string SortKey => "ねくろにか";

        /// <inheritdoc/>
        public override string HelpMessage => @"
【ネクロニカ】

・判定コマンド（nNC）
　3NC    3個のD10で判定
　        6以上で成功、10はクリティカル（2成功）
　        全て1でファンブル

・攻撃判定（NAn）
　NA2    攻撃値2で攻撃判定
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // NC判定
            var ncResult = EvalNcCommand(command, randomizer);
            if (ncResult != null)
            {
                return ncResult;
            }

            // 攻撃判定
            var attackResult = EvalAttackCommand(command, randomizer);
            if (attackResult != null)
            {
                return attackResult;
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? EvalNcCommand(string command, IRandomizer randomizer)
        {
            var match = NcRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            int diceCount = int.Parse(match.Groups[1].Value);
            if (diceCount <= 0)
            {
                return null;
            }

            // ダイスを振る
            var rolls = randomizer.RollBarabara(diceCount, 10).ToList();

            // 成功数をカウント（6以上で1成功、10で2成功）
            int successCount = 0;
            int criticalCount = 0;
            foreach (int roll in rolls)
            {
                if (roll == 10)
                {
                    successCount += 2;
                    criticalCount++;
                }
                else if (roll >= 6)
                {
                    successCount++;
                }
            }

            // ファンブル判定（全て1）
            bool isFumble = rolls.All(r => r == 1);
            bool isCritical = criticalCount > 0;

            var sb = new StringBuilder();
            sb.Append($"({diceCount}NC)");
            sb.Append($" ＞ [{string.Join(",", rolls)}]");
            sb.Append($" ＞ 成功数{successCount}");

            if (isCritical)
            {
                sb.Append($"（クリティカル{criticalCount}個）");
            }
            if (isFumble)
            {
                sb.Append("【ファンブル】");
            }

            var builder = Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (isFumble)
            {
                builder.SetFumble(true).SetFailure(true);
            }
            else if (isCritical)
            {
                builder.SetCritical(true);
            }

            if (successCount > 0)
            {
                builder.SetSuccess(true);
            }

            return builder.Build();
        }

        private Result? EvalAttackCommand(string command, IRandomizer randomizer)
        {
            var match = AttackRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("NA", StringComparison.OrdinalIgnoreCase);

            int attackValue = int.Parse(match.Groups[1].Value);

            // 1D10を振る
            int roll = randomizer.RollOnce(10);
            int damage = roll + attackValue;

            var sb = new StringBuilder();
            sb.Append($"(NA{attackValue})");
            sb.Append($" ＞ [{roll}]+{attackValue}");
            sb.Append($" ＞ ダメージ{damage}");

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
