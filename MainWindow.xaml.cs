using System;
using System.Windows;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Services;
using FullScreenMonitor.ViewModels;
using WinMessageBox = System.Windows.MessageBox;
using WinApplication = System.Windows.Application;

namespace FullScreenMonitor;

/// <summary>
/// メインウィンドウ（システムトレイ常駐）
/// </summary>
public partial class MainWindow : Window
{
    #region フィールド

    private MainViewModel? _viewModel;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        InitializeViewModel();
    }

    #endregion

    #region 初期化

    /// <summary>
    /// ViewModelを初期化
    /// </summary>
    private void InitializeViewModel()
    {
        try
        {
            if (App.ServiceContainer == null)
            {
                throw new InvalidOperationException("サービスコンテナが初期化されていません");
            }

            // ViewModelFactoryを使用してViewModelを作成
            _viewModel = ViewModelFactory.CreateMainViewModel(App.ServiceContainer!);

            // イベントハンドラーを設定
            _viewModel.ShowSettingsRequested += OnShowSettingsRequested;
            _viewModel.ExitRequested += OnExitRequested;

            // ウィンドウクローズイベントを設定
            Closing += Window_Closing;

            DataContext = _viewModel;
        }
        catch (Exception ex)
        {
            WinMessageBox.Show($"ViewModelの初期化中にエラーが発生しました:\n{ex.Message}\n\n詳細はログファイルを確認してください。",
                "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region イベントハンドラー

    /// <summary>
    /// 設定画面表示要求イベントハンドラー
    /// </summary>
    private void OnShowSettingsRequested()
    {
        try
        {
            // ViewModelFactoryを使用してSettingsViewModelを作成
            var monitorService = _viewModel!.GetMonitorService();
            
            var settingsViewModel = ViewModelFactory.CreateSettingsViewModelWithSettings(
                App.ServiceContainer!,
                _viewModel.CurrentSettings,
                monitorService
            );

            // 設定画面を閉じた時の復元処理を設定
            settingsViewModel.OnSettingsClosed += () =>
            {
                var restoredCount = _viewModel.RestoreWindowsManually();
                if (restoredCount > 0)
                {
                    // バルーンチップ表示はViewModelで処理
                }
            };

            // SettingsWindowをViewModelと共に作成
            var settingsWindow = new SettingsWindow(settingsViewModel);

            if (settingsWindow.ShowDialog() == true)
            {
                // 設定を更新
                _viewModel.UpdateSettings(settingsViewModel.GetSettings());
            }
        }
        catch (Exception ex)
        {
            WinMessageBox.Show($"設定画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// アプリケーション終了要求イベントハンドラー
    /// </summary>
    private void OnExitRequested()
    {
        WinApplication.Current.Shutdown();
    }

    /// <summary>
    /// ウィンドウクローズ処理
    /// </summary>
    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        try
        {
            _viewModel?.Dispose();
        }
        catch (Exception ex)
        {
            // ログ出力はViewModelで行われる
            System.Diagnostics.Debug.WriteLine($"MainWindow終了処理エラー: {ex.Message}");
        }
    }

    #endregion
}