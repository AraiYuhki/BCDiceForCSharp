using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// サタスペ
    /// </summary>
    public sealed class Satasupe : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Satasupe Instance = new Satasupe();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?(\d+)R>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CrimeRegex = new Regex(
            @"^S?CRIME(\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FumbleRegex = new Regex(
            @"^S?FUMBLE$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Satasupe";

        /// <inheritdoc/>
        public override string Name => "サタスペ";

        /// <inheritdoc/>
        public override string SortKey => "さたすへ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
【サタスペ】

・判定コマンド
　nR>=t   n個のD6を振り、t以上の達成値を得る
　        最大値を達成値とし、6が出たら振り足し
　        1ゾロはファンブル

・CRIME表（犯罪表）
　CRIME   犯罪表を振る
　CRIMEn  犯罪レベルnで振る

・FUMBLE  ファンブル表
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // 判定コマンド
            var judgeResult = EvalJudgeCommand(command, randomizer);
            if (judgeResult != null)
            {
                return judgeResult;
            }

            // 犯罪表
            var crimeResult = EvalCrimeCommand(command, randomizer);
            if (crimeResult != null)
            {
                return crimeResult;
            }

            // ファンブル表
            var fumbleResult = EvalFumbleCommand(command, randomizer);
            if (fumbleResult != null)
            {
                return fumbleResult;
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? EvalJudgeCommand(string command, IRandomizer randomizer)
        {
            var match = JudgeRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            int diceCount = int.Parse(match.Groups[1].Value);
            int target = int.Parse(match.Groups[2].Value);

            if (diceCount <= 0)
            {
                return null;
            }

            var allRolls = new List<List<int>>();
            int maxValue = 0;
            const int maxRerolls = 100;

            // 各ダイスごとに6で振り足し
            for (int i = 0; i < diceCount; i++)
            {
                var diceRolls = new List<int>();
                int rerollCount = 0;
                int total = 0;

                do
                {
                    int roll = randomizer.RollOnce(6);
                    diceRolls.Add(roll);
                    total += roll;
                    rerollCount++;
                } while (diceRolls.Last() == 6 && rerollCount < maxRerolls);

                allRolls.Add(diceRolls);
                maxValue = Math.Max(maxValue, total);
            }

            // ファンブル判定（最初のロールが全て1）
            bool isFumble = allRolls.All(rolls => rolls[0] == 1);

            var sb = new StringBuilder();
            sb.Append($"({diceCount}R>={target})");
            sb.Append(" ＞ [");

            var rollStrings = allRolls.Select(rolls =>
            {
                if (rolls.Count == 1)
                {
                    return rolls[0].ToString();
                }
                return string.Join("+", rolls) + $"={rolls.Sum()}";
            });
            sb.Append(string.Join(",", rollStrings));
            sb.Append("]");

            sb.Append($" ＞ 最大{maxValue}");

            bool success = maxValue >= target && !isFumble;

            if (isFumble)
            {
                sb.Append(" ＞ ファンブル！");
            }
            else
            {
                sb.Append($" ＞ {(success ? "成功" : "失敗")}");
            }

            var builder = Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (isFumble)
            {
                builder.SetFumble(true).SetFailure(true);
            }
            else
            {
                builder.SetCondition(success);
            }

            return builder.Build();
        }

        private Result? EvalCrimeCommand(string command, IRandomizer randomizer)
        {
            var match = CrimeRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int crimeLevel = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
            int roll = randomizer.RollSum(2, 6);
            int total = roll + crimeLevel;

            string crimeResult = GetCrimeResult(total);

            var sb = new StringBuilder();
            sb.Append($"犯罪表({roll}");
            if (crimeLevel > 0)
            {
                sb.Append($"+{crimeLevel}={total}");
            }
            sb.Append($") ＞ {crimeResult}");

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? EvalFumbleCommand(string command, IRandomizer randomizer)
        {
            var match = FumbleRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string fumbleResult = GetFumbleResult(roll);

            string text = $"ファンブル表({roll}) ＞ {fumbleResult}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetCrimeResult(int total)
        {
            if (total <= 2) return "無罪放免";
            if (total <= 4) return "厳重注意";
            if (total <= 6) return "罰金刑";
            if (total <= 8) return "禁固刑";
            if (total <= 10) return "懲役刑";
            if (total <= 12) return "無期懲役";
            return "死刑";
        }

        private static string GetFumbleResult(int roll)
        {
            return roll switch
            {
                2 => "大惨事！致命傷を負う",
                3 => "装備品が1つ壊れる",
                4 => "次の判定に-2",
                5 => "立ちすくむ（1ラウンド行動不能）",
                6 => "転倒する",
                7 => "何も起こらない",
                8 => "転倒する",
                9 => "立ちすくむ（1ラウンド行動不能）",
                10 => "次の判定に-2",
                11 => "装備品が1つ壊れる",
                12 => "大惨事！致命傷を負う",
                _ => "何も起こらない"
            };
        }
    }
}
