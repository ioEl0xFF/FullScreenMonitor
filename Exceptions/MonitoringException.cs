using System;

namespace FullScreenMonitor.Exceptions;

/// <summary>
/// 監視関連の例外
/// </summary>
public class MonitoringException : FullScreenMonitorException
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public MonitoringException(string message) : base(message, "MONITORING_ERROR")
    {
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="innerException">内部例外</param>
    public MonitoringException(string message, Exception innerException)
        : base(message, "MONITORING_ERROR", innerException)
    {
    }
}
