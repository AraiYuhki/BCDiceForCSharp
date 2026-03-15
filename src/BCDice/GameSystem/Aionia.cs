using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 慈悲なきアイオニア
    /// </summary>
    public sealed class Aionia : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Aionia Instance = new Aionia();

        /// <inheritdoc/>
        public override string Id => "Aionia";

        /// <inheritdoc/>
        public override string Name => "慈悲なきアイオニア";

        /// <inheritdoc/>
        public override string SortKey => "しひなきあいおにあ";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        - 技能判定（クリティカル・ファンブルなし）
        AB{n}>={dif} n=10面ダイスの数、dif=難易度
        - 技能判定（クリティカル・ファンブルあり）
        ABT{n}>={dif} n=10面ダイスの数、dif=難易度

        例:AB2>=5        （一般技能を活用して難易度5の技能判定。 クリファンなし。）
        例:ABT3>=15      （専門技能を活用して難易度15の技能判定。クリファンあり。）
        例:AB1+2>=8      （一般技能を活用せず難易度8の技能判定。 ボーナスとして+2点の補正あり。  クリファンなし。）
        例:ABT3-3>=10    （専門技能を活用して難易度10の技能判定。ペナルティとして-3点の補正あり。クリファンあり。）
        例:ABT2>=4/8/12  （一般技能を活用して難易度4/8/12の段階的な技能判定。クリファンあり。）
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollSkills(command, randomizer);
        }

        private Result? RollSkills(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"AB(T?)(\d+)([+-]\d+)?>=((\d+)((/\d+)*))");
            if (!m.Success)
            {
                return null;
            }

            var use_cf = m.Groups[1].Value != "";
            var times = Convert.ToInt32(m.Groups[2].Value);
            var bonus = m.Groups[3].Success && m.Groups[3].Value != "" ? Convert.ToInt32(m.Groups[3].Value) : 0;
            var targets = m.Groups[4].Value.Split('/').Select(x => int.Parse(x)).ToArray();
            var target = targets[0];
            var min_target = targets.Min();
            var max_target = targets.Max();
            var dice_list = randomizer.RollBarabara(times, 10);
            var dice_total = dice_list.Sum();
            var total = dice_total + bonus;
            var is_success = false;
            var has_critical = false;
            var has_fumble = false;
            string result;
            if (targets.Length == 1)
            {
                if (total >= target)
                {
                    is_success = true;
                    if (total >= target + 20 && use_cf)
                    {
                        result = "クリティカル";
                        has_critical = true;
                    }
                    else
                    {
                        if (target <= times)
                        {
                            result = "自動成功";
                        }
                        else
                        {
                            result = "成功";
                        }
                    }
                }
                else
                {
                    is_success = false;
                    if (dice_list.Count(v => v == 1) == times && use_cf)
                    {
                        result = "ファンブル";
                        has_fumble = true;
                    }
                    else
                    {
                        if (target > 10 * times)
                        {
                            result = "自動失敗";
                        }
                        else
                        {
                            result = "失敗";
                        }
                    }
                }
            }
            else
            {
                if (total >= min_target)
                {
                    is_success = true;
                    if (total >= max_target + 20 && use_cf)
                    {
                        result = "クリティカル";
                        has_critical = true;
                    }
                    else
                    {
                        if (max_target <= times)
                        {
                            result = "自動成功";
                        }
                        else
                        {
                            if (total >= max_target)
                            {
                                result = "全成功";
                            }
                            else
                            {
                                var times_suc = targets.Count(x => x <= total);
                                result = $"{times_suc}段階成功";
                            }
                        }
                    }
                }
                else
                {
                    is_success = false;
                    if (dice_list.Count(v => v == 1) == times && use_cf)
                    {
                        result = "ファンブル";
                        has_fumble = true;
                    }
                    else
                    {
                        if (min_target > 10 * times)
                        {
                            result = "自動失敗";
                        }
                        else
                        {
                            result = "失敗";
                        }
                    }
                }
            }
            var bonus_text = "";
            var bonus_result = "";
            if (bonus != 0)
            {
                if (bonus > 0)
                {
                    bonus_text = "+";
                }
                bonus_text += bonus.ToString();
                bonus_result = $"{total} ＞ ";
            }
            return Result.CreateBuilder($"({command}).Build() ＞ {dice_total}[{string.Join(",", dice_list)}]{bonus_text} ＞ {bonus_result}{result}")
                .SetCritical(has_critical)
                .SetFumble(has_fumble)
                .SetSuccess(is_success)
                .SetFailure(!is_success)
                .SetRands(randomizer.RandResults)
                .Build();
        }

    }
}
