using BCDice.Core;
using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class SwordWorld2_5Tests
    {
        [Fact]
        public void Id_ReturnsSwordWorld()
        {
            Assert.Equal("SwordWorld2.5", SwordWorld2_5.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Contains("ソードワールド", SwordWorld2_5.Instance.Name);
        }

        [Fact]
        public void D66SortType_ReturnsAscending()
        {
            Assert.Equal(D66SortType.Ascending, SwordWorld2_5.Instance.D66SortType);
        }

        [Fact]
        public void Eval_RatingK20_ReturnsResult()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll 7
            var result = SwordWorld2_5.Instance.Eval("K20", randomizer);

            Assert.NotNull(result);
            Assert.Contains("K20", result.Text);
            Assert.Contains("[7]", result.Text);
        }

        [Fact]
        public void Eval_RatingK20Critical_RollsAgain()
        {
            var randomizer = new MockRandomizer(5, 5, 3, 2); // First roll 10 (critical), then 5
            var result = SwordWorld2_5.Instance.Eval("K20@10", randomizer);

            Assert.NotNull(result);
            Assert.Contains("[10]", result.Text);
            Assert.Contains("[5]", result.Text);
        }

        [Fact]
        public void Eval_RatingWithModifier_AddsModifier()
        {
            var randomizer = new MockRandomizer(3, 4); // Roll 7
            var result = SwordWorld2_5.Instance.Eval("K20+5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("+5", result.Text);
        }

        [Fact]
        public void Eval_RatingWithCriticalValue_UsesCriticalValue()
        {
            var randomizer = new MockRandomizer(4, 4, 2, 3); // Roll 8 (critical at 8), then 5
            var result = SwordWorld2_5.Instance.Eval("K20@8", randomizer);

            Assert.NotNull(result);
            Assert.Contains("@8", result.Text);
        }

        [Fact]
        public void Eval_RatingPinzoro_ReturnsZero()
        {
            var randomizer = new MockRandomizer(1, 1); // Roll 2 (pinzoro)
            var result = SwordWorld2_5.Instance.Eval("K20", randomizer);

            Assert.NotNull(result);
            Assert.Contains("[2]:0", result.Text);
        }

        [Fact]
        public void Eval_SecretRating_IsSecret()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = SwordWorld2_5.Instance.Eval("SK20", randomizer);

            Assert.NotNull(result);
            Assert.True(result.IsSecret);
        }

        [Fact]
        public void Eval_CommonCommand_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = SwordWorld2_5.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result.Text);
        }
    }
}
