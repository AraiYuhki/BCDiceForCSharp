using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class CthulhuTech : GameSystemBase
    {
        public static readonly CthulhuTech Instance = new CthulhuTech();

        public override string Id => "CthulhuTech";
        public override string Name => "クトゥルフテック";
        public override string SortKey => "くとうるふてつく";

        private static readonly Regex TestReg = new Regex(
            @"^(\d+)D10((?:[-+]\d+)+)?(>=?)(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var node = Parse(command);
            if (node == null) return null;
            return node.Execute(randomizer);
        }

        private TestNode? Parse(string command)
        {
            var m = TestReg.Match(command);
            if (!m.Success) return null;
            int num = int.Parse(m.Groups[1].Value);
            int modifier = 0;
            if (m.Groups[2].Success)
            {
                // Parse modifier like +2-4 -> sum of each part
                string modStr = m.Groups[2].Value;
                var parts = System.Text.RegularExpressions.Regex.Split(modStr, @"(?=[-+])");
                foreach (var p in parts)
                    if (!string.IsNullOrEmpty(p)) modifier += int.Parse(p);
            }
            bool isContest = m.Groups[3].Value == ">";
            int difficulty = int.Parse(m.Groups[4].Value);
            return isContest
                ? (TestNode)new ContestNode(num, modifier, difficulty)
                : new TestNode(num, modifier, difficulty);
        }

        private class TestNode
        {
            protected readonly int Num, Modifier, Difficulty;
            protected virtual string CompareOp => ">=";

            public TestNode(int num, int mod, int diff)
            {
                Num = num; Modifier = mod; Difficulty = diff;
            }

            public Result Execute(IRandomizer randomizer)
            {
                var diceValues = randomizer.RollBarabara(Num, 10);
                bool fumble = diceValues.Count(v => v == 1) >= (diceValues.Length + 1) / 2;
                var sorted = diceValues.OrderBy(x => x).ToList();
                int rollResult = CalculateRollResult(sorted);
                int testValue = rollResult + Modifier;
                int diff = testValue - Difficulty;
                bool success = !fumble && CompareResult(diff);
                bool critical = diff >= 10;

                var parts = new List<string>
                {
                    $"({Expression()})",
                    TestValueExpression(sorted, rollResult),
                    testValue.ToString(),
                    GetResultStr(success, fumble, critical, diff),
                };

                string text = string.Join($" ＞ ", parts);
                return BuildResult(text, success, fumble, critical, diff);
            }

            protected virtual bool CompareResult(int diff) => diff >= 0;

            protected virtual Result BuildResult(string text, bool success, bool fumble, bool critical, int diff)
            {
                return Result.CreateBuilder(text)
                    .SetCondition(success)
                    .SetCritical(critical)
                    .SetFumble(fumble)
                    .Build();
            }

            protected virtual string Expression()
            {
                string modStr = FormatModifier(Modifier);
                return $"{Num}D10{modStr}{CompareOp}{Difficulty}";
            }

            private string TestValueExpression(List<int> diceValues, int rollResult)
            {
                string diceStr = string.Join(",", diceValues);
                string modStr = FormatModifier(Modifier);
                return $"{rollResult}[{diceStr}]{modStr}";
            }

            protected virtual string GetResultStr(bool success, bool fumble, bool critical, int diff)
            {
                if (fumble) return "ファンブル";
                if (critical) return "クリティカル";
                return success ? "成功" : "失敗";
            }

            private static string FormatModifier(int mod)
            {
                if (mod == 0) return "";
                return mod > 0 ? $"+{mod}" : $"{mod}";
            }

            private static int CalculateRollResult(List<int> sorted)
            {
                int highest = sorted.Last();
                int highestSet = sorted.GroupBy(v => v).Select(g => g.Sum()).Max();
                int highestStraight = SumOfLargestStraight(sorted);
                return Math.Max(highest, Math.Max(highestSet, highestStraight));
            }

            private static int SumOfLargestStraight(List<int> sorted)
            {
                if (sorted.Count < 3) return 0;
                int maxSum = 0;
                int nConsec = 0, sum = 0, last = -1;
                foreach (int v in sorted.Distinct())
                {
                    if (v - last > 1)
                    {
                        nConsec = 1; sum = v; last = v;
                        continue;
                    }
                    nConsec++; sum += v; last = v;
                    if (nConsec >= 3 && sum > maxSum) maxSum = sum;
                }
                return maxSum;
            }
        }

        private sealed class ContestNode : TestNode
        {
            public ContestNode(int num, int mod, int diff) : base(num, mod, diff) { }

            protected override string CompareOp => ">";
            protected override bool CompareResult(int diff) => diff > 0;

            protected override string GetResultStr(bool success, bool fumble, bool critical, int diff)
            {
                string base_ = base.GetResultStr(success, fumble, critical, diff);
                if (success)
                {
                    int dmgDice = (int)Math.Ceiling(diff / 5.0);
                    return $"{base_}(ダメージ：{dmgDice}D10)";
                }
                return base_;
            }
        }
    }
}
