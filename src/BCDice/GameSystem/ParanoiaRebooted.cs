using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// パラノイア リブーテッド
    /// </summary>
    public sealed class ParanoiaRebooted : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ParanoiaRebooted Instance = new ParanoiaRebooted();


        private static readonly Regex NdRegex = new Regex(
            @"^ND",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MpRegex = new Regex(
            @"^MP",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "ParanoiaRebooted";

        /// <inheritdoc/>
        public override string Name => "パラノイア リブーテッド";

        /// <inheritdoc/>
        public override string SortKey => "はらのいありふうてつと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ※コマンドは入力内容の前方一致で検出しています。
        ・通常の判定　NDx
        　x：ノードダイスの数.マイナスも可.
        　ノードダイスの絶対値 + 1個(コンピュータダイス)のダイスがロールされる.
        例）ND2　ND-3

        ・ミュータントパワー判定　MPx
          x：ノードダイスの数.
        　ノードダイスの値 + 1個(コンピュータダイス)のダイスがロールされる.
        例）MP2
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = NdRegex.Match(command);
            if (match.Success)
            {
                return GetNodeDiceRoll(command, randomizer);
            }

            match = MpRegex.Match(command);
            if (match.Success)
            {
                return GetMutantPowerRoll(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        /// <summary>
        /// ダイスリストの最後の要素が6なら'C'に置き換え、(結果リスト, Computerメッセージ)を返す
        /// </summary>
        private (List<string> results, string computerDiceMessage) GenerateRollResults(int[] dices)
        {
            var computerDiceMessage = "";
            var results = dices.Select(d => d.ToString()).ToList();
            if (results.Count > 0 && dices[dices.Length - 1] == 6)
            {
                results[results.Count - 1] = "C";
                computerDiceMessage = "(Computer)";
            }
            return (results, computerDiceMessage);
        }

        private Result? GetNodeDiceRoll(string command, IRandomizer randomizer)
        {
            Debug("eval_game_system_specific_command Begin");
            var m = Regex.Match(command, @"^ND((-)?\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            Debug("command", command);
            var parameterNum = Convert.ToInt32(m.Groups[1].Value);
            var diceCount = Math.Abs(parameterNum) + 1;
            var dices = randomizer.RollBarabara(diceCount, 6);
            var successRate = dices.Count(dice => dice >= 5);
            if (parameterNum < 0)
            {
                successRate -= dices.Count(dice => dice < 5);
            }
            Debug("dices", dices);
            var (results, computerDiceMessage) = GenerateRollResults(dices);
            Debug("eval_game_system_specific_command result");
            var text = $"({command}) ＞ [{string.Join(", ", results)}] ＞ 成功度{successRate}{computerDiceMessage}";
            return Result.CreateBuilder(text).SetRands(randomizer.RandResults).Build();
        }

        private Result? GetMutantPowerRoll(string command, IRandomizer randomizer)
        {
            Debug("eval_game_system_specific_command Begin");
            var m = Regex.Match(command, @"^MP(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            Debug("command", command);
            var parameterNum = Convert.ToInt32(m.Groups[1].Value);
            var diceCount = Math.Abs(parameterNum) + 1;
            var dices = randomizer.RollBarabara(diceCount, 6);
            var failureRate = dices.Count(v => v == 1);
            var message = (failureRate == 0) ? "成功" : $"失敗({failureRate})";
            var (results, computerDiceMessage) = GenerateRollResults(dices);
            Debug("dices", dices);
            Debug("eval_game_system_specific_command result");
            var text = $"({command}) ＞ [{string.Join(", ", results)}] ＞ {message}{computerDiceMessage}";
            return Result.CreateBuilder(text).SetRands(randomizer.RandResults).Build();
        }

    }
}
