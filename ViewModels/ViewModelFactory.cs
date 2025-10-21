using System;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;
using FullScreenMonitor.Services;

namespace FullScreenMonitor.ViewModels;

/// <summary>
/// ViewModelファクトリー
/// ViewModelの生成と依存性注入を管理
/// </summary>
public static class ViewModelFactory
{
    /// <summary>
    /// MainViewModelを作成
    /// </summary>
    /// <param name="serviceContainer">サービスコンテナ</param>
    /// <returns>MainViewModel</returns>
    /// <exception cref="ArgumentNullException">serviceContainerがnullの場合</exception>
    /// <exception cref="InvalidOperationException">必要なサービスが登録されていない場合</exception>
    public static MainViewModel CreateMainViewModel(ServiceContainer serviceContainer)
    {
        if (serviceContainer == null)
        {
            throw new ArgumentNullException(nameof(serviceContainer));
        }

        try
        {
            // 必要なサービスを取得
            var logger = serviceContainer.Resolve<ILogger>();
            var settingsManager = serviceContainer.Resolve<ISettingsManager>();
            var startupManager = serviceContainer.Resolve<IStartupManager>();
            var themeService = serviceContainer.Resolve<IThemeService>();
            var notifyIconService = serviceContainer.Resolve<INotifyIconService>();

            // 設定を読み込み
            var settings = settingsManager.LoadSettings();

            // WindowMonitorServiceを作成
            var monitorService = new WindowMonitorService(settings, logger);

            // MainViewModelを作成
            return new MainViewModel(
                logger,
                settingsManager,
                startupManager,
                themeService,
                notifyIconService,
                monitorService
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"MainViewModelの作成に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// SettingsViewModelを作成
    /// </summary>
    /// <param name="serviceContainer">サービスコンテナ</param>
    /// <param name="currentSettings">現在の設定</param>
    /// <param name="monitorService">監視サービス（オプション）</param>
    /// <returns>SettingsViewModel</returns>
    /// <exception cref="ArgumentNullException">serviceContainerまたはcurrentSettingsがnullの場合</exception>
    /// <exception cref="InvalidOperationException">必要なサービスが登録されていない場合</exception>
    public static SettingsViewModel CreateSettingsViewModel(
        ServiceContainer serviceContainer, 
        AppSettings currentSettings, 
        IWindowMonitorService? monitorService = null)
    {
        if (serviceContainer == null)
        {
            throw new ArgumentNullException(nameof(serviceContainer));
        }

        if (currentSettings == null)
        {
            throw new ArgumentNullException(nameof(currentSettings));
        }

        try
        {
            // 必要なサービスを取得
            var logger = serviceContainer.Resolve<ILogger>();
            var settingsManager = serviceContainer.Resolve<ISettingsManager>();
            var startupManager = serviceContainer.Resolve<IStartupManager>();
            var themeService = serviceContainer.Resolve<IThemeService>();
            var processManagementService = serviceContainer.Resolve<IProcessManagementService>();

            // SettingsViewModelを作成
            return new SettingsViewModel(
                logger,
                settingsManager,
                startupManager,
                themeService,
                processManagementService,
                monitorService
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SettingsViewModelの作成に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// SettingsViewModelを作成（設定と監視サービスから、設定も自動読み込み）
    /// </summary>
    /// <param name="serviceContainer">サービスコンテナ</param>
    /// <param name="currentSettings">現在の設定</param>
    /// <param name="monitorService">監視サービス（オプション）</param>
    /// <returns>SettingsViewModel</returns>
    /// <exception cref="ArgumentNullException">引数がnullの場合</exception>
    /// <exception cref="InvalidOperationException">必要なサービスが登録されていない場合</exception>
    public static SettingsViewModel CreateSettingsViewModelWithSettings(
        ServiceContainer serviceContainer, 
        AppSettings currentSettings, 
        IWindowMonitorService? monitorService)
    {
        if (serviceContainer == null)
        {
            throw new ArgumentNullException(nameof(serviceContainer));
        }

        if (currentSettings == null)
        {
            throw new ArgumentNullException(nameof(currentSettings));
        }

        // monitorServiceはオプションなのでnullチェックは不要

        try
        {
            // 必要なサービスを取得
            var logger = serviceContainer.Resolve<ILogger>();
            var settingsManager = serviceContainer.Resolve<ISettingsManager>();
            var startupManager = serviceContainer.Resolve<IStartupManager>();
            var themeService = serviceContainer.Resolve<IThemeService>();
            var processManagementService = serviceContainer.Resolve<IProcessManagementService>();

            // SettingsViewModelを作成
            var viewModel = new SettingsViewModel(
                logger,
                settingsManager,
                startupManager,
                themeService,
                processManagementService,
                monitorService
            );

            // 設定を読み込み
            if (monitorService != null)
            {
                viewModel.LoadSettings(currentSettings, monitorService);
            }

            return viewModel;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"SettingsViewModelの作成に失敗しました: {ex.Message}", ex);
        }
    }
}
