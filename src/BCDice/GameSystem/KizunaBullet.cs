using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// キズナバレット
    /// </summary>
    public sealed class KizunaBullet : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KizunaBullet Instance = new KizunaBullet();

        /// <inheritdoc/>
        public override string Id => "KizunaBullet";

        /// <inheritdoc/>
        public override string Name => "キズナバレット";

        /// <inheritdoc/>
        public override string SortKey => "きすなはれつと";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.NoSort;

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・ダイスロール
        nDM…n個の6面ダイスを転がして、一番高い出目を採用します。
        ・［調査判定］
        nIN…n個の6面ダイスを転がして、一番高い出目が5以上なら成功します。（［パートナーのヘルプ］使用可）
        ・［鎮静判定］
        SEn…2個の6面ダイスを転がして、出目の合計値がn（［ヒビワレ］状態の［キズナ］の個数）より高いと成功します。（［強制鎮静］使用可）
        ・［解決］ ［アクション］のダメージと［アクシデント］のダメージ軽減
        nSO…2+n個の6面ダイスを転がして、出目をすべて合計します。（nは減らした【励起値】。省略可能）
        ・各種表
        日常表・場所 OP
        日常表・内容 OC
        日常表・場所と内容 OPC
        日常表（仕事）・場所 OWP
        日常表（仕事）・内容 OWC
        日常表（仕事）・場所と内容 OWPC
        日常表（休暇）・場所 OHP
        日常表（休暇）・内容 OHC
        日常表（休暇）・場所と内容 OHPC
        日常表（出張）・場所 OTP
        日常表（出張）・内容 OTC
        日常表（出張）・場所と内容 OTPC
        ターンテーマ表 TT
        ターンテーマ表・親密 TTI
        ターンテーマ表・クール TTC
        ターンテーマ表・主従 TTH
        遭遇表・場所 EP
        遭遇表・登場順 EO
        遭遇表・状況（初対面） EF
        遭遇表・状況（知り合い） EA
        遭遇表・決着 EE
        遭遇表・場所と登場順と状況（初対面）と決着 EFA
        遭遇表・場所と登場順と状況（知り合い）と決着 EAA
        交流表・場所 CP
        交流表・内容 CC
        交流表・場所と内容 CPC
        調査表・ベーシック IB
        調査表・ダイナミック ID
        調査表・ベーシックとダイナミック IBD
        ハザード表 HA
        通常ダイジェスト　キミたちに新しい命令が下った（調査が依頼された）。
        1:その事件の内容は…… NI1
        2:捜査に向かった場所は…… NI2
        3:犯人のキセキ使いは…… NI3
        4:起きた出来事は…… NI4
        5:バレットの間では…… NI5
        6:戦いの結末は…… NI6
        通常ダイジェスト　キミたちは旅行（出張）である場所を訪れた。
        1:その場所とは…… NT1
        2:そこで始まったのは…… NT2
        3:極限状態のなかで…… NT3
        4:犯人のキセキ使いは…… NT4
        5:バレットの間では…… NT5
        6:戦いの結末は…… NT6
        ホリデーダイジェスト　キミたちは休日に出かけることにした。
        1:その場所とは…… HH1
        2:待ち合わせをしたら…… HH2
        3:そしてなんと…… HH3
        4:ふたりが決めたのは…… HH4
        5:結果的に…… HH5
        6:バレットは最後に…… HH6
        ホリデーダイジェスト　キミたちは奇妙な事件に出くわした。
        1:その場所とは…… HC1
        2:起きた事件は…… HC2
        3:犯人のキセキ使いは…… HC3
        4:犯人を追い詰めるべく…… HC4
        5:戦いの結果は…… HC5
        6:バレットは最後に…… HC6
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollMax(command, randomizer)
                ?? RollInvestigate(command, randomizer)
                ?? RollSedative(command, randomizer)
                ?? RollSolve(command, randomizer);
        }

        private static readonly Regex DmRegex = new Regex(@"^(\d+)DM$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex InRegex = new Regex(@"^(\d+)IN$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SeRegex = new Regex(@"^SE(\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex SoRegex = new Regex(@"^(\d*)SO$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Result? RollMax(string command, IRandomizer randomizer)
        {
            var m = DmRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int prefixNumber = Convert.ToInt32(m.Groups[1].Value);
            int[] dice_array = randomizer.RollBarabara(prefixNumber, 6);
            int max = dice_array.Max();

            return Result.CreateBuilder($"{command} ＞ [{string.Join(",", dice_array)}] ＞ {max}")
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollInvestigate(string command, IRandomizer randomizer)
        {
            var m = InRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int prefixNumber = Convert.ToInt32(m.Groups[1].Value);
            var texts = new List<string>();
            bool is_success = false;
            bool is_fumble = false;

            int[] dice_array = randomizer.RollBarabara(prefixNumber, 6);
            int max = dice_array.Max();

            if (max >= 5)
            {
                is_success = true;
                texts.Add(Translate("KizunaBullet.INVESTIGATE.success"));
            }
            else if (max >= 3)
            {
                texts.Add(Translate("KizunaBullet.INVESTIGATE.failure"));
                texts.Add(Translate("KizunaBullet.INVESTIGATE.partnerHelp"));
            }
            else
            {
                is_fumble = true;
                texts.Add(Translate("KizunaBullet.INVESTIGATE.failure"));
                texts.Add(Translate("KizunaBullet.INVESTIGATE.fumble"));
            }

            return Result.CreateBuilder($"{command} ＞ [{string.Join(",", dice_array)}] ＞ {string.Join("", texts)}")
                .SetCondition(is_success)
                .SetFumble(is_fumble)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollSedative(string command, IRandomizer randomizer)
        {
            var m = SeRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int suffixNumber = Convert.ToInt32(m.Groups[1].Value);
            string text;
            bool is_success = false;

            int sum = randomizer.RollSum(2, 6);

            if (suffixNumber > 12)
            {
                text = Translate("KizunaBullet.SEDATIVE.burst");
            }
            else if (suffixNumber < 6)
            {
                text = Translate("KizunaBullet.SEDATIVE.alive");
            }
            else if (sum > suffixNumber)
            {
                is_success = true;
                text = Translate("KizunaBullet.SEDATIVE.success");
            }
            else
            {
                int dif = suffixNumber - sum;
                int check = dif / 2 + 1;
                text = Translate("KizunaBullet.SEDATIVE.failure");
            }

            return Result.CreateBuilder($"{command} ＞ {sum} ＞ {text}")
                .SetCondition(is_success)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private Result? RollSolve(string command, IRandomizer randomizer)
        {
            var m = SoRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            int prefixNumber = m.Groups[1].Value.Length > 0 ? Convert.ToInt32(m.Groups[1].Value) : 0;
            int sum = randomizer.RollSum(prefixNumber + 2, 6);

            return Result.CreateBuilder($"{command} ＞ {sum}")
                .SetRands(randomizer.RandResults)
                .Build();
        }
    }
}
