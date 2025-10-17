using System;

namespace FullScreenMonitor.Exceptions;

/// <summary>
/// ウィンドウ操作関連の例外
/// </summary>
public class WindowOperationException : FullScreenMonitorException
{
    /// <summary>
    /// ウィンドウハンドル
    /// </summary>
    public IntPtr WindowHandle { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="windowHandle">ウィンドウハンドル</param>
    public WindowOperationException(string message, IntPtr windowHandle)
        : base(message, "WINDOW_OPERATION_ERROR")
    {
        WindowHandle = windowHandle;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="windowHandle">ウィンドウハンドル</param>
    /// <param name="innerException">内部例外</param>
    public WindowOperationException(string message, IntPtr windowHandle, Exception innerException)
        : base(message, "WINDOW_OPERATION_ERROR", innerException)
    {
        WindowHandle = windowHandle;
    }
}
