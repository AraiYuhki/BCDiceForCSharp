using System;
using System.Text.RegularExpressions;
using BCDice.Command;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// D66コマンド
    /// 2つのD6を振り、十の位と一の位として組み合わせる
    /// 例: D66 → 35（3と5を振った場合）
    /// </summary>
    public class D66Command : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly D66Command Instance = new D66Command();

        private static readonly Regex CommandRegex = new Regex(
            @"^S?D66(?:([ASN]))?(?:([<>=!]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"D66";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("D", StringComparison.OrdinalIgnoreCase);

            // ソート順を決定
            D66SortType sortType = gameSystem.D66SortType;
            if (match.Groups[1].Success)
            {
                sortType = match.Groups[1].Value.ToUpperInvariant() switch
                {
                    "A" => D66SortType.Ascending,
                    "S" => D66SortType.Ascending,
                    "N" => D66SortType.NoSort,
                    _ => sortType
                };
            }

            // D66を振る
            int result = randomizer.RollD66(sortType);
            int tens = result / 10;
            int ones = result % 10;

            string sortSuffix = sortType switch
            {
                D66SortType.Ascending => "S",
                D66SortType.Descending => "S",
                _ => ""
            };

            string text = $"(D66{sortSuffix}) ＞ {result}[{tens},{ones}]";

            // 比較演算がある場合
            bool? success = null;
            if (match.Groups[2].Success && match.Groups[3].Success)
            {
                string opStr = match.Groups[2].Value;
                int target = int.Parse(match.Groups[3].Value);
                var compareOp = Normalize.ComparisonOperator(opStr);

                if (compareOp.HasValue)
                {
                    success = Compare(result, compareOp.Value, target);
                    string normalizedOp = CompareOpToString(compareOp.Value);
                    text = $"(D66{sortSuffix}{normalizedOp}{target}) ＞ {result}[{tens},{ones}] ＞ {(success.Value ? "成功" : "失敗")}";
                }
            }

            var builder = Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (success.HasValue)
            {
                builder.SetCondition(success.Value);
            }

            return builder.Build();
        }

        private static string CompareOpToString(CompareOperator op)
        {
            return op switch
            {
                CompareOperator.Equal => "=",
                CompareOperator.NotEqual => "<>",
                CompareOperator.GreaterThan => ">",
                CompareOperator.GreaterThanOrEqual => ">=",
                CompareOperator.LessThan => "<",
                CompareOperator.LessThanOrEqual => "<=",
                _ => "?"
            };
        }

        private static bool Compare(int left, CompareOperator op, int right)
        {
            return op switch
            {
                CompareOperator.Equal => left == right,
                CompareOperator.NotEqual => left != right,
                CompareOperator.GreaterThan => left > right,
                CompareOperator.GreaterThanOrEqual => left >= right,
                CompareOperator.LessThan => left < right,
                CompareOperator.LessThanOrEqual => left <= right,
                _ => false
            };
        }
    }
}
