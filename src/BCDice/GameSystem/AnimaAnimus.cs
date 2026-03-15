using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// アニマアニムス
    /// </summary>
    public sealed class AnimaAnimus : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly AnimaAnimus Instance = new AnimaAnimus();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "AnimaAnimus";

        /// <inheritdoc/>
        public override string Name => "アニマアニムス";

        /// <inheritdoc/>
        public override string SortKey => "あにまあにむす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定(xAN<=y±z)
        　十面ダイスをx個振って判定します。達成値が算出されます(クリティカル発生時は2増加)。
        　x：振るダイスの数。魂魄値や攻撃値。
        　y：成功値。
        　z：成功値への補正。省略可能。
        　(例) 2AN<=3+1 5AN<=7
        ・各種表
        　情報収集表　IGT/喪失表　LT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)AN<=(\d+([+-]\d+)*)");
            if (TABLES.ContainsKey(command))
            {
                return RollTable(command, TABLES, randomizer);
            }
            else if (m.Success)
            {
                return CheckAction(m, randomizer);
            }
            else
            {
                return null;
            }
        }

        private Result? CheckAction(Match matchData, IRandomizer randomizer)
        {
            int diceCnt = ArithmeticEvaluator.Eval(matchData.Groups[1].Value, RoundType.Floor) ?? 0;
            int target = ArithmeticEvaluator.Eval(matchData.Groups[2].Value, RoundType.Floor) ?? 0;
            var diceArr = randomizer.RollBarabara(diceCnt, 10);
            var diceStr = string.Join(",", diceArr);
            var sucCnt = diceArr.Count(x => x <= target);
            var hasCritical = diceArr.Contains(1);
            var result = hasCritical ? sucCnt + 2 : sucCnt;
            return Result.CreateBuilder($"({diceCnt}B10<={target}).Build() ＞ {diceStr} ＞ {(result > 0 ? "成功" : "失敗")}(達成値:{result}){(hasCritical ? " (クリティカル発生)" : "")}")
                .SetCritical(hasCritical)
                .SetSuccess(result > 0)
                .SetFailure(result <= 0)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
