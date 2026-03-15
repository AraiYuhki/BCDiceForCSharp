using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 天才軍師になろう
    /// </summary>
    public sealed class TensaiGunshiNiNaro : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly TensaiGunshiNiNaro Instance = new TensaiGunshiNiNaro();

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        /// <inheritdoc/>
        public override string Id => "TensaiGunshiNiNaro";

        /// <inheritdoc/>
        public override string Name => "天才軍師になろう";

        /// <inheritdoc/>
        public override string SortKey => "てんさいくんしになろう";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定
        TN6…「有利」を得ていない場合、6面ダイスを2つ振って判定します。
        TN10…「有利」を得ている場合、10面ダイスを2つ振って判定します。
        不調 気づかぬうちの不満【C】…このセッションの間、「4」の出目を出しても判定は成功になりません。数字の後ろに【C】をつけます。
        　例）TN6C
        軍師スキル 〇〇サポート【S】…決戦フェイズの判定中「3」の出目を出しても判定に成功します。数字の後ろに【S】をつけます。
        　例）TN6S
        英傑スキル/武人 煌めく刃【B】…決戦フェイズの判定中「3」の出目を出しても判定に成功となり、スペシャルが発生します。数字の後ろに【B】をつけます。
        　例）TN6B
        英傑スキル/武人 力ずく…その判定のサイコロをすべて振った後、[使用者の【攻撃力】]個サイコロを振る。先頭に使用者の【攻撃力】をつけます。
        　例）4TN6
        英傑スキル/武人 必殺の剣【D】…《戦技》を使用している判定中「4」「5」の出目を出してもスペシャルが発生します。数字の後ろに【D】をつけます。
        　例）TN6K
        英傑スキル/武人 二刀流【T】…「攻撃」のスキルの判定中「2」の出目を出しても判定に成功となり、同じ出目のサイコロが2つ以上出ているとスペシャルが発生します。数字の後ろに【T】をつけます。
        　例）TN6T
        英傑スキル/カリスマ 御身のためならば【Y】…「交流」「スカウト」の判定中「3」の出目を出しても判定に成功となり、スペシャルが発生します。数字の後ろに【Y】をつけます。
        　例）TN6Y
        英傑スキル/弓取り 愛用の弓【A】…「攻撃」のスキルの判定中「3」の出目を出しても判定に成功となり、スペシャルが発生します。数字の後ろに【A】をつけます。
        　例）TN6A
        英傑スキル/ヤンキー&マイルドヤンキー その辺の物を武器に【C】…「4」の出目を出しても判定は成功になりません。数字の後ろに【C】をつけます。
        　例）TN6C
        英傑スキル/ヤンキー&マイルドヤンキー 熱血判定【C】…「4」の出目を出しても判定は成功になりません。数字の後ろに【C】をつけます。
        　例）TN6C
        英傑スキル/英傑汎用 凄腕エージェント【A】…活動フェイズの判定中「3」の出目を出しても判定に成功となり、スペシャルが発生します。数字の後ろに【A】をつけます。
        　例）TN6A
        数字の後ろに複数のコマンドを追加できます。
        　例）TN10CYA
        ・ダメージ計算 xDM+y>=t
        　[ダメージ計算]を行う。成否と【HP】の減少量を表示する。
        　x: 6面ダイス数
        　y: 補正値（省略可能）
        　t: 防御力
        ・各種表
        関係決定表 RELA
        平時天才軍師表 PTGS
        平時英傑表 PTHE
        スカウト表 SCOU
        変調表 BDST
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollJudge(command, randomizer) ?? RollDamage(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollJudge(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d*)TN(6|10)([ABCKSTY]*)$");
            if (!m.Success)
            {
                return null;
            }
            var successDices = new List<int> { 4, 5, 6, 7, 8, 9, 10 };
            var specialDices = new List<int> { 6, 10 };
            var fumbleDices = new[] { 1 };
            var advantage = m.Groups[2].Value == "10";
            var complaints = m.Groups[3].Value.Contains("C");
            var support = m.Groups[3].Value.Contains("S");
            var blade = m.Groups[3].Value.Contains("B");
            var killer = m.Groups[3].Value.Contains("K");
            var twin = m.Groups[3].Value.Contains("T");
            var you = m.Groups[3].Value.Contains("Y");
            var agent = m.Groups[3].Value.Contains("A");
            if (twin)
            {
                successDices.Add(2);
            }
            if (support | blade | you | agent)
            {
                successDices.Add(3);
            }
            if (blade | you | agent)
            {
                specialDices.Add(3);
            }
            if (killer)
            {
                specialDices.Add(4);
                specialDices.Add(5);
            }
            if (complaints)
            {
                successDices.Remove(4);
            }
            var prefixStr = m.Groups[1].Value;
            var times = 2 + (prefixStr.Length > 0 ? Convert.ToInt32(prefixStr) : 0);
            var diceSize = advantage ? 10 : 6;
            var diceList = randomizer.RollBarabara(times, diceSize);
            var texts = new List<string>();
            var isCritical = false;
            var isFumble = false;
            var isSuccess = false;

            // スペシャルとなる出目を含む、または、二刀流かつ同じ出目のサイコロが2つ以上ある場合
            var specialIntersect = diceList.Intersect(specialDices).ToList();
            var twinDuplicate = twin && (diceList.Length != diceList.Distinct().Count());
            if (specialIntersect.Count > 0 || twinDuplicate)
            {
                isCritical = true;
                texts.Add(Translate("TensaiGunshiNiNaro.JUDGE.critical"));
                var specialEffects = new List<string>();
                specialEffects.Add(Translate("TensaiGunshiNiNaro.NORMAL.critical"));
                if (blade)
                {
                    specialEffects.Add(Translate("TensaiGunshiNiNaro.BLADE.critical"));
                }
                if (you)
                {
                    specialEffects.Add(Translate("TensaiGunshiNiNaro.YOU.critical"));
                }
                texts.Add($"（{string.Join("", specialEffects)}）");
            }

            // ファンブルとなる出目を含む場合
            var fumbleIntersect = diceList.Intersect(fumbleDices).ToList();
            if (fumbleIntersect.Count > 0)
            {
                isFumble = true;
                texts.Add(Translate("TensaiGunshiNiNaro.JUDGE.fumble"));
                texts.Add($"（{Translate("TensaiGunshiNiNaro.NORMAL.fumble")}）");
            }

            // 成功/失敗
            var successIntersect = diceList.Intersect(successDices).ToList();
            if (successIntersect.Count == 0)
            {
                texts.Add(Translate("failure"));
            }
            else
            {
                isSuccess = true;
                texts.Add(Translate("success"));
            }

            return Result.CreateBuilder($"{command} ＞ [{string.Join(",", diceList)}] ＞ {string.Join("", texts)}")
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollDamage(string command, IRandomizer randomizer)
        {
            // xDM+y>=t or xDM>=t
            var m = Regex.Match(command, @"^(\d+)DM([+-]\d+)?>=(\d+)$");
            if (!m.Success)
            {
                return null;
            }
            var numDice = Convert.ToInt32(m.Groups[1].Value);
            var modifyStr = m.Groups[2].Value;
            var modify = modifyStr.Length > 0 ? Convert.ToInt32(modifyStr) : 0;
            var targetNumber = Convert.ToInt32(m.Groups[3].Value);

            var text = "";
            var isSuccess = false;
            var damage = randomizer.RollSum(numDice, 6) + modify;
            var dec = damage / targetNumber;
            if (dec > 3)
            {
                dec = 3;
            }
            if (dec > 0)
            {
                isSuccess = true;
                text = Translate("TensaiGunshiNiNaro.DAMAGE.success");
            }
            else
            {
                text = Translate("TensaiGunshiNiNaro.DAMAGE.failure");
            }
            return Result.CreateBuilder($"{command} ＞ {damage} ＞ {text}")
                .SetCondition(isSuccess)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
