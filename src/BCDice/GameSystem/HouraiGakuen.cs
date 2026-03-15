using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 蓬莱学園の冒険!!
    /// </summary>
    public sealed class HouraiGakuen : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly HouraiGakuen Instance = new HouraiGakuen();

        private const string CRITICAL = "大成功";
        private const string SUCCESS = "成功";
        private const string FAILURE = "失敗";
        private const string FUMBLE = "大失敗";

        private static readonly Regex RolRegex = new Regex(
            @"^ROL([-\d]+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MedRegex = new Regex(
            @"^MED\((\d+),(\d+)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ResRegex = new Regex(
            @"^RES\((\d+),(\d+)\)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex InyRegex = new Regex(
            @"^INY",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HtkRegex = new Regex(
            @"^HTK",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GogRegex = new Regex(
            @"^GOG$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "HouraiGakuen";

        /// <inheritdoc/>
        public override string Name => "蓬莱学園の冒険!!";

        /// <inheritdoc/>
        public override string SortKey => "ほうらいかくえんのほうけん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・基本ロール：ROL(x+n)
          ROLL(自分の能力値 + 簡単値 + 応石 or 蓬莱パワー)と記述します。3D6をロールし、成功したかどうかを表示します。
          例）ROL(4+6)
        ・対人判定：MED(x,y)
          自分の能力値 x と 相手の能力値 y でロールを行い、成功したかどうかを表示します。
          例）MED(5,2)
        ・対抗判定：RES(x,y)
          自分の能力値 x と 相手の能力値 y で相互にロールし、どちらが成功したかを表示します。両者とも成功 or 失敗の場合は引き分けとなります。
          例）RES(6,4)
        ・陰陽コマンド INY
          例）Hourai : 陽（奇数の方が多い）
        ・五行コマンド：GOG
          例）Hourai : 五行表(3) → 五行【土】
        ・八徳コマンド：HTK
          例）Hourai : 仁義八徳は、【義】(奇数、奇数、偶数)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string? text;
            Match match;

            match = RolRegex.Match(command);
            if (match.Success)
            {
                text = GetRollResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            match = MedRegex.Match(command);
            if (match.Success)
            {
                text = GetMedResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            match = ResRegex.Match(command);
            if (match.Success)
            {
                text = GetResResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            match = InyRegex.Match(command);
            if (match.Success)
            {
                text = GetInnyouResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            match = HtkRegex.Match(command);
            if (match.Success)
            {
                text = GetHattokuResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            match = GogRegex.Match(command);
            if (match.Success)
            {
                text = GetGogyouResult(command, randomizer);
                return text != null ? Result.CreateBuilder(text).Build() : null;
            }

            return null;
        }

        private string? GetRollResult(string command, IRandomizer randomizer)
        {
            var match = RolRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var target = Convert.ToInt32(match.Groups[1].Value);
            var diceList = randomizer.RollBarabara(3, 6);
            var total = diceList.Sum();
            var diceText = string.Join(",", diceList);
            var result = GetCheckResult(diceList, total, target);
            return $"(3d6<={target}) ＞ 出目{diceText}＝合計{total} ＞ {result}";
        }

        private string GetCheckResult(int[] diceList, int total, int target)
        {
            var sorted = diceList.OrderBy(x => x).ToArray();
            if (IsFamble(sorted))
            {
                return FUMBLE;
            }
            if (IsCritical(sorted))
            {
                return CRITICAL;
            }
            if (total <= target)
            {
                return SUCCESS;
            }
            return FAILURE;
        }

        private bool IsFamble(int[] diceList)
        {
            return diceList.SequenceEqual(new[] { 6, 6, 6 });
        }

        private bool IsCritical(int[] diceList)
        {
            return diceList.SequenceEqual(new[] { 1, 2, 3 });
        }

        private string? GetMedResult(string command, IRandomizer randomizer)
        {
            var match = MedRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var yourValue = Convert.ToInt32(match.Groups[1].Value);
            var enemyValue = Convert.ToInt32(match.Groups[2].Value);
            var target = GetTargetFromValue(yourValue, enemyValue);
            var diceList = randomizer.RollBarabara(3, 6);
            var total = diceList.Sum();
            var diceText = string.Join(",", diceList);
            var result = GetCheckResult(diceList, total, target);
            return $"(あなたの値{yourValue}、相手の値{enemyValue}、3d6<={target}) ＞ 出目{diceText}＝合計{total} ＞ {result}";
        }

        private int GetTargetFromValue(int yourValue, int enemyValue)
        {
            return yourValue + (10 - enemyValue);
        }

        private string? GetResResult(string command, IRandomizer randomizer)
        {
            var match = ResRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var yourValue = Convert.ToInt32(match.Groups[1].Value);
            var enemyValue = Convert.ToInt32(match.Groups[2].Value);
            var yourTarget = GetTargetFromValue(yourValue, enemyValue);
            var enemyTarget = GetTargetFromValue(enemyValue, yourValue);
            var yourDiceList = randomizer.RollBarabara(3, 6);
            var yourTotal = yourDiceList.Sum();
            var yourDiceText = string.Join(",", yourDiceList);
            var enemyDiceList = randomizer.RollBarabara(3, 6);
            var enemyTotal = enemyDiceList.Sum();
            var enemyDiceText = string.Join(",", enemyDiceList);
            var yourResult = GetCheckResult(yourDiceList, yourTotal, yourTarget);
            var enemyResult = GetCheckResult(enemyDiceList, enemyTotal, enemyTarget);
            var result = GetResistCheckResult(yourResult, enemyResult);
            return $"あなたの値{yourValue}、相手の値{enemyValue}\n"
                 + $"(あなたのロール 3d6<={yourTarget}) ＞ {yourDiceText}={yourTotal} ＞ {yourResult}\n"
                 + $"(相手のロール 3d6<={enemyTarget}) ＞ {enemyDiceText}={enemyTotal} ＞ {enemyResult}\n"
                 + $"＞{result}";
        }

        private string GetResistCheckResult(string yourResult, string enemyResult)
        {
            var yourRank = GetResultRank(yourResult);
            var enemyRank = GetResultRank(enemyResult);
            if (yourRank > enemyRank)
            {
                return "あなたが勝利";
            }
            if (yourRank < enemyRank)
            {
                return "相手が勝利";
            }
            return "引き分け";
        }

        private int GetResultRank(string result)
        {
            var ranks = new[] { FUMBLE, FAILURE, SUCCESS, CRITICAL };
            return Array.IndexOf(ranks, result);
        }

        private string GetInnyouResult(string command, IRandomizer randomizer)
        {
            var oddCount = 0;
            var evenCount = 0;
            for (var i = 0; i < 3; i++)
            {
                var dice = randomizer.RollOnce(6);
                if (dice % 2 == 0)
                {
                    evenCount += 1;
                }
                else
                {
                    oddCount += 1;
                }
            }
            if (evenCount < oddCount)
            {
                return "陽（奇数の方が多い）";
            }
            else
            {
                return "陰（偶数の方が多い）";
            }
        }

        private string GetHattokuResult(string command, IRandomizer randomizer)
        {
            var oddEvenList = new List<string>();
            for (var i = 0; i < 3; i++)
            {
                oddEvenList.Add(GetOddEven(randomizer));
            }
            var oddEvenText = string.Join("、", oddEvenList);
            switch (oddEvenText)
            {
                case "奇数、奇数、奇数":
                    return $"仁義八徳は、【仁】({oddEvenText})";
                case "奇数、奇数、偶数":
                    return $"仁義八徳は、【義】({oddEvenText})";
                case "奇数、偶数、奇数":
                    return $"仁義八徳は、【礼】({oddEvenText})";
                case "奇数、偶数、偶数":
                    return $"仁義八徳は、【智】({oddEvenText})";
                case "偶数、奇数、奇数":
                    return $"仁義八徳は、【忠】({oddEvenText})";
                case "偶数、奇数、偶数":
                    return $"仁義八徳は、【信】({oddEvenText})";
                case "偶数、偶数、奇数":
                    return $"仁義八徳は、【孝】({oddEvenText})";
                case "偶数、偶数、偶数":
                    return $"仁義八徳は、【悌】({oddEvenText})";
                default:
                    return "異常終了";
            }
        }

        private string GetOddEven(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(6);
            if (dice % 2 == 0)
            {
                return "偶数";
            }
            return "奇数";
        }

        private string GetGogyouResult(string command, IRandomizer randomizer)
        {
            var type = "五行表";
            var table = GetGogyouTable();
            var number = randomizer.RollSum(1, 6);
            var text = table[number - 1];
            var output = $"{type}({number}) ＞ {text}";
            return output;
        }

        private string[] GetGogyouTable()
        {
            return new[] { "五行【木】", "五行【火】", "五行【土】", "五行【金】", "五行【水】", "五行は【任意選択】" };
        }
    }
}
