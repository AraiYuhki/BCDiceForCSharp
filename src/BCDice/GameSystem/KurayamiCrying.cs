using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// クラヤミクライン
    /// </summary>
    public sealed class KurayamiCrying : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KurayamiCrying Instance = new KurayamiCrying();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "KurayamiCrying";

        /// <inheritdoc/>
        public override string Name => "クラヤミクライン";

        /// <inheritdoc/>
        public override string SortKey => "くらやみくらいん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・アクシデント表（ACT）
        ・感情表（EMO）
        ・幕間シーン表（情景）（SST）
        ・幕間シーン表（関係）（SRT）
        ■ ヨイヤミメビウス
        ・大事故イベント表1　日常と正気の破壊（SAE1）
        ・大事故イベント表2　アメリカンB級ホラー（SAE2）
        ・大事故イベント表3　牙をむく異世界（SAE3）
        ・大事故イベント表4　悪意のおとぎ話（SAE4）
        ・大事故イベント表5　悲劇の運命（SAE5）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollTable(command, TABLES, randomizer)
                ?? base.EvalGameSystemSpecificCommand(command, randomizer);
        }

    }
}