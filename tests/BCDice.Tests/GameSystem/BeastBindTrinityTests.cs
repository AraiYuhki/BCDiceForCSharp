using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class BeastBindTrinityTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("BeastBindTrinity", BeastBindTrinity.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ビーストバインド", BeastBindTrinity.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, BeastBindTrinity.Instance.D66SortType);
        }

        [Fact]
        public void Eval_BBRoll_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 2BB: dice 4, 5 -> max 5+something >= 7
            var result = BeastBindTrinity.Instance.Eval("2BB>=7", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_BBRoll_Critical()
        {
            var randomizer = new MockRandomizer(6, 6); // Critical on 6,6
            var result = BeastBindTrinity.Instance.Eval("2BB%50>=7", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void Eval_BBRoll_Failure()
        {
            var randomizer = new MockRandomizer(1, 2); // Low roll
            var result = BeastBindTrinity.Instance.Eval("2BB>=7", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = BeastBindTrinity.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = BeastBindTrinity.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
