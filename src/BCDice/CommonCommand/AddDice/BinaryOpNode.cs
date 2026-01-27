using System;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 二項演算ノード
    /// </summary>
    public sealed class BinaryOpNode : IAddDiceNode
    {
        private readonly IAddDiceNode _left;
        private readonly char _op;
        private readonly IAddDiceNode _right;

        /// <summary>
        /// 二項演算ノードを作成する
        /// </summary>
        public BinaryOpNode(IAddDiceNode left, char op, IAddDiceNode right)
        {
            _left = left;
            _op = op;
            _right = right;
        }

        /// <inheritdoc/>
        public int Eval(IGameSystemContext context, IRandomizer? randomizer)
        {
            int l = _left.Eval(context, randomizer);
            int r = _right.Eval(context, randomizer);

            return _op switch
            {
                '+' => l + r,
                '-' => l - r,
                '*' => l * r,
                '/' => r == 0 ? 1 : DivideWithRounding(l, r, context.RoundType),
                _ => throw new InvalidOperationException($"Unknown operator: {_op}")
            };
        }

        /// <inheritdoc/>
        public bool IncludesDice => _left.IncludesDice || _right.IncludesDice;

        /// <inheritdoc/>
        public string Expr(IGameSystemContext context)
        {
            return $"{_left.Expr(context)}{_op}{_right.Expr(context)}";
        }

        /// <inheritdoc/>
        public string Output => $"{_left.Output}{_op}{_right.Output}";

        private static int DivideWithRounding(int dividend, int divisor, RoundType roundType)
        {
            double result = (double)dividend / divisor;

            return roundType switch
            {
                RoundType.Ceiling => (int)Math.Ceiling(result),
                RoundType.Round => (int)Math.Round(result, MidpointRounding.AwayFromZero),
                _ => dividend / divisor
            };
        }
    }
}
