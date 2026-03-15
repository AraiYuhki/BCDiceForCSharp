using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// メタルヘッドエクストリーム
    /// </summary>
    public sealed class MetalHeadExtream : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly MetalHeadExtream Instance = new MetalHeadExtream();


        private static readonly Regex AsRDRegex = new Regex(
            @"([AS])R(\d+)(([*/]\d+)*)?(((@|A|L)\d+)*)(!M)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HuBkWaRegex = new Regex(
            @"(HU|BK|WA|SC|BG|IN|PT|HT|TA|AC|HE|TR|VT|BO|CS|TH|AM|GD|HC|BI|BT|AI)HIT(\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SuvAZRegex = new Regex(
            @"SUV([A-Z])(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex HtalmebpdDmgLmhoRegex = new Regex(
            @"([HTALMEBPD])DMG([LMHO])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CrtDRegex = new Regex(
            @"CRT(\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GsmeAcDRegex = new Regex(
            @"([GSME])AC(\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AslMaDRegex = new Regex(
            @"([ASL])MA(\d+)?(\+(\d+))?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WEncDRegex = new Regex(
            @"(W)ENC(\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "MetalHeadExtream";

        /// <inheritdoc/>
        public override string Name => "メタルヘッドエクストリーム";

        /// <inheritdoc/>
        public override string SortKey => "めたるへつとえくすとりいむ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定：ARn or SRn[*/a][@b][Ac][Ld][!M]　　[]内省略可。
        「n」で判定値、「*/a」でロール修正を指定。複数回指定可。
        「@b」でアクシデント値、省略時は「96」。
        「Ac」で高度なロール。「2、4、8」のみ指定可能。
        「Ld」でラックポイント、「!M」でパンドラ《ミューズ》。

        【書式例】
        AR84/2@99!M → 判定値84のAR1/2。アクシデント値99、パンドラ《ミューズ》。
        SR40*2A2L1@99 → 判定値80のSR、高度なロール2倍、ラック1点。

        ◆命中部位表：(命中部位)HIT[n]　　以降、ROC時は[n]を指定。
        HU：人間　　BK：バイク　　WA：ワゴン　　SC：シェルキャリア　　BG：バギー
        IN：インセクター　　PT：ポケットタンク　　HT：ホバータンク　　TA：戦車
        AC：装甲車　　HE：ヘリ　　TR：トレーラー　　VT：VTOL　　BO：ボート
        CS：通常、格闘型コンバットシェル　　TH：可変、重コンバットシェル
        AM：オートモビル　　GD：ガンドック　　HC：ホバークラフト
        BI：自転車　　BT：バトルトレーラー　　AI：エアクラフト
        ◆戦闘結果表：SUV(A～Z)n　　【書式例】SUVM100
        ◆損傷効果表：(命中部位)DMG(損傷種別)　　【書式例】TDMGH
        H：頭部　　T：胴部　　A：腕部　　L：脚部　　M：心理　　E：電子
        B：メカニック本体　　P：パワープラント　　D：ドライブ
        (損傷種別)　L：LW　　M：MW　　H：HW　　O：MO
        ◆クリティカル表：CRT[n]
        ◆アクシデント表：(種別)AC[n]
        G：格闘　　S：射撃、投擲　　M：心理　　E：電子
        ◆メカニック事故表：(場所)MA[n][+m]　　「+m」で修正を指定。
        A：空中　　S：水上、水中　　L：地上

        【マスコンバット】
        ストラテジーイベントチャート：SEC
        NPC攻撃処理チャート：NAC　　敗者運命チャート：LDC

        【各種表】
        荒野ランダムエンカウント表：WENC[n]
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            string? hitPart;
            string? resultText;
            int roc = 0;
            string? locationType;
            Match match;

            match = AsRDRegex.Match(command);
            if (match.Success)
            {
                var type = match.Groups[1].Value;
                var target = int.Parse(match.Groups[2].Value);
                var modify = GetValue(1, match.Groups[3].Value);
                var paramText = match.Groups[5].Success ? match.Groups[5].Value : "";
                var isMuse = match.Groups[8].Success;
                var accidentValue = 96;
                var advancedRoll = 1;
                var luckPoint = 0;
                var paramMatches = Regex.Matches(paramText, @"(.)(\d+)");
                foreach (Match pm in paramMatches)
                {
                    var marker = pm.Groups[1].Value;
                    var value = pm.Groups[2].Value;
                    var (newAccident, newAdvanced, newLuck) = GetRollParameter(accidentValue, advancedRoll, luckPoint, marker, value);
                    accidentValue = newAccident;
                    advancedRoll = newAdvanced;
                    luckPoint = newLuck;
                }
                return Result.CreateBuilder(CheckRoll(type, target, modify, accidentValue, advancedRoll, luckPoint, isMuse, randomizer))
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            match = HuBkWaRegex.Match(command);
            if (match.Success)
            {
                hitPart = match.Groups[1].Value;
                roc = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                resultText = GetHitTable(hitPart, roc, randomizer);
                if (resultText != null)
                {
                    return Result.CreateBuilder(resultText).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                }
            }

            match = SuvAZRegex.Match(command);
            if (match.Success)
            {
                var armorGrade = match.Groups[1].Value;
                var damage = int.Parse(match.Groups[2].Value);
                return Result.CreateBuilder(GetSuvTable(armorGrade, damage))
                    .SetSuccess(true)
                    .Build();
            }

            match = HtalmebpdDmgLmhoRegex.Match(command);
            if (match.Success)
            {
                hitPart = match.Groups[1].Value;
                var damageStage = match.Groups[2].Value;
                resultText = GetDamageEffectTable(hitPart, damageStage);
                if (resultText != null)
                {
                    return Result.CreateBuilder(resultText).SetSuccess(true).Build();
                }
            }

            match = CrtDRegex.Match(command);
            if (match.Success)
            {
                roc = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                return Result.CreateBuilder(GetCriticalTable(roc, randomizer))
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            match = GsmeAcDRegex.Match(command);
            if (match.Success)
            {
                var damageType = match.Groups[1].Value;
                roc = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                resultText = GetAccidentTable(damageType, roc, randomizer);
                if (resultText != null)
                {
                    return Result.CreateBuilder(resultText).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                }
            }

            match = AslMaDRegex.Match(command);
            if (match.Success)
            {
                locationType = match.Groups[1].Value;
                roc = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                var correction = match.Groups[4].Success ? int.Parse(match.Groups[4].Value) : 0;
                resultText = GetMechanicAccidentTable(locationType, roc, correction, randomizer);
                if (resultText != null)
                {
                    return Result.CreateBuilder(resultText).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                }
            }

            if (command == "SEC")
            {
                return Result.CreateBuilder(GetStrategyEventChart(randomizer))
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            if (command == "NAC")
            {
                return Result.CreateBuilder(GetNpcAttackChart(randomizer))
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            if (command == "LDC")
            {
                return Result.CreateBuilder(GetLoserDestinyChart(randomizer))
                    .SetSuccess(true)
                    .SetRands(randomizer.RandResults)
                    .Build();
            }

            match = WEncDRegex.Match(command);
            if (match.Success)
            {
                locationType = match.Groups[1].Value;
                roc = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                resultText = GetRandomEncounterTable(locationType, roc, randomizer);
                if (resultText != null)
                {
                    return Result.CreateBuilder(resultText).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                }
            }

            return null;
        }

        private string CheckRoll(string rollText, int target, double modify, int accidentValue, int advancedRoll, int luckPoint, bool isMuse, IRandomizer randomizer)
        {
            var rollTarget = (int)(target * modify / advancedRoll * Math.Pow(2, luckPoint));
            var dice = randomizer.RollOnce(100);
            var (resultText, successValue) = GetRollResultTextAndSuccessValue(dice, advancedRoll, rollTarget, accidentValue, isMuse);
            resultText += $" 達成値：{successValue}";
            var complementText = $"ACC:{accidentValue}";
            if (advancedRoll > 1)
            {
                complementText += $", ADV:*{advancedRoll}";
            }
            if (luckPoint > 0)
            {
                complementText += $", LUC:{luckPoint}";
            }
            string modifyText;
            if (modify >= 1)
            {
                modifyText = ((int)modify).ToString();
            }
            else
            {
                modifyText = $"1/{(int)(1 / modify)}";
            }
            var formulaText = GetFormulaText(target, modify, advancedRoll, luckPoint);
            var result = $"{rollText}R{modifyText}({complementText})：1D100<={rollTarget}{formulaText} ＞ [{dice}] {resultText}";
            if (isMuse)
            {
                result += " 《ミューズ》";
            }
            return result;
        }

        private (int, int, int) GetRollParameter(int accident, int advanced, int luck, string marker, string value)
        {
            var intValue = int.Parse(value);
            switch (marker)
            {
                case "@":
                    accident = intValue;
                    break;
                case "A":
                    if (new[] { 2, 4, 8 }.Contains(intValue))
                    {
                        advanced = intValue;
                    }
                    break;
                case "L":
                    luck = intValue;
                    break;
            }
            return (accident, advanced, luck);
        }

        private (string, int) GetRollResultTextAndSuccessValue(int dice, int advancedRoll, int rollTarget, int accidentValue, bool isMuse)
        {
            var successValue = 0;
            if (dice >= accidentValue)
            {
                return ("失敗（アクシデント）", successValue);
            }
            if (dice > rollTarget)
            {
                return ("失敗", successValue);
            }
            var dig1 = dice - (dice / 10) * 10;
            bool isCritical;
            if (isMuse)
            {
                isCritical = dig1 <= 1;
            }
            else
            {
                isCritical = dig1 == 1;
            }
            var resultText = "成功";
            if (isCritical)
            {
                resultText += "（クリティカル）";
            }
            successValue = dice * advancedRoll;
            return (resultText, successValue);
        }

        private string GetFormulaText(int target, double modify, int advancedRoll, int luckPoint)
        {
            var formulaText = target.ToString();
            if (modify > 1)
            {
                formulaText += $"*{(int)modify}";
            }
            if (modify < 1)
            {
                formulaText += $"/{(int)(1 / modify)}";
            }
            if (advancedRoll > 1)
            {
                formulaText += $"/{advancedRoll}";
            }
            if (luckPoint > 0)
            {
                formulaText += $"*{Math.Pow(2, luckPoint)}";
            }
            if (formulaText == target.ToString())
            {
                return "";
            }
            return $"[{formulaText}]";
        }

        private string? GetHitTable(string hitPart, int roc, IRandomizer randomizer)
        {
            string name;
            (int, string)[] table;
            switch (hitPart)
            {
                case "HU":
                    name = "命中部位表：人間";
                    table = new (int, string)[] { (1, "胴部（クリティカル）"), (2, "頭部"), (3, "左腕部"), (4, "右腕部"), (5, "胴部"), (6, "胴部"), (7, "胴部"), (8, "胴部"), (9, "脚部"), (10, "脚部") };
                    break;
                case "BK":
                    name = "命中部位表：バイク";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "パワープラント"), (6, "ドライブ"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "乗員"), (10, "乗員") };
                    break;
                case "WA":
                    name = "命中部位表：ワゴン";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "パワープラント"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "SC":
                    name = "命中部位表：シェルキャリア";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "パワープラント"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "BG":
                    name = "命中部位表：バギー";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "IN":
                    name = "命中部位表：インセクター";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "ドライブ"), (9, "ドライブ"), (10, "乗員") };
                    break;
                case "PT":
                    name = "命中部位表：ポケットタンク";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "パワープラント"), (8, "ドライブ"), (9, "ドライブ"), (10, "兵装・貨物") };
                    break;
                case "HT":
                    name = "命中部位表：ホバータンク";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "パワープラント"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "兵装・貨物") };
                    break;
                case "TA":
                    name = "命中部位表：戦車";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "兵装・貨物") };
                    break;
                case "AC":
                    name = "命中部位表：装甲車";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "兵装・貨物") };
                    break;
                case "HE":
                    name = "命中部位表：ヘリ";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "パワープラント"), (6, "ドライブ"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "TR":
                    name = "命中部位表：トレーラー";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "パワープラント"), (6, "ドライブ"), (7, "兵装・カーゴ"), (8, "兵装・カーゴ"), (9, "兵装・カーゴ"), (10, "乗員") };
                    break;
                case "VT":
                    name = "命中部位表：VTOL";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "BO":
                    name = "命中部位表：ボート";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "本体"), (8, "パワープラント"), (9, "ドライブ"), (10, "兵装・貨物") };
                    break;
                case "CS":
                    name = "命中部位表：通常・格闘型コンバットシェル";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "ザック"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "兵装・貨物") };
                    break;
                case "TH":
                    name = "命中部位表：可変・重コンバットシェル";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "本体"), (7, "ドライブ"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "兵装・貨物") };
                    break;
                case "AM":
                    name = "命中部位表：オートモビル";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "GD":
                    name = "命中部位表：ガンドック";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "ドライブ"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "HC":
                    name = "命中部位表：ホバークラフト";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "パワープラント"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "乗員"), (10, "乗員") };
                    break;
                case "BI":
                    name = "命中部位表：自転車";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "BT":
                    name = "命中部位表：バトルトレーラー";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                case "AI":
                    name = "命中部位表：エアクラフト";
                    table = new (int, string)[] { (1, "本体（クリティカル）"), (2, "本体"), (3, "本体"), (4, "本体"), (5, "本体"), (6, "パワープラント"), (7, "ドライブ"), (8, "兵装・貨物"), (9, "兵装・貨物"), (10, "乗員") };
                    break;
                default:
                    return null;
            }
            return GetMetalHeadExtream1d10TableResult(name, table, roc, randomizer);
        }

        private string GetSuvTable(string armorGrade, int damage)
        {
            var name = "戦闘結果表";
            var table = new int[][] {
                new[] { 0, 1, 6, 16, 26, 36 },
                new[] { 0, 1, 6, 26, 36, 46 },
                new[] { 0, 1, 16, 26, 46, 56 },
                new[] { 1, 6, 26, 36, 56, 76 },
                new[] { 1, 16, 36, 46, 66, 76 },
                new[] { 1, 26, 36, 56, 76, 86 },
                new[] { 1, 36, 56, 66, 76, 96 },
                new[] { 1, 56, 76, 86, 96, 106 },
                new[] { 1, 66, 86, 96, 106, 116 },
                new[] { 1, 66, 86, 96, 116, 136 },
                new[] { 1, 76, 96, 106, 126, 156 },
                new[] { 1, 76, 96, 116, 146, 166 },
                new[] { 1, 86, 106, 126, 166, 176 },
                new[] { 1, 106, 126, 136, 176, 196 },
                new[] { 1, 106, 126, 146, 186, 206 },
                new[] { 1, 116, 136, 156, 196, 206 },
                new[] { 1, 126, 146, 166, 206, 226 },
                new[] { 1, 126, 146, 176, 226, 246 },
                new[] { 1, 136, 156, 186, 246, 266 },
                new[] { 1, 156, 176, 206, 246, 286 },
                new[] { 1, 156, 176, 206, 266, 306 },
                new[] { 1, 166, 186, 206, 286, 346 },
                new[] { 1, 176, 196, 246, 326, 366 },
                new[] { 1, 196, 226, 266, 346, 386 },
                new[] { 1, 206, 226, 286, 366, 406 },
                new[] { 1, 226, 246, 306, 386, 406 }
            };
            var armorIndex = armorGrade[0] - 'A';
            var damageInfo = table[armorIndex];
            var woundRanks = new[] { "無傷", "LW(軽傷)", "MW(中傷)", "HW(重傷)", "MO(致命傷)", "KL(死亡)" };
            var woundText = "";
            for (var index = 0; index < damageInfo.Length; index++)
            {
                var rate = damageInfo[index];
                if (rate > damage)
                {
                    break;
                }
                woundText = woundRanks[index];
            }
            return $"{name}({armorGrade})：{damage} ＞ {woundText}";
        }

        private string? GetDamageEffectTable(string hitPart, string damageStage)
        {
            var damageInfos = new (string, string)[] { ("L", "(LW)"), ("M", "(MW)"), ("H", "(HW)"), ("O", "(MO)") };
            var index = -1;
            for (var i = 0; i < damageInfos.Length; i++)
            {
                if (damageInfos[i].Item1 == damageStage)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                return null;
            }
            var damageIndex = index + 1;
            var damageText = damageInfos[index].Item2;
            string name;
            (int, string)[] table;
            switch (hitPart)
            {
                case "H":
                    name = "対人損傷効果表：頭部";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。【PER】のAR、【PER】がベースアビリティのスキルのSRにSR1/2のロール修正。"), (3, "ダメージ修正+20。【PER】のAR、【PER】がベースアビリティのスキルのSRにSR1/4のロール修正。"), (4, "ダメージ修正+30。［死亡］。頭部がサイバーの場合は［戦闘不能］。") };
                    break;
                case "T":
                    name = "対人損傷効果表：胴部";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。【DEX】のAR、【DEX】がベースアビリティのスキルのSRにSR1/2のロール修正。"), (3, "ダメージ修正+20。【DEX】のAR、【DEX】がベースアビリティのスキルのSRにSR1/4のロール修正。"), (4, "ダメージ修正+30。［戦闘不能］。") };
                    break;
                case "A":
                    name = "対人損傷効果表：腕部";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。損傷した腕を使用する、また両腕を使用する行動にSR1/2のロール修正。"), (3, "ダメージ修正+20。損傷した腕を使用する、また両腕を使用する行動にSR1/4のロール修正。"), (4, "ダメージ修正+30。損傷した腕を使用する、また両腕を使用する行動不可。") };
                    break;
                case "L":
                    name = "対人損傷効果表：脚部";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。【REF】のAR、【REF】がベースアビリティのスキルのSRにSR1/2のロール修正。"), (3, "ダメージ修正+20。【REF】のAR、【REF】がベースアビリティのスキルのSRにSR1/4のロール修正。【MV】が1/2。"), (4, "ダメージ修正+30。［戦闘不能］。") };
                    break;
                case "M":
                    name = "心理損傷効果表";
                    table = new (int, string)[] { (1, "ダメージ修正+10。焦り。効果は特になし。シーン終了で自然回復。"), (2, "ダメージ修正+20。混乱。1シーン、すべてのロールがSR1/2となる。シーン終了で自然回復。"), (3, "ダメージ修正+30。恐怖。1シーン、すべてのロールがSR1/4となる。シーン終了で自然回復。"), (4, "ダメージ修正+50。喪失。［戦闘不能］。シーン終了で自然回復。") };
                    break;
                case "E":
                    name = "電子損傷効果表";
                    table = new (int, string)[] { (1, "ダメージ修正+10。処理落ち。効果は特になし。"), (2, "ダメージ修正+20。ノイズ。1シーン、キャラクターならすべてのロールが、アイテムならそれを使用したロールが1/2となる。"), (3, "ダメージ修正+30。恐怖。1シーン、キャラクターならすべてのロールが、アイテムならそれを使用したロールが1/4となる。"), (4, "ダメージ修正+50。クラッシュ。キャラクターなら［戦闘不能］。アイテムなら1シナリオ中、使用不可。") };
                    break;
                case "B":
                    name = "メカニック損傷効果表：本体";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正シフト1。修理費がフレーム価格の1/4かかる。"), (3, "ダメージ修正シフト2。修理費がフレーム価格の1/2かかる。"), (4, "ダメージ修正シフト3。移動不能。修理費がフレーム価格と同じだけかかる。走行中なら事故表を振ること。") };
                    break;
                case "P":
                    name = "メカニック損傷効果表：パワープラント";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。メカニックの【MV】が1/2になる。修理費がパワープラント価格の1/4かかる。"), (3, "ダメージ修正+20。メカニックの【MV】が1/4になる。修理費がパワープラント価格の1/2かかる。"), (4, "ダメージ修正+30。移動不能。修理費がパワープラント価格と同じだけかかる。走行中なら事故表を振ること。") };
                    break;
                case "D":
                    name = "メカニック損傷効果表：ドライブ";
                    table = new (int, string)[] { (1, "ダメージ修正+10。"), (2, "ダメージ修正+10。メカニックの【REF】が1/2になる。［メカニック］スキルにSR1/2の修正。修理費がドライブ価格の1/4かかる。"), (3, "ダメージ修正+20。メカニックの【REF】が1/2になる。［メカニック］スキルにSR1/4の修正。修理費がドライブ価格の1/2かかる。"), (4, "ダメージ修正+30。移動不能。修理費がドライブ価格と同じだけかかる。走行中なら事故表を振ること。") };
                    break;
                default:
                    return null;
            }
            var text = GetTableByNumber(damageIndex, table);
            return $"{name}{damageText} ＞ {text}";
        }

        private string GetCriticalTable(int roc, IRandomizer randomizer)
        {
            var name = "クリティカル表";
            var table = new (int, string)[] { (1, "特に追加被害は発生しない。"), (2, "対象はバランスを崩す。クリンナッププロセスまで、対象は命中ロールにSR1/2のロール修正を受ける。"), (3, "対象に隙を作る。クリンナッププロセスまで、対象はリアクションにSR1/2のロール修正を受ける。"), (4, "激しい一撃。最終火力に+20してダメージを算出すること。"), (5, "多大なダメージ。最終火力に+20してダメージを算出すること。"), (6, "弱点に直撃。対象の装甲値を無視してダメージを算出すること。"), (7, "効果的な一撃。対象の受ける損傷段階をシフト1する。"), (8, "致命的な一撃。対象の受ける損傷段階をシフト2する。"), (9, "中枢に直撃。対象の【SUV】を3ランク低いものとしてダメージを算出する。"), (10, "中枢を破壊。対象の装甲値を無視し、【SUV】を3ランク低いものとしてダメージを算出する。") };
            return GetMetalHeadExtream1d10TableResult(name, table, roc, randomizer);
        }

        private string? GetAccidentTable(string damageType, int roc, IRandomizer randomizer)
        {
            string name;
            (int, string)[] table;
            switch (damageType)
            {
                case "G":
                    name = "格闘アクシデント表";
                    table = new (int, string)[] { (1, "体勢を崩す。その攻撃は失敗する。"), (2, "体勢を崩す。その攻撃は失敗する。"), (3, "体勢を崩す。その攻撃は失敗する。"), (4, "転倒。格闘回避と機動回避にSR1/4、【MV】が半分に。"), (5, "転倒。格闘回避と機動回避にSR1/4、【MV】が半分に。"), (6, "転倒。格闘回避と機動回避にSR1/4、【MV】が半分に。"), (7, "武器が足下（0m離れたところ）に落ちる。素手のときは何もなし。"), (8, "武器が足下（0m離れたところ）に落ちる。素手のときは何もなし。"), (9, "武器が5m離れたところに落ちる。素手のときは関係ない。"), (10, "使用武器が壊れ、1シーン使用不可。") };
                    break;
                case "S":
                    name = "射撃／投擲アクシデント表";
                    table = new (int, string)[] { (1, "ささいなミス。その攻撃は失敗する。"), (2, "ささいなミス。その攻撃は失敗する。"), (3, "ささいなミス。その攻撃は失敗する。"), (4, "射撃武器はジャム。投擲武器ならば武器が取り出せないなど、マイナーアクションを消費しなければその武器を使用できない。"), (5, "射撃武器はジャム。投擲武器ならば武器が取り出せないなど、マイナーアクションを消費しなければその武器を使用できない。"), (6, "射撃武器はジャム。投擲武器ならば武器が取り出せないなど、マイナーアクションを消費しなければその武器を使用できない。"), (7, "故障。メジャーアクションで【DEX】のSR1のロールに成功しなければ、その武器を使用できない。"), (8, "故障。メジャーアクションで【DEX】のSR1のロールに成功しなければ、その武器を使用できない。"), (9, "破壊。以後、その武器は使用できない。"), (10, "武器の暴発。固定火力100のダメージを、装甲値無視で武器を持っていた腕（両手なら両手）、または兵装・貨物に受ける。") };
                    break;
                case "M":
                    name = "心理攻撃アクシデント表";
                    table = new (int, string)[] { (1, "集中失敗。攻撃は失敗する。"), (2, "集中失敗。攻撃は失敗する。"), (3, "集中失敗。攻撃は失敗する。"), (4, "思考ノイズ。クリンナップまですべてのリアクションにSR1/2。"), (5, "思考ノイズ。クリンナップまですべてのリアクションにSR1/2。"), (6, "思考ノイズ。クリンナップまですべてのリアクションにSR1/2。"), (7, "EXの暴走。頭部に装甲値無視、固定火力60のダメージを受ける。"), (8, "EXの暴走。頭部に装甲値無視、固定火力60のダメージを受ける。"), (9, "感情暴走。攻撃に使用したマニューバが1シーン使用不可。"), (10, "トラウマの再現。装甲値無視、固定火力100の心理ダメージを受ける。") };
                    break;
                case "E":
                    name = "電子攻撃アクシデント表";
                    table = new (int, string)[] { (1, "ショック。攻撃は失敗する。"), (2, "ショック。攻撃は失敗する。"), (3, "ショック。攻撃は失敗する。"), (4, "ノイズ発生。クリンナップまで電子攻撃のリアクションにSR1/2。"), (5, "ノイズ発生。クリンナップまで電子攻撃のリアクションにSR1/2。"), (6, "ノイズ発生。クリンナップまで電子攻撃のリアクションにSR1/2。"), (7, "ソフトウェア障害。攻撃に使用したソフトが1シーン使用不可。"), (8, "ソフトウェア障害。攻撃に使用したソフトが1シーン使用不可。"), (9, "ハードウェア障害。装甲値無視、固定火力80の電子ダメージを受ける。"), (10, "信号逆流。装甲値無視、固定火力100の心理ダメージを受ける。") };
                    break;
                default:
                    return null;
            }
            return GetMetalHeadExtream1d10TableResult(name, table, roc, randomizer);
        }

        private string? GetMechanicAccidentTable(string locationType, int roc, int correction, IRandomizer randomizer)
        {
            string name;
            (int, string)[] table;
            switch (locationType)
            {
                case "A":
                    name = "空中メカニック事故表";
                    table = new (int, string)[] { (3, "兵装／貨物。メカニックが装備している一番ENCの大きい武器ひとつが戦闘終了時まで使用不能になる。武器がない場合はメカニックオプションが使用不能になり、それもない場合は一番ENCの重い貨物（乗客をのぞく）が失われる。"), (6, "操作不能。メカニック本体にMWダメージ。操縦者は適切な［メカニック］スキルでSR1/4のロールを行い、成功したら体勢を立て直せる。失敗した場合、次のクリンナッププロセスまで、回避をふくめた一切の行動を取ることができない。"), (8, "不時着。メカニック本体にHWダメージ。次のクリンナッププロセスまで、回復をふくめた一切の行動を取ることができない。"), (9, "墜落。メカニック本体にMOダメージ。すべての乗員は、墜落のショックによってランダムな部位に〈物〉155の固定ダメージを受ける。このダメージは機動回避可能である。"), (10, "爆発。メカニックが爆発し、完全に破壊される。すべての乗員は、爆発と落下によって胴体に〈熱〉205の固定ダメージを受ける。このダメージは機動回避可能だが、SRに1/4の修正がある。") };
                    break;
                case "S":
                    name = "水上／水中メカニック事故表";
                    table = new (int, string)[] { (3, "横揺れ。次のクリンナッププロセスまで、このメカニックに乗っているキャラクターの行うすべての［メカニック］ロールに1/2の修正が与えられる。"), (6, "兵装／貨物。メカニックが装備している一番ENCの大きい武器ひとつが戦闘終了時まで使用不能になる。武器がない場合はメカニックオプションが使用不能になり、それもない場合は一番ENCの重い貨物（乗客をのぞく）が失われる。"), (8, "横転。メカニック本体にMWダメージ。操縦者は適切な［メカニック］スキルでSR1/4のロールを行い、成功したら体勢を立て直せる。失敗した場合、次のクリンナッププロセスまで、回避をふくめた一切の行動を取ることができない。"), (9, "激突。メカニック本体に〈物〉255の固定ダメージ。"), (10, "爆発。メカニックが爆発し、完全に破壊される。すべての乗員は、爆発によって胴体に〈熱〉155の固定ダメージを受ける。このダメージは機動回避可能だが、SRに1/4の修正がある。") };
                    break;
                case "L":
                    name = "地上メカニック事故表";
                    table = new (int, string)[] { (3, "接触。メカニック本体にLWダメージ。"), (6, "兵装／貨物。メカニックが装備している一番ENCの大きい武器ひとつが戦闘終了時まで使用不能になる。武器がない場合はメカニックオプションが使用不能になり、それもない場合は一番ENCの重い貨物（乗客をのぞく）が失われる。"), (8, "スピン。メカニック本体にMWダメージ。操縦者は適切な［メカニック］スキルでSR1/4のロールを行い、成功したら体勢を立て直せる。失敗した場合、次のクリンナッププロセスまで、回避をふくめた一切の行動を取ることができない。"), (9, "激突。メカニック本体に〈物〉255の固定ダメージ。次のクリンナッププロセスまで、回避をふくめた一切の行動を取ることができない。"), (10, "爆発。メカニックが爆発し、完全に破壊される。すべての乗員は、爆発によって胴体に〈熱〉155の固定ダメージを受ける。このダメージは機動回避可能だが、SRに1/4の修正がある。") };
                    break;
                default:
                    return null;
            }
            var dice = GetRocDice(roc, 10, randomizer);
            var diceText = dice.ToString();
            dice += correction;
            if (dice > 10)
            {
                dice = 10;
            }
            if (correction > 0)
            {
                diceText = $"{dice}[{diceText}+{correction}]";
            }
            var tableText = GetTableByNumber(dice, table);
            var text = $"{name}({diceText}) ＞ {tableText}";
            return text;
        }

        private string GetStrategyEventChart(IRandomizer randomizer)
        {
            var name = "ストラテジーイベントチャート";
            var table = new (int, string)[] { (50, "特に何事もなかった。"), (53, "スコール。種別：レーザーを装備している部隊の戦力はこのターン半減する。この効果は重複しない。"), (55, "ただよう不安。味方ユニットはWILのAR1を行い、失敗すると士気の10%を失う。"), (57, "狙撃！　司令官キャラクターは胴体に〈物〉155点の固定ダメージを受ける。機動回避は可能。"), (60, "敵の猛烈な反撃！　味方ユニットはREFのAR1を行い、失敗するとこのターン、移動力がマイナス1。"), (63, "敵弾幕の隙を見いだす。このターン、味方ユニットは突破判定がSR2に。"), (65, "突破のチャンス。このターン、味方ユニットは移動力が1点上昇する。"), (67, "士気高揚。味方ユニットの士気がそれぞれ現在値の10%だけ回復する。"), (70, "敵陣崩壊。敵ユニットの中で士気がもっとも低いユニットが戦場から撤退する。複数いた場合、すべて撤退。PC、ゲストには効果なし。"), (73, "大声援。戦闘がどこかのハッカーによって衛星中継され、喝采を浴びる。"), (75, "雨／雪。種別；レーザーを部隊の戦力はこのターン半減する。この効果は重複しない。"), (77, "磁気嵐。このターン、種別：ミサイルは戦力に数えず、突撃に使用することもできない。"), (80, "膠着した戦況。このターン、味方ユニットは突破判定がSR1/2に。"), (83, "メタルホッパー！　金属イナゴの襲来で視界をふさがれ、このラウンドは全てのMC射程が0となる。"), (85, "大竜巻！　飛行しているユニットの移動力は0となり、飛行ユニットはこのターン自分から突撃を行えない。"), (87, "通信の混乱。味方ユニットはINTのAR1を行い、失敗するとこのターン、移動力がマイナス1。"), (90, "幸運が微笑む。味方ユニットのラックポイントが1点ずつ回復。NPCには無効。"), (93, "致命的な狙撃！　司令官キャラクターは胴体に〈物〉205点の固定ダメージを受ける。機動回避は可能。"), (95, "敵の罠に落ちた。このターン、敵軍ユニットは移動力が1点上昇する。"), (97, "勝利の予感。味方ユニットの士気がそれぞれの現在値の20%だけ回復する。"), (99, "天変地異が襲いかかる！　このターン、すべてのユニットは移動できない。"), (100, "大混乱。後2回振る。") };
            return GetMetalHeadExtream1d100TableResult(name, table, 0, randomizer);
        }

        private string GetNpcAttackChart(IRandomizer randomizer)
        {
            var name = "NPC攻撃処理チャート";
            var table = new (int, string)[] { (5, "戦力の低い側だけが一方的に除去される。"), (8, "双方、一番戦力の少ないユニットひとつを除去する。"), (10, "戦力の高い側が最大戦力のユニットひとつを除去する。") };
            return GetMetalHeadExtream1d10TableResult(name, table, 0, randomizer);
        }

        private string GetLoserDestinyChart(IRandomizer randomizer)
        {
            var name = "敗者運命チャート";
            var table = new (int, string)[] { (1, "奇跡的に無傷で生き延びた。いずれ復讐の機会もあるだろう。"), (2, "ランダムな部位にLWを負う。"), (3, "戦力決定に使っていた武器が破壊される。"), (4, "ランダムな部位にMWを負う。"), (5, "外見に影響するような傷を負う。治療するなら$3000。"), (6, "ランダムな部位にHWを負う。"), (7, "着用している防具すべてが破壊される。衣服は壊れない。"), (8, "ランダムな部位にMOを負う。"), (9, "ランダムに決定した能力値ひとつを、永久に1点失う。"), (10, "残念ながら、君は死んでしまった。") };
            return GetMetalHeadExtream1d10TableResult(name, table, 0, randomizer);
        }

        private string? GetRandomEncounterTable(string locationType, int roc, IRandomizer randomizer)
        {
            string name;
            (int, string)[] table;
            switch (locationType)
            {
                case "W":
                    name = "荒野ランダムエンカウント表";
                    table = new (int, string)[] { (80, "特に遭遇は発生しなかった"), (85, "1d10名のバンデッド"), (87, "ヴェーダ・バウンサー1名に率いられた1d10+2（最低1）のヴェーダ・ソルジャー"), (89, "1d10+2体のウェーブコヨーテ"), (91, "1d10÷2体（最低1）のレーザーアント"), (93, "1d10-5体（最低1）のライトニングホーク"), (96, "1d10体のメタルホッパー"), (98, "1体のブラスビースト"), (100, "1d10÷3体（最低1）のサンドワーム") };
                    break;
                default:
                    return null;
            }
            return GetMetalHeadExtream1d100TableResult(name, table, roc, randomizer);
        }

        private string GetMetalHeadExtream1d10TableResult(string name, (int, string)[] table, int roc, IRandomizer randomizer)
        {
            return GetMetalHeadExtream1dxTableResult(name, table, roc, 10, randomizer);
        }

        private string GetMetalHeadExtream1d100TableResult(string name, (int, string)[] table, int roc, IRandomizer randomizer)
        {
            return GetMetalHeadExtream1dxTableResult(name, table, roc, 100, randomizer);
        }

        private string GetMetalHeadExtream1dxTableResult(string name, (int, string)[] table, int roc, int diceMax, IRandomizer randomizer)
        {
            var dice = GetRocDice(roc, diceMax, randomizer);
            var text = GetTableByNumber(dice, table);
            return $"{name}({dice}) ＞ {text}";
        }

        private int GetRocDice(int roc, int diceMax, IRandomizer randomizer)
        {
            var dice = roc;
            if (dice > diceMax)
            {
                dice = diceMax;
            }
            if (dice == 0)
            {
                dice = randomizer.RollOnce(diceMax);
            }
            return dice;
        }

        private double GetValue(double originalValue, string calculateText)
        {
            var result = originalValue;
            var text = calculateText ?? "";
            var calculateArray = Regex.Matches(text, @"[*/]\d*");
            foreach (Match m in calculateArray)
            {
                var opMatch = Regex.Match(m.Value, @"([*/])(\d*)");
                if (opMatch.Success)
                {
                    if (opMatch.Groups[1].Value == "*")
                    {
                        result *= int.Parse(opMatch.Groups[2].Value);
                    }
                    if (opMatch.Groups[1].Value == "/")
                    {
                        result /= int.Parse(opMatch.Groups[2].Value);
                    }
                }
            }
            return result;
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
