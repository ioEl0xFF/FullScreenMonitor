using Microsoft.Win32;
using System;
using System.IO;

namespace FullScreenMonitor.Helpers
{
    /// <summary>
    /// Windowsスタートアップ管理クラス
    /// </summary>
    public static class StartupManager
    {
        #region 定数

        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "FullScreenMonitor";

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// スタートアップに登録
        /// </summary>
        public static bool Register()
        {
            try
            {
                var executablePath = GetExecutablePath();
                if (string.IsNullOrEmpty(executablePath))
                {
                    return false;
                }

                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    return false;
                }

                key.SetValue(AppName, executablePath);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"スタートアップ登録エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// スタートアップから解除
        /// </summary>
        public static bool Unregister()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    return false;
                }

                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"スタートアップ解除エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// スタートアップに登録されているかどうかを確認
        /// </summary>
        public static bool IsRegistered()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                if (key == null)
                {
                    return false;
                }

                var value = key.GetValue(AppName);
                return value != null && !string.IsNullOrEmpty(value.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"スタートアップ確認エラー: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// スタートアップ登録状態を切り替え
        /// </summary>
        public static bool ToggleRegistration()
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
        private static string GetExecutablePath()
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
                        return executablePath;
                    }
                }

                // 代替方法：現在のアセンブリの場所を使用
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var location = assembly.Location;
                
                if (File.Exists(location))
                {
                    return location;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"実行ファイルパス取得エラー: {ex.Message}");
                return string.Empty;
            }
        }

        #endregion
    }
}
