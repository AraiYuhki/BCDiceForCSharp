using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 叛逆レゾンデートル
    /// </summary>
    public sealed class Raisondetre : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Raisondetre Instance = new Raisondetre();

        /// <inheritdoc/>
        public override string Id => "Raisondetre";

        /// <inheritdoc/>
        public override string Name => "叛逆レゾンデートル";

        /// <inheritdoc/>
        public override string SortKey => "はんきやくれそんてとおる";

        /// <inheritdoc/>
        public bool SortAddDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        判定：[判定値]RD[技能][@目標値]
        ダメージロール：[ダイス数]DD[装甲]

        []内のコマンドは省略可能。
        「判定値」で判定に使用するダイス数を指定。省略時は「1」。0以下も指定可。
        「技能」で有効なダイス数を指定。省略時は「1」。
        達成値はクリティカルを含めて、「最も高くなる」ように計算します。
        「@目標値」指定で、判定の成否を追加表示します。

        ダメージロールは[装甲]指定で、有効なダイス数と0の出目の数を表示します。
        [装甲]省略時は、ダイス結果のみ表示します。（複数の対象への攻撃時用）

        【書式例】
        ・RD → 1Dで達成値を表示。
        ・2RD1@8 → 2D（1個選択）で目標値8の判定。
        ・-3RD → 1Dでダイスペナルティ-4の判定。
        ・4DD2 → 4Dで装甲2のダメージロール。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            int diceCount = 0;
            Match m;

            m = Regex.Match(command, @"^(-)?(\d+)?RD(\d+)?(@(\d+))?$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                diceCount = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
                if (m.Groups[1].Success)
                {
                    diceCount *= -1;
                }
                var choiceCount = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 1;
                var target = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
                return CheckRoll(diceCount, choiceCount, target, randomizer);
            }

            m = Regex.Match(command, @"^(-)?(\d+)?DD([1-9])?([+-]\d+)?$", RegexOptions.IgnoreCase);
            if (m.Success)
            {
                diceCount = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
                if (m.Groups[1].Success)
                {
                    diceCount *= -1;
                }
                var armor = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
                if (armor > 0)
                {
                    armor += m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 0;
                    if (armor < 1) armor = 1;
                    if (armor > 9) armor = 9;
                }
                return CheckDamage(diceCount, armor, randomizer);
            }

            return null;
        }

        private Result? CheckRoll(int diceCount, int choiceCount, int target, IRandomizer randomizer)
        {
            int correction;
            int rollCount;
            if (diceCount <= 0)
            {
                correction = 1 + diceCount * -1;
                rollCount = 1;
            }
            else
            {
                correction = 0;
                rollCount = diceCount;
            }

            var diceArray = randomizer.RollBarabara(rollCount, 10).OrderBy(x => x).ToList();
            var diceText = string.Join(",", diceArray);

            // 10 -> 0 に変換
            diceArray = diceArray.Select(x => x == 10 ? 0 : x).ToList();
            // correction を引く
            diceArray = diceArray.Select(i => i - correction).ToList();
            var diceText2 = string.Join(",", diceArray.OrderBy(x => x));

            var funbleArray = diceArray.Where(i => i <= 1).ToList();
            var isFunble = funbleArray.Count >= rollCount;

            var dice = 0;
            var success = 0;
            var criticalCount = 0;
            var critical = 0;
            var choiceText = "";

            if (!isFunble)
            {
                criticalCount = diceArray.Count(x => x == 0);
                critical = criticalCount * 10;

                var choiceArray = diceArray.OrderByDescending(x => x).ToList();
                choiceArray.RemoveAll(x => x == 0);
                choiceArray = choiceArray.Take(choiceCount).ToList();
                choiceText = string.Join(",", choiceArray);
                dice = choiceArray.Sum();
                success = dice + critical;
            }

            var result = $"{rollCount}D10";
            if (correction > 0)
            {
                result += $"-{correction}";
            }
            result += $" ＞ [{diceText}] ＞ [{diceText2}] ＞ ";

            if (isFunble)
            {
                result += "達成値：0 (Funble)";
            }
            else
            {
                result += $"{dice}[{choiceText}]";
                if (critical > 0)
                {
                    result += $"+{critical}";
                }
                result += $"=達成値：{success}";
                if (critical > 0)
                {
                    result += $" ({criticalCount}Critical)";
                }
            }

            if (target > 0)
            {
                result += $">={target} ";
                if (success >= target)
                {
                    result += "【成功】";
                }
                if (success < target)
                {
                    result += "【失敗】";
                }
            }

            return Result.CreateBuilder(result).Build();
        }

        private Result? CheckDamage(int diceCount, int armor, IRandomizer randomizer)
        {
            int correction;
            int rollCount;
            if (diceCount <= 0)
            {
                correction = 1 + diceCount * -1;
                rollCount = 1;
            }
            else
            {
                correction = 0;
                rollCount = diceCount;
            }

            var diceList = randomizer.RollBarabara(rollCount, 10).OrderBy(x => x).ToList();
            var diceText = string.Join(",", diceList);

            var diceArray = diceList.Select(x => x == 10 ? 0 : x).OrderBy(x => x).ToList();
            var criticalCount = diceArray.Count(x => x == 0);
            diceArray = diceArray.Select(i => i - correction).ToList();
            var diceText2 = string.Join(",", diceArray);

            var result = $"{rollCount}D10";
            if (correction > 0)
            {
                result += $"-{correction}";
            }
            result += $" ＞ [{diceText}] ＞ [{diceText2}]";

            if (armor > 0)
            {
                var resultArray = new List<string>();
                var success = 0;

                foreach (var i in diceArray)
                {
                    if (i >= armor)
                    {
                        resultArray.Add(i.ToString());
                        success += 1;
                    }
                    else
                    {
                        resultArray.Add("\u00d7");
                    }
                }
                var resultText = string.Join(",", resultArray);
                result += $" ＞ [{resultText}]>={armor} 有効数：{success}";
            }

            result += $"\u30000={criticalCount}個";
            return Result.CreateBuilder(result).Build();
        }

    }
}
