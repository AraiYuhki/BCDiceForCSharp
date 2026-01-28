using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class AinecadetteTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Ainecadette", Ainecadette.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("エネカデット", Ainecadette.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("えねかてつと", Ainecadette.Instance.SortKey);
        }

        [Fact]
        public void AI_DefaultDice_Success()
        {
            // 2d10: 5, 4 -> max 5 >= 4 -> Success
            var randomizer = new MockRandomizer(5, 4);
            var result = Ainecadette.Instance.Eval("AI", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(AI)", result!.Text);
            Assert.Contains("[5,4]", result.Text);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void AI_DefaultDice_Failure()
        {
            // 2d10: 2, 3 -> max 3 < 4 -> Failure
            var randomizer = new MockRandomizer(2, 3);
            var result = Ainecadette.Instance.Eval("AI", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void AI_Fumble_MaxOne()
        {
            // 2d10: 1, 1 -> max 1 <= 1 -> Fumble
            var randomizer = new MockRandomizer(1, 1);
            var result = Ainecadette.Instance.Eval("AI", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result!.Text);
            Assert.Contains("もやもやカウンター", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void AI_Special_ContainsSix()
        {
            // 2d10: 6, 3 -> contains 6 -> Special
            var randomizer = new MockRandomizer(6, 3);
            var result = Ainecadette.Instance.Eval("AI", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result!.Text);
            Assert.Contains("後輩は先輩への感情", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CA_DefaultDice_Success()
        {
            // 2d6: 4, 5 -> max 5 >= 4 -> Success
            var randomizer = new MockRandomizer(4, 5);
            var result = Ainecadette.Instance.Eval("CA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(CA)", result!.Text);
            Assert.Contains("[4,5]", result.Text);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CA_DefaultDice_Failure()
        {
            // 2d6: 2, 3 -> max 3 < 4 -> Failure
            var randomizer = new MockRandomizer(2, 3);
            var result = Ainecadette.Instance.Eval("CA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void CA_Special_ContainsSix()
        {
            // 2d6: 6, 2 -> contains 6 -> Special
            var randomizer = new MockRandomizer(6, 2);
            var result = Ainecadette.Instance.Eval("CA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result!.Text);
            Assert.Contains("先輩は後輩への感情", result.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void AI_Advantage_ThreeDice()
        {
            // 3d10: 4, 5, 6 -> contains 6 -> Special
            var randomizer = new MockRandomizer(4, 5, 6);
            var result = Ainecadette.Instance.Eval("3AI", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(3AI)", result!.Text);
            Assert.Contains("[4,5,6]", result.Text);
        }

        [Fact]
        public void CA_Disadvantage_OneDice()
        {
            // 1d6: 5 -> max 5 >= 4 -> Success
            var randomizer = new MockRandomizer(5);
            var result = Ainecadette.Instance.Eval("1CA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(1CA)", result!.Text);
            Assert.Contains("[5]", result.Text);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void AI_ZeroDice_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = Ainecadette.Instance.Eval("0AI", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void AI_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(5, 4);
            var result = Ainecadette.Instance.Eval("ai", randomizer);

            Assert.NotNull(result);
            Assert.Contains("AI", result!.Text);
        }

        [Fact]
        public void CA_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(5, 4);
            var result = Ainecadette.Instance.Eval("ca", randomizer);

            Assert.NotNull(result);
            Assert.Contains("CA", result!.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Ainecadette.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }
    }
}
