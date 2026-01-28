using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ダンジョンズ＆ドラゴンズ
    /// </summary>
    public sealed class DungeonsAndDragons : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DungeonsAndDragons Instance = new DungeonsAndDragons();

        /// <inheritdoc/>
        public override string Id => "DungeonsAndDragons";

        /// <inheritdoc/>
        public override string Name => "ダンジョンズ＆ドラゴンズ";

        /// <inheritdoc/>
        public override string SortKey => "たんしよんすあんととらこんす";

        /// <inheritdoc/>
        public override string HelpMessage => @"※このダイスボットは部屋のシステム名表示用となります。
";

        private DungeonsAndDragons() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // このシステムは固有コマンドを持たない
            return null;
        }
    }
}
