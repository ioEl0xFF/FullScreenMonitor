using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

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
                MessageBox.Show("プロセス名を入力してください。", "入力エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var processName = NewProcessName.Trim().ToLower();
            
            if (TargetProcesses.Contains(processName))
            {
                MessageBox.Show("このプロセスは既に追加されています。", "重複エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TargetProcesses.Add(processName);
            NewProcessName = string.Empty;
        }

        /// <summary>
        /// プロセス削除ボタンクリック
        /// </summary>
        private void RemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItem is string selectedProcess)
            {
                TargetProcesses.Remove(selectedProcess);
            }
            else
            {
                MessageBox.Show("削除するプロセスを選択してください。", "選択エラー", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 保存ボタンクリック
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 設定を保存する処理は後で実装
            DialogResult = true;
            Close();
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
            StartWithWindows = settings.StartWithWindows;
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
        /// プロパティ変更通知
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
