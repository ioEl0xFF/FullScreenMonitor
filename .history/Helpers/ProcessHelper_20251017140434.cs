using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// プロセス操作ヘルパークラス
    /// </summary>
    public static class ProcessHelper
    {
        #region パブリックメソッド

        /// <summary>
        /// 現在稼働中のウィンドウを持つプロセスを取得
        /// </summary>
        /// <returns>プロセス情報のリスト</returns>
        public static List<ProcessInfo> GetProcessesWithWindows()
        {
            var processInfos = new Dictionary<string, ProcessInfo>();

            try
            {
                NativeMethods.EnumWindows((hWnd, lParam) =>
                {
                    try
                    {
                        // ウィンドウが可視でない場合はスキップ
                        if (!NativeMethods.IsWindowVisible(hWnd))
                        {
                            return true;
                        }

                        // システムウィンドウは除外
                        if (NativeMethods.IsSystemWindow(hWnd))
                        {
                            return true;
                        }

                        // ウィンドウタイトルを取得
                        var windowTitle = NativeMethods.GetWindowTitle(hWnd);
                        if (string.IsNullOrEmpty(windowTitle))
                        {
                            return true;
                        }

                        // プロセスIDを取得
                        if (NativeMethods.GetWindowThreadProcessId(hWnd, out uint processId) == 0)
                        {
                            return true;
                        }

                        // プロセス名を取得
                        var processName = NativeMethods.GetProcessName(processId);
                        if (string.IsNullOrEmpty(processName))
                        {
                            return true;
                        }

                        // 既に同じプロセス名が存在する場合は、より長いウィンドウタイトルを優先
                        var key = processName.ToLowerInvariant();
                        if (processInfos.ContainsKey(key))
                        {
                            var existing = processInfos[key];
                            if (windowTitle.Length > existing.WindowTitle.Length)
                            {
                                processInfos[key] = new ProcessInfo
                                {
                                    ProcessName = processName,
                                    WindowTitle = windowTitle
                                };
                            }
                        }
                        else
                        {
                            processInfos[key] = new ProcessInfo
                            {
                                ProcessName = processName,
                                WindowTitle = windowTitle
                            };
                        }
                    }
                    catch
                    {
                        // エラーが発生した場合はスキップ
                    }

                    return true; // 次のウィンドウをチェック
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"プロセス取得エラー: {ex.Message}");
            }

            // リストに変換してソート
            return processInfos.Values
                .OrderBy(p => p.ProcessName)
                .ToList();
        }

        /// <summary>
        /// 指定したプロセス名が実行中かどうかを確認
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>実行中の場合true</returns>
        public static bool IsProcessRunning(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return false;
            }

            try
            {
                var processes = Process.GetProcessesByName(processName);
                return processes.Length > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// プロセス名から実行ファイル名を取得
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>実行ファイル名、見つからない場合はnull</returns>
        public static string? GetProcessExecutablePath(string processName)
        {
            if (string.IsNullOrEmpty(processName))
            {
                return null;
            }

            try
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    return processes[0].MainModule?.FileName;
                }
            }
            catch
            {
                // エラーが発生した場合はnullを返す
            }

            return null;
        }

        #endregion
    }
}
