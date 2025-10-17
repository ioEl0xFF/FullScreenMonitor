using System;
using System.IO;
using System.Text.Json;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// 設定ファイル管理クラス
    /// </summary>
    public class SettingsManager : ISettingsManager
    {
        #region フィールド

        private readonly ILogger _logger;
        private readonly string _settingsDirectory;
        private readonly string _settingsFilePath;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger">ロガー</param>
        public SettingsManager(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                AppConstants.SettingsDirectoryName);

            _settingsFilePath = Path.Combine(_settingsDirectory, AppConstants.SettingsFileName);
        }

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    _logger.LogInfo("設定ファイルが存在しません。デフォルト設定を作成します。");
                    // 設定ファイルが存在しない場合はデフォルト設定を作成
                    var defaultSettings = AppSettings.GetDefault();
                    SaveSettings(defaultSettings);
                    return defaultSettings;
                }

                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                if (settings == null || !settings.IsValid())
                {
                    _logger.LogWarning("設定ファイルが無効です。デフォルト設定を使用します。");
                    // 設定が無効な場合はデフォルト設定を返す
                    return AppSettings.GetDefault();
                }

                _logger.LogInfo("設定を正常に読み込みました。");
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.SettingsLoadError, ex);
                throw new SettingsException(ErrorMessages.SettingsLoadError, ex);
            }
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public bool SaveSettings(AppSettings settings)
        {
            try
            {
                if (settings == null)
                {
                    throw new ArgumentNullException(nameof(settings));
                }

                if (!settings.IsValid())
                {
                    throw new ArgumentException(ErrorMessages.SettingsValidationError, nameof(settings));
                }

                // 設定ディレクトリが存在しない場合は作成
                if (!Directory.Exists(_settingsDirectory))
                {
                    Directory.CreateDirectory(_settingsDirectory);
                    _logger.LogInfo($"設定ディレクトリを作成しました: {_settingsDirectory}");
                }

                // JSONシリアライズオプション
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);

                _logger.LogInfo("設定を正常に保存しました。");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.SettingsSaveError, ex);
                throw new SettingsException(ErrorMessages.SettingsSaveError, ex);
            }
        }

        /// <summary>
        /// 設定ファイルの存在確認
        /// </summary>
        public bool SettingsFileExists()
        {
            return File.Exists(_settingsFilePath);
        }

        /// <summary>
        /// 設定ファイルを削除
        /// </summary>
        public bool DeleteSettingsFile()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                    _logger.LogInfo("設定ファイルを削除しました。");
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorMessages.SettingsDeleteError, ex);
                throw new SettingsException(ErrorMessages.SettingsDeleteError, ex);
            }
        }

        /// <summary>
        /// 設定ディレクトリのパスを取得
        /// </summary>
        public string GetSettingsDirectoryPath()
        {
            return _settingsDirectory;
        }

        /// <summary>
        /// 設定ファイルのパスを取得
        /// </summary>
        public string GetSettingsFilePath()
        {
            return _settingsFilePath;
        }

        #endregion
    }
}
