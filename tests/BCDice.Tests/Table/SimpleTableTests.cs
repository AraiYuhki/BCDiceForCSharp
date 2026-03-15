using BCDice.Table;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.Table
{
    public class SimpleTableTests
    {
        [Fact]
        public void Roll_FirstItem_ReturnsFirstItem()
        {
            var table = new SimpleTable("テスト表", "TEST",
                "結果A", "結果B", "結果C", "結果D", "結果E", "結果F");

            var randomizer = new MockRandomizer(1);
            var result = table.Roll(randomizer);

            Assert.Contains("結果A", result.Text);
            Assert.Contains("(1)", result.Text);
        }

        [Fact]
        public void Roll_LastItem_ReturnsLastItem()
        {
            var table = new SimpleTable("テスト表", "TEST",
                "結果A", "結果B", "結果C", "結果D", "結果E", "結果F");

            var randomizer = new MockRandomizer(6);
            var result = table.Roll(randomizer);

            Assert.Contains("結果F", result.Text);
            Assert.Contains("(6)", result.Text);
        }

        [Fact]
        public void Roll_MiddleItem_ReturnsCorrectItem()
        {
            var table = new SimpleTable("テスト表", "TEST",
                "結果A", "結果B", "結果C", "結果D", "結果E", "結果F");

            var randomizer = new MockRandomizer(3);
            var result = table.Roll(randomizer);

            Assert.Contains("結果C", result.Text);
        }

        [Fact]
        public void Roll_IncludesTableName()
        {
            var table = new SimpleTable("天候表", "WEATHER",
                "晴れ", "曇り", "雨");

            var randomizer = new MockRandomizer(2);
            var result = table.Roll(randomizer);

            Assert.Contains("天候表", result.Text);
            Assert.Contains("曇り", result.Text);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            var table = new SimpleTable("テスト表", "TEST", "A", "B");
            Assert.Equal("テスト表", table.Name);
        }

        [Fact]
        public void Command_ReturnsCorrectCommand()
        {
            var table = new SimpleTable("テスト表", "TEST", "A", "B");
            Assert.Equal("TEST", table.Command);
        }
    }
}
