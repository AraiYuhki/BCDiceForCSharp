using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class StellarKnightsTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("StellarKnights", StellarKnights.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("銀剣のステラナイツ", StellarKnights.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("きんけんのすてらないつ", StellarKnights.Instance.SortKey);
        }

        [Fact]
        public void D66SortType_IsNoSort()
        {
            Assert.Equal(BCDice.Core.D66SortType.NoSort, StellarKnights.Instance.D66SortType);
        }

        [Fact]
        public void SortBarabaraDice_IsTrue()
        {
            Assert.True(StellarKnights.Instance.SortBarabaraDice);
        }

        [Fact]
        public void SK_BasicRoll_ReturnsDice()
        {
            // 4SK: Roll 4 dice
            var randomizer = new MockRandomizer(3, 5, 2, 6);
            var result = StellarKnights.Instance.Eval("4SK", randomizer);

            Assert.NotNull(result);
            Assert.Contains("(4SK)", result!.Text);
            Assert.Contains("2,3,5,6", result.Text); // Sorted
        }

        [Fact]
        public void SK_WithDefence_CountsSuccesses()
        {
            // 5SK3: Roll 5 dice, count successes (>= 3)
            // Rolls: 1, 2, 3, 4, 5 -> successes: 3, 4, 5 = 3 successes
            var randomizer = new MockRandomizer(1, 2, 3, 4, 5);
            var result = StellarKnights.Instance.Eval("5SK3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数: 3", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void SK_WithDefence_NoSuccess()
        {
            // 3SK5: Roll 3 dice, count successes (>= 5)
            // Rolls: 1, 2, 3 -> no successes
            var randomizer = new MockRandomizer(1, 2, 3);
            var result = StellarKnights.Instance.Eval("3SK5", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数: 0", result!.Text);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void SK_WithDiceChange_ChangesValues()
        {
            // 3SK,1>6: Roll 3 dice, change 1s to 6s
            // Rolls: 1, 3, 1 -> becomes 6, 3, 6 = [3,6,6]
            var randomizer = new MockRandomizer(1, 3, 1);
            var result = StellarKnights.Instance.Eval("3SK,1>6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("[3,6,6]", result!.Text);
        }

        [Fact]
        public void SK_WithDefenceAndDiceChange_Works()
        {
            // 4SK4,1>6,2>6: Roll 4 dice, change 1->6 and 2->6, count successes >= 4
            // Rolls: 1, 2, 3, 4 -> becomes 6, 6, 3, 4 -> successes: 4, 6, 6 = 3
            var randomizer = new MockRandomizer(1, 2, 3, 4);
            var result = StellarKnights.Instance.Eval("4SK4,1>6,2>6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功数: 3", result!.Text);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void SK_ZeroDice_ReturnsError()
        {
            var randomizer = new MockRandomizer();
            var result = StellarKnights.Instance.Eval("0SK", randomizer);

            // Zero dice returns error message
            Assert.NotNull(result);
            Assert.Contains("ダイスが 0 個です", result!.Text);
        }

        [Fact]
        public void TT_ReturnsTopicTable()
        {
            // D66: 11 -> (0,0) -> "未来"
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("TT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("お題表", result!.Text);
            Assert.Contains("未来", result.Text);
        }

        [Fact]
        public void TT_DifferentRoll()
        {
            // D66: 32 -> (2,1) -> row 3, col 2 -> "決意"
            var randomizer = new MockRandomizer(3, 2);
            var result = StellarKnights.Instance.Eval("TT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("お題表", result!.Text);
            Assert.Contains("決意", result.Text);
        }

        [Fact]
        public void STA_ReturnsSituationTableA()
        {
            var randomizer = new MockRandomizer(1);
            var result = StellarKnights.Instance.Eval("STA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("シチュエーション表A", result!.Text);
            Assert.Contains("朝", result.Text);
        }

        [Fact]
        public void STB_ReturnsSituationTableB()
        {
            // 2D6 = 2+3 = 5 -> カフェテラス
            var randomizer = new MockRandomizer(2, 3);
            var result = StellarKnights.Instance.Eval("STB", randomizer);

            Assert.NotNull(result);
            Assert.Contains("シチュエーション表B", result!.Text);
            Assert.Contains("カフェテラス", result.Text);
        }

        [Fact]
        public void STC_ReturnsSituationTableC()
        {
            // 2D6 = 4+3 = 7 -> おいしいごはん
            var randomizer = new MockRandomizer(4, 3);
            var result = StellarKnights.Instance.Eval("STC", randomizer);

            Assert.NotNull(result);
            Assert.Contains("シチュエーション表C", result!.Text);
            Assert.Contains("おいしいごはん", result.Text);
        }

        [Fact]
        public void GAT_ReturnsOrganizationTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = StellarKnights.Instance.Eval("GAT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("所属組織決定", result!.Text);
            Assert.Contains("アーセルトレイ公立大学", result.Text);
        }

        [Fact]
        public void HOT_ReturnsHopeTable()
        {
            // 2D6 = 1+1 = 2 -> より良き世界
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("HOT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("希望表", result!.Text);
            Assert.Contains("より良き世界", result.Text);
        }

        [Fact]
        public void DET_ReturnsDespairTable()
        {
            // 2D6 = 1+1 = 2 -> 理不尽なる世界
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("DET", randomizer);

            Assert.NotNull(result);
            Assert.Contains("絶望表", result!.Text);
            Assert.Contains("理不尽なる世界", result.Text);
        }

        [Fact]
        public void WIT_ReturnsWishTable()
        {
            // 2D6 = 1+1 = 2 -> 未知の開拓者
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("WIT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("願い事表", result!.Text);
            Assert.Contains("未知の開拓者", result.Text);
        }

        [Fact]
        public void YST_ReturnsYourStoryTable()
        {
            // 2D6 = 1+1 = 2 -> 熟練ステラナイト
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("YST", randomizer);

            Assert.NotNull(result);
            Assert.Contains("あなたの物語表", result!.Text);
            Assert.Contains("熟練ステラナイト", result.Text);
        }

        [Fact]
        public void YSTA_ReturnsAnotherWorldTable()
        {
            // 2D6 = 1+1 = 2 -> 終わりなき戦場
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("YSTA", randomizer);

            Assert.NotNull(result);
            Assert.Contains("あなたの物語表：異世界", result!.Text);
            Assert.Contains("終わりなき戦場", result.Text);
        }

        [Fact]
        public void YSTM_ReturnsMarginaliaTable()
        {
            // 2D6 = 1+1 = 2 -> パブ/カフェー店員
            var randomizer = new MockRandomizer(1, 1);
            var result = StellarKnights.Instance.Eval("YSTM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("あなたの物語表：マルジナリア世界", result!.Text);
            Assert.Contains("パブ/カフェー店員", result.Text);
        }

        [Fact]
        public void ALLS_ReturnsAllSituationTables()
        {
            // STA: 1 -> 朝
            // STB: 1+1=2 -> 教室
            // STC: 1+1=2 -> 未来の話
            var randomizer = new MockRandomizer(1, 1, 1, 1, 1);
            var result = StellarKnights.Instance.Eval("ALLS", randomizer);

            Assert.NotNull(result);
            Assert.Contains("シチュエーション表A", result!.Text);
            Assert.Contains("シチュエーション表B", result.Text);
            Assert.Contains("シチュエーション表C", result.Text);
        }

        [Fact]
        public void PET_ReturnsPersonalityTable()
        {
            // D66 1: 1,1 = index 0 = "可憐"
            // D66 2: 2,3 = index (2-1)*6 + (3-1) = 6+2 = 8 = "惚れやすい"
            var randomizer = new MockRandomizer(1, 1, 2, 3);
            var result = StellarKnights.Instance.Eval("PET", randomizer);

            Assert.NotNull(result);
            Assert.Contains("性格表", result!.Text);
            Assert.Contains("可憐", result.Text);
            Assert.Contains("惚れやすい", result.Text);
            Assert.Contains("にして", result.Text);
        }

        [Fact]
        public void FT_DefaultCount_RollsFiveTimes()
        {
            // 5 x D66 rolls
            var randomizer = new MockRandomizer(
                1, 1,  // 出会い
                1, 2,  // 水族館
                1, 3,  // 動物園
                1, 4,  // 絵本
                1, 5   // 童話
            );
            var result = StellarKnights.Instance.Eval("FT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("フラグメント表", result!.Text);
            Assert.Contains("出会い", result.Text);
            Assert.Contains("水族館", result.Text);
            Assert.Contains("動物園", result.Text);
            Assert.Contains("絵本", result.Text);
            Assert.Contains("童話", result.Text);
        }

        [Fact]
        public void FT3_RollsThreeTimes()
        {
            // 3 x D66 rolls
            var randomizer = new MockRandomizer(
                1, 1,  // 出会い
                1, 2,  // 水族館
                1, 3   // 動物園
            );
            var result = StellarKnights.Instance.Eval("FT3", randomizer);

            Assert.NotNull(result);
            Assert.Contains("フラグメント表", result!.Text);
            Assert.Contains("出会い", result.Text);
            Assert.Contains("水族館", result.Text);
            Assert.Contains("動物園", result.Text);
        }

        [Fact]
        public void CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = StellarKnights.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }

        [Fact]
        public void Eval_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(1);
            var result = StellarKnights.Instance.Eval("sta", randomizer);

            Assert.NotNull(result);
            Assert.Contains("シチュエーション表A", result!.Text);
        }
    }
}
