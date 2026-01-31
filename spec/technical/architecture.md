# 技術仕様：アーキテクチャ

## 1. レイヤー分離 (クリーンアーキテクチャ)

- **依存性のルール**: 依存関係は常に外側（View）から内側（Domain/Service）へ向かう一方向とする。
- **Use Case 層**: 複雑なビジネスフローやアプリケーションの実行フローは ViewModel に直接記述せず、Use Case クラスに抽出する。
- **DI (Dependency Injection)**: サービス間の依存関係はインターフェースを介して DI コンテナにより解決し、ユニットテストでモック可能にする。

## 2. MVVM パターンの純粋性

- **WPF 依存の排除**: ViewModel は純粋な C# クラス (POCO) とし、`System.Windows` 名前空間（GridLength, Brush, Visibility 等）に直接依存してはならない。
- **型変換の責務**: UI 固有の型への変換は View 側の Converter で行い、ViewModel はプリミティブ型（double, bool, string 等）のみを公開する。
- **コードビハインドの最小化**: View のロジックは可能な限り Behavior や添付プロパティとして抽出する。

## 3. ウィンドウ管理

- **IWindowService**: ViewModel から直接 View (Window) を生成・操作することを避け、テスト容易性を確保する。ウィンドウの重複起動防止（Activate制御）を集約する。
