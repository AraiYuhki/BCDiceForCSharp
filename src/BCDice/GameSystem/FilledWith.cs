using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// フィルトウィズ
    /// </summary>
    public sealed class FilledWith : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly FilledWith Instance = new FilledWith();


        private static readonly Regex Cook18Regex = new Regex(
            @"^COOK([1-8])$",
            RegexOptions.Compiled);

        private static readonly Regex TrapEnhlxRegex = new Regex(
            @"^TRAP[ENHLX]$",
            RegexOptions.Compiled);

        private static readonly Regex TrsRegex = new Regex(
            @"^TRS.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RandRegex = new Regex(
            @"^RAND.*$",
            RegexOptions.Compiled);

        private static readonly Regex RencRegex = new Regex(
            @"^RENC.*$",
            RegexOptions.Compiled);

        private static readonly Regex RedRegex = new Regex(
            @"^RED.*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex RopEnhlxRegex = new Regex(
            @"^ROP[ENHLX]$",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "FilledWith";

        /// <inheritdoc/>
        public override string Name => "フィルトウィズ";

        /// <inheritdoc/>
        public override string SortKey => "ふいるとういす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 (3FW@x#y<=z or z-3FW@x#y)
         3個の6面ダイスを振る判定。
         @x:xにクリティカル値を入力。省略可。(省略時クリティカル値4)
         #y:yにファンブル値を入力。省略可(省略時ファンブル値17)
         <=z or z-:zに目標値を入力。±の計算に対応。省略可。
        ・【必殺技！】 (HST)
         ホムンクルス特技【必殺技！】表。
        ・マジカルクッキング (COOKx)
         マジカルクッキングのシェフのおすすめコース。
         xにクッキングレベルを入力。(1-8)
        ・ナンバーワンくじ (LOTN or LOTP)
         LOTNでノーマルくじ、LOTPでプレミアムくじ。(GURPS-FW版)
        ----------夢幻の迷宮用----------
        ・共通書式
         a:aに地形(1-6の数字)を入力。省略可。(省略時ランダム決定)
          (1:洞窟 2:遺跡 3:山岳 4:水辺 5:森林 6:墓場)
         d:dに難易度を入力。(E:初級 N:中級 H:上級 L:悪夢 X:伝説)
        ・ランダムイベント表 (RANDda)
        ・ランダムエンカウント表 (RENCda)
        ・エネミーデータ表 (REDde)
         エネミーデータ参照表。
         GMがシークレットダイスで参照するとPLに知られずにエネミーデータを参照可能。
         e:3桁のイベントダイスを入力(D666の結果)。
        ・トラップ表 (TRAPd)
        ・財宝表 (TRSr±x)
         r:rに財宝ランクを入力。
         ±x:xに財宝ランク修正値を入力。省略可。
        ・迷宮追加オプション表(ROPd)
        ";

        private static readonly string[] DIFFICULTYS = { "E", "N", "H", "L", "X" };
        private static readonly Dictionary<string, string> DIFFICULTY_NAMES = new Dictionary<string, string>
        {
            { "E", "初級" },
            { "N", "中級" },
            { "H", "上級" },
            { "L", "悪夢" },
            { "X", "伝説" },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            // ダイスロールコマンド
            var fwResult = FwRoll(command, randomizer);
            if (fwResult != null) return fwResult;

            if (command == "LOTN")
            {
                // TODO: RollJumpTable("ナンバーワンノーマルくじ", LOT_NORMAL_TABLES[1])
                return null;
            }

            if (command == "LOTP")
            {
                // TODO: RollJumpTable("ナンバーワンプレミアムくじ", LOT_PREMIUM_TABLES[1])
                return null;
            }

            var cookMatch = Cook18Regex.Match(command);
            if (cookMatch.Success)
            {
                var lv = Convert.ToInt32(cookMatch.Groups[1].Value);
                // TODO: RollJumpTable("マジカルクッキング", COOK_TABLES[lv])
                return null;
            }

            var trapMatch = TrapEnhlxRegex.Match(command);
            if (trapMatch.Success)
            {
                return RollTrapTable(command, randomizer);
            }

            var trsMatch = TrsRegex.Match(command);
            if (trsMatch.Success)
            {
                // TODO: GetTresureResult(command)
                return null;
            }

            var randMatch = RandRegex.Match(command);
            if (randMatch.Success)
            {
                return RollRandomEventTable(command, randomizer);
            }

            var rencMatch = RencRegex.Match(command);
            if (rencMatch.Success)
            {
                return RollRandomEventTable(command, randomizer);
            }

            var redMatch = RedRegex.Match(command);
            if (redMatch.Success)
            {
                // TODO: FetchEnemyData(command)
                return null;
            }

            var ropMatch = RopEnhlxRegex.Match(command);
            if (ropMatch.Success)
            {
                return RollRandomOptionTable(command, randomizer);
            }

            // TODO: RollTables(command, TABLES)

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private string FormatTableRollResult(string tableName, string number, string result)
        {
            return $"{tableName}({number}):{result}";
        }

        private Result? RollTrapTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^TRAP([ENHLX])$");
            if (!m.Success)
            {
                return null;
            }
            var difficultySign = m.Groups[1].Value;
            var difficultyIndex = Array.IndexOf(DIFFICULTYS, difficultySign);
            var difficultyName = DIFFICULTY_NAMES[difficultySign];
            var number = randomizer.RollSum(3, 6);
            var chosen = TRAP_TABLE[number - 3];
            var formatted = FormatTrapRowStr(chosen, difficultyIndex);
            return Result.CreateBuilder($"トラップ表<{difficultyName}>({number}).Build():{formatted}").Build();
        }

        private Result? RollRandomOptionTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^ROP([ENHLX])$");
            if (!m.Success)
            {
                return null;
            }
            var difficultySign = m.Groups[1].Value;
            var difficultyName = DIFFICULTY_NAMES[difficultySign];
            var difficultyIndex = Array.IndexOf(DIFFICULTYS, difficultySign);
            var value = randomizer.RollD66(D66SortType.NoSort);
            if (OPTION_TABLE.ContainsKey(value))
            {
                var row = OPTION_TABLE[value];
                var formatted = FormatTrapRowStr(row, difficultyIndex);
                return Result.CreateBuilder($"迷宮追加オプション表<{difficultyName}>({value}).Build():{formatted}").Build();
            }
            return null;
        }

        private Result? RollRandomEventTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(RAND|RENC)([ENHLX])([1-6])?$");
            if (!m.Success)
            {
                return null;
            }
            // type: null for RAND, 4 for RENC (encounter type)
            // var type = m.Groups[1].Value == "RAND" ? (int?)null : 4;
            var difficultySign = m.Groups[2].Value;
            var difficultyName = DIFFICULTY_NAMES[difficultySign];
            var area = m.Groups[3].Success && !string.IsNullOrEmpty(m.Groups[3].Value) ? int.Parse(m.Groups[3].Value) : randomizer.RollOnce(6);
            // TODO: Implement EVENT_TABLES lookup
            return Result.CreateBuilder($"ランダムイベント表<{difficultyName}>(地形:{area}).Build()").Build();
        }

        // FW dice roll implementation
        private Result? FwRoll(string command, IRandomizer randomizer)
        {
            var fw = FwParse(command);
            if (fw == null) return null;

            var diceList = randomizer.RollBarabara(fw.DiceCount, 6);
            var dice = diceList.Sum();
            var diceStr = string.Join(",", diceList);

            var res = FwResult(fw, dice, randomizer);

            var sequence = new List<string> { $"({FwExpr(fw)})", $"{dice}[{diceStr}]", res.Text }.Where(x => !string.IsNullOrEmpty(x));
            var fullText = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(fullText)
                .SetCritical(res.IsCritical)
                .SetFumble(res.IsFumble)
                .SetSuccess(res.IsSuccess)
                .SetFailure(res.IsFailure)
                .Build();
        }

        private string FwExpr(FwData fw)
        {
            var ret = $"{fw.DiceCount}FW";
            if (fw.Critical != 4) ret += $"@{fw.Critical}";
            if (fw.Fumble != 17) ret += $"#{fw.Fumble}";
            if (fw.Target.HasValue) ret += $"<={fw.Target.Value}";
            return ret;
        }

        private Result FwResult(FwData fw, int total, IRandomizer randomizer)
        {
            if (total <= fw.Critical)
            {
                return Result.CreateBuilder("クリティカル！").SetCritical(true).SetSuccess(true).Build();
            }
            else if (total >= fw.Fumble)
            {
                return Result.CreateBuilder("ファンブル！").SetFumble(true).SetFailure(true).Build();
            }
            else if (fw.Target.HasValue)
            {
                var success = fw.Target.Value - total;
                if (total <= fw.Target.Value)
                {
                    return Result.CreateBuilder($"成功(成功度:{success}).Build()").SetSuccess(true).Build();
                }
                else
                {
                    return Result.CreateBuilder($"失敗(失敗度:{success}).Build()").SetFailure(true).Build();
                }
            }
            else
            {
                return Result.CreateBuilder("").Build();
            }
        }

        private FwData FwParse(string command)
        {
            // Pattern 1: target-3FW@critical#fumble (e.g. "10-3FW@4#17")
            var m = Regex.Match(command, @"^(\d[+\-\d]*)-(\d+)FW(?:@(\d+))?(?:#(\d+))?$");
            if (m.Success)
            {
                return new FwData
                {
                    DiceCount = Convert.ToInt32(m.Groups[2].Value),
                    Target = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor),
                    Critical = m.Groups[3].Success && !string.IsNullOrEmpty(m.Groups[3].Value) ? int.Parse(m.Groups[3].Value) : 4,
                    Fumble = m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value) ? int.Parse(m.Groups[4].Value) : 17,
                };
            }

            // Pattern 2: 3FW@critical#fumble<=target (e.g. "3FW@4#17<=10")
            m = Regex.Match(command, @"^(\d+)FW(?:@(\d+))?(?:#(\d+))?(?:<=([+\-\d]+))?$");
            if (m.Success)
            {
                var fw = new FwData
                {
                    DiceCount = Convert.ToInt32(m.Groups[1].Value),
                    Critical = m.Groups[2].Success && !string.IsNullOrEmpty(m.Groups[2].Value) ? int.Parse(m.Groups[2].Value) : 4,
                    Fumble = m.Groups[3].Success && !string.IsNullOrEmpty(m.Groups[3].Value) ? int.Parse(m.Groups[3].Value) : 17,
                };
                if (m.Groups[4].Success && !string.IsNullOrEmpty(m.Groups[4].Value))
                {
                    fw.Target = ArithmeticEvaluator.Eval(m.Groups[4].Value, RoundType.Floor);
                }
                return fw;
            }

            return null;
        }

        private class FwData
        {
            public int DiceCount { get; set; }
            public int? Target { get; set; }
            public int Critical { get; set; } = 4;
            public int Fumble { get; set; } = 17;
        }

        // Trap row: body with format placeholders, plus optional difficulty-indexed args
        private static string FormatTrapRow((string Body, int[][] Args) row, int difficultyIndex)
        {
            if (row.Args == null || row.Args.Length == 0)
            {
                return row.Body;
            }
            var args = row.Args.Select(a => difficultyIndex < a.Length ? a[difficultyIndex].ToString() : "").ToArray();
            return string.Format(row.Body, args);
        }

        private static string FormatTrapRowStr((string Body, string[][] Args) row, int difficultyIndex)
        {
            if (row.Args == null || row.Args.Length == 0)
            {
                return row.Body;
            }
            var args = row.Args.Select(a => difficultyIndex < a.Length ? a[difficultyIndex] : "").ToArray();
            return string.Format(row.Body, args);
        }

        // Trap table data (body, args arrays indexed by difficulty)
        private static readonly (string Body, string[][] Args)[] TRAP_TABLE = new[]
        {
            ("トライディザスター:宝箱から広範囲に火炎・冷気・電撃が放たれる罠。PC全員に「{0}」の「火炎」「冷気」「電撃」属性ダメージ。", new[] { new[] { "3D6+3", "3D6+50", "3D6+70", "3D6+100", "300" } }),
            ("ペトロブラスター:広範囲に石化光線を放つ罠。PC全員[抵抗-{0}]判定を行い、失敗したPCはBS「石化」を受ける。", new[] { new[] { "2", "4", "6", "8", "10" } }),
            ("クロスボウストリーム:宝箱から矢の嵐が放たれる罠。PC全員に「{0}」の「刺突」属性ダメージ。「ドッジ-{1}」で〔回避〕が可能。", new[] { new[] { "3D6+20", "3D6+40", "3D6+60", "3D6+90", "200" }, new[] { "4", "6", "8", "10", "20" } }),
            ("フォーチュンイーター:PC全員の幸運を食らい、Ftを{0}点減少させる。Ftが0の場合「{1}」点の防護点無視ダメージ。", new[] { new[] { "1", "2", "3", "4", "5" }, new[] { "3D6+30", "3D6+50", "3D6+70", "3D6+100", "300" } }),
            ("スロット:解除に失敗しても害はないが、スロットが揃うまで開かない宝箱。スロットを1回まわすには{0}GPが必要。行動を消費して[感覚-{1}]判定に成功すればスロットは揃う。有利な特異点「ビビット反射」があれば判定に+4のボーナス。", new[] { new[] { "100", "300", "600", "1000", "10000" }, new[] { "4", "6", "8", "10", "15" } }),
            ("テレポーター:PC全員(とエンカウントしているエネミー)を転送して道に迷わせる。「財宝ランク」が1段階減少する。", null),
            ("アイスコフィン:宝箱を開けようとしたキャラクターを氷漬けにする罠。対象1体に「{0}」の「冷気」属性ダメージ。更にFPにも{1}点の防護点無視ダメージ。", new[] { new[] { "3D6+30", "3D6+50", "3D6+70", "3D6+100", "300" }, new[] { "5", "10", "15", "20", "30" } }),
            ("クロスボウ:宝箱を開けようとしたキャラクターに強力な矢が放たれる罠。対象1体に「{0}」の「刺突」属性ダメージ。「ドッジ-{1}」", new[] { new[] { "3D6+20", "3D6+40", "3D6+60", "3D6+90", "200" }, new[] { "4", "6", "8", "10", "20" } }),
            ("毒針:宝箱を開けようとしたキャラクターに毒針を突き刺す罠対象1体に{0}点の防護点無視ダメージ。更に[抵抗-{1}]判定に失敗するとシナリオ終了まであらゆる判定に-2のペナルティ。", new[] { new[] { "15", "30", "45", "60", "150" }, new[] { "4", "6", "8", "10", "15" } }),
            ("アラーム:即座にその地形のエンカウント表を振って、それに対応したエネミーが出現する。出現したエネミーはそのターンから行動順に組み込まれる。出現するエネミー以外の記述は無視する。", null),
            ("殺人鬼の斧:宝箱を開けようとしたキャラクターに斧が振り下ろされる罠。対象1体に「{0}」の「打撃」「斬撃」属性ダメージ。「ドッジ-{1}」か「シールド-{2}」で〔回避〕が可能。", new[] { new[] { "3D6+30", "3D6+50", "3D6+70", "3D6+100", "300" }, new[] { "4", "6", "8", "10", "20" }, new[] { "4", "6", "8", "10", "-20" } }),
            ("死神:宝箱を開けようとしたキャラクターに死神を取り憑かせる罠。4ラウンド目が終了するまであらゆる判定に-3のペナルティを受け、4ラウンド目の終了と同時に「{0}」の防護点無視ダメージ。", new[] { new[] { "3D6+30", "3D6+50", "3D6+70", "3D6+100", "300" } }),
            ("幻の宝:宝箱に偽の財宝を入れ、本物の財宝を入手させない罠。トラップが発動すると価値の無い偽の宝物「幻の宝」を入手してしまう。「幻の宝」はアイテム欄を3つ占有し、シナリオ終了まで捨てられない。アイテム欄に空きがない場合は、何かを捨てて誰かが必ず持たなくてはならない。", null),
            ("エクスプロージョン:宝箱が大爆発を起こし、中身を粉々にしてしまう罠。宝箱の中身は消滅する。PC全員に「{0}」の「打撃」「火炎」属性ダメージ。", new[] { new[] { "3D6+10", "3D6+30", "3D6+50", "3D6+80", "200" } }),
            ("レインボーポイズン:宝箱から七色の毒ガスが放たれる罠。PC全員に「{0}」の防護点無視ダメージ。更にシナリオ終了まであらゆる判定に-2のペナルティ。[抵抗-{1}]判定に成功すれば無効。", new[] { new[] { "3D6+10", "3D6+30", "3D6+50", "3D6+80", "200" }, new[] { "4", "6", "8", "10", "15" } }),
            ("デスクラウド:宝箱から致死性の毒ガスを放つ罠。PC全員を即死させる。[抵抗-{0}]判定に成功すれば無効。", new[] { new[] { "2", "4", "6", "8", "12" } }),
        };

        // Option table (D66)
        private static readonly Dictionary<int, (string Body, string[][] Args)> OPTION_TABLE = new Dictionary<int, (string, string[][])>
        {
            { 11, ("黄金の迷宮(財宝ランク+2):全てが黄金で彩られた迷宮。財宝ランクが大きく上昇する。", null) },
            { 12, ("密林の迷宮(財宝ランク+1):密林の中にひっそりとたたずむ迷宮。分類が「魔獣」「獣人」「霊獣」のエネミーが行うあらゆる判定に+2のボーナス。", null) },
            { 13, ("カラクリの迷宮:複雑なカラクリが周囲で絶え間なく動いている迷宮。分類「ギア」のエネミーが行うあらゆる判定に+2のボーナス。クリア時に「アタッチメント割引券」を全員が{0}枚獲得。", new[] { new[] { "1", "2", "3", "5", "10" } }) },
            { 14, ("フラウの舞踏会:あちこちに花畑のある迷宮。フラウが発生するランダムイベントが発生した際、「この迷宮を制覇して、私達が舞踏会を開けるようにしてね」とお願いされ、クリア時の報酬に{0}が追加される。", new[] { new[] { "「キノコの帽子」(装飾品)", "「猛毒の花」(装飾品)", "「フルブロウン」(鎧)", "「緊急召喚の宝珠」(装飾品)", "魔将樹の大剣（剣）" } }) },
            { 15, ("アズマ風の迷宮:風流なアズマ風の迷宮。武器に「刀」を持つエネミーが行うあらゆる判定に+2のボーナス。クリア時に「アタッチメント割引券」を全員が{0}枚獲得。", new[] { new[] { "1", "2", "3", "5", "10" } }) },
            { 16, ("枯れた泉の迷宮:「全地形1-1」の回復の泉が全て枯れており、回復効果を得ることができない。「山岳1-6」の貴重な水源や、「水辺1-6」の毒の泉などはそのまま存在する。", null) },
            { 21, ("天空への道(財宝ランク+1):上へ上へと果てしなく登っていく迷宮。空気が薄くなって疲労しやすくなる。【特技】特技などによるFP消費が全て+3。", null) },
            { 22, ("灼熱焦土の迷宮(財宝ランク+1):とてつもなく暑く、あちこちで炎が燃え盛る迷宮。エネミーが行う「火炎」属性を含む攻撃の致傷力に+{0}のボーナス。", new[] { new[] { "10", "20", "30", "50", "100" } }) },
            { 23, ("灼熱焦土の迷宮(財宝ランク+1):とてつもなく寒く、気温が氷点下の迷宮。エネミーが行う「冷気」属性を含む攻撃の致傷力に+{0}のボーナス。", new[] { new[] { "10", "20", "30", "50", "100" } }) },
            { 24, ("盗賊王の迷宮:迷宮内での罠や鍵を解除する[感覚]判定に-3のペナルティ。4ラウンドまでに出現した宝箱の「財宝ランク」+1。", null) },
            { 25, ("ミミック狂暴化:「全地形2-5」のミミックの致傷力に+{0}のボーナス。ミミックを見破った場合に得られるGPが{1}GP増加する。", new[] { new[] { "20", "30", "50", "80", "150" }, new[] { "500", "1000", "3000", "5000", "20000" } }) },
            { 26, ("トレジャーイーター狂暴化:「全地形2-6」のトレジャーイーターを見破る[知力]判定に-3のペナルティ。4ラウンドまでに出現した宝箱の「財宝ランク」+1。", null) },
            { 31, ("暗闇の迷宮:どこもかしこも真っ暗な迷宮。「猫の目」などがなければ視覚に関する[感覚]判定に-5のペナルティ。", null) },
            { 32, ("騒音の迷宮:常に大音量で謎の音楽(BGM)が鳴っている迷宮。聴覚に関する[感覚]判定に-5のペナルティ。", null) },
            { 33, ("未知の怪物の迷宮(財宝ランク+1):エネミーの姿がシルエットのみになる迷宮。エネミーのデータがいかなる手段でも判明させられなくなる。(通常通り〔HP〕〔FP〕〔先制〕は判明する)", null) },
            { 34, ("氾濫中の迷宮:大雨が降っており、川などが氾濫している迷宮。水泳を行う際の[敏捷]判定に-5のペナルティ。「森林3-6」の山火事イベントの効果は無視できる。", null) },
            { 35, ("間抜けの迷宮(財宝ランク+1):頭がおかしくなりそうな極彩色の迷宮。[知力][意志]判定に-2のペナルティ。[知力]や[意志]そのものが下がるわけではない。", null) },
            { 36, ("瘴気の迷宮(財宝ランク+1):生命力を奪う紫の霧で満ちた迷宮。〔HP〕の最大値に-{0}のペナルティ。", new[] { new[] { "10", "20", "30", "40", "50" } }) },
            { 41, ("加速する迷宮:狂ったように針の動く時計が多数された迷宮。「CT:安息の日」以外の【特技】が「CT:なし」になる。", null) },
            { 42, ("停滞する迷宮(財宝ランク+1):動かない時計が多数設置された迷宮。「CT:安息の日」以外のCTの存在する【特技】が「CT:シナリオ終了」になる。この効果はシナリオ終了まで持続する。", null) },
            { 43, ("猛毒の迷宮(財宝ランク+1):見るからに毒々しい紫色の沼があちこちにある迷宮。エネミーが行う、名称に「毒」もしくは「ポイズン」が入る【特技】や、名称に「毒」もしくは「ポイズン」が入るトラップの致傷力に+{0}のボーナス。", new[] { new[] { "10", "20", "40", "50", "100" } }) },
            { 44, ("死の迷宮(財宝ランク+2):死の運命から逃れることのできない、血まみれの迷宮。「生命保険証」の効果が適用されない。", null) },
            { 45, ("幸運の迷宮:何者かの加護を感じる迷宮。PC全員のFtの最大値と現在値に+1のボーナス。この効果はシナリオ終了まで持続する。", null) },
            { 46, ("不運の迷宮:PC全員のFt最大値と現在値に-1のペナルティ。この効果はシナリオ終了まで持続する。", null) },
            { 51, ("レアメタルの迷宮:非常にレアなエネミー「レアメタルキャンディー」「レアメタルクラウン」が生息している迷宮。キャンディークラウン(CL40)、ゴールデンクラウン(CL177)から獲得できる通常ドロップのGPが5倍になる。", null) },
            { 52, ("魔力の泉:PCとエネミーの双方が、〔FP〕を消費せずに【魔法】を使用できるようになる。最終的な消費〔FP〕が最大〔FP〕より大きい【魔法】は使用できない。", null) },
            { 53, ("ブルーの迷宮:陰鬱な気分になり、他のキャラクターと関わる気力を失う。PC全員が不利な特異点「嫌な奴」を1段階得る。", null) },
            { 54, ("レッドの迷宮:なぜか興奮して非常に好戦的になる。PC全員が不利な特異点「脳みそ筋肉」を得る。交戦中に「1:回復系」のイベントが発生しても戦闘を終了させることができない。", null) },
            { 55, ("ピンクの迷宮:なんだか身近な異性(同性も?)が気になって仕方なくなる。PC全員が不利な特異点「英雄色を好む」を得る。魔族も戦闘意欲を失い、「分類:魔族」のエネミーが出現するイベントは無視する。", null) },
            { 56, ("ハズレの迷宮(財宝ランク-1):ツギハギだらけの壁などでできた、ハリボテのような貧相な迷宮。宝箱の中身もなんだか貧相になる。", null) },
            { 61, ("ラダマンティスの迷宮(財宝ランク+2):第一魔将ラダマンティスの像が入口に設置された迷宮。全てのエネミーが行うあらゆる判定に+2のボーナス。また、「遺跡6-6」のイベントのダメージ+{0}。", new[] { new[] { "20", "40", "60", "80", "150" } }) },
            { 62, ("グレイヴディガーの迷宮(財宝ランク+2):第二魔将グレイヴディガーの像が入口に設置された迷宮。「分類:アンデッド」のエネミーが行うあらゆる判定に+5のボーナス。", null) },
            { 63, ("ハイペリオンの迷宮(財宝ランク+2):第三魔将ハイペリオンの像が入口に設置された迷宮。全てのエネミーが「ターン開始」時に〔HP〕を全回復する。", null) },
            { 64, ("ムスペルニヴルの迷宮(財宝ランク+2):勇ましくも美しい女性の像が設置された迷宮。エネミーが行う「火炎」もしくは「冷気」属性を含む攻撃の致傷力に+{0}のボーナス。", new[] { new[] { "20", "40", "60", "80", "150" } }) },
            { 65, ("ウェルスの迷宮:人懐っこそうなアズマ風の青年が設置された迷宮。シナリオ上で第五魔将の正体が明らかに鳴っている場合のみ、PC全員のFtの最大値と現在値に+5のボーナス。この効果はシナリオ終了まで持続する。", null) },
            { 66, ("バロールの迷宮(財宝ランク+2):第六魔将バロールの像が入口に設置された迷宮。「分類:ギア」のエネミーが行うあらゆる判定に+5のボーナス。", null) },
        };

    }
}
