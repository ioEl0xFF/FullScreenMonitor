using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;
using WinMessageBox = System.Windows.MessageBox;

namespace FullScreenMonitor.ViewModels;

/// <summary>
/// 設定ウィンドウのViewModel
/// 設定の管理とプロセス一覧の制御を行う
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    #region フィールド

    private readonly ILogger _logger;
    private readonly ISettingsManager _settingsManager;
    private readonly IStartupManager _startupManager;
    private readonly IThemeService _themeService;
    private readonly IProcessManagementService _processManagementService;
    private IWindowMonitorService? _monitorService;
    private readonly DispatcherTimer _statusUpdateTimer;

    private ObservableCollection<string> _targetProcesses = new();
    private string _newProcessName = string.Empty;
    private ObservableCollection<ProcessInfo> _runningProcesses = new();
    private int _monitorInterval = 500;
    private bool _startWithWindows = false;
    private bool _restoreOnSettingsClosed = true;
    private bool _restoreOnFullScreenExit = true;
    private bool _restoreOnAppExit = true;
    private int _selectedProcessCount = 0;
    private string _deleteButtonText = "選択項目を削除";
    private bool _isDarkTheme = false;

    #endregion

    #region プロパティ

    /// <summary>
    /// 監視対象プロセス一覧
    /// </summary>
    public ObservableCollection<string> TargetProcesses
    {
        get => _targetProcesses;
        set => SetProperty(ref _targetProcesses, value);
    }

    /// <summary>
    /// 新規プロセス名
    /// </summary>
    public string NewProcessName
    {
        get => _newProcessName;
        set
        {
            if (SetProperty(ref _newProcessName, value))
            {
                // テキスト変更時にボタン強調を制御
                if (!string.IsNullOrWhiteSpace(value))
                {
                    EmphasizeAddButton();
                }
                else
                {
                    ResetAddButton();
                }
            }
        }
    }

    /// <summary>
    /// 実行中プロセス一覧
    /// </summary>
    public ObservableCollection<ProcessInfo> RunningProcesses
    {
        get => _runningProcesses;
        set => SetProperty(ref _runningProcesses, value);
    }

    /// <summary>
    /// 監視間隔（ミリ秒）
    /// </summary>
    public int MonitorInterval
    {
        get => _monitorInterval;
        set => SetProperty(ref _monitorInterval, value);
    }

    /// <summary>
    /// Windows起動時に自動開始
    /// </summary>
    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetProperty(ref _startWithWindows, value);
    }

    /// <summary>
    /// 設定画面を閉じた時に復元
    /// </summary>
    public bool RestoreOnSettingsClosed
    {
        get => _restoreOnSettingsClosed;
        set => SetProperty(ref _restoreOnSettingsClosed, value);
    }

    /// <summary>
    /// 全画面解除時に復元
    /// </summary>
    public bool RestoreOnFullScreenExit
    {
        get => _restoreOnFullScreenExit;
        set => SetProperty(ref _restoreOnFullScreenExit, value);
    }

    /// <summary>
    /// アプリ終了時に復元
    /// </summary>
    public bool RestoreOnAppExit
    {
        get => _restoreOnAppExit;
        set => SetProperty(ref _restoreOnAppExit, value);
    }

    /// <summary>
    /// 選択されたプロセス数
    /// </summary>
    public int SelectedProcessCount
    {
        get => _selectedProcessCount;
        set
        {
            if (SetProperty(ref _selectedProcessCount, value))
            {
                UpdateDeleteButtonText();
            }
        }
    }

    /// <summary>
    /// 削除ボタンのテキスト
    /// </summary>
    public string DeleteButtonText
    {
        get => _deleteButtonText;
        set => SetProperty(ref _deleteButtonText, value);
    }

    /// <summary>
    /// ダークテーマを使用するかどうか
    /// </summary>
    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set
        {
            if (SetProperty(ref _isDarkTheme, value))
            {
                _themeService.SwitchTheme(value);
                SaveThemePreference();
            }
        }
    }

    #endregion

    #region イベント

    /// <summary>
    /// 設定画面を閉じた時のイベント
    /// </summary>
    public event Action? OnSettingsClosed;

    /// <summary>
    /// プロセス追加ボタン強調イベント
    /// </summary>
    public event Action? EmphasizeAddButtonRequested;

    /// <summary>
    /// プロセス追加ボタンリセットイベント
    /// </summary>
    public event Action? ResetAddButtonRequested;

    /// <summary>
    /// ComboBox選択アニメーションイベント
    /// </summary>
    public event Action? ComboBoxSelectionAnimationRequested;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    /// <param name="settingsManager">設定マネージャー</param>
    /// <param name="startupManager">スタートアップマネージャー</param>
    /// <param name="themeService">テーマサービス</param>
    /// <param name="processManagementService">プロセス管理サービス</param>
    /// <param name="monitorService">監視サービス（オプション）</param>
    public SettingsViewModel(
        ILogger logger,
        ISettingsManager settingsManager,
        IStartupManager startupManager,
        IThemeService themeService,
        IProcessManagementService processManagementService,
        IWindowMonitorService? monitorService = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _startupManager = startupManager ?? throw new ArgumentNullException(nameof(startupManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _processManagementService = processManagementService ?? throw new ArgumentNullException(nameof(processManagementService));
        _monitorService = monitorService;

        // 状態更新タイマーを初期化
        _statusUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;

        LoadRunningProcesses();
        LoadThemePreference();
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// 設定を読み込み
    /// </summary>
    /// <param name="settings">設定</param>
    /// <param name="monitorService">監視サービス（オプション）</param>
    public void LoadSettings(AppSettings settings, IWindowMonitorService? monitorService = null)
    {
        try
        {
            TargetProcesses.Clear();
            foreach (var process in settings.TargetProcesses)
            {
                TargetProcesses.Add(process);
            }

            MonitorInterval = settings.MonitorInterval;
            StartWithWindows = _startupManager.IsRegistered();
            RestoreOnSettingsClosed = settings.RestoreOnSettingsClosed;
            RestoreOnFullScreenExit = settings.RestoreOnFullScreenExit;
            RestoreOnAppExit = settings.RestoreOnAppExit;
            IsDarkTheme = settings.UseDarkTheme;

            _monitorService = monitorService;

            // 状態を初期表示
            UpdateApplicationStatus();

            // タイマー開始
            _statusUpdateTimer?.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定読み込みエラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// 設定を取得
    /// </summary>
    /// <returns>現在の設定</returns>
    public AppSettings GetSettings()
    {
        return new AppSettings
        {
            TargetProcesses = TargetProcesses.ToList(),
            MonitorInterval = MonitorInterval,
            StartWithWindows = StartWithWindows,
            RestoreOnSettingsClosed = RestoreOnSettingsClosed,
            RestoreOnFullScreenExit = RestoreOnFullScreenExit,
            RestoreOnAppExit = RestoreOnAppExit,
            UseDarkTheme = IsDarkTheme
        };
    }

    /// <summary>
    /// プロセスを追加
    /// </summary>
    public void AddProcess()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NewProcessName))
            {
                WinMessageBox.Show(ErrorMessages.ProcessNameInputError, "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var processName = _processManagementService.NormalizeProcessName(NewProcessName);

            if (_processManagementService.IsProcessNameDuplicate(processName, TargetProcesses.ToList()))
            {
                WinMessageBox.Show(ErrorMessages.ProcessNameDuplicateError, "重複エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TargetProcesses.Add(processName);
            NewProcessName = string.Empty;

            // 追加後にボタンを元の状態に戻す
            ResetAddButton();

            // 自動保存
            SaveSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError($"プロセス追加エラー: {ex.Message}", ex);
            WinMessageBox.Show($"プロセスの追加中にエラーが発生しました:\n{ex.Message}",
                "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 選択されたプロセスを削除
    /// </summary>
    /// <param name="selectedItems">選択されたアイテム</param>
    public void RemoveSelectedProcesses(System.Collections.IList selectedItems)
    {
        try
        {
            var selectedProcesses = selectedItems.Cast<string>().ToList();

            if (selectedProcesses.Count == 0)
            {
                WinMessageBox.Show(ErrorMessages.ProcessSelectionError, "選択エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 削除確認ダイアログ
            string message;
            if (selectedProcesses.Count == 1)
            {
                message = $"'{selectedProcesses[0]}'を削除しますか？";
            }
            else
            {
                message = $"選択された{selectedProcesses.Count}個のプロセスを削除しますか？\n\n" +
                         string.Join("\n", selectedProcesses.Take(5));
                if (selectedProcesses.Count > 5)
                {
                    message += $"\n... 他{selectedProcesses.Count - 5}個";
                }
            }

            var result = WinMessageBox.Show(message, "削除の確認",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 選択された項目を削除
            foreach (var item in selectedProcesses)
            {
                TargetProcesses.Remove(item);
            }

            // 自動保存
            SaveSettings();
        }
        catch (Exception ex)
        {
            _logger.LogError($"プロセス削除エラー: {ex.Message}", ex);
            WinMessageBox.Show($"プロセスの削除中にエラーが発生しました:\n{ex.Message}",
                "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// プロセス一覧を更新
    /// </summary>
    public void RefreshProcesses()
    {
        LoadRunningProcesses();
    }

    /// <summary>
    /// プロセスを選択
    /// </summary>
    /// <param name="processInfo">選択されたプロセス情報</param>
    public void SelectProcess(ProcessInfo processInfo)
    {
        try
        {
            if (processInfo != null)
            {
                NewProcessName = processInfo.ProcessName;
                
                // 選択時の視覚フィードバック
                TriggerComboBoxSelectionAnimation();
                
                // 追加ボタンを強調
                EmphasizeAddButton();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"プロセス選択エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 設定を保存
    /// </summary>
    public void SaveSettings()
    {
        try
        {
            // 設定の検証
            if (MonitorInterval < MonitorConstants.MinMonitorInterval || MonitorInterval > MonitorConstants.MaxMonitorInterval)
            {
                WinMessageBox.Show(string.Format(ErrorMessages.MonitorIntervalRangeError,
                    MonitorConstants.MinMonitorInterval, MonitorConstants.MaxMonitorInterval), "設定エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // スタートアップ設定の変更を処理
            var originalStartWithWindows = _startupManager.IsRegistered();
            if (StartWithWindows != originalStartWithWindows)
            {
                if (StartWithWindows)
                {
                    if (!_startupManager.Register())
                    {
                        WinMessageBox.Show(ErrorMessages.StartupRegistrationError, "エラー",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    if (!_startupManager.Unregister())
                    {
                        WinMessageBox.Show(ErrorMessages.StartupUnregistrationError, "エラー",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            // 設定を保存
            var settings = GetSettings();
            if (!_settingsManager.SaveSettings(settings))
            {
                WinMessageBox.Show(ErrorMessages.SettingsSaveError, "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _logger.LogInfo("設定を保存しました");
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定保存エラー: {ex.Message}", ex);
            WinMessageBox.Show($"設定保存中にエラーが発生しました:\n{ex.Message}", "エラー",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 設定画面を閉じる
    /// </summary>
    public void CloseSettings()
    {
        try
        {
            // タイマー停止
            _statusUpdateTimer?.Stop();

            // 設定を保存
            SaveSettings();

            // 設定に基づいて復元処理を実行
            if (RestoreOnSettingsClosed)
            {
                OnSettingsClosed?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定画面終了エラー: {ex.Message}", ex);
        }
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// 実行中のプロセスを読み込み
    /// </summary>
    private void LoadRunningProcesses()
    {
        try
        {
            var processes = _processManagementService.GetProcessesWithWindows();
            RunningProcesses.Clear();

            foreach (var process in processes)
            {
                RunningProcesses.Add(process);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorMessages.ProcessListUpdateError, ex);
            WinMessageBox.Show($"{ErrorMessages.ProcessListUpdateError}\n{ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// テーマ設定を保存
    /// </summary>
    private void SaveThemePreference()
    {
        try
        {
            var settings = _settingsManager.LoadSettings();
            settings.UseDarkTheme = IsDarkTheme;
            _settingsManager.SaveSettings(settings);
            _logger.LogInfo($"テーマ設定を保存しました: {(IsDarkTheme ? "ダーク" : "ライト")}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ設定の保存に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// テーマ設定を読み込み
    /// </summary>
    private void LoadThemePreference()
    {
        try
        {
            // 保存されたテーマ設定を読み込み
            var settings = _settingsManager.LoadSettings();
            IsDarkTheme = settings.UseDarkTheme;
            _logger.LogInfo($"テーマ設定を読み込みました: {(IsDarkTheme ? "ダーク" : "ライト")}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ設定の読み込みに失敗しました: {ex.Message}", ex);
            // エラー時はデフォルトのライトテーマを使用
            IsDarkTheme = false;
        }
    }

    /// <summary>
    /// タイマーイベントハンドラー
    /// </summary>
    private void StatusUpdateTimer_Tick(object? sender, EventArgs e)
    {
        UpdateApplicationStatus();
    }

    /// <summary>
    /// アプリケーション状態を更新
    /// </summary>
    private void UpdateApplicationStatus()
    {
        try
        {
            if (_monitorService != null)
            {
                var stats = _monitorService.GetStats();
                // 状態更新は必要に応じて実装
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"状態更新エラー: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// ComboBox選択時のアニメーションをトリガー
    /// </summary>
    private void TriggerComboBoxSelectionAnimation()
    {
        ComboBoxSelectionAnimationRequested?.Invoke();
    }

    /// <summary>
    /// 追加ボタンを強調表示
    /// </summary>
    private void EmphasizeAddButton()
    {
        EmphasizeAddButtonRequested?.Invoke();
    }

    /// <summary>
    /// 追加ボタンを元の状態に戻す
    /// </summary>
    private void ResetAddButton()
    {
        ResetAddButtonRequested?.Invoke();
    }

    /// <summary>
    /// 削除ボタンのテキストを更新
    /// </summary>
    private void UpdateDeleteButtonText()
    {
        if (SelectedProcessCount == 0)
        {
            DeleteButtonText = "選択項目を削除";
        }
        else
        {
            DeleteButtonText = $"選択項目を削除 ({SelectedProcessCount}個選択中)";
        }
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
                _statusUpdateTimer.Stop();
                _statusUpdateTimer.Tick -= StatusUpdateTimer_Tick;
            }
            catch (Exception ex)
            {
                _logger.LogError($"リソース解放エラー: {ex.Message}", ex);
            }
        }
    }

    #endregion
}
