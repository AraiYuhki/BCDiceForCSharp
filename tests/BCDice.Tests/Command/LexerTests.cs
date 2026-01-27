using BCDice.Command;
using BCDice.Core;
using Xunit;

namespace BCDice.Tests.Command
{
    public class LexerTests
    {
        [Theory]
        [InlineData("123", TokenType.Number, 123)]
        [InlineData("0", TokenType.Number, 0)]
        [InlineData("999999", TokenType.Number, 999999)]
        public void NextToken_Number_ReturnsNumberToken(string input, TokenType expectedType, int expectedValue)
        {
            var lexer = new Lexer(input);
            var token = lexer.NextToken();

            Assert.Equal(expectedType, token.Type);
            Assert.Equal(expectedValue, token.NumberValue);
        }

        [Theory]
        [InlineData("+", TokenType.Plus)]
        [InlineData("-", TokenType.Minus)]
        [InlineData("*", TokenType.Asterisk)]
        [InlineData("/", TokenType.Slash)]
        [InlineData("(", TokenType.ParenLeft)]
        [InlineData(")", TokenType.ParenRight)]
        [InlineData("?", TokenType.Question)]
        public void NextToken_Operator_ReturnsOperatorToken(string input, TokenType expectedType)
        {
            var lexer = new Lexer(input);
            var token = lexer.NextToken();

            Assert.Equal(expectedType, token.Type);
        }

        [Theory]
        [InlineData(">=", CompareOperator.GreaterThanOrEqual)]
        [InlineData("<=", CompareOperator.LessThanOrEqual)]
        [InlineData("<>", CompareOperator.NotEqual)]
        [InlineData(">", CompareOperator.GreaterThan)]
        [InlineData("<", CompareOperator.LessThan)]
        [InlineData("=", CompareOperator.Equal)]
        public void NextToken_CompareOp_ReturnsCompareOpToken(string input, CompareOperator expectedOp)
        {
            var lexer = new Lexer(input);
            var token = lexer.NextToken();

            Assert.Equal(TokenType.CompareOp, token.Type);
            Assert.Equal(expectedOp, token.CompareOperator);
        }

        [Theory]
        [InlineData("D", "D")]
        [InlineData("S", "S")]
        [InlineData("ABC", "A")]
        public void NextToken_Identifier_ReturnsSingleCharIdentifier(string input, string expectedValue)
        {
            var lexer = new Lexer(input);
            var token = lexer.NextToken();

            Assert.Equal(TokenType.Identifier, token.Type);
            Assert.Equal(expectedValue, token.Value);
        }

        [Fact]
        public void NextToken_EmptyInput_ReturnsEndToken()
        {
            var lexer = new Lexer("");
            var token = lexer.NextToken();

            Assert.Equal(TokenType.Eof, token.Type);
        }

        [Fact]
        public void NextToken_MultipleTokens_ParsesCorrectly()
        {
            var lexer = new Lexer("2D6+3");

            var t1 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t1.Type);
            Assert.Equal(2, t1.NumberValue);

            var t2 = lexer.NextToken();
            Assert.Equal(TokenType.Identifier, t2.Type);
            Assert.Equal("D", t2.Value);

            var t3 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t3.Type);
            Assert.Equal(6, t3.NumberValue);

            var t4 = lexer.NextToken();
            Assert.Equal(TokenType.Plus, t4.Type);

            var t5 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t5.Type);
            Assert.Equal(3, t5.NumberValue);

            var t6 = lexer.NextToken();
            Assert.Equal(TokenType.Eof, t6.Type);
        }

        [Fact]
        public void NextToken_DiceExpressionWithComparison_ParsesCorrectly()
        {
            var lexer = new Lexer("2D6>=7");

            var t1 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t1.Type);

            var t2 = lexer.NextToken();
            Assert.Equal(TokenType.Identifier, t2.Type);

            var t3 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t3.Type);

            var t4 = lexer.NextToken();
            Assert.Equal(TokenType.CompareOp, t4.Type);
            Assert.Equal(CompareOperator.GreaterThanOrEqual, t4.CompareOperator);

            var t5 = lexer.NextToken();
            Assert.Equal(TokenType.Number, t5.Type);
            Assert.Equal(7, t5.NumberValue);
        }
    }
}
