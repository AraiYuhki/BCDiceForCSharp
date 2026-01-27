using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 選択コマンド
    /// 例: choice[A,B,C], CHOICE[赤,青,緑]
    /// </summary>
    public class ChoiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly ChoiceCommand Instance = new ChoiceCommand();

        private static readonly Regex CommandRegex = new Regex(
            @"^S?CHOICE\[([^\]]+)\]",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"CHOICE\[";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !command.StartsWith("CHOICE", StringComparison.OrdinalIgnoreCase);

            string optionsText = match.Groups[1].Value;
            var options = ParseOptions(optionsText);

            if (options.Count == 0)
            {
                return null;
            }

            int index = randomizer.RollOnce(options.Count) - 1;
            string selected = options[index];

            string text = $"({string.Join(",", options)}) ＞ {selected}";

            return Result.CreateBuilder(text)
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static List<string> ParseOptions(string optionsText)
        {
            var options = new List<string>();
            int depth = 0;
            int start = 0;

            for (int i = 0; i < optionsText.Length; i++)
            {
                char c = optionsText[i];

                if (c == '[')
                {
                    depth++;
                }
                else if (c == ']')
                {
                    depth--;
                }
                else if (c == ',' && depth == 0)
                {
                    string option = optionsText.Substring(start, i - start).Trim();
                    if (!string.IsNullOrEmpty(option))
                    {
                        options.Add(option);
                    }
                    start = i + 1;
                }
            }

            // 最後のオプションを追加
            string lastOption = optionsText.Substring(start).Trim();
            if (!string.IsNullOrEmpty(lastOption))
            {
                options.Add(lastOption);
            }

            return options;
        }
    }
}
