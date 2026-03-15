using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class DungeonsAndDragons5Tests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("DungeonsAndDragons5", DungeonsAndDragons5.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("ダンジョンズ＆ドラゴンズ第5版", DungeonsAndDragons5.Instance.Name);
        }

        [Fact]
        public void SortKey_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(DungeonsAndDragons5.Instance.SortKey));
        }

        [Fact]
        public void HelpMessage_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(DungeonsAndDragons5.Instance.HelpMessage));
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(DungeonsAndDragons5.Instance, DungeonsAndDragons5.Instance);
        }
    }
}
