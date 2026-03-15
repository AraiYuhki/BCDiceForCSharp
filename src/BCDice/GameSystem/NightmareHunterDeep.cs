using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ナイトメアハンター=ディープ
    /// </summary>
    public sealed class NightmareHunterDeep : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NightmareHunterDeep Instance = new NightmareHunterDeep();

        /// <inheritdoc/>
        public override string Id => "NightmareHunterDeep";

        /// <inheritdoc/>
        public override string Name => "ナイトメアハンター=ディープ";

        /// <inheritdoc/>
        public override string SortKey => "ないとめあはんたあていいふ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        判定（xD6+y>=a, xD6+y, xD6)
          出目6の個数をカウントして、その4倍を合計値に加算します。
          また、宿命を獲得したか表示します。

          Lv目標値 (xD6+y>=LVn, xD6+y>=NLn)
            レベルで目標値を指定することができます。
            LVn -> n*5+1, NLn -> n*5+5 に変換されます。
          目標値'?' (xD6+y>=?)
            目標値を '?' にすると何Lv成功か、何NL成功かを表示します。

        ※判定コマンドは xD6 から始まる必要があります。また xD6 が複数あると反応しません。
        ";

        private static readonly Regex DiceCommandRegex = new Regex(
            @"^(\d+)D6([+\-]\d+)?(>=(.+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LvRegex = new Regex(@"LV(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex NlRegex = new Regex(@"NL(\d+)", RegexOptions.IgnoreCase);

        private NightmareHunterDeep() { }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // Lv/NL target value preprocessing
            // LVn -> n*5-1, NLn -> n*5+5
            var processedCommand = LvRegex.Replace(command, m => (int.Parse(m.Groups[1].Value) * 5 - 1).ToString(), 1);
            processedCommand = NlRegex.Replace(processedCommand, m => (int.Parse(m.Groups[1].Value) * 5 + 5).ToString(), 1);

            var match = DiceCommandRegex.Match(processedCommand);
            if (!match.Success)
            {
                return null;
            }

            // Ensure only one xD6 group (reject commands with multiple D6 patterns)
            if (Regex.Matches(processedCommand, @"\d+D6", RegexOptions.IgnoreCase).Count > 1)
            {
                return null;
            }

            var times = int.Parse(match.Groups[1].Value);
            var modifyNumber = 0;
            if (match.Groups[2].Success && !string.IsNullOrEmpty(match.Groups[2].Value))
            {
                modifyNumber = int.Parse(match.Groups[2].Value);
            }

            var hasCmpOp = match.Groups[3].Success;
            var targetStr = hasCmpOp && match.Groups[4].Success ? match.Groups[4].Value : null;
            var questionTarget = targetStr == "?";
            int? targetNumber = null;
            if (!questionTarget && targetStr != null)
            {
                if (int.TryParse(targetStr, out var t))
                {
                    targetNumber = t;
                }
            }

            var diceList = randomizer.RollBarabara(times, 6).OrderBy(x => x).ToList();
            var diceTotal = diceList.Sum();
            var total = diceTotal + modifyNumber;

            var (suffix, revision) = DiceRevision(diceList);
            total += revision;

            var resultText = ResultText(total, hasCmpOp, questionTarget, targetNumber);

            var sequence = new List<string?>();
            sequence.Add($"({command})");
            sequence.Add(InterimExpr(modifyNumber, diceTotal, diceList));
            sequence.Add(ExprWithRevision(diceTotal + modifyNumber, suffix));
            sequence.Add(total.ToString());
            sequence.Add(resultText);
            sequence.Add(Fate(diceList));

            var parts = sequence.Where(x => x != null).Cast<string>().ToArray();
            return Result.CreateBuilder(string.Join(" ＞ ", parts))
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string? ResultText(int total, bool hasCmpOp, bool questionTarget, int? targetNumber)
        {
            if (!hasCmpOp)
            {
                return null;
            }

            if (!questionTarget && targetNumber.HasValue)
            {
                return total >= targetNumber.Value ? "成功" : "失敗";
            }

            if (questionTarget)
            {
                var successLv = (total + 1) / 5;
                var successNl = (total - 5) / 5;
                return successLv > 0 ? $"Lv{successLv}/NL{successNl}成功" : "失敗";
            }

            return null;
        }

        private string? Fate(List<int> diceList)
        {
            return diceList.Count(v => v == 1) > 0 ? "宿命獲得" : null;
        }

        private string? InterimExpr(int modifyNumber, int diceTotal, List<int> diceList)
        {
            if (diceList.Count > 1 || modifyNumber != 0)
            {
                var modifier = Format.Modifier(modifyNumber);
                return $"{diceTotal}[{string.Join(",", diceList)}]{modifier}";
            }
            return null;
        }

        private string? ExprWithRevision(int total, string? suffix)
        {
            if (suffix != null)
            {
                return $"{total}{suffix}";
            }
            return null;
        }

        private (string? suffix, int revision) DiceRevision(List<int> diceList)
        {
            var count6 = diceList.Count(v => v == 6);
            if (count6 > 0)
            {
                return ($"+{count6}*4", count6 * 4);
            }
            else
            {
                return (null, 0);
            }
        }
    }
}
