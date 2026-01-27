namespace BCDice
{
    /// <summary>
    /// BCDiceライブラリのバージョン情報
    /// </summary>
    public static class BCDiceVersion
    {
        /// <summary>
        /// メジャーバージョン
        /// </summary>
        public const int Major = 0;

        /// <summary>
        /// マイナーバージョン
        /// </summary>
        public const int Minor = 1;

        /// <summary>
        /// パッチバージョン
        /// </summary>
        public const int Patch = 0;

        /// <summary>
        /// バージョン文字列
        /// </summary>
        public static string Version => $"{Major}.{Minor}.{Patch}";

        /// <summary>
        /// ライブラリ名
        /// </summary>
        public const string Name = "BCDice for C#";

        /// <summary>
        /// 完全なバージョン情報
        /// </summary>
        public static string FullVersion => $"{Name} {Version}";

        /// <summary>
        /// 元となるBCDice（Ruby版）のバージョン
        /// </summary>
        public const string OriginalBCDiceVersion = "3.x compatible";
    }
}
