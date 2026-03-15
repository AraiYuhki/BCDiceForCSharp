using System.Collections.Generic;
using BCDice.Core;
using BCDice.Table;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.Table
{
    public class D66TableTests
    {
        private static D66Table CreateSampleTable()
        {
            var entries = new Dictionary<int, string>
            {
                [11] = "項目11",
                [12] = "項目12",
                [35] = "項目35",
                [66] = "項目66"
            };

            return new D66Table("サンプル表", "SAMPLE", D66SortType.NoSort, entries);
        }

        [Fact]
        public void Roll_Entry11_ReturnsCorrectResult()
        {
            var table = CreateSampleTable();
            var randomizer = new MockRandomizer(1, 1);

            var result = table.Roll(randomizer);

            Assert.Contains("項目11", result.Text);
            Assert.Contains("(11)", result.Text);
        }

        [Fact]
        public void Roll_Entry35_ReturnsCorrectResult()
        {
            var table = CreateSampleTable();
            var randomizer = new MockRandomizer(3, 5);

            var result = table.Roll(randomizer);

            Assert.Contains("項目35", result.Text);
        }

        [Fact]
        public void Roll_Entry66_ReturnsCorrectResult()
        {
            var table = CreateSampleTable();
            var randomizer = new MockRandomizer(6, 6);

            var result = table.Roll(randomizer);

            Assert.Contains("項目66", result.Text);
        }

        [Fact]
        public void Roll_MissingEntry_ReturnsNotFound()
        {
            var table = CreateSampleTable();
            var randomizer = new MockRandomizer(2, 2); // 22 is not in the table

            var result = table.Roll(randomizer);

            Assert.Contains("該当なし", result.Text);
        }

        [Fact]
        public void Roll_IncludesTableName()
        {
            var table = CreateSampleTable();
            var randomizer = new MockRandomizer(1, 1);

            var result = table.Roll(randomizer);

            Assert.Contains("サンプル表", result.Text);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            var table = CreateSampleTable();
            Assert.Equal("サンプル表", table.Name);
        }

        [Fact]
        public void Command_ReturnsCorrectCommand()
        {
            var table = CreateSampleTable();
            Assert.Equal("SAMPLE", table.Command);
        }
    }
}
