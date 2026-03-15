using BCDice.CommonCommand;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class CountSuccessCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_CountSuccesses_ReturnsCorrectCount()
        {
            var randomizer = new MockRandomizer(2, 4, 5, 6, 3); // 4, 5, 6 >= 4
            var result = CountSuccessCommand.Instance.Eval("5S6>=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数3", result.Text);
        }

        [Fact]
        public void Eval_NoSuccesses_ReturnsZero()
        {
            var randomizer = new MockRandomizer(1, 2, 3); // none >= 4
            var result = CountSuccessCommand.Instance.Eval("3S6>=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数0", result.Text);
        }

        [Fact]
        public void Eval_AllSuccesses_ReturnsAll()
        {
            var randomizer = new MockRandomizer(5, 6, 4); // all >= 4
            var result = CountSuccessCommand.Instance.Eval("3S6>=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数3", result.Text);
        }

        [Fact]
        public void Eval_D10_Works()
        {
            var randomizer = new MockRandomizer(3, 7, 8, 2, 10); // 7, 8, 10 >= 6
            var result = CountSuccessCommand.Instance.Eval("5S10>=6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数3", result.Text);
        }

        [Fact]
        public void Eval_EqualComparison_Works()
        {
            var randomizer = new MockRandomizer(3, 3, 5, 3, 2); // three 3s
            var result = CountSuccessCommand.Instance.Eval("5S6=3", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数3", result.Text);
        }

        [Fact]
        public void Eval_LessThanComparison_Works()
        {
            var randomizer = new MockRandomizer(1, 2, 5, 6, 3); // 1, 2, 3 < 4
            var result = CountSuccessCommand.Instance.Eval("5S6<4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数3", result.Text);
        }

        [Fact]
        public void Eval_ShowsDiceRolls()
        {
            var randomizer = new MockRandomizer(2, 4, 6);
            var result = CountSuccessCommand.Instance.Eval("3S6>=4", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("[2,4,6]", result.Text);
        }

        [Fact]
        public void Eval_InvalidFormat_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = CountSuccessCommand.Instance.Eval("3D6", _context, randomizer);

            Assert.Null(result);
        }
    }
}
