using System.Collections.Generic;
using FullScreenMonitor.Helpers;

namespace FullScreenMonitor.Interfaces;

/// <summary>
/// プロセス管理サービスのインターフェース
/// </summary>
public interface IProcessManagementService
{
    #region メソッド

    /// <summary>
    /// ウィンドウを持つプロセスの一覧を取得
    /// </summary>
    /// <returns>プロセス情報のリスト</returns>
    List<ProcessInfo> GetProcessesWithWindows();

    /// <summary>
    /// プロセス名の重複チェック
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <param name="existingProcesses">既存のプロセス名リスト</param>
    /// <returns>重複している場合true</returns>
    bool IsProcessNameDuplicate(string processName, List<string> existingProcesses);

    /// <summary>
    /// プロセス名の有効性チェック
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns>有効な場合true</returns>
    bool IsValidProcessName(string processName);

    /// <summary>
    /// プロセス名を正規化（小文字化、トリム）
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns>正規化されたプロセス名</returns>
    string NormalizeProcessName(string processName);

    #endregion
}
