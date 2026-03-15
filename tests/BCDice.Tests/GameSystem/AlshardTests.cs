using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class AlshardTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Alshard", Alshard.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("アルシャード", Alshard.Instance.Name);
        }

        [Fact]
        public void Eval_ALAlias_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 4 + 5 = 9, +2 = 11 >= 10
            var result = Alshard.Instance.Eval("AL+2>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_ALAlias_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 2 + 3 = 5, +2 = 7 < 10
            var result = Alshard.Instance.Eval("AL+2>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Fumble_SnakeEyes()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble
            var result = Alshard.Instance.Eval("AL+2>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("自動失敗", result.Text);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_Critical_BoxCars()
        {
            var randomizer = new MockRandomizer(6, 6); // Critical
            var result = Alshard.Instance.Eval("AL+2>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("自動成功", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Alshard.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
