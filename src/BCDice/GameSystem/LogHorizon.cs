using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ログ・ホライズンTRPG
    /// xLH判定コマンド。クリティカル（6が2個以上）とファンブル（全て1）判定付き。
    /// 消耗表ロール（CT）と財宝表ロール（TRS）も実装。
    /// </summary>
    public sealed class LogHorizon : GameSystemBase
    {
        public static readonly LogHorizon Instance = new LogHorizon();

        // xLH+y>=z
        private static readonly Regex LhRegex = new Regex(
            @"^(\d+)LH((?:[+\-]\d+)+)?(?:>=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 消耗表ロール CT+y
        private static readonly Regex ConsumptionRollRegex = new Regex(
            @"^CT\d*([+\-\d]+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // 財宝表ロール TRSx+y
        private static readonly Regex TreasureRollRegex = new Regex(
            @"^TRS(\d+)?([+\-\d]+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "LogHorizon";
        public override string Name => "ログ・ホライズンTRPG";
        public override string SortKey => "ろくほらいすんTRPG";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"■ 判定 (xLH±y>=z)
　xD6の判定。クリティカル、ファンブルの自動判定を行います。
　x：xに振るダイス数を入力。
　±y：yに修正値を入力。±の計算に対応。省略可能。
　>=z：zに目標値を入力。±の計算に対応。省略可能。
　例） 3LH　2LH>=8　3LH+1>=10

■ 消耗表ロール (CTx±y)
　消耗表ロールを行い、出目を決定する。
　±y：修正値。＋と－の計算に対応。省略可能。

■ 財宝表ロール (TRSx±y)
　財宝表ロールを行い、出目を決定する。
　x：CRを指定。省略時はCR 0として扱う
　±y：修正値。＋と－の計算に対応。省略可能。

・D66ダイスあり
";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = RollLh(command, randomizer);
            if (result != null) return result;

            result = RollConsumption(command, randomizer);
            if (result != null) return result;

            result = RollTreasure(command, randomizer);
            if (result != null) return result;

            return null;
        }

        private Result? RollLh(string command, IRandomizer randomizer)
        {
            var m = LhRegex.Match(command);
            if (!m.Success) return null;

            int diceCount = int.Parse(m.Groups[1].Value);
            int modifier = m.Groups[2].Success ? EvalModifier(m.Groups[2].Value) : 0;
            int? targetNumber = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;

            var diceList = randomizer.RollBarabara(diceCount, 6);
            int diceTotal = diceList.Sum();
            int total = diceTotal + modifier;

            string? resultText = null;
            bool isSuccess = false;
            bool isFailure = false;
            bool isCritical = false;
            bool isFumble = false;

            int sixCount = diceList.Count(d => d == 6);
            int oneCount = diceList.Count(d => d == 1);

            if (sixCount >= 2)
            {
                resultText = "クリティカル";
                isCritical = true;
                isSuccess = true;
            }
            else if (oneCount >= diceCount)
            {
                resultText = "ファンブル";
                isFumble = true;
                isFailure = true;
            }
            else if (targetNumber.HasValue)
            {
                if (total >= targetNumber.Value)
                {
                    resultText = "成功";
                    isSuccess = true;
                }
                else
                {
                    resultText = "失敗";
                    isFailure = true;
                }
            }

            string modStr = FormatModifier(modifier);
            string cmdStr = $"{diceCount}LH{modStr}";
            if (targetNumber.HasValue) cmdStr += $">={targetNumber}";

            var parts = new List<string>
            {
                $"({cmdStr})",
                $"{diceTotal}[{string.Join(",", diceList)}]{modStr}",
                total.ToString(),
            };

            if (resultText != null)
            {
                parts.Add(resultText);
            }

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollConsumption(string command, IRandomizer randomizer)
        {
            var m = ConsumptionRollRegex.Match(command);
            if (!m.Success) return null;

            // CT のみの場合はマッチしない（通常の共通コマンドと混同を避ける）
            if (command.Equals("CT", StringComparison.OrdinalIgnoreCase)) return null;

            int modifier = m.Groups[1].Success ? EvalModifier(m.Groups[1].Value) : 0;
            string modStr = FormatModifier(modifier);
            int dice = randomizer.RollOnce(6);

            var parts = new List<string> { $"(1D6{modStr})" };

            if (!string.IsNullOrEmpty(modStr))
            {
                parts.Add($"{dice}{modStr}");
            }

            parts.Add((dice + modifier).ToString());

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollTreasure(string command, IRandomizer randomizer)
        {
            var m = TreasureRollRegex.Match(command);
            if (!m.Success) return null;

            int characterRank = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            int modifier = m.Groups[2].Success ? EvalModifier(m.Groups[2].Value) : 0;

            var diceList = randomizer.RollBarabara(2, 6);
            int diceTotal = diceList.Sum();
            int total = diceTotal + characterRank * 5 + modifier;

            string crModStr = FormatModifier(characterRank * 5 + modifier);

            string text = $"(2D6+{characterRank}*5{FormatModifier(modifier)}) ＞ " +
                          $"{diceTotal}[{string.Join(",", diceList)}]{crModStr} ＞ " +
                          $"{total}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static int EvalModifier(string expr)
        {
            if (string.IsNullOrEmpty(expr)) return 0;
            int result = 0;
            foreach (Match m in Regex.Matches(expr, @"[+\-]\d+"))
            {
                result += int.Parse(m.Value);
            }
            return result;
        }

        private static string FormatModifier(int value)
        {
            if (value == 0) return "";
            return value > 0 ? $"+{value}" : value.ToString();
        }
    }
}