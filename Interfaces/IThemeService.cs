using System;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// テーマ管理サービスのインターフェース
/// </summary>
public interface IThemeService
{
    #region イベント

    /// <summary>
    /// テーマ変更イベント
    /// </summary>
    event EventHandler<bool>? ThemeChanged;

    #endregion

    #region プロパティ

    /// <summary>
    /// 現在のテーマがダークテーマかどうか
    /// </summary>
    bool IsDarkTheme { get; }

    #endregion

    #region メソッド

    /// <summary>
    /// テーマを初期化
    /// </summary>
    /// <param name="useDarkTheme">ダークテーマを使用するかどうか</param>
    void InitializeTheme(bool useDarkTheme);

    /// <summary>
    /// テーマを切り替え
    /// </summary>
    /// <param name="useDarkTheme">ダークテーマを使用するかどうか</param>
    void SwitchTheme(bool useDarkTheme);

    /// <summary>
    /// テーマをトグル
    /// </summary>
    void ToggleTheme();

    /// <summary>
    /// ウィンドウのタイトルバー色を更新
    /// </summary>
    /// <param name="windowHandle">ウィンドウハンドル</param>
    void UpdateTitleBarColor(IntPtr windowHandle);

    #endregion
}
