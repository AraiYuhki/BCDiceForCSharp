using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Chill 3rd Edition
    /// </summary>
    public sealed class Chill3 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Chill3 Instance = new Chill3();

        /// <inheritdoc/>
        public override string Id => "Chill3";

        /// <inheritdoc/>
        public override string Name => "Chill 3rd Edition";

        /// <inheritdoc/>
        public override string SortKey => "ちる3";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・1D100で判定時に成否、Botchを判定
        　例）1D100<=50
        　　 (1D100<=50) ＞ 55 ＞ Botch
        ";


        private Result Result1d100(int total, int dice_total, object cmp_op, int target)
        {
            if (cmp_op?.ToString() != "<=")
            {
                return null;
            }
            var tens = dice_total / 10 % 10;
            var ones = dice_total % 10;
            if (tens == ones)
            {
                if (total > target || dice_total == 100)
                {
                    if (target > 100)
                    {
                        return Result.CreateBuilder("Failure").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder("Botch").SetFumble(true).SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                    }
                }
                else
                {
                    return Result.CreateBuilder("Colossal Success").SetCritical(true).SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                }
            }
            else
            {
                if (total <= target || dice_total == 1)
                {
                    if (total <= target / 2)
                    {
                        return Result.CreateBuilder("High Success").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder("Low Success").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                    }
                }
                else
                {
                    return Result.CreateBuilder("Failure").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                }
            }
        }

    }
}
