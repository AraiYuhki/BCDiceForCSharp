using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class ShadowRun5Tests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("ShadowRun5", ShadowRun5.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("シャドウラン 5th Edition", ShadowRun5.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(ShadowRun5.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(ShadowRun5.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(ShadowRun5.Instance, ShadowRun5.Instance);
        }
    }
}
