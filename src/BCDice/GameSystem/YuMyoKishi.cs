using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 幽冥鬼使
    /// </summary>
    public sealed class YuMyoKishi : GameSystemBase
    {
        public static readonly YuMyoKishi Instance = new YuMyoKishi();

        private static readonly Dictionary<string, object> TABLES = new Dictionary<string, object>();

        public override string Id => "YuMyoKishi";
        public override string Name => "幽冥鬼使";
        public override string SortKey => "ゆうみようきし";

        public override string HelpMessage => @"
        ■判定　YM+a>=b　a:技能値（省略可）　b:目標値（省略可）
        　例：YM+4>=8: 技能値による修正が+4で、目標値8の克服判定を行う
        　　　YM>=8  : 技能値による修正なしで、目標値8の克服判定を行う
        　　　YM+6   : 技能値による修正が+6で、達成値を確認する

        ■代償表　COT
        ■転禍表　TRT
        ";

        private static readonly Regex CmdRegex = new Regex(
            @"^YM([+-]\d+)?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollCommand(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? RollCommand(string command, IRandomizer randomizer)
        {
            var m = CmdRegex.Match(command);
            if (!m.Success)
                return null;

            int modify = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            int? target = m.Groups[3].Success ? (int?)int.Parse(m.Groups[3].Value) : null;

            var diceList = randomizer.RollBarabara(4, 6);
            var (value, status) = SipPatA(diceList);
            int achievement = status == "wumian" ? 0 : value + modify;

            var parts = new List<string> { command, string.Join(",", diceList), value.ToString() };
            if (value != achievement)
                parts.Add(achievement.ToString());
            string rollText = string.Join(" ＞ ", parts);

            if (target == null)
            {
                if (status == "wumian")
                    return Result.CreateBuilder(rollText).SetFailure(true).SetRands(randomizer.RandResults).Build();
                if (status == "yise")
                    return Result.CreateBuilder(rollText).SetCritical(true).SetSuccess(true).SetRands(randomizer.RandResults).Build();
                return Result.CreateBuilder(rollText).SetRands(randomizer.RandResults).Build();
            }

            if (status == "wumian")
                return Result.CreateBuilder(rollText + " ＞ 可").SetFailure(true).SetRands(randomizer.RandResults).Build();
            if (status == "yise")
                return Result.CreateBuilder(rollText + " ＞ 優").SetCritical(true).SetSuccess(true).SetRands(randomizer.RandResults).Build();
            if (achievement >= target.Value)
                return Result.CreateBuilder(rollText + " ＞ 良").SetSuccess(true).SetRands(randomizer.RandResults).Build();
            return Result.CreateBuilder(rollText + " ＞ 可").SetFailure(true).SetRands(randomizer.RandResults).Build();
        }

        private (int value, string status) SipPatA(IList<int> diceList)
        {
            var groups = diceList.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
            switch (groups.Count)
            {
                case 1:
                    return (20, "yise");
                case 2:
                    if (groups.Values.All(v => v == 2))
                        return (groups.Keys.Max() * 2, "normal");
                    return (groups.Keys.Sum(), "normal");
                case 3:
                    return (groups.Where(kv => kv.Value == 1).Sum(kv => kv.Key), "normal");
                default:
                    return (0, "wumian");
            }
        }
    }
}
