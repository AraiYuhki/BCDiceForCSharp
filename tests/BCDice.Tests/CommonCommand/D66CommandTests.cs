using BCDice.CommonCommand;
using BCDice.Core;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class D66CommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_SimpleD66_ReturnsCombinedResult()
        {
            var randomizer = new MockRandomizer(3, 5); // 35
            var result = D66Command.Instance.Eval("D66", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("35", result.Text);
            Assert.Contains("[3,5]", result.Text);
        }

        [Fact]
        public void Eval_D66WithSortAscending_SortsResult()
        {
            var randomizer = new MockRandomizer(5, 3); // 53 → sorted to 35
            var result = D66Command.Instance.Eval("D66S", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("35", result.Text);
        }

        [Fact]
        public void Eval_D66WithSortA_SortsResult()
        {
            var randomizer = new MockRandomizer(6, 2); // 62 → sorted to 26
            var result = D66Command.Instance.Eval("D66A", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("26", result.Text);
        }

        [Fact]
        public void Eval_D66NoSort_KeepsOrder()
        {
            var randomizer = new MockRandomizer(5, 3); // 53 not sorted
            var result = D66Command.Instance.Eval("D66N", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("53", result.Text);
        }

        [Fact]
        public void Eval_D66WithComparisonSuccess_ReturnsSuccess()
        {
            var randomizer = new MockRandomizer(4, 5); // 45 >= 40
            var result = D66Command.Instance.Eval("D66>=40", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_D66WithComparisonFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(2, 3); // 23 >= 40
            var result = D66Command.Instance.Eval("D66>=40", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_SecretD66_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = D66Command.Instance.Eval("SD66", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = D66Command.Instance.Eval("D67", _context, randomizer);

            Assert.Null(result);
        }
    }
}
