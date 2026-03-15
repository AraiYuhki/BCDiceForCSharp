using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 墜落世界
    /// </summary>
    public sealed class CrashWorld : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CrashWorld Instance = new CrashWorld();


        private static readonly Regex CwDRegex = new Regex(
            @"CW(\d+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override string Id => "CrashWorld";

        /// <inheritdoc/>
        public override string Name => "墜落世界";

        /// <inheritdoc/>
        public override string SortKey => "ついらくせかい";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 CWn
        初期目標値n (必須)
        例・CW8
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = CwDRegex.Match(command);
            if (match.Success)
            {
                var result = GetCrashWorldRoll(Convert.ToInt32(Regexp.LastMatch(1)), randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private string GetCrashWorldRoll(int target, IRandomizer randomizer) {
            Debug("target", target);
            var output = "(";
            var isEnd = false;
            var successness = 0;
            var num = 0;
            while (isEnd)
            {
                num = this._randomizer.RollOnce(12);
                if (output == "(")
                {
                    output = $"({num}";
                }
                else
                {
                    output = $"{output}, {num}";
                };
                if (num <= target || num == 11)
                {
                    var currentNum = num;
                    successness = 1;
                }
                else
                {
                    if (num == 12)
                    {
                        isEnd = true;
                    }
                    else
                    {
                        isEnd = true;
                    }
                }
            }
            if (num == 12)
            {
                successness = 0;
            }
            output = $"{output})  成功度 : {successness}";
            if (num == 12)
            {
                output = $"{output} ファンブル";
            }
            return output;
        }

    }
}