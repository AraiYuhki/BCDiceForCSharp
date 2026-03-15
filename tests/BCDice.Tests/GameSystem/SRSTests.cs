using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class SRSTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("SRS", SRS.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("スタンダード", SRS.Instance.Name);
        }

        [Fact]
        public void Eval_SRSRollWithTarget_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 4 + 5 = 9, +2 = 11 >= 10
            var result = SRS.Instance.Eval("2D6+2>=10[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_SRSRollWithTarget_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 2 + 3 = 5, +2 = 7 < 10
            var result = SRS.Instance.Eval("2D6+2>=10[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Fumble_SnakeEyes()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble on 2
            var result = SRS.Instance.Eval("2D6+2>=10[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.Contains("自動失敗", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Critical_BoxCars()
        {
            var randomizer = new MockRandomizer(6, 6); // Critical on 12
            var result = SRS.Instance.Eval("2D6+2>=10[12,2]", randomizer);

            Assert.NotNull(result);
            Assert.Contains("自動成功", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = SRS.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
