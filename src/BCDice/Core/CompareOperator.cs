namespace BCDice.Core
{
    /// <summary>
    /// 比較演算子を表す列挙型
    /// </summary>
    public enum CompareOperator
    {
        /// <summary>
        /// 等しい (==)
        /// </summary>
        Equal,

        /// <summary>
        /// 等しくない (!=)
        /// </summary>
        NotEqual,

        /// <summary>
        /// より大きい (&gt;)
        /// </summary>
        GreaterThan,

        /// <summary>
        /// 以上 (&gt;=)
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// より小さい (&lt;)
        /// </summary>
        LessThan,

        /// <summary>
        /// 以下 (&lt;=)
        /// </summary>
        LessThanOrEqual
    }
}
