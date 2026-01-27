using BCDice.Core;

namespace BCDice.Command
{
    /// <summary>
    /// 入力の正規化を行うユーティリティ
    /// </summary>
    public static class Normalize
    {
        /// <summary>
        /// 比較演算子文字列をCompareOperatorに正規化する
        /// </summary>
        /// <param name="op">比較演算子の文字列</param>
        /// <returns>正規化されたCompareOperator、無効な場合はnull</returns>
        public static CompareOperator? ComparisonOperator(string op)
        {
            if (string.IsNullOrEmpty(op))
            {
                return null;
            }

            // <= または =<
            if (op.Contains("<=") || op.Contains("=<"))
            {
                return Core.CompareOperator.LessThanOrEqual;
            }

            // >= または =>
            if (op.Contains(">=") || op.Contains("=>"))
            {
                return Core.CompareOperator.GreaterThanOrEqual;
            }

            // <> または != または =!
            if (op.Contains("<>") || op.Contains("!=") || op.Contains("=!"))
            {
                return Core.CompareOperator.NotEqual;
            }

            // < (他のパターンにマッチしなかった場合)
            if (op.Contains("<"))
            {
                return Core.CompareOperator.LessThan;
            }

            // > (他のパターンにマッチしなかった場合)
            if (op.Contains(">"))
            {
                return Core.CompareOperator.GreaterThan;
            }

            // = (他のパターンにマッチしなかった場合)
            if (op.Contains("="))
            {
                return Core.CompareOperator.Equal;
            }

            return null;
        }

        /// <summary>
        /// 目標値を正規化する
        /// </summary>
        /// <param name="value">目標値の文字列</param>
        /// <returns>整数の目標値、または"?"の場合はnull</returns>
        public static int? TargetNumber(string value)
        {
            if (string.IsNullOrEmpty(value) || value == "?")
            {
                return null;
            }

            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return null;
        }
    }
}
