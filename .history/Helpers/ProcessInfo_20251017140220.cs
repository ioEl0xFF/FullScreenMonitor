namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// プロセス情報データクラス
    /// </summary>
    public class ProcessInfo
    {
        /// <summary>
        /// プロセス名
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        public string WindowTitle { get; set; } = string.Empty;

        /// <summary>
        /// ComboBoxの表示用テキスト
        /// </summary>
        public string DisplayText => string.IsNullOrEmpty(WindowTitle) 
            ? ProcessName 
            : $"{ProcessName} - {WindowTitle}";

        /// <summary>
        /// 等価性比較
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is ProcessInfo other)
            {
                return ProcessName.Equals(other.ProcessName, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        /// <summary>
        /// ハッシュコード取得
        /// </summary>
        public override int GetHashCode()
        {
            return ProcessName.ToLowerInvariant().GetHashCode();
        }

        /// <summary>
        /// 文字列表現
        /// </summary>
        public override string ToString()
        {
            return DisplayText;
        }
    }
}
