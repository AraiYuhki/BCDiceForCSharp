using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 符号反転ノード
    /// </summary>
    public sealed class NegativeNode : IArithmeticNode
    {
        private readonly IArithmeticNode _body;

        /// <summary>
        /// 符号反転ノードを作成する
        /// </summary>
        /// <param name="body">反転対象のノード</param>
        public NegativeNode(IArithmeticNode body)
        {
            _body = body;
        }

        /// <inheritdoc/>
        public int Eval(RoundType roundType)
        {
            return -_body.Eval(roundType);
        }

        /// <inheritdoc/>
        public string Output()
        {
            return $"-{_body.Output()}";
        }
    }
}
