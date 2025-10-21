using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.ViewModels;
using WinMessageBox = System.Windows.MessageBox;

namespace FullScreenMonitor
{
    /// <summary>
    /// SettingsWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        #region フィールド

        private SettingsViewModel? _viewModel;

        #endregion

        #region コンストラクタ

        public SettingsWindow()
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

                // ViewModelを作成
                _viewModel = new SettingsViewModel(
                    App.ServiceContainer.Resolve<ILogger>(),
                    App.ServiceContainer.Resolve<ISettingsManager>(),
                    App.ServiceContainer.Resolve<IStartupManager>(),
                    App.ServiceContainer.Resolve<IThemeService>(),
                    App.ServiceContainer.Resolve<IProcessManagementService>()
                );

                // イベントハンドラーを設定
                _viewModel.OnSettingsClosed += OnSettingsClosed;
                _viewModel.EmphasizeAddButtonRequested += OnEmphasizeAddButtonRequested;
                _viewModel.ResetAddButtonRequested += OnResetAddButtonRequested;
                _viewModel.ComboBoxSelectionAnimationRequested += OnComboBoxSelectionAnimationRequested;

                // ウィンドウクローズイベントを設定
                Closing += Window_Closing;

                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                WinMessageBox.Show($"ViewModelの初期化中にエラーが発生しました:\n{ex.Message}",
                    "初期化エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region イベントハンドラー

        /// <summary>
        /// プロセス追加ボタンクリック
        /// </summary>
        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.AddProcess();
        }

        /// <summary>
        /// プロセス削除ボタンクリック
        /// </summary>
        private void RemoveProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessListBox.SelectedItems != null)
            {
                _viewModel?.RemoveSelectedProcesses(ProcessListBox.SelectedItems);
                ProcessListBox.SelectedItems.Clear();
            }
        }

        /// <summary>
        /// プロセスリスト選択変更
        /// </summary>
        private void ProcessListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel!.SelectedProcessCount = ProcessListBox.SelectedItems.Count;
        }

        /// <summary>
        /// 実行中プロセスComboBox選択変更
        /// </summary>
        private void ProcessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProcessComboBox.SelectedItem is ProcessInfo selectedProcess)
            {
                _viewModel?.SelectProcess(selectedProcess);
            }
        }

        /// <summary>
        /// プロセス一覧更新ボタンクリック
        /// </summary>
        private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.RefreshProcesses();
        }

        /// <summary>
        /// テーマ切り替えボタンクリック
        /// </summary>
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel!.IsDarkTheme = !_viewModel.IsDarkTheme;
        }

        /// <summary>
        /// 閉じるボタンクリック
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _viewModel?.CloseSettings();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// ウィンドウクローズ処理
        /// </summary>
        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _viewModel?.CloseSettings();
                _viewModel?.Dispose();
            }
        catch (Exception ex)
        {
            // ログ出力はViewModelで行われる
            System.Diagnostics.Debug.WriteLine($"SettingsWindow終了処理エラー: {ex.Message}");
        }
        }

        /// <summary>
        /// 設定画面を閉じた時のイベントハンドラー
        /// </summary>
        private void OnSettingsClosed()
        {
            // 必要に応じて追加の処理を実装
        }

        /// <summary>
        /// 追加ボタン強調要求イベントハンドラー
        /// </summary>
        private void OnEmphasizeAddButtonRequested()
        {
            try
            {
                var storyboard = (Storyboard)FindResource("ButtonEmphasisAnimation");
                storyboard.Begin(AddProcessButton);
            }
            catch (Exception ex)
            {
                // アニメーションエラーは無視
                System.Diagnostics.Debug.WriteLine($"追加ボタン強調アニメーションエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// 追加ボタンリセット要求イベントハンドラー
        /// </summary>
        private void OnResetAddButtonRequested()
        {
            try
            {
                var storyboard = (Storyboard)FindResource("ButtonEmphasisResetAnimation");
                storyboard.Begin(AddProcessButton);
            }
            catch (Exception ex)
            {
                // アニメーションエラーは無視
                System.Diagnostics.Debug.WriteLine($"追加ボタンリセットアニメーションエラー: {ex.Message}");
            }
        }

        /// <summary>
        /// ComboBox選択アニメーション要求イベントハンドラー
        /// </summary>
        private void OnComboBoxSelectionAnimationRequested()
        {
            try
            {
                var storyboard = (Storyboard)FindResource("ComboBoxSelectionAnimation");
                storyboard.Begin(ProcessComboBox);

                // 3秒後にアニメーションをリセット
                var resetTimer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                resetTimer.Tick += (s, e) =>
                {
                    resetTimer.Stop();
                    var resetStoryboard = (Storyboard)FindResource("ComboBoxSelectionResetAnimation");
                    resetStoryboard.Begin(ProcessComboBox);
                };
                resetTimer.Start();
            }
            catch (Exception ex)
            {
                // アニメーションエラーは無視
                System.Diagnostics.Debug.WriteLine($"ComboBox選択アニメーションエラー: {ex.Message}");
            }
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public void LoadSettings(Models.AppSettings settings, IWindowMonitorService? monitorService = null)
        {
            _viewModel?.LoadSettings(settings, monitorService);
        }

        /// <summary>
        /// 設定を取得
        /// </summary>
        public Models.AppSettings GetSettings()
        {
            return _viewModel?.GetSettings() ?? Models.AppSettings.GetDefault();
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
    }
}

