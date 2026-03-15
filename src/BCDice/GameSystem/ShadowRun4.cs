using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// シャドウラン 4th Edition
    /// </summary>
    public sealed class ShadowRun4 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ShadowRun4 Instance = new ShadowRun4();

        /// <inheritdoc/>
        public override string Id => "ShadowRun4";

        /// <inheritdoc/>
        public override string Name => "シャドウラン 4th Edition";

        /// <inheritdoc/>
        public override string SortKey => "しやとうらん4";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override CompareOperator? DefaultCmpOp => CompareOperator.GreaterThanOrEqual;

        /// <inheritdoc/>
        public override int? DefaultTargetNumber => 5;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        個数振り足しロール(xRn)の境界値を6にセット、バラバラロール(xBn)の目標値を5以上にセットします。
        BコマンドとRコマンド時に、グリッチの表示を行います。
        ";


        private string GrichText(int numberSpot1, int dice_cnt_total, int successCount)
        {
            var dice_cnt_total_half = 1.0 * dice_cnt_total / 2;
            Debug("dice_cnt_total_half", dice_cnt_total_half);
            if (numberSpot1 >= dice_cnt_total_half)
            {
                return null;
            }
            else
            {
                return null;
            }
            return successCount == 0 ? "クリティカルグリッチ" : "グリッチ";
        }

    }
}