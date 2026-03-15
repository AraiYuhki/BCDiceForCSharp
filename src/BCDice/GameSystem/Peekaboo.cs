using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ピーカーブー
    /// </summary>
    public sealed class Peekaboo : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Peekaboo Instance = new Peekaboo();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "Peekaboo";

        /// <inheritdoc/>
        public override string Name => "ピーカーブー";

        /// <inheritdoc/>
        public override string SortKey => "ひいかあふう";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　判定時にクリティカルとファンブルを自動判定します。
        ・各種表
        　・学校イベント表　　　　　　　　SET
        　・個別学校イベント表　　　　　　PSET
        　・オバケ屋敷イベント表　　　　　OET
        　・イノセント用バタンキュー！表　IBT
        　・スプーキー用バタンキュー！表　SBT
        　・日中ブラブラ表　　　　　　　　NET
        　・オバケぶらり旅表　　　　　　　STT
        　・仲間効果表　　　　　　　　　　NST
        　・ランダム特技決定表　　　　　　RTT
        　・ランダム分野決定表　　　　　　RCT
        　・指定特技(不良)表　　　　　　　RTT1, TNT
        　・指定特技(運動)表　　　　　　　RTT2, TET
        　・指定特技(友達)表　　　　　　　RTT3, TFT
        　・指定特技(遊び)表　　　　　　　RTT4, TPT
        　・指定特技(勉強)表　　　　　　　RTT5, TST
        　・指定特技(大人)表　　　　　　　RTT6, TAT
        ・D66ダイスあり
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollTables(command, TABLES);
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
                return Result.CreateBuilder("ファンブル(【眠気】が1d6点上昇).Build()").SetFumble(true).SetFailure(true).Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder("スペシャル(【魔力】あるいは【眠気】が1d6点回復).Build()").SetCritical(true).SetSuccess(true).Build();
            }

            return null;
        }

    }
}
