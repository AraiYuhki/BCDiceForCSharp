using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゲームシステムのインターフェース
    /// </summary>
    public interface IGameSystem
    {
        /// <summary>
        /// ゲームシステムの一意識別子
        /// </summary>
        string Id { get; }

        /// <summary>
        /// ゲームシステムの表示名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// ソート用キー（省略時はNameを使用）
        /// </summary>
        string SortKey { get; }

        /// <summary>
        /// コマンドのヘルプメッセージ
        /// </summary>
        string HelpMessage { get; }

        /// <summary>
        /// 端数処理タイプ
        /// </summary>
        RoundType RoundType { get; }

        /// <summary>
        /// D66のソート順
        /// </summary>
        D66SortType D66SortType { get; }

        /// <summary>
        /// コマンドを評価する
        /// </summary>
        /// <param name="command">入力コマンド</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、コマンドが認識されない場合はnull</returns>
        Result? Eval(string command, IRandomizer randomizer);

        /// <summary>
        /// コマンドを評価する（デフォルトの乱数生成器を使用）
        /// </summary>
        /// <param name="command">入力コマンド</param>
        /// <returns>評価結果、コマンドが認識されない場合はnull</returns>
        Result? Eval(string command);
    }
}
