using BCDice.GameSystem;
using BCDice.Tests.Core;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    public class KanColleTests
    {
        [Fact]
        public void Id_ReturnsCorrectId()
        {
            Assert.Equal("KanColle", KanColle.Instance.Id);
        }

        [Fact]
        public void Name_ReturnsJapaneseName()
        {
            Assert.Equal("艦これRPG", KanColle.Instance.Name);
        }

        [Fact]
        public void SortKey_ReturnsHiragana()
        {
            Assert.Equal("かんこれRPG", KanColle.Instance.SortKey);
        }

        [Fact]
        public void D66SortType_IsAscending()
        {
            Assert.Equal(BCDice.Core.D66SortType.Ascending, KanColle.Instance.D66SortType);
        }

        [Fact]
        public void SortBarabaraDice_IsTrue()
        {
            Assert.True(KanColle.Instance.SortBarabaraDice);
        }

        [Fact]
        public void Eval_2D6_Fumble_WhenDoubleOne()
        {
            // 1+1=2 should be fumble
            var randomizer = new MockRandomizer(1, 1);
            var result = KanColle.Instance.Eval("2D6>=7", randomizer);

            Assert.NotNull(result);
            Assert.Contains("ファンブル", result!.Text);
            Assert.True(result.IsFumble);
            Assert.True(result.IsFailure);
        }

        [Fact]
        public void Eval_2D6_Special_WhenDoubleSix()
        {
            // 6+6=12 should be special
            var randomizer = new MockRandomizer(6, 6);
            var result = KanColle.Instance.Eval("2D6>=7", randomizer);

            Assert.NotNull(result);
            Assert.Contains("スペシャル", result!.Text);
            Assert.True(result.IsCritical);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void Eval_2D6_NormalSuccess()
        {
            // 4+4=8 >= 7 should succeed without special
            var randomizer = new MockRandomizer(4, 4);
            var result = KanColle.Instance.Eval("2D6>=7", randomizer);

            Assert.NotNull(result);
            Assert.Contains("成功", result!.Text);
            Assert.DoesNotContain("スペシャル", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
        }

        [Fact]
        public void Eval_2D6_NormalFailure()
        {
            // 2+3=5 < 7 should fail without fumble
            var randomizer = new MockRandomizer(2, 3);
            var result = KanColle.Instance.Eval("2D6>=7", randomizer);

            Assert.NotNull(result);
            Assert.Contains("失敗", result!.Text);
            Assert.DoesNotContain("スペシャル", result.Text);
            Assert.DoesNotContain("ファンブル", result.Text);
        }

        [Fact]
        public void Eval_ET_ReturnsEmotionTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("ET", randomizer);

            Assert.NotNull(result);
            Assert.Contains("感情表", result!.Text);
            Assert.Contains("かわいい", result.Text);
        }

        [Fact]
        public void Eval_ACT_ReturnsAccidentTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("ACT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("アクシデント表", result!.Text);
            Assert.Contains("何もなし", result.Text);
        }

        [Fact]
        public void Eval_DVT_ReturnsDevelopTable()
        {
            var randomizer = new MockRandomizer(3);
            var result = KanColle.Instance.Eval("DVT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("開発表", result!.Text);
            Assert.Contains("装備２種表", result.Text);
        }

        [Fact]
        public void Eval_WP1T_ReturnsEquipment1Table()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("WP1T", randomizer);

            Assert.NotNull(result);
            Assert.Contains("装備１種表", result!.Text);
            Assert.Contains("小口径主砲", result.Text);
        }

        [Fact]
        public void Eval_WP2T_ReturnsEquipment2Table()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("WP2T", randomizer);

            Assert.NotNull(result);
            Assert.Contains("装備２種表", result!.Text);
            Assert.Contains("副砲", result.Text);
        }

        [Fact]
        public void Eval_WP3T_ReturnsEquipment3Table()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("WP3T", randomizer);

            Assert.NotNull(result);
            Assert.Contains("装備３種表", result!.Text);
            Assert.Contains("艦上爆撃機", result.Text);
        }

        [Fact]
        public void Eval_WP4T_ReturnsEquipment4Table()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("WP4T", randomizer);

            Assert.NotNull(result);
            Assert.Contains("装備４種表", result!.Text);
            Assert.Contains("彗星", result.Text);
        }

        [Fact]
        public void Eval_ITT_ReturnsItemTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("ITT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("アイテム表", result!.Text);
            Assert.Contains("アイス", result.Text);
        }

        [Fact]
        public void Eval_MHT_ReturnsTargetTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("MHT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("目標表", result!.Text);
            Assert.Contains("航行序列", result.Text);
        }

        [Fact]
        public void Eval_SNT_ReturnsResultTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("SNT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("戦果表", result!.Text);
            Assert.Contains("燃料", result.Text);
        }

        [Fact]
        public void Eval_SNZ_ReturnsBattlefieldTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("SNZ", randomizer);

            Assert.NotNull(result);
            Assert.Contains("戦場表", result!.Text);
            Assert.Contains("同航戦", result.Text);
        }

        [Fact]
        public void Eval_RNT_ReturnsBerserkTable()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("RNT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("暴走表", result!.Text);
            Assert.Contains("妄想", result.Text);
        }

        [Fact]
        public void Eval_EVNT_ReturnsDailyEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVNT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("日常イベント表", result!.Text);
            Assert.Contains("海軍カレー", result.Text);
        }

        [Fact]
        public void Eval_EVKT_ReturnsInteractionEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVKT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("交流イベント表", result!.Text);
            Assert.Contains("深夜のガールズトーク", result.Text);
        }

        [Fact]
        public void Eval_EVAT_ReturnsPlayEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVAT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("遊びイベント表", result!.Text);
            Assert.Contains("大会開催", result.Text);
        }

        [Fact]
        public void Eval_EVET_ReturnsTrainingEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVET", randomizer);

            Assert.NotNull(result);
            Assert.Contains("演習イベント表", result!.Text);
            Assert.Contains("砲撃演習", result.Text);
        }

        [Fact]
        public void Eval_EVENT_ReturnsExpeditionEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVENT", randomizer);

            Assert.NotNull(result);
            Assert.Contains("遠征イベント表", result!.Text);
            Assert.Contains("海上護衛任務", result.Text);
        }

        [Fact]
        public void Eval_EVST_ReturnsOperationEventTable()
        {
            // 2D6 roll: 3+4=7
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("EVST", randomizer);

            Assert.NotNull(result);
            Assert.Contains("作戦イベント表", result!.Text);
            Assert.Contains("作戦会議", result.Text);
        }

        [Fact]
        public void Eval_DVTM_Dice1_Equipment1Table()
        {
            // First dice=1 -> WP1T, second dice=3 -> item 3
            var randomizer = new MockRandomizer(1, 3);
            var result = KanColle.Instance.Eval("DVTM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("開発表（一括）", result!.Text);
            Assert.Contains("装備１種表", result.Text);
            Assert.Contains("中口径主砲", result.Text);
        }

        [Fact]
        public void Eval_DVTM_Dice3_Equipment2Table()
        {
            // First dice=3 -> WP2T, second dice=2 -> item 2
            var randomizer = new MockRandomizer(3, 2);
            var result = KanColle.Instance.Eval("DVTM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("開発表（一括）", result!.Text);
            Assert.Contains("装備２種表", result.Text);
            Assert.Contains("８ｃｍ高角砲", result.Text);
        }

        [Fact]
        public void Eval_DVTM_Dice5_Equipment3Table()
        {
            // First dice=5 -> WP3T, second dice=4 -> item 4
            var randomizer = new MockRandomizer(5, 4);
            var result = KanColle.Instance.Eval("DVTM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("開発表（一括）", result!.Text);
            Assert.Contains("装備３種表", result.Text);
            Assert.Contains("偵察機", result.Text);
        }

        [Fact]
        public void Eval_DVTM_Dice6_Equipment4Table()
        {
            // First dice=6 -> WP4T, second dice=5 -> item 5
            var randomizer = new MockRandomizer(6, 5);
            var result = KanColle.Instance.Eval("DVTM", randomizer);

            Assert.NotNull(result);
            Assert.Contains("開発表（一括）", result!.Text);
            Assert.Contains("装備４種表", result.Text);
            Assert.Contains("魚雷", result.Text);
        }

        [Fact]
        public void Eval_CommonCommand_2D6_Works()
        {
            var randomizer = new MockRandomizer(3, 4);
            var result = KanColle.Instance.Eval("2D6", randomizer);

            Assert.NotNull(result);
            Assert.Contains("7", result!.Text);
        }

        [Fact]
        public void Eval_D66_Works()
        {
            // D66 with Ascending sort: lower die is tens place
            // Rolls 4, 2 -> sorted to 2, 4 -> D66 = 24
            var randomizer = new MockRandomizer(4, 2);
            var result = KanColle.Instance.Eval("D66", randomizer);

            Assert.NotNull(result);
            Assert.Contains("24", result!.Text);
        }

        [Fact]
        public void Eval_CaseInsensitive()
        {
            var randomizer = new MockRandomizer(1);
            var result = KanColle.Instance.Eval("et", randomizer);

            Assert.NotNull(result);
            Assert.Contains("感情表", result!.Text);
        }
    }
}
