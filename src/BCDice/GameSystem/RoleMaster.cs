using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ロールマスター
    /// </summary>
    public sealed class RoleMaster : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RoleMaster Instance = new RoleMaster();

        /// <inheritdoc/>
        public override string Id => "RoleMaster";

        /// <inheritdoc/>
        public override string Name => "ロールマスター";

        /// <inheritdoc/>
        public override string SortKey => "ろおるますたあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        上方無限ロール(xUn)の境界値を96にセットします。
        ";


    }
}