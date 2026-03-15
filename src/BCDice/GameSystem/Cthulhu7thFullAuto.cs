using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// Cthulhu7thの自動火器射撃判定
    /// </summary>
    internal static class Cthulhu7thFullAuto
    {
        private const int BonusDiceMin = -2;
        private const int BonusDiceMax = 2;

        private static readonly Regex FullAutoRegex = new Regex(
            @"^FAR\((-?\d+),(-?\d+),(-?\d+)(?:,(-?\d+)?)?(?:,(-?\w+)?)?(?:,(-?\d+)?)?\)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Dictionary<string, int> DifficultyThreshold = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "r", 0 },  // レギュラー
            { "h", 1 },  // ハード
            { "e", 2 }   // イクストリーム
        };

        public static Result? Eval(string command, IRandomizer randomizer)
        {
            var match = FullAutoRegex.Match(command);
            if (!match.Success)
            {
                return null;
            }

            int bulletCount = int.Parse(match.Groups[1].Value);
            int diff = int.Parse(match.Groups[2].Value);
            int brokenNumber = int.Parse(match.Groups[3].Value);
            int bonusDiceCount = match.Groups[4].Success && !string.IsNullOrEmpty(match.Groups[4].Value)
                ? int.Parse(match.Groups[4].Value)
                : 0;
            string stopCount = match.Groups[5].Success ? match.Groups[5].Value.ToLowerInvariant() : "";
            int bulletSetCountCap = match.Groups[6].Success && !string.IsNullOrEmpty(match.Groups[6].Value)
                ? int.Parse(match.Groups[6].Value)
                : diff / 10;

            var output = new StringBuilder();

            // 弾数上限チェック
            const int bulletCountLimit = 100;
            if (bulletCount > bulletCountLimit)
            {
                output.AppendLine($"弾薬が多すぎます。装填された弾薬を{bulletCountLimit}発に変更します。");
                bulletCount = bulletCountLimit;
            }

            // ボレーの上限チェック
            bool hasBulletSetParam = match.Groups[6].Success && !string.IsNullOrEmpty(match.Groups[6].Value);
            if (bulletSetCountCap > diff / 10 && diff > 39 && hasBulletSetParam)
            {
                bulletSetCountCap = diff / 10;
                output.AppendLine($"ボレーの弾丸の数の上限は[技能値÷10（切り捨て）]発なので、それより高い数を指定できません。ボレーの弾丸の数を{bulletSetCountCap}発に変更します。");
            }
            else if (diff <= 39 && bulletSetCountCap > 3 && hasBulletSetParam)
            {
                bulletSetCountCap = 3;
                output.AppendLine($"技能値が39以下ではボレーの弾丸の数の上限および下限は3発です。ボレーの弾丸の数を{bulletSetCountCap}発に変更します。");
            }

            // ボレーの下限チェック
            if (bulletSetCountCap <= 0 && hasBulletSetParam)
            {
                return Result.CreateBuilder("ボレーの弾丸の数は正の数です。").Build();
            }

            if (bulletSetCountCap < 3 && hasBulletSetParam)
            {
                bulletSetCountCap = 3;
                output.AppendLine("ボレーの弾丸の数の下限は3発です。ボレーの弾丸の数を3発に変更します。");
            }

            if (bulletCount <= 0)
            {
                return Result.CreateBuilder("弾薬は正の数です。").Build();
            }

            if (diff <= 0)
            {
                return Result.CreateBuilder("目標値は正の数です。").Build();
            }

            if (brokenNumber < 0)
            {
                output.AppendLine("故障ナンバーは正の数です。マイナス記号を外します。");
                brokenNumber = Math.Abs(brokenNumber);
            }

            if (bonusDiceCount < BonusDiceMin || bonusDiceCount > BonusDiceMax)
            {
                return Result.CreateBuilder($"エラー。ボーナス・ペナルティダイスの値は{BonusDiceMin}～{BonusDiceMax}です。").Build();
            }

            output.Append($"ボーナス・ペナルティダイス[{bonusDiceCount}]");
            output.Append(RollFullAuto(bulletCount, diff, brokenNumber, bonusDiceCount, stopCount, bulletSetCountCap, randomizer));

            return Result.CreateBuilder(output.ToString())
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private static string RollFullAuto(int bulletCount, int diff, int brokenNumber, int diceNum,
            string stopCount, int bulletSetCountCap, IRandomizer randomizer)
        {
            var output = new StringBuilder();
            int loopCount = 0;

            int hitBullet = 0;
            int impaleBullet = 0;
            int bullet = bulletCount;

            // 難易度変更用ループ
            for (int moreDifficulty = 0; moreDifficulty < 4; moreDifficulty++)
            {
                output.Append(GetNextDifficultyMessage(moreDifficulty));

                // ペナルティダイスを減らしながらロール用ループ
                while (diceNum >= BonusDiceMin)
                {
                    loopCount++;
                    var (hitResult, total, totalList) = GetHitResultInfos(diceNum, diff, moreDifficulty, randomizer);
                    output.Append($"\n{loopCount}回目: ＞ {string.Join(", ", totalList)} ＞ {hitResult}");

                    if (total >= brokenNumber)
                    {
                        output.Append("　ジャム");
                        return GetHitResultText(output.ToString(), hitBullet, impaleBullet, bullet);
                    }

                    var hitType = GetHitType(moreDifficulty, hitResult);
                    var (hitBulletCount, impaleBulletCount, lostBulletCount) =
                        GetBulletResults(bullet, hitType, diff, bulletSetCountCap);

                    output.Append($"　（{hitBulletCount}発が命中、{impaleBulletCount}発が貫通）");

                    hitBullet += hitBulletCount;
                    impaleBullet += impaleBulletCount;
                    bullet -= lostBulletCount;

                    if (bullet <= 0)
                    {
                        return GetHitResultText(output.ToString(), hitBullet, impaleBullet, bullet);
                    }

                    diceNum--;
                }

                // 指定された難易度となった場合、連射処理を途中で止める
                if (ShouldStopRollFullAuto(stopCount, moreDifficulty))
                {
                    output.Append("\n【指定の難易度となったので、処理を終了します。】");
                    break;
                }

                diceNum++;
            }

            return GetHitResultText(output.ToString(), hitBullet, impaleBullet, bullet);
        }

        private static bool ShouldStopRollFullAuto(string stopCount, int difficulty)
        {
            if (string.IsNullOrEmpty(stopCount))
            {
                return false;
            }

            if (DifficultyThreshold.TryGetValue(stopCount, out int threshold))
            {
                return difficulty >= threshold;
            }

            return false;
        }

        private static (string HitResult, int Total, int[] TotalList) GetHitResultInfos(
            int diceNum, int diff, int moreDifficulty, IRandomizer randomizer)
        {
            var (total, totalList) = Cthulhu7th.RollWithBonus(diceNum, randomizer);

            bool fumbleable = GetFumbleable(moreDifficulty);
            var hitResult = ResultLevel.FromValues(total, diff, fumbleable).ToString();

            return (hitResult, total, totalList);
        }

        private static string GetHitResultText(string output, int hitBullet, int impaleBullet, int bullet)
        {
            return $"{output}\n＞ {hitBullet}発が通常命中、{impaleBullet}発が貫通、残弾{bullet}発";
        }

        private static HitType GetHitType(int moreDifficulty, string hitResult)
        {
            var (successList, impaleBulletList) = GetSuccessListImpaleBulletList(moreDifficulty);

            if (Array.IndexOf(successList, hitResult) >= 0)
            {
                return HitType.Hit;
            }

            if (Array.IndexOf(impaleBulletList, hitResult) >= 0)
            {
                return HitType.Impale;
            }

            return HitType.None;
        }

        private static (int HitBullet, int ImpaleBullet, int LostBullet) GetBulletResults(
            int bulletCount, HitType hitType, int diff, int bulletSetCountCap)
        {
            int bulletSetCount = GetSetOfBullet(diff, bulletSetCountCap);
            int hitBulletCountBase = GetHitBulletCountBase(diff, bulletSetCount);
            double impaleBulletCountBase = bulletSetCount / 2.0;

            int lostBulletCount = 0;
            int hitBulletCount = 0;
            int impaleBulletCount = 0;

            if (!IsLastBulletTurn(bulletCount, bulletSetCount))
            {
                switch (hitType)
                {
                    case HitType.Hit:
                        hitBulletCount = hitBulletCountBase;
                        break;

                    case HitType.Impale:
                        impaleBulletCount = (int)Math.Floor(impaleBulletCountBase);
                        hitBulletCount = (int)Math.Ceiling(impaleBulletCountBase);
                        break;
                }

                lostBulletCount = bulletSetCount;
            }
            else
            {
                switch (hitType)
                {
                    case HitType.Hit:
                        hitBulletCount = GetLastHitBulletCount(bulletCount);
                        break;

                    case HitType.Impale:
                        impaleBulletCount = GetLastHitBulletCount(bulletCount);
                        hitBulletCount = bulletCount - impaleBulletCount;
                        break;
                }

                lostBulletCount = bulletCount;
            }

            return (hitBulletCount, impaleBulletCount, lostBulletCount);
        }

        private static (string[] SuccessList, string[] ImpaleBulletList) GetSuccessListImpaleBulletList(int moreDifficulty)
        {
            return moreDifficulty switch
            {
                0 => (new[] { "ハード成功", "レギュラー成功" }, new[] { "クリティカル", "イクストリーム成功" }),
                1 => (new[] { "ハード成功" }, new[] { "クリティカル", "イクストリーム成功" }),
                2 => (Array.Empty<string>(), new[] { "クリティカル", "イクストリーム成功" }),
                3 => (new[] { "クリティカル" }, Array.Empty<string>()),
                _ => (Array.Empty<string>(), Array.Empty<string>())
            };
        }

        private static string GetNextDifficultyMessage(int moreDifficulty)
        {
            return moreDifficulty switch
            {
                1 => "\n【難易度がハードに変更】",
                2 => "\n【難易度がイクストリームに変更】",
                3 => "\n【難易度がクリティカルに変更】",
                _ => ""
            };
        }

        private static int GetSetOfBullet(int diff, int bulletSetCountCap)
        {
            int bulletSetCount = diff / 10;

            if (bulletSetCountCap < bulletSetCount)
            {
                bulletSetCount = bulletSetCountCap;
            }

            if (diff >= 1 && diff < 30)
            {
                bulletSetCount = 3; // 技能値が29以下での最低値保障処理
            }

            return bulletSetCount;
        }

        private static int GetHitBulletCountBase(int diff, int bulletSetCount)
        {
            int hitBulletCountBase = bulletSetCount / 2;

            if (diff >= 1 && diff < 30)
            {
                hitBulletCountBase = 1; // 技能値29以下での最低値保障
            }

            return hitBulletCountBase;
        }

        private static bool IsLastBulletTurn(int bulletCount, int bulletSetCount)
        {
            return (bulletCount - bulletSetCount) < 0;
        }

        private static int GetLastHitBulletCount(int bulletCount)
        {
            // 残弾1での最低値保障処理
            if (bulletCount == 1)
            {
                return 1;
            }

            return (int)Math.Floor(bulletCount / 2.0);
        }

        private static bool GetFumbleable(int moreDifficulty)
        {
            // 成功が49以下の出目のみとなるため、ファンブル値は上昇
            return moreDifficulty >= 1;
        }

        private enum HitType
        {
            None,
            Hit,
            Impale
        }
    }
}
