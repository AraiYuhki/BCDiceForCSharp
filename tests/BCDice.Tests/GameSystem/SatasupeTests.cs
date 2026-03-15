using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class SatasupeTests
    {
        [Fact]
        public void Id_ReturnsSatasupe()
        {
            Assert.Equal("Satasupe", Satasupe.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("サタスペ", Satasupe.Instance.Name);
        }

        [Fact]
        public void Eval_JudgeCommand_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // max = 5 >= 4
            var result = Satasupe.Instance.Eval("2R>=4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.Contains("最大5", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // max = 3 < 5
            var result = Satasupe.Instance.Eval("2R>=5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_Fumble()
        {
            var randomizer = new MockRandomizer(1, 1); // All 1s = fumble
            var result = Satasupe.Instance.Eval("2R>=4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_JudgeCommand_Reroll()
        {
            var randomizer = new MockRandomizer(6, 3, 2); // First die: 6+3=9, Second die: 2, max = 9
            var result = Satasupe.Instance.Eval("2R>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.Contains("6+3=9", result.Text);
            Assert.Contains("最大9", result.Text);
        }

        [Fact]
        public void Eval_CrimeTable_Basic()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Satasupe.Instance.Eval("CRIME", randomizer);

            Assert.NotNull(result);
            Assert.Contains("犯罪表", result.Text);
            Assert.Contains("(7)", result.Text);
        }

        [Fact]
        public void Eval_CrimeTable_WithLevel()
        {
            var randomizer = new MockRandomizer(3, 4); // 7 + 3 = 10
            var result = Satasupe.Instance.Eval("CRIME3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("犯罪表", result.Text);
            Assert.Contains("+3=10", result.Text);
        }

        [Fact]
        public void Eval_FumbleTable()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = Satasupe.Instance.Eval("FUMBLE", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル表", result.Text);
            Assert.Contains("(7)", result.Text);
            Assert.Contains("何も起こらない", result.Text);
        }

        [Fact]
        public void Eval_SecretJudge_IsSecret()
        {
            var randomizer = new MockRandomizer(4, 5);
            var result = Satasupe.Instance.Eval("S2R>=4", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Satasupe.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
