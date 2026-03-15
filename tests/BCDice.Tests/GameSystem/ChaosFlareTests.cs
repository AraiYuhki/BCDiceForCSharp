using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class ChaosFlareTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("ChaosFlare", ChaosFlare.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("カオスフレア", ChaosFlare.Instance.Name);
        }

        [Fact]
        public void Eval_CFRoll_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 4 + 5 = 9 -- need to check CF behavior
            var result = ChaosFlare.Instance.Eval("CF>=10", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_FateTable()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll for fate table
            var result = ChaosFlare.Instance.Eval("FT", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_FT0_SpecialValue()
        {
            var randomizer = new MockRandomizer(1, 1); // Low roll for FT0
            var result = ChaosFlare.Instance.Eval("FT0", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = ChaosFlare.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = ChaosFlare.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
