using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class InsaneTests
    {
        [Fact]
        public void Id_ReturnsInsane()
        {
            Assert.Equal("Insane", Insane.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("インセイン", Insane.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Insane.Instance.D66SortType);
        }

        [Fact]
        public void Eval_NormalRoll_ReturnsResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Insane.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
            Assert.DoesNotContain("スペシャル", result.Text);
        }

        [Fact]
        public void Eval_SnakeEyes_ReturnsFumble()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble
            var result = Insane.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_BoxCars_ReturnsSpecial()
        {
            var randomizer = new MockRandomizer(6, 6); // Special
            var result = Insane.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_SpecialTable_ReturnsEffect()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll 7
            var result = Insane.Instance.Eval("ST", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル表", result.Text);
            Assert.Contains("(7)", result.Text);
        }

        [Fact]
        public void Eval_FumbleTable_ReturnsEffect()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll 7
            var result = Insane.Instance.Eval("FT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル表", result.Text);
            Assert.Contains("(7)", result.Text);
        }

        [Fact]
        public void Eval_SecretSpecialTable_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Insane.Instance.Eval("SST", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Insane.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
