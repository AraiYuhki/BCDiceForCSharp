using BCDice.Core;
using Xunit;

namespace BCDice.Tests.Core
{
    public class RandomizerTests
    {
        [Fact]
        public void RollOnce_ReturnsValueBetween1AndSides()
        {
            var randomizer = new Randomizer(42);

            for (int i = 0; i < 100; i++)
            {
                int result = randomizer.RollOnce(6);
                Assert.InRange(result, 1, 6);
            }
        }

        [Fact]
        public void RollOnce_InvalidSides_ReturnsZero()
        {
            var randomizer = new Randomizer();

            Assert.Equal(0, randomizer.RollOnce(0));
            Assert.Equal(0, randomizer.RollOnce(-1));
            Assert.Equal(0, randomizer.RollOnce(Randomizer.UpperLimitDiceSides + 1));
        }

        [Fact]
        public void RollSum_ReturnsCorrectSum()
        {
            var mock = new MockRandomizer(1, 2, 3, 4, 5);

            int sum = mock.RollSum(5, 6);

            Assert.Equal(15, sum);
        }

        [Fact]
        public void RollBarabara_ReturnsCorrectArray()
        {
            var mock = new MockRandomizer(3, 1, 4, 1, 5);

            int[] results = mock.RollBarabara(5, 6);

            Assert.Equal(new[] { 3, 1, 4, 1, 5 }, results);
        }

        [Fact]
        public void RollBarabara_InvalidTimes_ReturnsEmptyArray()
        {
            var randomizer = new Randomizer();

            Assert.Empty(randomizer.RollBarabara(0, 6));
            Assert.Empty(randomizer.RollBarabara(-1, 6));
            Assert.Empty(randomizer.RollBarabara(Randomizer.UpperLimitDiceTimes + 1, 6));
        }

        [Fact]
        public void RollIndex_ReturnsZeroBasedValue()
        {
            var mock = new MockRandomizer(1, 6);

            Assert.Equal(0, mock.RollIndex(6));
            Assert.Equal(5, mock.RollIndex(6));
        }

        [Fact]
        public void RollTensD10_ReturnsMultiplesOf10()
        {
            var mock = new MockRandomizer(1, 5, 10);

            Assert.Equal(10, mock.RollTensD10());
            Assert.Equal(50, mock.RollTensD10());
            Assert.Equal(0, mock.RollTensD10());
        }

        [Fact]
        public void RollD9_ReturnsValueBetween0And9()
        {
            var mock = new MockRandomizer(1, 10);

            Assert.Equal(0, mock.RollD9());
            Assert.Equal(9, mock.RollD9());
        }

        [Fact]
        public void RollD66_NoSort_ReturnsCorrectValue()
        {
            var mock = new MockRandomizer(3, 5);

            int result = mock.RollD66(D66SortType.NoSort);

            Assert.Equal(35, result);
        }

        [Fact]
        public void RollD66_Ascending_SortsCorrectly()
        {
            var mock = new MockRandomizer(5, 3);

            int result = mock.RollD66(D66SortType.Ascending);

            Assert.Equal(35, result);
        }

        [Fact]
        public void RollD66_Descending_SortsCorrectly()
        {
            var mock = new MockRandomizer(3, 5);

            int result = mock.RollD66(D66SortType.Descending);

            Assert.Equal(53, result);
        }

        [Fact]
        public void RandResults_TracksAllRolls()
        {
            var mock = new MockRandomizer(1, 2, 3);

            mock.RollOnce(6);
            mock.RollOnce(10);
            mock.RollOnce(20);

            Assert.Equal(3, mock.RandResults.Count);
            Assert.Equal((1, 6), mock.RandResults[0]);
            Assert.Equal((2, 10), mock.RandResults[1]);
            Assert.Equal((3, 20), mock.RandResults[2]);
        }

        [Fact]
        public void DetailedRandResults_TracksAllRolls()
        {
            var mock = new MockRandomizer(3, 5);

            mock.RollOnce(6);
            mock.RollTensD10();

            Assert.Equal(2, mock.DetailedRandResults.Count);
            Assert.Equal(RandResultKind.Normal, mock.DetailedRandResults[0].Kind);
            Assert.Equal(6, mock.DetailedRandResults[0].Sides);
            Assert.Equal(3, mock.DetailedRandResults[0].Value);
            Assert.Equal(RandResultKind.TensD10, mock.DetailedRandResults[1].Kind);
            Assert.Equal(10, mock.DetailedRandResults[1].Sides);
            Assert.Equal(50, mock.DetailedRandResults[1].Value);
        }

        [Fact]
        public void TooManyRands_ThrowsException()
        {
            var randomizer = new Randomizer();

            Assert.Throws<TooManyRandsException>(() =>
            {
                randomizer.RollBarabara(Randomizer.UpperLimitRands + 1, 6);
            });
        }
    }
}
