namespace FullScreenMonitor.Interfaces;

/// <summary>
/// スタートアップ管理サービスインターフェース
/// </summary>
public interface IStartupManager
{
    /// <summary>
    /// スタートアップに登録
    /// </summary>
    /// <returns>登録に成功した場合true</returns>
    bool Register();

    /// <summary>
    /// スタートアップから解除
    /// </summary>
    /// <returns>解除に成功した場合true</returns>
    bool Unregister();

    /// <summary>
    /// スタートアップに登録されているかどうかを確認
    /// </summary>
    /// <returns>登録されている場合true</returns>
    bool IsRegistered();

    /// <summary>
    /// スタートアップ登録状態を切り替え
    /// </summary>
    /// <returns>切り替えに成功した場合true</returns>
    bool ToggleRegistration();
}
