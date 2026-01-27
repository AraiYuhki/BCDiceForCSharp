using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Command;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 成功数カウントダイスコマンド
    /// 例: 5S6>=4 - 5個のD6を振り、4以上の出目を数える
    /// </summary>
    public class CountSuccessCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CountSuccessCommand Instance = new CountSuccessCommand();

        private static readonly Regex CommandRegex = new Regex(
            @"^S?(\d+)S(\d+)([<>=!]+)(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"\d+S\d+";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            int count = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);
            string opStr = match.Groups[3].Value;
            int target = int.Parse(match.Groups[4].Value);

            if (count <= 0 || sides <= 0)
            {
                return null;
            }

            var compareOp = Normalize.ComparisonOperator(opStr);
            if (!compareOp.HasValue)
            {
                return null;
            }

            // ダイスを振る
            var rolls = randomizer.RollBarabara(count, sides);

            // 成功数をカウント
            int successCount = rolls.Count(r => Compare(r, compareOp.Value, target));

            var sb = new StringBuilder();
            sb.Append($"({count}S{sides}{CompareOpToString(compareOp.Value)}{target})");
            sb.Append($" ＞ [{string.Join(",", rolls)}]");
            sb.Append($" ＞ 成功数{successCount}");

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
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
