using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Tests.Core
{
    /// <summary>
    /// テスト用のモック乱数生成器
    /// 事前に設定された値を順番に返す
    /// </summary>
    public class MockRandomizer : Randomizer
    {
        private readonly Queue<int> _predefinedValues;

        /// <summary>
        /// 事前定義された値のリストでモックを初期化する
        /// </summary>
        /// <param name="values">順番に返す値のリスト</param>
        public MockRandomizer(IEnumerable<int> values)
        {
            _predefinedValues = new Queue<int>(values);
        }

        /// <summary>
        /// 事前定義された値のリストでモックを初期化する
        /// </summary>
        /// <param name="values">順番に返す値のリスト</param>
        public MockRandomizer(params int[] values)
            : this((IEnumerable<int>)values)
        {
        }

        /// <inheritdoc/>
        protected override int RollInternal(int sides)
        {
            if (_predefinedValues.Count > 0)
            {
                int value = _predefinedValues.Dequeue();
                AddToRandResults(value, sides);
                return value;
            }

            return base.RollInternal(sides);
        }
    }
}
