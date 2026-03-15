using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// トリニティセブンRPG
    /// </summary>
    public sealed class TrinitySeven : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TrinitySeven Instance = new TrinitySeven();

        /// <inheritdoc/>
        public override string Id => "TrinitySeven";

        /// <inheritdoc/>
        public override string Name => "トリニティセブンRPG";

        /// <inheritdoc/>
        public override string SortKey => "とりにていせふんRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        クリティカルが変動した命中及び、7の出目がある場合のダメージ計算が行なえます。
        なお、通常の判定としても利用できます。

        ・発動/命中　［TR(±c*)<=(x)±(y*) 又は TR<=(x) など］*は必須ではない項目です。
        ""TR(クリティカルの修正値*)<=(発動/命中)±(発動/命中の修正値*)""
        加算減算のみ修正値も付けられます。 ［修正値］は必須ではありません。
        例）TR<=50 TR<=60+20 TR7<=40 TR-7<=80 TR+10<=80+20

        ・ダメージ計算　［(x)DM(c*)±(y*) 又は (x)DM(c*) 又は (x)DM±(y*)］*は必須ではない項目です。
        ""(ダイス数)DM(7の出目の数*)+(修正*)""
        加算減算のみ修正値も付けられます。 ［7の出目の数］および［修正値］は必須ではありません。
        例）6DM2+1 5DM2 4DM 3DM+3
        後から7の出目に変更する場合はC(7*6＋5)のように入力して計算してください。

        ・名前表　[TRNAME]
        名字と名前を出します。PCや突然現れたNPCの名付けにどうぞ。

        ";

        private static readonly Regex HitRegex = new Regex(
            @"^TR([+\-]?\d*)<=(\d+)([+\-]\d+)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DamageRegex = new Regex(
            @"^(\d+)DM(\d*)([+\-]\d+)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollHit(command, randomizer) ?? RollDamage(command, randomizer) ?? RollName(command, randomizer);
        }

        private Result? RollHit(string command, IRandomizer randomizer)
        {
            var m = HitRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var critModStr = m.Groups[1].Value;
            var critMod = string.IsNullOrEmpty(critModStr) ? 0 : int.Parse(critModStr);
            var target = int.Parse(m.Groups[2].Value);
            var modifyStr = m.Groups[3].Value;
            var modify = string.IsNullOrEmpty(modifyStr) ? 0 : int.Parse(modifyStr);

            var totalModify = critMod + modify;
            var critical = 7 + totalModify;

            var total = randomizer.RollOnce(100);
            var result = GetHitRollResult(total, target, critical);

            // Build command text for display
            var modText = GetModifyText(totalModify);
            var cmdText = $"(TR{modText}<={target})";

            var text = $"{cmdText} ＞ {total} ＞ {result.Text}";

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private Result GetHitRollResult(int total, int target, int critical)
        {
            if (total >= 96)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= critical)
            {
                return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total <= target)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

        private Result? RollDamage(string command, IRandomizer randomizer)
        {
            var m = DamageRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var diceCount = int.Parse(m.Groups[1].Value);
            var criticalStr = m.Groups[2].Value;
            var critical = string.IsNullOrEmpty(criticalStr) ? 0 : int.Parse(criticalStr);
            var modifyStr = m.Groups[3].Value;
            var modify = string.IsNullOrEmpty(modifyStr) ? 0 : int.Parse(modifyStr);

            var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            var diceText = string.Join(",", diceList);

            var (total, additionalList) = GetRollDamageResult(diceCount, critical, diceList, modify);

            var additionalListText = (additionalList == null) ? "" : $"→[{string.Join(",", additionalList)}]";
            var modText = GetModifyText(modify);

            var cmdDisplay = $"{diceCount}DM{(critical > 0 ? critical.ToString() : "")}{(modify != 0 ? (modify > 0 ? $"+{modify}" : modify.ToString()) : "")}";
            var text = $"({cmdDisplay}) ＞ [{diceText}]{additionalListText}{modText} ＞ {total}";

            return Result.CreateBuilder(text).Build();
        }

        private (int total, List<int> additionalList) GetRollDamageResult(int diceCount, int critical, List<int> diceList, int modify)
        {
            if (critical <= 0)
            {
                var sum = diceList.Sum() + modify;
                return (sum, null);
            }

            var restDice = new List<int>(diceList);

            if (critical > diceCount)
            {
                critical = diceCount;
            }

            for (var i = 0; i < critical; i++)
            {
                restDice.RemoveAt(0);
                diceList.RemoveAt(0);
                diceList.Add(7);
            }

            int max;
            if (restDice.Count > 0)
            {
                max = restDice[restDice.Count - 1];
                restDice.RemoveAt(restDice.Count - 1);
            }
            else
            {
                max = 1;
            }

            var total = (int)(max * Math.Pow(7, critical) + restDice.Sum() + modify);
            return (total, diceList);
        }

        private Result? RollName(string command, IRandomizer randomizer)
        {
            if (command != "TRNAME")
            {
                return null;
            }

            // Roll on NAME1 and NAME2 tables (1d100 each)
            var firstRoll = randomizer.RollOnce(100);
            var firstName = NAME1[firstRoll - 1];
            var secondRoll = randomizer.RollOnce(100);
            var secondName = NAME2[secondRoll - 1];

            var text = $"{firstName} , {secondName}";
            return Result.CreateBuilder(text).Build();
        }

        private string GetModifyText(int modify)
        {
            if (modify == 0) return "";
            if (modify > 0) return $"+{modify}";
            return modify.ToString();
        }

        private static readonly string[] NAME1 = new[]
        {
            "春日", "浅見", "風間", "神無月", "倉田", "不動", "山奈", "シャルロック", "霧隠", "果心",
            "今井", "長瀬", "明智", "風祭", "志貫", "一文字", "月夜野", "桜田門", "果瀬", "九十九",
            "速水", "片桐", "葉月", "ウィンザー", "時雨里", "神城", "水際", "一ノ江", "仁藤", "北千住",
            "西村", "諏訪", "藤宮", "御代", "橘", "霧生", "白石", "椎名", "綾小路", "二条",
            "光明寺", "春秋", "雪見", "刀条院", "ランカスター", "ハクア", "エルタニア", "ハーネス", "アウグストゥス", "椎名町",
            "鍵守", "茜ヶ崎", "鎮宮", "美柳", "鎖々塚", "櫻ノ杜", "鏡ヶ守", "輝井", "南陽", "雪乃城",
            "六角屋", "鈴々", "東三条", "朱雀院", "青龍院", "白虎院", "玄武院", "麒麟院", "リーシュタット", "サンクチュアリ",
            "六実", "須藤", "ミレニアム", "七里", "三枝", "八殿", "藤里", "久宝", "東", "赤西",
            "神ヶ崎", "グランシア", "ダークブーレード", "天光寺", "月見里", "璃宮", "藤見澤", "赤聖", "姫宮", "華ノ宮",
            @"""天才""", @"""達人""", @"""賢者""", @"""疾風""", @"""海の""", @"""最強""", @"""凶器""", @"""灼熱""", @"""人間兵器""", @"""魔王"""
        };

        private static readonly string[] NAME2 = new[]
        {
            "アラタ/聖", "アビィス/リリス", "ルーグ/レヴィ", "ラスト/アリン", "ソラ/ユイ",
            "イーリアス/アキオ", "アカーシャ/ミラ", "アリエス/リーゼロッテ", "ムラサメ/シャルム", "龍貴/竜姫",
            "英樹/春菜", "準一/湊", "急司郎/光理", "夕也/愛奈", "晴彦/アキ",
            "疾風/ヤシロ", "カガリ/灯花", "次郎/優都", "春太郎/静理", "ジン/時雨",
            "イオリ/伊織", "ユウヒ/優姫", "サツキ/翠名", "シュライ/サクラ", "ミナヅキ/姫乃",
            "カエデ/優樹菜", "ハル/フユ", "ドール/瑞江", "ニトゥレスト/キリカ", "スカー/綾瀬",
            "真夏/小夏", "光一/ののか", "彩/翠", "トウカ/柊花", "命/ミコト",
            "司/つかさ", "ゆとり/なごみ", "冬彦/観月", "カレン/華恋", "清次郎/亜矢",
            "サード/夢子", "ボックス/詩子", "ヘリオス/カエデ", "ゲート/京香", "オンリー/パトリシア",
            "ザッハーク/アーリ", "ラスタバン/ラスティ", "桜花/燁澄", "計都/リヴィア", "カルヴァリオ/香夜",
            "悠人/夜々子", "太子/羽菜", "夕立/夕凪", "アルフ/愛美", "ファロス/灯利",
            "スプートニク/詩姫", "アーネスト/累", "ナイン/カグヤ", "クリア/ヒマワリ", "ウォーカー/オリビア",
            "ダーク/クオン", "ウェイヴ/凛", "ルーン/マリエ", "エンギ/セイギ", "シラヌイ/ミライ",
            "ブライン/キズナ", "クロウ/カナタ", "スレイヤー/ヒカル", "レス/ミリアリア", "ミフユ/サリエル",
            "鳴央/音央", "モンジ/理亜", "パルデモントゥム/スナオ", "ミシェル/詩穂", "フレンズ/サン",
            "サトリ/識", "ロード/唯花", "クロノス/久宝", "フィラデルフィア/冬海", "ティンダロス/美星",
            "勇弥/ユーリス", "エイト/アンジェラ", "サタン/ルシエル", "エース/小波", "セージ/胡蝶",
            "忍/千之", "重吾/キリコ", "マイケル/ミホシ", "カズマ/鶴香", "ヤマト/エリシエル",
            "歴史上の人物の名前（信長、ジャンヌなど）", "スポーツ選手の名前（ベッカム、沙保里など）",
            "学者の名前（ソクラテス、エレナなど）", "アイドルの名前（タクヤ、聖子など）",
            "土地、国、町の名前（イングランド、ワシントンなど）", "モンスターの名前（ドラゴン、ラミアなど）",
            "武器防具の名前（ソード、メイルなど）", "自然現象の名前（カザンハリケーンなど）",
            "機械の名前（洗濯機、テレビなど）", "目についた物の名前（シャーペン、メガネなど）"
        };
    }
}
