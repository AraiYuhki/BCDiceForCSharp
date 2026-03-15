using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class Arianrhod2ETests
    {
        [Fact]
        public void Id_ReturnsArianrhod2E()
        {
            Assert.Equal("Arianrhod2E", Arianrhod2E.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("アリアンロッド", Arianrhod2E.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Arianrhod2E.Instance.D66SortType);
        }

        [Fact]
        public void Eval_NormalRoll_ReturnsResult()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Arianrhod2E.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.DoesNotContain("クリティカル", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
        }

        [Fact]
        public void Eval_Critical_On12()
        {
            var randomizer = new MockRandomizer(6, 6); // 12
            var result = Arianrhod2E.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_Fumble_On2()
        {
            var randomizer = new MockRandomizer(1, 1); // 2
            var result = Arianrhod2E.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_Failure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(2, 2); // 4 < 5
            var result = Arianrhod2E.Instance.Eval("2D6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Arianrhod2E.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
