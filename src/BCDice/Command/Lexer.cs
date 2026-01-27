using System;

namespace BCDice.Command
{
    /// <summary>
    /// ダイスコマンドの字句解析器
    /// </summary>
    public class Lexer
    {
        private readonly string _source;
        private int _position;

        /// <summary>
        /// 元の入力文字列
        /// </summary>
        public string Source => _source;

        /// <summary>
        /// 字句解析器を初期化する
        /// </summary>
        /// <param name="source">入力文字列</param>
        public Lexer(string source)
        {
            // 空白より前の部分だけを取る
            if (string.IsNullOrEmpty(source))
            {
                _source = string.Empty;
            }
            else
            {
                int spaceIndex = source.IndexOf(' ');
                _source = spaceIndex >= 0 ? source.Substring(0, spaceIndex) : source;
            }

            _position = 0;
        }

        /// <summary>
        /// 入力終端に達しているか
        /// </summary>
        public bool IsEof => _position >= _source.Length;

        /// <summary>
        /// 次のトークンを取得する
        /// </summary>
        /// <returns>トークン</returns>
        public Token NextToken()
        {
            if (IsEof)
            {
                return Token.Eof;
            }

            // 数値
            if (char.IsDigit(_source[_position]))
            {
                return ScanNumber();
            }

            // 比較演算子
            if (IsCompareOpStart(_source[_position]))
            {
                return ScanCompareOp();
            }

            // 単一文字トークン
            char c = _source[_position];
            _position++;

            return c switch
            {
                '+' => new Token(TokenType.Plus, "+"),
                '-' => new Token(TokenType.Minus, "-"),
                '*' => new Token(TokenType.Asterisk, "*"),
                '/' => new Token(TokenType.Slash, "/"),
                '(' => new Token(TokenType.ParenLeft, "("),
                ')' => new Token(TokenType.ParenRight, ")"),
                '[' => new Token(TokenType.BracketLeft, "["),
                ']' => new Token(TokenType.BracketRight, "]"),
                '?' => new Token(TokenType.Question, "?"),
                '@' => new Token(TokenType.At, "@"),
                '#' => new Token(TokenType.Sharp, "#"),
                ',' => new Token(TokenType.Comma, ","),
                _ => new Token(TokenType.Identifier, char.ToUpperInvariant(c).ToString())
            };
        }

        /// <summary>
        /// 次のトークンを先読みする（位置は進めない）
        /// </summary>
        /// <returns>トークン</returns>
        public Token Peek()
        {
            int savedPosition = _position;
            Token token = NextToken();
            _position = savedPosition;
            return token;
        }

        private Token ScanNumber()
        {
            int start = _position;

            while (_position < _source.Length && char.IsDigit(_source[_position]))
            {
                _position++;
            }

            string numberStr = _source.Substring(start, _position - start);
            int number = int.Parse(numberStr);

            return new Token(number);
        }

        private static bool IsCompareOpStart(char c)
        {
            return c == '<' || c == '>' || c == '=' || c == '!';
        }

        private Token ScanCompareOp()
        {
            int start = _position;

            while (_position < _source.Length && IsCompareOpStart(_source[_position]))
            {
                _position++;
            }

            string opStr = _source.Substring(start, _position - start);
            var compareOp = Normalize.ComparisonOperator(opStr);

            if (compareOp.HasValue)
            {
                return new Token(opStr, compareOp.Value);
            }

            // 無効な比較演算子の場合は識別子として扱う
            return new Token(TokenType.Identifier, opStr);
        }
    }
}
