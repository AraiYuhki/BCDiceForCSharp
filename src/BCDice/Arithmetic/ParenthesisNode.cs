using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// カッコノード
    /// </summary>
    public sealed class ParenthesisNode : IArithmeticNode
    {
        private readonly IArithmeticNode _expr;

        /// <summary>
        /// カッコノードを作成する
        /// </summary>
        /// <param name="expr">カッコ内の式</param>
        public ParenthesisNode(IArithmeticNode expr)
        {
            _expr = expr;
        }

        /// <inheritdoc/>
        public int Eval(RoundType roundType)
        {
            return _expr.Eval(roundType);
        }

        /// <inheritdoc/>
        public string Output()
        {
            return $"({_expr.Output()})";
        }
    }
}
