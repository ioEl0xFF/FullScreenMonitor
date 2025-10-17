using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using WinFormsApplication = System.Windows.Forms.Application;

namespace FullScreenMonitor;

/// <summary>
/// アプリケーションエントリーポイント
/// </summary>
public partial class App : Application
{
    #region フィールド

    private static readonly Mutex _mutex = new(false, "FullScreenMonitor_SingleInstance");
    private MainWindow? _mainWindow;

    #endregion

    #region アプリケーションライフサイクル

    /// <summary>
    /// アプリケーション起動処理
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        // 多重起動の防止
        if (!_mutex.WaitOne(TimeSpan.Zero, false))
        {
            MessageBox.Show(
                "FullScreenMonitorは既に実行中です。\nシステムトレイを確認してください。",
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
            // MainWindowを非表示で起動
            _mainWindow = new MainWindow();
            _mainWindow.Show();
            _mainWindow.Hide(); // システムトレイ常駐のため非表示

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"アプリケーションの起動中にエラーが発生しました:\n{ex.Message}",
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
            _mainWindow?.Close();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"終了処理エラー: {ex.Message}");
        }
        finally
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
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
        var errorMessage = $"予期しないエラーが発生しました:\n\n" +
                          $"発生元: {source}\n" +
                          $"エラータイプ: {exception.GetType().Name}\n" +
                          $"エラーメッセージ: {exception.Message}\n\n" +
                          $"詳細情報はデバッグ出力を確認してください。";

        System.Diagnostics.Debug.WriteLine($"未処理例外 ({source}): {exception}");

        try
        {
            MessageBox.Show(errorMessage, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            // MessageBoxも失敗した場合は何もしない
        }
    }

    #endregion
}

