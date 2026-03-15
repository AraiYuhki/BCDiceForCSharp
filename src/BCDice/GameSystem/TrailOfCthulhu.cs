using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トレイル・オブ・クトゥルー
    /// </summary>
    public sealed class TrailOfCthulhu : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TrailOfCthulhu Instance = new TrailOfCthulhu();

        /// <inheritdoc/>
        public override string Id => "TrailOfCthulhu";

        /// <inheritdoc/>
        public override string Name => "トレイル・オブ・クトゥルー";

        /// <inheritdoc/>
        public override string SortKey => "とれいるおふくとうるう";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■技能判定　TCb[>=t]   b:消費プール・ポイント t:難易度(省略可能)

        例)TC2>=5:消費プール・ポイント2,難易度5で技能判定し、その結果を表示する。
           TC>=3: 難易度3で技能判定し、その結果を表示する。
           TC:    難易度指定せずに技能判定する。
           TC3:   消費プール・ポイント3,難易度指定せずに技能判定する。

        ■神話的狂気表　MMT[a,b]   a,b:除外する神話的狂気(省略時は全神話的狂気を表示する)

        例)MMT[1,8]: 神話的狂気のうち、1番と8番を除外してロールし、神話的狂気を決定する。
           MMT2,6:   神話的狂気のうち、2番と6番を除外してロールし、神話的狂気を決定する。
           MMT:      神話的狂気を1番から8番まで列挙する。

        ";

        private static readonly string[] MITHOS_MADDNESS = new[]
        {
            "1:強迫性障害",
            "2:恐怖症",
            "3:誇大妄想狂",
            "4:殺人狂",
            "5:恣意的記憶喪失",
            "6:多重人格障害",
            "7:偏執症",
            "8:妄想症",
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? RollMythosMadnessTable(command, randomizer);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^TC([+\d]*)(>=(\d+))?");
            if (!m.Success)
            {
                return null;
            }

            int bonus;
            if (string.IsNullOrEmpty(m.Groups[1].Value))
            {
                bonus = 0;
            }
            else
            {
                bonus = ArithmeticEvaluator.Eval(m.Groups[1].Value, this.RoundType) ?? 0;
            }

            var difficulty = m.Groups[3].Success && !string.IsNullOrEmpty(m.Groups[3].Value)
                ? Convert.ToInt32(m.Groups[3].Value)
                : 0;

            var dice = randomizer.RollOnce(6);
            var total = dice + bonus;

            var sequence = new List<string>();
            bool isSuccess = false;
            bool isFailure = false;

            if (difficulty > 0)
            {
                isSuccess = total >= difficulty;
                isFailure = !isSuccess;
                sequence.Add($"(TC{bonus}>={difficulty})");
                sequence.Add($"{dice}+{bonus}");
                sequence.Add(total.ToString());
                sequence.Add(isSuccess ? "成功" : "失敗");
            }
            else
            {
                sequence.Add($"(TC{bonus})");
                sequence.Add($"{dice}+{bonus}");
                sequence.Add(total.ToString());
            }

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .Build();
        }

        private Result? RollMythosMadnessTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^MMT(\[?([1-8],[1-8])\]?)?");
            if (!m.Success)
            {
                return null;
            }

            var sequence = new List<string>();
            string resultText;

            if (!string.IsNullOrEmpty(m.Groups[1].Value))
            {
                var exclusionNumber = m.Groups[2].Value.Split(',');
                if (exclusionNumber.Length != 2)
                {
                    return null;
                }

                sequence.Add($"(MMT[{string.Join(",", exclusionNumber)}])");
                var ex0 = Convert.ToInt32(exclusionNumber[0]);
                var ex1 = Convert.ToInt32(exclusionNumber[1]);

                int idx;
                do
                {
                    idx = randomizer.RollOnce(8);
                } while (idx == ex0 || idx == ex1);

                resultText = MITHOS_MADDNESS[idx - 1];
            }
            else
            {
                sequence.Add("(MMT)");
                var allMadness = new List<string>();
                for (var i = 1; i <= 8; i++)
                {
                    allMadness.Add(MITHOS_MADDNESS[i - 1]);
                }
                resultText = string.Join(", ", allMadness);
            }

            sequence.Add(resultText);
            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }
    }
}
