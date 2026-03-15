using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// エムブリオマシンRPG
    /// </summary>
    public sealed class EmbryoMachine : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly EmbryoMachine Instance = new EmbryoMachine();


        private static readonly Regex HltRegex = new Regex(
            @"HLT",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SftRegex = new Regex(
            @"SFT",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MftRegex = new Regex(
            @"MFT",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "EmbryoMachine";

        /// <inheritdoc/>
        public override string Name => "エムブリオマシンRPG";

        /// <inheritdoc/>
        public override string SortKey => "えむふりおましんRPG";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定ロール(EMt+m@c#f)
        　目標値t、修正値m、クリティカル値c(省略時は20)、ファンブル値f(省略時は2)で攻撃判定を行います。
        　命中した場合は命中レベルと命中部位も自動出力します。
        　Rコマンドに読み替えされます。
        ・各種表
        　・命中部位表　HLT
        　・白兵攻撃ファンブル表　MFT
        　・射撃攻撃ファンブル表　SFT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var checkResult = CheckRoll(command, randomizer);
            if (checkResult != null)
            {
                return checkResult;
            }

            var output = "1";
            var type = "";
            int number;

            Match match;

            match = HltRegex.Match(command);
            if (match.Success)
            {
                type = "命中部位";
                number = randomizer.RollSum(2, 10);
                output = GetHitLocationTable(number);
                if (output != "1")
                {
                    output = $"{type}表({number}) ＞ {output}";
                }
                return Result.CreateBuilder(output).Build();
            }

            match = SftRegex.Match(command);
            if (match.Success)
            {
                type = "射撃ファンブル";
                number = randomizer.RollSum(2, 10);
                output = GetShootFumbleTable(number);
                if (output != "1")
                {
                    output = $"{type}表({number}) ＞ {output}";
                }
                return Result.CreateBuilder(output).Build();
            }

            match = MftRegex.Match(command);
            if (match.Success)
            {
                type = "白兵ファンブル";
                number = randomizer.RollSum(2, 10);
                output = GetMeleeFumbleTable(number);
                if (output != "1")
                {
                    output = $"{type}表({number}) ＞ {output}";
                }
                return Result.CreateBuilder(output).Build();
            }

            return null;
        }

        private string ReplaceText(string str)
        {
            str = Regex.Replace(str, @"EM(\d+)([+-][+\-\d]+)(@(\d+))(\#(\d+))", m => $"2R10{m.Groups[2].Value}>={m.Groups[1].Value}[{m.Groups[4].Value},{m.Groups[6].Value}]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)([+-][+\-\d]+)(\#(\d+))", m => $"2R10{m.Groups[2].Value}>={m.Groups[1].Value}[20,{m.Groups[4].Value}]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)([+-][+\-\d]+)(@(\d+))", m => $"2R10{m.Groups[2].Value}>={m.Groups[1].Value}[{m.Groups[4].Value},2]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)([+-][+\-\d]+)", m => $"2R10{m.Groups[2].Value}>={m.Groups[1].Value}[20,2]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)(@(\d+))(\#(\d+))", m => $"2R10>={m.Groups[1].Value}[{m.Groups[3].Value},{m.Groups[5].Value}]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)(\#(\d+))", m => $"2R10>={m.Groups[1].Value}[20,{m.Groups[3].Value}]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)(@(\d+))", m => $"2R10>={m.Groups[1].Value}[{m.Groups[3].Value},2]", RegexOptions.IgnoreCase);
            str = Regex.Replace(str, @"EM(\d+)", m => $"2R10>={m.Groups[1].Value}[20,2]", RegexOptions.IgnoreCase);
            return str;
        }

        private Result? ResultNd10(int total, int dice_total, int _dice_list, CompareOperator? cmp_op, int target)
        {
            if (cmp_op != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }

            if (dice_total <= 2)
            {
                return Result.CreateBuilder("ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (dice_total >= 20)
            {
                return Result.CreateBuilder("クリティカル").SetSuccess(true).SetCritical(true).Build();
            }
            else if (total >= target)
            {
                return Result.CreateBuilder("成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder("失敗").SetFailure(true).Build();
            }
        }

        private Result? CheckRoll(string str, IRandomizer randomizer)
        {
            str = ReplaceText(str);
            var m = Regex.Match(str, @"(^|\s)S?(2[rR]10([+\-\d]+)?([>=]+(\d+))(\[(\d+),(\d+)\]))(\s|$)", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var matchedStr = m.Groups[2].Value;
            var diff = 0;
            var crit = 20;
            var fumble = 2;
            var mod = 0;
            var modText = m.Groups[3].Value;

            if (m.Groups[5].Success && m.Groups[5].Value.Length > 0)
            {
                diff = int.Parse(m.Groups[5].Value);
            }
            if (m.Groups[7].Success && m.Groups[7].Value.Length > 0)
            {
                crit = int.Parse(m.Groups[7].Value);
            }
            if (m.Groups[8].Success && m.Groups[8].Value.Length > 0)
            {
                fumble = int.Parse(m.Groups[8].Value);
            }
            if (!string.IsNullOrEmpty(modText))
            {
                mod = ArithmeticEvaluator.Eval(modText, RoundType) ?? 0;
            }

            var dice_arr = randomizer.RollBarabara(2, 10).OrderBy(x => x).ToList();
            var dice_now = dice_arr.Sum();
            var dice_str = string.Join(",", dice_arr);

            var dice_loc = randomizer.RollSum(2, 10);
            var big_dice = dice_arr[1];
            var output = $"{dice_now}[{dice_str}]";
            var total_n = dice_now + mod;

            if (mod > 0)
            {
                output += $"+{mod}";
            }
            else if (mod < 0)
            {
                output += mod.ToString();
            }

            if (Regex.IsMatch(output, @"[^\d\[\]]+"))
            {
                output = $"({matchedStr}) ＞ {output} ＞ {total_n}";
            }
            else
            {
                output = $"({matchedStr}) ＞ {output}";
            }

            if (dice_now <= fumble)
            {
                output += " ＞ ファンブル";
            }
            else if (dice_now >= crit)
            {
                output += " ＞ クリティカル ＞ " + GetHitLevelTable(big_dice) + $"(ダメージ+10) ＞ [{dice_loc}]{GetHitLocationTable(dice_loc)}";
            }
            else if (total_n >= diff)
            {
                output += " ＞ 成功 ＞ " + GetHitLevelTable(big_dice) + $" ＞ [{dice_loc}]{GetHitLocationTable(dice_loc)}";
            }
            else
            {
                output += " ＞ 失敗";
            }

            return Result.CreateBuilder(output).Build();
        }

        private string GetHitLocationTable(int num)
        {
            var table = new (int threshold, string text)[] {
                (4, "頭"), (7, "左脚"), (9, "左腕"), (12, "胴"), (14, "右腕"), (17, "右脚"), (20, "頭")
            };
            return GetTableByNumber(num, table);
        }

        private string GetShootFumbleTable(int num)
        {
            var output = "1";
            var table = new[] { "暴発した。使用した射撃武器が搭載されている部位に命中レベルAで命中する。", "あまりに無様な誤射をした。パイロットの精神的負傷が2段階上昇する。", "誤射をした。自機に最も近い味方機体に命中レベルAで命中する。", "誤射をした。対象に最も近い味方機体に命中レベルAで命中する。", "武装が暴発した。使用した射撃武器が破損する。ダメージは発生しない。", "転倒した。次のセグメントのアクションが待機に変更される。", "弾詰まりを起こした。使用した射撃武器は戦闘終了まで使用できなくなる。", "砲身が大きく歪んだ。使用した射撃武器による射撃攻撃の命中値が戦闘終了まで-3される。", "熱量が激しく増大した。使用した射撃武器の消費弾薬が戦闘終了まで+3される。", "暴発した。使用した射撃武器が搭載されている部位に命中レベルBで命中する。", "弾薬が劣化した。使用した射撃武器の全てのダメージが戦闘終了まで-2される。", "無様な誤射をした。パイロットの精神的負傷が1段階上昇する。", "誤射をした。対象に最も近い味方機体に命中レベルBで命中する。", "誤射をした。自機に最も近い味方機体に命中レベルBで命中する。", "砲身が歪んだ。使用した射撃武器による射撃攻撃の命中値が戦闘終了まで-2される。", "熱量が増大した。使用した射撃武器の消費弾薬が戦闘終了まで+2される。", "砲身がわずかに歪んだ。使用した射撃武器による射撃攻撃の命中値が戦闘終了まで-1される。", "熱量がやや増大した。使用した射撃武器の消費弾薬が戦闘終了まで+1される。", "何も起きなかった。" };
            var dc = 2;
            var index = num - dc;
            if (index >= 0 && index < table.Length)
            {
                output = table[index];
            }
            return output;
        }

        private string GetMeleeFumbleTable(int num)
        {
            var output = "1";
            var table = new[] { "大振りしすぎた。使用した白兵武器が搭載されている部位の反対の部位(右腕に搭載されているなら左側)に命中レベルAで命中する。", "激しく頭を打った。パイロットの肉体的負傷が2段階上昇する。", "過負荷で部位が爆発した。使用した白兵武器が搭載されている部位が全壊する。ダメージは発生せず、搭載されている武装も破損しない。", "大振りしすぎた。使用した白兵武器が搭載されている部位の反対の部位(右腕に搭載されているなら左側)に命中レベルBで命中する。", "武装が爆発した。使用した白兵武器が破損する。ダメージは発生しない。", "部分的に機能停止した。使用した白兵武器は戦闘終了まで使用できなくなる。", "転倒した。次のセグメントのアクションが待機に変更される。", "激しい刃こぼれを起こした。使用した白兵武器の全てのダメージが戦闘終了まで-3される。", "地面の凹凸にはまった。次の2セグメントは移動を行うことができない。", "刃こぼれを起こした。使用した白兵武器の全てのダメージが戦闘終了まで-2される。", "大振りしすぎた。使用した白兵武器が搭載されている部位の反対の部位(右腕に搭載されているなら左側)に命中レベルCで命中する。", "頭を打った。パイロットの肉体的負傷が1段階上昇する。", "駆動系が損傷した。移動力が戦闘終了まで-2される(最低1)。", "間合いを取り損ねた。隣接している機体(複数の場合は1機をランダムに決定)に激突する。", "機体ごと突っ込んだ。機体が向いている方角へ移動力をすべて消費するまで移動する。", "制御系が損傷した。回避値が戦闘終了まで-1される(最低1)。", "踏み誤った。機体が向いている方角へ移動力の半分を消費するまで移動する。", "たたらを踏んだ。機体が向いている方角へ1の移動力で移動する。", "何も起きなかった。" };
            var dc = 2;
            var index = num - dc;
            if (index >= 0 && index < table.Length)
            {
                output = table[index];
            }
            return output;
        }

        private string GetHitLevelTable(int num)
        {
            var table = new (int threshold, string text)[] {
                (6, "命中レベルC"), (9, "命中レベルB"), (10, "命中レベルA")
            };
            return GetTableByNumber(num, table);
        }

        private static string GetTableByNumber(int num, (int threshold, string text)[] table)
        {
            foreach (var (threshold, text) in table)
            {
                if (num <= threshold)
                {
                    return text;
                }
            }
            return table[table.Length - 1].text;
        }

    }
}
