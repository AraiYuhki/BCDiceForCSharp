using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ビギニングアイドル
    /// </summary>
    public sealed class BeginningIdol : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BeginningIdol Instance = new BeginningIdol();

        /// <inheritdoc/>
        public override string Id => "BeginningIdol";

        /// <inheritdoc/>
        public override string Name => "ビギニングアイドル";

        /// <inheritdoc/>
        public override string SortKey => "ひきにんくあいとる";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        これは、2015年に新書サイズで発売された『駆け出しアイドルRPG ビギニングアイドル チャレンジガールズ』およびそのサプリメントに対応したコマンドです。

        ・パフォーマンス　[r]PDn[+m/-m](r：場に残った出目　n：振る数　m：修正値)
        ・ワールドセッティング仕事表　BWT：大手芸能プロ　LWT：弱小芸能プロ
        　TWT：ライブシアター　CWT：アイドル部　LO[n]：地方アイドル(n：チャンス)
        　SU：情熱の夏　WI：ぬくもりの冬　NA：大自然　GA：女学園　BA：アカデミー
        ・仕事表　WT　VA：バラエティ　MU：音楽関係　DR：ドラマ関係
        　VI：ビジュアル関係　SP：スポーツ　CHR：クリスマス　PAR：パートナー関係
        　SW：お菓子　AN：動物　MOV：映画　FA：ファンタジー
        ・ランダムイベント　RE
        ・ハプニング表　HA
        ・特技リスト　AT[n](n：分野No.)
        ・アイドルスキル修得表　SGT：チャレンジガールズ　RS：ロードトゥプリンス
        ・変調　BT[n](n：発生数)
        ・アイテム　IT[n](n：獲得数)
        ・アクセサリー　ACT：種別決定　ACB：ブランド決定　ACE：効果表
        ・衣装　DT：チャレンジガールズ　RC：ロードトゥプリンス　FC:フォーチュンスターズ
        ・無茶ぶり表　LUR：地方アイドル　SUR：情熱の夏　WUR：ぬくもりの冬
        　NUR：大自然　GUR：女学園　BUR：アカデミー
        ・センタールール　HW：向かい風シーン表　FL：駆け出しシーン表　LN：孤独表
        　マイスキル【MS：名前決定　MSE：効果表】　演出表【ST　FST：ファンタジー】
        ・合宿ルール　散策表【SH：ショッピングモール　MO：山　SEA：海　SPA：温泉街】
        　TN：夜語りシチュエーション表　成長表【CG：コモン　GG：ゴールド】
        ・サビ表　CHO　SCH：情熱の夏　WCH：ぬくもりの冬　NCH：大自然
        　GCH：女性向け　PCH：力強い
        ・キャラ空白表　CBT：チャレンジガールズ　RCB：ロードトゥプリンス
        ・趣味空白表　HBT：チャレンジガールズ　RHB：ロードトゥプリンス
        ・マスコット暴走表　RU
        ・アイドル熱湯風呂　nC：バーストタイム(n：温度)　BU：バースト表
        ・攻撃　n[S]A[r][+m/-m](n：振る数　S：失敗しない　r：取り除く出目　m：修正値)
        ・かんたんパーソン表　SIP
        ・会場表
        　BVT：大手芸能プロ　LVT：弱小芸能プロ　TVT：ライブシアター　CVT：アイドル部
        ・場所表
        　BST：大手芸能プロ　LST：弱小芸能プロ　TST：ライブシアター　CST：アイドル部
        ・プレッシャー種別決定表
        　BPT：大手芸能プロ　LPT：弱小芸能プロ　TPT：ライブシアター　CPT：アイドル部
        ・道具表
        　BIT：大手芸能プロ　LIT：弱小芸能プロ　TIT：ライブシアター　CIT：アイドル部
        []内は省略可　D66入れ替えあり
        ";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollAttack(command, randomizer)
                ?? RollBurst(command, randomizer)
                ?? RollPerformance(command, randomizer);
        }

        private Result? RollBurst(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d{2})C$");
            if (!m.Success)
            {
                return null;
            }

            var degrees = int.Parse(m.Groups[1].Value);
            if (degrees < 45 || degrees > 55)
            {
                return null;
            }

            int counts;
            if (degrees <= 49)
            {
                counts = 3;
            }
            else if (degrees <= 52)
            {
                counts = 4;
            }
            else if (degrees <= 54)
            {
                counts = 5;
            }
            else
            {
                counts = 6;
            }

            var diceList = randomizer.RollBarabara(counts, 6).OrderBy(x => x).ToArray();
            var total = diceList.Sum() + degrees;

            string result;
            if (total >= 80)
            {
                result = "バースト";
            }
            else if (total >= 75)
            {
                result = "クリティカル";
            }
            else if (total >= 65)
            {
                result = "成功";
            }
            else
            {
                result = "失敗";
            }

            var name = "バーストタイム";
            var text = $"{name} ＞ {degrees}+[{string.Join(",", diceList)}] ＞ {total} ＞ {result}";
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollAttack(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)(S?)A([1-6]*)([+-]\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var counts = int.Parse(m.Groups[1].Value);
            if (counts <= 0)
            {
                return null;
            }

            var sure = !string.IsNullOrEmpty(m.Groups[2].Value);
            var removeStr = m.Groups[3].Value;
            var remove = removeStr.Select(c => c - '0').ToList();
            var adjustStr = m.Groups[4].Value;
            var adjust = string.IsNullOrEmpty(adjustStr) ? 0 : int.Parse(adjustStr);
            var adjustModStr = FormatModifier(adjust);

            var dice = randomizer.RollBarabara(counts, 6).OrderBy(x => x).ToList();
            var diceStr = string.Join(",", dice);

            // remove specified values from dice
            var filteredDice = new List<int>(dice);
            foreach (var r in remove)
            {
                filteredDice.Remove(r);
            }

            var text = $"攻撃 ＞ [{diceStr}]{adjustModStr} ＞ ";

            if (!(filteredDice.Count == counts) && filteredDice.Count > 0)
            {
                text += $"[{string.Join(",", filteredDice)}]{adjustModStr} ＞ ";
            }

            if (sure || filteredDice.Count == filteredDice.Distinct().Count())
            {
                var total = Math.Max(filteredDice.Sum() + adjust, 0);
                text += $"{total}ダメージ";
            }
            else
            {
                text += "失敗";
            }

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollPerformance(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([1-7]*)PD(\d+)([+-]\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var counts = int.Parse(m.Groups[2].Value);
            if (counts <= 0)
            {
                return null;
            }

            var carry = m.Groups[1].Value.Select(c => c - '0').OrderBy(x => x).ToList();
            var modifierStr = m.Groups[3].Value;
            var modifier = string.IsNullOrEmpty(modifierStr) ? 0 : int.Parse(modifierStr);

            var diceList = randomizer.RollBarabara(counts, 6).OrderBy(x => x).ToList();
            var allDice = diceList.Concat(carry).OrderBy(x => x).ToList();
            var filtered = SelectUniqs(allDice);

            var title = carry.Count == 0 ? "パフォーマンス" : "シンフォニー";

            string result;
            if (carry.Count == 0)
            {
                result = ResultPerformance(filtered, modifier, allDice);
            }
            else
            {
                result = ResultSymphony(filtered, modifier);
            }

            var sequence = new[]
            {
                title,
                FormatDiceList(diceList, carry, modifier),
                result
            };

            return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private List<int> SelectUniqs(List<int> diceList)
        {
            return diceList.GroupBy(x => x)
                .Where(g => g.Count() == 1)
                .Select(g => g.Key)
                .OrderBy(x => x)
                .ToList();
        }

        private string FormatDiceList(List<int> diceList, List<int> carry, int modifier)
        {
            if (carry.Count == 0)
            {
                return $"[{string.Join(",", diceList)}]{FormatModifier(modifier)}";
            }
            else
            {
                return $"[{string.Join(",", diceList)}],[{string.Join(",", carry)}]{FormatModifier(modifier)}";
            }
        }

        private string ResultPerformance(List<int> list, int modifier, List<int> allList)
        {
            if (list.Count == 0)
            {
                return $"ミラクル！({modifier + 10})";
            }
            else if (list.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            {
                return $"パーフェクトミラクル！({modifier + 30})";
            }
            else if (list.Count != allList.Count)
            {
                return $"[{string.Join(",", list)}]{FormatModifier(modifier)} ＞ {list.Sum() + modifier}";
            }
            else
            {
                return (list.Sum() + modifier).ToString();
            }
        }

        private string ResultSymphony(List<int> list, int modifier)
        {
            if (list.Count == 0)
            {
                return $"ミラクルシンクロ！({modifier + 15})";
            }
            else if (list.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6 }))
            {
                var perfectMiracle = $"パーフェクトミラクル！({modifier + 30})";
                return $"[{string.Join(",", list)}]{FormatModifier(modifier)} ＞ {perfectMiracle}";
            }
            else
            {
                return $"[{string.Join(",", list)}]{FormatModifier(modifier)} ＞ {list.Sum() + modifier}";
            }
        }

        private static string FormatModifier(int value)
        {
            if (value > 0) return $"+{value}";
            if (value < 0) return value.ToString();
            return "";
        }
    }
}
