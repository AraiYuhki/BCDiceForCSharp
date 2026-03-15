using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// カードランカー
    /// </summary>
    public sealed class CardRanker : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CardRanker Instance = new CardRanker();

        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        /// <summary>カラーテーブル（色コードのリスト）</summary>
        private static readonly string[] COLOR_TABLE = new[] { "W", "U", "V", "G", "R", "B" };

        /// <summary>モンスターカード一覧（色ごとのスキルリスト）</summary>
        private static readonly string[][] MONSTER_CATEGORIES = new[]
        {
            new[] { "白竜", "僧侶", "格闘家", "斧使い", "剣士", "槍士", "歩兵", "弓兵", "砲兵", "天使", "軍神" },
            new[] { "水竜", "魚", "魚人", "イカ", "蟹", "探偵", "海賊", "魔術師", "使い魔", "雲", "水精霊" },
            new[] { "緑竜", "ワーム", "鳥人", "鳥", "獣", "獣人", "エルフ", "妖精", "昆虫", "植物", "森精霊" },
            new[] { "金竜", "宝石", "岩石", "鋼", "錬金術師", "魔法生物", "ドワーフ", "機械", "運命", "女神", "土精霊" },
            new[] { "火竜", "竜人", "恐竜", "戦車", "蛮族", "小鬼", "大鬼", "巨人", "雷", "炎", "火精霊" },
            new[] { "黒竜", "闇騎士", "怪物", "忍者", "妖怪", "蝙蝠", "吸血鬼", "不死者", "幽霊", "悪魔", "邪神" },
        };

        private static readonly Regex CmWDRegex = new Regex(@"^CM(\w)(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "CardRanker";

        /// <inheritdoc/>
        public override string Name => "カードランカー";

        /// <inheritdoc/>
        public override string SortKey => "かあとらんかあ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ランダムでモンスターカードを選ぶ (RM) (RTTn n：色番号、省略可能)
        ランダム分野表 RCT
        特定のモンスターカードを選ぶ (CMxy　x：色、y：番号）
        　白：W、青：U、緑：V、金：G、赤：R、黒：B
        　例）CMW2→白の2：白竜　CMG12→金の12：土精霊
        場所表 (ST)
        街中場所表 (CST)
        郊外場所表 (OST)
        学園場所表 (GST)
        運命表 (DT)
        大会運命表 (TDT)
        学園運命表 (GDT)
        崩壊運命表 (CDT)
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var monsterResult = GetMonster(command);
            if (monsterResult != null)
            {
                return Result.CreateBuilder(monsterResult).Build();
            }
            return RollTables(command, TABLES);
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
                return Result.Fumble("ファンブル");
            }
            else if (diceTotal >= 12)
            {
                return Result.Critical("スペシャル");
            }
            else
            {
                return total >= target
                    ? Result.Success("成功")
                    : Result.Failure("失敗");
            }
        }

        private string? GetMonster(string command)
        {
            var m = CmWDRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }
            var colorKey = m.Groups[1].Value.ToUpperInvariant();
            var cat = Array.IndexOf(COLOR_TABLE, colorKey);
            if (cat < 0)
            {
                return null;
            }
            var rowDice = Convert.ToInt32(m.Groups[2].Value);
            if (rowDice < 2 || rowDice > 12)
            {
                return null;
            }
            var skill = MONSTER_CATEGORIES[cat][rowDice - 2];
            return $"モンスター選択 ＞ {skill}";
        }

    }
}
