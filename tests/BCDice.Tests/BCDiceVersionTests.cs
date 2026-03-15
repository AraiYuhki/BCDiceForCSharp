using BCDice;
using Xunit;

namespace BCDice.Tests
{
    public class BCDiceVersionTests
    {
        [Fact]
        public void Version_ReturnsFormattedString()
        {
            var version = BCDiceVersion.Version;

            Assert.Matches(@"^\d+\.\d+\.\d+$", version);
        }

        [Fact]
        public void FullVersion_ContainsName()
        {
            var fullVersion = BCDiceVersion.FullVersion;

            Assert.Contains(BCDiceVersion.Name, fullVersion);
        }

        [Fact]
        public void FullVersion_ContainsVersion()
        {
            var fullVersion = BCDiceVersion.FullVersion;

            Assert.Contains(BCDiceVersion.Version, fullVersion);
        }

        [Fact]
        public void Name_IsNotEmpty()
        {
            Assert.False(string.IsNullOrEmpty(BCDiceVersion.Name));
        }

        [Fact]
        public void Major_IsNonNegative()
        {
            Assert.True(BCDiceVersion.Major >= 0);
        }

        [Fact]
        public void Minor_IsNonNegative()
        {
            Assert.True(BCDiceVersion.Minor >= 0);
        }

        [Fact]
        public void Patch_IsNonNegative()
        {
            Assert.True(BCDiceVersion.Patch >= 0);
        }
    }
}
