using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ロストロイヤル
    /// </summary>
    public sealed class LostRoyal : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly LostRoyal Instance = new LostRoyal();


        private static readonly Regex Lr05Regex = new Regex(
            @"LR\[([0-5]),([0-5]),([0-5]),([0-5]),([0-5]),([0-5])\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FcRegex = new Regex(
            @"FC",
            RegexOptions.Compiled);

        private static readonly Regex WpcRegex = new Regex(
            @"WPC",
            RegexOptions.Compiled);

        private static readonly Regex EcRegex = new Regex(
            @"EC",
            RegexOptions.Compiled);

        private static readonly Regex Hr12Regex = new Regex(
            @"HR([1-2])",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "LostRoyal";

        /// <inheritdoc/>
        public override string Name => "ロストロイヤル";

        /// <inheritdoc/>
        public override string SortKey => "ろすとろいやる";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・D66ダイスあり

        行為判定
        　LR[x,x,x,x,x,x]
        　　x の並びには【判定表】の数値を順番に入力する。
        　　（例： LR[1,3,0,1,2,3] ）

        ファンブル表
        　FC

        風力決定表
        　WPC

        感情決定表
        　EC

        希望点の決定
        　HRx
        　　x にはダイスの数（ 1 - 2 ）を指定
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = Lr05Regex.Match(command);
            if (match.Success)
            {
                var checkingTable = new int[]
                {
                    Convert.ToInt32(match.Groups[1].Value),
                    Convert.ToInt32(match.Groups[2].Value),
                    Convert.ToInt32(match.Groups[3].Value),
                    Convert.ToInt32(match.Groups[4].Value),
                    Convert.ToInt32(match.Groups[5].Value),
                    Convert.ToInt32(match.Groups[6].Value),
                };
                return CheckLostroyal(checkingTable, randomizer);
            }

            match = FcRegex.Match(command);
            if (match.Success)
            {
                return RollFumbleChart(randomizer);
            }

            match = WpcRegex.Match(command);
            if (match.Success)
            {
                return RollWindPowerChart(randomizer);
            }

            match = EcRegex.Match(command);
            if (match.Success)
            {
                return RollEmotionChart(randomizer);
            }

            match = Hr12Regex.Match(command);
            if (match.Success)
            {
                return RollHope(Convert.ToInt32(match.Groups[1].Value), randomizer);
            }

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? CheckLostroyal(int[] checkingTable, IRandomizer randomizer)
        {
            var keys = new List<int>();
            for (var i = 0; i < 3; i++)
            {
                var key = randomizer.RollOnce(6);
                keys.Add(key);
            }

            var scores = keys.Select(k => checkingTable[k - 1]).ToArray();
            var totalScore = scores.Sum();

            var chainedSequence = FindSequence(keys);

            var text = $"3D6 => [{string.Join(",", keys)}] => ({string.Join("+", scores)}) => {totalScore}";

            if (chainedSequence != null && chainedSequence.Count > 0)
            {
                var bonus = Fumble(keys, chainedSequence) ? 3 : chainedSequence.Count;
                text += $" | {chainedSequence.Count} chain! ({string.Join(",", chainedSequence)}) => {totalScore + bonus}";

                if (chainedSequence.Count >= 3)
                {
                    text += " [スペシャル]";
                }

                if (Fumble(keys, chainedSequence))
                {
                    text += " [ファンブル]";
                }
            }

            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private List<int>? FindSequence(List<int> keys)
        {
            var sortedKeys = keys.OrderBy(x => x).ToList();

            List<int>? best = null;
            for (int startKey = 1; startKey <= 5; startKey++)
            {
                var seq = FindSequenceFromStartKey(sortedKeys, startKey);
                if (seq.Count > 1)
                {
                    if (best == null || seq.Count > best.Count)
                    {
                        best = seq;
                    }
                }
            }

            return best;
        }

        private List<int> FindSequenceFromStartKey(List<int> keys, int startKey)
        {
            var chainedKeys = new List<int>();

            int key = startKey;
            while (keys.Contains(key))
            {
                chainedKeys.Add(key);
                key += 1;
            }

            if (chainedKeys.Count > 0 && chainedKeys[0] == 1)
            {
                key = 6;
                while (keys.Contains(key))
                {
                    chainedKeys.Insert(0, key);
                    key -= 1;
                }
            }

            return chainedKeys;
        }

        private bool Fumble(List<int> keys, List<int> chainedSequence)
        {
            foreach (var k in chainedSequence)
            {
                if (keys.Count(x => x == k) >= 2)
                {
                    return true;
                }
            }
            return false;
        }

        private Result? RollFumbleChart(IRandomizer randomizer)
        {
            var key = randomizer.RollOnce(6);
            var texts = new[]
            {
                "何かの問題で言い争い、主君に無礼を働いてしまう。あなたは主君の名誉点を１点失うか、【時間】を１点消費して和解の話し合いを持つか選べる。",
                "見過ごせば人々を不幸にする危険に遭遇する。あなたは逃げ出して冒険の名誉点を１点失うか、これに立ち向かい【命数】を２点減らすかを選べる。",
                "あなたが惹かれたのは好意に付け込む人だった。あなたはその場を去って恋慕の名誉点を１点失うか【正義】を１点減らして礼を尽くすかを選べる。",
                "金銭的な問題で、生命と魂の苦しみを背負う人に出会う。あなたは庇護の名誉点を１点失うか出費を３点増やすかを選べる。",
                "襲撃を受ける。苦もなく叩き伏せると、卑屈な態度で命乞いをしてきた。容赦なく命を奪い寛容の名誉点を１点失うか、密告によって【血路】が１Ｄ６点増えるかを選ぶことができる。",
                "風聞により、友が悪に身を貶めたと知る。共に並んだ戦場が色褪せる想いだ。戦友の名誉点を１点減らすか、【酒と歌】すべてを失うかを選べる。",
            };
            var text = texts[key - 1];
            return Result.CreateBuilder($"1D6 => [{key}] {text}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollWindPowerChart(IRandomizer randomizer)
        {
            int key = 0;
            int totalBonus = 0;
            string text = "";

            while (true)
            {
                int dice = randomizer.RollOnce(6);
                key += dice;

                var tableEntry = new object[][]
                {
                    new object[] { true, 0, "ほぼ凪（振り足し）" },
                    new object[] { true, 0, "弱い風（振り足し）" },
                    new object[] { false, 0, "ゆるやかな風" },
                    new object[] { false, 0, "ゆるやかな風" },
                    new object[] { false, 1, "やや強い風（儀式点プラス１）" },
                    new object[] { false, 2, "強い風（龍を幻視、儀式点プラス２）" },
                    new object[] { false, 3, "体が揺らぐほどの風（龍を幻視、儀式点プラス３）" },
                };

                int tableIndex = Math.Min(key, 7) - 1;
                var entry = tableEntry[tableIndex];
                bool add = (bool)entry[0];
                int bonus = (int)entry[1];
                string currentText = (string)entry[2];

                totalBonus += bonus;

                if (key != dice)
                {
                    currentText = $"1D6[{dice}]+{key - dice} {currentText}";
                }
                else
                {
                    currentText = $"1D6[{dice}] {currentText}";
                }

                if (string.IsNullOrEmpty(text))
                {
                    text = currentText;
                }
                else
                {
                    text = $"{text} => {currentText}";
                }

                if (!add)
                {
                    text += $" [合計：儀式点 +{totalBonus} ]";
                    return Result.CreateBuilder(text)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
            }
        }

        private Result? RollEmotionChart(IRandomizer randomizer)
        {
            var key = randomizer.RollOnce(6);
            var texts = new[]
            {
                "愛情／殺意",
                "友情／負目",
                "崇拝／嫌悪",
                "興味／侮蔑",
                "信頼／嫉妬",
                "守護／欲情",
            };
            var text = texts[key - 1];
            return Result.CreateBuilder($"1D6 => [{key}] {text}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollHope(int numberOfDice, IRandomizer randomizer)
        {
            int total = 0;
            string text = "";

            while (true)
            {
                int d1 = randomizer.RollOnce(6);
                int d2 = 0;

                if (numberOfDice >= 2)
                {
                    d2 = randomizer.RollOnce(6);
                }

                total += d1 + d2;

                if (numberOfDice == 2)
                {
                    text += $"2D6[{d1},{d2}]";
                }
                else
                {
                    text += $"1D6[{d1}]";
                }

                if (Is1or2(d1) || Is1or2(d2))
                {
                    text += " （振り足し） => ";
                }
                else
                {
                    text += $" => 合計 {total}";
                    return Result.CreateBuilder(text)
                        .SetRands(randomizer.RandResults)
                        .Build();
                }
            }
        }

        private bool Is1or2(int n)
        {
            return n == 1 || n == 2;
        }
    }
}
