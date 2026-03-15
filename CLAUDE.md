# CLAUDE.md - BCDice for C# プロジェクトガイドライン

このファイルは Claude Code がこのリポジトリで作業する際のガイダンスを提供します。

## プロジェクト概要

BCDice の C# 移植版。Ruby 版 BCDice 3.x の機能を C# で再実装し、.NET Standard 2.0 をターゲットとして Unity LTS との互換性を確保しています。

## ビルド・テストコマンド

```bash
# ビルド
dotnet build

# 全テスト実行
dotnet test

# 単一テスト実行
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# 特定クラスのテスト実行
dotnet test --filter "FullyQualifiedName~SatasupeTests"
```

## 設計原則（優先順位順）

1. **可読性** - コードは明確で理解しやすく
2. **パフォーマンス** - 効率的な実装
3. **拡張性** - 新しいゲームシステムの追加が容易
4. **堅牢性** - エラー処理とエッジケース対応

## コーディング規約

### 一般
- **1ファイル1クラス** - 各クラスは独自のファイルに配置
- **DRY原則** - 重複コードを避ける
- ファイル名はクラス名と一致させる
- 日本語コメント・ドキュメント可

### 命名規則
- クラス・メソッド: PascalCase (`GameSystemBase`, `EvalCommand`)
- プライベートフィールド: _camelCase (`_registry`, `_currentSystem`)
- ローカル変数・パラメータ: camelCase (`command`, `randomizer`)
- 定数: PascalCase (`MaxRerolls`) または UPPER_SNAKE_CASE

### パターン
- **シングルトン**: ゲームシステムとコマンドは `public static readonly Instance` を使用
- **ビルダー**: `Result` オブジェクトは `Result.CreateBuilder()` で構築
- **インターフェース依存**: `IRandomizer`, `IGameSystem`, `ICommonCommand` を使用

## ディレクトリ構造

```
src/BCDice/
├── Core/           # IRandomizer, Result, 列挙型など
├── Arithmetic/     # 式のレキサー・パーサー・評価器
├── Preprocessor/   # コマンド前処理、コンテキスト
├── CommonCommand/  # AddDiceCommand, D66Command など
├── Command/        # コマンド基盤クラス
├── Table/          # SimpleTable, RangeTable, D66Table
└── GameSystem/     # 各ゲームシステム実装

tests/BCDice.Tests/
├── Core/           # MockRandomizer など
├── Arithmetic/     # パーサーテスト
├── CommonCommand/  # 共通コマンドテスト
├── Table/          # テーブルテスト
└── GameSystem/     # ゲームシステムテスト
```

## ゲームシステム実装パターン

新しいゲームシステムを追加する際の標準パターン：

```csharp
public sealed class NewGameSystem : GameSystemBase
{
    // シングルトンインスタンス
    public static readonly NewGameSystem Instance = new NewGameSystem();

    // コマンドマッチング用の正規表現
    private static readonly Regex CommandRegex = new Regex(
        @"^PATTERN$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // 必須プロパティ
    public override string Id => "NewGameSystem";
    public override string Name => "新しいゲームシステム";
    public override string SortKey => "あたらしいけえむしすてむ";  // ひらがなソート用

    // オプション: D66ソートタイプ
    public override D66SortType D66SortType => D66SortType.Ascending;

    // ヘルプメッセージ
    public override string HelpMessage => @"
【新しいゲームシステム】
・コマンド説明
";

    // コマンド処理
    protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
    {
        var match = CommandRegex.Match(command);
        if (!match.Success)
        {
            return null;  // マッチしない場合は null を返す
        }

        // シークレットダイス判定
        bool isSecret = command.StartsWith("S", StringComparison.OrdinalIgnoreCase);

        // ダイスロール
        int roll = randomizer.RollOnce(6);

        // 結果構築
        return Result.CreateBuilder($"結果: {roll}")
            .SetSecret(isSecret)
            .SetRands(randomizer.RandResults)
            .SetDetailedRands(randomizer.DetailedRandResults)
            .SetCondition(roll >= 4)  // 成功/失敗
            .Build();
    }
}
```

## テストパターン

各ゲームシステムのテストは以下を含める：

```csharp
public class NewGameSystemTests
{
    [Fact]
    public void Id_ReturnsCorrectId() { }

    [Fact]
    public void Name_ReturnsJapaneseName() { }

    [Fact]
    public void Eval_BasicCommand_Works() { }

    [Fact]
    public void Eval_Critical_OnSpecificRoll() { }

    [Fact]
    public void Eval_Fumble_OnSpecificRoll() { }

    [Fact]
    public void Eval_SecretCommand_IsSecret() { }

    [Fact]
    public void Eval_CommonCommand_Works() { }
}
```

`MockRandomizer` を使用してダイス結果を制御：

```csharp
var randomizer = new MockRandomizer(3, 4);  // 順番に 3, 4 を返す
var result = GameSystem.Instance.Eval("2D6", randomizer);
```

## 登録忘れ注意

新しいゲームシステムを作成したら、`BCDice.cs` の静的コンストラクタに登録を追加：

```csharp
static BCDiceRunner()
{
    // ... 既存の登録 ...
    _registry.Register(NewGameSystem.Instance);
}
```

## Result オブジェクトのフラグ

- `SetCondition(bool)` - 成功/失敗を設定（相互排他）
- `SetSuccess(true)` - 成功フラグのみ設定
- `SetFailure(true)` - 失敗フラグのみ設定
- `SetCritical(true)` - クリティカル（通常は SetSuccess も併用）
- `SetFumble(true)` - ファンブル（通常は SetFailure も併用）
- `SetSecret(bool)` - シークレットダイス

## 注意事項

- .NET Standard 2.0 互換を維持すること
- C# 12 の機能は使用可能（LangVersion 12）
- Nullable 参照型は有効（`#nullable enable`）
- テストは xUnit を使用
