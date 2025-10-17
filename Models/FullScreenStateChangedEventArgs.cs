using System;

namespace FullScreenMonitor.Models;

/// <summary>
/// 全画面状態変更イベント引数
/// </summary>
public class FullScreenStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// 全画面状態かどうか
    /// </summary>
    public bool IsFullScreen { get; set; }

    /// <summary>
    /// ウィンドウハンドル
    /// </summary>
    public IntPtr WindowHandle { get; set; }

    /// <summary>
    /// モニターハンドル
    /// </summary>
    public IntPtr MonitorHandle { get; set; }

    /// <summary>
    /// プロセス名
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// ウィンドウタイトル
    /// </summary>
    public string WindowTitle { get; set; } = string.Empty;

    /// <summary>
    /// 状態変更が発生した時刻
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="isFullScreen">全画面状態かどうか</param>
    /// <param name="windowHandle">ウィンドウハンドル</param>
    /// <param name="monitorHandle">モニターハンドル</param>
    /// <param name="processName">プロセス名</param>
    /// <param name="windowTitle">ウィンドウタイトル</param>
    public FullScreenStateChangedEventArgs(bool isFullScreen, IntPtr windowHandle, IntPtr monitorHandle,
        string processName = "", string windowTitle = "")
    {
        IsFullScreen = isFullScreen;
        WindowHandle = windowHandle;
        MonitorHandle = monitorHandle;
        ProcessName = processName;
        WindowTitle = windowTitle;
        Timestamp = DateTime.Now;
    }

    /// <summary>
    /// デフォルトコンストラクタ
    /// </summary>
    public FullScreenStateChangedEventArgs()
    {
        Timestamp = DateTime.Now;
    }
}
