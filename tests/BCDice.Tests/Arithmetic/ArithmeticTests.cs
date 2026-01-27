using BCDice.Arithmetic;
using BCDice.Core;
using Xunit;

namespace BCDice.Tests.Arithmetic
{
    public class ArithmeticEvaluatorTests
    {
        [Theory]
        [InlineData("1+2", 3)]
        [InlineData("10-3", 7)]
        [InlineData("4*5", 20)]
        [InlineData("20/4", 5)]
        public void Eval_BasicOperations_ReturnsCorrectResult(string expr, int expected)
        {
            var result = ArithmeticEvaluator.Eval(expr, RoundType.Floor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("1+2*3", 7)]
        [InlineData("10-2*3", 4)]
        [InlineData("(1+2)*3", 9)]
        [InlineData("10/(2+3)", 2)]
        public void Eval_Precedence_ReturnsCorrectResult(string expr, int expected)
        {
            var result = ArithmeticEvaluator.Eval(expr, RoundType.Floor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("-5", -5)]
        [InlineData("--5", 5)]
        [InlineData("-(-5)", 5)]
        [InlineData("10+-5", 5)]
        [InlineData("10--5", 15)]
        public void Eval_NegativeNumbers_ReturnsCorrectResult(string expr, int expected)
        {
            var result = ArithmeticEvaluator.Eval(expr, RoundType.Floor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("((1+2))", 3)]
        [InlineData("(((5)))", 5)]
        [InlineData("(1+(2+3))", 6)]
        public void Eval_NestedParentheses_ReturnsCorrectResult(string expr, int expected)
        {
            var result = ArithmeticEvaluator.Eval(expr, RoundType.Floor);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("7/3", RoundType.Floor, 2)]
        [InlineData("7/3", RoundType.Ceiling, 3)]
        [InlineData("7/3", RoundType.Round, 2)]
        [InlineData("8/3", RoundType.Round, 3)]
        public void Eval_DivisionRounding_RespectsRoundType(string expr, RoundType roundType, int expected)
        {
            var result = ArithmeticEvaluator.Eval(expr, roundType);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Eval_ComplexExpression_ReturnsCorrectResult()
        {
            var result = ArithmeticEvaluator.Eval("1+2*3-4/2+(5-3)*2", RoundType.Floor);
            // 1 + 6 - 2 + 4 = 9
            Assert.Equal(9, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("abc")]
        [InlineData("1+")]
        [InlineData("(1+2")]
        public void Eval_InvalidExpression_ReturnsNull(string expr)
        {
            var result = ArithmeticEvaluator.Eval(expr, RoundType.Floor);
            Assert.Null(result);
        }

        [Fact]
        public void Eval_DivisionByZero_ReturnsNull()
        {
            var result = ArithmeticEvaluator.Eval("10/0", RoundType.Floor);
            Assert.Null(result);
        }
    }
}
