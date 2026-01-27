namespace BCDice.Arithmetic
{
    /// <summary>
    /// 除算時の端数処理指定の種類
    /// </summary>
    public enum DivideRoundingType
    {
        /// <summary>ゲームシステムのデフォルト設定を使用</summary>
        GameSystemDefault,

        /// <summary>切り上げ (U または C)</summary>
        Ceiling,

        /// <summary>四捨五入 (R)</summary>
        Round,

        /// <summary>切り捨て (F)</summary>
        Floor
    }
}
