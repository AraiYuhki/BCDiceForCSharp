using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ハンターズ・ムーン
    /// </summary>
    public sealed class HuntersMoon : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly HuntersMoon Instance = new HuntersMoon();


        private static readonly Regex SaRegex = new Regex(
            @"^SA",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "HuntersMoon";

        /// <inheritdoc/>
        public override string Name => "ハンターズ・ムーン";

        /// <inheritdoc/>
        public override string SortKey => "はんたあすむうん";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　判定時にクリティカルとファンブルを自動判定します。
        ・各種表
        　・遭遇表　(ET)
        　・都市ロケーション表　(CLT)
        　・閉所ロケーション表　(SLT)
        　・炎熱ロケーション表　(HLT)
        　・冷暗ロケーション表　(FLT)
        　・部位ダメージ決定表　(DLT)
        　・モノビースト行動表　(MAT)
        　・異形アビリティー表　(SATx) (xは個数)
        　・異形アビリティー表2　(SA2Tx) (xは個数)
        　　→表１と表２の振り分けも判定
        　・指定特技表  RTTn(n＝分野番号、省略可能)
              1.社会(TST) 2.頭部(THT) 3.腕部(TAT) 4.胴部(TBT) 5.脚部(TLT) 6.環境(TET)
        　・異形化表　　　　　　(MST)
        　・代償表　　　　　　　(ERT)
        　・ディフェンス遭遇表1/2/3 (DS1ET/DS2ET/DS3ET)
        　・エスケープ遭遇表1/2/3 (EE1ET/EE2ET/EE3ET)
        　・エスコート遭遇表1/2/3 (ET1ET/ET2ET/ET3ET)
        　・トラッキング遭遇表1/2/3 (TK1ET/TK2ET/TK3ET)
        ・D66ダイスあり
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = SaRegex.Match(command);
            if (match.Success)
            {
                return RollStrangeAbilityTable(command, randomizer);
            }

            return null;
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (cmpOp != CompareOperator.GreaterThanOrEqual)
            {
                return null;
            }

            if (diceTotal <= 2)
            {
                return Result.CreateBuilder("ファンブル(モノビースト追加行動+1).Build()")
                    .SetFumble(true)
                    .SetFailure(true)
                    .Build();
            }
            else if (diceTotal >= 12)
            {
                return Result.CreateBuilder("スペシャル(変調1つ回復orダメージ+1D6).Build()")
                    .SetSuccess(true)
                    .SetCritical(true)
                    .Build();
            }

            return null;
        }

        private Result? RollStrangeAbilityTable(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^SA(2)?T(\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var count = m.Groups[2].Success ? int.Parse(m.Groups[2].Value) : 1;
            var type2 = m.Groups[1].Success;
            var table = type2 ? (IStrangeAbilityTable)new StrangeAbilityTable2() : new StrangeAbilityTable1();
            var diceList = new List<string>();
            var chosenList = new List<string>();
            for (var i = 0; i < count; i++)
            {
                table.Roll(randomizer);
                diceList.Add(table.Dice);
                chosenList.Add(table.Chosen);
            }
            var dice = string.Join(",", diceList);
            var chosen = string.Join("/", chosenList);
            return Result.CreateBuilder($"異形アビリティー表({dice}).Build() ＞ {chosen}")
                .SetSuccess(true)
                .Build();
        }

        private interface IStrangeAbilityTable
        {
            string Dice { get; }
            string Chosen { get; }
            void Roll(IRandomizer randomizer);
        }

        private class StrangeAbilityTable1 : IStrangeAbilityTable
        {
            public static readonly string[] BODY = new[] { "大牙", "大鎌", "針山", "大鋏", "吸血根", "巨大化", "瘴気", "火炎放射", "鑢", "ドリル", "絶叫", "粘液噴射", "潤滑液", "皮膚装甲", "器官生成", "翼", "四肢複製", "分解", "異言", "閃光", "冷気", "悪臭", "化膿歯", "気嚢", "触手", "肉瘤", "暗視", "邪視", "超振動", "酸分泌", "結晶化", "裏腹", "融合", "嘔吐", "腐敗", "変色" };

            public string Dice { get; private set; } = "";
            public string Chosen { get; private set; } = "";

            public void Roll(IRandomizer randomizer)
            {
                var dice1 = randomizer.RollOnce(6);
                var dice2 = randomizer.RollOnce(6);
                var index = (dice1 - 1) * 6 + (dice2 - 1);
                Dice = (dice1 * 10 + dice2).ToString();
                Chosen = BODY[index];
            }
        }

        private class StrangeAbilityTable2 : IStrangeAbilityTable
        {
            public static readonly string[] BODY = new[] { "電撃", "障壁", "追加肢", "破裂球", "死病", "ソナー", "未来視", "寄生体", "再構築", "分身", "大角", "鉄塊", "硬質化", "生命力吸収", "鬼火", "金縛り", "排出口", "金属化", "鋼鱗", "神経接合", "光翼", "環境適応", "消化剤", "プロペラ", "血栓", "骨槍", "回転", "怒髪", "煙幕", "脂肪層", "逆棘", "偽頭", "赤化", "発条", "凶運", "巨砲" };

            public string Dice { get; private set; } = "";
            public string Chosen { get; private set; } = "";

            public void Roll(IRandomizer randomizer)
            {
                var dice = randomizer.RollOnce(6);
                var table = dice % 2 != 0 ? StrangeAbilityTable1.BODY : BODY;
                var tableId = dice % 2 != 0 ? 1 : 2;
                var dice1 = randomizer.RollOnce(6);
                var dice2 = randomizer.RollOnce(6);
                var index = (dice1 - 1) * 6 + (dice2 - 1);
                Dice = $"{dice}-{dice1 * 10 + dice2}";
                Chosen = $"[表{tableId}]{table[index]}";
            }
        }

    }
}
