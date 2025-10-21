using System;
using System.Collections.Generic;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Services;

/// <summary>
/// サービスクラスの基底クラス
/// 共通処理とヘルパーメソッドを提供
/// </summary>
public abstract class ServiceBase : IDisposable
{
    #region フィールド

    protected readonly ILogger _logger;
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed = false;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    protected ServiceBase(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region プロパティ

    /// <summary>
    /// ロガー
    /// </summary>
    protected ILogger Logger => _logger;

    /// <summary>
    /// 破棄済みかどうか
    /// </summary>
    protected bool IsDisposed => _disposed;

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// リソースを解放
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region 保護メソッド

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
                // 登録されたリソースを解放
                foreach (var disposable in _disposables)
                {
                    try
                    {
                        disposable?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"リソース解放エラー: {ex.Message}", ex);
                    }
                }
                _disposables.Clear();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// 破棄可能なリソースを登録
    /// </summary>
    /// <param name="disposable">破棄可能なリソース</param>
    protected void RegisterDisposable(IDisposable disposable)
    {
        if (disposable != null)
        {
            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// 安全な操作を実行（エラーハンドリング付き）
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <param name="operationName">操作名</param>
    /// <returns>実行結果</returns>
    protected Result ExecuteSafe(Action action, string operationName)
    {
        try
        {
            if (_disposed)
            {
                return Result.Failure($"{operationName}: サービスが破棄されています");
            }

            action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.LogError($"{operationName}中にエラーが発生しました: {ex.Message}", ex);
            return Result.Failure($"{operationName}中にエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// 安全な操作を実行（エラーハンドリング付き、戻り値あり）
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する関数</param>
    /// <param name="operationName">操作名</param>
    /// <param name="defaultValue">失敗時のデフォルト値</param>
    /// <returns>実行結果</returns>
    protected Result<T> ExecuteSafe<T>(Func<T> func, string operationName, T defaultValue = default!)
    {
        try
        {
            if (_disposed)
            {
                return Result<T>.Failure($"{operationName}: サービスが破棄されています");
            }

            var result = func();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            Logger.LogError($"{operationName}中にエラーが発生しました: {ex.Message}", ex);
            return Result<T>.Failure($"{operationName}中にエラーが発生しました", ex);
        }
    }

    /// <summary>
    /// 破棄済みチェック
    /// </summary>
    /// <exception cref="ObjectDisposedException">破棄済みの場合</exception>
    protected void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().Name);
        }
    }

    /// <summary>
    /// ログ付きの情報メッセージを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    protected void LogInfo(string message)
    {
        Logger.LogInfo($"[{GetType().Name}] {message}");
    }

    /// <summary>
    /// ログ付きの警告メッセージを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    protected void LogWarning(string message, Exception? exception = null)
    {
        Logger.LogWarning($"[{GetType().Name}] {message}", exception);
    }

    /// <summary>
    /// ログ付きのエラーメッセージを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    protected void LogError(string message, Exception? exception = null)
    {
        Logger.LogError($"[{GetType().Name}] {message}", exception);
    }

    /// <summary>
    /// ログ付きのデバッグメッセージを出力
    /// </summary>
    /// <param name="message">メッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    protected void LogDebug(string message, Exception? exception = null)
    {
        Logger.LogDebug($"[{GetType().Name}] {message}", exception);
    }

    #endregion
}
