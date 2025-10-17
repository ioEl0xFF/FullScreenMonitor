using System;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// コンソールロガー実装（デバッグ用）
/// </summary>
public class ConsoleLogger : ILogger
{
    private readonly LogLevel _minimumLevel;
    private bool _disposed = false;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="minimumLevel">最小ログレベル</param>
    public ConsoleLogger(LogLevel minimumLevel = LogLevel.Debug)
    {
        _minimumLevel = minimumLevel;
    }

    /// <summary>
    /// デバッグレベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    public void LogDebug(string message, Exception? exception = null)
    {
        Log(LogLevel.Debug, message, exception);
    }

    /// <summary>
    /// 情報レベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    public void LogInfo(string message, Exception? exception = null)
    {
        Log(LogLevel.Info, message, exception);
    }

    /// <summary>
    /// 警告レベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    public void LogWarning(string message, Exception? exception = null)
    {
        Log(LogLevel.Warning, message, exception);
    }

    /// <summary>
    /// エラーレベルのログを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    public void LogError(string message, Exception? exception = null)
    {
        Log(LogLevel.Error, message, exception);
    }

    /// <summary>
    /// 指定されたレベルのログを出力
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    public void Log(LogLevel level, string message, Exception? exception = null)
    {
        if (_disposed || !IsEnabled(level))
        {
            return;
        }

        try
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            var levelText = level.ToString().ToUpper().PadRight(7);
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D2");

            Console.WriteLine($"[{timestamp}] [{levelText}] [T{threadId}] {message}");

            if (exception != null)
            {
                Console.WriteLine($"    例外: {exception.GetType().Name}");
                Console.WriteLine($"    メッセージ: {exception.Message}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    Console.WriteLine($"    スタックトレース: {exception.StackTrace}");
                }
            }
        }
        catch
        {
            // コンソール出力に失敗した場合は無視
        }
    }

    /// <summary>
    /// ログレベルが有効かどうかを確認
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <returns>有効な場合true</returns>
    public bool IsEnabled(LogLevel level)
    {
        return level >= _minimumLevel;
    }

    /// <summary>
    /// リソースを解放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソースを解放
    /// </summary>
    /// <param name="disposing">マネージリソースを解放するかどうか</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
