using System;
using System.Windows;
using FullScreenMonitor.Interfaces;
using MaterialDesignThemes.Wpf;

namespace FullScreenMonitor.Services;

/// <summary>
/// テーマ管理サービス
/// Material Designテーマの管理とタイトルバー色の制御を行う
/// </summary>
public class ThemeService : IThemeService
{
    #region フィールド

    private readonly ILogger _logger;
    private bool _isDarkTheme;

    #endregion

    #region イベント

    /// <summary>
    /// テーマ変更イベント
    /// </summary>
    public event EventHandler<bool>? ThemeChanged;

    #endregion

    #region プロパティ

    /// <summary>
    /// 現在のテーマがダークテーマかどうか
    /// </summary>
    public bool IsDarkTheme => _isDarkTheme;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public ThemeService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// テーマを初期化
    /// </summary>
    /// <param name="useDarkTheme">ダークテーマを使用するかどうか</param>
    public void InitializeTheme(bool useDarkTheme)
    {
        try
        {
            _isDarkTheme = useDarkTheme;
            ApplyTheme();
            _logger.LogInfo($"テーマを初期化しました: {(_isDarkTheme ? "ダーク" : "ライト")}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ初期化エラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// テーマを切り替え
    /// </summary>
    /// <param name="useDarkTheme">ダークテーマを使用するかどうか</param>
    public void SwitchTheme(bool useDarkTheme)
    {
        try
        {
            if (_isDarkTheme != useDarkTheme)
            {
                _isDarkTheme = useDarkTheme;
                ApplyTheme();
                ThemeChanged?.Invoke(this, _isDarkTheme);
                _logger.LogInfo($"テーマを切り替えました: {(_isDarkTheme ? "ダーク" : "ライト")}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ切り替えエラー: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// テーマをトグル
    /// </summary>
    public void ToggleTheme()
    {
        SwitchTheme(!_isDarkTheme);
    }

    /// <summary>
    /// ウィンドウのタイトルバー色を更新
    /// </summary>
    /// <param name="windowHandle">ウィンドウハンドル</param>
    public void UpdateTitleBarColor(IntPtr windowHandle)
    {
        try
        {
            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            // Windows APIを使用してタイトルバーの色を変更
            var titleBarColor = _isDarkTheme ? 
                System.Drawing.ColorTranslator.FromHtml("#1976D2") : // ダークテーマ用の濃い青
                System.Drawing.ColorTranslator.FromHtml("#2196F3");  // ライトテーマ用の明るい青
            
            // DwmSetWindowAttributeを使用してタイトルバーの色を設定
            var color = (uint)((titleBarColor.B << 16) | (titleBarColor.G << 8) | titleBarColor.R);
            
            // DWMWA_CAPTION_COLOR を使用
            Helpers.NativeMethods.DwmSetWindowAttribute(windowHandle, Helpers.NativeMethods.DWMWA_CAPTION_COLOR, ref color, sizeof(uint));
        }
        catch (Exception ex)
        {
            _logger.LogError($"タイトルバーの色更新に失敗しました: {ex.Message}", ex);
        }
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// テーマを適用
    /// </summary>
    private void ApplyTheme()
    {
        try
        {
            var paletteHelper = new PaletteHelper();
            var theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(_isDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
            paletteHelper.SetTheme(theme);
        }
        catch (Exception ex)
        {
            _logger.LogError($"テーマ適用エラー: {ex.Message}", ex);
            throw;
        }
    }

    #endregion
}
