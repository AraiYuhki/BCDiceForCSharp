using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ワールド・オブ・ダークネス
    /// </summary>
    public sealed class WorldOfDarkness : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly WorldOfDarkness Instance = new WorldOfDarkness();

        private static readonly Regex WodRegex = new Regex(
            @"^(\d+)(ST[SAB]?)(\d+)?([+-]\d+)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "WorldOfDarkness";

        /// <inheritdoc/>
        public override string Name => "ワールド・オブ・ダークネス";

        /// <inheritdoc/>
        public override string SortKey => "わあるとおふたあくねす";

        /// <inheritdoc/>
        public override string HelpMessage => @"
・判定コマンド(xSTn+y or xSTSn+y or xSTAn+y)
　(ダイス個数)ST(難易度)+(自動成功)
　(ダイス個数)STS(難易度)+(自動成功) ※出目10で振り足し、振り足し分の出目1で打ち消されない
　(ダイス個数)STB(難易度)+(自動成功) ※出目10で振り足し、振り足し分の出目1で打ち消される
　(ダイス個数)STA(難易度)+(自動成功) ※出目10は2成功 [20thルール]

　難易度=省略時6
　自動成功=省略時0、出目1で打ち消されない自動成功を指定
";

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var md = WodRegex.Match(command);
            if (!md.Success) return null;

            var dicePool = int.Parse(md.Groups[1].Value);
            var enabledReroll = false;
            var enabledRerollWithBotch = false;
            var enabled20th = false;

            switch (md.Groups[2].Value.ToUpperInvariant())
            {
                case "STS":
                    enabledReroll = true;
                    break;
                case "STA":
                    enabled20th = true;
                    break;
                case "STB":
                    enabledRerollWithBotch = true;
                    break;
            }

            var difficulty = md.Groups[3].Success ? int.Parse(md.Groups[3].Value) : 6;
            var autoSuccess = md.Groups[4].Success ? int.Parse(md.Groups[4].Value) : 0;

            if (difficulty < 2)
            {
                difficulty = 6;
            }

            var sequence = new List<string>();
            sequence.Add($"DicePool={dicePool}, Difficulty={difficulty}, AutomaticSuccess={autoSuccess}");

            // 出力では Difficulty=11..12 もあり得る
            if (difficulty > 10)
            {
                difficulty = 10;
            }

            var totalSuccess = 0;
            var totalBotch = 0;
            var onceSuccess = false;

            var (dice, tenSuccess, success, botch) = RollWod(dicePool, difficulty, randomizer);
            sequence.Add(string.Join(",", dice));
            totalSuccess += success;
            totalBotch += botch;

            // 成功がひとつでもあったか覚えておく
            if (success > 0 || tenSuccess > 0)
            {
                onceSuccess = true;
            }

            if (enabled20th)
            {
                // 20周年記念版なら10の目は2成功扱い
                totalSuccess += tenSuccess * 2;
            }
            else
            {
                // Revised Editionでは10は1成功と数える
                totalSuccess += tenSuccess;

                // 振り足し判定ありなら10が出ただけ振り足しを行う
                if (enabledReroll || enabledRerollWithBotch)
                {
                    while (tenSuccess > 0)
                    {
                        var (dice2, tenSuccess2, success2, botch2) = RollWod(tenSuccess, difficulty, randomizer);
                        sequence.Add(string.Join(",", dice2));
                        totalSuccess += success2 + tenSuccess2;

                        if (enabledRerollWithBotch)
                        {
                            // 振り足しでのボッチありなら出目1をカウントする
                            totalBotch += botch2;
                        }

                        tenSuccess = tenSuccess2;
                    }
                }
            }

            totalSuccess -= Math.Min(totalSuccess, totalBotch);
            totalSuccess += autoSuccess; // 意志力による自動成功は打ち消されない

            if (totalSuccess > 0)
            {
                sequence.Add($"成功数{totalSuccess}");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                    .SetSuccess(true)
                    .Build();
            }
            else if (totalBotch > 0 && !onceSuccess)
            {
                // ボッチが存在し、かつ成功がひとつもない場合のみ大失敗
                sequence.Add("大失敗");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                    .SetFumble(true)
                    .SetFailure(true)
                    .Build();
            }
            else
            {
                sequence.Add("失敗");
                return Result.CreateBuilder(string.Join(" ＞ ", sequence))
                    .SetFailure(true)
                    .Build();
            }
        }

        /// <summary>
        /// 出目10と1、難易度以上が出た成功の目をカウントする。
        /// それぞれの解釈はバージョンによって異なるため、呼び出し元で行う。
        /// </summary>
        private (List<int> dice, int tenSuccess, int success, int botch) RollWod(int dicePool, int difficulty, IRandomizer randomizer)
        {
            var dice = new List<int>();
            for (int i = 0; i < dicePool; i++)
            {
                dice.Add(randomizer.RollOnce(10));
            }
            dice.Sort();

            var success = 0;
            var botch = 0;
            var tenSuccess = 0;

            foreach (var d in dice)
            {
                if (d == 10)
                {
                    tenSuccess += 1;
                }
                else if (d >= difficulty && d < 10)
                {
                    success += 1;
                }
                else if (d == 1)
                {
                    botch += 1;
                }
            }

            return (dice, tenSuccess, success, botch);
        }
    }
}
