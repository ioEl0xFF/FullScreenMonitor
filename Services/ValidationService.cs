using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Constants;
using FullScreenMonitor.Interfaces;
using FullScreenMonitor.Models;

namespace FullScreenMonitor.Services;

/// <summary>
/// 入力値検証サービス
/// 各種入力値の検証ロジックを集約
/// </summary>
public class ValidationService : IValidationService
{
    #region フィールド

    private readonly ILogger _logger;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="logger">ロガー</param>
    public ValidationService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// プロセス名の検証
    /// </summary>
    /// <param name="processName">プロセス名</param>
    /// <param name="existingProcesses">既存のプロセス名リスト</param>
    /// <returns>検証結果</returns>
    public Result<string> ValidateProcessName(string processName, List<string> existingProcesses)
    {
        try
        {
            // 空文字チェック
            if (string.IsNullOrWhiteSpace(processName))
            {
                return Result<string>.Failure(ErrorMessages.ProcessNameInputError);
            }

            var trimmed = processName.Trim();

            // 長さチェック
            if (trimmed.Length < 1 || trimmed.Length > 100)
            {
                return Result<string>.Failure("プロセス名は1文字以上100文字以下で入力してください。");
            }

            // 無効な文字チェック
            var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
            if (invalidChars.Any(c => trimmed.Contains(c)))
            {
                return Result<string>.Failure("プロセス名に無効な文字が含まれています。");
            }

            // 重複チェック
            if (existingProcesses != null)
            {
                var normalizedName = trimmed.ToLowerInvariant();
                if (existingProcesses.Any(existing => 
                    string.Equals(existing.ToLowerInvariant(), normalizedName, StringComparison.OrdinalIgnoreCase)))
                {
                    return Result<string>.Failure(ErrorMessages.ProcessNameDuplicateError);
                }
            }

            return Result<string>.Success(trimmed);
        }
        catch (Exception ex)
        {
            _logger.LogError($"プロセス名検証エラー: {ex.Message}", ex);
            return Result<string>.Failure("プロセス名の検証中にエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 監視間隔の検証
    /// </summary>
    /// <param name="interval">監視間隔（ミリ秒）</param>
    /// <returns>検証結果</returns>
    public Result<int> ValidateMonitorInterval(int interval)
    {
        try
        {
            if (interval < MonitorConstants.MinMonitorInterval)
            {
                return Result<int>.Failure($"監視間隔は{MonitorConstants.MinMonitorInterval}ms以上で設定してください。");
            }

            if (interval > MonitorConstants.MaxMonitorInterval)
            {
                return Result<int>.Failure($"監視間隔は{MonitorConstants.MaxMonitorInterval}ms以下で設定してください。");
            }

            return Result<int>.Success(interval);
        }
        catch (Exception ex)
        {
            _logger.LogError($"監視間隔検証エラー: {ex.Message}", ex);
            return Result<int>.Failure("監視間隔の検証中にエラーが発生しました。", ex);
        }
    }

    /// <summary>
    /// 設定の検証
    /// </summary>
    /// <param name="settings">設定</param>
    /// <returns>検証結果</returns>
    public Result<AppSettings> ValidateSettings(AppSettings settings)
    {
        try
        {
            if (settings == null)
            {
                return Result<AppSettings>.Failure("設定がnullです。");
            }

            // 監視間隔の検証
            var intervalResult = ValidateMonitorInterval(settings.MonitorInterval);
            if (intervalResult.IsFailure)
            {
                return Result<AppSettings>.Failure(intervalResult.ErrorMessage);
            }

            // プロセス名の検証
            if (settings.TargetProcesses != null)
            {
                foreach (var processName in settings.TargetProcesses)
                {
                    var processResult = ValidateProcessName(processName, new List<string>());
                    if (processResult.IsFailure)
                    {
                        return Result<AppSettings>.Failure($"プロセス名 '{processName}' が無効です: {processResult.ErrorMessage}");
                    }
                }
            }

            return Result<AppSettings>.Success(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定検証エラー: {ex.Message}", ex);
            return Result<AppSettings>.Failure("設定の検証中にエラーが発生しました。", ex);
        }
    }

    #endregion
}
