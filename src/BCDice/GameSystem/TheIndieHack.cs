using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// The Indie Hack
    /// </summary>
    public sealed class TheIndieHack : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TheIndieHack Instance = new TheIndieHack();

        /// <inheritdoc/>
        public override string Id => "TheIndieHack";

        /// <inheritdoc/>
        public override string Name => "The Indie Hack";

        /// <inheritdoc/>
        public override string SortKey => "しいんていはつく";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■判定　cIH+a        c:CL  a:能力値

        例)IH: ライトダイスとダークダイスを1個ずつ振って、その結果を表示
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            ResoluteAction(command, randomizer);

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result ResoluteAction(string command, IRandomizer randomizer)

        {
            string output = "";
            var m = Regex.Match(command, @"([+-]?\d)?IH([+-]\d)?");
            if (m.Success) {
                return null;
            }
            else
            {
                return null;
            }
            var cl = Convert.ToInt32(m.Groups[1].Value);
            var abilities = Convert.ToInt32(m.Groups[2].Value);
            var dices = randomizer.RollBarabara(2, 6);
            var dice_text = string.Join(",", dices);
            dices[0] += cl;
            dices[1] += abilities;
            var dice_text2 = string.Join(",", dices);
            var diff = dices[1] - dices[0];
            var side = diff < 0 ? "ライト" : "ダーク";
            if (diff == 0)
            {
                side = "両";
            }
            if (dice_text == dice_text2)
            {
                output = $"(IH) ＞ {dice_text} ＞ {side}{GetSuccessLevel(diff)}";
            }
            else
            {
                output = $"({m.Groups[1].Value}IH{m.Groups[2].Value}) ＞ [{dice_text}] ＞ {dice_text2} ＞ {side}{GetSuccessLevel(diff)}";
            }
            if (diff > 0)
            {
                return Result.CreateBuilder(output).SetSuccess(true).Build();
            }
            else
            {
                if (diff < 0)
                {
                    return Result.CreateBuilder(output).SetFailure(true).Build();
                }
                else
                {
                    return Result.CreateBuilder(output).Build();
                }
            }
        }

        private string GetSuccessLevel(int die_difference)

        {
            switch (Math.Abs(die_difference))
            {
                case 0:
                {
                    return "陣営がそれぞれ確定描写を1つ追加します";
                    break;
                }
                case 1:
                {
                    return "陣営が確定描写を1つ追加しますが、味方によって追加されたネガティブな確定描写を1つ受けます";
                    break;
                }
                case 2:
                {
                    return "陣営が確定描写を1つ追加します";
                    break;
                }
                case 3:
                {
                    return "陣営が確定描写を1つ追加し、さらに場面描写を1つ追加します";
                    break;
                }
                case 4:
                {
                    return "陣営が確定描写を1つ追加し、さらにその陣営の味方ひとりも確定描写を1つ追加します";
                    break;
                }
                default:
                {
                    return "陣営が確定描写を2つ追加します";
                    break;
                }
            }
        }

    }
}