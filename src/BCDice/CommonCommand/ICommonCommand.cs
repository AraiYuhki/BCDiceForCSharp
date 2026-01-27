using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 共通コマンドのインターフェース
    /// </summary>
    public interface ICommonCommand
    {
        /// <summary>
        /// コマンドにマッチするプレフィックスパターン
        /// </summary>
        string PrefixPattern { get; }

        /// <summary>
        /// コマンドを評価する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="gameSystem">ゲームシステムコンテキスト</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、コマンドが実行できない場合はnull</returns>
        Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer);
    }
}
