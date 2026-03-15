using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 蒼天のヴィラシエル
    /// </summary>
    public sealed class Villaciel : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Villaciel Instance = new Villaciel();


        private static readonly Regex DVbsRegex = new Regex(
            @"\d+VBS",
            RegexOptions.Compiled);

        private static readonly Regex DVfRegex = new Regex(
            @"\d+VF",
            RegexOptions.Compiled);

        private static readonly Regex DVmRegex = new Regex(
            @"\d+VM",
            RegexOptions.Compiled);

        private static readonly Regex DVgRegex = new Regex(
            @"\d+VG",
            RegexOptions.Compiled);

        private static readonly Regex PjVaRegex = new Regex(
            @"PJ[VA]?",
            RegexOptions.Compiled);

        private static readonly Regex PqVaRegex = new Regex(
            @"PQ[VA]?",
            RegexOptions.Compiled);

        private static readonly Regex MmIadVRegex = new Regex(
            @"MM([IAD]|V[VA]?)",
            RegexOptions.Compiled);

        private static readonly Regex FLrwgbcsRegex = new Regex(
            @"F[LRWGBCS]",
            RegexOptions.Compiled);

        private static readonly Regex IpVaRegex = new Regex(
            @"IP[VA]?",
            RegexOptions.Compiled);

        private static readonly Regex EpVaDRegex = new Regex(
            @"EP[VA]?\d?",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "Villaciel";

        /// <inheritdoc/>
        public override string Name => "蒼天のヴィラシエル";

        /// <inheritdoc/>
        public override string SortKey => "そうてんのういらしえる";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　　　　　　　　nVBS[>=d]
        　[]内省略時は達成数の計算のみ。トライアンフあり。
        　n: ダイス数、d: 難易度
        ・フロンティア判定　　nVF
        　n: ダイス数
        　nVBSを行い、うでまえ表を参照した結果を表示します。
        ・採掘スキル判定　　　nVM
        　n: ダイス数
        　判定に成功した場合、自動的に獲得できるアイテム数も表示されます。
        ・宝石加工スキル判定　nVG
        　n: ダイス数
        ・前職表　　　　　　　PJ[x]    x=V,A
        　[]内は省略可能。
        　PJ, PJV: 「蒼天のヴィラシエル」掲載の前職表　PJA: 「白雲のアルメサール」掲載の前職表
        ・ぷちクエスト表　　　PQ[x]    x=V,A
        　[]内は省略可能。
        　PQ, PQV: 「蒼天のヴィラシエル」掲載のぷちクエスト表　PQA: 「白雲のアルメサール」掲載のぷちクエスト表
        ・アクシデント表　　　AC
        ・もふもふ表　　　　　MMx      x=I,A,V,VV,VA,D
          MMI: 昆虫　MMA: 動物　MMV, MMVV: ヴィラシエル種（「蒼天のヴィラシエル」掲載）　MMVA: ヴィラシエル種（「白雲のアルメサール」掲載）　MMD: 鋼龍種
        ・釣り表　　　　　　　Fx       x=L,R,W,G,B,C,S
        　FL: 湖　FR: 河　FW: 白雲　FG: 灰雲　FB: 黒雲　FC: 共通　FS: 塩湖
        ・不食植物表　　　　　IP[x]    x=V,A
        　IP, IPV: 「蒼天のヴィラシエル」掲載の不食植物表　IPA: 「白雲のアルメサール」掲載の不食植物表
        ・可食植物表　　　　　EP[x][n] x=V,A
        　[]内は省略可能。
        　n: 可食植物表番号
        　EP[n], EPV[n]: 「蒼天のヴィラシエル」掲載の可食植物表。[]内省略時はnを1D6で決定し、EPVnを実行。ただし、1D6の出目が6ならば、「好きな表を選んでおっけー！」と表示。
        　EPA[n]: 「白雲のアルメサール」掲載の可食植物表。[]内省略時は1D6を振り、出目が偶数ならばEPA1、奇数ならばEPA2を実行。
        ・変異植物表　　　　　MP
        ・改良種表　　　　　　IS
        ";


        private const int D6 = 6;
        private const int LeastSuccessRoll = 4;
        private const int LeastMiningSuccessRoll = 5;
        private const int LeastGemSuccessRoll = 6;
        private const string SuccessStr = " ＞ 成功";
        private const string FailureStr = " ＞ 失敗";

        private static readonly string[] SKILL_CHART = new string[]
        {
            "左に3マス、上に3マス動かす",
            "左に2マス、上に2マス動かす",
            "右か下に1マス動かしてもよい",
            "右に1マス、下に1マス動かす",
            "好きな方向に最大で3マス動かしてもよい（1マスでも良い）",
            "好きな方向に最大で5マス動かしてもよい（1〜3マスでもよい）",
        };

        private static readonly string[][] VILLACIEL_PREVIOUS_JOB_CHART = new string[][]
        {
            new string[]
            {
                "農家: 知力+1 器用さ+1 開拓／1Lv",
                "漁師: 知力+1 ひらめき+1 釣り／1Lv",
                "狩人: 武力+1 ひらめき+1 穴掘り／1Lv",
                "鍛冶職人: 武力+1 器用さ+1 採掘／1Lv",
                "牧場主: 仲良し+2 開拓／1Lv",
                "採掘師: 器用さ+1 ひらめき+1 採掘／1Lv",
            },
            new string[]
            {
                "家事手伝い: 器用さ+1 仲良し+1 調理／1Lv",
                "調理師: 知力+1 ひらめき+1 調理／1Lv",
                "細工師: 器用さ+2 採掘／1Lv",
                "大工: 武力+1 器用さ+1 木こり／1Lv",
                "荒くれ者: 武力+2 穴掘り／1Lv",
                "王国騎士: 武力+1 知力+1 木こり／1Lv",
            },
        };

        private static readonly string[][] ARMESEAR_PREVIOUS_JOB_CHART = new string[][]
        {
            new string[]
            {
                "農家: 知力+1 器用さ+1 開拓／1Lv",
                "漁師: 知力+1 ひらめき+1 釣り／1Lv",
                "狩人: 武力+1 ひらめき+1 穴掘り／1Lv",
                "鍛冶職人: 武力+1 器用さ+1 採掘／1Lv",
                "牧場主: 仲良し+2 開拓／1Lv",
                "採掘師: 器用さ+1 ひらめき+1 採掘／1Lv",
            },
            new string[]
            {
                "羊飼い: 仲良し+2 もふもふ／1Lv",
                "芽拾い: 知力+1 武力+1 採集／1Lv",
                "服屋見習い: 器用さ+2 裁縫／1Lv",
                "革細工見習い: 知力+2 裁縫／1Lv",
                "商人: 知力+1 仲良し+1 基礎になるスキル／1Lv",
                "旅人: 武力+1 知力+1 基礎になるスキル／1Lv",
            },
            new string[]
            {
                "家事手伝い: 器用さ+1 仲良し+1 調理／1Lv",
                "調理師: 知力+1 ひらめき+1 調理／1Lv",
                "細工師: 器用さ+2 採掘／1Lv or 調合・細工／1Lv",
                "大工: 武力+1 器用さ+1 木こり／1Lv",
                "荒くれ者: 武力+2 穴掘り／1Lv",
                "王国騎士: 武力+1 知力+1 木こり／1Lv",
            },
        };

        private static readonly string[][] VILLACIEL_PETIT_QUEST_CHART = new string[][]
        {
            new string[]
            {
                "家の補強のために: 【目的：木を1個納品】【報酬：各自2プサイ】見えを張っていい木材で家を作ったら木材が枯渇しちまった。頼む、原木を分けてくれないか？",
                "孫のために: 【目的：花を1個納品】【報酬：各自2プサイ】綺麗な花があればいい色に染められるだろうと思うてな。孫のために必要なの。",
                "人間界の草: 【目的：草を2個納品】【報酬：各自3プサイ】魔界にはない草が生えていると噂で聞いたことがある。その草がほしい。",
                "種の生存のために: 【目的：可食植物（改良種を除く）を1個納品】【報酬：各自1プサイ】育ちが悪い同種の植物と掛け合わせてみたいのでサンプルがほしい。",
                "にんげんのたべもの！: 【目的：可食植物（改良種を除く）を1個納品】【報酬：各自2プサイ】ひゅーいあはなにをたべるの！ たべたい！",
                "まかいのたべものって？: 【目的：可食植物の改良種を2個納品】【報酬：各自3プサイ】まぞくさんはなにたべるですか！ おしえてください。",
            },
            new string[]
            {
                "おうちなおしたいの！: 【目的：石材を1個納品】【報酬：各自1プサイ】おうちがぼろぼろだから、ママのかわりになおしたいの。",
                "娘の結婚式に必要なんだ！: 【目的：宝石を2個納品】【報酬：各自3プサイ】ちょっとさきなんですが、娘が結婚するので結婚式用の宝石を集めています。",
                "金属がたりない！: 【目的：金属を1個納品】【報酬：各自2プサイ】いい武器にはいい金属を。今回必要なのは……。",
                "村の聖堂を直したいんだ！: 【目的：石材を1個納品】【報酬：各自2プサイ】聖堂を直していたが石材がたりない！",
                "弟の甲冑に使うんだ！: 【目的：金属を2個納品】【報酬：各自3プサイ】最近、近くの鉱山から「ある金属」が姿を消した。",
                "おねえちゃんのたんじょうびに: 【目的：宝石を1個納品】【報酬：各自2プサイ】たんじょうびぷれぜんとにほうせきあげたらおねえちゃんよろこぶかな？",
            },
            new string[]
            {
                "パパのために: 【目的：木材の家具を1個納品】【報酬：各自2プサイ】はたらいてばっかりのパパにプレゼントしたいの。おねがいします！",
                "癒やされたい……: 【目的：石材の家具を1個納品】【報酬：各自2プサイ】仕事時間は短いとはいえ、激務。めちゃつらい。癒しになる家具がほしい。",
                "いい家具に囲まれてみたい: 【目的：金属の家具を1個納品】【報酬：各自2プサイ】開拓も最高だけど、他の島の人とも交流したい。人を呼べるような家を作るためには最高の家具が必要！",
                "家具の在庫不足: 【目的：木材の装飾品を1個納品】【報酬：各自3プサイ】困ったことに職人に逃げられた！ このままじゃ、お店開けない！！",
                "と、ともだちにあげるの！: 【目的：石材の装飾品を1個納品】【報酬：各自3プサイ】えっと、お、おきにいりのともだちがいるんだ。そ、そのこのたんじょうびだから、プレゼントしたくって。",
                "親の木に飾りを: 【目的：金属の装飾品を1個納品】【報酬：各自3プサイ】元気のない親の木を心配してペッコ達が大騒ぎしているんだ。君はいつまでも美しいよと伝えたくてね。一つ助力をお願いするよ。",
            },
            new string[]
            {
                "そちらの河魚を食してみたい: 【目的：河魚を2個納品】【報酬：各自3プサイ】おいしい河魚がいるときいたことがあるのです。さぁ、はやく、釣ってきてくださいまし。",
                "研究に使用したい: 【目的：湖魚を1個納品】【報酬：各自1プサイ】そちらの世界にある同名の魚が本当にこちらの世界にいるものと一緒か確かめたいのです。",
                "しろいくもにすむおさかながみたい！: 【目的：白雲の雲魚を1個納品】【報酬：各自2プサイ】こっちにはしろいくもってなかなかないの！ しろいくものおさかな、たべてみたいな。",
                "釣り師がいないのでお魚がほしい: 【目的：灰雲の雲魚を2個納品】【報酬：各自3プサイ】野菜や肉もいいが魚も食べたい……。頼む、魚を釣ってきてくれないか？",
                "まっくろなくもにすむおさかな！: 【目的：黒雲の雲魚を1個納品】【報酬：各自2プサイ】まっくろなくもにはどんなさかながすんでるの？ みせて、みせて！",
                "人間界では見られない魚が見たい！: 【目的：共通の雲魚を1個納品】【報酬：各自2プサイ】他の魚の雲を利用して泳ぎ回る魚がいると聞いたよ。ぜひ見せてほしいな。",
            },
        };

        private static readonly string[][] ARMESEAR_PETIT_QUEST_CHART = new string[][]
        {
            new string[]
            {
                "お祭り用の布が足りないの！: 【目的：布を2個納品】【報酬：各自4プサイ】お祭り前なのに、布職人が腰を痛めちゃったの！",
                "お洋服がぼろぼろになっちゃったの: 【目的：布を1個納品】【報酬：各自2プサイ】おばあちゃんに作ってもらった服がボロボロになっちゃったから、なおしたいの。",
                "ぎっくり腰からのヘルプ: 【目的：薪を3個納品】【報酬：各自3プサイ】仕事してたらぎっくり腰になっちゃったのだ。頼むのだ。",
                "不調には栄養たっぷりのミルクを: 【目的：ミルクを1個納品】【報酬：各自3プサイ】体調を崩しちゃったの。栄養満点のミルクを頂戴。",
                "材料がたりない！: 【目的：？？？の粗皮を1個納品】【報酬：各自3プサイ】革細工師を目指してるんだけど、皮が足りないんだ。種類は問わないから、早めに頼むよ。",
                "愛しのガードナーのために: 【目的：？？？の肉を1個納品】【報酬：各自3プサイ】ガードナーの調子が悪いから、栄養をつけさせたいんだ。肉はなんだっていい、とびっきりのを頼むよ。",
            },
            new string[]
            {
                "灯火をひとつ: 【目的：キャンドルを1個納品】【報酬：各自3プサイ】家の裏に知らない建物があるんだ。まっくらだから明かりが必要で……。",
                "布の色を頂戴: 【目的：染料を1個納品】【報酬：各自2プサイ】んー、コンテストのために布を織ったのだけど、色が決められないんだ。お願いするよ。",
                "きれいなのお花を: 【目的：花を1個納品】【報酬：各自2プサイ】パパの誕生日プレゼントを妹と作りたいんだ。お願いできる？",
                "旅立ちのために: 【目的：衣類を1個納品】【報酬：各自15プサイ】旅立つ弟に服をプレゼントしたいんだ。",
                "納品物が足りない！: 【目的：革を1個納品】【報酬：各自4プサイ】どうしても納品する皮がたりない……頼む、なんとか用意できないか？",
                "求）照明: 【目的：照明を1個納品】【報酬：各自10プサイ】引っ越しする最中に照明を壊してしまった！ 明日から明かりがないのはつらい……。作ってくれないか？",
            },
            new string[]
            {
                "装備の修復のため: 【目的：革を2個納品】【報酬：各自5プサイ】大事な装備が壊れちゃったんだ！ 直すのに必要なんだけど、革を持っているかい？",
                "主に祝いの品を: 【目的：敷物を1個納品】【報酬：各自15プサイ】誕生日を迎える主にささやかなながらわたしからも祝いの品を送りたいのです。",
                "手料理を求めて: 【目的：出来栄え5の料理を1個納品】【報酬：各自5プサイ】たまには誰かの料理が食べたいんだ。",
                "釣り竿が折れちゃって……: 【目的：塩魚を2個納品】【報酬：各自3プサイ】釣り竿が折れちゃったから釣りができないんだ。一匹頼める？",
                "蝋がほしいの: 【目的：蝋を1個納品】【報酬：各自2プサイ】お兄ちゃんとパパの誕生日プレゼントを作るの。見つからないからお願いできる？",
                "美しさを求めて: 【目的：アルメサール産の花を1個納品】【報酬：各自3プサイ】美しいお花を摘んで来てくださらない？ 美のために必要でしてよ。",
            },
        };

        private static readonly string[] ACCIDENT_CHART = new string[]
        {
            "飛び猪襲来！: 空飛ぶ猪が浮遊島めがけて突撃してきた！ 建物が粉砕される前に迎撃だ！（「蒼天のヴィラシエル」P.46）",
            "嵐がくるぞ！: 嵐が来るらしいぞ！ どれだけ対策できるかが鍵だ！（「蒼天のヴィラシエル」P.47）",
            "雨が降らないぞ！: おかしいなぁ、雨が降らないぞぉ……？ こうなったら雨乞いの踊りだ！（「蒼天のヴィラシエル」P.48）",
            "トビウオ流星群: きらきら光る流れ星……いや待て！ あれはトビウオの群れだー！？（「蒼天のヴィラシエル」P.49）",
            "すごい雷雨: すごい。ごろごろばりばり聞こえてくる。これは早々に対策しないと直撃するぞ！（「蒼天のヴィラシエル」P.50）",
            "野菜泥棒出現！: 畑の野菜が盗まれているぞ……？ これは犯人を捕まえないと！（「蒼天のヴィラシエル」P.51）",
        };

        private static readonly string[][] MOHUMOHU_INSECT_CHART = new string[][]
        {
            new string[] { "小さな虫", "小さな虫", "カマキリ", "カマキリ", "バッタ", "クワガタ" },
            new string[] { "小さな虫", "カラスアゲハ", "カマキリ", "バッタ", "オオスカシバ", "カイコ" },
            new string[] { "ハンミョウ", "カラスアゲハ", "カマキリ", "バッタ", "カイコ", "トンボ" },
            new string[] { "ハンミョウ", "カラスアゲハ", "カラスアゲハ", "チッチハチ", "トンボ", "トンボ" },
            new string[] { "クワガタ", "カラスアゲハ", "チッチハチ", "チッチハチ", "アリ", "アリ" },
            new string[] { "クワガタ", "チッチハチ", "チッチハチ", "チッチハチ", "アリ", "アリ" },
        };

        private static readonly string[][] MOHUMOHU_ANIMAL_CHART = new string[][]
        {
            new string[] { "トリサン", "トリサン", "ブタ", "ヒツジ", "タヌキ", "タヌキ" },
            new string[] { "トリサン", "ブタ", "ヒツジ", "ウッシ", "キツネ", "タヌキ" },
            new string[] { "ブタ", "オグマ", "ヒツジ", "キツネ", "キツネ", "アタウサギ" },
            new string[] { "ブタ", "ヒツジ", "ヒツジ", "リス", "シシ", "ヴィラシエル種(MMV)" },
            new string[] { "ウッシ", "ウサギ", "ウサギ", "シシ", "アタウサギ", "オオカミ" },
            new string[] { "ウッシ", "オグマ", "クーマ", "シシ", "オオカミ", "ヴィラシエル種(MMV)" },
        };

        private static readonly string[][] MOHUMOHU_VILLACIEL_CHART = new string[][]
        {
            new string[] { "ウドン", "ウドン", "オボン", "オボン", "オボン", "オワン" },
            new string[] { "ウドン", "ウドン", "オボン", "オワン", "オワン", "オワン" },
        };

        private static readonly string[][] MOHUMOHU_VILLACIEL2_CHART = new string[][]
        {
            new string[] { "すねーくあし", "すねーくあし", "すねーくあし", "ウタヒ", "オオトリサン", "オオトリサン" },
            new string[] { "すねーくあし", "すねーくあし", "ホネホネ", "オオトリサン", "アマアマガニ", "ホワホワ" },
            new string[] { "すねーくあし", "ホネホネ", "オオトリサン", "ウタヒ", "アマアマガニ", "ペロリ" },
            new string[] { "オオトリサン", "オオトリサン", "ホネホネ", "ホネホネ", "ホワホワ", "アマアマガニ" },
            new string[] { "ホネホネ", "ウタヒ", "アマアマガニ", "ペロリ", "ペロリ", "ペロリ" },
            new string[] { "オオトリサン", "ホワホワ", "ホワホワ", "アマアマガニ", "ペロリ", "ペロリ" },
        };

        private static readonly string[][] MOHUMOHU_DRAGON_CHART = new string[][]
        {
            new string[] { "モドモドリス", "テロメ", "モドモドリス", "オジサン", "オジサン", "グロッチ" },
            new string[] { "テロメ", "モドモドリス", "オジサン", "テロメ", "ニホンツノ", "グロッチ" },
            new string[] { "テロメ", "グロッチ", "グロッチ", "グロッチ", "オジサン", "コディ" },
            new string[] { "モドモドリス", "グロッチ", "ニホンツノ", "テロメ", "テーリー", "ケラプス" },
            new string[] { "オジサン", "テロメ", "テロメ", "コディ", "コディ", "ケラプス" },
            new string[] { "コディ", "テーリー", "テーリー", "コディ", "ケラプス", "アサール・ゴッツ" },
        };

        private static readonly string[][] FISHING_LAKE_CHART = new string[][]
        {
            new string[] { "ヤマアイズリ", "ヤマアイズリ", "ヤマアイズリ", "シコウチャ", "シコウチャ", "ハナロクショウ" },
            new string[] { "ヤマアイズリ", "ヤマアイズリ", "ヤマアイズリ", "シコウチャ", "ハナロクショウ", "ハナロクショウ" },
            new string[] { "ヤマアイズリ", "ヤマアイズリ", "シコウチャ", "シコウチャ", "ハナモエギ", "トノチャ" },
            new string[] { "ヤマアイズリ", "カラスアゲハ", "シコウチャ", "ハナロクショウ", "トノチャ", "ハナモエギ" },
            new string[] { "シコウチャ", "シコウチャ", "ハナロクショウ", "ハナロクショウ", "トノチャ", "ハナモエギ" },
            new string[] { "シコウチャ", "ハナロクショウ", "トノチャ", "トノチャ", "ハナモエギ", "シンペキ" },
        };

        private static readonly string[][] FISHING_RIVER_CHART = new string[][]
        {
            new string[] { "ケイカンセキ", "ケイカンセキ", "ケイカンセキ", "ケイカンセキ", "カナリア", "イワヌ" },
            new string[] { "ケイカンセキ", "ケイカンセキ", "カナリア", "カナリア", "カナリア", "イワヌ" },
            new string[] { "ケイカンセキ", "ケイカンセキ", "カナリア", "イワヌ", "イワヌ", "ヤマブキ" },
            new string[] { "ケイカンセキ", "カナリア", "イワヌ", "アメイロ", "アメイロ", "ヤマブキ" },
            new string[] { "カナリア", "カナリア", "イワヌ", "アメイロ", "ヤマブキ", "ヤマブキ" },
            new string[] { "カナリア", "イワヌ", "アメイロ", "アメイロ", "ヤマブキ", "コハク" },
        };

        private static readonly string[][] FISHING_WHITE_CHART = new string[][]
        {
            new string[] { "ウメガサネ", "ウメガサネ", "ウメガサネ", "ウメガサネ", "ハネズ", "ユルシ" },
            new string[] { "ウメガサネ", "ウメガサネ", "ウメガサネ", "ハネズ", "ソホ", "シンク" },
            new string[] { "ウメガサネ", "ウメガサネ", "ハネズ", "ソホ", "ユルシ", "ユルシ" },
            new string[] { "ウメガサネ", "ハネズ", "ソホ", "ユルシ", "シンク", "シンク" },
            new string[] { "ハネズ", "ソホ", "ソホ", "ユルシ", "シンク", "共通(FC)" },
            new string[] { "ハネズ", "ソホ", "ユルシ", "シンク", "共通(FC)", "シュアン" },
        };

        private static readonly string[][] FISHING_GRAY_CHART = new string[][]
        {
            new string[] { "ウメガサネ", "ウメガサネ", "セイラン", "セイラン", "ミハナダ", "ミハナダ" },
            new string[] { "ウメガサネ", "セイラン", "セイラン", "ミハナダ", "ミハナダ", "ミハナダ" },
            new string[] { "ウメガサネ", "ユルシ", "ミハナダ", "ミハナダ", "ミハナダ", "リンドウ" },
            new string[] { "ユルシ", "ユルシ", "セイラン", "リンドウ", "リンドウ", "スミレ" },
            new string[] { "ユルシ", "ユルシ", "リンドウ", "スミレ", "スミレ", "共通(FC)" },
            new string[] { "ユルシ", "リンドウ", "スミレ", "スミレ", "共通(FC)", "シゴク" },
        };

        private static readonly string[][] FISHING_BLACK_CHART = new string[][]
        {
            new string[] { "セイラン", "セイラン", "テツコン", "テツコン", "ウスハナ", "ウスハナ" },
            new string[] { "セイラン", "セイラン", "テツコン", "ウスハナ", "ウスハナ", "フカガワネズミ" },
            new string[] { "セイラン", "テツコン", "ウスハナ", "ウスハナ", "ミハナダ", "フカガワネズミ" },
            new string[] { "セイラン", "テツコン", "ミハナダ", "ウスハナ", "フカガワネズミ", "フカガワネズミ" },
            new string[] { "セイラン", "ウスハナ", "ミハナダ", "ミハナダ", "ミハナダ", "共通(FC)" },
            new string[] { "テツコン", "ウスハナ", "ミハナダ", "フカガワネズミ", "共通(FC)", "ルリ" },
        };

        private static readonly string[][] FISHING_COMMON_CHART = new string[][]
        {
            new string[] { "トビウオ", "トビウオ", "トビウオ", "オオガメ", "ロブスター", "オオサンショウウオ" },
            new string[] { "トビウオ", "トビウオ", "エイ", "オオガメ", "クジラ", "ロブスター" },
            new string[] { "トビウオ", "エイ", "マグロ", "マグロ", "カジキ", "イタチザメ" },
            new string[] { "トビウオ", "ミズダコ", "クラゲ", "マグロ", "オオクラゲ", "ハンマーヘッド・シャーク" },
            new string[] { "トビウオ", "エイ", "オオガメ", "オオガメ", "イタチザメ", "ミズダコ" },
            new string[] { "トビウオ", "クラゲ", "ロブスター", "ハンマーヘッド・シャーク", "ミズダコ", "ダイオウイカ" },
        };

        private static readonly string[][] FISHING_SALT_LAKE_CHART = new string[][]
        {
            new string[] { "シラユリ", "シラユリ", "シラユリ", "ゲッパク", "ゲッパク", "ゲッパク" },
            new string[] { "シラユリ", "シラユリ", "シラユリ", "ゲッパク", "スズ", "ナマリ" },
            new string[] { "シラユリ", "ゲッパク", "ゲッパク", "スズ", "ナマリ", "ナマリ" },
            new string[] { "シラユリ", "シラユリ", "ナマリ", "ナマリ", "ナマリ", "ナマリ" },
            new string[] { "ゲッパク", "ゲッパク", "スズ", "スズ", "ロイロ", "ロイロ" },
            new string[] { "ナマリ", "スズ", "スズ", "スズ", "ロイロ", "クロツルバミ" },
        };

        private static readonly string[][] INEDIBLE_PLANT_CHART = new string[][]
        {
            new string[] { "シュイの花", "ダデオの花", "ロキの花", "シェラの花", "トトイト", "ポロネイマ" },
            new string[] { "シュイの花", "ロキの花", "アウディの花", "イディウの花", "トトイト", "ポロネイマ" },
            new string[] { "ダデオの花", "アウディの花", "イディウの花", "マトイト", "ポポトマ", "ルタタ" },
            new string[] { "シュイの花", "ミカギの花", "ロトイト", "ロトイト", "ツルイド", "ルタタ" },
            new string[] { "ミカギの花", "ロトイト", "ロトイト", "ツルイド", "ルタタ", "変異植物(MP)" },
            new string[] { "トトイト", "マトイト", "ポポトマ", "ツルイド", "変異植物(MP)", "サボサボ" },
        };

        private static readonly string[][] INEDIBLE_PLANT2_CHART = new string[][]
        {
            new string[] { "マトラの花", "マトラの花", "蜜蝋", "ポルラの花", "ウェスドの花", "ポルラの花" },
            new string[] { "マトラの花", "ホイの花", "マトラの花", "ウェスドの花", "蜜蝋", "ロロの花" },
            new string[] { "ホイの花", "ポルラの花", "ウェスドの花", "ホイの花", "ポルラの花", "ポルラの花" },
            new string[] { "ポルラの花", "ホイの花", "ロロの花", "ウェスドの花", "ポルラの花", "ドダの実" },
            new string[] { "ポルラの花", "ウェスドの花", "ロロの花", "ロロの花", "ロロの花", "ロロの花" },
            new string[] { "ウェスドの花", "ロロの花", "ポルラの花", "ロロの花", "ドダの実", "ロロの花" },
        };

        private static readonly string[][][] EDIBLE_PLANT_CHARTS = new string[][][]
        {
            new string[][]
            {
                new string[] { "小麦", "小麦", "さつまいも", "ねぎ", "白菜", "きゅうり" },
                new string[] { "小麦", "さつまいも", "さといも", "白菜", "白菜", "とうもろこし" },
                new string[] { "さといも", "さついも", "ねぎ", "白菜", "とうもろこし", "枝豆" },
                new string[] { "シソ", "ひらたけ", "エリンギ", "枝豆", "枝豆", "ラズベリー" },
                new string[] { "シソ", "ひらたけ", "ひらたけ", "エリンギ", "ラズベリー", "さといも" },
                new string[] { "ナシ", "ナシ", "ナシ", "ラズベリー", "ラズベリー", "さといも" },
            },
            new string[][]
            {
                new string[] { "米", "米", "にんじん", "じゃがいも", "ふき", "まいたけ" },
                new string[] { "米", "じゃがいも", "じゃがいも", "にら", "ふき", "きくらげ" },
                new string[] { "冬瓜", "しょうが", "冬瓜", "ふき", "ふき", "きくらげ" },
                new string[] { "しょうが", "冬瓜", "ビワ", "にら", "まいたけ", "まいたけ" },
                new string[] { "ビワ", "ビワ", "もも", "かぼちゃ", "グリーンピース", "まいたけ" },
                new string[] { "ビワ", "もも", "もも", "かぼちゃ", "かぼちゃ", "かぼちゃ" },
            },
            new string[][]
            {
                new string[] { "もち米", "トマト", "オクラ", "とうがらし", "大根", "グミ" },
                new string[] { "もち米", "オクラ", "オクラ", "大根", "大根", "とうがらし" },
                new string[] { "しいたけ", "マッシュルーム", "オクラ", "グミ", "玉ねぎ", "小松菜" },
                new string[] { "ブロッコリー", "しいたけ", "トマト", "玉ねぎ", "さやえんどう", "玉ねぎ" },
                new string[] { "しいたけ", "マッシュルーム", "ブロッコリー", "小松菜", "さやえんどう", "改良種(IS)" },
                new string[] { "マッシュルーム", "ブロッコリー", "マッシュルーム", "小松菜", "改良種(IS)", "グミ" },
            },
            new string[][]
            {
                new string[] { "大豆", "大豆", "にんにく", "そらまめ", "しめじ", "みかん" },
                new string[] { "かぶ", "大豆", "かぶ", "キャベツ", "そらまめ", "みかん" },
                new string[] { "にんにく", "かぶ", "にんにく", "しめじ", "クランベリー", "ピーマン" },
                new string[] { "キャベツ", "キャベツ", "ほうれん草", "しめじ", "レタス", "ピーマン" },
                new string[] { "ほうれん草", "ほうれん草", "クランベリー", "レタス", "ピーマン", "改良種(IS)" },
                new string[] { "松茸", "ほうれん草", "松茸", "レタス", "クランベリー", "改良種(IS)" },
            },
            new string[][]
            {
                new string[] { "小豆", "れんこん", "みつば", "やまのいも", "デコポン", "イチゴ" },
                new string[] { "れんこん", "れんこん", "小豆", "なめこ", "かいわれ大根", "なめこ" },
                new string[] { "やまのいも", "アスパラガス", "なす", "なめこ", "やまのいも", "デコポン" },
                new string[] { "なす", "やまのいも", "みつば", "えのきたけ", "かいわれ大根", "デコポン" },
                new string[] { "アスパラガス", "アスパラガス", "やまのいも", "みつば", "なめこ", "改良種(IS)" },
                new string[] { "なす", "もやし", "えのきたけ", "えのきたけ", "改良種(IS)", "イチゴ" },
            },
        };

        private static readonly string[][][] EDIBLE_PLANT2_CHARTS = new string[][][]
        {
            new string[][]
            {
                new string[] { "テンサイ", "バノ", "テンサイ", "サトウモロ", "サトウモロ", "パンノミ" },
                new string[] { "テンサイ", "バノ", "サトウモロ", "バノ", "ミソレグア", "パンノミ" },
                new string[] { "テンサイ", "サトウモロ", "バノ", "ニクニク", "パンノミ", "メーズム" },
                new string[] { "バノ", "バノ", "バノ", "パンノミ", "ミソレグア", "メーズム" },
                new string[] { "テンサイ", "パンノミ", "ニクニク", "ニクニク", "メーズム", "ミソレグア" },
                new string[] { "サトウモロ", "ニクニク", "メーズム", "ミソレグア", "メーズム", "メーズム" },
            },
            new string[][]
            {
                new string[] { "アロアベリー", "パンノミ", "ミソレグア", "サイングア", "パンノミ", "アロアベリー" },
                new string[] { "パンノミ", "サイングア", "パンノミ", "ミソレグア", "アロアベリー", "ミソレグア" },
                new string[] { "パンノミ", "アロアベリー", "サイングア", "パンノミ", "パンノミ", "トロアベリア" },
                new string[] { "パンノミ", "アロアベリー", "パンノミ", "ミソレグア", "ミソレグア", "トロアベリア" },
                new string[] { "サイングア", "パンノミ", "トロアベリア", "ミソレグア", "アロアベリー", "サイングア" },
                new string[] { "ミソレグア", "トロアベリア", "サイングア", "アロアベリー", "トロアベリア", "トロアベリア" },
            },
        };

        private static readonly string[][] MUTANT_PLANT_CHART = new string[][]
        {
            new string[] { "ガドゴン", "ガドゴン", "レディダン", "ボディア", "ブタマル", "ブタマル" },
            new string[] { "レディダン", "レディダン", "ボディア", "トロコッコ", "ブタマル", "ツァイド" },
            new string[] { "ボディア", "ボディア", "マメノキ", "ナッキュ", "ツァイド", "ボディア" },
            new string[] { "ナッキュ", "マメノキ", "ナッキュ", "ガドゴン", "レディダン", "レディダン" },
            new string[] { "ポメラマ", "ポメラマ", "ナッキュ", "ツァイド", "ガドゴン", "ボディア" },
            new string[] { "ナッキュ", "ツァイド", "ツァイド", "ツァイド", "ボディア", "グラディエゴ" },
        };

        private static readonly string[][] IMPROVED_SPECIES_CHART = new string[][]
        {
            new string[] { "ワワ", "ワワ", "ブラックカロット", "ビーズ", "レモン", "ブラッドオレンジ" },
            new string[] { "ポポ", "ポポ", "グランツェ", "オオカサゲ", "ブラッドオレンジ", "レモン" },
            new string[] { "ヒットト", "グランツェ", "ブラックベリー", "ピマット", "ブラッドオレンジ", "レモン" },
            new string[] { "ブルーベリー", "ヒットト", "グランツェ", "ブラッドオレンジ", "ユズ", "ブラックベリー" },
            new string[] { "ビーズ", "ピマット", "オオカサゲ", "ライム", "ブルーベリー", "ユズ" },
            new string[] { "ビーズ", "レッドキャベツ", "ライム", "オオカサゲ", "ライム", "リンゴ" },
        };


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = DVbsRegex.Match(command);
            if (match.Success)
            {
                return ResoluteAction(command, randomizer);
            }

            match = DVfRegex.Match(command);
            if (match.Success)
            {
                return ResoluteFrontierAction(command, randomizer);
            }

            match = DVmRegex.Match(command);
            if (match.Success)
            {
                return ResoluteMiningAction(command, randomizer);
            }

            match = DVgRegex.Match(command);
            if (match.Success)
            {
                return ResoluteCuttingGemAction(command, randomizer);
            }

            match = PjVaRegex.Match(command);
            if (match.Success)
            {
                return UsePreviousJobChart(command, randomizer);
            }

            match = PqVaRegex.Match(command);
            if (match.Success)
            {
                return UsePetitQuestChart(command, randomizer);
            }

            if (command == "AC")
            {
                return UseAccidentChart(command, randomizer);
            }

            match = MmIadVRegex.Match(command);
            if (match.Success)
            {
                return UseMohumohuChart(command, randomizer);
            }

            match = FLrwgbcsRegex.Match(command);
            if (match.Success)
            {
                return UseFishingChart(command, randomizer);
            }

            match = IpVaRegex.Match(command);
            if (match.Success)
            {
                return UseInediblePlantChart(command, randomizer);
            }

            match = EpVaDRegex.Match(command);
            if (match.Success)
            {
                return UseEdiblePlantChart(command, randomizer);
            }

            if (command == "MP")
            {
                return UseMutantPlantChart(command, randomizer);
            }

            if (command == "IS")
            {
                return UseImprovedSpeciesChart(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private (int achievement, string output) DeriveAchievement(int num_dices, string command, IRandomizer randomizer)
        {
            var dice_list = randomizer.RollBarabara(num_dices, D6);
            var dice_str = string.Join(",", dice_list);
            var num_triumph_dices = dice_list.Count(d => d == 6);
            var num_successes = dice_list.Count(dice => dice >= LeastSuccessRoll);
            var achievement = num_successes + num_triumph_dices;
            var output = $"({command}) ＞ [{dice_str}] ＞ 達成数: {achievement}";
            return (achievement, output);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var match_data = Regex.Match(command, @"(\d+)VBS(>=(\d+))?");
            var num_dices = int.Parse(match_data.Groups[1].Value);
            var (achievement, output) = DeriveAchievement(num_dices, command, randomizer);
            if (!match_data.Groups[2].Success)
            {
                return Result.CreateBuilder(output).Build();
            }
            var difficulty = int.Parse(match_data.Groups[3].Value);
            output += achievement >= difficulty ? SuccessStr : FailureStr;
            return Result.CreateBuilder(output).Build();
        }

        private Result? ResoluteFrontierAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)VF");
            var num_dices = int.Parse(m.Groups[1].Value);
            var (achievement, output) = DeriveAchievement(num_dices, command, randomizer);

            var chart_index = achievement switch
            {
                >= 0 and <= 2 => achievement,
                3 or 4 => 3,
                >= 5 and <= 8 => 4,
                _ => 5,
            };
            var skill = SKILL_CHART[chart_index];
            return Result.CreateBuilder($"{output} ＞ {skill}").Build();
        }

        private (string output, bool isSuccessful) ResoluteDifficultAction(int num_dices, int least_success_roll, string command, IRandomizer randomizer)
        {
            var dice_list = randomizer.RollBarabara(num_dices, D6);
            var dice_str = string.Join(",", dice_list);
            var largest_roll = dice_list.Max();
            var isSuccessful = largest_roll >= least_success_roll;

            var output = $"({command}) ＞ [{dice_str}]";
            output += isSuccessful ? SuccessStr : FailureStr;

            return (output, isSuccessful);
        }

        private Result? ResoluteMiningAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)VM");
            var num_dices = int.Parse(m.Groups[1].Value);
            var (output, is_successful) = ResoluteDifficultAction(num_dices, LeastMiningSuccessRoll, command, randomizer);
            if (!is_successful)
            {
                return Result.CreateBuilder(output).Build();
            }
            var roll_result = randomizer.RollOnce(D6);
            return Result.CreateBuilder($"{output} ＞ (1D6).Build() ＞ [{roll_result}] ＞ アイテムを{roll_result}個獲得").Build();
        }

        private Result? ResoluteCuttingGemAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"(\d+)VG");
            var num_dices = int.Parse(m.Groups[1].Value);
            var (output, _) = ResoluteDifficultAction(num_dices, LeastGemSuccessRoll, command, randomizer);
            return Result.CreateBuilder(output).Build();
        }

        private Result? UsePreviousJobChart(string command, IRandomizer randomizer)
        {
            var match_data = Regex.Match(command, @"PJ([VA]?)");
            var chart_symbol = match_data.Groups[1].Value == "" ? "V" : match_data.Groups[1].Value;
            var roll_result1 = randomizer.RollOnce(D6);

            var (chart_text, roll_result2) = chart_symbol switch
            {
                "V" => GetTableBy1d6(VILLACIEL_PREVIOUS_JOB_CHART[(roll_result1 - 1) / 3]),
                "A" => GetTableBy1d6(ARMESEAR_PREVIOUS_JOB_CHART[(roll_result1 - 1) / 2]),
                _ => (null, 0),
            };

            var chart_title = chart_symbol switch
            {
                "V" => "前職表（ヴィラシエル）",
                "A" => "前職表（アルメサール）",
                _ => "",
            };
            return Result.CreateBuilder($"{chart_title} ＞ [{roll_result1},{roll_result2}] ＞ {chart_text}").Build();
        }

        private Result? UsePetitQuestChart(string command, IRandomizer randomizer)
        {
            var match_data = Regex.Match(command, @"PQ([VA]?)");
            var chart_symbol = match_data.Groups[1].Value == "" ? "V" : match_data.Groups[1].Value;

            var roll_result1 = randomizer.RollOnce(D6);
            var (chart_text, roll_result2) = chart_symbol switch
            {
                "V" => GetTableBy1d6(VILLACIEL_PETIT_QUEST_CHART[roll_result1 switch
                {
                    1 or 2 => 0,
                    3 or 4 => 1,
                    5 => 2,
                    6 => 3,
                    _ => 0,
                }]),
                "A" => GetTableBy1d6(ARMESEAR_PETIT_QUEST_CHART[(roll_result1 - 1) / 2]),
                _ => (null, 0),
            };

            var chart_title = chart_symbol switch
            {
                "V" => "ぷちクエスト表（ヴィラシエル）",
                "A" => "ぷちクエスト表（アルメサール）",
                _ => "",
            };
            return Result.CreateBuilder($"{chart_title} ＞ [{roll_result1},{roll_result2}] ＞ {chart_text}").Build();
        }

        private Result? UseAccidentChart(string _command, IRandomizer randomizer)
        {
            var (chart_text, roll_result) = GetTableBy1d6(ACCIDENT_CHART);
            return Result.CreateBuilder($"アクシデント表 ＞ [{roll_result}] ＞ {chart_text}").Build();
        }

        private string Use6x6ChartString(string[][] chart, string chart_name, IRandomizer randomizer)
        {
            var y_roll = randomizer.RollOnce(D6);
            var (cell_text, x_roll) = GetTableBy1d6(chart[y_roll - 1]);
            return $"{chart_name} ＞ [{y_roll},{x_roll}] ＞ 下{y_roll}マス、右{x_roll}マス ＞ {cell_text}";
        }

        private Result? Use6x6Chart(string[][] chart, string chart_name, IRandomizer randomizer)
        {
            return Result.CreateBuilder(Use6x6ChartString(chart, chart_name, randomizer)).Build();
        }

        private Result? UseMohumohuChart(string command, IRandomizer randomizer)
        {
            if (command == "MMI")
            {
                return Use6x6Chart(MOHUMOHU_INSECT_CHART, "もふもふ表・昆虫", randomizer);
            }
            else if (command == "MMA")
            {
                return Use6x6Chart(MOHUMOHU_ANIMAL_CHART, "もふもふ表・動物", randomizer);
            }
            else if (Regex.IsMatch(command, @"^MMV[VA]?$"))
            {
                var match_data = Regex.Match(command, @"MMV([VA]?)");
                var chart_symbol = match_data.Groups[1].Value == "" ? "V" : match_data.Groups[1].Value;
                switch (chart_symbol)
                {
                    case "V":
                    {
                        var y_roll = randomizer.RollOnce(D6);
                        var (cell_text, x_roll) = GetTableBy1d6(MOHUMOHU_VILLACIEL_CHART[1 - y_roll % 2]);
                        return Result.CreateBuilder($"もふもふ表・ヴィラシエル種（ヴィラシエル） ＞ [{y_roll},{x_roll}] ＞ 下{(y_roll % 2 == 0 ? "偶数" : "奇数")}、右{x_roll}マス ＞ {cell_text}").Build();
                    }
                    case "A":
                        return Use6x6Chart(MOHUMOHU_VILLACIEL2_CHART, "もふもふ表・ヴィラシエル種（アルメサール）", randomizer);
                }
            }
            else if (command == "MMD")
            {
                return Use6x6Chart(MOHUMOHU_DRAGON_CHART, "もふもふ表・鋼龍種", randomizer);
            }
            return null;
        }

        private Result? UseFishingChart(string command, IRandomizer randomizer)
        {
            return command switch
            {
                "FL" => Use6x6Chart(FISHING_LAKE_CHART, "釣り・湖表", randomizer),
                "FR" => Use6x6Chart(FISHING_RIVER_CHART, "釣り・河表", randomizer),
                "FW" => Use6x6Chart(FISHING_WHITE_CHART, "釣り・白雲表", randomizer),
                "FG" => Use6x6Chart(FISHING_GRAY_CHART, "釣り・灰雲表", randomizer),
                "FB" => Use6x6Chart(FISHING_BLACK_CHART, "釣り・黒雲表", randomizer),
                "FC" => Use6x6Chart(FISHING_COMMON_CHART, "釣り・共通表", randomizer),
                "FS" => Use6x6Chart(FISHING_SALT_LAKE_CHART, "釣り・塩湖表", randomizer),
                _ => null,
            };
        }

        private Result? UseInediblePlantChart(string command, IRandomizer randomizer)
        {
            var match_data = Regex.Match(command, @"IP([VA]?)");
            var chart_symbol = match_data.Groups[1].Value == "" ? "V" : match_data.Groups[1].Value;
            return chart_symbol switch
            {
                "V" => Use6x6Chart(INEDIBLE_PLANT_CHART, "不食植物表（ヴィラシエル）", randomizer),
                "A" => Use6x6Chart(INEDIBLE_PLANT2_CHART, "不食植物表（アルメサール）", randomizer),
                _ => null,
            };
        }

        private Result? UseVillacielEdiblePlantChart(int chart_id, string output, IRandomizer randomizer)
        {
            return Result.CreateBuilder(output + Use6x6ChartString(EDIBLE_PLANT_CHARTS[chart_id - 1], $"可食植物表{chart_id}（ヴィラシエル）", randomizer)).Build();
        }

        private Result? UseArmesearEdiblePlantChart(int chart_id, string output, IRandomizer randomizer)
        {
            return Result.CreateBuilder(output + Use6x6ChartString(EDIBLE_PLANT2_CHARTS[chart_id - 1], $"可食植物表{chart_id}（アルメサール）", randomizer)).Build();
        }

        private Result? UseEdiblePlantChart(string command, IRandomizer randomizer)
        {
            int roll_result = 0;
            int chart_id = 0;
            var match_data = Regex.Match(command, @"EP([VA]?)(\d?)");
            var chart_symbol = match_data.Groups[1].Value == "" ? "V" : match_data.Groups[1].Value;
            switch (chart_symbol)
            {
                case "V":
                    if (match_data.Groups[2].Value == "")
                    {
                        roll_result = randomizer.RollOnce(D6);
                        if (roll_result == D6)
                        {
                            return Result.CreateBuilder("(1D6).Build() ＞ [6] ＞ 好きな表を選んでおっけー！").Build();
                        }
                        return UseVillacielEdiblePlantChart(roll_result, $"(1D6) ＞ [{roll_result}] ＞ ", randomizer);
                    }
                    else
                    {
                        chart_id = int.Parse(match_data.Groups[2].Value);
                        if (!(chart_id >= 1 && chart_id <= 5))
                        {
                            return Result.CreateBuilder("").Build();
                        }
                        return UseVillacielEdiblePlantChart(chart_id, "", randomizer);
                    }
                case "A":
                    if (match_data.Groups[2].Value == "")
                    {
                        roll_result = randomizer.RollOnce(D6);
                        return UseArmesearEdiblePlantChart(roll_result % 2 == 0 ? 1 : 2, $"(1D6) ＞ [{roll_result}] ＞ ", randomizer);
                    }
                    else
                    {
                        chart_id = int.Parse(match_data.Groups[2].Value);
                        if (!(chart_id == 1 || chart_id == 2))
                        {
                            return Result.CreateBuilder("").Build();
                        }
                        return UseArmesearEdiblePlantChart(chart_id, "", randomizer);
                    }
                default:
                    return null;
            }
        }

        private Result? UseMutantPlantChart(string _command, IRandomizer randomizer)
        {
            return Use6x6Chart(MUTANT_PLANT_CHART, "変異植物表", randomizer);
        }

        private Result? UseImprovedSpeciesChart(string _command, IRandomizer randomizer)
        {
            return Use6x6Chart(IMPROVED_SPECIES_CHART, "改良種表", randomizer);
        }

    }
}
