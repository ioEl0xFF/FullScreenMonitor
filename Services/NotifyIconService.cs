using System;
using System.Drawing;
using System.Windows.Forms;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// システムトレイアイコン管理サービス
/// NotifyIconの管理とイベント処理を行う
/// </summary>
public class NotifyIconService : INotifyIconService
{
    #region フィールド

    private NotifyIcon? _notifyIcon;
    private readonly ILogger _logger;
    private bool _disposed = false;

    #endregion

    #region イベント

    /// <summary>
    /// ダブルクリックイベント
    /// </summary>
    public event EventHandler? DoubleClick;

    /// <summary>
    /// マウス移動イベント
    /// </summary>
    public event MouseEventHandler? MouseMove;

    #endregion

    #region プロパティ

    /// <summary>
    /// アイコンが表示されているかどうか
    /// </summary>
    public bool Visible
    {
        get => _notifyIcon?.Visible ?? false;
        set
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Visible = value;
            }
        }
    }

    /// <summary>
    /// ツールチップテキスト
    /// </summary>
    public string Text
    {
        get => _notifyIcon?.Text ?? string.Empty;
        set
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = value;
            }
        }
    }

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public NotifyIconService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// システムトレイアイコンを初期化
    /// </summary>
    public void Initialize()
    {
        try
        {
            if (_notifyIcon != null)
            {
                return; // 既に初期化済み
            }

            var customIcon = LoadIcon();
            
            _notifyIcon = new NotifyIcon
            {
                Icon = customIcon,
                Text = AppConstants.SystemTrayTextMonitoring,
                Visible = true
            };

            // イベントハンドラーを設定
            _notifyIcon.DoubleClick += OnDoubleClick;
            _notifyIcon.MouseMove += OnMouseMove;

            _logger.LogInfo("システムトレイアイコンを初期化しました");
        }
        catch (Exception ex)
        {
            _logger.LogError($"システムトレイアイコンの初期化に失敗しました: {ex.Message}", ex);
            throw;
        }
    }

    /// <summary>
    /// バルーンチップを表示
    /// </summary>
    /// <param name="timeout">表示時間（ミリ秒）</param>
    /// <param name="title">タイトル</param>
    /// <param name="text">メッセージ</param>
    /// <param name="icon">アイコン</param>
    public void ShowBalloonTip(int timeout, string title, string text, ToolTipIcon icon)
    {
        try
        {
            _notifyIcon?.ShowBalloonTip(timeout, title, text, icon);
        }
        catch (Exception ex)
        {
            _logger.LogError($"バルーンチップの表示に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// コンテキストメニューを設定
    /// </summary>
    /// <param name="contextMenu">コンテキストメニュー</param>
    public void SetContextMenu(ContextMenuStrip contextMenu)
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.ContextMenuStrip = contextMenu;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"コンテキストメニューの設定に失敗しました: {ex.Message}", ex);
        }
    }

    #endregion

    #region プライベートメソッド

    /// <summary>
    /// アイコンを読み込み
    /// </summary>
    /// <returns>アイコン</returns>
    private Icon LoadIcon()
    {
        try
        {
            // 出力ディレクトリからアイコンファイルを読み込み
            var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "app.ico");
            _logger.LogInfo($"アイコンファイルパス: {iconPath}");
            
            if (System.IO.File.Exists(iconPath))
            {
                var customIcon = new Icon(iconPath);
                _logger.LogInfo("アイコンを正常に読み込みました");
                return customIcon;
            }
            else
            {
                // アイコンファイルが見つからない場合はWindows標準アイコンを使用
                _logger.LogWarning($"アイコンファイルが見つかりません: {iconPath}");
                return SystemIcons.Application;
            }
        }
        catch (Exception ex)
        {
            // アイコンの読み込みに失敗した場合はWindows標準アイコンを使用
            _logger.LogError($"アイコンの読み込みに失敗しました: {ex.Message}", ex);
            return SystemIcons.Application;
        }
    }

    /// <summary>
    /// ダブルクリックイベントハンドラー
    /// </summary>
    private void OnDoubleClick(object? sender, EventArgs e)
    {
        DoubleClick?.Invoke(sender, e);
    }

    /// <summary>
    /// マウス移動イベントハンドラー
    /// </summary>
    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        MouseMove?.Invoke(sender, e);
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
                if (_notifyIcon != null)
                {
                    _notifyIcon.DoubleClick -= OnDoubleClick;
                    _notifyIcon.MouseMove -= OnMouseMove;
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                    _notifyIcon = null;
                }
            }

            _disposed = true;
        }
    }

    #endregion
}
