using BCDice.CommonCommand;
using BCDice.Preprocessor;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.CommonCommand
{
    public class ChoiceCommandTests
    {
        private readonly IGameSystemContext _context = DefaultGameSystemContext.Instance;

        [Fact]
        public void Eval_SimpleChoice_ReturnsSelectedOption()
        {
            var randomizer = new MockRandomizer(1); // 最初のオプションを選択
            var result = ChoiceCommand.Instance.Eval("choice[A,B,C]", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("A", result.Text);
        }

        [Fact]
        public void Eval_ChoiceSecondOption_ReturnsSecondOption()
        {
            var randomizer = new MockRandomizer(2); // 2番目のオプションを選択
            var result = ChoiceCommand.Instance.Eval("choice[赤,青,緑]", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("青", result.Text);
        }

        [Fact]
        public void Eval_ChoiceThirdOption_ReturnsThirdOption()
        {
            var randomizer = new MockRandomizer(3); // 3番目のオプションを選択
            var result = ChoiceCommand.Instance.Eval("CHOICE[X,Y,Z]", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("Z", result.Text);
        }

        [Fact]
        public void Eval_TwoOptions_Works()
        {
            var randomizer = new MockRandomizer(2);
            var result = ChoiceCommand.Instance.Eval("choice[はい,いいえ]", _context, randomizer);

            Assert.NotNull(result);
            Assert.Contains("いいえ", result.Text);
        }

        [Fact]
        public void Eval_SecretChoice_IsSecret()
        {
            var randomizer = new MockRandomizer(1);
            var result = ChoiceCommand.Instance.Eval("Schoice[A,B,C]", _context, randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_EmptyBrackets_ReturnsNull()
        {
            var randomizer = new MockRandomizer(1);
            var result = ChoiceCommand.Instance.Eval("choice[]", _context, randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void Eval_InvalidFormat_ReturnsNull()
        {
            var randomizer = new MockRandomizer(1);
            var result = ChoiceCommand.Instance.Eval("choiceABC", _context, randomizer);

            Assert.Null(result);
        }
    }
}
