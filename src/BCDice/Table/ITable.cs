using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// ランダムテーブルのインターフェース
    /// </summary>
    public interface ITable
    {
        /// <summary>
        /// テーブルの名前
        /// </summary>
        string Name { get; }

        /// <summary>
        /// テーブルを引くためのコマンドパターン
        /// </summary>
        string Command { get; }

        /// <summary>
        /// テーブルをロールする
        /// </summary>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>結果</returns>
        Result Roll(IRandomizer randomizer);
    }
}
