using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// 範囲指定のランダムテーブル（2D6形式など）
    /// </summary>
    public class RangeTable : ITable
    {
        private readonly IReadOnlyList<(int Min, int Max, string Text)> _entries;
        private readonly int _diceCount;
        private readonly int _diceSides;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// 範囲テーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="diceCount">ダイス個数</param>
        /// <param name="diceSides">ダイス面数</param>
        /// <param name="entries">テーブルエントリ（最小値、最大値、テキスト）</param>
        public RangeTable(
            string name,
            string command,
            int diceCount,
            int diceSides,
            IReadOnlyList<(int Min, int Max, string Text)> entries)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));
            _diceCount = diceCount;
            _diceSides = diceSides;
            _entries = entries ?? throw new ArgumentNullException(nameof(entries));
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int total = randomizer.RollSum(_diceCount, _diceSides);
            string? item = FindEntry(total);

            if (item == null)
            {
                string errorText = $"{Name}({total}) ＞ 該当なし";
                return Result.CreateBuilder(errorText)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            string text = $"{Name}({total}) ＞ {item}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private string? FindEntry(int value)
        {
            foreach (var entry in _entries)
            {
                if (value >= entry.Min && value <= entry.Max)
                {
                    return entry.Text;
                }
            }
            return null;
        }
    }
}
