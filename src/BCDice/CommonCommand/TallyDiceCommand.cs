using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand
{
    /// <summary>
    /// 個数カウントダイスコマンド
    /// 例: 5TY6 - 5個のD6を振り、各出目が何個あるかをカウント（0個は非表示）
    /// 例: 5TZ6 - 5個のD6を振り、各出目が何個あるかをカウント（0個も表示）
    /// </summary>
    public class TallyDiceCommand : ICommonCommand
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TallyDiceCommand Instance = new TallyDiceCommand();

        /// <summary>
        /// 最大面数
        /// </summary>
        private const int MaxSides = 20;

        private static readonly Regex CommandRegex = new Regex(
            @"^S?(\d+)T([YZ])(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public string PrefixPattern => @"\d+T[YZ]\d+";

        /// <inheritdoc/>
        public Result? Eval(string command, IGameSystemContext gameSystem, IRandomizer randomizer)
        {
            var match = CommandRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase) &&
                           !char.IsDigit(command[0]);

            int times = int.Parse(match.Groups[1].Value);
            string typeChar = match.Groups[2].Value.ToUpperInvariant();
            int sides = int.Parse(match.Groups[3].Value);

            bool showZeros = typeChar == "Z";

            // バリデーション
            if (times <= 0 || sides <= 0)
            {
                return null;
            }

            string notation = $"{times}T{typeChar}{sides}";

            if (sides > MaxSides)
            {
                return Result.CreateBuilder($"({notation}) ＞ 面数は1以上、{MaxSides}以下としてください")
                    .SetSecret(isSecret)
                    .Build();
            }

            // ダイスを振る
            var rolls = randomizer.RollBarabara(times, sides);

            // ソート設定に応じてソート（表示用）
            var sortedRolls = gameSystem.SortBarabaraDice
                ? rolls.OrderBy(x => x).ToArray()
                : rolls;

            // 各出目の個数をカウント
            var valueCounts = rolls
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());

            // 結果文字列を構築
            var countStrings = new List<string>();
            for (int v = 1; v <= sides; v++)
            {
                int count = valueCounts.TryGetValue(v, out int c) ? c : 0;

                if (count == 0 && !showZeros)
                {
                    continue;
                }

                countStrings.Add($"[{v}]×{count}");
            }

            var sb = new StringBuilder();
            sb.Append($"({notation})");
            sb.Append($" ＞ {string.Join(",", sortedRolls)}");
            sb.Append($" ＞ {string.Join(", ", countStrings)}");

            return Result.CreateBuilder(sb.ToString())
                .SetSecret(isSecret)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
