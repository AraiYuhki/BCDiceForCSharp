using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エルリック！
    /// </summary>
    public sealed class Elric : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Elric Instance = new Elric();

        /// <inheritdoc/>
        public override string Id => "Elric";

        /// <inheritdoc/>
        public override string Name => "エルリック！";

        /// <inheritdoc/>
        public override string SortKey => "えるりつく";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        貫通、クリティカル、ファンブルの自動判定を行います。
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

            if (total <= 1)
            {
                return Result.CreateBuilder("貫通").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total >= 100)
            {
                return Result.CreateBuilder("致命的失敗").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= (int)Math.Ceiling(target / 5.0))
            {
                return Result.CreateBuilder("決定的成功").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total <= target)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else if (total >= 99 && target < 100)
            {
                return Result.CreateBuilder("致命的失敗").SetFumble(true).SetFailure(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }
    }
}
