using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// プロセス情報を取得するヘルパークラス
    /// </summary>
    public static class ProcessHelper
    {
        #region パブリックメソッド

        /// <summary>
        /// 現在稼働中のプロセス情報を取得
        /// </summary>
        /// <returns>プロセス情報のリスト</returns>
        public static List<ProcessInfo> GetRunningProcesses()
        {
            var processes = new List<ProcessInfo>();

            try
            {
                var runningProcesses = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                    .OrderBy(p => p.ProcessName)
                    .DistinctBy(p => p.ProcessName.ToLower());

                foreach (var process in runningProcesses)
                {
                    try
                    {
                        var processInfo = new ProcessInfo
                        {
                            ProcessName = process.ProcessName.ToLower(),
                            DisplayName = $"{process.ProcessName} ({process.Id})",
                            ProcessId = process.Id,
                            MainWindowTitle = GetMainWindowTitle(process),
                            HasMainWindow = !string.IsNullOrEmpty(process.MainWindowTitle)
                        };

                        processes.Add(processInfo);
                    }
                    catch
                    {
                        // プロセス情報の取得に失敗した場合はスキップ
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"プロセス取得エラー: {ex.Message}");
            }

            return processes;
        }

        /// <summary>
        /// ウィンドウを持つプロセスのみを取得
        /// </summary>
        /// <returns>ウィンドウを持つプロセス情報のリスト</returns>
        public static List<ProcessInfo> GetProcessesWithWindows()
        {
            return GetRunningProcesses()
                .Where(p => p.HasMainWindow)
                .ToList();
        }

        /// <summary>
        /// プロセス名でプロセス情報を検索
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>一致するプロセス情報、見つからない場合はnull</returns>
        public static ProcessInfo? FindProcessByName(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return null;

            return GetRunningProcesses()
                .FirstOrDefault(p => p.ProcessName.Equals(processName.ToLower(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// プロセス名の存在確認
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>存在する場合はtrue</returns>
        public static bool IsProcessRunning(string processName)
        {
            return FindProcessByName(processName) != null;
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// プロセスのメインウィンドウタイトルを取得
        /// </summary>
        /// <param name="process">プロセス</param>
        /// <returns>ウィンドウタイトル</returns>
        private static string GetMainWindowTitle(Process process)
        {
            try
            {
                if (!string.IsNullOrEmpty(process.MainWindowTitle))
                {
                    return process.MainWindowTitle;
                }

                // MainWindowTitleが空の場合は、ウィンドウハンドルをチェック
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    return $"ウィンドウ #{process.MainWindowHandle}";
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }

    /// <summary>
    /// プロセス情報クラス
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// プロセス名（小文字）
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// 表示名（プロセス名 + PID）
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// プロセスID
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// メインウィンドウタイトル
        /// </summary>
        public string MainWindowTitle { get; set; } = string.Empty;

        /// <summary>
        /// メインウィンドウを持つかどうか
        /// </summary>
        public bool HasMainWindow { get; set; }

        /// <summary>
        /// 文字列表現
        /// </summary>
        /// <returns>表示名</returns>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}
