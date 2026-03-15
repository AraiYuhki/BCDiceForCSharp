using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 碧空のストレイヴ
    /// </summary>
    public sealed class Strave : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Strave Instance = new Strave();


        private static readonly Regex MpDRegex = new Regex(
            @"MP(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DStDRegex = new Regex(
            @"(\d+)ST(\d+)(x|\*)(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "Strave";

        /// <inheritdoc/>
        public override string Name => "碧空のストレイヴ";

        /// <inheritdoc/>
        public override string SortKey => "へきくうのすとれいふ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・モラトリアムフェイズ用判定：MPm
        ・命中判定：nSTm*p

        「n」でダイス数を指定。
        「m」で目標値を指定。省略は出来ません。
        「p」で攻撃力を指定。「*」は「x」でも可。

        【書式例】
        ・MP6 → 目標値6のモラトリアムフェイズ用判定。
        ・5ST6*10 → 5d10で目標値6、攻撃力10の命中判定。

        【各種表】
        ・所属表：AFF　　VN版：AFV
        ・アイデンティティ表：IDT　　VN版：IDV

        ※アイデンティティ表はエラッタ適用済です。
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            int diceCount = 0;
            int target = 0;
            Match match;

            match = MpDRegex.Match(command);
            if (match.Success)
            {
                diceCount = 2;
                target = int.Parse(match.Groups[1].Value);
                return CheckRoll(diceCount, target, null, randomizer);
            }

            match = DStDRegex.Match(command);
            if (match.Success)
            {
                diceCount = int.Parse(match.Groups[1].Value);
                target = int.Parse(match.Groups[2].Value);
                var damage = int.Parse(match.Groups[4].Value);
                return CheckRoll(diceCount, target, damage, randomizer);
            }

            if (command == "AFF")
            {
                return Result.CreateBuilder(GetAffiliationTable(randomizer)).SetSuccess(true).Build();
            }

            if (command == "IDT")
            {
                return Result.CreateBuilder(GetIdentityTable(randomizer)).SetSuccess(true).Build();
            }

            if (command == "AFV")
            {
                return Result.CreateBuilder(GetAffiliationTable2(randomizer)).SetSuccess(true).Build();
            }

            if (command == "IDV")
            {
                return Result.CreateBuilder(GetIdentityTable2(randomizer)).SetSuccess(true).Build();
            }

            return null;
        }

        private Result CheckRoll(int diceCount, int target, int? damage, IRandomizer randomizer)
        {
            if (target < 1)
            {
                target = 1;
            }
            if (target > 10)
            {
                target = 10;
            }
            var diceArray = randomizer.RollBarabara(diceCount, 10).OrderBy(x => x).ToArray();
            var diceText = string.Join(",", diceArray);
            var successCount = diceArray.Where(i => i <= target).Count();
            var isDamage = damage != null;
            string result;
            if (isDamage)
            {
                var totalDamage = successCount * damage.Value;
                result = $"({diceCount}D10<={target}) ＞ {diceText} ＞ Hits：{successCount}*{damage} ＞ {totalDamage}ダメージ";
            }
            else
            {
                result = $"({diceCount}D10<={target}) ＞ {diceText}";
                if (successCount > 0)
                {
                    result += " ＞ 【成功】";
                }
                else
                {
                    result += " ＞ 【失敗】";
                }
            }
            return Result.CreateBuilder(result).SetSuccess(true).Build();
        }

        private string GetAffiliationTable(IRandomizer randomizer)
        {
            var name = "所属表：基本";
            var table = new (int, string)[] { (1, "アリウス管理委員会：あなたはアリウス管理委員会に所属している。"), (2, "オーヴァーブルー：あなたはオーヴァーブルーに所属している。"), (3, "ウォルゲイト：あなたはウォルゲイトに所属している。"), (4, "暁部隊：あなたはかつて、反逆者・暁弥琴と同じ部隊に所属していた。"), (5, "天文部：あなたは天文部に所属している。"), (6, "吹奏楽部：あなたは吹奏楽部に所属している。"), (7, "剣道部：あなたは剣道部に所属している。"), (8, "ボクシング部：あなたはボクシング部に所属している。"), (9, "陸上部：あなたは陸上部に所属している。"), (10, "茶道部：あなたは茶道部に所属している。"), (11, "パソコン部：あなたはパソコン部に所属している。"), (12, "新聞部：あなたは新聞部に所属している。"), (13, "弓道部：あなたは弓道部に所属している。"), (14, "美術部：あなたは美術部に所属している。"), (15, "ミリタリー研究会：あなたはミリタリー研究会に所属している。"), (16, "歴史研究会：あなたは歴史研究会に所属している。"), (17, "ロボット研究会：あなたはロボット研究会に所属している。"), (18, "図書委員会：あなたは図書委員会に所属している。"), (19, "任意：あなたの任意の所属を設定せよ。"), (20, "任意：あなたの任意の所属を設定せよ。") };
            return GetStrave1d100TableResult(name, table, randomizer);
        }

        private string GetIdentityTable(IRandomizer randomizer)
        {
            var name = "アイデンティティ表：基本";
            var table = new (int, string)[] { (1, "戦い：戦いこそが、あなたをあなたたらしめている。"), (2, "守護：あなたには守るべきものがある。"), (3, "復讐：あなたは復讐を誓っている。何かに、あるいは誰かに。"), (4, "名声：その身に浴びる脚光を、何よりも誉としている。"), (5, "恋愛：あなたはその身を焦がす恋に生きている。"), (6, "家族：あなたにとって、家族はかけがえの無いものだ。"), (7, "友人：あなたは友のために戦っている。"), (8, "部隊：共に戦う部隊の仲間が、あなたに力をくれる。"), (9, "ストレイヴ：あなたは自身のストレイヴを誇りに思っている。"), (10, "スフィアブレイク：あなたはスフィアブレイクを熱烈に目指している。"), (11, "お金：あなたはお金を求めている。報酬こそが自分の価値だ。"), (12, "夢：あなたには夢がある。自分を突き動かす夢が。"), (13, "忠誠：あなたは忠誠を誓っている。何かに、あるいは誰かに。"), (14, "共生：あなたは、ヴァイエルと人類との共生を望んでいる。"), (15, "居場所：自身の居場所こそが、あなたに力をくれる。"), (16, "強制：あなたは不本意ながら今の立場にいる。"), (17, "碧空：見上げた青空が、あなたを変えた。"), (18, "任意：あなたの任意のアイデンティティを設定せよ。"), (19, "任意：あなたの任意のアイデンティティを設定せよ。"), (20, "任意：あなたの任意のアイデンティティを設定せよ。") };
            return GetStrave1d100TableResult(name, table, randomizer);
        }

        private string GetAffiliationTable2(IRandomizer randomizer)
        {
            var name = "所属表：ヴァリアンスネイヴァー";
            var table = new (int, string)[] { (1, "シュヴァレ・トワール：あなたはシュヴァレ・トワールに所属している。"), (2, "ディープシンカー：あなたはディープシンカーに所属している。"), (3, "ヴェルクシュタット：あなたはヴェルクシュタットに所属している。"), (4, "アウスヴァル：あなたはアウスヴァルに所属している。"), (5, "美術科：あなたは美術科に所属している。"), (6, "哲学科：あなたは哲学科に所属している。"), (7, "数学科：あなたは数学科に所属している。"), (8, "地理学科：あなたは地理学科に所属している。"), (9, "工学科：あなたは工学科に所属している。"), (10, "体育学科：あなたは体育学科に所属している。"), (11, "農学科：あなたは農学科に所属している。"), (12, "歴史学科：あなたは歴史学科に所属している。"), (13, "医学科：あなたは医学科に所属している。"), (14, "情報学科：あなたは情報学科に所属している。"), (15, "音楽科：あなたは音楽科に所属している。"), (16, "心理学科：あなたは心理学科に所属している。"), (17, "文学科：あなたは文学科に所属している。"), (18, "任意：あなたの任意の所属を設定すること。"), (19, "任意：あなたの任意の所属を設定すること。"), (20, "任意：あなたの任意の所属を設定すること。") };
            return GetStrave1d100TableResult(name, table, randomizer);
        }

        private string GetIdentityTable2(IRandomizer randomizer)
        {
            var name = "アイデンティティ表：ヴァリアンスネイヴァー";
            var table = new (int, string)[] { (1, "戦い：戦いへの衝動が、あなたをあなたたらしめている。"), (2, "守護：守るべきものの存在が、あなたをあなたたらしめている。"), (3, "復讐：復讐の誓いこそが、あなたをあなたたらしめている。"), (4, "名声：与えられた名誉こそが、あなたをあなたたらしめている。"), (5, "恋愛：愛する者への想いが、あなたをあなたたらしめている。"), (6, "家族：かけがえのない家族が、あなたをあなたたらしめている。"), (7, "友人：友の存在が、あなたをあなたたらしめている。"), (8, "部隊：部隊の戦友こそが、あなたをあなたたらしめている。"), (9, "ストレイヴ：ストレイヴの存在が、あなたの心を保っている。"), (10, "宇宙：やがて来る旅立ちの日まで、あなたはあなたであろうとしている。"), (11, "お金：与えられる報酬のため、あなたはあなたであろうとしている。"), (12, "夢：あなたには、己の心に誓った夢がある。"), (13, "忠誠：その心でもって、誓った忠義がある。"), (14, "共生：あなたは、ヴァイエルと人類との共生を望んでいる。"), (15, "居場所：自身の居場所への思いが、あなたをあなたたらしめている。"), (16, "ヴァイエル：あなたと同じでありながら、あなたと異なる存在。彼らへの思いが、あなたをあなたたらしめている。"), (17, "エコール：自身の生きる場所への思いが、あなたをあなたたらしめている。"), (18, "任意：あなたの任意のアイデンティティを設定せよ。"), (19, "任意：あなたの任意のアイデンティティを設定せよ。"), (20, "任意：あなたの任意のアイデンティティを設定せよ。") };
            return GetStrave1d100TableResult(name, table, randomizer);
        }

        private string GetStrave1d100TableResult(string name, (int, string)[] table, IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(100);
            var dice2 = (int)Math.Floor((double)(dice - 1) / 5) + 1;
            var result = GetTableByNumber(dice2, table);
            return GetStraveTableResult(name, dice, result);
        }

        private string GetStraveTableResult(string name, int dice, string result)
        {
            return $"{name}({dice}) ＞ {result}";
        }

        /// <summary>
        /// テーブルから番号に対応する値を取得する
        /// </summary>
        private static string GetTableByNumber(int index, (int Number, string Value)[] table)
        {
            foreach (var (number, value) in table)
            {
                if (number >= index)
                {
                    return value;
                }
            }
            return "1";
        }

    }
}
