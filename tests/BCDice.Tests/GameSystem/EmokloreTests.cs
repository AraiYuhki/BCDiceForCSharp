using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class EmokloreTests
    {
        [Fact]
        public void Id_ReturnsEmoklore()
        {
            Assert.Equal("Emoklore", Emoklore.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("エモクロア", Emoklore.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Emoklore.Instance.D66SortType);
        }

        [Fact]
        public void Eval_NormalRoll_ReturnsResult()
        {
            var randomizer = new MockRandomizer(4, 5); // 9
            var result = Emoklore.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
            Assert.DoesNotContain("スペシャル", result.Text);
        }

        [Fact]
        public void Eval_Fumble_OnSnakeEyes()
        {
            var randomizer = new MockRandomizer(1, 1);
            var result = Emoklore.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Special_OnBoxCars()
        {
            var randomizer = new MockRandomizer(6, 6);
            var result = Emoklore.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_EmotionTable_ReturnsEmotion()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = Emoklore.Instance.Eval("EMT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("感情表", result.Text);
        }

        [Fact]
        public void Eval_ResonanceTable_ReturnsResonance()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Emoklore.Instance.Eval("RT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("共鳴表", result.Text);
            Assert.Contains("(7)", result.Text);
        }

        [Fact]
        public void Eval_SecretEmotionTable_IsSecret()
        {
            var randomizer = new MockRandomizer(2, 3);
            var result = Emoklore.Instance.Eval("SEMT", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Emoklore.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
