# BCDice for C#

BCDice の C# 移植版です。.NET Standard 2.0 をターゲットとし、Unity LTS との互換性を確保しています。

## 概要

BCDice は TRPG (テーブルトークロールプレイングゲーム) 用のダイスコマンドエンジンです。このライブラリは Ruby 版 BCDice 3.x の機能を C# で再実装したものです。

## 要件

- .NET Standard 2.0 互換環境
- Unity 2021.3 LTS 以降（Unity で使用する場合）

## インストール

### NuGet（予定）

```
dotnet add package BCDice
```

### Unity

`src/BCDice` フォルダを Unity プロジェクトの `Assets` 以下にコピーしてください。

## 基本的な使い方

```csharp
using BCDice;
using BCDice.Core;

// 基本的なダイスロール
var result = BCDiceRunner.Eval("2D6");
Console.WriteLine(result.Text);  // 例: "(2D6) ＞ 7[3,4] ＞ 7"

// ゲームシステムを指定して実行
BCDiceRunner.SetGameSystem("Cthulhu");
var ccbResult = BCDiceRunner.Eval("CCB<=50");
Console.WriteLine(ccbResult.Text);

// 直接ゲームシステムを指定
var swResult = BCDiceRunner.Eval("SwordWorld2_5", "K20@10");
Console.WriteLine(swResult.Text);
```

## 対応ゲームシステム

| ID | 名前 | 主なコマンド |
|----|------|-------------|
| DiceBot | ダイスボット（汎用） | nDm, nDm>=t |
| Cthulhu | クトゥルフ神話TRPG | CCB, RES, CBR |
| SwordWorld2_5 | ソードワールド2.5 | K (レーティング表) |
| DoubleCross3 | ダブルクロス3rd | nDX (爆発ダイス) |
| Insane | インセイン | 2D6判定, ST (特技表) |
| Shinobigami | シノビガミ | 2D6判定, ET (感情表) |
| Nechronica | ネクロニカ | nNC (成功数カウント) |
| Arianrhod2E | アリアンロッド2E | 2D6判定 |
| Emoklore | エモクロアTRPG | 2D6判定, EMT (感情表) |
| Satasupe | サタスペ | nR>=t (振り足し), CRIME |
| MagicaLogia | マギカロギア | 2D6判定, MGT (魔法名表) |
| Kamigakari | 神我狩 | KG (霊力ダイス) |
| MeikyuuKingdom | 迷宮キングダム | 2D6判定, FT (施設表) |

## 共通コマンド

すべてのゲームシステムで使用可能なコマンド：

| コマンド | 説明 | 例 |
|---------|------|-----|
| `nDm` | m面ダイスをn個振る | `2D6`, `1D100` |
| `nDm>=t` | 目標値判定 | `2D6>=7` |
| `nDm+x` | 修正値付きロール | `2D6+3` |
| `nBm` | 上方無限ロール | `2B6` |
| `nRm` | 下方無限ロール | `2R6` |
| `nSm>=t` | 成功数カウント | `5S6>=4` |
| `D66` | D66ロール | `D66` |
| `CHOICE[a,b,c]` | ランダム選択 | `CHOICE[攻撃,防御,回避]` |
| `xRn` | リピートロール | `x3 2D6` |
| `C(式)` | 計算 | `C(10+5*2)` |

## アーキテクチャ

```
BCDice/
├── Core/           # コアインターフェース・型定義
├── Arithmetic/     # 式パーサー・評価器
├── Preprocessor/   # コマンド前処理
├── CommonCommand/  # 共通ダイスコマンド
├── Command/        # コマンド基盤
├── Table/          # テーブル機能
└── GameSystem/     # ゲームシステム実装
```

## 開発

### ビルド

```bash
dotnet build
```

### テスト

```bash
dotnet test
```

### ゲームシステムの追加

1. `GameSystem` フォルダに新しいクラスを作成
2. `GameSystemBase` を継承
3. `EvalGameSystemSpecificCommand` をオーバーライド
4. `BCDiceRunner` の静的コンストラクタで登録

```csharp
public sealed class MyGameSystem : GameSystemBase
{
    public static readonly MyGameSystem Instance = new MyGameSystem();

    public override string Id => "MyGameSystem";
    public override string Name => "マイゲームシステム";
    public override string SortKey => "まいけえむしすてむ";

    protected override Result? EvalGameSystemSpecificCommand(string command, IRandomizer randomizer)
    {
        // ゲームシステム固有のコマンド処理
        return base.EvalGameSystemSpecificCommand(command, randomizer);
    }
}
```

## ライセンス

BCDice は BSD 3-Clause License のもとで公開されています。

## クレジット

- オリジナル BCDice: https://github.com/bcdice/BCDice
- BCDice Contributors
