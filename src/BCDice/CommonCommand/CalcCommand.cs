using System.Text.RegularExpressions;
using BCDice.Arithmetic;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 計算コマンド (C式)
    /// 例: C(10+5), SC(3*4)
    /// </summary>
    public class CalcCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CalcCommand Instance = new CalcCommand();

        private static readonly Regex PrefixRegex = new Regex(
            @"^S?C[+\-(]*\d+",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"C[+\-(]*\d+";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            if (!PrefixRegex.IsMatch(command))
            {
                return null;
            }

            var parsed = Parse(command);
            if (parsed == null)
            {
                return null;
            }

            return parsed.Value.Evaluate(gameSystem.RoundType);
        }

        private CalcParsed? Parse(string command)
        {
            string upper = command.ToUpperInvariant();
            int index = 0;
            bool isSecret = false;

            // シークレットチェック
            if (index < upper.Length && upper[index] == 'S')
            {
                isSecret = true;
                index++;
            }

            // 'C' のチェック
            if (index >= upper.Length || upper[index] != 'C')
            {
                return null;
            }
            index++;

            // 残りの部分を算術式としてパース
            string expr = command.Substring(index);
            var node = ArithmeticParser.Parse(expr);

            if (node == null)
            {
                return null;
            }

            return new CalcParsed(isSecret, node, expr);
        }

        private readonly struct CalcParsed
        {
            public bool IsSecret { get; }
            public IArithmeticNode Expression { get; }
            public string OriginalExpr { get; }

            public CalcParsed(bool isSecret, IArithmeticNode expression, string originalExpr)
            {
                IsSecret = isSecret;
                Expression = expression;
                OriginalExpr = originalExpr;
            }

            public Result Evaluate(RoundType roundType)
            {
                try
                {
                    int value = Expression.Eval(roundType);
                    string output = Expression.Output();
                    string text = $"C({output}) ＞ {value}";

                    return Result.CreateBuilder(text)
                        .SetSecret(IsSecret)
                        .Build();
                }
                catch
                {
                    return Result.CreateBuilder("計算エラー")
                        .SetSecret(IsSecret)
                        .Build();
                }
            }
        }
    }
}
