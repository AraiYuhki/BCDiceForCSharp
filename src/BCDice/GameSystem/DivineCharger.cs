using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 神聖課金RPGディヴァインチャージャー
    /// </summary>
    public sealed class DivineCharger : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly DivineCharger Instance = new DivineCharger();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>アクションデータ（テキスト・ダイス・目標値を保持）</summary>
        private sealed class ActionData
        {
            public string Text { get; }
            public IReadOnlyList<int> Dice { get; }
            public string Target { get; }

            public ActionData(string text, IReadOnlyList<int> dice, string target)
            {
                Text = text;
                Dice = dice;
                Target = target;
            }
        }

        /// <inheritdoc/>
        public override string Id => "DivineCharger";

        /// <inheritdoc/>
        public override string Name => "神聖課金RPGディヴァインチャージャー";

        /// <inheritdoc/>
        public override string SortKey => "しんせいかきんRPGていうあいんちやあしやあ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override bool SortBarabaraDice => true;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■判定　　nDC>=t          n:能力値 t:目標値
        例)3DC>=7: ダイスを3個振って、目標値7で判定。その結果(達成値,成功・失敗,クリティカル,ファンブル)を表示
        　 3DC>=?: 　同上　目標値が不明なので、達成値,クリティカル,ファンブルのみ表示。

        ■反転判定　　REV[n]>=t   n:ダイス目(カンマなし) t:目標値
        例)REV[123]>=7: 振ったダイスが[1,2,3]で、目標値7で反転判定。その結果(達成値,成功・失敗,クリティカル,ファンブル)を表示


        ■ランダムイベント　　RET
        ■神器　　　　　　　　aksT a:表(AかB) k:種別(K:近接, S:射撃, M:魔法, Y:鎧, T:盾, A:装飾品) s:ランク(1～5)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? ResoluteReverse(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)DC>=(\d+|\?)$");
            if (!m.Success)
            {
                return null;
            }
            var numDice = Convert.ToInt32(m.Groups[1].Value);
            var target = m.Groups[2].Value;
            var dice = randomizer.RollBarabara(numDice, 6).OrderBy(x => x).ToList();
            var diceText = string.Join(",", dice);
            var output = $"({numDice}DC>={target}) ＞ {diceText}";
            var action = new ActionData(output, dice, target);
            return ActionResult(action, randomizer);
        }

        private Result ActionResult(ActionData action, IRandomizer randomizer)
        {
            var output = action.Text;
            var dice = action.Dice;
            var target = action.Target;
            var count6 = dice.Count(v => v == 6);
            var count1 = dice.Count(v => v == 1);
            var successNum = dice.Sum();
            if (count6 >= 2)
            {
                output += $" ＞ 達成値{successNum}";
                output += " ＞ クリティカル";
                return Result.CreateBuilder(output).SetCritical(true).SetSuccess(true).SetRands(randomizer.RandResults).Build();
            }
            else if (count1 >= 2)
            {
                output += " ＞ 達成値0";
                output += " ＞ ファンブル([神聖石]5個)";
                return Result.CreateBuilder(output).SetFumble(true).SetFailure(true).SetRands(randomizer.RandResults).Build();
            }
            output += $" ＞ 達成値{successNum}";
            if (target == "?")
            {
                return Result.CreateBuilder(output).SetRands(randomizer.RandResults).Build();
            }
            else
            {
                if (successNum >= Convert.ToInt32(target))
                {
                    output += " ＞ 成功";
                    return Result.CreateBuilder(output).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                }
                else
                {
                    output += " ＞ 失敗";
                    return Result.CreateBuilder(output).SetFailure(true).SetRands(randomizer.RandResults).Build();
                }
            }
        }

        private Result? ResoluteReverse(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^REV\[([\d,]+)\]>=(\d+|\?)$");
            if (!m.Success)
            {
                return null;
            }
            var rawDice = m.Groups[1].Value;
            var target = m.Groups[2].Value;
            rawDice = rawDice.Replace(",", "");
            var diceChars = rawDice.ToCharArray().Select(c => c.ToString()).ToArray();
            var dice = ReverseDice(diceChars);
            var diceText = string.Join(",", dice);
            var output = $"(REV[{rawDice}]>={target}) ＞ {diceText}";
            var action = new ActionData(output, dice, target);
            return ActionResult(action, randomizer);
        }

        private List<int> ReverseDice(string[] arrayDice)
        {
            return arrayDice
                .Select(s => { int v; return int.TryParse(s, out v) ? v : 0; })
                .Where(v => v >= 1 && v <= 6)
                .Select(v => 7 - v)
                .OrderBy(v => v)
                .ToList();
        }

    }
}
