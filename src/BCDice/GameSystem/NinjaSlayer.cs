using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.CommonCommand;

namespace BCDice.GameSystem
{
    public sealed class NinjaSlayer : GameSystemBase
    {
        public static readonly NinjaSlayer Instance = new NinjaSlayer();

        private static readonly Dictionary<string, int> DifficultyMap =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            { ["K"] = 2, ["E"] = 3, ["N"] = 4, ["H"] = 5, ["UH"] = 6 };

        private static readonly Regex EvRegex = new Regex(
            @"^EV(\d+)(?:\[([^]]+)\]|@([^/\s]+))?(?:/(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex AtRegex = new Regex(
            @"^AT(\d+)(?:\[([^]]+)\]|@([^\s]+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ElRegex = new Regex(
            @"^EL(\d+)(?:\[([^]]+)\]|@([^\s]+))?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public override string Id => "NinjaSlayer";
        public override string Name => "ニンジャスレイヤーTRPG";
        public override string SortKey => "にんしやすれいやあTRPG";
        public override string HelpMessage => "NJ EV AT EL SB";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (command == "SB") return RollSbTable(randomizer);
            var evM = EvRegex.Match(command);
            if (evM.Success) return ExecuteEv(evM, randomizer);
            var atM = AtRegex.Match(command);
            if (atM.Success) return ExecuteAt(atM, randomizer);
            var elM = ElRegex.Match(command);
            if (elM.Success) return ExecuteEl(elM, randomizer);
            return null;
        }

        private int GetDifficulty(string s)
        {
            if (string.IsNullOrEmpty(s)) return 4;
            if (int.TryParse(s, out int n) && n >= 2 && n <= 6) return n;
            return DifficultyMap.TryGetValue(s, out int d) ? d : 4;
        }

        private string BRollCmd(int num, int diff) => num + "B6>=" + diff;

        private Result? ExecuteEv(Match m, IRandomizer randomizer)
        {
            int num = int.Parse(m.Groups[1].Value);
            string ds = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Success ? m.Groups[3].Value : null;
            int diff = GetDifficulty(ds);
            int? tv = m.Groups[4].Success ? (int?)int.Parse(m.Groups[4].Value) : null;
            var r = BarabaraDiceCommand.Instance.Eval(BRollCmd(num, diff), this, randomizer);
            if (r == null) return null;
            var parts = new List<string> { r.Text };
            if (tv.HasValue && CountVal(r, v => v >= diff) > tv.Value) parts.Add("カウンターカラテ!!");
            return Result.CreateBuilder(string.Join(" ＞ ", parts)).Build();
        }

        private Result? ExecuteAt(Match m, IRandomizer randomizer)
        {
            int num = int.Parse(m.Groups[1].Value);
            string ds = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Success ? m.Groups[3].Value : null;
            var r = BarabaraDiceCommand.Instance.Eval(BRollCmd(num, GetDifficulty(ds)), this, randomizer);
            if (r == null) return null;
            var parts = new List<string> { r.Text };
            if (CountVal(r, v => v == 6) >= 2) parts.Add("サツバツ!!");
            return Result.CreateBuilder(string.Join(" ＞ ", parts)).Build();
        }

        private Result? ExecuteEl(Match m, IRandomizer randomizer)
        {
            int num = int.Parse(m.Groups[1].Value);
            string ds = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[3].Success ? m.Groups[3].Value : null;
            int diff = GetDifficulty(ds);
            var r = BarabaraDiceCommand.Instance.Eval(BRollCmd(num, diff), this, randomizer);
            if (r == null) return null;
            int maxCount = CountVal(r, v => v == 6);
            int succCount = CountVal(r, v => v >= diff);
            if (maxCount >= 1)
            {
                string text = r.Text + " + " + maxCount + " ＞ " + (succCount + maxCount);
                return Result.CreateBuilder(text).Build();
            }
            return Result.CreateBuilder(r.Text).Build();
        }

        private static int CountVal(Result r, Func<int, bool> pred)
            => r.Rands == null ? 0 : r.Rands.Count(rv => pred(rv.Value));

        private static Result RollSbTable(IRandomizer randomizer)
        {
            int roll = randomizer.RollOnce(6);
            string[] tbl = {
                "「死ねーッ！」腹部に強烈な一撃！本来のダメージ+1。敵は後方へ弾き飛ばされ壁に激突。",
                "「イヤーッ！」頭部への痛烈なカラテ！【ニューロン】と【ワザマエ】が各1減少。【万札】D3発生。",
                "「苦しみ抜いて死ぬがいい」急所破壊！本来のダメージ+1。【精神力】-2、【ニューロン】-1。【万札】D3発生。",
                "「逃げられるものなら逃げてみよ」脚粉砕！本来のダメージ。【脚力】D3減少。【万札】D3発生。",
                "「これで手も足も出まい！」両腕切断！ダメージ+1。【ワザマエ】と【カラテ】各2減少。【万札】D3発生。",
                "「イイイヤアアアアーーーーッ！」ナムアミダブツ！敵即死。【万札】D6発生。"
            };
            return Result.CreateBuilder("サツバツ表(" + roll + ").Build() ＞ " + tbl[roll - 1]).Build();
        }
    }
}
