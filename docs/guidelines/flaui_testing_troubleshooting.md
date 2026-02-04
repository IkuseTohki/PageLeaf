# FlaUI トラブルシューティングガイド

FlaUI の導入およびテスト作成時に発生した主要なエラーと、その解決策をまとめています。

## 1. アプリケーション起動・ウィンドウ特定

### エラー: `Main window could not be found.`

- **現象**: アプリは起動しているが、FlaUI がメインウィンドウを取得できずタイムアウトする。
- **原因**:
  - .NET アプリにおいて、起動用の `.exe` (shim) と実際のロジックを持つ `.dll` のプロセス境界が影響している。
  - アプリの初期化（DIコンテナの構築やリソース展開）に時間がかかっている。
- **対策**:
  - `Application.Launch` 後に `App.GetMainWindow` だけでなく、`App.GetAllTopLevelWindows` を使ってウィンドウタイトルで再検索するフォールバック処理を実装する。
  - 探索時にリトライループ（10秒〜20秒程度）を設ける。

## 2. 依存ライブラリ (WebView2)

### エラー: `System.DllNotFoundException: Unable to load DLL 'WebView2Loader.dll'`

- **現象**: テスト実行中にアプリが起動するが、WebView2 の初期化でクラッシュする。
- **原因**: WebView2 の動作に必要な `WebView2Loader.dll` が、テスト実行ディレクトリ（`bin\Debug\net8.0-windows`）の直下に配置されていない。
- **対策**:
  - `PageLeafSession` クラス（テストのセットアップ層）で、アプリ起動前に `runtimes\win-x64\native` から実行ファイルと同階層へ DLL を自動コピーする処理を追加する。

## 3. ディレクトリ・パスの不一致

### エラー: 保存テストでファイルが更新されない (`Assert.AreNotEqual` の失敗)

- **現象**: テスト上で「保存」ボタンをクリックしたが、監視しているファイルのタイムスタンプが変わらない。
- **原因**:
  - **ワーキングディレクトリ**: アプリが起動ディレクトリではなく、テスト実行ディレクトリを「ベースパス」と誤認して、別の場所の CSS を編集していた。
  - **バインディング**: UIA 経由で値をセットしただけでは WPF のデータバインディングが走らず、保存ボタンが有効にならなかった（または変更が空だった）。
- **対策**:
  - `ProcessStartInfo.WorkingDirectory` を実行ファイルのディレクトリに明示的に設定する。
  - テストコード内で監視するパスを `AppDomain.CurrentDomain.BaseDirectory` ではなく、実際に起動したアプリのログ等から特定したディレクトリに合わせる。
  - 値の入力後、`Enter` や `Tab` キーを送信してバインディングを強制的に実行させる。

## 4. UI 操作の不安定さ

### エラー: `FlaUI.Core.Exceptions.NoClickablePointException`

- **現象**: ボタンをクリックしようとすると例外が発生する。
- **原因**: 対象のボタンが `Popup` 内にある、またはアニメーション中で座標が確定していない。
- **対策**:
  - `Click()` (物理マウス操作) の代わりに `Patterns.Invoke.Pattern.Invoke()` を使用する。これは座標を必要とせず、コントロールの機能を直接実行するため非常に安定する。

### エラー: `System.TimeoutException: UIA Timeout ---> COMException: Operation timed out. (0x80131505)`

- **現象**: `ComboBox.Select()` や要素のクリック中にハングまたはタイムアウトする。

- **原因**: 内部で COM 呼び出しが行われている最中に、WPF の Popup が閉じられたり、別のウィンドウが前面に来たりして、UI ツリーの状態が不安定になった。

- **対策**:
  - `ComboBox.Select()` の代わりに、`Expand()` してから `Items` を一つずつ取得し、ターゲットのアイテムに対して `Patterns.Invoke.Pattern.Invoke()` を実行する。

  - テストの `Dispose` で確実にプロセスを Kill する（ハングした COM 呼び出しを解放するため）。

## 5. XAML 実装上のミス

### エラー: `error MC3072: Property 'AutomationId' does not exist...`

- **現象**: プロジェクトがビルドエラーになる。
- **原因**: WPF において `AutomationId` は標準プロパティではなく、添付プロパティである。
- **誤**: `AutomationId="MyButton"`
- **正**: `AutomationProperties.AutomationId="MyButton"`
- **対策**: `AutomationProperties` 名前空間を介して指定する。
