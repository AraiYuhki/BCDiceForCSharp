using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// シンプルなランダムテーブル（1Dn形式）
    /// </summary>
    public class SimpleTable : ITable
    {
        private readonly IReadOnlyList<string> _items;
        private readonly int _sides;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// シンプルテーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="items">テーブル項目（インデックス0が出目1に対応）</param>
        public SimpleTable(string name, string command, IReadOnlyList<string> items)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("Items cannot be null or empty.", nameof(items));
            }

            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _items = items;
            _sides = items.Count;
        }

        /// <summary>
        /// 配列から簡易的にテーブルを作成する
        /// </summary>
        public SimpleTable(string name, string command, params string[] items)
            : this(name, command, (IReadOnlyList<string>)items)
        {
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int roll = randomizer.RollOnce(_sides);
            string item = _items[roll - 1];

            string text = $"{Name}({roll}) ＞ {item}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
