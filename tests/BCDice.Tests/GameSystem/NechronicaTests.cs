using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class NechronicaTests
    {
        [Fact]
        public void Id_ReturnsNechronica()
        {
            Assert.Equal("Nechronica", Nechronica.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ネクロニカ", Nechronica.Instance.Name);
        }

        [Fact]
        public void Eval_NcCommand_CountsSuccesses()
        {
            var randomizer = new MockRandomizer(3, 6, 8, 2); // 6, 8 >= 6 = 2 successes
            var result = Nechronica.Instance.Eval("4NC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数2", result.Text);
        }

        [Fact]
        public void Eval_NcCritical_CountsDouble()
        {
            var randomizer = new MockRandomizer(3, 10, 5); // 10 = 2 successes
            var result = Nechronica.Instance.Eval("3NC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数2", result.Text);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_NcFumble_AllOnes()
        {
            var randomizer = new MockRandomizer(1, 1, 1); // All 1s
            var result = Nechronica.Instance.Eval("3NC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_NcNoSuccess_ReturnsZero()
        {
            var randomizer = new MockRandomizer(2, 3, 4, 5); // None >= 6
            var result = Nechronica.Instance.Eval("4NC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数0", result.Text);
        }

        [Fact]
        public void Eval_AttackCommand_Works()
        {
            var randomizer = new MockRandomizer(7); // 7 + 3 = 10
            var result = Nechronica.Instance.Eval("NA3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ダメージ10", result.Text);
        }

        [Fact]
        public void Eval_SecretNc_IsSecret()
        {
            var randomizer = new MockRandomizer(5, 6, 7);
            var result = Nechronica.Instance.Eval("S3NC", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Nechronica.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
