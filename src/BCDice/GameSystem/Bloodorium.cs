using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ブラドリウム
    /// </summary>
    public sealed class Bloodorium : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Bloodorium Instance = new Bloodorium();

        /// <inheritdoc/>
        public override string Id => "Bloodorium";

        /// <inheritdoc/>
        public override string Name => "ブラドリウム";

        /// <inheritdoc/>
        public override string SortKey => "ふらとりうむ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・ダイスチェック xDC+y
        　【ダイスチェック】を行う。《トライアンフ》を結果に自動反映する。
        　x: ダイス数
        　y: 結果への修正値 （省略可）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return Dicecheck(command, randomizer);
        }

        private Result? Dicecheck(string command, IRandomizer randomizer)
        {
            // xDC[+y] or xDC[-y]: prefix number = dice count, optional modifier, no cmp op
            var m = Regex.Match(command, @"^(\d+)DC([+-]\d+)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }
            var prefixNumber = int.Parse(m.Groups[1].Value);
            var modifyNumber = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 0;

            var dice_list = randomizer.RollBarabara(prefixNumber, 6).OrderBy(x => x).ToList();
            var dice_value = dice_list.Max();
            var values_count = dice_list.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            var triumph = values_count.Values.Max();
            var total = dice_value * triumph + modifyNumber;

            var commandText = $"{prefixNumber}DC{Format.Modifier(modifyNumber)}";
            var sequence = new List<string>();
            sequence.Add($"({commandText})");
            sequence.Add($"[{string.Join(",", dice_list)}]{Format.Modifier(modifyNumber)}");
            if (triumph > 1)
            {
                sequence.Add($"《トライアンフ》(*{triumph})");
            }
            if (total != dice_value)
            {
                sequence.Add(TotalExpr(dice_value, triumph, modifyNumber));
            }
            sequence.Add(total.ToString());

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetCritical(triumph > 1)
                .Build();
        }

        private string TotalExpr(int dice_value, int triumph, int modify_number)
        {
            var formated_triumph = (triumph > 1) ? $"*{triumph}" : "";
            return $"{dice_value}{formated_triumph}{Format.Modifier(modify_number)}";
        }

    }
}
