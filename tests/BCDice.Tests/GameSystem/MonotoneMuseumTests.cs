using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class MonotoneMuseumTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("MonotoneMuseum", MonotoneMuseum.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("モノトーンミュージアム", MonotoneMuseum.Instance.Name);
        }

        [Fact]
        public void Eval_CheckRoll_Success()
        {
            var randomizer = new MockRandomizer(4, 3); // 4 + 3 = 7, +1 = 8 >= 8
            var result = MonotoneMuseum.Instance.Eval("2D6+1>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_CheckRoll_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 2 + 3 = 5, +1 = 6 < 8
            var result = MonotoneMuseum.Instance.Eval("2D6+1>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Fumble_SnakeEyes()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble on 2
            var result = MonotoneMuseum.Instance.Eval("2D6>=8[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Critical_BoxCars()
        {
            var randomizer = new MockRandomizer(6, 6); // Critical on 12
            var result = MonotoneMuseum.Instance.Eval("2D6>=8[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = MonotoneMuseum.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
