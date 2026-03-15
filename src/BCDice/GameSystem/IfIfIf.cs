using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// イフ・イフ・イフ
    /// </summary>
    public sealed class IfIfIf : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly IfIfIf Instance = new IfIfIf();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "IfIfIf";

        /// <inheritdoc/>
        public override string Name => "イフ・イフ・イフ";

        /// <inheritdoc/>
        public override string SortKey => "いふいふいふ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        100の質問：HQT
        ハイリスク歪み表：HDT
        ローリスク歪み表：LDT
          「HQT1」「LDT4」のように、表の後ろに数字を付けることで、10個の表から個別に振ることができます。
        致命的な歪みの回避：ID
        改竄判定：IM
        強制改竄判定：IE
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CheckAction(command, randomizer) ?? RollTables(command, TABLES);
        }

        private Result? CheckAction(string command, IRandomizer randomizer)
        {
            int[] dicearr;
            int successCount;
            switch (command)
            {
                case "ID":
                {
                    return EvalCommonCommand("3TY6", randomizer);
                }
                case "IE":
                {
                    dicearr = randomizer.RollBarabara(3, 6);
                    bool success = dicearr.Count(v => v == 6) == 3;
                    successCount = dicearr.Count(v => v == 6);
                    if (success)
                    {
                        return Result.CreateBuilder($"(3B6=6).Build() ＞ {string.Join(",", dicearr)} ＞ 6の数:{successCount}").SetSuccess(true).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder($"(3B6=6).Build() ＞ {string.Join(",", dicearr)} ＞ 6の数:{successCount} 不足数:{3 - successCount}").SetFailure(true).Build();
                    }
                }
                case "IM":
                {
                    dicearr = randomizer.RollBarabara(3, 6);
                    successCount = dicearr.Count(v => v == 6);
                    if (successCount == 0)
                    {
                        return Result.CreateBuilder($"(3B6=6).Build() ＞ {string.Join(",", dicearr)} ＞ 6の数:{successCount} 選べるエンド数:{successCount + 1}").SetFailure(true).Build();
                    }
                    else
                    {
                        return Result.CreateBuilder($"(3B6=6).Build() ＞ {string.Join(",", dicearr)} ＞ 6の数:{successCount} 選べるエンド数:{successCount + 1}").SetSuccess(true).Build();
                    }
                }
                default:
                {
                    return null;
                }
            }
        }

    }
}
