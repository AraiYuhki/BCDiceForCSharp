using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class GURPSTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("GURPS", GURPS.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("ガープス", GURPS.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GURPS.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(GURPS.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(GURPS.Instance, GURPS.Instance);
        }
    }
}
