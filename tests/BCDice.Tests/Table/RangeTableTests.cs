using System.Collections.Generic;
using BCDice.Table;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.Table
{
    public class RangeTableTests
    {
        private static RangeTable Create2D6Table()
        {
            var entries = new List<(int Min, int Max, string Text)>
            {
                (2, 3, "大失敗"),
                (4, 6, "失敗"),
                (7, 7, "普通"),
                (8, 10, "成功"),
                (11, 12, "大成功")
            };

            return new RangeTable("判定表", "JUDGE", 2, 6, entries);
        }

        [Fact]
        public void Roll_MinRange_ReturnsCorrectResult()
        {
            var table = Create2D6Table();
            var randomizer = new MockRandomizer(1, 1); // 2

            var result = table.Roll(randomizer);

            Assert.Contains("大失敗", result.Text);
            Assert.Contains("(2)", result.Text);
        }

        [Fact]
        public void Roll_MaxRange_ReturnsCorrectResult()
        {
            var table = Create2D6Table();
            var randomizer = new MockRandomizer(6, 6); // 12

            var result = table.Roll(randomizer);

            Assert.Contains("大成功", result.Text);
            Assert.Contains("(12)", result.Text);
        }

        [Fact]
        public void Roll_MiddleRange_ReturnsCorrectResult()
        {
            var table = Create2D6Table();
            var randomizer = new MockRandomizer(3, 4); // 7

            var result = table.Roll(randomizer);

            Assert.Contains("普通", result.Text);
        }

        [Fact]
        public void Roll_RangeBoundary_ReturnsCorrectResult()
        {
            var table = Create2D6Table();
            var randomizer = new MockRandomizer(4, 4); // 8 (成功の下限)

            var result = table.Roll(randomizer);

            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Roll_IncludesTableName()
        {
            var table = Create2D6Table();
            var randomizer = new MockRandomizer(3, 4);

            var result = table.Roll(randomizer);

            Assert.Contains("判定表", result.Text);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            var table = Create2D6Table();
            Assert.Equal("判定表", table.Name);
        }

        [Fact]
        public void Command_ReturnsCorrectCommand()
        {
            var table = Create2D6Table();
            Assert.Equal("JUDGE", table.Command);
        }
    }
}
