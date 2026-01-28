using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// 出目の偶奇による場合分け機能をもつD66表
    /// </summary>
    public class D66ParityTable : ITable
    {
        private readonly IReadOnlyList<string> _oddItems;
        private readonly IReadOnlyList<string> _evenItems;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string Command { get; }

        /// <summary>
        /// D66パリティテーブルを作成する
        /// </summary>
        /// <param name="name">テーブル名</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="oddItems">左ダイスが奇数の場合の項目（6要素）</param>
        /// <param name="evenItems">左ダイスが偶数の場合の項目（6要素）</param>
        public D66ParityTable(string name, string command, IReadOnlyList<string> oddItems, IReadOnlyList<string> evenItems)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Command = command ?? throw new ArgumentNullException(nameof(command));

            if (oddItems == null || oddItems.Count != 6)
            {
                throw new ArgumentException("Odd items must have exactly 6 elements.", nameof(oddItems));
            }
            if (evenItems == null || evenItems.Count != 6)
            {
                throw new ArgumentException("Even items must have exactly 6 elements.", nameof(evenItems));
            }

            _oddItems = oddItems;
            _evenItems = evenItems;
        }

        /// <summary>
        /// 配列から簡易的にテーブルを作成する
        /// </summary>
        public D66ParityTable(string name, string command, string[] oddItems, string[] evenItems)
            : this(name, command, (IReadOnlyList<string>)oddItems, (IReadOnlyList<string>)evenItems)
        {
        }

        /// <inheritdoc/>
        public Result Roll(IRandomizer randomizer)
        {
            int dice1 = randomizer.RollOnce(6);
            int dice2 = randomizer.RollOnce(6);

            var items = (dice1 % 2 == 1) ? _oddItems : _evenItems;
            string item = items[dice2 - 1];

            int value = dice1 * 10 + dice2;
            string text = $"{Name}({value}) ＞ {item}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }
    }
}
