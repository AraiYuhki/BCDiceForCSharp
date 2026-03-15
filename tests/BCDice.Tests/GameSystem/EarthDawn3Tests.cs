using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class EarthDawn3Tests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("EarthDawn3", EarthDawn3.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("アースドーン3版", EarthDawn3.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(EarthDawn3.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(EarthDawn3.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(EarthDawn3.Instance, EarthDawn3.Instance);
        }
    }
}
