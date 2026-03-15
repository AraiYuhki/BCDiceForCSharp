using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// カルカミ
    /// </summary>
    public sealed class Karukami : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Karukami Instance = new Karukami();

        /// <inheritdoc/>
        public override string Id => "Karukami";

        /// <inheritdoc/>
        public override string Name => "カルカミ";

        /// <inheritdoc/>
        public override string SortKey => "かるかみ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 行為判定、ダメージ算出 (xUB+y@c>=t)
          6面ダイスをx個ダイスロールし、クリティカル値以上の出目が出たら振り足して合計値を算出します。
          x: ダイス数
          y: 修正値（省略可）
          c: クリティカル値（省略可）
          t: 目標値値（省略可）
          例）2UB, 2UB>=7, 3UB+1@5, 3UB+1@5<10
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollUb(command, randomizer);
        }

        private Result? RollUb(string command, IRandomizer randomizer)
        {
            // Parse: xUB+y@c>=t
            var m = Regex.Match(command, @"^(\d+)UB([+-]\d+)?(?:@(\d+))?(?:(>=|<=|>|<|==|!=)(\d+))?$");
            if (!m.Success)
            {
                return null;
            }

            var prefixNumber = int.Parse(m.Groups[1].Value);
            var modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;
            var critical = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 6;
            var cmpOp = m.Groups[4].Success ? m.Groups[4].Value : null;
            var targetNumber = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (int?)null;

            if (critical <= 1)
            {
                return Result.CreateBuilder($"({command}).Build() ＞ クリティカル値は2以上としてください").Build();
            }

            var listList = new List<int[]>();
            var criticals = 0;
            var stack = prefixNumber;
            while (stack > 0)
            {
                var diceList = randomizer.RollBarabara(stack, 6);
                listList.Add(diceList);
                stack = diceList.Count(x => x >= critical);
                criticals += stack;
            }

            var total = listList.SelectMany(x => x).Sum() + modifyNumber;

            Result result;
            if (listList.First().All(x => x == 1))
            {
                total = 0;
                result = Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (cmpOp == null)
            {
                result = Result.CreateBuilder("").Build();
            }
            else if (CompareOperator(total, cmpOp, targetNumber!.Value))
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            var isCritical = criticals > 0;

            var sequence = new List<string>();
            sequence.Add($"({command})");
            foreach (var list in listList)
            {
                sequence.Add($"[{string.Join(",", list)}]");
            }
            sequence.Add(total.ToString());
            if (isCritical)
            {
                sequence.Add($"{criticals}クリティカル");
            }
            if (!string.IsNullOrEmpty(result.Text))
            {
                sequence.Add(result.Text);
            }

            var text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(isCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private static bool CompareOperator(int left, string op, int right) => op switch
        {
            ">=" => left >= right,
            "<=" => left <= right,
            ">" => left > right,
            "<" => left < right,
            "==" => left == right,
            "!=" => left != right,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };
    }
}
