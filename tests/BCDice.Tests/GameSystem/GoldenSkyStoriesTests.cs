using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class GoldenSkyStoriesTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("GoldenSkyStories", GoldenSkyStories.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("ゆうやけこやけ", GoldenSkyStories.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("ゆうやけこやけ", GoldenSkyStories.Instance.SortKey);
        }

        [Fact]
        public void Geta_Roll1_ReturnsAme()
        {
            var randomizer = new MockRandomizer(1);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("下駄占い", result!.Text);
            Assert.Contains("裏：あめ", result.Text);
        }

        [Fact]
        public void Geta_Roll2_ReturnsHare()
        {
            var randomizer = new MockRandomizer(2);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("表：はれ", result!.Text);
        }

        [Fact]
        public void Geta_Roll3_ReturnsAme()
        {
            var randomizer = new MockRandomizer(3);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("裏：あめ", result!.Text);
        }

        [Fact]
        public void Geta_Roll4_ReturnsHare()
        {
            var randomizer = new MockRandomizer(4);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("表：はれ", result!.Text);
        }

        [Fact]
        public void Geta_Roll5_ReturnsAme()
        {
            var randomizer = new MockRandomizer(5);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("裏：あめ", result!.Text);
        }

        [Fact]
        public void Geta_Roll6_ReturnsHare()
        {
            var randomizer = new MockRandomizer(6);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("表：はれ", result!.Text);
        }

        [Fact]
        public void Geta_Roll7_ReturnsKumori()
        {
            var randomizer = new MockRandomizer(7);
            var result = GoldenSkyStories.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("横：くもり", result!.Text);
        }

        [Fact]
        public void Geta_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(1);
            var result = GoldenSkyStories.Instance.Eval("geta", randomizer);

            Assert.NotNull(result);
            Assert.Contains("下駄占い", result!.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = GoldenSkyStories.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }

        [Fact]
        public void InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = GoldenSkyStories.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
