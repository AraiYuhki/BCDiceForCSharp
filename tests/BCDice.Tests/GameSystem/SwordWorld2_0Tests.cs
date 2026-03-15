using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class SwordWorld2_0Tests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("SwordWorld2.0", SwordWorld2_0.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("ソード・ワールド2.0", SwordWorld2_0.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(SwordWorld2_0.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(SwordWorld2_0.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(SwordWorld2_0.Instance, SwordWorld2_0.Instance);
        }
    }
}
