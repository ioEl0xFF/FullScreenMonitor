namespace FullScreenMonitor.Constants;

/// <summary>
/// 監視関連の定数
/// </summary>
public static class MonitorConstants
{
    /// <summary>
    /// 監視間隔の最小値（ミリ秒）
    /// </summary>
    public const int MinMonitorInterval = 100;

    /// <summary>
    /// 監視間隔の最大値（ミリ秒）
    /// </summary>
    public const int MaxMonitorInterval = 2000;

    /// <summary>
    /// デフォルトの監視間隔（ミリ秒）
    /// </summary>
    public const int DefaultMonitorInterval = 500;

    /// <summary>
    /// 監視間隔のスライダーの刻み値
    /// </summary>
    public const int MonitorIntervalTickFrequency = 100;

    /// <summary>
    /// デフォルトの監視対象プロセス
    /// </summary>
    public static readonly string[] DefaultTargetProcesses = { "chrome", "firefox", "msedge" };

    /// <summary>
    /// プロセス名の最大長
    /// </summary>
    public const int MaxProcessNameLength = 64;

    /// <summary>
    /// ウィンドウテキストの最大長
    /// </summary>
    public const int MaxWindowTextLength = 256;

    /// <summary>
    /// クラス名の最大長
    /// </summary>
    public const int MaxClassNameLength = 256;

    /// <summary>
    /// プロセスパスの最大長
    /// </summary>
    public const int MaxProcessPathLength = 1024;
}
