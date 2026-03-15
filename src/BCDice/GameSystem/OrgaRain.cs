using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 在りて遍くオルガレイン
    /// </summary>
    public sealed class OrgaRain : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly OrgaRain Instance = new OrgaRain();

        private static readonly Regex DOrDRegex = new Regex(
            @"^(\d+)?OR(\d{0,6})$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "OrgaRain";

        /// <inheritdoc/>
        public override string Name => "在りて遍くオルガレイン";

        /// <inheritdoc/>
        public override string SortKey => "ありてあまねくおるかれいん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        判定：[n]OR(count)

        []内のコマンドは省略可能。
        「n」でダイス数を指定。省略時は「1」。
        (count)で命数を指定。「3111」のように記述。最大6つ。順不同可。

        【書式例】
        ・5OR6042 → 5dで命数「0,2,4,6」の判定
        ・6OR33333 → 6dで命数「3,3,3,3,3」の判定。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = DOrDRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int diceCount = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;
            // Parse each character of the count string as a digit, then sort
            string countStr = m.Groups[2].Success ? m.Groups[2].Value : "";
            var countNo = countStr.Select(c => c - '0').OrderBy(x => x).ToList();

            return CheckRoll(diceCount, countNo, randomizer);
        }

        private Result CheckRoll(int diceCount, List<int> countNo, IRandomizer randomizer)
        {
            var diceArray = randomizer.RollBarabara(diceCount, 10).OrderBy(x => x).ToList();
            var diceText = string.Join(",", diceArray);
            var resultArray = new List<string>();
            int success = 0;

            foreach (var i in diceArray.Select(x => x == 10 ? 0 : x))
            {
                int multiple = countNo.Count(c => c == i);
                if (multiple > 0)
                {
                    resultArray.Add($"{i}(x{multiple})");
                    success += multiple;
                }
                else
                {
                    resultArray.Add("×");
                }
            }

            var countText = string.Join(",", countNo);
            var resultText = string.Join(",", resultArray);
            string text = $"{diceCount}D10(命数：{countText}) ＞ {diceText} ＞ {resultText} ＞ 成功数：{success}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
