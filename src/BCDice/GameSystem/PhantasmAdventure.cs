using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ファンタズム・アドベンチャー
    /// </summary>
    public sealed class PhantasmAdventure : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly PhantasmAdventure Instance = new PhantasmAdventure();

        /// <inheritdoc/>
        public override string Id => "PhantasmAdventure";

        /// <inheritdoc/>
        public override string Name => "ファンタズム・アドベンチャー";

        /// <inheritdoc/>
        public override string SortKey => "ふあんたすむあとへんちやあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        成功、失敗、決定的成功、決定的失敗の表示とクリティカル・ファンブル値計算の実装。
        ";


        private Result? Result1d20(int total, int _diceTotal, CompareOperator? cmpOp, int? diff, IRandomizer randomizer)
        {
            if (diff == null)
            {
                return Result.CreateBuilder("").Build();
            }
            if (cmpOp != CompareOperator.LessThanOrEqual)
            {
                return null;
            }

            var diffVal = diff.Value;

            var skill_mod = 0;
            if (diffVal < 1)
            {
                skill_mod = diffVal - 1;
            }
            else if (diffVal > 20)
            {
                skill_mod = diffVal - 20;
            }

            var fumble = 20 + skill_mod;
            if (fumble > 20)
            {
                fumble = 20;
            }
            var critical = 1 + skill_mod;
            var dice_now = randomizer.RollOnce(20);

            if (total >= fumble || total >= 20)
            {
                var fum_num = dice_now - skill_mod;
                if (fum_num > 20)
                {
                    fum_num = 20;
                }
                if (fum_num < 1)
                {
                    fum_num = 1;
                }
                var fum_str = dice_now.ToString();
                if (skill_mod < 0)
                {
                    fum_str = $"+{skill_mod * -1}={fum_num}";
                }
                else
                {
                    fum_str = $"-{skill_mod}={fum_num}";
                }
                return Result.CreateBuilder($"致命的失敗({fum_str}).Build()").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= critical || total <= 1)
            {
                var crit_num = dice_now + skill_mod;
                if (crit_num > 20)
                {
                    crit_num = 20;
                }
                if (crit_num < 1)
                {
                    crit_num = 1;
                }
                if (skill_mod < 0)
                {
                    return Result.CreateBuilder("成功").SetSuccess(true).Build();
                }
                return Result.CreateBuilder($"決定的成功({dice_now}+{skill_mod}={crit_num}).Build()").SetCritical(true).SetSuccess(true).Build();
            }
            else if (total <= diffVal)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

    }
}
