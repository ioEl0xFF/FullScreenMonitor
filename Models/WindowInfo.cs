using System;

namespace FullScreenMonitor.Models
{
    /// <summary>
    /// ウィンドウ情報を保持する軽量な構造体
    /// </summary>
    public struct WindowInfo
    {
        /// <summary>
        /// ウィンドウハンドル
        /// </summary>
        public IntPtr Handle { get; set; }

        /// <summary>
        /// プロセスID
        /// </summary>
        public uint ProcessId { get; set; }

        /// <summary>
        /// プロセス名（小文字）
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string WindowTitle { get; set; }

        /// <summary>
        /// 最大化状態かどうか
        /// </summary>
        public bool IsMaximized { get; set; }

        /// <summary>
        /// モニターハンドル
        /// </summary>
        public IntPtr MonitorHandle { get; set; }

        /// <summary>
        /// ウィンドウの配置情報
        /// </summary>
        public Helpers.NativeMethods.WINDOWPLACEMENT Placement { get; set; }

        /// <summary>
        /// ウィンドウの矩形
        /// </summary>
        public Helpers.NativeMethods.RECT WindowRect { get; set; }

        /// <summary>
        /// ウィンドウが可視かどうか
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// システムウィンドウかどうか
        /// </summary>
        public bool IsSystemWindow { get; set; }

        /// <summary>
        /// キャッシュされた時刻
        /// </summary>
        public DateTime CachedAt { get; set; }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public WindowInfo()
        {
            Handle = IntPtr.Zero;
            ProcessId = 0;
            ProcessName = string.Empty;
            WindowTitle = string.Empty;
            IsMaximized = false;
            MonitorHandle = IntPtr.Zero;
            Placement = new Helpers.NativeMethods.WINDOWPLACEMENT();
            WindowRect = new Helpers.NativeMethods.RECT();
            IsVisible = false;
            IsSystemWindow = false;
            CachedAt = DateTime.Now;
        }

        /// <summary>
        /// ウィンドウが有効かどうか
        /// </summary>
        public bool IsValid => Handle != IntPtr.Zero && IsVisible && !IsSystemWindow;

        /// <summary>
        /// 全画面状態かどうかを判定
        /// </summary>
        /// <param name="monitorInfo">モニター情報</param>
        /// <returns>全画面状態の場合true</returns>
        public bool IsFullScreen(Helpers.NativeMethods.MONITORINFO monitorInfo)
        {
            if (!IsValid || !IsMaximized)
                return false;

            var workArea = monitorInfo.WorkArea;
            return WindowRect.Left <= workArea.Left &&
                   WindowRect.Top <= workArea.Top &&
                   WindowRect.Right >= workArea.Right &&
                   WindowRect.Bottom >= workArea.Bottom;
        }
    }
}
