using System;
using System.IO;
using System.Text;
using System.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// ファイルロガー実装
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _logFilePath;
    private readonly LogLevel _minimumLevel;
    private readonly object _lockObject = new();
    private FileStream? _fileStream;
    private StreamWriter? _writer;
    private bool _disposed = false;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logDirectory">ログディレクトリ（nullの場合はアプリケーションデータディレクトリを使用）</param>
    /// <param name="minimumLevel">最小ログレベル</param>
    public FileLogger(string? logDirectory = null, LogLevel minimumLevel = LogLevel.Info)
    {
        _minimumLevel = minimumLevel;

        // ログディレクトリの設定
        if (string.IsNullOrEmpty(logDirectory))
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            logDirectory = Path.Combine(appDataPath, AppConstants.SettingsDirectoryName);
        }

        // ログディレクトリが存在しない場合は作成
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // ログファイル名を生成（日付付き）
        var fileName = $"{AppConstants.LogFilePrefix}_{DateTime.Now:yyyyMMdd}{AppConstants.LogFileExtension}";
        _logFilePath = Path.Combine(logDirectory, fileName);

        // ファイルストリームを初期化
        InitializeWriter();
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
            var logEntry = CreateLogEntry(level, message, exception);
            WriteToFile(logEntry);
        }
        catch
        {
            // ログ書き込みに失敗した場合は無視（無限ループを防ぐため）
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
            if (disposing)
            {
                lock (_lockObject)
                {
                    try
                    {
                        _writer?.Dispose();
                        _fileStream?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"FileLogger Dispose エラー: {ex.Message}");
                    }
                    finally
                    {
                        _writer = null;
                        _fileStream = null;
                    }
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// ログエントリを作成
    /// </summary>
    /// <param name="level">ログレベル</param>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外</param>
    /// <returns>ログエントリ文字列</returns>
    private static string CreateLogEntry(LogLevel level, string message, Exception? exception)
    {
        var sb = new StringBuilder();
        sb.Append($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ");
        sb.Append($"[{level.ToString().ToUpper()}] ");
        sb.Append($"[{Thread.CurrentThread.ManagedThreadId:D2}] ");
        sb.Append(message);

        if (exception != null)
        {
            sb.AppendLine();
            sb.Append($"例外: {exception.GetType().Name}");
            sb.AppendLine();
            sb.Append($"メッセージ: {exception.Message}");

            if (!string.IsNullOrEmpty(exception.StackTrace))
            {
                sb.AppendLine();
                sb.Append($"スタックトレース: {exception.StackTrace}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// ファイルストリームを初期化
    /// </summary>
    private void InitializeWriter()
    {
        try
        {
            _fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
            _writer = new StreamWriter(_fileStream, Encoding.UTF8) { AutoFlush = true };
        }
        catch (Exception ex)
        {
            // フォールバック処理：ファイルストリームが失敗した場合は従来の方法を使用
            _writer = null;
            _fileStream = null;
            System.Diagnostics.Debug.WriteLine($"FileLogger初期化エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// ファイルにログを書き込み
    /// </summary>
    /// <param name="logEntry">ログエントリ</param>
    private void WriteToFile(string logEntry)
    {
        lock (_lockObject)
        {
            if (_disposed)
                return;

            try
            {
                if (_writer != null)
                {
                    _writer.WriteLine(logEntry);
                }
                else
                {
                    // フォールバック：ファイルストリームが使用できない場合
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                // ログ書き込みに失敗した場合は無視（無限ループを防ぐため）
                System.Diagnostics.Debug.WriteLine($"ログ書き込みエラー: {ex.Message}");
            }
        }
    }
}
