using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using FullScreenMonitor.Constants;

namespace FullScreenMonitor.Models
{
    /// <summary>
    /// アプリケーション設定データモデル
    /// </summary>
    public class AppSettings : INotifyPropertyChanged
    {
        #region イベント

        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region プライベートフィールド

        private List<string> _targetProcesses = new();
        private int _monitorInterval = 500;
        private bool _startWithWindows = false;
        private bool _restoreOnSettingsClosed = true;
        private bool _restoreOnAppExit = true;
        private bool _restoreOnFullScreenExit = true;
        private bool _useDarkTheme = false;

        #endregion

        #region パブリックプロパティ

        /// <summary>
        /// 監視対象プロセス名のリスト
        /// </summary>
        [JsonPropertyName("targetProcesses")]
        public List<string> TargetProcesses 
        { 
            get => _targetProcesses; 
            set => SetProperty(ref _targetProcesses, value); 
        }

        /// <summary>
        /// 監視間隔（ミリ秒）
        /// </summary>
        [JsonPropertyName("monitorInterval")]
        public int MonitorInterval 
        { 
            get => _monitorInterval; 
            set => SetProperty(ref _monitorInterval, value); 
        }

        /// <summary>
        /// Windows起動時に自動でアプリを開始するかどうか
        /// </summary>
        [JsonPropertyName("startWithWindows")]
        public bool StartWithWindows 
        { 
            get => _startWithWindows; 
            set => SetProperty(ref _startWithWindows, value); 
        }

        /// <summary>
        /// 設定画面を閉じた時に最小化されたウィンドウを復元するかどうか
        /// </summary>
        [JsonPropertyName("restoreOnSettingsClosed")]
        public bool RestoreOnSettingsClosed 
        { 
            get => _restoreOnSettingsClosed; 
            set => SetProperty(ref _restoreOnSettingsClosed, value); 
        }

        /// <summary>
        /// アプリ終了時に最小化されたウィンドウを復元するかどうか
        /// </summary>
        [JsonPropertyName("restoreOnAppExit")]
        public bool RestoreOnAppExit 
        { 
            get => _restoreOnAppExit; 
            set => SetProperty(ref _restoreOnAppExit, value); 
        }

        /// <summary>
        /// 全画面解除時に最小化されたウィンドウを復元するかどうか
        /// </summary>
        [JsonPropertyName("restoreOnFullScreenExit")]
        public bool RestoreOnFullScreenExit 
        { 
            get => _restoreOnFullScreenExit; 
            set => SetProperty(ref _restoreOnFullScreenExit, value); 
        }

        /// <summary>
        /// ダークテーマを使用するかどうか
        /// </summary>
        [JsonPropertyName("useDarkTheme")]
        public bool UseDarkTheme 
        { 
            get => _useDarkTheme; 
            set => SetProperty(ref _useDarkTheme, value); 
        }

        /// <summary>
        /// デフォルト設定を取得
        /// </summary>
        public static AppSettings GetDefault()
        {
            return new AppSettings
            {
                TargetProcesses = new List<string>(MonitorConstants.DefaultTargetProcesses),
                MonitorInterval = MonitorConstants.DefaultMonitorInterval,
                StartWithWindows = false
            };
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        /// <returns>設定が有効な場合true</returns>
        public bool IsValid()
        {
            return MonitorInterval >= MonitorConstants.MinMonitorInterval &&
                   MonitorInterval <= MonitorConstants.MaxMonitorInterval &&
                   TargetProcesses != null &&
                   TargetProcesses.All(p => !string.IsNullOrWhiteSpace(p));
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">フィールドへの参照</param>
        /// <param name="value">新しい値</param>
        /// <param name="propertyName">プロパティ名</param>
        /// <returns>値が変更された場合true</returns>
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// プロパティ変更イベントを発生させる
        /// </summary>
        /// <param name="propertyName">プロパティ名</param>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
