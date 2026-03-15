using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 算術式のASTノードインターフェース
    /// </summary>
    public interface IArithmeticNode
    {
        /// <summary>
        /// ノードを評価して整数値を返す
        /// </summary>
        /// <param name="roundType">端数処理タイプ</param>
        /// <returns>評価結果</returns>
        int Eval(RoundType roundType);

        /// <summary>
        /// メッセージ出力用の文字列を返す
        /// </summary>
        /// <returns>出力文字列</returns>
        string Output();
    }
}
