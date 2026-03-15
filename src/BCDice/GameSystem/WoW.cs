using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ワンダーオブワンダラー
    /// </summary>
    public sealed class WoW : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly WoW Instance = new WoW();

        private static readonly Regex GgAHRegex = new Regex(
            @"^GG([A-H])$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WowRegex = new Regex(
            @"^(\d+)WW12(?:@(\d+))?(?:#(\d+))?(?:<=(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "WoW";

        /// <inheritdoc/>
        public override string Name => "ワンダーオブワンダラー";

        /// <inheritdoc/>
        public override string SortKey => "わんたあおふわんたらあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
行為判定 nWW12@s#f<=x
n: ダイス数
@s = 大成功値（省略可：デフォルトは1）
#f = 大失敗値（省略可：デフォルトは12）
x = 目標値（省略可：デフォルトは6）
例）1WW12 5WW12<=6 6WW12@5#3<=7+1

ランダムギフトガチャ表 GG
ランダムギフトガチャ表（アルファベット指定） GGx 例）GGA GGB

ファンブル表 FT
";

        // テーブルデータ
        private static readonly Dictionary<string, string[]> TABLES = new Dictionary<string, string[]>
        {
            ["A"] = new[] { "演者の声", "言いくるめ", "誤魔化し", "代弁者", "腕利き弁護人", "魔性", "魔術", "魔法的物理", "誤り指摘", "専門知識", "理力増幅", "協力的な有識者" },
            ["B"] = new[] { "百科全書", "地道な下調べ", "思い…出した！", "目星", "ハッキング", "再考察", "迷探偵", "逆転の発想", "炯眼", "安楽椅子探偵", "密室トリック解明", "丁寧な処置" },
            ["C"] = new[] { "慈愛", "クイックヒール", "エリアヒール", "クリアランス", "俯瞰視点", "パターン化", "瞬時看破", "警鐘", "賢者の瞳", "千里眼", "危険感知", "リバーサル" },
            ["D"] = new[] { "転禍為福", "受け身", "九死に一生", "軽業", "バックドア", "着服", "闇に隠れる", "変装", "証拠隠滅", "サポート", "技師の指", "妨害" },
            ["E"] = new[] { "ゴッドハンド", "生存者の切り札", "狙撃", "プラチナ免許", "ドライバーズ・ハイ", "相乗り", "愛車／愛馬", "ビーストフレンズ", "ドゥ・ライブ", "カツアゲ", "マッドドッグ", "目の上の瘤" },
            ["F"] = new[] { "叱咤激励", "ふいに見せた優しさ", "スゴ味", "達人", "必殺技", "二刀流", "急所狙い", "ジャンプショット", "パルクール", "疾風怒濤", "スパート", "走為上" },
            ["G"] = new[] { "ヒット＆アウェイ", "ウーバー", "割れもの注意", "もしもの備え", "アブダクション", "追加機材", "自在配送", "不屈の精神", "防壁", "心頭滅却", "三時間しか寝てない", "βエンドルフィン" },
            ["H"] = new[] { "怒髪天", "頭の体操", "精神統一", "リトルラック", "いいね！", "幻視", "慎重性", "バレットストッパー", "褪せぬ想い", "アピール上手", "土俵際の魔術師", "真実の愛" },
            ["FT"] = new[]
            {
                "何も起きなかった！　ラッキー（？）",
                "ランダムに武器または防具が外れる。該当箇所に何も装備していなければ1点のダメージ（軽減無効）を受ける。",
                "GMの指定したLOVEの【深度】が1増加する。誰かに対するLOVEを新規取得させても良い。",
                "GMの指定したハンドアウト1つの強度が［自身のソウルLV／2］増加する。",
                "1点のダメージ（軽減無効）を受ける。",
                "プレイス内のPCが所持している消耗品からGMが1つ指定し、破壊する。破壊したくない場合、かわりに自身のHPを最大値の1／3（切り捨て）減らす。",
                "不調強度［自身のソウルLV／2］のランダムな不調を受ける。",
                "ファンブル表を2回振る。この効果は判定につき1度までで、以降は1点のダメージ（軽減無効）を受ける。",
                "ランダムなLOVEの【深度】が1減少する。",
                "ランダムなLOVEの【エモ】が2増加する。",
                "トラブルが発生する。ランダムトラブル表を使用し、場にトラブルのハンドアウトを追加する。",
                "ランダムなギフト1つのMPが0になる。"
            }
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (command == "GG")
            {
                return RollGg(randomizer);
            }

            var match = GgAHRegex.Match(command);
            if (match.Success)
            {
                return RollTable(match.Groups[1].Value, randomizer);
            }

            if (command == "FT")
            {
                return RollFumbleTable(randomizer);
            }

            return RollWow(command, randomizer);
        }

        private Result? RollGg(IRandomizer randomizer)
        {
            var diceResults = randomizer.RollBarabara(2, 12);
            var firstRoll = diceResults[0];
            var secondRoll = diceResults[1];

            if (firstRoll >= 9)
            {
                return Result.CreateBuilder("GG ＞ 自由（アルファベットを決めてGGXを振る）").Build();
            }

            var alphabet = ((char)(64 + firstRoll)).ToString();
            var table = TABLES[alphabet];
            return Result.CreateBuilder($"ランダムギフトガチャ {alphabet}-{secondRoll} ＞ {table[secondRoll - 1]}").Build();
        }

        private Result? RollTable(string alphabet, IRandomizer randomizer)
        {
            var table = TABLES[alphabet];
            var diceResult = randomizer.RollOnce(12);
            return Result.CreateBuilder($"ランダムギフトガチャ {alphabet}-{diceResult} ＞ {table[diceResult - 1]}").Build();
        }

        private Result? RollFumbleTable(IRandomizer randomizer)
        {
            var diceResult = randomizer.RollOnce(12);
            var table = TABLES["FT"];
            return Result.CreateBuilder($"FT({diceResult}).Build() ＞ {table[diceResult - 1]}").Build();
        }

        private Result? RollWow(string command, IRandomizer randomizer)
        {
            var m = WowRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var numDice = int.Parse(m.Groups[1].Value);
            var criticalSuccessValue = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
            var criticalFailValue = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 12;
            var successThreshold = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 6;

            string commandWithDefaults;
            if (!m.Groups[4].Success)
            {
                commandWithDefaults = $"{m.Groups[1].Value}WW12<={successThreshold}";
            }
            else
            {
                commandWithDefaults = command;
            }

            // ダイスを振る
            var diceResults = randomizer.RollBarabara(numDice, 12);

            // 出目を分類
            var criticalSuccess = diceResults.Count(r => r <= criticalSuccessValue);
            var criticalFail = diceResults.Count(r => r >= criticalFailValue);
            var normalSuccess = diceResults.Count(r => r > criticalSuccessValue && r <= successThreshold && r < criticalFailValue);

            var criticalSuccessFirst = criticalSuccess;
            var criticalFailFirst = criticalFail;

            // 大成功と大失敗の相殺
            var offset = Math.Min(criticalSuccess, criticalFail);
            criticalSuccess -= offset;
            criticalFail -= offset;

            // 成功数とファンブルの判定
            var successes = normalSuccess + criticalSuccess * 2;
            var isFumble = criticalFail > 0;

            var text = $"({commandWithDefaults}) ＞ [{string.Join(",", diceResults)}] ＞ 成功数{successes}（大成功{criticalSuccessFirst}個、大失敗{criticalFailFirst}個）{(isFumble ? " ＞ ファンブル！" : "")}";

            return Result.CreateBuilder(text)
                .SetCritical(criticalSuccess > 0)
                .SetFumble(isFumble)
                .SetSuccess(successes > 0 && !isFumble)
                .SetFailure(successes == 0 || isFumble)
                .Build();
        }
    }
}
