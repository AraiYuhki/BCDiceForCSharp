using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// デッドラインヒーローズRPG
    /// </summary>
    public sealed class DeadlineHeroes : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DeadlineHeroes Instance = new DeadlineHeroes();

        private static readonly Regex DlhRegex = new Regex(
            @"^DLH",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DcWRegex = new Regex(
            @"^DC\w",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "DeadlineHeroes";

        /// <inheritdoc/>
        public override string Name => "デッドラインヒーローズRPG";

        /// <inheritdoc/>
        public override string SortKey => "てつとらいんひいろおすRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定（DLHx）
        　x：成功率
        　例）DLH80
        　クリティカル、ファンブルの自動的判定を行います。
        　「DLH50+20-30」のように加減算記述も可能。
        　成功率は上限100％、下限０％
        ・デスチャート(DCxY)
        　x：チャートの種類。肉体：DCL、精神：DCS、環境：DCC
        　Y=マイナス値
        　例）DCL5：ライフが -5 の判定
        　　　DCS3：サニティーが -3 の判定
        　　　DCC0：クレジット 0 の判定
        ・ヒーローネームチャート（HNC）
        ・リアルネームチャート　日本（RNCJ）、海外（RNCO）
        ";

        private const string SUCCESS_STR = "成功";
        private const string FAILURE_STR = "失敗";
        private const string CRITICAL_STR = SUCCESS_STR + " ＞ クリティカル！ パワーの代償１／２";
        private const string FUMBLE_STR = FAILURE_STR + " ＞ ファンブル！ パワーの代償２倍＆振り直し不可";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = DlhRegex.Match(command);
            if (match.Success)
            {
                return ResoluteAction(command, randomizer);
            }

            match = DcWRegex.Match(command);
            if (match.Success)
            {
                return RollDeathChart(command, randomizer);
            }

            if (command == "HNC")
            {
                return RollHeroNameChart(randomizer);
            }

            // TODO: roll_tables(command, TABLES) for RNCJ/RNCO

            return null;
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^DLH(\d+([+-]\d+)*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var success_rate = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType) ?? 0;

            var (roll_result, dice10, dice01) = RollD100(randomizer);
            var roll_result_text = roll_result.ToString("D2");

            var result = ActionResult(roll_result, dice10, dice01, success_rate);

            var sequence = new[]
            {
                $"行為判定(成功率:{success_rate}％)",
                $"1D100[{dice10},{dice01}]={roll_result_text}",
                roll_result_text,
                result.Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private Result ActionResult(int total, int tens, int ones, int success_rate)
        {
            if (total == 100 || success_rate <= 0)
            {
                return Result.CreateBuilder(FUMBLE_STR).SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= success_rate - 100)
            {
                return Result.CreateBuilder(CRITICAL_STR).SetSuccess(true).SetCritical(true).Build();
            }
            else if (tens == ones)
            {
                if (total <= success_rate)
                {
                    return Result.CreateBuilder(CRITICAL_STR).SetSuccess(true).SetCritical(true).Build();
                }
                else
                {
                    return Result.CreateBuilder(FUMBLE_STR).SetFumble(true).SetFailure(true).Build();
                }
            }
            else if (total <= success_rate)
            {
                return Result.CreateBuilder(SUCCESS_STR).SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder(FAILURE_STR).SetFailure(true).Build();
            }
        }

        private (int rollResult, int dice10, int dice01) RollD100(IRandomizer randomizer)
        {
            var dice10 = randomizer.RollOnce(10);
            if (dice10 == 10)
            {
                dice10 = 0;
            }
            var dice01 = randomizer.RollOnce(10);
            if (dice01 == 10)
            {
                dice01 = 0;
            }

            var roll_result = dice10 * 10 + dice01;
            if (roll_result == 0)
            {
                roll_result = 100;
            }

            return (roll_result, dice10, dice01);
        }

        private Result? RollDeathChart(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^DC([LSC])(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            if (!DEATH_CHARTS.TryGetValue(m.Groups[1].Value.ToUpper(), out var chart))
            {
                return null;
            }

            var minus_score = int.Parse(m.Groups[2].Value);
            return chart.Roll(randomizer, minus_score);
        }

        private Result? RollHeroNameChart(IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(10);
            var template = HERO_NAME_TEMPLATES[dice - 1];

            var template_result = $"ヒーローネームチャート({dice}) ＞ {template.Text}";
            if (template.Text == "任意")
            {
                return Result.CreateBuilder(template_result).Build();
            }

            var results = new List<string> { template_result };
            var elements = new List<string>();
            foreach (var type in template.Elements)
            {
                if (!HERO_NAME_BASE_CHARTS.TryGetValue(type, out var base_chart))
                {
                    elements.Add(type);
                    continue;
                }

                var (result, element) = base_chart.Roll(randomizer);
                results.Add(result);
                elements.Add(element);
            }

            var hero_name = Regex.Replace(string.Join("", elements), @"・{2,}", "・");
            hero_name = Regex.Replace(hero_name, @"・$", "");
            results.Add($"ヒーローネーム ＞ {hero_name}");

            return Result.CreateBuilder(string.Join("\n", results)).Build();
        }

        #region DeathChart

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
                    throw new ArgumentException($"unexpected chart size {name} (given {_chart.Length}, expected 11)");
                }
            }

            public Result Roll(IRandomizer randomizer, int minus_score)
            {
                var dice = randomizer.RollOnce(10);
                var key_number = dice + minus_score;

                var (key_text, chosen) = At(key_number);

                return Result.CreateBuilder($"デスチャート（{_name}）[マイナス値:{minus_score} + 1D10(->{dice}).Build() = {key_number}] ＞ {key_text} ： {chosen}").Build();
            }

            private (string keyText, string chosen) At(int key_number)
            {
                if (key_number < 10)
                {
                    return ("10以下", _chart[0]);
                }
                else if (key_number > 20)
                {
                    return ("20以上", _chart[_chart.Length - 1]);
                }
                else
                {
                    return (key_number.ToString(), _chart[key_number - 10]);
                }
            }
        }

        #endregion

        #region HeroNameCharts

        private class HeroNameTemplate
        {
            public string Text { get; }
            public string[] Elements { get; }

            public HeroNameTemplate(string text, string[] elements)
            {
                Text = text;
                Elements = elements;
            }
        }

        private class HeroNameBaseChart
        {
            private readonly string _name;
            private readonly string[] _items;

            public HeroNameBaseChart(string name, string[] items)
            {
                _name = name;
                _items = items;
            }

            public (string result, string chosen) Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(10);
                var chosen = _items[dice - 1];

                var result = $"{_name}({dice}) ＞ {chosen}";
                var m = Regex.Match(chosen, @"^［(.+)］$");
                if (m.Success)
                {
                    var element_type = m.Groups[1].Value;
                    if (HERO_NAME_ELEMENT_CHARTS.TryGetValue(element_type, out var element_chart))
                    {
                        var (element_result, element_chosen) = element_chart.Roll(randomizer);
                        result = string.Join(" ＞ ", new[] { result, element_result });
                        chosen = element_chosen;
                    }
                }

                return (result, chosen);
            }
        }

        private class HeroNameElementChart
        {
            private readonly string _name;
            private readonly (string element, string mean)[] _items;

            public HeroNameElementChart(string name, (string element, string mean)[] items)
            {
                _name = name;
                _items = items;
            }

            public (string result, string chosen) Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(10);
                var item = _items[dice - 1];

                var result = $"{_name}({dice}) ＞ {item.element} （意味：{item.mean}）";
                return (result, item.element);
            }
        }

        #endregion

        #region Static Data

        private static readonly Dictionary<string, DeathChart> DEATH_CHARTS = new Dictionary<string, DeathChart>
        {
            ["L"] = new DeathChart("肉体", new[]
            {
                "何も無し。キミは奇跡的に一命を取り留めた。闘いは続く。",
                "激痛が走る。以後、イベント終了時まで、全ての判定の成功率－10％。",
                "キミは［硬直］ポイント２点を得る。［硬直］ポイントを所持している間、キミは「属性：妨害」のパワーを使用することができない。各ラウンド終了時、キミは所持している［硬直］ポイントを１点減らしてもよい。",
                "渾身の一撃!!　キミは〈生存〉判定を行なう。失敗した場合、［死亡］する。",
                "キミは［気絶］ポイント２点を得る。［気絶］ポイントを所持している間、キミはあらゆるパワーを使用できず、自身のターンを得ることもできない。各ラウンド終了時、キミは所持している［気絶］ポイントを１点減らしてもよい。",
                "以後、イベント終了時まで、全ての判定の成功率－20％。",
                "記録的一撃!!　キミは〈生存〉－20％の判定を行なう。失敗した場合、［死亡］する。",
                "キミは［瀕死］ポイント２点を得る。［瀕死］ポイントを所持している間、キミはあらゆるパワーを使用できず、自身のターンを得ることもできない。各ラウンド終了時、キミは所持している［瀕死］ポイントを１点を失う。全ての［瀕死］ポイントを失う前に戦闘が終了しなかった場合、キミは［死亡］する。",
                "叙事詩的一撃!!　キミは〈生存〉－30％の判定を行なう。失敗した場合、［死亡］する。",
                "以後、イベント終了時まで、全ての判定の成功率－30％。",
                "神話的一撃!!　キミは宙を舞って三回転ほどした後、地面に叩きつけられる。見るも無惨な姿。肉体は原型を留めていない（キミは［死亡］した）。",
            }),
            ["S"] = new DeathChart("精神", new[]
            {
                "何も無し。キミは歯を食いしばってストレスに耐えた。",
                "以後、イベント終了時まで、全ての判定の成功率－10％。",
                "キミは［恐怖］ポイント２点を得る。［恐怖］ポイントを所持している間、キミは「属性：攻撃」のパワーを使用できない。各ラウンド終了時、キミは所持している［恐怖］ポイントを１点減らしてもよい。",
                "とても傷ついた。キミは〈意志〉判定を行なう。失敗した場合、［絶望］してＮＰＣとなる。",
                "キミは［気絶］ポイント２点を得る。［気絶］ポイントを所持している間、キミはあらゆるパワーを使用できず、自身のターンを得ることもできない。各ラウンド終了時、キミは所持している［気絶］ポイントを１点減らしてもよい。",
                "以後、イベント終了時まで、全ての判定の成功率－20％。",
                "信じるものに裏切られたような痛み。キミは〈意志〉－20％の判定を行なう。失敗した場合、［絶望］してＮＰＣとなる。",
                "キミは［混乱］ポイント２点を得る。［混乱］ポイントを所持している間、キミは本来味方であったキャラクターに対して、可能な限り最大の被害を与える様、行動し続ける。各ラウンド終了時、キミは所持している［混乱］ポイントを１点減らしてもよい。",
                "あまりに残酷な現実。キミは〈意志〉－30％の判定を行なう。失敗した場合、［絶望］してＮＰＣとなる。",
                "以後、イベント終了時まで、全ての判定の成功率－30％。",
                "宇宙開闢の理に触れるも、それは人類の認識限界を超える何かであった。キミは［絶望］し、以後ＮＰＣとなる。",
            }),
            ["C"] = new DeathChart("環境", new[]
            {
                "何も無し。キミは黒い噂を握りつぶした。",
                "以後、イベント終了時まで、全ての判定の成功率－10％。",
                "ピンチ！　以後、イベント終了時まで、キミは《支援》を使用できない。",
                "裏切り!!　キミは〈経済〉判定を行なう。失敗した場合、キミはヒーローとしての名声を失い、［汚名］を受ける。",
                "以後、シナリオ終了時まで、代償にクレジットを消費するパワーを使用できない。",
                "キミの悪評は大変なもののようだ。協力者からの支援が打ち切られる。以後、シナリオ終了時まで、全ての判定の成功率－20％。",
                "信頼の失墜!!　キミは〈経済〉－20％の判定を行なう。失敗した場合、キミはヒーローとしての名声を失い、［汚名］を受ける。",
                "以後、シナリオ終了時まで、【環境】系の技能のレベルがすべて０となる。",
                "捏造報道!!　身の覚えのない犯罪への荷担が、スクープとして報道される。キミは〈経済〉－30％の判定を行なう。失敗した場合、キミはヒーローとしての名声を失い、［汚名］を受ける。",
                "以後、イベント終了時まで、全ての判定の成功率－30％。",
                "キミの名は史上最悪の汚点として永遠に歴史に刻まれる。もはやキミを信じる仲間はなく、キミを助ける社会もない。キミは［汚名］を受けた。",
            }),
        };

        private static readonly HeroNameTemplate[] HERO_NAME_TEMPLATES = new[]
        {
            new HeroNameTemplate("ベースＡ＋ベースＡ", new[] { "ベースＡ", "ベースＢ" }),
            new HeroNameTemplate("ベースＢ", new[] { "ベースＢ" }),
            new HeroNameTemplate("ベースＢ×2回", new[] { "ベースＢ", "ベースＢ" }),
            new HeroNameTemplate("ベースＢ＋ベースＣ", new[] { "ベースＢ", "ベースＣ" }),
            new HeroNameTemplate("ベースＡ＋ベースＢ＋ベースＣ", new[] { "ベースＡ", "ベースＢ", "ベースＣ" }),
            new HeroNameTemplate("ベースＡ＋ベースＢ×2回", new[] { "ベースＡ", "ベースＢ", "ベースＢ" }),
            new HeroNameTemplate("ベースＢ×2回＋ベースＣ", new[] { "ベースＢ", "ベースＢ", "ベースＣ" }),
            new HeroNameTemplate("（ベースＢ）・オブ・（ベースＢ）", new[] { "ベースＢ", "・オブ・", "ベースＢ" }),
            new HeroNameTemplate("（ベースＢ）・ザ・（ベースＢ）", new[] { "ベースＢ", "・ザ・", "ベースＢ" }),
            new HeroNameTemplate("任意", new[] { "任意" }),
        };

        private static readonly Dictionary<string, HeroNameBaseChart> HERO_NAME_BASE_CHARTS = new Dictionary<string, HeroNameBaseChart>
        {
            ["ベースＡ"] = new HeroNameBaseChart("ベースＡ", new[]
            {
                "ザ・", "キャプテン・", "ミスター／ミス／ミセス・", "ドクター／プロフェッサー・",
                "ロード／バロン／ジェネラル・", "マン・オブ・", "［強さ］", "［色］",
                "マダム／ミドル・", "数字（1～10）・",
            }),
            ["ベースＢ"] = new HeroNameBaseChart("ベースＢ", new[]
            {
                "［神話／夢］", "［武器］", "［動物］", "［鳥］", "［虫／爬虫類］",
                "［部位］", "［光］", "［攻撃］", "［その他］", "数字（1～10）・",
            }),
            ["ベースＣ"] = new HeroNameBaseChart("ベースＣ", new[]
            {
                "マン／ウーマン", "ボーイ／ガール", "マスク／フード", "ライダー", "マスター",
                "ファイター／ソルジャー", "キング／クイーン", "［色］", "ヒーロー／スペシャル", "ヒーロー／スペシャル",
            }),
        };

        private static readonly Dictionary<string, HeroNameElementChart> HERO_NAME_ELEMENT_CHARTS = new Dictionary<string, HeroNameElementChart>
        {
            ["部位"] = new HeroNameElementChart("部位", new[]
            {
                ("ハート", "心臓"), ("フェイス", "顔"), ("アーム", "腕"), ("ショルダー", "肩"), ("ヘッド", "頭"),
                ("アイ", "眼"), ("フィスト", "拳"), ("ハンド", "手"), ("クロウ", "爪"), ("ボーン", "骨"),
            }),
            ["武器"] = new HeroNameElementChart("武器", new[]
            {
                ("ナイヴス", "短剣"), ("ソード", "剣"), ("ハンマー", "鎚"), ("ガン", "銃"), ("スティール", "刃"),
                ("タスク", "牙"), ("ニューク", "核"), ("アロー", "矢"), ("ソウ", "ノコギリ"), ("レイザー", "剃刀"),
            }),
            ["色"] = new HeroNameElementChart("色", new[]
            {
                ("ブラック", "黒"), ("グリーン", "緑"), ("ブルー", "青"), ("イエロー", "黃"), ("レッド", "赤"),
                ("バイオレット", "紫"), ("シルバー", "銀"), ("ゴールド", "金"), ("ホワイト", "白"), ("クリア", "透明"),
            }),
            ["動物"] = new HeroNameElementChart("動物", new[]
            {
                ("バニー", "ウサギ"), ("タイガー", "虎"), ("シャーク", "鮫"), ("キャット", "猫"), ("コング", "ゴリラ"),
                ("ドッグ", "犬"), ("フォックス", "狐"), ("パンサー", "豹"), ("アス", "ロバ"), ("バット", "蝙蝠"),
            }),
            ["神話／夢"] = new HeroNameElementChart("神話／夢", new[]
            {
                ("アポカリプス", "黙示録"), ("ウォー", "戦争"), ("エターナル", "永遠"), ("エンジェル", "天使"), ("デビル", "悪魔"),
                ("イモータル", "死なない"), ("デス", "死神"), ("ドリーム", "夢"), ("ゴースト", "幽霊"), ("デッド", "死んでいる"),
            }),
            ["攻撃"] = new HeroNameElementChart("攻撃", new[]
            {
                ("ストローク", "一撃"), ("クラッシュ", "壊す"), ("ブロウ", "吹き飛ばす"), ("ヒット", "打つ"), ("パンチ", "殴る"),
                ("キック", "蹴る"), ("スラッシュ", "斬る"), ("ペネトレイト", "貫く"), ("ショット", "撃つ"), ("キル", "殺す"),
            }),
            ["その他"] = new HeroNameElementChart("その他", new[]
            {
                ("ヒューマン", "人間"), ("エージェント", "代理人"), ("ブースター", "泥棒"), ("アイアン", "鉄"), ("サンダー", "雷"),
                ("ウォッチャー", "監視者"), ("プール", "水たまり"), ("マシーン", "機械"), ("コールド", "冷たい"), ("サイド", "側面"),
            }),
            ["鳥"] = new HeroNameElementChart("鳥", new[]
            {
                ("ホーク", "鷹"), ("ファルコン", "隼"), ("キャナリー", "カナリア"), ("ロビン", "コマツグミ"), ("イーグル", "鷲"),
                ("オウル", "フクロウ"), ("レイブン", "ワタリガラス"), ("ダック", "アヒル"), ("ペンギン", "ペンギン"), ("フェニックス", "不死鳥"),
            }),
            ["光"] = new HeroNameElementChart("光", new[]
            {
                ("ライト", "光"), ("シャドウ", "影"), ("ファイアー", "炎"), ("ダーク", "暗い"), ("ナイト", "夜"),
                ("ファントム", "幻影"), ("トーチ", "灯火"), ("フラッシュ", "閃光"), ("ランタン", "手さげランプ"), ("サン", "太陽"),
            }),
            ["虫／爬虫類"] = new HeroNameElementChart("虫／爬虫類", new[]
            {
                ("ビートル", "甲虫"), ("バタフライ／モス", "蝶／蛾"), ("スネーク／コブラ", "蛇"), ("アリゲーター", "ワニ"), ("ローカスト", "バッタ"),
                ("リザード", "トカゲ"), ("タートル", "亀"), ("スパイダー", "蜘蛛"), ("アント", "アリ"), ("マンティス", "カマキリ"),
            }),
            ["強さ"] = new HeroNameElementChart("強さ", new[]
            {
                ("スーパー／ウルトラ", "超"), ("ワンダー", "驚異的"), ("アルティメット", "究極の"), ("ファンタスティック", "途方もない"), ("マイティ", "強い"),
                ("インクレディブル", "凄い"), ("アメージング", "素晴らしい"), ("ワイルド", "狂乱の"), ("グレイテスト", "至高の"), ("マーベラス", "驚くべき"),
            }),
        };

        #endregion

    }
}
