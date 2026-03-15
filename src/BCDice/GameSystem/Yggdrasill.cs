using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// 鋼鉄のユグドラシル
    /// </summary>
    public sealed class Yggdrasill : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly Yggdrasill Instance = new Yggdrasill();

        /// <inheritdoc/>
        public override string Id => "Yggdrasill";

        /// <inheritdoc/>
        public override string Name => "鋼鉄のユグドラシル";

        /// <inheritdoc/>
        public override string SortKey => "こうてつのゆくとらしる";

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ■ 行為判定 (CFx+nD6)
          クリティカルとファンブルによるダイス追加を行う
          先頭のcfを変更することで、動作が変更される
          hcf: 達成値が半減
          cfl: 付加効果【幸運】を付与
          cfg: 付加効果【ギャンブル】を付与
          cft: 【応急処置】判定 (tは末尾に記入してください)
          例）
            CF10+1D6, HCFL6+2D6, CFG11+1D6-2, cfgt10+1D6

        ■ 暴走ロール (RAx)
          暴走率xの暴走ロールおよび臨界ロールを行う
          例）
            RA50, RA110, RA150

        ■ SOペナルティ表 (SOx)
          スペック数がxオーバーした際のペナルティロールを行う
          例）
            SO1, SO5

        ■ 【応急処置】 (TREATx)
          達成値xの【応急処置】による回復量を決定する
          例）
            TREAT1, TREAT18

        ■ その他の判定および表
          down：気絶判定
          cont：コンティニュー判定
          risk：リスク判定
          guki：偶奇判定
          cond：コンディションロール
          allr：オールレンジ発動ロール
          pafe：パーフェクト発動ロール
          stag：ステージ決定（電脳ロワイヤル用）
          fatal1：後遺症
          fatal2：因子変化ロール
          mikuzi：浅草寺みくじ。1d100であなたの運勢を占います
        ";


        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return RollCf(command, randomizer)
                ?? RollRa(command, randomizer)
                ?? RollTreat(command, randomizer)
                ?? RollDown(command, randomizer)
                ?? RollCond(command, randomizer)
                ?? RollGuki(command, randomizer)
                ?? RollCont(command, randomizer)
                ?? RollAllr(command, randomizer)
                ?? RollPafe(command, randomizer);
        }

        private Result? RollCf(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^(H)?CF([LG])?(T)?((?:[+-]*\d+|\+?\d+D\d+)(?:[+-]+\d+|\++\d+D\d+)*)$");
            if (!m.Success)
            {
                return null;
            }

            var hoge = m.Groups[1].Success;
            var luckyState = m.Groups[2].Success ? m.Groups[2].Value : null;
            var treatFlg = m.Groups[3].Success;
            var expr = m.Groups[4].Value;

            // TODO: CommonCommand.AddDice.Parser integration
            // For now, simplified implementation: parse expr as simple arithmetic with dice
            // node = CommonCommand::AddDice::Parser.parse(expr)
            // return nil unless node
            // This is a placeholder - the full AddDice parser integration is needed
            return null;
        }

        private int CountCritical(int[] diceList, string? luckyState)
        {
            int threshold;
            if (luckyState == "G")
            {
                threshold = 4;
            }
            else if (luckyState != null)
            {
                threshold = 5;
            }
            else
            {
                threshold = 6;
            }
            return diceList.Count(x => x >= threshold);
        }

        private int CountFumble(int[] diceList, string? luckyState)
        {
            int threshold;
            if (luckyState == "G")
            {
                threshold = 3;
            }
            else if (luckyState != null)
            {
                threshold = 2;
            }
            else
            {
                threshold = 1;
            }
            return diceList.Count(x => x <= threshold);
        }

        private Result? RollRa(string command, IRandomizer randomizer)
        {
            var m = Regex.Match(command, @"^RA(\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            int? raValue = m.Groups[1].Success ? int.Parse(m.Groups[1].Value) : (int?)null;

            string body;
            switch (raValue)
            {
                case 50:
                    body = RollRa50Table(randomizer);
                    break;
                case 70:
                    body = RollRa70Table(randomizer);
                    break;
                case 90:
                    body = RollRa90Table(randomizer);
                    break;
                case 110:
                case 120:
                case 130:
                case 140:
                    body = RollRa110Table(randomizer);
                    break;
                case 150:
                    body = "＞ 因子崩壊【キャラロスト】";
                    break;
                case null:
                    body = "＞ このコマンドは数値を付けてください";
                    break;
                default:
                    body = "＞ 指定の暴走率の暴走ロールはありません";
                    break;
            }
            return Result.CreateBuilder($"({command}).Build() {body}").Build();
        }

        private Result? RollTreat(string command, IRandomizer randomizer)
        {
            int dice = 0;
            object total = null;
            var m = Regex.Match(command, @"^TREAT(-?\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            if (!m.Groups[1].Success)
            {
                return Result.CreateBuilder("ＡＥ【応急処置】 ＞ このコマンドは数値を付けてください").Build();
            }

            var value = int.Parse(m.Groups[1].Value);

            string recovery;
            if (value <= 5)
            {
                recovery = "0";
            }
            else if (value <= 7)
            {
                recovery = "1";
            }
            else if (value <= 11)
            {
                dice = randomizer.RollOnce(6);
                total = dice / 2;
                recovery = $"{total}({dice}[{dice}]/2)";
            }
            else if (value <= 14)
            {
                dice = randomizer.RollOnce(6);
                recovery = $"{dice}({dice}[{dice}])";
            }
            else if (value <= 17)
            {
                dice = randomizer.RollOnce(6);
                total = dice + 3;
                recovery = $"{total}({dice}[{dice}]+3)";
            }
            else
            {
                var list = randomizer.RollBarabara(2, 6);
                dice = list.Sum();
                total = dice + 2;
                recovery = $"{total}({dice}[{string.Join(",", list)}]+2)";
            }

            return Result.CreateBuilder($"ＡＥ【応急処置】 ＞ HPが{recovery}回復").Build();
        }

        private Result? RollDown(string command, IRandomizer randomizer)
        {
            if (command != "DOWN")
            {
                return null;
            }

            var dice = randomizer.RollOnce(6);

            string result;
            if (dice % 2 == 0)
            {
                result = "回避";
            }
            else
            {
                var fell = randomizer.RollOnce(6);
                result = $"気絶【{fell}R行動不能】";
            }

            return Result.CreateBuilder($"気絶判定 ＞ {dice} ＞ {result}").Build();
        }

        private Result? RollCond(string command, IRandomizer randomizer)
        {
            if (command != "COND")
            {
                return null;
            }

            var hpList = randomizer.RollBarabara(2, 6);
            var hpTotal = hpList.Sum();
            var hpStr = string.Join(",", hpList);
            var ppList = randomizer.RollBarabara(2, 6);
            var ppTotal = ppList.Sum();
            var ppStr = string.Join(",", ppList);

            return Result.CreateBuilder($"({command}).Build() ＞ HP{hpTotal}[{hpStr}] 、 PP{ppTotal}[{ppStr}]").Build();
        }

        private Result? RollGuki(string command, IRandomizer randomizer)
        {
            if (command != "GUKI")
            {
                return null;
            }

            var dice = randomizer.RollOnce(6);
            var result = dice % 2 == 0 ? "成功" : "失敗";

            return Result.CreateBuilder($"(GUKI).Build() ＞ {dice} ＞ {result}").Build();
        }

        private Result? RollCont(string command, IRandomizer randomizer)
        {
            if (!Regex.IsMatch(command, @"^CO(NT)?$"))
            {
                return null;
            }

            var dice = randomizer.RollOnce(6);
            var text = dice <= 4 ? "1R追加" : "2R追加";

            return Result.CreateBuilder($"コンティニュー判定 ＞ {dice} ＞ {text}").Build();
        }

        private Result? RollAllr(string command, IRandomizer randomizer)
        {
            if (command != "ALLR")
            {
                return null;
            }

            var dice = randomizer.RollOnce(6);
            var text = dice == 1 ? "発動失敗【技対象が敵味方含めた全員となる】" : "発動成功";

            return Result.CreateBuilder($"オールレンジ判定 ＞ {dice} ＞ {text}").Build();
        }

        private Result? RollPafe(string command, IRandomizer randomizer)
        {
            if (command != "PAFE")
            {
                return null;
            }

            var dice = randomizer.RollOnce(6);
            var text = dice == 1 ? "発動失敗【通常命中・回避判定となり、発動時のアクション内の命中力＆回避力が半減する】" : "発動成功";

            return Result.CreateBuilder($"発動ロール ＞ {dice} ＞ {text}").Build();
        }

        // Helper methods for RA tables (inlined since complex table classes had too many errors)

        private static readonly string[] Ra50Items = new[]
        {
            "発作【自爆÷2ダメージ。（自身に能力攻撃ロールダメージ÷2）。防御無視】",
            "高揚【1D6暴走率上昇】",
            "高揚【1D6暴走率上昇】",
            "自制【暴走なし】",
            "自制【暴走なし】",
            "自制【暴走なし】",
        };

        private string RollRa50Table(IRandomizer randomizer)
        {
            var value = randomizer.RollOnce(6);
            var chosen = Ra50Items[value - 1];
            if (value == 2 || value == 3)
            {
                var list = randomizer.RollBarabara(1, 6);
                var total = list.Sum();
                var listStr = string.Join(",", list);
                return $"＞ 暴走Lv.1({value}) ＞ {chosen} ： {total}[{listStr}] ％";
            }
            return $"＞ 暴走Lv.1({value}) ＞ {chosen}";
        }

        private static readonly string[] Ra70Items = new[]
        {
            "自爆【自爆ダメージ。自身に能力攻撃ロールダメージ。防御無視】",
            "自爆【自爆ダメージ。自身に能力攻撃ロールダメージ。防御無視】",
            "暴発【ランダム攻撃。基本的に能力攻撃。対象は自分、キャラ、オブジェクトの三種類】",
            "連鎖【2D6暴走率上昇】",
            "発症",
            "自制【暴走無し】",
        };

        private string RollRa70Table(IRandomizer randomizer)
        {
            var value = randomizer.RollOnce(6);
            var chosen = Ra70Items[value - 1];
            if (value == 5)
            {
                // out_of_control: roll RA90
                return $"＞ 暴走Lv.3({value}) ＞ {chosen} ： {RollRa90Table(randomizer)}";
            }
            if (value == 4)
            {
                var list = randomizer.RollBarabara(2, 6);
                var total = list.Sum();
                var listStr = string.Join(",", list);
                return $"＞ 暴走Lv.3({value}) ＞ {chosen} ： {total}[{listStr}] ％";
            }
            return $"＞ 暴走Lv.3({value}) ＞ {chosen}";
        }

        private string RollRa90Table(IRandomizer randomizer)
        {
            var d1 = randomizer.RollOnce(6);
            var d2 = randomizer.RollOnce(6);
            string[] evenItems = new[]
            {
                "能力異常【能力使用時に偶奇判定。奇数の場合は消費だけ行い能力発動失敗。暴走チェックごとに+2％される（発症時も発生）。能力精度の判定結果が半減】",
                "言語異常【AE使用時に偶奇判定。奇数の場合は消費だけ行いAE発動失敗。話術の判定結果が半減】",
                "記憶異常【命中判定結果が半減する。知識の判定結果が半減】",
                "精神異常【自分のリアクション（回避判定など）で偶奇判定。奇数の場合は行動自動失敗。隠密、読心の判定結果が半減】",
                "忘我【自プリアクション時に偶奇判定。奇数の場合は宣言せずにターン終了。あらゆる技能判定結果が半減】",
                "自制【暴走無し】",
            };
            string[] oddItems = new[]
            {
                "制御異常【自プリアクション毎（行動決定前）に偶奇判定。奇数の場合は暴発によるランダム攻撃。（発症時も発生）。技術、幸運の判定結果が半減】",
                "過負荷【ワンアクション毎に能力精度÷3の防御無視ダメージ（発症時も発生）。閃きの判定結果が半減】",
                "聴覚異常【回避判定結果が半減する。察知の半減結果が半減】",
                "視覚異常【SS＆命中力＆回避力が半減する※判定結果は半減しない。観察眼の判定結果が半減】",
                "身体異常【防御を差し引く前のダメージロールが半減する。力技、俊敏の判定結果が半減】",
                "自制【暴走なし】",
            };
            var items = d1 % 2 == 0 ? evenItems : oddItems;
            var chosen = items[d2 - 1];
            return $"＞ 暴走状態表({d1}{d2}) ＞ {chosen}";
        }

        private static readonly string[] Ra110Items = new[]
        {
            "自壊【自爆ダメージ。自身の最も高い攻撃ロールダメージ。防御無視】",
            "超活性【HP・PPを2D6回復】",
            "自壊【自爆ダメージ。自身の最も高い攻撃ロールダメージ。防御無視】",
            "超活性【HP・PPを2D6回復】",
            "自壊【自爆ダメージ。自身の最も高い攻撃ロールダメージ。防御無視】",
            "超活性【HP・PPを2D6回復】",
        };

        private string RollRa110Table(IRandomizer randomizer)
        {
            var value = randomizer.RollOnce(6);
            var chosen = Ra110Items[value - 1];
            if (value == 2 || value == 4 || value == 6)
            {
                var list = randomizer.RollBarabara(2, 6);
                var total = list.Sum();
                var listStr = string.Join(",", list);
                return $"＞ 臨界ロール({value}) ＞ {chosen} ： {total}[{listStr}] 回復";
            }
            return $"＞ 臨界ロール({value}) ＞ {chosen}";
        }
    }
}
