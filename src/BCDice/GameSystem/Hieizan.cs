using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 比叡山炎上
    /// </summary>
    public sealed class Hieizan : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Hieizan Instance = new Hieizan();

        /// <inheritdoc/>
        public override string Id => "Hieizan";

        /// <inheritdoc/>
        public override string Name => "比叡山炎上";

        /// <inheritdoc/>
        public override string SortKey => "ひえいさんえんしよう";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        大成功、自動成功、失敗、自動失敗、大失敗の自動判定を行います。
        ";


        private Result Result1d100(int total, int _dice_total, object cmp_op, int target)
        {
            if (cmp_op?.ToString() != "<=")
            {
                return null;
            }
            if (total >= 100)
            {
                return Result.CreateBuilder("大失敗").SetFumble(true).SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
            else
            {
                if (total >= 96)
                {
                    return Result.CreateBuilder("自動失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                }
                else
                {
                    if (total <= target / 5)
                    {
                        return Result.CreateBuilder("大成功").SetCritical(true).SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                    }
                    else
                    {
                        if (total <= 1)
                        {
                            return Result.CreateBuilder("自動成功").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                        }
                        else
                        {
                            return total <= target ? Result.CreateBuilder("成功").SetSuccess(true).SetRands(_randomizer!.RandResults).Build() : Result.CreateBuilder("失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                        }
                    }
                }
            }
        }

    }
}
