using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 終末アイドル育成TRPG セイレーン
    /// </summary>
    public sealed class Siren : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Siren Instance = new Siren();


        private static readonly Regex SlRegex = new Regex(
            @"^SL",
            RegexOptions.Compiled);

        private static readonly Regex TrRegex = new Regex(
            @"^TR",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Siren";

        /// <inheritdoc/>
        public override string Name => "終末アイドル育成TRPG セイレーン";

        /// <inheritdoc/>
        public override string SortKey => "せいれえん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定: SL+a<=b±c
          a=達成値への修正(0の場合は省略)
          b=能力値
          c=判定への修正(0の場合は省略、複数可)
        例)判定修正-10の装備を装着しながら【技術：60】〈兵器：2〉で判定する場合。
        SL+2<=60+40-10

        ・育成: TR$a<=b
          a=育成した回数
          b=ヘルス
        例）ヘルスの現在値が60で2回目の【身体】の育成を行う場合。
        TR$2<=60
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = SlRegex.Match(command);
            if (match.Success)
            {
                return CheckAction(command, randomizer);
            }

            match = TrRegex.Match(command);
            if (match.Success)
            {
                return CheckTraining(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            // SL[+/-modifier]<=target[+/-adjustments...]
            // modifier is achievement value bonus, target is ability value (can be arithmetic sum)
            var m = Regex.Match(command, @"^SL([+-]\d+)?<=(.+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }
            var modifyNumber = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var targetExpr = m.Groups[2].Value;
            // Evaluate simple arithmetic target (e.g. "60+40-10")
            var target = EvalArithmetic(targetExpr);
            if (target == null)
            {
                return null;
            }
            var dice = randomizer.RollOnce(100);
            if (dice > target)
            {
                return Result.CreateBuilder($"(1D100<={target}) ＞ {dice} ＞ 失敗").SetFailure(true).Build();
            }
            var dig10 = dice / 10;
            var dig1 = dice % 10;
            if (dig10 == 0)
            {
                dig10 = 10;
            }
            if (dig1 == 0)
            {
                dig1 = 10;
            }
            var achievement_value = dig10 + dig1 + modifyNumber;
            return Result.CreateBuilder($"(1D100<={target}) ＞ {dice} ＞ 成功(達成値：{achievement_value})").SetSuccess(true).Build();
        }

        private Result? CheckTraining(string command, IRandomizer randomizer)
        {
            // TR$count<=target
            var m = Regex.Match(command, @"^TR\$(\d+)<=(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }
            var count = int.Parse(m.Groups[1].Value);
            var target = int.Parse(m.Groups[2].Value);
            var dice = randomizer.RollOnce(100);
            var dig10 = dice / 10;
            var dig1 = dice % 10;
            if (dig10 == 0)
            {
                dig10 = 10;
            }
            if (dig1 == 0)
            {
                dig1 = 10;
            }
            var achievement_value = dig10 + dig1;
            if (dice > target)
            {
                return Result.CreateBuilder($"(1D100<={target}) ＞ {dice} ＞ 失敗(能力値減少：10 / ヘルス減少：{achievement_value})").SetFailure(true).Build();
            }
            return Result.CreateBuilder($"(1D100<={target}) ＞ {dice} ＞ 成功(能力値上昇：{count * 5 + achievement_value} / ヘルス減少：{achievement_value})").SetSuccess(true).Build();
        }

        /// <summary>
        /// 単純な四則演算式（整数の加減算）を評価する
        /// </summary>
        private static int? EvalArithmetic(string expr)
        {
            var matches = Regex.Matches(expr, @"([+-]?\d+)");
            if (matches.Count == 0)
            {
                return null;
            }
            int total = 0;
            foreach (Match match in matches)
            {
                total += int.Parse(match.Value);
            }
            return total;
        }

    }
}