namespace BCDice.Command
{
    /// <summary>
    /// トークンの種類
    /// </summary>
    public enum TokenType
    {
        /// <summary>数値</summary>
        Number,

        /// <summary>比較演算子 (&lt;=, &gt;=, &lt;, &gt;, ==, !=)</summary>
        CompareOp,

        /// <summary>+</summary>
        Plus,

        /// <summary>-</summary>
        Minus,

        /// <summary>*</summary>
        Asterisk,

        /// <summary>/</summary>
        Slash,

        /// <summary>(</summary>
        ParenLeft,

        /// <summary>)</summary>
        ParenRight,

        /// <summary>[</summary>
        BracketLeft,

        /// <summary>]</summary>
        BracketRight,

        /// <summary>?</summary>
        Question,

        /// <summary>@</summary>
        At,

        /// <summary>#</summary>
        Sharp,

        /// <summary>,</summary>
        Comma,

        /// <summary>識別子（D, B, R, U, C, F など）</summary>
        Identifier,

        /// <summary>入力終端</summary>
        Eof
    }
}
