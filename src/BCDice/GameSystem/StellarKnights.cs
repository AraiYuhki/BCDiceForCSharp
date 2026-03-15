using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 銀剣のステラナイツ
    /// </summary>
    public sealed class StellarKnights : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly StellarKnights Instance = new StellarKnights();

        /// <inheritdoc/>
        public override string Id => "StellarKnights";

        /// <inheritdoc/>
        public override string Name => "銀剣のステラナイツ";

        /// <inheritdoc/>
        public override string SortKey => "きんけんのすてらないつ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
・アタック判定　nSK[d][,k>l,...]
[]内は省略可能。
n: ダイス数、d: 対象の防御力、k,l: ダイスの出目がkならばlに変更

4SK: ダイスを4個振って、その結果を表示
5SK3: 【アタック判定：5ダイス】、対象の防御力を3として成功数を表示
3SK,1>6: ダイスを3個振り、出目が1のダイスを全て6に変更
6SK4,1>6,2>6: 出目が1と2を6に変更、防御力4として成功数を表示

・各種表
TT：お題表、STA：シチュエーション表A、STB：シチュエーション表B、STC：シチュエーション表C
ALLS：シチュエーション表一括、GAT：所属組織決定
HOT：希望表、DET：絶望表、WIT：願い事表
YST：あなたの物語表、YSTA：異世界、YSTM：マルジナリア
PET：性格表、FT[x]：フラグメント表（x回、省略時5回）
";

        private readonly Dictionary<string, ITable> _tables;

        private static readonly Regex SkCommand = new Regex(
            @"^(\d+)SK(\d)?((,\d>\d)+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FtCommand = new Regex(
            @"^FT(\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private StellarKnights()
        {
            _tables = CreateTables();
        }

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            command = command.ToUpperInvariant();

            // テーブルコマンド
            if (_tables.TryGetValue(command, out var table))
            {
                return table.Roll(randomizer);
            }

            // SK コマンド
            var skMatch = SkCommand.Match(command);
            if (skMatch.Success)
            {
                return EvalSkCommand(skMatch, randomizer);
            }

            // ALLS コマンド
            if (command == "ALLS")
            {
                return RollAllSituationTables(randomizer);
            }

            // PET コマンド
            if (command == "PET")
            {
                return RollPersonalityTable(randomizer);
            }

            // FT コマンド
            var ftMatch = FtCommand.Match(command);
            if (ftMatch.Success)
            {
                int count = ftMatch.Groups[1].Success ? int.Parse(ftMatch.Groups[1].Value) : 5;
                return RollFragmentTable(count, randomizer);
            }

            return null;
        }

        private Result? EvalSkCommand(Match match, IRandomizer randomizer)
        {
            int numDice = int.Parse(match.Groups[1].Value);
            int? defence = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : (int?)null;
            string? diceChangeText = match.Groups[3].Success ? match.Groups[3].Value : null;

            if (numDice <= 0)
            {
                return Result.CreateBuilder("ダイスが 0 個です（アタック判定が発生しません）")
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }

            // ダイスロール
            var dices = new List<int>();
            for (int i = 0; i < numDice; i++)
            {
                dices.Add(randomizer.RollOnce(6));
            }
            dices.Sort();

            var output = new StringBuilder();
            output.Append($"({RemakeCommand(numDice, defence, diceChangeText)}) ＞ {string.Join(",", dices)}");

            // ダイスの置き換え
            var rules = ParseDiceChangeRules(diceChangeText);
            if (rules.Count > 0)
            {
                foreach (var rule in rules)
                {
                    for (int i = 0; i < dices.Count; i++)
                    {
                        if (dices[i] == rule.From)
                        {
                            dices[i] = rule.To;
                        }
                    }
                }
                dices.Sort();
                output.Append($" ＞ [{string.Join(",", dices)}]");
            }

            bool isSuccess = false;
            bool isFailure = false;

            if (defence.HasValue)
            {
                int successNum = dices.Count(d => d >= defence.Value);
                output.Append($" ＞ 成功数: {successNum}");
                isSuccess = successNum > 0;
                isFailure = !isSuccess;
            }

            return Result.CreateBuilder(output.ToString())
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string RemakeCommand(int numDice, int? defence, string? diceChangeText)
        {
            var command = $"{numDice}SK";
            if (defence.HasValue)
            {
                command += defence.Value.ToString();
            }
            if (diceChangeText != null)
            {
                command += diceChangeText;
            }
            return command;
        }

        private static List<(int From, int To)> ParseDiceChangeRules(string? text)
        {
            var rules = new List<(int From, int To)>();
            if (string.IsNullOrEmpty(text))
            {
                return rules;
            }

            // 先頭のカンマを除去
            text = text.TrimStart(',');
            var parts = text.Split(',');
            foreach (var part in parts)
            {
                var values = part.Split('>');
                if (values.Length == 2)
                {
                    if (int.TryParse(values[0], out int from) && int.TryParse(values[1], out int to))
                    {
                        rules.Add((from, to));
                    }
                }
            }
            return rules;
        }

        private Result RollAllSituationTables(IRandomizer randomizer)
        {
            var results = new List<string>();
            results.Add(_tables["STA"].Roll(randomizer).Text);
            results.Add(_tables["STB"].Roll(randomizer).Text);
            results.Add(_tables["STC"].Roll(randomizer).Text);

            return Result.CreateBuilder(string.Join("\n", results))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result RollPersonalityTable(IRandomizer randomizer)
        {
            var items = PersonalityItems;
            int index1 = RollD66Index(randomizer);
            int index2 = RollD66Index(randomizer);
            string value1 = items[index1];
            string value2 = items[index2];

            int d66_1 = IndexToD66(index1);
            int d66_2 = IndexToD66(index2);

            return Result.CreateBuilder($"性格表({d66_1},{d66_2}) ＞ {value1}にして{value2}")
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollFragmentTable(int count, IRandomizer randomizer)
        {
            if (count <= 0)
            {
                return null;
            }

            var items = FragmentItems;
            var values = new List<string>();
            var indexes = new List<int>();

            for (int i = 0; i < count; i++)
            {
                int index = RollD66Index(randomizer);
                values.Add(items[index]);
                indexes.Add(IndexToD66(index));
            }

            return Result.CreateBuilder($"フラグメント表({string.Join(",", indexes)}) ＞ {string.Join(",", values)}")
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private int RollD66Index(IRandomizer randomizer)
        {
            int tens = randomizer.RollOnce(6);
            int ones = randomizer.RollOnce(6);
            return (tens - 1) * 6 + (ones - 1);
        }

        private static int IndexToD66(int index)
        {
            int tens = (index / 6) + 1;
            int ones = (index % 6) + 1;
            return tens * 10 + ones;
        }

        // 性格表の項目（36項目）
        private static readonly string[] PersonalityItems = new[]
        {
            "可憐", "冷静", "勇敢", "楽観主義", "負けず嫌い", "コレクター気質",
            "クール", "癒やし系", "惚れやすい", "悲観主義", "泣きやすい", "お嬢様",
            "純粋", "頑固", "辛辣", "まじめ", "落ち込みやすい", "謙虚",
            "スマート", "ゆるふわ", "好奇心旺盛", "はらぺこ", "華麗", "狭いところが好き",
            "冷徹", "朴念仁", "王子様", "目立ちたがり", "過激", "マゾヒスト",
            "ダンディ", "あらあらうふふ", "過保護", "死にたがり", "強い自尊心", "サディスト"
        };

        // フラグメント表の項目（36項目）
        private static readonly string[] FragmentItems = new[]
        {
            "出会い", "水族館", "動物園", "絵本", "童話", "神話",
            "怒った", "笑った", "泣いた", "好き", "愛情", "憎悪",
            "寒い", "暑い", "甘い", "苦い", "お菓子", "路地",
            "部屋", "身体", "ぬくもり", "毛布", "空想", "願い",
            "笑顔", "味覚", "映画", "朗読", "うた", "音楽",
            "視力", "肌の色", "聴力", "声", "痛覚", "触覚"
        };

        private static Dictionary<string, ITable> CreateTables()
        {
            return new Dictionary<string, ITable>(StringComparer.OrdinalIgnoreCase)
            {
                // お題表（D66グリッド）
                ["TT"] = new D66GridTable("お題表", "TT", new string[][]
                {
                    new[] { "未来", "占い", "遠雷", "恋心", "歯磨き", "鏡" },
                    new[] { "過去", "キス", "ささやき声", "黒い感情", "だっこ", "青空" },
                    new[] { "童話", "決意", "風の音", "愛情", "寝顔", "鎖" },
                    new[] { "ふたりの秘密", "アクシデント！", "小鳥の鳴き声", "笑顔", "食事", "宝石" },
                    new[] { "思い出", "うとうと", "鼓動", "嫉妬", "ベッド", "泥" },
                    new[] { "恋の話", "デート", "ため息", "内緒話", "お風呂", "小さな傷" },
                }),

                // シチュエーション表A：時間（1D6）
                ["STA"] = new SimpleTable("シチュエーション表A：時間", "STA",
                    "朝、誰もいない",
                    "騒がしい昼間の",
                    "寂しい夕暮れの横たわる",
                    "星の瞬く夜、",
                    "静謐の夜更けに包まれた",
                    "夜明け前の"
                ),

                // シチュエーション表B：場所（D66 1/3テーブル）
                ["STB"] = new RangeTable("シチュエーション表B：場所", "STB", 2, 6, new (int, int, string)[]
                {
                    (2, 4, "教室 　小道具：窓、机、筆記用具、チョークと黒板、窓の外から聞こえる部活動の声"),
                    (5, 5, "カフェテラス　小道具：珈琲、紅茶、お砂糖とミルク、こちらに手を振っている学友"),
                    (6, 6, "学園の中庭　小道具：花壇、鳥籠めいたエクステリア、微かに聴こえる鳥の囁き"),
                    (7, 7, "音楽室　小道具：楽器、楽譜、足踏みオルガン、壁に掛けられた音楽家の肖像画"),
                    (8, 8, "図書館　小道具：高い天井、天井に迫る程の本棚、無数に収められた本"),
                    (9, 9, "渡り廊下　小道具：空に届きそうな高さ、遠くに別の学園が見える、隣を飛び過ぎて行く鳥"),
                    (10, 10, "花の咲き誇る温室　小道具：むせ返るような花の香り、咲き誇る花々、ガラス越しの陽光"),
                    (11, 11, "アンティークショップ　小道具：アクセサリーから置物まで、見慣れない古い機械は地球時代のもの？"),
                    (12, 12, "ショッピングモール　小道具：西欧の街並みを思わせるショッピングモール、衣類に食事、お茶屋さんも"),
                }),

                // シチュエーション表C：話題（D66ハーフグリッド風、簡易実装）
                ["STC"] = new RangeTable("シチュエーション表C：話題", "STC", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "未来の話：決闘を勝ち抜いたら、あるいは負けてしまったら……未来のふたりはどうなるのだろう。"),
                    (4, 5, "衣服の話：冴えない服を着たりしていないか？　あるいはハイセンス過ぎたりしないだろうか。よぉし、私が選んであげよう!!"),
                    (6, 6, "ステラバトルの話：世界の未来は私たちにかかっている。頭では分かっていても、まだ感情が追いつかないな……。"),
                    (7, 7, "おいしいごはんの話：おいしいごはんは正義。おかわり！"),
                    (8, 8, "家族の話：生徒たちは寮生活が多い。離れて暮らす家族は、どんな人たちなのか。"),
                    (9, 9, "次の週末の話：週末、何をしますか？"),
                    (10, 10, "好きな人の話：……好きな人、いるんですか？　これはきっと真剣な話。"),
                    (11, 11, "子供の頃の話：ちいさな頃、パートナーはどんな子供だったのだろうか。"),
                    (12, 12, "好きなタイプの話：パートナーはどんな人が好みなのでしょうか……。"),
                }),

                // 所属組織決定（1D6）
                ["GAT"] = new SimpleTable("所属組織決定", "GAT",
                    "アーセルトレイ公立大学",
                    "イデアグロリア芸術総合大学",
                    "シトラ女学院",
                    "フィロソフィア大学",
                    "聖アージェティア学園",
                    "スポーン・オブ・アーセルトレイ"
                ),

                // 希望表（D66ハーフグリッド風）
                ["HOT"] = new RangeTable("希望表", "HOT", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "より良き世界：世界はもっとステキになる。きっと、ずっと、もっと。"),
                    (4, 5, "まだまだ物足りない：もっと上へ、もっと強く、あなたの未来は輝いている。"),
                    (6, 6, "立ち止まっている暇はない!：止まっている時間がもったいない。もっともっと世界を駆けるのだ!"),
                    (7, 7, "私が守るよ：君を傷つける全てから、私が絶対守ってあげる。"),
                    (8, 8, "未来は希望に満ちている：生きていないと、素敵なことは起きないんだ!"),
                    (9, 9, "慈愛の手：届く限り、あなたは手を差し伸べ続ける。"),
                    (10, 10, "自分を犠牲にしてでも：世界はもっときらきらしているんだよ。"),
                    (11, 11, "右手を伸ばす：救いたいもの、助けたいもの、なにひとつ見捨てるつもりはない!"),
                    (12, 12, "無限の愛：愛を注ごう。この胸に溢れんばかりのこの愛を!"),
                }),

                // 絶望表（D66ハーフグリッド風）
                ["DET"] = new RangeTable("絶望表", "DET", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "理不尽なる世界：あなたは世界が如何に理不尽であるか思い知った。"),
                    (4, 5, "この手は届かない：あなたにも目標はあった。しかし、もうこの手は届かないのだ。"),
                    (6, 6, "停滞した世界：どんなにあがこうと、世界は変わらない。"),
                    (7, 7, "どうして僕をいじめるの：あなたは虐げられて生きてきた。"),
                    (8, 8, "過去は絶望に満ちている：ずっとずっと、悪いことばかり、辛いことばかりだった。"),
                    (9, 9, "周囲の視線：世界があなたを見る目は、限りなく冷たいものだった。"),
                    (10, 10, "大事故：それは壮絶な事故、いいや、それは事故なんて優しいものですらなかった。"),
                    (11, 11, "目の前で消えたモノ：あなたの目の前で、大切なものは消えてしまった。"),
                    (12, 12, "喪失：何よりも大事にしていたものは、もう二度と、この手には戻らない。"),
                }),

                // 願い事表（D66 1/3テーブル風）
                ["WIT"] = new RangeTable("願い事表", "WIT", 2, 6, new (int, int, string)[]
                {
                    (2, 4, "未知の開拓者：誰も知らない世界、誰も知らない宇宙、誰も知らない星に旅立つんだ!【願いの階梯：4】"),
                    (5, 6, "故郷の復興：あなたの故郷である異世界、あるいは地球を復興する。【願いの階梯：4~7】"),
                    (7, 7, "復讐：絶対にこの復讐を果たすのだ。【願いの階梯：2】"),
                    (8, 9, "私だけのもの：独り占めしたいモノがある。【願いの階梯：5】"),
                    (10, 10, "新たなる存在：この世に存在しないけれど、存在してほしいと願うモノ。【願いの階梯：6】"),
                    (11, 11, "誰かの笑顔：誰かの笑顔の為に戦っても、いいだろう?【願いの階梯：1~6】"),
                    (12, 12, "世界を平和に：平穏な日々を願っても許されるような世界に……。【願いの階梯：7】"),
                }),

                // あなたの物語表（D66 1/3テーブル風）
                ["YST"] = new RangeTable("あなたの物語表", "YST", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "熟練ステラナイト：あなたは既に何度もステラバトルを征してきた熟練者である。"),
                    (4, 4, "権力者の血筋：統治政府や企業の上層部、あるいは学園組織の運営者の家系である。"),
                    (5, 5, "天才：あなたは紛うことなき天才だ。"),
                    (6, 6, "天涯孤独：あなたに両親はいない。"),
                    (7, 7, "救いの手：あなたは誰かに助けてもらった。"),
                    (8, 8, "欠損：心や身体、大切な宝物、家族、あなたは何かを失って、そのまま今に至った。"),
                    (9, 9, "大切なもの：大事にしているものがある。"),
                    (10, 10, "お気に入りの場所：好きな場所がある。"),
                    (11, 11, "パートナー大好き!!!!!!：私はー!! パートナーがー!! 大好きだー!!!"),
                    (12, 12, "探求者：世界の真実、隠された真実、万物の真理……あなたが追い求めるものはどこまでも尽きない。"),
                }),

                // あなたの物語表：異世界
                ["YSTA"] = new RangeTable("あなたの物語表：異世界", "YSTA", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "終わりなき戦場：あなたは果ての見えない戦場の世界からここへ流れ着いた。"),
                    (4, 4, "滅びの世界：戦争、あるいは環境汚染か、滅びた後の世界から、あなたはここへ流れ着いた。"),
                    (5, 5, "獣人たちの世界：人とは、獣の特徴を備えた者を指す言葉だった――あなたの世界では。"),
                    (6, 6, "箱庭の世界：都市か、屋敷か、あなたの住んでいた世界は極狭いものだった。"),
                    (7, 7, "永遠なる迷宮の世界：無限に広がる迷宮の世界、人々はそこを旅する探索者だった。"),
                    (8, 8, "巡礼者の世界：広大な自然と石造りの都の世界を、誰もが旅をし続ける世界からあなたはここへ流れ着いた。"),
                    (9, 9, "永遠のヴィクトリア：200年にわたるヴィクトリア女王の統治が続く常闇の世界。"),
                    (10, 10, "剣撃乱舞する世界：戦国と呼ばれた極東の一時代を、永遠に繰り返していた世界からあなたはここへ流れ着いた。"),
                    (11, 11, "先進科学の世界：地球の科学がどこまでも真っ直ぐに育った世界から、あなたはここへ流れ着いた。"),
                    (12, 12, "草花の世界：植物と人が融合した世界から、あなたはここへ流れ着いた。"),
                }),

                // あなたの物語表：マルジナリア世界
                ["YSTM"] = new RangeTable("あなたの物語表：マルジナリア世界", "YSTM", 2, 6, new (int, int, string)[]
                {
                    (2, 3, "パブ/カフェー店員：あなたは霧の帝都に無数に存在するパブ、あるいは桜の帝都で増え始めたカフェーの店員です。"),
                    (4, 4, "屋台の店員：あなたは霧の帝都の至る所に存在する、屋台の店員です。"),
                    (5, 5, "商人：お空に都市がやってきて、販路が増えたぞがっぽがっぽ。"),
                    (6, 6, "飛行船・船長：あなたはふたつの都市をつなぐ交通の要、飛行船の船長です!"),
                    (7, 7, "帝都警察：あなたは帝都警察に所属し、都市の平和を守っています。"),
                    (8, 8, "警察軍：あなたは都市の有事の際に出動するべく、日々訓練に励む軍人です。"),
                    (9, 9, "流浪の民：あなたの祖先は、この世界に「都市の外側」があった時代に、この場所へ流れ着いてきた者です。"),
                    (10, 10, "貴族：あなたは貴族として、民を守り、国を守ってきた一族の末裔です。"),
                    (11, 11, "職人：あなたは何らかの技術を身に付けた職人です。"),
                    (12, 12, "バックアップ：あなたはかつて桜の皇帝、霧の女王のバックアップとして作成された人造生命体です。"),
                }),
            };
        }
    }
}
