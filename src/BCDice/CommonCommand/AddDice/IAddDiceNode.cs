using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 加算ダイスのASTノードインターフェース
    /// </summary>
    public interface IAddDiceNode
    {
        /// <summary>
        /// ノードを評価する
        /// </summary>
        /// <param name="context">ゲームシステムコンテキスト</param>
        /// <param name="randomizer">乱数生成器（ダイスロール用）</param>
        /// <returns>評価結果</returns>
        int Eval(IGameSystemContext context, IRandomizer? randomizer);

        /// <summary>
        /// ダイスを含むか
        /// </summary>
        bool IncludesDice { get; }

        /// <summary>
        /// 式の文字列表現
        /// </summary>
        string Expr(IGameSystemContext context);

        /// <summary>
        /// 出力用の文字列（ダイス結果を含む）
        /// </summary>
        string Output { get; }
    }
}
