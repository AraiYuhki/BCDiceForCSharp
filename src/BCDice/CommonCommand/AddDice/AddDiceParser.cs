using BCDice.Command;
using BCDice.Core;

namespace BCDice.CommonCommand.AddDice
{
    /// <summary>
    /// 加算ダイスコマンドのパーサ
    /// </summary>
    public class AddDiceParser
    {
        private Lexer _lexer = null!;
        private Token _currentToken;

        /// <summary>
        /// コマンドをパースする
        /// </summary>
        /// <param name="source">入力文字列</param>
        /// <returns>パース結果、失敗時はnull</returns>
        public static AddDiceParsed? Parse(string source)
        {
            return new AddDiceParser().ParseInternal(source);
        }

        private AddDiceParsed? ParseInternal(string source)
        {
            _lexer = new Lexer(source);
            _currentToken = _lexer.NextToken();

            try
            {
                bool isSecret = ParseSecret();
                var lhs = ParseAdd();

                if (!lhs.IncludesDice)
                {
                    return null;
                }

                // 比較演算子があるか
                if (_currentToken.Type == TokenType.CompareOp)
                {
                    var cmpOp = _currentToken.CompareOperator!.Value;
                    Advance();

                    var rhs = ParseTarget();
                    if (rhs == null || rhs.IncludesDice)
                    {
                        return null;
                    }

                    return new AddDiceParsed(isSecret, lhs, cmpOp, rhs);
                }

                return new AddDiceParsed(isSecret, lhs, null, null);
            }
            catch
            {
                return null;
            }
        }

        private bool ParseSecret()
        {
            if (_currentToken.Type == TokenType.Identifier && _currentToken.Value == "S")
            {
                Advance();
                return true;
            }
            return false;
        }

        private IAddDiceNode? ParseTarget()
        {
            // ? は未定の目標値
            if (_currentToken.Type == TokenType.Question)
            {
                Advance();
                return new NumberNode(-1); // 特殊な値として-1を使用（後で処理）
            }

            return ParseAdd();
        }

        private IAddDiceNode ParseAdd()
        {
            var left = ParseMul();

            while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
            {
                char op = _currentToken.Type == TokenType.Plus ? '+' : '-';
                Advance();

                var right = ParseMul();

                // 右辺が負数の場合、演算子を反転
                if (right is NegateNode negateNode)
                {
                    op = op == '+' ? '-' : '+';
                    right = negateNode.Body;
                }

                left = new BinaryOpNode(left, op, right);
            }

            return left;
        }

        private IAddDiceNode ParseMul()
        {
            var left = ParseUnary();

            while (_currentToken.Type == TokenType.Asterisk || _currentToken.Type == TokenType.Slash)
            {
                char op = _currentToken.Type == TokenType.Asterisk ? '*' : '/';
                Advance();

                var right = ParseUnary();

                // 除算の端数処理指定をスキップ（U, C, R, F）
                if (op == '/' && _currentToken.Type == TokenType.Identifier)
                {
                    string v = _currentToken.Value.ToUpperInvariant();
                    if (v == "U" || v == "C" || v == "R" || v == "F")
                    {
                        Advance();
                    }
                }

                left = new BinaryOpNode(left, op, right);
            }

            return left;
        }

        private IAddDiceNode ParseUnary()
        {
            if (_currentToken.Type == TokenType.Plus)
            {
                Advance();
                return ParseUnary();
            }

            if (_currentToken.Type == TokenType.Minus)
            {
                Advance();
                var body = ParseUnary();

                // 二重否定を解消
                if (body is NegateNode negateNode)
                {
                    return negateNode.Body;
                }

                return new NegateNode(body);
            }

            return ParseDice();
        }

        private IAddDiceNode ParseDice()
        {
            // D6 のような形式（個数省略）
            if (_currentToken.Type == TokenType.Identifier && _currentToken.Value == "D")
            {
                Advance();

                if (_currentToken.Type == TokenType.Number)
                {
                    var sides = new NumberNode(_currentToken.NumberValue);
                    Advance();

                    // フィルタをスキップ（KH, KL, DH, DL, MAX, MIN）
                    SkipFilter();

                    return new DiceRollNode(new NumberNode(1), sides);
                }

                // 暗黙の面数
                return new DiceRollNode(new NumberNode(1), null);
            }

            var term = ParseTerm();

            // nD または nDm の形式
            if (_currentToken.Type == TokenType.Identifier && _currentToken.Value == "D")
            {
                Advance();

                IAddDiceNode? sides = null;

                if (_currentToken.Type == TokenType.Number)
                {
                    sides = new NumberNode(_currentToken.NumberValue);
                    Advance();
                }

                // フィルタをスキップ
                SkipFilter();

                return new DiceRollNode(term, sides);
            }

            return term;
        }

        private void SkipFilter()
        {
            // KH, KL, DH, DL, MAX, MIN などのフィルタをスキップ
            if (_currentToken.Type == TokenType.Identifier)
            {
                string v = _currentToken.Value.ToUpperInvariant();
                if (v == "K" || v == "D" || v == "M")
                {
                    Advance();
                    // 続く H, L, A, I, N などもスキップ
                    while (_currentToken.Type == TokenType.Identifier)
                    {
                        string v2 = _currentToken.Value.ToUpperInvariant();
                        if (v2 == "H" || v2 == "L" || v2 == "A" || v2 == "X" || v2 == "I" || v2 == "N")
                        {
                            Advance();
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 数値もスキップ
                    if (_currentToken.Type == TokenType.Number)
                    {
                        Advance();
                    }
                }
            }
        }

        private IAddDiceNode ParseTerm()
        {
            if (_currentToken.Type == TokenType.ParenLeft)
            {
                Advance();
                var expr = ParseAdd();

                if (_currentToken.Type != TokenType.ParenRight)
                {
                    throw new ParseException("Expected ')'");
                }
                Advance();

                return new ParenthesisNode(expr);
            }

            if (_currentToken.Type == TokenType.Number)
            {
                var number = new NumberNode(_currentToken.NumberValue);
                Advance();
                return number;
            }

            throw new ParseException($"Unexpected token: {_currentToken}");
        }

        private void Advance()
        {
            _currentToken = _lexer.NextToken();
        }

        private class ParseException : System.Exception
        {
            public ParseException(string message) : base(message) { }
        }
    }

    /// <summary>
    /// 加算ダイスのパース結果
    /// </summary>
    public class AddDiceParsed
    {
        /// <summary>シークレットダイスか</summary>
        public bool IsSecret { get; }

        /// <summary>左辺（ダイス式）</summary>
        public IAddDiceNode Left { get; }

        /// <summary>比較演算子</summary>
        public CompareOperator? CompareOp { get; }

        /// <summary>右辺（目標値）</summary>
        public IAddDiceNode? Right { get; }

        /// <summary>
        /// パース結果を作成する
        /// </summary>
        public AddDiceParsed(bool isSecret, IAddDiceNode left, CompareOperator? compareOp, IAddDiceNode? right)
        {
            IsSecret = isSecret;
            Left = left;
            CompareOp = compareOp;
            Right = right;
        }
    }
}
