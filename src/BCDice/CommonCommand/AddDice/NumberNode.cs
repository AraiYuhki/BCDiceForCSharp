using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 数値ノード
    /// </summary>
    public sealed class NumberNode : IAddDiceNode
    {
        /// <summary>
        /// 数値の値
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// 数値ノードを作成する
        /// </summary>
        public NumberNode(int value)
        {
            Value = value;
        }

        /// <inheritdoc/>
        public int Eval(IGameSystemContext context, IRandomizer? randomizer)
        {
            return Value;
        }

        /// <inheritdoc/>
        public bool IncludesDice => false;

        /// <inheritdoc/>
        public string Expr(IGameSystemContext context)
        {
            return Value.ToString();
        }

        /// <inheritdoc/>
        public string Output => Value.ToString();
    }
}
