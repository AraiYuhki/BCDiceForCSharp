using BCDice.Core;

namespace BCDice.Preprocessor
{
    /// <summary>
    /// デフォルトのゲームシステムコンテキスト
    /// </summary>
    public class DefaultGameSystemContext : IGameSystemContext
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DefaultGameSystemContext Instance = new DefaultGameSystemContext();

        /// <inheritdoc/>
        public RoundType RoundType => RoundType.Floor;

        /// <inheritdoc/>
        public int SidesImplicitD => 6;

        /// <inheritdoc/>
        public D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public CompareOperator? DefaultCmpOp => null;

        /// <inheritdoc/>
        public int? DefaultTargetNumber => null;

        /// <inheritdoc/>
        public bool SortBarabaraDice => false;

        /// <inheritdoc/>
        public int? RerollDiceRerollThreshold => null;

        /// <inheritdoc/>
        public string ChangeText(string text)
        {
            return text;
        }

        /// <inheritdoc/>
        public string? GetGrichText(int countOne, int diceCount, int successCount)
        {
            return null;
        }
    }
}
