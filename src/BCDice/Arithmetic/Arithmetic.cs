using System;
using BCDice.Core;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 算術式の評価を行う静的クラス
    /// </summary>
    public static class ArithmeticEvaluator
    {
        /// <summary>
        /// 算術式を評価する
        /// </summary>
        /// <param name="source">算術式の文字列</param>
        /// <param name="roundType">端数処理タイプ</param>
        /// <returns>評価結果、パースエラーやゼロ除算の場合はnull</returns>
        public static int? Eval(string source, RoundType roundType)
        {
            try
            {
                var node = ArithmeticParser.Parse(source);
                return node?.Eval(roundType);
            }
            catch (DivideByZeroException)
            {
                return null;
            }
        }
    }
}
