using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class DracurougeTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Dracurouge", Dracurouge.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ドラクルージュ", Dracurouge.Instance.Name);
        }

        [Fact]
        public void Eval_DRCommand()
        {
            var randomizer = new MockRandomizer(3, 4, 2, 5); // 4 dice rolls
            var result = Dracurouge.Instance.Eval("DR", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_DRRCommand()
        {
            var randomizer = new MockRandomizer(2, 4, 6); // 3 dice rolls for DRR3
            var result = Dracurouge.Instance.Eval("DRR3", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_CTCommand()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll for CT3
            var result = Dracurouge.Instance.Eval("CT3", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Dracurouge.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = Dracurouge.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
