using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 特命転攻生
    /// </summary>
    public sealed class TokumeiTenkousei : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TokumeiTenkousei Instance = new TokumeiTenkousei();

        /// <inheritdoc/>
        public override string Id => "TokumeiTenkousei";

        /// <inheritdoc/>
        public override string Name => "特命転攻生";

        /// <inheritdoc/>
        public override string SortKey => "とくめいてんこうせい";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 (xD6+y>=n)
        　ゾロ目での自動振り足し
        　1の出目に応じてEPPの獲得量を表示
        　目標値 ""?"" には未対応
        ";

        private static readonly Regex CmdRegex = new Regex(
            @"^(\d+)D6([+\-]\d+)?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = CmdRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var times = int.Parse(m.Groups[1].Value);
            var modifyNumber = m.Groups[2].Success && !string.IsNullOrEmpty(m.Groups[2].Value) ? int.Parse(m.Groups[2].Value) : 0;
            CompareOperator? cmpOp = null;
            int? targetNumber = null;
            if (m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value))
            {
                cmpOp = CompareOperator.GreaterThanOrEqual;
                targetNumber = int.Parse(m.Groups[4].Value);
            }

            var diceList = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToList();
            var allDiceLists = new List<List<int>> { diceList };
            while (SameAllDice(diceList))
            {
                diceList = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToList();
                allDiceLists.Add(diceList);
            }

            var diceListFlatten = allDiceLists.SelectMany(x => x).ToList();
            var diceTotal = diceListFlatten.Sum();
            var countOne = diceListFlatten.Count(x => x == 1);

            var total = diceTotal + modifyNumber;

            Result result = null;
            if (cmpOp != null && targetNumber != null)
            {
                result = total >= targetNumber.Value
                    ? Result.CreateBuilder("成功").SetSuccess(true).Build()
                    : Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            // Build command display
            var cmdDisplay = $"{times}D6";
            if (modifyNumber != 0)
            {
                cmdDisplay += modifyNumber > 0 ? $"+{modifyNumber}" : modifyNumber.ToString();
            }
            if (targetNumber != null)
            {
                cmdDisplay += $">={targetNumber}";
            }

            var sequence = new List<string>();
            sequence.Add($"({cmdDisplay})");

            var interimText = InterimExpr(allDiceLists, modifyNumber, diceTotal);
            if (interimText != null)
            {
                sequence.Add(interimText);
            }

            sequence.Add(total.ToString());

            if (result != null)
            {
                sequence.Add(result.Text);
            }

            var eppText = Epp(countOne);
            if (eppText != null)
            {
                sequence.Add(eppText);
            }

            var text = string.Join(" ＞ ", sequence);

            var builder = Result.CreateBuilder(text);
            if (result != null)
            {
                builder.SetSuccess(result.IsSuccess);
                builder.SetFailure(result.IsFailure);
            }
            return builder.Build();
        }

        private bool SameAllDice(List<int> diceList)
        {
            return diceList.Count > 1 && diceList.Distinct().Count() == 1;
        }

        private string InterimExpr(List<List<int>> allDiceLists, int modifyNumber, int diceTotal)
        {
            if (allDiceLists.SelectMany(x => x).Count() == 1 && modifyNumber == 0)
            {
                return null;
            }

            var diceListStr = string.Join("", allDiceLists.Select(ds => $"[{string.Join(",", ds)}]"));
            var modifier = GetModifyText(modifyNumber);

            return $"{diceTotal}{diceListStr}{modifier}";
        }

        private string Epp(int countOne)
        {
            if (countOne > 0)
            {
                return $"{countOne * 5}EPP獲得";
            }
            return null;
        }

        private string GetModifyText(int modify)
        {
            if (modify == 0) return "";
            if (modify > 0) return $"+{modify}";
            return modify.ToString();
        }
    }
}
