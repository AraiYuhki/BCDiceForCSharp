using System;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.CommonCommand.AddDice;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 繰り返しコマンド
    /// 例: x3 2D6, 3#2D6, rep3 2D6
    /// </summary>
    public class RepeatCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly RepeatCommand Instance = new RepeatCommand();

        private const int MaxRepeatCount = 100;

        private static readonly Regex CommandRegex = new Regex(
            @"^S?(?:X(\d+)|(\d+)#|REP(\d+))\s*(.+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"(?:X\d+|REP\d+|\d+#)";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]) &&
                           !command.StartsWith("X", StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("REP", StringComparison.OrdinalIgnoreCase);

            // 繰り返し回数を取得
            int repeatCount = 0;
            if (match.Groups[1].Success)
            {
                repeatCount = int.Parse(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                repeatCount = int.Parse(match.Groups[2].Value);
            }
            else if (match.Groups[3].Success)
            {
                repeatCount = int.Parse(match.Groups[3].Value);
            }

            if (repeatCount <= 0 || repeatCount > MaxRepeatCount)
            {
                return null;
            }

            string innerCommand = match.Groups[4].Value.Trim();
            if (string.IsNullOrEmpty(innerCommand))
            {
                return null;
            }

            // 内部コマンドを繰り返し実行
            var sb = new StringBuilder();
            sb.Append($"({repeatCount}回) ＞ ");

            var results = new System.Collections.Generic.List<string>();
            for (int i = 0; i < repeatCount; i++)
            {
                var innerResult = AddDiceCommand.Instance.Eval(innerCommand, gameSystem, randomizer);
                if (innerResult == null)
                {
                    return null;
                }

                results.Add(innerResult.Text);
            }

            sb.Append(string.Join(" / ", results));

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
