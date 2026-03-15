using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 無限のファンタジア
    /// </summary>
    public sealed class InfiniteFantasia : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly InfiniteFantasia Instance = new InfiniteFantasia();

        /// <inheritdoc/>
        public override string Id => "InfiniteFantasia";

        /// <inheritdoc/>
        public override string Name => "無限のファンタジア";

        /// <inheritdoc/>
        public override string SortKey => "むけんのふあんたしあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        1D20に目標値を設定した場合に、成功レベルの自動判定を行います。
        例： 1D20<=16
        ";


        private Result Result1d20(int total, int _dice_total, object cmp_op, int target, IRandomizer randomizer)
        {
            if (cmp_op?.ToString() != "<=")
            {
                return null;
            }
            if (total > target)
            {
                return Result.CreateBuilder("失敗").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
            string output;
            if (total <= target / 32)
            {
                output = "32レベル成功(32Lv+)";
            }
            else
            {
                if (total <= target / 16)
                    output = "16レベル成功(16Lv+)";
                else if (total <= target / 8)
                    output = "8レベル成功";
                else if (total <= target / 4)
                    output = "4レベル成功";
                else
                    output = total <= target / 2 ? "2レベル成功" : "1レベル成功";
            }
                if (total <= 1)
                {
                    output += "/クリティカル";
                    return Result.CreateBuilder(output)
                        .SetSuccess(true)
                        .SetCritical(true)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
                return Result.CreateBuilder(output)
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
        }

    }
}
