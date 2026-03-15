using System.Collections.Generic;

namespace BCDice.Core
{
    /// <summary>
    /// 乱数生成器のインターフェース
    /// </summary>
    public interface IRandomizer
    {
        /// <summary>
        /// 複数のダイスを振り、各出目を配列で返す
        /// </summary>
        /// <param name="times">振るダイスの個数</param>
        /// <param name="sides">ダイスの面数</param>
        /// <returns>各ダイスの出目の配列</returns>
        int[] RollBarabara(int times, int sides);

        /// <summary>
        /// 複数のダイスを振り、合計値を返す
        /// </summary>
        /// <param name="times">振るダイスの個数</param>
        /// <param name="sides">ダイスの面数</param>
        /// <returns>出目の合計</returns>
        int RollSum(int times, int sides);

        /// <summary>
        /// 1回だけダイスを振る
        /// </summary>
        /// <param name="sides">ダイスの面数</param>
        /// <returns>1以上sides以下の出目</returns>
        int RollOnce(int sides);

        /// <summary>
        /// ダイス表などでインデックスを参照する用のダイスロール
        /// </summary>
        /// <param name="sides">ダイスの面数</param>
        /// <returns>0以上sides未満の整数</returns>
        int RollIndex(int sides);

        /// <summary>
        /// D10で十の位を決定するためのダイスロール
        /// </summary>
        /// <returns>0, 10, 20, ..., 90のいずれか</returns>
        int RollTensD10();

        /// <summary>
        /// D10を0-9として扱うダイスロール
        /// </summary>
        /// <returns>0以上9以下の整数</returns>
        int RollD9();

        /// <summary>
        /// D66のダイスロールを行う
        /// </summary>
        /// <param name="sortType">出目のソート方法</param>
        /// <returns>D66の結果（11-66の範囲）</returns>
        int RollD66(D66SortType sortType);

        /// <summary>
        /// ダイスロールの履歴（値と面数のペア）
        /// </summary>
        IReadOnlyList<(int Value, int Sides)> RandResults { get; }

        /// <summary>
        /// ダイスロールの詳細な履歴
        /// </summary>
        IReadOnlyList<DetailedRandResult> DetailedRandResults { get; }
    }
}
