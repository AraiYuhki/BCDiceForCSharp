using System;
using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 除算ノード
    /// </summary>
    public sealed class DivideNode : IArithmeticNode
    {
        private readonly IArithmeticNode _left;
        private readonly IArithmeticNode _right;
        private readonly DivideRoundingType _roundingType;

        /// <summary>
        /// 除算ノードを作成する
        /// </summary>
        /// <param name="left">被除数</param>
        /// <param name="right">除数</param>
        /// <param name="roundingType">端数処理指定</param>
        public DivideNode(IArithmeticNode left, IArithmeticNode right, DivideRoundingType roundingType)
        {
            _left = left;
            _right = right;
            _roundingType = roundingType;
        }

        /// <inheritdoc/>
        public int Eval(RoundType gameSystemRoundType)
        {
            int dividend = _left.Eval(gameSystemRoundType);
            int divisor = _right.Eval(gameSystemRoundType);

            if (divisor == 0)
            {
                throw new DivideByZeroException();
            }

            return DivideAndRound(dividend, divisor, gameSystemRoundType);
        }

        /// <inheritdoc/>
        public string Output()
        {
            string suffix = _roundingType switch
            {
                DivideRoundingType.Ceiling => "C",
                DivideRoundingType.Round => "R",
                DivideRoundingType.Floor => "F",
                _ => ""
            };

            return $"{_left.Output()}/{_right.Output()}{suffix}";
        }

        private int DivideAndRound(int dividend, int divisor, RoundType gameSystemRoundType)
        {
            // 実際に使用する端数処理を決定
            RoundType effectiveRoundType = _roundingType switch
            {
                DivideRoundingType.Ceiling => RoundType.Ceiling,
                DivideRoundingType.Round => RoundType.Round,
                DivideRoundingType.Floor => RoundType.Floor,
                _ => gameSystemRoundType // GameSystemDefault
            };

            double result = (double)dividend / divisor;

            return effectiveRoundType switch
            {
                RoundType.Ceiling => (int)Math.Ceiling(result),
                RoundType.Round => (int)Math.Round(result, MidpointRounding.AwayFromZero),
                _ => dividend / divisor // Floor（整数除算）
            };
        }
    }
}
