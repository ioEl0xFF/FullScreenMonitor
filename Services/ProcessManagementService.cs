using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Exceptions;
using FullScreenMonitor.Helpers;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// プロセス管理サービス
/// プロセス一覧の取得、検証、正規化を行う
/// </summary>
public class ProcessManagementService : IProcessManagementService
{
    #region フィールド

    private readonly ILogger _logger;
    private readonly ProcessHelper _processHelper;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public ProcessManagementService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processHelper = new ProcessHelper(_logger);
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// ウィンドウを持つプロセスの一覧を取得
    /// </summary>
    /// <returns>プロセス情報のリスト</returns>
    public List<ProcessInfo> GetProcessesWithWindows()
    {
        try
        {
            var processes = _processHelper.GetProcessesWithWindows();
            _logger.LogInfo($"プロセス一覧を取得しました: {processes.Count}個");
            return processes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ErrorMessages.ProcessListUpdateError, ex);
            throw new MonitoringException(ErrorMessages.ProcessListUpdateError, ex);
        }
    }

    /// <summary>
    /// プロセス名の重複チェック
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <param name="existingProcesses">既存のプロセス名リスト</param>
    /// <returns>重複している場合true</returns>
    public bool IsProcessNameDuplicate(string processName, List<string> existingProcesses)
    {
        if (string.IsNullOrWhiteSpace(processName) || existingProcesses == null)
        {
            return false;
        }

        var normalizedName = NormalizeProcessName(processName);
        return existingProcesses.Any(existing => 
            string.Equals(NormalizeProcessName(existing), normalizedName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// プロセス名の有効性チェック
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns>有効な場合true</returns>
    public bool IsValidProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return false;
        }

        var trimmed = processName.Trim();
        
        // 長さチェック
        if (trimmed.Length < 1 || trimmed.Length > 100)
        {
            return false;
        }

        // 無効な文字チェック
        var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
        if (invalidChars.Any(c => trimmed.Contains(c)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// プロセス名を正規化（小文字化、トリム）
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <returns>正規化されたプロセス名</returns>
    public string NormalizeProcessName(string processName)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            return string.Empty;
        }

        return processName.Trim().ToLowerInvariant();
    }

    #endregion
}
