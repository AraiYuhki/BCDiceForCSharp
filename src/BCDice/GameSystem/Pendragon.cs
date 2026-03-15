using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ペンドラゴン
    /// </summary>
    public sealed class Pendragon : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Pendragon Instance = new Pendragon();

        /// <inheritdoc/>
        public override string Id => "Pendragon";

        /// <inheritdoc/>
        public override string Name => "ペンドラゴン";

        /// <inheritdoc/>
        public override string SortKey => "へんとらこん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        クリティカル、成功、失敗、ファンブルの自動判定を行います。
        ";

        /// <summary>
        /// ゲーム別成功度判定(1d20)
        /// </summary>
        private Result? Result1d20(int total, int _diceTotal, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            if (total <= target)
            {
                if (total >= (40 - target) || total == target)
                {
                    return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
                }
                else
                {
                    return Result.CreateBuilder("成功").SetSuccess(true).Build();
                }
            }
            else
            {
                if (total == 20)
                {
                    return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    return Result.CreateBuilder("失敗").SetFailure(true).Build();
                }
            }
        }
    }
}
