using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 歯車の塔の探空士（六畳間幻想空間）
    /// </summary>
    public sealed class Skynauts : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Skynauts Instance = new Skynauts();

        /// <inheritdoc/>
        public override string Id => "Skynauts";

        /// <inheritdoc/>
        public override string Name => "歯車の塔の探空士（六畳間幻想空間）";

        /// <inheritdoc/>
        public override string SortKey => "はくるまのとうのすかいのおつ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ◆判定　(SNn)、(2D6<=n)　n:目標値（省略時:7）
        　例）SN5　SN5　SN(3+2)
        ◆航行チェック　(NV+n)　n:修正値（省略時:0）
        　例）NV　NV+1
        ◆ダメージチェック　(Dx/y@m)　x:ダメージ左側の値、y:ダメージ右側の値
        　m:《弾道学》（省略可）上:8、下:2、左:4、右:6
        　飛空艇シート外の座標は()が付きます。
        　例） D/4　D19/2　D/3@8　D[大揺れ]/2
        ◆砲撃判定+ダメージチェック　(BOMn/Dx/y@m)　n:目標値（省略時:7）
        　x:ダメージ左側の値、y:ダメージ右側の値
        　m:《弾道学》（省略可）上:8、下:2、左:4、右:6
        　例） BOM/D/4　BOM9/D19/2@4
        ◆《回避運動》　(AVOn@mXX)　n:目標値（省略時:7）
        　m:回避方向。上:8、下:2、左:4、右:6、XX：ダメージチェック結果
        　例）
        　AVO9@8[縦1,横4],[縦2,横6],[縦3,横8]　AVO@2[縦6,横4],[縦2,横6]
        ";

        /// <summary>
        /// 方向情報の辞書
        /// </summary>
        private static readonly Dictionary<int, (string Name, int DiffX, int DiffY)> DIRECTION_INFOS =
            new Dictionary<int, (string Name, int DiffX, int DiffY)>
            {
                { 1, ("左下", -1, +1) },
                { 2, ("下", 0, +1) },
                { 3, ("右下", +1, +1) },
                { 4, ("左", -1, 0) },
                // 5 は中央。算出する意味がないので対象外
                { 6, ("右", +1, 0) },
                { 7, ("左上", -1, -1) },
                { 8, ("上", 0, -1) },
                { 9, ("右上", +1, -1) },
            };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return GetJudgeResult(command, randomizer)
                ?? NavigationResult(command, randomizer)
                ?? GetFireResult(command, randomizer)
                ?? GetBombResult(command, randomizer)
                ?? GetAvoidResult(command, randomizer);
        }

        private Result? GetJudgeResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^2D6<=(\d)$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                m = Regex.Match(command, @"^SN(\d*)$", RegexOptions.IgnoreCase);
            }
            if (!m.Success)
            {
                return null;
            }

            var target = string.IsNullOrEmpty(m.Groups[1].Value) ? 7 : Convert.ToInt32(m.Groups[1].Value);
            var diceList = randomizer.RollBarabara(2, 6);
            var total = diceList.Sum();
            var text = $"(2D6<={target}) ＞ {total}[{string.Join(",", diceList)}] ＞ {total}";

            if (total <= 2)
            {
                return Result.CreateBuilder(text + " ＞ ファンブル").SetFumble(true).SetFailure(true).Build();
            }
            else if (total <= target)
            {
                return Result.CreateBuilder(text + " ＞ 成功").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder(text + " ＞ 失敗").SetFailure(true).Build();
            }
        }

        private Result? NavigationResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^NV(\+(\d+))?$");
            if (!m.Success)
            {
                return null;
            }

            var bonus = string.IsNullOrEmpty(m.Groups[2].Value) ? 0 : Convert.ToInt32(m.Groups[2].Value);
            var total = randomizer.RollOnce(6);
            var movePointBase = (total / 2) <= 0 ? 1 : (total / 2);
            var movePoint = movePointBase + bonus;

            return Result.CreateBuilder($"航行チェック(最低1)　(1D6/2+{bonus}) ＞ {total} /2+{bonus} ＞ {movePointBase}+{bonus} ＞ {movePoint}エリア進む").Build();
        }

        private string GetDirectionName(int direction, string defaultValue = "")
        {
            if (DIRECTION_INFOS.TryGetValue(direction, out var info))
            {
                return info.Name;
            }
            return defaultValue;
        }

        private (int DiffX, int DiffY) GetDirectionPositionDiff(int direction)
        {
            if (DIRECTION_INFOS.TryGetValue(direction, out var info))
            {
                return (info.DiffX, info.DiffY);
            }
            return (0, 0);
        }

        private Result? GetFireResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^D([12346789]*)(\[.+\])*/(\d{1,2})(@([2468]))?$");
            if (!m.Success)
            {
                return null;
            }

            var fireCount = Convert.ToInt32(m.Groups[3].Value);
            var fireRange = m.Groups[1].Value;
            var ballistics = string.IsNullOrEmpty(m.Groups[5].Value) ? 0 : Convert.ToInt32(m.Groups[5].Value);

            var firePoint = GetFirePoint(fireRange, fireCount, randomizer);
            var result = new List<string> { command, GetFirePointText(firePoint, fireCount).Text };

            if (ballistics != 0)
            {
                result.Add($"《弾道学》:{GetDirectionName(ballistics)}\n");
                result.Add(GetFirePointText(firePoint, fireCount, ballistics).Text);
            }

            return Result.CreateBuilder(string.Join(" ＞ ", result)).Build();
        }

        private List<List<int[]>> GetFirePoint(string fireRange, int fireCount, IRandomizer randomizer)
        {
            var firePoint = new List<List<int[]>>();

            for (var count = 0; count < fireCount; count++)
            {
                firePoint.Add(new List<int[]>());

                var yPos = randomizer.RollOnce(6);
                var xPos = randomizer.RollSum(2, 6);
                var position = new int[] { xPos, yPos };

                firePoint[firePoint.Count - 1].Add(position);

                foreach (var rangeChar in fireRange.ToCharArray())
                {
                    var rangeNum = (int)char.GetNumericValue(rangeChar);
                    var diff = GetDirectionPositionDiff(rangeNum);
                    var rangePosition = new int[] { xPos + diff.DiffX, yPos + diff.DiffY };
                    firePoint[firePoint.Count - 1].Add(rangePosition);
                }
            }

            return firePoint;
        }

        private Result GetFirePointText(List<List<int[]>> firePoint, int fireCount, int direction = 0)
        {
            var fireTextList = new List<string>();

            foreach (var point in firePoint)
            {
                var text = "";
                foreach (var pos in point)
                {
                    var x = pos[0];
                    var y = pos[1];
                    var (movedX, movedY) = GetMovePoint(x, y, direction);
                    text += InMapPosition(movedX, movedY)
                        ? $"[縦{movedY},横{movedX}]"
                        : $"([縦{movedY},横{movedX}])";
                }
                fireTextList.Add(text);
            }

            return Result.CreateBuilder(string.Join(",", fireTextList)).Build();
        }

        private bool InMapPosition(int x, int y)
        {
            return (1 <= y && y <= 6) && (2 <= x && x <= 12);
        }

        private (int X, int Y) GetMovePoint(int x, int y, int direction)
        {
            var diff = GetDirectionPositionDiff(direction);
            x += diff.DiffX;
            y += diff.DiffY;
            return (x, y);
        }

        private Result? GetBombResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^BOM(\d*)?/D([12346789]*)(\[.+\])*/(\d+)(@([2468]))?$", RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return null;
            }

            var target = m.Groups[1].Value;
            var direction = string.IsNullOrEmpty(m.Groups[6].Value) ? 0 : Convert.ToInt32(m.Groups[6].Value);

            var sn = GetJudgeResult("SN" + target, randomizer);
            if (sn == null)
            {
                return null;
            }

            if (sn.IsFailure)
            {
                return Result.CreateBuilder($"{command} ＞ {sn.Text}").SetFailure(true).Build();
            }

            // ダメージチェック部分
            var fireCommandMatch = Regex.Match(command, @"D([12346789]*)(\[.+\])*/(\d+)(@([2468]))?");
            if (!fireCommandMatch.Success)
            {
                return sn;
            }
            var fireCommand = fireCommandMatch.Value;
            var fireResult = GetFireResult(fireCommand, randomizer);

            if (fireResult == null)
            {
                return sn;
            }

            return Result.CreateBuilder($"{command} ＞ {sn.Text}\n ＞ {fireResult.Text}")
                .SetSuccess(sn.IsSuccess)
                .Build();
        }

        private Result? GetAvoidResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^AVO(\d*)?(@([2468]))(\(?\[縦\d+,横\d+\]\)?,?)+$");
            if (!m.Success)
            {
                return null;
            }

            var direction = Convert.ToInt32(m.Groups[3].Value);

            // 判定部分
            var judgeMatch = Regex.Match(command, @"^AVO(\d*)?(@([2468]))");
            var judgeCommand = judgeMatch.Value;
            var targetStr = judgeMatch.Groups[1].Value;
            var sn = GetJudgeResult("SN" + targetStr, randomizer);

            if (sn == null)
            {
                return null;
            }

            if (sn.IsFailure)
            {
                return Result.CreateBuilder($"{judgeCommand} ＞ 《回避運動》{sn.Text}").SetFailure(true).Build();
            }

            // 砲撃座標部分
            var pointMatch = Regex.Match(command, @"(\(?\[縦\d+,横\d+\]\)?,?)+");
            var pointCommand = pointMatch.Value;

            var firePoint = ScanFirePoint(pointCommand);
            var fireCount = firePoint.Count;

            var parts = new List<string>
            {
                judgeCommand,
                $"《回避運動》{sn.Text}\n",
                pointCommand,
                "《回避運動》:" + GetDirectionName(direction) + "\n",
                GetFirePointText(firePoint, fireCount, direction).Text
            };

            return Result.CreateBuilder(string.Join(" ＞ ", parts.Where(x => x != null))).SetSuccess(true).Build();
        }

        private List<List<int[]>> ScanFirePoint(string command)
        {
            // 正規表現が大変なので最初に括弧を外しておく
            command = Regex.Replace(command, @"\(|\)", "");

            var firePoint = new List<List<int[]>>();

            // 一組ずつに分ける("[縦y,横x]"の単位)
            foreach (var pointText in command.Split(new[] { "]," }, StringSplitOptions.None))
            {
                firePoint.Add(new List<int[]>());

                // D以外の砲撃範囲がある時に必要
                foreach (var point in pointText.Split(new[] { ']' }))
                {
                    firePoint[firePoint.Count - 1].Add(new int[] { 0, 0 });

                    var pointMatch = Regex.Match(point, @"[^\d]*(\d+),[^\d]*(\d+)");
                    if (!pointMatch.Success)
                    {
                        continue;
                    }

                    var y = Convert.ToInt32(pointMatch.Groups[1].Value);
                    var x = Convert.ToInt32(pointMatch.Groups[2].Value);

                    firePoint[firePoint.Count - 1][firePoint[firePoint.Count - 1].Count - 1] = new int[] { x, y };
                }
            }

            return firePoint;
        }
    }
}
