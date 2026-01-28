using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class ParanoiaTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Paranoia", Paranoia.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("パラノイア", Paranoia.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("はらのいあ", Paranoia.Instance.SortKey);
        }

        [Fact]
        public void Geta_Roll1_ReturnsHappy()
        {
            var randomizer = new MockRandomizer(1);
            var result = Paranoia.Instance.Eval("geta", randomizer);

            Assert.NotNull(result);
            Assert.Contains("幸福ですか？", result!.Text);
            Assert.Contains("幸福です", result.Text);
        }

        [Fact]
        public void Geta_Roll2_ReturnsNotHappy()
        {
            var randomizer = new MockRandomizer(2);
            var result = Paranoia.Instance.Eval("geta", randomizer);

            Assert.NotNull(result);
            Assert.Contains("幸福ではありません", result!.Text);
        }

        [Fact]
        public void Geta_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(1);
            var result = Paranoia.Instance.Eval("GETA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("幸福です", result!.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Paranoia.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }
    }
}
