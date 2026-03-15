using System;
using System.Collections.Generic;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 艦これRPG
    /// </summary>
    public sealed class KanColle : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KanColle Instance = new KanColle();

        /// <inheritdoc/>
        public override string Id => "KanColle";

        /// <inheritdoc/>
        public override string Name => "艦これRPG";

        /// <inheritdoc/>
        public override string SortKey => "かんこれRPG";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
例) 2D6 ： 単純に2D6した値を出します。
例) 2D6>=7 ： 行為判定。2D6して目標値7以上出れば成功。
例) 2D6+2>=7 ： 行為判定。2D6に修正+2をした上で目標値7以上になれば成功。

2D6での行為判定時は1ゾロでファンブル、6ゾロでスペシャル扱いになります。
天龍ちゃんスペシャルは手動で判定してください。

・各種表
　・感情表　ET／アクシデント表　ACT
　・日常イベント表　EVNT／交流イベント表　EVKT／遊びイベント表　EVAT
　　演習イベント表　EVET／遠征イベント表　EVENT／作戦イベント表　EVST
　・開発表　DVT／開発表（一括）DVTM
　　　装備１種表　WP1T／装備２種表　WP2T／装備３種表　WP3T／装備４種表　WP4T
　・アイテム表　ITT／目標表　MHT／戦果表　SNT
　・戦場表　SNZ　暴走表／RNT
・D66ダイス(D66S相当=低い方が10の桁になる)
";

        private readonly Dictionary<string, ITable> _tables;

        private KanColle()
        {
            _tables = CreateTables();
        }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // テーブルコマンド
            if (_tables.TryGetValue(command, out var table))
            {
                return table.Roll(randomizer);
            }

            // 開発表（一括）
            if (command == "DVTM")
            {
                return RollDevelopMatome(randomizer);
            }

            return null;
        }

        /// <summary>
        /// 2D6判定時のファンブル・スペシャル判定を行う
        /// </summary>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            // 2D6>=N形式の判定に対してファンブル・スペシャルをチェック
            if (result != null && randomizer.RandResults.Count >= 2)
            {
                // 最初の2つのダイスが2D6かどうかチェック
                var rands = randomizer.RandResults;
                if (rands.Count >= 2 && rands[0].Sides == 6 && rands[1].Sides == 6)
                {
                    int diceTotal = rands[0].Value + rands[1].Value;

                    if (diceTotal <= 2)
                    {
                        // ファンブル
                        return Result.CreateBuilder(result.Text + " ＞ ファンブル（判定失敗。アクシデント表を自分のPCに適用）")
                            .SetFumble(true)
                            .SetFailure(true)
                            .SetRands(randomizer.RandResults)
                            .SetDetailedRands(randomizer.DetailedRandResults)
                            .Build();
                    }
                    else if (diceTotal >= 12)
                    {
                        // スペシャル
                        return Result.CreateBuilder(result.Text + " ＞ スペシャル（判定成功。【行動力】が1D6点回復）")
                            .SetCritical(true)
                            .SetSuccess(true)
                            .SetRands(randomizer.RandResults)
                            .SetDetailedRands(randomizer.DetailedRandResults)
                            .Build();
                    }
                }
            }

            return result;
        }

        private Result RollDevelopMatome(IRandomizer randomizer)
        {
            int dice = randomizer.RollOnce(6);
            string output1;
            Result subResult;

            switch (dice)
            {
                case 1:
                case 2:
                    output1 = "装備１種表";
                    subResult = _tables["WP1T"].Roll(randomizer);
                    break;
                case 3:
                case 4:
                    output1 = "装備２種表";
                    subResult = _tables["WP2T"].Roll(randomizer);
                    break;
                case 5:
                    output1 = "装備３種表";
                    subResult = _tables["WP3T"].Roll(randomizer);
                    break;
                default:
                    output1 = "装備４種表";
                    subResult = _tables["WP4T"].Roll(randomizer);
                    break;
            }

            string text = $"開発表（一括）({dice}) ＞ {output1}：{subResult.Text}";

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static Dictionary<string, ITable> CreateTables()
        {
            return new Dictionary<string, ITable>(StringComparer.OrdinalIgnoreCase)
            {
                ["ET"] = new SimpleTable("感情表", "ET",
                    "かわいい（プラス）／むかつく（マイナス）",
                    "すごい（プラス）／ざんねん（マイナス）",
                    "たのしい（プラス）／こわい（マイナス）",
                    "かっこいい（プラス）／しんぱい（マイナス）",
                    "いとしい（プラス）／かまってほしい（マイナス）",
                    "だいすき（プラス）／だいっきらい（マイナス）"
                ),

                ["ACT"] = new SimpleTable("アクシデント表", "ACT",
                    "よかったぁ。何もなし。",
                    "意外な手応え。その判定に使った個性の属性（【長所】と【弱点】）が反対になる。自分が判定を行うとき以外はこの効果は無視する。",
                    "えーん。大失態。このキャラクターに対して【感情値】を持っているキャラクター全員の声援欄にチェックが入る。",
                    "奇妙な猫がまとわりつく。サイクルの終了時、もしくは、艦隊戦の終了時まで、自分の行う行為判定にマイナス１の修正がつく（この効果は、マイナス２まで累積する）。",
                    "いててて。損傷が一つ発生する。もしも艦隊戦中なら、自分と同じ航行序列にいる味方艦にも損傷が一つ発生する。",
                    "ううう。やりすぎちゃった！自分の【行動力】が１Ｄ６点減少する。"
                ),

                ["DVT"] = new SimpleTable("開発表", "DVT",
                    "装備１種表（WP1T）",
                    "装備１種表（WP1T）",
                    "装備２種表（WP2T）",
                    "装備２種表（WP2T）",
                    "装備３種表（WP3T）",
                    "装備４種表（WP4T）"
                ),

                ["WP1T"] = new SimpleTable("装備１種表", "WP1T",
                    "小口径主砲（P249）",
                    "１０ｃｍ連装高角砲（P249）",
                    "中口径主砲（P249）",
                    "１５．２ｃｍ連装砲（P249）",
                    "２０．３ｃｍ連装砲（P249）",
                    "魚雷（P252）"
                ),

                ["WP2T"] = new SimpleTable("装備２種表", "WP2T",
                    "副砲（P250）",
                    "８ｃｍ高角砲（P250）",
                    "大口径主砲（P249）",
                    "４１ｃｍ連装砲（P250）",
                    "４６ｃｍ三連装砲（P250）",
                    "機銃（P252）"
                ),

                ["WP3T"] = new SimpleTable("装備３種表", "WP3T",
                    "艦上爆撃機（P250）",
                    "艦上攻撃機（P251）",
                    "艦上戦闘機（P251）",
                    "偵察機（P251）",
                    "電探（P252）",
                    "２５ｍｍ連装機銃（P252）"
                ),

                ["WP4T"] = new SimpleTable("装備４種表", "WP4T",
                    "彗星（P250）",
                    "天山（P251）",
                    "零式艦戦５２型（P251）",
                    "彩雲（P251）",
                    "６１ｃｍ四連装（酸素）魚雷（P252）",
                    "改良型艦本式タービン（P252）"
                ),

                ["ITT"] = new SimpleTable("アイテム表", "ITT",
                    "アイス（P241）",
                    "羊羹（P241）",
                    "開発資材（P241）",
                    "高速修復剤（P241）",
                    "応急修理要員（P241）",
                    "思い出の品（P241）"
                ),

                ["MHT"] = new SimpleTable("目標表", "MHT",
                    "敵艦の中で、もっとも航行序列の高いＰＣ",
                    "敵艦の中で、もっとも損傷の多いＰＣ",
                    "敵艦の中で、もっとも【装甲力】の低いＰＣ",
                    "敵艦の中で、もっとも【回避力】の低いＰＣ",
                    "敵艦の中で、もっとも【火力】の高いＰＣ",
                    "敵艦の中から完全にランダムに決定"
                ),

                ["SNT"] = new SimpleTable("戦果表", "SNT",
                    "燃料／１Ｄ６＋[敵艦隊の人数]個",
                    "弾薬／１Ｄ６＋[敵艦隊の人数]個",
                    "鋼材／１Ｄ６＋[敵艦隊の人数]個",
                    "ボーキサイト／１Ｄ６＋[敵艦隊の人数]個",
                    "任意の資材／１Ｄ６＋[敵艦隊の人数]個",
                    "感情値／各自好きなキャラクターへの【感情値】＋１"
                ),

                ["SNZ"] = new SimpleTable("戦場表", "SNZ",
                    "同航戦（P231）",
                    "反航戦（P231）",
                    "Ｔ字有利（P231）",
                    "Ｔ字不利（P231）",
                    "悪天候（P231）",
                    "悪海象（あくかいしょう）（P231）"
                ),

                ["RNT"] = new SimpleTable("暴走表", "RNT",
                    "妄想（建造弐p164）",
                    "狂戦士（建造弐p164）",
                    "興奮（建造弐p164）",
                    "溺愛（建造弐p164）",
                    "慢心（建造弐p164）",
                    "絶望（建造弐p164）"
                ),

                ["EVNT"] = new RangeTable("日常イベント表", "EVNT", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "何もない日々：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は、《待機／航海７》で判定。"),
                    (3, 3, "ティータイム：《外国暮らし／背景１２》で判定。"),
                    (4, 4, "釣り：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《おおらか／性格３》で判定。"),
                    (5, 5, "お昼寝：《寝る／趣味２》で判定。"),
                    (6, 6, "綺麗におそうじ！：《衛生／航海１１》で判定。"),
                    (7, 7, "海軍カレー：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《食べ物／趣味６》で判定。"),
                    (8, 8, "銀蝿／ギンバイ：《規律／航海５》で判定。"),
                    (9, 9, "日々の訓練：《素直／魅力２》で判定。"),
                    (10, 10, "取材：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《名声／背景３》で判定。"),
                    (11, 11, "海水浴：《突撃／戦闘６》で判定。"),
                    (12, 12, "マイブーム：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《口ぐせ／背景６》で判定。"),
                }),

                ["EVKT"] = new RangeTable("交流イベント表", "EVKT", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "一触即発！：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《笑顔／魅力７》で判定。"),
                    (3, 3, "手取り足取り：自分以外の好きなＰＣ１人を選んで、《えっち／魅力１１》で判定。"),
                    (4, 4, "恋は戦争：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《恋愛／趣味１２》で判定。"),
                    (5, 5, "マッサージ：自分以外の好きなＰＣ１人を選んで、《けなげ／魅力６》で判定。"),
                    (6, 6, "裸のつきあい：《入浴／趣味１１》で判定。"),
                    (7, 7, "深夜のガールズトーク：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《おしゃべり／趣味７》で判定。"),
                    (8, 8, "いいまちがえ：《ばか／魅力８》で判定。"),
                    (9, 9, "小言百より慈愛の一語：自分以外の好きなＰＣ１人を選んで、《面倒見／性格４》で判定。"),
                    (10, 10, "差し入れ：自分以外の好きなＰＣ１人を選んで、提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《優しい／魅力４》で判定。"),
                    (11, 11, "お手紙：自分以外の好きなＰＣ１人を選んで、《古風／背景５》で判定。"),
                    (12, 12, "昔語り：自分以外の好きなＰＣ１人を選んで、提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《暗い過去／背景４》で判定。"),
                }),

                ["EVAT"] = new RangeTable("遊びイベント表", "EVAT", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "遊びのつもりが……：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《さわやか／魅力９》で判定。"),
                    (3, 3, "新しい遊びの開発：《空想／趣味３》で判定。"),
                    (4, 4, "宴会：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《元気／性格７》で判定。"),
                    (5, 5, "街をぶらつく：《面白い／魅力１０》で判定。"),
                    (6, 6, "ガールズコーデ：《おしゃれ／趣味１０》で判定。"),
                    (7, 7, "○○大会開催！：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《大胆／性格１２》で判定。"),
                    (8, 8, "チェス勝負：自分以外の好きなＰＣ１人を選んで、《クール／魅力３》で判定。"),
                    (9, 9, "熱唱カラオケ大会：《芸能／趣味９》で判定。"),
                    (10, 10, "アイドルコンサート：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《アイドル／背景８》で判定。"),
                    (11, 11, "スタイル自慢！：《スタイル／背景１１》で判定。"),
                    (12, 12, "ちゃんと面倒みるから！：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《生き物／趣味４》で判定。"),
                }),

                ["EVET"] = new RangeTable("演習イベント表", "EVET", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "大げんか！：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《負けず嫌い／性格６》で判定。"),
                    (3, 3, "雷撃演習：《魚雷／戦闘１０》で判定。"),
                    (4, 4, "座学の講義：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《マジメ／性格５》で判定。"),
                    (5, 5, "速力演習：《機動／航海８》で判定。"),
                    (6, 6, "救援演習：《支援／戦闘９》で判定。"),
                    (7, 7, "砲撃演習：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《砲撃／戦闘７》で判定。"),
                    (8, 8, "艦隊戦演習：《派手／魅力１２》で判定。"),
                    (9, 9, "整備演習：《整備／航海１２》で判定。"),
                    (10, 10, "夜戦演習：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《夜戦／戦闘１２》で判定。"),
                    (11, 11, "開発演習：《秘密兵器／背景９》で判定。"),
                    (12, 12, "防空射撃演習：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《対空戦闘／戦闘５》で判定。"),
                }),

                ["EVENT"] = new RangeTable("遠征イベント表", "EVENT", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "謎の深海棲艦：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《退却／戦闘８》で判定。"),
                    (3, 3, "資源輸送任務：《買い物／趣味８》で判定。"),
                    (4, 4, "強行偵察任務：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《索敵／航海４》で判定。"),
                    (5, 5, "航空機輸送作戦：《航空戦／戦闘４》で判定。"),
                    (6, 6, "タンカー護衛任務：《丁寧／性格９》で判定。"),
                    (7, 7, "海上護衛任務：提督が選んだ（キーワード）に対応した指定能力で判定。思いつかない場合は《不思議／性格２》で判定。"),
                    (8, 8, "観艦式：《おしとやか／魅力５》で判定。"),
                    (9, 9, "ボーキサイト輸送任務：《補給／航海６》で判定。"),
                    (10, 10, "社交界デビュー？：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《お嬢様／背景１０》で判定。"),
                    (11, 11, "対潜警戒任務：《対潜戦闘／戦闘１１》で判定。"),
                    (12, 12, "大規模遠征作戦、発令！：提督の選んだ（キーワード）に対応した指定能力値で判定。思いつかない場合は《指揮／航海１０》で判定。"),
                }),

                ["EVST"] = new RangeTable("作戦イベント表", "EVST", 2, 6, new (int, int, string)[]
                {
                    (2, 2, "電子の目：提督が選んだ(キーワード)に対応した指定個性で判定。思いつかない場合は《電子戦／戦闘２》で判定。"),
                    (3, 3, "直掩部隊：《航空戦／戦闘４》で判定。"),
                    (4, 4, "噂によれば：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《通信／航海３》で判定。"),
                    (5, 5, "資料室にて：《海図／航海９》で判定。"),
                    (6, 6, "守護天使：《幸運／背景７》で判定。"),
                    (7, 7, "作戦会議！：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《自由奔放／性格１１》で判定。"),
                    (8, 8, "暗号解読：《暗号／航海２》で判定。"),
                    (9, 9, "一か八か？：《楽観的／性格８》で判定。"),
                    (10, 10, "特務機関との邂逅：提督が選んだ（キーワード）に対応した指定個性で判定。思いつかない場合は《人脈／背景２》で判定。"),
                    (11, 11, "クイーンズ・ギャンビット：《いじわる／性格１０》で判定。"),
                    (12, 12, "知彼知己者、百戦不殆：《読書／趣味５》で判定。"),
                }),
            };
        }
    }
}
