using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 初音ミクTRPG ココロダンジョン
    /// </summary>
    public sealed class HatsuneMiku : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly HatsuneMiku Instance = new HatsuneMiku();

        /// <inheritdoc/>
        public override string Id => "HatsuneMiku";

        /// <inheritdoc/>
        public override string Name => "初音ミクTRPG ココロダンジョン";

        /// <inheritdoc/>
        public override string SortKey => "はつねみくTRPGこころたんしよん";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定(Rx±y@z>=t)
        　能力値のダイスごとに成功・失敗の判定を行います。
        　x：能力ランク(S,A～D)。数字指定で直接その個数のダイスが振れます
        　y：修正値。A+2 あるいは A++ のように表記。混在時は A++,+1 のように記述も可能
        　z：スペシャル最低値（省略：6）　t：目標値（省略：4）
        　　例） RA　R2　RB+1　RC++　RD+,+2　RA>=5　RS-1@5>=6
        　結果はネイロを取得した残りで最大値を表示
        例） RB
        　HatsuneMiku : (RB>=4) ＞ [3,5] ＞
        　　ネイロに3(青)を取得した場合 5:成功
        　　ネイロに5(白)を取得した場合 3:失敗
        ・各種表
        　ファンブル表 FT／致命傷表 CWT／休憩表 BT／目標表 TT／関係表 RT
        　障害表 OT／リクエスト表 RQT／クロウル表 CLT／報酬表 RWT／悪夢表 NMT／情景表 ST
        ・キーワード表
        　ダーク DKT／ホット HKT／ラブ LKT／エキセントリック EKT／メランコリー MKT
        ・名前表 NT
        　コア別　ダーク DNT／ホット HNT／ラブ LNT／エキセントリック ENT／メランコリー MNT
        ・オトダマ各種表
        　性格表A OPA／性格表B OPB／趣味表 OHT／外見表 OLT／一人称表 OIT／呼び名表 OYT
        　リアクション表 ORT／出会い表 OMT
        ";

        private static readonly Regex JudgeRegex = new Regex(
            @"^(R([A-DS]|\d+)([+\-\d,]*))(@(\d))?((>(=)?)([+\-\d]*))?(@(\d))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var text = JudgeRoll(command, randomizer);
            if (text != null)
            {
                return text;
            }

            return null;
        }

        private Result? JudgeRoll(string command, IRandomizer randomizer)
        {
            int dice = 0;
            int total = 0;
            string result = "";
            var m = JudgeRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var skillRank = m.Groups[2].Value;
            var modifyText = m.Groups[3].Value;
            var signOfInequality = m.Groups[7].Success ? m.Groups[7].Value : ">=";
            var targetText = m.Groups[9].Success && !string.IsNullOrEmpty(m.Groups[9].Value) ? m.Groups[9].Value : "4";

            string specialNumStr = null;
            if (m.Groups[5].Success && !string.IsNullOrEmpty(m.Groups[5].Value))
            {
                specialNumStr = m.Groups[5].Value;
            }
            if (specialNumStr == null && m.Groups[11].Success && !string.IsNullOrEmpty(m.Groups[11].Value))
            {
                specialNumStr = m.Groups[11].Value;
            }
            var specialNum = specialNumStr != null ? int.Parse(specialNumStr) : 6;
            var specialText = (specialNum == 6) ? "" : $"@{specialNum}";

            modifyText = GetChangedModifyText(modifyText);

            var commandText = $"R{skillRank}{modifyText}";

            var rankDiceList = new Dictionary<string, int> { { "S", 4 }, { "A", 3 }, { "B", 2 }, { "C", 1 }, { "D", 2 } };
            int diceCount;
            if (Regex.IsMatch(skillRank, @"^\d+$"))
            {
                diceCount = int.Parse(skillRank);
            }
            else
            {
                diceCount = rankDiceList.ContainsKey(skillRank.ToUpper()) ? rankDiceList[skillRank.ToUpper()] : 2;
            }

            var modify = string.IsNullOrEmpty(modifyText) ? 0 : ArithmeticEvaluator.Eval(modifyText, RoundType.Floor) ?? 0;
            var target = ArithmeticEvaluator.Eval(targetText, RoundType.Floor) ?? 4;

            var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            var diceText = string.Join(",", diceList);

            if (skillRank.ToUpper() == "D")
            {
                diceList = new List<int> { diceList.Min() };
            }

            var message = $"({commandText}{specialText}{signOfInequality}{targetText}) ＞ [{diceText}]{modifyText} ＞ ";

            if (diceList.Count <= 1)
            {
                dice = diceList.First();
                total = dice + modify;
                result = CheckSuccess(total, dice, signOfInequality, target, specialNum);
                message += $"{total}:{result}";
            }
            else
            {
                var texts = new List<string>();
                for (var index = 0; index < diceList.Count; index++)
                {
                    var pickupDice = diceList[index];
                    var rests = new List<int>(diceList);
                    rests.RemoveAt(index);
                    dice = rests.Max();
                    total = dice + modify;
                    result = CheckSuccess(total, dice, signOfInequality, target, specialNum);

                    var colorList = new[] { "黒", "赤", "青", "緑", "白", "任意" };
                    var color = colorList[pickupDice - 1];
                    texts.Add($"　ネイロに{pickupDice}({color})を取得した場合 {total}:{result}");
                }
                texts = texts.Distinct().ToList();
                message += "\n" + string.Join("\n", texts);
            }

            return Result.CreateBuilder(message).Build();
        }

        private string GetChangedModifyText(string text)
        {
            var modifyText = "";
            if (string.IsNullOrEmpty(text))
            {
                return modifyText;
            }

            var values = text.Split(',');
            foreach (var value in values)
            {
                switch (value)
                {
                    case "++":
                        modifyText += "+2";
                        break;
                    case "+":
                        modifyText += "+1";
                        break;
                    default:
                        modifyText += value;
                        break;
                }
            }
            return modifyText;
        }

        private string CheckSuccess(int totalN, int diceN, string signOfInequality, int diff, int specialN)
        {
            if (diceN == 1)
            {
                return "ファンブル";
            }
            if (diceN >= specialN)
            {
                return "スペシャル";
            }

            var cmpOp = Command.Normalize.ComparisonOperator(signOfInequality);
            var targetNum = diff;
            return CompareOperatorHelper(totalN, cmpOp, targetNum) ? "成功" : "失敗";
        }

        private static bool CompareOperatorHelper(int left, CompareOperator? op, int right) => op switch
        {
            CompareOperator.GreaterThanOrEqual => left >= right,
            CompareOperator.LessThanOrEqual => left <= right,
            CompareOperator.GreaterThan => left > right,
            CompareOperator.LessThan => left < right,
            CompareOperator.Equal => left == right,
            CompareOperator.NotEqual => left != right,
            _ => throw new ArgumentException($"Unknown operator: {op}")
        };
    }
}
