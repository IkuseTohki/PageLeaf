# クラス詳細

## MainWindow クラス

### 概要

アプリケーションのメインウィンドウを表す View クラスです。ユーザーインターフェースの表示とユーザーからの操作イベントのハンドリングを担当します。MVVM パターンにおける View として機能し、直接的なビジネスロジックは持ちません。ViewModel からのデータバインディングを通じて表示内容を更新し、ViewModel へのコマンドバインディングを通じてユーザー操作を伝達します。

### 主要な責務

- アプリケーションのメイン画面レイアウトの定義と表示。
- メニューバー、ツールバー、メインエディタ/ビューアエリア、CSS 編集パネルなどの UI 要素の配置。
- ユーザー入力（ボタンクリック、メニュー選択など）のイベントを ViewModel のコマンドにバインドして伝達。
- ViewModel のプロパティ変更通知に応じて UI を更新。

## SettingsWindow クラス

### 概要

アプリケーションの設定画面を表す View クラスです。各種設定項目の表示とユーザーによる設定変更の受付を担当します。MVVM パターンにおける View として機能し、直接的なビジネスロジックは持ちません。ViewModel からのデータバインディングを通じて表示内容を更新し、ViewModel へのコマンドバインディングを通じてユーザー操作を伝達します。

### 主要な責務

- 設定項目の UI 要素（チェックボックスなど）の配置と表示。
- ユーザーによる設定変更のイベントを ViewModel のコマンドにバインドして伝達。
- ViewModel のプロパティ変更通知に応じて UI を更新。
- 設定の保存、キャンセル操作の受付。

## MainWindowViewModel クラス

### 概要

MainWindow のプレゼンテーションロジックと状態を管理する ViewModel クラスです。View（MainWindow）と Model（MarkdownDocument, ApplicationSettings, 各種 Service）の間の仲介役として機能します。View に表示するデータを公開し、View からのユーザー操作を受け取って Model 層のサービスを呼び出します。

### 主要なプロパティ

- `MarkdownContent`: string 型。メインエディタ/ビューアエリアに表示される Markdown コンテンツ。Model の`MarkdownDocument`と同期します。
- `CurrentMode`: `DisplayMode`型（enum）。現在の表示モード（ビューアー、Markdown 編集、リアルタイム編集）。
- `SelectedCss`: string 型。現在選択されている CSS ファイルのパス。View の CSS 選択プルダウンとバインドします。
- `IsCssEditorVisible`: bool 型。CSS 編集パネルの表示/非表示を制御します。

### 主要なコマンド

- `OpenFileCommand`: ファイルを開く操作を処理します。`IFileService`を利用します。
- `SaveFileCommand`: ファイルを保存する操作を処理します。`IFileService`を利用します。
- `ExportCommand`: Markdown コンテンツを各種形式でエクスポートする操作を処理します。`IExportService`を利用します。
- `OpenSettingsCommand`: 設定画面を開く操作を処理します。
- `ToggleCssEditorCommand`: CSS 編集パネルの表示/非表示を切り替える操作を処理します。

### 主要な責務

- View の状態管理とデータ提供。
- ユーザー操作に応じた Model 層のサービス呼び出し。
- アプリケーション設定（`ApplicationSettings`）の読み込みと適用。
- CSS の管理と編集に関するロジックの実行（`ICssService`を利用）。

## SettingsViewModel クラス

### 概要

SettingsWindow のプレゼンテーションロジックと状態を管理する ViewModel クラスです。View（SettingsWindow）と Model（ApplicationSettings, ISettingsService）の間の仲介役として機能します。View に表示する設定データを公開し、View からのユーザー操作を受け取って設定の保存やキャンセルを処理します。

### 主要なコマンド

- `SaveSettingsCommand`: 現在の設定を保存し、設定画面を閉じる操作を処理します。`ISettingsService`を利用します。
- `CancelSettingsCommand`: 設定変更を破棄し、設定画面を閉じる操作を処理します。

### 主要な責務

- 設定画面の状態管理とデータ提供。
- ユーザーによる設定変更の受付と一時的な保持。
- 設定の保存（`ISettingsService`を利用）および破棄。

## MarkdownDocument クラス

### 概要

Markdown ファイルのデータモデルを表すクラスです。ファイルの内容やパスなどの情報を保持します。ビジネスロジックは含まず、純粋なデータコンテナとして機能します。

### 主要なプロパティ

- `Content`: string 型。Markdown ファイルのテキスト内容。
- `FilePath`: string 型。Markdown ファイルの絶対パス。

### 主要な責務

- Markdown コンテンツとそのメタデータの保持。

## ApplicationSettings クラス

### 概要

アプリケーション全体のユーザー設定を保持するデータモデルクラスです。設定の永続化や読み込み時に使用されます。ビジネスロジックは含まず、純粋なデータコンテナとして機能します。

### 主要なプロパティ

- `SelectedCss`: string 型。現在選択されている CSS ファイルのパス。

### 主要な責務

- アプリケーション設定値の保持。

## IFileService インターフェース

### 概要

ファイルシステム操作（ファイルの読み書き）に関する契約を定義するインターフェースです。Model 層とインフラストラクチャ層の境界を明確にし、依存性逆転の原則を適用します。これにより、ファイル操作の実装詳細が ViewModel や Model から隠蔽されます。

### 主要なメソッド

- `Open(filePath: string): MarkdownDocument`: 指定されたパスの Markdown ファイルを読み込み、`MarkdownDocument`オブジェクトとして返します。
- `Save(document: MarkdownDocument): void`: `MarkdownDocument`オブジェクトの内容を指定されたパスに保存します。

### 主要な責務

- ファイル操作の抽象化。
- ファイルシステムへの依存を分離。

## FileService クラス

### 概要

`IFileService`インターフェースの実装クラスです。実際のファイルシステムへのアクセスを行い、ファイルの読み書きなどの具体的な処理を提供します。

### 主要な責務

- `IFileService`で定義されたメソッドの具体的な実装。
- ファイル I/O 処理の実行。
- エラーハンドリング（ファイルの読み込み失敗など）。

## IExportService インターフェース

### 概要

Markdown コンテンツを各種形式（HTML, PDF, Word, 画像）にエクスポートする機能に関する契約を定義するインターフェースです。Model 層とインフラストラクチャ層の境界を明確にし、依存性逆転の原則を適用します。これにより、エクスポートの実装詳細が ViewModel や Model から隠蔽されます。

### 主要なメソッド

- `Export(document: MarkdownDocument, format: ExportFormat): void`: `MarkdownDocument`の内容を指定された形式でエクスポートします。

### 主要な責務

- エクスポート機能の抽象化。
- エクスポート処理への依存を分離。

## ExportService クラス

### 概要

`IExportService`インターフェースの実装クラスです。Markdown コンテンツを HTML、PDF、Word、画像などの指定された形式に変換し、ファイルとして出力する具体的な処理を提供します。

### 主要な責務

- `IExportService`で定義されたメソッドの具体的な実装。
- Markdown から各種形式への変換処理の実行。
- エクスポート時のファイル I/O 処理。

## ISettingsService インターフェース

### 概要

アプリケーション設定の読み込みと保存に関する契約を定義するインターフェースです。Model 層とインフラストラクチャ層の境界を明確にし、依存性逆転の原則を適用します。これにより、設定の永続化の実装詳細が ViewModel や Model から隠蔽されます。

### 主要なメソッド

- `LoadSettings(): ApplicationSettings`: 保存されているアプリケーション設定を読み込み、`ApplicationSettings`オブジェクトとして返します。
- `SaveSettings(settings: ApplicationSettings): void`: `ApplicationSettings`オブジェクトの内容を永続化します。

### 主要な責務

- アプリケーション設定の抽象化。
- 設定の永続化メカニズムへの依存を分離。

## SettingsService クラス

### 概要

`ISettingsService`インターフェースの実装クラスです。アプリケーション設定をファイル（例: XML, JSON）やレジストリなどに読み書きする具体的な処理を提供します。

### 主要な責務

- `ISettingsService`で定義されたメソッドの具体的な実装。
- 設定の読み書き処理の実行。
- 設定の永続化メカニズムの管理。

## ICssService インターフェース

### 概要

CSS ファイルの管理（取得、保存、作成、削除）に関する契約を定義するインターフェースです。ViewModel が具体的なファイル操作を意識することなく、CSS の管理機能を利用できるようにします。

### 主要なメソッド

- `GetCssFiles(): IEnumerable<string>`: 利用可能な CSS ファイルの一覧を取得します。
- `GetCssContent(fileName: string): string`: 指定された CSS ファイルの内容を取得します。
- `SaveCssContent(fileName: string, content: string): void`: 指定された CSS ファイルに内容を保存します。
- `CreateCssFile(fileName: string): void`: 新しい CSS ファイルを作成します。
- `DeleteCssFile(fileName: string): void`: 指定された CSS ファイルを削除します。

### 主要な責務

- CSS ファイル操作の抽象化。
- CSS ファイルへの依存を分離。

## CssService クラス

### 概要

`ICssService`インターフェースの実装クラスです。CSS ファイルの具体的な読み書き、作成、削除処理を提供します。

### 主要な責務

- `ICssService`で定義されたメソッドの具体的な実装。
- CSS ファイルの I/O 処理の実行。
- エラーハンドリング。
