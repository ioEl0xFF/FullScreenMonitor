using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        /// デフォルト設定を取得
        /// </summary>
        public static AppSettings GetDefault()
        {
            return new AppSettings
            {
                TargetProcesses = new List<string> { "chrome", "firefox", "msedge" },
                MonitorInterval = 500,
                StartWithWindows = false
            };
        }

        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool IsValid()
        {
            return MonitorInterval >= 100 && 
                   MonitorInterval <= 2000 && 
                   TargetProcesses.Count > 0;
        }
    }
}
