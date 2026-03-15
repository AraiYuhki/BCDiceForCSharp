using System.Collections.Generic;
using BCDice.Core;
using BCDice.GameSystem;

namespace BCDice
{
    /// <summary>
    /// BCDiceライブラリのメインファサードクラス
    /// </summary>
    public static class BCDiceRunner
    {
        private static readonly GameSystemRegistry _registry = new GameSystemRegistry();
        private static IGameSystem _currentSystem = DiceBot.Instance;

        /// <summary>
        /// 静的コンストラクタ - デフォルトのゲームシステムを登録
        /// </summary>
        static BCDiceRunner()
        {
            _registry.Register(DiceBot.Instance);
            _registry.Register(Cthulhu.Instance);
            _registry.Register(Cthulhu7th.Instance);
            _registry.Register(SwordWorld2_5.Instance);
            _registry.Register(DoubleCross3.Instance);
            _registry.Register(Insane.Instance);
            _registry.Register(Shinobigami.Instance);
            _registry.Register(Nechronica.Instance);
            _registry.Register(Arianrhod2E.Instance);
            _registry.Register(Emoklore.Instance);
            _registry.Register(Satasupe.Instance);
            _registry.Register(MagicaLogia.Instance);
            _registry.Register(Kamigakari.Instance);
            _registry.Register(MeikyuuKingdom.Instance);
            _registry.Register(KanColle.Instance);
            _registry.Register(StellarKnights.Instance);
            _registry.Register(FateCoreSystem.Instance);
            _registry.Register(GoldenSkyStories.Instance);
            _registry.Register(Ryutama.Instance);
            _registry.Register(Ainecadette.Instance);
            _registry.Register(DungeonsAndDragons.Instance);
            _registry.Register(Paranoia.Instance);
            // Tier 1: Phase 4 追加分
            _registry.Register(NightWizard3rd.Instance);
            _registry.Register(LogHorizon.Instance);
            _registry.Register(GranCrest.Instance);
            _registry.Register(TokyoNova.Instance);
            _registry.Register(Alshard.Instance);
            _registry.Register(ChaosFlare.Instance);
            _registry.Register(MonotoneMuseum.Instance);
            _registry.Register(BeastBindTrinity.Instance);
            _registry.Register(Dracurouge.Instance);
            _registry.Register(BloodMoon.Instance);
            _registry.Register(SRS.Instance);
            // Tier 2-4: 全228システム追加
            _registry.Register(AFF2e.Instance);
            _registry.Register(AceKillerGene.Instance);
            _registry.Register(Agnostos.Instance);
            _registry.Register(Aionia.Instance);
            _registry.Register(Airgetlamh.Instance);
            _registry.Register(AlchemiaStruggle.Instance);
            _registry.Register(Alsetto.Instance);
            _registry.Register(AlterRaise.Instance);
            _registry.Register(Amadeus.Instance);
            _registry.Register(AngelGear.Instance);
            _registry.Register(AniMalus.Instance);
            _registry.Register(AnimaAnimus.Instance);
            _registry.Register(Aoharubaan.Instance);
            _registry.Register(ArknightsFan.Instance);
            _registry.Register(ArsMagica.Instance);
            _registry.Register(AssaultEngine.Instance);
            _registry.Register(Avandner.Instance);
            _registry.Register(Ayabito.Instance);
            _registry.Register(BBN.Instance);
            _registry.Register(BadLife.Instance);
            _registry.Register(Bakenokawa.Instance);
            _registry.Register(BarnaKronika.Instance);
            _registry.Register(BattleTech.Instance);
            _registry.Register(BeginningIdol.Instance);
            _registry.Register(BeginningIdol2022.Instance);
            _registry.Register(BlackJacket.Instance);
            _registry.Register(BladeOfArcana.Instance);
            _registry.Register(BlindMythos.Instance);
            _registry.Register(BloodCrusade.Instance);
            _registry.Register(Bloodorium.Instance);
            _registry.Register(CardRanker.Instance);
            _registry.Register(CastleInGray.Instance);
            _registry.Register(CharonSanctions.Instance);
            _registry.Register(Chill.Instance);
            _registry.Register(Chill3.Instance);
            _registry.Register(CodeLayerd.Instance);
            _registry.Register(ColossalHunter.Instance);
            _registry.Register(Comes.Instance);
            _registry.Register(ConvictorDrive.Instance);
            _registry.Register(CrashWorld.Instance);
            _registry.Register(CthulhuTech.Instance);
            _registry.Register(CyberpunkRed.Instance);
            _registry.Register(DarkBlaze.Instance);
            _registry.Register(DarkDaysDrive.Instance);
            _registry.Register(DarkSouls.Instance);
            _registry.Register(DeadlineHeroes.Instance);
            _registry.Register(DemonParasite.Instance);
            _registry.Register(DemonSpike.Instance);
            _registry.Register(DesperateRun.Instance);
            _registry.Register(DetatokoSaga.Instance);
            _registry.Register(DiceOfTheDead.Instance);
            _registry.Register(DivineCharger.Instance);
            _registry.Register(DungeonsAndDragons5.Instance);
            _registry.Register(EarthDawn.Instance);
            _registry.Register(EarthDawn3.Instance);
            _registry.Register(EarthDawn4.Instance);
            _registry.Register(EclipsePhase.Instance);
            _registry.Register(EdgeFlippers.Instance);
            _registry.Register(Elric.Instance);
            _registry.Register(Elysion.Instance);
            _registry.Register(EmbryoMachine.Instance);
            _registry.Register(EndBreaker.Instance);
            _registry.Register(EtrianOdysseySRS.Instance);
            _registry.Register(Fiasco.Instance);
            _registry.Register(FilledWith.Instance);
            _registry.Register(FinalFantasyXIV.Instance);
            _registry.Register(FullFace.Instance);
            _registry.Register(FullMetalPanic.Instance);
            _registry.Register(FutariSousa.Instance);
            _registry.Register(GURPS.Instance);
            _registry.Register(Garactier.Instance);
            _registry.Register(Garako.Instance);
            _registry.Register(GardenOrder.Instance);
            _registry.Register(GehennaAn.Instance);
            _registry.Register(GeishaGirlwithKatana.Instance);
            _registry.Register(GhostLive.Instance);
            _registry.Register(GoblinSlayer.Instance);
            _registry.Register(Gorilla.Instance);
            _registry.Register(GundamSentinel.Instance);
            _registry.Register(Gundog.Instance);
            _registry.Register(GundogRevised.Instance);
            _registry.Register(GundogZero.Instance);
            _registry.Register(GurpsFW.Instance);
            _registry.Register(HarnMaster.Instance);
            _registry.Register(HatsuneMiku.Instance);
            _registry.Register(HeroScale.Instance);
            _registry.Register(Hieizan.Instance);
            _registry.Register(HouraiGakuen.Instance);
            _registry.Register(HunterTheReckoning5th.Instance);
            _registry.Register(HuntersMoon.Instance);
            _registry.Register(IfIfIf.Instance);
            _registry.Register(Illusio.Instance);
            _registry.Register(InfiniteBabeL.Instance);
            _registry.Register(InfiniteFantasia.Instance);
            _registry.Register(InvisibleLiar.Instance);
            _registry.Register(Irisbane.Instance);
            _registry.Register(IthaWenUa.Instance);
            _registry.Register(JamesBond.Instance);
            _registry.Register(JekyllAndHyde.Instance);
            _registry.Register(JuinKansen.Instance);
            _registry.Register(KamitsubakiCityUnderConstructionNarrative.Instance);
            _registry.Register(Karukami.Instance);
            _registry.Register(KemonoNoMori.Instance);
            _registry.Register(KillDeathBusiness.Instance);
            _registry.Register(KimitoYell.Instance);
            _registry.Register(KizunaBullet.Instance);
            _registry.Register(KurayamiCrying.Instance);
            _registry.Register(Kutulu.Instance);
            _registry.Register(KyokoShinshoku.Instance);
            _registry.Register(Liminal.Instance);
            _registry.Register(LiverLabyrinth.Instance);
            _registry.Register(LiveraDoll.Instance);
            _registry.Register(LostRecord.Instance);
            _registry.Register(LostRoyal.Instance);
            _registry.Register(MagicPunk.Instance);
            _registry.Register(Magius.Instance);
            _registry.Register(Magius_3rdNewTokyoCity.Instance);
            _registry.Register(MamonoScramble.Instance);
            _registry.Register(MeikyuDays.Instance);
            _registry.Register(MeikyuKingdomBasic.Instance);
            _registry.Register(MetalHead.Instance);
            _registry.Register(MetalHeadExtream.Instance);
            _registry.Register(MetallicGuardian.Instance);
            _registry.Register(NRR.Instance);
            _registry.Register(NSSQ.Instance);
            _registry.Register(NervWhitePaper.Instance);
            _registry.Register(NeverCloud.Instance);
            _registry.Register(NightWizard.Instance);
            _registry.Register(NightmareHunterDeep.Instance);
            _registry.Register(NinjaSlayer.Instance);
            _registry.Register(NinjaSlayer2.Instance);
            _registry.Register(NjslyrBattle.Instance);
            _registry.Register(NobunagasBlackCastle.Instance);
            _registry.Register(Nuekagami.Instance);
            _registry.Register(OneWayHeroics.Instance);
            _registry.Register(OracleEngine.Instance);
            _registry.Register(OrgaRain.Instance);
            _registry.Register(Oukahoushin3rd.Instance);
            _registry.Register(Paradiso.Instance);
            _registry.Register(ParanoiaPerfect.Instance);
            _registry.Register(ParanoiaRebooted.Instance);
            _registry.Register(ParasiteBlood.Instance);
            _registry.Register(PastFutureParadox.Instance);
            _registry.Register(Pathfinder.Instance);
            _registry.Register(Peekaboo.Instance);
            _registry.Register(Pendragon.Instance);
            _registry.Register(PersonaO.Instance);
            _registry.Register(PhantasmAdventure.Instance);
            _registry.Register(Postman.Instance);
            _registry.Register(PulpCthulhu.Instance);
            _registry.Register(Raisondetre.Instance);
            _registry.Register(RecordOfLodossWar.Instance);
            _registry.Register(RecordOfSteam.Instance);
            _registry.Register(Revulture.Instance);
            _registry.Register(RogueLikeHalf.Instance);
            _registry.Register(RokumonSekai2.Instance);
            _registry.Register(RoleMaster.Instance);
            _registry.Register(RuinBreakers.Instance);
            _registry.Register(RuneQuest.Instance);
            _registry.Register(RuneQuestRoleplayingInGlorantha.Instance);
            _registry.Register(RyuTuber.Instance);
            _registry.Register(SajinsenkiAGuS.Instance);
            _registry.Register(SajinsenkiAGuS2E.Instance);
            _registry.Register(SamsaraBallad.Instance);
            _registry.Register(ScreamHighSchool.Instance);
            _registry.Register(Sengensyou.Instance);
            _registry.Register(SevenFortressMobius.Instance);
            _registry.Register(ShadowRun.Instance);
            _registry.Register(ShadowRun4.Instance);
            _registry.Register(ShadowRun5.Instance);
            _registry.Register(SharedFantasia.Instance);
            _registry.Register(ShinMegamiTenseiKakuseihen.Instance);
            _registry.Register(ShinkuuGakuen.Instance);
            _registry.Register(Shiranui.Instance);
            _registry.Register(ShoujoTenrankai.Instance);
            _registry.Register(ShuumatsuBargainWars.Instance);
            _registry.Register(ShuumatsuKikou.Instance);
            _registry.Register(Siren.Instance);
            _registry.Register(Skynauts.Instance);
            _registry.Register(SkynautsBouken.Instance);
            _registry.Register(StarryDolls.Instance);
            _registry.Register(SteamPunkers.Instance);
            _registry.Register(StellarLife.Instance);
            _registry.Register(StrangerOfSwordCity.Instance);
            _registry.Register(StratoShout.Instance);
            _registry.Register(Strave.Instance);
            _registry.Register(SwordWorld.Instance);
            _registry.Register(SwordWorld2_0.Instance);
            _registry.Register(TalesFromTheLoop.Instance);
            _registry.Register(TenkaRyouran.Instance);
            _registry.Register(TensaiGunshiNiNaro.Instance);
            _registry.Register(TheIndieHack.Instance);
            _registry.Register(TheOneRing2nd.Instance);
            _registry.Register(TheUnofficialHollowKnightRPG.Instance);
            _registry.Register(TherapieSein.Instance);
            _registry.Register(TokumeiTenkousei.Instance);
            _registry.Register(TokyoGhostResearch.Instance);
            _registry.Register(Torg.Instance);
            _registry.Register(Torg1_5.Instance);
            _registry.Register(TorgEternity.Instance);
            _registry.Register(ToshiakiHolyGrailWar.Instance);
            _registry.Register(TrailOfCthulhu.Instance);
            _registry.Register(TrinitySeven.Instance);
            _registry.Register(TunnelsAndTrolls.Instance);
            _registry.Register(TwilightGunsmoke.Instance);
            _registry.Register(UnsungDuet.Instance);
            _registry.Register(Utakaze.Instance);
            _registry.Register(VampireTheMasquerade5th.Instance);
            _registry.Register(Ventangle.Instance);
            _registry.Register(Villaciel.Instance);
            _registry.Register(VisionConnect.Instance);
            _registry.Register(WARPS.Instance);
            _registry.Register(WaresBlade.Instance);
            _registry.Register(Warhammer.Instance);
            _registry.Register(Warhammer4.Instance);
            _registry.Register(WerewolfTheApocalypse5th.Instance);
            _registry.Register(WitchQuest.Instance);
            _registry.Register(WoW.Instance);
            _registry.Register(WorldOfDarkness.Instance);
            _registry.Register(WorldsEndFrontline.Instance);
            _registry.Register(YankeeMustDie.Instance);
            _registry.Register(YankeeYogSothoth.Instance);
            _registry.Register(YearZeroEngine.Instance);
            _registry.Register(Yggdrasill.Instance);
            _registry.Register(Yotabana.Instance);
            _registry.Register(YuMyoKishi.Instance);
            _registry.Register(ZettaiReido.Instance);
            _registry.Register(ZombiLine.Instance);
        }

        /// <summary>
        /// ゲームシステムレジストリ
        /// </summary>
        public static GameSystemRegistry Registry => _registry;

        /// <summary>
        /// 現在選択されているゲームシステム
        /// </summary>
        public static IGameSystem CurrentSystem
        {
            get => _currentSystem;
            set => _currentSystem = value ?? DiceBot.Instance;
        }

        /// <summary>
        /// 登録されている全ゲームシステム
        /// </summary>
        public static IReadOnlyList<IGameSystem> GameSystems => _registry.Systems;

        /// <summary>
        /// IDでゲームシステムを取得する
        /// </summary>
        /// <param name="id">ゲームシステムID</param>
        /// <returns>ゲームシステム、見つからない場合はnull</returns>
        public static IGameSystem? GetGameSystem(string id)
        {
            return _registry.GetById(id);
        }

        /// <summary>
        /// ゲームシステムをIDで切り替える
        /// </summary>
        /// <param name="id">ゲームシステムID</param>
        /// <returns>切り替え成功時true</returns>
        public static bool SetGameSystem(string id)
        {
            var system = _registry.GetById(id);
            if (system != null)
            {
                _currentSystem = system;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 現在のゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>実行結果、コマンドが認識されない場合はnull</returns>
        public static Result? Eval(string command)
        {
            return _currentSystem.Eval(command);
        }

        /// <summary>
        /// 現在のゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>実行結果、コマンドが認識されない場合はnull</returns>
        public static Result? Eval(string command, IRandomizer randomizer)
        {
            return _currentSystem.Eval(command, randomizer);
        }

        /// <summary>
        /// 指定したゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="systemId">ゲームシステムID</param>
        /// <param name="command">コマンド文字列</param>
        /// <returns>実行結果、ゲームシステムまたはコマンドが認識されない場合はnull</returns>
        public static Result? Eval(string systemId, string command)
        {
            var system = _registry.GetById(systemId);
            return system?.Eval(command);
        }

        /// <summary>
        /// 指定したゲームシステムでコマンドを実行する
        /// </summary>
        /// <param name="systemId">ゲームシステムID</param>
        /// <param name="command">コマンド文字列</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>実行結果、ゲームシステムまたはコマンドが認識されない場合はnull</returns>
        public static Result? Eval(string systemId, string command, IRandomizer randomizer)
        {
            var system = _registry.GetById(systemId);
            return system?.Eval(command, randomizer);
        }

        /// <summary>
        /// ゲームシステムを登録する
        /// </summary>
        /// <param name="gameSystem">登録するゲームシステム</param>
        public static void RegisterGameSystem(IGameSystem gameSystem)
        {
            _registry.Register(gameSystem);
        }

        /// <summary>
        /// 名前でゲームシステムを検索する
        /// </summary>
        /// <param name="name">検索文字列（部分一致）</param>
        /// <returns>マッチしたゲームシステムのリスト</returns>
        public static IReadOnlyList<IGameSystem> SearchGameSystems(string name)
        {
            return _registry.SearchByName(name);
        }
    }
}
