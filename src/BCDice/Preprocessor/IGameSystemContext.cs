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
        /// デフォルトの比較演算子（目標値が空欄の場合に使用）
        /// </summary>
        CompareOperator? DefaultCmpOp { get; }

        /// <summary>
        /// デフォルトの目標値（目標値が空欄の場合に使用）
        /// </summary>
        int? DefaultTargetNumber { get; }

        /// <summary>
        /// バラバラダイスでダイス目をソートするかどうか
        /// </summary>
        bool SortBarabaraDice { get; }

        /// <summary>
        /// RerollDiceで振り足しをする出目の閾値
        /// </summary>
        int? RerollDiceRerollThreshold { get; }

        /// <summary>
        /// ゲームシステム固有のテキスト変換を行う
        /// </summary>
        /// <param name="text">入力テキスト</param>
        /// <returns>変換後のテキスト</returns>
        string ChangeText(string text);

        /// <summary>
        /// グリッチテキストを取得する（シャドウラン用）
        /// </summary>
        /// <param name="countOne">出目1の数</param>
        /// <param name="diceCount">ダイスの総数</param>
        /// <param name="successCount">成功数</param>
        /// <returns>グリッチテキスト、なければnull</returns>
        string? GetGrichText(int countOne, int diceCount, int successCount);
    }
}
