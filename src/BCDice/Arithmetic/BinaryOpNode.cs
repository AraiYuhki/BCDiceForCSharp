using System;
using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 二項演算ノード（加算、減算、乗算）
    /// </summary>
    public sealed class BinaryOpNode : IArithmeticNode
    {
        private readonly IArithmeticNode _left;
        private readonly BinaryOperator _op;
        private readonly IArithmeticNode _right;

        /// <summary>
        /// 二項演算ノードを作成する
        /// </summary>
        /// <param name="left">左オペランド</param>
        /// <param name="op">演算子</param>
        /// <param name="right">右オペランド</param>
        public BinaryOpNode(IArithmeticNode left, BinaryOperator op, IArithmeticNode right)
        {
            _left = left;
            _op = op;
            _right = right;
        }

        /// <inheritdoc/>
        public int Eval(RoundType roundType)
        {
            int l = _left.Eval(roundType);
            int r = _right.Eval(roundType);

            return _op switch
            {
                BinaryOperator.Add => l + r,
                BinaryOperator.Subtract => l - r,
                BinaryOperator.Multiply => l * r,
                _ => throw new InvalidOperationException($"Unknown operator: {_op}")
            };
        }

        /// <inheritdoc/>
        public string Output()
        {
            char opChar = _op switch
            {
                BinaryOperator.Add => '+',
                BinaryOperator.Subtract => '-',
                BinaryOperator.Multiply => '*',
                _ => '?'
            };

            return $"{_left.Output()}{opChar}{_right.Output()}";
        }
    }
}
