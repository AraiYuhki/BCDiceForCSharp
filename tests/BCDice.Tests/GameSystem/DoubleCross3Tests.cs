using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class DoubleCross3Tests
    {
        [Fact]
        public void Id_ReturnsDoubleCross3()
        {
            Assert.Equal("DoubleCross3", DoubleCross3.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ダブルクロス", DoubleCross3.Instance.Name);
        }

        [Fact]
        public void Eval_SimpleDx_ReturnsMaxValue()
        {
            var randomizer = new MockRandomizer(3, 7, 5, 8); // 4 dice: 3, 7, 5, 8 -> max 8
            var result = DoubleCross3.Instance.Eval("4DX", randomizer);

            Assert.NotNull(result);
            Assert.Contains("最大8", result.Text);
        }

        [Fact]
        public void Eval_DxWithCritical_Explodes()
        {
            var randomizer = new MockRandomizer(3, 10, 5, 7); // Die 2 explodes: 10+5=15
            var result = DoubleCross3.Instance.Eval("2DX", randomizer);

            Assert.NotNull(result);
            Assert.Contains("10+5=15", result.Text);
            Assert.Contains("最大15", result.Text);
        }

        [Fact]
        public void Eval_DxWithModifier_AddsModifier()
        {
            var randomizer = new MockRandomizer(5, 8); // max 8 + 3 = 11
            var result = DoubleCross3.Instance.Eval("2DX+3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("+3", result.Text);
        }

        [Fact]
        public void Eval_DxWithTarget_ComparesAgainstTarget()
        {
            var randomizer = new MockRandomizer(7, 9); // max 9 >= 8
            var result = DoubleCross3.Instance.Eval("2DX>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_DxWithTargetFailure_ReturnsFailure()
        {
            var randomizer = new MockRandomizer(3, 5); // max 5 >= 10
            var result = DoubleCross3.Instance.Eval("2DX>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_DxWithLowerCritical_ExplodesEarlier()
        {
            var randomizer = new MockRandomizer(8, 5, 3); // Die 1 explodes at 8: 8+5=13
            var result = DoubleCross3.Instance.Eval("2DX@8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("8+5=13", result.Text);
        }

        [Fact]
        public void Eval_SecretDx_IsSecret()
        {
            var randomizer = new MockRandomizer(5, 8);
            var result = DoubleCross3.Instance.Eval("S2DX", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = DoubleCross3.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
