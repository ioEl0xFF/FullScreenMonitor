namespace FullScreenMonitor.Constants;

/// <summary>
/// アプリケーション全体で使用される定数
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// アプリケーション名
    /// </summary>
    public const string ApplicationName = "FullScreenMonitor";

    /// <summary>
    /// アプリケーションのバージョン
    /// </summary>
    public const string ApplicationVersion = "1.0.0";

    /// <summary>
    /// 設定ディレクトリ名
    /// </summary>
    public const string SettingsDirectoryName = "FullScreenMonitor";

    /// <summary>
    /// 設定ファイル名
    /// </summary>
    public const string SettingsFileName = "settings.json";

    /// <summary>
    /// スタートアップ登録用のレジストリキー名
    /// </summary>
    public const string StartupRegistryKeyName = "FullScreenMonitor";

    /// <summary>
    /// 単一インスタンス用のミューテックス名
    /// </summary>
    public const string SingleInstanceMutexName = "FullScreenMonitor_SingleInstance";

    /// <summary>
    /// ログファイル名のプレフィックス
    /// </summary>
    public const string LogFilePrefix = "FullScreenMonitor";

    /// <summary>
    /// ログファイルの拡張子
    /// </summary>
    public const string LogFileExtension = ".log";

    /// <summary>
    /// システムトレイアイコンのテキスト（監視中）
    /// </summary>
    public const string SystemTrayTextMonitoring = "FullScreenMonitor - 監視中";

    /// <summary>
    /// システムトレイアイコンのテキスト（停止中）
    /// </summary>
    public const string SystemTrayTextStopped = "FullScreenMonitor - 停止中";
}
