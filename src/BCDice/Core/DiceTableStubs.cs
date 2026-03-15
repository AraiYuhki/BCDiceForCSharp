using System.Collections.Generic;

namespace BCDice
{
    /// <summary>
    /// DiceTable スタブ（Ruby BCDice DiceTable:: モジュール互換）
    /// i18n ベースのテーブルロード機能は未実装
    /// </summary>
    public static class DiceTable
    {
        /// <summary>通常ダイステーブル</summary>
        public class Table
        {
            public string Name { get; }
            public string Die { get; }
            public string[] Items { get; }

            public Table(string name, string die, string[] items)
            {
                Name = name; Die = die; Items = items;
            }

            /// <summary>i18nからテーブルを生成（スタブ）</summary>
            public static Table FromI18n(string key, object locale)
                => new Table(key, "1D6", new string[0]);
        }

        /// <summary>D66テーブル</summary>
        public class D66Table
        {
            public string Name { get; }
            public string[][] Items { get; }

            public D66Table(string name, string[][] items)
            {
                Name = name; Items = items;
            }

            /// <summary>i18nからD66テーブルを生成（スタブ）</summary>
            public static D66Table FromI18n(string key, object locale)
                => new D66Table(key, new string[0][]);
        }

        /// <summary>チェーンテーブル</summary>
        public class ChainTable
        {
            public string Name { get; }
            public string Die { get; }
            public object[] Tables { get; }

            public ChainTable(string name, string die, object[] tables)
            {
                Name = name; Die = die; Tables = tables;
            }
        }
    }

    /// <summary>
    /// I18n スタブ（Ruby i18n gem 互換）
    /// 翻訳はキー文字列をそのまま返す
    /// </summary>
    public static class I18n
    {
        /// <summary>翻訳（スタブ）キーをそのまま返す</summary>
        public static string Translate(string key, Dictionary<string, object>? opts = null)
            => key;

        /// <summary>翻訳（スタブ）配列版</summary>
        public static string[] TranslateArray(string key, object? locale = null)
            => new string[0];
    }
}
