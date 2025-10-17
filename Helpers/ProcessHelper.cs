using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// プロセス操作ヘルパークラス
    /// </summary>
    public class ProcessHelper
    {
        #region フィールド

        private readonly ILogger _logger;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public ProcessHelper(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 現在稼働中のウィンドウを持つプロセスを取得
        /// </summary>
        /// <returns>プロセス情報のリスト</returns>
        public List<ProcessInfo> GetProcessesWithWindows()
        {
            var processInfos = new Dictionary<string, ProcessInfo>();

            try
            {
                NativeMethods.EnumWindows((windowHandle, lParam) =>
                {
                    try
                    {
                        // ウィンドウが可視でない場合はスキップ
                        if (!NativeMethods.IsWindowVisible(windowHandle))
                        {
                            return true;
                        }

                        // システムウィンドウは除外
                        if (NativeMethods.IsSystemWindow(windowHandle))
                        {
                            return true;
                        }

                        // ウィンドウタイトルを取得
                        var windowTitle = NativeMethods.GetWindowTitle(windowHandle);
                        if (string.IsNullOrEmpty(windowTitle))
                        {
                            return true;
                        }

                        // プロセスIDを取得
                        if (NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId) == 0)
                        {
                            return true;
                        }

                        // プロセス名を取得
                        var processName = NativeMethods.GetProcessName(processId);
                        if (string.IsNullOrEmpty(processName))
                        {
                            return true;
                        }

                        // 重複を排除（プロセス名のみで判定）
                        var key = processName.ToLowerInvariant();
                        if (!processInfos.ContainsKey(key))
                        {
                            processInfos[key] = new ProcessInfo
                            {
                                ProcessName = processName,
                                WindowTitle = windowTitle
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"プロセス情報取得中にエラーが発生しました: {ex.Message}", ex);
                    }

                    return true; // 次のウィンドウをチェック
                }, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.ProcessInfoGetError, ex);
                throw new MonitoringException(ErrorMessages.ProcessInfoGetError, ex);
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
        public bool IsProcessRunning(string processName)
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
        public string? GetProcessExecutablePath(string processName)
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
