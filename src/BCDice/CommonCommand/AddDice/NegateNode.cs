using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 符号反転ノード
    /// </summary>
    public sealed class NegateNode : IAddDiceNode
    {
        private readonly IAddDiceNode _body;

        /// <summary>
        /// 内部のノード
        /// </summary>
        public IAddDiceNode Body => _body;

        /// <summary>
        /// 符号反転ノードを作成する
        /// </summary>
        public NegateNode(IAddDiceNode body)
        {
            _body = body;
        }

        /// <inheritdoc/>
        public int Eval(IGameSystemContext context, IRandomizer? randomizer)
        {
            return -_body.Eval(context, randomizer);
        }

        /// <inheritdoc/>
        public bool IncludesDice => _body.IncludesDice;

        /// <inheritdoc/>
        public string Expr(IGameSystemContext context)
        {
            return $"-{_body.Expr(context)}";
        }

        /// <inheritdoc/>
        public string Output => $"-{_body.Output}";
    }
}
