using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 剣の街の異邦人TRPG
    /// </summary>
    public sealed class StrangerOfSwordCity : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly StrangerOfSwordCity Instance = new StrangerOfSwordCity();

        /// <inheritdoc/>
        public override string Id => "StrangerOfSwordCity";

        /// <inheritdoc/>
        public override string Name => "剣の街の異邦人TRPG";

        /// <inheritdoc/>
        public override string SortKey => "つるきのまちのいほうしんTRPG";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Floor;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　xSR or xSRy or xSR+y or xSR-y or xSR+y>=z
        　x=ダイス数、y=修正値(省略可、±省略時は＋として扱う)、z=難易度(省略可)
        　判定時はクリティカル、ファンブルの自動判定を行います。
        ・通常のnD6ではクリティカル、ファンブルの自動判定は行いません。
        ・D66ダイスあり
        ";

        private static readonly Regex SrRegex = new Regex(
            @"^(\d+)SR([+-]?\d+)?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            command = command.ToUpper();
            return CheckRoll(command, randomizer);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = SrRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var modify = m.Groups[2].Success && !string.IsNullOrEmpty(m.Groups[2].Value) ? int.Parse(m.Groups[2].Value) : 0;
            int? difficulty = m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value) ? int.Parse(m.Groups[4].Value) : (int?)null;

            var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            var dice = diceList.Sum();

            var totalValue = dice + modify;
            var modifyText = GetModifyText(modify);
            var resultText = $"({command}) ＞ {dice}[{string.Join(",", diceList)}]{modifyText} ＞ {totalValue}";

            var criticalResult = GetCriticalResult(diceList);
            if (criticalResult != null)
            {
                resultText += $" ＞ クリティカル(+{criticalResult}D6)";
                return Result.CreateBuilder(resultText)
                    .SetCritical(true)
                    .SetSuccess(true)
                    .Build();
            }

            if (IsFumble(diceList, diceCount))
            {
                resultText += " ＞ ファンブル";
                return Result.CreateBuilder(resultText)
                    .SetFumble(true)
                    .SetFailure(true)
                    .Build();
            }

            if (difficulty != null)
            {
                if (totalValue >= difficulty.Value)
                {
                    resultText += " ＞ 成功";
                    return Result.CreateBuilder(resultText)
                        .SetSuccess(true)
                        .Build();
                }
                else
                {
                    resultText += " ＞ 失敗";
                    return Result.CreateBuilder(resultText)
                        .SetFailure(true)
                        .Build();
                }
            }

            return Result.CreateBuilder(resultText).Build();
        }

        private string GetModifyText(int modify)
        {
            if (modify == 0) return "";
            if (modify < 0) return modify.ToString();
            return $"+{modify}";
        }

        private string GetCriticalResult(List<int> diceList)
        {
            var dice6Count = diceList.Count(i => i == 6);
            if (dice6Count >= 2)
            {
                return dice6Count.ToString();
            }
            return null;
        }

        private bool IsFumble(List<int> diceList, int diceCount)
        {
            return diceList.Count(i => i == 1) >= diceCount;
        }
    }
}
