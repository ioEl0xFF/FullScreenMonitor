using System;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// ウィンドウ監視サービスインターフェース
/// </summary>
public interface IWindowMonitorService : IDisposable
{
    /// <summary>
    /// 監視状態が変更された時のイベント
    /// </summary>
    event EventHandler<bool>? MonitoringStateChanged;

    /// <summary>
    /// ウィンドウが最小化された時のイベント
    /// </summary>
    event EventHandler<int>? WindowsMinimized;

    /// <summary>
    /// ウィンドウが復元された時のイベント
    /// </summary>
    event EventHandler<int>? WindowsRestored;

    /// <summary>
    /// エラーが発生した時のイベント
    /// </summary>
    event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// 監視中かどうか
    /// </summary>
    bool IsMonitoring { get; }

    /// <summary>
    /// 現在の設定
    /// </summary>
    AppSettings CurrentSettings { get; }

    /// <summary>
    /// 最後にチェックした時刻
    /// </summary>
    DateTime LastCheckTime { get; }

    /// <summary>
    /// 監視を開始
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// 監視を停止
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// 設定を更新
    /// </summary>
    /// <param name="newSettings">新しい設定</param>
    void UpdateSettings(AppSettings newSettings);

    /// <summary>
    /// 手動でウィンドウを復元
    /// </summary>
    /// <returns>復元したウィンドウ数</returns>
    int RestoreWindowsManually();

    /// <summary>
    /// 監視統計情報を取得
    /// </summary>
    /// <returns>監視統計情報</returns>
    MonitoringStats GetStats();
}
