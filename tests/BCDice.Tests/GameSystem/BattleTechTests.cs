using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class BattleTechTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("BattleTech", BattleTech.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("バトルテック", BattleTech.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(BattleTech.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(BattleTech.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(BattleTech.Instance, BattleTech.Instance);
        }
    }
}
