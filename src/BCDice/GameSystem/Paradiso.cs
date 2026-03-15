using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// チェレステ色のパラディーゾ
    /// </summary>
    public sealed class Paradiso : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Paradiso Instance = new Paradiso();


        /// <inheritdoc/>
        public override string Id => "Paradiso";

        /// <inheritdoc/>
        public override string Name => "チェレステ色のパラディーゾ";

        /// <inheritdoc/>
        public override string SortKey => "ちえれすていろのはらていいそ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定　(nCPt[f]@c)、(nD20<=t[f]@c)　n:ダイス数（省略時:1）　t:目標値（省略時:14）　f:絶不調の追加ファンブル値　c:人機一体の追加クリティカル値
        　例）CP12　CP(13+1)　3CP12[18,19]@7
        ◆各種表
        　・ラジオ・マリエッタ表　RMT
        　・移動表　TOT
        　・探索表　EXT
        　・補給表　SUT
        ◆ダメージチェック　(DCa[20,30])　a:【攻撃力】、[20]:20mm機銃追加、[30]:30mmガンポッド追加
        　例）DC4:【攻撃力】4でダメージチェック　DC5[20]:【攻撃力】5でダメージチェック、うち1つは20mm機銃　DC5[20,30]:【攻撃力】5でダメージチェック、うち1つは20mm機銃、うち1つは30mmガンポッド
        ";

        private static readonly Regex DD20DRegex = new Regex(
            @"^(\d+)?D20<=(\d+)?(\[(\d+)(,(\d+))?\])?(@(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DCpDRegex = new Regex(
            @"^(\d+)?CP(\d+)?(\[(\d+)(,(\d+))?\])?(@(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DcDDRegex = new Regex(
            @"^DC(\d+)(\[(\d+)(,(\d+))?(,(\d+))?(,(\d+))?(,(\d+))?(,(\d+))?\])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // 通常判定
            var result = GetJudgeResult(command, randomizer);
            if (result != null) return result;

            // ダメージチェック
            result = GetDamageResult(command, randomizer);
            if (result != null) return result;

            // テーブル
            var upperCommand = command.ToUpperInvariant();
            switch (upperCommand)
            {
                case "RMT":
                    return ResultFromText(GetRadiomariettaTable(randomizer));
                case "TOT":
                    return ResultFromText(GetTakeoffTable(randomizer));
                case "EXT":
                    return ResultFromText(GetExplorationTable(randomizer));
                case "SUT":
                    return ResultFromText(GetFlightsupplyTable(randomizer));
            }

            return null;
        }

        private static Result? ResultFromText(string? text)
        {
            if (text == null) return null;
            return Result.CreateBuilder(text).Build();
        }

        private Result? GetJudgeResult(string command, IRandomizer randomizer)
        {
            int number, target, fumble1, fumble2, critical;

            var match = DD20DRegex.Match(command);
            if (match.Success)
            {
                number = ToIntOrDefault(match.Groups[1].Value, 1);
                target = ToIntOrDefault(match.Groups[2].Value, 14);
                fumble1 = ToIntOrDefault(match.Groups[4].Value, 21);
                fumble2 = ToIntOrDefault(match.Groups[6].Value, 21);
                critical = ToIntOrDefault(match.Groups[8].Value, 21);
            }
            else
            {
                match = DCpDRegex.Match(command);
                if (match.Success)
                {
                    number = ToIntOrDefault(match.Groups[1].Value, 1);
                    target = ToIntOrDefault(match.Groups[2].Value, 14);
                    fumble1 = ToIntOrDefault(match.Groups[4].Value, 21);
                    fumble2 = ToIntOrDefault(match.Groups[6].Value, 21);
                    critical = ToIntOrDefault(match.Groups[8].Value, 21);
                }
                else
                {
                    return null;
                }
            }

            var dice = 0;
            var dicetext = "";
            var critFlg = false;
            var fumbFlg = false;
            var succFlg = false;

            for (var i = 0; i < number; i++)
            {
                dice = randomizer.RollOnce(20);

                if (dicetext == "")
                    dicetext = dice.ToString();
                else
                    dicetext = dicetext + "," + dice.ToString();

                if (dice == 1 || dice == critical)
                {
                    critFlg = true;
                }
                else if (dice == 20 || dice == fumble1 || dice == fumble2)
                {
                    fumbFlg = true;
                }
                else if (dice <= target)
                {
                    succFlg = true;
                }
            }

            string result;
            if (critFlg)
                result = "クリティカル";
            else if (fumbFlg)
                result = "ファンブル";
            else if (succFlg)
                result = "成功";
            else
                result = "失敗";

            var text = $"({number}D20 目標値{target}) ＞ ({dicetext}) ＞ {result}";
            return Result.CreateBuilder(text).Build();
        }

        private Result? GetDamageResult(string command, IRandomizer randomizer)
        {
            var match = DcDDRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            var attack = ToIntOrDefault(match.Groups[1].Value, 1);
            var biggun = new List<int>();
            biggun.Add(ToIntOrDefault(match.Groups[3].Value, 0));
            biggun.Add(ToIntOrDefault(match.Groups[5].Value, 0));
            biggun.Add(ToIntOrDefault(match.Groups[7].Value, 0));
            biggun.Add(ToIntOrDefault(match.Groups[9].Value, 0));
            biggun.Add(ToIntOrDefault(match.Groups[11].Value, 0));
            biggun.Add(ToIntOrDefault(match.Groups[13].Value, 0));

            var dice = 0;
            var dicetext = "";
            var damage = new int[20];
            var doubledam = 0;
            var tripledam = 0;

            foreach (var bg in biggun)
            {
                switch (bg)
                {
                    case 30:
                        tripledam += 1;
                        break;
                    case 20:
                        doubledam += 1;
                        break;
                }
            }

            for (var i = 0; i < attack; i++)
            {
                dice = randomizer.RollOnce(20);

                if (dicetext == "")
                    dicetext = dice.ToString();
                else
                    dicetext = dicetext + "," + dice.ToString();

                if (tripledam >= 1)
                {
                    damage[dice - 1] += 3;
                    tripledam += -1;
                    dicetext += "【30mm】";
                }
                else if (doubledam >= 1)
                {
                    damage[dice - 1] += 2;
                    doubledam += -1;
                    dicetext += "【20mm】";
                }
                else
                {
                    damage[dice - 1] += 1;
                }
            }

            var result = $"\n{damage[0]}{damage[1]}{damage[2]}{damage[3]}{damage[4]}" +
                         $"\n{damage[5]}{damage[6]}{damage[7]}{damage[8]}{damage[9]}" +
                         $"\n{damage[10]}{damage[11]}{damage[12]}{damage[13]}{damage[14]}" +
                         $"\n{damage[15]}{damage[16]}{damage[17]}{damage[18]}{damage[19]}";
            var text = $"攻撃力{attack}ダメージチェック ＞ ({dicetext}) ＞ {result}";
            return Result.CreateBuilder(text).Build();
        }

        private string GetRadiomariettaTable(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(20);
            string text;
            switch (dice)
            {
                case 1:
                    text = "「なんてこった！　ここで事故のお知らせだ！」\n通行止め……。ランダムなマス1つを決定する。この一日中、そのマスに移動する事はできない。";
                    break;
                case 2:
                case 3:
                case 4:
                    text = "「今日はまたずいぶんと湿気てるねぇ……。古傷がある人は要注意だよ」\n天候が悪い。この一日中、「●移動」のアクションで移動できるマス数は常に1マス低くなる。";
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    text = "「日常は至上！　異常は退場！　なんにもないからラジオは以上！　……なんつて」\nいつもどおりの日々。のんびりとした風で、何事もなし。";
                    break;
                case 11:
                case 12:
                    text = "「それじゃ、本日のメインコーナー。行ってみよう！」\n軽妙なトーク。PC全員の【乗り手コンディション】が1点小さくなる。";
                    break;
                case 13:
                case 14:
                case 15:
                    text = "「いーなー、こんな日はボクも飛んでみたい気分だよ！　ブオノあたりまでバーッとね！」\nとんでもなく快晴で絶好のフライト日和。【機体コンディション】【乗り手コンディション】がそれぞれ1点小さくなる。";
                    break;
                case 16:
                    text = "「店頭で言えば嬉しい値引き。本日のラッキーワードをメモする用意はできたかい？」\nおトクな情報。この一日中、各PCごとに一回ずつ、「価格」の効果値が2低いものとして効果を処理できる（最低値0）";
                    break;
                case 17:
                    text = "「いっやー……熱演だったね。もーぅ次回が待ちきれなぁい！！」\nラジオドラマが神回だった。その日一日に行う「交流」で獲得できる【キズナ】の点数が+1される。";
                    break;
                case 18:
                    text = "「イエス！ナイス！エレガンス！あのサーカス団が帰ってくる！」\nサーカス団がやってくる！ランダムなマス1つを決定する。\nこの一日中、そのマスは「娯楽施設：5」（P.55）の効果を得る。";
                    break;
                case 19:
                    text = "「ラジオネーム、ハプニングさんからのお便り！　おっとぉ、これは興味深い相談だ」\nラジオの話している内容から手がかりが見つかる。[手がかり]が1箇所追加で配置される。";
                    break;
                case 20:
                    text = "「今夜は素敵なパーリィデイ！　みんな！今夜の仮装を何にするかはもう決めてるかな？」\n酒場でパーティだ！「酒場」のスポット効果を持つスポットに「レストラン：「パーティ」」が追加される。";
                    break;
                default:
                    text = "";
                    break;
            }
            return "ラジオマリエッタ表" + "(" + dice.ToString() + ")：" + text;
        }

        private string GetTakeoffTable(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(20);
            string text;
            switch (dice)
            {
                case 1:
                    text = "エンジンがぶっ壊れた！ただちに【機体コンディション】が「20」となり、このターン中は2つ目のアクションも含め「●移動」することができない。";
                    break;
                case 2:
                    text = "離水に失敗した！　キミの愛機のダメージマップ上の任意の「翼」部位のダメージボックスに1点のダメージを与え、このターン中は2つ目のアクションも含め「●移動」することができない。";
                    break;
                case 3:
                    text = "軽いエンジントラブル。このアクションでは移動することができない。";
                    break;
                case 4:
                    text = "同業者に遭遇。しかし煽られて曲芸飛行につきあわされる。\n任意の方向に強制的に3マス移動し、【物資点】3点を失う。";
                    break;
                case 5:
                    text = "道を間違えたらしい。【物資点】を5点消費し、ランダムな方向に1マス移動する効果を3回繰り返す。";
                    break;
                case 6:
                    text = "気づいたらオイル漏れを起こしていた！【物資点】を3点消費する。その後、1マスにつき1点の【物資点】を消費して最大4マスまで移動できる。";
                    break;
                case 7:
                    text = "あいにくのにわか雨。あまり飛びたくないなあ。1マスにつき1点の【物資点】を消費して最大2マスまで移動できる。";
                    break;
                case 8:
                    text = "唐突な襲撃。一撃加えたあと、謎の襲撃者はいずこかへ去っていった……。命中判定の達成値が12であると扱う、【火力】3のダメージチェックを受ける。その後、1マスにつき1点の【物資点】を消費して最大4マスまで移動できる。";
                    break;
                case 9:
                    text = "んー、少し調子が悪いかな？　1マスにつき1点の【物資点】を消費して最大3マスまで移動できる。";
                    break;
                case 10:
                case 11:
                case 12:
                    text = "順調な空の旅。1マスにつき1点の【物資点】を消費して最大5マスまで移動できる。";
                    break;
                case 13:
                    text = "島巡りの観光艇と遭遇。ちやほやされていい気分。1マスにつき1点の【物資点】を消費して最大5マスまで移動できる上、キミの【乗り手コンディション】を2点までの任意の点数下げる事ができる。";
                    break;
                case 14:
                    text = "同業者と遭遇。1マスにつき1点の【物資点】を消費して最大5マスまで移動できる上、同業者は「キミへの【キズナ】」を1点得る。同業者はこのセッション中、キミが望む場面でキミに「判定支援」を行ってくれる。";
                    break;
                case 15:
                    text = "すごく調子がいいぞ！1マスにつき1点の【物資点】を消費して最大7マスまで移動できる上、キミの【機体コンディション】を2点までの任意の点数下げる事ができる。";
                    break;
                case 16:
                    text = "すごく調子がいいぞ！1マスにつき1点の【物資点】を消費して最大5マスまで移動できる上、このアクションがこのターンに行う1回目のアクションである場合、2回目のアクションでも続けて「●移動」を行う事ができる。";
                    break;
                case 17:
                    text = "通りかかった先に思わぬ情報が！1マスにつき1点の【物資点】を消費して最大5マスまで移動できる上、このアクションがこのターンに行う1回目のアクションである場合、2回目のアクションでは今いるマスに[手がかり]が配置されているものとして「●探索」が行える。";
                    break;
                case 18:
                    text = "酒場が恋しい……。【物資点】を5点消費し、即座に同じ「クエストマップ」内の「酒場」のスポット効果を持つマスに移動する。";
                    break;
                case 19:
                    text = "アジトが恋しい……。【物資点】を5点消費し、即座に同じ「クエストマップ」内の任意のキミの「アジト」に移動する。";
                    break;
                case 20:
                    text = "仲間が恋しい……。【物資点】を5点消費し、即座に任意のPC一人のいる場所に移動する。";
                    break;
                default:
                    text = "";
                    break;
            }
            return "移動表" + "(" + dice.ToString() + ")：" + text;
        }

        private string GetExplorationTable(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(20);
            string text;
            switch (dice)
            {
                case 1:
                    text = "クソっ！このマスに付与されていた[手がかり]を失う。";
                    break;
                case 2:
                    text = "「ツケ払いやがれ！」見に覚えがあるかないか。キミに詰め寄ってくるヤツがいる。【物資点】を10点消費するか、「ツケを伸ばす」のどちらかを選択する。ツケを伸ばすを選択した場合、次にキミが行う「●探索」のアクションでも、探索表の結果は参照せず、自動的にこの効果が適用される。";
                    break;
                case 3:
                    text = "謎は深まる。このマスに付与されていた[手がかり]を失い、ランダムな場所に再付与する。【情報点】は得られない。";
                    break;
                case 4:
                    text = "コネクションは大事だ。「支援チェック」をチェックしていない【キズナ】が1点以上存在すれば、その「支援チェック」を入れたあと、このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。";
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    text = "情報を手に入れるためには、少し骨を折る必要がありそうだ。好きな能力値を2つ組み合わせて｛探索判定｝を行う。成功すればこのマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。";
                    break;
                case 9:
                    text = "情報を提供してくれるというアイツは見返りを要求してきた。【物資点】を4点消費できる。そうした場合、このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。";
                    break;
                case 10:
                case 11:
                case 12:
                case 13:
                    text = "危なげなく情報ゲット。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。";
                    break;
                case 14:
                    text = "手がかりを追っている事を話すと、ソイツは協力を持ちかけてきてくれた。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。さらに、【物資点】を5点獲得する。";
                    break;
                case 15:
                    text = "手がかりを追っている事を話すと、ソイツは協力を持ちかけてきてくれた。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。さらに、アイテム「チケット」（P.69）を入手する。";
                    break;
                case 16:
                    text = "昔の仲間から手がかりについて聞くことになった。ついでに積もる話も少々。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。さらに同業者は「キミへの【キズナ】」を1点得る。同業者はこのセッション中、キミが望む場面でキミに「判定支援」を行ってくれる。";
                    break;
                case 17:
                    text = "空軍にいる友人から手がかりについて聞くことになった。「なあ、お前もフラフラしてないで空軍に入ったらどうだ？」耳に痛い。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。さらに、アイテム「空軍のツテ」（P.69）を入手する。";
                    break;
                case 18:
                    text = "手がかりを追っていたら他にもボロボロと……。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。さらに、1D20を二回振り、この「クエストマップ」上のランダムなマス2つを求める。それらのマスに[手がかり]が付与されていなければ、[手がかり]を付与する。";
                    break;
                case 19:
                    text = "あっさり情報を掴むことができてしまった。このマスに付与されていた[手がかり]を失い、【情報点】を1点獲得する。この「●探索」ではアクションを消費せず、追加で別のアクションを宣言する事ができる。";
                    break;
                case 20:
                    text = "これは重要な手がかりだ！　このマスに付与されていた[手がかり]を失い、【情報点】を2点獲得する。";
                    break;
                default:
                    text = "";
                    break;
            }
            return "探索表" + "(" + dice.ToString() + ")：" + text;
        }

        private string GetFlightsupplyTable(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(20);
            string text;
            switch (dice)
            {
                case 1:
                    text = "……えっ？！　キミの【物資点】は0点となる。";
                    break;
                case 2:
                    text = "おいおい勘弁してくれよ……。このアクションがそのセグメントの一回目のアクションだった場合、キミは2回目のアクションを行えない。その後【物資点】を5点獲得する。";
                    break;
                case 3:
                    text = "取材に巻き込まれる。【物資点】は獲得できないが、記者の発言からはぽろりとなにかが見えたような?1D20を振り、出た目に対応したマスに「手がかり」を1つ配置する。";
                    break;
                case 4:
                    text = "成果ゼロ。ま、こんな日もあるかな。【物資点】は獲得できない。";
                    break;
                case 5:
                    text = "うまいこと補給できなかった。【物資点】を5点獲得する。";
                    break;
                case 6:
                    text = "「一稼ぎと言ったらこれだろ?」と声をかけてくる悪友たち。「カジノ」(『基本ルールブック』P.55)のスポット効果を即座に適用する。ただしこの処理では判定の失敗により「刑務所」のスポット効果を持つスポットに移動する効果は発生せず、代わりに「酒場」のスポット効果を持つスポットに移動した上で、自身が持つ全ての【キズナ】の「支援チェック」にチェックを入れる。その後、次のセグメントが終了するまでの間アクションは行えない。";
                    break;
                case 7:
                case 8:
                case 9:
                    text = "のんびり釣りといこう。釣果は運次第だ。1D20を振り、出た目と同じ数だけ【物資点】を獲得する。";
                    break;
                case 10:
                case 11:
                case 12:
                    text = "なにごともなく補給が完了する。【物資点】を10点獲得する。";
                    break;
                case 13:
                    text = "ラジオの音が聞こえる。PCが望むなら、1D20を振り、出た目を「ラジオ・マリエッタ表」(『基本ルールブック』P.29)に照らし合わせて、その結果を反映する。これ以後、朝セグメントで振られたラジオ・マリエッタ表の効果は失われる。その後【物資点】を10点獲得する。";
                    break;
                case 14:
                    text = "補給の合間、ちょっと口寂しくなってしまって露店へ。【物資点】を8点獲得し、アイテム「レモネード」(『基本ルールブック』P.69)を入手する。";
                    break;
                case 15:
                    text = "補給の合間、軽くメンテナンス。【機体コンディション】を1点下げることができる。その後【物資点】を10点獲得する。";
                    break;
                case 16:
                    text = "補給の合間、店主と軽く談笑。【乗り手コンデイション】を1点下げることができる。その後【物資点】を10点獲得する。";
                    break;
                case 17:
                    text = "補給の合間、仲間に軽く挨拶しておこうか。同じマスに他のPCがいた場合、そのPC1人への【キズナ】を1点獲得する。その後【物資点】を10点獲得する。";
                    break;
                case 18:
                    text = "補給の合間に通りがかった相手と意気投合。相手はキミへの【キズナ】を1点取得する。【物資点】を10点獲得する。";
                    break;
                case 19:
                    text = "あっさり補給が終わってしまった。どうしようかな。この補給ではアクションを消費せず、【物資点】を10点獲得する。";
                    break;
                case 20:
                    text = "降って湧いた幸運！【物資点】が20点になる。";
                    break;
                default:
                    text = "";
                    break;
            }
            return "補給表" + "(" + dice.ToString() + ")：" + text;
        }

        private static int ToIntOrDefault(string? value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;
            return int.TryParse(value, out var result) ? result : defaultValue;
        }
    }
}
