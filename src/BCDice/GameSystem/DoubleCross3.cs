using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ダブルクロス The 3rd Edition
    /// </summary>
    public sealed class DoubleCross3 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DoubleCross3 Instance = new DoubleCross3();

        private static readonly Regex DxRegex = new Regex(
            @"^S?(\d+)DX(?:@(\d+))?(?:\+(\d+))?(?:\-(\d+))?(?:([<>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "DoubleCross3";

        /// <inheritdoc/>
        public override string Name => "ダブルクロス The 3rd Edition";

        /// <inheritdoc/>
        public override string SortKey => "たふるくろす3";

        /// <inheritdoc/>
        public override string HelpMessage => @"
【ダブルクロス The 3rd Edition】

・判定コマンド（nDX）
　4DX       4個のD10で判定（10で振り足し）
　4DX@8     クリティカル値8で判定
　4DX+5     達成値修正+5
　4DX@8+5   クリティカル値8、修正+5
　4DX>=12   目標値12で判定
　4DX@8+5>=12 全部入り

・クリティカル値はダイス目がその値以上で振り足し
・デフォルトのクリティカル値は10
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = EvalDxCommand(command, randomizer);
            if (result != null)
            {
                return result;
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? EvalDxCommand(string command, IRandomizer randomizer)
        {
            var match = DxRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            int diceCount = int.Parse(match.Groups[1].Value);
            int critical = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 10;
            int modifier = 0;
            if (match.Groups[3].Success) modifier += int.Parse(match.Groups[3].Value);
            if (match.Groups[4].Success) modifier -= int.Parse(match.Groups[4].Value);

            int? target = null;
            string? compareOp = null;
            if (match.Groups[5].Success && match.Groups[6].Success)
            {
                compareOp = match.Groups[5].Value;
                target = int.Parse(match.Groups[6].Value);
            }

            // クリティカル値の制限（2-12）
            critical = Math.Max(2, Math.Min(12, critical));

            // 各ダイスの最終値を追跡
            var diceResults = new List<List<int>>();
            const int maxRerolls = 100;

            for (int i = 0; i < diceCount; i++)
            {
                var singleDiceRolls = new List<int>();
                int rerollCount = 0;

                do
                {
                    int roll = randomizer.RollOnce(10);
                    singleDiceRolls.Add(roll);
                    rerollCount++;
                } while (singleDiceRolls.Last() >= critical && rerollCount < maxRerolls);

                diceResults.Add(singleDiceRolls);
            }

            // 各ダイスの最終達成値を計算（振り足しを含めた合計）
            var finalValues = diceResults.Select(rolls => rolls.Sum()).ToList();
            int maxValue = finalValues.Max();
            int totalValue = maxValue + modifier;

            // 結果文字列を構築
            var sb = new StringBuilder();
            sb.Append($"({diceCount}DX");
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
            if (target.HasValue)
            {
                sb.Append($">={target}");
            }
            sb.Append(") ＞ ");

            // ダイス結果を表示
            var diceStrings = diceResults.Select(rolls =>
            {
                if (rolls.Count == 1)
                {
                    return rolls[0].ToString();
                }
                return string.Join("+", rolls) + $"={rolls.Sum()}";
            });
            sb.Append($"[{string.Join(",", diceStrings)}]");

            sb.Append($" ＞ 最大{maxValue}");
            if (modifier != 0)
            {
                sb.Append($"+{modifier}={totalValue}");
            }

            // 成功/失敗判定
            bool? success = null;
            if (target.HasValue)
            {
                success = totalValue >= target.Value;
                sb.Append($" ＞ {(success.Value ? "成功" : "失敗")}");
            }

            var builder = Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (success.HasValue)
            {
                builder.SetCondition(success.Value);
            }

            return builder.Build();
        }
    }
}
