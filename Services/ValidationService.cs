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
    public ValidationResult ValidateProcessName(string processName, List<string> existingProcesses)
    {
        try
        {
            // 空文字チェック
            if (string.IsNullOrWhiteSpace(processName))
            {
                return ValidationResult.Failure(ErrorMessages.ProcessNameInputError);
            }

            var trimmed = processName.Trim();

            // 長さチェック
            if (trimmed.Length < 1 || trimmed.Length > 100)
            {
                return ValidationResult.Failure("プロセス名は1文字以上100文字以下で入力してください。");
            }

            // 無効な文字チェック
            var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
            if (invalidChars.Any(c => trimmed.Contains(c)))
            {
                return ValidationResult.Failure("プロセス名に無効な文字が含まれています。");
            }

            // 重複チェック
            if (existingProcesses != null)
            {
                var normalizedName = trimmed.ToLowerInvariant();
                if (existingProcesses.Any(existing => 
                    string.Equals(existing.ToLowerInvariant(), normalizedName, StringComparison.OrdinalIgnoreCase)))
                {
                    return ValidationResult.Failure(ErrorMessages.ProcessNameDuplicateError);
                }
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"プロセス名検証エラー: {ex.Message}", ex);
            return ValidationResult.Failure("プロセス名の検証中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 監視間隔の検証
    /// </summary>
    /// <param name="interval">監視間隔（ミリ秒）</param>
    /// <returns>検証結果</returns>
    public ValidationResult ValidateMonitorInterval(int interval)
    {
        try
        {
            if (interval < MonitorConstants.MinMonitorInterval)
            {
                return ValidationResult.Failure($"監視間隔は{MonitorConstants.MinMonitorInterval}ms以上で設定してください。");
            }

            if (interval > MonitorConstants.MaxMonitorInterval)
            {
                return ValidationResult.Failure($"監視間隔は{MonitorConstants.MaxMonitorInterval}ms以下で設定してください。");
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"監視間隔検証エラー: {ex.Message}", ex);
            return ValidationResult.Failure("監視間隔の検証中にエラーが発生しました。");
        }
    }

    /// <summary>
    /// 設定の検証
    /// </summary>
    /// <param name="settings">設定</param>
    /// <returns>検証結果</returns>
    public ValidationResult ValidateSettings(AppSettings settings)
    {
        try
        {
            if (settings == null)
            {
                return ValidationResult.Failure("設定がnullです。");
            }

            // 監視間隔の検証
            var intervalResult = ValidateMonitorInterval(settings.MonitorInterval);
            if (!intervalResult.IsValid)
            {
                return intervalResult;
            }

            // プロセス名の検証
            if (settings.TargetProcesses != null)
            {
                foreach (var processName in settings.TargetProcesses)
                {
                    var processResult = ValidateProcessName(processName, new List<string>());
                    if (!processResult.IsValid)
                    {
                        return ValidationResult.Failure($"プロセス名 '{processName}' が無効です: {processResult.ErrorMessage}");
                    }
                }
            }

            return ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError($"設定検証エラー: {ex.Message}", ex);
            return ValidationResult.Failure("設定の検証中にエラーが発生しました。");
        }
    }

    #endregion
}
