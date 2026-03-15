using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// キミトエール！
    /// </summary>
    public sealed class KimitoYell : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KimitoYell Instance = new KimitoYell();

        /// <inheritdoc/>
        public override string Id => "KimitoYell";

        /// <inheritdoc/>
        public override string Name => "キミトエール！";

        /// <inheritdoc/>
        public override string SortKey => "きみとええる";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 判定 （nKY6 / nKY10）
        指定された能力値分（n個）のダイスを使って判定を行います。
        ・nKY6…「有利」を得ていない場合、6面ダイスをn個振って判定します。
        ・nKY10…「有利」を得ている場合、10面ダイスをn個振って判定します。
        6もしくは10の出目があればスペシャル。1の出目があればファンブル。
        スペシャルとファンブルは同時に発生した場合、両方の処理を行う。

        ■ 表
        ─ ファンブル表（FT）
          ファンブル時の処理を決定します。

        ─ 新しい出会いを求める
          ─ 一括 新しい出会い表（NMTA） # New Meet Table
            その後の表を含めてすべて同時に決定します。
            ひとつひとつ振る場合には下記のコマンドを使用してください。
          ─ 新しい出会い表（NMT） # New Meet Table
          ─ 偶然出会った表（MCT） # Meet by Chance Table
          ─ 交流のなかった身近な人表（CIT） # someone Close to you but no Interaction Table
          ─ 助けてくれた人表（HYT） # someone Help You table
          ─ どんな人だったか表（TLT） # what's They Like Table
          ─ 変わった人だった表（EPT） # Eccentric Person Table

        ─ ランダム命名表
          ─ フルネーム一括生成（FNG） # Full Name Generation
          ─ 名字表（LNT） # LastName Table
          ─ 名前表（FNT） # FirstName Table
          ─ 日本名字表1（JLTO） # Japanese Lastname Table One
          ─ 日本名字表2（JLTT） # Japanese Lastname Table Two
          ─ カタカナ名字表（FLT） # Foreien Lastname Table
          ─ 日本名前表1（JFTO） # Japanese Firstname Table One
          ─ 日本名前表2（JFTT） # Japanese Firstname Table Two
          ─ カタカナ名前表（FFT） # Foreien Firstname Table
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (Regex.IsMatch(command, @"^(\d+)KY(6|10)$"))
            {
                return RollKyJudge(command, randomizer);
            }
            else if (Regex.IsMatch(command, @"^FT$") && !Regex.IsMatch(command, @"JFTO|JFTT|FFT"))
            {
                return RollFumble(command, randomizer);
            }
            else if (Regex.IsMatch(command, @"^(NMTA|NMT|MCT|CIT|HYT|TLT|EPT)$"))
            {
                return GenerateNewEncounter(command, randomizer);
            }
            else if (Regex.IsMatch(command, @"^(FNG|LNT|FNT|JLTO|JLTT|FLT|JFTO|JFTT|FFT)$"))
            {
                return GenerateNewName(command, randomizer);
            }

            return null;
        }

        private Result? RollKyJudge(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)KY(6|10)$");
            if (!m.Success)
            {
                return null;
            }

            var nOfDiceside = int.Parse(m.Groups[2].Value) == 10 ? 10 : 6;
            var nOfRolldice = int.Parse(m.Groups[1].Value);

            var successDices = new[] { 4, 5, 6, 7, 8, 9, 10 };
            var specialDices = new[] { 6, 10 };
            var fumbleDices = new[] { 1 };

            var txtSpecial = "スペシャル（がんばりが1点上昇！）";
            var txtFumble = "ファンブル（ファンブル表：FTを振る）";
            var txtSuccess = "成功";
            var txtFailure = "失敗";

            var resultTxts = new List<string>();
            var isCritical = false;
            var isFumble = false;
            var isSuccess = false;
            var isFailure = false;

            var diceList = randomizer.RollBarabara(nOfRolldice, nOfDiceside);

            isCritical = diceList.Intersect(specialDices).Any();
            isFumble = diceList.Intersect(fumbleDices).Any();
            isSuccess = diceList.Intersect(successDices).Any();
            isFailure = !diceList.Intersect(successDices).Any();

            if (isSuccess)
            {
                resultTxts.Add(txtSuccess);
            }
            if (isFailure)
            {
                resultTxts.Add(txtFailure);
            }
            if (isCritical)
            {
                resultTxts.Add(txtSpecial);
            }
            if (isFumble)
            {
                resultTxts.Add(txtFumble);
            }

            var text = $"({command}) ＞ [{string.Join(",", diceList)}] ＞ {string.Join("・", resultTxts)}";

            return Result.CreateBuilder(text)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        private Result? RollFumble(string command, IRandomizer randomizer)
        {
            var fumbledice = randomizer.RollOnce(6);
            var fumbletext = FTABLE[fumbledice - 1];

            return Result.CreateBuilder($"ファンブル表({command}:{fumbledice}).Build() ＞ {fumbletext}").Build();
        }

        private Result? GenerateNewEncounter(string command, IRandomizer randomizer)
        {
            string[]? table0 = null;
            string? table0txt = null;
            int table0dice;
            string[]? table1 = null;
            string[]? table2 = null;

            if (Regex.IsMatch(command, @"NMTA|NMT"))
            {
                table0 = NMTABLE;
                table0dice = randomizer.RollOnce(6);
                table0txt = table0[table0dice];
                if (table0dice == 1)
                {
                    table1 = MCTABLE;
                    table2 = TLTABLE;
                }
                else if (table0dice == 2)
                {
                    table1 = MCTABLE;
                    table2 = EPTABLE;
                }
                else if (table0dice == 3)
                {
                    table1 = CITABLE;
                    table2 = TLTABLE;
                }
                else if (table0dice == 4)
                {
                    table1 = CITABLE;
                    table2 = EPTABLE;
                }
                else if (table0dice == 5)
                {
                    table1 = HYTABLE;
                    table2 = TLTABLE;
                }
                else
                {
                    table1 = HYTABLE;
                    table2 = EPTABLE;
                }
            }
            else
            {
                table0dice = randomizer.RollOnce(10);
            }

            if (Regex.IsMatch(command, @"^MCT$"))
            {
                table0 = MCTABLE;
                table0txt = table0[table0dice];
            }
            else if (Regex.IsMatch(command, @"^CIT$"))
            {
                table0 = CITABLE;
                table0txt = table0[table0dice];
            }
            else if (Regex.IsMatch(command, @"^HYT$"))
            {
                table0 = HYTABLE;
                table0txt = table0[table0dice];
            }
            else if (Regex.IsMatch(command, @"^TLT$"))
            {
                table0 = TLTABLE;
                table0txt = table0[table0dice];
            }
            else if (Regex.IsMatch(command, @"^EPT$"))
            {
                table0 = EPTABLE;
                table0txt = table0[table0dice];
            }

            string resulttxt;
            if (Regex.IsMatch(command, @"NMTA"))
            {
                var table1dice = randomizer.RollOnce(10);
                var table1txt = table1![table1dice];
                var table2dice = randomizer.RollOnce(10);
                var table2txt = table2![table2dice];

                resulttxt = $"{table0![0]}({table0dice}) ＞ {table0txt}\n{table1[0]}({table1dice}) ＞ {table1txt}\n{table2[0]}({table2dice}) ＞ {table2txt}";
            }
            else
            {
                resulttxt = $"{table0![0]}({table0dice}) ＞ {table0txt}";
            }

            return Result.CreateBuilder(resulttxt).Build();
        }

        private Result? GenerateNewName(string command, IRandomizer randomizer)
        {
            int nametabledice1 = 0;
            string? result1 = null;
            string? result2 = null;
            string? result3 = null;
            string? result4 = null;

            if (Regex.IsMatch(command, @"^FNG$"))
            {
                nametabledice1 = randomizer.RollOnce(6);
                result1 = $"{LNTABLE[0]}({nametabledice1}) ＞ {LNTABLE[nametabledice1]}";
                var nametabledice2 = randomizer.RollOnce(6);
                result2 = $"{FNTABLE[0]}({nametabledice2}) ＞ {FNTABLE[nametabledice2]}";

                if (new[] { 1, 2 }.Contains(nametabledice1))
                {
                    result3 = RollTable("JLTO", randomizer);
                }
                else if (new[] { 3, 4 }.Contains(nametabledice1))
                {
                    result3 = RollTable("JLTT", randomizer);
                }
                else
                {
                    result3 = RollTable("FLT", randomizer);
                }

                if (new[] { 1, 2 }.Contains(nametabledice1))
                {
                    result4 = RollTable("JFTO", randomizer);
                }
                else if (new[] { 3, 4 }.Contains(nametabledice1))
                {
                    result4 = RollTable("JFTT", randomizer);
                }
                else
                {
                    result4 = RollTable("FFT", randomizer);
                }
            }

            if (Regex.IsMatch(command, @"^LNT$"))
            {
                nametabledice1 = randomizer.RollOnce(6);
                result1 = $"{LNTABLE[0]}({nametabledice1}) ＞ {LNTABLE[nametabledice1]}";
                if (new[] { 1, 2 }.Contains(nametabledice1))
                {
                    result2 = RollTable("JLTO", randomizer);
                }
                else if (new[] { 3, 4 }.Contains(nametabledice1))
                {
                    result2 = RollTable("JLTT", randomizer);
                }
                else
                {
                    result2 = RollTable("FLT", randomizer);
                }
            }
            else if (Regex.IsMatch(command, @"^FNT$"))
            {
                nametabledice1 = randomizer.RollOnce(6);
                result1 = $"{FNTABLE[0]}({nametabledice1}) ＞ {FNTABLE[nametabledice1]}";
                if (new[] { 1, 2 }.Contains(nametabledice1))
                {
                    result2 = RollTable("JFTO", randomizer);
                }
                else if (new[] { 3, 4 }.Contains(nametabledice1))
                {
                    result2 = RollTable("JFTT", randomizer);
                }
                else
                {
                    result2 = RollTable("FFT", randomizer);
                }
            }

            if (Regex.IsMatch(command, @"^(JLTO|JLTT|FLT|JFTO|JFTT|FFT)$"))
            {
                result1 = RollTable(command, randomizer);
            }

            string resulttxt;
            if (result4 != null)
            {
                resulttxt = result1 + "\n" + result3 + "\n" + result2 + "\n" + result4;
            }
            else if (result2 != null)
            {
                resulttxt = result1 + "\n" + result2;
            }
            else
            {
                resulttxt = result1!;
            }

            return Result.CreateBuilder(resulttxt).Build();
        }

        private string? RollTable(string command, IRandomizer randomizer)
        {
            if (!TABLES.TryGetValue(command, out var table))
            {
                return null;
            }

            return table.Roll(randomizer).Text;
        }

        // Fumble Table
        private static readonly string[] FTABLE = new[]
        {
            "とんでもない大失敗！　魔法でないと取り返しがつかない！　出目にかかわらず、「魔法の提案」をしない限り判定は失敗になる。",
            "もうちょっと何かが足りない。自分の【がんばり】を1点消費することで、出目にかかわらず判定を成功にできる。【がんばり】を消費しなければ出目にかかわらず判定が失敗になる。",
            "トラブルが発生したけど「大切な想い」を思い出して何とか乗り切った。大切だと思う世界から学んだこと、大切だと思いたい世界に対する気持ちを思い出して、なんとかしよう。自分の持っているカードの「大切な想い」を1つ選んで〇で囲む。〇で囲めない場合、判定は出目にかかわらず失敗になる。",
            "トラブルが発生した。こんな自分を、あの人が見たらどう思うかな。自分が持っているカードに、「大切な想い」を考えて1つ書き込む。",
            "トラブルが発生したけど、偶然にも自分の「守りたい人」が助けてくれた。あるいは、「守りたい人」の教えてくれたことが役立った。ありがとう……。",
            "ちょっとヒヤリとする瞬間があったけど、何も起こらなかった。よかった。",
        };

        // New Meet Table
        private static readonly string[] NMTABLE = new[]
        {
            "新しい出会い表",
            "「偶然出会った表（MCT）」と「どんな人だったか表（TLT）」を使用してNPCを作成する。",
            "「偶然出会った表（MCT）」と「変わった人だった表（EPT）」を使用してNPCを作成する。",
            "「交流のなかった身近な人表（CIT）」と「どんな人だったか表（TLT）」を使用してNPCを作成する。",
            "「交流のなかった身近な人表（CIT）」と「変わった人だった表（EPT）」を使用してNPCを作成する。",
            "「助けてくれた人表（HYT）」と「どんな人だったか表（TLT）」を使用してNPCを作成する。",
            "「助けてくれた人表（HYT）」と「変わった人だった表（EPT）」を使用してNPCを作成する。",
        };

        // Meet by Chance Table
        private static readonly string[] MCTABLE = new[]
        {
            "偶然出会った表",
            "何らかの事件や事故が起こり、それに巻き込まれた人を助けるために動いた。そのお礼をしたいと声をかけられた。",
            "急に振り出した雨。屋根のある所に雨宿りをした際に、同じく雨宿りをしていた人物と話をした。",
            "図書館の資料を集めていたところ、偶然にも同じ資料を借りようとしていた人物とバッティングしてしまった。どちらが先にするか話し合った。",
            "魔法やオカルトの事を調べるのが趣味らしく、ちょっとした魔法の事件が起こった場所をうろついていた。巻き込まれないように声をかけたら、自分が怪しまれた。",
            "街を歩いている時に、うずくまっている人を見つけた。何があったのかと声をかけてみると、何か困っていることがあるらしい。それを助けた。",
            "偶然にも稼働していない魔具を見つけたので、回収するために持ち主と話をすることになった。",
            "以前、魔具関係の事件で駆け回った時の自分を見かけた人がいたらしい。その時の顔が印象に残ったらしく、何をしていたのか聞かれた。",
            "「MAGIA」にたちよったところ、知らない店員がいた。新しく雇ったアルバイトらしく、何をしていたのか聞かれた。",
            "たまたま立ち寄った飲食店の店長から、試供品が提供されて、味の感想を求められた。どうやら新メニューを作りたいらしく、いろいろな人の意見を聞いているらしい。",
            "「守りたい人」に会いに行ったら、「守りたい人」の親友と名乗る人と出会った。自分の知らない「守りたい人」について話してくれた。",
        };

        // someone Close to you but no Interaction Table
        private static readonly string[] CITABLE = new[]
        {
            "交流のなかった身近な人表",
            "その人は自分の親戚で、家の用事で出かけたときに親などから紹介をされた。話をしてみたら、自分の興味と同じものを研究していた。",
            "親などに頼まれて、ご近所に挨拶へ伺った。その人は近所で見かけることがあったが、不思議と今まで交流がなく、よくわからない人だった。",
            "「守りたい人」がたまに話題に出す友人と、「守りたい人」に紹介される形で会う機会ができた。この人はどんな人なのか、見てみよう。",
            "きょうだいの知り合いで、挨拶ぐらいはしていたけど、二人きりになったのは初めてだった。きょうだいも用事でいないし、どう話したものか。",
            "学業やスポーツの関係で、遠くの国で暮らしていたきょうだい（あるいはいとこ）が帰ってきた。優秀な成績を修めて、世間からも注目されている人物にどう接しようか。",
            "昔はずっと一緒に遊んでいた幼馴染だったけど、事情があって最近まで遠くに出かけていた。数日前に帰ってきたらしく、挨拶しにやってきた。",
            "「MAGIA」に立ち寄ったところ、店長から話しかけられた。どうも今は暇らしく、話し相手になってほしいとのことだ。",
            "SNSなどで知り合い、趣味が合ってネット上の友人となれた人物と、外でも会うことになった。",
            "クラスや職場で人気の人が、たまたま一人でいるところを見かけた。向こうもこちらに気づいたらしく、話しかけてきた。さて、どうするかな。",
            "趣味の集いに行ったら、クラスメイトや元クラスメイトがいた。趣味の場で会ってみると教室との印象が違った。",
        };

        // someone Help You table
        private static readonly string[] HYTABLE = new[]
        {
            "助けてくれた人表",
            "忘れ物をしたが、それを届けてくれた。その際に少し話をしたが、優しく丁寧で好感の持てる人だった。",
            "自分が転びそうになった、あるいは轢かれそうになった時に、助けてくれた。",
            "用事があって普段行かない場所に行ったとき、迷子になった自分を助けてくれた。",
            "自分がケガをした子供の対処に困っている時、一緒になって子供の面倒を診てくれた。",
            "自分は幼い頃、外でケガをしてしまったことがある。その時に応急処置をして、病院まで運んでくれた人がいる。その人と偶然再会した。",
            "自分は幼い頃、何かしらの事情で孤独になり、辛かった時期がある。そんな時に、声をかけて一緒に遊んでくれた人がいた。その人と再会した。",
            "自分が不良や話しかけられたくないタイプの人に絡まれた時、声をかけて助けてくれた人がいる。",
            "図書館などで資料集めをしている際に、声をかけて助けてくれた人がいた。",
            "魔具や財布など重要なものを失くしてしまい、探している必死そうな自分を見て助けてくれた。",
            "昔、魔法関係で困った時に、助けてくれた魔法使いがいた。その人が何かの用事で「MAGIA」に立ち寄っており、その時に声をかけた。",
        };

        // what's They Like Table
        private static readonly string[] TLTABLE = new[]
        {
            "どんな人だったか表",
            "「守りたい人」によく似ている。",
            "不良っぽいファッションだけど、単にそういう格好が好きなだけで丁寧な人だった。",
            "パリッとした服装、きちんとした身なりで優等生あるいは真面目な人という感じ。",
            "活発そうな人物で、スポーツに打ち込んでいそうな体格と口調。いかにも体育会系。",
            "サバサバとした性格で、いろいろな人の悩み事を聞いては解決しようとしていた兄貴分（姉貴分）。",
            "細かいことが気になるタイプのようで、何かとチェックしてそうな視線を感じた。",
            "優しい性格で、自分の言葉を待って聞いてくれる人だった。",
            "おしゃべりな人物で、いろいろなことをしゃべってくれる。そのうえで、こちらの話も聞いてくれた。",
            "不思議と小動物のような印象を受けた。懐いてきて、自分の反応を楽しみにしているような、そんな人だ。",
            "「守りたい人」の知り合いで、共通の知り合いの話題ができた。",
        };

        // Eccentric Person Table
        private static readonly string[] EPTABLE = new[]
        {
            "変わった人だった表",
            "元気すぎる。声も大きいし、自分は振り回されるし。ちょっと疲れる。",
            "優しそうだけど、どこか底知れない、何を考えているのかわからない人物だった。",
            "見るからに遊び人で、言動も軽く見える。どこか一定以上親しくなるのを恐れている部分が垣間見えた。",
            "ぶっきらぼうで一見とっつきづらい印象だけど、こちらのことをよく見ていて、助けようとしている。たぶん、寂しがり屋。",
            "トレンドを着こなしていて、いかにもおしゃれな人だった。ファッションだけでなく、あらゆることを調べて理解しようと動いている。努力の人なんだと思う。",
            "自分がその人にとって大事な人に似ているらしく、世話を焼こうとしてくれている。",
            "お菓子職人や料理人を目指しているらしく、試食を頼んでくる。",
            "誰かを助けなければならない、という理念があり、とにかく人助けをして回っている人だった。自分のことは二の次のようだ。",
            "すごい資産家の一族らしく、身に着けている物はすべて高級で、教養もあった。",
            "常に何かのアルバイトをしている。掛け持ちだから忙しくしているらしい。",
        };

        // LastName Table
        private static readonly string[] LNTABLE = new[]
        {
            "名字表",
            "日本名字表1（JLTO）を使用する。",
            "日本名字表1（JLTO）を使用する。",
            "日本名字表2（JLTT）を使用する。",
            "日本名字表2（JLTT）を使用する。",
            "カタカナ名字表（FLT）を使用する。",
            "カタカナ名字表（FLT）を使用する。",
        };

        // FirstName Table
        private static readonly string[] FNTABLE = new[]
        {
            "名前表",
            "日本名前表1（JFTO）を使用する。",
            "日本名前表1（JFTO）を使用する。",
            "日本名前表2（JFTT）を使用する。",
            "日本名前表2（JFTT）を使用する。",
            "カタカナ名前表（FFT）を使用する。",
            "カタカナ名前表（FFT）を使用する。",
        };

        private static readonly Dictionary<string, D66Table> TABLES = new Dictionary<string, D66Table>
        {
            // Japanese Lastname Table One
            {
                "JLTO", new D66Table(
                    "日本名字表1",
                    "JLTO",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "有栖（ありす）" },
                        { 12, "佐藤（さとう）" },
                        { 13, "鈴木（すずき）" },
                        { 14, "葉月（はづき）" },
                        { 15, "如月（きさらぎ）" },
                        { 16, "皐月（さつき）" },
                        { 22, "九重（ここのえ）" },
                        { 23, "高橋（たかはし）" },
                        { 24, "田中（たなか）" },
                        { 25, "右京（うきょう）" },
                        { 26, "七海（ななみ）" },
                        { 33, "小春（こはる）" },
                        { 34, "伊藤（いとう）" },
                        { 35, "渡辺（わたなべ）" },
                        { 36, "飛鳥（あすか）" },
                        { 44, "渡井（わたらい）" },
                        { 45, "井上（いのうえ）" },
                        { 46, "氷室（ひむろ）" },
                        { 55, "錦（にしき）" },
                        { 56, "柳（やなぎ）" },
                        { 66, "蓬莱（ほうらい）" },
                    })
            },
            // Japanese Lastname Table Two
            {
                "JLTT", new D66Table(
                    "日本名字表2",
                    "JLTT",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "蜂須賀（はちすか）" },
                        { 12, "山本（やまもと）" },
                        { 13, "中村（なかむら）" },
                        { 14, "御影（みかげ）" },
                        { 15, "四季（しき）" },
                        { 16, "常磐（ときわ）" },
                        { 22, "栗栖（くりす）" },
                        { 23, "小林（こばやし）" },
                        { 24, "加藤（かとう）" },
                        { 25, "花野井（はなのい）" },
                        { 26, "綾瀬（あやせ）" },
                        { 33, "乙女（おとめ）" },
                        { 34, "吉田（よしだ）" },
                        { 35, "山田（やまだ）" },
                        { 36, "桐葉（きりは）" },
                        { 44, "桔梗（ききょう）" },
                        { 45, "松本（まつもと）" },
                        { 46, "音羽（おとわ）" },
                        { 55, "蓮見（はすみ）" },
                        { 56, "桜森（さくらもり）" },
                        { 66, "百合園（ゆりぞの）" },
                    })
            },
            // Foreien Lastname Table
            {
                "FLT", new D66Table(
                    "カタカナ名字表",
                    "FLT",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "レイエス" },
                        { 12, "スミス" },
                        { 13, "ジョンソン" },
                        { 14, "ウィリアム" },
                        { 15, "ブラウン" },
                        { 16, "ジョーンズ" },
                        { 22, "シュルツ" },
                        { 23, "エメリヒ" },
                        { 24, "ファル" },
                        { 25, "クルツ" },
                        { 26, "マイアー" },
                        { 33, "コスキネン" },
                        { 34, "モロー" },
                        { 35, "ルノー" },
                        { 36, "ロベール" },
                        { 44, "ラ" },
                        { 45, "エン" },
                        { 46, "キョウ" },
                        { 55, "ハン" },
                        { 56, "ユン" },
                        { 66, "ホン" },
                    })
            },
            // Japanese Firstname Table One
            {
                "JFTO", new D66Table(
                    "日本名前表1",
                    "JFTO",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "涼太（りょうた）／八重（やえ）" },
                        { 12, "蒼（あおい）／雅（みやび）" },
                        { 13, "樹（いつき）／凛（りん）" },
                        { 14, "蓮（れん）詩（うた）" },
                        { 15, "翔（しょう）／舞（まい）" },
                        { 16, "翼（つばさ）／鈴（すず）" },
                        { 22, "遼（りょう）／瑠華（るか）" },
                        { 23, "陽翔（はると）／結菜（ゆな）" },
                        { 24, "律（りつ）／莉子（りこ）" },
                        { 25, "輝（ひかる）／陽葵（ひまり）" },
                        { 26, "仁（じん）／乃愛（のあ）" },
                        { 33, "大夢（ひろむ）／阿澄（あすみ）" },
                        { 34, "朝陽（あさひ）／結月（ゆづき）" },
                        { 35, "大翔（ひろと）／結愛（ゆあ）" },
                        { 36, "隼人（はやと）／萌花（もか）" },
                        { 44, "公太（こうた）／春歌（はるか）" },
                        { 45, "大和（やまと）／澪（みお）" },
                        { 46, "拓真（たくま）／奈々（なな）" },
                        { 55, "雄大（ゆうだい）／明日香（あすか）" },
                        { 56, "悠（ゆう）／彩（あや）" },
                        { 66, "秀助（しゅうすけ）／那留（なる）" },
                    })
            },
            // Japanese Firstname Table Two
            {
                "JFTT", new D66Table(
                    "日本名前表2",
                    "JFTT",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "一郎（いちろう）／かぐや" },
                        { 12, "太一（たいち）／さくら" },
                        { 13, "颯太（そうた）／あかり" },
                        { 14, "瑛斗（えいと）／こはる" },
                        { 15, "俊輔（しゅんすけ）／ひなた" },
                        { 16, "大地（だいち）／すみれ" },
                        { 22, "健太（けんた）／里奈（りな）" },
                        { 23, "歩（あゆむ）／春菜（はるな）" },
                        { 24, "伊織（いおり）／芽衣（めい）" },
                        { 25, "航（わたる）／愛美（あいみ）" },
                        { 26, "優希（ゆうき）／綾乃（あやの）" },
                        { 33, "直樹（なおき）／茜（あかね）" },
                        { 34, "煌（こう）／もも" },
                        { 35, "陽向（ひなた）／ひかり" },
                        { 36, "将吾（しょうご）／ほのか" },
                        { 44, "和也（かずや）／美穂（みほ）" },
                        { 45, "巧（たくみ）／未来（みらい）" },
                        { 46, "直哉（なおや）／朱里（しゅり）" },
                        { 55, "亮（りょう）／瞳（ひとみ）" },
                        { 56, "陸人（りくと）／心音（ここね）" },
                        { 66, "康平（こうへい）／沙織（さおり）" },
                    })
            },
            // Foreien Firstname Table
            {
                "FFT", new D66Table(
                    "カタカナ名前表",
                    "FFT",
                    D66SortType.Ascending,
                    new Dictionary<int, string>
                    {
                        { 11, "カルロ／ビアンカ" },
                        { 12, "リアム／オリビア" },
                        { 13, "イライジャ／エイヴァ" },
                        { 14, "オリバー／ミア" },
                        { 15, "ジェームズ／アメリア" },
                        { 16, "メイソン／シャーロット" },
                        { 22, "オネスト／カルメン" },
                        { 23, "ブルーノ／アンネ" },
                        { 24, "エーミール／クラーラ" },
                        { 25, "ラインハルト／エッダ" },
                        { 26, "テオ／リア" },
                        { 33, "ロメオ／ルチア" },
                        { 34, "セドリック／マリアンヌ" },
                        { 35, "コーム／リーズ" },
                        { 36, "ギー／カトリーヌ" },
                        { 44, "ハオユー／ルーシー" },
                        { 45, "ハオラン／イーラン" },
                        { 46, "イーチェン／シンイー" },
                        { 55, "ウヌ／ハユン" },
                        { 56, "ソジュン／ソア" },
                        { 66, "ジュウォン／スピン" },
                    })
            },
        };
    }
}
