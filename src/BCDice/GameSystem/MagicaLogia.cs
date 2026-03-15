using System;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// マギカロギア
    /// </summary>
    public sealed class MagicaLogia : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MagicaLogia Instance = new MagicaLogia();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?2D6([+-]\d+)?>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex MagicTableRegex = new Regex(
            @"^S?MGT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RuneTableRegex = new Regex(
            @"^S?RT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "MagicaLogia";

        /// <inheritdoc/>
        public override string Name => "マギカロギア";

        /// <inheritdoc/>
        public override string SortKey => "まきかろきあ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【マギカロギア】

・判定コマンド
　2D6>=t   2D6で目標値t以上を判定
　2D6+n>=t 修正値付き判定
　         1ゾロはファンブル、6ゾロはスペシャル

・魔法名表
　MGT      D66で魔法名を生成

・ルーン表
　RT       2D6でルーンを決定
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

            // 魔法名表
            var magicResult = EvalMagicTableCommand(command, randomizer);
            if (magicResult != null)
            {
                return magicResult;
            }

            // ルーン表
            var runeResult = EvalRuneTableCommand(command, randomizer);
            if (runeResult != null)
            {
                return runeResult;
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

            int modifier = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
            int target = int.Parse(match.Groups[2].Value);

            int die1 = randomizer.RollOnce(6);
            int die2 = randomizer.RollOnce(6);
            int diceTotal = die1 + die2;
            int total = diceTotal + modifier;

            bool isFumble = die1 == 1 && die2 == 1;
            bool isSpecial = die1 == 6 && die2 == 6;
            bool success = total >= target && !isFumble;

            var sb = new StringBuilder();
            sb.Append($"(2D6{(modifier >= 0 && modifier != 0 ? "+" : "")}{(modifier != 0 ? modifier.ToString() : "")}>={target})");
            sb.Append($" ＞ {die1},{die2}[{diceTotal}]");
            if (modifier != 0)
            {
                sb.Append($"{(modifier >= 0 ? "+" : "")}{modifier}");
                sb.Append($" ＞ {total}");
            }

            if (isSpecial)
            {
                sb.Append(" ＞ スペシャル！");
            }
            else if (isFumble)
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

            if (isSpecial)
            {
                builder.SetCritical(true).SetSuccess(true);
            }
            else if (isFumble)
            {
                builder.SetFumble(true).SetFailure(true);
            }
            else
            {
                builder.SetCondition(success);
            }

            return builder.Build();
        }

        private Result? EvalMagicTableCommand(string command, IRandomizer randomizer)
        {
            var match = MagicTableRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int die1 = randomizer.RollOnce(6);
            int die2 = randomizer.RollOnce(6);
            int d66 = Math.Min(die1, die2) * 10 + Math.Max(die1, die2);

            string magicName = GetMagicName(d66);

            string text = $"魔法名表({d66}) ＞ {magicName}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? EvalRuneTableCommand(string command, IRandomizer randomizer)
        {
            var match = RuneTableRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string rune = GetRune(roll);

            string text = $"ルーン表({roll}) ＞ {rune}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetMagicName(int d66)
        {
            return d66 switch
            {
                11 => "星",
                12 => "夜",
                13 => "影",
                14 => "月",
                15 => "夢",
                16 => "幻",
                22 => "血",
                23 => "炎",
                24 => "剣",
                25 => "嵐",
                26 => "雷",
                33 => "森",
                34 => "海",
                35 => "泉",
                36 => "地",
                44 => "獣",
                45 => "鳥",
                46 => "蟲",
                55 => "愛",
                56 => "歌",
                66 => "鏡",
                _ => "不明"
            };
        }

        private static string GetRune(int roll)
        {
            return roll switch
            {
                2 => "星（運命）",
                3 => "獣（本能）",
                4 => "力（暴力）",
                5 => "歌（感情）",
                6 => "夢（幻想）",
                7 => "闇（秘密）",
                8 => "火（破壊）",
                9 => "創（創造）",
                10 => "泉（生命）",
                11 => "空（自由）",
                12 => "鏡（真実）",
                _ => "不明"
            };
        }
    }
}
