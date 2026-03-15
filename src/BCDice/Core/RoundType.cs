namespace BCDice.Core
{
    /// <summary>
    /// 割り算をした後の端数の扱いを指定する列挙型
    /// </summary>
    public enum RoundType
    {
        /// <summary>
        /// 切り捨て（デフォルト）
        /// </summary>
        Floor,

        /// <summary>
        /// 切り上げ
        /// </summary>
        Ceiling,

        /// <summary>
        /// 四捨五入
        /// </summary>
        Round
    }
}
