using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// としあきの聖杯戦争TRPG
    /// </summary>
    public sealed class ToshiakiHolyGrailWar : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ToshiakiHolyGrailWar Instance = new ToshiakiHolyGrailWar();

        private static readonly Regex FCommandRegex = new Regex(
            @"^F(\d+)((?:\+\d+)+)?((?:-\d+)+)?(@([-+]?\d+))?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "ToshiakiHolyGrailWar";

        /// <inheritdoc/>
        public override string Name => "としあきの聖杯戦争TRPG";

        /// <inheritdoc/>
        public override string SortKey => "としあきのせいはいせんそうTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定 (Fx+y-z@a>=t)
          補正値ペナルティを自動計算してダイスの面数を決定しダイスロールを実行します。
          ダイス面数は2以上、10以下の範囲に制限されます。
          x: ステータス
          y: 補正値 (任意)
          z: マイナス補正値 (任意)
          a: ダイス面数の増量 (任意)
          t: 目標値 (任意)
          例)
            F8+11, F8+11-5, F8+11-5@1, F8+11+9-3-2@-1, F8+11-5>=50, F8
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollF(command, randomizer);
        }

        private Result? RollF(string command, IRandomizer randomizer)
        {
            var m = FCommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var status = int.Parse(m.Groups[1].Value);
            var positiveModifier = m.Groups[2].Success ? EvalSimpleArithmetic(m.Groups[2].Value) : 0;
            var negativeModifier = m.Groups[3].Success ? EvalSimpleArithmetic(m.Groups[3].Value) : 0;
            var sideBonus = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 0;
            var hasCmpOp = m.Groups[6].Success;
            var targetNumber = m.Groups[7].Success ? int.Parse(m.Groups[7].Value) : 0;

            var times = Math.Max(status + positiveModifier + negativeModifier, 0);
            var sides = ((6 - PositiveModifierPenalty(positiveModifier) + NegativeModifierBonus(negativeModifier) + sideBonus) < 2 ? 2 : (6 - PositiveModifierPenalty(positiveModifier) + NegativeModifierBonus(negativeModifier) + sideBonus) > 10 ? 10 : (6 - PositiveModifierPenalty(positiveModifier) + NegativeModifierBonus(negativeModifier) + sideBonus));

            var list = randomizer.RollBarabara(times, sides);
            var total = list.Sum();

            Result result;
            if (!hasCmpOp)
            {
                result = Result.CreateBuilder("").Build();
            }
            else if (total >= targetNumber)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            var cmpOpStr = hasCmpOp ? $">={targetNumber}" : "";
            var sequence = new List<string?>
            {
                command,
                $"({times}D{sides}{cmpOpStr})",
                $"{total}[{string.Join(",", list)}]",
                total.ToString(),
                result.Text
            }.Where(x => !string.IsNullOrEmpty(x)).ToList();

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .Build();
        }

        private int PositiveModifierPenalty(int modifier)
        {
            return modifier <= 10 ? 0 : modifier / 10;
        }

        private int NegativeModifierBonus(int modifier)
        {
            return modifier <= -5 ? 1 : 0;
        }

        /// <summary>
        /// 簡易的な加減算の評価（"+3+5-2" のような文字列）
        /// </summary>
        private static int EvalSimpleArithmetic(string expr)
        {
            var result = 0;
            var current = "";
            foreach (var ch in expr)
            {
                if (ch == '+' || ch == '-')
                {
                    if (current.Length > 0)
                    {
                        result += int.Parse(current);
                    }
                    current = ch.ToString();
                }
                else
                {
                    current += ch;
                }
            }
            if (current.Length > 0)
            {
                result += int.Parse(current);
            }
            return result;
        }

    }
}
