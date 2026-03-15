using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// NJSLYRBATTLE
    /// </summary>
    public sealed class NjslyrBattle : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NjslyrBattle Instance = new NjslyrBattle();

        /// <inheritdoc/>
        public override string Id => "NjslyrBattle";

        /// <inheritdoc/>
        public override string Name => "NJSLYRBATTLE";

        /// <inheritdoc/>
        public override string SortKey => "にんしやすれいやあはとる";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・カラテロール
        2d6<=(カラテ点)
        例）2d6<=5
        (2D6<=5) ＞ 2[1,1] ＞ 2 ＞ 成功 重点 3 溜まる
        ";


        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != CompareOperator.LessThanOrEqual)
            {
                return null;
            }

            string baseText = total <= target ? "成功" : "失敗";
            string juutenText = Juuten(diceList);
            string fullText = baseText + juutenText;

            if (total <= target)
            {
                return Result.CreateBuilder(fullText).SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder(fullText).SetFailure(true).Build();
            }
        }

        private string Juuten(IReadOnlyList<int> diceList)
        {
            int juuten = diceList.Count(v => v == 1) + diceList.Count(v => v == 6);
            if (diceList.Count >= 2 && diceList[0] == diceList[1])
            {
                juuten += 1;
            }
            if (juuten > 0)
            {
                return $" 重点 {juuten} 溜まる";
            }
            else
            {
                return "";
            }
        }

    }
}
