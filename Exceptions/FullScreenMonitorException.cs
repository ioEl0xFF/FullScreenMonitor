using System;
using System.Runtime.Serialization;

namespace FullScreenMonitor.Exceptions;

/// <summary>
/// FullScreenMonitorアプリケーションの基底例外クラス
/// </summary>
[Serializable]
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

    /// <summary>
    /// シリアライゼーション用コンストラクタ
    /// </summary>
    /// <param name="info">シリアライゼーション情報</param>
    /// <param name="context">ストリーミングコンテキスト</param>
    protected FullScreenMonitorException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ErrorCode = info.GetString(nameof(ErrorCode)) ?? "UNKNOWN_ERROR";
    }

    /// <summary>
    /// シリアライゼーション情報を取得
    /// </summary>
    /// <param name="info">シリアライゼーション情報</param>
    /// <param name="context">ストリーミングコンテキスト</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ErrorCode), ErrorCode);
    }
}
