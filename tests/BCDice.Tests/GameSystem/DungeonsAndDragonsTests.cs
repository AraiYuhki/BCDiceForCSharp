using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class DungeonsAndDragonsTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("DungeonsAndDragons", DungeonsAndDragons.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("ダンジョンズ＆ドラゴンズ", DungeonsAndDragons.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("たんしよんすあんととらこんす", DungeonsAndDragons.Instance.SortKey);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = DungeonsAndDragons.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }

        [Fact]
        public void CommonCommand_1D20_Works()
        {
            var randomizer = new MockRandomizer(15);
            var result = DungeonsAndDragons.Instance.Eval("1D20", randomizer);

            Assert.NotNull(result);
            Assert.Contains("15", result!.Text);
        }

        [Fact]
        public void SpecificCommand_ReturnsNull()
        {
            // This system has no specific commands
            var randomizer = new MockRandomizer();
            var result = DungeonsAndDragons.Instance.Eval("SPECIAL", randomizer);

            Assert.Null(result);
        }
    }
}
