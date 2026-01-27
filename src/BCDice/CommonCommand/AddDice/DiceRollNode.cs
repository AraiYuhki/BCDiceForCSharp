using System.Collections.Generic;
using System.Text;
using BCDice.Core;
using BCDice.Preprocessor;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// ダイスロールノード
    /// </summary>
    public sealed class DiceRollNode : IAddDiceNode
    {
        private readonly IAddDiceNode _times;
        private readonly IAddDiceNode? _sides;
        private string _output = string.Empty;

        /// <summary>
        /// ダイスロールノードを作成する
        /// </summary>
        /// <param name="times">ダイス個数</param>
        /// <param name="sides">面数（nullの場合は暗黙の面数を使用）</param>
        public DiceRollNode(IAddDiceNode times, IAddDiceNode? sides = null)
        {
            _times = times;
            _sides = sides;
        }

        /// <inheritdoc/>
        public int Eval(IGameSystemContext context, IRandomizer? randomizer)
        {
            if (randomizer == null)
            {
                return 0;
            }

            int times = _times.Eval(context, null);
            int sides = _sides?.Eval(context, null) ?? context.SidesImplicitD;

            int[] diceList = randomizer.RollBarabara(times, sides);

            int total = 0;
            var sb = new StringBuilder();
            sb.Append('{');

            for (int i = 0; i < diceList.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(diceList[i]);
                total += diceList[i];
            }

            sb.Append('}');
            _output = $"{total}[{sb}]";

            return total;
        }

        /// <inheritdoc/>
        public bool IncludesDice => true;

        /// <inheritdoc/>
        public string Expr(IGameSystemContext context)
        {
            int times = _times.Eval(context, null);
            int sides = _sides?.Eval(context, null) ?? context.SidesImplicitD;
            return $"{times}D{sides}";
        }

        /// <inheritdoc/>
        public string Output => _output;
    }
}
