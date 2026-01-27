using BCDice.Command;

namespace BCDice.Arithmetic
{
    /// <summary>
    /// 算術式パーサ
    /// 再帰下降パーサによる実装
    /// </summary>
    /// <remarks>
    /// 文法:
    /// add: add ('+' | '-') mul | mul
    /// mul: mul '*' unary | mul '/' unary round_type | unary
    /// round_type: ε | 'U' | 'C' | 'R' | 'F'
    /// unary: '+' unary | '-' unary | term
    /// term: '(' add ')' | NUMBER
    /// </remarks>
    public class ArithmeticParser
    {
        private Lexer _lexer = null!;
        private Token _currentToken;

        /// <summary>
        /// 算術式をパースする
        /// </summary>
        /// <param name="source">入力文字列</param>
        /// <returns>ASTのルートノード、パース失敗時はnull</returns>
        public static IArithmeticNode? Parse(string source)
        {
            return new ArithmeticParser().ParseInternal(source);
        }

        private IArithmeticNode? ParseInternal(string source)
        {
            _lexer = new Lexer(source);
            _currentToken = _lexer.NextToken();

            try
            {
                var node = ParseAdd();

                // 式全体が消費されていない場合はエラー
                if (_currentToken.Type != TokenType.Eof)
                {
                    return null;
                }

                return node;
            }
            catch
            {
                return null;
            }
        }

        private IArithmeticNode ParseAdd()
        {
            var left = ParseMul();

            while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
            {
                var op = _currentToken.Type == TokenType.Plus
                    ? BinaryOperator.Add
                    : BinaryOperator.Subtract;

                Advance();
                var right = ParseMul();
                left = new BinaryOpNode(left, op, right);
            }

            return left;
        }

        private IArithmeticNode ParseMul()
        {
            var left = ParseUnary();

            while (_currentToken.Type == TokenType.Asterisk || _currentToken.Type == TokenType.Slash)
            {
                if (_currentToken.Type == TokenType.Asterisk)
                {
                    Advance();
                    var right = ParseUnary();
                    left = new BinaryOpNode(left, BinaryOperator.Multiply, right);
                }
                else // Slash
                {
                    Advance();
                    var right = ParseUnary();
                    var roundingType = ParseRoundType();
                    left = new DivideNode(left, right, roundingType);
                }
            }

            return left;
        }

        private DivideRoundingType ParseRoundType()
        {
            if (_currentToken.Type == TokenType.Identifier)
            {
                string value = _currentToken.Value.ToUpperInvariant();

                switch (value)
                {
                    case "U":
                    case "C":
                        Advance();
                        return DivideRoundingType.Ceiling;

                    case "R":
                        Advance();
                        return DivideRoundingType.Round;

                    case "F":
                        Advance();
                        return DivideRoundingType.Floor;
                }
            }

            return DivideRoundingType.GameSystemDefault;
        }

        private IArithmeticNode ParseUnary()
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
                return new NegativeNode(body);
            }

            return ParseTerm();
        }

        private IArithmeticNode ParseTerm()
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
                var number = _currentToken.NumberValue;
                Advance();
                return new NumberNode(number);
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
}
