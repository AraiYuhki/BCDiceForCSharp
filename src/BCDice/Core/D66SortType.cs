namespace BCDice.Core
{
    /// <summary>
    /// D66ダイスの出目の入れ替え方法を指定する列挙型
    /// </summary>
    public enum D66SortType
    {
        /// <summary>
        /// 入れ替えない（出目をそのまま使用）
        /// </summary>
        NoSort,

        /// <summary>
        /// 昇順にソートする（一の位が大きな出目になる）
        /// </summary>
        Ascending,

        /// <summary>
        /// 降順にソートする（一の位が小さな出目になる）
        /// </summary>
        Descending
    }
}
