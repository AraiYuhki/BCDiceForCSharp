using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class RyutamaTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Ryutama", Ryutama.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("りゅうたま", Ryutama.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("りゆうたま", Ryutama.Instance.SortKey);
        }

        [Fact]
        public void R_TwoDice_Comma_Success()
        {
            // R8,6>=10 with rolls 5(d8) + 6(d6) = 11 >= 10 -> success
            var randomizer = new MockRandomizer(5, 6);
            var result = Ryutama.Instance.Eval("R8,6>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8,6>=10)", result!.Text);
            Assert.Contains("5(8)", result.Text);
            Assert.Contains("+6(6)", result.Text);
            Assert.Contains("11", result.Text);
            Assert.Contains("成功", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void R_TwoDice_Comma_Failure()
        {
            // R8,6>=15 with rolls 4(d8) + 3(d6) = 7 < 15 -> failure
            var randomizer = new MockRandomizer(4, 3);
            var result = Ryutama.Instance.Eval("R8,6>=15", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void R_TwoDice_Combined_86()
        {
            // R86>=10 -> R8,6 with rolls 5+6=11
            var randomizer = new MockRandomizer(5, 6);
            var result = Ryutama.Instance.Eval("R86>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8,6>=10)", result!.Text);
            Assert.Contains("11", result.Text);
        }

        [Fact]
        public void R_TwoDice_Combined_128()
        {
            // R128>=15 -> R12,8 with rolls 10+8=18
            var randomizer = new MockRandomizer(10, 8);
            var result = Ryutama.Instance.Eval("R128>=15", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R12,8>=15)", result!.Text);
            Assert.Contains("18", result.Text);
        }

        [Fact]
        public void R_SingleDice()
        {
            // R8>=5 with roll 6 >= 5 -> success
            var randomizer = new MockRandomizer(6);
            var result = Ryutama.Instance.Eval("R8>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8>=5)", result!.Text);
            Assert.Contains("6(8)", result.Text);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void R_WithModifier_Positive()
        {
            // R8,6+2>=10 with rolls 3+4+2=9
            var randomizer = new MockRandomizer(3, 4);
            var result = Ryutama.Instance.Eval("R8,6+2>=10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8,6+2>=10)", result!.Text);
            Assert.Contains("+2", result.Text);
            Assert.Contains("9", result.Text);
        }

        [Fact]
        public void R_WithModifier_Negative()
        {
            // R8,6-3>=5 with rolls 5+4-3=6
            var randomizer = new MockRandomizer(5, 4);
            var result = Ryutama.Instance.Eval("R8,6-3>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8,6-3>=5)", result!.Text);
            Assert.Contains("-3", result.Text);
            Assert.Contains("6", result.Text);
        }

        [Fact]
        public void R_NoDifficulty()
        {
            // R8,6 without difficulty - no success/failure result
            var randomizer = new MockRandomizer(5, 4);
            var result = Ryutama.Instance.Eval("R8,6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(R8,6)", result!.Text);
            Assert.Contains("9", result.Text);
            Assert.DoesNotContain("成功", result.Text);
            Assert.DoesNotContain("失敗", result.Text);
        }

        [Fact]
        public void R_Fumble_BothOne()
        {
            // Both dice show 1 -> Fumble
            var randomizer = new MockRandomizer(1, 1);
            var result = Ryutama.Instance.Eval("R8,6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("１ゾロ", result!.Text);
            Assert.Contains("１ゾロポイント＋１", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void R_Critical_BothSix()
        {
            // Both dice show 6 -> Critical
            var randomizer = new MockRandomizer(6, 6);
            var result = Ryutama.Instance.Eval("R8,6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル成功", result!.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void R_Critical_BothMax()
        {
            // Both dice show max (8 and 6) -> Critical
            var randomizer = new MockRandomizer(8, 6);
            var result = Ryutama.Instance.Eval("R8,6>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル成功", result!.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void R_Critical_D20AndD12_BothMax()
        {
            // d20=20, d12=12 -> Critical (both max)
            var randomizer = new MockRandomizer(20, 12);
            var result = Ryutama.Instance.Eval("R20,12>=25", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル成功", result!.Text);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void R_InvalidDice_ReturnsNull()
        {
            // d7 is not valid
            var randomizer = new MockRandomizer();
            var result = Ryutama.Instance.Eval("R7>=5", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void R_ValidDiceTypes()
        {
            // Test all valid dice types: d20, d12, d10, d8, d6, d4, d2
            var validTypes = new[] { 20, 12, 10, 8, 6, 4, 2 };
            foreach (var diceType in validTypes)
            {
                var randomizer = new MockRandomizer(1);
                var result = Ryutama.Instance.Eval($"R{diceType}>=1", randomizer);

                Assert.NotNull(result);
                Assert.Contains($"({diceType})", result!.Text);
            }
        }

        [Fact]
        public void R_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(5, 4);
            var result = Ryutama.Instance.Eval("r8,6>=9", randomizer);

            Assert.NotNull(result);
            Assert.Contains("9", result!.Text);
        }

        [Fact]
        public void R_SingleDice_NoCritical()
        {
            // Single dice roll cannot be critical (need two dice)
            var randomizer = new MockRandomizer(8);
            var result = Ryutama.Instance.Eval("R8>=5", randomizer);

            Assert.NotNull(result);
            Assert.DoesNotContain("クリティカル", result!.Text);
            Assert.False(result.IsCritical);
        }

        [Fact]
        public void R_Fumble_OverridesDifficulty()
        {
            // Fumble even if total would succeed
            var randomizer = new MockRandomizer(1, 1);
            var result = Ryutama.Instance.Eval("R8,6>=2", randomizer);

            Assert.NotNull(result);
            Assert.Contains("１ゾロ", result!.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Ryutama.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }
    }
}
