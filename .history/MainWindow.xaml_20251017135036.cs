using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;
using WinApplication = System.Windows.Application;

namespace FullScreenMonitor;

/// <summary>
/// メインウィンドウ（システムトレイ常駐）
/// </summary>
public partial class MainWindow : Window
{
    #region フィールド

    private NotifyIcon? _notifyIcon;
    private WindowMonitorService? _monitorService;
    private AppSettings _currentSettings = AppSettings.GetDefault();
    private readonly object _lockObject = new();

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        InitializeApplication();
    }

    #endregion

    #region 初期化

    /// <summary>
    /// アプリケーションの初期化
    /// </summary>
    private void InitializeApplication()
    {
        try
        {
            // 設定を読み込み
            LoadSettings();

            // システムトレイアイコンを初期化
            InitializeNotifyIcon();

            // 監視サービスを初期化
            InitializeMonitorService();

            // ウィンドウクローズイベントを設定
            Closing += Window_Closing;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"初期化エラー: {ex.Message}");
            System.Windows.MessageBox.Show($"アプリケーションの初期化中にエラーが発生しました:\n{ex.Message}", 
                "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// システムトレイアイコンの初期化
    /// </summary>
    private void InitializeNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application, // Windows標準アイコンを使用
            Text = "FullScreenMonitor - 全画面監視中",
            Visible = true
        };

        // コンテキストメニューを作成
        var contextMenu = new ContextMenuStrip();
        
        var showSettingsItem = new ToolStripMenuItem("設定を開く");
        showSettingsItem.Click += ShowSettings_Click;
        contextMenu.Items.Add(showSettingsItem);

        var separatorItem = new ToolStripSeparator();
        contextMenu.Items.Add(separatorItem);

        var exitItem = new ToolStripMenuItem("終了");
        exitItem.Click += Exit_Click;
        contextMenu.Items.Add(exitItem);

        _notifyIcon.ContextMenuStrip = contextMenu;

        // ダブルクリックイベント
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        // マウス移動時のツールチップ更新
        _notifyIcon.MouseMove += NotifyIcon_MouseMove;
    }

    /// <summary>
    /// 監視サービスの初期化
    /// </summary>
    private void InitializeMonitorService()
    {
        _monitorService = new WindowMonitorService(_currentSettings);
        
        // イベントハンドラーを設定
        _monitorService.MonitoringStateChanged += OnMonitoringStateChanged;
        _monitorService.WindowsMinimized += OnWindowsMinimized;
        _monitorService.WindowsRestored += OnWindowsRestored;
        _monitorService.ErrorOccurred += OnErrorOccurred;

        // 監視を開始
        _monitorService.StartMonitoring();
    }

    #endregion

    #region イベントハンドラー

    /// <summary>
    /// システムトレイアイコンダブルクリック
    /// </summary>
    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    /// <summary>
    /// 設定を開くメニュークリック
    /// </summary>
    private void ShowSettings_Click(object? sender, EventArgs e)
    {
        ShowSettingsWindow();
    }

    /// <summary>
    /// 終了メニュークリック
    /// </summary>
    private void Exit_Click(object? sender, EventArgs e)
    {
        WinApplication.Current.Shutdown();
    }

    /// <summary>
    /// トレイアイコンマウス移動
    /// </summary>
    private void NotifyIcon_MouseMove(object? sender, MouseEventArgs e)
    {
        if (_monitorService != null)
        {
            var stats = _monitorService.GetStats();
            var tooltipText = $"FullScreenMonitor\n" +
                            $"状態: {(stats.IsMonitoring ? "監視中" : "停止中")}\n" +
                            $"対象プロセス: {stats.TargetProcessCount}個\n" +
                            $"最小化ウィンドウ: {stats.MinimizedWindowCount}個\n" +
                            $"最終チェック: {stats.LastCheckTime:HH:mm:ss}";
            
            _notifyIcon.Text = tooltipText;
        }
    }

    /// <summary>
    /// 監視状態変更イベント
    /// </summary>
    private void OnMonitoringStateChanged(object? sender, bool isMonitoring)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Text = isMonitoring ? 
                "FullScreenMonitor - 監視中" : 
                "FullScreenMonitor - 停止中";
        }
    }

    /// <summary>
    /// ウィンドウ最小化イベント
    /// </summary>
    private void OnWindowsMinimized(object? sender, int count)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(3000, "FullScreenMonitor", 
                $"{count}個のウィンドウを最小化しました。", ToolTipIcon.Info);
        }
    }

    /// <summary>
    /// ウィンドウ復元イベント
    /// </summary>
    private void OnWindowsRestored(object? sender, int count)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(3000, "FullScreenMonitor", 
                $"{count}個のウィンドウを復元しました。", ToolTipIcon.Info);
        }
    }

    /// <summary>
    /// エラー発生イベント
    /// </summary>
    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(5000, "FullScreenMonitor - エラー", 
                errorMessage, ToolTipIcon.Error);
        }
    }

    /// <summary>
    /// ウィンドウクローズ処理
    /// </summary>
    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // システムトレイから非表示にする
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        // 監視サービスを停止
        _monitorService?.StopMonitoring();
        _monitorService?.Dispose();
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// 設定画面を表示
    /// </summary>
    public void ShowSettingsWindow()
    {
        try
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.LoadSettings(_currentSettings);

            var originalStartWithWindows = _currentSettings.StartWithWindows;

            if (settingsWindow.ShowDialog() == true)
            {
                var newSettings = settingsWindow.GetSettings();
                SaveSettings(newSettings);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定画面の表示中にエラーが発生しました:\n{ex.Message}", 
                "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// 設定を読み込み
    /// </summary>
    private void LoadSettings()
    {
        lock (_lockObject)
        {
            _currentSettings = SettingsManager.LoadSettings();
        }
    }

    /// <summary>
    /// 設定を保存
    /// </summary>
    private void SaveSettings(AppSettings settings)
    {
        lock (_lockObject)
        {
            var originalStartWithWindows = _currentSettings.StartWithWindows;
            
            // 設定を保存
            if (SettingsManager.SaveSettings(settings))
            {
                _currentSettings = settings;

                // スタートアップ設定の変更を反映
                if (settings.StartWithWindows != originalStartWithWindows)
                {
                    if (settings.StartWithWindows)
                    {
                        StartupManager.Register();
                    }
                    else
                    {
                        StartupManager.Unregister();
                    }
                }

                // 監視サービスの設定を更新
                _monitorService?.UpdateSettings(settings);

                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(3000, "FullScreenMonitor", 
                        "設定を保存しました。", ToolTipIcon.Info);
                }
            }
            else
            {
                MessageBox.Show("設定の保存に失敗しました。", "エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #endregion
}