using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class NightWizard3rdTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("NightWizard3rd", NightWizard3rd.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ナイトウィザード", NightWizard3rd.Instance.Name);
        }

        [Fact]
        public void Eval_NWCommand_BasicRoll()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll values
            var result = NightWizard3rd.Instance.Eval("10NW@10#5>=15", randomizer);

            Assert.NotNull(result);
        }

        [Fact]
        public void Eval_NWCommand_Fumble()
        {
            var randomizer = new MockRandomizer(2, 3); // Low roll -> fumble
            var result = NightWizard3rd.Instance.Eval("10NW@10#5>=15", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = NightWizard3rd.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_InvalidCommand_ReturnsNull()
        {
            var randomizer = new MockRandomizer();
            var result = NightWizard3rd.Instance.Eval("INVALID", randomizer);

            Assert.Null(result);
        }
    }
}
