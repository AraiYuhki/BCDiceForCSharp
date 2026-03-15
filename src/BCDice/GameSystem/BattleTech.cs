using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    public sealed class BattleTech : GameSystemBase
    {
        public static readonly BattleTech Instance = new BattleTech();
        private const int NoCritHitLimit = 7;
        private const int LrmLimit = 5;
        private static readonly Regex HitRx = new Regex(
            @"\A([LCR][LU]?)?(\+\d+)?>=(\d+)", RegexOptions.Compiled);

        public override string Id => "BattleTech";
        public override string Name => "バトルテック";
        public override string SortKey => "はとるてつく";
        public override string HelpMessage => "BT CT DW CDx SRM LRM PPC";

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var tbl = TryTable(command, randomizer);
            if (tbl != null) return tbl;

            var ppc = Regex.Match(command, @"^(\d+)?PPC([LCR][LU]?)?([+-]\d+)?=(\d+)$", RegexOptions.IgnoreCase);
            if (ppc.Success)
            {
                int cnt = ppc.Groups[1].Success ? int.Parse(ppc.Groups[1].Value) : 1;
                string sd = ppc.Groups[2].Success ? ppc.Groups[2].Value.ToUpper() : "C";
                string md = ppc.Groups[3].Success ? ppc.Groups[3].Value : "";
                int tg = int.Parse(ppc.Groups[4].Value);
                return HitResult(cnt, () => (10, (int?)null, false), sd + md + ">=" + tg, randomizer);
            }

            int rep = 1; string cmd = command;
            var cm = Regex.Match(command, @"^(\d+)(.+)");
            if (cm.Success) { rep = int.Parse(cm.Groups[1].Value); cmd = cm.Groups[2].Value; }

            var cd = Regex.Match(cmd, @"\ACD([1-6])\z");
            if (cd.Success) return ConsRoll(int.Parse(cd.Groups[1].Value), randomizer);

            var slrm = Regex.Match(cmd, @"^((S|L)RM\d+)(.+)");
            if (slrm.Success)
            {
                try { return HitResult(rep, () => XrmDamage(slrm.Groups[1].Value, randomizer), slrm.Groups[3].Value, randomizer); }
                catch (XrmEx) { return null; }
            }

            var bt = Regex.Match(cmd, @"^BT(\d+)(.+)");
            if (bt.Success)
            {
                int dv = int.Parse(bt.Groups[1].Value);
                return HitResult(rep, () => (dv, (int?)null, false), bt.Groups[2].Value, randomizer);
            }
            return null;
        }

        private Result? TryTable(string cmd, IRandomizer r)
        {
            if (cmd == "CT")
            {
                int t = r.RollSum(2, 6);
                string c = t <= NoCritHitLimit ? "致命的命中はなかった" : t <= 9 ? "1箇所の致命的命中" :
                           t <= 11 ? "2箇所の致命的命中" : "その部位が吹き飛ぶ（腕、脚、頭）または3箇所の致命的命中（胴）";
                return Result.CreateBuilder("致命的命中表(" + t + ").Build() ＞ " + c).SetRands(r.RandResults).Build();
            }
            if (cmd == "DW")
            {
                int v = r.RollOnce(6);
                string[] dw = { "同じ（前面から転倒） 正面／背面", "1ヘクスサイド右（側面から転倒） 右側面",
                    "2ヘクスサイド右（側面から転倒） 右側面", "180度逆（背面から転倒） 正面／背面",
                    "2ヘクスサイド左（側面から転倒） 左側面", "1ヘクスサイド左（側面から転倒） 左側面" };
                return Result.CreateBuilder("転倒後の向き表(" + v + ").Build() ＞ " + dw[v - 1]).SetRands(r.RandResults).Build();
            }
            return null;
        }

        private (int d, int? dice, bool lrm) XrmDamage(string type, IRandomizer r)
        {
            int roll = r.RollSum(2, 6);
            bool lrm = type.StartsWith("L", StringComparison.OrdinalIgnoreCase);
            int raw;
            switch (type.ToUpper())
            {
                case "SRM2": raw = roll <= 7 ? 1 : 2; break;
                case "SRM4": raw = roll == 2 ? 1 : roll <= 6 ? 2 : roll <= 10 ? 3 : 4; break;
                case "SRM6": raw = roll <= 3 ? 2 : roll <= 5 ? 3 : roll <= 8 ? 4 : roll <= 10 ? 5 : 6; break;
                case "LRM5": raw = roll == 2 ? 1 : roll <= 4 ? 2 : roll <= 8 ? 3 : roll <= 10 ? 4 : 5; break;
                case "LRM10": raw = roll <= 3 ? 3 : roll == 4 ? 4 : roll <= 8 ? 6 : roll <= 10 ? 8 : 10; break;
                case "LRM15": raw = roll <= 3 ? 5 : roll == 4 ? 6 : roll <= 8 ? 9 : roll <= 10 ? 12 : 15; break;
                case "LRM20": raw = roll <= 3 ? 6 : roll == 4 ? 9 : roll <= 8 ? 12 : roll <= 10 ? 16 : 20; break;
                default: throw new XrmEx("Unknown XRM: " + type);
            }
            return (lrm ? raw : raw * 2, roll, lrm);
        }

        private Result? HitResult(int count, Func<(int d, int? dice, bool lrm)> dmgF, string tail, IRandomizer r)
        {
            var m = HitRx.Match(tail); if (!m.Success) return null;
            string side = m.Groups[1].Success ? m.Groups[1].Value.ToUpper() : "C";
            int target = int.Parse(m.Groups[3].Value);
            int bv = GetBase(m.Groups[2].Success ? m.Groups[2].Value : null);
            if (!HPTables.TryGetValue(side, out var tbl)) return null;
            var lines = new List<string>();
            var dmgs = new Dictionary<string, (List<int> pd, List<string> cr)>();
            int hits = 0;
            for (int i = 0; i < count; i++)
            {
                var (hit, hr) = HitText(bv, target, r);
                if (hit) { hits++; var (nd, dt) = Damages(dmgF, tbl, dmgs, r); dmgs = nd; hr += dt; }
                lines.Add(hr);
            }
            bool anyHit = hits > 0;
            string hct = " ＞ " + hits + "回命中";
            lines.Add(anyHit ? hct + " 命中箇所：" + TotalDmg(dmgs) : hct);
            string rt = string.Join("\n", lines);
            return anyHit
                ? Result.CreateBuilder(rt).SetSuccess(true).SetRands(r.RandResults).Build()
                : Result.CreateBuilder(rt).SetFailure(true).SetRands(r.RandResults).Build();
        }
        private int GetBase(string s) { if (string.IsNullOrEmpty(s)) return 0; return (int)ArithmeticEvaluator.Eval(s, RoundType.Floor); }
        private (bool hit, string text) HitText(int bv, int tgt, IRandomizer r)
        { int d1=r.RollOnce(6),d2=r.RollOnce(6),tot=d1+d2+bv; bool hit=tot>=tgt;
          string bs=bv>0?"+"+bv:"";
          return (hit,tot+"["+d1+","+d2+bs+"]>="+tgt+" ＞ "+(hit?"命中 ＞ ":"外れ")); }
        private (Dictionary<string, (List<int> pd, List<string> cr)> dmgs, string rt)
            Damages(Func<(int d, int? dice, bool lrm)> dmgF, HPEntry[] tbl,
                Dictionary<string, (List<int> pd, List<string> cr)> dmgs, IRandomizer r)
        {
            string rt = ""; var (dmg, dice, lrm) = dmgF();
            int parts = 1;
            if (lrm) { parts = (int)Math.Ceiling(1.0*dmg/LrmLimit); rt += "["+dice+"] "+dmg+"点"; }
            for (int i = 0; i < parts; i++)
            {
                var (cd, dt) = DmgInfo(dice, dmg, lrm, i);
                var (tx, pt, ct) = HitOne(dt, tbl, r);
                if (lrm) rt += " "; rt += tx;
                if (!dmgs.ContainsKey(pt)) dmgs[pt] = (new List<int>(), new List<string>());
                var e = dmgs[pt]; e.pd.Add(cd);
                if (!string.IsNullOrEmpty(ct)) e.cr.Add(ct);
                dmgs[pt] = e;
            }
            return (dmgs, rt);
        }
        private (int cd, string dt) DmgInfo(int? dice, int dmg, bool lrm, int idx)
        { if (dice==null) return (dmg,dmg.ToString()); if (!lrm) return (dmg,"["+dice+"] "+dmg);
          int c=Math.Min(dmg-LrmLimit*idx,LrmLimit); return (c,c.ToString()); }
        private string TotalDmg(Dictionary<string, (List<int> pd, List<string> cr)> dmgs)
        {
            string[] order = { "頭", "胴中央", "右胴", "左胴", "右脚", "左脚", "右腕", "左腕" };
            int all = 0; var txts = new List<string>();
            foreach (var p2 in order) { if (!dmgs.TryGetValue(p2, out var di)) continue;
                dmgs.Remove(p2); int d=di.pd.Sum(); all+=d;
                string t=p2+"("+di.pd.Count+"回) "+d+"点";
                if (di.cr.Count>0) t+=" "+string.Join(" ",di.cr); txts.Add(t); }
            return string.Join(" ／ ",txts)+" ＞ 合計ダメージ "+all+"点"; }
        private (string text, string part, string ct) HitOne(string dt, HPEntry[] tbl, IRandomizer r)
        {
            bool is2 = tbl[0].Sides == 6;
            int roll = is2 ? r.RollSum(2,6) : r.RollOnce(6);
            var hp = FindPart(tbl, roll);
            string cs = hp.Crit ? "（致命的命中）" : "";
            var rp = new List<string> { "["+roll+"] "+hp.Name+cs+" "+dt+"点" };
            string ct = "";
            if (hp.Crit) {
                int cr = r.RollSum(2,6);
                string cc = cr<=NoCritHitLimit ? "致命的命中はなかった" : cr<=9 ? "1箇所の致命的命中" :
                            cr<=11 ? "2箇所の致命的命中" : "その部位が吹き飛ぶ";
                if (cr>NoCritHitLimit) ct=cc; rp.Add("["+cr+"] "+cc); }
            return (string.Join(" ＞ ",rp), hp.Name, ct);
        }
        private static HPEntry FindPart(HPEntry[] t, int r) { foreach(var e in t) if(r>=e.Min&&r<=e.Max) return e; return t[t.Length-1]; }
        private Result? ConsRoll(int d, IRandomizer r)
        {
            if (d<1||d>6) return null;
            string cmd = "CD"+d;
            if (d==6) return Result.CreateBuilder(cmd+" ＞ 死亡").SetFumble(true).SetFailure(true).SetRands(r.RandResults).Build();
            int[] tgts = { 0,3,5,7,10,11 }; int tg=tgts[d];
            var vals = r.RollBarabara(2,6); int sum=vals.Sum(); bool ok=sum>=tg;
            string text = string.Join(" ＞ ", new[] { cmd,"(2D6>="+tg+")",sum+"["+string.Join(",",vals)+"]",sum.ToString(),ok?"成功":"失敗" });
            return ok ? Result.CreateBuilder(text).SetSuccess(true).SetRands(r.RandResults).Build()
                      : Result.CreateBuilder(text).SetFailure(true).SetRands(r.RandResults).Build();
        }
        private class XrmEx : Exception { public XrmEx(string m) : base(m) { } }
        private struct HPEntry { public int Min, Max, Sides; public string Name; public bool Crit; }
        private static readonly Dictionary<string, HPEntry[]> HPTables = new Dictionary<string, HPEntry[]>
        {
            ["L"] = new[] {
                new HPEntry{Min=2,Max=2,Sides=6,Name="左胴",Crit=true},
                new HPEntry{Min=3,Max=3,Sides=6,Name="左脚",Crit=false},
                new HPEntry{Min=4,Max=5,Sides=6,Name="左腕",Crit=false},
                new HPEntry{Min=6,Max=6,Sides=6,Name="左脚",Crit=false},
                new HPEntry{Min=7,Max=7,Sides=6,Name="左胴",Crit=false},
                new HPEntry{Min=8,Max=8,Sides=6,Name="胴中央",Crit=false},
                new HPEntry{Min=9,Max=9,Sides=6,Name="右胴",Crit=false},
                new HPEntry{Min=10,Max=10,Sides=6,Name="右腕",Crit=false},
                new HPEntry{Min=11,Max=11,Sides=6,Name="右脚",Crit=false},
                new HPEntry{Min=12,Max=12,Sides=6,Name="頭",Crit=false},
            },
            ["C"] = new[] {
                new HPEntry{Min=2,Max=2,Sides=6,Name="胴中央",Crit=true},
                new HPEntry{Min=3,Max=4,Sides=6,Name="右腕",Crit=false},
                new HPEntry{Min=5,Max=5,Sides=6,Name="右脚",Crit=false},
                new HPEntry{Min=6,Max=6,Sides=6,Name="右胴",Crit=false},
                new HPEntry{Min=7,Max=7,Sides=6,Name="胴中央",Crit=false},
                new HPEntry{Min=8,Max=8,Sides=6,Name="左胴",Crit=false},
                new HPEntry{Min=9,Max=9,Sides=6,Name="左脚",Crit=false},
                new HPEntry{Min=10,Max=11,Sides=6,Name="左腕",Crit=false},
                new HPEntry{Min=12,Max=12,Sides=6,Name="頭",Crit=false},
            },
            ["R"] = new[] {
                new HPEntry{Min=2,Max=2,Sides=6,Name="右胴",Crit=true},
                new HPEntry{Min=3,Max=3,Sides=6,Name="右脚",Crit=false},
                new HPEntry{Min=4,Max=5,Sides=6,Name="右腕",Crit=false},
                new HPEntry{Min=6,Max=6,Sides=6,Name="右脚",Crit=false},
                new HPEntry{Min=7,Max=7,Sides=6,Name="右胴",Crit=false},
                new HPEntry{Min=8,Max=8,Sides=6,Name="胴中央",Crit=false},
                new HPEntry{Min=9,Max=9,Sides=6,Name="左胴",Crit=false},
                new HPEntry{Min=10,Max=10,Sides=6,Name="左腕",Crit=false},
                new HPEntry{Min=11,Max=11,Sides=6,Name="左脚",Crit=false},
                new HPEntry{Min=12,Max=12,Sides=6,Name="頭",Crit=false},
            },
            ["LU"] = new[] {
                new HPEntry{Min=1,Max=2,Sides=1,Name="左胴"},
                new HPEntry{Min=3,Max=3,Sides=1,Name="胴中央"},
                new HPEntry{Min=4,Max=5,Sides=1,Name="左腕"},
                new HPEntry{Min=6,Max=6,Sides=1,Name="頭"},
            },
            ["CU"] = new[] {
                new HPEntry{Min=1,Max=1,Sides=1,Name="左腕"},
                new HPEntry{Min=2,Max=2,Sides=1,Name="左胴"},
                new HPEntry{Min=3,Max=3,Sides=1,Name="胴中央"},
                new HPEntry{Min=4,Max=4,Sides=1,Name="右胴"},
                new HPEntry{Min=5,Max=5,Sides=1,Name="右腕"},
                new HPEntry{Min=6,Max=6,Sides=1,Name="頭"},
            },
            ["RU"] = new[] {
                new HPEntry{Min=1,Max=2,Sides=1,Name="右胴"},
                new HPEntry{Min=3,Max=3,Sides=1,Name="胴中央"},
                new HPEntry{Min=4,Max=5,Sides=1,Name="右腕"},
                new HPEntry{Min=6,Max=6,Sides=1,Name="頭"},
            },
            ["LL"] = new[] { new HPEntry{Min=1,Max=6,Sides=1,Name="左脚"} },
            ["CL"] = new[] {
                new HPEntry{Min=1,Max=3,Sides=1,Name="右脚"},
                new HPEntry{Min=4,Max=6,Sides=1,Name="左脚"},
            },
            ["RL"] = new[] { new HPEntry{Min=1,Max=6,Sides=1,Name="右脚"} },
        };
    }
}
