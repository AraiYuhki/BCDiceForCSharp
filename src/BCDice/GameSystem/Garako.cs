using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Arithmetic;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ガラコと破界の塔
    /// </summary>
    public sealed class Garako : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Garako Instance = new Garako();
        /// <summary>各種テーブル（スタブ）</summary>
        private static readonly System.Collections.Generic.Dictionary<string, object> TABLES = new System.Collections.Generic.Dictionary<string, object>();


        /// <inheritdoc/>
        public override string Id => "Garako";

        /// <inheritdoc/>
        public override string Name => "ガラコと破界の塔";

        /// <inheritdoc/>
        public override string SortKey => "からことはかいのとう";

        /// <inheritdoc/>
        public override int SidesImplicitD => 10;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定 GR+n#f>=X （+n：判定値、#f：不安定による自動失敗基準値、X：目標値、それぞれ省略可能）
        ・部位決定チャート：HIT
        ・ダメージ+部位決定：GHAn（n：火力）
        ・ダメージチャート：xDCy（CDC/EDC/FDC/ADC/LDC )
        ・ダメージチャートver2：xDTy（CDT/EDT/FDT/ADT/LDT）
        　xは C：コックピット、E：エンジン、F：フレーム、A：アーム、L：レッグ
        　yはダメージ値
        各種表
        ・個性表：IDI／動機決定表：MTV
        ・名前表
        ピグマー族　　男：PNM　女：PNF　　エレメント族　男：ENM　女：ENF
        ノーマッド族　男：NNM　女：NNF　　ラット族　　　男：RNM　女：RNF
        ブレイン族　　１：BN1　２：BN2　　テンタクル族　１：TN1　２：TN2
        ・ガラコ改造チャート表：GCC
        ・武器改造チャート表：WCC
        ・イベントチャート表：EVC
        ・戦闘開始距離：BSD

        デフォルトダイス：10面
        ";

        private static readonly Regex GrRegex = new Regex(
            @"^GR([+-]\d+)?(#(\d+))?(>=(\d+))?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex GhaRegex = new Regex(
            @"^GHA([-+\d]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex DamageChartRegex = new Regex(
            @"^([CEFAL]D[CT])([-+\d]+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollTables(command, TABLES) ?? RollGr(command, randomizer) ?? RollDamageChart(command) ?? RollAttackHit(command, randomizer);
        }

        private Result? RollGr(string command, IRandomizer randomizer)
        {
            var m = GrRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var modifyNumber = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : 0;
            var autoFailureNumber = m.Groups[3].Success ? int.Parse(m.Groups[3].Value) : 1;
            var targetNumber = m.Groups[5].Success ? (int?)int.Parse(m.Groups[5].Value) : null;

            var dice = randomizer.RollOnce(10);
            var total = dice + modifyNumber;

            string? result;
            if (dice == 1)
            {
                result = "ファンブル";
            }
            else if (dice <= autoFailureNumber)
            {
                result = "自動失敗";
            }
            else if (dice == 10)
            {
                result = "クリティカル";
            }
            else if (targetNumber != null && total >= targetNumber)
            {
                result = "成功";
            }
            else if (targetNumber != null)
            {
                result = "失敗";
            }
            else
            {
                result = null;
            }

            var formatedModifier = Format.Modifier(modifyNumber);
            var formatedAutoFailure = autoFailureNumber >= 2 ? $"#{autoFailureNumber}" : "";
            var formatTarget = targetNumber != null ? $">={targetNumber}" : "";

            var sequence = new List<string>
            {
                $"(1D10{formatedModifier}{formatedAutoFailure}{formatTarget})",
                $"{dice}[{dice}]{formatedModifier}",
                total.ToString()
            };
            if (result != null)
            {
                sequence.Add(result);
            }

            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private Result? RollAttackHit(string command, IRandomizer randomizer)
        {
            var m = GhaRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var modifier = ArithmeticEvaluator.Eval(m.Groups[1].Value, RoundType.Floor) ?? 0;
            var attack = randomizer.RollOnce(10);
            var total = attack + modifier;

            string hitDice = "";
            string hitText = $"{total}（ダメージを受けない）";
            if (total > 0)
            {
                // HIT table roll: 1D10 → 1-indexed part name
                var hitRoll = randomizer.RollOnce(10);
                var hitParts = new[] { "コックピット", "エンジン", "フレーム", "フレーム", "フレーム", "フレーム", "ライトアーム", "レフトアーム", "ライトレッグ", "レフトレッグ" };
                var hitBody = hitRoll >= 1 && hitRoll <= hitParts.Length ? hitParts[hitRoll - 1] : "フレーム";
                hitDice = $", HIT[{hitRoll}]";
                hitText = $"{hitBody}に {total} -【部位装甲】";
            }

            var formatedModifier = Format.Modifier(modifier);
            var sequence = new[]
            {
                $"(1D10{formatedModifier})",
                $"{attack}[{attack}]{formatedModifier}{hitDice}",
                hitText
            };
            return Result.CreateBuilder(string.Join(" ＞ ", sequence)).Build();
        }

        private Result? RollDamageChart(string command)
        {
            var m = DamageChartRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var chartKey = m.Groups[1].Value.ToUpperInvariant();
            if (!DamageCharts.ContainsKey(chartKey))
            {
                return null;
            }

            var chart = DamageCharts[chartKey];
            var rawDamage = ArithmeticEvaluator.Eval(m.Groups[2].Value, RoundType.Floor) ?? 0;
            var damage = rawDamage < 0 ? 0 : rawDamage > 10 ? 10 : rawDamage;

            if (damage <= 0)
            {
                return Result.CreateBuilder("ダメージを受けない").Build();
            }

            var tableResult = chart.Table[damage - 1];
            return Result.CreateBuilder($"{chart.Name}({damage}).Build() ＞ {tableResult}").Build();
        }

        private class DamageChart
        {
            public string Name { get; }
            public string[] Table { get; }

            public DamageChart(string name, string[] table)
            {
                Name = name;
                Table = table;
            }
        }

        private static readonly Dictionary<string, DamageChart> DamageCharts = new Dictionary<string, DamageChart>
        {
            { "CDC", new DamageChart("部位ダメージチャート：コックピット", new[]
                {
                    "小破（アーマー損傷）：以後、この部位の【部位装甲】-1。",
                    "小破（視界不良）：モニターやハッチの歪み等により、視界を大きく遮られる。以後、【視認性】-1、【部位装甲】-1。",
                    "小破（強震）：大きく揺さぶられる。キミは【身体】10の判定を行う。失敗した場合、次のターンを失う。【部位装甲】-1。",
                    "小破（収納直撃）：アイテム収納スペースに直撃！　所持品一つにつき1d10を振れ。出目が5以下だった所持品は破壊される。【部位装甲】-1。",
                    "中破（計器損傷）：コンソールの一部が停止する。［弱体1］を受ける。",
                    "中破（制御不能）：コントロールが効かなくなる。キミは次のターンを失う。［弱体1］を受ける。",
                    "中破（貫通！）：パイロットに被害が！　キミはHPダメージ（1d10-【身体】）に加え、［弱体1］を受ける。",
                    "大破（故障）：コックピットが完全にいかれる。キミは次のラウンド終了時まで、あらゆる判定に自動的にファンブルする。［弱体1］を受ける。",
                    "大破（貫通！）：パイロットに被害が！　キミはHPダメージ（1d10+3-【身体】）に加え、［弱体1］を受ける。",
                    "修復不能（破壊）：コックピットが［修復不能］となる。キミは2d10-【身体】点のHPダメージを受ける。ガラコはすべての機能を停止する。コックピットのハッチが自動的に開く。",
                })
            },
            { "EDC", new DamageChart("部位ダメージチャート：エンジン", new[]
                {
                    "小破（アーマー損傷）：以後、この部位の【部位装甲】-1。",
                    "小破（アーマー損傷）：以後、この部位の【部位装甲】-1。",
                    "小破（燃料漏れ）：タンクから燃料が漏れる。燃料-1。この部位の【部位装甲】-1。",
                    "小破（燃料漏れ）：タンクから燃料が漏れる。燃料-2。この部位の【部位装甲】-1。",
                    "中破（エンジン不調）：時々エンジンが動かなくなる。［弱体1］を受ける。",
                    "中破（燃料漏れ）：タンクから燃料が漏れる。燃料-2。［弱体1］を受ける。",
                    "中破（ヒート）：オーバーヒートする。次のターンの終了時まで、移動と攻撃を行えない。［弱体1］を受ける。",
                    "大破（エンジン不調）：キミは次のターンを失う。［弱体1］を受ける。",
                    "大破（故障）：以後、この部位の【部位装甲】が0になる。［弱体1］を受ける。",
                    "修復不能（エンジン停止）：エンジンが停止する。ガラコはすべての機能を停止する。コックピットのハッチが自動的に開く。【操作性】10の判定を行うこと。失敗するとエンジンが爆発する。その場合、すべての部位が［修復不能］となり、キミは2d10-【身体】点のダメージを受ける。",
                })
            },
            { "FDC", new DamageChart("部位ダメージチャート：フレーム", new[]
                {
                    "小破（不安定）：体勢を崩す。次のターン、キミは攻撃を行えない。この部位の【部位装甲】-1。",
                    "小破（スクラッチ！）：フレームに醜い傷が残る。この部位の【部位装甲】-1。",
                    "小破（アーマー損傷）：フレームが歪む。この部位の【部位装甲】-1。",
                    "小破（アーマー損傷）：フレームがきしみ始め、ガラコの動きを阻害し始める。【移動力】-1。さらに、この部位の【部位装甲】-1。",
                    "中破（放熱板損傷）：熱を機体外に逃すことができなくなる。［弱体1］を受ける。",
                    "中破（スタビライザー損傷）：機体のバランス調整装置が故障する。【身体】10の判定を行うこと。失敗した場合、キミは次のターンを失う。［弱体1］を受ける。",
                    "中破（貫通！）：パイロットに被害が！　キミはHPダメージ（1d10-【身体】）を受ける。［弱体1］を受ける。",
                    "大破（停止）：フレームが動かない。キミは次のターンを失う。［弱体1］を受ける。",
                    "大破（アーマー損傷）：フレームに甚大なダメージを受ける。以後、この部位の【部位装甲】に-3。［弱体1］を受ける。",
                    "修復不能（フレーム崩壊）：フレームが［修復不能］となる。フレームの大部分が剥がれ落ち、ガラコの内部が晒される。以後、キミに対して部位狙いが行われる場合、その命中判定に対する修正（p21）は発生しなくなる。［弱体2］を受ける。",
                })
            },
            { "ADC", new DamageChart("部位ダメージチャート：アーム", new[]
                {
                    "小破（アーマー損傷）：アームの装甲にヒビが入る。【部位装甲】-1。",
                    "小破（武器落とし！）：【身体】8の判定を行う。失敗した場合、ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。【部位装甲】-1。",
                    "小破（マニュピレータ損傷）：指が何本かちぎれ飛んだ。【操作性】-1、【部位装甲】-1。",
                    "小破（機能停止）：次のターンの終了時まで、このアームを使った攻撃はできない。以後、この部位の【部位装甲】-1。",
                    "中破（痙攣）：アームの動きがぶれ始める。［弱体1］を受ける。",
                    "中破（武器落とし！）：ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。［弱体1］を受ける。",
                    "中破（スピン）：機体が大きく回転する。【身体】10の判定を行うこと。失敗した場合、［伏せ］状態となった上、次のターンを失う。［弱体1］を受ける。",
                    "大破（アーマー損傷）：以後、この部位の【部位装甲】を-3。［弱体1］を受ける。",
                    "大破（武器落とし！）：ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。以後、この部位の【部位装甲】が0になる。［弱体1］を受ける。",
                    "修復不能（破壊）：ダメージを受けた側のアームが［修復不能］となる。［弱体2］を受ける。",
                })
            },
            { "LDC", new DamageChart("部位ダメージチャート：レッグ", new[]
                {
                    "小破（アーマー損傷）：以後、この部位の【部位装甲】-1。",
                    "小破（よろめき）：以後、この部位の【部位装甲】-1。次のターン終了時まで、キミは移動できない。",
                    "小破（スネア）：足元をすくわれる。【部位装甲】-1。さらに【身体】8の判定を行うこと。失敗した場合、キミは［伏せ］状態になる。",
                    "小破（跛足）：以後、【移動力】-1、【部位装甲】-1。",
                    "中破（シャフト損傷）：脚部の軸に歪みが生じる。［弱体1］を受ける。",
                    "中破（アクチュエータ損傷）：脚部のアクチュエータに大きな損傷を受ける。【移動力】-1。［弱体1］を受ける。",
                    "中破（スピン）：機体が大きく回転する。【身体】10の判定を行うこと。失敗した場合、［伏せ］状態となった上、次のターンを失う。［弱体1］を受ける。",
                    "大破（アーマー損傷）：以後、この部位の【部位装甲】を-3。［弱体1］を受ける。",
                    "大破（跛足）：以後、【移動力】-2。この部位の【部位装甲】が0になる。［弱体1］を受ける。",
                    "修復不能（破壊）：ダメージを受けた側のレッグが［修復不能］となる。【移動力】-2。［弱体2］を受ける。",
                })
            },
            { "CDT", new DamageChart("部位ダメージチャートv2: コックピット", new[]
                {
                    "アーマー損傷（小破/弱体0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "視界不良（小破/弱体0/装甲ダメージ1）モニターやハッチの歪み等により、視界を大きく遮られる。以後、【視認性】-1。",
                    "強震（小破/弱体0/装甲ダメージ1）大きく揺さぶられる。キミは【身体】10 の判定を行なう。失敗した場合、次のターンを失う。",
                    "貫通！（小破/弱体0/装甲ダメージ1）パイロットに被害が！キミはＨＰダメージ（1d10-【身体】）を受ける。",
                    "計器損傷（中破/弱体1/装甲ダメージ1）コンソールの一部が停止する。",
                    "制御不能（中破/弱体1/装甲ダメージ1）コントロールが効かなくなる。キミは次のターンを失う。",
                    "貫通！（中破/弱体1/装甲ダメージ1）パイロットに被害が！キミはＨＰダメージ（1d10-【身体】）を受ける。",
                    "故障（大破/弱体1/装甲ダメージ2）コックピットが完全にいかれる。キミは次のラウンド終了時まで、あらゆる判定に自動的にファンブルする。",
                    "貫通！（大破/弱体1/装甲ダメージ2）パイロットに被害が！キミはＨＰダメージ（1d10+3-【身体】）を受ける。",
                    "破壊（修復不能/弱体2/装甲ダメージ3）コックピットが「修復不能」となる。キミは2d10-【身体】点のＨＰダメージを受ける。ガラコはすべての機能を停止する。コックピットのハッチが自動的に開く。",
                })
            },
            { "EDT", new DamageChart("部位ダメージチャートv2: エンジン", new[]
                {
                    "アーマー損傷（小破/弱体0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "アーマー損傷（小破/弱体0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "燃料漏れ（小破/弱体0/装甲ダメージ1）タンクから燃料が漏れる。燃料-1。",
                    "燃料漏れ（小破/弱体0/装甲ダメージ1）タンクから燃料が漏れる。燃料-2。",
                    "エンジン不調（中破/弱体1/装甲ダメージ1）エンジンの調子が安定しない。",
                    "オーバーヒート（中破/弱体1/装甲ダメージ1）オーバーヒートする。次のターンの終了時まで、移動と攻撃を行えない。",
                    "エンジン不調（中破/弱体1/装甲ダメージ1）なんだか調子悪い。キミは次のターンを失う。",
                    "燃料漏れ（大破/弱体1/装甲ダメージ2）タンクから燃料が漏れる。燃料-3。",
                    "貫通！（大破/弱体1/装甲ダメージ2）パイロットに被害が！キミはＨＰダメージ（1d10+3-【身体】）を受ける。",
                    "エンジン停止（修復不能/弱体2/装甲ダメージ3）エンジンが停止する。ガラコはすべての機能を停止する。コックピットのハッチが自動的に開く。【操作性】10の判定を行なうこと。失敗するとエンジンが爆発する。その場合、すべての部位が［修復不能］となり、キミは2d10-【身体】点のＨＰダメージを受ける",
                })
            },
            { "FDT", new DamageChart("部位ダメージチャートv2: フレーム", new[]
                {
                    "アーマー損傷 （小破 /0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "スクラッチ！（小破/弱体0/装甲ダメージ1）フレームに醜い傷が残る。",
                    "歪み（小破/弱体0/装甲ダメージ1）フレームが歪み、ガラコの動きを阻害し始める。【移動力】-1。",
                    "ハードポイント損傷（小破/弱体0/装甲ダメージ1）このフレームに装着している武器、及びオプションをすべて落とす。装着していた武器やオプションが外れかかる。キミは【身体】8の判定を行なう。失敗した場合、",
                    "放熱板損傷（中破/弱体1/装甲ダメージ1）熱を機体外に逃すことができなくなる。これはヤバい。",
                    "スタビライザー損傷（中破/弱体1/装甲ダメージ1）ターンを失う。機体のバランス調整装置が故障する。【身体】10の判定を行なうこと。失敗した場合、キミは次の",
                    "貫通！（中破/弱体1/装甲ダメージ1）パイロットに被害が！キミはＨＰダメージ（1d10-【身体】）を受ける。",
                    "停止（大破/弱体1/装甲ダメージ2）フレームが動かない。キミは次のターンを失う。",
                    "ハードポイント破壊（大破/弱体1/装甲ダメージ2）武器やオプションを取り付ける箇所が破壊される。以後、このフレームに（スロットを消費して）装着している武器やオプションは使用できない（常時効果のあるものも、効果を失う）。",
                    "フレーム崩壊（修復不能/弱体2/装甲ダメージ3）フレームが「修復不能」となる。フレームの大部分が剥がれ落ち、ガラコの内部が晒される。以後キミに対して部位狙いが行われる場合、その命中判定に対する修正（『GHT』p21）は発生しない。",
                })
            },
            { "ADT", new DamageChart("部位ダメージチャートv2: アーム", new[]
                {
                    "アーマー損傷（小破/弱体0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "武器落とし！（小破/弱体0/装甲ダメージ1）キミは【身体】8の判定を行う。失敗した場合、ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。",
                    "マニピュレータ損傷（小破/弱体0/装甲ダメージ1）指が何本かちぎれ飛んだ。【操作性】-1。",
                    "機能停止（小破/弱体0/装甲ダメージ1）次のターンの終了時まで、このアームを使った攻撃はできない。",
                    "痙攣（中破/弱体1/装甲ダメージ1）アームの動きがぶれ始める。",
                    "武器落とし！（中破/弱体1/装甲ダメージ1）ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。",
                    "スピン（中破/弱体1/装甲ダメージ1）機体が大きく回転する。【身体】10の判定を行うこと。失敗した場合、［伏せ］状態となった上、次のターンを失う。",
                    "武器落とし！（大破/弱体1/装甲ダメージ2）ダメージを受けた側のアームに（スロットを消費して）装着していた武器を落とす。",
                    "ハードポイント破壊（大破/弱体1/装甲ダメージ2）している武器やオプションは使用できない（常時効果のあるものも、効果を失う）。武器やオプションを取り付ける箇所が破壊される。以後、このアームに（スロットを消費して）装着している武器やオプションは使用できない（常時効果のあるものも、効果を失う）。",
                    "破壊（修復不能/2/装甲ダメージ3）ダメージを受けた側のアームが「修復不能」となる。",
                })
            },
            { "LDT", new DamageChart("部位ダメージチャートv2: レッグ", new[]
                {
                    "アーマー損傷 （小破 /弱体0/装甲ダメージ1）装甲に軽いひび割れが走る。",
                    "よろめき （小破 /弱体0/装甲ダメージ1）次のターンの終了時まで、キミは移動できない。",
                    "スネア （小破 /弱体0/装甲ダメージ1）足元をすくわれる。【身体】8 の判定を行うこと。失敗した場合、キミは［伏せ］状態になる。",
                    "跛足 （小破 /弱体0/装甲ダメージ1）以後、【移動力】-1。",
                    "シャフト損傷 （中破 /弱体1/装甲ダメージ1）脚部の軸に歪みが生じる。",
                    "アクチュエータ損傷 （中破 /弱体1/装甲ダメージ1）脚部のアクチュエータに大きな損傷を受ける。【移動力】-1。",
                    "スピン （中破 /弱体1/装甲ダメージ1）機体が大きく回転する。【身体】10 の判定を行うこと。失敗した場合、［伏せ］状態となった上、次のターンを失う。",
                    "跛足 （大破 /弱体1/装甲ダメージ2）以後、【移動力】-2。",
                    "ハードポイント破壊 （大破 /弱体1/装甲ダメージ2）している武器やオプションは使用できない（常時効果のあるものも、効果を失う）。 武器やオプションを取り付ける箇所が破壊される。以後、このレッグに（スロットを消費して）装着している武器やオプションは使用できない（常時効果のあるものも、効果を失う）。",
                    "破壊 （修復不能 /弱体2/装甲ダメージ3）ダメージを受けた側のレッグが「修復不能」となる。【移動力】-2。",
                })
            },
        };
    }
}
