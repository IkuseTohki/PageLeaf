# FlaUI 導入手順書

本プロジェクトでは、WPF アプリケーションの UI 自動テストに **FlaUI (UIA3)** を採用しています。

## 1. プロジェクト構成

UI テスト用のプロジェクトとして `src/PageLeaf.UITests` を作成し、以下のパッケージを導入しています。

- **FlaUI.Core**: 共通コアライブラリ
- **FlaUI.UIA3**: Microsoft UI Automation 3 用のライブラリ
- **MSTest**: テストフレームワーク

## 2. セットアップ手順

### パッケージの追加

NuGet または dotnet CLI を使用して以下のパッケージを追加します。

```bash
dotnet add src/PageLeaf.UITests/PageLeaf.UITests.csproj package FlaUI.Core
dotnet add src/PageLeaf.UITests/PageLeaf.UITests.csproj package FlaUI.UIA3
```

### ネイティブ依存関係の処理 (WebView2)

WPF アプリケーションで WebView2 を使用している場合、`WebView2Loader.dll` が実行ディレクトリに必要です。テストプロジェクトからアプリを起動する際、DLL が不足して `DllNotFoundException` が発生することがあります。

本プロジェクトでは `PageLeafSession` クラス内で、起動前に DLL の存在を確認し、不足していれば `runtimes\win-x64\native` からコピーする処理を実装しています。

## 3. アプリケーションセッションの管理

テストごとにアプリの起動・終了を制御するため、`PageLeafSession` クラス（`IDisposable`）を使用してライフサイクルを管理します。

- **起動**: `Application.Launch` を使用。
- **ディレクトリ**: `ProcessStartInfo.WorkingDirectory` を明示的に指定することで、設定ファイルや CSS フォルダのパス誤認を防ぎます。
- **終了**: `Dispose` 内で `App.Close()` および必要に応じた `process.Kill()` を行います。

## 4. 実行方法

以下のコマンドで UI テストを実行できます。

```bash
dotnet test src/PageLeaf.UITests/PageLeaf.UITests.csproj
```
