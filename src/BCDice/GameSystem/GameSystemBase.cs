using System;
using System.Collections.Generic;
using System.Linq;
using BCDice.CommonCommand;
using BCDice.CommonCommand.AddDice;
using BCDice.Core;
using BCDice.Preprocessor;
using BCDice.Table;

namespace BCDice.GameSystem
{
    /// <summary>
    /// ゲームシステムの基底クラス
    /// </summary>
    public abstract class GameSystemBase : IGameSystem, IGameSystemContext
    {
        /// <inheritdoc/>
        public abstract string Id { get; }

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <inheritdoc/>
        public virtual string SortKey => Name;

        /// <inheritdoc/>
        public virtual string HelpMessage => "";

        /// <inheritdoc/>
        public virtual RoundType RoundType => RoundType.Floor;

        /// <inheritdoc/>
        public virtual D66SortType D66SortType => D66SortType.NoSort;

        /// <summary>
        /// 暗黙のダイス面数（デフォルト: 6）
        /// </summary>
        public virtual int SidesImplicitD => 6;

        /// <summary>
        /// デフォルトの比較演算子
        /// </summary>
        public virtual CompareOperator? DefaultCmpOp => null;

        /// <summary>
        /// デフォルトの目標値
        /// </summary>
        public virtual int? DefaultTargetNumber => null;

        /// <summary>
        /// バラバラダイスでダイス目をソートするかどうか
        /// </summary>
        public virtual bool SortBarabaraDice => false;

        /// <summary>
        /// RerollDiceで振り足しをする出目の閾値
        /// </summary>
        public virtual int? RerollDiceRerollThreshold => null;

        /// <summary>
        /// グリッチテキストを取得する（シャドウラン用）
        /// </summary>
        public virtual string? GetGrichText(int countOne, int diceCount, int successCount)
        {
            return null;
        }

        /// <summary>
        /// 共通コマンドのリスト
        /// </summary>
        protected virtual IReadOnlyList<ICommonCommand> CommonCommands { get; } = new ICommonCommand[]
        {
            VersionCommand.Instance,
            CalcCommand.Instance,
            ChoiceCommand.Instance,
            D66Command.Instance,
            RepeatCommand.Instance,
            UpperDiceCommand.Instance,
            LowerDiceCommand.Instance,
            CountSuccessCommand.Instance,
            BarabaraDiceCommand.Instance,
            TallyDiceCommand.Instance,
            RerollDiceCommand.Instance,
            AddDiceCommand.Instance,
        };

        /// <summary>
        /// 現在のランダマイザー（テーブルヘルパーメソッド用）
        /// </summary>
        protected IRandomizer? _randomizer;

        /// <inheritdoc/>
        public Result? Eval(string command)
        {
            return Eval(command, new Randomizer());
        }

        /// <inheritdoc/>
        public Result? Eval(string command, IRandomizer randomizer)
        {
            _randomizer = randomizer;
            if (string.IsNullOrWhiteSpace(command))
            {
                return null;
            }

            // コマンドを前処理
            string normalizedCommand = NormalizeCommand(command);

            // ゲームシステム固有コマンドを先に試す
            var result = EvalGameSystemSpecificCommand(normalizedCommand, randomizer);
            if (result != null)
            {
                return result;
            }

            // 共通コマンドを試す
            return EvalCommonCommand(normalizedCommand, randomizer);
        }

        /// <summary>
        /// ゲームシステム固有のコマンドを評価する
        /// </summary>
        /// <param name="command">正規化されたコマンド</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、認識されない場合はnull</returns>
        protected virtual Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
        {
            return null;
        }

        /// <summary>
        /// 共通コマンドを評価する
        /// </summary>
        /// <param name="command">正規化されたコマンド</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>評価結果、認識されない場合はnull</returns>
        protected virtual Result? EvalCommonCommand(string command, IRandomizer randomizer)
        {
            foreach (var commonCommand in CommonCommands)
            {
                var result = commonCommand.Eval(command, this, randomizer);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// コマンドを正規化する
        /// </summary>
        /// <param name="command">入力コマンド</param>
        /// <returns>正規化されたコマンド</returns>
        protected virtual string NormalizeCommand(string command)
        {
            // 先頭・末尾の空白を除去し、大文字に変換
            string normalized = command.Trim().ToUpperInvariant();

            // ゲームシステム固有のテキスト変換を適用
            normalized = ChangeText(normalized);

            return normalized;
        }

        /// <inheritdoc/>
        public virtual string ChangeText(string text)
        {
            return text;
        }

        // ===== テーブルヘルパーメソッド (Ruby BCDice base.rb 互換) =====

        /// <summary>
        /// nDxロールでテーブルを参照する
        /// </summary>
        protected (string text, int num) GetTableByNdx(string[] table, int count, int diceType)
        {
            int num = _randomizer!.RollSum(count, diceType);
            int index = num - count;
            if (index < 0 || index >= table.Length || table[index] == null)
                return ("1", 0);
            return (table[index], num);
        }

        /// <summary>
        /// 1D6ロールでテーブルを参照する
        /// </summary>
        protected (string text, int num) GetTableBy1d6(string[] table)
            => GetTableByNdx(table, 1, 6);

        /// <summary>
        /// nD6ロールでテーブルを参照する
        /// </summary>
        protected (string text, int num) GetTableByNd6(string[] table, int count)
            => GetTableByNdx(table, count, 6);

        /// <summary>
        /// D66ロールでテーブルを参照する
        /// </summary>
        protected (string text, string indexText) GetTableByD66(string[] table)
        {
            int dice1 = _randomizer!.RollOnce(6);
            int dice2 = _randomizer!.RollOnce(6);
            int num = (dice1 - 1) * 6 + (dice2 - 1);
            string indexText = $"{dice1}{dice2}";
            if (num >= table.Length || table[num] == null)
                return ("1", indexText);
            return (table[num], indexText);
        }

        /// <summary>
        /// インデックスで(int key, string[][] data)[]テーブルを参照する
        /// </summary>
        protected static string[][] GetTableByNumber(int index, object[][] table)
        {
            foreach (var item in table)
            {
                int num = (int)item[0];
                if (num >= index)
                    return (string[][])item[1];
            }
            return new[] { new[] { "1" } };
        }

        /// <summary>
        /// デバッグ出力（Ruby互換スタブ）
        /// </summary>
        protected static void Debug(string target, params object[] values) { }

        // ===== i18n / テーブルヘルパー (Ruby BCDice 互換スタブ) =====

        /// <summary>
        /// i18n翻訳スタブ（Ruby translate 互換、キー文字列をそのまま返す）
        /// </summary>
        protected virtual string Translate(string key, Dictionary<string, object>? vars = null) => key;

        /// <summary>
        /// コマンドに対応するテーブルをロールする（Ruby roll_table 互換スタブ）
        /// </summary>
        protected virtual Result? RollTable(string command, Dictionary<string, object> tables, IRandomizer randomizer)
        {
            return null;
        }

        /// <summary>
        /// コマンドに対応するテーブルをロールする（Ruby roll_tables 互換スタブ）
        /// </summary>
        protected virtual Result? RollTables(string command, Dictionary<string, object> tables)
        {
            return null;
        }

        /// <summary>
        /// SimpleTableの辞書でテーブルをロールする（Dictionary型変換オーバーロード）
        /// </summary>
        protected Result? RollTables(string command, Dictionary<string, SimpleTable> tables)
        {
            var converted = new Dictionary<string, object>(tables.Count, tables.Comparer);
            foreach (var kv in tables)
            {
                converted[kv.Key] = kv.Value;
            }
            return RollTables(command, converted);
        }

        /// <summary>
        /// D66スワップテーブルを参照する（小さい目を十の位にする）
        /// </summary>
        protected (string text, string indexText) GetTableByD66Swap(string tableData, IRandomizer randomizer)
        {
            return GetTableByD66(new string[] { tableData });
        }

        /// <summary>
        /// D66スワップテーブルを参照する（object[][]形式のテーブル）
        /// </summary>
        protected (string text, string indexText) GetTableByD66Swap(object[][] table, IRandomizer randomizer)
        {
            int dice1 = _randomizer!.RollOnce(6);
            int dice2 = _randomizer!.RollOnce(6);
            int lo = Math.Min(dice1, dice2);
            int hi = Math.Max(dice1, dice2);
            string indexText = $"{lo}{hi}";
            int num = (lo - 1) * 6 + (hi - 1);
            if (num >= table.Length)
                return ("1", indexText);
            return (table[num][1]?.ToString() ?? "1", indexText);
        }
    }
}