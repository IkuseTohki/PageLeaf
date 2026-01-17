# PageLeaf

![PageLeaf Icon](spec/assets/icon.svg)

PageLeaf は、Markdown ドキュメントのデザイン（CSS）作成・編集に特化したデスクトップツールです。
プレビュー画面でリアルタイムにスタイルの変化を確認しながら、GUI を通じて直感的に Markdown 用の CSS を作成・カスタマイズできます。

## 🍃 主な機能

- **Markdown 編集 & プレビュー**: リアルタイムでレンダリング結果を確認しながら Markdown を編集可能。
- **モード切替 & 目次 (TOC)**: 編集モードと閲覧モードを瞬時に切り替え（`Alt+Shift+←/→`）。目次から見出しへ素早くジャンプ。
- **GUI CSS エディター**: プログラミングの知識がなくても、フォント、色、余白などのスタイルを直感的にカスタマイズ。
- **CSS 管理**: 複数の CSS プロファイルを切り替えたり、保存・読み込みが可能。
- **フロントマター制御**: Markdown の先頭に YAML 形式で設定を記述可能。新規作成時にタイトルや作成日時、カスタムプロパティを自動挿入する設定も完備。CSS やハイライトテーマをドキュメントごとに上書き。
- **リソースの読み込み先切り替え**: シンタックスハイライトや図解ライブラリを、オフライン向けの「ローカル」または常に最新機能が使える「CDN」から選択可能。
- **画像貼り付け支援**: クリップボードからの画像を自動保存し、Markdown リンクを即座に挿入（`Ctrl+Shift+V`）。保存先やファイル名の命名規則も設定可能。
- **編集支援**: オートインデント、リストの自動継続、ペア記号の自動補完、改ページの挿入（`Shift+Enter`）など。

## ⌨️ ショートカットキー

| ショートカット      | 機能                              |
| ------------------- | --------------------------------- |
| `Alt+Shift + ← / →` | 編集モード / 閲覧モードの切り替え |
| `Ctrl + S`          | 保存                              |
| `Ctrl + Shift + S`  | 名前を付けて保存                  |
| `Ctrl + Shift + V`  | 画像を貼り付け                    |
| `Shift + Enter`     | 改ページ挿入                      |
| `Ctrl + B`          | 太字                              |
| `Ctrl + I`          | 斜体                              |
| `Ctrl + K`          | リンク挿入                        |

## 🛠 技術スタック

- **言語**: C# / .NET 8.0 (Windows)
- **UI フレームワーク**: WPF (Windows Presentation Foundation)
- **アーキテクチャ**: MVVM + クリーンアーキテクチャ
- **主なライブラリ**:
  - [Markdig](https://github.com/lunet-io/markdig): Markdown 解析
  - [WebView2](https://developer.microsoft.com/en-us/microsoft-edge/webview2/): 高性能な HTML/CSS プレビュー
  - [Highlight.js](https://highlightjs.org/) / [Mermaid](https://mermaid.js.org/): コードハイライトと図解レンダリング
  - [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/): 依存関係の注入 (DI)
  - [Serilog](https://serilog.net/): 構造化ロギング

## 🚀 開発の始め方

### 動作環境

- Windows 10 / 11
- .NET 8.0 Runtime 以上
- Microsoft Edge WebView2 Runtime

### ビルド手順

1. リポジトリをクローンします。
2. ソリューションファイルを開くか、コマンドラインで以下を実行します。

```powershell
# 復元とビルド
dotnet build PageLeaf.sln

# テストの実行
dotnet test PageLeaf.sln

# アプリケーションの実行
dotnet run --project src/PageLeaf/PageLeaf.csproj
```

## 📂 ディレクトリ構造

- `src/PageLeaf/`: メインアプリケーション
- `src/PageLeaf.Tests/`: ユニットテストプロジェクト
- `spec/`: 仕様書、アイコンデザイン等のアセット
- `task/`: 開発タスク管理ドキュメント

## ⚖️ ライセンス

Copyright © 2026 PageLeaf Project.
このプロジェクトは [MIT ライセンス](LICENSE) の下で公開されています。
