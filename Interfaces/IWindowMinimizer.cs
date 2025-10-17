using System;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// ウィンドウ最小化サービスインターフェース
/// </summary>
public interface IWindowMinimizer
{
    /// <summary>
    /// 最小化したウィンドウ数を取得
    /// </summary>
    int MinimizedWindowCount { get; }

    /// <summary>
    /// 指定モニター上のウィンドウを最小化
    /// </summary>
    /// <param name="monitorHandle">モニターハンドル</param>
    /// <param name="excludeWindow">除外するウィンドウハンドル</param>
    /// <returns>最小化したウィンドウ数</returns>
    int MinimizeWindowsOnMonitor(IntPtr monitorHandle, IntPtr excludeWindow = default);

    /// <summary>
    /// 最小化したウィンドウを復元
    /// </summary>
    /// <returns>復元したウィンドウ数</returns>
    int RestoreMinimizedWindows();

    /// <summary>
    /// 同一モニター上の対象プロセス以外のウィンドウを最小化
    /// </summary>
    /// <param name="targetWindow">対象ウィンドウハンドル</param>
    /// <param name="monitorHandle">モニターハンドル</param>
    /// <returns>最小化したウィンドウ数</returns>
    int MinimizeAllNonTargetWindows(IntPtr targetWindow, IntPtr monitorHandle);

    /// <summary>
    /// 最小化履歴をクリア
    /// </summary>
    void ClearMinimizedHistory();
}
