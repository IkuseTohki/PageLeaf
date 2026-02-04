# FlaUI テストケース作成ガイド & プラクティス

FlaUI を使用して、PageLeaf の機能を検証するためのテストケースを作成する手順とベストプラクティスをまとめます。

導入時に発生しやすいエラーの解決策については、[FlaUI トラブルシューティングガイド](./flaui_testing_troubleshooting.md) を参照してください。

## 1. テストケースの基本構造

### 並列実行の禁止

デスクトップアプリの UI テストは、マウスやキーボードといったシステムリソースを占有します。複数のテストを同時に実行すると、フォーカスの奪い合いや入力の混線が発生するため、**並列実行は厳禁**です。

テストプロジェクトの `MSTestSettings.cs` 等で以下のように設定してください。

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
[assembly: DoNotParallelize]
```

### AAA パターンの基本構造

UI テストは一般的に以下の `AAA (Arrange-Act-Assert)` パターンで記述します。

```csharp
[TestMethod]
public void TestName()
{
    // Arrange: セッションの開始と準備
    using var session = new PageLeafSession();
    var window = session.MainWindow!;

    // Act: 操作
    // ...

    // Assert: 検証
    // ...
}
```

## 2. 要素の特定 (Best Practices)

### AutomationId を使用する

`Name` (ボタンのテキスト) やインデックスによる特定は、多言語対応やデザイン変更に弱いため避けてください。
WPF 側で `AutomationProperties.AutomationId` を付与し、テストコードから `ByAutomationId` で検索します。

**WPF (XAML):**

```xml
<Button AutomationProperties.AutomationId="SaveButton" ... />
```

**Test (C#):**

```csharp
var button = window.FindFirstDescendant(cf => cf.ByAutomationId("SaveButton"))?.AsButton();
```

### Invoke パターンの活用

物理的なマウス操作 (`Click()`) は、要素が隠れていたり Popup 内にあったりする場合に `NoClickablePointException` を投げることがあります。UI 要素が標準的なボタンやメニューであれば、`Patterns.Invoke` を使用することで、マウスカーソルを動かさずに直接コマンドを実行でき、テストが安定します。

```csharp
if (button.Patterns.Invoke.IsSupported)
{
    button.Patterns.Invoke.Pattern.Invoke();
}
```

## 3. ファイル I/O を伴うテストのプラクティス

ファイルの作成、読み込み、保存をテストする場合、テストの実行順序や回数に依存しない（冪等性を保つ）ための工夫が必要です。

### 冪等性を確保するための原則

- **ユニークな名前の使用**: 新規作成テストでは `Guid.NewGuid()` を含めたファイル名を使用し、既存ファイルとの衝突を避けます。
- **バックアップとリストア**: 既存の共通設定ファイル（`github.css` など）を編集するテストでは、テスト開始時にファイルをバックアップし、終了時に必ず元の状態に書き戻します。
- **クリーンアップの徹底**: 作成した一時ファイルは、テストの成否に関わらず削除します。

### 手順 A: 書き込み・保存のテスト（既存ファイル）

1. **バックアップ**: ターゲットファイル（`github.css`）を一時的にコピー（`github.css.bak`）します。
2. **パスの特定**: アプリが現在使用しているベースディレクトリを取得します。
3. **事前状態の記録**: ターゲットファイルの `File.GetLastWriteTime()` を記録します。
4. **UI 操作**:
   - 値を変更し、`Enter` や `Tab` で確定させます。
5. **保存実行**: 保存ボタンをクリックします。
6. **完了待機**: `Thread.Sleep(2000)` などで余裕を持って待ちます。
7. **事後検証**:
   - タイムスタンプの更新と、ファイル内容の検証を行います。
8. **リストア (Cleanup)**: テスト終了時に `github.css.bak` を `github.css` に戻し、バックアップを削除します。

### 手順 B: 新規作成のテスト（アプリ生成ファイルの検証）

UI 操作による新規作成ダイアログの呼び出しが不安定な場合（ComboBox の UIA タイムアウトなど）、以下のバイパス策が有効です。

1. **物理作成**: テスト開始前に、アプリのテンプレートを模した `.css` ファイルを `Guid` を含めた名前で `css` フォルダに直接作成します。
2. **ブラインド選択**: ComboBox を展開せずに `Focus()` し、`HOME` キー送信後にファイル名を `Keyboard.Type()` で入力して `ENTER` を押します。これにより UIA のハングを回避しつつ、目的のファイルを選択状態にできます。
3. **編集・保存検証**: 選択されたファイルに対して編集操作を行い、`File.ReadAllText()` で保存内容を検証します。
4. **削除 (Cleanup)**: テスト終了時にそのファイルを削除します。

## 4. 特殊な要素の扱い

### ComboBox の操作（重要）

WPF の ComboBox を UIA で操作する場合、`Select()` メソッドは非常に不安定（ハングしやすい）です。以下の手法を優先してください。

- **手法 1 (キー入力)**: `Focus()` -> `Keyboard.Type("itemName")` -> `ENTER`。Popup を開かないため最も安全です。
- **手法 2 (インデックス指定)**: `Expand()` -> `Keyboard.Press(DOWN)` を必要な回数繰り返す。

### Popup

WPF の `Popup` はメインウィンドウの視覚ツリーの外にあるため、`window.FindFirstDescendant` では見つからないことがあります。この場合、`automation.GetDesktop()` から探索してください。

### 待機処理の原則

UI のアニメーションや非同期処理に加え、**ディスク I/O の反映**には数秒の待機（`Thread.Sleep(2000)` 以上）を設けることで、テストのフレイキー（不安定さ）を抑えられます。
