using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// 連鎖テーブル（結果が別のテーブル参照になりうる）
    /// </summary>
    public class ChainTable : ITable
    {
        private readonly IReadOnlyList<object> _items;
        private readonly int _times;
        private readonly int _sides;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// 連鎖テーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="type">ダイスの種類 (例: "1D6", "2D6")</param>
        /// <param name="items">テーブル項目（文字列またはITable）</param>
        public ChainTable(string name, string command, string type, IReadOnlyList<object> items)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _items = items ?? throw new ArgumentNullException(nameof(items));

            var match = Regex.Match(type, @"(\d+)D(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                throw new ArgumentException($"Unexpected table type: {type}", nameof(type));
            }

            _times = int.Parse(match.Groups[1].Value);
            _sides = int.Parse(match.Groups[2].Value);
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int value = randomizer.RollSum(_times, _sides);
            int index = value - _times;

            if (index < 0 || index >= _items.Count)
            {
                return Result.CreateBuilder($"{Name}({value}) ＞ 範囲外")
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            object chosen = _items[index];

            // 結果がテーブルの場合は再帰的にロール
            string resultText;
            if (chosen is ITable subTable)
            {
                var subResult = subTable.Roll(randomizer);
                resultText = subResult.Text ?? "";
            }
            else
            {
                resultText = chosen?.ToString() ?? "";
            }

            string text = $"{Name}({value}) ＞ {resultText}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
