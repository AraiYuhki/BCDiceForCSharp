using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class PathfinderTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Pathfinder", Pathfinder.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("Pathfinder", Pathfinder.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(Pathfinder.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(Pathfinder.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(Pathfinder.Instance, Pathfinder.Instance);
        }
    }
}
