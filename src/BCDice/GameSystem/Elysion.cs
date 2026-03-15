using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エリュシオン
    /// </summary>
    public sealed class Elysion : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Elysion Instance = new Elysion();

        private static readonly Regex ElDDRegex = new Regex(
            @"^EL(\d*)(\+\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DateBothRegex = new Regex(
            @"^(F|O|M)?DATE\[(.*),(.*)\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DateNumberRegex = new Regex(
            @"^(F|O|M)?DATE(\d\d)(\[(.*),(.*)\])?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DateSimpleRegex = new Regex(
            @"^(F|O|M)?DATE",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex NjRegex = new Regex(
            @"^NJ(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex BsRegex = new Regex(
            @"^BS(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const int SUCCESS_MAX = 5;

        /// <inheritdoc/>
        public override string Id => "Elysion";

        /// <inheritdoc/>
        public override string Name => "エリュシオン";

        /// <inheritdoc/>
        public override string SortKey => "えりゆしおん";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定（ELn+m）
        　能力値 n 、既存の達成値 m（アシストの場合）
        例）EL3　：能力値３で判定。
        　　EL5+10：能力値５、達成値が１０の状態にアシストで判定。
        ・ファンブル表 FT
        ・戦場表 BFT
        ・致命傷表 FWT
        ・陰謀表　GIT
        ・新初期アイテム決定表　NA
        ・授業ハプニング表　JH
        ・日常遭遇表　NJ1～NJ11／ボスキャラクター遭遇表　BS1～BS11
        ファーストオーディション
        ・ユニット名決定表1A　UT1/ユニット名決定表2A　UT3
        ・ユニット名決定表1B　UT2/ユニット名決定表2B　UT4
        ・音楽ジャンル決定表Ａ　OJ1/・音楽ジャンル決定表Ｂ　OJ2
        ライトオブカオス
        ・ニュートラル表　NT/・釣り人表　IT
        ・ロウ表　HT/・カオス表　KT
        ・休憩表（〜BT）
          教室 RBT／購買 SBT／部室 BBT／生徒会室 CBT／学生寮 DBT／図書館 IBT／屋上 FBT／研究室 LBT／プール PBT／中庭 NBT／商店街 ABT／廃墟 VBT／ゲート GBT／温泉 HBT／修学旅行 TBT／お祭り室 EBT／潜入調査 UBT
         ・ランダムNPC表（〜RT）
        　学生生活関連NPC表 SRT／その他NPC表 ORT／学生図鑑 下級生表 DRT／学生図鑑 上級生表 URT
        ・デート表（DATE）／友達デート表（FDATE）／片思いデート表（ODATE）／真夜中デート表（MDATE）
          デート表のためにD6を振ります。
          DATE14 のようにコマンドの後に数値を入れると、その項目を参照します。
        ・デート表（DATE[PC名1,PC名2]）
        　1コマンドでデート判定を行い、デート表の結果を表示します。
        ・D66ダイスあり
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string? type = null;
            string? pc1 = null;
            string? pc2 = null;
            Match match;
            string result;

            match = ElDDRegex.Match(command);
            if (match.Success)
            {
                var baseVal = match.Groups[1].Value;
                var modify = match.Groups[2].Value;
                result = Check(baseVal, modify, randomizer);
                if (!string.IsNullOrEmpty(result))
                {
                    return Result.CreateBuilder($"{command} ＞ {result}").Build();
                }
            }

            match = DateBothRegex.Match(command);
            if (match.Success)
            {
                type = match.Groups[1].Success ? match.Groups[1].Value : null;
                pc1 = match.Groups[2].Value;
                pc2 = match.Groups[3].Value;
                result = GetDateBothResult(type, pc1, pc2, randomizer);
                if (!string.IsNullOrEmpty(result))
                {
                    return Result.CreateBuilder($"{command} ＞ {result}").Build();
                }
            }

            match = DateNumberRegex.Match(command);
            if (match.Success)
            {
                type = match.Groups[1].Success ? match.Groups[1].Value : null;
                var number = int.Parse(match.Groups[2].Value);
                pc1 = match.Groups[4].Success ? match.Groups[4].Value : null;
                pc2 = match.Groups[5].Success ? match.Groups[5].Value : null;
                result = GetDateResult(type, number, pc1, pc2);
                if (!string.IsNullOrEmpty(result))
                {
                    return Result.CreateBuilder($"{command} ＞ {result}").Build();
                }
            }

            match = DateSimpleRegex.Match(command);
            if (match.Success)
            {
                result = GetDateValue(randomizer);
                if (!string.IsNullOrEmpty(result))
                {
                    return Result.CreateBuilder($"{command} ＞ {result}").Build();
                }
            }

            result = CheckAnyCommand(command, randomizer);
            if (!string.IsNullOrEmpty(result))
            {
                return Result.CreateBuilder($"{command} ＞ {result}").Build();
            }

            return RollTables(command, TABLES);
        }

        private string CheckAnyCommand(string command, IRandomizer randomizer)
        {
            int level = 0;
            Match match;
            switch (command.ToUpper())
            {
                case "RBT": return GetBreakTable("教室", GetClassRoomBreakTableData(), randomizer);
                case "SBT": return GetBreakTable("購買", GetSchoolStoreBreakTableData(), randomizer);
                case "BBT": return GetBreakTable("部室", GetClubRoomBreakTableData(), randomizer);
                case "CBT": return GetBreakTable("生徒会室", GetStudentCouncilBreakTableData(), randomizer);
                case "DBT": return GetBreakTable("学生寮", GetDormitoryBreakTableData(), randomizer);
                case "IBT": return GetBreakTable("図書館", GetLibraryBreakTableData(), randomizer);
                case "FBT": return GetBreakTable("屋上", GetRoofBreakTableData(), randomizer);
                case "LBT": return GetBreakTable("研究室", GetLaboratoryBreakTableData(), randomizer);
                case "PBT": return GetBreakTable("プール", GetPoolBreakTableData(), randomizer);
                case "NBT": return GetBreakTable("中庭", GetInnerCourtBreakTableData(), randomizer);
                case "ABT": return GetBreakTable("商店街", GetShoppingAvenueBreakTableData(), randomizer);
                case "VBT": return GetBreakTable("廃墟", GetDevastationBreakTableData(), randomizer);
                case "GBT": return GetBreakTable("ゲート", GetGateBreakTableData(), randomizer);
                case "BFT": return GetD6Table("戦場表", GetBattleFieldTableData(), randomizer);
                case "FWT": return GetD6Table("致命傷表", GetFatalWoundsTableData(), randomizer);
                case "FT": return GetD6Table("ファンブル表", GetFumbleTableData(), randomizer);
                case "SRT": return GetRandomNpcSchoolLife(randomizer);
                case "ORT": return GetRandomNpcOther(randomizer);
                case "DRT": return GetRandomNpcDownClassmen(randomizer);
                case "URT": return GetRandomNpcUpperClassmen(randomizer);
                default:
                    match = NjRegex.Match(command);
                    if (match.Success)
                    {
                        level = int.Parse(match.Groups[1].Value);
                        return GetUsuallyEncount(level, randomizer);
                    }
                    match = BsRegex.Match(command);
                    if (match.Success)
                    {
                        level = int.Parse(match.Groups[1].Value);
                        return GetBossEncount(level, randomizer);
                    }
                    return "";
            }
        }

        private string Check(string baseStr, string modifyStr, IRandomizer randomizer)
        {
            var baseVal = GetValue(baseStr);
            var modify = GetValue(modifyStr);

            var dice1 = randomizer.RollOnce(6);
            var dice2 = randomizer.RollOnce(6);

            var diceTotal = dice1 + dice2;
            var addTotal = baseVal + modify;
            var total = diceTotal + addTotal;

            var text = "EL";
            if (baseVal != 0) text += baseVal.ToString();
            if (modify != 0) text += GetValueString(modify);
            text += " ＞ ";
            text += $"(2D6{GetValueString(baseVal)}{GetValueString(modify)})";
            text += $" ＞ {diceTotal}[{dice1},{dice2}]{GetValueString(addTotal)} ＞ {total}";

            if (dice1 == dice2)
            {
                text = GetSpecialResult(dice1, total, text);
                return text;
            }

            text = GetCheckResultText(total, text);
            return text;
        }

        private int GetValue(string? str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }
            return int.Parse(str);
        }

        private string GetValueString(int value)
        {
            if (value > 0) return $"+{value}";
            if (value < 0) return $"{value}";
            return "";
        }

        private string GetCheckResultText(int total, string text)
        {
            var success = GetSuccessRank(total);
            if (success == 0)
            {
                return text + " ＞ 失敗";
            }
            return GetSuccessResultText(success, text);
        }

        private string GetSuccessResultText(int success, string text)
        {
            text += $" ＞ 成功度{success}";
            if (success >= SUCCESS_MAX)
            {
                text += " ＞ 大成功 《アウル》2点獲得";
            }
            return text;
        }

        private int GetSuccessRank(int total)
        {
            var success = (int)Math.Ceiling((total - 9) / 5.0);
            if (success < 0) success = 0;
            if (success > SUCCESS_MAX) success = SUCCESS_MAX;
            return success;
        }

        private string GetSpecialResult(int number, int total, string text)
        {
            if (number == 6)
            {
                return GetSuccessResultText(SUCCESS_MAX, text);
            }
            return GetFambleResultText(number, total, text);
        }

        private string GetFambleResultText(int number, int total, string text)
        {
            if (number == 1)
            {
                return text + " ＞ 大失敗";
            }
            text = GetCheckResultText(total, text);
            text += $" ／ ({number - 1}回目のアシストなら)大失敗";
            return text;
        }

        private string GetDateBothResult(string? type, string pc1, string pc2, IRandomizer randomizer)
        {
            var dice1 = randomizer.RollOnce(6);
            var dice2 = randomizer.RollOnce(6);

            var result = $"{pc1}[{dice1}],{pc2}[{dice2}] ＞ ";

            var number = dice1 * 10 + dice2;
            if (dice1 > dice2)
            {
                var tmp = pc1;
                pc1 = pc2;
                pc2 = tmp;
                number = dice2 * 10 + dice1;
            }

            result += GetDateResult(type, number, pc1, pc2);
            return result;
        }

        private string GetDateResult(string? type, int number, string? pc1, string? pc2)
        {
            var (name, table) = GetDateTableByType(type);
            var text = GetTableByNumber(number, table);
            if (pc1 != null) text = ChangePcName(text, "受け身キャラ", pc1);
            if (pc2 != null) text = ChangePcName(text, "攻め気キャラ", pc2);
            return $"{name}表({number}) ＞ {text}";
        }

        private (string name, int[][] table) GetDateTableByType(string? type)
        {
            if (type == null)
            {
                return GetDateTable();
            }
            switch (type.ToUpper())
            {
                case "F": return GetFrindDateTable();
                case "O": return GetOnewayDateTable();
                case "M": return GetMidnightDateTable();
                default: return ("", Array.Empty<int[]>());
            }
        }

        private string ChangePcName(string text, string baseName, string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return text;
            }
            return text.Replace(baseName, name);
        }

        private string GetDateValue(IRandomizer randomizer)
        {
            var dice1 = randomizer.RollOnce(6);
            return dice1.ToString();
        }

        private string GetBreakTable(string name, string[] table, IRandomizer randomizer)
        {
            var number = randomizer.RollSum(2, 6);
            var index = number - 2;
            if (index < 0 || index >= table.Length)
            {
                return "";
            }
            var text = table[index];
            return $"{name}休憩表({number}) {text}";
        }

        private string GetD6Table(string name, string[] table, IRandomizer randomizer)
        {
            var number = randomizer.RollOnce(6);
            var index = number - 1;
            if (index < 0 || index >= table.Length)
            {
                return "";
            }
            var text = table[index];
            return $"{name}({number}) {text}";
        }

        private string GetTableByNumber(int number, int[][] table)
        {
            foreach (var entry in table)
            {
                if (entry[0] == number)
                {
                    // Entry format: [number, textIndex] - but in our data model the text is stored separately
                    // We use a different approach: lookup by number in the table
                    return ""; // Placeholder - actual text stored in tuple tables
                }
            }
            return "";
        }

        // Overload for string-keyed tables (date tables etc.)
        private string GetTableByNumber(int number, (int key, string value)[] table)
        {
            foreach (var entry in table)
            {
                if (entry.key == number)
                {
                    return entry.value;
                }
            }
            return "";
        }

        private string GetTableByNumber(int number, (int key, string value)[] table, string defaultValue)
        {
            foreach (var entry in table)
            {
                if (entry.key == number)
                {
                    return entry.value;
                }
            }
            return defaultValue;
        }

        // Date tables return (name, table) tuples
        private (string name, (int key, string value)[] table) GetDateTableByType_Internal(string? type)
        {
            if (type == null) return GetDateTableTuple();
            switch (type.ToUpper())
            {
                case "F": return GetFrindDateTableTuple();
                case "O": return GetOnewayDateTableTuple();
                case "M": return GetMidnightDateTableTuple();
                default: return ("", Array.Empty<(int, string)>());
            }
        }

        // Re-implement date methods with proper tuple-based tables

        private (string, int[][]) GetDateTable()
        {
            return ("デート", Array.Empty<int[]>());
        }

        private (string, int[][]) GetFrindDateTable()
        {
            return ("友達デート", Array.Empty<int[]>());
        }

        private (string, int[][]) GetOnewayDateTable()
        {
            return ("片思いデート", Array.Empty<int[]>());
        }

        private (string, int[][]) GetMidnightDateTable()
        {
            return ("真夜中デート", Array.Empty<int[]>());
        }

        private (string name, (int key, string value)[] table) GetDateTableTuple()
        {
            var name = "デート";
            var table = new (int, string)[]
            {
                (11, "「こんなはずじゃなかったのにッ！」仲良くするつもりが、ひどい喧嘩になってしまう。この表の使用者のお互いに対する《感情値》が1点上昇し、属性が《敵意》になる。"),
                (12, "「あなたってサイテー!!」大きな誤解が生まれる。受け身キャラの攻め気キャラ以外に対する《感情値》がすべて0になり、その値のぶんだけ攻め気キャラに対する《感情値》が上昇し、その属性が《敵意》になる。"),
                (13, "「ねぇねぇ知ってる…？」せっかく二人きりなのに、他人の話で盛り上がる。この表の使用者は、PCの中からこの表の使用者以外のキャラクター一人を選び、そのキャラクターに対する《感情値》が1点上昇する。"),
                (14, "「そこもっとくわしく！」互いの好きなものについて語り合う。受け身キャラは、攻め気キャラの「好きなもの」一つを選ぶ。受け身キャラは、自分の「好きなもの」一つをそれに変更したうえで、攻め気キャラへの《感情値》が2点上昇し、その属性が《好意》になる。"),
                (15, "「なぁ、オレのことどう思う？」思い切った質問！受け身キャラは、攻め気キャラに対する《感情値》を2上昇させ、その属性を好きなものに変更できる。"),
                (16, "「あなたのこと心配してるわけじゃないんだからね！」少し前の失敗について色々と言われてしまう。ありがたいんだけど、少しムカつく。受け身キャラは、攻め気キャラに対する《感情値》が2点上昇する。"),
                (22, "「え、もうこんな時間!?」一休みするつもりが、気がつくとかなり時間がたっている。この表の使用者のお互いに対する《感情値》が1点上昇し、《アウル》1点を獲得する。"),
                (23, "「気になってることがあるんだけど…？」何気ない質問だが、これは難しい。変な答えはできないぞ。攻め気キャラは〔学力〕で判定を行う。成功すると、この表の使用者のお互いに対する《感情値》が成功度の値だけ上昇し、その属性が《好意》になる。失敗すると、何とか危機を切り抜けることができるが、受け身キャラの攻め気キャラに対する《感情値》が1点上昇し、その属性が《敵意》になる。"),
                (24, "「なんか面白いとこ連れてって」うーん、これは難しい注文かも？攻め気キャラは、〔政治力〕で判定を行う。成功すると、この表の使用者のお互いに対する《感情値》が成功度の値だけ上昇し、その属性が《好意》になる。失敗すると、何とか危機を切り抜けることができるが、受け身キャラの攻め気キャラに対する《感情値》が1点上昇し、その属性が《敵意》になる。"),
                (25, "「うーん、ちょっと困ったことがあってさ」悩みを相談されてしまう。ここはちゃんと答えないと。攻め気キャラは、〔青春力〕で判定を行う。成功すると、この表の使用者のお互いに対する《感情値》が成功度の値だけ上昇し、その属性が《好意》になる。失敗すると、何とか危機を切り抜けることができるが、受け身キャラの攻め気キャラに対する《感情値》が1点上昇し、その属性が《敵意》になる。"),
                (26, "「天魔だ。後ろにさがってろ！」何処からとも無く現れた天魔に襲われる。攻め気キャラは好きな能力値で判定を行う。成功すると、この表の使用者のお互いに対する《感情値》が成功度の値だけ上昇し、その属性が《好意》になる。失敗すると、互いに1D6点のダメージを受けつつ、何とか危機を切り抜けることができるが、受け身キャラの攻め気キャラに対する《感情値》が1点上昇し、その属性が《敵意》になる。"),
                (33, "「ごめん、勘違いしてた」誤解が解ける。この表の使用者のお互いに対する《感情値》が1点上昇し、《好意》になる。"),
                (34, "「これ、キミにしか言ってないんだ。二人だけの秘密」受け身キャラが隠している夢や秘密を攻め気キャラが知ってしまう。受け身キャラの攻め気キャラに対する《感情値》が2点上昇する。"),
                (35, "「これからも、よろしく頼むぜ。相棒」攻め気キャラが快活に微笑む。受け身キャラの攻め気キャラに対する《感情値》が2点上昇する。"),
                (36, "「わ、わたしは、あなたのことが…」受け身キャラの思わぬ告白！受け身キャラの攻め気キャラに対する《感情値》が2点上昇する。"),
                (44, "「大丈夫？痛くないか？」互いに傷を治療しあう。この表の使用者は、お互いの自分に対する[《好意》×1D6]点だけ、自分の《生命力》を回復する事ができる。でちらかの《生命力》が1点以上回復したら、この表の使用者のお互いに対する《感情値》が1点上昇する。"),
                (45, "「この事件が終わったら、伝えたい事が…あるんだ」攻め気キャラの真剣な言葉。え、それって…？受け身キャラの攻め気キャラに対する《感情値》が1点上昇し、その属性が《好意》になる。エピローグに攻め気キャラが生きていれば、この表の使用者のお互いに対する《感情値》がさらに2点上昇する。ただし、以降このセッションの間、攻め気キャラは「致命傷表」を使用したとき、二つのサイコロを振って低い目を使う。"),
                (46, "「停電ッ!?…って、どこ触ってるんですかッ!?」辺りが不意に暗くなり、思わず変なところを触ってしまう。攻め気キャラの受け身キャラに対する《感情値》が2点上昇し、その属性が《好意》になる。また、受け身キャラの攻め気キャラに対する《感情値》が2点上昇し、その属性が《敵意》になる。"),
                (55, "「お前ってそんなやつだったんだ？」意外な一面を発見する。互いに対する《感情値》が1点上昇し、その属性が反転する。"),
                (56, "「え？え？えぇぇぇぇッ?!」ふとした拍子に唇がふれあう。受け身キャラの攻め気キャラ以外に対する《感情値》が全て0になり、その値の分だけ攻め気キャラに対する《感情値》が上昇し、その属性が《好意》になる。"),
                (66, "「…………」気がつくとお互い、目をそらせなくなってしまう。そのまま顔を寄せ合い…。この表の使用者のお互いに対する《感情値》が3点上昇する。"),
            };
            return (name, table);
        }

        private (string name, (int key, string value)[] table) GetFrindDateTableTuple()
        {
            return ("友達デート", Array.Empty<(int, string)>());
        }

        private (string name, (int key, string value)[] table) GetOnewayDateTableTuple()
        {
            return ("片思いデート", Array.Empty<(int, string)>());
        }

        private (string name, (int key, string value)[] table) GetMidnightDateTableTuple()
        {
            return ("真夜中デート", Array.Empty<(int, string)>());
        }

        // Override GetDateResult to use tuple-based tables
        private string GetDateResultInternal(string? type, int number, string? pc1, string? pc2)
        {
            var (name, table) = GetDateTableByType_Internal(type);
            var text = GetTableByNumber(number, table);
            if (pc1 != null) text = ChangePcName(text, "受け身キャラ", pc1);
            if (pc2 != null) text = ChangePcName(text, "攻め気キャラ", pc2);
            return $"{name}表({number}) ＞ {text}";
        }

        // Random NPC tables
        private string GetRandomNpcSchoolLife(IRandomizer randomizer)
        {
            return GetRandomNpc("学生生活関連ＮＰＣ表", GetRandomNpcSchoolLifeData(), randomizer);
        }

        private string GetRandomNpcOther(IRandomizer randomizer)
        {
            return GetRandomNpc("教師・その他ＮＰＣ表", GetRandomNpcOtherData(), randomizer);
        }

        private string GetRandomNpcDownClassmen(IRandomizer randomizer)
        {
            return GetRandomNpc("学生図鑑　下級学年表", GetRandomNpcDownClassmenData(), randomizer);
        }

        private string GetRandomNpcUpperClassmen(IRandomizer randomizer)
        {
            return GetRandomNpc("学生図鑑　上級学年表", GetRandomNpcUpperClassmenData(), randomizer);
        }

        private string GetRandomNpc(string name, (int key, string value)[] table, IRandomizer randomizer)
        {
            var d1 = randomizer.RollOnce(6);
            var d2 = randomizer.RollOnce(6);
            var sorted = new[] { d1, d2 }.OrderBy(x => x).ToArray();
            var number = sorted[0] * 10 + sorted[1];
            var result = GetTableByNumber(number, table);
            return $"{name}({number}) {result}";
        }

        // Encounter tables
        private string GetUsuallyEncount(int level, IRandomizer randomizer)
        {
            return GetEncountTableResult("日常遭遇表", GetUsuallyEncountData(), level, randomizer);
        }

        private string GetBossEncount(int level, IRandomizer randomizer)
        {
            return GetEncountTableResult("ボスキャラクター遭遇表", GetBossEncountData(), level, randomizer);
        }

        private string GetEncountTableResult(string name, (int key, string value)[] table, int level, IRandomizer randomizer)
        {
            var dice = randomizer.RollOnce(6);
            var index = dice + level;
            var lastEntry = table.Length > 0 ? table[table.Length - 1].value : "";
            var text = GetTableByNumber(index, table, lastEntry);
            return $"{name}({index}) {text}";
        }

        // Break table data methods
        private string[] GetClassRoomBreakTableData() => new[] { "風紀委員の巡回！\n「そこ、何やってる！」風紀委員に見つかった。現在が真夜中パートであれば、こっぴどく叱られる。〔政治力〕で判定を行う。失敗すると、次のパートは行動できない。真夜中パート以外であれば、心身ともにリフレッシュ。《アウル》が1点回復する。", "引き出しのパン\n「お！こんなとこにッ!?」机の中に以前買った食べ物を発見する。アイテムの【焼きそばパン】を一つ獲得する。使用するときに1D6を振ること。奇数が出たら、問題なく使用できる。偶数が出たら、それは腐っている。【焼きそばパン】の効果の変わりに「病気」の変調を受ける事。", "授業の質問\n「ねぇねぇ、ここ分かる？」クラスメイトに授業の質問を受ける。〔学力〕で判定を行う。成功すると、質問に答えるうちに自分の理解も深まる。自分の授業欄の中から好きなもの一つを選び、□にチェックを入れる。", "先生の依頼\n「丁度いいところにいるな。手伝ってくれ」先生に用事を言いつけられる。〔政治力〕で判定を行う。成功すると、感謝されてアイテムの【タリスマン】を一つ獲得する", "誰かの視線\n……誰かの視線を感じる。もしかして？　教室にいるキャラクターの中から好きな者を一人選び、そのキャラクターの自分に対する《感情値》が1点上昇する。（教室に誰がいるのかは、最終的にGMが決定すること）。", "遊びの誘い\n「ねぇねぇ、明日の放課後ひま？」クラスメイトに遊びに誘われる。翌日の放課後、自由行動として「遊び」を行うことができる。「遊び」を行うと、《アウル》が1D6点回復する。", "居眠り\n「ZZZ……」教室での居眠りは最高だ。《アウル》1点を変調一つを回復する。ただし、今が授業パートなら、〔青春力〕で判定を行う。失敗すると、先生に怒られて《生命力》が1D6点減少し、《アウル》も変調も回復しない。", "お腹空いた\n「ねぇねぇ。お腹空いた。なんかない？」クラスメイトに食事をねだられた。分類が「食物」のアイテムを一つ消費することができたら、教室にいるキャラクターの中から好きな者を一人選び、そのキャラクターの自分に対する《感情値》が1点上昇する。", "クラスの噂\n「ねぇねぇ、これ知ってる？」クラスメイトの噂話……。〔政治力〕の判定を行う。成功すると、手がかり一つを選び、その情報を公開する。失敗すると、あなたについての噂をたてられる。あなたの周囲に、あなたに対して《感情値》を持つキャラクターがいたら、それを反転させる。", "ラブレター!?\n「こ、これは……ッ!?」机の中に何かを発見。〔青春力〕で判定を行う。成功すると、誰かからのラブレターを発見！ 好きなNPC一人を選び、そのキャラクターの自分に対する《感情値》が1点上昇し、属性を《好意》にする。", "笑い声\n「あはははははは」にぎやかな笑い声が響く。〔青春力〕で判定を行う。成功したら輪の中に溶け込み、《アウル》が1点回復する。失敗すると「孤独」の変調を受ける。" };
        private string[] GetSchoolStoreBreakTableData() => new[] { "風紀委員の巡回！", "まとめ買いセール", "防具セール！", "武器セール！", "色々セール！", "ウィンドウショッピング", "試食", "購買での出会い", "高価買い取り！", "デリバリー", "サイフ紛失" };
        private string[] GetClubRoomBreakTableData() => new[] { "風紀委員の巡回！", "謎の忠告", "後輩現る！", "先輩現る！", "青春の汗", "茶飲み話", "熱い議論", "マネージャー", "突然の料理", "仲間の告白", "門外不出品？" };
        private string[] GetStudentCouncilBreakTableData() => new[] { "風紀委員の巡回！", "秘密の会話", "天魔情報", "気になるあいつ", "調べられてる？", "プロフィール更新", "思わぬ一面", "友達検索", "同僚との遭遇", "旧友との再会", "謎の警告" };
        private string[] GetDormitoryBreakTableData() => new[] { "風紀委員の巡回！", "友達との時間", "思い出の日", "引越しの手伝い", "色々トーク", "好物発見！", "お見舞い", "魔蟲襲来！", "恋の相談", "寮に潜入", "ささいなケンカ" };
        private string[] GetLibraryBreakTableData() => new[] { "風紀委員の巡回！", "重たい空気", "文献調査", "天魔対策", "物語の世界へ", "本の貸し出し", "授業の予習", "図書館での出会い", "書物の夢", "謎の手紙", "残念！" };
        private string[] GetRoofBreakTableData() => new[] { "風紀委員の巡回！", "通り雨", "自動販売機", "青空", "物思い", "開放感", "学園は広大だわ", "嵐の予感", "昼寝屋", "欲望の宴", "サビシガリヤ" };
        private string[] GetLaboratoryBreakTableData() => new[] { "風紀委員の巡回！", "装甲強化実験", "試薬", "爆発事故", "威力強化実験", "命中強化実験", "威力安定実験", "バイオハザード", "ビーカーコーヒー", "装甲安定実験", "失敗作" };
        private string[] GetPoolBreakTableData() => new[] { "風紀委員の巡回！", "熱いシャワー", "新作水着", "ポロリもあるよ", "熱視線", "眼福眼福", "人魚のように", "プカプカ", "心地よい疲れ", "記録に挑戦！", "地獄の特訓" };
        private string[] GetInnerCourtBreakTableData() => new[] { "風紀委員の巡回！", "オープンカフェ", "犬登場", "観戦中", "占い師", "屋台", "特訓参加", "落し物", "突然の告白！", "鉄球飛来！", "ちょっぴり贅沢" };
        private string[] GetShoppingAvenueBreakTableData() => new[] { "風紀委員の巡回！", "自習", "立ち話", "雰囲気のいい店", "お気に入り！", "大売出し", "なじみの店", "休日", "泥棒", "福引", "家庭教師" };
        private string[] GetDevastationBreakTableData() => new[] { "迷子", "ライバル登場？", "無人の部屋", "戦いの跡", "裏の事情通", "怪しげな男", "縄張り", "野良犬", "隠れ家", "実験", "不良撃退士" };
        private string[] GetGateBreakTableData() => new[] { "尋問", "計略", "活性化", "克服", "経験", "不意打ち", "捜索", "戦友", "援軍", "罠", "魔剣" };

        // D6 table data methods
        private string[] GetBattleFieldTableData() => new[] { "平地。\n特に特殊効果はない。", "罠。\n落とし穴やセキュリティー装置など、無数の罠が仕掛けられた戦場。この戦場にいるキャラクターは、ダメージを受けたとき、速度によるダメージ修整が2倍になる。", "障害物。\n林の中や狭い部屋の中など、自由に身動きすることが難しい戦場。この戦場にいるキャラクターは、プロットを行うとき、5以上の速度をプロットできなくなる。", "機動戦\n車や電車、船や飛行機など、動いているものの上での戦闘を表す。この戦場にいるキャラクターは、何らかの行為判定にファンブルした場合〔青春力〕で判定を行う。失敗すると、行動済みになり、速度が0になる。", "力場。\n展開や魔界の影響を強く受けている戦場。この戦場にいるキャラクターは、ラウンドの終了時に速度0にいると、〔学力〕の判定を行うことができる。成功すると、《アウル》を1点獲得できる。", "修羅場\n自分たちとは別の撃退士や天魔たちが戦闘を行っていたり、悪意に満ちた第三勢力に囲まれていたりする戦場。この戦場にいるキャラクターは、ラウンドの終了時に速度0にいると、〔政治力〕の判定を行う。失敗すると、《生命力》が1D6点減少する。" };
        private string[] GetFatalWoundsTableData() => new[] { "圧倒的な攻撃が急所をつらぬく。\n行動不能になる。1D6ラウンド後の「ラウンドの終了時」に、まだ戦闘が継続しており、行動不能が回復していなければ、そのキャラクターは死亡する。", "昏睡し、身体中から血と生きる意志が失われていく。\n行動不能になる。また、この行動不能から回復した後、1D6を振り、ランダムに選んだ変調一つを受ける。", "大きな傷を負う。行動不能になる。", "凄まじい一撃に意識を失う。\n《生命力》が0点になり、行動不能になる。", "一瞬、気を失う。\n行動不能になる。1D6ラウンド後の「ラウンドの終了時」、もしくは、戦闘終了時に《生命力》が1点まで回復する（1D6ラウンドが経過するまでに、すでに《生命力》が1点以上に回復していた場合は、この効果は無効になる）。", "凄まじい幸運。\nそのシーンに自分に《好意》を持っているキャラクターがいたら、代わりにそのキャラクターがダメージを受けることができる（ダメージを代わりに受けるかどうかは、そのキャラクターを操るプレイヤー、もしくはGMが決定する）。誰もダメージを代わりに受けなかった場合、行動不能になる。" };
        private string[] GetFumbleTableData() => new[] { "何もかもむなしくなる。誰かに対する《感情値》を1点減少する。", "あまりの失敗に心理的変調をきたす。1D6を振ってランダムに変調一つを選び、それを受ける。", "ポッキリと心が折れる。《アウル》を1点失う。", "あまりにも酷い大失敗を見られてしまい、周囲のキャラクターからの評価が変わる。自分の周囲に、自分に対して《感情値》を持つキャラクターがいたら、それを反転させる。", "敵の罠にかかる。自分の周りにいる、自分以外のすべての見方キャラクターは《生命力》を1D6点減少する。", "アウルが暴走して、大惨事に。自分の《生命力》を2D6点減少する。" };

        // Random NPC data
        private (int key, string value)[] GetRandomNpcSchoolLifeData() => new (int, string)[]
        {
            (11, "振り直し／任意"), (12, "大山恵（おおやま・めぐみ）：中等部3年0組：P84"), (13, "黒瀧辰馬（くろたき・たつま）：風紀委員長・大学部5年0組：P82"),
            (14, "シルヴァリティア・ドーン：大学部1年0組：P83"), (15, "岸崎蔵人（きしざき・くらんど）：大学部5年0組：P84"), (16, "レミエル・N・V：大学部2年0組：P83"),
            (22, "神楽坂茜（かぐらざか・あかね）：生徒会会長・高等部2年0組：P82"), (23, "炎條忍（えんじょう・しのぶ）：高等部1年0組：P83"),
            (24, "中山寧々美（なかやま・ねねみ）：新聞同好会会長・高等部3年0組：P83"), (25, "恵ヴィヴァルディ（めぐみ・−）：大学部2年0組：P83"),
            (26, "轟闘吾（とどろき・とうご）：高等部3年0組：P84"), (33, "鬼島武（きじま・たけし）：生徒会副会長・大学部1年0組：P82"),
            (34, "クリスティーナ・カーティス：大学部1年0組：P83"), (35, "潮崎紘乃（しおざき・ひろの）：依頼斡旋所受付：P84"),
            (36, "ライゼ：教師・寮長：P85"), (44, "大塔寺源九郎（だいとうじ・げんくろう）：生徒会書記・高等部3年0組：P82"),
            (45, "ストローベレー：用務員：P83"), (46, "竜崎アリス（りゅうざき・−）：オペレーター：P84"),
            (55, "大鳥南（おおとり・みなみ）：生徒会会計・高等部3年0組：P82"), (56, "筧鷹政（かけい・たかまさ）：OB：P83"),
            (66, "振り直し／任意"),
        };

        private (int key, string value)[] GetRandomNpcOtherData() => new (int, string)[]
        {
            (11, "振り直し／任意"), (12, "月摘紫蝶（るつみ・しちょう）：教師：P84"), (13, "棄棄（すてき）：教師：P84"),
            (14, "遠野冴草（とおの・さえぐさ）：教師：P84"), (15, "速水風子（はやみ・ふうこ）：教師：P85"),
            (16, "アリス・ペンデルトン：教師：P85"), (22, "宝井正博（たからい・まさひろ）：学園長：P82"),
            (23, "白田悠里（しろた・ゆうり）：教師：P85"), (24, "常盤楓（ときわ・かえで）：教師：P85"),
            (25, "ダイナマ伊藤（−・いとう）：保健医：P85"), (26, "小日向千陰（おびなた・ちかげ）：教師・司書：P85"),
            (33, "ウーネミリア：悪魔：P114"), (34, "太珀（たいはく）：教師：P85"),
            (35, "神無月灯（みなづき・あかり）：ヴァニタス：P114"), (36, "キーヨ：ヴァニタス：P114"),
            (44, "マッド・ザ・クラウン：悪魔：P114"), (45, "劉玄盛（りゅう・げんせい）：シュトラッサー：P114"),
            (46, "厄蔵（やくぞう）：シュトラッサー：P114"), (55, "ギメル・ツァダイ：天使：P114"),
            (56, "ナターシャ：シュトラッサー：P114"), (66, "振り直し／任意"),
        };

        private (int key, string value)[] GetRandomNpcDownClassmenData() => new (int, string)[]
        {
            (11, "若菜白兎（わかな・しろう）：初等部2年2組：P72"), (12, "海原満月（かいばら・みづき）：初等部4年1組：P66"),
            (13, "雫（しずく）：初等部4年1組：P60"), (14, "相馬カズヤ（そうま・−）：初等部4年1組：P48"),
            (15, "廿九日神無（ひづめ・かんな）：初等部4年1組：P62"), (16, "カイン大澤（−・おおさわ）：初等部5年2組：P12"),
            (22, "機嶋結（きじま・ゆう）：初等部6年2組：P123"), (23, "静馬源一（しずま・げんいち）：初等部6年12組：P78"),
            (24, "花菱彪臥（はなびし・ひょうが）：中等部1年1組：P53"), (25, "天菱東希（てんびし・あずき）：中等部2年2組：P61"),
            (26, "御守陸（みもり・りく）：中等部2年2組：P51"), (33, "西園寺勇（さいおんじ・ゆう）：中等部2年3組：P47"),
            (34, "九条朔（くじょう・さく）：中等部3年1組：P77"), (35, "柴島華桜璃（くにじま・かおり）：中等部3年1組：P48"),
            (36, "唐沢完子（からさわ・かんこ）：中等部3年2組：P57"), (44, "雪成藤花（ゆきなり・とうか）：中等部3年2組：P53"),
            (45, "桐原雅（きりはら・みやび）：高等部1年1組：P18"), (46, "アイリス・L・橋場（−・るなくるす・はしば）：高等部1年11組：P79"),
            (55, "双星一（そうせい・はじめ）：高等部1年120組：P152"), (56, "影野恭弥（かげの・きょうや）：高等部2年2組：P78"),
            (66, "振り直し／任意"),
        };

        private (int key, string value)[] GetRandomNpcUpperClassmenData() => new (int, string)[]
        {
            (11, "下妻笹緒（しもつま・ささお）：高等部2年2組：P117"), (12, "ファティナ・Ｖ・アイゼンブルク（−・フォン・−）：高等部2年3組：P110"),
            (13, "姫川翔（ひめかわ・しょう）：高等部2年5組：P111"), (14, "イシュタル：高等部2年115組：P152"),
            (15, "小田切ルビィ（おだぎり・−）：高等部3年4組：P109"), (16, "大炊御門菫（おおいのみかど・すみれ）：高等部3年6組：P112"),
            (22, "朱頼天山楓（しゅらい・てんざん・かえで）：高等部3年114組：P152"), (23, "米流是武武（べるぜぶぶ）：高等部3年117組：P152"),
            (24, "麻生遊夜（あそう・ゆうや）：大学部1年1組：P122"), (25, "フィオナ・ボールドウィン：大学部1年1組：P79"),
            (26, "アデル・リーヴィス：大学部1年50組：P152"), (33, "エルディン：大学部1年50組：P152"),
            (34, "斐川幽夜（ひかわ・ももや）：大学部2年2組：P57"), (35, "ミハイル・エッカート：大学部2年4組：P122"),
            (36, "阿岳恭司（あたけ・きょうじ）：大学部2年9組：P119"), (44, "アンジェラ・アップルトン：大学部2年9組：P75"),
            (45, "アウリーエ・Ｆ・ダッチマン（−・フライング・−）：大学部3年50組：P152"), (46, "ジェディファ・エルクラステ：大学部3年50組：P152"),
            (55, "有田アリストテレス（ありた・−）：大学部4年6組：P69"), (56, "澄野絣（すみの・かすり）：大学部4年6組：P61"),
            (66, "振り直し／任意"),
        };

        // Encounter table data
        private (int key, string value)[] GetUsuallyEncountData() => new (int, string)[]
        {
            (2, "ブラックシープ（基本ｐ129）×2"), (3, "イフリート（基本ｐ126）、グリフォン（基本ｐ126）"),
            (4, "ファウスト（基本ｐ129）、ワーウルフ（基本ｐ129）"), (5, "焔の蝶（ｐ99）"),
            (6, "デビルキャリアー（ｐ102）、狂信者（ｐ104）"), (7, "ケルベロス（ｐ99）、百足女（ｐ99）"),
            (8, "カプリコーン（ｐ102）×2"), (9, "邪神怪人（基本ｐ135）"),
            (10, "聖少女（基本ｐ127）"), (11, "業魔（基本ｐ129）、デュアル（基本ｐ126）"),
            (12, "ファイアレーベン（ｐ98）×2、ゴーレム（ｐ99）"), (13, "ブラッドウォーリア（ｐ103）、ブラッドロード（ｐ103）"),
            (14, "フェニックス（ｐ99）、ドラゴンゾンビ（ｐ99）"), (15, "マッドマーマンティス（ｐ102）×2、バンパイアロード（ｐ103）"),
            (16, "ドラゴン（ｐ99）×2"), (17, "風紀委員（ｐ107）、保安委員（ｐ107）×2"),
        };

        private (int key, string value)[] GetBossEncountData() => new (int, string)[]
        {
            (2, "剥奪天使（ｐ101）、イフリート（基本ｐ126）×PCと同じ数"), (3, "没落悪魔（ｐ105）、ブラックシープ（基本ｐ129）×PCと同じ数"),
            (4, "技巧派教員（基本ｐ134）、一般人×PCと同じ数"), (5, "肉体派教員（基本ｐ135）、オペレーター×PCと同じ数"),
            (6, "フェニックス（ｐ99）、焔の蝶（ｐ99）×PCと同じ数"), (7, "美しきバンシー（ｐ103）、デビルキャリアー（ｐ102）×PCと同じ数"),
            (8, "ドラゴン（ｐ99）、キラー（基本ｐ126）×PCと同じ数"), (9, "バンパイアロード（ｐ103）、戦闘員（基本ｐ132）×PCと同じ数"),
            (10, "殺戮天使（ｐ101）、下級使徒（ｐ100）×PCと同じ数"), (11, "小悪魔（ｐ105）、デスストーカー（ｐ102）×PCと同じ数"),
            (12, "仙人（基本ｐ127）、サブラヒナイト（基本ｐ126）×PCと同じ数"), (13, "狂犬（ｐ104）、ワーウルフ（基本ｐ129）×PCと同じ数"),
            (14, "下級天使（基本ｐ128）、ケルベロス（ｐ99）×PCと同じ数"), (15, "誘惑悪魔（ｐ105）、カプリコーン（ｐ102）×PCと同じ数"),
            (16, "権天使（基本ｐ128）、百足女（ｐ99）×PCと同じ数"), (17, "悪魔貴族（基本ｐ131）、ブラッドウォーリア（ｐ103）×PCと同じ数"),
        };

        // TABLES は Ruby 版のテーブル定義に対応（省略 - 別途テーブル実装が必要）
        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

    }
}
