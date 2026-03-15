using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class TokyoNovaTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("TokyoNova", TokyoNova.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("トーキョーN◎VA", TokyoNova.Instance.Name);
        }

        [Fact]
        public void Eval_CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4); // 3 + 4 = 7
            var result = TokyoNova.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_CommonCommand_1D10_Works()
        {
            var randomizer = new MockRandomizer(7);
            var result = TokyoNova.Instance.Eval("1D10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = TokyoNova.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
