using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ライバー＆ラビリンス
    /// </summary>
    public sealed class LiverLabyrinth : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly LiverLabyrinth Instance = new LiverLabyrinth();

        /// <inheritdoc/>
        public override string Id => "LiverLabyrinth";

        /// <inheritdoc/>
        public override string Name => "ライバー＆ラビリンス";

        /// <inheritdoc/>
        public override string SortKey => "らいはああんとらひりんす";

        /// <inheritdoc/>
        public override RoundType RoundType => RoundType.Ceiling;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        同人TRPGシステム『ライバー＆ラビリンス』用ダイスボット。
        ・判定コマンド(xLL+y@c$d>=z)
          x：能力値
          +y：ダメージ判定時の攻撃力(省略可。省略時は0)
          c：クリティカル値(省略可。省略時は10)
          d：クリティカル時の加算値(省略可。省略時は1)
          z：難易度(4以下のとき5に。11以上は10になり、サイコロの数が減る）
          (例) 6LL@8>=6
               10LL>=5
               4LL+5@10$2>=10
        ・各種表 ：
            コマンド末尾に数字を入れると複数回の一括実行が可能　例）GETCT4
            コマンド末尾に""=""(イコール)と数字を入れると、特定のダイス目の結果の実行が可能　例）CRITICALT=5
          ・クリティカル表(CriticalT)
          ・命中ファンブル表(FumbleT)
          ・致命傷表(FatalT)
          ・休憩表(RestT)
          ・痛恨表(TerribleT)
          ・お宝表(レベル1~4)(GetCT)
          ・お宝表(レベル5~8)(GetRT)
          ・お宝表(レベル9~14)(GetSRT)
          ・お宝表(レベル15~99)(GetURT)
        ";

        // xLL(+y)?(@c)?($d)?>=z
        private static readonly Regex CheckRollRegex = new Regex(
            @"^(\d+)LL([+](\d+))?(@(\d+))?(\$(\d+))?>=(\d+)$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            Debug("eval_game_system_specific_command begin string", command);
            return CheckRoll(command, randomizer) ?? RollTableCommand(command, randomizer);
        }

        private Result? CheckRoll(string command, IRandomizer randomizer)
        {
            var m = CheckRollRegex.Match(command);
            if (!m.Success)
            {
                return null;
            }

            var dice_cnt = Convert.ToInt32(m.Groups[1].Value);
            var modify = m.Groups[3].Success && m.Groups[3].Value.Length > 0
                ? Convert.ToInt32(m.Groups[3].Value)
                : 0;
            var critical_target = m.Groups[5].Success && m.Groups[5].Value.Length > 0
                ? Convert.ToInt32(m.Groups[5].Value)
                : 10;
            var critical_addition = m.Groups[7].Success && m.Groups[7].Value.Length > 0
                ? Convert.ToInt32(m.Groups[7].Value)
                : 1;
            var target = Convert.ToInt32(m.Groups[8].Value);

            var text = "";
            if (target < 5)
            {
                text += $"【{command}】 ＞ あらゆる難易度は5未満にはならないため、難易度は5になる！\n";
                target = 5;
            }
            else if (target >= 11)
            {
                text += $"【{command}】 ＞ 難易度が11を超えたため、超過分、ダイスの数が減少！\n";
                var over = target - 10;
                target = 10;
                dice_cnt -= over;
            }

            if (dice_cnt < 0)
            {
                dice_cnt = 0;
            }

            text += $"【ダイスの数{dice_cnt}、難易度{target}、クリティカル{critical_target}{(critical_addition > 1 ? "(+" + critical_addition.ToString() + ")" : "")}{(modify > 0 ? "、攻撃力" + modify.ToString() : "")}】";

            var dice_arr = randomizer.RollBarabara(dice_cnt, 10);
            var dice_count = dice_arr.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

            var success_cnt = 0;
            var critical_cnt = 0;

            foreach (var kvp in dice_count)
            {
                int v = kvp.Key;
                int count = kvp.Value;

                if (count == 0)
                {
                    continue;
                }

                if (v >= target)
                {
                    success_cnt += count;
                }

                if (v >= critical_target)
                {
                    success_cnt += count * critical_addition;
                    critical_cnt += count;
                }
            }

            var dice_count_strs = new List<string>();
            for (int v = 1; v <= 10; v++)
            {
                int count = dice_count.ContainsKey(v) ? dice_count[v] : 0;
                if (count == 0)
                {
                    continue;
                }
                dice_count_strs.Add($"[{v}]×{count}");
            }

            var has_critical = critical_cnt >= 3;
            var has_fumble = dice_cnt > 0 && (dice_count.ContainsKey(1) ? dice_count[1] : 0) >= (int)Math.Ceiling((double)dice_cnt / 2);

            if (has_fumble)
            {
                // ファンブルの場合、クリティカルは無視される
                has_critical = false;
                success_cnt = 0;
            }

            var result = success_cnt > 0;

            text += $" ＞ {string.Join(",", dice_count_strs)} ＞ 成功度{success_cnt} ＞ {(result ? "成功" : "失敗")}{(has_critical ? "(クリティカル)" : "")}{(has_fumble ? "(ファンブル)" : "")}";

            if (result && modify > 0)
            {
                text += $" ＞ {success_cnt + modify}ダメージ";
            }

            return Result.CreateBuilder(text)
                .SetCritical(has_critical)
                .SetFumble(has_fumble)
                .SetSuccess(result)
                .SetFailure(!result)
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        private Result? RollTableCommand(string command, IRandomizer randomizer)
        {
            var upperCommand = command.ToUpper();
            var m = Regex.Match(upperCommand, @"^([A-Z]+)(\d+)?(=)?(\d+)?$");
            if (!m.Success)
            {
                return null;
            }

            var table_name = m.Groups[1].Value;
            if (!TABLES.ContainsKey(table_name))
            {
                return null;
            }
            var table = TABLES[table_name];

            var counts = 1;
            if (m.Groups[2].Success && m.Groups[2].Value.Length > 0)
            {
                counts = Convert.ToInt32(m.Groups[2].Value);
            }

            var hasOperator = m.Groups[3].Success && m.Groups[3].Value.Length > 0;
            var hasValue = m.Groups[4].Success && m.Groups[4].Value.Length > 0;
            int value = hasValue ? Convert.ToInt32(m.Groups[4].Value) : 0;

            if (hasOperator && (value <= 0 || value >= 11))
            {
                return null;
            }

            var result_texts = new List<string>();
            for (var i = 0; i < counts; i++)
            {
                string text;
                if (hasOperator && m.Groups[3].Value == "=" && hasValue)
                {
                    // 指定した出目の結果を直接取得
                    text = $"{table.Name}({value}) ＞ {table.GetItem(value)}";
                }
                else
                {
                    var rollResult = table.Roll(randomizer);
                    text = rollResult.Text;
                }
                result_texts.Add(text);
            }

            return Result.CreateBuilder(string.Join("\n", result_texts))
                .SetRands(randomizer.RandResults)
                .SetDetailedRands(randomizer.DetailedRandResults)
                .Build();
        }

        //####################
        // 各種表

        private static readonly Dictionary<string, LiverLabyrinthTable> TABLES = new Dictionary<string, LiverLabyrinthTable>
        {
            {
                "CRITICALT", new LiverLabyrinthTable(
                    "クリティカル表",
                    "CRITICALT",
                    new[]
                    {
                        "視聴者が沸き立つ一撃！閲覧数を1D10点増加させる。",
                        "致命的な一撃！最終的に与えるダメージが2倍になる。",
                        "肉体を変容させる一撃！ランダムで対象にバステを付与する。",
                        "魔力の消費を最小限に抑えることに成功！最終的にこのアクションで消費する《EP》が0になる。",
                        "取れ高発生！《トレダカ》を1点増加させる。",
                        "相手の動きを阻害することに成功！対象の《行動値》を0にする。",
                        "華麗に素材をゲット！《クレジット》を1D10点獲得する。",
                        "狙いが的確に決まった！対象のスキル、アプリ、ツールのうちどれか一つ、この戦闘の間、使用不能にする。",
                        "意識の外から刈り取る一撃！このアクションに対して、対象は防御判定を行えない。また、スキル、アプリ、ツールによるダメージ減少も無視する。",
                        "次の動作への連携が決まる！次に行う自身のアクションのクリティカル値を2点減少させる。",
                    })
            },
            {
                "FUMBLET", new LiverLabyrinthTable(
                    "命中ファンブル表",
                    "FUMBLET",
                    new[]
                    {
                        "急にコメントが荒れて攻撃を外してしまう。「炎上」のバステを受ける。",
                        "攻撃が自分に命中。1D10点のダメージを受ける（防御判定不可）",
                        "アクション中に盛大にすっころぶ。「ストップ」のバステを受ける。",
                        "アクションが大失敗。配信の空気が冷える。《トレダカ》が1点減少する。",
                        "魔力の消費が爆増！このアクションで消費した《EP》を再度消費する。",
                        "タンマツの調子が悪い。「オフライン」のバステを受ける。",
                        "敵のカウンターを受ける。1D10点のダメージを受ける（防御判定不可）",
                        "うっかり武器を落としてしまう。支援行動で武器を拾うまで、汎用アクション以外のアクションを行うことができない。",
                        "仲間との連携に失敗。ランダムな味方一人の《EP》を1D10点減少する。",
                        "攻撃は失敗だが、ネタとして大ウケ。《閲覧数》が1D10点増加する。",
                    })
            },
            {
                "FATALT", new LiverLabyrinthTable(
                    "致命傷表",
                    "FATALT",
                    new[]
                    {
                        "行動不能。ダンジョンに身体を侵食される。異形トロフィーを1つ獲得する。",
                        "ドラマチックなやられ方で配信が盛り上がる。《閲覧数》が2D10点増加する。自身は行動不能になる。",
                        "お前も道連れだ！自分にダメージを与えた対象に同じダメージを与える。このダメージ減少できない。自身は行動不能になる。",
                        "奇跡が起きた！？〔幸運〕で難易度10の判定に成功すると受けたダメージを０にする。",
                        "致命傷だがまだ動ける！《EP》を1にする。「スリップ」のバステを受ける。",
                        "行動不能。ダンジョンに身体を侵食される。異形トロフィーを1つ獲得する。",
                        "行動不能。だが、タンマツにはまだエネルギーが残っている。１ラウンド後、《EP》を1にして戦線に復帰する。",
                        "走馬灯が過る！走馬灯に回避のアイデアが！〔反応〕で難易度10の判定に成功すると、受けたダメージを０にする。",
                        "死んだかと思ったが、ギリギリのところで持ちこたえる。《EP》を1にする。",
                        "行動不能。ダンジョンに身体を侵食される。異形トロフィーを1つ獲得する。",
                    })
            },
            {
                "RESTT", new LiverLabyrinthTable(
                    "休憩表",
                    "RESTT",
                    new[]
                    {
                        "辺りを探索すると、ツールを発見する。誰かがここに残していたのだろうか？お宝表(レベル1~4)を一回振る。",
                        "希少な鉱床を発見。【ダンジョン資源(中級)】を一つ獲得。",
                        "自身の存在が大きくブレる。任意のアクションを一つ、別のアクションに変更してもよい。",
                        "素晴らしい戦術を思いつく。次回のバトルフェイズでの行動値判定で振ることができるダイスが１つ増える。",
                        "視聴者の無茶振りについつい応えてしまう。調子に乗りすぎて体力が…。 《EP》が1D10点減少する。《トレダカ》を1点獲得。",
                        "何気ない雑談配信。だが危うくリテラシーのない発言をしてしまい…。 〔魅力〕で難易度8の判定を行う。閲覧数が成功度分増加。判定に失敗した場合、「炎上」のバステを受ける。",
                        "休憩の合間にネットサーフィン。うわ！なんか変なリンク踏んだ！？〔技術〕で難易度9の判定を行う。失敗した場合、「フリーズ」のバステを受ける。成功した場合、奇跡的に冒険者用の通販サイトに繋がる。買い物を行うことができる。",
                        "急にタンマツのアプリのアップデートがはじまる。アップデートが重すぎて他の通信がうまくいかない！？〔幸運〕で難易度9の判定を行う。失敗した場合、「オフライン」のバステを受ける。成功した場合、タンマツのアプデが成功し、〔EP〕が全回復する。",
                        "バッチリ熟睡。しっかりとした休憩を取ることができた。〔EP〕が2D10点回復する。",
                        "やたら魔力の巡りがいい。絶好調ってやつか！？このセッションの間、すべての主能力が1点増加する。副能力の再計算を行うこと。",
                    })
            },
            {
                "TERRIBLET", new LiverLabyrinthTable(
                    "痛恨表",
                    "TERRIBLET",
                    new[]
                    {
                        "脳が揺さぶられた！「ブライン」のバステを付与する。",
                        "痛恨の一撃！最終的に与えるダメージが2倍になる。",
                        "肉体の動きを阻害する一撃！対象の《行動値》を0にする。",
                        "致命的な一撃！ダメージを与える代わりに、対象の《EP》を1にする。",
                        "追撃を決められてしまった！ダメージを2D10点追加する。",
                        "場外へ吹っ飛ばした！対象を戦場から取り除く。取り除かれた対象は、ラウンド終了時に最後尾に再配置する。",
                        "悔しいが見栄えする一撃だ！《閲覧数》が1D10点増加する。",
                        "衝撃が貫通する！アクションの対象になっていないキャラ1体を選択し、そのキャラにもダメージを与える。",
                        "意識の外から刈り取る一撃！このアクションに対して、対象は防御判定を行えない。また、スキル、アプリ、ツールによるダメージ減少も無視する。",
                        "魔力を奪う一撃！与えたダメージと同じ値だけ《EP》が回復する。",
                    })
            },
            {
                "GETCT", new LiverLabyrinthTable(
                    "お宝表(レベル1~4)",
                    "GETCT",
                    new[]
                    {
                        "携帯食料を1つ手に入れた！ ⇒54頁参照",
                        "エアバッグを1つ手に入れた！ ⇒53頁参照",
                        "携帯テントを1つ手に入れた！ ⇒54頁参照",
                        "特効薬を1つ手に入れた！ ⇒52頁参照",
                        "ダンジョン資源（低級）を1つ手に入れた！ ⇒55頁参照",
                        "スモークボールを1つ手に入れた！ ⇒53頁参照",
                        "ポーションを1つ手に入れた！ ⇒52頁参照",
                        "クイックポーションを1つ手に入れた！ ⇒52頁参照",
                        "ダンジョン資源（低級）を1つ手に入れた！ ⇒55頁参照",
                        "素晴らしい戦果で配信が盛り上がる！現在の閲覧数が1D10点上昇する。",
                    })
            },
            {
                "GETRT", new LiverLabyrinthTable(
                    "お宝表(レベル5~8)",
                    "GETRT",
                    new[]
                    {
                        "携帯保健室を1つ手に入れた！ ⇒54頁参照",
                        "マショウストーンを1つ手に入れた！ ⇒55頁参照",
                        "ぬいぐるみ爆弾を1つ手に入れた！ ⇒54頁参照",
                        "生命の粉塵を1つ手に入れた！ ⇒52頁参照",
                        "ダンジョン資源（中級）を1つ手に入れた！ ⇒55頁参照",
                        "パワーポーションを1つ手に入れた！ ⇒52頁参照",
                        "クリティカッターを1つ手に入れた！ ⇒53頁参照",
                        "ダンジョン資源（中級）を1つ手に入れた！ ⇒55頁参照",
                        "ハイポーションを1つ手に入れた！ ⇒52頁参照",
                        "素晴らしい戦果で配信が盛り上がる！現在の閲覧数が2D10点上昇する。",
                    })
            },
            {
                "GETSRT", new LiverLabyrinthTable(
                    "お宝表(レベル9~14)",
                    "GETSRT",
                    new[]
                    {
                        "携帯食料を1つ手に入れた！ ⇒54頁参照",
                        "フウマスリケンを1つ手に入れた！ ⇒55頁参照",
                        "ダンジョン資源（上級）を1つ手に入れた！ ⇒55頁参照",
                        "生命の粉塵を1つ手に入れた！ ⇒52頁参照",
                        "コンティニューコインを1つ手に入れた！ ⇒52頁参照",
                        "ダンジョン資源（低級）を1D10個手に入れた！ ⇒55頁参照",
                        "フィリピンバクチクを1つ手に入れた！ ⇒55頁参照",
                        "ダンジョン資源（上級）を1つ手に入れた！ ⇒55頁参照",
                        "携帯病院を1つ手に入れた！ ⇒54頁参照",
                        "素晴らしい戦果で配信が盛り上がる！現在の閲覧数が4D10点上昇する。",
                    })
            },
            {
                "GETURT", new LiverLabyrinthTable(
                    "お宝表(レベル15~99)",
                    "GETURT",
                    new[]
                    {
                        "ダンジョン資源（伝説）を1つ手に入れた！ ⇒55頁参照",
                        "マショウストーンを1D10個手に入れた！ ⇒55頁参照",
                        "エリキシルを1つ手に入れた！ ⇒54頁参照",
                        "ダンジョン資源（伝説）を1つ手に入れた！ ⇒55頁参照",
                        "盗賊の鍵を1つ手に入れた！ ⇒53頁参照",
                        "コンティニューコインを1つ手に入れた！ ⇒52頁参照",
                        "経験値を1つ手に入れた！ ⇒55頁参照",
                        "ダイナマイトを1つ手に入れた！ ⇒55頁参照",
                        "エリキシルを1つ手に入れた！ ⇒54頁参照",
                        "素晴らしい戦果で配信が盛り上がる！現在の閲覧数が8D10点上昇する。",
                    })
            },
        };

        /// <summary>
        /// LiverLabyrinth用の1d10テーブル（Choice メソッド付き）
        /// </summary>
        private class LiverLabyrinthTable
        {
            private readonly IReadOnlyList<string> _items;

            public string Name { get; }
            public string Command { get; }

            public LiverLabyrinthTable(string name, string command, IReadOnlyList<string> items)
            {
                Name = name;
                Command = command;
                _items = items;
            }

            /// <summary>
            /// 指定した出目の結果を取得する（1-indexed）
            /// </summary>
            public string GetItem(int index)
            {
                return _items[index - 1];
            }

            /// <summary>
            /// テーブルをロールする
            /// </summary>
            public Result Roll(IRandomizer randomizer)
            {
                int roll = randomizer.RollOnce(_items.Count);
                string item = _items[roll - 1];
                string text = $"{Name}({roll}) ＞ {item}";

                return Result.CreateBuilder(text)
                    .SetRands(randomizer.RandResults)
                    .SetDetailedRands(randomizer.DetailedRandResults)
                    .Build();
            }
        }
    }
}
