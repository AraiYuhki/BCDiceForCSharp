using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Shared+Fantasia
    /// </summary>
    public sealed class SharedFantasia : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SharedFantasia Instance = new SharedFantasia();

        /// <inheritdoc/>
        public override string Id => "SharedFantasia";

        /// <inheritdoc/>
        public override string Name => "Shared\u2020Fantasia";

        /// <inheritdoc/>
        public override string SortKey => "しえああとふあんたしあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        2D6の成功判定に 自動成功、自動失敗、致命的失敗、劇的成功 の判定があります。

        SF/ST = 2D6のショートカット

        例) SF+4>=9 : 2D6して4を足した値が9以上なら成功
        ";


        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != CompareOperator.GreaterThanOrEqual && cmpOp != CompareOperator.GreaterThan)
            {
                return null;
            }

            var critical = false;
            var fumble = false;

            if (diceTotal == 12)
            {
                critical = true;
            }
            else if (diceTotal == 2)
            {
                fumble = true;
            }

            var totalValueBonus = cmpOp == CompareOperator.GreaterThanOrEqual ? 1 : 0;

            if (total + totalValueBonus > target)
            {
                if (critical)
                {
                    return Result.CreateBuilder("自動成功(劇的成功)").SetSuccess(true).SetCritical(true).Build();
                }
                else if (fumble)
                {
                    return Result.CreateBuilder("自動失敗").SetFailure(true).Build();
                }
                else
                {
                    return Result.CreateBuilder("成功").SetSuccess(true).Build();
                }
            }
            else
            {
                if (critical)
                {
                    return Result.CreateBuilder("自動成功").SetSuccess(true).Build();
                }
                else if (fumble)
                {
                    return Result.CreateBuilder("自動失敗(致命的失敗)").SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    return Result.CreateBuilder("失敗").SetFailure(true).Build();
                }
            }
        }

        /// <summary>
        /// SF/ST を 2D6 に変換する
        /// </summary>
        public override string ChangeText(string str)
        {
            return Regex.Replace(str, @"S[FT]", "2D6", RegexOptions.IgnoreCase);
        }

    }
}
