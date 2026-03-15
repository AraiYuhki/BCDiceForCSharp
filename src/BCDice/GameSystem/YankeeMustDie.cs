using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ヤンキーマストダイ
    /// </summary>
    public sealed class YankeeMustDie : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly YankeeMustDie Instance = new YankeeMustDie();

        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES =
            new System.Collections.Generic.Dictionary<string, object>();

        private static readonly Regex YdCommandRegex = new Regex(
            @"^YD((?:[+\-]\d+)*)(?:(>=|>)(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "YankeeMustDie";

        /// <inheritdoc/>
        public override string Name => "ヤンキーマストダイ";

        /// <inheritdoc/>
        public override string SortKey => "やんきいますとたい";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override int SidesImplicitD => 10;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定の方法
          基本形式：
          (YD+a>=b) または (YD+a>b)
          a：能力値、技能レベル、ドウグ等による修正値(複数指定可)
          b：目標となる成功段階

        ■ 成功の条件
          >=b：目標となる成功段階b以上の場合に成功となります。この条件では、目標となる成功段階と同じ数値でも成功とみなされます。
          >b：目標となる成功段階bより高い成功段階を出した場合に成功となります。この条件では、目標となる成功段階と同じ数値では失敗となります。

        ■ 各種表
        　関係表 RT
        　場面表 ST
        　ハプニング表 HT
        　闇堕ち表 DT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = YdCommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            // Parse modify number from groups like "+3-1+2"
            string modifyStr = m.Groups[1].Value;
            int modifyNumber = 0;
            if (!string.IsNullOrEmpty(modifyStr))
            {
                var modParts = Regex.Matches(modifyStr, @"[+\-]\d+");
                foreach (Match part in modParts)
                {
                    modifyNumber += int.Parse(part.Value);
                }
            }

            string? cmpOp = m.Groups[2].Success ? m.Groups[2].Value : null;
            int? targetNumber = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;

            // Roll 3D10, repeat if all three dice are identical
            var diceAll = new List<int[]>();
            while (true)
            {
                int[] diceList = randomizer.RollBarabara(3, 10).OrderBy(x => x).ToArray();
                diceAll.Add(diceList);
                // Break unless all dice are the same (uniq.one? means only one unique value)
                if (diceList.Distinct().Count() != 1)
                {
                    break;
                }
            }

            int diceSum = diceAll.SelectMany(x => x).Sum();
            int achievementValue = diceSum + modifyNumber;

            int successLevel;
            if (achievementValue <= 9)
                successLevel = 0;
            else if (achievementValue <= 19)
                successLevel = 1;
            else if (achievementValue <= 29)
                successLevel = 2;
            else if (achievementValue <= 39)
                successLevel = 3;
            else if (achievementValue <= 49)
                successLevel = 4;
            else if (achievementValue <= 59)
                successLevel = 5;
            else if (achievementValue <= 69)
                successLevel = 6;
            else if (achievementValue <= 79)
                successLevel = 7;
            else if (achievementValue <= 89)
                successLevel = 8;
            else if (achievementValue <= 99)
                successLevel = 9;
            else
                successLevel = 10;

            bool isSuccess;
            bool isFailure;
            string? successMessage;

            if (cmpOp == ">")
            {
                isSuccess = successLevel > targetNumber;
                isFailure = !isSuccess;
                successMessage = isSuccess ? "成功" : "失敗";
            }
            else if (cmpOp == ">=")
            {
                isSuccess = successLevel >= targetNumber;
                isFailure = !isSuccess;
                successMessage = isSuccess ? "成功" : "失敗";
            }
            else
            {
                isSuccess = false;
                isFailure = false;
                successMessage = null;
            }

            var diceToMessageArr = new List<string>();
            foreach (int[] arr in diceAll)
            {
                diceToMessageArr.Add($"{arr.Sum()}[{string.Join(",", arr)}]");
            }

            string commandStr = cmpOp != null
                ? $"(YD{(modifyNumber >= 0 ? "+" : "")}{modifyNumber}{cmpOp}{targetNumber})"
                : $"(YD{(modifyNumber >= 0 ? "+" : "")}{modifyNumber})";

            string modifyStr2 = modifyNumber >= 0 ? $"+{modifyNumber}" : modifyNumber.ToString();
            var sequenceList = new List<string>
            {
                commandStr,
                $"{string.Join(" + ", diceToMessageArr)} {modifyStr2}",
                achievementValue.ToString(),
                $"成功段階{successLevel}"
            };

            if (successMessage != null)
            {
                sequenceList.Add(successMessage);
            }

            string text = string.Join(" ＞ ", sequenceList);

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
