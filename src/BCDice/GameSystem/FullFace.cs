using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// フルフェイス
    /// </summary>
    public sealed class FullFace : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly FullFace Instance = new FullFace();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "FullFace";

        /// <inheritdoc/>
        public override string Name => "フルフェイス";

        /// <inheritdoc/>
        public override string SortKey => "ふるふえいす";

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■判定　x+bFF<=a[,t][&d]   x:ヒート(省略時は3) b:判定修正 a:能力値 t:難易度(省略可) d:基本ダメージ(省略可)

        例)FF<=2:     能力値2で判定し、その結果(成功数,1の目の数,6の目の数,バースト)を表示。
           6FF<=3:    ヒート6,能力値3で戦闘判定し、その結果( 〃 )を表示。
           8+2FF<=3:  ヒート8,判定修正+2,能力値3で戦闘判定し、その結果( 〃 )を表示。
           FF<=2,1:   能力値2,難易度1で判定し、その結果(成功数,1の目の数,6の目の数,成功・失敗,バースト)を表示。
           6FF<=3,2&1:ヒート6,能力値3,難易度2,基本ダメージ1で戦闘判定し、その結果(成功数,1の目の数,6の目の数,ダメージ,バースト)を表示。

        ■ジャンク表　JKT

        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d*)([+\d]+)*FF<=(\d)(,(\d))?(&(\d))?$");
            if (!m.Success)
            {
                return null;
            }

            int heatLevel = Convert.ToInt32(m.Groups[1].Value);
            if (heatLevel == 0)
            {
                heatLevel = 3;
            }
            int modify = ArithmeticEvaluator.Eval($"0{m.Groups[2].Value}", this.RoundType) ?? 0;
            int statusNo = Convert.ToInt32(m.Groups[3].Value);
            int targetNo = Convert.ToInt32(m.Groups[5].Value);
            int damageNo = Convert.ToInt32(m.Groups[7].Value);

            var diceArrayParts = new System.Collections.Generic.List<string>();
            var dice = randomizer.RollBarabara(heatLevel, 6).OrderBy(x => x).ToList();
            int ones = dice.Count(v => v == 1);
            int sixs = dice.Count(v => v == 6);
            int successNum = dice.Count(val => val <= statusNo);
            diceArrayParts.Add(string.Join(",", dice));

            if (modify > 0)
            {
                var modDice = randomizer.RollBarabara(modify, 6).OrderBy(x => x).ToList();
                ones += modDice.Count(v => v == 1);
                successNum += modDice.Count(val => val <= statusNo);
                diceArrayParts.Add(string.Join(",", modDice));
            }

            int onesTotal = ones;
            while (ones > 0)
            {
                var rerollDice = randomizer.RollBarabara(ones, 6).OrderBy(x => x).ToList();
                ones = rerollDice.Count(v => v == 1);
                onesTotal = ones;
                successNum += rerollDice.Count(val => val <= statusNo);
                diceArrayParts.Add(string.Join(",", rerollDice));
            }

            bool isFumble = sixs >= 2;
            bool isSuccess;
            bool isCritical = onesTotal > 0;

            if (isFumble)
            {
                isSuccess = false;
            }
            else
            {
                isSuccess = successNum > 0;
            }

            string commandOut = $"({heatLevel}{Format.Modifier(modify)}FF<={statusNo}";

            var resultTxt = new System.Collections.Generic.List<string>();
            resultTxt.Add($"成功度({successNum})");
            if (onesTotal > 0)
            {
                resultTxt.Add($"1の目({onesTotal})");
            }
            if (sixs > 0)
            {
                resultTxt.Add($"6の目({sixs})");
            }
            if (targetNo > 0)
            {
                commandOut += $",{targetNo}";
                if (successNum >= targetNo)
                {
                    resultTxt.Add("成功");
                    isSuccess = true;
                }
                else
                {
                    resultTxt.Add("失敗");
                    isSuccess = false;
                }
            }
            if (damageNo > 0)
            {
                commandOut += $"&{damageNo}";
                int damage = damageNo + onesTotal;
                resultTxt.Add($"ダメージ({damage})");
            }
            if (isFumble)
            {
                resultTxt.Add("バースト");
            }
            commandOut += ")";

            var sequence = new[] { commandOut, string.Join("+", diceArrayParts), string.Join(",", resultTxt) };
            string text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetCondition(isSuccess)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

    }
}
