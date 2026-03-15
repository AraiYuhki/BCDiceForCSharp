using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ワープス
    /// </summary>
    public sealed class WARPS : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly WARPS Instance = new WARPS();

        /// <inheritdoc/>
        public override string Id => "WARPS";

        /// <inheritdoc/>
        public override string Name => "ワープス";

        /// <inheritdoc/>
        public override string SortKey => "わあふす";

        /// <inheritdoc/>
        public override string HelpMessage => "失敗、成功度の自動判定を行います。\n";

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        private Result? Result2d6(int total, int diceTotal, IReadOnlyList<int> diceList, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            if (diceTotal <= 2)
            {
                return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (target.ToString() == "?")
            {
                return Result.CreateBuilder("").Build();
            }
            else if (total <= target)
            {
                return Result.CreateBuilder($"{target - total}成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }
    }
}
