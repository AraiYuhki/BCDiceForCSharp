using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class MeikyuuKingdomTests
    {
        [Fact]
        public void Id_ReturnsMeikyuuKingdom()
        {
            Assert.Equal("MeikyuuKingdom", MeikyuuKingdom.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("迷宮キングダム", MeikyuuKingdom.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, MeikyuuKingdom.Instance.D66SortType);
        }

        [Fact]
        public void Eval_JudgeCommand_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 9 >= 8
            var result = MeikyuuKingdom.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 5 < 8
            var result = MeikyuuKingdom.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_WithModifier()
        {
            var randomizer = new MockRandomizer(3, 4); // 7 + 2 = 9 >= 8
            var result = MeikyuuKingdom.Instance.Eval("2D6+2>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.Contains("+2", result.Text);
        }

        [Fact]
        public void Eval_Critical_On12()
        {
            var randomizer = new MockRandomizer(6, 6);
            var result = MeikyuuKingdom.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_Fumble_On2()
        {
            var randomizer = new MockRandomizer(1, 1);
            var result = MeikyuuKingdom.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_FacilityTable()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = MeikyuuKingdom.Instance.Eval("FT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("施設表", result.Text);
            Assert.Contains("(23)", result.Text);
            Assert.Contains("宿屋", result.Text);
        }

        [Fact]
        public void Eval_NameTable()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = MeikyuuKingdom.Instance.Eval("NT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("名前表", result.Text);
            Assert.Contains("(23)", result.Text);
            Assert.Contains("クル", result.Text);
        }

        [Fact]
        public void Eval_RandomEventTable()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = MeikyuuKingdom.Instance.Eval("RE", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ランダムイベント表", result.Text);
            Assert.Contains("(7)", result.Text);
            Assert.Contains("何も起こらない", result.Text);
        }

        [Fact]
        public void Eval_SecretFacilityTable_IsSecret()
        {
            var randomizer = new MockRandomizer(2, 3);
            var result = MeikyuuKingdom.Instance.Eval("SFT", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = MeikyuuKingdom.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
