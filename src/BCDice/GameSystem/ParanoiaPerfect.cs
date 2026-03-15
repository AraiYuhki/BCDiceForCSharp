using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// パラノイア・パーフェクト エディション
    /// </summary>
    public sealed class ParanoiaPerfect : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ParanoiaPerfect Instance = new ParanoiaPerfect();

        /// <inheritdoc/>
        public override string Id => "ParanoiaPerfect";

        /// <inheritdoc/>
        public override string Name => "パラノイア・パーフェクト エディション";

        /// <inheritdoc/>
        public override string SortKey => "はらのいあはあふえくとえていしよん";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ※コマンドは入力内容の前方一致で検出しています。
        ・通常の判定　NDx,y
        　x：ノードダイスの数.マイナスも可.
        　y: 反逆スターの数.省略可.省略時0
        　ノードダイスの絶対値 + 1個(コンピュータダイス)のダイスがロールされる.
        例）ND2　ND-3　ND2,1　ND-3,2
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return GetNodeDiceRoll(command, randomizer);
        }

        private (string[] results, string computerDiceMessage) GenerateRollResults(int traitorous_star, int[] dices)
        {
            var results = dices.Select(d => d.ToString()).ToArray();
            var computer_dice_message = "";
            var last_die = dices[dices.Length - 1];
            if (last_die >= 6 - traitorous_star)
            {
                results[results.Length - 1] = $"{last_die}C";
                computer_dice_message = "(Computer)";
            }
            return (results, computer_dice_message);
        }

        private Result? GetNodeDiceRoll(string command, IRandomizer randomizer)
        {
            Debug("eval_game_system_specific_command Begin");

            var m = Regex.Match(command, @"^ND((-)?\d+)(,(\d+))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            Debug("command", command);

            var parameter_num = Convert.ToInt32(m.Groups[1].Value);
            var traitorous_star = m.Groups[4].Success && m.Groups[4].Value.Length > 0
                ? Convert.ToInt32(m.Groups[4].Value)
                : 0;
            var dice_count = Math.Abs(parameter_num) + 1;

            var dices = randomizer.RollBarabara(dice_count, 6);

            var success_rate = dices.Count(dice => dice >= 5);
            if (parameter_num < 0)
            {
                success_rate -= dices.Count(dice => dice < 5);
            }

            Debug("dices", string.Join(", ", dices));

            var (results, computer_dice_message) = GenerateRollResults(traitorous_star, dices);

            Debug("eval_game_system_specific_command result");

            var text = $"({command}) ＞ [{string.Join(", ", results)}] ＞ 成功度{success_rate}{computer_dice_message}";
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

    }
}
