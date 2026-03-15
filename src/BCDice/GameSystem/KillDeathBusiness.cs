using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    /// <summary>
    /// キルデスビジネス
    /// </summary>
    public sealed class KillDeathBusiness : GameSystemBase
    {
        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static readonly KillDeathBusiness Instance = new KillDeathBusiness();

        /// <inheritdoc/>
        public override string Id => "KillDeathBusiness";

        /// <inheritdoc/>
        public override string Name => "キルデスビジネス";

        /// <inheritdoc/>
        public override string SortKey => "きるてすひしねす";

        /// <inheritdoc/>
        public override D66SortType D66SortType => D66SortType.Ascending;

        /// <inheritdoc/>
        public override string HelpMessage => @"
        ・判定
        　JDx or JDx±y or JDx,z JDx#z or JDx±y,z JDx±y#z
        　（x＝難易度、y＝補正、z＝ファンブル率(リスク)）
        ・履歴表 (HST)
        ・願い事表 (-WT)
        　死(DWT)、復讐(RWT)、勝利(VWT)、獲得(PWT)、支配(CWT)、繁栄(FWT)
        　強化(IWT)、健康(HWT)、安全(SAWT)、長寿(LWT)、生(EWT)
        ・万能命名表 (NAME, NAMEx) xに数字(1,2,3)で表を個別ロール
        ・サブプロット表 (-SPT)
        　オカルト(OSPT)、家族(FSPT)、恋愛(LOSPT)、正義(JSPT)、修行(TSPT)
        　笑い(BSPT)、意地悪(MASPT)、恨み(UMSPT)、人気(POSPT)、仕切り(PASPT)
        　金儲け(MOSPT)、対悪魔(ANSPT)
        ・シーン表 (ST)、サービスシーン表 (EST)
        ・CM表 (CMT)
        ・蘇生副作用表 (ERT)
        ・一週間表（WKT)
        ・ソウル放出表 (SOUL)
        ・汎用演出表 (STGT)
        ・ヘルスタイリスト罵倒表 (HSAT、HSATx) xに数字(1,2)で表を個別ロール
        ・指定特技ランダム決定表 (SKLT, RTTn nは分野番号)、指定特技分野ランダム決定表 (RCT, SKLJ)
        ・エキストラ表 (EXT、EXTx) xに数字(1,2,3,4)で表を個別ロール
        ・製作委員決定表　PCDT/実際どうだったのか表　OHT
        ・タスク表　ヘルライオン　PCT1/ヘルクロウ　PCT2/ヘルスネーク　PCT3/
        　ヘルドラゴン　PCT4/ヘルフライ　PCT5/ヘルゴート　PCT6/ヘルベア　PCT7
        ・大喜利スペシャル表 (-OT)
        　お題決定表(TOT)、〇〇を見て一言表(OOT)
        　単語表(WOT, WOTx) xに英字(A,B,C)で単語表A(人物)(AOT)、単語表B(物)(BOT)、単語表C(場所)を個別ロール
        　動詞表(VOT)、長め単語表(LOT)
        　ヘル司会者 リアクション表(好印象ver)(POT)、ヘル司会者 リアクション表(不満ver)(NOT)
        ・D66ダイスあり
        ";

        private static readonly Regex StRegex = new Regex(@"^ST(\d)?$", RegexOptions.Compiled);
        private static readonly Regex NameRegex = new Regex(@"^NAME(\d)?$", RegexOptions.Compiled);
        private static readonly Regex EstRegex = new Regex(@"^EST$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex HsatRegex = new Regex(@"^HSAT(\d)?$", RegexOptions.Compiled);
        private static readonly Regex ExtRegex = new Regex(@"^EXT(\d)?$", RegexOptions.Compiled);
        private static readonly Regex TotRegex = new Regex(@"^TOT?$", RegexOptions.Compiled);
        private static readonly Regex OotRegex = new Regex(@"^OOT?$", RegexOptions.Compiled);
        private static readonly Regex WotRegex = new Regex(@"^WOT?$", RegexOptions.Compiled);
        private static readonly Regex PotRegex = new Regex(@"^POT?$", RegexOptions.Compiled);
        private static readonly Regex NotRegex = new Regex(@"^NOT?$", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> ALIAS = new Dictionary<string, string>
        {
            { "DEATHWT", "DWT" },
            { "REVENGEWT", "RWT" },
            { "VICTORYWT", "VWT" },
            { "POSSESIONWT", "PWT" },
            { "CONTROLWT", "CWT" },
            { "FLOURISHWT", "FWT" },
            { "INTENSIFYWT", "IWT" },
            { "HEALTHWT", "HWT" },
            { "SAFETYWT", "SAWT" },
            { "LONGEVITYWT", "LWT" },
            { "EXISTWT", "EWT" },
            { "OCCULTSPT", "OSPT" },
            { "FAMILYSPT", "FSPT" },
            { "LOVESPT", "LOSPT" },
            { "JUSTICESPT", "JSPT" },
            { "TRAININGSPT", "TSPT" },
            { "BEAMSPT", "BSPT" },
        };

        /// <inheritdoc/>
        protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            if (command.StartsWith("JD"))
            {
                return JudgeDice(command, randomizer);
            }
            else
            {
                return RollTableCommand(command, randomizer);
            }
        }

        private Result? JudgeDice(string command, IRandomizer randomizer)
        {
            var fumbleMatch = Regex.Match(command, @",(\d+)$");

            var parseCommand = fumbleMatch.Success ? command.Substring(0, fumbleMatch.Index) : command;
            // TODO: Command.Parser integration
            // var parser = new Command.Parser(new Regex(@"JD\d+"), round_type: RoundType)
            //     .EnableCritical()
            //     .EnableFumble()
            //     .RestrictCmpOpTo(null);
            // var cmd = parser.Parse(parseCommand);
            // if (cmd == null) return null;

            var jdMatch = Regex.Match(parseCommand, @"^JD(\d+)([+\-]\d+)?$");
            if (!jdMatch.Success)
            {
                return null;
            }

            var target = int.Parse(jdMatch.Groups[1].Value);
            var modify = jdMatch.Groups[2].Success ? int.Parse(jdMatch.Groups[2].Value) : 0;
            var fumble = fumbleMatch.Success ? int.Parse(fumbleMatch.Groups[1].Value) : 0;

            var expr = JudgeExpr(target, modify, fumble);

            var result = "";

            if (target > 12)
            {
                result += $"\u3010{expr}\u3011 \uff1e {Translate("KillDeathBusiness.JD.warning.over_target_number")}\n";
                target = 12;
            }

            if (target < 5)
            {
                result += $"\u3010{expr}\u3011 \uff1e {Translate("KillDeathBusiness.JD.warning.min_target_is_five")}\n";
                target = 5;
            }

            if (fumble < 2)
            {
                fumble = 2;
            }
            else if (fumble > 11)
            {
                result += $"\u3010{expr}\u3011 \uff1e {Translate("KillDeathBusiness.JD.warning.over_fumble")}\n";
                fumble = 11;
            }

            var diceList = randomizer.RollBarabara(2, 6);
            var number = diceList.Sum();
            var diceText = string.Join(",", diceList);

            result += string.Join("", new[]
            {
                Translate("KillDeathBusiness.JD.options", new Dictionary<string, object> { { "target", target }, { "modifier", modify }, { "fumble", fumble } }),
                " \uff1e ",
                Translate("KillDeathBusiness.JD.dice_value", new Dictionary<string, object> { { "dice_value", diceText } }),
                " \uff1e ",
            });

            if (number == 2)
            {
                result += Translate("KillDeathBusiness.JD.fumble");
            }
            else if (number == 12)
            {
                result += Translate("KillDeathBusiness.JD.special");
            }
            else if (number <= fumble)
            {
                result += Translate("KillDeathBusiness.JD.less_than_fumble_target");
            }
            else
            {
                number += modify;
                if (number < target)
                {
                    result += Translate("KillDeathBusiness.JD.failure", new Dictionary<string, object> { { "value", number } });
                }
                else
                {
                    result += Translate("KillDeathBusiness.JD.success", new Dictionary<string, object> { { "value", number } });
                }
            }

            var text = Translate("KillDeathBusiness.JD.name") + result;
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private string JudgeExpr(int target, int modifier, int fumble)
        {
            var modStr = Format.Modifier(modifier);
            var fumbleStr = fumble > 0 ? $",{fumble}" : "";
            return $"JD{target}{modStr}{fumbleStr}";
        }

        private Result? RollTableCommand(string command, IRandomizer randomizer)
        {
            int type = 0;
            if (ALIAS.TryGetValue(command, out var aliased))
            {
                command = aliased;
            }

            // TODO: RollTables / RTT integration
            // var tableResult = RollTables(command, TABLES) ?? RTT.RollCommand(randomizer, command);
            // if (tableResult != null) return tableResult;

            var tableName = "";
            var result = "";
            string number = "";
            Match match;

            if ((match = StRegex.Match(command)).Success)
            {
                type = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                (tableName, result, number) = GetSceneTableResult(type, randomizer);
            }
            else if ((match = NameRegex.Match(command)).Success)
            {
                type = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                (tableName, result, number) = GetNameTableResult(type, randomizer);
            }
            else if ((match = EstRegex.Match(command)).Success)
            {
                (tableName, result, number) = GetServiceSceneTableResult(randomizer);
            }
            else if ((match = HsatRegex.Match(command)).Success)
            {
                type = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                (tableName, result, number) = GetHairStylistAbuseTableResult(type, randomizer);
            }
            else if ((match = ExtRegex.Match(command)).Success)
            {
                type = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
                (tableName, result, number) = GetExtraTableResult(type, randomizer);
            }
            else if ((match = TotRegex.Match(command)).Success)
            {
                (tableName, result, number) = GetThemeTableResult(randomizer);
            }
            else if ((match = OotRegex.Match(command)).Success)
            {
                string oneWord;
                (tableName, result, number, oneWord) = GetOneWordTableResult(randomizer);
            }
            else if ((match = WotRegex.Match(command)).Success)
            {
                string body;
                (tableName, result, number, body) = GetWordTableResult(randomizer);
            }
            else if ((match = PotRegex.Match(command)).Success)
            {
                (tableName, result, number) = GetPositiveTableResult(randomizer);
            }
            else if ((match = NotRegex.Match(command)).Success)
            {
                (tableName, result, number) = GetNegativeTableResult(randomizer);
            }

            if (string.IsNullOrEmpty(result))
            {
                return null;
            }

            var text = $"{tableName}({number}) \uff1e {result}";
            return Result.CreateBuilder(text)
                .SetRands(randomizer.RandResults)
                .Build();
        }

        private (string tableName, string result, string number) GetSceneTableResult(int type, IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.ST.name");

            var sceneTable1 = Translate("KillDeathBusiness.ST.table1");
            var sceneTable2 = Translate("KillDeathBusiness.ST.table2");

            string result;
            string number;

            switch (type)
            {
                case 1:
                    (result, number) = GetTableByD66Swap(sceneTable1, randomizer);
                    break;
                case 2:
                    (result, number) = GetTableByD66Swap(sceneTable2, randomizer);
                    break;
                default:
                {
                    var (result1, num1) = GetTableByD66Swap(sceneTable1, randomizer);
                    var (result2, num2) = GetTableByD66Swap(sceneTable2, randomizer);
                    result = Translate("KillDeathBusiness.ST.format", new Dictionary<string, object> { { "result1", result1 }, { "result2", result2 } });
                    number = $"{num1},{num2}";
                    break;
                }
            }

            return (tableName, result, number);
        }

        private (string tableName, string result, string number) GetNameTableResult(int type, IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.NAME.name");

            var nameTable1 = Translate("KillDeathBusiness.NAME.table1");
            var nameTable2 = Translate("KillDeathBusiness.NAME.table2");
            var nameTable3 = Translate("KillDeathBusiness.NAME.table3");

            string result;
            string number;

            switch (type)
            {
                case 1:
                    (result, number) = GetTableByD66Swap(nameTable1, randomizer);
                    break;
                case 2:
                    (result, number) = GetTableByD66Swap(nameTable2, randomizer);
                    break;
                case 3:
                    (result, number) = GetTableByD66Swap(nameTable3, randomizer);
                    break;
                default:
                {
                    var (result1, num1) = GetTableByD66Swap(nameTable1, randomizer);
                    var (result2, num2) = GetTableByD66Swap(nameTable2, randomizer);
                    var (result3, num3) = GetTableByD66Swap(nameTable3, randomizer);
                    result = $"{result1}{result2}{result3}";
                    number = $"{num1},{num2},{num3}";
                    break;
                }
            }

            return (tableName, result, number);
        }

        private (string tableName, string result, string number) GetServiceSceneTableResult(IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.EST.name");
            var tables = new[]
            {
                Translate("KillDeathBusiness.EST.tables.undressing"),
                Translate("KillDeathBusiness.EST.tables.violence"),
                Translate("KillDeathBusiness.EST.tables.travel"),
                Translate("KillDeathBusiness.EST.tables.love"),
                Translate("KillDeathBusiness.EST.tables.emotion"),
                Translate("KillDeathBusiness.EST.tables.other_genre"),
            };

            var number1 = randomizer.RollOnce(6);
            var sceneTable = tables[number1 - 1];

            var number2 = randomizer.RollOnce(6);
            // TODO: sceneTable is a translated object with :name and :items keys
            // var scene = sceneTable["items"][number2 - 1];
            // var result = Translate("KillDeathBusiness.EST.format", new Dictionary<string, object> { { "scene", sceneTable["name"] }, { "chosen", scene } });
            var result = $"({number1},{number2})"; // placeholder

            var number = $"{number1}{number2}";

            return (tableName, result, number);
        }

        private (string tableName, string result, string number) GetHairStylistAbuseTableResult(int type, IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.HSAT.name");

            var hellStylistAbuseTable1 = Translate("KillDeathBusiness.HSAT.abuse_table1");
            var hellStylistAbuseTable2 = Translate("KillDeathBusiness.HSAT.abuse_table2");
            var hellStylistPrefixTable = Translate("KillDeathBusiness.HSAT.prefix_table");
            var hellStylistSuffixTable = Translate("KillDeathBusiness.HSAT.suffix_table");

            string result;
            string number;

            switch (type)
            {
                case 1:
                    (result, number) = GetTableByD66Swap(hellStylistAbuseTable1, randomizer);
                    break;
                case 2:
                    (result, number) = GetTableByD66Swap(hellStylistAbuseTable2, randomizer);
                    break;
                default:
                {
                    var (result1, num1) = GetTableByD66Swap(hellStylistAbuseTable1, randomizer);
                    var (result2, num2) = GetTableByD66Swap(hellStylistAbuseTable2, randomizer);
                    var (before, _) = GetTableBy1d6(hellStylistPrefixTable, randomizer);
                    var (after, _2) = GetTableBy1d6(hellStylistSuffixTable, randomizer);
                    result = $"{before}{result1} {result2}{after}";
                    number = $"{num1},{num2}";
                    break;
                }
            }

            return (tableName, result, number);
        }

        private (string tableName, string result, string number) GetExtraTableResult(int type, IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.EXT.name");

            // extraTable1 has a special lambda entry for item 56
            var extraTable1 = new Dictionary<int, Func<string>>
            {
                { 11, () => Translate("KillDeathBusiness.EXT.table1.11") },
                { 12, () => Translate("KillDeathBusiness.EXT.table1.12") },
                { 13, () => Translate("KillDeathBusiness.EXT.table1.13") },
                { 14, () => Translate("KillDeathBusiness.EXT.table1.14") },
                { 15, () => Translate("KillDeathBusiness.EXT.table1.15") },
                { 16, () => Translate("KillDeathBusiness.EXT.table1.16") },
                { 22, () => Translate("KillDeathBusiness.EXT.table1.22") },
                { 23, () => Translate("KillDeathBusiness.EXT.table1.23") },
                { 24, () => Translate("KillDeathBusiness.EXT.table1.24") },
                { 25, () => Translate("KillDeathBusiness.EXT.table1.25") },
                { 26, () => Translate("KillDeathBusiness.EXT.table1.26") },
                { 33, () => Translate("KillDeathBusiness.EXT.table1.33") },
                { 34, () => Translate("KillDeathBusiness.EXT.table1.34") },
                { 35, () => Translate("KillDeathBusiness.EXT.table1.35") },
                { 36, () => Translate("KillDeathBusiness.EXT.table1.36") },
                { 44, () => Translate("KillDeathBusiness.EXT.table1.44") },
                { 45, () => Translate("KillDeathBusiness.EXT.table1.45") },
                { 46, () => Translate("KillDeathBusiness.EXT.table1.46") },
                { 55, () => Translate("KillDeathBusiness.EXT.table1.55") },
                { 56, () => Translate("KillDeathBusiness.EXT.table1.56", new Dictionary<string, object> { { "name", GetNameTableResult(0, randomizer).result } }) },
                { 66, () => Translate("KillDeathBusiness.EXT.table1.66") },
            };

            var extraTable2 = Translate("KillDeathBusiness.EXT.table2");
            var extraTable3 = Translate("KillDeathBusiness.EXT.table3");
            var extraTable4 = Translate("KillDeathBusiness.EXT.table4");

            string result;
            string number;

            switch (type)
            {
                case 1:
                    (result, number) = GetTableByD66SwapWithLambda(extraTable1, randomizer);
                    break;
                case 2:
                    (result, number) = GetTableByD66Swap(extraTable2, randomizer);
                    break;
                case 3:
                    (result, number) = GetTableByD66Swap(extraTable3, randomizer);
                    break;
                case 4:
                    (result, number) = GetTableByD66Swap(extraTable4, randomizer);
                    break;
                default:
                {
                    var (result1, num1) = GetTableByD66SwapWithLambda(extraTable1, randomizer);
                    var (result2, num2) = GetTableByD66Swap(extraTable2, randomizer);
                    var (result3, num3) = GetTableByD66Swap(extraTable3, randomizer);
                    var (result4, num4) = GetTableByD66Swap(extraTable4, randomizer);
                    result = $"{result1}{result2}\u304c{result3}{result4}";
                    number = $"{num1},{num2},{num3},{num4}";
                    break;
                }
            }

            return (tableName, result, number);
        }

        private (string tableName, string result, string number) GetThemeTableResult(IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.table.TOT.name");

            var result = "";
            var d6 = randomizer.RollOnce(6);

            switch (d6)
            {
                case 1:
                {
                    var (oneTableName, oneResult, oneD6, one) = GetOneWordTableResult(randomizer);
                    result += $"[{oneTableName}]\u3092\u898b\u3066\u4e00\u8a00\u3002\n{oneTableName}({oneD6}) \uff1e {oneResult}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.1", new Dictionary<string, object> { { "one", one } });
                    break;
                }
                case 2:
                {
                    var (word1TableName, word1Result, word1D6, word1) = GetWordTableResult(randomizer);
                    var (word2TableName, word2Result, word2D6, word2) = GetWordTableResult(randomizer);
                    result += $"\u3053\u306e[{word1TableName}]\u3001\u3072\u3087\u3063\u3068\u3057\u3066[{word1TableName}]\u304b\u3082\u3001\u3069\u3046\u3057\u3066\u305d\u3046\u601d\u3063\u305f\uff1f\n{word1TableName}({word1D6}) \uff1e {word1Result}\n{word2TableName}({word2D6}) \uff1e {word2Result}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.2", new Dictionary<string, object> { { "word1", word1 }, { "word2", word2 } });
                    break;
                }
                case 3:
                {
                    // TODO: TABLES["VOT"] integration
                    // var vot = TABLES["VOT"].Roll(randomizer);
                    // var verbTableName = vot.TableName;
                    // var verb = vot.Body;
                    // var votNumber = vot.Value;
                    var verbTableName = "\u52d5\u8a5e\u8868";
                    var verb = ""; // placeholder
                    var votNumber = "0";
                    var (wordTableName, wordResult, wordD6, word) = GetWordTableResult(randomizer);
                    result += $"[{verbTableName}]\u3057\u305f[{wordTableName}]\u304c\u8a00\u3044\u305d\u3046\u306a\u3053\u3068\u3002\n{verbTableName}({votNumber}) \uff1e {verb}\n{wordTableName}({wordD6}) \uff1e {wordResult}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.3", new Dictionary<string, object> { { "verb", verb }, { "word", word } });
                    break;
                }
                case 4:
                {
                    var (word1TableName, word1Result, word1D6, word1) = GetWordTableResult(randomizer);
                    var (word2TableName, word2Result, word2D6, word2) = GetWordTableResult(randomizer);
                    result += $"[{word1TableName}]\u304c[{word1TableName}]\u306b\u306a\u3063\u305f\u4e16\u754c\u3067\u306f\u3069\u3093\u306a\u3053\u3068\u304c\u8d77\u3053\u308b\uff1f\n{word1TableName}({word1D6}) \uff1e {word1Result}\n{word2TableName}({word2D6}) \uff1e {word2Result}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.4", new Dictionary<string, object> { { "word1", word1 }, { "word2", word2 } });
                    break;
                }
                case 5:
                {
                    var (wordTableName, wordResult, wordD6, word) = GetWordTableResult(randomizer);
                    result += $"\u3053\u3093\u306a[{wordTableName}]\u306f\u5acc\u3060\u3002\u3069\u3093\u306a\u306e\uff1f\n{wordTableName}({wordD6}) \uff1e {wordResult}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.5", new Dictionary<string, object> { { "word", word } });
                    break;
                }
                case 6:
                {
                    // TODO: TABLES["LOT"] integration
                    // var lot = TABLES["LOT"].Roll(randomizer);
                    // var longTableName = lot.TableName;
                    // var longWord = lot.Body;
                    // var lotNumber = lot.Value;
                    var longTableName = "\u9577\u3081\u5358\u8a9e\u8868";
                    var longWord = ""; // placeholder
                    var lotNumber = "0";
                    result += $"[{longTableName}]\u307f\u305f\u3044\u306a\u3053\u3068\u3092\u8a00\u3063\u3066\u4e0b\u3055\u3044\u3002\n{longTableName}({lotNumber}) \uff1e {longWord}\n\uff1e ";
                    result += Translate("KillDeathBusiness.table.TOT.items.6", new Dictionary<string, object> { { "long", longWord } });
                    break;
                }
            }

            return (tableName, result, d6.ToString());
        }

        private (string tableName, string result, string number, string oneWord) GetOneWordTableResult(IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.table.OOT.name");

            var result = "";
            var d6 = randomizer.RollOnce(6);
            var oneWord = "";

            switch (d6)
            {
                case 1:
                case 2:
                    oneWord = Translate("KillDeathBusiness.table.OOT.items.1");
                    result = oneWord;
                    break;
                case 3:
                case 4:
                    oneWord = Translate("KillDeathBusiness.table.OOT.items.3");
                    result = oneWord;
                    break;
                case 5:
                case 6:
                {
                    var (wordTableName, wordResult, wordD6, word) = GetWordTableResult(randomizer);
                    result += $"[{wordTableName}]\u3067\u691c\u7d22\u3057\u3066\u51fa\u3066\u304f\u308b\uff16\u756a\u76ee\u306e\u753b\u50cf\n{wordTableName}({wordD6}) \uff1e {wordResult}\n\uff1e ";
                    oneWord = Translate("KillDeathBusiness.table.OOT.items.5", new Dictionary<string, object> { { "word", word } });
                    result += oneWord;
                    break;
                }
            }

            return (tableName, result, d6.ToString(), oneWord);
        }

        private (string tableName, string result, string number, string body) GetWordTableResult(IRandomizer randomizer)
        {
            var tableName = "\u5358\u8a9e\u8868";

            var d6 = randomizer.RollOnce(6);

            // TODO: TABLES["WOTA"/"WOTB"/"WOTC"] integration
            // DiceTableResult table;
            // switch (d6)
            // {
            //     case 1: case 2: table = TABLES["WOTA"]; break;
            //     case 3: case 4: table = TABLES["WOTB"]; break;
            //     case 5: case 6: table = TABLES["WOTC"]; break;
            // }
            // var rollResult = table.Roll(randomizer);
            // return (tableName, rollResult.ToString(), d6.ToString(), rollResult.Body);

            var result = ""; // placeholder
            var body = ""; // placeholder

            return (tableName, result, d6.ToString(), body);
        }

        private (string tableName, string result, string number) GetPositiveTableResult(IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.table.POT.name");

            var table = new Func<string>[]
            {
                () => Translate("KillDeathBusiness.table.POT.items.1", new Dictionary<string, object> { { "size", randomizer.RollSum(1, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.POT.items.2", new Dictionary<string, object> { { "size", randomizer.RollSum(1, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.POT.items.3", new Dictionary<string, object> { { "size", randomizer.RollSum(2, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.POT.items.4", new Dictionary<string, object> { { "size", randomizer.RollSum(2, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.POT.items.5"),
                () => Translate("KillDeathBusiness.table.POT.items.6", new Dictionary<string, object> { { "size", (randomizer.RollSum(1, 6) - 3).ToString() } }),
            };

            var number = randomizer.RollOnce(6);
            var result = table[number - 1]();

            return (tableName, result, number.ToString());
        }

        private (string tableName, string result, string number) GetNegativeTableResult(IRandomizer randomizer)
        {
            var tableName = Translate("KillDeathBusiness.table.NOT.name");

            var table = new Func<string>[]
            {
                () => Translate("KillDeathBusiness.table.NOT.items.1"),
                () => Translate("KillDeathBusiness.table.NOT.items.2"),
                () => Translate("KillDeathBusiness.table.NOT.items.3", new Dictionary<string, object> { { "size", randomizer.RollSum(1, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.NOT.items.4", new Dictionary<string, object> { { "size", randomizer.RollSum(1, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.NOT.items.5", new Dictionary<string, object> { { "size", randomizer.RollSum(1, 6).ToString() } }),
                () => Translate("KillDeathBusiness.table.NOT.items.6"),
            };

            var number = randomizer.RollOnce(6);
            var result = table[number - 1]();

            return (tableName, result, number.ToString());
        }

        // Helper methods - these are typically defined in GameSystemBase
        // but are stubbed here until the base class integration is complete

        private (string result, string number) GetTableByD66Swap(object table, IRandomizer randomizer)
        {
            // TODO: Integrate with base class GetTableByD66Swap
            return ("", "0");
        }

        private (string result, string number) GetTableByD66SwapWithLambda(Dictionary<int, Func<string>> table, IRandomizer randomizer)
        {
            // D66 swap: roll 2d6, sort ascending, combine as tens+ones
            var d1 = randomizer.RollOnce(6);
            var d2 = randomizer.RollOnce(6);
            var low = Math.Min(d1, d2);
            var high = Math.Max(d1, d2);
            var key = low * 10 + high;

            if (table.TryGetValue(key, out var func))
            {
                var result = func();
                return (result, key.ToString());
            }

            return ("", key.ToString());
        }

        private (string result, string number) GetTableBy1d6(object table, IRandomizer randomizer)
        {
            // TODO: Integrate with base class GetTableBy1d6
            return ("", "0");
        }

        // TODO: TranslateTables and TranslateRtt - these are class-level table definitions
        // that should be initialized as static readonly dictionaries once the Table infrastructure is in place.
        //
        // private static Dictionary<string, ITable> TranslateTables(string locale)
        // {
        //     return new Dictionary<string, ITable>
        //     {
        //         { "HST", DiceTable.Table.FromI18n("KillDeathBusiness.table.HST", locale) },
        //         { "DWT", DiceTable.Table.FromI18n("KillDeathBusiness.table.DWT", locale) },
        //         // ... etc
        //     };
        // }
        //
        // private static SaiFicSkillTable TranslateRtt(string locale)
        // {
        //     return DiceTable.SaiFicSkillTable.FromI18n("KillDeathBusiness.RTT", locale,
        //         new Dictionary<string, string> { { "rtt", "SKLT" }, { "rct", "SKLJ" } });
        // }
    }
}
