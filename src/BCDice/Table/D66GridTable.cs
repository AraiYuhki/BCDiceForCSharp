using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// D66を振って6x6マスの表を参照するテーブル
    /// </summary>
    public class D66GridTable : ITable
    {
        private readonly IReadOnlyList<IReadOnlyList<string>> _items;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// D66グリッドテーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="items">6x6のテーブル項目</param>
        public D66GridTable(string name, string command, IReadOnlyList<IReadOnlyList<string>> items)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));

            if (items == null || items.Count != 6)
            {
                throw new ArgumentException("Items must be a 6x6 grid.", nameof(items));
            }

            foreach (var row in items)
            {
                if (row == null || row.Count != 6)
                {
                    throw new ArgumentException("Items must be a 6x6 grid.", nameof(items));
                }
            }

            _items = items;
        }

        /// <summary>
        /// 2次元配列から簡易的にテーブルを作成する
        /// </summary>
        public D66GridTable(string name, string command, string[][] items)
            : this(name, command, ConvertToReadOnlyList(items))
        {
        }

        private static IReadOnlyList<IReadOnlyList<string>> ConvertToReadOnlyList(string[][] items)
        {
            var result = new List<IReadOnlyList<string>>();
            foreach (var row in items)
            {
                result.Add(row);
            }
            return result;
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int dice1 = randomizer.RollOnce(6);
            int dice2 = randomizer.RollOnce(6);
            int value = dice1 * 10 + dice2;

            int index1 = dice1 - 1;
            int index2 = dice2 - 1;

            string item = _items[index1][index2];
            string text = $"{Name}({value}) ＞ {item}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
