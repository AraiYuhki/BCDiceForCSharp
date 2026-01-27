using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class CthulhuTests
    {
        [Fact]
        public void Id_ReturnsCthulhu()
        {
            Assert.Equal("Cthulhu", Cthulhu.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("クトゥルフ", Cthulhu.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, Cthulhu.Instance.D66SortType);
        }

        [Fact]
        public void Eval_CcbSuccess_ReturnsSuccess()
        {
            var randomizer = new MockRandomizer(50); // 50 <= 80
            var result = Cthulhu.Instance.Eval("CCB<=80", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_CcbFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(85); // 85 > 80
            var result = Cthulhu.Instance.Eval("CCB<=80", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_CcbCritical_ReturnsCritical()
        {
            var randomizer = new MockRandomizer(3); // 3 <= 5
            var result = Cthulhu.Instance.Eval("CCB<=80", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_CcbFumble_ReturnsFumble()
        {
            var randomizer = new MockRandomizer(98); // 98 >= 96
            var result = Cthulhu.Instance.Eval("CCB<=80", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_CcShortForm_Works()
        {
            var randomizer = new MockRandomizer(50);
            var result = Cthulhu.Instance.Eval("CC<=80", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Eval_CcWithParens_Works()
        {
            var randomizer = new MockRandomizer(50);
            var result = Cthulhu.Instance.Eval("CCB(80)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("50", result.Text);
        }

        [Fact]
        public void Eval_CcNoTarget_ReturnsRollOnly()
        {
            var randomizer = new MockRandomizer(42);
            var result = Cthulhu.Instance.Eval("CCB", randomizer);

            Assert.NotNull(result);
            Assert.Contains("42", result.Text);
            Assert.DoesNotContain("成功", result.Text);
            Assert.DoesNotContain("失敗", result.Text);
        }

        [Fact]
        public void Eval_SecretCcb_IsSecret()
        {
            var randomizer = new MockRandomizer(50);
            var result = Cthulhu.Instance.Eval("SCCB<=80", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_D66_UsesSorting()
        {
            var randomizer = new MockRandomizer(5, 2); // 52 → 25 (sorted)
            var result = Cthulhu.Instance.Eval("D66", randomizer);

            Assert.NotNull(result);
            Assert.Contains("25", result.Text);
        }

        [Fact]
        public void Eval_CommonCommand_StillWorks()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Cthulhu.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
