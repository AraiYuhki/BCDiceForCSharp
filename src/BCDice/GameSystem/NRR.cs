using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// nRR
    /// </summary>
    public sealed class NRR : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NRR Instance = new NRR();

        /// <inheritdoc/>
        public override string Id => "NRR";

        /// <inheritdoc/>
        public override string Name => "nRR";

        /// <inheritdoc/>
        public override string SortKey => "えぬああるあある";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ▪️判定
        ・ノーマルダイス　NR8
        ・有利ダイス　NR10
        ・不利ダイス　NR6
        ・Exダイス　NR12

        ダイスの個数を指定しての判定ができます。
        例：有利ダイス2個で判定　2NR10

        ▪️判定結果とシンボル
        ⭕：成功
        ❌：失敗
        ✨：クリティカル（大成功）
        💀：ファンブル（大失敗）
        🌈：ミラクル（奇跡）
        ";

        // レベル定義
        private const string LEVEL_FUMBLE = "fumble";
        private const string LEVEL_FAILURE = "failure";
        private const string LEVEL_SUCCESS = "success";
        private const string LEVEL_CRITICAL = "critical";
        private const string LEVEL_MIRACLE = "miracle";

        private static readonly string[] LEVELS = { LEVEL_FUMBLE, LEVEL_FAILURE, LEVEL_SUCCESS, LEVEL_CRITICAL, LEVEL_MIRACLE };

        private static readonly HashSet<string> SUCCESSES = new HashSet<string> { LEVEL_SUCCESS, LEVEL_CRITICAL, LEVEL_MIRACLE };
        private static readonly HashSet<string> CRITICALS = new HashSet<string> { LEVEL_CRITICAL, LEVEL_MIRACLE };

        private static readonly string[] DISADVANTAGE = { LEVEL_FUMBLE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_SUCCESS, LEVEL_SUCCESS };
        private static readonly string[] NORMAL = { LEVEL_FUMBLE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_CRITICAL };
        private static readonly string[] ADVANTAGE = { LEVEL_FUMBLE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_CRITICAL, LEVEL_CRITICAL };
        private static readonly string[] EXTRA = { LEVEL_FUMBLE, LEVEL_FUMBLE, LEVEL_FAILURE, LEVEL_FAILURE, LEVEL_SUCCESS, LEVEL_SUCCESS, LEVEL_CRITICAL, LEVEL_CRITICAL, LEVEL_CRITICAL, LEVEL_CRITICAL, LEVEL_MIRACLE, LEVEL_MIRACLE };

        private static readonly Dictionary<string, string> ICON = new Dictionary<string, string>
        {
            { LEVEL_FUMBLE, "\U0001F480" },
            { LEVEL_FAILURE, "\u274C" },
            { LEVEL_SUCCESS, "\u2B55\uFE0F" },
            { LEVEL_CRITICAL, "\u2728" },
            { LEVEL_MIRACLE, "\U0001F308" },
        };

        private static readonly Dictionary<string, string> RESULT_LABEL = new Dictionary<string, string>
        {
            { LEVEL_FUMBLE, "ファンブル（大失敗）" },
            { LEVEL_FAILURE, "失敗" },
            { LEVEL_SUCCESS, "成功" },
            { LEVEL_CRITICAL, "クリティカル（大成功）" },
            { LEVEL_MIRACLE, "ミラクル（奇跡）" },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollNr(command, randomizer);
        }

        private Result? RollNr(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)?NR(6|8|10|12)$");
            if (!m.Success)
            {
                return null;
            }

            var times = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 1;

            string[] table;
            switch (m.Groups[2].Value)
            {
                case "6":
                    table = DISADVANTAGE;
                    break;
                case "8":
                    table = NORMAL;
                    break;
                case "10":
                    table = ADVANTAGE;
                    break;
                default:
                    table = EXTRA;
                    break;
            }

            var values = randomizer.RollBarabara(times, table.Length);

            bool isSuccess = false;
            bool isFumble = false;
            bool isCritical = false;
            string text;

            if (times == 1)
            {
                var level = table[values[0] - 1];
                isSuccess = SUCCESSES.Contains(level);
                isFumble = level == LEVEL_FUMBLE;
                isCritical = CRITICALS.Contains(level);

                text = $"{ICON[level]} {RESULT_LABEL[level]}";
            }
            else
            {
                var levels = values.Select(v => table[v - 1]).ToArray();
                var valuesCount = levels
                    .GroupBy(x => x)
                    .ToDictionary(g => g.Key, g => g.Count());

                var valuesCountStrs = new List<string>();
                foreach (var l in LEVELS)
                {
                    var count = valuesCount.ContainsKey(l) ? valuesCount[l] : 0;
                    if (count == 0)
                    {
                        continue;
                    }
                    valuesCountStrs.Add($"{ICON[l]} {count}");
                }

                text = string.Join(", ", valuesCountStrs);
            }

            var timesStr = times == 1 ? "" : times.ToString();
            var resultText = $"({timesStr}NR{m.Groups[2].Value}) ＞ {string.Join(",", values)} ＞ {text}";

            return Result.CreateBuilder(resultText)
                .SetCondition(isSuccess)
                .SetFumble(isFumble)
                .SetCritical(isCritical)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
