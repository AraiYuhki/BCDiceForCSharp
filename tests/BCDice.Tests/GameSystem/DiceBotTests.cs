using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class DiceBotTests
    {
        [Fact]
        public void Id_ReturnsDiceBot()
        {
            Assert.Equal("DiceBot", DiceBot.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ダイスボット", DiceBot.Instance.Name);
        }

        [Fact]
        public void Eval_AddDice_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = DiceBot.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_AddDiceWithComparison_Works()
        {
            var randomizer = new MockRandomizer(4, 5);
            var result = DiceBot.Instance.Eval("2D6>=7", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Eval_UpperDice_Works()
        {
            var randomizer = new MockRandomizer(2, 5);
            var result = DiceBot.Instance.Eval("2B6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("5", result.Text);
        }

        [Fact]
        public void Eval_LowerDice_Works()
        {
            var randomizer = new MockRandomizer(2, 5);
            var result = DiceBot.Instance.Eval("2R6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("2", result.Text);
        }

        [Fact]
        public void Eval_D66_Works()
        {
            var randomizer = new MockRandomizer(3, 5);
            var result = DiceBot.Instance.Eval("D66", randomizer);

            Assert.NotNull(result);
            Assert.Contains("35", result.Text);
        }

        [Fact]
        public void Eval_Choice_Works()
        {
            var randomizer = new MockRandomizer(2);
            var result = DiceBot.Instance.Eval("choice[A,B,C]", randomizer);

            Assert.NotNull(result);
            Assert.Contains("B", result.Text);
        }

        [Fact]
        public void Eval_Calc_Works()
        {
            var randomizer = new MockRandomizer();
            var result = DiceBot.Instance.Eval("C(10+5*2)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("20", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = DiceBot.Instance.Eval("unknown", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_EmptyCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = DiceBot.Instance.Eval("", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_CaseInsensitive_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = DiceBot.Instance.Eval("2d6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_WithDefaultRandomizer_Works()
        {
            var result = DiceBot.Instance.Eval("2D6");

            Assert.NotNull(result);
            // 結果は乱数なので内容はチェックしない
        }
    }
}
