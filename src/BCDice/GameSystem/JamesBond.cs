using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ジェームズ・ボンド007
    /// </summary>
    public sealed class JamesBond : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly JamesBond Instance = new JamesBond();

        /// <inheritdoc/>
        public override string Id => "JamesBond";

        /// <inheritdoc/>
        public override string Name => "ジェームズ・ボンド007";

        /// <inheritdoc/>
        public override string SortKey => "しええむすほんと007";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・1D100の目標値判定で、効果レーティングを1～4で自動判定。
        　例）1D100<=50
        　　　JamesBond : (1D100<=50) ＞ 20 ＞ 効果3（良）
        ";


        private Result Result1d100(int total, int _dice_total, object cmp_op, int target)
        {
            if (cmp_op?.ToString() != "<=")
            {
                return null;
            }
            var @base = (int)Math.Floor((double)(target + 9) / 10);
            if (total >= 100)
            {
                return Result.CreateBuilder("失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
            else
            {
                if (total <= @base)
                {
                    return Result.CreateBuilder("効果1（完璧）").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                }
                else
                {
                    if (total <= @base * 2)
                    {
                        return Result.CreateBuilder("効果2（かなり良い）").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                    }
                    else
                    {
                        if (total <= @base * 5)
                        {
                            return Result.CreateBuilder("効果3（良）").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
                        }
                        else
                        {
                            return total <= target ? Result.CreateBuilder("効果4（まあまあ）").SetSuccess(true).SetRands(_randomizer!.RandResults).Build() : Result.CreateBuilder("失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
                        }
                    }
                }
            }
        }

    }
}
