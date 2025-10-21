using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FullScreenMonitor.ViewModels;

/// <summary>
/// ViewModelの基底クラス
/// INotifyPropertyChangedの実装を提供
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    #region イベント

    /// <summary>
    /// プロパティ変更イベント
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region プロテクテッドメソッド

    /// <summary>
    /// プロパティ変更通知を発火
    /// </summary>
    /// <param name="propertyName">プロパティ名（自動取得）</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// プロパティ値を設定し、変更時に通知を発火
    /// </summary>
    /// <typeparam name="T">プロパティの型</typeparam>
    /// <param name="field">フィールドへの参照</param>
    /// <param name="value">新しい値</param>
    /// <param name="propertyName">プロパティ名（自動取得）</param>
    /// <returns>値が変更された場合true</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}
