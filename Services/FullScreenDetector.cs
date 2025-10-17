using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Services
{
    /// <summary>
    /// 全画面検出サービス
    /// </summary>
    public class FullScreenDetector : IFullScreenDetector
    {
        #region フィールド

        private readonly DispatcherTimer _timer;
        private readonly List<string> _targetProcesses;
        private readonly object _lockObject = new();
        private readonly ILogger _logger;
        private bool _disposed = false;

        #endregion

        #region イベント

        /// <summary>
        /// 全画面状態が変更された時のイベント
        /// </summary>
        public event EventHandler<FullScreenStateChangedEventArgs>? FullScreenStateChanged;

        /// <summary>
        /// 対象プロセスがフォーカスされた時のイベント
        /// </summary>
        public event EventHandler<FullScreenStateChangedEventArgs>? TargetProcessFocused;

        #endregion

        #region プロパティ

        /// <summary>
        /// 監視中かどうか
        /// </summary>
        public bool IsMonitoring { get; private set; }

        /// <summary>
        /// 前回の全画面状態
        /// </summary>
        public bool WasFullScreen { get; private set; }

        /// <summary>
        /// 現在の全画面ウィンドウのハンドル
        /// </summary>
        public IntPtr CurrentFullScreenWindow { get; private set; }

        /// <summary>
        /// 現在の全画面ウィンドウが属するモニター
        /// </summary>
        public IntPtr CurrentMonitor { get; private set; }

        /// <summary>
        /// 前回のフォアグラウンドウィンドウ
        /// </summary>
        private IntPtr _lastForegroundWindow = IntPtr.Zero;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="targetProcesses">監視対象プロセス名のリスト</param>
        /// <param name="intervalMs">監視間隔（ミリ秒）</param>
        /// <param name="logger">ロガー</param>
        public FullScreenDetector(List<string> targetProcesses, int intervalMs = MonitorConstants.DefaultMonitorInterval, ILogger? logger = null)
        {
            _targetProcesses = targetProcesses ?? new List<string>();
            _logger = logger ?? new Services.ConsoleLogger();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(intervalMs)
            };
            _timer.Tick += Timer_Tick;
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 監視を開始
        /// </summary>
        public void StartMonitoring()
        {
            lock (_lockObject)
            {
                if (!_disposed && !IsMonitoring)
                {
                    _timer.Start();
                    IsMonitoring = true;
                }
            }
        }

        /// <summary>
        /// 監視を停止
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lockObject)
            {
                if (IsMonitoring)
                {
                    _timer.Stop();
                    IsMonitoring = false;
                }
            }
        }

        /// <summary>
        /// 監視間隔を更新
        /// </summary>
        /// <param name="intervalMs">新しい監視間隔（ミリ秒）</param>
        public void UpdateInterval(int intervalMs)
        {
            lock (_lockObject)
            {
                if (!_disposed)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(intervalMs);
                }
            }
        }

        /// <summary>
        /// 監視対象プロセスを更新
        /// </summary>
        /// <param name="targetProcesses">新しい監視対象プロセス名のリスト</param>
        public void UpdateTargetProcesses(List<string> targetProcesses)
        {
            lock (_lockObject)
            {
                _targetProcesses.Clear();
                if (targetProcesses != null)
                {
                    _targetProcesses.AddRange(targetProcesses);
                }
            }
        }

        #endregion

        #region イベントハンドラー

        /// <summary>
        /// タイマーイベントハンドラー
        /// </summary>
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                CheckForFullScreenWindows();
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.FullScreenDetectionError, ex);
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 全画面ウィンドウをチェック
        /// </summary>
        private void CheckForFullScreenWindows()
        {
            lock (_lockObject)
            {
                if (_disposed || _targetProcesses.Count == 0)
                {
                    return;
                }

                var fullScreenWindow = FindFullScreenWindow();
                var isFullScreen = fullScreenWindow != IntPtr.Zero;

                // 状態が変更された場合のみイベントを発火
                if (isFullScreen != WasFullScreen)
                {
                    WasFullScreen = isFullScreen;
                    CurrentFullScreenWindow = fullScreenWindow;
                    CurrentMonitor = isFullScreen ? GetWindowMonitor(fullScreenWindow) : IntPtr.Zero;

                    var windowTitle = fullScreenWindow != IntPtr.Zero ? NativeMethods.GetWindowTitle(fullScreenWindow) : string.Empty;
                    var processName = fullScreenWindow != IntPtr.Zero ? GetProcessNameFromWindow(fullScreenWindow) : string.Empty;

                    FullScreenStateChanged?.Invoke(this, new FullScreenStateChangedEventArgs(
                        isFullScreen, fullScreenWindow, CurrentMonitor, processName, windowTitle));
                }

                // 全画面状態中はフォーカス変更をチェック
                if (isFullScreen)
                {
                    CheckForFocusChange();
                }
            }
        }

        /// <summary>
        /// フォーカス変更をチェック
        /// </summary>
        private void CheckForFocusChange()
        {
            try
            {
                var currentForegroundWindow = NativeMethods.GetForegroundWindow();
                
                // フォーカスが変更された場合
                if (currentForegroundWindow != _lastForegroundWindow && currentForegroundWindow != IntPtr.Zero)
                {
                    _lastForegroundWindow = currentForegroundWindow;

                    // フォアグラウンドウィンドウのプロセス名を取得
                    var processName = GetProcessNameFromWindow(currentForegroundWindow);
                    
                    // 対象プロセスかどうかチェック
                    if (!string.IsNullOrEmpty(processName) && _targetProcesses.Contains(processName))
                    {
                        var windowTitle = NativeMethods.GetWindowTitle(currentForegroundWindow);
                        
                        _logger.LogInfo($"対象プロセスがフォーカスされました: {processName} ({windowTitle})");
                        
                        // TargetProcessFocusedイベントを発火
                        TargetProcessFocused?.Invoke(this, new FullScreenStateChangedEventArgs(
                            true, currentForegroundWindow, CurrentMonitor, processName, windowTitle));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"フォーカス変更チェック中にエラーが発生しました: {ex.Message}");
            }
        }

        /// <summary>
        /// 全画面ウィンドウを検索
        /// </summary>
        /// <returns>全画面ウィンドウのハンドル、見つからない場合はIntPtr.Zero</returns>
        private IntPtr FindFullScreenWindow()
        {
            IntPtr fullScreenWindow = IntPtr.Zero;

            NativeMethods.EnumWindows((windowHandle, lParam) =>
            {
                try
                {
                    // ウィンドウが可視で、システムウィンドウでない場合のみチェック
                    if (!NativeMethods.IsWindowVisible(windowHandle) || NativeMethods.IsSystemWindow(windowHandle))
                    {
                        return true; // 次のウィンドウをチェック
                    }

                    // プロセス名をチェック
                    if (NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId) == 0)
                    {
                        return true;
                    }

                    var processName = NativeMethods.GetProcessName(processId);
                    if (string.IsNullOrEmpty(processName))
                    {
                        return true;
                    }

                    // 監視対象プロセスかどうかチェック
                    if (!_targetProcesses.Contains(processName))
                    {
                        return true;
                    }

                    // ウィンドウの配置情報を取得
                    var placement = new NativeMethods.WINDOWPLACEMENT();
                    if (!NativeMethods.GetWindowPlacement(windowHandle, ref placement))
                    {
                        return true;
                    }

                    // 最大化状態かどうかチェック
                    if (placement.ShowCmd != NativeMethods.SW_SHOWMAXIMIZED)
                    {
                        return true;
                    }

                    // ウィンドウサイズを取得
                    if (!NativeMethods.GetWindowRect(windowHandle, out NativeMethods.RECT windowRect))
                    {
                        return true;
                    }

                    // モニター情報を取得
                    var monitor = NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
                    if (monitor == IntPtr.Zero)
                    {
                        return true;
                    }

                    var monitorInfo = new NativeMethods.MONITORINFO();
                    if (!NativeMethods.GetMonitorInfo(monitor, ref monitorInfo))
                    {
                        return true;
                    }

                    // ウィンドウがモニターの作業領域を完全に覆っているかチェック
                    var workArea = monitorInfo.WorkArea;
                    if (windowRect.Left <= workArea.Left &&
                        windowRect.Top <= workArea.Top &&
                        windowRect.Right >= workArea.Right &&
                        windowRect.Bottom >= workArea.Bottom)
                    {
                        fullScreenWindow = windowHandle;
                        return false; // 見つかったので列挙を停止
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"ウィンドウチェック中にエラーが発生しました: {ex.Message}");
                }

                return true; // 次のウィンドウをチェック
            }, IntPtr.Zero);

            return fullScreenWindow;
        }

        /// <summary>
        /// ウィンドウが属するモニターを取得
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>モニターハンドル</returns>
        private IntPtr GetWindowMonitor(IntPtr windowHandle)
        {
            return NativeMethods.MonitorFromWindow(windowHandle, NativeMethods.MONITOR_DEFAULTTONEAREST);
        }

        /// <summary>
        /// ウィンドウからプロセス名を取得
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>プロセス名</returns>
        private string GetProcessNameFromWindow(IntPtr windowHandle)
        {
            try
            {
                if (NativeMethods.GetWindowThreadProcessId(windowHandle, out uint processId) != 0)
                {
                    return NativeMethods.GetProcessName(processId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"プロセス名取得中にエラーが発生しました: {ex.Message}");
            }

            return string.Empty;
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
                    StopMonitoring();
                    _timer.Tick -= Timer_Tick;
                }

                _disposed = true;
            }
        }

        #endregion
    }

}
