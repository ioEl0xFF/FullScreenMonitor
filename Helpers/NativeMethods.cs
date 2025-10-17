using System;
using System.Runtime.InteropServices;
using System.Text;
using FullScreenMonitor.Constants;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// Win32 API定義クラス
    /// </summary>
    public static class NativeMethods
    {
        #region 定数

        // ShowWindow定数
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;

        // GetWindowLong定数
        public const int GWL_EXSTYLE = -20;
        public const int GWL_STYLE = -16;
        public const int GWL_ID = -12;
        public const int GWL_USERDATA = -21;

        // 拡張ウィンドウスタイル
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        // モニター情報
        public const int MONITOR_DEFAULTTONULL = 0;
        public const int MONITOR_DEFAULTTOPRIMARY = 1;
        public const int MONITOR_DEFAULTTONEAREST = 2;

        #endregion

        #region 構造体

        /// <summary>
        /// RECT構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width => Right - Left;
            public int Height => Bottom - Top;
        }

        /// <summary>
        /// POINT構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// MONITORINFO構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFO
        {
            public int Size;
            public RECT Monitor;
            public RECT WorkArea;
            public uint Flags;

            public MONITORINFO()
            {
                Size = Marshal.SizeOf(typeof(MONITORINFO));
                Monitor = new RECT();
                WorkArea = new RECT();
                Flags = 0;
            }
        }

        /// <summary>
        /// WINDOWPLACEMENT構造体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int Length;
            public int Flags;
            public int ShowCmd;
            public POINT MinPosition;
            public POINT MaxPosition;
            public RECT NormalPosition;

            public WINDOWPLACEMENT()
            {
                Length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                Flags = 0;
                ShowCmd = 0;
                MinPosition = new POINT();
                MaxPosition = new POINT();
                NormalPosition = new RECT();
            }
        }

        #endregion

        #region API定義

        /// <summary>
        /// すべてのウィンドウを列挙
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        /// <summary>
        /// ウィンドウ列挙デリゲート
        /// </summary>
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        /// <summary>
        /// ウィンドウの位置とサイズを取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        /// <summary>
        /// ウィンドウが属するモニターを取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        /// <summary>
        /// モニター情報を取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        /// <summary>
        /// ウィンドウの表示状態を取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        /// <summary>
        /// ウィンドウを表示/非表示/最小化
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// ウィンドウのプロセスIDを取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        /// <summary>
        /// ウィンドウの可視状態を確認
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        /// <summary>
        /// ウィンドウの拡張スタイルを取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// ウィンドウテキストを取得
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// ウィンドウテキストの長さを取得
        /// </summary>
        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        /// <summary>
        /// ウィンドウクラス名を取得
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>
        /// プロセス名を取得
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        /// <summary>
        /// プロセスハンドルを閉じる
        /// </summary>
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// プロセス名を取得
        /// </summary>
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern int QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref uint lpdwSize);

        #endregion

        #region 定数

        // プロセスアクセス権限
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

        #endregion

        #region ヘルパーメソッド

        /// <summary>
        /// ウィンドウのタイトルを取得
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>ウィンドウタイトル</returns>
        public static string GetWindowTitle(IntPtr windowHandle)
        {
            var length = GetWindowTextLength(windowHandle);
            if (length == 0)
                return string.Empty;

            var builder = new StringBuilder(length + 1);
            GetWindowText(windowHandle, builder, builder.Capacity);
            return builder.ToString();
        }

        /// <summary>
        /// ウィンドウのクラス名を取得
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>ウィンドウクラス名</returns>
        public static string GetWindowClassName(IntPtr windowHandle)
        {
            var builder = new StringBuilder(MonitorConstants.MaxClassNameLength);
            GetClassName(windowHandle, builder, builder.Capacity);
            return builder.ToString();
        }

        /// <summary>
        /// プロセス名を取得
        /// </summary>
        /// <param name="processId">プロセスID</param>
        /// <returns>プロセス名（小文字）</returns>
        public static string GetProcessName(uint processId)
        {
            try
            {
                var processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
                if (processHandle == IntPtr.Zero)
                    return string.Empty;

                var builder = new StringBuilder(MonitorConstants.MaxProcessPathLength);
                var size = (uint)builder.Capacity;
                var result = QueryFullProcessImageName(processHandle, 0, builder, ref size);

                CloseHandle(processHandle);

                if (result != 0)
                {
                    var fullPath = builder.ToString();
                    return System.IO.Path.GetFileNameWithoutExtension(fullPath).ToLower();
                }
            }
            catch
            {
                // エラーが発生した場合は空文字を返す
            }

            return string.Empty;
        }

        /// <summary>
        /// ツールウィンドウかどうかを判定
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>ツールウィンドウの場合true</returns>
        public static bool IsToolWindow(IntPtr windowHandle)
        {
            var exStyle = GetWindowLong(windowHandle, GWL_EXSTYLE);
            return (exStyle & WS_EX_TOOLWINDOW) != 0;
        }

        /// <summary>
        /// システムウィンドウかどうかを判定
        /// </summary>
        /// <param name="windowHandle">ウィンドウハンドル</param>
        /// <returns>システムウィンドウの場合true</returns>
        public static bool IsSystemWindow(IntPtr windowHandle)
        {
            var className = GetWindowClassName(windowHandle);
            var title = GetWindowTitle(windowHandle);

            // システムウィンドウのクラス名やタイトルをチェック
            var systemClasses = new[]
            {
                "Shell_TrayWnd",      // タスクバー
                "WorkerW",            // デスクトップ
                "Progman",            // Program Manager
                "DV2ControlHost",     // デスクトップ
                "ImmersiveLauncher",  // スタートメニュー
                "SearchUI",           // 検索UI
                "Shell_SecondaryTrayWnd" // セカンダリタスクバー
            };

            return systemClasses.Contains(className) ||
                   string.IsNullOrEmpty(title) ||
                   IsToolWindow(windowHandle);
        }

        #endregion
    }
}
