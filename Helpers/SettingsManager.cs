using System;
using System.IO;
using System.Text.Json;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// 設定ファイル管理クラス
    /// </summary>
    public static class SettingsManager
    {
        #region 定数

        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "FullScreenMonitor");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// 設定を読み込み
        /// </summary>
        public static AppSettings LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    // 設定ファイルが存在しない場合はデフォルト設定を作成
                    var defaultSettings = AppSettings.GetDefault();
                    SaveSettings(defaultSettings);
                    return defaultSettings;
                }

                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                
                if (settings == null || !settings.IsValid())
                {
                    // 設定が無効な場合はデフォルト設定を返す
                    return AppSettings.GetDefault();
                }

                return settings;
            }
            catch (Exception ex)
            {
                // エラーが発生した場合はデフォルト設定を返す
                System.Diagnostics.Debug.WriteLine($"設定読み込みエラー: {ex.Message}");
                return AppSettings.GetDefault();
            }
        }

        /// <summary>
        /// 設定を保存
        /// </summary>
        public static bool SaveSettings(AppSettings settings)
        {
            try
            {
                if (settings == null)
                {
                    throw new ArgumentNullException(nameof(settings));
                }

                // 設定ディレクトリが存在しない場合は作成
                if (!Directory.Exists(SettingsDirectory))
                {
                    Directory.CreateDirectory(SettingsDirectory);
                }

                // JSONシリアライズオプション
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFilePath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定保存エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 設定ファイルの存在確認
        /// </summary>
        public static bool SettingsFileExists()
        {
            return File.Exists(SettingsFilePath);
        }

        /// <summary>
        /// 設定ファイルを削除
        /// </summary>
        public static bool DeleteSettingsFile()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    File.Delete(SettingsFilePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"設定ファイル削除エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 設定ディレクトリのパスを取得
        /// </summary>
        public static string GetSettingsDirectoryPath()
        {
            return SettingsDirectory;
        }

        /// <summary>
        /// 設定ファイルのパスを取得
        /// </summary>
        public static string GetSettingsFilePath()
        {
            return SettingsFilePath;
        }

        #endregion
    }
}
