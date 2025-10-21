using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;
using WinMessageBox = System.Windows.MessageBox;

namespace FullScreenMonitor.ViewModels;

/// <summary>
/// メインウィンドウのViewModel
/// システムトレイ管理と監視サービスの制御を行う
/// </summary>
public class MainViewModel : ViewModelBase
{
    #region フィールド

    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private readonly IStartupManager _startupManager;
    private readonly IThemeService _themeService;
    private readonly INotifyIconService _notifyIconService;
    private readonly IWindowMonitorService _monitorService;
    private readonly object _lockObject = new();

    private AppSettings _currentSettings = AppSettings.GetDefault();
    private bool _isMonitoring;

    #endregion

    #region プロパティ

    /// <summary>
    /// 監視中かどうか
    /// </summary>
    public bool IsMonitoring
    {
        get => _isMonitoring;
        private set => SetProperty(ref _isMonitoring, value);
    }

    /// <summary>
    /// 現在の設定
    /// </summary>
    public AppSettings CurrentSettings
    {
        get => _currentSettings;
        private set => SetProperty(ref _currentSettings, value);
    }

    #endregion

    #region イベント

    /// <summary>
    /// 設定画面表示要求イベント
    /// </summary>
    public event Action? ShowSettingsRequested;

    /// <summary>
    /// アプリケーション終了要求イベント
    /// </summary>
    public event Action? ExitRequested;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settingsManager">設定マネージャー</param>
    /// <param name="startupManager">スタートアップマネージャー</param>
    /// <param name="themeService">テーマサービス</param>
    /// <param name="notifyIconService">通知アイコンサービス</param>
    /// <param name="monitorService">監視サービス</param>
    public MainViewModel(
        ILogger logger,
        ISettingsManager settingsManager,
        IStartupManager startupManager,
        IThemeService themeService,
        INotifyIconService notifyIconService,
        IWindowMonitorService monitorService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _notifyIconService = notifyIconService ?? throw new ArgumentNullException(nameof(notifyIconService));
        _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));

        InitializeApplication();
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// アプリケーションを初期化
    /// </summary>
    public void InitializeApplication()
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

            _logger.LogInfo("アプリケーションの初期化が完了しました");
        }
        catch (Exception ex)
        {
            _logger.LogError($"アプリケーション初期化エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// 設定画面を表示
    /// </summary>
    public void ShowSettings()
    {
        try
        {
            ShowSettingsRequested?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定画面表示エラー: {ex.Message}", ex);
            WinMessageBox.Show($"設定画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// アプリケーションを終了
    /// </summary>
    public void ExitApplication()
    {
        try
        {
            ExitRequested?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError($"アプリケーション終了エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 設定を更新
    /// </summary>
    /// <param name="newSettings">新しい設定</param>
    public void UpdateSettings(AppSettings newSettings)
    {
        lock (_lockObject)
        {
            CurrentSettings = newSettings;
            _monitorService.UpdateSettings(newSettings);
        }
    }

    /// <summary>
    /// 監視統計情報を取得
    /// </summary>
    /// <returns>監視統計情報</returns>
    public MonitoringStats GetStats()
    {
        return _monitorService.GetStats();
    }

    /// <summary>
    /// 手動でウィンドウを復元
    /// </summary>
    /// <returns>復元したウィンドウ数</returns>
    public int RestoreWindowsManually()
    {
        return _monitorService.RestoreWindowsManually();
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
            CurrentSettings = _settingsManager.LoadSettings();
        }
    }

    /// <summary>
    /// テーマ設定を初期化
    /// </summary>
    private void InitializeTheme()
    {
        try
        {
            _themeService.InitializeTheme(CurrentSettings.UseDarkTheme);
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ初期化エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// システムトレイアイコンの初期化
    /// </summary>
    private void InitializeNotifyIcon()
    {
        try
        {
            _notifyIconService.Initialize();

            // コンテキストメニューを作成
            var contextMenu = new ContextMenuStrip();

            var showSettingsItem = new ToolStripMenuItem("設定を開く");
            showSettingsItem.Click += (s, e) => ShowSettings();
            contextMenu.Items.Add(showSettingsItem);

            var separatorItem = new ToolStripSeparator();
            contextMenu.Items.Add(separatorItem);

            var exitItem = new ToolStripMenuItem("終了");
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);

            _notifyIconService.SetContextMenu(contextMenu);

            // イベントハンドラーを設定
            _notifyIconService.DoubleClick += (s, e) => ShowSettings();
            _notifyIconService.MouseMove += OnNotifyIconMouseMove;
        }
        catch (Exception ex)
        {
            _logger.LogError($"システムトレイアイコン初期化エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// 監視サービスの初期化
    /// </summary>
    private void InitializeMonitorService()
    {
        try
        {
            // イベントハンドラーを設定
            _monitorService.MonitoringStateChanged += OnMonitoringStateChanged;
            _monitorService.WindowsMinimized += OnWindowsMinimized;
            _monitorService.WindowsRestored += OnWindowsRestored;
            _monitorService.ErrorOccurred += OnErrorOccurred;

            // 監視を開始
            _monitorService.StartMonitoring();
        }
        catch (Exception ex)
        {
            _logger.LogError($"監視サービス初期化エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// トレイアイコンマウス移動イベントハンドラー
    /// </summary>
    private void OnNotifyIconMouseMove(object? sender, MouseEventArgs e)
    {
        try
        {
            var stats = GetStats();
            var tooltipText = $"FullScreenMonitor\n" +
                            $"状態: {(stats.IsMonitoring ? "監視中" : "停止中")}\n" +
                            $"対象プロセス: {stats.TargetProcessCount}個\n" +
                            $"最小化ウィンドウ: {stats.MinimizedWindowCount}個\n" +
                            $"最終チェック: {stats.LastCheckTime:HH:mm:ss}";

            _notifyIconService.Text = tooltipText;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ツールチップ更新エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 監視状態変更イベントハンドラー
    /// </summary>
    private void OnMonitoringStateChanged(object? sender, bool isMonitoring)
    {
        IsMonitoring = isMonitoring;
        _notifyIconService.Text = isMonitoring ?
            AppConstants.SystemTrayTextMonitoring :
            AppConstants.SystemTrayTextStopped;
    }

    /// <summary>
    /// ウィンドウ最小化イベントハンドラー
    /// </summary>
    private void OnWindowsMinimized(object? sender, int count)
    {
        _notifyIconService.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
            $"{count}個のウィンドウを最小化しました。", ToolTipIcon.Info);
    }

    /// <summary>
    /// ウィンドウ復元イベントハンドラー
    /// </summary>
    private void OnWindowsRestored(object? sender, int count)
    {
        _notifyIconService.ShowBalloonTip(WindowConstants.BalloonTipInfoDuration, WindowConstants.BalloonTipTitle,
            $"{count}個のウィンドウを復元しました。", ToolTipIcon.Info);
    }

    /// <summary>
    /// エラー発生イベントハンドラー
    /// </summary>
    private void OnErrorOccurred(object? sender, string errorMessage)
    {
        _notifyIconService.ShowBalloonTip(WindowConstants.BalloonTipErrorDuration, WindowConstants.BalloonTipErrorTitle,
            errorMessage, ToolTipIcon.Error);
    }

    #endregion

    #region IDisposable

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
        if (disposing)
        {
            try
            {
                // 設定に基づいて復元処理を実行
                if (CurrentSettings.RestoreOnAppExit)
                {
                    var restoredCount = RestoreWindowsManually();
                    if (restoredCount > 0)
                    {
                        _logger.LogInfo($"アプリ終了時に{restoredCount}個のウィンドウを復元しました");
                    }
                }

                // イベントハンドラーを解除
                if (_monitorService != null)
                {
                    _monitorService.MonitoringStateChanged -= OnMonitoringStateChanged;
                    _monitorService.WindowsMinimized -= OnWindowsMinimized;
                    _monitorService.WindowsRestored -= OnWindowsRestored;
                    _monitorService.ErrorOccurred -= OnErrorOccurred;
                }

                // 監視サービスを停止
                _monitorService?.StopMonitoring();
                _monitorService?.Dispose();

                // システムトレイアイコンを非表示
                _notifyIconService.Visible = false;
                _notifyIconService.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError($"リソース解放エラー: {ex.Message}", ex);
            }
        }
    }

    #endregion
}
