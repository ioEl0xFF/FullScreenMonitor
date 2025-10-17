using Microsoft.Win32;
using System;
using System.IO;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// Windowsスタートアップ管理クラス
    /// </summary>
    public class StartupManager : IStartupManager
    {
        #region フィールド

        private readonly ILogger _logger;
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public StartupManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// スタートアップに登録
        /// </summary>
        public bool Register()
        {
            try
            {
                var executablePath = GetExecutablePath();
                if (string.IsNullOrEmpty(executablePath))
                {
                    _logger.LogError(ErrorMessages.ExecutablePathGetError);
                    return false;
                }

                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    _logger.LogError("レジストリキーを開けませんでした");
                    return false;
                }

                key.SetValue(AppConstants.StartupRegistryKeyName, executablePath);
                _logger.LogInfo("スタートアップに登録しました");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.StartupRegistrationError, ex);
                throw new FullScreenMonitorException(ErrorMessages.StartupRegistrationError, ex);
            }
        }

        /// <summary>
        /// スタートアップから解除
        /// </summary>
        public bool Unregister()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    _logger.LogError("レジストリキーを開けませんでした");
                    return false;
                }

                if (key.GetValue(AppConstants.StartupRegistryKeyName) != null)
                {
                    key.DeleteValue(AppConstants.StartupRegistryKeyName);
                    _logger.LogInfo("スタートアップから解除しました");
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.StartupUnregistrationError, ex);
                throw new FullScreenMonitorException(ErrorMessages.StartupUnregistrationError, ex);
            }
        }

        /// <summary>
        /// スタートアップに登録されているかどうかを確認
        /// </summary>
        public bool IsRegistered()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null)
                {
                    return false;
                }

                var value = key.GetValue(AppConstants.StartupRegistryKeyName);
                var isRegistered = value != null && !string.IsNullOrEmpty(value.ToString());

                _logger.LogDebug($"スタートアップ登録状態: {isRegistered}");
                return isRegistered;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.StartupCheckError, ex);
                throw new FullScreenMonitorException(ErrorMessages.StartupCheckError, ex);
            }
        }

        /// <summary>
        /// スタートアップ登録状態を切り替え
        /// </summary>
        public bool ToggleRegistration()
        {
            if (IsRegistered())
            {
                return Unregister();
            }
            else
            {
                return Register();
            }
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// 実行ファイルのパスを取得
        /// </summary>
        private string GetExecutablePath()
        {
            try
            {
                var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                var mainModule = currentProcess.MainModule;

                if (mainModule != null)
                {
                    var executablePath = mainModule.FileName;

                    // パスが存在するか確認
                    if (File.Exists(executablePath))
                    {
                        _logger.LogDebug($"実行ファイルパス: {executablePath}");
                        return executablePath;
                    }
                }

                // 代替方法：現在のアセンブリの場所を使用
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var location = assembly.Location;

                if (File.Exists(location))
                {
                    _logger.LogDebug($"アセンブリパス: {location}");
                    return location;
                }

                _logger.LogError("実行ファイルパスを取得できませんでした");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.ExecutablePathGetError, ex);
                throw new FullScreenMonitorException(ErrorMessages.ExecutablePathGetError, ex);
            }
        }

        #endregion
    }
}
