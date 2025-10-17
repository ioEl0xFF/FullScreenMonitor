using System;
using System.Collections.Generic;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// シンプルな依存性注入コンテナ
/// </summary>
public class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();

    /// <summary>
    /// サービスを登録（インスタンス）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <typeparam name="TImplementation">実装型</typeparam>
    /// <param name="instance">インスタンス</param>
    public void RegisterInstance<TInterface, TImplementation>(TImplementation instance)
        where TImplementation : class, TInterface
    {
        _services[typeof(TInterface)] = instance;
    }

    /// <summary>
    /// サービスを登録（ファクトリー）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    public void RegisterFactory<TInterface>(Func<TInterface> factory)
    {
        _factories[typeof(TInterface)] = () => factory()!;
    }

    /// <summary>
    /// サービスを登録（シングルトン）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    public void RegisterSingleton<TInterface>(Func<TInterface> factory)
    {
        var lazyInstance = new Lazy<TInterface>(factory);
        _factories[typeof(TInterface)] = () => lazyInstance.Value!;
    }

    /// <summary>
    /// サービスを解決
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <returns>サービスインスタンス</returns>
    public TInterface Resolve<TInterface>()
    {
        var type = typeof(TInterface);

        // インスタンスから解決を試行
        if (_services.TryGetValue(type, out var instance))
        {
            return (TInterface)instance;
        }

        // ファクトリーから解決を試行
        if (_factories.TryGetValue(type, out var factory))
        {
            return (TInterface)factory();
        }

        throw new InvalidOperationException($"サービス '{type.Name}' が登録されていません。");
    }

    /// <summary>
    /// サービスが登録されているかチェック
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <returns>登録されている場合true</returns>
    public bool IsRegistered<TInterface>()
    {
        var type = typeof(TInterface);
        return _services.ContainsKey(type) || _factories.ContainsKey(type);
    }

    /// <summary>
    /// すべてのサービスをクリア
    /// </summary>
    public void Clear()
    {
        // 登録されているサービスを適切に破棄
        foreach (var service in _services.Values)
        {
            if (service is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _services.Clear();
        _factories.Clear();
    }

    /// <summary>
    /// コンテナを破棄
    /// </summary>
    public void Dispose()
    {
        Clear();
    }
}
