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

namespace FullScreenMonitor
{
    /// <summary>
    /// プロセス表示用アイテムクラス
    /// </summary>
    public class TargetProcessItem : INotifyPropertyChanged
    {
        private string _processName = string.Empty;
        private bool _isRunning = false;
        private string _displayName = string.Empty;

        public string ProcessName
        {
            get => _processName;
            set
            {
                _processName = value;
                OnPropertyChanged(nameof(ProcessName));
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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

        #endregion

        #region プロパティ

        private ObservableCollection<TargetProcessItem> _targetProcesses = new();
        public ObservableCollection<TargetProcessItem> TargetProcesses
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

            // 状態更新タイマーを初期化
            _statusUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _statusUpdateTimer.Tick += StatusUpdateTimer_Tick;

            // キーボードショートカットの設定
            KeyDown += SettingsWindow_KeyDown;
        }

        #endregion

        #region イベントハンドラー

        /// <summary>
        /// プロセス名を正規化する
        /// </summary>
        private string NormalizeProcessName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
                return string.Empty;

            // .exeを除去し、小文字に変換
            var normalized = processName.Trim().ToLower();
            if (normalized.EndsWith(".exe"))
            {
                normalized = normalized.Substring(0, normalized.Length - 4);
            }

            return normalized;
        }

        /// <summary>
        /// プロセス名の入力バリデーション
        /// </summary>
        private string? ValidateProcessName(string processName)
        {
            if (string.IsNullOrWhiteSpace(processName))
            {
                return "プロセス名を入力してください";
            }

            var normalized = NormalizeProcessName(processName);
            if (string.IsNullOrEmpty(normalized))
            {
                return "有効なプロセス名を入力してください";
            }

            // 特殊文字のチェック
            if (normalized.Any(c => !char.IsLetterOrDigit(c) && c != '.' && c != '-' && c != '_'))
            {
                return "プロセス名に使用できない文字が含まれています";
            }

            // 長さのチェック
            if (normalized.Length > 50)
            {
                return "プロセス名が長すぎます（50文字以内）";
            }

            return null; // バリデーション成功
        }

        /// <summary>
        /// プロセス追加ボタンクリック
        /// </summary>
        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            // 入力バリデーション
            var validationError = ValidateProcessName(NewProcessName);
            if (validationError != null)
            {
                System.Windows.MessageBox.Show(validationError, "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var normalizedProcessName = NormalizeProcessName(NewProcessName);

            // 重複チェック
            if (TargetProcesses.Any(p => p.ProcessName == normalizedProcessName))
            {
                System.Windows.MessageBox.Show(ErrorMessages.ProcessNameDuplicateError, "重複エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 新しいプロセスアイテムを作成
            var processItem = new TargetProcessItem
            {
                ProcessName = normalizedProcessName,
                DisplayName = normalizedProcessName,
                IsRunning = IsProcessRunning(normalizedProcessName)
            };

            TargetProcesses.Add(processItem);
            NewProcessName = string.Empty;

            // 自動保存
            SaveSettings();
        }

        /// <summary>
        /// 個別プロセス削除ボタンクリック
        /// </summary>
        private void RemoveIndividualProcess_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.DataContext is TargetProcessItem processItem)
            {
                TargetProcesses.Remove(processItem);
                SaveSettings();
            }
        }

        /// <summary>
        /// プロセス削除ボタンクリック（一括削除）
        /// </summary>
        private void RemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ProcessListBox.SelectedItems.Cast<TargetProcessItem>().ToList();

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
                message = $"'{selectedItems[0].ProcessName}'を削除しますか？";
            }
            else
            {
                message = $"選択された{selectedItems.Count}個のプロセスを削除しますか？\n\n" +
                         string.Join("\n", selectedItems.Take(5).Select(p => p.ProcessName));
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
        /// キーボードショートカット処理
        /// </summary>
        private void SettingsWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Enter:
                    // Enterキーでプロセス追加
                    if (ProcessComboBox.IsFocused || !string.IsNullOrWhiteSpace(NewProcessName))
                    {
                        AddProcess_Click(sender, new RoutedEventArgs());
                        e.Handled = true;
                    }
                    break;

                case System.Windows.Input.Key.Delete:
                    // Deleteキーで選択プロセス削除
                    if (ProcessListBox.IsFocused && ProcessListBox.SelectedItems.Count > 0)
                    {
                        RemoveProcess_Click(sender, new RoutedEventArgs());
                        e.Handled = true;
                    }
                    break;

                case System.Windows.Input.Key.Escape:
                    // Escキーでウィンドウを閉じる
                    Close_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                    break;
            }
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
                var processItem = new TargetProcessItem
                {
                    ProcessName = process,
                    DisplayName = process,
                    IsRunning = IsProcessRunning(process)
                };
                TargetProcesses.Add(processItem);
            }

            MonitorInterval = settings.MonitorInterval;

            // 現在のスタートアップ登録状態を取得
            StartWithWindows = _startupManager.IsRegistered();

            // 復元設定を読み込み
            RestoreOnSettingsClosed = settings.RestoreOnSettingsClosed;
            RestoreOnAppExit = settings.RestoreOnAppExit;

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
                TargetProcesses = TargetProcesses.Select(p => p.ProcessName).ToList(),
                MonitorInterval = MonitorInterval,
                StartWithWindows = StartWithWindows,
                RestoreOnSettingsClosed = RestoreOnSettingsClosed,
                RestoreOnAppExit = RestoreOnAppExit
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
        /// プロセスが実行中かどうかをチェック
        /// </summary>
        private bool IsProcessRunning(string processName)
        {
            try
            {
                var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// プロセスの実行状態を更新
        /// </summary>
        private void UpdateProcessRunningStates()
        {
            foreach (var processItem in TargetProcesses)
            {
                processItem.IsRunning = IsProcessRunning(processItem.ProcessName);
            }
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

            // プロセスの実行状態を更新
            UpdateProcessRunningStates();
        }

        #endregion
    }
}
