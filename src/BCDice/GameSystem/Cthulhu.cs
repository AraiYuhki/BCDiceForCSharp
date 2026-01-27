using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// クトゥルフ神話TRPG
    /// </summary>
    public sealed class Cthulhu : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Cthulhu Instance = new Cthulhu();

        private static readonly Regex CcbRegex = new Regex(
            @"^S?CCB?(?:\((\d+)\)|(\d+))?(?:([<>=]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Cthulhu";

        /// <inheritdoc/>
        public override string Name => "クトゥルフ神話TRPG";

        /// <inheritdoc/>
        public override string SortKey => "くとうるふ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【クトゥルフ神話TRPG】

・1D100での技能判定
　CCB<=80 （技能値80で判定）
　CC<=80  （同上）

・D66ダイス（昇順ソート）
　D66

・組み合わせ判定
　(STR+CON)*5 などの計算式を使用可能
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // CCB または CC コマンド
            var result = EvalCcbCommand(command, randomizer);
            if (result != null)
            {
                return result;
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? EvalCcbCommand(string command, IRandomizer randomizer)
        {
            var match = CcbRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", System.StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("CC", System.StringComparison.OrdinalIgnoreCase);

            // ダイスを振る
            int roll = randomizer.RollOnce(100);

            // 目標値
            int? target = null;
            if (match.Groups[3].Success && match.Groups[4].Success)
            {
                target = int.Parse(match.Groups[4].Value);
            }
            else if (match.Groups[1].Success)
            {
                target = int.Parse(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                target = int.Parse(match.Groups[2].Value);
            }

            string text;
            bool? success = null;

            if (target.HasValue)
            {
                success = roll <= target.Value;
                string resultText = GetResultText(roll, target.Value);
                text = $"(1D100<={target.Value}) ＞ {roll} ＞ {resultText}";
            }
            else
            {
                text = $"(1D100) ＞ {roll}";
            }

            var builder = Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (success.HasValue)
            {
                builder.SetCondition(success.Value);

                // クリティカル/ファンブル判定
                if (roll <= 5)
                {
                    builder.SetCritical(true);
                }
                else if (roll >= 96)
                {
                    builder.SetFumble(true);
                }
            }

            return builder.Build();
        }

        private static string GetResultText(int roll, int target)
        {
            if (roll <= 5)
            {
                return "決定的成功/クリティカル";
            }
            if (roll >= 96)
            {
                return "致命的失敗/ファンブル";
            }
            if (roll <= target)
            {
                return "成功";
            }
            return "失敗";
        }
    }
}
