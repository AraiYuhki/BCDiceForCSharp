using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// フタリソウサ
    /// </summary>
    public sealed class FutariSousa : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly FutariSousa Instance = new FutariSousa();

        private const int SUCCESS_THRESHOLD = 4;
        private const int SPECIAL_DICE = 6;
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();

        /// <inheritdoc/>
        public override string Id => "FutariSousa";

        /// <inheritdoc/>
        public override string Name => "フタリソウサ";

        /// <inheritdoc/>
        public override string SortKey => "ふたりそうさ";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定用コマンド
        探偵用：【DT】…10面ダイスを2つ振って判定します。『有利』なら【3DT】、『不利』なら【1DT】を使います。
        　　　　【DT6】…6面ダイスを2つ振って判定します。『有利』なら【3DT6】、『不利』なら【1DT6】を使います。
        助手用：【AS】…6面ダイスを2つ振って判定します。『有利』なら【3AS】、『不利』なら【1AS】を使います。
        　　　　【AS10】…10面ダイスを2つ振って判定します。『有利』なら【3AS10】、『不利』なら【1AS10】を使います。
        ・各種表
        【セッション時】
        異常な癖決定表　　　　　 SHRD／新・異常な癖決定表　　 SHND
        普通の？・異常な癖決定表 SHAD／ケイジ異常な癖決定表　 SHKD
        超探偵向け異常な癖表　　 SHLD
        　口から出る表　　　 SHFM／強引な捜査表　　　　　 SHBT／すっとぼけ表　　　　　　 SHPI
        　事件に夢中表　　　 SHEG／パートナーと……表　　　 SHWP／何かしている表　　　　　 SHDS
        　奇想天外表　　　　 SHFT／急なひらめき表　　　　 SHIN／喜怒哀楽表　　　　　　　 SHEM
        　人間エミュレート表 SHHE／人間エミュレート失敗表 SHHF／パートナーへのいたずら表 SHMP
        　思わせぶり表　　　 SHSB／もどかしい表　　　　　 SHFR／突然どうした表　　　　　 SHIS
        　わがままを言う表　 SHSE／普通に見える表　　　　 SHLM／嫉妬に狂う表　　　　　　 SHJS
        　傲慢な態度表　　　 SHAR／比較的軽度なもの表　　 SHRM／ノータイム表　　　　　　 SHNT
        　捜査のやり方表　　 SHIM／貴族表　　　　　　　　 SHNO／説明しない表　　　　　　 SHNE
        　刑事としての癖表　 SHHD／名誉ある探偵表　　　　 SHGD／超すごい表　　　　　　　 SHSA
        　超事件に夢中表　　 SHEP／超パートナーと……表　　 SHXP
        イベント表
        　現場にて　 EVS／なぜ？　 EVW／協力者と共に EVN
        　向こうから EVC／VS容疑者 EVV
        　閉鎖空間　 EVE
        　探偵のみ捜査 EVD／助手のみ捜査　　 EVA／観光捜査　 EVT
        　思わぬヒント EVH／実験をしてみよう EVX／ゲスト捜査 EVG
        　ケイジ聞き込み捜査　　　 EVQ／ケイジ大規模捜査　　　　　 EVM／こっそり情報の受け渡し EVP
        　同僚たちと一緒に捜査する EVO／頻染みの店シチュエーション EVF／ハードBデカアクション  EVB
        　探偵を大人しくさせる捜査 EVL／伝統的捜査　　　　　　　　 EVZ／原始的捜査　　　　　　 EVR
        　超探偵調査　　　　　　　EV6S／神速捜査　　　　　　　　　EV6F
        感情表
        　感情表A／B　　 FLT66・FLT10
        　気に入っているところ　 FLTL66　／気に入らないところ　 FLTD66
        　ランダム感情決定表（あなた）　 FLTRA
        　顔のパーツ　　　　 FLTF66／体のパーツ　 FLTB66／生活習慣　　　 FLTH66
        　ふわっとした感覚　 FLTS66／他人への態度 FLTA66／ヘビーウェイト FLTW66
        　同僚　　　　 FLTC66／部下　　　　 FLTU66／上司　　　　 FLTO66
        　捜査のやり方 FLTI66
        調査の障害表 OBT　　変調表 ACT　　目撃者表 EWT　　迷宮入り表 WMT
        思い出の品決定表 MIT　　エピソード付き思い出の品表 MITE　　呼び名表A・B　 NCT66・NCT10
        【設定時】
        背景表
        　探偵　運命の血統 BGDD／天性の才能 BGDG／マニア　　　　 BGDM
        　助手　正義の人　 BGAJ／情熱の人　 BGAP／巻き込まれの人 BGAI
        身長表 HT　　たまり場表 BT　　関係表 GRT
        職業表A・B　　JBT66・JBT10　　ファッション特徴表A・B　　　　FST66・FST10
        好きなもの／嫌いなもの表A・B　LDT66・LDT10
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(\d+)?DT(?:6)?$");
            if (m.Success)
            {
                int count = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
                return RollDt(command, count, randomizer);
            }

            m = Regex.Match(command, @"^(\d+)?AS(?:10)?$");
            if (m.Success)
            {
                int count = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 2;
                return RollAs(command, count, randomizer);
            }

            return RollTable(command, TABLES, randomizer)
                ?? base.EvalGameSystemSpecificCommand(command, randomizer);
        }

        private Result? RollDt(string command, int count, IRandomizer randomizer)
        {
            var dice_list = Regex.IsMatch(command, @"^(\d*)DT6$") ? randomizer.RollBarabara(count, 6) : randomizer.RollBarabara(count, 10);
            var max = dice_list.Max();
            string message;
            bool fumble = false, special = false, success = false, failure = false;
            if (max <= 1)
            {
                message = Translate("FutariSousa.DT.fumble");
                fumble = true;
                failure = true;
            }
            else if (dice_list.Contains(SPECIAL_DICE))
            {
                message = Translate("FutariSousa.DT.special");
                special = true;
                success = true;
            }
            else if (max >= SUCCESS_THRESHOLD)
            {
                message = Translate("success");
                success = true;
            }
            else
            {
                message = Translate("failure");
                failure = true;
            }
            string text = $"{command}({string.Join(",", dice_list)}) ＞ {message}";
            var builder = Result.CreateBuilder(text).SetRands(randomizer.RandResults);
            if (fumble) builder = builder.SetFumble(true);
            if (special) builder = builder.SetCritical(true);
            if (success) builder = builder.SetSuccess(true);
            if (failure) builder = builder.SetFailure(true);
            return builder.Build();
        }

        private Result? RollAs(string command, int count, IRandomizer randomizer)
        {
            var dice_list = Regex.IsMatch(command, @"^(\d*)AS10$") ? randomizer.RollBarabara(count, 10) : randomizer.RollBarabara(count, 6);
            var max = dice_list.Max();
            string message;
            bool fumble = false, special = false, success = false, failure = false;
            if (max <= 1)
            {
                message = Translate("FutariSousa.AS.fumble");
                fumble = true;
                failure = true;
            }
            else if (dice_list.Contains(SPECIAL_DICE))
            {
                message = Translate("FutariSousa.AS.special");
                special = true;
                success = true;
            }
            else if (max >= SUCCESS_THRESHOLD)
            {
                message = Translate("FutariSousa.AS.success");
                success = true;
            }
            else
            {
                message = Translate("failure");
                failure = true;
            }
            string text = $"{command}({string.Join(",", dice_list)}) ＞ {message}";
            var builder = Result.CreateBuilder(text).SetRands(randomizer.RandResults);
            if (fumble) builder = builder.SetFumble(true);
            if (special) builder = builder.SetCritical(true);
            if (success) builder = builder.SetSuccess(true);
            if (failure) builder = builder.SetFailure(true);
            return builder.Build();
        }

        private Dictionary<string, object> TranslateTables(object locale)
        {
            return new Dictionary<string, object>();
        }

    }
}