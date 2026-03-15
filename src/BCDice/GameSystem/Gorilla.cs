using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゴリラTRPG
    /// </summary>
    public sealed class Gorilla : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Gorilla Instance = new Gorilla();

        /// <inheritdoc/>
        public override string Id => "Gorilla";

        /// <inheritdoc/>
        public override string Name => "ゴリラTRPG";

        /// <inheritdoc/>
        public override string SortKey => "こりらTRPG";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        2D6ロール時のゴリティカル自動判定を行います。

        G = 2D6のショートカット

        例) G>=7 : 2D6して7以上なら成功
        ";

        /// <inheritdoc/>
        public override string ChangeText(string text)
        {
            text = Regex.Replace(text, @"^(S)?G", m => $"{m.Groups[1].Value}2D6", RegexOptions.IgnoreCase);
            return text;
        }

        /// <summary>
        /// 2D6の判定結果を返す
        /// </summary>
        protected Result? EvalResult2D6(int total, int diceTotal, IReadOnlyList<int> diceList, CompareOperator cmpOp, int target, IRandomizer randomizer)
        {
            if (diceList.SequenceEqual(new[] { 5, 5 }))
            {
                return Result.CreateBuilder("ゴリティカル（自動的成功）").SetSuccess(true).SetCritical(true).Build();
            }
            return null;
        }
    }
}
