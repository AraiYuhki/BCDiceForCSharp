using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 信長の黒い城
    /// </summary>
    public sealed class NobunagasBlackCastle : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly NobunagasBlackCastle Instance = new NobunagasBlackCastle();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "NobunagasBlackCastle";

        /// <inheritdoc/>
        public override string Name => "信長の黒い城";

        /// <inheritdoc/>
        public override string SortKey => "のふなかのくろいしろ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■判定　sDRt        s: 能力値 t:目標値

        例)+3DR12: 能力値+3、DR12で1d20を振って、その結果を表示(クリティカル・ファンブルも表示)

        ■イニシアティヴ　INS

        例)INS: 1d6を振って、イニシアティヴの結果を表示(PC先行を成功として表示)

        ■NPC能力値作成　NPCST

        例)NPCST: 3d6を4回振って、各能力値とHPを表示


        ■各種表

        ・遭遇反応表(ERT)
        ・武器表(SWT)/その他の奇妙な武器表(OSWT)
        ・鎧表(ART)

        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return ResoluteAction(command, randomizer) ?? ResoluteInitiative(command, randomizer) ?? RollTables(command, TABLES) ?? MakeNpcStatus(command, randomizer);
        }

        private Result ResultDr(int total, int dice_total, int target)
        {
            if (dice_total <= 1)
            {
                return Result.Fumble("ファンブル");
            }
            else if (dice_total >= 20)
            {
                return Result.Critical("クリティカル");
            }
            else if (total >= target)
            {
                return Result.Success("成功");
            }
            else
            {
                return Result.Failure("失敗");
            }
        }

        private Result? ResoluteAction(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^([+-]?\d*)DR(\d+)$");
            if (!m.Success)
            {
                return null;
            }

            var num_status = m.Groups[1].Success && m.Groups[1].Value != "" ? Convert.ToInt32(m.Groups[1].Value) : 0;
            var num_target = Convert.ToInt32(m.Groups[2].Value);
            var total = randomizer.RollOnce(20);
            var total_status = total.ToString() + WithSymbol(num_status);
            var result = ResultDr(total + num_status, total, num_target);

            var sequence = new object[] { $"({command})", total_status, total + num_status, result.Text };
            var text = string.Join(" ＞ ", sequence);

            return Result.CreateBuilder(text)
                .SetSuccess(result.IsSuccess)
                .SetFailure(result.IsFailure)
                .SetCritical(result.IsCritical)
                .SetFumble(result.IsFumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string WithSymbol(int number)
        {
            if (number == 0)
            {
                return "+0";
            }
            else if (number > 0)
            {
                return $"+{number}";
            }
            else
            {
                return number.ToString();
            }
        }

        private Result? ResoluteInitiative(string command, IRandomizer randomizer)
        {
            if (command != "INS")
            {
                return null;
            }

            var total = randomizer.RollOnce(6);
            Result inner = total >= 4
                ? Result.Success("PC先行")
                : Result.Failure("敵先行");

            return Result.CreateBuilder($"({command}).Build() ＞ {total} ＞ {inner.Text}")
                .SetSuccess(inner.IsSuccess)
                .SetFailure(inner.IsFailure)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? MakeNpcStatus(string command, IRandomizer randomizer)
        {
            if (command != "NPCST")
            {
                return null;
            }

            var pre = randomizer.RollSum(3, 6);
            var agi = randomizer.RollSum(3, 6);
            var str = randomizer.RollSum(3, 6);
            var tgh = randomizer.RollSum(3, 6);
            var hpd = randomizer.RollOnce(8);
            var hp = hpd + CalcStatus(tgh);
            if (hp < 1)
            {
                hp = 1;
            }
            var text = string.Join(", ", new[]
            {
                $"心{WithSymbol(CalcStatus(pre))}({pre})",
                $"技{WithSymbol(CalcStatus(agi))}({agi})",
                $"体{WithSymbol(CalcStatus(str))}({str})",
                $"耐久{WithSymbol(CalcStatus(tgh))}({tgh})",
                $"HP{hp}({hpd})",
            });
            return Result.CreateBuilder($"({command}).Build() ＞ {text}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private int CalcStatus(int st)
        {
            if (st <= 4)
            {
                return -3;
            }
            else if (st <= 6)
            {
                return -2;
            }
            else if (st <= 8)
            {
                return -1;
            }
            else if (st <= 12)
            {
                return 0;
            }
            else if (st <= 14)
            {
                return 1;
            }
            else if (st <= 16)
            {
                return 2;
            }
            else if (st <= 20)
            {
                return 3;
            }
            return 0;
        }

    }
}
