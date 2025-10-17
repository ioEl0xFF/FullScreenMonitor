using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Helpers;

namespace FullScreenMonitor.Services
{
    /// <summary>
    /// ウィンドウ最小化サービス
    /// </summary>
    public class WindowMinimizer
    {
        #region フィールド

        private readonly List<IntPtr> _minimizedWindows = new();
        private readonly object _lockObject = new();

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
                    var windowsOnMonitor = GetWindowsOnMonitor(monitorHandle, excludeWindow);
                    
                    foreach (var hWnd in windowsOnMonitor)
                    {
                        if (NativeMethods.ShowWindow(hWnd, NativeMethods.SW_MINIMIZE))
                        {
                            _minimizedWindows.Add(hWnd);
                            minimizedCount++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ウィンドウ最小化エラー: {ex.Message}");
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
                    System.Diagnostics.Debug.WriteLine($"ウィンドウ復元エラー: {ex.Message}");
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

        #region プライベートメソッド

        /// <summary>
        /// 指定モニター上のウィンドウを取得
        /// </summary>
        /// <param name="monitorHandle">モニターハンドル</param>
        /// <param name="excludeWindow">除外するウィンドウハンドル</param>
        /// <returns>ウィンドウハンドルのリスト</returns>
        private List<IntPtr> GetWindowsOnMonitor(IntPtr monitorHandle, IntPtr excludeWindow)
        {
            var windows = new List<IntPtr>();

            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                try
                {
                    // 除外ウィンドウかチェック
                    if (hWnd == excludeWindow)
                    {
                        return true;
                    }

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

                    // ウィンドウが指定モニター上にあるかチェック
                    var windowMonitor = NativeMethods.MonitorFromWindow(hWnd, NativeMethods.MONITOR_DEFAULTTONEAREST);
                    if (windowMonitor != monitorHandle)
                    {
                        return true;
                    }

                    // ウィンドウが最小化されていないかチェック
                    var placement = new NativeMethods.WINDOWPLACEMENT();
                    if (NativeMethods.GetWindowPlacement(hWnd, ref placement))
                    {
                        if (placement.ShowCmd == NativeMethods.SW_SHOWMINIMIZED)
                        {
                            return true; // 既に最小化されているウィンドウはスキップ
                        }
                    }

                    windows.Add(hWnd);
                }
                catch
                {
                    // エラーが発生した場合はスキップ
                }

                return true; // 次のウィンドウをチェック
            }, IntPtr.Zero);

            return windows;
        }

        #endregion
    }
}
