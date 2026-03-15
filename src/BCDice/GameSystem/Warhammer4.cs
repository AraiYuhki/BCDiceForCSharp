using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ウォーハンマーRPG第4版
    /// </summary>
    public sealed class Warhammer4 : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Warhammer4 Instance = new Warhammer4();

        /// <inheritdoc/>
        public override string Id => "Warhammer4";

        /// <inheritdoc/>
        public override string Name => "ウォーハンマーRPG第4版";

        /// <inheritdoc/>
        public override string SortKey => "うおおはんまあ4";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ クリティカル表 (whctH, whctA, whctB, whctL, whctHU, whctAU, whctBU, whctLU)
          ""WH部位 頑健ボーナス以下フラグ""の形で指定します。
          部位は「H(頭部)」「A(腕)」「B(胴体)」「L(足)」の４カ所です。
          頑健ボーナスフラグは頑健ボーナス以下のダメージの時にUをつけます。
          例）whctH whctAU WHCTL

        ■ 命中部位表 (WHLT)
          命中部位をランダムに決定します。(クリティカル用)

        ■ やっちまった！表 (WHFT)
          やっちまった！表をロールして、結果を表示します。

        ■ 神々の天罰表 (WHWGTx)
          ""WHWGT(原罪点)""の形で指定します。
          原罪点を省略した場合は、原罪点=0として処理します。
          神々の天罰表をロールして、結果を表示します。

        ■ 小誤発動表 (WHMIT)
          小誤発動表をロールして、結果を表示します。

        ■ 大誤発動表 (WHMAT)
          大誤発動表をロールして、結果を表示します。

        ■ 命中判定 (WHx)
          ""WH(命中値)""の形で指定します。
          命中判定を行って、クリティカルかファンブルでなければ部位も表示します。
          例）wh60　　wh(43-20)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollCriticalTable(command, randomizer) ?? RollAttack(command, randomizer) ?? RollWrathOfGodTable(command, randomizer) ?? RollTables(command, randomizer);
        }

        private Result? Result1d100(int total, int _diceTotal, string cmpOp, int target)
        {
            if (cmpOp != "<=")
            {
                return null;
            }

            var t10 = total / 10;
            var d10 = target / 10;
            var sl = d10 - t10;

            if (total <= 5 && sl < 1)
            {
                // 自動成功時のSLは最低でも1
                sl = 1;
            }
            else if (total >= 96 && sl > -1)
            {
                // 自動失敗時のSLは最大でも-1
                sl = -1;
            }

            Result result;
            if (total <= 5)
            {
                result = Result.CreateBuilder("自動成功").SetSuccess(true).Build();
            }
            else if (total >= 96)
            {
                result = Result.CreateBuilder("自動失敗").SetFailure(true).Build();
            }
            else if (total <= target)
            {
                result = Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                result = Result.CreateBuilder("失敗").SetFailure(true).Build();
            }

            var slText = string.Format("(SL{0:+#;-#;+0})", sl);
            if (sl == 0 && total > target)
            {
                slText = "(SL-0)";
            }

            var text = $"{result.Text}{slText} ＞ {ResultDetail(sl, total, target)}";
            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private string ResultDetail(int sl, int total, int target)
        {
            if (sl == 0)
            {
                if (total <= 5)
                {
                    return "小成功";
                }
                else if (total >= 96)
                {
                    return "小失敗";
                }
                else if (total <= target)
                {
                    return "小成功";
                }
                else
                {
                    return "小失敗";
                }
            }
            else if (sl >= 6)
            {
                return "超大成功";
            }
            else if (sl >= 4)
            {
                return "大成功";
            }
            else if (sl >= 2)
            {
                return "成功";
            }
            else if (sl > 0)
            {
                return "小成功";
            }
            else if (sl >= -1)
            {
                return "小失敗";
            }
            else if (sl >= -3)
            {
                return "失敗";
            }
            else if (sl >= -5)
            {
                return "大失敗";
            }
            else
            {
                return "超大失敗";
            }
        }

        private Result? RollCriticalTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^WHCT([HABTL])(U)?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var part = m.Groups[1].Value.ToUpper();
            if (part == "T")
            {
                part = "B";
            }

            var underGankenBonus = m.Groups[2].Success;

            if (!CRITICAL_TABLES.ContainsKey(part))
            {
                return null;
            }

            return CRITICAL_TABLES[part].Roll(randomizer, underGankenBonus);
        }

        private Result? RollAttack(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^WH(\d+)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var targetNumber = int.Parse(m.Groups[1].Value);
            var total = randomizer.RollOnce(100);
            var resultObj = Result1d100(total, total, "<=", targetNumber);
            var resultText = resultObj?.Text;

            var parts = new List<string> { $"({command})", total.ToString() };
            if (resultText != null)
            {
                parts.Add(resultText);
            }
            var additional = AdditionalResult(total, targetNumber);
            if (additional != null)
            {
                parts.Add(additional);
            }

            var text = string.Join(" ＞ ", parts);
            return Result.CreateBuilder(text)
                .SetSuccess(resultObj?.IsSuccess ?? false)
                .SetFailure(resultObj?.IsFailure ?? false)
                .SetCritical(resultObj?.IsCritical ?? false)
                .SetFumble(resultObj?.IsFumble ?? false)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string? AdditionalResult(int total, int targetNumber)
        {
            var (tens, ones) = SplitD100(total);
            if (total > targetNumber || total > 95)
            {
                if (ones == tens)
                {
                    return "ファンブル";
                }
                else
                {
                    return $"({GetHitPart(MergeD100(ones, tens))})";
                }
            }
            else
            {
                if (ones == tens)
                {
                    return "クリティカル";
                }
                else
                {
                    return GetHitPart(MergeD100(ones, tens));
                }
            }
        }

        private (int tens, int ones) SplitD100(int dice)
        {
            if (dice == 100)
            {
                return (0, 0);
            }
            else
            {
                return (dice / 10, dice % 10);
            }
        }

        private int MergeD100(int tens, int ones)
        {
            return (tens == 0 && ones == 0) ? 100 : tens * 10 + ones;
        }

        private string GetHitPart(int value)
        {
            if (value >= 1 && value <= 9) return "頭部";
            if (value >= 10 && value <= 24) return "左腕(逆腕)";
            if (value >= 25 && value <= 44) return "右腕(利腕)";
            if (value >= 45 && value <= 79) return "胴体";
            if (value >= 80 && value <= 89) return "左脚";
            if (value >= 90 && value <= 100) return "右脚";
            return "不明";
        }

        private Result? RollWrathOfGodTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^WHWGT(\d*)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var sinPoint = string.IsNullOrEmpty(m.Groups[1].Value) ? 0 : int.Parse(m.Groups[1].Value);
            var total = randomizer.RollOnce(100) + sinPoint * 10;
            if (total > 151)
            {
                total = 151;
            }

            var content = GetWrathOfGodResult(total);
            var text = $"神々の天罰表({total}) ＞ {content}";
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string GetWrathOfGodResult(int value)
        {
            if (value >= 1 && value <= 5) return "聖なる幻視：神の幻視が君の感覚を苦しめる。「普通」（+20）の〈肉体抵抗〉テストを行なうこと。失敗すると「朦朧状態」1つを得る。幻視の内容はGMが定めること。";
            if (value >= 6 && value <= 10) return "行ないを顧みよ：次の1週間にわたって、成功したすべての〈祈念〉テストで得られる成功レベル（SL）の上限が＋0となる。";
            if (value >= 11 && value <= 15) return "戒めを心に留めよ：次の1d10+「原罪点」のラウンドにわたって、〈祈念〉技能に-10のペナルティを被る。";
            if (value >= 16 && value <= 20) return "忠誠を示せ：「伏せ状態」になる。この「状態」は、「普通」（+20）の〈祈念〉テストに成功するまで取り除くことができない。";
            if (value >= 21 && value <= 25) return "物事には限度というものがある：1d10ラウンドの間、君はいかなる〈祈念〉テストも行なうことができない。";
            if (value >= 26 && value <= 30) return "我が意図を理解しておらず：次の1d10+「原罪点」の時間数にわたって、君は仕える神に関連するすべての技能（GMが決定する）に-10のペナルティを被る。";
            if (value >= 31 && value <= 35) return "信念の欠如が心をかき乱すのだ：1d10+「原罪点」のラウンドにわたって、君はいかなる〈祈念〉テストも行なうことができない。";
            if (value >= 36 && value <= 40) return "我が痛みを分かとう：【頑健力】ボーナスとアーマー・ポイント〔AP〕による修正を無視して、1+「原罪点」の「耐久値」を失う。加えて、「普通」（+20）の〈肉体抵抗〉テストを行なうこと。失敗すると、「朦朧状態」1つを得る。";
            if (value >= 41 && value <= 45) return "汝の大義に価値はない：君の目標（たち）は「伏せ状態」になる。次の1d10+「原罪点」の日数にわたって、前述の目標に対して行なわれる君の仕える神による「祝福」や「奇跡」は、ことごとく自動失敗となる。";
            if (value >= 46 && value <= 50) return "戯言はもうやめよ：次の2d10+「原罪点」のラウンドにわたって、君はいかなる〈祈念〉テストも行なうことができない。";
            if (value >= 51 && value <= 55) return "我が怒りを感じよ：1d10+「原罪点」に等しい「耐久値」を失う。加えて、「手強い」（+0）〈肉体抵抗〉テストを行なうこと。失敗すると、「朦朧状態」1つを得る。";
            if (value >= 56 && value <= 60) return "汝を助けてはやらぬ：次の1d10+「原罪点」の日数にわたって、君は仕える神に関連するすべての技能（GMが決定する）に-10 のペナルティを被る。";
            if (value >= 61 && value <= 65) return "神の手傷：1+「原罪点」に等しい数の「出血状態」を得る。";
            if (value >= 66 && value <= 70) return "目が見えない：「伏せ状態」になることに加えて、1+「原罪点」に等しい数の「目がくらんだ状態」を得る。そのうち後者は、「手強い」（+0）〈祈念〉テストに成功することによってのみ取り除くことができる。このテストに成功することで、1+成功レベル（SL）に等しい数の「目がくらんだ状態」を取り除く。";
            if (value >= 71 && value <= 75) return "何を犠牲にするのか？：【頑健力】ボーナスとアーマー・ポイント〔AP〕による修正値を無視して、1d10+「原罪点」に等しい「耐久値」を失う。加えて、「難しい」（-10）〈肉体抵抗〉テストを行なうこと。失敗すると、「朦朧状態」1つを得る。";
            if (value >= 76 && value <= 80) return "我に逆らうとは何事ぞ：君の神は非常に苛立っている。懺悔として、次の1d10+「原罪点」に等しいラウンド数にわたって、君は〈祈念〉テストを自分の「アクション」として行ない続けなければならない。";
            if (value >= 81 && value <= 87) return "肉体を清めよ：【頑健力】ボーナスとアーマー・ポイント〔AP〕による修正を無視して、2d10+「原罪点」に等しい「耐久値」を失う。加えて、「多難」（-20）な〈肉体抵抗〉テストを行なうこと。失敗すると、「朦朧状態」1つを得る。成功レベル（SL）-4以下で失敗した場合、最短でも1d10ラウンドに渡って継続し、その間は取り除くことができない「気絶状態」となる。";
            if (value == 88) return "悪魔の干渉：君の守護神に代わり、暗黒の神々が君の懇願に応じる。君のいる場所から2d10ヤード以内に1d10体のレッサー・ディーモンが現れ、最も近い目標（群）を攻撃する。";
            if (value >= 89 && value <= 95) return "我が怒りに慄け：1+「原罪点」に等しい数の「戦意喪失状態」を得る。";
            if (value >= 96 && value <= 100) return "懺悔せよ：君は「贖罪」に向かわなければならない。";
            if (value >= 101 && value <= 105) return "折檻：「耐久値」を0点まで減じ、（君が既にその場にいなくとも）、「気絶状態」になる。この「状態」は、君の「耐久値」が少なくとも1点に回復するまで、取り除くことができない。";
            if (value >= 106 && value <= 110) return "みだりに我が名を使うでない：次の1d10+「原罪点」の日数にわたって、君は《祝福》異能と《奇跡》異能を失う。";
            if (value >= 111 && value <= 115) return "うぬぼれるな：すべての「所持品」が取り除かれ、君は丸裸になる。君が何らかの魔法アイテムを所持していた場合には、「贖罪」を1つ完遂するごとに、魔法のアイテム1つが君の元へと戻ってくる。";
            if (value >= 116 && value <= 120) return "我が慈悲を悪用した：次の2d10+「原罪点」の日数にわたって、君は《祝福》異能と《奇跡》異能を失う。";
            if (value >= 121 && value <= 125) return "汝の邪悪を見よ：君はあらゆるしくじりに関する耐え難い幻視に苦しむ。それは永遠とも思われるが、一瞬に過ぎないものだ。忘れられない経験に対応したあつらえの「心理特徴」（P.190）をGMと相談して作り、君のキャラクターに適用すること。";
            if (value >= 126 && value <= 130) return "神の雷：君の神が咎める。「耐久値」を0点まで減じ、（君が既にその場にいなくとも）、「炎上状態」1つを得る。";
            if (value >= 131 && value <= 135) return "我が苦しみをいざ分かたん：「贖罪」を完遂するまでの毎朝、君は1＋「原罪点」に等しい数の「出血状態」を得る。";
            if (value >= 136 && value <= 140) return "破門宣告：2つの「贖罪」を完遂するまで、君は《祝福》異能と《奇跡》異能を失う。最初の贖罪で《祝福》異能が戻り、次の贖罪で《奇跡》異能が戻る。君の神の信徒は全員、君の置かれた状況をひとりでに知る。信徒と交流するすべてのテストは自動的に「超多難」（-30）の難易度となり、プラスの修正値を適用することはできない。";
            if (value >= 141 && value <= 145) return "汝の価値を示せ：君の神の従者1人が1d100ヤード以内に現れ、怒れる神の性質に従って攻撃や介入、叱責、およびそれらに準ずる行動を取る。";
            if (value >= 146 && value <= 150) return "追放：君は神に見捨てられた。《祝福》異能と《奇跡》異能を永続的に失い、さらに、〈祈念〉技能でのすべての成長を失う。それに加えて、君の神の信徒は全員、君の置かれた状況をひとりでに知る。信徒と交流するすべてのテストは自動的に「超多難」（-30）の難易度となり、プラスの修正値を適用することはできない。";
            if (value >= 151) return "汝の責任を果たせ：最後の審判と向き合うよう、君は神の元へと召喚される。「運命点」が残っていない限り、君は二度と戻ることができない。「運命点」1点を消費することでGMが選択した地点へと戻るが、その上で、146〜150の「追放」の効果を受けねばならない。";
            return "不明";
        }

        private Result? RollTables(string command, IRandomizer randomizer)
        {
            var upperCommand = command.ToUpper();

            switch (upperCommand)
            {
                case "WHLT":
                    return RollHitPartsTable(randomizer);
                case "WHFT":
                    return RollFumbleTable(randomizer);
                case "WHMIT":
                    return RollMinorMiscastTable(randomizer);
                case "WHMAT":
                    return RollMajorMiscastTable(randomizer);
                default:
                    return null;
            }
        }

        private Result? RollHitPartsTable(IRandomizer randomizer)
        {
            var total = randomizer.RollOnce(100);
            var content = GetHitPart(total);
            return Result.CreateBuilder($"命中部位表({total}).Build() ＞ {content}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollFumbleTable(IRandomizer randomizer)
        {
            var total = randomizer.RollOnce(100);
            string content;
            if (total >= 1 && total <= 20) content = "君は自分の体の一部に当ててしまった。「耐久度」1点を失う。";
            else if (total >= 21 && total <= 40) content = "君の武器が1ダメージを受ける。次のラウンドの行動順が一番最後になる。";
            else if (total >= 41 && total <= 60) content = "君は身体動作の目算を誤る。次のラウンドのアクションに-10。";
            else if (total >= 61 && total <= 70) content = "君はひどく躓きバランスを回復するのに手間取る。次の「移動」を失う。";
            else if (total >= 71 && total <= 80) content = "君は武器を取り回しを誤る。次の「アクション」を失う。";
            else if (total >= 81 && total <= 90) content = "君は筋を違えるか、足首をひねってしまう。「筋断裂(軽度)」の負傷を得る。「致命的負傷」としてカウントされる。";
            else content = "君は射程内のランダムな味方を攻撃してしまう。命中判定の出目の1の位をSLとして用いること。攻撃可能な味方が居ない場合は、自己を攻撃してしまい「朦朧状態」1つを得る。";

            return Result.CreateBuilder($"やっちまった！表({total}).Build() ＞ {content}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollMinorMiscastTable(IRandomizer randomizer)
        {
            var total = randomizer.RollOnce(100);
            string content;
            if (total >= 1 && total <= 5) content = "魔女の印：1マイル以内で次に新たに生まれたクリーチャー1体が混沌変異する。";
            else if (total >= 6 && total <= 10) content = "酸っぱいミルク：1d100ヤード以内のすべてのミルクが即座に酸っぱくなる。";
            else if (total >= 11 && total <= 15) content = "立ち枯れ：君の【意志力】ボーナス×1 マイル以内の農場が立ち枯れの病にかかり、すべての穀物が一夜のうちに腐れてしまう。";
            else if (total >= 16 && total <= 20) content = "耳垢閉塞：君の耳が即座に分厚い耳垢で詰まってしまう。「耳鳴り状態」1つを得る。誰かが君に耳掃除してくれない限りこれを取り除くことはできない（〈治療〉技能を用いてテストに成功しなければならない）。";
            else if (total >= 21 && total <= 25) content = "魔女の光：君は自分の「魔法体系」に関連した気味の悪い光を放ち、大きな焚き火ほどの明るさで発光する。これは1d10ラウンドにわたって持続する。";
            else if (total >= 26 && total <= 30) content = "地獄の囁き：「普通」（+20）の【意志力】テストを行わねばならず、失敗したなら「堕落ポイント」1点を得る。";
            else if (total >= 31 && total <= 35) content = "破裂：君の鼻、目、耳からおびただしく出血する。1d10個の「出血状態」を得る。";
            else if (total >= 36 && total <= 40) content = "魂が震える：「伏せ状態」になる。";
            else if (total >= 41 && total <= 45) content = "結び目ほどき：君の「所持品」のあらゆる留め金が外れ、あらゆる紐の結び目がほどけてしまう。ベルトはずり落ち、ポーチは開き、バッグは落ち、鎧も外れてしまうのだ。";
            else if (total >= 46 && total <= 50) content = "気まぐれな装い：君の衣服が自分の意志を持っているかのように振る舞う。「組み付かれた状態」1つを得る。これに抵抗するための【筋力】は1d10×5となる。";
            else if (total >= 51 && total <= 55) content = "節制の呪い：1d100ヤード以内のすべての酒が苦くて臭いひどい味になる。";
            else if (total >= 56 && total <= 60) content = "魂の消耗：「疲労状態」1 つを得る。これは1d10時間にわたって持続する。";
            else if (total >= 61 && total <= 65) content = "困惑する：戦闘に参加しているなら、「不意を討たれた状態」になる。それ以外の場合、君は完全にびくついてしまい、心はあたふたとし、ほんの片時も集中することができなくなる。";
            else if (total >= 66 && total <= 70) content = "不浄なる幻視：穢らわしく不浄な行為の幻視が浮かび上がり君を悩ませる。「目がくらんだ状態」1つを得る。「手強い」（+0）〈冷静さ〉テストを行ない、失敗したなら、さらに1つの同「状態」を得る。";
            else if (total >= 71 && total <= 75) content = "鼻につく言葉：1d10ラウンドにわたって、すべての〈言語〉テスト（発動テストも含む）に-10のペナルティを被る。";
            else if (total >= 76 && total <= 80) content = "恐怖！：「多難」（-20）な〈冷静さ〉テストを行ない、失敗したなら「戦意喪失状態」1つを得る。";
            else if (total >= 81 && total <= 85) content = "堕落の呪い：「堕落ポイント」1点を得る。";
            else if (total >= 86 && total <= 90) content = "二重の災難：君が発動した呪文の効果は1d10マイル以内の他の場所で発生する。そこでどのような結果が起こるかはGMの裁定による。";
            else if (total >= 91 && total <= 95) content = "倍増する不幸：この表を2回ロールする。91〜100が出た場合には再ロールをすること。";
            else content = "ほとばしる混沌：『大誤発動表』でもう一度ロールすること。";

            return Result.CreateBuilder($"小誤発動表({total}).Build() ＞ {content}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollMajorMiscastTable(IRandomizer randomizer)
        {
            var total = randomizer.RollOnce(100);
            string content;
            if (total >= 1 && total <= 5) content = "亡霊の声：【意志力】×1ヤード以内のすべての者が\"混沌の領域\"から現れた暗く誘惑的な囁き声を耳にする。知性のあるクリーチャーは皆、「普通」（+20）の〈冷静さ〉テストを行ない、失敗したなら、「堕落ポイント」1点を得る。";
            else if (total >= 6 && total <= 10) content = "魔女の目：君の目が1d10時間にわたって、自分の「魔法体系」に関連した不自然な色に変わる。目の色が変わっている間、君はいかなる手段でも取り除くことができない「目がくらんだ状態」1つを得る。";
            else if (total >= 11 && total <= 15) content = "エーテルの衝撃：君は、【頑健力】ボーナスとアーマー・ポイント〔AP〕による修正値を無視して、1d10点の「耐久値」を失う。さらに、「普通」（+20）の〈肉体抵抗〉テストを行ない、失敗したなら、「朦朧状態」1つも得る。";
            else if (total >= 16 && total <= 20) content = "死の歩み：君の足跡はその行く手に死を残していく。次の1d10時間にわたって、君の周囲のあらゆる植物は枯れ死に果てる。";
            else if (total >= 21 && total <= 25) content = "腸内大暴動：君はお腹の不調を抑えきれず、粗相してしまう。「疲労状態」1つを得るが、これは君が衣服を取り替え、身体を綺麗にするまで取り除くことができない。";
            else if (total >= 26 && total <= 30) content = "魂の火：君は自分の「魔法体系」に関連した色の不浄な炎に取り巻かれ、「炎上状態」1つを得る。";
            else if (total >= 31 && total <= 35) content = "抑えの効かぬ舌：君は1d10ラウンドにわたって、わけのわからないことを喋りまくる。この間、君は言葉による意思疏通ができず、あらゆる発動テストを行なうこともできないが、それ以外は通常通り行動してもよい。";
            else if (total >= 36 && total <= 40) content = "群れの襲撃：君はエーテル体のラット、ジャイアント・スパイダー、スネーク、その他（GM の任意）の群れに囲まれる。関連するクリーチャー種別の通常プロフィールに「群れ」の「クリーチャー特徴」を加えて用いること。1d10ラウンドの終了後に、まだ倒されていなければその群れは撤退する。";
            else if (total >= 41 && total <= 45) content = "布人形：君はランダムな方向に1d10ヤード空中に投げ飛ばされ、地面に落ちて1d10点の「耐久値」を失い（アーマー・ポイント〔AP〕は無視する）、さらに「伏せ状態」になる。";
            else if (total >= 46 && total <= 50) content = "凍りつく四肢：四肢のひとつ（ランダムに決定する）が1d10ラウンドにわたって動かなくなる。この四肢はまるで「身体欠損」（P.180を参照）になったかのように使い物にならない。";
            else if (total >= 51 && total <= 55) content = "闇の目：君は1d10時間にわたって、《第二の視覚》の異能の利益を失う。この時間中、君は〈魔風交信〉テストにも-20のペナルティを被る。";
            else if (total >= 56 && total <= 60) content = "混沌の先見：1d10点ぶんの「幸運ポイント」の余剰プールを得る（これは通常の上限を超えても構わない）。君が「幸運ポイント」を1点消費するたびに、「堕落ポイント」を1点獲得する。なお、セッションの終了時に残存している追加で得たポイントはすべて失われる。";
            else if (total >= 61 && total <= 65) content = "浮遊：君は\"魔力の風\"に乗り、1d10分にわたって地上から1d10ヤードの高さに浮かび上がる。他のキャラクターたちが君を無理やり動かすこともできるし、君も呪文や翼などを使って移動することもできるが、そのままにしておくと君が浮かんだ位置に常に戻ってくる。「浮遊」が終了した後に何が起こるかについては、「落下」（P.166）を参照のこと。";
            else if (total >= 66 && total <= 70) content = "吐き気：君は吐き気が抑えきれなくなり、君の体内にあった量以上の、悪臭を放つ汚物を吐きちらす。「朦朧状態」1つを得る。これは1d10ラウンドにわたって持続する。";
            else if (total >= 71 && total <= 75) content = "混沌の地震：1d100ヤード以内にいるすべてのクリーチャーは「普通」（+20）の〈運動〉テストを行ない、失敗したなら、「伏せ状態」になる。";
            else if (total >= 76 && total <= 80) content = "裏切り：暗黒の神々が恐るべき背信を行なうよう君を誘惑する。君が味方の1人に対する攻撃か、その他何らかの裏切り行為に全力を尽くしたなら、「幸運ポイント」をすべて回復できる。君が他のキャラクターに「運命点」1点を失わせたなら、君がその1点の「運命点」を得る。";
            else if (total >= 81 && total <= 85) content = "穢らわしい弱体化：1点の「堕落ポイント」に加えて、「疲労状態」1つを獲得し、さらに「伏せ状態」にもなる。";
            else if (total >= 86 && total <= 90) content = "凄まじき悪臭：いまや君は鼻が曲がりそうなまでに臭い！君は「攪乱」の「クリーチャー特徴」（P.338）を獲得し、おそらく嗅覚を持つ誰彼すべてからの敵意にさらされる。この効果は1d10時間持続する。";
            else if (total >= 91 && total <= 95) content = "力の流出：君は1d10分にわたって、呪文を発動するために使う異能を使用することができない（通常は《秘術魔術》だが、《混沌魔術》や同種の異能ということもありえる）。";
            else content = "エーテルの逆流：君の【意志力】ボーナス×1ヤード以内にいるすべての者は──敵も味方も同様に──（【頑健力】ボーナスとアーマー・ポイント〔AP〕を無視して）1d10点の「耐久値」を失い、さらに「伏せ状態」になる。射程内に目標が存在しなければ、魔法は行き場を失い、君の頭は爆発し、君は即死する。";

            return Result.CreateBuilder($"大誤発動表({total}).Build() ＞ {content}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private class CriticalTable
        {
            private readonly string _name;
            private readonly List<(int Key, string Value)> _items;

            public CriticalTable(string name, List<(int Key, string Value)> items)
            {
                _name = name;
                _items = items;
            }

            public Result? Roll(IRandomizer randomizer, bool underGankenBonus)
            {
                var dice = randomizer.RollOnce(100);
                if (underGankenBonus)
                {
                    dice = ((dice - 20) < 1 ? 1 : (dice - 20) > 100 ? 100 : (dice - 20));
                }

                string? chosen = null;
                foreach (var (key, value) in _items)
                {
                    if (key >= dice)
                    {
                        chosen = value;
                        break;
                    }
                }

                if (chosen == null)
                {
                    return null;
                }

                return Result.CreateBuilder($"{_name}({dice}).Build() ＞ {chosen}")
                    .SetRands(randomizer.RandResults)
                    .Build();
            }
        }

        private static readonly Dictionary<string, CriticalTable> CRITICAL_TABLES = new Dictionary<string, CriticalTable>
        {
            {
                "H", new CriticalTable("頭部CT表", new List<(int, string)>
                {
                    (10, "勲章となる傷：耐久値-1：「出血状態」1つを得る。社交系テストSL+1(累積なし)。"),
                    (20, "小さな裂傷：耐久値-1：「出血状態」1つを得る。"),
                    (25, "眼窩の負傷：耐久値-1：「目がくらんだ状態」1つを得る。"),
                    (30, "耳朶への強打：耐久値-1：「耳鳴り状態」1つを得る。"),
                    (35, "朦朧化打撃：耐久値-2：「朦朧状態」1つを得る。"),
                    (40, "眼の周りの青あざ：耐久値-2：「目がくらんだ状態」2つを得る。"),
                    (45, "耳朶の裂傷：耐久値-2：「出血状態」2つに加えて、「耳鳴り状態」1つを得る。"),
                    (50, "額への一打：耐久値-2：「出血状態」2つに加えて「目がくらんだ状態」1つを得る。なお後者は「出血状態」を取り除いた後でなければ取り除けない。"),
                    (55, "顎骨折：耐久値-3：「朦朧状態」2つを得る。さらに「骨折(軽度)」の負傷を得る。"),
                    (60, "眼窩の大怪我：耐久値-3：「出血状態」2つを得る。さらに「目がくらんだ状態」1つも得て、こちらは「治療行為」を受けない限り回復できない。"),
                    (65, "耳朶の大怪我：耐久値-3：片耳の聴力を失う。聴覚に関するテストに-20。この効果をもう一度受けた場合、永続的な聴覚喪失者となる。これは魔法でしか治療できない。"),
                    (70, "鼻砕き：耐久値-3：「出血状態」2つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したら「朦朧状態」1つも得る。社交系テストに+1/-1。"),
                    (75, "顎粉砕：耐久値-4：「朦朧状態」3つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したら「気絶状態」を得る。「骨折(重度)」の負傷を得る。"),
                    (80, "脳震盪打撃：耐久値-4：「出血状態」2つと「耳鳴り状態」1つに加えて1d10の「朦朧状態」を得る。さらに1d10日間継続する「疲労状態」1つも得る。この「疲労状態」継続中に頭部への「致命的負傷」を再度得たなら、「普通(+20)」の〈肉体抵抗〉テストを行い、失敗したなら追加で「気絶状態」になる。"),
                    (85, "口粉砕：耐久値-4：「出血状態」2つを得る。1d10本の歯が失われる。「身体欠損(容易)」を被る。"),
                    (90, "耳が屑肉に：耐久値-4：「耳鳴り状態」3つに加えて「出血状態」2つを得る。片耳の機能を失う。「身体欠損(普通)」を被る。"),
                    (93, "眼球破裂：耐久値-5：「目がくらんだ状態」3つと、「出血状態」2つ、「朦朧状態」1つを得る。片目の機能を失う。「身体欠損(難しい)」を被る。"),
                    (96, "顔が化け物のように：耐久値-5：「出血状態」3つ「目がくらんだ状態」3つ、「朦朧状態」2つを得る。片目の機能を失い、鼻梁も失う。「身体欠損(多難)」を被る。"),
                    (99, "顎が屑肉に：耐久値-5：「出血状態」4つと「朦朧状態」3つを得る。「超多難(-30)」な〈肉体抵抗〉テストを行い、失敗したら「気絶状態」になる。「骨折(重度)」の負傷を得て、舌と1d10本の歯を失う。「身体欠損(多難)」を被る。"),
                    (100, "斬首刑さながらに：即死："),
                })
            },
            {
                "A", new CriticalTable("腕部CT表", new List<(int, string)>
                {
                    (10, "腕痺れ：耐久値-1：手に持っているものを落としてしまう。"),
                    (20, "小さな裂傷：耐久値-1：「出血状態」1つを得る。"),
                    (25, "捻挫：耐久値-1：「筋断裂(軽度)」の負傷を得る。"),
                    (30, "ひどい腕痺れ：耐久値-2：手に持っているものを落としてしまい、その腕は1d10-【頑健力】ボーナスのラウンドに渡って使えなくなる。"),
                    (35, "筋断裂：耐久値-2：「出血状態」1つと、「筋断裂(軽度)」の負傷を得る。"),
                    (40, "手の出血：耐久値-2：「出血状態」1つを得る。その手で何かを握る「アクション」を行う場合、先立って「普通(+20)」の【器用度】テストを行い、失敗したら取り落としてしまう。"),
                    (45, "肩の捻挫：耐久値-2：手に持っているものを落としてしまい、その腕は1d10ラウンドに渡って使えなくなる。"),
                    (50, "大きな傷口：耐久値-3：「出血状態」2つを得る。傷口を縫う「手術」を受ける前に同じ腕にダメージを受けた場合、傷口が開き「出血状態」1つを追加で得る。"),
                    (55, "単純骨折：耐久値-3：手に持っているものを落としてしまい、「骨折(軽度)」の負傷を得る。「難しい(-10)」〈肉体抵抗〉テストを行い、失敗したなら「朦朧状態」1つを得る。"),
                    (60, "靱帯断裂：耐久値-3：手に持っているものを落としてしまい、「筋断裂(重度)」の負傷を得る。"),
                    (65, "深手：耐久値-3：「出血状態」2つを得る。「朦朧状態」1つを得て、「筋断裂(軽度)」の負傷を得る。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「気絶状態」になる。"),
                    (70, "動脈破損：耐久値-4：「出血状態」4つを得る。「手術」を受ける前に同じ腕にダメージを受けるたびに「出血状態」2つを得る。"),
                    (75, "肘粉砕：耐久値-4：手に持っているものを落としてしまい、「筋断裂(重度)」の負傷を得る。"),
                    (80, "肩の脱臼：耐久値-4：「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「朦朧状態」1つを得て「伏せ状態」になる。手に持っているものを落としてしまう。その腕を使うことはできず、欠損したものとして扱う。さらに「治療行為」を受けるまでの間「朦朧状態」1つを得る。"),
                    (85, "指切断：耐久値-4：「身体欠損(普通)」を被る。「出血状態」1つを得る。"),
                    (90, "手の半切断：耐久値-5：指一本を失い、「身体欠損(多難)」を被る。「出血状態」2つと「朦朧状態」1つを得る。君が「治療行為」を受けるまでの以降の各ラウンドに指を一本ずつ失う。すべての指を失ったら、その手の機能を失い「身体欠損(難しい)」を得る。"),
                    (93, "力こぶがズタズタに：耐久値-5：手に持っていたものを落とすとともに「筋断裂(重度)」の負傷を得て、「出血状態」2つと「朦朧状態」1つを得る。"),
                    (96, "片手が屑肉同然に：耐久値-5：その手の機能を失い、「身体欠損(多難)」を被る。「出血状態」2つを得る。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「気絶状態」になる。"),
                    (99, "靱帯断裂：耐久値-5：腕は無用の長物と化す―「身体欠損(超多難)」を被る。「出血状態」3つ、「朦朧状態」1つを得て、「伏せ状態」になる。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「気絶状態」になる。"),
                    (100, "残酷な腕の喪失：即死："),
                })
            },
            {
                "B", new CriticalTable("胴体CT表", new List<(int, string)>
                {
                    (10, "ただのかすり傷だって！：耐久値-1：「出血状態」1つを得る。"),
                    (20, "みぞおちへの一打：耐久値-1：「朦朧状態」1つを得る。「容易(+40)」な〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。"),
                    (25, "下腹部に直撃！：耐久値-1：「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「朦朧状態」3つを得る。"),
                    (30, "背中を捻じる：耐久値-1：「筋断裂(軽度)」の負傷を得る。"),
                    (35, "息ができない：耐久値-2：「朦朧状態」1つを得る。「普通(+20)」の〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。また、1d10ラウンドに渡って「移動力」が半分になる。"),
                    (40, "肋骨に青あざ：耐久値-2：1d10日間に渡って【敏捷力】に関連するテストに-10。"),
                    (45, "鎖骨捻じれ：耐久値-2：左右どちらの鎖骨かランダムに決定する。そちらの手に持っている物を落としてしまい、1d10ラウンドに渡ってその腕は使用できない。"),
                    (50, "裂傷：耐久値-2：「出血状態」2つを得る。"),
                    (55, "肋骨にひびが：耐久値-3：「朦朧状態」1つを得る。また、「骨折(軽度)」の負傷を得る。"),
                    (60, "大きな傷口：耐久値-3：「出血状態」3つを得る。「手術」を受けるまでの間、胴体に命中を受けるたびに追加で1つの「出血状態」を得る。"),
                    (65, "激痛を伴う裂傷：耐久値-3：「出血状態」2つと「朦朧状態」1つを得る。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「気絶状態」になる。またSL+4以上でテストに成功していない限り、金切り声を上げてしまう。"),
                    (70, "動脈損傷：耐久値-3：「出血状態」4つを得る。「手術」を受けるまでの間、胴体に攻撃を受けるたびに追加で2つの「出血状態」を得る。"),
                    (75, "背筋断裂：耐久値-4：「筋断裂(重度)」の負傷を得る。"),
                    (80, "股関節骨折：耐久値-4：「朦朧状態」1つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」にもなる。「骨折(軽度)」の負傷を得る。"),
                    (85, "胸郭の大怪我：耐久値-4：「出血状態」4つを得る。「手術」を受けるまでの間、胴体に命中を受けるたびに追加で2つの「出血状態」を得る。"),
                    (90, "内臓の損傷：耐久値-4：「膿み傷」に罹患し、「出血状態」2つを得る。"),
                    (93, "胸郭粉砕：耐久値-5：「朦朧状態」1つを得る。これは「治療行為」を受けない限り取り除くことは出来ない。さらに「骨折(重度)」の負傷を得る。"),
                    (96, "鎖骨粉砕：耐久値-5：「気絶状態」になる。これは「治療行為」を受けない限り取り除くことは出来ない。さらに「骨折(重度)」の負傷を得る。"),
                    (99, "内臓出血：耐久値-5：「出血状態」1つを得る。これは「手術」を受けない限り取り除くことは出来ない。さらに「血液腐れ病」に罹患する。"),
                    (100, "胴部両断：即死："),
                })
            },
            {
                "L", new CriticalTable("脚部CT表", new List<(int, string)>
                {
                    (10, "爪先をぶつける：耐久値-1：「普通(+20)」の〈肉体抵抗〉テストを行い、失敗したなら次のターンの終了時まで【敏捷力】に関するテストに-10。"),
                    (20, "足首の捻挫：耐久値-1：以降1d10ラウンドに渡って【敏捷力】に関連するテストに-10。"),
                    (25, "小さな裂傷：耐久値-1：「出血状態」1つを得る。"),
                    (30, "足がかりを失う：耐久値-1：「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。"),
                    (35, "太腿への打撃：耐久値-2：「出血状態」1つを得て「普通(+20)」の〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。"),
                    (40, "足首の筋断裂：耐久値-2：「筋断裂(軽度)」の負傷を得る。"),
                    (45, "膝の捻挫：耐久値-2：以降1d10ラウンドに渡って【敏捷力】に関連するテストに-20。"),
                    (50, "爪先のひどい裂傷：耐久値-2：「出血状態」1つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したなら片方の爪先を失う―「身体欠損(普通)」を被る。"),
                    (55, "ひどい裂傷：耐久値-3：「出血状態」2つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。"),
                    (60, "膝のひどい捻挫：耐久値-3：「筋断裂(重度)」の負傷を得る。"),
                    (65, "脚への痛打：耐久値-3：「伏せ状態」に加えて「出血状態」2つを得て、「骨折(軽度)」の負傷を得る。さらに「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「朦朧状態」1つを追加で得る。"),
                    (70, "太腿への深手：耐久値-3：「出血状態」3つを得る。「手強い(+0)」〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」になる。「手術」を受けるまでの間、この脚に命中を受けるたびに、追加で1つの「出血状態」を得る。"),
                    (75, "靱帯断裂：耐久値-4：「伏せ状態」に加えて「朦朧状態」1つを得る。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「気絶状態」になる。君のその脚は使えなくなる。"),
                    (80, "脛への深手：耐久値-4：敵の武器が君の膝下に当たり、骨に食い込んで腱を切断する。「朦朧状態」1つを得て、「伏せ状態」になる。さらに「筋断裂(重度)」と「骨折(軽度)」の負傷を得る。"),
                    (85, "膝粉砕：耐久値-4：「出血状態」1つと「朦朧状態」1つを得ることに加えて、「伏せ状態」になる。「骨折(重度)」の負傷を得る。"),
                    (90, "膝関節脱臼：耐久値-4：「伏せ状態」になる。「多難(-20)」な〈肉体抵抗〉テストを行い、失敗したなら「朦朧状態」1つを得る。これは「治療行為」を受けない限り取り除くことができない。"),
                    (93, "脚粉砕：耐久値-5：「普通(+20)」の〈肉体抵抗〉テストを行い、失敗したなら「伏せ状態」を得て、足の指1本を失う。さら、SLが-1あるごとに追加で1つの足指を失う―「身体欠損(普通)」を被る。「出血状態」2つを得る。"),
                    (96, "足切断：耐久値-5：「身体欠損(多難)」を被る。「出血状態」3つと「朦朧状態」2つを得て「伏せ状態」になる。"),
                    (99, "アキレス腱切断：耐久値-5：苦痛に金切り声を上げながら倒れる。「出血状態」2つと「朦朧状態」2つを得て「伏せ状態」になる。「身体欠損(超多難)」を被る。"),
                    (100, "骨盤粉砕：即死："),
                })
            },
        };

    }
}
