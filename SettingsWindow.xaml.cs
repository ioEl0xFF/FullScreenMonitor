using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;
using MaterialDesignThemes.Wpf;

namespace FullScreenMonitor
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        #region フィールド

        private readonly ILogger _logger;
        private readonly ISettingsManager _settingsManager;
        private readonly IStartupManager _startupManager;
        private readonly ProcessHelper _processHelper;
        private DispatcherTimer? _statusUpdateTimer;
        private IWindowMonitorService? _monitorService;
        private bool _isDarkTheme = false;

        #endregion

        #region プロパティ

        private ObservableCollection<string> _targetProcesses = new();
        public ObservableCollection<string> TargetProcesses
        {
            get => _targetProcesses;
            set
            {
                _targetProcesses = value;
                OnPropertyChanged(nameof(TargetProcesses));
            }
        }

        private string _newProcessName = string.Empty;
        public string NewProcessName
        {
            get => _newProcessName;
            set
            {
                _newProcessName = value;
                OnPropertyChanged(nameof(NewProcessName));
            }
        }

        private ObservableCollection<ProcessInfo> _runningProcesses = new();
        public ObservableCollection<ProcessInfo> RunningProcesses
        {
            get => _runningProcesses;
            set
            {
                _runningProcesses = value;
                OnPropertyChanged(nameof(RunningProcesses));
            }
        }

        private int _monitorInterval = 500;
        public int MonitorInterval
        {
            get => _monitorInterval;
            set
            {
                _monitorInterval = value;
                OnPropertyChanged(nameof(MonitorInterval));
            }
        }

        private bool _startWithWindows = false;
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                _startWithWindows = value;
                OnPropertyChanged(nameof(StartWithWindows));
            }
        }

        private bool _restoreOnSettingsClosed = true;
        public bool RestoreOnSettingsClosed
        {
            get => _restoreOnSettingsClosed;
            set
            {
                _restoreOnSettingsClosed = value;
                OnPropertyChanged(nameof(RestoreOnSettingsClosed));
            }
        }

        private bool _restoreOnAppExit = true;
        public bool RestoreOnAppExit
        {
            get => _restoreOnAppExit;
            set
            {
                _restoreOnAppExit = value;
                OnPropertyChanged(nameof(RestoreOnAppExit));
            }
        }

        #endregion

        #region イベント

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? OnSettingsClosed;

        #endregion

        #region コンストラクタ

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;

            // サービスを初期化
            _logger = new FileLogger();
            _settingsManager = new SettingsManager(_logger);
            _startupManager = new StartupManager(_logger);
            _processHelper = new ProcessHelper(_logger);

            LoadRunningProcesses();
            Closing += Window_Closing;

            // テーマ設定を読み込み
            LoadThemePreference();

            // 状態更新タイマーを初期化
            _statusUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;
        }

        #endregion

        #region イベントハンドラー

        /// <summary>
        /// プロセス追加ボタンクリック
        /// </summary>
        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewProcessName))
            {
                System.Windows.MessageBox.Show(ErrorMessages.ProcessNameInputError, "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var processName = NewProcessName.Trim().ToLower();

            if (TargetProcesses.Contains(processName))
            {
                System.Windows.MessageBox.Show(ErrorMessages.ProcessNameDuplicateError, "重複エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TargetProcesses.Add(processName);
            NewProcessName = string.Empty;

            // 自動保存
            SaveSettings();
        }

        /// <summary>
        /// プロセス削除ボタンクリック
        /// </summary>
        private void RemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ProcessListBox.SelectedItems.Cast<string>().ToList();

            if (selectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show(ErrorMessages.ProcessSelectionError, "選択エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 削除確認ダイアログ
            string message;
            if (selectedItems.Count == 1)
            {
                message = $"'{selectedItems[0]}'を削除しますか？";
            }
            else
            {
                message = $"選択された{selectedItems.Count}個のプロセスを削除しますか？\n\n" +
                         string.Join("\n", selectedItems.Take(5));
                if (selectedItems.Count > 5)
                {
                    message += $"\n... 他{selectedItems.Count - 5}個";
                }
            }

            var result = System.Windows.MessageBox.Show(message, "削除の確認",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            // 選択された項目を削除
            foreach (var item in selectedItems)
            {
                TargetProcesses.Remove(item);
            }

            // 選択状態をクリア
            ProcessListBox.SelectedItems.Clear();

            // 自動保存
            SaveSettings();
        }

        /// <summary>
        /// 実行中プロセスComboBox選択変更
        /// </summary>
        private void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessComboBox.SelectedItem is ProcessInfo selectedProcess)
            {
                NewProcessName = selectedProcess.ProcessName;
            }
        }

        /// <summary>
        /// プロセス一覧更新ボタンクリック
        /// </summary>
        private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
        {
            LoadRunningProcesses();
        }

        /// <summary>
        /// テーマ切り替えボタンクリック
        /// </summary>
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
            
            // タイトルバーの色を更新
            UpdateTitleBarColor();
            
            // テーマ設定を永続化
            SaveThemePreference();
        }

        /// <summary>
        /// テーマ設定を保存
        /// </summary>
        private void SaveThemePreference()
        {
            try
            {
                var settings = _settingsManager.LoadSettings();
                settings.UseDarkTheme = _isDarkTheme;
                _settingsManager.SaveSettings(settings);
                _logger.LogInfo($"テーマ設定を保存しました: {(_isDarkTheme ? "ダーク" : "ライト")}");
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
                _isDarkTheme = settings.UseDarkTheme;
                
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();
                theme.SetBaseTheme(_isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
                paletteHelper.SetTheme(theme);
                
                // タイトルバーの色を初期設定
                UpdateTitleBarColor();
                
                _logger.LogInfo($"テーマ設定を読み込みました: {(_isDarkTheme ? "ダーク" : "ライト")}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"テーマ設定の読み込みに失敗しました: {ex.Message}", ex);
                // エラー時はデフォルトのライトテーマを使用
                _isDarkTheme = false;
                var paletteHelper = new PaletteHelper();
                var theme = paletteHelper.GetTheme();
                theme.SetBaseTheme(BaseTheme.Light);
                paletteHelper.SetTheme(theme);
            }
        }
        
        /// <summary>
        /// タイトルバーの色を更新
        /// </summary>
        private void UpdateTitleBarColor()
        {
            try
            {
                // Windows APIを使用してタイトルバーの色を変更
                var titleBarColor = _isDarkTheme ? 
                    System.Drawing.ColorTranslator.FromHtml("#1976D2") : // ダークテーマ用の濃い青
                    System.Drawing.ColorTranslator.FromHtml("#2196F3");  // ライトテーマ用の明るい青
                
                // DwmSetWindowAttributeを使用してタイトルバーの色を設定
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var color = (uint)((titleBarColor.B << 16) | (titleBarColor.G << 8) | titleBarColor.R);
                
                // DWMWA_CAPTION_COLOR を使用
                NativeMethods.DwmSetWindowAttribute(hwnd, NativeMethods.DWMWA_CAPTION_COLOR, ref color, sizeof(uint));
            }
            catch (Exception ex)
            {
                _logger.LogError($"タイトルバーの色更新に失敗しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // 設定を保存してから閉じる
            SaveSettings();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// ウィンドウクローズ処理
        /// </summary>
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // タイマー停止
            _statusUpdateTimer?.Stop();

            // DialogResultが設定されていない場合（Xボタンなどで閉じられた場合）は設定を保存
            if (DialogResult != true)
            {
                SaveSettings();
                DialogResult = true;
            }

            // 設定に基づいて復元処理を実行
            if (RestoreOnSettingsClosed)
            {
                OnSettingsClosed?.Invoke();
            }
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public void LoadSettings(Models.AppSettings settings, IWindowMonitorService? monitorService = null)
        {
            TargetProcesses.Clear();
            foreach (var process in settings.TargetProcesses)
            {
                TargetProcesses.Add(process);
            }

            MonitorInterval = settings.MonitorInterval;

            // 現在のスタートアップ登録状態を取得
            StartWithWindows = _startupManager.IsRegistered();

            // 復元設定を読み込み
            RestoreOnSettingsClosed = settings.RestoreOnSettingsClosed;
            RestoreOnAppExit = settings.RestoreOnAppExit;
            
            // テーマ設定を読み込み
            _isDarkTheme = settings.UseDarkTheme;

            // 監視サービスを設定
            _monitorService = monitorService;

            // デバッグ用：初期表示を強制実行
            UpdateStatus("初期化中...", "--", 0, 0);

            // 状態を初期表示
            UpdateApplicationStatus();

            // タイマー開始
            _statusUpdateTimer?.Start();
        }

        /// <summary>
        /// 設定を取得
        /// </summary>
        public Models.AppSettings GetSettings()
        {
            return new Models.AppSettings
            {
                TargetProcesses = TargetProcesses.ToList(),
                MonitorInterval = MonitorInterval,
                StartWithWindows = StartWithWindows,
                RestoreOnSettingsClosed = RestoreOnSettingsClosed,
                RestoreOnAppExit = RestoreOnAppExit,
                UseDarkTheme = _isDarkTheme
            };
        }

        /// <summary>
        /// ステータスを更新
        /// </summary>
        public void UpdateStatus(string status, string lastCheck, int minimizedCount, int targetProcessCount)
        {
            try
            {
                StatusTextBlock.Text = status;
                LastCheckTextBlock.Text = $"最終チェック: {lastCheck}";
                MinimizedWindowCountTextBlock.Text = $"最小化ウィンドウ: {minimizedCount}個";
                TargetProcessCountTextBlock.Text = $"対象プロセス: {targetProcessCount}個";

                // デバッグ用：コンソールに出力
                System.Diagnostics.Debug.WriteLine($"UpdateStatus called: {status}, {lastCheck}, {minimizedCount}, {targetProcessCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateStatus error: {ex.Message}");
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 設定を保存
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                // 設定の検証
                if (MonitorInterval < MonitorConstants.MinMonitorInterval || MonitorInterval > MonitorConstants.MaxMonitorInterval)
                {
                    System.Windows.MessageBox.Show(string.Format(ErrorMessages.MonitorIntervalRangeError,
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
                            System.Windows.MessageBox.Show(ErrorMessages.StartupRegistrationError, "エラー",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (!_startupManager.Unregister())
                        {
                            System.Windows.MessageBox.Show(ErrorMessages.StartupUnregistrationError, "エラー",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // 設定を保存
                var settings = GetSettings();
                if (!_settingsManager.SaveSettings(settings))
                {
                    System.Windows.MessageBox.Show(ErrorMessages.SettingsSaveError, "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"設定保存中にエラーが発生しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 実行中のプロセスを読み込み
        /// </summary>
        private void LoadRunningProcesses()
        {
            try
            {
                var processes = _processHelper.GetProcessesWithWindows();
                RunningProcesses.Clear();

                foreach (var process in processes)
                {
                    RunningProcesses.Add(process);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.ProcessListUpdateError, ex);
                System.Windows.MessageBox.Show($"{ErrorMessages.ProcessListUpdateError}\n{ex.Message}",
                    "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            if (_monitorService != null)
            {
                var stats = _monitorService.GetStats();
                var status = stats.IsMonitoring ? "監視中" : "停止中";
                var lastCheck = stats.LastCheckTime != DateTime.MinValue
                    ? stats.LastCheckTime.ToString("HH:mm:ss")
                    : "--";
                UpdateStatus(status, lastCheck, stats.MinimizedWindowCount, stats.TargetProcessCount);
            }
            else
            {
                // デバッグ用：監視サービスがnullの場合の表示
                UpdateStatus("監視サービス未接続", "--", 0, 0);
            }
        }

        #endregion
    }
}
