namespace FullScreenMonitor.Constants;

/// <summary>
/// ウィンドウとUI関連の定数
/// </summary>
public static class WindowConstants
{
    /// <summary>
    /// メインウィンドウのサイズ（システムトレイ常駐用の隠しウィンドウ）
    /// </summary>
    public const int HiddenWindowSize = 1;

    /// <summary>
    /// 設定ウィンドウのデフォルトサイズ
    /// </summary>
    public const int SettingsWindowWidth = 500;
    public const int SettingsWindowHeight = 400;

    /// <summary>
    /// 設定ウィンドウのマージン
    /// </summary>
    public const int SettingsWindowMargin = 20;

    /// <summary>
    /// グループボックス間のマージン
    /// </summary>
    public const int GroupBoxMargin = 15;

    /// <summary>
    /// プロセス一覧の高さ
    /// </summary>
    public const int ProcessListHeight = 120;

    /// <summary>
    /// ボタンの標準サイズ
    /// </summary>
    public const int StandardButtonHeight = 25;
    public const int StandardButtonWidth = 80;

    /// <summary>
    /// コンボボックスの高さ
    /// </summary>
    public const int ComboBoxHeight = 25;

    /// <summary>
    /// リフレッシュボタンのサイズ
    /// </summary>
    public const int RefreshButtonSize = 25;

    /// <summary>
    /// バルーンチップの表示時間（ミリ秒）
    /// </summary>
    public const int BalloonTipInfoDuration = 3000;
    public const int BalloonTipErrorDuration = 5000;

    /// <summary>
    /// バルーンチップのタイトル
    /// </summary>
    public const string BalloonTipTitle = "FullScreenMonitor";

    /// <summary>
    /// バルーンチップのエラー用タイトル
    /// </summary>
    public const string BalloonTipErrorTitle = "FullScreenMonitor - エラー";
}
