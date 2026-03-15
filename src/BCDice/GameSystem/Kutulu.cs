using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Kutulu
    /// </summary>
    public sealed class Kutulu : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Kutulu Instance = new Kutulu();

        /// <inheritdoc/>
        public override string Id => "Kutulu";

        /// <inheritdoc/>
        public override string Name => "Kutulu";

        /// <inheritdoc/>
        public override string SortKey => "くとうるう";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■判定　nKU        n: ダイス数

        例)3KU: ダイスを3個振って、その結果を表示(ギリギリでの成功も表示)

        ■対抗判定　nKR        n: ダイス数

        例)2KR: ダイスを2個振って、その結果を表示。対抗判定用の3桁の数字も出力。(大きい方が勝利)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? ResoluteCompetition(command, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result ResoluteAction(string command, IRandomizer randomizer)

        {
            var m = Regex.Match(command, @"(\d)KU");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var num_dices = Convert.ToInt32(m.Groups[1].Value);
            var dices = this._randomizer.RollBarabara(num_dices, 6).OrderBy(x => x);
            var dice_text = string.Join(",", dices);
            var output = $"({num_dices}KU) ＞ {dice_text}";
            var success_num = dices.Count(val => val >= 4);
            var counts_4 = dices.Count(v => v == 4);
            if (success_num > 0)
            {
                output = $" ＞ 成功数{success_num}";
                if (success_num == 1 && counts_4 == 1)
                {
                    output = " ＞ *ギリギリでの成功";
                }
                return Result.CreateBuilder(output).SetSuccess(true).Build();
            }
            else
            {
                output = " ＞ 失敗";
                return Result.CreateBuilder(output).SetFailure(true).Build();
            }
        }

        private Result ResoluteCompetition(string command, IRandomizer randomizer)

        {
            var m = Regex.Match(command, @"(\d)KR");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var num_dices = Convert.ToInt32(m.Groups[1].Value);
            var dices = this._randomizer.RollBarabara(num_dices, 6).OrderBy(x => x);
            var dice_text = string.Join(",", dices);
            var counts_6 = dices.Count(v => v == 6);
            var counts_5 = dices.Count(v => v == 5);
            var success_num = dices.Count(val => val >= 4);
            var com_text = $"({success_num}{counts_6}{counts_5})";
            var output = $"({num_dices}KR) ＞ {dice_text} ＞ {com_text}";
            if (success_num > 0)
            {
                return Result.CreateBuilder(output).SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder(output).SetFailure(true).Build();
            }
        }

    }
}