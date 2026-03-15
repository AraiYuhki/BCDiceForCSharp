using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class AmadeusTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Amadeus", Amadeus.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("アマデウス", Amadeus.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(Amadeus.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(Amadeus.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(Amadeus.Instance, Amadeus.Instance);
        }
    }
}
