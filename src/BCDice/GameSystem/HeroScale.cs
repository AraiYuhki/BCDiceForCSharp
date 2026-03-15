using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 英雄の尺度
    /// </summary>
    public sealed class HeroScale : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly HeroScale Instance = new HeroScale();

        /// <inheritdoc/>
        public override string Id => "HeroScale";

        /// <inheritdoc/>
        public override string Name => "英雄の尺度";

        /// <inheritdoc/>
        public override string SortKey => "えいゆうのしやくと";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        同人TRPGシステム『英雄の尺度』用ダイスボット。
        基本ルールブック＋サプリメント対応。仮称は非対応。
        コマンド一覧は以下の通り。*添え字で内容は[]。†がついていたら添え字必須。
        5hs4 超越
        5hs4,b 肉体の超越
        5hs4,s,* 科学の超越[†達成値への加算値]
        5hs4,p 激情の超越
        4hs6 加護
        4hs6,s 選択の加護
        4hs6,p 安寧の加護
        4hs6,r 逆転の加護
        3hs8,*,* 契約[奉納の出目1,奉納の出目2]
        3hs8,a,*,* 享受の契約[†受諾出目1][†受諾出目2]
        3hs8,e,* 収奪の契約[†取得出目]
        3hs8,b 燃焼の契約
        3hs8,o,*,* 奉納の契約[奉納の出目1,奉納の出目2]
        2hs20 呪い
        2hs20,r 歪曲の呪い
        2hs20,c 崩壊の呪い
        2hs20,d 破滅の呪い
        3hs10 異物
        3hs10,i 模造の異物
        3hs10,m,* 混血の異物[追加振り基準出目（初期値10）]
        3hs10,b,* 彼方の異物[追加振り停止基準値（初期値666）]
        1hs60 報い
        1hs60,d 堕落の報い
        1hs60,o 忘却の報い
        1hs60,s,* 封印の報い[出目への係数]
        12hs2 同化
        12hs2,m,*,*,*,*,*,*,*,* 怪物の同化[*d2,*d4,*d6,*d8,*d10,*d12,*d20,*d60]
        12hs2,t,* [†2の枚数宣言]
        12hs2,c,* 法則の同化[†1の枚数宣言]
        1hs12 下位存在
        2hs12 中位存在
        2hs12,t 変遷の中位存在
        2hs12,c 偶然の中位存在
        2hs12,g,* 萌芽の上位存在[加算値]
        3hs12 上位存在
        3hs12,g 大神の上位存在
        3hs12,h 神性の上位存在
        3hs12,w 魔性の上位存在
        3hs12,m 悪意の上位存在
        3hs12,s,* 大罪の上位存在[†確定する目標値]
        3hs12,d 破壊の上位存在
        3hs12,a 懊悩の上位存在
        3hs12,o 試練の上位存在
        3hs12,c 創造の上位存在
        3hs12,e 元素の上位存在
        *hs* 乗算ロール
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var text = SelectOrigin(command, randomizer);
            if (text == null)
            {
                return null;
            }

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private string SelectOrigin(string command, IRandomizer randomizer)
        {
            var order = command.Split(',');
            if (order.Length == 0)
            {
                return null;
            }

            switch (order[0])
            {
                case "5HS4":
                    return OriginGreat(order, randomizer);
                case "4HS6":
                    return OriginProtection(order, randomizer);
                case "3HS8":
                    return OriginVow(order, randomizer);
                case "2HS20":
                    return OriginCurse(order, randomizer);
                case "3HS10":
                    return OriginStranger(order, randomizer);
                case "1HS60":
                    return OriginKarma(order, randomizer);
                case "12HS2":
                    return OriginAbsorption(order, randomizer);
                case "1HS12":
                    return OriginNormal(randomizer);
                case "2HS12":
                    return OriginUnique(order, randomizer);
                case "3HS12":
                    return OriginOmnipotent(order, randomizer);
                default:
                {
                    // Rpartition("HS") equivalent: split on last occurrence of "HS"
                    var raw = order[0];
                    var hsIndex = raw.LastIndexOf("HS", StringComparison.Ordinal);
                    if (hsIndex < 0)
                    {
                        return null;
                    }

                    var diceLeft = raw.Substring(0, hsIndex);
                    var diceRight = raw.Substring(hsIndex + 2);
                    if (diceLeft.Length > 0 && diceRight.Length > 0 &&
                        Regex.IsMatch(diceLeft, @"^\d+$") && Regex.IsMatch(diceRight, @"^\d+$"))
                    {
                        var naturalResult = randomizer.RollBarabara(Convert.ToInt32(diceLeft), Convert.ToInt32(diceRight)).ToList();
                        var total = ResultsMultiplication(naturalResult);
                        var message = $"{raw} ＞ {total}[{string.Join(",", naturalResult)}]";
                        return message;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private string OriginGreat(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(5, 4).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "P":
                        message = FatePassion(naturalResult);
                        break;
                    case "S":
                        message = FateScience(naturalResult, order);
                        break;
                    case "B":
                        message = FateBody(naturalResult, randomizer);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"超越 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"超越 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FatePassion(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            var numberOfOne = naturalResult.Count(x => x == 1);
            foreach (var result in naturalResult)
            {
                modifiedResult.Add(result + numberOfOne);
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"激情の超越 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateScience(List<int> naturalResult, string[] order)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                if (Convert.ToInt32(order[2]) < 1024)
                {
                    var total = subtotal + Convert.ToInt32(order[2]);
                    message = $"科学の超越 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
                    if (total > 1023)
                    {
                        message += "(科学臨界)";
                    }
                }
                else
                {
                    message = "エラー：科学力が1024を超えています。";
                }
            }
            else
            {
                message = "エラー：科学力を設定してください。";
            }

            return message;
        }

        private string FateBody(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            foreach (var result in naturalResult)
            {
                if (result == 4)
                {
                    modifiedResult.Add(randomizer.RollOnce(4));
                }
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"肉体の超越 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string OriginProtection(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(4, 6).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "R":
                        message = FateReversal(naturalResult);
                        break;
                    case "P":
                        message = FatePeace(naturalResult);
                        break;
                    case "S":
                        message = FateChoice(naturalResult, randomizer);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"加護 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"加護 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateReversal(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            foreach (var result in naturalResult)
            {
                if (result < 4)
                {
                    modifiedResult.Add(7 - result);
                }
                else
                {
                    modifiedResult.Add(result);
                }
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"逆転の加護 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FatePeace(List<int> naturalResult)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal + 250;
            var message = $"安寧の加護 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            return message;
        }

        private string FateChoice(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.AddRange(randomizer.RollBarabara(3, 6));
            var evenTotal = 1;
            var oddTotal = 1;
            foreach (var result in modifiedResult)
            {
                if (result % 2 == 0)
                {
                    evenTotal *= result;
                }
                else
                {
                    oddTotal *= result;
                }
            }

            int total;
            if (evenTotal > oddTotal)
            {
                total = evenTotal;
            }
            else
            {
                total = oddTotal;
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var message = $"選択の加護 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string OriginVow(string[] order, IRandomizer randomizer)
        {
            object subtotal = null;
            var naturalResult = randomizer.RollBarabara(3, 8).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "O":
                        message = FateOffering(naturalResult, order);
                        break;
                    case "B":
                        message = FateBurning(naturalResult);
                        break;
                    case "E":
                        message = FateExploitation(naturalResult, order);
                        break;
                    case "A":
                        message = FateAcceptance(naturalResult, order);
                        break;
                    default:
                    {
                        subtotal = ResultsMultiplication(naturalResult);
                        if (order.Length > 2 && Regex.IsMatch(order[1], @"^\d+$") && Regex.IsMatch(order[2], @"^\d+$"))
                        {
                            var modifiedResult = naturalResult.ToList();
                            modifiedResult.Add(Convert.ToInt32(order[1]));
                            modifiedResult.Add(Convert.ToInt32(order[2]));
                            var total = ResultsMultiplication(modifiedResult);
                            message = $"契約 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
                        }
                        else
                        {
                            message = $"契約 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
                        }

                        break;
                    }
                }
            }
            else
            {
                subtotal = ResultsMultiplication(naturalResult);
                message = $"契約 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateOffering(List<int> naturalResult, string[] order)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            string message;
            if (order.Length > 3 && Regex.IsMatch(order[2], @"^\d+$") && Regex.IsMatch(order[3], @"^\d+$"))
            {
                var modifiedResult = naturalResult.ToList();
                modifiedResult.Add(Convert.ToInt32(order[2]));
                modifiedResult.Add(Convert.ToInt32(order[3]));
                var total = ResultsMultiplication(modifiedResult);
                message = $"奉納の契約 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            }
            else
            {
                message = $"奉納の契約 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
            }

            // Ruby: offering_result = natural_result.dup; offering_result.sort!.reverse!.shift(1)
            var offeringResult = naturalResult.ToList();
            offeringResult.Sort();
            offeringResult.Reverse();
            offeringResult.RemoveAt(0);
            message += $"(奉納：{string.Join(",", offeringResult)})";
            return message;
        }

        private string FateBurning(List<int> naturalResult)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal * 6;
            var message = $"燃焼の契約 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            return message;
        }

        private string FateExploitation(List<int> naturalResult, string[] order)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                var modifiedResult = naturalResult.ToList();
                modifiedResult[modifiedResult.IndexOf(modifiedResult.Min())] = Convert.ToInt32(order[2]);
                var total = ResultsMultiplication(modifiedResult);
                message = $"収奪の契約 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            }
            else
            {
                message = "エラー：収奪数を指定してください。";
            }

            return message;
        }

        private string FateAcceptance(List<int> naturalResult, string[] order)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            string message;
            if (order.Length > 3 && Regex.IsMatch(order[2], @"^\d+$") && Regex.IsMatch(order[3], @"^\d+$"))
            {
                // Ruby: change_result = natural_result.min(2) — returns the 2 smallest elements
                var changeResult = naturalResult.OrderBy(x => x).Take(2).ToList();
                var modifiedResult = naturalResult.ToList();
                modifiedResult[modifiedResult.IndexOf(changeResult[0])] = Convert.ToInt32(order[2]);
                modifiedResult[modifiedResult.IndexOf(changeResult[1])] = Convert.ToInt32(order[3]);
                var total = ResultsMultiplication(modifiedResult);
                message = $"享受の契約 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            }
            else
            {
                message = "エラー：享受数を指定してください。";
            }

            return message;
        }

        private string OriginCurse(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(2, 20).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "R":
                        message = FateRuin(naturalResult, randomizer);
                        break;
                    case "C":
                        message = FateCollapse(naturalResult);
                        break;
                    case "D":
                        message = FateDistortion(naturalResult);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"呪い ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"呪い ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateRuin(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.AddRange(randomizer.RollBarabara(2, 20));
            var total = 1;
            foreach (var result in modifiedResult)
            {
                if (result > 10)
                {
                    total *= result;
                }
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var message = $"破滅の呪い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateCollapse(List<int> naturalResult)
        {
            var modifiedResult = naturalResult.ToList();
            var collapseResult = ResultRoundup(naturalResult[naturalResult.IndexOf(naturalResult.Max())]);
            if (modifiedResult[0] == modifiedResult[1])
            {
                modifiedResult[0] = collapseResult;
                modifiedResult[1] = collapseResult;
                modifiedResult.Add(collapseResult);
                modifiedResult.Add(collapseResult);
            }
            else
            {
                var maxIndex = naturalResult.IndexOf(naturalResult.Max());
                modifiedResult[maxIndex] = collapseResult;
                modifiedResult.Insert(maxIndex, collapseResult);
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"崩壊の呪い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateDistortion(List<int> naturalResult)
        {
            var modifiedResult = naturalResult.ToList();
            if (modifiedResult[0] == modifiedResult[1])
            {
                modifiedResult[0] += 13;
                modifiedResult[1] += 13;
            }
            else
            {
                modifiedResult[naturalResult.IndexOf(naturalResult.Min())] += 13;
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"歪曲の呪い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string OriginStranger(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var rawResult = randomizer.RollBarabara(3, 10);
            var naturalResult = StrangerEffection(rawResult.ToList());
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "I":
                        message = FateImitation(naturalResult);
                        break;
                    case "M":
                        message = FateMixed(naturalResult, order, randomizer);
                        break;
                    case "B":
                        message = FateBeyond(naturalResult, order, randomizer);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"異物 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"異物 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateImitation(List<int> naturalResult)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.Sort();
            var combined = modifiedResult[0] + modifiedResult[1] * 10;
            modifiedResult[0] = (combined == 0) ? 100 : combined;
            modifiedResult[1] = modifiedResult[2];
            modifiedResult.RemoveAt(2);
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"模造の異物 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateMixed(List<int> naturalResult, string[] order, IRandomizer randomizer)
        {
            object total = null;
            var subtotal = ResultsMultiplication(naturalResult);
            int mixedScore;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                mixedScore = Convert.ToInt32(order[2]);
            }
            else
            {
                mixedScore = 1;
            }

            var modifiedResult = naturalResult.ToList();
            string message;
            if (mixedScore <= naturalResult.Min())
            {
                modifiedResult.Add(randomizer.RollOnce(12));
                total = ResultsMultiplication(modifiedResult);
                message = $"混血の異物 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}](追加振り)";
            }
            else
            {
                modifiedResult[naturalResult.IndexOf(naturalResult.Min())] = 10;
                total = ResultsMultiplication(modifiedResult);
                message = $"混血の異物 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}](10置換)";
            }

            return message;
        }

        private string FateBeyond(List<int> naturalResult, string[] order, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal;
            var beyondLimit = 666;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$") && Convert.ToInt32(order[2]) < 666)
            {
                beyondLimit = Convert.ToInt32(order[2]);
            }

            while (total != 0 && total <= beyondLimit)
            {
                modifiedResult.Add(randomizer.RollD9());
                total = ResultsMultiplication(modifiedResult);
            }

            var message = $"彼方の異物 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string OriginKarma(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(1, 60).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "D":
                        message = FateDepravity(naturalResult);
                        break;
                    case "O":
                        message = FateOblivion(naturalResult, randomizer);
                        break;
                    case "S":
                        message = FateSealed(naturalResult, order);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"報い ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"報い ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateDepravity(List<int> naturalResult)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var modifiedResult = naturalResult.ToList();
            var depravityNum1 = naturalResult[0] % 10;
            var depravityNum10 = naturalResult[0] / 10;
            if (depravityNum10 > 1)
            {
                modifiedResult.Add(depravityNum10);
            }

            if (depravityNum1 > 1)
            {
                modifiedResult.Add(depravityNum1);
            }

            var total = ResultsMultiplication(modifiedResult);
            var message = $"堕落の報い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateOblivion(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.Add(randomizer.RollOnce(60));
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult) / 2;
            var message = $"忘却の報い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateSealed(List<int> naturalResult, string[] order)
        {
            var modifiedResult = naturalResult.ToList();
            var subtotal = ResultsMultiplication(naturalResult);
            var sealedBreak = 1;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                sealedBreak = Convert.ToInt32(order[2]);
            }

            modifiedResult[0] *= sealedBreak;
            var total = subtotal * sealedBreak;
            var message = $"封印の報い ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            if (total <= 30)
            {
                sealedBreak *= 4;
                message += $"(封印解除成功：{sealedBreak})";
            }
            else if (total <= 60)
            {
                sealedBreak *= 2;
                message += $"(封印解除成功：{sealedBreak})";
            }
            else
            {
                message += $"(封印解除失敗：{sealedBreak})";
            }

            return message;
        }

        private string OriginAbsorption(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(12, 2).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "M":
                        message = FateMonster(naturalResult, order, randomizer);
                        break;
                    case "T":
                        message = FateTreasure(naturalResult, order);
                        break;
                    case "C":
                        message = FateConcept(naturalResult, order);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"同化 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"同化 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateMonster(List<int> _naturalResult, string[] order, IRandomizer randomizer)
        {
            var modifiedResult = new List<int>();
            string message;
            if (order.Length > 9 &&
                Regex.IsMatch(order[2], @"^\d+$") && Regex.IsMatch(order[3], @"^\d+$") &&
                Regex.IsMatch(order[4], @"^\d+$") && Regex.IsMatch(order[5], @"^\d+$") &&
                Regex.IsMatch(order[6], @"^\d+$") && Regex.IsMatch(order[7], @"^\d+$") &&
                Regex.IsMatch(order[8], @"^\d+$") && Regex.IsMatch(order[9], @"^\d+$"))
            {
                if (Convert.ToInt32(order[2]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[2]), 2));
                }

                if (Convert.ToInt32(order[3]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[3]), 4));
                }

                if (Convert.ToInt32(order[4]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[4]), 6));
                }

                if (Convert.ToInt32(order[5]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[5]), 8));
                }

                var countOf10 = Convert.ToInt32(order[6]);
                while (countOf10 > 0)
                {
                    modifiedResult.Add(randomizer.RollD9());
                    countOf10 -= 1;
                }

                if (Convert.ToInt32(order[7]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[7]), 12));
                }

                if (Convert.ToInt32(order[8]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[8]), 20));
                }

                if (Convert.ToInt32(order[9]) > 0)
                {
                    modifiedResult.AddRange(randomizer.RollBarabara(Convert.ToInt32(order[9]), 60));
                }

                var total = ResultsMultiplication(modifiedResult);
                var subtotal = modifiedResult.Sum();
                message = $"怪物の同化 ＞ {total}[{string.Join(",", modifiedResult)}] 浸蝕値：{subtotal}";
                if (new[] { 2, 4, 6, 8, 10, 12, 20, 60 }.Contains(subtotal))
                {
                    message += "(変異進行)";
                }

                if (modifiedResult.Contains(1))
                {
                    message += "(人間性喪失)";
                }
            }
            else
            {
                message = "エラー：変異状態を指定してください。";
            }

            return message;
        }

        private string FateTreasure(List<int> naturalResult, string[] order)
        {
            var modifiedResult = naturalResult.ToList();
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal;
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                var treasurePoint = Convert.ToInt32(order[2]);
                if (modifiedResult.Count(x => x == 2) >= treasurePoint)
                {
                    total *= treasurePoint;
                    message = $"秘宝の同化 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}(同調成功)";
                }
                else
                {
                    message = $"秘宝の同化 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}(同調失敗)";
                }
            }
            else
            {
                message = "エラー：解放率を指定してください。";
            }

            return message;
        }

        private string FateConcept(List<int> naturalResult, string[] order)
        {
            var modifiedResult = naturalResult.ToList();
            var subtotal = ResultsMultiplication(naturalResult);
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                var existenceScale = Convert.ToInt32(order[2]);
                if (existenceScale > 12)
                {
                    existenceScale = 12;
                }

                // Ruby: modified_result.fill(2, 0, existence_scale)
                // fills elements from index 0 to existence_scale-1 with value 2
                for (var i = 0; i < existenceScale && i < modifiedResult.Count; i++)
                {
                    modifiedResult[i] = 2;
                }

                var total = ResultsMultiplication(modifiedResult);
                message = $"概念の同化 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            }
            else
            {
                message = "エラー：事象強度を指定してください。";
            }

            return message;
        }

        private string OriginNormal(IRandomizer randomizer)
        {
            var naturalResult = randomizer.RollBarabara(1, 12).ToList();
            var total = ResultsMultiplication(naturalResult);
            var message = $"下位存在 ＞ {total}[{string.Join(",", naturalResult)}]";
            return message;
        }

        private string OriginUnique(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(2, 12).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "G":
                        message = FateGrowth(naturalResult, order);
                        break;
                    case "T":
                        message = FateTransition(naturalResult, randomizer);
                        break;
                    case "C":
                        message = FateChance(naturalResult);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"中位存在 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"中位存在 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateGrowth(List<int> naturalResult, string[] order)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal;
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                total += Convert.ToInt32(order[2]);
                message = $"萌芽の中位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            }
            else
            {
                message = $"萌芽の中位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
            }

            var growthLevel = Convert.ToInt32(order[2]) + 50;
            message += $"(成長段階：{growthLevel})";
            return message;
        }

        private string FateTransition(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.Add(randomizer.RollD9());
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"変遷の中位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateChance(List<int> naturalResult)
        {
            var modifiedResult = naturalResult.ToList();
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            string message;
            if (modifiedResult[0] == modifiedResult[1])
            {
                total *= 24;
                message = $"偶然の中位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            }
            else
            {
                message = $"偶然の中位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string OriginOmnipotent(string[] order, IRandomizer randomizer)
        {
            object total = null;
            var naturalResult = randomizer.RollBarabara(3, 12).ToList();
            string message;
            if (order.Length > 1)
            {
                switch (order[1])
                {
                    case "G":
                        message = FateGod(naturalResult);
                        break;
                    case "H":
                        message = FateHoly(naturalResult);
                        break;
                    case "W":
                        message = FateWicked(naturalResult);
                        break;
                    case "M":
                        message = FateMalice(naturalResult);
                        break;
                    case "S":
                        message = FateSin(naturalResult, order, randomizer);
                        break;
                    case "D":
                        message = FateDestruction(naturalResult, randomizer);
                        break;
                    case "A":
                        message = FateAnguish(naturalResult);
                        break;
                    case "O":
                        message = FateOrdeal(naturalResult);
                        break;
                    case "C":
                        message = FateCreation(naturalResult);
                        break;
                    case "E":
                        message = FateElement(randomizer);
                        break;
                    default:
                    {
                        total = ResultsMultiplication(naturalResult);
                        message = $"上位存在 ＞ {total}[{string.Join(",", naturalResult)}]";
                        break;
                    }
                }
            }
            else
            {
                total = ResultsMultiplication(naturalResult);
                message = $"上位存在 ＞ {total}[{string.Join(",", naturalResult)}]";
            }

            return message;
        }

        private string FateGod(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            foreach (var result in naturalResult)
            {
                modifiedResult.Add(result + 3);
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"大神の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateHoly(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            foreach (var result in naturalResult)
            {
                modifiedResult.Add(result + 1);
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"神性の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateWicked(List<int> naturalResult)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal + 120;
            var message = $"魔性の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            return message;
        }

        private string FateMalice(List<int> naturalResult)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal * 2;
            var message = $"悪意の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}";
            return message;
        }

        private string FateSin(List<int> naturalResult, string[] order, IRandomizer randomizer)
        {
            var subtotal = ResultsMultiplication(naturalResult);
            var total = subtotal;
            string message;
            if (order.Length > 2 && Regex.IsMatch(order[2], @"^\d+$"))
            {
                message = $"大罪の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}]";
                var sinWeight = Convert.ToInt32(order[2]);
                var sinCount = 0;
                while (sinCount < 3 && total < sinWeight)
                {
                    var modifiedResult = randomizer.RollBarabara(3, 12).ToList();
                    total = ResultsMultiplication(modifiedResult);
                    message += $" ＞ {total}[{string.Join(",", modifiedResult)}]";
                    sinCount += 1;
                }
            }
            else
            {
                message = "エラー：罪の重さを指定してください。";
            }

            return message;
        }

        private string FateDestruction(List<int> naturalResult, IRandomizer randomizer)
        {
            var modifiedResult = naturalResult.ToList();
            modifiedResult.Add(randomizer.RollOnce(12));
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult) - 300;
            var message = $"破壊の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateAnguish(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            foreach (var result in naturalResult)
            {
                if (result < 7)
                {
                    modifiedResult.Add(7);
                }
                else
                {
                    modifiedResult.Add(result);
                }
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"懊悩の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateOrdeal(List<int> naturalResult)
        {
            var modifiedResult = new List<int>();
            foreach (var result in naturalResult)
            {
                if (result < 9)
                {
                    modifiedResult.Add(9);
                }
                else
                {
                    modifiedResult.Add(result);
                }
            }

            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"試練の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateCreation(List<int> naturalResult)
        {
            var modifiedResult = naturalResult.ToList();
            var temporaryResult = naturalResult.ToList();
            temporaryResult.Sort();
            modifiedResult.Add(temporaryResult[1]);
            var subtotal = ResultsMultiplication(naturalResult);
            var total = ResultsMultiplication(modifiedResult);
            var message = $"創造の上位存在 ＞ {subtotal}[{string.Join(",", naturalResult)}] ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private string FateElement(IRandomizer randomizer)
        {
            var modifiedResult = new List<int>();
            modifiedResult.Add(randomizer.RollOnce(4));
            modifiedResult.Add(randomizer.RollOnce(6));
            modifiedResult.Add(randomizer.RollOnce(8));
            modifiedResult.Add(randomizer.RollOnce(12));
            modifiedResult.Add(randomizer.RollOnce(20));
            var total = ResultsMultiplication(modifiedResult);
            var message = $"元素の上位存在 ＞ {total}[{string.Join(",", modifiedResult)}]";
            return message;
        }

        private int ResultsMultiplication(List<int> resultList)
        {
            var total = 1;
            foreach (var result in resultList)
            {
                total *= result;
            }

            return total;
        }

        private List<int> StrangerEffection(List<int> resultList)
        {
            var strangerList = new List<int>();
            foreach (var result in resultList)
            {
                strangerList.Add(result - 1);
            }

            return strangerList;
        }

        private int ResultRoundup(int result)
        {
            if (result % 2 == 0)
            {
                return result / 2;
            }
            else
            {
                return result / 2 + 1;
            }
        }
    }
}
