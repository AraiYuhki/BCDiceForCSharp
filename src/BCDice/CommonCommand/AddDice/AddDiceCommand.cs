using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 加算ダイスコマンド
    /// 例: 2D6, 2D6+3, 2D6+3>=7
    /// </summary>
    public class AddDiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly AddDiceCommand Instance = new AddDiceCommand();

        private static readonly Regex PrefixRegex = new Regex(
            @"^S?[+\-(]*(\d+|D\d+)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"[+\-(]*(\d+|D\d+)";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            if (!PrefixRegex.IsMatch(command))
            {
                return null;
            }

            var parsed = AddDiceParser.Parse(command);
            if (parsed == null)
            {
                return null;
            }

            return Evaluate(parsed, gameSystem, randomizer);
        }

        private Result Evaluate(AddDiceParsed parsed, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            int total = parsed.Left.Eval(gameSystem, randomizer);
            string expr = parsed.Left.Expr(gameSystem);
            string output = parsed.Left.Output;

            var sb = new StringBuilder();
            sb.Append('(');
            sb.Append(expr);

            if (parsed.CompareOp.HasValue && parsed.Right != null)
            {
                int target = parsed.Right.Eval(gameSystem, null);
                sb.Append(CompareOpToString(parsed.CompareOp.Value));
                sb.Append(target);
            }

            sb.Append(')');

            // 中間結果を追加（単一ダイスロールでない場合）
            if (output != total.ToString() && output.Contains("["))
            {
                sb.Append(" ＞ ");
                sb.Append(output);
            }

            sb.Append(" ＞ ");
            sb.Append(total);

            // 成否判定
            bool? success = null;
            if (parsed.CompareOp.HasValue && parsed.Right != null)
            {
                int target = parsed.Right.Eval(gameSystem, null);
                success = Compare(total, parsed.CompareOp.Value, target);

                sb.Append(" ＞ ");
                sb.Append(success.Value ? "成功" : "失敗");
            }

            var builder = Result.CreateBuilder(sb.ToString())
                .SetSecret(parsed.IsSecret)
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
