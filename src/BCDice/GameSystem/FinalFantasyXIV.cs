using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// FINAL FANTSY XIV TTRPG
    /// </summary>
    public sealed class FinalFantasyXIV : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly FinalFantasyXIV Instance = new FinalFantasyXIV();

        /// <inheritdoc/>
        public override string Id => "FinalFantasyXIV";

        /// <inheritdoc/>
        public override string Name => "FINAL FANTSY XIV TTRPG";

        /// <inheritdoc/>
        public override string SortKey => "ふあいなるふあんたしい14TTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・アビリティ判定 nAB+m>=x
          d20のアビリティ判定を行う。ダイス数が指定された場合、大きい出目1個を採用する。
          n: ダイス数（省略時 1）
          m: 修正値（省略可）
          x: 目標値（省略可）
          基本効果のみ、ダイレクトヒット、クリティカルを自動判定。
          例）AB, AB+5, AB+5>=14, 2AB+5>=14
        ・行為判定 nDC+m>=x
          アビリティ判定と同様。
          失敗、成功を自動判定。
        ";

        // nAB+m>=x  or  nAB  or  AB+m  etc.
        private static readonly Regex AbilityRegex = new Regex(
            @"^(\d*)AB([+-]\d+)?(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // nDC+m>=x  or  nDC  or  DC+m  etc.
        private static readonly Regex ActionRegex = new Regex(
            @"^(\d*)DC([+-]\d+)?(>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return AbirityRoll(command, randomizer) ?? ActionRoll(command, randomizer);
        }

        private Result? AbirityRoll(string command, IRandomizer randomizer)
        {
            var m = AbilityRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int times = m.Groups[1].Value == "" ? 1 : int.Parse(m.Groups[1].Value);
            if (times < 1) times = 1;

            int modify = m.Groups[2].Value == "" ? 0 : int.Parse(m.Groups[2].Value);
            int? targetNumber = m.Groups[4].Value == "" ? (int?)null : int.Parse(m.Groups[4].Value);

            string diceListFullStr = "";
            var diceListFull = randomizer.RollBarabara(times, 20).OrderBy(x => x).ToList();
            if (times > 1)
            {
                diceListFullStr = $"[{string.Join(",", diceListFull)}]";
            }

            // Take the last (highest) die
            var diceList = new List<int> { diceListFull[diceListFull.Count - 1] };
            int diceResult = diceList[0];
            int total = diceResult + modify;

            Result result;
            if (diceResult == 20)
            {
                result = Result.Critical(Translate("critical"));
            }
            else if (targetNumber == null)
            {
                result = Result.CreateBuilder("").Build();
            }
            else if (total >= targetNumber.Value)
            {
                result = Result.Success(Translate("FinalFantasyXIV.directhit"));
            }
            else
            {
                result = Result.Failure(Translate("FinalFantasyXIV.normalhit"));
            }

            string modifyStr = modify == 0 ? "" : (modify > 0 ? $"+{modify}" : $"{modify}");
            string cmdStr = times > 1 ? $"{times}AB{modifyStr}" : $"AB{modifyStr}";
            if (targetNumber != null) cmdStr += $">={targetNumber}";

            var sequence = new List<string>();
            sequence.Add($"({cmdStr})");
            if (diceListFullStr != "") sequence.Add(diceListFullStr);
            sequence.Add($"{diceResult}[{string.Join(",", diceList)}]{modifyStr}");
            sequence.Add(total.ToString());
            if (result.Text != "") sequence.Add(result.Text);

            string text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? ActionRoll(string command, IRandomizer randomizer)
        {
            var m = ActionRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int times = m.Groups[1].Value == "" ? 1 : int.Parse(m.Groups[1].Value);
            if (times < 1) times = 1;

            int modify = m.Groups[2].Value == "" ? 0 : int.Parse(m.Groups[2].Value);
            int? targetNumber = m.Groups[4].Value == "" ? (int?)null : int.Parse(m.Groups[4].Value);

            string diceListFullStr = "";
            var diceListFull = randomizer.RollBarabara(times, 20).OrderBy(x => x).ToList();
            if (times > 1)
            {
                diceListFullStr = $"[{string.Join(",", diceListFull)}]";
            }

            // Take the last (highest) die
            var diceList = new List<int> { diceListFull[diceListFull.Count - 1] };
            int diceResult = diceList[0];
            int total = diceResult + modify;

            Result result;
            if (targetNumber == null)
            {
                result = Result.CreateBuilder("").Build();
            }
            else if (total >= targetNumber.Value)
            {
                result = Result.Success(Translate("success"));
            }
            else
            {
                result = Result.Failure(Translate("failure"));
            }

            string modifyStr = modify == 0 ? "" : (modify > 0 ? $"+{modify}" : $"{modify}");
            string cmdStr = times > 1 ? $"{times}DC{modifyStr}" : $"DC{modifyStr}";
            if (targetNumber != null) cmdStr += $">={targetNumber}";

            var sequence = new List<string>();
            sequence.Add($"({cmdStr})");
            if (diceListFullStr != "") sequence.Add(diceListFullStr);
            sequence.Add($"{diceResult}[{string.Join(",", diceList)}]{modifyStr}");
            sequence.Add(total.ToString());
            if (result.Text != "") sequence.Add(result.Text);

            string text = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
