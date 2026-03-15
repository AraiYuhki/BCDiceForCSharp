using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class BloodMoonTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("BloodMoon", BloodMoon.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ブラッドムーン", BloodMoon.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, BloodMoon.Instance.D66SortType);
        }

        [Fact]
        public void Eval_MITTable()
        {
            var randomizer = new MockRandomizer(3); // Roll for MIT table
            var result = BloodMoon.Instance.Eval("MIT", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_2D6Special_BoxCars()
        {
            var randomizer = new MockRandomizer(6, 6); // Special on 12
            var result = BloodMoon.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_2D6_NormalSuccess()
        {
            var randomizer = new MockRandomizer(3, 4); // 7 >= 5
            var result = BloodMoon.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Eval_2D6_Fumble()
        {
            var randomizer = new MockRandomizer(1, 1); // Fumble
            var result = BloodMoon.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = BloodMoon.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
