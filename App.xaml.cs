using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Services;
using WinFormsApplication = System.Windows.Forms.Application;

namespace FullScreenMonitor;

/// <summary>
/// アプリケーションエントリーポイント
/// </summary>
public partial class App : System.Windows.Application
{
    #region フィールド

    private static readonly Mutex _mutex = new(false, AppConstants.SingleInstanceMutexName);
    private MainWindow? _mainWindow;
    private ILogger? _logger;

    #endregion

    #region アプリケーションライフサイクル

    /// <summary>
    /// アプリケーション起動処理
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        // ロガーを初期化
        _logger = new FileLogger();
        _logger.LogInfo($"{AppConstants.ApplicationName} v{AppConstants.ApplicationVersion} を起動中...");
        _logger.LogInfo($"起動引数: {string.Join(" ", e.Args)}");

        // 多重起動の防止
        if (!_mutex.WaitOne(TimeSpan.Zero, false))
        {
            _logger.LogWarning("アプリケーションの多重起動を検出しました");
            System.Windows.MessageBox.Show(
                $"{AppConstants.ApplicationName}は既に実行中です。\nシステムトレイを確認してください。",
                "多重起動エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Current.Shutdown();
            return;
        }

        // グローバル例外ハンドラーの設定
        SetupExceptionHandling();

        try
        {
            _logger.LogInfo("Material Designリソースの読み込みを開始...");

            // MainWindowを非表示で起動
            _logger.LogInfo("MainWindowの初期化を開始...");
            _mainWindow = new MainWindow();
            _logger.LogInfo("MainWindowの初期化が完了しました");

            _logger.LogInfo("MainWindowを表示中...");
            _mainWindow.Show();
            _logger.LogInfo("MainWindowを非表示に設定中...");
            _mainWindow.Hide(); // システムトレイ常駐のため非表示

            _logger.LogInfo("アプリケーションの起動が完了しました");
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            _logger.LogError($"アプリケーション初期化エラー: {ex.Message}", ex);
            System.Windows.MessageBox.Show(
                $"アプリケーションの初期化中にエラーが発生しました:\n{ex.Message}\n\n詳細はログファイルを確認してください。",
                "起動エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Current.Shutdown();
        }
    }

    /// <summary>
    /// アプリケーション終了処理
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            _logger?.LogInfo("アプリケーションを終了中...");
            _mainWindow?.Close();
            _logger?.LogInfo("アプリケーションの終了が完了しました");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ErrorMessages.ApplicationExitError, ex);
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _logger?.Dispose();
            base.OnExit(e);
        }
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// 例外ハンドリングの設定
    /// </summary>
    private void SetupExceptionHandling()
    {
        // UIスレッドの未処理例外
        DispatcherUnhandledException += (sender, e) =>
        {
            e.Handled = true;
            HandleUnhandledException(e.Exception, "UIスレッド");
        };

        // アプリケーションドメインの未処理例外
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            HandleUnhandledException(e.ExceptionObject as Exception ?? new Exception("不明なエラー"), "アプリケーションドメイン");
        };
    }

    /// <summary>
    /// 未処理例外のハンドリング
    /// </summary>
    private void HandleUnhandledException(Exception exception, string source)
    {
        var errorMessage = $"{ErrorMessages.UnexpectedError}\n\n" +
                          $"発生元: {source}\n" +
                          $"エラータイプ: {exception.GetType().Name}\n" +
                          $"エラーメッセージ: {exception.Message}\n\n" +
                          $"詳細情報はログファイルを確認してください。";

        _logger?.LogError($"未処理例外 ({source}): {exception.Message}", exception);

        try
        {
            System.Windows.MessageBox.Show(errorMessage, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            // MessageBoxも失敗した場合は何もしない
        }
    }

    #endregion
}

