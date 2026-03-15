using BCDice.Table;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.Table
{
    public class TableCollectionTests
    {
        [Fact]
        public void Add_SingleTable_IncreasesCount()
        {
            var collection = new TableCollection();
            var table = new SimpleTable("テスト", "TEST", "A", "B");

            collection.Add(table);

            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void GetByCommand_ExistingTable_ReturnsTable()
        {
            var collection = new TableCollection();
            var table = new SimpleTable("テスト", "TEST", "A", "B");
            collection.Add(table);

            var result = collection.GetByCommand("TEST");

            Assert.NotNull(result);
            Assert.Equal("テスト", result.Name);
        }

        [Fact]
        public void GetByCommand_CaseInsensitive_ReturnsTable()
        {
            var collection = new TableCollection();
            var table = new SimpleTable("テスト", "TEST", "A", "B");
            collection.Add(table);

            var result = collection.GetByCommand("test");

            Assert.NotNull(result);
        }

        [Fact]
        public void GetByCommand_NonExisting_ReturnsNull()
        {
            var collection = new TableCollection();
            var table = new SimpleTable("テスト", "TEST", "A", "B");
            collection.Add(table);

            var result = collection.GetByCommand("UNKNOWN");

            Assert.Null(result);
        }

        [Fact]
        public void Eval_ExistingCommand_ReturnsResult()
        {
            var collection = new TableCollection();
            var table = new SimpleTable("テスト", "TEST", "結果A", "結果B");
            collection.Add(table);

            var randomizer = new MockRandomizer(1);
            var result = collection.Eval("TEST", randomizer);

            Assert.NotNull(result);
            Assert.Contains("結果A", result.Text);
        }

        [Fact]
        public void Eval_NonExistingCommand_ReturnsNull()
        {
            var collection = new TableCollection();
            var randomizer = new MockRandomizer(1);

            var result = collection.Eval("UNKNOWN", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void AddRange_MultipleTables_AddsAll()
        {
            var collection = new TableCollection();
            var tables = new ITable[]
            {
                new SimpleTable("表1", "TABLE1", "A", "B"),
                new SimpleTable("表2", "TABLE2", "C", "D")
            };

            collection.AddRange(tables);

            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void Clear_RemovesAllTables()
        {
            var collection = new TableCollection();
            collection.Add(new SimpleTable("表1", "TABLE1", "A", "B"));
            collection.Add(new SimpleTable("表2", "TABLE2", "C", "D"));

            collection.Clear();

            Assert.Equal(0, collection.Count);
        }

        [Fact]
        public void GetAll_ReturnsAllTables()
        {
            var collection = new TableCollection();
            collection.Add(new SimpleTable("表1", "TABLE1", "A", "B"));
            collection.Add(new SimpleTable("表2", "TABLE2", "C", "D"));

            var all = collection.GetAll();

            Assert.Equal(2, System.Linq.Enumerable.Count(all));
        }
    }
}
