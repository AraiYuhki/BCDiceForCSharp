using System;
using System.Collections.Generic;
using BCDice.Core;

namespace BCDice.Table
{
    /// <summary>
    /// テーブルのコレクション
    /// </summary>
    public class TableCollection
    {
        private readonly Dictionary<string, ITable> _tables;

        /// <summary>
        /// 新しいテーブルコレクションを作成する
        /// </summary>
        public TableCollection()
        {
            _tables = new Dictionary<string, ITable>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 登録されているテーブルの数
        /// </summary>
        public int Count => _tables.Count;

        /// <summary>
        /// テーブルを登録する
        /// </summary>
        /// <param name="table">登録するテーブル</param>
        public void Add(ITable table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            _tables[table.Command] = table;
        }

        /// <summary>
        /// 複数のテーブルを一括登録する
        /// </summary>
        /// <param name="tables">登録するテーブル</param>
        public void AddRange(IEnumerable<ITable> tables)
        {
            foreach (var table in tables)
            {
                Add(table);
            }
        }

        /// <summary>
        /// コマンドに対応するテーブルを取得する
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <returns>テーブル、見つからない場合はnull</returns>
        public ITable? GetByCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                return null;
            }

            return _tables.TryGetValue(command, out var table) ? table : null;
        }

        /// <summary>
        /// コマンドを評価してテーブルをロールする
        /// </summary>
        /// <param name="command">コマンド文字列</param>
        /// <param name="randomizer">乱数生成器</param>
        /// <returns>結果、テーブルが見つからない場合はnull</returns>
        public Result? Eval(string command, IRandomizer randomizer)
        {
            var table = GetByCommand(command);
            return table?.Roll(randomizer);
        }

        /// <summary>
        /// 登録されている全てのテーブルを取得する
        /// </summary>
        public IEnumerable<ITable> GetAll()
        {
            return _tables.Values;
        }

        /// <summary>
        /// 全てのテーブルを削除する
        /// </summary>
        public void Clear()
        {
            _tables.Clear();
        }
    }
}
