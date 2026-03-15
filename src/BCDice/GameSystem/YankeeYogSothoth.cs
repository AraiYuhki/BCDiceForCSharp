using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ヤンキー＆ヨグ＝ソトース
    /// </summary>
    public sealed class YankeeYogSothoth : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly YankeeYogSothoth Instance = new YankeeYogSothoth();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "YankeeYogSothoth";

        /// <inheritdoc/>
        public override string Name => "ヤンキー＆ヨグ＝ソトース";

        /// <inheritdoc/>
        public override string SortKey => "やんきいあんとよくそとおす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        スペシャル／ファンブル／成功／失敗を判定
        ・各種表
        ※うろつき～決闘フェイズ
        FT	ファンブル表
        WT	変調表
        RTTn	ランダム特技決定表(n：分野番号、省略可能)
        RCT ランダム分野決定表
        KKT	関係表
        DBRT	他愛のない会話表
        TKT	戦う理由表

        ※武勇伝フェイズ
        BUDT	武勇伝表
        GUDT	ガイヤンキー武勇伝表
        FTNT	二つ名決定表
        DAIT	第一印象表
        TKKT	ツレ関係表

        ※帰還フェイズ
        GSST	現実世界生活表
        GYST	ガイヤンキー生活表
        HPST	病院生活表
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
                return Result.CreateBuilder("ファンブル(判定失敗。ファンブル表（FT）を振ること).Build()").SetFumble(true).SetFailure(true).Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder("スペシャル(判定成功。【テンション】が１段階上昇).Build()").SetCritical(true).SetSuccess(true).Build();
            }

            return null;
        }

    }
}
