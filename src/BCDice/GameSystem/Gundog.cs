using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガンドッグ
    /// </summary>
    public sealed class Gundog : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Gundog Instance = new Gundog();

        /// <inheritdoc/>
        public override string Id => "Gundog";

        /// <inheritdoc/>
        public override string Name => "ガンドッグ";

        /// <inheritdoc/>
        public override string SortKey => "かんとつく";

        /// <inheritdoc/>
        public override string HelpMessage => @"
失敗、成功、クリティカル、ファンブルとロールの達成値の自動判定を行います。
nD9ロールも対応。
";

        /// <summary>
        /// 1d100の判定結果を返す
        /// </summary>
        private Result? Result1d100(int total, int _diceTotal, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            if (total >= 100)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= 1)
            {
                return Result.CreateBuilder("絶対成功(達成値1+SL)").SetSuccess(true).SetCritical(true).Build();
            }
            else if (target.ToString() == "?")
            {
                return Result.CreateBuilder("").Build();
            }
            else if (total <= target)
            {
                var dig10 = total / 10;
                var dig1 = total - dig10 * 10;
                if (dig10 >= 10)
                {
                    dig10 = 0;
                }
                if (dig1 >= 10)
                {
                    dig1 = 0;
                }

                if (dig1 <= 0)
                {
                    return Result.CreateBuilder("クリティカル(達成値20+SL)").SetSuccess(true).SetCritical(true).Build();
                }
                else
                {
                    return Result.CreateBuilder($"成功(達成値{dig10 + dig1}+SL)").SetSuccess(true).Build();
                }
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }
    }
}
