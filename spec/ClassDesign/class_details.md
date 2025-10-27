# クラス詳細

## MainWindow クラス

### 概要

アプリケーションのメインウィンドウを表す View クラスです。ユーザーインターフェースの表示とユーザーからの操作イベントのハンドリングを担当します。MVVM パターンにおける View として機能し、直接的なビジネスロジックは持ちません。ViewModel からのデータバインディングを通じて表示内容を更新し、ViewModel へのコマンドバインディングを通じてユーザー操作を伝達します。

### 主要な責務

- アプリケーションのメイン画面レイアウトの定義と表示。
- メニューバー、ツールバー、フォルダツリー、メインエディタ/ビューアエリアなどの UI 要素の配置。
- ユーザー入力（ボタンクリック、メニュー選択など）のイベントを ViewModel のコマンドにバインドして伝達。
- ViewModel のプロパティ変更通知に応じて UI を更新。

## SettingsWindow クラス

### 概要

アプリケーションの設定画面を表す View クラスです。フォルダツリーの表示位置など、各種設定項目の表示とユーザーによる設定変更の受付を担当します。MVVM パターンにおける View として機能し、直接的なビジネスロジックは持ちません。ViewModel からのデータバインディングを通じて表示内容を更新し、ViewModel へのコマンドバインディングを通じてユーザー操作を伝達します。

### 主要な責務

- 設定項目の UI 要素（ラジオボタン、チェックボックスなど）の配置と表示。
- ユーザーによる設定変更（ラジオボタン選択など）のイベントを ViewModel のコマンドにバインドして伝達。
- ViewModel のプロパティ変更通知に応じて UI を更新。
- 設定の保存、キャンセル操作の受付。

## MainWindowViewModel クラス

### 概要

MainWindow のプレゼンテーションロジックと状態を管理する ViewModel クラスです。View（MainWindow）と Model（MarkdownDocument, ApplicationSettings, 各種 Service）の間の仲介役として機能します。View に表示するデータを公開し、View からのユーザー操作を受け取って Model 層のサービスを呼び出します。

### 主要なプロパティ

- `MarkdownContent`: string 型。メインエディタ/ビューアエリアに表示される Markdown コンテンツ。Model の`MarkdownDocument`と同期します。
- `CurrentMode`: `DisplayMode`型（enum）。現在の表示モード（ビューアー、Markdown 編集、リアルタイム編集）。
- `SelectedCss`: string 型。現在選択されている CSS ファイルのパス。View の CSS 選択プルダウンとバインドします。
- `FolderTreePosition`: `FolderTreePosition`型（enum）。フォルダツリーの表示位置（左/右）。View のレイアウトに影響します。
- `FileTree`: `IEnumerable<FileTreeNode>` 型。フォルダツリーのトップレベルノードのコレクション。View の TreeView とバインドします。

### 主要なコマンド

- `OpenFileCommand`: ファイルを開く操作を処理します。`IFileService`を利用します。
- `OpenFolderCommand`: フォルダを開く操作を処理します。`IFileService`を利用して `FileTree` プロパティを更新し、フォルダツリーの構築をトリガーします。
- `SaveFileCommand`: ファイルを保存する操作を処理します。`IFileService`を利用します。
- `ExportCommand`: Markdown コンテンツを各種形式でエクスポートする操作を処理します。`IExportService`を利用します。
- `OpenSettingsCommand`: 設定画面を開く操作を処理します。

### 主要な責務

- View の状態管理とデータ提供。
- ユーザー操作に応じた Model 層のサービス呼び出し。
- アプリケーション設定（`ApplicationSettings`）の読み込みと適用。
- フォルダツリーの構築と管理（`IFileService`から取得した `FileTreeNode` データを元に）。

## SettingsViewModel クラス

### 概要

SettingsWindow のプレゼンテーションロジックと状態を管理する ViewModel クラスです。View（SettingsWindow）と Model（ApplicationSettings, ISettingsService）の間の仲介役として機能します。View に表示する設定データを公開し、View からのユーザー操作を受け取って設定の保存やキャンセルを処理します。

### 主要なプロパティ

- `FolderTreePosition`: `FolderTreePosition`型（enum）。フォルダツリーの表示位置。View のラジオボタンとバインドします。

### 主要なコマンド

- `SaveSettingsCommand`: 現在の設定を保存し、設定画面を閉じる操作を処理します。`ISettingsService`を利用します。
- `CancelSettingsCommand`: 設定変更を破棄し、設定画面を閉じる操作を処理します。

### 主要な責務

- 設定画面の状態管理とデータ提供。
- ユーザーによる設定変更の受付と一時的な保持。
- 設定の保存（`ISettingsService`を利用）および破棄。

## FileTreeNode クラス

### 概要

フォルダツリーの各ノード（ファイルまたはフォルダ）を表すデータモデルクラスです。階層的な親子関係を持つことができます。ビジネスロジックは含まず、純粋なデータコンテナとして機能します。

### 主要なプロパティ

- `Name`: string 型。ファイルまたはフォルダの名前。
- `FilePath`: string 型。ファイルまたはフォルダのフルパス。
- `IsDirectory`: bool 型。フォルダであるかどうかを示します。
- `Children`: `IEnumerable<FileTreeNode>` 型。子ノードのコレクションです（フォルダの場合）。

### 主要な責務

- ファイルシステムの階層構造の保持。

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

- `FolderTreePosition`: `FolderTreePosition`型（enum）。フォルダツリーの表示位置（左/右）。
- `SelectedCss`: string 型。現在選択されている CSS ファイルのパス。
- `LastOpenedFolder`: string 型。最後に開いたフォルダのパス。アプリケーション起動時に前回の状態を復元するために使用されます。

### 主要な責務

- アプリケーション設定値の保持。

## IFileService インターフェース

### 概要

ファイルシステム操作（ファイルの読み書き、フォルダの探索）に関する契約を定義するインターフェースです。Model 層とインフラストラクチャ層の境界を明確にし、依存性逆転の原則を適用します。これにより、ファイル操作の実装詳細が ViewModel や Model から隠蔽されます。

### 主要なメソッド

- `Open(filePath: string): MarkdownDocument`: 指定されたパスの Markdown ファイルを読み込み、`MarkdownDocument`オブジェクトとして返します。
- `Save(document: MarkdownDocument): void`: `MarkdownDocument`オブジェクトの内容を指定されたパスに保存します。
- `OpenFolder(folderPath: string): IEnumerable<FileTreeNode>`: 指定されたフォルダの階層構造を取得し、`FileTreeNode`のコレクションとして返します。フォルダツリー構築のために使用されます。

### 主要な責務

- ファイル操作の抽象化。
- ファイルシステムへの依存を分離。

## FileService クラス

### 概要

`IFileService`インターフェースの実装クラスです。実際のファイルシステムへのアクセスを行い、ファイルの読み書きやフォルダの内容取得などの具体的な処理を提供します。

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
