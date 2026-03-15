using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エクリプス・フェイズ
    /// </summary>
    public sealed class EclipsePhase : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly EclipsePhase Instance = new EclipsePhase();

        /// <inheritdoc/>
        public override string Id => "EclipsePhase";

        /// <inheritdoc/>
        public override string Name => "エクリプス・フェイズ";

        /// <inheritdoc/>
        public override string SortKey => "えくりふすふえいす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        1D100<=m 方式の判定で成否、クリティカル・ファンブルを自動判定
        ";

        /// <summary>
        /// 1D100の判定結果を返す
        /// </summary>
        private Result? Result1d100(int total, int diceTotal, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            var diceValue = total % 100;
            var diceTenPlace = diceValue / 10;
            var diceOnePlace = diceValue % 10;

            if (diceTenPlace == diceOnePlace)
            {
                if (diceValue == 99)
                {
                    return Result.CreateBuilder("決定的失敗").SetFumble(true).SetFailure(true).Build();
                }
                if (diceValue == 0)
                {
                    return Result.CreateBuilder("00 ＞ 決定的成功").SetSuccess(true).SetCritical(true).Build();
                }
                if (total <= target)
                {
                    return Result.CreateBuilder("決定的成功").SetSuccess(true).SetCritical(true).Build();
                }
                return Result.CreateBuilder("決定的失敗").SetFumble(true).SetFailure(true).Build();
            }

            var diffThreshold = 30;

            if (total <= target)
            {
                return total >= diffThreshold
                    ? Result.CreateBuilder("エクセレント").SetSuccess(true).Build()
                    : Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return (total - target) >= diffThreshold
                    ? Result.CreateBuilder("シビア").SetFailure(true).Build()
                    : Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }
    }
}
