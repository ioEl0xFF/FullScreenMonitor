using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Models;

namespace FullScreenMonitor
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
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

        #endregion

        #region イベント

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region コンストラクタ

        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadRunningProcesses();
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
                System.Windows.MessageBox.Show("プロセス名を入力してください。", "入力エラー",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var processName = NewProcessName.Trim().ToLower();

            if (TargetProcesses.Contains(processName))
            {
                System.Windows.MessageBox.Show("このプロセスは既に追加されています。", "重複エラー",
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
                System.Windows.MessageBox.Show("削除するプロセスを選択してください。", "選択エラー",
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
        /// 保存ボタンクリック
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 設定の検証
                if (TargetProcesses.Count == 0)
                {
                    System.Windows.MessageBox.Show("監視対象プロセスを1つ以上設定してください。", "設定エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MonitorInterval < 100 || MonitorInterval > 2000)
                {
                    System.Windows.MessageBox.Show("監視間隔は100ms〜2000msの範囲で設定してください。", "設定エラー",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // スタートアップ設定の変更を処理
                var originalStartWithWindows = StartupManager.IsRegistered();
                if (StartWithWindows != originalStartWithWindows)
                {
                    if (StartWithWindows)
                    {
                        if (!StartupManager.Register())
                        {
                            System.Windows.MessageBox.Show("スタートアップ登録に失敗しました。", "エラー",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (!StartupManager.Unregister())
                        {
                            System.Windows.MessageBox.Show("スタートアップ解除に失敗しました。", "エラー",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                // 設定を保存
                var settings = GetSettings();
                if (!SettingsManager.SaveSettings(settings))
                {
                    System.Windows.MessageBox.Show("設定の保存に失敗しました。", "エラー",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"設定保存中にエラーが発生しました:\n{ex.Message}", "エラー",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public void LoadSettings(Models.AppSettings settings)
        {
            TargetProcesses.Clear();
            foreach (var process in settings.TargetProcesses)
            {
                TargetProcesses.Add(process);
            }

            MonitorInterval = settings.MonitorInterval;

            // 現在のスタートアップ登録状態を取得
            StartWithWindows = StartupManager.IsRegistered();
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
                StartWithWindows = StartWithWindows
            };
        }

        /// <summary>
        /// ステータスを更新
        /// </summary>
        public void UpdateStatus(string status, string lastCheck)
        {
            StatusTextBlock.Text = status;
            LastCheckTextBlock.Text = $"最終チェック: {lastCheck}";
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
                var processes = ProcessHelper.GetProcessesWithWindows();
                RunningProcesses.Clear();

                foreach (var process in processes)
                {
                    RunningProcesses.Add(process);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"プロセス読み込みエラー: {ex.Message}");
                System.Windows.MessageBox.Show($"実行中プロセスの取得中にエラーが発生しました:\n{ex.Message}",
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

        #endregion
    }
}
