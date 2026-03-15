using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ビーストバインド トリニティ
    /// nBBコマンドによる判定。上位2個のダイスで達成値を算出。
    /// クリティカル時+20、ファンブル時達成値0。
    /// </summary>
    public sealed class BeastBindTrinity : GameSystemBase
    {
        public static readonly BeastBindTrinity Instance = new BeastBindTrinity();

        private static readonly Regex BbRegex = new Regex(
            @"^(\d+)(?:R6|BB6?)((?:[+\-]\d+)+)?(?:%(-?\d+))?(?:@([+\-\d]+))?(?:#(A)?([+\-\d]+))?(?:\$([1-6]+))?(?:&([1-6]))?(?:(>=)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "BeastBindTrinity";
        public override string Name => "ビーストバインド トリニティ";
        public override string SortKey => "ひいすとはいんととりにてい";
        public override D66SortType D66SortType => D66SortType.Ascending;

        public override string HelpMessage => @"・判定　(nBB+m%w@x#y$z&v)
　n個のD6を振り、出目の大きい2個から達成値を算出。修正mも可能。

　%w、@x、#y、$z、&vはすべて省略可能。
＞%w：現在の人間性が w であるとして、クリティカル値(C値)を計算。
＞@x：クリティカル値修正。
＞#y、#Ay：ファンブル値修正。#Ayだとファンブルでも達成値を通常通り算出。
＞$z：ダイスの出目をzに固定して判定する。
＞&v：出目がv未満のダイスをvとして扱う。

・D66ダイスあり
・邂逅表：EMO
";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollBb(command, randomizer);
        }

        private Result? RollBb(string command, IRandomizer randomizer)
        {
            var m = BbRegex.Match(command);
            if (!m.Success) return null;

            int diceNum = int.Parse(m.Groups[1].Value);
            int modifyNumber = m.Groups[2].Success ? EvalModifier(m.Groups[2].Value) : 0;
            int? humanity = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;
            string? atmark = m.Groups[4].Success ? m.Groups[4].Value : null;
            bool keepValueOnFumble = m.Groups[5].Success; // "A" フラグ;
            string? sharp = m.Groups[6].Success ? m.Groups[6].Value : null;
            int[] dicePool = m.Groups[7].Success
                ? m.Groups[7].Value.Select(c => c - '0').ToArray()
                : Array.Empty<int>();
            int diceValueLowerLimit = m.Groups[8].Success ? int.Parse(m.Groups[8].Value) : 0;
            string? cmpOp = m.Groups[9].Success ? ">=" : null;
            int? targetNumber = m.Groups[10].Success ? int.Parse(m.Groups[10].Value) : (int?)null;

            // ダイスプールがダイス数を超えたら切り捨て
            if (dicePool.Length > diceNum)
            {
                dicePool = dicePool.Take(diceNum).ToArray();
            }

            int criticalValue = ParseCritical(humanity, atmark);
            int fumbleValue = ParseFumble(sharp);

            // ダイスを振る（固定ダイス＋通常ダイス）
            int diceTimes = diceNum - dicePool.Length;
            var rolledDice = diceTimes > 0 ? randomizer.RollBarabara(diceTimes, 6) : Array.Empty<int>();
            var allDice = rolledDice.Concat(dicePool).OrderBy(x => x).ToArray();

            if (allDice.Length == 0)
            {
                return Result.CreateBuilder("ERROR:振るダイスの数が0個です")
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            // 下限値の適用
            int[]? originalDice = null;
            if (diceValueLowerLimit > 0)
            {
                originalDice = (int[])allDice.Clone();
                for (int i = 0; i < allDice.Length; i++)
                {
                    if (allDice[i] < diceValueLowerLimit)
                        allDice[i] = diceValueLowerLimit;
                }
                Array.Sort(allDice);
            }

            // 上位2個の合計
            int diceTotal = allDice.Length >= 2
                ? allDice[allDice.Length - 1] + allDice[allDice.Length - 2]
                : allDice[0];

            bool isCritical = diceTotal >= criticalValue;
            bool isFumble = diceTotal <= fumbleValue;

            // 達成値の計算
            int total = diceTotal + modifyNumber;
            if (isFumble && !keepValueOnFumble)
            {
                total = 0;
            }
            else if (isCritical)
            {
                total += 20;
            }
            if (total < 0) total = 0;

            // 比較結果
            bool isSuccess = false;
            bool isFailure = false;
            string? resultStr = null;

            if (cmpOp != null && targetNumber.HasValue)
            {
                if (total >= targetNumber.Value)
                {
                    isSuccess = true;
                    resultStr = "成功";
                }
                else
                {
                    isFailure = true;
                    resultStr = "失敗";
                }
            }

            // 出力テキストの構築
            string modStr = FormatModifier(modifyNumber);
            string cmdExpr = $"({diceNum}BB{modStr}@{criticalValue}#{fumbleValue}{(cmpOp != null ? $">={targetNumber}" : "")})";

            string? orgDiceStr = null;
            if (originalDice != null && !originalDice.SequenceEqual(allDice))
            {
                orgDiceStr = $"[{string.Join(",", originalDice)}]";
            }

            string interimExpr = $"{diceTotal}[{string.Join(",", allDice)}]{modStr}";
            if (isCritical) interimExpr += "+20";

            string? diceStatus = null;
            if (isFumble) diceStatus = "ファンブル";
            else if (isCritical) diceStatus = "クリティカル";

            var parts = new List<string> { cmdExpr };
            if (orgDiceStr != null) parts.Add(orgDiceStr);
            parts.Add(interimExpr);
            if (diceStatus != null) parts.Add(diceStatus);
            parts.Add(total.ToString());
            if (resultStr != null) parts.Add(resultStr);

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetSuccess(isSuccess || isCritical)
                .SetFailure(isFailure || isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static int ParseCritical(int? humanity, string? atmark)
        {
            int humanityVal = humanity ?? 99;
            int critBase = CriticalFromHumanity(humanityVal);

            if (string.IsNullOrEmpty(atmark)) return critBase;

            if (atmark.StartsWith("+") || atmark.StartsWith("-"))
            {
                int atmarkValue = EvalModifier(atmark);
                return Math.Min(critBase + atmarkValue, 12);
            }
            else
            {
                return EvalModifier("+" + atmark);
            }
        }

        private static int CriticalFromHumanity(int humanity)
        {
            if (humanity <= 0) return 9;
            if (humanity <= 20) return 10;
            if (humanity <= 40) return 11;
            return 12;
        }

        private static int ParseFumble(string? sharp)
        {
            if (string.IsNullOrEmpty(sharp)) return 2;

            if (sharp.StartsWith("+") || sharp.StartsWith("-"))
            {
                return 2 + EvalModifier(sharp);
            }
            else
            {
                return EvalModifier("+" + sharp);
            }
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            int result = 0;
            foreach (Match m in Regex.Matches(expr, @"[+\-]\d+"))
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        private static string FormatModifier(int value)
        {
            if (value == 0) return "";
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}