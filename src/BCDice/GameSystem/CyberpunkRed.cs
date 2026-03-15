using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// サイバーパンクRED
    /// </summary>
    public sealed class CyberpunkRed : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly CyberpunkRed Instance = new CyberpunkRed();

        /// <inheritdoc/>
        public override string Id => "CyberpunkRed";

        /// <inheritdoc/>
        public override string Name => "サイバーパンクRED";

        /// <inheritdoc/>
        public override string SortKey => "さいはあはんくれつと";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定　CPx+y>z
        　(x＝能力値と技能値の合計、y＝修正値、z＝難易度 or 受動側　x、y、zは省略可)
        　例）CP12 CP10+2>12　CP7-1　CP8+4　CP7>12　CP　CP>9

        各種表
        ・致命的損傷表
        　FFD　：身体への致命的損傷
        　HFD　：頭部への致命的損傷
        ・遭遇表
        　NCDT　：ナイトシティ(日中)
        　NCMT　：ナイトシティ(深夜)
        ・スクリームシート
        　SCSR　：スクリームシート(ランダム)
        　SCST　：スクリームシート分類
        　SCSA　：ヘッドラインA
        　SCSB　：ヘッドラインB
        　SCSC　：ヘッドラインC
        ・最寄りの自販機
        　VMCR　：最寄りの自販機表
        　VMCT　：自販機タイプ決定表
        　VMCE　：食品
        　VMCF　：ファッション
        　VMCS　：変なもの
        ・ボデガの客
        　STORE　：ボデガの客と店員
        　STOREA　：店主またはレジ係
        　STOREB　：変わった客その1
        　STOREC　：変わった客その2
        ・夜の市
        　NMCT　：商品の分野
        　NMCFO　：食品とドラッグ
        　NMCME　：個人用電子機器
        　NMCWE　：武器と防具
        　NMCCY　：サイバーウェア
        　NMCFA　：衣料品とファッションウェア
        　NMCSU　：サバイバル用品
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return CpRollResult(command, randomizer);
        }

        private Result? CpRollResult(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^CP(?<ability>\d+)?(?<modifier>[+-]\d+)?(>(?<target>\d+))?$");
            if (!m.Success) return null;

            var modifyNumber = 0;
            if (m.Groups["ability"].Success)
                modifyNumber += Convert.ToInt32(m.Groups["ability"].Value);
            if (m.Groups["modifier"].Success)
                modifyNumber += Convert.ToInt32(m.Groups["modifier"].Value);
            var hasTarget = m.Groups["target"].Success;
            var targetNumber = hasTarget ? Convert.ToInt32(m.Groups["target"].Value) : 0;

            var dices = new System.Collections.Generic.List<int>();
            dices.Add(randomizer.RollOnce(10));
            var total = dices[0] + modifyNumber;

            bool isCritical = false;
            bool isFumble = false;

            switch (dices[0])
            {
                case 10:
                    dices.Add(randomizer.RollOnce(10));
                    total += dices[1];
                    isCritical = true;
                    break;
                case 1:
                    dices.Add(randomizer.RollOnce(10));
                    total -= dices[1];
                    isFumble = true;
                    break;
            }

            bool isSuccess = false;
            bool hasCondition = false;
            if (hasTarget)
            {
                hasCondition = true;
                isSuccess = total > targetNumber;
            }

            var modStr = modifyNumber > 0 ? "+" + modifyNumber.ToString() : (modifyNumber < 0 ? modifyNumber.ToString() : "");
            var targetStr = hasTarget ? ">" + targetNumber.ToString() : "";
            var sb = new System.Text.StringBuilder();
            sb.Append("(1D10" + modStr + targetStr + ")");
            sb.Append(" ▶ ");
            sb.Append(dices[0].ToString() + "[" + dices[0].ToString() + "]" + modStr);
            sb.Append(" ▶ ");

            if (isCritical)
            {
                sb.Append(Translate("CyberpunkRed.critical") + " ▶ ");
                sb.Append(dices[1].ToString() + "[" + dices[1].ToString() + "] ▶ ");
            }
            if (isFumble)
            {
                sb.Append(Translate("CyberpunkRed.fumble") + " ▶ ");
                sb.Append(dices[1].ToString() + "[" + dices[1].ToString() + "] ▶ ");
            }
            sb.Append(total.ToString());
            if (hasCondition && isSuccess)
                sb.Append(" ▶ " + Translate("success"));
            else if (hasCondition && !isSuccess)
                sb.Append(" ▶ " + Translate("failure"));

            var builder = Result.CreateBuilder(sb.ToString())
                .SetRands(randomizer.RandResults);
            if (hasCondition)
                builder = builder.SetCondition(isSuccess);
            if (isCritical)
                builder = builder.SetCritical(true);
            if (isFumble)
                builder = builder.SetFumble(true);
            return builder.Build();
        }

    }
}