using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class SamsaraBallad : GameSystemBase
    {
        public static readonly SamsaraBallad Instance = new SamsaraBallad();
        public override string Id => "SamsaraBallad";
        public override string Name => "サンサーラ・バラッド";
        public override string SortKey => "さんさあらはらつと";

        private static readonly Regex SbReg = new Regex(
            @"^(SBS?)(#(\d+))?(@(\d+))?(<=?|<)(\d+)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = SbReg.Match(command);
            if (!m.Success) return null;

            bool isSwap = m.Groups[1].Value.ToUpper() == "SBS";
            int? fumble = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : (int?)null;
            int? critical = m.Groups[5].Success ? int.Parse(m.Groups[5].Value) : (int?)null;
            string cmpOp = m.Groups[6].Value;
            int target = int.Parse(m.Groups[7].Value);

            string? placesText = null;
            int total;

            if (!isSwap)
            {
                total = randomizer.RollOnce(100);
            }
            else
            {
                int a = randomizer.RollOnce(10);
                int b = randomizer.RollOnce(10);
                placesText = a + "," + b;
                var places = new[] { a == 10 ? 0 : a, b == 10 ? 0 : b }.OrderBy(x => x).ToArray();
                total = places[0] * 10 + places[1];
                if (total == 0) total = 100;
            }

            bool success = cmpOp == "<=" ? total <= target : total < target;
            bool isFumb = fumble.HasValue && (total % 10 <= fumble.Value);
            bool isCrit = critical.HasValue && (total % 10 >= critical.Value);

            string cmdStr = "D100" + cmpOp + target;
            if (fumble.HasValue) cmdStr = "D100#" + fumble.Value + (critical.HasValue ? "@" + critical.Value : "") + cmpOp + target;
            else if (critical.HasValue) cmdStr = "D100@" + critical.Value + cmpOp + target;

            var parts = new List<string>();
            parts.Add("(" + cmdStr + ")");
            if (placesText != null) parts.Add(placesText);
            parts.Add(total.ToString());
            if (success) parts.Add("成功");
            else parts.Add("失敗");
            if (isFumb) parts.Add("ファンブル");
            else if (isCrit && success) parts.Add("クリティカル");

            string text = string.Join(" ＞ ", parts);

            return Result.CreateBuilder(text)
                .SetCondition(success)
                .SetCritical(isCrit && success)
                .SetFumble(isFumb)
                .Build();
        }
    }
}
