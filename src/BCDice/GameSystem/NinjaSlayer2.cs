using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.CommonCommand;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class NinjaSlayer2 : GameSystemBase
    {
        public static readonly NinjaSlayer2 Instance = new NinjaSlayer2();
        public override string Id => "NinjaSlayer2";
        public override string Name => "ニンジャスレイヤーTRPG 2版";
        public override string SortKey => "にんじやすれいやあTRPG2";

        private static readonly Dictionary<string, int> DifficultyMap =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                ["K"]  = 2, ["E"] = 3, ["N"] = 4, ["H"] = 5, ["U"] = 6, ["UH"] = 6,
            };

        private static readonly Regex ReSB    = new Regex(@"S([1-6]|[1-6]+(?:/[1-6]+)?\+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReCrit  = new Regex(@"C([1-6]|[1-6]+\+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReJudge = new Regex(@"(=|!=|>=|>|<=|<)([1-6]+\+|[1-6])", RegexOptions.Compiled);
        private static readonly Regex ReMain  = new Regex(
            @"^((?:(?:UH|[KENHU])?\d+,?)+)(?:\[((?:(?:S([1-6]|[1-6]+(?:/[1-6]+)?\+)?|C([1-6]*\+?)?|(=|!=|>=|>|<=|<)([1-6]+\+?))(?:\]\[)?)+)\])?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReSBCmd  = new Regex(@"^SB(?:@([1-6]))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReWS     = new Regex(@"^WS([1-9]|10|11|12)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReWSE    = new Regex(@"^WSE(?:@([1-6]))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReNRS    = new Regex(@"^NRS(?:_(E|N|H|U|UH)(\d+))?(?:@([1-7]))?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReSubCmd = new Regex(@"^(UH|[KENHU])?(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            try
            {
                var mMain = ReMain.Match(command);
                if (mMain.Success) return ProcDice2nd(mMain, randomizer);

                var mSB = ReSBCmd.Match(command);
                if (mSB.Success)
                {
                    int t = mSB.Groups[1].Success ? int.Parse(mSB.Groups[1].Value) : 0;
                    return ProcSatzBatz(t, randomizer);
                }

                var mWS = ReWS.Match(command);
                if (mWS.Success)
                    return ProcWasshoi(int.Parse(mWS.Groups[1].Value), randomizer);

                var mWSE = ReWSE.Match(command);
                if (mWSE.Success)
                {
                    int t = mWSE.Groups[1].Success ? int.Parse(mWSE.Groups[1].Value) : 0;
                    return ProcWasshoiEntry(t, randomizer);
                }

                var mNRS = ReNRS.Match(command);
                if (mNRS.Success)
                {
                    string? diffStr = mNRS.Groups[1].Success ? mNRS.Groups[1].Value : null;
                    int diceNum = mNRS.Groups[2].Success ? int.Parse(mNRS.Groups[2].Value) : 0;
                    int type = mNRS.Groups[3].Success ? int.Parse(mNRS.Groups[3].Value) : 0;
                    return ProcNRS(diceNum, diffStr, type, randomizer);
                }

                return null;
            }
            catch { return null; }
        }

        private static int SToI(string? s, int def) =>
            string.IsNullOrEmpty(s) ? def : int.Parse(s);

        private static List<int> CheckDifficulty(List<int> dice, int diff, string cmpOp)
        {
            var result = new List<int>();
            foreach (int d in dice)
            {
                bool ok = cmpOp switch
                {
                    ">=" => d >= diff,
                    ">"  => d > diff,
                    "<=" => d <= diff,
                    "<"  => d < diff,
                    "="  => d == diff,
                    "!=" => d != diff,
                    _    => false,
                };
                if (ok) result.Add(d);
            }
            return result;
        }

        private static bool CheckDifficultyNew(List<int> dice, string? difficulty, string cmpOp)
        {
            if (string.IsNullOrEmpty(difficulty) || dice.Count == 0) return false;
            var sD = dice.OrderBy(x => x).ToList();
            var sV = difficulty.ToCharArray().OrderBy(c => c).Select(c => c - '0').ToList();
            int idx = 0;
            foreach (int dv in sD)
            {
                if (idx >= sV.Count) break;
                bool ok = cmpOp switch
                {
                    ">=" => dv >= sV[idx],
                    ">"  => dv > sV[idx],
                    "<=" => dv <= sV[idx],
                    "<"  => dv < sV[idx],
                    "="  => dv == sV[idx],
                    "!=" => dv != sV[idx],
                    _    => false,
                };
                if (ok) idx++;
            }
            return idx >= sV.Count;
        }

        private static string ProcAppendix(List<int> dl, string apCmd)
        {
            var mSB = ReSB.Match(apCmd);
            if (mSB.Success)
            {
                string? dc = mSB.Groups[1].Success ? mSB.Groups[1].Value : null;
                if (apCmd.EndsWith("+"))
                {
                    string raw = dc != null ? dc.Replace("+", "") : "";
                    int si = raw.IndexOf('/');
                    string sbC = si >= 0 ? raw.Substring(0, si) : raw;
                    string? nmC = si >= 0 ? raw.Substring(si + 1) : null;
                    if (nmC != null && CheckDifficultyNew(dl, nmC, ">=")) return $", ナムアミダブツ！[{apCmd}]";
                    if (CheckDifficultyNew(dl, sbC, ">=")) return $", サツバツ！[{apCmd}]";
                    return "";
                }
                else
                {
                    var arr = CheckDifficulty(dl, SToI(dc, 6), ">=");
                    string s = $", サツバツ判定[{apCmd}]:{arr.Count}";
                    if (arr.Count > 0) s += $"[{string.Join(",", arr.OrderByDescending(x => x))}]";
                    return s;
                }
            }
            var mC = ReCrit.Match(apCmd);
            if (mC.Success)
            {
                string? dc = mC.Groups[1].Success ? mC.Groups[1].Value : null;
                if (apCmd.EndsWith("+"))
                {
                    if (CheckDifficultyNew(dl, dc != null ? dc.Replace("+", "") : null, ">="))
                        return $", クリティカル！[{apCmd}]";
                    return "";
                }
                else
                {
                    var arr = CheckDifficulty(dl, SToI(dc, 6), ">=");
                    string s = $", クリティカル判定[{apCmd}]:{arr.Count}";
                    if (arr.Count > 0) s += $"[{string.Join(",", arr.OrderByDescending(x => x))}]";
                    return s;
                }
            }
            var mJ = ReJudge.Match(apCmd);
            if (mJ.Success)
            {
                string cmpOp = mJ.Groups[1].Value; string dc2 = mJ.Groups[2].Value;
                if (apCmd.EndsWith("+"))
                {
                    if (CheckDifficultyNew(dl, dc2.Replace("+", ""), cmpOp))
                        return $", 追加判定成功！[{apCmd}]";
                    return "";
                }
                else
                {
                    var arr = CheckDifficulty(dl, SToI(dc2, 6), cmpOp);
                    string s = $", 追加判定[{apCmd}]:{arr.Count}";
                    if (arr.Count > 0) s += $"[{string.Join(",", arr.OrderByDescending(x => x))}]";
                    return s;
                }
            }
            return "";
        }

        private Result ProcDice2nd(Match match, IRandomizer randomizer)
        {
            string cp = match.Groups[1].Value;
            string? ap = match.Groups[2].Success ? match.Groups[2].Value : null;
            var sb = new StringBuilder();
            int totalSuccess = 0;
            int difficulty = 0;
            foreach (string sc in cp.Split(','))
            {
                var mSub = ReSubCmd.Match(sc.Trim());
                if (!mSub.Success) continue;
                if (mSub.Groups[1].Success)
                {
                    string sym = mSub.Groups[1].Value.ToUpperInvariant();
                    if (DifficultyMap.TryGetValue(sym, out int d)) difficulty = d;
                }
                int dn = int.Parse(mSub.Groups[2].Value);
                string rc = $"{dn}B6>={difficulty}";
                var rr = BarabaraDiceCommand.Instance.Eval(rc, this, randomizer);
                if (rr == null) continue;
                var dl = randomizer.RandResults.Select(r => r.Value).ToList();
                int sn = dl.Count(v => v >= difficulty);
                sb.Append($"({rc}) ＞ {string.Join(",", dl)} ＞ 成功数:{sn}");
                totalSuccess += sn;
                if (ap != null)
                {
                    foreach (string apCmd in ap.Split(new string[]{"]["} , StringSplitOptions.None))
                        sb.Append(ProcAppendix(dl, apCmd));
                }
                sb.Append(" \n");
            }
            string text = sb.ToString().TrimEnd();
            return totalSuccess > 0 ? Result.Success(text) : Result.Failure(text);
        }

        private static readonly string[] SbTable = {
            "キル！（そのキャラクターはただちに死亡する）",
            "グロッキー！（そのキャラクターのHPが0になり行動不能になる）",
            "ノックバック！（そのキャラクターは1エリア後退し、行動不能になる）",
            "スタン！（そのキャラクターは次のラウンドの行動ができなくなる）",
            "ディスアーム！（そのキャラクターは武器を落とし、拾うまで素手になる）",
            "ファンブル！（そのキャラクターの次のロールに-2のペナルティがつく）",
        };

        private Result ProcSatzBatz(int type, IRandomizer randomizer)
        {
            if (type > 0 && type <= 6)
                return Result.Success($"サツバツ!!({type}) ＞ {SbTable[type - 1]}");
            int roll = randomizer.RollOnce(6);
            return Result.Success($"サツバツ!!(1D6) ＞ {roll} ＞ {SbTable[roll - 1]}");
        }

        private Result ProcWasshoi(int dkk, IRandomizer randomizer)
        {
            var dice = randomizer.RollBarabara(2, 6);
            int total = dice.Sum();
            string base_ = $"Wasshoi!判定(2D6) ＞ ({string.Join("+", dice)}) ＞ {total}";
            if (total > dkk)
                return Result.Failure(base_ + $"(>{dkk}) 判定失敗");
            return Result.Success(base_ + $"(<=={dkk}) 判定成功!! \nニンジャスレイヤー=サンのエントリーだり!!");
        }

        private static readonly string[] WasshoiTable = {
            "カラテ＋1D6",
            "ニンジャソウル＋1",
            "全員がニンジャソウル＋1",
            "プレイヤーが選択したキャラクターがニンジャソウル＋1",
            "全員がカラテ＋1D3",
            "今回のセッションのニンジャ出現回数が＋1",
        };

        private Result ProcWasshoiEntry(int type, IRandomizer randomizer)
        {
            if (type > 0 && type <= 6)
                return Result.Success($"ニンジャスレイヤー=サンのエントリー!!({type}) ＞ {WasshoiTable[type - 1]}");
            int roll = randomizer.RollOnce(6);
            return Result.Success($"ニンジャスレイヤー=サンのエントリー!!(1D6) ＞ {roll} ＞ {WasshoiTable[roll - 1]}");
        }

        private static readonly string[] NrsTable = {
            "発狂(1): モータル状態(HPが0の状態)になる",
            "発狂(2): 最も近くにいるキャラクターをランダムで攻撃する",
            "発狂(3): 最も近くにいるキャラクターに向かって移動し続ける",
            "発狂(4): 逃走する。逃走できない場合は行動不能になる",
            "発狂(5): 1D6ラウンド、完全に行動不能になる",
            "発狂(6): 1D3ラウンド後に死亡する",
            "発狂(7): 即死する",
        };

        private Result? ProcNRS(int diceNum, string? diffStr, int type, IRandomizer randomizer)
        {
            int diffI = 0;
            if (diffStr != null) DifficultyMap.TryGetValue(diffStr.ToUpperInvariant(), out diffI);
            if (diffI == 0 && type == 0) return null;

            var sb = new System.Text.StringBuilder();
            if (diffI > 0)
            {
                string rc = $"{diceNum}B6>={diffI}";
                var rr = BarabaraDiceCommand.Instance.Eval(rc, this, randomizer);
                if (rr == null) return null;
                var dl = randomizer.RandResults.Select(r => r.Value).ToList();
                int sn = dl.Count(v => v >= diffI);
                sb.Append($"NRS判定({rc}) ＞ {string.Join(",", dl)} ＞ 成功数:{sn}");
                if (sn > 0) { sb.Append($" NRS克服!!"); return Result.Success(sb.ToString()); }
                sb.Append($" NRS発症!! \n");
            }
            int df = 0; int add = 0;
            if (type == 0)
            {
                switch (diffStr?.ToUpperInvariant())
                {
                    case "E": df = 3; break;
                    case "N": df = 6; break;
                    case "H": case "U": df = 6; add = 1; break;
                }
                type = randomizer.RollOnce(df) + add;
            }
            string rcStr = df > 0 ? $"1D{df}{(add > 0 ? "+" + add : "")}" : "";
            string pfx = df > 0 ? $"({rcStr}) ＞ " : "";
            int idx = Math.Max(0, type - 1);
            sb.Append($"NRS発狂{pfx}({type}) ＞ {NrsTable[idx]}");
            return Result.Failure(sb.ToString());
        }
    }
}
