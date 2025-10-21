using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Services
{
    /// <summary>
    /// ウィンドウ情報のキャッシュサービス
    /// </summary>
    public class WindowCache : IDisposable
    {
        #region フィールド

        private readonly Dictionary<IntPtr, WindowInfo> _windowCache = new();
        private readonly Dictionary<uint, string> _processNameCache = new();
        private readonly object _lockObject = new();
        private readonly ILogger _logger;
        private DateTime _lastUpdate = DateTime.MinValue;
        private readonly TimeSpan _cacheExpiry = TimeSpan.FromSeconds(2);
        private bool _disposed = false;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public WindowCache(ILogger? logger = null)
        {
            _logger = logger ?? new ConsoleLogger();
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// キャッシュされたウィンドウ情報を取得
        /// </summary>
        /// <returns>ウィンドウ情報のリスト</returns>
        public List<WindowInfo> GetWindows()
        {
            lock (_lockObject)
            {
                if (_disposed)
                    return new List<WindowInfo>();

                if (DateTime.Now - _lastUpdate > _cacheExpiry)
                {
                    UpdateCache();
                }

                return _windowCache.Values.ToList();
            }
        }

        /// <summary>
        /// 指定されたプロセス名のウィンドウを取得
        /// </summary>
        /// <param name="processName">プロセス名</param>
        /// <returns>該当するウィンドウ情報のリスト</returns>
        public List<WindowInfo> GetWindowsByProcess(string processName)
        {
            if (string.IsNullOrEmpty(processName))
                return new List<WindowInfo>();

            var windows = GetWindows();
            return windows.Where(w => w.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        /// <summary>
        /// 指定されたモニター上のウィンドウを取得
        /// </summary>
        /// <param name="monitorHandle">モニターハンドル</param>
        /// <param name="excludeWindow">除外するウィンドウハンドル</param>
        /// <returns>該当するウィンドウ情報のリスト</returns>
        public List<WindowInfo> GetWindowsOnMonitor(IntPtr monitorHandle, IntPtr excludeWindow = default)
        {
            var windows = GetWindows();
            return windows.Where(w => w.MonitorHandle == monitorHandle && w.Handle != excludeWindow).ToList();
        }

        /// <summary>
        /// 全画面状態のウィンドウを検索
        /// </summary>
        /// <param name="targetProcesses">監視対象プロセス名のリスト</param>
        /// <returns>全画面状態のウィンドウ情報、見つからない場合はnull</returns>
        public WindowInfo? FindFullScreenWindow(List<string> targetProcesses)
        {
            if (targetProcesses == null || targetProcesses.Count == 0)
                return null;

            var windows = GetWindows();
            
            foreach (var window in windows)
            {
                if (!window.IsValid || !targetProcesses.Contains(window.ProcessName))
                    continue;

                if (!window.IsMaximized)
                    continue;

                // モニター情報を取得
                var monitorInfo = new NativeMethods.MONITORINFO();
                if (!NativeMethods.GetMonitorInfo(window.MonitorHandle, ref monitorInfo))
                    continue;

                if (window.IsFullScreen(monitorInfo))
                {
                    return window;
                }
            }

            return null;
        }

        /// <summary>
        /// キャッシュを強制的に更新
        /// </summary>
        public void ForceUpdate()
        {
            lock (_lockObject)
            {
                if (!_disposed)
                {
                    UpdateCache();
                }
            }
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void Clear()
        {
            lock (_lockObject)
            {
                _windowCache.Clear();
                _processNameCache.Clear();
                _lastUpdate = DateTime.MinValue;
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// キャッシュを更新
        /// </summary>
        private void UpdateCache()
        {
            try
            {
                var currentWindows = new Dictionary<IntPtr, WindowInfo>();
                
                NativeMethods.EnumWindows((windowHandle, lParam) =>
                {
                    try
                    {
                        var windowInfo = CreateWindowInfo(windowHandle);
                        if (windowInfo.HasValue)
                        {
                            currentWindows[windowHandle] = windowInfo.Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug($"ウィンドウ情報作成中にエラーが発生しました: {ex.Message}");
                    }
                    
                    return true; // 次のウィンドウをチェック
                }, IntPtr.Zero);

                _windowCache.Clear();
                foreach (var kvp in currentWindows)
                {
                    _windowCache[kvp.Key] = kvp.Value;
                }
                
                _lastUpdate = DateTime.Now;
                _logger.LogDebug($"ウィンドウキャッシュを更新しました: {_windowCache.Count}個のウィンドウ");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ウィンドウキャッシュの更新中にエラーが発生しました: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// ウィンドウ情報を作成
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>ウィンドウ情報、作成できない場合はnull</returns>
        private WindowInfo? CreateWindowInfo(IntPtr windowHandle)
        {
            try
            {
                // ウィンドウが可視でない場合はスキップ
                if (!NativeMethods.IsWindowVisible(windowHandle))
                    return null;

                // システムウィンドウは除外
                if (NativeMethods.IsSystemWindow(windowHandle))
                    return null;

                // プロセスIDを取得
                if (NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId) == 0)
                    return null;

                // プロセス名を取得（キャッシュから）
                var processName = GetProcessName(processId);
                if (string.IsNullOrEmpty(processName))
                    return null;

                // ウィンドウの配置情報を取得
                var placement = new NativeMethods.WINDOWPLACEMENT();
                if (!NativeMethods.GetWindowPlacement(windowHandle, ref placement))
                    return null;

                // ウィンドウサイズを取得
                if (!NativeMethods.GetWindowRect(windowHandle, out NativeMethods.RECT windowRect))
                    return null;

                // モニター情報を取得
                var monitorHandle = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
                if (monitorHandle == IntPtr.Zero)
                    return null;

                var windowInfo = new WindowInfo
                {
                    Handle = windowHandle,
                    ProcessId = processId,
                    ProcessName = processName,
                    WindowTitle = NativeMethods.GetWindowTitle(windowHandle),
                    IsMaximized = placement.ShowCmd == NativeMethods.SW_SHOWMAXIMIZED,
                    MonitorHandle = monitorHandle,
                    Placement = placement,
                    WindowRect = windowRect,
                    IsVisible = true,
                    IsSystemWindow = false,
                    CachedAt = DateTime.Now
                };

                return windowInfo;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"ウィンドウ情報作成中にエラーが発生しました: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// プロセス名を取得（キャッシュから）
        /// </summary>
        /// <param name="processId">プロセスID</param>
        /// <returns>プロセス名</returns>
        private string GetProcessName(uint processId)
        {
            if (_processNameCache.TryGetValue(processId, out var cachedName))
            {
                return cachedName;
            }

            var processName = NativeMethods.GetProcessName(processId);
            if (!string.IsNullOrEmpty(processName))
            {
                _processNameCache[processId] = processName;
            }

            return processName;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        /// <param name="disposing">マネージリソースを解放するかどうか</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    lock (_lockObject)
                    {
                        _windowCache.Clear();
                        _processNameCache.Clear();
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
