using BCDice.CommonCommand.AddDice;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class AddDiceCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_SimpleDiceRoll_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 2D6 = 3 + 4 = 7
            var result = AddDiceCommand.Instance.Eval("2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
            Assert.Equal(2, result.Rands.Count);
        }

        [Fact]
        public void Eval_DiceRollWithAddition_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 2D6 = 7, +3 = 10
            var result = AddDiceCommand.Instance.Eval("2D6+3", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("10", result.Text);
        }

        [Fact]
        public void Eval_DiceRollWithSubtraction_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(5, 6); // 2D6 = 11, -3 = 8
            var result = AddDiceCommand.Instance.Eval("2D6-3", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("8", result.Text);
        }

        [Fact]
        public void Eval_DiceRollWithMultiplication_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 2D6 = 7, *2 = 14
            var result = AddDiceCommand.Instance.Eval("2D6*2", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("14", result.Text);
        }

        [Fact]
        public void Eval_DiceRollWithComparisonSuccess_ReturnsSuccess()
        {
            var randomizer = new MockRandomizer(4, 5); // 2D6 = 9 >= 7
            var result = AddDiceCommand.Instance.Eval("2D6>=7", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_DiceRollWithComparisonFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(2, 3); // 2D6 = 5 >= 7
            var result = AddDiceCommand.Instance.Eval("2D6>=7", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_SecretDiceRoll_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = AddDiceCommand.Instance.Eval("S2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_SingleDice_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(5);
            var result = AddDiceCommand.Instance.Eval("1D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("5", result.Text);
        }

        [Fact]
        public void Eval_ImplicitDiceCount_UsesSidesFromContext()
        {
            // D6 without count prefix means 1D6
            var randomizer = new MockRandomizer(4);
            var result = AddDiceCommand.Instance.Eval("D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("4", result.Text);
            Assert.Single(result.Rands!);
        }

        [Fact]
        public void Eval_NoDice_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = AddDiceCommand.Instance.Eval("5+3", _context, randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = AddDiceCommand.Instance.Eval("abc", _context, randomizer);

            Assert.Null(result);
        }

        [Theory]
        [InlineData("2D6=7", 3, 4, true)]   // 7 = 7
        [InlineData("2D6=7", 3, 5, false)]  // 8 != 7
        [InlineData("2D6<>7", 3, 5, true)]  // 8 <> 7
        [InlineData("2D6<>7", 3, 4, false)] // 7 <> 7
        [InlineData("2D6>7", 4, 4, true)]   // 8 > 7
        [InlineData("2D6>7", 3, 4, false)]  // 7 > 7
        [InlineData("2D6<7", 2, 3, true)]   // 5 < 7
        [InlineData("2D6<7", 3, 4, false)]  // 7 < 7
        [InlineData("2D6<=7", 3, 4, true)]  // 7 <= 7
        [InlineData("2D6<=7", 4, 4, false)] // 8 <= 7
        public void Eval_ComparisonOperators_WorkCorrectly(string command, int dice1, int dice2, bool expectedSuccess)
        {
            var randomizer = new MockRandomizer(dice1, dice2);
            var result = AddDiceCommand.Instance.Eval(command, _context, randomizer);

            Assert.NotNull(result);
            Assert.Equal(expectedSuccess, result.IsSuccess);
        }

        [Fact]
        public void Eval_ComplexExpression_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(3, 4, 2); // 2D6 = 7, 1D6 = 2, total = 9
            var result = AddDiceCommand.Instance.Eval("2D6+1D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("9", result.Text);
            Assert.Equal(3, result.Rands.Count);
        }

        [Fact]
        public void Eval_ParenthesizedExpression_ReturnsCorrectResult()
        {
            var randomizer = new MockRandomizer(3); // (1+2)D6 = 3D6, but parser treats differently
            var result = AddDiceCommand.Instance.Eval("(1D6)", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("3", result.Text);
        }
    }
}
