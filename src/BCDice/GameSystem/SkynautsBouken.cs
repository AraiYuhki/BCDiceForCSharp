using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 歯車の塔の探空士（冒険企画局）
    /// </summary>
    public sealed class SkynautsBouken : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly SkynautsBouken Instance = new SkynautsBouken();

        /// <inheritdoc/>
        public override string Id => "SkynautsBouken";

        /// <inheritdoc/>
        public override string Name => "歯車の塔の探空士（冒険企画局）";

        /// <inheritdoc/>
        public override string SortKey => "はくるまのとうのすかいのおつ2";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定（nSNt#f） n:ダイス数(省略時2)、t:目標値(省略時7)、f:ファンブル値(省略時1)
            例）SN6#2　3SN
        ・ダメージチェック (Dx/y@m) x:ダメージ範囲、y:攻撃回数
        　　m:《弾道学》（省略可）上:8、下:2、左:4、右:6
        　　例） D/4　D19/2　D/3@8　D[大揺れ]/2
        ・回避(AVO@mダメージ)
        　　m:回避方向（上:8、下:2、左:4、右:6）、ダメージ：ダメージチェック結果
        　　例）AVO@8[1,4],[2,6],[3,8]　AVO@2[6,4],[2,6]
        ・FT ファンブル表(p76)
        ・NV 航行表
        ・航行イベント表
        　・NEN 航行系
        　・NEE 遭遇系
        　・NEO 船内系
        　・NEH 困難系
        　・NEL 長旅系

        ■ 判定セット
        ・《回避運動》判定+回避（nSNt#f/AVO@ダメージ）
        　　nSNt#f → 成功なら AVO@m
        　　例）SN/AVO@8[1,4],[2,6],[3,8]　3SN#2/AVO@2[6,4],[2,6]
        ・砲撃判定+ダメージチェック　(nSNt#f/Dx/y@m)
        　　行為判定の出目変更タイミングを逃すので要GMの許可
        　　nSNt#f → 成功なら Dx/y@m
        　　例）SN/D/4　3SN#2/D[大揺れ]/2
        ";

        private static readonly Regex D_REGEXP = new Regex(
            @"^D([1-46-9]{0,8})(\[.+\]|S|F|SF|FS)?/(\d{1,2})(@([2468]))?$",
            RegexOptions.Compiled);

        private static readonly Regex Avo2468Regex = new Regex(
            @"^AVO@([2468])(.*?)$",
            RegexOptions.Compiled);

        // Direction infos: key -> (name, diffX, diffY)
        private static readonly Dictionary<int, (string Name, int DiffX, int DiffY)> DIRECTION_INFOS = new Dictionary<int, (string, int, int)>
        {
            { 0, ("", 0, 0) },
            { 1, ("左下", -1, 1) },
            { 2, ("下", 0, 1) },
            { 3, ("右下", 1, 1) },
            { 4, ("左", -1, 0) },
            { 5, ("", 0, 0) },
            { 6, ("右", 1, 0) },
            { 7, ("左上", -1, -1) },
            { 8, ("上", 0, -1) },
            { 9, ("右上", 1, -1) },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var snResult = CommandSn(command, randomizer);
            if (snResult != null) return snResult;

            var dResult = CommandD(command, randomizer);
            if (dResult != null) return dResult;

            var avoResult = CommandAvo(command);
            if (avoResult != null) return avoResult;

            var snavoResult = CommandSnavo(command, randomizer);
            if (snavoResult != null) return snavoResult;

            var sndResult = CommandSnd(command, randomizer);
            if (sndResult != null) return sndResult;

            // TODO: RollTables(command, TABLES)

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? CommandSn(string command, IRandomizer randomizer)
        {
            // Parse: [1-9]?SN(\d{0,2})
            var snMatch = Regex.Match(command, @"^([1-9]?)SN(\d{0,2})(?:#(\d+))?$");
            if (!snMatch.Success)
            {
                return null;
            }

            var diceCountStr = snMatch.Groups[1].Value;
            var targetStr = snMatch.Groups[2].Value;
            var fumbleStr = snMatch.Groups[3].Value;

            var diceCount = string.IsNullOrEmpty(diceCountStr) ? 0 : int.Parse(diceCountStr);
            var target = string.IsNullOrEmpty(targetStr) ? 0 : int.Parse(targetStr);

            if (diceCount == 0) diceCount = 2;
            if (target == 0) target = 7;
            var fumble = string.IsNullOrEmpty(fumbleStr) ? 1 : int.Parse(fumbleStr);

            var diceList = randomizer.RollBarabara(diceCount, 6);
            var diceTopTwo = diceList.OrderBy(x => x).ToList();
            // Get last 2 elements (highest 2)
            diceTopTwo = diceTopTwo.Skip(Math.Max(0, diceTopTwo.Count - 2)).ToList();

            string statusText;
            bool isSuccess = false;
            bool isFailure = false;
            bool isCritical = false;
            bool isFumble = false;
            if (diceTopTwo.SequenceEqual(new[] { 6, 6 }))
            {
                statusText = Translate("SkynautsBouken.special");
                isCritical = true;
                isSuccess = true;
            }
            else if (diceList.Max() <= fumble)
            {
                statusText = Translate("SkynautsBouken.fumble");
                isFumble = true;
                isFailure = true;
            }
            else if (diceTopTwo.Sum() >= target)
            {
                statusText = Translate("success");
                isSuccess = true;
            }
            else
            {
                statusText = Translate("failure");
                isFailure = true;
            }

            string fullText;
            if (diceCount == 2)
            {
                fullText = string.Join(" ＞ ", new[] { $"({diceCount}SN{target}#{fumble})", $"{diceTopTwo.Sum()}[{string.Join(",", diceList)}]", statusText }.Where(x => x != null));
            }
            else
            {
                fullText = string.Join(" ＞ ", new[] { $"({diceCount}SN{target}#{fumble})", "[" + string.Join(",", diceList) + "]", $"{diceTopTwo.Sum()}[{string.Join(",", diceTopTwo)}]", statusText }.Where(x => x != null));
            }
            return Result.CreateBuilder(fullText)
                .SetSuccess(isSuccess)
                .SetFailure(isFailure)
                .SetCritical(isCritical)
                .SetFumble(isFumble)
                .Build();
        }

        private Result? CommandD(string command, IRandomizer randomizer)
        {
            var m = D_REGEXP.Match(command);
            if (!m.Success)
            {
                return null;
            }
            var fireCount = Convert.ToInt32(m.Groups[3].Value);
            var fireRange = m.Groups[1].Value;
            var ballistics = string.IsNullOrEmpty(m.Groups[5].Value) ? 0 : Convert.ToInt32(m.Groups[5].Value);
            var points = GetFirePoints(fireCount, fireRange, randomizer);
            var cmdText = command;
            cmdText = Regex.Replace(cmdText, @"SF/", $"[{Translate("SkynautsBouken.big_shake")},{Translate("SkynautsBouken.fire")}]/", RegexOptions.None);
            cmdText = Regex.Replace(cmdText, @"FS/", $"[{Translate("SkynautsBouken.fire")},{Translate("SkynautsBouken.big_shake")}]/", RegexOptions.None);
            cmdText = Regex.Replace(cmdText, @"F/", $"[{Translate("SkynautsBouken.fire")}]/", RegexOptions.None);
            cmdText = Regex.Replace(cmdText, @"S/", $"[{Translate("SkynautsBouken.big_shake")}]/", RegexOptions.None);
            var result = new List<string> { $"({cmdText})", GetPointsText(points, 0, 0) };
            if (ballistics != 0)
            {
                var dir = DIRECTION_INFOS[ballistics];
                var diffX = dir.DiffX;
                var diffY = dir.DiffY;
                result[result.Count - 1] += "\n";
                result.Add($"《{Translate("SkynautsBouken.ballistics")}》{dir.Name}");
                result.Add(GetPointsText(points, diffX, diffY));
            }
            return Result.CreateBuilder(string.Join(" ＞ ", result.Where(x => x != null))).Build();
        }

        private Result? CommandAvo(string command)
        {
            var dmg = Avo2468Regex.Match(command);
            if (!dmg.Success)
            {
                return null;
            }
            var dir = DIRECTION_INFOS[Convert.ToInt32(dmg.Groups[1].Value)];
            var diffX = dir.DiffX;
            var diffY = dir.DiffY;
            var avoText = $"《{Translate("SkynautsBouken.avoidance")}》{dir.Name} ＞ " + Regex.Replace(dmg.Groups[2].Value, @"\(?\[(\d),(\d{1,2})\]\)?", match =>
            {
                var y = int.Parse(match.Groups[1].Value) + diffY;
                var x = int.Parse(match.Groups[2].Value) + diffX;
                return GetXyText(x, y);
            });
            return Result.CreateBuilder(avoText).Build();
        }

        private Result? CommandSnavo(string command, IRandomizer randomizer)
        {
            var parts = Regex.Split(command, @"/?AVO");
            if (parts.Length < 2) return null;
            var sn = parts[0];
            var avo = parts[1];
            var am = Regex.Match(avo, @"^@([2468])(.*?)$");
            if (!am.Success)
            {
                return null;
            }
            var res = CommandSn(sn, randomizer);
            if (res == null)
            {
                return null;
            }
            if (res.IsSuccess)
            {
                var avoResult = CommandAvo("AVO" + avo);
                var appendText = "\n ＞ " + (avoResult != null ? avoResult.Text : "");
                res = Result.CreateBuilder(res.Text + appendText)
                    .SetSuccess(res.IsSuccess)
                    .SetFailure(res.IsFailure)
                    .SetCritical(res.IsCritical)
                    .SetFumble(res.IsFumble)
                    .Build();
            }
            return res;
        }

        private Result? CommandSnd(string command, IRandomizer randomizer)
        {
            var parts = Regex.Split(command, @"/?D");
            if (parts.Length < 2) return null;
            var sn = parts[0];
            var d = string.Join("D", parts.Skip(1)); // Rejoin in case there were multiple D
            // Actually, we split on first /?D only
            var firstDIdx = command.IndexOf("D", StringComparison.Ordinal);
            if (firstDIdx <= 0) return null;
            // Check for /D prefix
            if (firstDIdx > 0 && command[firstDIdx - 1] == '/')
            {
                sn = command.Substring(0, firstDIdx - 1);
                d = command.Substring(firstDIdx + 1);
            }
            else
            {
                sn = command.Substring(0, firstDIdx);
                d = command.Substring(firstDIdx + 1);
            }

            var dCheck = D_REGEXP.Match("D" + d);
            if (!dCheck.Success)
            {
                return null;
            }
            var res = CommandSn(sn, randomizer);
            if (res == null)
            {
                return null;
            }
            if (res.IsSuccess)
            {
                var dResult = CommandD("D" + d, randomizer);
                var appendText = "\n ＞ " + (dResult != null ? dResult.Text : "");
                res = Result.CreateBuilder(res.Text + appendText)
                    .SetSuccess(res.IsSuccess)
                    .SetFailure(res.IsFailure)
                    .SetCritical(res.IsCritical)
                    .SetFumble(res.IsFumble)
                    .Build();
            }
            return res;
        }

        private string GetPointsText(List<List<int[]>> points, int diffX, int diffY)
        {
            return $"[{Translate("SkynautsBouken.y")},{Translate("SkynautsBouken.x")}]=" + string.Join(",", points.Select(list => string.Join("", list.Select(xy => GetXyText(xy[0] + diffX, xy[1] + diffY)))));
        }

        private string GetXyText(int x, int y)
        {
            if (x >= 2 && x <= 12 && y >= 1 && y <= 6)
            {
                return $"[{y},{x}]";
            }
            else
            {
                return $"([{y},{x}])";
            }
        }

        private List<List<int[]>> GetFirePoints(int fireCount, string fireRange, IRandomizer randomizer)
        {
            var range = fireRange.Select(c => c - '0').Where(r => r >= 0 && r <= 9).ToList();
            var result = new List<List<int[]>>();
            for (int i = 0; i < fireCount; i++)
            {
                var y = randomizer.RollOnce(6);
                var x = randomizer.RollSum(2, 6);
                var list = new List<int[]> { new[] { x, y } };
                foreach (var r in range)
                {
                    if (DIRECTION_INFOS.ContainsKey(r))
                    {
                        var dirInfo = DIRECTION_INFOS[r];
                        list.Add(new[] { x + dirInfo.DiffX, y + dirInfo.DiffY });
                    }
                }
                result.Add(list);
            }
            return result;
        }

    }
}
