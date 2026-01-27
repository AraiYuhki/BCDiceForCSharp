using System;
using System.Collections.Generic;

namespace BCDice.Core
{
    /// <summary>
    /// 標準の乱数生成器実装
    /// </summary>
    public class Randomizer : IRandomizer
    {
        /// <summary>
        /// ダイスを振る回数の上限
        /// </summary>
        public const int UpperLimitDiceTimes = 200;

        /// <summary>
        /// ダイスの面数の上限
        /// </summary>
        public const int UpperLimitDiceSides = 10000;

        /// <summary>
        /// 乱数生成回数の上限
        /// </summary>
        public const int UpperLimitRands = 10000;

        private readonly List<(int Value, int Sides)> _randResults = new List<(int, int)>();
        private readonly List<DetailedRandResult> _detailedRandResults = new List<DetailedRandResult>();
        private readonly Random _random;

        /// <summary>
        /// 新しい乱数生成器を初期化します
        /// </summary>
        public Randomizer()
        {
            _random = new Random();
        }

        /// <summary>
        /// 指定されたシードで乱数生成器を初期化します
        /// </summary>
        /// <param name="seed">乱数シード</param>
        public Randomizer(int seed)
        {
            _random = new Random(seed);
        }

        /// <inheritdoc/>
        public IReadOnlyList<(int Value, int Sides)> RandResults => _randResults;

        /// <inheritdoc/>
        public IReadOnlyList<DetailedRandResult> DetailedRandResults => _detailedRandResults;

        /// <inheritdoc/>
        public int[] RollBarabara(int times, int sides)
        {
            if (_randResults.Count + times > UpperLimitRands)
            {
                throw new TooManyRandsException();
            }

            if (times <= 0 || times > UpperLimitDiceTimes)
            {
                return Array.Empty<int>();
            }

            var results = new int[times];
            for (int i = 0; i < times; i++)
            {
                results[i] = RollOnce(sides);
            }

            return results;
        }

        /// <inheritdoc/>
        public int RollSum(int times, int sides)
        {
            var rolls = RollBarabara(times, sides);
            int sum = 0;
            for (int i = 0; i < rolls.Length; i++)
            {
                sum += rolls[i];
            }
            return sum;
        }

        /// <inheritdoc/>
        public int RollOnce(int sides)
        {
            if (sides <= 0 || sides > UpperLimitDiceSides)
            {
                return 0;
            }

            int dice = RollInternal(sides);
            PushToDetail(RandResultKind.Normal, sides, dice);

            return dice;
        }

        /// <inheritdoc/>
        public int RollIndex(int sides)
        {
            return RollOnce(sides) - 1;
        }

        /// <inheritdoc/>
        public int RollTensD10()
        {
            int dice = RollInternal(10);
            if (dice == 10)
            {
                dice = 0;
            }

            int result = dice * 10;
            PushToDetail(RandResultKind.TensD10, 10, result);

            return result;
        }

        /// <inheritdoc/>
        public int RollD9()
        {
            int dice = RollInternal(10) - 1;
            PushToDetail(RandResultKind.D9, 10, dice);

            return dice;
        }

        /// <inheritdoc/>
        public int RollD66(D66SortType sortType)
        {
            int dice1 = RollOnce(6);
            int dice2 = RollOnce(6);

            switch (sortType)
            {
                case D66SortType.Ascending:
                    if (dice1 > dice2)
                    {
                        (dice1, dice2) = (dice2, dice1);
                    }
                    break;

                case D66SortType.Descending:
                    if (dice1 < dice2)
                    {
                        (dice1, dice2) = (dice2, dice1);
                    }
                    break;

                case D66SortType.NoSort:
                default:
                    break;
            }

            return dice1 * 10 + dice2;
        }

        /// <summary>
        /// 内部の乱数生成メソッド（1からsidesまでの整数を返す）
        /// </summary>
        /// <param name="sides">ダイスの面数</param>
        /// <returns>1以上sides以下の整数</returns>
        protected virtual int RollInternal(int sides)
        {
            if (_randResults.Count >= UpperLimitRands)
            {
                throw new TooManyRandsException();
            }

            int dice = _random.Next(sides) + 1;
            AddToRandResults(dice, sides);

            return dice;
        }

        /// <summary>
        /// ダイスロール結果を履歴に追加する（サブクラス用）
        /// </summary>
        /// <param name="value">ダイスの出目</param>
        /// <param name="sides">ダイスの面数</param>
        protected void AddToRandResults(int value, int sides)
        {
            _randResults.Add((value, sides));
        }

        private void PushToDetail(RandResultKind kind, int sides, int value)
        {
            _detailedRandResults.Add(new DetailedRandResult(kind, sides, value));
        }
    }
}
