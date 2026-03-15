using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 朱の孤塔のエアゲトラム
    /// </summary>
    public sealed class Airgetlamh : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Airgetlamh Instance = new Airgetlamh();

        /// <inheritdoc/>
        public override string Id => "Airgetlamh";

        /// <inheritdoc/>
        public override string Name => "朱の孤塔のエアゲトラム";

        /// <inheritdoc/>
        public override string SortKey => "あけのことうのえあけとらむ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        【Reg2.0『THE ANSWERER』～】
        ・調査判定（成功数を表示）：[n]AA[m]
        ・命中判定（ダメージ表示）：[n]AA[m]*p[+t][Cx]
        【～Reg1.1『昇華』】
        ・調査判定（成功数を表示）：[n]AL[m]
        ・命中判定（ダメージ表示）：[n]AL[m]*p
        ----------------------------------------
        []内のコマンドは省略可能。

        「n」でダイス数（攻撃回数）を指定。省略時は「2」。
        「m」で目標値を指定。省略時は「6」。
        「p」で威力を指定。「*」は「x」で代用可。
        「+t」でクリティカルトリガーを指定。省略可。
        「Cx」でクリティカル値を指定。省略時は「1」、最大値は「3」、「0」でクリティカル無し。

        攻撃力指定で命中判定となり、成功数ではなく、ダメージを結果表示します。
        クリティカルヒットの分だけ、自動で振り足し処理を行います。
        （ALコマンドではクリティカル処理を行いません）

        【書式例】
        ・AL → 2d10で目標値6の調査判定。
        ・5AA7*12 → 5d10で目標値7、威力12の命中判定。
        ・AA7x28+5 → 2d10で目標値7、威力28、クリティカルトリガー5の命中判定。
        ・9aa5*10C2 → 9d10で目標値5、威力10、クリティカル値2の命中判定。
        ・15AAx4c0 → 15d10で目標値6、威力4、クリティカル無しの命中判定。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            int criticalTrigger = 0;
            int criticalNumber = 0;
            if (Regexp.IsMatch(command, @"(\d+)?A(A|L)(\d+)?((x|\*)(\d+)(\+(\d+))?)?(C(\d+))?$"))
            {
                var diceCount = Regexp.LastMatchInt(1, 2);
                var target = Regexp.LastMatchInt(3, 6);
                var damage = Regexp.LastMatchInt(6, 0);
                if (Regexp.LastMatch(2) == "L")
                {
                    criticalTrigger = 0;
                    criticalNumber = 0;
                }
                else
                {
                    criticalTrigger = Regexp.LastMatchInt(8, 0);
                    criticalNumber = Regexp.LastMatchInt(10, 1);
                }
                if (criticalNumber > 4)
                {
                    criticalNumber = 3;
                }
                return CheckRoll(diceCount, target, damage, criticalTrigger, criticalNumber, randomizer);
            }
            return null;
        }

        private Result? CheckRoll(int diceCount, int target, int damage, int criticalTrigger, int criticalNumber, IRandomizer randomizer)
        {
            var totalSuccessCount = 0;
            var totalCriticalCount = 0;
            var text = "";
            var rollCount = diceCount;
            while (rollCount > 0)
            {
                var diceArray = randomizer.RollBarabara(rollCount, 10).OrderBy(x => x).ToArray();
                var diceText = string.Join(",", diceArray);
                var successCount = diceArray.Count(i => i <= target);
                var criticalCount = diceArray.Count(i => i <= criticalNumber);
                totalSuccessCount += successCount;
                totalCriticalCount += criticalCount;
                if (text.Length > 0)
                {
                    text += "+";
                }
                text += $"{successCount}[{diceText}]";
                rollCount = criticalCount;
            }
            var result = "";
            var isDamage = damage != 0;
            if (isDamage)
            {
                var totalDamage = totalSuccessCount * damage + totalCriticalCount * criticalTrigger;
                result += $"({diceCount}D10<={target}) ＞ {text} ＞ Hits：{totalSuccessCount}*{damage}";
                if (criticalTrigger > 0)
                {
                    result += $" + Trigger：{totalCriticalCount}*{criticalTrigger}";
                }
                result += $" ＞ {totalDamage}ダメージ";
            }
            else
            {
                result += $"({diceCount}D10<={target}) ＞ {text} ＞ 成功数：{totalSuccessCount}";
            }
            if (totalCriticalCount > 0)
            {
                result += $" / {totalCriticalCount}クリティカル";
            }
            return Result.Success(result);
        }

    }
}
