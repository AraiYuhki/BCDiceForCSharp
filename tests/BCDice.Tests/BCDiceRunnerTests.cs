using BCDice;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests
{
    public class BCDiceRunnerTests
    {
        [Fact]
        public void GameSystems_ContainsDefaultSystems()
        {
            Assert.True(BCDiceRunner.GameSystems.Count >= 2);
        }

        [Fact]
        public void GetGameSystem_DiceBot_ReturnsSystem()
        {
            var system = BCDiceRunner.GetGameSystem("DiceBot");

            Assert.NotNull(system);
            Assert.Equal("DiceBot", system.Id);
        }

        [Fact]
        public void GetGameSystem_Cthulhu_ReturnsSystem()
        {
            var system = BCDiceRunner.GetGameSystem("Cthulhu");

            Assert.NotNull(system);
            Assert.Equal("Cthulhu", system.Id);
        }

        [Fact]
        public void GetGameSystem_Unknown_ReturnsNull()
        {
            var system = BCDiceRunner.GetGameSystem("UnknownSystem");

            Assert.Null(system);
        }

        [Fact]
        public void SetGameSystem_ValidId_ReturnsTrue()
        {
            var result = BCDiceRunner.SetGameSystem("Cthulhu");

            Assert.True(result);
            Assert.Equal("Cthulhu", BCDiceRunner.CurrentSystem.Id);

            // Reset to default
            BCDiceRunner.SetGameSystem("DiceBot");
        }

        [Fact]
        public void SetGameSystem_InvalidId_ReturnsFalse()
        {
            var originalSystem = BCDiceRunner.CurrentSystem;

            var result = BCDiceRunner.SetGameSystem("Unknown");

            Assert.False(result);
            Assert.Equal(originalSystem.Id, BCDiceRunner.CurrentSystem.Id);
        }

        [Fact]
        public void Eval_WithSystemId_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = BCDiceRunner.Eval("DiceBot", "2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }

        [Fact]
        public void Eval_WithInvalidSystemId_ReturnsNull()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = BCDiceRunner.Eval("Unknown", "2D6", randomizer);

            Assert.Null(result);
        }

        [Fact]
        public void SearchGameSystems_PartialMatch_ReturnsResults()
        {
            var results = BCDiceRunner.SearchGameSystems("ダイス");

            Assert.NotEmpty(results);
        }

        [Fact]
        public void SearchGameSystems_NoMatch_ReturnsEmpty()
        {
            var results = BCDiceRunner.SearchGameSystems("存在しないシステム");

            Assert.Empty(results);
        }

        [Fact]
        public void CurrentSystem_DefaultIsDiceBot()
        {
            // Reset to default first
            BCDiceRunner.SetGameSystem("DiceBot");

            Assert.Equal("DiceBot", BCDiceRunner.CurrentSystem.Id);
        }
    }
}
