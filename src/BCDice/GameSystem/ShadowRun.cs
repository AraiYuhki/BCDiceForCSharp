using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// シャドウラン
    /// </summary>
    public sealed class ShadowRun : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ShadowRun Instance = new ShadowRun();

        /// <inheritdoc/>
        public override string Id => "ShadowRun";

        /// <inheritdoc/>
        public override string Name => "シャドウラン";

        /// <inheritdoc/>
        public override string SortKey => "しやとうらん";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        上方無限ロール(xUn)の境界値を6にセットします。
        ";


    }
}