using FullScreenMonitor.Models;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// 設定管理サービスインターフェース
/// </summary>
public interface ISettingsManager
{
    /// <summary>
    /// 設定を読み込み
    /// </summary>
    /// <returns>アプリケーション設定</returns>
    AppSettings LoadSettings();

    /// <summary>
    /// 設定を保存
    /// </summary>
    /// <param name="settings">保存する設定</param>
    /// <returns>保存に成功した場合true</returns>
    bool SaveSettings(AppSettings settings);

    /// <summary>
    /// 設定ファイルの存在確認
    /// </summary>
    /// <returns>設定ファイルが存在する場合true</returns>
    bool SettingsFileExists();

    /// <summary>
    /// 設定ファイルを削除
    /// </summary>
    /// <returns>削除に成功した場合true</returns>
    bool DeleteSettingsFile();

    /// <summary>
    /// 設定ディレクトリのパスを取得
    /// </summary>
    /// <returns>設定ディレクトリのパス</returns>
    string GetSettingsDirectoryPath();

    /// <summary>
    /// 設定ファイルのパスを取得
    /// </summary>
    /// <returns>設定ファイルのパス</returns>
    string GetSettingsFilePath();
}
