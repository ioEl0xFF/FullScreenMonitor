using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using FullScreenMonitor.Constants;

namespace FullScreenMonitor.Models
{
    /// <summary>
    /// アプリケーション設定データモデル
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 監視対象プロセス名のリスト
        /// </summary>
        [JsonPropertyName("targetProcesses")]
        public List<string> TargetProcesses { get; set; } = new();

        /// <summary>
        /// 監視間隔（ミリ秒）
        /// </summary>
        [JsonPropertyName("monitorInterval")]
        public int MonitorInterval { get; set; } = 500;

        /// <summary>
        /// Windows起動時に自動でアプリを開始するかどうか
        /// </summary>
        [JsonPropertyName("startWithWindows")]
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// 設定画面を閉じた時に最小化されたウィンドウを復元するかどうか
        /// </summary>
        [JsonPropertyName("restoreOnSettingsClosed")]
        public bool RestoreOnSettingsClosed { get; set; } = true;

        /// <summary>
        /// アプリ終了時に最小化されたウィンドウを復元するかどうか
        /// </summary>
        [JsonPropertyName("restoreOnAppExit")]
        public bool RestoreOnAppExit { get; set; } = true;

        /// <summary>
        /// ダークテーマを使用するかどうか
        /// </summary>
        [JsonPropertyName("useDarkTheme")]
        public bool UseDarkTheme { get; set; } = false;

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
    }
}
