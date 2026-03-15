using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ダークデイズドライブ
    /// </summary>
    public sealed class DarkDaysDrive : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DarkDaysDrive Instance = new DarkDaysDrive();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        // IAX コマンド用の正規表現 (IA table body に埋め込まれた "(IAA)" 等を抽出)
        private static readonly Regex AZRegex = new Regex(@"\(([A-Z]+)\)", RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "DarkDaysDrive";

        /// <inheritdoc/>
        public override string Name => "ダークデイズドライブ";

        /// <inheritdoc/>
        public override string SortKey => "たあくていすとらいふ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        スペシャル／ファンブル／成功／失敗を判定
        ・各種表
        RTTn ランダム特技決定表(n：分野番号、省略可能)
        RCT  ランダム分野決定表
        ABRT アビリティ決定表
        DT ダメージ表
        FT 失敗表
        GJT 大成功表
        ITT 移動トラブル表
        NTT 任務トラブル表
        STT 襲撃トラブル表
        HTT 変身トラブル表
        DET ドライブイベント表
        BET ブレイクイベント表
        CT キャンプ表
        KZT 関係属性表
        IA イケメンアクション決定表
         IAA 遠距離 IAB 移動 IAC 近距離 IAD 善人 IAE 悪人
         IAF 幼い IAG バカ IAH 渋い IAI 賢い IAJ 超自然
        IAX イケメンアクション決定表 → IA表
        ■本格的な戦闘
        CAC センターの行動決定
        DDC 対話ダメージ表
        ・D66ダイス昇順
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollTables(command, TABLES) ?? CommandIax(command, randomizer);
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }

            if (diceTotal <= 2)
            {
                return Result.Fumble("ファンブル(判定失敗。失敗表(FT)を追加で１回振る)");
            }
            else if (diceTotal >= 12)
            {
                return Result.Critical("スペシャル(判定成功。大成功表(GJT)を１回使用可能)");
            }
            else if (target < 0)
            {
                // target == "?" 相当: 目標値不明
                return Result.CreateBuilder("").Build();
            }
            else
            {
                return total >= target
                    ? Result.Success("成功")
                    : Result.Failure("失敗");
            }
        }

        private Result? CommandIax(string command, IRandomizer randomizer)
        {
            if (command != "IAX")
            {
                return null;
            }

            // TABLES["IA"] が設定されていない場合はnullを返す
            if (!TABLES.ContainsKey("IA") || !TABLES.ContainsKey("IA"))
            {
                return null;
            }

            // スタブ実装: IAXコマンドは現時点ではテーブルが未実装のためnullを返す
            return null;
        }
    }
}
