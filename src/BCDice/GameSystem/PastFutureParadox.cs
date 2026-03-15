using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// パストフューチャーパラドックス
    /// </summary>
    public sealed class PastFutureParadox : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly PastFutureParadox Instance = new PastFutureParadox();

        /// <inheritdoc/>
        public override string Id => "PastFutureParadox";

        /// <inheritdoc/>
        public override string Name => "パストフューチャーパラドックス";

        /// <inheritdoc/>
        public override string SortKey => "はすとふゆうちやあはらとつくす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定 PP@s#f[+m/-m]>=x 　2D6の行為判定を行う。
        　s: スペシャル値 (省略時 12)、 f: ファンブル値 (省略時 2)
        　[+m/-m]: 修正値（省略可）、 x: 目標値 (省略可)
        　例）PP, PP-1, PP@11, PP@11+2, PP@11#3, PP@11#3-1,
        　　　PP#3>=7, PP#3+2>=7, PP>=7, PP-1>=7
        ・特技表
        ランダム分野表 RCT
        ランダム特技表 RTTn（n：分野番号、省略時は全分野からランダム）
        　科学　（RTT1）、知識（RTT2）、身体（RTT3）、
        　センス（RTT4）、知恵（RTT5）、迷信（RTT6）
        ・各種表　※1D6および2D6を振る表は、末尾に=nと付けることで出目nの内容を指定可能。末尾に-n／+nと付けることで、出目に修正を付けることが可能。
        　例：SBET=2　MBET-1　TBET+2
        シーン表（2D6）
        　現代シーン表 ST4
        経歴表（D66）
        　原始時代経歴表 CT1 、古代経歴表 CT2 、中世時代経歴表 CT3
        　現代経歴表 CT4 、超情報化時代経歴表 CT5 、宇宙時代経歴表 CT6
        名前表（D66）
        　原始時代名前表（男性名） NMT1 、原始時代名前表（女性名） NFT1
        　古代名前表（男性名） NMT2 、古代名前表（女性名） NFT2 、古代名前表（姓） NLT2
        　中世時代（日本）名前表（男性名） NMT3 、中世時代（日本）名前表（女性名） NFT3 、中世時代（日本）名前表（姓） NLT3
        　中世時代（西洋）名前表（男性名） NMT3W 、中世時代（西洋）名前表（女性名） NFT3W 、中世時代（西洋）名前表（姓） NLT3W
        　現代（日本）名前表（男性名） NMT4 、現代（日本）名前表（女性名） NFT4 、現代（日本）名前表（姓） NLT4
        　現代（西洋）名前表（男性名） NMT4W 、現代（西洋）名前表（女性名） NFT4W 、現代（西洋）名前表（姓） NLT4W
        　超情報化時代名前表（男性名） NMT5 、超情報化時代名前表（女性名） NFT5 、超情報化時代名前表（姓） NLT5
        　宇宙時代名前表（男性名） NMT6 、宇宙時代名前表（女性名） NFT6 、宇宙時代名前表（姓） NLT6
        因縁種別表（D66） CTT 、ポジティブ因縁内容表（1D6） CPT 、ネガティブ因縁内容表（1D6） CNT
        バタフライエフェクト表(2D6)　※バタフライエフェクト表は-5～12までの結果を算出可能
        　重度バタフライエフェクト表 SBET 、軽度バタフライエフェクト表 MBET 、タイムトラベラー重度バタフライエフェクト表 TBET
        アクシデント表（2D6） ACT 、タイムトラベル演出表（2D6） TT 、帰還演出表（1D6） RT
        アイテム決定表（1D6） IT 、時代決定表（1D6） AGT
        ・D66ダイスあり
        ";

        // ===== テーブル定義 =====
        // 各テーブルは (名前, 項目配列) のタプルとして定義
        // インデックスは1始まり（dice roll値に対応）

        private static readonly Dictionary<string, (string Name, string[] Items)> TABLES_MOD_2D =
            new Dictionary<string, (string Name, string[] Items)>
            {
                ["ACT"] = ("アクシデント表", new string[]
                {
                    "一匹の蝶の羽ばたきが、地球の裏側では竜巻を巻き起こす。あなたは目の前にある他愛のない何かを拾って、置き直した。GMはランダムに指定特技を決定する。シーンに登場しているPCは全員その指定特技で判定する。判定に成功すれば何も起こらない。判定に失敗すると自分を対象としてバタフライエフェクトが発生する。その際の対象の因縁強度は6として処理する。",
                    "目の前で大事故が発生する。事故に巻き込まれた人を助けなければ。シーンプレイヤーのPCは《医療》《労働》《持ち上げる》《倫理》《応急処置》《魔除け》のいずれかを指定特技として判定を行う。判定に成功すると、目の前の人を助けることができる。PCの［疲労度］が1D6点減少する。だがその人物は本来はここで死ぬべき運命だったようだ。自分の【因縁】からランダムに選んだキャラクターを対象としてバタフライエフェクトが発生する。判定に失敗するとPCの［疲労度］が1D6点増加する。",
                    "あなたの出身時代では知りえない情報を知ってしまう。その情報をあなたが覚えている限り歴史に重大な影響をもたらすだろう。シーンプレイヤーのPCは《記憶力》の判定を行う。判定に失敗すると自分の【因縁】のうちの好きなキャラクターを対象としてバタフライエフェクトが発生する。",
                    "誰かと出会い頭に衝突しそうになる。シーンプレイヤーは舞台となっている時代の名前表と経歴表を使用し、年齢は8D6を振って、相手の設定概要を決める。TAはその相手の設定に従って、最適な指定特技を決定する。シーンプレイヤーのPCはその指定特技で判定を行う。判定に失敗すると、相手とぶつかってしまい、シーンプレイヤーのPCの［改変度］が1D6増加する。",
                    "困っている人がいる。シーンプレイヤーは舞台となっている時代の名前表と経歴表を使用し、年齢は8D6を振って、相手の設定概要を決める。TAはその相手の設定に従って、最適な指定特技を決定する。シーンプレイヤーのPCはその指定特技で判定を行う。判定に成功すれば、［改変度］が1点増加しつつも困りごとを解決してあげることができる。シーンプレイヤーのPCはお礼として好きな【アイテム】1つを獲得する。",
                    "シーンプレイヤーは好きなPCを登場させることができる。そしてシーンプレイヤーはシーンに登場しているキャラクター一人を選び、二人で何かしらひと時を過ごす。シーンプレイヤーのPCは好きな指定特技で判定を行う。判定に成功すれば、選んだキャラクターを新たな【因縁】として獲得できる。相手のPCもシーンプレイヤーのPCを新たな【因縁】として獲得できる。因縁種別は「タイムトラベル仲間／因縁強度6」となる。因縁内容は状況にふさわしいものをポジティブ因縁内容表もしくはネガティブ因縁内容表から選択する。",
                    "運命的な出会い。シーンプレイヤーのPCは好きな指定特技で判定を行う。判定に成功すれば、シーンプレイヤーはセッションの舞台となっている時代/ELの新たな【因縁】を獲得する。因縁内容表を用いて【因縁】を作成する。1D6を振って奇数が出ればポジティブ因縁内容表、偶数が出ればネガティブ因縁内容表を使用する。ただし因縁種別は「親友／因縁強度3」もしくは「恋人／因縁強度4」のいずれかから選択する。細かい設定は名前表や経歴表を振って決めること。",
                    "遠く離れて、思い起こされる人物。シーンプレイヤーのPCは好きな指定特技で判定を行う。判定に成功すれば、自分の出身時代/ELの新たな【因縁】を獲得する。初期作成時と同じく、因縁種別表と因縁内容表を用いて【因縁】を作成する。1D6を振って奇数が出ればポジティブ因縁内容表、偶数が出ればネガティブ因縁内容表を使用する。細かい設定は名前表や経歴表を振って決めること。",
                    "シーンプレイヤーは好きなPCを登場させることができる。宇宙開闢の女神があなたに微笑みかける。これを成功させれば歴史改変を修正できる、という理路が導き出される。シーンプレイヤーのPCはランダムに決定した指定特技の判定を行う。判定に成功すれば、登場しているPCのうち好きな一人の［改変度］が1D6点減少する。",
                    "シーンプレイヤーは好きなPCを登場させることができる。この時代にも心を落ち着けることができる場所を見つけることができた。何をして疲れを癒そう。シーンプレイヤーのPCはランダムに決定した指定特技で判定を行う。判定に成功すれば、シーンに登場しているPC全員の［疲労度］が1D6点減少する。",
                    "シーンプレイヤーは好きなPCを登場させることができる。宇宙開闢の女神があなたたちを抱擁する。これを成功させれば大幅に歴史改変を修正できる、という理路が導き出される。シーンプレイヤーのPCはランダムに決定した指定特技の判定を行う。判定に成功すれば、シーンに登場しているPC全員の［改変度］が1D6点減少する。",
                }),
                ["ST4"] = ("現代シーン表", new string[]
                {
                    "鳴り響く雷鳴と土砂降りの雨。嵐の路地でなすべきことをしろ。",
                    "その地域の有名観光地。旅行者たちが自撮りにいそしんでいる。気楽なものだ。",
                    "ブロックバスター映画のポスターが飾られた映画館。この時代の文化を知るには役に立つだろうか。",
                    "ノートPCを開く人々でひしめき合う、有名コーヒーチェーン店。ひとまず落ち着こう。焦ってもいいことはない。",
                    "古びた公園。撤去された遊具の支柱だけがさびたままに放置されており、少し寂しげだ。",
                    "駅前のショッピングビル。この建物だけで欲しいものが全て手に入る。なんて便利な時代なんだ。",
                    "鮮やかなネオンが照らす夜の繁華街。若者たちが新型の携帯通信端末を振り回しながら往来している。この時代が平和な証拠だろう。",
                    "様々なテナントが軒を連ねる大型ショッピングモール。物質にあふれたこの時代に心の安らぎはあるのか。",
                    "ヴィーガンメニューが並ぶスタイリッシュなレストラン。野菜だけを食べる習慣。この時代に流行りの多様性ってやつか。",
                    "旅行者で賑わう空港のラウンジ。離着陸する飛行機は流体力学を利用した単純な作りだが、安全性は高いらしい。",
                    "おだやかな風が通り抜ける。自然あふれるこんな場所も、この時代にあったのだな。",
                }),
                ["TT"] = ("タイムトラベル演出表", new string[]
                {
                    "科学者の実験で発生した七色の光を浴びてしまい、あなたの身体は分解され、違う時代で再構成された。",
                    "科学者が作った物の大きさを自在に変化させる量子物質に触れてしまい、あなたの体は微小レベルまで縮んでしまった。量子世界をしばらくさまよったあと、運よく元の大きさに戻れたが別の時代へと移動してしまっていた。",
                    "あなたは偶然手に入れた腕時計を気まぐれで腕に装着した。時刻を合わせようとをリューズを回すと、時空にゆがみが生じ、タイムトラベルしてしまった。腕時計はその力を使い果たし粉々にくだけた。",
                    "あなたの乗った乗物が突如超高速で走り始めた。それは光速に近づきつつあった。いや、すでに光速を超えているかもしれない。光速に限りなく近い速度なら未来に、光速を超えたスピードなら過去に移動してしまうだろう。",
                    "科学者が試乗をしてくれというので、乗り込んだ車が怪しげなメカが搭載されたタイムマシンだった。帰りの分の燃料は当然ない。",
                    "雷に打たれ、気を失った。そして目を覚ますと、あなたがもといた時代とは、違う時代へ来てしまっていた。",
                    "あなたの目の前に偶然ワームホールが出現する。ワームホールから放たれる引力には抗えず、あなたは時空を超えてしまう。",
                    "緑色に光る怪しげな石を見つけたあなたは、その美しさに魅入られてしまう。どれくらい時がたっただろうか。気づけはあなたは違う時代にいた。",
                    "偶然手に入れた謎の書物に書かれた呪文を読み上げた瞬間、あなたの身体は光に包まれ、違う時代へと転送されてしまった。",
                    "あなたは黒ずくめの服を着た謎の組織に拘束され、怪しげなクスリを飲まされてしまった。気づけばそこは違う時代だった。",
                    "自宅の机の引き出しを開けると、そこは混沌の空間が広がっていた。青い腕の先についた指のない白い手があなたを掴み（指がないにも関わらず、だ）、怪しげな板にあなたを乗せた。正体不明の青いずんぐりとしたフォルムの存在はその板に設置された操縦桿を操り、混沌の中を進んだ。しばらくしてあなたはもといた時代とは全く別の場所へ放り出された。",
                }),
            };

        private static readonly Dictionary<string, (string Name, string[] Items)> TABLES_MOD_1D =
            new Dictionary<string, (string Name, string[] Items)>
            {
                ["RT"] = ("帰還演出表", new string[]
                {
                    "この時代に来た方法と同じ演出で帰還できる。",
                    "この時代に来た方法と同じ演出で帰還できる。",
                    "この時代に来た方法と同じ演出で帰還できる。",
                    "少し目を閉じて、故郷へ想いを馳せる。眼を開けると懐かしい景色が広がっている。元の時代へ戻って来たのだ。",
                    "目の前の空間に別時代へのポータルが開く。それをくぐればあなたの住んでいた元の時代だ。",
                    "天から神々しい光が降りそそぐ。宇宙開闢の女神が微笑みかけると、あなたは強い光に包まれる。その光はあなたを元の時代へと導く。",
                }),
                ["CPT"] = ("ポジティブ因縁内容表", new string[]
                {
                    "共存。一緒にいて自然な関係だ。",
                    "互助。つらい時にはいつでもそばにいた。",
                    "同志。共に道を歩むかけがえのない仲間だ。",
                    "片愛。あなたは、相手のことが大好きだ。",
                    "相愛。お互いのことが大好きだ。",
                    "理解。何も言わなくても相手のことならなんでもわかる。",
                }),
                ["CNT"] = ("ネガティブ因縁内容表", new string[]
                {
                    "邪魔。なぜかいつも視界の端にいる。",
                    "不快。一緒にいるとちょっとイラつく。",
                    "厄介。関わりたくもないのに、いつもちょっかいを出してくる。",
                    "嫌悪。やることなすことすべてが気に食わない。",
                    "憎悪。過去の恨みか、激しい感情を持っている。",
                    "天敵。不倶戴天の敵、いつでも対立して喧嘩ばかりしている。",
                }),
                ["IT"] = ("アイテム決定表", new string[]
                {
                    "癒しの品。いつでも使用可能。好きなキャラクター（自身含む）の［疲労度］を1D6点減少させることができる。使用すると失われる。",
                    "癒しの品。いつでも使用可能。好きなキャラクター（自身含む）の［疲労度］を1D6点減少させることができる。使用すると失われる。",
                    "幸運の品。誰か（自身含む）の判定のサイコロを振った直後に使用可能。自身の［改変度］を1点増加すれば、その判定にプラス1の修正をつけることができる。使用すると失われる。",
                    "幸運の品。誰か（自身含む）の判定のサイコロを振った直後に使用可能。自身の［改変度］を1点増加すれば、その判定にプラス1の修正をつけることができる。使用すると失われる。",
                    "運命の品。誰か（自身含む）がシステムおよびシナリオで用意された表を使用してダイスを振った直後に使用可能。ダイスの結果を±1ずらすことができる。ただしその表に設定されていない値にずらすことはできない。使用すると失われる。",
                    "運命の品。誰か（自身含む）がシステムおよびシナリオで用意された表を使用してダイスを振った直後に使用可能。ダイスの結果を±1ずらすことができる。ただしその表に設定されていない値にずらすことはできない。使用すると失われる。",
                }),
                ["AGT"] = ("時代決定表", new string[]
                {
                    "原始時代／EL1",
                    "古代／EL2",
                    "中世時代／EL3",
                    "現代／EL4",
                    "超情報化時代／EL5",
                    "宇宙時代／EL6",
                }),
            };

        private static readonly Dictionary<string, (string Name, string[] Items)> TABLES_MOD_MINUS =
            new Dictionary<string, (string Name, string[] Items)>
            {
                ["SBET"] = ("重度バタフライエフェクト表", new string[]
                {
                    "消失。対象の存在自体が時空連続体から完全に消失する。対象を【因縁】としていた全てのPCはその【因縁】の消失欄にチェックを入れ、その対象との【因縁】内容がネガティブなら［疲労度］が「対象の因縁強度+3」点減少、ポジティブなら［疲労度］と［改変度］が「対象の因縁強度+3」点ずつ増加する。",
                    "消失。対象の存在自体が時空連続体から完全に消失する。対象を【因縁】としていた全てのPCはその【因縁】の消失欄にチェックを入れ、その対象との【因縁】内容がネガティブなら［疲労度］が「対象の因縁強度+3」点減少、ポジティブなら［疲労度］と［改変度］が「対象の因縁強度+3」点ずつ増加する。",
                    "消失の可能性。対象の存在自体があいまいになってしまう。",
                    "消失の可能性。対象の存在自体があいまいになってしまう。",
                    "時代変更。対象の存在している時代が変わってしまう。",
                    "時代変更。対象の存在している時代が変わってしまう。",
                    "死亡。対象は死亡してしまう。対象を【因縁】としていた全てのPCは対象の【因縁】の死亡欄にチェックを入れ、［疲労度］と［改変度］が「対象の因縁強度」点ずつ増加する。",
                    "死亡。対象は死亡してしまう。対象を【因縁】としていた全てのPCは対象の【因縁】の死亡欄にチェックを入れ、［疲労度］と［改変度］が「対象の因縁強度」点ずつ増加する。",
                    "別人化。対象はあなたとの因縁種別は維持したまま、完全な別人になってしまう。",
                    "因縁種別変化。対象との因縁種別が変わってしまう。",
                    "忘却。対象はあなたのことを忘れてしまう。",
                    "困窮。対象は経済的に困窮してしまい、その生活は荒れ果ててしまう。",
                    "病。対象は不治の病に侵されてしまう。",
                    "年齢変化。対象の年齢が変わってしまう。",
                    "性別反転。対象の性別が反転してしまう。",
                    "性格反転。対象の性格が反転する。",
                    "因縁内容変化。対象との因縁内容が変わってしまう。",
                    "宇宙開闢の女神が微笑む。何も変化は起こらなかった。",
                }),
                ["MBET"] = ("軽度バタフライエフェクト表", new string[]
                {
                    "激痛。耐え難い激しい痛みが全身を襲う。対象の［疲労度］が「対象の因縁強度」点、［改変度］が「対象の因縁強度+2D6」点増加する。",
                    "激痛。耐え難い激しい痛みが全身を襲う。対象の［疲労度］が「対象の因縁強度」点、［改変度］が「対象の因縁強度+2D6」点増加する。",
                    "吐血。激しいせき込みの末、吐血してしまう。",
                    "吐血。激しいせき込みの末、吐血してしまう。",
                    "頭痛。頭が割れるような激しい頭痛に襲われる。",
                    "頭痛。頭が割れるような激しい頭痛に襲われる。",
                    "時間結晶化。身体の一部が時間結晶化する。対象の［改変度］が「対象の因縁強度の半分」点増加する。",
                    "前兆。軽いめまいを感じる。嫌な前兆だ。対象の［疲労度］が1点、対象の［改変度］が「対象の因縁強度の半分」点増加する。",
                    "遭遇。自分自身に出会ってしまい時空連続体に亀裂が生じる。対象の［改変度］が「対象の因縁強度」点増加する。",
                    "半透明化。身体がはっきりと半透明になってきた。対象の［改変度］が「対象の因縁強度の半分」点増加する。",
                    "外見変化。対象の外見・服装に見た目が変化してしまう。対象の［改変度］が「対象の因縁強度の半分」点増加する。",
                    "記憶喪失。記憶が混濁し失われていく。対象の［改変度］が「対象の因縁強度の半分」点増加する。",
                    "半透明化の兆し。身体が半透明になってきた気がする。対象の［改変度］が1点増加する。",
                    "年齢変化。急激に対象の年齢が変化する。対象の［改変度］が1点増加する。",
                    "過去改変。自身の過去が少しだけ書き換わる。対象の［改変度］が1点増加する。",
                    "郷愁。ふと意識が宙に浮かび、目の前に故郷の風景が広がる。対象の［改変度］が1点増加する。",
                    "不安。落ち着かない気分になる。対象の［改変度］が1点増加する。",
                    "宇宙開闢の女神が微笑む。何も変化は起こらなかった。",
                }),
                ["TBET"] = ("タイムトラベラー重度バタフライエフェクト表", new string[]
                {
                    "消失。PCの存在自体が時空連続体から完全に消失する。PCはロストする。",
                    "消失。PCの存在自体が時空連続体から完全に消失する。PCはロストする。",
                    "消失の可能性。PCの存在自体があいまいになってしまう。PCはロストする可能性がある。",
                    "消失の可能性。PCの存在自体があいまいになってしまう。PCはロストする可能性がある。",
                    "時代変更。PCの存在している時代が変わってしまう。",
                    "時代変更。PCの存在している時代が変わってしまう。",
                    "永続時間結晶化。PCの身体の一部が永続的に時間結晶化する。好きな【タイムトラベラースキル】を追加で修得できる。",
                    "死亡。PCは死亡してしまう。PCはロストする。",
                    "忘却。PCは記憶を失う。全ての【因縁】の消失欄にチェックを入れる。",
                    "改変体質。今後、PCの［改変度］が増加する値が常にプラス1されてしまう。",
                    "コミュニケーション障害。今後、PCが行う接近判定に常にマイナス1の修正が付いてしまう。",
                    "病。PCは病に侵されてしまう。",
                    "年齢変化。急激にPCの年齢が変化する。",
                    "経歴変化。PCの経歴が変化してしまう。",
                    "性別反転。PCの性別が反転してしまう。",
                    "語尾変化。PCは時代特有の語尾が口をついて出てしまうようになる。",
                    "時代侵食。PCの過去に別の時代が少しだけ侵食する。",
                    "宇宙開闢の女神が微笑む。何も変化は起こらなかった。",
                }),
            };

        // D66テーブル（スタブ - 簡略化のため省略）
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var actionResult = ActionRoll(command, randomizer);
            if (actionResult != null) return actionResult;

            var tableResult = RollTables(command, TABLES);
            if (tableResult != null) return tableResult;

            var tableCommandResult = RollTableCommand(command, randomizer);
            if (tableCommandResult != null) return tableCommandResult;

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? RollTableCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([A-Za-z0-9]+)(([+]|-|=)((-\d+)|\d+))?$");
            if (!m.Success)
            {
                return null;
            }
            var cmd = m.Groups[1].Value;
            var op = m.Groups[3].Value;
            var value = string.IsNullOrEmpty(m.Groups[4].Value) ? 0 : Convert.ToInt32(m.Groups[4].Value);
            var result = GetTableResult(cmd, op, value, randomizer);
            if (result == null || result.Count == 0) return null;
            return Result.CreateBuilder(string.Join("\n", result)).Build();
        }

        private List<string> GetTableResult(string command, string op, int value, IRandomizer randomizer)
        {
            var result = new List<string>();
            if (TABLES_MOD_2D.ContainsKey(command))
            {
                result = GetTableIndex(TABLES_MOD_2D[command], op, value, 2, 6, randomizer);
            }
            else if (TABLES_MOD_1D.ContainsKey(command))
            {
                result = GetTableIndex(TABLES_MOD_1D[command], op, value, 1, 6, randomizer);
            }
            else if (TABLES_MOD_MINUS.ContainsKey(command))
            {
                result = GetTableMinusIndex(TABLES_MOD_MINUS[command], op, value, randomizer);
            }
            return result;
        }

        private (string TableName, int Value, string Body) GetTableInfo((string Name, string[] Items) table, int index)
        {
            // index is 1-based, clamp to valid range
            int clamped = Math.Max(1, Math.Min(table.Items.Length, index));
            return (table.Name, clamped, table.Items[clamped - 1]);
        }

        private List<string> GetTableMinusIndex((string Name, string[] Items) table, string op, int value, IRandomizer randomizer)
        {
            var index = 7;
            var modify = 0;
            List<int> diceList;
            var result = new List<string>();
            string text;
            if (op == "=")
            {
                index = value + 7;
                index = Math.Max(2, Math.Min(19, index));
                var info = GetTableInfo(table, index);
                text = $"{info.TableName}:{info.Value - 7} ＞ {info.Value - 7}:{info.Body}";
            }
            else
            {
                switch (op)
                {
                    case "+":
                        modify = value;
                        break;
                    case "-":
                        modify = value * -1;
                        break;
                }
                diceList = randomizer.RollBarabara(2, 6).ToList();
                index += diceList.Sum();
                index += modify;
                index = Math.Max(2, Math.Min(19, index));
                var info = GetTableInfo(table, index);
                if (modify != 0)
                {
                    text = $"{info.TableName}:{diceList.Sum()}[{string.Join(",", diceList)}]{op}{Math.Abs(modify)} ＞ {info.Value - 7}:{info.Body}";
                }
                else
                {
                    text = $"{info.TableName}:{diceList.Sum()}[{string.Join(",", diceList)}] ＞ {info.Value - 7}:{info.Body}";
                }
            }
            result.Add(text);
            return result;
        }

        private List<string> GetTableIndex((string Name, string[] Items) table, string op, int value, int diceCount, int diceType, IRandomizer randomizer)
        {
            var index = 0;
            var modify = 0;
            List<int> diceList;
            var result = new List<string>();
            string text;
            if (op == "=")
            {
                index = value;
                index = Math.Max(diceCount * 1, Math.Min(diceCount * diceType, index));
                var info = GetTableInfo(table, index);
                text = $"{info.TableName}:{info.Value} ＞ {info.Value}:{info.Body}";
            }
            else
            {
                switch (op)
                {
                    case "+":
                        modify = value;
                        break;
                    case "-":
                        modify = value * -1;
                        break;
                }
                diceList = randomizer.RollBarabara(diceCount, diceType).ToList();
                index += diceList.Sum();
                index += modify;
                index = Math.Max(diceCount * 1, Math.Min(diceCount * diceType, index));
                var info = GetTableInfo(table, index);
                if (modify != 0)
                {
                    text = $"{info.TableName}:{diceList.Sum()}[{string.Join(",", diceList)}]{op}{Math.Abs(modify)} ＞ {info.Value}:{info.Body}";
                }
                else
                {
                    text = $"{info.TableName}:{diceList.Sum()}[{string.Join(",", diceList)}] ＞ {info.Value}:{info.Body}";
                }
            }
            result.Add(text);
            return result;
        }

        // PP command regex: PP[@critical][#fumble][+/-modifier][>=target]
        private static readonly Regex PpCommandRegex = new Regex(
            @"^PP(?:@(\d+))?(?:#(\d+))?([+-]\d+)?(?:>=([\d]+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private Result? ActionRoll(string command, IRandomizer randomizer)
        {
            var m = PpCommandRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int critical = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 12;
            int fumble = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 2;
            int modifier = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 0;
            int? targetNumber = m.Groups[4].Success ? (int?)int.Parse(m.Groups[4].Value) : null;

            var diceList = randomizer.RollBarabara(2, 6);
            var diceTotal = diceList.Sum();
            var total = diceTotal + modifier;

            Result result;
            if (diceTotal <= fumble)
            {
                result = Result.CreateBuilder("ファンブル(判定失敗。改変度を1D6点増加してバタフライエフェクト発生).Build()")
                    .SetFumble(true).SetFailure(true).Build();
            }
            else if (diceTotal >= critical)
            {
                result = Result.CreateBuilder("スペシャル(判定成功。疲労度を1D6点減少してバタフライエフェクト発生).Build()")
                    .SetCritical(true).SetSuccess(true).Build();
            }
            else if (targetNumber == null)
            {
                result = Result.CreateBuilder("").Build();
            }
            else if (total >= targetNumber.Value)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            var modifierStr = Format.Modifier(modifier);
            var targetStr = targetNumber.HasValue ? $">={targetNumber.Value}" : "";
            var cmdStr = $"(PP{(m.Groups[1].Success ? $"@{critical}" : "")}{(m.Groups[2].Success ? $"#{fumble}" : "")}{modifierStr}{targetStr})";

            var parts = new List<string> { cmdStr, $"{diceTotal}[{string.Join(",", diceList)}]{modifierStr}", total.ToString() };
            if (!string.IsNullOrEmpty(result.Text))
            {
                parts.Add(result.Text);
            }

            var finalText = string.Join(" ＞ ", parts);
            return Result.CreateBuilder(finalText)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

    }
}
