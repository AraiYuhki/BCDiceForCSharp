using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ヨタバナ
    /// </summary>
    public sealed class Yotabana : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Yotabana Instance = new Yotabana();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "Yotabana";

        /// <inheritdoc/>
        public override string Name => "ヨタバナ";

        /// <inheritdoc/>
        public override string SortKey => "よたはな";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ▪️ 各種表
          COT 収束表
          EVT イベント表
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            RollTable(command, TABLES, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

    }
}