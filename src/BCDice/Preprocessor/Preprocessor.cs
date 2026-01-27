using System;
using System.Text.RegularExpressions;
using BCDice.Arithmetic;

namespace BCDice.Preprocessor
{
    /// <summary>
    /// 入力文字列に対して前処理を行う
    /// </summary>
    /// <example>
    /// Preprocessor.Process("1d6+4D+(3*4) 切り取られる部分", gameSystem)
    /// // => "1d6+4D6+12"
    /// </example>
    public class Preprocessor
    {
        private static readonly Regex ParenthesesPattern = new Regex(
            @"\([\d/+*\-CURF]+\)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ImplicitDPattern = new Regex(
            @"(\d+)D([^\w]|$)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string _text;
        private readonly IGameSystemContext _gameSystem;

        /// <summary>
        /// 入力文字列を前処理する
        /// </summary>
        /// <param name="text">入力文字列</param>
        /// <param name="gameSystem">ゲームシステムコンテキスト</param>
        /// <returns>前処理後の文字列</returns>
        public static string Process(string text, IGameSystemContext gameSystem)
        {
            return new Preprocessor(text, gameSystem).Process();
        }

        /// <summary>
        /// デフォルトのゲームシステムで前処理する
        /// </summary>
        /// <param name="text">入力文字列</param>
        /// <returns>前処理後の文字列</returns>
        public static string Process(string text)
        {
            return Process(text, DefaultGameSystemContext.Instance);
        }

        private Preprocessor(string text, IGameSystemContext gameSystem)
        {
            _text = text ?? string.Empty;
            _gameSystem = gameSystem;
        }

        private string Process()
        {
            TrimAfterWhitespace();
            ReplaceParentheses();

            _text = _gameSystem.ChangeText(_text);

            ReplaceImplicitD();

            return _text;
        }

        /// <summary>
        /// 空白より前だけを取る
        /// </summary>
        private void TrimAfterWhitespace()
        {
            _text = _text.Trim();
            int spaceIndex = _text.IndexOf(' ');
            if (spaceIndex >= 0)
            {
                _text = _text.Substring(0, spaceIndex);
            }
        }

        /// <summary>
        /// カッコ書きの数式を事前計算する
        /// </summary>
        private void ReplaceParentheses()
        {
            string prev;
            do
            {
                prev = _text;
                _text = ParenthesesPattern.Replace(_text, match =>
                {
                    var result = ArithmeticEvaluator.Eval(match.Value, _gameSystem.RoundType);
                    return result.HasValue ? result.Value.ToString() : match.Value;
                });
            }
            while (prev != _text);
        }

        /// <summary>
        /// nDをゲームシステムに応じて置き換える
        /// </summary>
        private void ReplaceImplicitD()
        {
            _text = ImplicitDPattern.Replace(_text, match =>
            {
                string times = match.Groups[1].Value;
                int sides = _gameSystem.SidesImplicitD;
                string trailer = match.Groups[2].Value;

                return $"{times}D{sides}{trailer}";
            });
        }
    }
}
