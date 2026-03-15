using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 詩片のアルセット
    /// </summary>
    public sealed class Alsetto : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Alsetto Instance = new Alsetto();

        /// <inheritdoc/>
        public override string Id => "Alsetto";

        /// <inheritdoc/>
        public override string Name => "詩片のアルセット";

        /// <inheritdoc/>
        public override string SortKey => "うたかたのあるせつと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・成功判定：nAL[m]　　　　・トライアンフ無し：nALC[m]
        ・命中判定：nAL[m]*p　　　・トライアンフ無し：nALC[m]*p
        ・命中判定（ガンスリンガーの根源詩）：nALG[m]*p
        []内は省略可能。

        ALコマンドはトライアンフの分だけ、自動で振り足し処理を行います。
        「n」でダイス数を指定。
        「m」で目標値を指定。省略時は、デフォルトの「3」が使用されます。
        「p」で攻撃力を指定。「*」は「x」でも可。
        攻撃力指定で命中判定となり、成功数ではなく、ダメージを結果表示します。

        ALCコマンドはトライアンフ無しで、成功数、ダメージを結果表示します。
        ALGコマンドは「2以下」でトライアンフ処理を行います。

        【書式例】
        ・5AL → 5d6で目標値3。
        ・5ALC → 5d6で目標値3。トライアンフ無し。
        ・6AL2 → 6d6で目標値2。
        ・4AL*5 → 4d6で目標値3、攻撃力5の命中判定。
        ・7AL2x10 → 7d6で目標値2、攻撃力10の命中判定。
        ・8ALC4x5 → 8d6で目標値4、攻撃力5、トライアンフ無しの命中判定。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            int criticalNumber = 0;
            if (Regexp.IsMatch(command, @"(\d+)AL(C|G)?(\d+)?((x|\*)(\d+))?$"))
            {
                var rapid = Regexp.LastMatchInt(1);
                var isCritical = Regexp.LastMatch(2) == null;
                if (isCritical)
                {
                    criticalNumber = 1;
                }
                else
                {
                    if (Regexp.LastMatch(2) == "G")
                    {
                        isCritical = true;
                        criticalNumber = 2;
                    }
                    else
                    {
                        criticalNumber = 0;
                    }
                }
                var target = Regexp.LastMatchInt(3, 3);
                var damage = Regexp.LastMatchInt(6, 0);
                return CheckRoll(rapid, target, damage, isCritical, criticalNumber, randomizer);
            }
            return null;
        }

        private Result? CheckRoll(int rapid, int target, int damage, bool isCritical, int criticalNumber, IRandomizer randomizer)
        {
            string result = "";
            var totalSuccessCount = 0;
            var totalCriticalCount = 0;
            var text = "";
            var rollCount = rapid;
            while (rollCount > 0)
            {
                var diceArray = randomizer.RollBarabara(rollCount, 6).OrderBy(x => x).ToArray();
                var diceText = string.Join(",", diceArray);
                var successCount = 0;
                var criticalCount = 0;
                foreach (var i in diceArray)
                {
                    if (i <= target)
                    {
                        successCount += 1;
                    }
                    if (i <= criticalNumber)
                    {
                        criticalCount += 1;
                    }
                }
                totalSuccessCount += successCount;
                if (criticalCount != 0)
                {
                    totalCriticalCount += 1;
                }
                if (text.Length > 0)
                {
                    text += "+";
                }
                text += $"{successCount}[{diceText}]";
                if (!isCritical)
                {
                    break;
                }
                rollCount = criticalCount;
            }
            var isDamage = damage != 0;
            if (isDamage)
            {
                var totalDamage = totalSuccessCount * damage;
                result = $"({rapid}D6<={target}) ＞ {text} ＞ Hits：{totalSuccessCount}*{damage} ＞ {totalDamage}ダメージ";
            }
            else
            {
                result = $"({rapid}D6<={target}) ＞ {text} ＞ 成功数：{totalSuccessCount}";
            }
            if (isCritical)
            {
                result += $" / {totalCriticalCount}トライアンフ";
            }
            return Result.Success(result);
        }

    }
}
