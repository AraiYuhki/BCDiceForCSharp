using BCDice.CommonCommand;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class RepeatCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_X2_2D6_RepeatsCommand()
        {
            var randomizer = new MockRandomizer(3, 4, 5, 6); // First: 3+4=7, Second: 5+6=11
            var result = RepeatCommand.Instance.Eval("x2 2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("(2回)", result.Text);
            Assert.Contains("7", result.Text);
            Assert.Contains("11", result.Text);
        }

        [Fact]
        public void Eval_3Hash_2D6_RepeatsCommand()
        {
            var randomizer = new MockRandomizer(1, 2, 3, 4, 5, 6); // 3, 7, 11
            var result = RepeatCommand.Instance.Eval("3#2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("(3回)", result.Text);
        }

        [Fact]
        public void Eval_Rep2_1D6_RepeatsCommand()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = RepeatCommand.Instance.Eval("rep2 1D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("(2回)", result.Text);
        }

        [Fact]
        public void Eval_X1_2D6_SingleRepeat()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = RepeatCommand.Instance.Eval("x1 2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("(1回)", result.Text);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_X0_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = RepeatCommand.Instance.Eval("x0 2D6", _context, randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_TooManyRepeats_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = RepeatCommand.Instance.Eval("x101 2D6", _context, randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_SecretRepeat_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 4, 5, 6);
            var result = RepeatCommand.Instance.Eval("Sx2 2D6", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_EmptyInnerCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = RepeatCommand.Instance.Eval("x2 ", _context, randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_InvalidInnerCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = RepeatCommand.Instance.Eval("x2 abc", _context, randomizer);

            Assert.Null(result);
        }
    }
}
