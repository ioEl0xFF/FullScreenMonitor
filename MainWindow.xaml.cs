using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;
using MaterialDesignThemes.Wpf;
using WinApplication = System.Windows.Application;

namespace FullScreenMonitor;

/// <summary>
/// メインウィンドウ（システムトレイ常駐）
/// </summary>
public partial class MainWindow : Window
{
    #region フィールド

    private NotifyIcon? _notifyIcon;
    private IWindowMonitorService? _monitorService;
    private AppSettings _currentSettings = AppSettings.GetDefault();
    private readonly object _lockObject = new();
    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private readonly IStartupManager _startupManager;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // サービスコンテナを設定
        var container = new ServiceContainer();
        container.RegisterSingleton<ILogger>(() => new FileLogger());
        container.RegisterSingleton<ISettingsManager>(() => new SettingsManager(container.Resolve<ILogger>()));
        container.RegisterSingleton<IStartupManager>(() => new StartupManager(container.Resolve<ILogger>()));

        _logger = container.Resolve<ILogger>();
        _settingsManager = container.Resolve<ISettingsManager>();
        _startupManager = container.Resolve<IStartupManager>();

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
            _logger.LogInfo("アプリケーションを初期化中...");

            // 設定を読み込み
            LoadSettings();
            
            // テーマ設定を初期化
            InitializeTheme();

            // システムトレイアイコンを初期化
            InitializeNotifyIcon();

            // 監視サービスを初期化
            InitializeMonitorService();

            // ウィンドウクローズイベントを設定
            Closing += Window_Closing;

            _logger.LogInfo("アプリケーションの初期化が完了しました");
        }
        catch (Exception ex)
        {
            _logger.LogError($"アプリケーション初期化エラー: {ex.Message}", ex);
            System.Windows.MessageBox.Show($"アプリケーションの初期化中にエラーが発生しました:\n{ex.Message}\n\n詳細はログファイルを確認してください。",
                "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// システムトレイアイコンの初期化
    /// </summary>
    private void InitializeNotifyIcon()
    {
        Icon customIcon;
        
        try
        {
            // 出力ディレクトリからアイコンファイルを読み込み
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
            _logger.LogInfo($"アイコンファイルパス: {iconPath}");
            
            if (System.IO.File.Exists(iconPath))
            {
                customIcon = new Icon(iconPath);
                _logger.LogInfo("アイコンを正常に読み込みました");
            }
            else
            {
                // アイコンファイルが見つからない場合はWindows標準アイコンを使用
                customIcon = SystemIcons.Application;
                _logger.LogWarning($"アイコンファイルが見つかりません: {iconPath}");
            }
        }
        catch (Exception ex)
        {
            // アイコンの読み込みに失敗した場合はWindows標準アイコンを使用
            customIcon = SystemIcons.Application;
            _logger.LogError($"アイコンの読み込みに失敗しました: {ex.Message}", ex);
        }

        _notifyIcon = new NotifyIcon
        {
            Icon = customIcon,
            Text = AppConstants.SystemTrayTextMonitoring,
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
        _monitorService = new WindowMonitorService(_currentSettings, _logger);

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

            _notifyIcon!.Text = tooltipText;
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
                AppConstants.SystemTrayTextMonitoring :
                AppConstants.SystemTrayTextStopped;
        }
    }

    /// <summary>
    /// ウィンドウ最小化イベント
    /// </summary>
    private void OnWindowsMinimized(object? sender, int count)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
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
            _notifyIcon.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
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
            _notifyIcon.ShowBalloonTip(WindowConstants.BalloonTipErrorDuration, WindowConstants.BalloonTipErrorTitle,
                errorMessage, ToolTipIcon.Error);
        }
    }

    /// <summary>
    /// ウィンドウクローズ処理
    /// </summary>
    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // 設定に基づいて復元処理を実行
        if (_currentSettings.RestoreOnAppExit && _monitorService != null)
        {
            var restoredCount = _monitorService.RestoreWindowsManually();
            if (restoredCount > 0)
            {
                _logger.LogInfo($"アプリ終了時に{restoredCount}個のウィンドウを復元しました");
            }
        }

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
            settingsWindow.LoadSettings(_currentSettings, _monitorService);

            // 設定画面を閉じた時の復元処理を設定
            settingsWindow.OnSettingsClosed += () =>
            {
                if (_monitorService != null)
                {
                    var restoredCount = _monitorService.RestoreWindowsManually();
                    if (restoredCount > 0 && _notifyIcon != null)
                    {
                        _notifyIcon.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
                            $"{restoredCount}個のウィンドウを復元しました。", ToolTipIcon.Info);
                    }
                }
            };

            if (settingsWindow.ShowDialog() == true)
            {
                // 設定ウィンドウ側で既に保存されているので、現在の設定を再読み込み
                LoadSettings();

                // 監視サービスの設定を更新
                _monitorService?.UpdateSettings(_currentSettings);

                if (_notifyIcon != null)
                {
                    _notifyIcon.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
                        "設定を保存しました。", ToolTipIcon.Info);
                }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"設定画面の表示中にエラーが発生しました:\n{ex.Message}",
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
            _currentSettings = _settingsManager.LoadSettings();
        }
    }
    
    /// <summary>
    /// テーマ設定を初期化
    /// </summary>
    private void InitializeTheme()
    {
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_currentSettings.UseDarkTheme ? 
                BaseTheme.Dark : 
                BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            
            _logger.LogInfo($"テーマを初期化しました: {(_currentSettings.UseDarkTheme ? "ダーク" : "ライト")}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ初期化エラー: {ex.Message}", ex);
        }
    }

    #endregion
}