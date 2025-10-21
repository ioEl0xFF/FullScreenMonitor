using System;
using System.Collections.Generic;
using System.Windows.Threading;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Services
{
    /// <summary>
    /// ウィンドウ監視サービス統合クラス
    /// </summary>
    public class WindowMonitorService : IWindowMonitorService
    {
        #region フィールド

        private IFullScreenDetector? _detector;
        private readonly IWindowMinimizer _minimizer;
        private readonly object _lockObject = new();
        private readonly ILogger _logger;
        private bool _disposed = false;

        #endregion

        #region イベント

        /// <summary>
        /// 監視状態が変更された時のイベント
        /// </summary>
        public event EventHandler<bool>? MonitoringStateChanged;

        /// <summary>
        /// ウィンドウが最小化された時のイベント
        /// </summary>
        public event EventHandler<int>? WindowsMinimized;

        /// <summary>
        /// ウィンドウが復元された時のイベント
        /// </summary>
        public event EventHandler<int>? WindowsRestored;

        /// <summary>
        /// エラーが発生した時のイベント
        /// </summary>
        public event EventHandler<string>? ErrorOccurred;

        #endregion

        #region プロパティ

        /// <summary>
        /// 監視中かどうか
        /// </summary>
        public bool IsMonitoring { get; private set; }

        /// <summary>
        /// 現在の設定
        /// </summary>
        public AppSettings CurrentSettings { get; private set; }

        /// <summary>
        /// 最後にチェックした時刻
        /// </summary>
        public DateTime LastCheckTime { get; private set; }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">初期設定</param>
        /// <param name="logger">ロガー</param>
        public WindowMonitorService(AppSettings settings, ILogger? logger = null)
        {
            CurrentSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? new Services.ConsoleLogger();
            
            // サービスコンテナからWindowCacheを取得してWindowMinimizerを作成
            var windowCache = App.ServiceContainer?.Resolve<IWindowCache>() ?? throw new InvalidOperationException("WindowCacheサービスが登録されていません");
            _minimizer = new WindowMinimizer(_logger, windowCache);
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 監視を開始
        /// </summary>
        public void StartMonitoring()
        {
            lock (_lockObject)
            {
                if (_disposed || IsMonitoring)
                {
                    return;
                }

                try
                {
                    // サービスコンテナからWindowCacheを取得
                    var windowCache = App.ServiceContainer?.Resolve<IWindowCache>() ?? throw new InvalidOperationException("WindowCacheサービスが登録されていません");
                    
                    _detector = new FullScreenDetector(CurrentSettings.TargetProcesses, CurrentSettings.MonitorInterval, _logger, windowCache);
                    _detector.FullScreenStateChanged += Detector_FullScreenStateChanged;
                    _detector.TargetProcessFocused += Detector_TargetProcessFocused;
                    _detector.StartMonitoring();

                    IsMonitoring = true;
                    _logger.LogInfo("監視を開始しました");
                    MonitoringStateChanged?.Invoke(this, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ErrorMessages.MonitoringStartError, ex);
                    ErrorOccurred?.Invoke(this, ErrorMessages.MonitoringStartError);
                }
            }
        }

        /// <summary>
        /// 監視を停止
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lockObject)
            {
                if (!IsMonitoring)
                {
                    return;
                }

                try
                {
                    if (_detector != null)
                    {
                        _detector.FullScreenStateChanged -= Detector_FullScreenStateChanged;
                        _detector.TargetProcessFocused -= Detector_TargetProcessFocused;
                        _detector.StopMonitoring();
                        _detector.Dispose();
                        _detector = null;
                    }

                    // 最小化されたウィンドウがあれば復元（手動制御に変更）
                    // 復元処理は RestoreWindowsManually メソッドで行う

                    IsMonitoring = false;
                    _logger.LogInfo("監視を停止しました");
                    MonitoringStateChanged?.Invoke(this, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ErrorMessages.MonitoringStopError, ex);
                    ErrorOccurred?.Invoke(this, ErrorMessages.MonitoringStopError);
                }
            }
        }

        /// <summary>
        /// 設定を更新
        /// </summary>
        /// <param name="newSettings">新しい設定</param>
        public void UpdateSettings(AppSettings newSettings)
        {
            lock (_lockObject)
            {
                if (newSettings == null)
                {
                    throw new ArgumentNullException(nameof(newSettings));
                }

                var oldSettings = CurrentSettings;
                CurrentSettings = newSettings;

                // 監視中の場合は部分更新を試行
                if (IsMonitoring && _detector != null)
                {
                    try
                    {
                        // 監視間隔のみ変更の場合
                        if (oldSettings.MonitorInterval != newSettings.MonitorInterval)
                        {
                            _detector.UpdateInterval(newSettings.MonitorInterval);
                            _logger.LogInfo($"監視間隔を更新しました: {newSettings.MonitorInterval}ms");
                        }

                        // 対象プロセスのみ変更の場合
                        if (!oldSettings.TargetProcesses.SequenceEqual(newSettings.TargetProcesses))
                        {
                            _detector.UpdateTargetProcesses(newSettings.TargetProcesses);
                            _logger.LogInfo($"監視対象プロセスを更新しました: {string.Join(", ", newSettings.TargetProcesses)}");
                        }

                        // その他の設定変更時のみ再起動
                        if (NeedsRestart(oldSettings, newSettings))
                        {
                            _logger.LogInfo("設定変更により監視サービスを再起動します");
                            StopMonitoring();
                            StartMonitoring();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"設定更新中にエラーが発生しました: {ex.Message}", ex);
                        // エラー時は完全再起動を試行
                        try
                        {
                            StopMonitoring();
                            StartMonitoring();
                        }
                        catch (Exception restartEx)
                        {
                            _logger.LogError($"監視サービス再起動中にエラーが発生しました: {restartEx.Message}", restartEx);
                            ErrorOccurred?.Invoke(this, "設定更新中にエラーが発生しました");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 設定変更で再起動が必要かどうかを判定
        /// </summary>
        /// <param name="oldSettings">古い設定</param>
        /// <param name="newSettings">新しい設定</param>
        /// <returns>再起動が必要な場合true</returns>
        private bool NeedsRestart(AppSettings oldSettings, AppSettings newSettings)
        {
            // 監視間隔と対象プロセス以外の変更がある場合のみ再起動
            return oldSettings.StartWithWindows != newSettings.StartWithWindows ||
                   oldSettings.RestoreOnSettingsClosed != newSettings.RestoreOnSettingsClosed ||
                   oldSettings.RestoreOnAppExit != newSettings.RestoreOnAppExit ||
                   oldSettings.UseDarkTheme != newSettings.UseDarkTheme;
        }

        /// <summary>
        /// 手動でウィンドウを復元
        /// </summary>
        /// <returns>復元したウィンドウ数</returns>
        public int RestoreWindowsManually()
        {
            lock (_lockObject)
            {
                var restoredCount = _minimizer.RestoreMinimizedWindows();
                if (restoredCount > 0)
                {
                    WindowsRestored?.Invoke(this, restoredCount);
                }
                return restoredCount;
            }
        }

        /// <summary>
        /// 監視統計情報を取得
        /// </summary>
        /// <returns>監視統計情報</returns>
        public MonitoringStats GetStats()
        {
            lock (_lockObject)
            {
                return new MonitoringStats
                {
                    IsMonitoring = IsMonitoring,
                    LastCheckTime = LastCheckTime,
                    MinimizedWindowCount = _minimizer.MinimizedWindowCount,
                    TargetProcessCount = CurrentSettings.TargetProcesses.Count,
                    MonitorInterval = CurrentSettings.MonitorInterval
                };
            }
        }

        #endregion

        #region イベントハンドラー

        /// <summary>
        /// 全画面状態変更イベントハンドラー
        /// </summary>
        private void Detector_FullScreenStateChanged(object? sender, FullScreenStateChangedEventArgs e)
        {
            try
            {
                LastCheckTime = DateTime.Now;

                if (e.IsFullScreen)
                {
                    _logger.LogInfo($"全画面状態を検出: {e.ProcessName} ({e.WindowTitle})");
                    // 全画面になった場合：同一モニター上のウィンドウを最小化
                    var minimizedCount = _minimizer.MinimizeWindowsOnMonitor(e.MonitorHandle, e.WindowHandle);
                    if (minimizedCount > 0)
                    {
                        _logger.LogInfo($"{minimizedCount}個のウィンドウを最小化しました");
                        WindowsMinimized?.Invoke(this, minimizedCount);
                    }
                }
                else
                {
                    _logger.LogInfo("全画面状態が解除されました");
                    // 全画面が解除された場合：設定に基づいて最小化したウィンドウを復元
                    if (CurrentSettings.RestoreOnFullScreenExit)
                    {
                        var restoredCount = _minimizer.RestoreMinimizedWindows();
                        if (restoredCount > 0)
                        {
                            _logger.LogInfo($"{restoredCount}個のウィンドウを復元しました");
                            WindowsRestored?.Invoke(this, restoredCount);
                        }
                    }
                    else
                    {
                        _logger.LogInfo("全画面解除時の自動復元が無効のため、ウィンドウを復元しません");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("全画面状態変更処理中にエラーが発生しました", ex);
                ErrorOccurred?.Invoke(this, "全画面状態変更処理中にエラーが発生しました");
            }
        }

        /// <summary>
        /// 対象プロセスフォーカスイベントハンドラー
        /// </summary>
        private void Detector_TargetProcessFocused(object? sender, FullScreenStateChangedEventArgs e)
        {
            try
            {
                LastCheckTime = DateTime.Now;

                _logger.LogInfo($"対象プロセスがフォーカスされました: {e.ProcessName} ({e.WindowTitle})");

                // 同一モニター上の対象プロセス以外のウィンドウを最小化
                var minimizedCount = _minimizer.MinimizeAllNonTargetWindows(e.WindowHandle, e.MonitorHandle);
                if (minimizedCount > 0)
                {
                    _logger.LogInfo($"{minimizedCount}個の対象外ウィンドウを最小化しました");
                    WindowsMinimized?.Invoke(this, minimizedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("対象プロセスフォーカス処理中にエラーが発生しました", ex);
                ErrorOccurred?.Invoke(this, "対象プロセスフォーカス処理中にエラーが発生しました");
            }
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
                    StopMonitoring();
                }

                _disposed = true;
            }
        }

        #endregion
    }

}
