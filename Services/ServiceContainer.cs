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
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly List<IDisposable> _disposables = new();
    private readonly object _lockObject = new();
    private bool _disposed = false;

    /// <summary>
    /// シングルトンサービスを登録（インスタンス）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <typeparam name="TImplementation">実装型</typeparam>
    /// <param name="instance">インスタンス</param>
    public void RegisterSingleton<TInterface, TImplementation>(TImplementation instance)
        where TImplementation : class, TInterface
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        lock (_lockObject)
        {
            _singletons[typeof(TInterface)] = instance;

            // IDisposableの場合は追跡リストに追加
            if (instance is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }
        }
    }

    /// <summary>
    /// シングルトンサービスを登録（ファクトリー）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    public void RegisterSingleton<TInterface>(Func<TInterface> factory)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        lock (_lockObject)
        {
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
        }
    }

    /// <summary>
    /// トランジェントサービスを登録（ファクトリー）
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <param name="factory">ファクトリー関数</param>
    public void RegisterTransient<TInterface>(Func<TInterface> factory)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(ServiceContainer));
        }

        lock (_lockObject)
        {
            _factories[typeof(TInterface)] = () => factory()!;
        }
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

        lock (_lockObject)
        {
            var type = typeof(TInterface);

            // シングルトンから解決を試行
            if (_singletons.TryGetValue(type, out var instance))
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
    }

    /// <summary>
    /// サービスが登録されているかチェック
    /// </summary>
    /// <typeparam name="TInterface">インターフェース型</typeparam>
    /// <returns>登録されている場合true</returns>
    public bool IsRegistered<TInterface>()
    {
        lock (_lockObject)
        {
            var type = typeof(TInterface);
            return _singletons.ContainsKey(type) || _factories.ContainsKey(type);
        }
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

        lock (_lockObject)
        {
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

            _singletons.Clear();
            _factories.Clear();
            _disposables.Clear();
        }
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
