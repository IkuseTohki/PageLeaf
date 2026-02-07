# Markdown CSS エディター マスター仕様書

## 1. プロジェクト概要

本アプリケーションは、Markdown ドキュメントのデザイン（CSS）作成・編集に特化したデスクトップツールである。
ユーザーは、プレビュー画面でリアルタイムにスタイルの変化を確認しながら、GUI を通じて直感的に Markdown 用の CSS を作成・編集できる。
Markdown エディターとしての機能も兼ね備えており、スタイル確認用のテキスト編集も同一画面内でシームレスに行える。

## 2. 仕様ドキュメント体系

本プロジェクトの仕様は、役割ごとに以下のドキュメントに分割して定義されている。

### 2.1. 機能仕様 (Functional)

アプリが「何ができるか」を定義する。

- **[エディタ機能](functional/editor.md)**: AvalonEdit を用いた編集・入力支援機能。
- **[ファイル操作・管理](functional/file_management.md)**: 新規作成、保存、外部変更検知など。
- **[CSS 編集・管理](functional/css_editor.md)**: GUI によるスタイル編集、単位変換、ファイル管理。
- **[YAML フロントマター](functional/frontmatter.md)**: メタデータ処理、予約プロパティ、プレビュー制御。
- **[システム設定](functional/system_settings.md)**: ログ出力設定、アプリケーションの動作環境設定。

### 2.2. UI/UX 仕様 (User Interface)

見た目の一貫性とユーザー体験を定義する。

- **[画面構成・レイアウト](ui/window_layout.md)**: メインウィンドウ、設定画面、チートシートの構造。
- **[インタラクション・振る舞い](ui/interaction.md)**: モーダル挙動、Dirty インジケータ、TOC ジャンプ。
- **[デザインシステム](ui/design_system.md)**: カラーシステム、Soft Emphasis 規約。
- **[デザインシステム指針](ui/%23デザインシステムとは.md)**: デザインシステムの思想と運用ルール。

### 2.3. 技術仕様 (Technical)

開発者が「どう実装するか」を定義する。

- **[アーキテクチャ](technical/architecture.md)**: レイヤー分離、MVVM の純粋性、DI。
- **[実装規約・標準](technical/standards.md)**: リソース管理、品質担保、コーディング規約。

## 3. 採用技術

- **開発言語**: C#
- **プラットフォーム**: .NET 8.0 (Windows)
- **フレームワーク**: WPF (Windows Presentation Foundation)
- **エディタコンポーネント**: AvalonEdit
- **デザインパターン**: MVVM、Use Case 層、 DI
- **DI コンテナ**: Microsoft.Extensions.DependencyInjection
