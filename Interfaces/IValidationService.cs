using System.Collections.Generic;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// 入力値検証サービスのインターフェース
/// </summary>
public interface IValidationService
{
    #region メソッド

    /// <summary>
    /// プロセス名の検証
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <param name="existingProcesses">既存のプロセス名リスト</param>
    /// <returns>検証結果</returns>
    Result<string> ValidateProcessName(string processName, List<string> existingProcesses);

    /// <summary>
    /// 監視間隔の検証
    /// </summary>
    /// <param name="interval">監視間隔（ミリ秒）</param>
    /// <returns>検証結果</returns>
    Result<int> ValidateMonitorInterval(int interval);

    /// <summary>
    /// 設定の検証
    /// </summary>
    /// <param name="settings">設定</param>
    /// <returns>検証結果</returns>
    Result<Models.AppSettings> ValidateSettings(Models.AppSettings settings);

    #endregion
}

