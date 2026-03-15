using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ブラインド・ミトスRPG
    /// </summary>
    public sealed class BlindMythos : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly BlindMythos Instance = new BlindMythos();

        /// <inheritdoc/>
        public override string Id => "BlindMythos";

        /// <inheritdoc/>
        public override string Name => "ブラインド・ミトスRPG";

        /// <inheritdoc/>
        public override string SortKey => "ふらいんとみとすRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
・判定：BMx@y>=z、BMSx@y>=z
  　x:スキルレベル
　　y:目標難易度（省略可。デフォルト4）
　　z:必要成功度
　BMコマンドはダイスの振り足しを常に行い、
　BMSは振り足しを自動では行いません。
 例）BM>=1　BM@3>=1　BMS2>=1

・判定振り足し：ReRollx,x,x...@y>=z
  　x:振るダイスの個数
　　y:目標難易度（省略可。デフォルト4）
　　z:必要成功度
　振り足しを自動で行わない場合（BMSコマンド）に使用します。

・LE：失う感情表
・感情後遺症表 ESx
　ESH：喜、ESA：怒、ESS：哀、ESP：楽、ESL：愛、ESE：感
・DT：汚染チャート
・RPxyz：守護星表チェック
 xyz:守護星ナンバーを指定
 例）RP123　RP258
";

        private static readonly Regex JudgeRollRegex = new Regex(
            @"^BM(S)?(\d*)(@(\d+))?>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReRollRegex = new Regex(
            @"^ReRoll([\d,]+)(@(\d+))?>=(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex RulingPlanetRegex = new Regex(
            @"^RP(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DurtyTableRegex = new Regex(
            @"^DT$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// テーブル定義
        /// </summary>
        private static readonly Dictionary<string, SimpleTable> TABLES = new Dictionary<string, SimpleTable>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "LE", new SimpleTable(
                    "失う感情表",
                    "LE",
                    new[]
                    {
                        "喜：喜びは消えた。嬉しい気持ちとは、なんだっただろう。",
                        "怒：激情は失われ、憎しみもどこかへと消える。",
                        "哀：どんなに辛くても、悲しさを感じない。どうやら涙も涸れたらしい。",
                        "楽：もはや楽しいことなどない。希望を抱くだけ無駄なのだ。",
                        "愛：愛など幻想……無力で儚い、役に立たない世迷い言だ。",
                        "感：なにを見ても、感動はない。心は凍てついている。",
                    })
            },
            {
                "ESH", new SimpleTable(
                    "「喜」の感情後遺症表",
                    "ESH",
                    new[]
                    {
                        "日々喜びを求めてしまう。",
                        "日々喜びを求めてしまう。",
                        "嬉しい時間が長続きしない。",
                        "素直に喜びを共有できないことがある。",
                        "小さなことで大きく喜びを感じる。",
                        "小さなことで大きく喜びを感じる。",
                        "影響なし。",
                        "影響なし。",
                        "「喜」の後遺症をひとつ消してもよい。",
                        "「喜」の後遺症をひとつ消してもよい。",
                        "「喜」の後遺症をひとつ消してもよい。",
                    })
            },
            {
                "ESA", new SimpleTable(
                    "「怒」の感情後遺症表",
                    "ESA",
                    new[]
                    {
                        "始終不機嫌になる。",
                        "始終不機嫌になる。",
                        "一度怒ると、なかなか収まらない。",
                        "怒りっぽくなる",
                        "怒りかたが激しくなる。",
                        "怒りかたが激しくなる。",
                        "影響なし。",
                        "影響なし。",
                        "「怒」の後遺症をひとつ消してもよい。",
                        "「怒」の後遺症をひとつ消してもよい。",
                        "「怒」の後遺症をひとつ消してもよい。",
                    })
            },
            {
                "ESS", new SimpleTable(
                    "「哀」の感情後遺症表",
                    "ESS",
                    new[]
                    {
                        "一度涙が出るとなかなか止まらない。",
                        "一度涙が出るとなかなか止まらない。",
                        "夜、哀しいことを思い出して目が覚める。",
                        "不意に哀しい気持ちになる。",
                        "涙もろくなる。",
                        "涙もろくなる。",
                        "影響なし。",
                        "影響なし。",
                        "「哀」の後遺症をひとつ消してもよい。",
                        "「哀」の後遺症をひとつ消してもよい。",
                        "「哀」の後遺症をひとつ消してもよい。",
                    })
            },
            {
                "ESP", new SimpleTable(
                    "「楽」の感情後遺症表",
                    "ESP",
                    new[]
                    {
                        "突然陽気になったり、不意に笑い出してしまう。",
                        "突然陽気になったり、不意に笑い出してしまう。",
                        "周りが楽しくなさそうだと不安になる。",
                        "楽しいことがないと落ち着かない。",
                        "些細なことでも笑ってしまう。",
                        "些細なことでも笑ってしまう。",
                        "影響なし。",
                        "影響なし。",
                        "「楽」の後遺症をひとつ消してもよい。",
                        "「楽」の後遺症をひとつ消してもよい。",
                        "「楽」の後遺症をひとつ消してもよい。",
                    })
            },
            {
                "ESL", new SimpleTable(
                    "「愛」の感情後遺症表",
                    "ESL",
                    new[]
                    {
                        "少しでも気になる相手に愛を求めてしまう。",
                        "少しでも気になる相手に愛を求めてしまう。",
                        "愛する相手（恋人・家族・ペット・空想）から離れたくない。",
                        "誰彼構わず優しくしてしまう。",
                        "ひとりでいると不安を感じる。",
                        "ひとりでいると不安を感じる。",
                        "影響なし。",
                        "影響なし。",
                        "「愛」の後遺症をひとつ消してもよい。",
                        "「愛」の後遺症をひとつ消してもよい。",
                        "「愛」の後遺症をひとつ消してもよい。",
                    })
            },
            {
                "ESE", new SimpleTable(
                    "「感」の感情後遺症表",
                    "ESE",
                    new[]
                    {
                        "感動を共有できない相手を不信に思ってしまう。",
                        "感動を共有できない相手を不信に思ってしまう。",
                        "嬉しくても哀しくてもすぐに涙が出る。",
                        "リアクションがオーバーになる。",
                        "ちょっとしたことで感動する。",
                        "ちょっとしたことで感動する。",
                        "影響なし。",
                        "影響なし。",
                        "「感」の後遺症をひとつ消してもよい。",
                        "「感」の後遺症をひとつ消してもよい。",
                        "「感」の後遺症をひとつ消してもよい。",
                    })
            },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var result = JudgeRoll(command, randomizer);
            if (result != null)
            {
                return result;
            }

            var isStop = true;
            var (text, _, _, _) = ReRoll(command, isStop, randomizer);
            if (text != null)
            {
                return Result.CreateBuilder(text).Build();
            }

            result = GetRulingPlanetDiceCommandResult(command, randomizer);
            if (result != null)
            {
                return result;
            }

            var tableResult = GetDurtyTableCommandReuslt(command, randomizer);
            if (tableResult != null)
            {
                return Result.CreateBuilder(tableResult).Build();
            }

            return RollTables(command, randomizer);
        }

        /// <summary>
        /// テーブルコマンドを検索してロールする
        /// </summary>
        private Result? RollTables(string command, IRandomizer randomizer)
        {
            if (TABLES.TryGetValue(command, out var table))
            {
                return table.Roll(randomizer);
            }

            return null;
        }

        private Result? JudgeRoll(string command, IRandomizer randomizer)
        {
            var m = JudgeRollRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var isStop = m.Groups[1].Success; // "S" がマッチしたかどうか
            var skillRank = m.Groups[2].Value.Length > 0 ? int.Parse(m.Groups[2].Value) : 0;
            var judgeNumberText = m.Groups[3].Value; // "@4" 部分（省略時は空文字列）
            var judgeNumber = m.Groups[4].Success ? int.Parse(m.Groups[4].Value) : 4;
            var targetNumber = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : 1;

            var message = "";
            var diceCount = skillRank + 2;
            var isReRoll = false;
            var (text, bitList, successList, countOneList, canReRoll) =
                GetRollResult(new[] { diceCount }, judgeNumberText, judgeNumber, targetNumber, isReRoll, isStop, randomizer);

            message += text;
            var result = GetTotalResult(bitList, successList, countOneList, targetNumber, isStop, canReRoll);
            return Result.CreateBuilder(message + result.Text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .Build();
        }

        private (string? text, List<int> successList, List<int> countOneList, int targetNumber) ReRoll(string command, bool isStop, IRandomizer randomizer)
        {
            var m = ReRollRegex.Match(command);
            if (!m.Success)
            {
                return (null, new List<int>(), new List<int>(), 0);
            }

            var rerollCountsText = m.Groups[1].Value;
            var judgeNumberText = m.Groups[2].Value;
            var judgeNumber = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 4;
            var targetNumber = int.Parse(m.Groups[4].Value);

            var rerollCounts = rerollCountsText.Split(',').Select(s => int.Parse(s)).ToArray();

            var commandText = "";
            foreach (var diceCount in rerollCounts)
            {
                if (commandText.Length > 0)
                {
                    commandText += ",";
                }
                commandText += $"ReRoll{diceCount}{judgeNumberText}>={targetNumber}";
            }

            var message = "";
            if (rerollCounts.Length > 1 && isStop)
            {
                message += $"({commandText})";
            }
            message += "\n";
            var isReRoll = true;
            var (text, _bitList, successList, countOneList, _canReRoll) =
                GetRollResult(rerollCounts, judgeNumberText, judgeNumber, targetNumber, isReRoll, isStop, randomizer);

            message += text;

            return (message, successList, countOneList, targetNumber);
        }

        private (string text, List<int> bitList, List<int> successList, List<int> countOneList, bool canReRoll) GetRollResult(
            int[] rerollCounts, string judgeNumberText, int judgeNumber, int targetNumber,
            bool isReRoll, bool isStop, IRandomizer randomizer)
        {
            var bitList = new List<int>();
            var successList = new List<int>();
            var countOneList = new List<int>();
            var rerollTargetList = new List<string>();

            var message = "";
            for (var index = 0; index < rerollCounts.Length; index++)
            {
                var diceCount = rerollCounts[index];
                if (index != 0)
                {
                    message += "\n";
                }

                var commandName = $"ReRoll{diceCount}";
                if (!isReRoll)
                {
                    if (isStop)
                    {
                        commandName = $"BMS{diceCount - 2}";
                    }
                    else
                    {
                        commandName = $"BM{diceCount - 2}";
                    }
                }
                var commandText = $"{commandName}{judgeNumberText}>={targetNumber}";

                var diceList = randomizer.RollBarabara(diceCount, 6).OrderBy(x => x).ToArray();
                var diceText = string.Join(",", diceList);

                if (isReRoll)
                {
                    message += " ＞ ";
                }
                message += $"({commandText}) ＞ {diceCount}D6[{diceText}] ＞ ";

                var (success, countOne, resultText) = GetSuccessResultText(diceList, judgeNumber);
                if (!isReRoll)
                {
                    bitList.AddRange(diceList.Where(i => i >= 4));
                }
                successList.Add(success);
                countOneList.Add(countOne);
                message += resultText;

                var sameDiceList = GetSameDieList(diceList);
                if (sameDiceList.Count == 0)
                {
                    continue;
                }

                var rerollText = "";
                foreach (var list in sameDiceList)
                {
                    if (rerollText.Length > 0)
                    {
                        rerollText += ",";
                    }
                    rerollText += string.Join("", list);
                }

                rerollTargetList.Add(string.Join(",", sameDiceList.Select(list => list.Count)));

                message += $"、リロール[{rerollText}]";
            }

            var rerollCommand = "";
            if (rerollTargetList.Count > 0)
            {
                rerollCommand = $"ReRoll{string.Join(",", rerollTargetList)}{judgeNumberText}>={targetNumber}";
                if (isStop)
                {
                    message += $"\n ＞ コマンド：{rerollCommand}";
                }
            }

            var canReRoll = rerollCommand.Length > 0;

            if (canReRoll && !isStop)
            {
                var (text, successListTmp, countOneListTmp, _) = ReRoll(rerollCommand, isStop, randomizer);
                if (text != null)
                {
                    message += text;
                    successList.AddRange(successListTmp);
                    countOneList.AddRange(countOneListTmp);
                }
            }

            return (message, bitList, successList, countOneList, canReRoll);
        }

        private Result GetTotalResult(List<int> bitList, List<int> successList, List<int> countOneList,
            int targetNumber, bool isStop, bool canReRoll)
        {
            var success = successList.Aggregate((sum, i) => sum + i);
            var countOne = countOneList.Aggregate((sum, i) => sum + i);

            var result = "";

            if (successList.Count > 1)
            {
                result += $"\n ＞ 最終成功数:{success}";
            }

            if (canReRoll && isStop)
            {
                result += "\n";

                if (success >= targetNumber)
                {
                    result += " ＞ 現状で成功。コマンド実行で追加リロールも可能";
                    return Result.CreateBuilder(result).SetSuccess(true).Build();
                }
                else
                {
                    result += " ＞ 現状のままでは失敗";
                    if (countOne >= 1)
                    {
                        result += $"。汚染ポイント+{countOne}";
                        return Result.CreateBuilder(result).SetFumble(true).SetFailure(true).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder(result).SetFailure(true).Build();
                    }
                }
            }

            if (success >= targetNumber)
            {
                result += " ＞ 成功";
                if (bitList.Count >= 1)
                {
                    result += $"、禁書ビット発生[{string.Join(",", bitList)}]";
                    return Result.CreateBuilder(result).SetSuccess(true).SetCritical(true).Build();
                }
                else
                {
                    return Result.CreateBuilder(result).SetSuccess(true).Build();
                }
            }
            else
            {
                result += " ＞ 失敗";
                if (countOne >= 1)
                {
                    result += $"。汚染ポイント+{countOne}";
                    return Result.CreateBuilder(result).SetFumble(true).SetFailure(true).Build();
                }
                else
                {
                    return Result.CreateBuilder(result).SetFailure(true).Build();
                }
            }
        }

        private List<List<int>> GetSameDieList(int[] diceList)
        {
            var sameDiceList = new List<List<int>>();

            foreach (var i in diceList.Distinct())
            {
                if (i == 1)
                {
                    continue;
                }

                var list = diceList.Where(dice => dice == i).ToList();
                if (list.Count <= 1)
                {
                    continue;
                }

                sameDiceList.Add(list);
            }

            return sameDiceList;
        }

        private (int success, int countOne, string resultText) GetSuccessResultText(int[] diceList, int judgeNumber)
        {
            var success = 0;
            var countOne = 0;

            foreach (var i in diceList)
            {
                if (i == 1)
                {
                    countOne += 1;
                }

                if (i >= judgeNumber)
                {
                    success += 1;
                }
            }

            var resultText = $"成功数:{success}";

            return (success, countOne, resultText);
        }

        private Result? GetRulingPlanetDiceCommandResult(string command, IRandomizer randomizer)
        {
            var m = RulingPlanetRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var targetNumbers = m.Groups[1].Value.Select(c => int.Parse(c.ToString())).ToList();
            var diceList = GetRulingPlanetDice(randomizer);

            var condition = diceList.Any(dice => targetNumbers.Contains(dice));

            var resultText = condition ? "発動" : "失敗";
            var text = $"守護星表チェック({string.Join(",", targetNumbers)}) ＞ {diceList.Count}D10[{string.Join(",", diceList)}] ＞ {resultText}";

            return Result.CreateBuilder(text)
                .SetCondition(condition)
                .Build();
        }

        private List<int> GetRulingPlanetDice(IRandomizer randomizer)
        {
            var rolls = randomizer.RollBarabara(2, 10);
            var dice1 = rolls[0];
            var dice2 = rolls[1];

            while (dice1 == dice2)
            {
                dice2 = randomizer.RollOnce(10);
            }

            if (dice1 == 10)
            {
                dice1 = 0;
            }
            if (dice2 == 10)
            {
                dice2 = 0;
            }

            return new List<int> { dice1, dice2 };
        }

        private string? GetDurtyTableCommandReuslt(string command, IRandomizer randomizer)
        {
            if (!DurtyTableRegex.IsMatch(command))
            {
                return null;
            }

            var table = new[]
            {
                "汚染チャートを２回振り、その効果を適用する（1・2-2,5・6-12 なら振り直す）",
                "ＰＣ全員の「トラウマ」「喪失」すべてに２ダメージ",
                "ＰＣ全員の「喪失」２つに４ダメージ",
                "ＰＣ全員の「トラウマ」すべてに２ダメージ。その後さらに汚染が２増える",
                "ＰＣ全員、１つの【記憶】の両方の値が０になる。このときアクロバットダイス獲得不可",
                "ＰＣ全員の「喪失」１つに４ダメージ。このときアクロバットダイス獲得不可",
                "ＰＣ全員の「トラウマ」すべてに１ダメージ。その後さらに汚染が３増える",
                "ＰＣ全員の「トラウマ」すべてに１ダメージ。その後アクロバットダイスをＰＣ人数分失う",
                "ＰＣ全員の「喪失」すべてに２ダメージ。禁書ビットをすべて失う",
                "ＰＣ全員の「トラウマ」２つに３ダメージ。その後さらに汚染が１増える",
                "ＰＣ全員の「トラウマ」「喪失」すべてに１ダメージ",
                "ＰＣ全員の「喪失」１つに４ダメージ。禁書ビットをすべて失う",
                "ＰＣ全員の「トラウマ」すべてに２ダメージ",
                "ＰＣ全員の１つの【記憶】の「トラウマ」「喪失」それぞれに３ダメージ",
                "ＰＣ全員の「喪失」すべてに１ダメージ",
                "ＰＣ全員の「トラウマ」３つに２ダメージ",
                "ＰＣ全員の「トラウマ」と「喪失」それぞれ１つに３ダメージ",
                "ＰＣ全員の「喪失」３つに２ダメージ",
                "ＰＣ全員のすべての「トラウマ」に1 ダメージ",
                "ＰＣ全員のひとつの【記憶】の「トラウマ」「喪失」それぞれに３ダメージ",
                "ＰＣ全員の「喪失」すべてに２ダメージ",
                "ＰＣ全員の「トラウマ」ひとつに４ダメージ。禁書ビットをすべて失う",
                "ＰＣ全員の「トラウマ」「喪失」すべてに１ダメージ",
                "ＰＣ全員の「喪失」２つに３ダメージ。その後さらに汚染が１増える",
                "ＰＣ全員の「トラウマ」すべてに２ダメージ。禁書ビットをすべて失う",
                "ＰＣ全員の「喪失」すべてに１ダメージ。その後アクロバットダイスをＰＣ人数分失う",
                "ＰＣ全員の「喪失」すべてに１ダメージ。その後さらに汚染が３増える",
                "ＰＣ全員の「トラウマ」１つに４ダメージ。このときアクロバットダイス獲得不可",
                "ＰＣ全員、１つの【記憶】の両方の値が０になる。このときアクロバットダイス獲得不可",
                "ＰＣ全員の「喪失」すべてに２ダメージ。その後さらに汚染が２増える",
                "ＰＣ全員の「トラウマ」２つに４ダメージ",
                "ＰＣ全員の「トラウマ」「喪失」すべてに２ダメージ",
                "汚染チャートを２回振り、その効果を適用する（1・2-2,5・6-12 なら振り直す）",
            };

            var dice1 = randomizer.RollOnce(6);
            var dice2 = randomizer.RollSum(2, 6);

            var index = (dice2 - 2) * 3 + (int)Math.Ceiling(dice1 / 2.0) - 1;
            return $"汚染チャート({dice1},{dice2}) ＞ {table[index]}";
        }
    }
}
