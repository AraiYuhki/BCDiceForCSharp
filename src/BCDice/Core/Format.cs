namespace BCDice.Core
{
    /// <summary>
    /// BCDice Format ユーティリティ（BCDice Ruby版 format.rb 互換）
    /// </summary>
    public static class Format
    {
        /// <summary>
        /// 修正値を文字列表記にする（例: +2, -3, ""）
        /// </summary>
        public static string Modifier(int number)
        {
            if (number == 0) return "";
            if (number > 0) return $"+{number}";
            return number.ToString();
        }

        /// <summary>
        /// 修正値を文字列表記にする（nullable版）
        /// </summary>
        public static string? Modifier(int? number)
        {
            if (number == null) return null;
            return Modifier(number.Value);
        }

        /// <summary>
        /// 比較演算子を文字列表記にする
        /// </summary>
        public static string ComparisonOperator(string op)
        {
            return op switch
            {
                "==" => "=",
                "!=" => "<>",
                _ => op
            };
        }
    }
}
