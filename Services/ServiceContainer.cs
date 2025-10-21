using System;
using System.Collections.Generic;
using System.Linq;
using FullScreenMonitor.Interfaces;

namespace FullScreenMonitor.Services;

/// <summary>
/// 依存性注入コンテナ
/// スコープ管理とIDisposable自動解放機能を提供
/// </summary>
public class ServiceContainer : IDisposable
{
    private readonly Dictionary<Type, object> _services = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly Dictionary<Type, ServiceLifetime> _lifetimes = new();
    private readonly List<IDisposable> _disposables = new();
    private bool _disposed = false;

    /// <summary>
    /// サービスを登録（インスタンス）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <typeparam name="TImplementation">実装型</typeparam>
    /// <param name="instance">インスタンス</param>
    /// <param name="lifetime">サービスライフタイム</param>
    public void RegisterInstance<TInterface, TImplementation>(TImplementation instance, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TImplementation : class, TInterface
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        _services[typeof(TInterface)] = instance;
        _lifetimes[typeof(TInterface)] = lifetime;

        // IDisposableの場合は追跡リストに追加
        if (instance is IDisposable disposable)
        {
            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// サービスを登録（ファクトリー）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    /// <param name="lifetime">サービスライフタイム</param>
    public void RegisterFactory<TInterface>(Func<TInterface> factory, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        _factories[typeof(TInterface)] = () => factory()!;
        _lifetimes[typeof(TInterface)] = lifetime;
    }

    /// <summary>
    /// サービスを登録（シングルトン）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    public void RegisterSingleton<TInterface>(Func<TInterface> factory)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        var lazyInstance = new Lazy<TInterface>(() =>
        {
            var instance = factory();
            
            // IDisposableの場合は追跡リストに追加
            if (instance is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }
            
            return instance;
        });
        
        _factories[typeof(TInterface)] = () => lazyInstance.Value!;
        _lifetimes[typeof(TInterface)] = ServiceLifetime.Singleton;
    }

    /// <summary>
    /// サービスを解決
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <returns>サービスインスタンス</returns>
    public TInterface Resolve<TInterface>()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        var type = typeof(TInterface);

        // インスタンスから解決を試行
        if (_services.TryGetValue(type, out var instance))
        {
            return (TInterface)instance;
        }

        // ファクトリーから解決を試行
        if (_factories.TryGetValue(type, out var factory))
        {
            var resolvedInstance = (TInterface)factory();
            
            // Transientの場合はIDisposableを追跡しない
            if (_lifetimes.TryGetValue(type, out var lifetime) && 
                lifetime == ServiceLifetime.Singleton && 
                resolvedInstance is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }
            
            return resolvedInstance;
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
        if (_disposed)
        {
            return;
        }

        // 登録されているサービスを適切に破棄
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception)
            {
                // 破棄エラーは無視
            }
        }

        _services.Clear();
        _factories.Clear();
        _lifetimes.Clear();
        _disposables.Clear();
    }

    /// <summary>
    /// 循環依存をチェック
    /// </summary>
    /// <param name="type">チェックする型</param>
    /// <param name="visited">訪問済みの型</param>
    /// <returns>循環依存がある場合true</returns>
    private bool HasCircularDependency(Type type, HashSet<Type> visited)
    {
        if (visited.Contains(type))
        {
            return true;
        }

        visited.Add(type);

        // ここでより詳細な循環依存チェックを実装可能
        // 現在は基本的な実装のみ

        visited.Remove(type);
        return false;
    }

    /// <summary>
    /// コンテナを破棄
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// リソースを解放
    /// </summary>
    /// <param name="disposing">マネージリソースを解放するかどうか</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                Clear();
            }

            _disposed = true;
        }
    }
}
