using System.Collections.Generic;

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
    ValidationResult ValidateProcessName(string processName, List<string> existingProcesses);

    /// <summary>
    /// 監視間隔の検証
    /// </summary>
    /// <param name="interval">監視間隔（ミリ秒）</param>
    /// <returns>検証結果</returns>
    ValidationResult ValidateMonitorInterval(int interval);

    /// <summary>
    /// 設定の検証
    /// </summary>
    /// <param name="settings">設定</param>
    /// <returns>検証結果</returns>
    ValidationResult ValidateSettings(Models.AppSettings settings);

    #endregion
}

/// <summary>
/// 検証結果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 検証が成功したかどうか
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// 成功時のValidationResultを作成
    /// </summary>
    /// <returns>成功時のValidationResult</returns>
    public static ValidationResult Success()
    {
        return new ValidationResult { IsValid = true };
    }

    /// <summary>
    /// 失敗時のValidationResultを作成
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <returns>失敗時のValidationResult</returns>
    public static ValidationResult Failure(string errorMessage)
    {
        return new ValidationResult { IsValid = false, ErrorMessage = errorMessage };
    }
}
