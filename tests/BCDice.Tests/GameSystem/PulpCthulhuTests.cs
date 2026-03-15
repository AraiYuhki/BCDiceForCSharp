using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class PulpCthulhuTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("PulpCthulhu", PulpCthulhu.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("パルプ・クトゥルフ", PulpCthulhu.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(PulpCthulhu.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(PulpCthulhu.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(PulpCthulhu.Instance, PulpCthulhu.Instance);
        }
    }
}
