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
    /// 下方ダイス（ペナルティダイス）コマンド
    /// 複数のダイスを振り、最小値を採用する
    /// 例: 2R6 → 2つのD6を振り、小さい方を採用
    /// </summary>
    public class LowerDiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly LowerDiceCommand Instance = new LowerDiceCommand();

        private static readonly Regex CommandRegex = new Regex(
            @"^S?(\d+)[RL](\d+)(?:([<>=!]+)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"\d+[RL]";

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

            if (count <= 0 || sides <= 0)
            {
                return null;
            }

            // ダイスを振る
            var rolls = randomizer.RollBarabara(count, sides);
            int minValue = rolls.Min();

            var sb = new StringBuilder();
            sb.Append($"({count}R{sides})");

            // 比較演算がある場合
            bool? success = null;
            if (match.Groups[3].Success && match.Groups[4].Success)
            {
                string opStr = match.Groups[3].Value;
                int target = int.Parse(match.Groups[4].Value);
                var compareOp = Normalize.ComparisonOperator(opStr);

                if (compareOp.HasValue)
                {
                    string normalizedOp = CompareOpToString(compareOp.Value);
                    sb.Append($"{normalizedOp}{target}");
                }
            }

            sb.Append($" ＞ [{string.Join(",", rolls)}]");
            sb.Append($" ＞ {minValue}");

            if (match.Groups[3].Success && match.Groups[4].Success)
            {
                int target = int.Parse(match.Groups[4].Value);
                var compareOp = Normalize.ComparisonOperator(match.Groups[3].Value);

                if (compareOp.HasValue)
                {
                    success = Compare(minValue, compareOp.Value, target);
                    sb.Append($" ＞ {(success.Value ? "成功" : "失敗")}");
                }
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
