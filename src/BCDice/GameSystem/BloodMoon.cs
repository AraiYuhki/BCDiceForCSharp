using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ブラッドムーン
    /// 2D6判定でファンブル・スペシャル判定付き。各種表あり。
    /// </summary>
    public sealed class BloodMoon : GameSystemBase
    {
        public static readonly BloodMoon Instance = new BloodMoon();

        public override string Id => "BloodMoon";
        public override string Name => "ブラッドムーン";
        public override string SortKey => "ふらつとむうん";
        public override D66SortType D66SortType => D66SortType.Ascending;
        public override RoundType RoundType => RoundType.Ceiling;

        public override string HelpMessage => @"・各種表
　・関係属性表　RAT
　・導入タイプ決定表(ノーマル)　IDT
　・導入タイプ決定表(ハード込み)　ID2T
　・シーン表           ST
　・先制判定指定特技表 IST
　・身体部位決定表　　 BRT
　・自信幸福表　　　　 CHT
　・地位幸福表　　　　 SHT
　・日常幸福表　　　　 DHT
　・人脈幸福表　　　　 LHT
　・退路幸福表　　　　 EHT
　・ランダム全特技表　 AST
　・軽度狂気表　　　　 MIT
　・重度狂気表　　　　 SIT
・D66ダイスあり
";

        private static readonly Dictionary<string, ITable> Tables = new Dictionary<string, ITable>(StringComparer.OrdinalIgnoreCase)
        {
            ["ST"] = new RangeTable("シーン表", "ST", 2, 6, new List<(int, int, string)>
            {
                (2, 2, "どこまでも広がる荒野。風が吹き抜けていく。"),
                (3, 3, "血まみれの惨劇の跡。いったい誰がこんなことを？"),
                (4, 4, "都市の地下。かぼそい明かりがコンクリートを照らす。"),
                (5, 5, "豪華な調度が揃えられた室内。くつろぎの空間を演出。"),
                (6, 6, "普通の道端。様々な人が道を行き交う。"),
                (7, 7, "明るく浮かぶ月の下。暴力の気配が満ちていく。"),
                (8, 8, "打ち捨てられた廃墟。荒れ果てた景色に心も荒む。"),
                (9, 9, "生活の様子が色濃く残る部屋の中。誰の部屋だろう？"),
                (10, 10, "にぎやかな飲食店。騒ぐ人々に紛れつつ事態は進行する。"),
                (11, 11, "ざわめく木立。踊る影。"),
                (12, 12, "高い塔の上。都市を一望できる。"),
            }),
            ["MIT"] = new SimpleTable("軽度狂気表", "MIT", new[]
            {
                "【誇大妄想】（判定に失敗するたびに【テンション】が１増加する。）",
                "【記憶喪失】（【幸福】の修復判定にマイナス２の修正。）",
                "【こだわり】（戦闘中の行動を「パス」以外で一つ選択し、その行動をすると【テンション】が６増加する。）",
                "【お守り中毒】（「幸運のお守り」を装備していない場合、全ての2d6判定にマイナス１の修正。）",
                "【不死幻想】（自分が受けるダメージが全て１増加する。）",
                "【血の飢え】（戦闘中、ラウンドごとに他のキャラクターにダメージを与えないと、ラウンド終了時に【テンション】４増加。）",
            }),
            ["SIT"] = new SimpleTable("重度狂気表", "SIT", new[]
            {
                "【幸福依存】（【幸福】を一つ選択し、その【幸福】が結果フェイズに失われたとき、死亡する。）",
                "【見えない友達】（交流判定にマイナス３の修正がつく。）",
                "【臆病】（自分の行う妨害判定にマイナス２の修正がつく。）",
                "【陰謀論】（休息判定にマイナス３の修正がつく。）",
                "【指令受信】（メインフェイズの３サイクル目の自分のシーンでは、可能な範囲でGMが行動を決定する。）",
                "【猜疑心】（自分が「連携攻撃」を行うとき、関係の【深度】をダメージに加えられない。）",
            }),
            ["CHT"] = new SimpleTable("自信幸福表", "CHT", new[]
            {
                "【戦闘能力】あなたはハンターとしての自分の戦闘能力に自信を持っています。",
                "【美貌】あなたは自分が美しいことを知っています。",
                "【血筋】あなたは名家の血を引く者です。",
                "【趣味の技量】あなたは趣味の分野では第一人者です。",
                "【仕事の技量】職場で最も有能なもの、それがあなたです。",
                "【長生き】あなたはハンターとしてかなりの年月を過ごしてきたが、まだ死んでいません。",
            }),
            ["SHT"] = new SimpleTable("地位幸福表", "SHT", new[]
            {
                "【役職】あなたは職場、あるいはハンターの組織のなかで高い階級についています。",
                "【英雄】あなたはかつて偉業を成し遂げたことがあり、誰でもそれを知っています。",
                "【お金持ち】あなたには財産があります。",
                "【特権階級】あなたは国が定める特権階級の一員です。",
                "【人格者】誰もが認める人格者としての評判を持っています。",
                "【リーダー】あなたは所属している何らかの組織を率いる立場にあります。",
            }),
            ["DHT"] = new SimpleTable("日常幸福表", "DHT", new[]
            {
                "【家】あなたの家はとても快適な空間です。",
                "【職場】あなたは仕事が楽しくて仕方ありません。",
                "【行きつけの店】あなたには休みの日や職場帰りに立ち寄る行きつけの店があります。",
                "【ペット】あなたは動物を飼っています。",
                "【親しい隣人】おとなりさんやお向かいさんと仲が良い。",
                "【思い出】あなたは昔の思い出を心の支えにしています。",
            }),
            ["LHT"] = new SimpleTable("人脈幸福表", "LHT", new[]
            {
                "【理解ある家族】あなたの家族は、あなたがハンターであることを知ったうえで協力してくれます。",
                "【有能な友人】あなたの友人は、吸血鬼の存在とあなたの本当の仕事を知っています。",
                "【愛する恋人】あなたには愛する人がいます。",
                "【同志の権力者】あなたには吸血鬼の存在を知りながら、奴らに屈していない権力者との繋がりがあります。",
                "【得がたい師匠】あなたは使う武器を学んだ師匠がいます。",
                "【可愛い子供】あなたには子供がいます。",
            }),
            ["EHT"] = new SimpleTable("退路幸福表", "EHT", new[]
            {
                "【故郷の町】あなたは生まれ育った街を離れてハンターとして活動しています。",
                "【待っている人】あなたがハンターをやめて、普通の暮らしに戻ることを待ちわびている人がいます。",
                "【就職先】あなたは吸血鬼狩りの報酬がなくなっても、すぐに入ることができる就職先があります。",
                "【配偶者】あなたはハンターをやめたあとに家庭に入ろうと考えています。",
                "【大志】あなたには「本当にやりたかったこと」があり、いつかその夢をかなえる気でいます。",
                "【空想の王国】あなたには辛いことがあると白昼夢にふける癖があります。",
            }),
            ["IDT"] = new SimpleTable("導入タイプ決定表(ノーマル)", "IDT", new[]
            {
                "依頼",
                "防衛",
                "復讐",
                "関係",
                "挑戦",
                "救済",
            }),
        };

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // テーブルコマンドのチェック
            if (Tables.TryGetValue(command, out var table))
            {
                return table.Roll(randomizer);
            }

            return null;
        }

        /// <summary>
        /// 2D6判定の結果を拡張（ファンブル・スペシャル判定）
        /// </summary>
        protected override Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            var result = base.EvalCommonCommand(command, randomizer);

            if (result != null && result.Rands != null && result.Rands.Count >= 2)
            {
                if (result.Rands[0].Sides == 6 && result.Rands[1].Sides == 6)
                {
                    int die1 = result.Rands[0].Value;
                    int die2 = result.Rands[1].Value;
                    int diceTotal = die1 + die2;

                    if (diceTotal <= 2)
                    {
                        return Result.CreateBuilder(result.Text + " ＞ ファンブル(【余裕】が 0 に).Build()")
                            .SetSecret(result.IsSecret)
                            .SetFumble(true)
                            .SetFailure(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                    else if (diceTotal >= 12)
                    {
                        return Result.CreateBuilder(result.Text + " ＞ スペシャル(【余裕】+3）")
                            .SetSecret(result.IsSecret)
                            .SetCritical(true)
                            .SetSuccess(true)
                            .SetRands(result.Rands)
                            .SetDetailedRands(result.DetailedRands)
                            .Build();
                    }
                }
            }

            return result;
        }
    }
}