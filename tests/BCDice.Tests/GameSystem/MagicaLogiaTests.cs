using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class MagicaLogiaTests
    {
        [Fact]
        public void Id_ReturnsMagicaLogia()
        {
            Assert.Equal("MagicaLogia", MagicaLogia.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("マギカロギア", MagicaLogia.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, MagicaLogia.Instance.D66SortType);
        }

        [Fact]
        public void Eval_JudgeCommand_Success()
        {
            var randomizer = new MockRandomizer(4, 5); // 9 >= 8
            var result = MagicaLogia.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_Failure()
        {
            var randomizer = new MockRandomizer(2, 3); // 5 < 8
            var result = MagicaLogia.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result.Text);
        }

        [Fact]
        public void Eval_JudgeCommand_WithModifier()
        {
            var randomizer = new MockRandomizer(3, 4); // 7 + 2 = 9 >= 8
            var result = MagicaLogia.Instance.Eval("2D6+2>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result.Text);
            Assert.Contains("+2", result.Text);
        }

        [Fact]
        public void Eval_Special_On66()
        {
            var randomizer = new MockRandomizer(6, 6);
            var result = MagicaLogia.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_Fumble_On11()
        {
            var randomizer = new MockRandomizer(1, 1);
            var result = MagicaLogia.Instance.Eval("2D6>=8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_MagicTable()
        {
            var randomizer = new MockRandomizer(2, 3); // D66 = 23
            var result = MagicaLogia.Instance.Eval("MGT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("魔法名表", result.Text);
            Assert.Contains("(23)", result.Text);
        }

        [Fact]
        public void Eval_RuneTable()
        {
            var randomizer = new MockRandomizer(3, 4); // 7
            var result = MagicaLogia.Instance.Eval("RT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ルーン表", result.Text);
            Assert.Contains("(7)", result.Text);
            Assert.Contains("闇", result.Text);
        }

        [Fact]
        public void Eval_SecretMagicTable_IsSecret()
        {
            var randomizer = new MockRandomizer(2, 3);
            var result = MagicaLogia.Instance.Eval("SMGT", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = MagicaLogia.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
