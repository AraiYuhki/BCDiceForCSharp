using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 数値ノード
    /// </summary>
    public sealed class NumberNode : IArithmeticNode
    {
        private readonly int _value;

        /// <summary>
        /// 数値ノードを作成する
        /// </summary>
        /// <param name="value">数値</param>
        public NumberNode(int value)
        {
            _value = value;
        }

        /// <inheritdoc/>
        public int Eval(RoundType roundType)
        {
            return _value;
        }

        /// <inheritdoc/>
        public string Output()
        {
            return _value.ToString();
        }
    }
}
