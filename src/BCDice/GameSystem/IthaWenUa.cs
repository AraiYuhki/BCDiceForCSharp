using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// イサー・ウェン＝アー
    /// </summary>
    public sealed class IthaWenUa : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly IthaWenUa Instance = new IthaWenUa();

        /// <inheritdoc/>
        public override string Id => "IthaWenUa";

        /// <inheritdoc/>
        public override string Name => "イサー・ウェン＝アー";

        /// <inheritdoc/>
        public override string SortKey => "いさあうえんああ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        1D100<=m 方式の判定で成否、クリティカル(01)・ファンブル(00)を自動判定します。
        ";


        private Result Result1d100(int total, int _dice_total, object cmp_op, int target)
        {
            if (cmp_op?.ToString() != "<=")
            {
                return null;
            }
            if (total % 100 == 1)
            {
                return Result.CreateBuilder("01 ＞ クリティカル").SetCritical(true).SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
            }
            else
            {
                if (total % 100 == 0)
                {
                    return Result.CreateBuilder("00 ＞ ファンブル").SetFumble(true).SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                }
                else
                {
                    return total <= target
                        ? Result.CreateBuilder("成功").SetSuccess(true).SetRands(_randomizer!.RandResults).Build()
                        : Result.CreateBuilder("失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                }
            }
        }

    }
}
