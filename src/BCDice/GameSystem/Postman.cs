using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 壊れた世界のポストマン
    /// </summary>
    public sealed class Postman : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Postman Instance = new Postman();


        private static readonly Regex DPoDRegex = new Regex(
            @"(\d+)?PO(\d+)?(([+-]\d+)*)?((>|>=|@)(\d+)(([+-]\d+)*)?)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WeaDRegex = new Regex(
            @"WEA(\d+)?",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "Postman";

        /// <inheritdoc/>
        public override string Name => "壊れた世界のポストマン";

        /// <inheritdoc/>
        public override string SortKey => "こわれたせかいのほすとまん";

        /// <inheritdoc/>
        public bool SortAddDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定：[n]PO[+-a][> or >= or @X]　　[]内省略可。

        達成値と判定の成否、クリティカル、ファンブルを結果表示します。
        「n」でダイス数を指定。省略時は2D。
        「+-a」で達成値への修正を指定。「+2+1-4」のような複数回指定可。
        「>X」「>=X」「@X」で難易度を指定可。
        「>X」は達成値>難易度、「>=X」「@X」は達成値>=難易度で判定します。

        【書式例】
        3PO+2-1 → 3Dで達成値修正+1の判定。達成値のみ表示。
        PO@5+2 → 2Dで目標値7の判定。判定の成否と達成値を表示。
        4PO-2+1>7+2 → 4Dで達成値修正-1、目標値9（同値は失敗）の判定。


        ◆天候チェック：WEA[n]　　[]内省略可。

        天候チェック表を参照します。
        「n」を指定すると、指定した結果を表示します。（【幸運点】使用時用）


        ◆自由行動シチュエーション表：FRE
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Result? result = null;
            var upperCommand = command.ToUpper();

            var match = DPoDRegex.Match(upperCommand);
            if (match.Success)
            {
                var diceCount = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 2;
                if (diceCount < 2)
                {
                    diceCount = 2;
                }
                var modify = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
                var modifyAddString = match.Groups[3].Success ? match.Groups[3].Value : "";
                var type = match.Groups[6].Success ? match.Groups[6].Value : "";
                var target = match.Groups[7].Success ? int.Parse(match.Groups[7].Value) : 0;
                var targetAddString = match.Groups[8].Success ? match.Groups[8].Value : "";

                var modifyMatches = Regex.Matches(modifyAddString, @"[+-]\d+");
                foreach (Match m in modifyMatches)
                {
                    modify += int.Parse(m.Value);
                }

                if (target != 0)
                {
                    var targetMatches = Regex.Matches(targetAddString, @"[+-]\d+");
                    foreach (Match m in targetMatches)
                    {
                        target += int.Parse(m.Value);
                    }
                }

                result = CheckRoll(diceCount, modify, type, target, randomizer);
                if (result != null)
                {
                    return result;
                }
            }

            match = WeaDRegex.Match(upperCommand);
            if (match.Success)
            {
                var roc = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                result = GetWeatherTable(roc, randomizer);
                if (result != null)
                {
                    return result;
                }
            }

            if (upperCommand == "FRE")
            {
                return GetFreeSituationTable(randomizer);
            }

            return null;
        }

        private Result? CheckRoll(int diceCount, int modify, string type, int target, IRandomizer randomizer)
        {
            var diceArray = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToList();
            var dice = diceArray.Sum();
            var diceText = string.Join(",", diceArray);
            var dice2 = diceArray[diceArray.Count - 2] + diceArray[diceArray.Count - 1];
            var diceText2 = $"{diceArray[diceArray.Count - 2]},{diceArray[diceArray.Count - 1]}";
            var criticalCount = diceArray.Count(x => x == 6);

            var modifyText = "";
            if (modify != 0)
            {
                modifyText = modify > 0 ? "+" : "";
                modifyText += modify.ToString();
            }

            var result = dice2 + modify;

            var resultText = "";
            var operatorText = "";
            if (type != "")
            {
                resultText = " 【失敗】";
                operatorText = ">";
                if (type == ">")
                {
                    if (result > target)
                    {
                        resultText = " 【成功】";
                    }
                }
                else
                {
                    operatorText += "=";
                    if (result >= target)
                    {
                        resultText = " 【成功】";
                    }
                }
            }

            if (criticalCount >= 2)
            {
                resultText = " 【成功】（クリティカル）";
            }
            else if (dice == diceCount)
            {
                resultText = " 【失敗】（ファンブル）";
            }

            var text = $"{diceCount}D6({diceText}){modifyText} ＞ {dice2}({diceText2}){modifyText} = 達成値：{result}";
            if (target > 0)
            {
                text += $"{operatorText}{target} ";
            }
            text += resultText;

            return Result.CreateBuilder(text).Build();
        }

        private Result? GetWeatherTable(int roc, IRandomizer randomizer)
        {
            var name = "天候チェック";
            var table = new (int Number, string Text)[] {
                (2, "大雨と強風。探索判定の難易度に+4。"),
                (3, "風が強い1日になりそう。探索判定の難易度に+2。"),
                (4, "晴れ。特になし。"),
                (5, "夜の間の雨でぬかるむ。探索判定の難易度に+2。"),
                (6, "それなりの雨足。探索判定の難易度に+2。"),
                (7, "晴れ。特になし。"),
                (8, "天気は大荒れ。探索判定の難易度に+4。"),
                (9, "小雨が降る。探索判定の難易度に+1。"),
                (10, "それなりの雨足。探索判定の難易度に+2。"),
                (11, "晴れ。特になし。"),
                (12, "風が強い1日になりそう。探索判定の難易度に+2。"),
            };

            int dice;
            string diceText;
            if (roc == 0)
            {
                var diceList = randomizer.RollBarabara(2, 6);
                dice = diceList.Sum();
                diceText = string.Join(",", diceList);
            }
            else
            {
                if (roc < 2) roc = 2;
                if (roc > 12) roc = 12;
                dice = roc;
                diceText = $"Choice:{roc}";
            }

            var tableText = table.FirstOrDefault(t => t.Number == dice).Text ?? "";
            var text = $"{name}({diceText}) ＞ {dice}：{tableText}";
            return Result.CreateBuilder(text).Build();
        }

        private Result? GetFreeSituationTable(IRandomizer randomizer)
        {
            var name = "自由行動シチュエーション表";
            var table = new (int Number, string Text)[] {
                (2, "何をするでもなく、霞がかったような夜空を見上げる。ふと隣に目を向ければ、彼/彼女が居た。彼/彼女は、こうなる前の夜空を知っているのだろうか。"),
                (3, "夢を見た。大戦の最中、街が、人が、世界が焼けていく悪夢を。追い立てられるようにして目を覚ますと、彼/彼女が君を見ていた。　……もしかして、自分はよほどうなされていたのだろうか。"),
                (4, "周囲で見つけたガラクタを使って、ちょっとしたビックリ玩具を作ってみた。「彼/ 彼女」にコイツをけしかけたら、どんな反応をするだろうか？"),
                (5, "使えそうなものがないか探していると、カタンと物音がして何かが落ちた。拾い上げてみたそれは、かつてここで生活していた誰かの名残（写真、家具、玩具等）だった。"),
                (6, "テントの中で夜を過ごしていると、ふと彼/彼女と話したくてたまらない気持ちになった。言ってしまえば、夜の静けさに寂しさを覚えてしまったのだ。"),
                (7, "ここまでの配達の記録をつけていたら、背後から視線を感じる……！　もしや、彼/彼女に覗かれている……！？"),
                (8, "周囲を探索していると、君一人では手の届かないところに金属製の箱か何かがあることに気づいた。彼/彼女に手伝ってもらえば、取れるだろうか……？"),
                (9, "朝まではまだしばらくあるというのに、目が覚めてしまった。二度寝しようにも寝付けずに居ると、隣でもぞもぞと動く気配がする。彼/彼女も、どうやら同じらしい。"),
                (10, "他愛のない話をするうちに、君は彼/彼女に問いかけていた。「何故、ポストマンになろうと思ったのか」　……そういえば、君自身はどうだったろうか。"),
                (11, "保存食にありつこうとしたその時、君は気づいた。一匹のネズミが、彼/彼女の荷物の中に潜り込もうとしている。彼/彼女は気づいていないが、このままでは食料が危ない！"),
                (12, "テントを設営し、落ち着いた頃にふと気づく。　……身体が熱い。少し、だるさもあるような気もする。大したことはないと思うが、彼/彼女に相談しておいた方がいいだろうか。"),
            };

            var diceList = randomizer.RollBarabara(2, 6);
            var dice = diceList.Sum();
            var diceText = string.Join(",", diceList);
            var tableText = table.FirstOrDefault(t => t.Number == dice).Text ?? "";
            var text = $"{name}({diceText}) ＞ {dice}：{tableText}";
            return Result.CreateBuilder(text).Build();
        }

    }
}
