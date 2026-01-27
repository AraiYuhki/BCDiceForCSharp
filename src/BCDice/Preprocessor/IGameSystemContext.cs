using BCDice.Core;

namespace BCDice.Preprocessor
{
    /// <summary>
    /// 前処理に必要なゲームシステムの情報を提供するインターフェース
    /// </summary>
    public interface IGameSystemContext
    {
        /// <summary>
        /// 端数処理タイプ
        /// </summary>
        RoundType RoundType { get; }

        /// <summary>
        /// 暗黙のダイス面数（nDの場合に使用）
        /// </summary>
        int SidesImplicitD { get; }

        /// <summary>
        /// D66のソート順
        /// </summary>
        D66SortType D66SortType { get; }

        /// <summary>
        /// ゲームシステム固有のテキスト変換を行う
        /// </summary>
        /// <param name="text">入力テキスト</param>
        /// <returns>変換後のテキスト</returns>
        string ChangeText(string text);
    }
}
