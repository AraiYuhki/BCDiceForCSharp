using System;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 迷宮キングダム
    /// </summary>
    public sealed class MeikyuuKingdom : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MeikyuuKingdom Instance = new MeikyuuKingdom();

        private static readonly Regex JudgeRegex = new Regex(
            @"^S?2D6([+-]\d+)?>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FacilityTableRegex = new Regex(
            @"^S?FT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex NameTableRegex = new Regex(
            @"^S?NT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RandomEventRegex = new Regex(
            @"^S?RE$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "MeikyuuKingdom";

        /// <inheritdoc/>
        public override string Name => "迷宮キングダム";

        /// <inheritdoc/>
        public override string SortKey => "めいきゆうきんくたむ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
【迷宮キングダム】

・判定コマンド
　2D6>=t   2D6で目標値t以上を判定
　2D6+n>=t 修正値付き判定
　         2はファンブル、12はクリティカル

・施設表
　FT       D66で施設を決定

・名前表
　NT       D66で名前を生成

・ランダムイベント表
　RE       2D6でイベントを決定
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

            // 施設表
            var facilityResult = EvalFacilityTableCommand(command, randomizer);
            if (facilityResult != null)
            {
                return facilityResult;
            }

            // 名前表
            var nameResult = EvalNameTableCommand(command, randomizer);
            if (nameResult != null)
            {
                return nameResult;
            }

            // ランダムイベント表
            var eventResult = EvalRandomEventCommand(command, randomizer);
            if (eventResult != null)
            {
                return eventResult;
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

            bool isFumble = diceTotal == 2;
            bool isCritical = diceTotal == 12;
            bool success = total >= target && !isFumble;

            var sb = new StringBuilder();
            sb.Append($"(2D6{(modifier >= 0 && modifier != 0 ? "+" : "")}{(modifier != 0 ? modifier.ToString() : "")}>={target})");
            sb.Append($" ＞ {die1},{die2}[{diceTotal}]");
            if (modifier != 0)
            {
                sb.Append($"{(modifier >= 0 ? "+" : "")}{modifier}");
                sb.Append($" ＞ {total}");
            }

            if (isCritical)
            {
                sb.Append(" ＞ クリティカル！");
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

            if (isCritical)
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

        private Result? EvalFacilityTableCommand(string command, IRandomizer randomizer)
        {
            var match = FacilityTableRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int die1 = randomizer.RollOnce(6);
            int die2 = randomizer.RollOnce(6);
            int d66 = Math.Min(die1, die2) * 10 + Math.Max(die1, die2);

            string facility = GetFacility(d66);

            string text = $"施設表({d66}) ＞ {facility}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? EvalNameTableCommand(string command, IRandomizer randomizer)
        {
            var match = NameTableRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int die1 = randomizer.RollOnce(6);
            int die2 = randomizer.RollOnce(6);
            int d66 = Math.Min(die1, die2) * 10 + Math.Max(die1, die2);

            string name = GetName(d66);

            string text = $"名前表({d66}) ＞ {name}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? EvalRandomEventCommand(string command, IRandomizer randomizer)
        {
            var match = RandomEventRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

            int roll = randomizer.RollSum(2, 6);
            string eventResult = GetRandomEvent(roll);

            string text = $"ランダムイベント表({roll}) ＞ {eventResult}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string GetFacility(int d66)
        {
            return d66 switch
            {
                11 => "宮殿",
                12 => "城壁",
                13 => "塔",
                14 => "神殿",
                15 => "広場",
                16 => "市場",
                22 => "酒場",
                23 => "宿屋",
                24 => "武器屋",
                25 => "防具屋",
                26 => "道具屋",
                33 => "訓練所",
                34 => "図書館",
                35 => "工房",
                36 => "倉庫",
                44 => "農場",
                45 => "牧場",
                46 => "鉱山",
                55 => "港",
                56 => "橋",
                66 => "遺跡",
                _ => "空き地"
            };
        }

        private static string GetName(int d66)
        {
            return d66 switch
            {
                11 => "アル",
                12 => "イル",
                13 => "ウル",
                14 => "エル",
                15 => "オル",
                16 => "カル",
                22 => "キル",
                23 => "クル",
                24 => "ケル",
                25 => "コル",
                26 => "サル",
                33 => "シル",
                34 => "スル",
                35 => "セル",
                36 => "ソル",
                44 => "タル",
                45 => "チル",
                46 => "ツル",
                55 => "テル",
                56 => "トル",
                66 => "ナル",
                _ => "ムル"
            };
        }

        private static string GetRandomEvent(int roll)
        {
            return roll switch
            {
                2 => "大災害！王国に危機が迫る",
                3 => "モンスターの襲撃",
                4 => "疫病が流行る",
                5 => "税収が減少",
                6 => "平和な日々",
                7 => "何も起こらない",
                8 => "平和な日々",
                9 => "交易が活発に",
                10 => "豊作の予感",
                11 => "冒険者が訪れる",
                12 => "宝の発見！王国に幸運が訪れる",
                _ => "何も起こらない"
            };
        }
    }
}
