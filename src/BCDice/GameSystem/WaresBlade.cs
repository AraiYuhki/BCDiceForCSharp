using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ワースブレイド
    /// </summary>
    public sealed class WaresBlade : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly WaresBlade Instance = new WaresBlade();

        /// <inheritdoc/>
        public override string Id => "WaresBlade";

        /// <inheritdoc/>
        public override string Name => "ワースブレイド";

        /// <inheritdoc/>
        public override string SortKey => "わあすふれいと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        nD10>=m 方式の判定で成否、完全成功、完全失敗を自動判定します。
        ";

        /// <summary>
        /// nD10の結果判定
        /// </summary>
        private Result? ResultNd10(int total, int diceTotal, int[] diceList, string cmpOp, int target)
        {
            if (cmpOp != ">=")
            {
                return null;
            }

            if (diceList.Count(d => d == 10) == diceList.Length)
            {
                return Result.CreateBuilder("完全成功").SetSuccess(true).SetCritical(true).Build();
            }
            else if (diceList.Count(d => d == 1) == diceList.Length)
            {
                return Result.CreateBuilder("絶対失敗").SetFumble(true).SetFailure(true).Build();
            }

            return null;
        }
    }
}
