using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class GranCrestTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("GranCrest", GranCrest.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("グランクレスト", GranCrest.Instance.Name);
        }

        [Fact]
        public void Eval_CTTable()
        {
            var randomizer = new MockRandomizer(2); // Roll for CT table
            var result = GranCrest.Instance.Eval("CT", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_2D6_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 9 >= 8
            var result = GranCrest.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_2D6_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 5 < 8
            var result = GranCrest.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = GranCrest.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = GranCrest.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
