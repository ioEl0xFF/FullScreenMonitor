using System;

namespace FullScreenMonitor.Exceptions;

/// <summary>
/// 設定関連の例外
/// </summary>
public class SettingsException : FullScreenMonitorException
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public SettingsException(string message) : base(message, "SETTINGS_ERROR")
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="innerException">内部例外</param>
    public SettingsException(string message, Exception innerException)
        : base(message, "SETTINGS_ERROR", innerException)
    {
    }
}
