using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ネバークラウドTRPG
    /// </summary>
    public sealed class NeverCloud : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NeverCloud Instance = new NeverCloud();

        /// <inheritdoc/>
        public override string Id => "NeverCloud";

        /// <inheritdoc/>
        public override string Name => "ネバークラウドTRPG";

        /// <inheritdoc/>
        public override string SortKey => "ねはあくらうとTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・LIST　のコマンドを入力して、ルールの解説・2D6表、一覧の表示

        ・判定(xNC±y>=z)
        　xD6の判定を行います。ファンブル、クリティカルの場合、その旨を出力します。
        　x：振るダイスの数。
        　±y：固定値・修正値。省略可能。
        　z：目標値。省略可能。
        　ダイスの出目ふたつが6ならクリティカル(自動成功)
        　ダイスの出目すべてが1ならファンブル(自動失敗)
        　例）　2NC+2>=5　1NC
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (Regex.IsMatch(command, @"^(\d+)(?:NC|D6?)((?:[-+]\d+)*)(>=(\d+))?$", RegexOptions.IgnoreCase))
            {
                return CheckAction(command, randomizer);
            }
            // TODO: TEXTS, TABLES, ALIAS_TEXTS dictionaries not yet ported
            return null;
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)(?:NC|D6?)((?:[-+]\d+)*)(>=(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var dice_count = int.Parse(m.Groups[1].Value);
            var modify_str = m.Groups[2].Value;
            var modify_number = !string.IsNullOrEmpty(modify_str)
                ? (ArithmeticEvaluator.Eval(modify_str, RoundType) ?? 0)
                : 0;
            var cmp_str = m.Groups[3].Value;
            int? target = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : (int?)null;

            if (modify_number == 0)
            {
                modify_str = "";
            }

            var dice_list = randomizer.RollBarabara(dice_count, 6);
            var dice_value = dice_list.Sum();
            var dice_str = string.Join(",", dice_list);

            var total = dice_value + modify_number;

            string? result;
            if (dice_list.Count(x => x == 1) == dice_count)
            {
                total = 0;
                result = "ファンブル";
            }
            else if (dice_list.Count(x => x == 6) >= 2)
            {
                result = "クリティカル";
            }
            else if (target.HasValue)
            {
                result = total >= target.Value ? "成功" : "失敗";
            }
            else
            {
                result = null;
            }

            var sequence = new object?[]
            {
                $"({dice_count}D6{modify_str}{cmp_str})",
                $"{dice_value}[{dice_str}]{modify_str}",
                total,
                result
            }.Where(x => x != null).Select(x => x!.ToString());

            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

    }
}
