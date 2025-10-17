# Windows全画面監視アプリ実装計画

## 概要

指定したアプリケーションが全画面表示された際に、同一モニター上の他ウィンドウを自動最小化する常駐アプリケーション。

## 技術スタック

- **言語**: C# (.NET 6以降)
- **UI フレームワーク**: WPF
- **Windows API**: User32.dll (P/Invoke)
- **JSON処理**: System.Text.Json
- **起動方式**: Windows スタートアップ登録（レジストリ）

## プロジェクト構造

```
FullScreenMonitor/
├── FullScreenMonitor.csproj         # プロジェクトファイル (.NET 6-8, WPF, Windows)
├── App.xaml / App.xaml.cs          # アプリケーションエントリーポイント
├── MainWindow.xaml / .cs            # メインウィンドウ (起動時非表示)
├── SettingsWindow.xaml / .cs        # 設定画面UI
├── Services/
│   ├── WindowMonitorService.cs     # 監視サービス統合クラス
│   ├── FullScreenDetector.cs       # 全画面検出ロジック
│   └── WindowMinimizer.cs          # ウィンドウ最小化処理
├── Models/
│   └── AppSettings.cs              # 設定データモデル
├── Helpers/
│   ├── NativeMethods.cs            # Win32 API定義 (P/Invoke)
│   ├── StartupManager.cs           # スタートアップ登録/解除
│   └── SettingsManager.cs          # 設定ファイル読み書き
└── Resources/
    └── app.ico                      # トレイアイコン
.gitignore                           # Git除外設定
README.md                            # プロジェクト説明書
```

## 実装の詳細

### 1. Win32 API連携 (NativeMethods.cs)

必要なAPI定義:

- `EnumWindows`: 全ウィンドウを列挙
- `GetWindowRect`: ウィンドウの位置・サイズ取得
- `MonitorFromWindow`: ウィンドウが属するモニター取得
- `GetMonitorInfo`: モニター情報取得
- `GetWindowPlacement`: ウィンドウの表示状態取得（全画面判定用）
- `ShowWindow(SW_MINIMIZE)`: ウィンドウ最小化
- `GetWindowThreadProcessId`: ウィンドウのプロセスID取得
- `IsWindowVisible`: ウィンドウの可視状態確認
- `GetWindowLong(GWL_EXSTYLE)`: 拡張スタイル取得（ツールウィンドウ除外用）

### 2. 全画面検出ロジック (FullScreenDetector.cs)

- `System.Windows.Threading.DispatcherTimer`で定期監視 (500ms間隔)
- 検出手順:

  1. 全ウィンドウを列挙
  2. 対象プロセス名に一致するウィンドウを抽出
  3. `GetWindowPlacement`で最大化状態（SW_MAXIMIZE）を確認
  4. ウィンドウサイズとモニターの作業領域を比較（タスクバー除外）
  5. 全画面と判定されたら、所属モニターハンドルを返す

### 3. ウィンドウ最小化処理 (WindowMinimizer.cs)

- 処理フロー:

  1. 指定モニター上の全ウィンドウを列挙
  2. 以下を除外: 自分自身、全画面アプリ、非表示ウィンドウ、ツールウィンドウ
  3. `ShowWindow(SW_MINIMIZE)`で最小化実行

- 最小化履歴を保持（復元機能のため）

### 4. システムトレイ統合 (MainWindow.xaml.cs)

- `System.Windows.Forms.NotifyIcon`を使用（NuGetパッケージ不要）
- コンテキストメニュー: 「設定を開く」「終了」
- ダブルクリックで設定画面を表示
- アプリケーション起動時はウィンドウを非表示（`ShowInTaskbar=False`, `WindowState=Minimized`）

### 5. 設定画面 (SettingsWindow.xaml)

UI要素:

- 監視対象プロセス名のListBox（追加・削除ボタン付き）
- プロセス名入力TextBox
- 監視間隔スライダー（100ms〜2000ms）
- スタートアップ登録CheckBox
- 保存・キャンセルボタン

データバインディング:

- `AppSettings`モデルとバインド
- `INotifyPropertyChanged`実装

### 6. スタートアップ登録 (StartupManager.cs)

- レジストリパス: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- キー名: `FullScreenMonitor`
- 値: 実行ファイルの絶対パス
- メソッド: `Register()`, `Unregister()`, `IsRegistered()`

### 7. 設定管理 (SettingsManager.cs)

- 保存先: `%AppData%\FullScreenMonitor\settings.json`
- `System.Text.Json`でシリアライズ/デシリアライズ
- デフォルト設定を提供
- エラーハンドリング（ファイル破損時はデフォルトに戻す）

### 8. 監視サービス統合 (WindowMonitorService.cs)

- `FullScreenDetector`と`WindowMinimizer`を統合
- 状態管理: 前回の全画面状態を保持
- 全画面→通常画面の遷移時の処理も実装

## 設定ファイル構造 (settings.json)

```json
{
  "targetProcesses": ["chrome", "firefox", "msedge"],
  "monitorInterval": 500,
  "startWithWindows": true
}
```

## Git設定

### .gitignore

Visual Studio / .NET標準の除外設定:

- `bin/`, `obj/` - ビルド出力
- `.vs/`, `.vscode/` - IDE設定
- `*.user` - ユーザー固有設定
- `*.suo`, `*.cache` - 一時ファイル

### README.md

含める内容:

- アプリケーション概要
- 機能説明
- システム要件（Windows 10/11, .NET 6以降）
- ビルド方法
- 使用方法

### コミット戦略

- ユーザールールに従ったコミットメッセージ（`feat:`, `fix:`, `refactor:`など）
- 機能単位での適切なコミット粒度

## 実装手順

1. **初期セットアップ**: Git初期化、.gitignore、README作成
2. **プロジェクト作成**: WPFプロジェクト作成、NuGet参照追加
3. **Win32 API**: NativeMethodsクラス実装
4. **モデル層**: AppSettings、SettingsManager実装
5. **検出ロジック**: FullScreenDetector実装
6. **最小化処理**: WindowMinimizer実装
7. **統合サービス**: WindowMonitorService実装
8. **システムトレイ**: MainWindow改修、NotifyIcon統合
9. **設定UI**: SettingsWindow実装
10. **スタートアップ**: StartupManager実装
11. **テスト・修正**: 動作確認とバグ修正

## 注意事項

- 全画面検出は最大化状態（F11キー）のみ対応
- マルチモニター環境で適切に動作すること
- 最小化対象から除外すべきウィンドウ: システムトレイ、デスクトップ、タスクバー
- エラーハンドリングを適切に実装（アクセス拒否エラーなど）