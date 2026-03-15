using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// カッコノード
    /// </summary>
    public sealed class ParenthesisNode : IAddDiceNode
    {
        private readonly IAddDiceNode _expr;

        /// <summary>
        /// カッコノードを作成する
        /// </summary>
        public ParenthesisNode(IAddDiceNode expr)
        {
            _expr = expr;
        }

        /// <inheritdoc/>
        public int Eval(IGameSystemContext context, IRandomizer? randomizer)
        {
            return _expr.Eval(context, randomizer);
        }

        /// <inheritdoc/>
        public bool IncludesDice => _expr.IncludesDice;

        /// <inheritdoc/>
        public string Expr(IGameSystemContext context)
        {
            return $"({_expr.Expr(context)})";
        }

        /// <inheritdoc/>
        public string Output => $"({_expr.Output})";
    }
}
