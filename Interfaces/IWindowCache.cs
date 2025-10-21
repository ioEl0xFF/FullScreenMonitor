using System;
using System.Collections.Generic;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// ウィンドウ情報キャッシュインターフェース
/// </summary>
public interface IWindowCache : IDisposable
{
    /// <summary>
    /// キャッシュされたウィンドウ情報を取得
    /// </summary>
    /// <returns>ウィンドウ情報のリスト</returns>
    List<WindowInfo> GetWindows();

    /// <summary>
    /// 指定されたプロセス名のウィンドウを取得
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns>該当するウィンドウ情報のリスト</returns>
    List<WindowInfo> GetWindowsByProcess(string processName);

    /// <summary>
    /// 指定されたモニター上のウィンドウを取得
    /// </summary>
    /// <param name="monitorHandle">モニターハンドル</param>
    /// <param name="excludeWindow">除外するウィンドウハンドル</param>
    /// <returns>該当するウィンドウ情報のリスト</returns>
    List<WindowInfo> GetWindowsOnMonitor(IntPtr monitorHandle, IntPtr excludeWindow = default);

    /// <summary>
    /// 全画面状態のウィンドウを検索
    /// </summary>
    /// <param name="targetProcesses">監視対象プロセス名のリスト</param>
    /// <returns>全画面状態のウィンドウ情報、見つからない場合はnull</returns>
    WindowInfo? FindFullScreenWindow(List<string> targetProcesses);

    /// <summary>
    /// キャッシュを強制的に更新
    /// </summary>
    void ForceUpdate();

    /// <summary>
    /// キャッシュをクリア
    /// </summary>
    void Clear();
}
