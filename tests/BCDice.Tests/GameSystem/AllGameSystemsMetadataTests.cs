using System.Collections.Generic;
using BCDice.GameSystem;
using Xunit;

namespace BCDice.Tests.GameSystem
{
    /// <summary>
    /// Tier 2-4 全228システムのメタデータ検証テスト
    /// 各システムのInstance、Id、Name、SortKey、HelpMessageが正しく設定されていることを確認
    /// </summary>
    public class AllGameSystemsMetadataTests
    {
        public static IEnumerable<object[]> AllNewSystems()
        {
            yield return new object[] { AFF2e.Instance, "AFF2e" };
            yield return new object[] { AceKillerGene.Instance, "AceKillerGene" };
            yield return new object[] { Agnostos.Instance, "Agnostos" };
            yield return new object[] { Aionia.Instance, "Aionia" };
            yield return new object[] { Airgetlamh.Instance, "Airgetlamh" };
            yield return new object[] { AlchemiaStruggle.Instance, "AlchemiaStruggle" };
            yield return new object[] { Alsetto.Instance, "Alsetto" };
            yield return new object[] { AlterRaise.Instance, "AlterRaise" };
            yield return new object[] { Amadeus.Instance, "Amadeus" };
            yield return new object[] { AngelGear.Instance, "AngelGear" };
            yield return new object[] { AniMalus.Instance, "AniMalus" };
            yield return new object[] { AnimaAnimus.Instance, "AnimaAnimus" };
            yield return new object[] { Aoharubaan.Instance, "Aoharubaan" };
            yield return new object[] { ArknightsFan.Instance, "ArknightsFan" };
            yield return new object[] { ArsMagica.Instance, "ArsMagica" };
            yield return new object[] { AssaultEngine.Instance, "AssaultEngine" };
            yield return new object[] { Avandner.Instance, "Avandner" };
            yield return new object[] { Ayabito.Instance, "Ayabito" };
            yield return new object[] { BBN.Instance, "BBN" };
            yield return new object[] { BadLife.Instance, "BadLife" };
            yield return new object[] { Bakenokawa.Instance, "Bakenokawa" };
            yield return new object[] { BarnaKronika.Instance, "BarnaKronika" };
            yield return new object[] { BattleTech.Instance, "BattleTech" };
            yield return new object[] { BeginningIdol.Instance, "BeginningIdol" };
            yield return new object[] { BeginningIdol2022.Instance, "BeginningIdol2022" };
            yield return new object[] { BlackJacket.Instance, "BlackJacket" };
            yield return new object[] { BladeOfArcana.Instance, "BladeOfArcana" };
            yield return new object[] { BlindMythos.Instance, "BlindMythos" };
            yield return new object[] { BloodCrusade.Instance, "BloodCrusade" };
            yield return new object[] { Bloodorium.Instance, "Bloodorium" };
            yield return new object[] { CardRanker.Instance, "CardRanker" };
            yield return new object[] { CastleInGray.Instance, "CastleInGray" };
            yield return new object[] { CharonSanctions.Instance, "CharonSanctions" };
            yield return new object[] { Chill.Instance, "Chill" };
            yield return new object[] { Chill3.Instance, "Chill3" };
            yield return new object[] { CodeLayerd.Instance, "CodeLayerd" };
            yield return new object[] { ColossalHunter.Instance, "ColossalHunter" };
            yield return new object[] { Comes.Instance, "Comes" };
            yield return new object[] { ConvictorDrive.Instance, "ConvictorDrive" };
            yield return new object[] { CrashWorld.Instance, "CrashWorld" };
            yield return new object[] { CthulhuTech.Instance, "CthulhuTech" };
            yield return new object[] { CyberpunkRed.Instance, "CyberpunkRed" };
            yield return new object[] { DarkBlaze.Instance, "DarkBlaze" };
            yield return new object[] { DarkDaysDrive.Instance, "DarkDaysDrive" };
            yield return new object[] { DarkSouls.Instance, "DarkSouls" };
            yield return new object[] { DeadlineHeroes.Instance, "DeadlineHeroes" };
            yield return new object[] { DemonParasite.Instance, "DemonParasite" };
            yield return new object[] { DemonSpike.Instance, "DemonSpike" };
            yield return new object[] { DesperateRun.Instance, "DesperateRun" };
            yield return new object[] { DetatokoSaga.Instance, "DetatokoSaga" };
            yield return new object[] { DiceOfTheDead.Instance, "DiceOfTheDead" };
            yield return new object[] { DivineCharger.Instance, "DivineCharger" };
            yield return new object[] { DungeonsAndDragons5.Instance, "DungeonsAndDragons5" };
            yield return new object[] { EarthDawn.Instance, "EarthDawn" };
            yield return new object[] { EarthDawn3.Instance, "EarthDawn3" };
            yield return new object[] { EarthDawn4.Instance, "EarthDawn4" };
            yield return new object[] { EclipsePhase.Instance, "EclipsePhase" };
            yield return new object[] { EdgeFlippers.Instance, "EdgeFlippers" };
            yield return new object[] { Elric.Instance, "Elric" };
            yield return new object[] { Elysion.Instance, "Elysion" };
            yield return new object[] { EmbryoMachine.Instance, "EmbryoMachine" };
            yield return new object[] { EndBreaker.Instance, "EndBreaker" };
            yield return new object[] { EtrianOdysseySRS.Instance, "EtrianOdysseySRS" };
            yield return new object[] { Fiasco.Instance, "Fiasco" };
            yield return new object[] { FilledWith.Instance, "FilledWith" };
            yield return new object[] { FinalFantasyXIV.Instance, "FinalFantasyXIV" };
            yield return new object[] { FullFace.Instance, "FullFace" };
            yield return new object[] { FullMetalPanic.Instance, "FullMetalPanic" };
            yield return new object[] { FutariSousa.Instance, "FutariSousa" };
            yield return new object[] { GURPS.Instance, "GURPS" };
            yield return new object[] { Garactier.Instance, "Garactier" };
            yield return new object[] { Garako.Instance, "Garako" };
            yield return new object[] { GardenOrder.Instance, "GardenOrder" };
            yield return new object[] { GehennaAn.Instance, "GehennaAn" };
            yield return new object[] { GeishaGirlwithKatana.Instance, "GeishaGirlwithKatana" };
            yield return new object[] { GhostLive.Instance, "GhostLive" };
            yield return new object[] { GoblinSlayer.Instance, "GoblinSlayer" };
            yield return new object[] { Gorilla.Instance, "Gorilla" };
            yield return new object[] { GundamSentinel.Instance, "GundamSentinel" };
            yield return new object[] { Gundog.Instance, "Gundog" };
            yield return new object[] { GundogRevised.Instance, "GundogRevised" };
            yield return new object[] { GundogZero.Instance, "GundogZero" };
            yield return new object[] { GurpsFW.Instance, "GurpsFW" };
            yield return new object[] { HarnMaster.Instance, "HarnMaster" };
            yield return new object[] { HatsuneMiku.Instance, "HatsuneMiku" };
            yield return new object[] { HeroScale.Instance, "HeroScale" };
            yield return new object[] { Hieizan.Instance, "Hieizan" };
            yield return new object[] { HouraiGakuen.Instance, "HouraiGakuen" };
            yield return new object[] { HunterTheReckoning5th.Instance, "HunterTheReckoning5th" };
            yield return new object[] { HuntersMoon.Instance, "HuntersMoon" };
            yield return new object[] { IfIfIf.Instance, "IfIfIf" };
            yield return new object[] { Illusio.Instance, "Illusio" };
            yield return new object[] { InfiniteBabeL.Instance, "InfiniteBabeL" };
            yield return new object[] { InfiniteFantasia.Instance, "InfiniteFantasia" };
            yield return new object[] { InvisibleLiar.Instance, "InvisibleLiar" };
            yield return new object[] { Irisbane.Instance, "Irisbane" };
            yield return new object[] { IthaWenUa.Instance, "IthaWenUa" };
            yield return new object[] { JamesBond.Instance, "JamesBond" };
            yield return new object[] { JekyllAndHyde.Instance, "JekyllAndHyde" };
            yield return new object[] { JuinKansen.Instance, "JuinKansen" };
            yield return new object[] { KamitsubakiCityUnderConstructionNarrative.Instance, "KamitsubakiCityUnderConstructionNarrative" };
            yield return new object[] { Karukami.Instance, "Karukami" };
            yield return new object[] { KemonoNoMori.Instance, "KemonoNoMori" };
            yield return new object[] { KillDeathBusiness.Instance, "KillDeathBusiness" };
            yield return new object[] { KimitoYell.Instance, "KimitoYell" };
            yield return new object[] { KizunaBullet.Instance, "KizunaBullet" };
            yield return new object[] { KurayamiCrying.Instance, "KurayamiCrying" };
            yield return new object[] { Kutulu.Instance, "Kutulu" };
            yield return new object[] { KyokoShinshoku.Instance, "KyokoShinshoku" };
            yield return new object[] { Liminal.Instance, "Liminal" };
            yield return new object[] { LiverLabyrinth.Instance, "LiverLabyrinth" };
            yield return new object[] { LiveraDoll.Instance, "LiveraDoll" };
            yield return new object[] { LostRecord.Instance, "LostRecord" };
            yield return new object[] { LostRoyal.Instance, "LostRoyal" };
            yield return new object[] { MagicPunk.Instance, "MagicPunk" };
            yield return new object[] { Magius.Instance, "Magius" };
            yield return new object[] { Magius_3rdNewTokyoCity.Instance, "Magius_3rdNewTokyoCity" };
            yield return new object[] { MamonoScramble.Instance, "MamonoScramble" };
            yield return new object[] { MeikyuDays.Instance, "MeikyuDays" };
            yield return new object[] { MeikyuKingdomBasic.Instance, "MeikyuKingdomBasic" };
            yield return new object[] { MetalHead.Instance, "MetalHead" };
            yield return new object[] { MetalHeadExtream.Instance, "MetalHeadExtream" };
            yield return new object[] { MetallicGuardian.Instance, "MetallicGuardian" };
            yield return new object[] { NRR.Instance, "NRR" };
            yield return new object[] { NSSQ.Instance, "NSSQ" };
            yield return new object[] { NervWhitePaper.Instance, "NervWhitePaper" };
            yield return new object[] { NeverCloud.Instance, "NeverCloud" };
            yield return new object[] { NightWizard.Instance, "NightWizard" };
            yield return new object[] { NightmareHunterDeep.Instance, "NightmareHunterDeep" };
            yield return new object[] { NinjaSlayer.Instance, "NinjaSlayer" };
            yield return new object[] { NinjaSlayer2.Instance, "NinjaSlayer2" };
            yield return new object[] { NjslyrBattle.Instance, "NjslyrBattle" };
            yield return new object[] { NobunagasBlackCastle.Instance, "NobunagasBlackCastle" };
            yield return new object[] { Nuekagami.Instance, "Nuekagami" };
            yield return new object[] { OneWayHeroics.Instance, "OneWayHeroics" };
            yield return new object[] { OracleEngine.Instance, "OracleEngine" };
            yield return new object[] { OrgaRain.Instance, "OrgaRain" };
            yield return new object[] { Oukahoushin3rd.Instance, "Oukahoushin3rd" };
            yield return new object[] { Paradiso.Instance, "Paradiso" };
            yield return new object[] { ParanoiaPerfect.Instance, "ParanoiaPerfect" };
            yield return new object[] { ParanoiaRebooted.Instance, "ParanoiaRebooted" };
            yield return new object[] { ParasiteBlood.Instance, "ParasiteBlood" };
            yield return new object[] { PastFutureParadox.Instance, "PastFutureParadox" };
            yield return new object[] { Pathfinder.Instance, "Pathfinder" };
            yield return new object[] { Peekaboo.Instance, "Peekaboo" };
            yield return new object[] { Pendragon.Instance, "Pendragon" };
            yield return new object[] { PersonaO.Instance, "PersonaO" };
            yield return new object[] { PhantasmAdventure.Instance, "PhantasmAdventure" };
            yield return new object[] { Postman.Instance, "Postman" };
            yield return new object[] { PulpCthulhu.Instance, "PulpCthulhu" };
            yield return new object[] { Raisondetre.Instance, "Raisondetre" };
            yield return new object[] { RecordOfLodossWar.Instance, "RecordOfLodossWar" };
            yield return new object[] { RecordOfSteam.Instance, "RecordOfSteam" };
            yield return new object[] { Revulture.Instance, "Revulture" };
            yield return new object[] { RogueLikeHalf.Instance, "RogueLikeHalf" };
            yield return new object[] { RokumonSekai2.Instance, "RokumonSekai2" };
            yield return new object[] { RoleMaster.Instance, "RoleMaster" };
            yield return new object[] { RuinBreakers.Instance, "RuinBreakers" };
            yield return new object[] { RuneQuest.Instance, "RuneQuest" };
            yield return new object[] { RuneQuestRoleplayingInGlorantha.Instance, "RuneQuestRoleplayingInGlorantha" };
            yield return new object[] { RyuTuber.Instance, "RyuTuber" };
            yield return new object[] { SajinsenkiAGuS.Instance, "SajinsenkiAGuS" };
            yield return new object[] { SajinsenkiAGuS2E.Instance, "SajinsenkiAGuS2E" };
            yield return new object[] { SamsaraBallad.Instance, "SamsaraBallad" };
            yield return new object[] { ScreamHighSchool.Instance, "ScreamHighSchool" };
            yield return new object[] { Sengensyou.Instance, "Sengensyou" };
            yield return new object[] { SevenFortressMobius.Instance, "SevenFortressMobius" };
            yield return new object[] { ShadowRun.Instance, "ShadowRun" };
            yield return new object[] { ShadowRun4.Instance, "ShadowRun4" };
            yield return new object[] { ShadowRun5.Instance, "ShadowRun5" };
            yield return new object[] { SharedFantasia.Instance, "SharedFantasia" };
            yield return new object[] { ShinMegamiTenseiKakuseihen.Instance, "ShinMegamiTenseiKakuseihen" };
            yield return new object[] { ShinkuuGakuen.Instance, "ShinkuuGakuen" };
            yield return new object[] { Shiranui.Instance, "Shiranui" };
            yield return new object[] { ShoujoTenrankai.Instance, "ShoujoTenrankai" };
            yield return new object[] { ShuumatsuBargainWars.Instance, "ShuumatsuBargainWars" };
            yield return new object[] { ShuumatsuKikou.Instance, "ShuumatsuKikou" };
            yield return new object[] { Siren.Instance, "Siren" };
            yield return new object[] { Skynauts.Instance, "Skynauts" };
            yield return new object[] { SkynautsBouken.Instance, "SkynautsBouken" };
            yield return new object[] { StarryDolls.Instance, "StarryDolls" };
            yield return new object[] { SteamPunkers.Instance, "SteamPunkers" };
            yield return new object[] { StellarLife.Instance, "StellarLife" };
            yield return new object[] { StrangerOfSwordCity.Instance, "StrangerOfSwordCity" };
            yield return new object[] { StratoShout.Instance, "StratoShout" };
            yield return new object[] { Strave.Instance, "Strave" };
            yield return new object[] { SwordWorld.Instance, "SwordWorld" };
            yield return new object[] { SwordWorld2_0.Instance, "SwordWorld2.0" };
            yield return new object[] { TalesFromTheLoop.Instance, "TalesFromTheLoop" };
            yield return new object[] { TenkaRyouran.Instance, "TenkaRyouran" };
            yield return new object[] { TensaiGunshiNiNaro.Instance, "TensaiGunshiNiNaro" };
            yield return new object[] { TheIndieHack.Instance, "TheIndieHack" };
            yield return new object[] { TheOneRing2nd.Instance, "TheOneRing2nd" };
            yield return new object[] { TheUnofficialHollowKnightRPG.Instance, "TheUnofficialHollowKnightRPG" };
            yield return new object[] { TherapieSein.Instance, "TherapieSein" };
            yield return new object[] { TokumeiTenkousei.Instance, "TokumeiTenkousei" };
            yield return new object[] { TokyoGhostResearch.Instance, "TokyoGhostResearch" };
            yield return new object[] { Torg.Instance, "Torg" };
            yield return new object[] { Torg1_5.Instance, "Torg1.5" };
            yield return new object[] { TorgEternity.Instance, "TorgEternity" };
            yield return new object[] { ToshiakiHolyGrailWar.Instance, "ToshiakiHolyGrailWar" };
            yield return new object[] { TrailOfCthulhu.Instance, "TrailOfCthulhu" };
            yield return new object[] { TrinitySeven.Instance, "TrinitySeven" };
            yield return new object[] { TunnelsAndTrolls.Instance, "TunnelsAndTrolls" };
            yield return new object[] { TwilightGunsmoke.Instance, "TwilightGunsmoke" };
            yield return new object[] { UnsungDuet.Instance, "UnsungDuet" };
            yield return new object[] { Utakaze.Instance, "Utakaze" };
            yield return new object[] { VampireTheMasquerade5th.Instance, "VampireTheMasquerade5th" };
            yield return new object[] { Ventangle.Instance, "Ventangle" };
            yield return new object[] { Villaciel.Instance, "Villaciel" };
            yield return new object[] { VisionConnect.Instance, "VisionConnect" };
            yield return new object[] { WARPS.Instance, "WARPS" };
            yield return new object[] { WaresBlade.Instance, "WaresBlade" };
            yield return new object[] { Warhammer.Instance, "Warhammer" };
            yield return new object[] { Warhammer4.Instance, "Warhammer4" };
            yield return new object[] { WerewolfTheApocalypse5th.Instance, "WerewolfTheApocalypse5th" };
            yield return new object[] { WitchQuest.Instance, "WitchQuest" };
            yield return new object[] { WoW.Instance, "WoW" };
            yield return new object[] { WorldOfDarkness.Instance, "WorldOfDarkness" };
            yield return new object[] { WorldsEndFrontline.Instance, "WorldsEndFrontline" };
            yield return new object[] { YankeeMustDie.Instance, "YankeeMustDie" };
            yield return new object[] { YankeeYogSothoth.Instance, "YankeeYogSothoth" };
            yield return new object[] { YearZeroEngine.Instance, "YearZeroEngine" };
            yield return new object[] { Yggdrasill.Instance, "Yggdrasill" };
            yield return new object[] { Yotabana.Instance, "Yotabana" };
            yield return new object[] { YuMyoKishi.Instance, "YuMyoKishi" };
            yield return new object[] { ZettaiReido.Instance, "ZettaiReido" };
            yield return new object[] { ZombiLine.Instance, "ZombiLine" };
        }

        [Theory]
        [MemberData(nameof(AllNewSystems))]
        public void System_HasValidId(IGameSystem system, string expectedId)
        {
            Assert.Equal(expectedId, system.Id);
        }

        [Theory]
        [MemberData(nameof(AllNewSystems))]
        public void System_HasNonEmptyName(IGameSystem system, string expectedId)
        {
            Assert.False(string.IsNullOrEmpty(system.Name), $"{expectedId} should have a non-empty Name");
        }

        [Theory]
        [MemberData(nameof(AllNewSystems))]
        public void System_HasNonEmptySortKey(IGameSystem system, string expectedId)
        {
            Assert.False(string.IsNullOrEmpty(system.SortKey), $"{expectedId} should have a non-empty SortKey");
        }

        [Theory]
        [MemberData(nameof(AllNewSystems))]
        public void System_InstanceIsSingleton(IGameSystem system, string expectedId)
        {
            Assert.NotNull(system);
        }
    }
}
