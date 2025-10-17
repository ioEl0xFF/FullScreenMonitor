using System;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// ログレベル
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// デバッグ情報
    /// </summary>
    Debug,

    /// <summary>
    /// 一般情報
    /// </summary>
    Info,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// エラー
    /// </summary>
    Error
}

/// <summary>
/// ロガーインターフェース
/// </summary>
public interface ILogger : IDisposable
{
    /// <summary>
    /// デバッグレベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    void LogDebug(string message, Exception? exception = null);

    /// <summary>
    /// 情報レベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    void LogInfo(string message, Exception? exception = null);

    /// <summary>
    /// 警告レベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    void LogWarning(string message, Exception? exception = null);

    /// <summary>
    /// エラーレベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    void LogError(string message, Exception? exception = null);

    /// <summary>
    /// 指定されたレベルのログを出力
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    void Log(LogLevel level, string message, Exception? exception = null);

    /// <summary>
    /// ログレベルが有効かどうかを確認
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <returns>有効な場合true</returns>
    bool IsEnabled(LogLevel level);
}
