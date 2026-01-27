using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class ShinobigamiTests
    {
        [Fact]
        public void Id_ReturnsShinobigami()
        {
            Assert.Equal("Shinobigami", Shinobigami.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("シノビガミ", Shinobigami.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Shinobigami.Instance.D66SortType);
        }

        [Fact]
        public void Eval_NormalRoll_ReturnsResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Shinobigami.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
            Assert.DoesNotContain("スペシャル", result.Text);
        }

        [Fact]
        public void Eval_SnakeEyes_ReturnsFumble()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble
            var result = Shinobigami.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_BoxCars_ReturnsSpecial()
        {
            var randomizer = new MockRandomizer(6, 6); // Special
            var result = Shinobigami.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_EmotionTable_ReturnsEmotion()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = Shinobigami.Instance.Eval("ET", randomizer);

            Assert.NotNull(result);
            Assert.Contains("感情表", result.Text);
        }

        [Fact]
        public void Eval_FumbleTable_ReturnsEffect()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll 7
            var result = Shinobigami.Instance.Eval("FT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル表", result.Text);
            Assert.Contains("(7)", result.Text);
        }

        [Fact]
        public void Eval_SecretEmotionTable_IsSecret()
        {
            var randomizer = new MockRandomizer(2, 3);
            var result = Shinobigami.Instance.Eval("SET", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Shinobigami.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
