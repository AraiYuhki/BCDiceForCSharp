using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// デモンスパイク
    /// </summary>
    public sealed class DemonSpike : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DemonSpike Instance = new DemonSpike();

        /// <inheritdoc/>
        public override string Id => "DemonSpike";

        /// <inheritdoc/>
        public override string Name => "デモンスパイク";

        /// <inheritdoc/>
        public override string SortKey => "てもんすはいく";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定 xDS+y
        　行為判定を行い、達成値、成否、成功度を出力する。
        　x: ダイス数（省略：2）
        　y: 能力値やスパイク能力による達成値の修正（省略可）
        ";

        private static readonly Regex DsRegex = new Regex(
            @"^(\d*)DS([+\-]\d+)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollAction(command, randomizer);
        }

        private Result? RollAction(string command, IRandomizer randomizer)
        {
            var m = DsRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int prefixNumber = m.Groups[1].Value.Length > 0 ? Convert.ToInt32(m.Groups[1].Value) : 2;
            int modifyNumber = m.Groups[2].Value.Length > 0
                ? Convert.ToInt32(m.Groups[2].Value)
                : 0;

            if (prefixNumber < 2)
            {
                return null;
            }

            var step = RollStep(prefixNumber, randomizer);
            var step_list = new List<(int[] dice_list, int dice_sum)> { step };
            while (step.dice_sum == 10)
            {
                step = RollStep(prefixNumber, randomizer);
                step_list.Add(step);
            }

            bool is_fumble = step_list[0].dice_sum == 2;
            int total = is_fumble ? 0 : step_list.Sum(s => s.dice_sum) + modifyNumber;
            int success_level = total / 10;
            bool is_success = total >= 10;

            string res;
            if (is_success)
            {
                res = $"成功, 成功度{success_level}";
            }
            else if (is_fumble)
            {
                res = "自動的失敗";
            }
            else
            {
                res = "失敗";
            }

            var parts = new List<string>();
            parts.Add($"({command})");
            foreach (var s in step_list)
            {
                parts.Add($"{s.dice_sum}[{string.Join(",", s.dice_list)}]");
            }
            parts.Add(total.ToString());
            parts.Add(res);

            string sequenceText = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(sequenceText)
                .SetCondition(is_success)
                .SetCritical(step_list.Count > 1)
                .SetFumble(is_fumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private (int[] dice_list, int dice_sum) RollStep(int times, IRandomizer randomizer)
        {
            int[] dice_array = randomizer.RollBarabara(times, 6);
            var dice_list = dice_array.OrderByDescending(x => x).ToArray();
            int dice_sum = ((dice_list[0] + dice_list[1]) < 2 ? 2 : (dice_list[0] + dice_list[1]) > 10 ? 10 : (dice_list[0] + dice_list[1]));
            return (dice_list, dice_sum);
        }
    }
}
