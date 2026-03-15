using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class LogHorizonTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("LogHorizon", LogHorizon.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ログ・ホライズン", LogHorizon.Instance.Name);
        }

        [Fact]
        public void Eval_LHCommand_Success()
        {
            var randomizer = new MockRandomizer(4, 3, 5); // 3LH: 4+3+5 = 12 >= 10
            var result = LogHorizon.Instance.Eval("3LH>=10", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_LHCommand_Critical()
        {
            var randomizer = new MockRandomizer(6, 6, 3); // Critical: two 6s
            var result = LogHorizon.Instance.Eval("3LH>=10", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_LHCommand_Fumble()
        {
            var randomizer = new MockRandomizer(1, 1, 1); // Fumble: all 1s
            var result = LogHorizon.Instance.Eval("3LH", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_TRSCommand()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll for TRS2
            var result = LogHorizon.Instance.Eval("TRS2", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = LogHorizon.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = LogHorizon.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
