namespace FullScreenMonitor.Constants;

/// <summary>
/// エラーメッセージの定数
/// </summary>
public static class ErrorMessages
{
    #region 一般的なエラー

    /// <summary>
    /// 予期しないエラー
    /// </summary>
    public const string UnexpectedError = "予期しないエラーが発生しました";

    /// <summary>
    /// 引数がnullまたは空
    /// </summary>
    public const string ArgumentNullOrEmpty = "引数がnullまたは空です";

    /// <summary>
    /// 操作がタイムアウト
    /// </summary>
    public const string OperationTimeout = "操作がタイムアウトしました";

    #endregion

    #region アプリケーション関連

    /// <summary>
    /// アプリケーション初期化エラー
    /// </summary>
    public const string ApplicationInitializationError = "アプリケーションの初期化中にエラーが発生しました";

    /// <summary>
    /// 多重起動エラー
    /// </summary>
    public const string MultipleInstanceError = "アプリケーションは既に実行中です";

    /// <summary>
    /// アプリケーション終了エラー
    /// </summary>
    public const string ApplicationExitError = "アプリケーションの終了中にエラーが発生しました";

    #endregion

    #region 設定関連

    /// <summary>
    /// 設定読み込みエラー
    /// </summary>
    public const string SettingsLoadError = "設定の読み込みに失敗しました";

    /// <summary>
    /// 設定保存エラー
    /// </summary>
    public const string SettingsSaveError = "設定の保存に失敗しました";

    /// <summary>
    /// 設定ファイル削除エラー
    /// </summary>
    public const string SettingsDeleteError = "設定ファイルの削除に失敗しました";

    /// <summary>
    /// 設定検証エラー
    /// </summary>
    public const string SettingsValidationError = "設定値が無効です";

    /// <summary>
    /// 監視間隔範囲エラー
    /// </summary>
    public const string MonitorIntervalRangeError = "監視間隔は{0}ms〜{1}msの範囲で設定してください";

    /// <summary>
    /// プロセス名重複エラー
    /// </summary>
    public const string ProcessNameDuplicateError = "このプロセスは既に追加されています";

    /// <summary>
    /// プロセス名入力エラー
    /// </summary>
    public const string ProcessNameInputError = "プロセス名を入力してください";

    /// <summary>
    /// プロセス選択エラー
    /// </summary>
    public const string ProcessSelectionError = "削除するプロセスを選択してください";

    #endregion

    #region 監視関連

    /// <summary>
    /// 監視開始エラー
    /// </summary>
    public const string MonitoringStartError = "監視の開始に失敗しました";

    /// <summary>
    /// 監視停止エラー
    /// </summary>
    public const string MonitoringStopError = "監視の停止に失敗しました";

    /// <summary>
    /// 全画面検出エラー
    /// </summary>
    public const string FullScreenDetectionError = "全画面検出中にエラーが発生しました";

    /// <summary>
    /// 監視対象プロセス未設定エラー
    /// </summary>
    public const string NoTargetProcessesError = "監視対象プロセスが設定されていません";

    #endregion

    #region ウィンドウ操作関連

    /// <summary>
    /// ウィンドウ最小化エラー
    /// </summary>
    public const string WindowMinimizeError = "ウィンドウの最小化に失敗しました";

    /// <summary>
    /// ウィンドウ復元エラー
    /// </summary>
    public const string WindowRestoreError = "ウィンドウの復元に失敗しました";

    /// <summary>
    /// ウィンドウ情報取得エラー
    /// </summary>
    public const string WindowInfoGetError = "ウィンドウ情報の取得に失敗しました";

    /// <summary>
    /// プロセス情報取得エラー
    /// </summary>
    public const string ProcessInfoGetError = "プロセス情報の取得に失敗しました";

    #endregion

    #region スタートアップ関連

    /// <summary>
    /// スタートアップ登録エラー
    /// </summary>
    public const string StartupRegistrationError = "スタートアップ登録に失敗しました";

    /// <summary>
    /// スタートアップ解除エラー
    /// </summary>
    public const string StartupUnregistrationError = "スタートアップ解除に失敗しました";

    /// <summary>
    /// スタートアップ確認エラー
    /// </summary>
    public const string StartupCheckError = "スタートアップ状態の確認に失敗しました";

    /// <summary>
    /// 実行ファイルパス取得エラー
    /// </summary>
    public const string ExecutablePathGetError = "実行ファイルパスの取得に失敗しました";

    #endregion

    #region UI関連

    /// <summary>
    /// 設定画面表示エラー
    /// </summary>
    public const string SettingsWindowDisplayError = "設定画面の表示中にエラーが発生しました";

    /// <summary>
    /// プロセス一覧更新エラー
    /// </summary>
    public const string ProcessListUpdateError = "実行中プロセスの取得中にエラーが発生しました";

    /// <summary>
    /// 通知表示エラー
    /// </summary>
    public const string NotificationDisplayError = "通知の表示に失敗しました";

    #endregion
}
