using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 央華封神RPG 第三版
    /// </summary>
    public sealed class Oukahoushin3rd : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Oukahoushin3rd Instance = new Oukahoushin3rd();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "Oukahoushin3rd";

        /// <inheritdoc/>
        public override string Name => "央華封神RPG 第三版";

        /// <inheritdoc/>
        public override string SortKey => "おうかほうしんRPG3";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・各種表
        　・能力値判定裏成功表（NHT）
        　・武器攻撃裏成功表（BKT）
        　・受け・回避裏成功表（UKT）
        　・仙術行使裏成功表（SKT）
        　・仙術抵抗裏成功表（STT）
        　・精神値ダメージ悪影響表（SDT）
        　・狂気表（KKT）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var chosen = RollTable(command, TABLES, randomizer);
            var text = ReplaceDiceNotation(chosen?.Text, randomizer);
            if (text == null) return null;
            return Result.CreateBuilder(text).Build();

            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private string ReplaceDiceNotation(string text, IRandomizer randomizer)
        {
            if (text == null) return null;
            return Regex.Replace(text, @"(\d+)D(\d+)", m =>
            {
                var parts = m.Value.Split('D');
                var times = int.Parse(parts[0]);
                var sides = int.Parse(parts[1]);
                var value = randomizer.RollSum(times, sides);
                return $"{m.Value}(=>{value})";
            });
        }

    }
}