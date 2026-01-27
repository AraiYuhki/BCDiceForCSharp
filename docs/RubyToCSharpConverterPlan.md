# Ruby→C# 自動変換ツール実装計画書

## 概要

BCDice（Ruby版）のゲームシステム実装ファイルを解析し、C#コードを自動生成するツール。
200以上のゲームシステムを効率的に移植し、将来のアップデートにも追従可能にする。

## 目標

1. Ruby版BCDiceのGameSystemファイル（.rb）をC#ファイル（.cs）に変換
2. 変換精度80%以上（残り20%は手動調整）
3. 新規システム追加時の作業時間を大幅短縮

---

## Phase 1: 基本構造の変換

### 対象
- クラス定義とメタデータ
- 定数（ID, NAME, SORT_KEY, HELP_MESSAGE）
- 初期化処理

### Ruby入力例
```ruby
module BCDice
  module GameSystem
    class Cthulhu < Base
      ID = 'Cthulhu'
      NAME = 'クトゥルフ神話TRPG'
      SORT_KEY = 'くとうるふしんわTRPG'

      HELP_MESSAGE = <<~TEXT
        ・1D100<=n    技能判定
        ・CCB<=n      組み合わせロール
      TEXT

      def initialize(command)
        super(command)
        @d66_sort_type = D66SortType::NO_SORT
      end
    end
  end
end
```

### C#出力例
```csharp
using System;
using System.Text.RegularExpressions;
using BCDice.Core;

namespace BCDice.GameSystem
{
    public sealed class Cthulhu : GameSystemBase
    {
        public static readonly Cthulhu Instance = new Cthulhu();

        public override string Id => "Cthulhu";
        public override string Name => "クトゥルフ神話TRPG";
        public override string SortKey => "くとうるふしんわTRPG";
        public override D66SortType D66SortType => D66SortType.NoSort;

        public override string HelpMessage => @"
・1D100<=n    技能判定
・CCB<=n      組み合わせロール
";
    }
}
```

### 実装タスク
- [ ] Rubyファイルパーサー（クラス定義抽出）
- [ ] 定数抽出（ID, NAME, SORT_KEY, HELP_MESSAGE）
- [ ] ヒアドキュメント（<<~TEXT）のパース
- [ ] D66SortType変換マッピング
- [ ] C#テンプレート生成

---

## Phase 2: コマンド処理の変換

### 対象
- `register_prefix` の解析
- `eval_game_system_specific_command` メソッド
- `case/when` パターンマッチング
- 正規表現の変換

### Ruby入力例
```ruby
register_prefix('CC', 'CCB', 'RES', '\d+D\d+')

def eval_game_system_specific_command(command)
  case command
  when /^CC<=?(\d+)$/i
    roll_cc(Regexp.last_match(1).to_i)
  when /^CCB<=?(\d+)$/i
    roll_ccb(Regexp.last_match(1).to_i)
  when /^RES\((\d+)-(\d+)\)$/i
    roll_res(Regexp.last_match(1).to_i, Regexp.last_match(2).to_i)
  end
end

private

def roll_cc(target)
  total = @randomizer.roll_once(100)
  if total <= target
    Result.success("(1D100<=#{target}) ＞ #{total} ＞ 成功")
  else
    Result.failure("(1D100<=#{target}) ＞ #{total} ＞ 失敗")
  end
end
```

### C#出力例
```csharp
private static readonly Regex CcRegex = new Regex(
    @"^CC<=?(\d+)$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

private static readonly Regex CcbRegex = new Regex(
    @"^CCB<=?(\d+)$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

private static readonly Regex ResRegex = new Regex(
    @"^RES\((\d+)-(\d+)\)$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
{
    Match match;

    match = CcRegex.Match(command);
    if (match.Success)
    {
        return RollCc(int.Parse(match.Groups[1].Value), randomizer);
    }

    match = CcbRegex.Match(command);
    if (match.Success)
    {
        return RollCcb(int.Parse(match.Groups[1].Value), randomizer);
    }

    match = ResRegex.Match(command);
    if (match.Success)
    {
        return RollRes(
            int.Parse(match.Groups[1].Value),
            int.Parse(match.Groups[2].Value),
            randomizer);
    }

    return base.EvalGameSystemSpecificCommand(command, randomizer);
}

private Result RollCc(int target, IRandomizer randomizer)
{
    int total = randomizer.RollOnce(100);
    if (total <= target)
    {
        return Result.CreateBuilder($"(1D100<={target}) ＞ {total} ＞ 成功")
            .SetSuccess(true)
            .SetRands(randomizer.RandResults)
            .Build();
    }
    else
    {
        return Result.CreateBuilder($"(1D100<={target}) ＞ {total} ＞ 失敗")
            .SetFailure(true)
            .SetRands(randomizer.RandResults)
            .Build();
    }
}
```

### 変換ルール

| Ruby | C# |
|------|-----|
| `/^pattern$/i` | `@"^pattern$"` + `RegexOptions.IgnoreCase` |
| `Regexp.last_match(1)` | `match.Groups[1].Value` |
| `Regexp.last_match(1).to_i` | `int.Parse(match.Groups[1].Value)` |
| `@randomizer.roll_once(n)` | `randomizer.RollOnce(n)` |
| `@randomizer.roll_sum(n, sides)` | `randomizer.RollSum(n, sides)` |
| `@randomizer.roll_barabara(n, sides)` | `randomizer.RollBarabara(n, sides)` |
| `Result.success(text)` | `Result.CreateBuilder(text).SetSuccess(true).Build()` |
| `Result.failure(text)` | `Result.CreateBuilder(text).SetFailure(true).Build()` |
| `Result.critical(text)` | `Result.CreateBuilder(text).SetCritical(true).Build()` |
| `Result.fumble(text)` | `Result.CreateBuilder(text).SetFumble(true).Build()` |
| `"#{var}"` | `$"{var}"` または `string.Format` |

### 実装タスク
- [ ] `register_prefix` パーサー
- [ ] `case/when` → `if/else` 変換
- [ ] Ruby正規表現 → C#正規表現 変換
- [ ] `Regexp.last_match` → `Match.Groups` 変換
- [ ] Randomizerメソッド呼び出し変換
- [ ] Result生成パターン変換
- [ ] 文字列補間変換

---

## Phase 3: テーブル定義の変換

### 対象
- `DiceTable::Table`
- `DiceTable::D66GridTable`
- `DiceTable::RangeTable`
- `DiceTable::ChainTable`
- テーブルのi18n定義

### Ruby入力例
```ruby
TABLES = {
  'FT' => DiceTable::Table.new(
    'ファンブル表',
    '2D6',
    [
      '大失敗',
      '装備破損',
      '転倒',
      '何も起きない',
      '転倒',
      '装備破損',
      '大失敗'
    ]
  ),
  'CT' => DiceTable::RangeTable.new(
    'クリティカル表',
    '2D6',
    [
      [2..4, '効果小'],
      [5..9, '効果中'],
      [10..12, '効果大']
    ]
  )
}.freeze

def roll_tables(command, tables)
  table = tables[command]
  return nil unless table
  table.roll(@randomizer)
end
```

### C#出力例
```csharp
private static readonly Dictionary<string, ITable> Tables = new Dictionary<string, ITable>
{
    ["FT"] = new SimpleTable(
        "ファンブル表",
        2, 6,
        new[] {
            "大失敗",
            "装備破損",
            "転倒",
            "何も起きない",
            "転倒",
            "装備破損",
            "大失敗"
        }),
    ["CT"] = new RangeTable(
        "クリティカル表",
        2, 6,
        new (int Min, int Max, string Result)[] {
            (2, 4, "効果小"),
            (5, 9, "効果中"),
            (10, 12, "効果大")
        })
};

private Result? RollTable(string command, IRandomizer randomizer)
{
    if (!Tables.TryGetValue(command, out var table))
    {
        return null;
    }
    return table.Roll(randomizer);
}
```

### 実装タスク
- [ ] テーブル定義パーサー
- [ ] SimpleTable変換
- [ ] RangeTable変換
- [ ] D66Table変換
- [ ] i18n YAMLパーサー（テーブル定義抽出）
- [ ] テーブルコレクション生成

---

## Phase 4: i18n（国際化）対応

### 対象
- YAMLファイルからのテーブル定義抽出
- 翻訳テキストの埋め込み
- 多言語バリアント生成

### Ruby i18n構造
```yaml
# i18n/ja.yml
Cthulhu:
  name: "クトゥルフ神話TRPG"
  help_message: |
    ・CC<=n  技能判定
  result:
    success: "成功"
    failure: "失敗"
    critical: "クリティカル"
    fumble: "ファンブル"
  table:
    sanity:
      name: "正気度喪失表"
      type: "1D10"
      items:
        - "軽微な狂気"
        - "一時的狂気"
        ...
```

### C#出力方針
- 日本語版: 直接文字列埋め込み（デフォルト）
- 多言語対応: リソースファイル（.resx）または定数クラス

### 実装タスク
- [ ] YAMLパーサー統合
- [ ] テーブル定義抽出
- [ ] 翻訳テキスト抽出
- [ ] 言語バリアント生成オプション

---

## Phase 5: CLI・統合テスト

### CLIインターフェース
```bash
# 単一ファイル変換
dotnet run -- convert path/to/Cthulhu.rb -o output/

# ディレクトリ一括変換
dotnet run -- convert-all path/to/game_system/ -o output/

# 差分チェック（既存C#との比較）
dotnet run -- diff path/to/ruby/ path/to/csharp/

# 検証（変換結果のコンパイルチェック）
dotnet run -- validate output/
```

### 実装タスク
- [ ] CLIフレームワーク（System.CommandLine）
- [ ] 単一ファイル変換コマンド
- [ ] 一括変換コマンド
- [ ] 差分検出機能
- [ ] コンパイル検証機能
- [ ] 変換レポート生成

---

## ディレクトリ構造

```
tools/
└── RubyToCSharpConverter/
    ├── RubyToCSharpConverter.csproj
    ├── Program.cs
    │
    ├── Parsing/
    │   ├── RubyLexer.cs           # トークン化
    │   ├── RubyToken.cs           # トークン定義
    │   ├── RubyParser.cs          # AST構築
    │   ├── RubyAst.cs             # AST ノード定義
    │   └── GameSystemExtractor.cs # ゲームシステム情報抽出
    │
    ├── Analysis/
    │   ├── CommandAnalyzer.cs     # コマンドパターン解析
    │   ├── TableAnalyzer.cs       # テーブル定義解析
    │   └── RegexAnalyzer.cs       # 正規表現解析
    │
    ├── Conversion/
    │   ├── CSharpGenerator.cs     # C#コード生成
    │   ├── ClassConverter.cs      # クラス構造変換
    │   ├── MethodConverter.cs     # メソッド変換
    │   ├── RegexConverter.cs      # 正規表現変換
    │   ├── TableConverter.cs      # テーブル変換
    │   └── StringConverter.cs     # 文字列補間変換
    │
    ├── I18n/
    │   ├── YamlParser.cs          # YAML解析
    │   └── LocaleExtractor.cs     # ロケール情報抽出
    │
    ├── Templates/
    │   ├── GameSystemTemplate.cs  # クラステンプレート
    │   └── TemplateEngine.cs      # テンプレートエンジン
    │
    ├── Validation/
    │   ├── SyntaxValidator.cs     # 構文検証
    │   └── CompileValidator.cs    # コンパイル検証
    │
    └── Commands/
        ├── ConvertCommand.cs      # 変換コマンド
        ├── ConvertAllCommand.cs   # 一括変換コマンド
        ├── DiffCommand.cs         # 差分コマンド
        └── ValidateCommand.cs     # 検証コマンド
```

---

## 変換難易度分類

### 自動変換可能（90%以上）
- クラス定義、メタデータ（ID, NAME等）
- シンプルな正規表現マッチング
- 基本的なRandomizerメソッド呼び出し
- シンプルなテーブル定義
- 文字列補間

### 半自動変換（要調整）
- 複雑なcase/when分岐
- ネストした正規表現
- 動的なテーブル生成
- Rubyブロック構文

### 手動変換必要
- メタプログラミング（define_method等）
- 複雑な継承・ミックスイン
- 外部ライブラリ依存
- 非標準的な実装パターン

---

## 優先度とスケジュール案

| Phase | 優先度 | 推定作業量 | 変換カバー率 |
|-------|--------|-----------|-------------|
| Phase 1 | 高 | 2-3日 | 基本構造100% |
| Phase 2 | 高 | 5-7日 | コマンド処理70% |
| Phase 3 | 中 | 3-4日 | テーブル80% |
| Phase 4 | 低 | 2-3日 | i18n対応 |
| Phase 5 | 中 | 2-3日 | CLI・検証 |

**合計推定**: 2-3週間

---

## 参考リソース

- BCDice本家リポジトリ: https://github.com/bcdice/BCDice
- ゲームシステム一覧: https://bcdice.org/systems/
- Ruby正規表現ドキュメント: https://docs.ruby-lang.org/ja/latest/doc/spec=2fregexp.html
- C#正規表現ドキュメント: https://docs.microsoft.com/ja-jp/dotnet/standard/base-types/regular-expressions

---

## 次のアクション

1. BCDice本家リポジトリをクローン
2. Phase 1の基本パーサー実装開始
3. テスト用にシンプルなゲームシステム（Cthulhu）で検証
4. 段階的に変換ルールを拡充
