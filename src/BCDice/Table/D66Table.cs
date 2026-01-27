using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// D66テーブル（11-66の36項目）
    /// </summary>
    public class D66Table : ITable
    {
        private readonly IReadOnlyDictionary<int, string> _entries;
        private readonly D66SortType _sortType;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// D66テーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="sortType">ソートタイプ</param>
        /// <param name="entries">テーブルエントリ（キーは11-66）</param>
        public D66Table(
            string name,
            string command,
            D66SortType sortType,
            IReadOnlyDictionary<int, string> entries)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _sortType = sortType;
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        /// <summary>
        /// 配列からD66テーブルを作成する（11から順に36項目）
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="sortType">ソートタイプ</param>
        /// <param name="items">36項目の配列</param>
        public D66Table(string name, string command, D66SortType sortType, IReadOnlyList<string> items)
            : this(name, command, sortType, CreateEntriesFromList(items, sortType))
        {
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int roll = randomizer.RollD66(_sortType);

            if (!_entries.TryGetValue(roll, out string? item))
            {
                string errorText = $"{Name}({roll}) ＞ 該当なし";
                return Result.CreateBuilder(errorText)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            string text = $"{Name}({roll}) ＞ {item}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static IReadOnlyDictionary<int, string> CreateEntriesFromList(
            IReadOnlyList<string> items, D66SortType sortType)
        {
            if (items.Count != 36)
            {
                throw new ArgumentException("D66 table must have exactly 36 items.", nameof(items));
            }

            var entries = new Dictionary<int, string>();
            int index = 0;

            if (sortType == D66SortType.Ascending)
            {
                // 昇順: 11,12,13,14,15,16,21,22,...,66
                for (int tens = 1; tens <= 6; tens++)
                {
                    for (int ones = tens; ones <= 6; ones++)
                    {
                        int key = tens * 10 + ones;
                        if (index < items.Count)
                        {
                            entries[key] = items[index++];
                        }
                    }
                }
                // 昇順の場合、21種類のキーしかないので、配列サイズと合わない
                // 通常は36項目全てを使うので、NoSort形式で作成し直す
                entries.Clear();
                index = 0;
            }

            // NoSort（または降順）: 11,12,13,14,15,16,21,22,...,66
            for (int tens = 1; tens <= 6; tens++)
            {
                for (int ones = 1; ones <= 6; ones++)
                {
                    int key = tens * 10 + ones;
                    entries[key] = items[index++];
                }
            }

            return entries;
        }
    }
}
