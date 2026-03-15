using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;
using BCDice.CommonCommand.AddDice;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガープス
    /// </summary>
    public sealed class GURPS : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly GURPS Instance = new GURPS();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "GURPS";

        /// <inheritdoc/>
        public override string Name => "ガープス";

        /// <inheritdoc/>
        public override string SortKey => "かあふす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定においてクリティカル・ファンブルの自動判別、成功度の自動計算。(3d6<=目標値／目標値-3d6)
         ・祝blessing等のダイス目にかかる修正は「3d6-1<=目標値」といった記述で計算されます。(ダイス目の修正値はクリティカル・ファンブルに影響を与えません)
         ・クリティカル値・ファンブル値への修正については現在対応していません。
        ・クリティカル表 (CRT)
        ・頭部打撃クリティカル表 (HCRT)
        ・ファンブル表 (FMB)
        ・呪文ファンブル表 (MFMB)
        ・妖魔夜行スペシャルクリティカル表 (YSCRT)
        ・妖魔夜行スペシャルファンブル表 (YSFMB)
        ・妖術ファンブル表 (YFMB)
        ・命中部位表 (HIT)
        ・恐怖表 (FEAR+n)
        　nには恐怖判定の失敗度を入れてください。
        ・反応判定表 (REACT, REACT±n)
        　nには反応修正を入れてください。
        ・D66ダイスあり
        ";

        private static readonly string[] FEAR_TABLE = new[]
        {
            "1ターン朦朧状態。2ターン目に自動回復。",
            "1ターン朦朧状態。2ターン目に自動回復。",
            "1ターン朦朧状態。以後、毎ターン不利な修正を無視した意志判定を行い、成功すると回復。",
            "1ターン朦朧状態。以後、毎ターン不利な修正を無視した意志判定を行い、成功すると回復。",
            "1ターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "1ターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "1Dターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "2Dターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "思考不能。15ターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "新たな癖をひとつ植え付けられる。",
            "1D点疲労。さらに1Dターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "1D点疲労。さらに1Dターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "新たな癖をひとつ獲得。さらに1Dターン朦朧状態。以後、毎ターン通常の意志判定を行い、成功すると回復。",
            "1D分間意識を失う。以後、1分ごとに生命力判定を行い、成功すると回復。",
            "生命力判定を行い、失敗すると1点の負傷を受ける。さらに1D分間意識を失う。以後、1分ごとに生命力判定を行い、成功すると回復。",
            "1点負傷。2D分間意識を失う。以後、1分ごとに生命力判定を行い、成功すると回復。",
            "卒倒。4D分間意識不明。1D点疲労。",
            "パニック。1D分間のあいだ、叫びながら走り回ったり、座り込んで泣きわめいたりする。以後、1分ごとに知力判定(修正なし)を行い、成功すると回復。",
            "-10CPの妄想を植え付けられる。",
            "-10CPの軽い恐怖症を植え付けられる。",
            "肉体的な変化。髪が真白になったり、老化したりする。-15CPぶんの肉体的特徴に等しい。",
            "その恐怖に関連する軽い恐怖症を持っているならそれが強い恐怖症(CP2倍)になる。そうでなければ、-10CPぶんの精神的特徴を植え付けられる。",
            "-10CPの妄想を植え付けられる。生命力判定を行い、失敗すると1点の負傷を受ける。さらに1D分間意識を失う。以後、1分ごとに生命力判定を行い、成功すると回復。",
            "-10CPの軽い恐怖症を植え付けられる。生命力判定を行い、失敗すると1点の負傷を受ける。さらに1D分間意識を失う。以後、1分ごとに生命力判定を行い、成功すると回復。",
            "浅い昏睡状態。30分ごとに生命力判定を行い、成功すると目覚める。目覚めてから6時間はあらゆる判定に-2の修正。",
            "昏睡状態。1時間ごとに生命力判定を行い、成功すると目覚める。目覚めてから6時間はあらゆる判定に-2の修正。",
            "硬直。1D日のあいだ身動きしない。その時点で生命力判定を行い、成功すると動けるようになる。失敗するとさらに1D日硬直。その間、適切な医学的処置を受けていないかぎり、初日に1点、2日目に2点、3日目に3点と生命力を失っていく。動けるようになってからも、硬直していたのと同じ日数だけ、あらゆる判定に-2の修正。",
            "痙攣。1D分間地面に倒れて痙攣する。2D点疲労。また、生命力判定に失敗すると1D点負傷。これがファンブルなら生命力1点を永遠に失う。",
            "発作。軽い心臓発作を起こし、地面に倒れる。2D点負傷。",
            "大パニック。キャラクターは支離滅裂な行動に出る。GMが3Dを振り、目が大きければ大きいほど馬鹿げた行動を行う。その行動が終わったら知力判定を行い、成功すると我に返る。失敗すると新たな馬鹿げた行動をとる。",
            "強い妄想(-15CP)を植え付けられる。",
            "強い恐怖症、ないし-15CPぶんの精神的特徴を植え付けられる。",
            "激しい肉体的変化。髪が真白になったり、老化したりする。-20CPぶんの肉体的特徴に等しい。",
            "激しい肉体的変化。髪が真白になったり、老化したりする。-30CPぶんの肉体的特徴に等しい。",
            "昏睡状態。1時間ごとに生命力判定を行い、成功すると目覚める。目覚めてから6時間はあらゆる判定に-2の修正。さらに強い妄想(-15CP)を植え付けられる。",
            "昏睡状態。1時間ごとに生命力判定を行い、成功すると目覚める。目覚めてから6時間はあらゆる判定に-2の修正。さらに強い恐怖症、ないし-30CPぶんの精神的特徴を植え付けられる。",
            "昏睡状態。1時間ごとに生命力判定を行い、成功すると目覚める。目覚めてから6時間はあらゆる判定に-2の修正。さらに強い恐怖症、ないし-30CPぶんの精神的特徴を植え付けられる。知力が1点永遠に低下する。あわせて精神系の技能、呪文、超能力のレベルも低下する。",
        };

        private struct ReactionEntry
        {
            public int Min;
            public int Max;
            public string Text;
            public ReactionEntry(int min, int max, string text) { Min = min; Max = max; Text = text; }
        }

        private static readonly ReactionEntry[] REACTION_TABLE = new[]
        {
            new ReactionEntry(int.MinValue, 0, "最悪"),
            new ReactionEntry(1, 3, "とても悪い"),
            new ReactionEntry(4, 6, "悪い"),
            new ReactionEntry(7, 9, "良くない"),
            new ReactionEntry(10, 12, "中立"),
            new ReactionEntry(13, 15, "良い"),
            new ReactionEntry(16, 18, "とても良い"),
            new ReactionEntry(19, int.MaxValue, "最高"),
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return Roll3d6(command, randomizer) ?? RollFear(command, randomizer) ?? RollReact(command, randomizer) ?? RollTables(command, TABLES);
        }

        /// <summary>
        /// ゲーム別成功度判定(nD6)
        /// </summary>
        private Result? ResultNd6(int total, int diceTotal, List<int> diceList, string cmpOp, int? target)
        {
            if (target == null)
            {
                return null;
            }
            if (diceList.Count != 3 || cmpOp != "<=")
            {
                return null;
            }

            int success = target.Value - total;

            if (Critical(diceTotal, target.Value))
            {
                return Result.CreateBuilder($"クリティカル(成功度：{success}).Build()").SetCritical(true).SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
            }
            else if (Fumble(diceTotal, target.Value))
            {
                return Result.CreateBuilder($"ファンブル(失敗度：{success}).Build()").SetFumble(true).SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
            else if (diceTotal >= 17)
            {
                return Result.CreateBuilder($"自動失敗(失敗度：{success}).Build()").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
            else if (total <= target.Value)
            {
                return Result.CreateBuilder($"成功(成功度：{success}).Build()").SetSuccess(true).SetRands(_randomizer!.RandResults).Build();
            }
            else
            {
                return Result.CreateBuilder($"失敗(失敗度：{success}).Build()").SetFailure(true).SetRands(_randomizer!.RandResults).Build();
            }
        }

        private bool Critical(int diceTotal, int target)
        {
            return (diceTotal <= 6 && target >= 16) || (diceTotal <= 5 && target >= 15) || diceTotal <= 4;
        }

        private bool Fumble(int diceTotal, int target)
        {
            return (target - diceTotal <= -10) || (diceTotal >= 17 && target <= 15) || diceTotal >= 18;
        }

        private Result? Roll3d6(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([\d+-]+)-3D6?([\d+-]*)$");
            if (!m.Success)
            {
                return null;
            }

            int? targetNumber = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor);
            int modifier = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType.Floor) ?? 0;
            string formatedModifier = Format.Modifier(modifier);

            string cmd = $"3D6{formatedModifier}<={targetNumber}";
            return AddDiceCommand.Instance.Eval(cmd, this, randomizer);
        }

        private Result? RollFear(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^FEAR(\+?\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            int modifier = m.Groups[1].Success ? Convert.ToInt32(m.Groups[1].Value) : 0;
            int dice = randomizer.RollSum(3, 6);
            int number = dice + modifier;

            int num = number > 40 ? 36 : number - 4;
            // Clamp to valid table index
            num = Math.Max(0, Math.Min(num, FEAR_TABLE.Length - 1));

            string text = $"恐怖表({number}) ＞ {FEAR_TABLE[num]}";
            return Result.CreateBuilder(text).Build();
        }

        private Result? RollReact(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^REACT([+-]?\d*)$");
            if (!m.Success)
            {
                return null;
            }

            int modifier = string.IsNullOrEmpty(m.Groups[1].Value) ? 0 : Convert.ToInt32(m.Groups[1].Value);
            int dice = randomizer.RollSum(3, 6);
            int number = dice + modifier;

            string text = $"反応表({number}) ＞ {Reaction(number)}";
            return Result.CreateBuilder(text).Build();
        }

        private string Reaction(int number)
        {
            foreach (var entry in REACTION_TABLE)
            {
                if (number >= entry.Min && number <= entry.Max)
                {
                    return entry.Text;
                }
            }
            return "不明";
        }

    }
}
