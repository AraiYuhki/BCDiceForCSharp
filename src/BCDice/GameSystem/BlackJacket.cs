using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ブラックジャケットRPG
    /// </summary>
    public sealed class BlackJacket : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BlackJacket Instance = new BlackJacket();

        private const string SuccessStr = "成功";
        private const string FailureStr = "失敗";
        private const string CriticalStr = SuccessStr + " ＞ クリティカル！ パワーの代償１／２";
        private const string FumbleStr = FailureStr + " ＞ ファンブル！ パワーの代償２倍＆振り直し不可";
        private const string MiseryStr = FailureStr + " ＞ ミザリー！ パワーの代償２倍＆振り直し不可";

        /// <inheritdoc/>
        public override string Id => "BlackJacket";

        /// <inheritdoc/>
        public override string Name => "ブラックジャケットRPG";

        /// <inheritdoc/>
        public override string SortKey => "ふらつくしあけつとRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定（BJx）
        　x：成功率
        　例）BJ80
        　クリティカル、ファンブルの自動的判定を行います。
        　「BJ50+20-30」のように加減算記述も可能。
        　成功率は上限100％、下限０％
        ・デスチャート(DCxY)
        　x：チャートの種類。肉体：DCL、精神：DCS、環境：DCC
        　Y=マイナス値
        　例）DCL5：ライフが -5 の判定
        　　　DCS3：サニティーが -3 の判定
        　　　DCC0：クレジット 0 の判定
        ・チャレンジ・ペナルティ・チャート（CPC）
        ・サイドトラック・チャート（STC）
        ";

        private static readonly TableCollection Tables = new TableCollection();

        static BlackJacket()
        {
            Tables.Add(new SimpleTable(
                "チャレンジ・ペナルティ・チャート",
                "CPC",
                "逝去\n助けるべきＮＰＣ（ヒロインなど）が死亡する。",
                "黒星\n敵が目的を成就し、事件はPCの敗北で終了する。そのまま余韻フェイズへ。",
                "活性\n敵のボスのライフを２倍にしたうえで決戦フェイズを開始する。",
                "攻勢\n敵ボスの与ダメージに＋２D6の修正を与えたうえで決戦フェイズを開始する。",
                "大挙\n敵の数（ボス以外）を２倍にしたうえで決戦フェイズを開始する。",
                "暗黒\nすべてのエリアを［暗闇］にしたうえで決戦フェイズを開始する。",
                "猛火\n２つの戦場エリアを［ダメージゾーン２］にして、決戦フェイズを開始する。",
                "伏兵\n敵の半分をエリア１とエリア２に移動させた状態で決戦フェイズを開始する。",
                "満腹\nボス以外の敵のライフをすべて２倍にしたうえで決戦フェイズを開始する。",
                "封印\n決戦フェイズの間、PCはカルマを使用できない。決戦フェイズを開始する。"
            ));
            Tables.Add(new SimpleTable(
                "サイドトラック・チャート",
                "STC",
                "邂逅\n偶然、ＮＰＣと出会う。どのＮＰＣが現れるかはGMが決定すること。",
                "事故\n交通事故に出くわす。周囲ではパニックが起きているかも知れない。",
                "午睡\n強烈な睡魔に襲われる。まさか、新手のヴィランの能力か？",
                "告白\nＮＰＣのひとりから、今まで秘めていた思いを吐露される。",
                "設定\n新たな設定が明かされる。実はＮＰＣの父だったとか、生来目が見えん、とか。",
                "刺客\n何者かから攻撃を受ける。第３勢力か？",
                "会敵\n偶然、仇敵のひとりと出くわす。追うべきか？　無視すべきか？",
                "不審\n怪しい人物を見かける。追うべきか？　無視すべきか？",
                "遭遇\nシナリオと関係のないヴィラン組織と遭遇する。",
                "平和\n特に何も起きなかった。"
            ));
        }

        private static readonly Dictionary<string, DeathChart> DeathCharts = new Dictionary<string, DeathChart>
        {
            ["L"] = new DeathChart(
                "肉体",
                new[]
                {
                    "何も無し。キミは奇跡的に一命を取り留めた。闘いは続く。",
                    "激痛が走る。以後、イベント終了時まで、全ての判定の成功率－10％。",
                    "もう、体が動かない……。キミは［硬直２］を受ける。",
                    "渾身の一撃!!　キミは〈生存〉判定を行なう。失敗した場合、［死亡］する。",
                    "突然、目の前が真っ暗になった。キミは［気絶２］を受ける。",
                    "以後、イベント終了時まで、全ての判定の成功率－20％。",
                    "記録的一撃!!　キミは〈生存〉－20％の判定を行なう。失敗した場合、［死亡］する。",
                    "生きているのか死んでいるのか。キミは［瀕死２］を受ける。",
                    "叙事詩的一撃!!　キミは〈生存〉－30％の判定を行なう。失敗した場合、［死亡］する。",
                    "以後、イベント終了時まで、全ての判定の成功率－30％。",
                    "神話的一撃!!　キミは宙を舞って三回転ほどした後、地面に叩きつけられる。見るも無惨な姿。肉体は原型を留めていない（キミは［死亡］した）。",
                }
            ),
            ["S"] = new DeathChart(
                "精神",
                new[]
                {
                    "何も無し。キミは歯を食いしばってストレスに耐えた。",
                    "以後、イベント終了時まで、全ての判定の成功率－10％。",
                    "云い知れぬ恐怖がキミを襲う。キミは［恐怖２］を受ける。",
                    "とても傷ついた。キミは〈意思〉判定を行なう。失敗した場合、［絶望］してNPCとなる。",
                    "キミは意識を失った。キミは［気絶２］を受ける。",
                    "以後、イベント終了時まで、全ての判定の成功率－20％。",
                    "信じる者にだまされたような痛み。キミは〈意思〉－20％の判定を行なう。失敗した場合、［絶望］してＮＰＣとなる。",
                    "仲間に裏切られたのかも知れない。キミは［混乱２］を受ける。",
                    "あまりに残酷な現実。キミは〈意思〉－30％の判定を行なう。失敗した場合、［絶望］してＮＰＣとなる。",
                    "以後、イベント終了時まで、全ての判定の成功率－30％。",
                    "宇宙開闢の理に触れるも、それは人類の認識限界を超える何かであった。キミは［絶望］し、以後ＮＰＣとなる。",
                }
            ),
            ["C"] = new DeathChart(
                "環境",
                new[]
                {
                    "何も無し。キミは黒い噂を握りつぶした。",
                    "以後、イベント終了時まで、全ての判定の成功率－10％。",
                    "ピンチ！　以後、ラウンド終了時まで、キミはカルマを使用できない。",
                    "悪い噂が流れる。キミは〈交渉〉判定を行なう。失敗した場合、キミは仲間からの信頼を失って［無縁］され、ＮＰＣとなる。",
                    "以後、イベント終了時まで、代償にクレジットを消費するパワーを使用できない。",
                    "キミの悪評が世間に知れ渡る。協力者からの支援が打ち切られる。以後、シナリオ終了時まで、全ての判定の成功率－20％。",
                    "裏切り!!　キミは〈経済〉－20％の判定を行なう。失敗した場合、キミは周囲からの信頼を失い、［無縁］され、ＮＰＣとなる。",
                    "以後、シナリオ終了時まで、【環境】系の技能のレベルがすべて０となる。",
                    "捏造報道？　身に覚えのない背信行為がスクープとして報道される。キミは〈心理〉－30％の判定を行なう。失敗した場合、キミは人としての尊厳を失い、［無縁］を受ける。",
                    "以後、イベント終了時まで、全ての判定の成功率－30％。",
                    "キミの名は史上最悪の汚点として歴史に刻まれる。もはらキミを信じる仲間はなく、キミを助ける社会もない。キミは［無縁］され、以後ＮＰＣとなる。",
                }
            ),
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer)
                ?? RollDeathChart(command, randomizer)
                ?? Tables.Eval(command, randomizer);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^BJ(\d+([+-]\d+)*)$");
            if (!m.Success)
            {
                return null;
            }

            int successRate = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor) ?? 0;

            var (rollResult, dice10, dice01) = RollD100(randomizer);
            string rollResultText = rollResult.ToString("D2");

            var result = ActionResult(rollResult, dice10, dice01, successRate);

            var sequence = new[]
            {
                $"行為判定(成功率:{successRate}％)",
                $"1D100[{dice10},{dice01}]={rollResultText}",
                rollResultText,
                result.Text,
            };

            string fullText = string.Join(" ＞ ", sequence);
            return Result.CreateBuilder(fullText)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result ActionResult(int total, int tens, int ones, int successRate)
        {
            if (total == 100)
            {
                return Result.Fumble(MiseryStr);
            }
            else if (successRate <= 0)
            {
                return Result.Fumble(FumbleStr);
            }
            else if (total <= successRate - 100)
            {
                return Result.Critical(CriticalStr);
            }
            else if (tens == ones)
            {
                return total <= successRate
                    ? Result.Critical(CriticalStr)
                    : Result.Fumble(FumbleStr);
            }
            else
            {
                return total <= successRate
                    ? Result.Success(SuccessStr)
                    : Result.Failure(FailureStr);
            }
        }

        private (int rollResult, int dice10, int dice01) RollD100(IRandomizer randomizer)
        {
            int dice10 = randomizer.RollOnce(10);
            if (dice10 == 10)
            {
                dice10 = 0;
            }

            int dice01 = randomizer.RollOnce(10);
            if (dice01 == 10)
            {
                dice01 = 0;
            }

            int rollResult = dice10 * 10 + dice01;
            if (rollResult == 0)
            {
                rollResult = 100;
            }

            return (rollResult, dice10, dice01);
        }

        private Result? RollDeathChart(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^DC([LSC])(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            string chartKey = m.Groups[1].Value.ToUpperInvariant();
            if (!DeathCharts.TryGetValue(chartKey, out var chart))
            {
                return null;
            }

            int minusScore = Convert.ToInt32(m.Groups[2].Value);
            return chart.Roll(randomizer, minusScore);
        }

        private class DeathChart
        {
            private readonly string _name;
            private readonly string[] _chart;

            public DeathChart(string name, string[] chart)
            {
                _name = name;
                _chart = chart;

                if (_chart.Length != 11)
                {
                    throw new ArgumentException($"unexpected chart size {name} (given {chart.Length}, expected 11)");
                }
            }

            public Result Roll(IRandomizer randomizer, int minusScore)
            {
                int dice = randomizer.RollOnce(10);
                int keyNumber = dice + minusScore;

                var (keyText, chosen) = At(keyNumber);
                string text = $"デスチャート（{_name}）[マイナス値:{minusScore} + 1D10(->{dice}) = {keyNumber}] ＞ {keyText} ： {chosen}";

                return Result.CreateBuilder(text)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            private (string keyText, string chosen) At(int keyNumber)
            {
                if (keyNumber < 10)
                {
                    return ("10以下", _chart[0]);
                }
                else if (keyNumber > 20)
                {
                    return ("20以上", _chart[_chart.Length - 1]);
                }
                else
                {
                    return (keyNumber.ToString(), _chart[keyNumber - 10]);
                }
            }
        }
    }
}
