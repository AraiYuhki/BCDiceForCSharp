using BCDice.CommonCommand;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class UpperDiceCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_2B6_ReturnsMaximum()
        {
            var randomizer = new MockRandomizer(3, 5); // max is 5
            var result = UpperDiceCommand.Instance.Eval("2B6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("5", result.Text);
            Assert.Contains("[3,5]", result.Text);
        }

        [Fact]
        public void Eval_3B6_ReturnsMaximum()
        {
            var randomizer = new MockRandomizer(2, 6, 4); // max is 6
            var result = UpperDiceCommand.Instance.Eval("3B6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("[2,6,4]", result.Text);
            Assert.Contains(" ＞ 6", result.Text);
        }

        [Fact]
        public void Eval_2U6_ReturnsMaximum()
        {
            var randomizer = new MockRandomizer(1, 4); // max is 4
            var result = UpperDiceCommand.Instance.Eval("2U6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains(" ＞ 4", result.Text);
        }

        [Fact]
        public void Eval_2B6WithComparisonSuccess_ReturnsSuccess()
        {
            var randomizer = new MockRandomizer(3, 5); // max 5 >= 4
            var result = UpperDiceCommand.Instance.Eval("2B6>=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_2B6WithComparisonFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(2, 3); // max 3 >= 5
            var result = UpperDiceCommand.Instance.Eval("2B6>=5", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Secret2B6_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = UpperDiceCommand.Instance.Eval("S2B6", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = UpperDiceCommand.Instance.Eval("2D6", _context, randomizer);

            Assert.Null(result);
        }
    }
}
