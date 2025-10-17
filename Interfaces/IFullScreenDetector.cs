using System;
using System.Collections.Generic;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// 全画面検出サービスインターフェース
/// </summary>
public interface IFullScreenDetector : IDisposable
{
    /// <summary>
    /// 全画面状態が変更された時のイベント
    /// </summary>
    event EventHandler<FullScreenStateChangedEventArgs>? FullScreenStateChanged;

    /// <summary>
    /// 対象プロセスがフォーカスされた時のイベント
    /// </summary>
    event EventHandler<FullScreenStateChangedEventArgs>? TargetProcessFocused;

    /// <summary>
    /// 監視中かどうか
    /// </summary>
    bool IsMonitoring { get; }

    /// <summary>
    /// 前回の全画面状態
    /// </summary>
    bool WasFullScreen { get; }

    /// <summary>
    /// 現在の全画面ウィンドウのハンドル
    /// </summary>
    IntPtr CurrentFullScreenWindow { get; }

    /// <summary>
    /// 現在の全画面ウィンドウが属するモニター
    /// </summary>
    IntPtr CurrentMonitor { get; }

    /// <summary>
    /// 監視を開始
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// 監視を停止
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// 監視間隔を更新
    /// </summary>
    /// <param name="intervalMs">新しい監視間隔（ミリ秒）</param>
    void UpdateInterval(int intervalMs);

    /// <summary>
    /// 監視対象プロセスを更新
    /// </summary>
    /// <param name="targetProcesses">新しい監視対象プロセス名のリスト</param>
    void UpdateTargetProcesses(List<string> targetProcesses);
}
