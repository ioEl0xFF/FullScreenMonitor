namespace FullScreenMonitor.Services;

/// <summary>
/// サービスライフタイム
/// </summary>
public enum ServiceLifetime
{
    /// <summary>
    /// シングルトン（アプリケーション全体で1つのインスタンス）
    /// </summary>
    Singleton,

    /// <summary>
    /// スコープ（スコープ内で1つのインスタンス）
    /// </summary>
    Scoped,

    /// <summary>
    /// トランジェント（毎回新しいインスタンス）
    /// </summary>
    Transient
}
