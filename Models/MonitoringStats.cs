using System;

namespace FullScreenMonitor.Models;

/// <summary>
/// 監視統計情報
/// </summary>
public class MonitoringStats
{
    /// <summary>
    /// 監視中かどうか
    /// </summary>
    public bool IsMonitoring { get; set; }

    /// <summary>
    /// 最後にチェックした時刻
    /// </summary>
    public DateTime LastCheckTime { get; set; }

    /// <summary>
    /// 最小化されたウィンドウ数
    /// </summary>
    public int MinimizedWindowCount { get; set; }

    /// <summary>
    /// 監視対象プロセス数
    /// </summary>
    public int TargetProcessCount { get; set; }

    /// <summary>
    /// 監視間隔（ミリ秒）
    /// </summary>
    public int MonitorInterval { get; set; }

    /// <summary>
    /// 累計最小化回数
    /// </summary>
    public int TotalMinimizedCount { get; set; }

    /// <summary>
    /// 累計復元回数
    /// </summary>
    public int TotalRestoredCount { get; set; }

    /// <summary>
    /// アプリケーション開始時刻
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 稼働時間を取得
    /// </summary>
    public TimeSpan Uptime => DateTime.Now - StartTime;

    /// <summary>
    /// デフォルトの統計情報を作成
    /// </summary>
    /// <returns>デフォルトの統計情報</returns>
    public static MonitoringStats CreateDefault()
    {
        return new MonitoringStats
        {
            IsMonitoring = false,
            LastCheckTime = DateTime.MinValue,
            MinimizedWindowCount = 0,
            TargetProcessCount = 0,
            MonitorInterval = MonitorConstants.DefaultMonitorInterval,
            TotalMinimizedCount = 0,
            TotalRestoredCount = 0,
            StartTime = DateTime.Now
        };
    }
}
