using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 獸ノ森
    /// </summary>
    public sealed class KemonoNoMori : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KemonoNoMori Instance = new KemonoNoMori();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();



        private static readonly Regex KaDDRegex = new Regex(
            @"KA\d[-+\d]*",
            RegexOptions.Compiled);

        private static readonly Regex KcDDRegex = new Regex(
            @"KC\d[-+\d]*",
            RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string Id => "KemonoNoMori";

        /// <inheritdoc/>
        public override string Name => "獸ノ森";

        /// <inheritdoc/>
        public override string SortKey => "けもののもり";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・行為判定(成功度自動算出)(P119): KAx[±y]
        ・継続判定(成功度+1固定): KCx[±y]
           x=目標値
           y=目標値への修正(任意) x+y-z のように複数指定可能
             例1）KA7+3 → 目標値7にプラス3の修正を加えた行為判定
             例2）KC6 → 目標値6の継続判定
        ・罠動作チェック+獲物表(P163): CTR
           罠ごとに1D12を振り、12が出た場合には生き物が罠を動作させ、その影響を受けている。
        ・各種表（基本ルールブック）
          ・大失敗表(P120): FT
          ・能力値ランダム決定表(P121): RST
          ・ランダム所要時間表(P122): RTT
          ・ランダム消耗表(P122): RET
          ・ランダム天気表(P128): RWT
          ・ランダム天気持続表(P128): RWDT
          ・ランダム遮蔽物表（屋外）(P140): ROMT
          ・ランダム遮蔽物表（屋内）(P140): RIMT
          ・逃走体験表(P144): EET
          ・食材採集表(P157): GFT
          ・水採集表(P157): GWT
          ・白の魔石効果表(P186): WST
        ・部位ダメージ関連の表（参照先ページはリプレイ&データブック「嚙神ノ宴」のもの）
          ・人間部位表(P216): HPT
          ・部位ダメージ段階表(P217): PDT
          ・四足動物部位表(P225): QPT
          ・無足動物部位表(P225): APT
          ・二足動物部位表(P226): TPT
          ・鳥部位表(P226): BPT
          ・頭足動物部位表(P227): CPT
          ・昆虫部位表(P227): IPT
          ・蜘蛛部位表(P228): SPT
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Match match;

            match = KaDDRegex.Match(command);
            if (match.Success)
            {
                return Check1d12(command, true, randomizer);
            }

            match = KcDDRegex.Match(command);
            if (match.Success)
            {
                return Check1d12(command, false, randomizer);
            }

            if (command == "CTR")
            {
                return GetTrapResult(randomizer);
            }

            if (command == "EET")
            {
                return GetEscapeExperienceTableResult(command, randomizer);
            }


            return base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? Check1d12(string command, bool is_action_judge, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"K[AC](\d[-+\d]*)");
            if (!m.Success)
            {
                return null;
            }

            var target_total = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor) ?? 0;
            var success_degree = is_action_judge ? target_total / 10 + 1 : 1;
            var dice_total = randomizer.RollOnce(12);

            if (dice_total == 12)
            {
                return Result.CreateBuilder($"(1D12<={target_total}).Build() ＞ {dice_total} ＞ 大失敗").SetFumble(true).SetFailure(true).Build();
            }
            else if (dice_total == 11)
            {
                return Result.CreateBuilder($"(1D12<={target_total}).Build() ＞ {dice_total} ＞ 大成功（成功度+{success_degree}, 次の継続判定の目標値を10に変更）").SetCritical(true).SetSuccess(true).Build();
            }
            else if (dice_total <= target_total)
            {
                return Result.CreateBuilder($"(1D12<={target_total}).Build() ＞ {dice_total} ＞ 成功（成功度+{success_degree}）").SetSuccess(true).Build();
            }
            else
            {
                return Result.CreateBuilder($"(1D12<={target_total}).Build() ＞ {dice_total} ＞ 失敗").SetFailure(true).Build();
            }
        }

        private Result GetTrapResult(IRandomizer randomizer)
        {
            var tra_check_num = randomizer.RollOnce(12);
            if (tra_check_num != 12)
            {
                return Result.CreateBuilder($"罠動作チェック(1D12).Build() ＞ {tra_check_num} ＞ 罠は動作していなかった").Build();
            }

            var chase_num = randomizer.RollOnce(12);
            string chase;
            switch (chase_num)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    chase = "小型動物";
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    chase = "大型動物";
                    break;
                default:
                    chase = "人間の放浪者";
                    break;
            }
            return Result.CreateBuilder($"罠動作チェック(1D12).Build() ＞ {tra_check_num} ＞ 罠が動作していた！ ＞ 獲物表({chase_num}) ＞ {chase}が罠にかかっていた").Build();
        }

        private Result? GetEscapeExperienceTableResult(string command, IRandomizer randomizer)
        {
            var tableResult = RollTable(command, TABLES, randomizer);
            if (tableResult == null)
            {
                return null;
            }

            var escape_duration = randomizer.RollOnce(12);
            return Result.CreateBuilder($"{tableResult.Text} (再登場: {escape_duration}時間後).Build()").Build();
        }

    }
}
