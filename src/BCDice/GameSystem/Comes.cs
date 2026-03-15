using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// カムズ
    /// </summary>
    public sealed class Comes : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Comes Instance = new Comes();

        /// <inheritdoc/>
        public override string Id => "Comes";

        /// <inheritdoc/>
        public override string Name => "カムズ";

        /// <inheritdoc/>
        public override string SortKey => "かむす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・各種表
        　判定ペナルティ表 PT
        ";

        /// <summary>各種テーブル</summary>
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollTable(command, TABLES, randomizer);
        }

    }
}
