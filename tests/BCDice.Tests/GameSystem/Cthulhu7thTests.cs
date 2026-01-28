using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class Cthulhu7thTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("Cthulhu7th", Cthulhu7th.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("新クトゥルフ神話TRPG", Cthulhu7th.Instance.Name);
        }

        [Fact]
        public void CC_SimpleRoll_ReturnsResult()
        {
            // CC without target value - just 1D100
            var randomizer = new MockRandomizer(50);
            var result = Cthulhu7th.Instance.Eval("CC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(1D100)", result!.Text);
            Assert.Contains("50", result.Text);
        }

        [Fact]
        public void CC_WithTargetValue_Critical()
        {
            // Roll 1 should be critical
            // tens=0 (RollTensD10 with 10), ones=1 (RollOnce(10) with 1) => total=1
            var randomizer = new MockRandomizer(10, 1);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("クリティカル", result!.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CC_WithTargetValue_Fumble()
        {
            // Roll 100 should be fumble
            // tens=0 (RollTensD10 with 10), ones=0 (RollOnce(10) with 10) => total=100
            var randomizer = new MockRandomizer(10, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result!.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void CC_WithTargetValue_RegularSuccess()
        {
            // Roll 40 with target 50 should be regular success
            // tens=40 (RollTensD10 with 4), ones=0 (RollOnce(10) with 10) => total=40
            var randomizer = new MockRandomizer(4, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("レギュラー成功", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CC_WithTargetValue_HardSuccess()
        {
            // Roll 20 with target 50 should be hard success (20 <= 50/2=25)
            // tens=20 (RollTensD10 with 2), ones=0 (RollOnce(10) with 10) => total=20
            var randomizer = new MockRandomizer(2, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ハード成功", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CC_WithTargetValue_ExtremeSuccess()
        {
            // Roll 10 with target 50 should be extreme success (10 <= 50/5=10)
            // tens=10 (RollTensD10 with 1), ones=0 (RollOnce(10) with 10) => total=10
            var randomizer = new MockRandomizer(1, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("イクストリーム成功", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CC_WithTargetValue_Failure()
        {
            // Roll 60 with target 50 should be failure
            // tens=60 (RollTensD10 with 6), ones=0 (RollOnce(10) with 10) => total=60
            var randomizer = new MockRandomizer(6, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void CC_WithBonusDice_SelectsMinimum()
        {
            // Bonus dice +1: should roll 2 tens dice and select minimum
            // tens1=30 (with 3), tens2=50 (with 5), ones=5 => 35 vs 55 => min=35
            var randomizer = new MockRandomizer(3, 5, 5);
            var result = Cthulhu7th.Instance.Eval("CC+1<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("35, 55", result!.Text);
            Assert.Contains("35", result.Text);
        }

        [Fact]
        public void CC_WithPenaltyDice_SelectsMaximum()
        {
            // Penalty dice -1: should roll 2 tens dice and select maximum
            // tens1=30 (with 3), tens2=50 (with 5), ones=5 => 35 vs 55 => max=55
            var randomizer = new MockRandomizer(3, 5, 5);
            var result = Cthulhu7th.Instance.Eval("CC-1<=50", randomizer);

            Assert.NotNull(result);
            Assert.Contains("35, 55", result!.Text);
            Assert.Contains("55", result.Text);
            Assert.Contains("失敗", result.Text);
        }

        [Fact]
        public void CC_WithDifficultyLevelH_HalvesTarget()
        {
            // CC<=70H should halve target to 35
            // Roll 30 should be regular success (30 <= 35)
            // tens=30 (with 3), ones=0 (with 10) => total=30
            var randomizer = new MockRandomizer(3, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=70H", randomizer);

            Assert.NotNull(result);
            Assert.Contains("<=35", result!.Text);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void CC_WithDifficultyLevelE_DividesByFive()
        {
            // CC<=70E should divide target by 5 to 14
            // Roll 10 should be success (10 <= 14)
            // tens=10 (with 1), ones=0 (with 10) => total=10
            var randomizer = new MockRandomizer(1, 10);
            var result = Cthulhu7th.Instance.Eval("CC<=70E", randomizer);

            Assert.NotNull(result);
            Assert.Contains("<=14", result!.Text);
            Assert.Contains("成功", result.Text);
        }

        [Fact]
        public void CBR_BothSuccess()
        {
            // CBR(50,40): roll 30 should succeed both
            var randomizer = new MockRandomizer(30);
            var result = Cthulhu7th.Instance.Eval("CBR(50,40)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void CBR_PartialSuccess()
        {
            // CBR(50,30): roll 40 should succeed first, fail second
            var randomizer = new MockRandomizer(40);
            var result = Cthulhu7th.Instance.Eval("CBR(50,30)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("部分的成功", result!.Text);
        }

        [Fact]
        public void CBR_BothFailure()
        {
            // CBR(30,20): roll 50 should fail both
            var randomizer = new MockRandomizer(50);
            var result = Cthulhu7th.Instance.Eval("CBR(30,20)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void BMR_ReturnsTableResult()
        {
            // BMR: Roll 1D10 for table, 1D10 for duration
            var randomizer = new MockRandomizer(1, 5);
            var result = Cthulhu7th.Instance.Eval("BMR", randomizer);

            Assert.NotNull(result);
            Assert.Contains("狂気の発作（リアルタイム）", result!.Text);
            Assert.Contains("健忘症", result.Text);
            Assert.Contains("5ラウンド", result.Text);
        }

        [Fact]
        public void BMS_ReturnsTableResult()
        {
            // BMS: Roll 1D10 for table, 1D10 for duration
            var randomizer = new MockRandomizer(2, 8);
            var result = Cthulhu7th.Instance.Eval("BMS", randomizer);

            Assert.NotNull(result);
            Assert.Contains("狂気の発作（サマリー）", result!.Text);
            Assert.Contains("盗難", result.Text);
            Assert.Contains("8時間", result.Text);
        }

        [Fact]
        public void FCL_ReturnsTableResult()
        {
            var randomizer = new MockRandomizer(3);
            var result = Cthulhu7th.Instance.Eval("FCL", randomizer);

            Assert.NotNull(result);
            Assert.Contains("キャスティング・ロール失敗(小)表", result!.Text);
            Assert.Contains("強風", result.Text);
        }

        [Fact]
        public void FCM_ReturnsTableResult()
        {
            var randomizer = new MockRandomizer(2);
            var result = Cthulhu7th.Instance.Eval("FCM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("キャスティング・ロール失敗(大)表", result!.Text);
            Assert.Contains("電撃", result.Text);
        }

        [Fact]
        public void PH_ReturnsPhobiaTableResult()
        {
            var randomizer = new MockRandomizer(1);
            var result = Cthulhu7th.Instance.Eval("PH", randomizer);

            Assert.NotNull(result);
            Assert.Contains("恐怖症表", result!.Text);
            Assert.Contains("入浴恐怖症", result.Text);
        }

        [Fact]
        public void MA_ReturnsManiaTableResult()
        {
            var randomizer = new MockRandomizer(1);
            var result = Cthulhu7th.Instance.Eval("MA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("マニア表", result!.Text);
            Assert.Contains("洗浄マニア", result.Text);
        }

        [Fact]
        public void FAR_BasicFire_ReturnsResult()
        {
            // FAR(10,70,98): 10 bullets, skill 70, jam on 98+
            // Need to mock multiple rolls for full auto
            // First shot: tens=50 (with 5), ones=0 (with 10) => total=50 (regular success)
            var randomizer = new MockRandomizer(
                5, 10,    // First shot: 50
                6, 10,    // Second shot: 60 (failure)
                7, 10,    // Additional rolls
                8, 10
            );
            var result = Cthulhu7th.Instance.Eval("FAR(10,70,98)", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ボーナス・ペナルティダイス[0]", result!.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = Cthulhu7th.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }
    }
}
