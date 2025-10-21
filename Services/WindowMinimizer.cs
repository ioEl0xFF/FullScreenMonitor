using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services
{
    /// <summary>
    /// ウィンドウ最小化サービス
    /// </summary>
    public class WindowMinimizer : ServiceBase, IWindowMinimizer
    {
        #region フィールド

        private readonly List<IntPtr> _minimizedWindows = new();
        private readonly object _lockObject = new();
        private readonly IWindowCache _windowCache;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        /// <param name="windowCache">ウィンドウキャッシュ</param>
        public WindowMinimizer(ILogger logger, IWindowCache windowCache) : base(logger)
        {
            _windowCache = windowCache ?? throw new ArgumentNullException(nameof(windowCache));
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 指定モニター上のウィンドウを最小化
        /// </summary>
        /// <param name="monitorHandle">モニターハンドル</param>
        /// <param name="excludeWindow">除外するウィンドウハンドル</param>
        /// <returns>最小化したウィンドウ数</returns>
        public int MinimizeWindowsOnMonitor(IntPtr monitorHandle, IntPtr excludeWindow = default)
        {
            lock (_lockObject)
            {
                var minimizedCount = 0;
                _minimizedWindows.Clear();

                try
                {
                    var windowsOnMonitor = _windowCache.GetWindowsOnMonitor(monitorHandle, excludeWindow);

                    foreach (var windowInfo in windowsOnMonitor)
                    {
                        if (NativeMethods.ShowWindow(windowInfo.Handle, NativeMethods.SW_MINIMIZE))
                        {
                            _minimizedWindows.Add(windowInfo.Handle);
                            minimizedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ErrorMessages.WindowMinimizeError, ex);
                    throw new WindowOperationException(ErrorMessages.WindowMinimizeError, IntPtr.Zero, ex);
                }

                return minimizedCount;
            }
        }

        /// <summary>
        /// 最小化したウィンドウを復元
        /// </summary>
        /// <returns>復元したウィンドウ数</returns>
        public int RestoreMinimizedWindows()
        {
            lock (_lockObject)
            {
                var restoredCount = 0;

                try
                {
                    foreach (var hWnd in _minimizedWindows.ToList())
                    {
                        // ウィンドウがまだ存在するかチェック
                        if (NativeMethods.IsWindowVisible(hWnd))
                        {
                            if (NativeMethods.ShowWindow(hWnd, NativeMethods.SW_RESTORE))
                            {
                                restoredCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ErrorMessages.WindowRestoreError, ex);
                    throw new WindowOperationException(ErrorMessages.WindowRestoreError, IntPtr.Zero, ex);
                }
                finally
                {
                    _minimizedWindows.Clear();
                }

                return restoredCount;
            }
        }

        /// <summary>
        /// 最小化したウィンドウ数を取得
        /// </summary>
        public int MinimizedWindowCount
        {
            get
            {
                lock (_lockObject)
                {
                    return _minimizedWindows.Count;
                }
            }
        }

        /// <summary>
        /// 同一モニター上の対象プロセス以外のウィンドウを最小化
        /// </summary>
        /// <param name="targetWindow">対象ウィンドウハンドル</param>
        /// <param name="monitorHandle">モニターハンドル</param>
        /// <returns>最小化したウィンドウ数</returns>
        public int MinimizeAllNonTargetWindows(IntPtr targetWindow, IntPtr monitorHandle)
        {
            lock (_lockObject)
            {
                var minimizedCount = 0;

                try
                {
                    var windowsOnMonitor = _windowCache.GetWindowsOnMonitor(monitorHandle, targetWindow);

                    foreach (var windowInfo in windowsOnMonitor)
                    {
                        // ウィンドウが有効な場合のみ最小化
                        if (windowInfo.IsValid)
                        {
                            if (NativeMethods.ShowWindow(windowInfo.Handle, NativeMethods.SW_MINIMIZE))
                            {
                                _minimizedWindows.Add(windowInfo.Handle);
                                minimizedCount++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ErrorMessages.WindowMinimizeError, ex);
                    throw new WindowOperationException(ErrorMessages.WindowMinimizeError, IntPtr.Zero, ex);
                }

                return minimizedCount;
            }
        }

        /// <summary>
        /// 最小化履歴をクリア
        /// </summary>
        public void ClearMinimizedHistory()
        {
            lock (_lockObject)
            {
                _minimizedWindows.Clear();
            }
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// リソースを解放
        /// </summary>
        /// <param name="disposing">マネージリソースを解放するかどうか</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
