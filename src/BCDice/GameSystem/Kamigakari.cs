using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 神我狩
    /// </summary>
    public sealed class Kamigakari : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Kamigakari Instance = new Kamigakari();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?(\d+)?KG([+-]\d+)?(@(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TalentTableRegex = new Regex(
            @"^S?TT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Kamigakari";

        /// <inheritdoc/>
        public override string Name => "神我狩";

        /// <inheritdoc/>
        public override string SortKey => "かみかかり";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【神我狩】

・判定コマンド
　KG       2D6で判定（クリティカル12、ファンブル2）
　nKG      霊力n個を使用してダイスを追加
　KG+m     修正値mを加算
　KG@c     クリティカル値を変更

・才能表
　TT       D66で才能を決定
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

            // 才能表
            var talentResult = EvalTalentTableCommand(command, randomizer);
            if (talentResult != null)
            {
                return talentResult;
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

            int spiritDice = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
            int modifier = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
            int criticalValue = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 12;

            int totalDice = 2 + spiritDice;
            var rolls = new List<int>();
            for (int i = 0; i < totalDice; i++)
            {
                rolls.Add(randomizer.RollOnce(6));
            }

            // 上位2つを取得
            var topTwo = rolls.OrderByDescending(x => x).Take(2).ToList();
            int diceTotal = topTwo.Sum();
            int total = diceTotal + modifier;

            bool isFumble = rolls[0] == 1 && rolls[1] == 1 && spiritDice == 0;
            bool isCritical = diceTotal >= criticalValue;

            var sb = new StringBuilder();
            sb.Append($"({(spiritDice > 0 ? spiritDice.ToString() : "")}KG");
            if (modifier != 0)
            {
                sb.Append($"{(modifier >= 0 ? "+" : "")}{modifier}");
            }
            if (criticalValue != 12)
            {
                sb.Append($"@{criticalValue}");
            }
            sb.Append(")");

            sb.Append($" ＞ [{string.Join(",", rolls)}]");

            if (spiritDice > 0)
            {
                sb.Append($" ＞ 上位2個[{string.Join(",", topTwo)}]={diceTotal}");
            }
            else
            {
                sb.Append($" ＞ {diceTotal}");
            }

            if (modifier != 0)
            {
                sb.Append($"{(modifier >= 0 ? "+" : "")}{modifier} ＞ {total}");
            }

            if (isFumble)
            {
                sb.Append(" ＞ ファンブル！");
            }
            else if (isCritical)
            {
                sb.Append(" ＞ クリティカル！");
            }

            var builder = Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults);

            if (isFumble)
            {
                builder.SetFumble(true).SetFailure(true);
            }
            else if (isCritical)
            {
                builder.SetCritical(true).SetSuccess(true);
            }

            return builder.Build();
        }

        private Result? EvalTalentTableCommand(string command, IRandomizer randomizer)
        {
            var match = TalentTableRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int die1 = randomizer.RollOnce(6);
            int die2 = randomizer.RollOnce(6);
            int d66 = Math.Min(die1, die2) * 10 + Math.Max(die1, die2);

            string talent = GetTalent(d66);

            string text = $"才能表({d66}) ＞ {talent}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetTalent(int d66)
        {
            return d66 switch
            {
                11 => "格闘",
                12 => "射撃",
                13 => "斬撃",
                14 => "刺突",
                15 => "打撃",
                16 => "投擲",
                22 => "回避",
                23 => "防御",
                24 => "耐久",
                25 => "抵抗",
                26 => "隠密",
                33 => "知覚",
                34 => "探索",
                35 => "交渉",
                36 => "情報",
                44 => "操縦",
                45 => "医療",
                46 => "修理",
                55 => "芸術",
                56 => "生存",
                66 => "神秘",
                _ => "汎用"
            };
        }
    }
}
