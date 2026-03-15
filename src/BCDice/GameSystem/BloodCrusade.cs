using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ブラッド・クルセイド
    /// </summary>
    public sealed class BloodCrusade : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BloodCrusade Instance = new BloodCrusade();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "BloodCrusade";

        /// <inheritdoc/>
        public override string Name => "ブラッド・クルセイド";

        /// <inheritdoc/>
        public override string SortKey => "ふらつとくるせいと";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・各種表
        　・関係属性表         RT
        　・シーン表           ST
        　・先制判定指定特技表 IST
        　・身体部位決定表　　 BRT
        　・自信幸福表　　　　 CHT
        　・地位幸福表　　　　 SHT
        　・日常幸福表　　　　 DHT
        　・人脈幸福表　　　　 LHT
        　・退路幸福表　　　　 EHT
        　・ランダム全特技表　 AST
        　・軽度狂気表　　　　 MIT
        　・重度狂気表　　　　 SIT
        　・戦場シーン表　　　 BDST
        　・夢シーン表　　　　 DMST
        　・田舎シーン表　　　 CYST
        　・学校シーン表　　　 SLST
        　・館シーン表　　　　 MNST
        　・時間経過表（10代～60代、半吸血鬼）TD1T～TD6T、TDHT
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
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int? target, IRandomizer randomizer)
        {
            if (target == null || cmpOp != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }
            if (diceTotal <= 2)
            {
                return Result.CreateBuilder("ファンブル(【モラル】-3。追跡フェイズなら吸血シーンを追加。戦闘フェイズなら吸血鬼は追加行動を一回得る).Build()").SetFumble(true).SetFailure(true).Build();
            }
            if (diceTotal >= 12)
            {
                return Result.CreateBuilder("スペシャル(【モラル】+3。追跡フェイズならあなたに関係を持つPCの【モラル】+2。攻撃判定ならダメージ+1D6）").SetCritical(true).SetSuccess(true).Build();
            }
            return total >= target.Value
                ? Result.CreateBuilder("成功").SetSuccess(true).Build()
                : Result.CreateBuilder("失敗").SetFailure(true).Build();
        }

    }
}
