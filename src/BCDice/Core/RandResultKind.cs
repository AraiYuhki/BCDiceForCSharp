namespace BCDice.Core
{
    /// <summary>
    /// ダイスロール結果の種類を表す列挙型
    /// </summary>
    public enum RandResultKind
    {
        /// <summary>
        /// 通常のダイスロール
        /// </summary>
        Normal,

        /// <summary>
        /// D10で十の位を決定するロール（0, 10, 20, ..., 90）
        /// </summary>
        TensD10,

        /// <summary>
        /// D10を0-9として扱うロール
        /// </summary>
        D9
    }
}
