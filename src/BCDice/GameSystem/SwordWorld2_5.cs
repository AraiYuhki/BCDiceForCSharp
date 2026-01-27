using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ソードワールド2.5
    /// </summary>
    public sealed class SwordWorld2_5 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SwordWorld2_5 Instance = new SwordWorld2_5();

        private static readonly Regex RatingRegex = new Regex(
            @"^S?K(\d+)(?:@(\d+))?(?:\+(\d+))?(?:\-(\d+))?(?:\$\+?(\d+))?(?:#(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?2D6?([<>=]+)(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "SwordWorld2.5";

        /// <inheritdoc/>
        public override string Name => "ソードワールド2.5";

        /// <inheritdoc/>
        public override string SortKey => "そおとわあると2.5";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【ソードワールド2.5】

・威力表コマンド（Kコマンド）
　K20      威力20で威力表を振る
　K20@10   威力20、クリティカル値10
　K20+5    威力20、ダメージ修正+5
　K20@10+5 威力20、クリティカル値10、修正+5
　K20$+2   威力20、回転時ダメージ+2
　K20#1    威力20、首切り刀（追加ダメージ）

・判定コマンド
　2D6>=10  目標値10で判定
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // 威力表コマンド
            var ratingResult = EvalRatingCommand(command, randomizer);
            if (ratingResult != null)
            {
                return ratingResult;
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? EvalRatingCommand(string command, IRandomizer randomizer)
        {
            var match = RatingRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("K", StringComparison.OrdinalIgnoreCase);

            int power = int.Parse(match.Groups[1].Value);
            int critical = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 10;
            int modifier = 0;
            if (match.Groups[3].Success) modifier += int.Parse(match.Groups[3].Value);
            if (match.Groups[4].Success) modifier -= int.Parse(match.Groups[4].Value);
            int criticalBonus = match.Groups[5].Success ? int.Parse(match.Groups[5].Value) : 0;
            int extraDamage = match.Groups[6].Success ? int.Parse(match.Groups[6].Value) : 0;

            // クリティカル値の範囲制限
            critical = Math.Max(3, Math.Min(13, critical));

            var rolls = new List<int>();
            var damages = new List<int>();
            int totalDamage = 0;
            int rollCount = 0;
            const int maxRolls = 100;

            do
            {
                if (rollCount >= maxRolls)
                {
                    break;
                }

                int roll = randomizer.RollSum(2, 6);
                rolls.Add(roll);
                rollCount++;

                int damage = GetRatingDamage(power, roll);
                if (rollCount > 1)
                {
                    damage += criticalBonus;
                }
                damages.Add(damage);
                totalDamage += damage;

            } while (rolls[rolls.Count - 1] >= critical && critical <= 12);

            totalDamage += modifier + extraDamage;

            // 結果文字列を構築
            var sb = new StringBuilder();
            sb.Append($"(K{power}");
            if (critical != 10)
            {
                sb.Append($"@{critical}");
            }
            if (modifier > 0)
            {
                sb.Append($"+{modifier}");
            }
            else if (modifier < 0)
            {
                sb.Append($"{modifier}");
            }
            if (criticalBonus > 0)
            {
                sb.Append($"$+{criticalBonus}");
            }
            if (extraDamage > 0)
            {
                sb.Append($"#{extraDamage}");
            }
            sb.Append(") ＞ ");

            // ロール結果
            for (int i = 0; i < rolls.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(" ＞ ");
                }
                sb.Append($"[{rolls[i]}]:{damages[i]}");
            }

            sb.Append($" ＞ {totalDamage}");

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        /// <summary>
        /// 威力表からダメージを取得する
        /// </summary>
        private static int GetRatingDamage(int power, int roll)
        {
            // 簡易的な威力表（実際のSW2.5の威力表を近似）
            // 出目2はピンゾロで0固定
            if (roll <= 2)
            {
                return 0;
            }

            // 基本ダメージ = (出目 - 2) + 威力補正
            int baseDamage = roll - 2;
            int powerBonus = power / 10;

            return Math.Max(0, baseDamage + powerBonus);
        }
    }
}
