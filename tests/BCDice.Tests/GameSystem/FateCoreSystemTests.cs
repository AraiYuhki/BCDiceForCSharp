using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class FateCoreSystemTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("FateCoreSystem", FateCoreSystem.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsCorrectName()
        {
            Assert.Equal("Fate Core System", FateCoreSystem.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("ふえいとこあしすてむ", FateCoreSystem.Instance.SortKey);
        }

        [Fact]
        public void DF_DefaultDice_RollsFour()
        {
            // 4 dice: 1,2,3,1 -> -1,0,+1,-1 = -1
            var randomizer = new MockRandomizer(1, 2, 3, 1);
            var result = FateCoreSystem.Instance.Eval("DF", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(4DF)", result!.Text);
            Assert.Contains("[-][ ][+][-]", result.Text);
            Assert.Contains("Poor(-1)", result.Text);
        }

        [Fact]
        public void DF_ExplicitDiceCount()
        {
            // 2 dice: 3,3 -> +1,+1 = +2
            var randomizer = new MockRandomizer(3, 3);
            var result = FateCoreSystem.Instance.Eval("2DF", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(2DF)", result!.Text);
            Assert.Contains("[+][+]", result.Text);
            Assert.Contains("Fair(+2)", result.Text);
        }

        [Fact]
        public void DF_WithPositiveModifier()
        {
            // 4 dice: 2,2,2,2 -> 0,0,0,0 = 0, +3 = 3
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(4DF+3)", result!.Text);
            Assert.Contains("[ ][ ][ ][ ]+3", result.Text);
            Assert.Contains("Good(+3)", result.Text);
        }

        [Fact]
        public void DF_WithNegativeModifier()
        {
            // 4 dice: 2,2,2,2 -> 0,0,0,0 = 0, -2 = -2
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF-2", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(4DF-2)", result!.Text);
            Assert.Contains("[ ][ ][ ][ ]-2", result.Text);
            Assert.Contains("Terrible(-2)", result.Text);
        }

        [Fact]
        public void DF_WithTarget_Succeed()
        {
            // 4 dice: 3,3,3,3 -> +1,+1,+1,+1 = +4, target 2
            var randomizer = new MockRandomizer(3, 3, 3, 3);
            var result = FateCoreSystem.Instance.Eval("4DF>=2", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Succeed", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DF_WithTarget_SucceedWithStyle()
        {
            // 4 dice: 3,3,3,3 -> +4, target 1, +4 >= 1+3 = Succeed with Style
            var randomizer = new MockRandomizer(3, 3, 3, 3);
            var result = FateCoreSystem.Instance.Eval("4DF>=1", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Succeed with Style", result!.Text);
            Assert.True(result.IsSuccess);
            Assert.True(result.IsCritical);
        }

        [Fact]
        public void DF_WithTarget_Tie()
        {
            // 4 dice: 2,2,2,2 -> 0, target 0
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF>=0", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Tie(+0)", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DF_WithTarget_SucceedPlusOne()
        {
            // 4 dice: 2,2,2,3 -> 0,0,0,+1 = +1, target 0
            var randomizer = new MockRandomizer(2, 2, 2, 3);
            var result = FateCoreSystem.Instance.Eval("4DF>=0", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Succeed(+1)", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void DF_WithTarget_Fail()
        {
            // 4 dice: 1,1,1,1 -> -1,-1,-1,-1 = -4, target 0
            var randomizer = new MockRandomizer(1, 1, 1, 1);
            var result = FateCoreSystem.Instance.Eval("4DF>=0", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Fail", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void DF_WithModifierAndTarget()
        {
            // 4 dice: 2,2,2,2 -> 0 +3 = 3, target 2
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+3>=2", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(4DF+3>=2)", result!.Text);
            Assert.Contains("Succeed(+1)", result.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ResultLadder_Legendary()
        {
            // 4 dice: 3,3,3,3 -> +4 +4 = +8
            var randomizer = new MockRandomizer(3, 3, 3, 3);
            var result = FateCoreSystem.Instance.Eval("4DF+4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Legendary(+8)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Epic()
        {
            // 4 dice: 3,3,3,2 -> +3 +4 = +7
            var randomizer = new MockRandomizer(3, 3, 3, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Epic(+7)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Fantastic()
        {
            // 4 dice: 3,3,2,2 -> +2 +4 = +6
            var randomizer = new MockRandomizer(3, 3, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Fantastic(+6)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Superb()
        {
            // 4 dice: 3,2,2,2 -> +1 +4 = +5
            var randomizer = new MockRandomizer(3, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Superb(+5)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Great()
        {
            // 4 dice: 2,2,2,2 -> 0 +4 = +4
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF+4", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Great(+4)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Mediocre()
        {
            // 4 dice: 2,2,2,2 -> 0
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Mediocre(+0)", result!.Text);
        }

        [Fact]
        public void ResultLadder_Average()
        {
            // 4 dice: 3,2,2,2 -> +1
            var randomizer = new MockRandomizer(3, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("4DF", randomizer);

            Assert.NotNull(result);
            Assert.Contains("Average(+1)", result!.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = FateCoreSystem.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }

        [Fact]
        public void Eval_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(2, 2, 2, 2);
            var result = FateCoreSystem.Instance.Eval("df", randomizer);

            Assert.NotNull(result);
            Assert.Contains("4DF", result!.Text);
        }
    }
}
