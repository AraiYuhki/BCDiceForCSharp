using BCDice.CommonCommand;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class LowerDiceCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_2R6_ReturnsMinimum()
        {
            var randomizer = new MockRandomizer(3, 5); // min is 3
            var result = LowerDiceCommand.Instance.Eval("2R6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("[3,5]", result.Text);
            Assert.Contains(" ＞ 3", result.Text);
        }

        [Fact]
        public void Eval_3R6_ReturnsMinimum()
        {
            var randomizer = new MockRandomizer(4, 1, 6); // min is 1
            var result = LowerDiceCommand.Instance.Eval("3R6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("[4,1,6]", result.Text);
            Assert.Contains(" ＞ 1", result.Text);
        }

        [Fact]
        public void Eval_2L6_ReturnsMinimum()
        {
            var randomizer = new MockRandomizer(5, 2); // min is 2
            var result = LowerDiceCommand.Instance.Eval("2L6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains(" ＞ 2", result.Text);
        }

        [Fact]
        public void Eval_2R6WithComparisonSuccess_ReturnsSuccess()
        {
            var randomizer = new MockRandomizer(3, 5); // min 3 <= 4
            var result = LowerDiceCommand.Instance.Eval("2R6<=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_2R6WithComparisonFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(4, 5); // min 4 <= 2
            var result = LowerDiceCommand.Instance.Eval("2R6<=2", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Secret2R6_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = LowerDiceCommand.Instance.Eval("S2R6", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = LowerDiceCommand.Instance.Eval("2D6", _context, randomizer);

            Assert.Null(result);
        }
    }
}
