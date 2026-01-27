using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class KamigakariTests
    {
        [Fact]
        public void Id_ReturnsKamigakari()
        {
            Assert.Equal("Kamigakari", Kamigakari.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("神我狩", Kamigakari.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Kamigakari.Instance.D66SortType);
        }

        [Fact]
        public void Eval_KgCommand_Basic()
        {
            var randomizer = new MockRandomizer(4, 5); // 9
            var result = Kamigakari.Instance.Eval("KG", randomizer);

            Assert.NotNull(result);
            Assert.Contains("[4,5]", result.Text);
            Assert.Contains("9", result.Text);
        }

        [Fact]
        public void Eval_KgCommand_WithModifier()
        {
            var randomizer = new MockRandomizer(3, 4); // 7 + 2 = 9
            var result = Kamigakari.Instance.Eval("KG+2", randomizer);

            Assert.NotNull(result);
            Assert.Contains("+2", result.Text);
            Assert.Contains("9", result.Text);
        }

        [Fact]
        public void Eval_KgCommand_Critical()
        {
            var randomizer = new MockRandomizer(6, 6); // 12 = critical
            var result = Kamigakari.Instance.Eval("KG", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_KgCommand_Fumble()
        {
            var randomizer = new MockRandomizer(1, 1); // 2 = fumble
            var result = Kamigakari.Instance.Eval("KG", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_KgCommand_WithSpiritDice()
        {
            var randomizer = new MockRandomizer(3, 4, 6, 5); // Top 2: 6, 5 = 11
            var result = Kamigakari.Instance.Eval("2KG", randomizer);

            Assert.NotNull(result);
            Assert.Contains("上位2個", result.Text);
            Assert.Contains("11", result.Text);
        }

        [Fact]
        public void Eval_KgCommand_CustomCritical()
        {
            var randomizer = new MockRandomizer(5, 5); // 10 >= 10 = critical
            var result = Kamigakari.Instance.Eval("KG@10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_TalentTable()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = Kamigakari.Instance.Eval("TT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("才能表", result.Text);
            Assert.Contains("(23)", result.Text);
        }

        [Fact]
        public void Eval_SecretKg_IsSecret()
        {
            var randomizer = new MockRandomizer(4, 5);
            var result = Kamigakari.Instance.Eval("SKG", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Kamigakari.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
