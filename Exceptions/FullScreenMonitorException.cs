using System;

namespace FullScreenMonitor.Exceptions;

/// <summary>
/// FullScreenMonitorアプリケーションの基底例外クラス
/// </summary>
public class FullScreenMonitorException : Exception
{
    /// <summary>
    /// エラーコード
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public FullScreenMonitorException() : base()
    {
        ErrorCode = "UNKNOWN_ERROR";
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    public FullScreenMonitorException(string message) : base(message)
    {
        ErrorCode = "GENERAL_ERROR";
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="errorCode">エラーコード</param>
    public FullScreenMonitorException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="innerException">内部例外</param>
    public FullScreenMonitorException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = "GENERAL_ERROR";
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="message">エラーメッセージ</param>
    /// <param name="errorCode">エラーコード</param>
    /// <param name="innerException">内部例外</param>
    public FullScreenMonitorException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }

}
