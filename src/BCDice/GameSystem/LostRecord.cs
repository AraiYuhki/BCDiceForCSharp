using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ロストレコード
    /// </summary>
    public sealed class LostRecord : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly LostRecord Instance = new LostRecord();

        /// <inheritdoc/>
        public override string Id => "LostRecord";

        /// <inheritdoc/>
        public override string Name => "ロストレコード";

        /// <inheritdoc/>
        public override string SortKey => "ろすとれこおと";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ※このダイスボットは部屋のシステム名表示用となります。
        D66を振った時、小さい目が十の位になります。
        ";


    }
}