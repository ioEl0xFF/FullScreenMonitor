using System;
using System.Windows.Forms;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// システムトレイアイコン管理サービスのインターフェース
/// </summary>
public interface INotifyIconService : IDisposable
{
    #region イベント

    /// <summary>
    /// ダブルクリックイベント
    /// </summary>
    event EventHandler? DoubleClick;

    /// <summary>
    /// マウス移動イベント
    /// </summary>
    event MouseEventHandler? MouseMove;

    #endregion

    #region プロパティ

    /// <summary>
    /// アイコンが表示されているかどうか
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// ツールチップテキスト
    /// </summary>
    string Text { get; set; }

    #endregion

    #region メソッド

    /// <summary>
    /// システムトレイアイコンを初期化
    /// </summary>
    void Initialize();

    /// <summary>
    /// バルーンチップを表示
    /// </summary>
    /// <param name="timeout">表示時間（ミリ秒）</param>
    /// <param name="title">タイトル</param>
    /// <param name="text">メッセージ</param>
    /// <param name="icon">アイコン</param>
    void ShowBalloonTip(int timeout, string title, string text, ToolTipIcon icon);

    /// <summary>
    /// コンテキストメニューを設定
    /// </summary>
    /// <param name="contextMenu">コンテキストメニュー</param>
    void SetContextMenu(ContextMenuStrip contextMenu);

    #endregion
}
