using System;

namespace FullScreenMonitor.Models;

/// <summary>
/// 操作結果を表す汎用的なResult型
/// </summary>
/// <typeparam name="T">成功時の値の型</typeparam>
public class Result<T>
{
    /// <summary>
    /// 操作が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 操作が失敗したかどうか
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// 成功時の値
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// 例外情報（オプション）
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="value">値</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="exception">例外</param>
    private Result(bool isSuccess, T value, string errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage ?? string.Empty;
        Exception = exception;
    }

    /// <summary>
    /// 成功時のResultを作成
    /// </summary>
    /// <param name="value">成功時の値</param>
    /// <returns>成功時のResult</returns>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty, null);
    }

    /// <summary>
    /// 失敗時のResultを作成
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    /// <returns>失敗時のResult</returns>
    public static Result<T> Failure(string errorMessage, Exception? exception = null)
    {
        return new Result<T>(false, default!, errorMessage, exception);
    }

    /// <summary>
    /// 失敗時のResultを作成（値なし）
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    /// <returns>失敗時のResult</returns>
    public static Result<T> Failure<TValue>(string errorMessage, Exception? exception = null)
    {
        return new Result<T>(false, default!, errorMessage, exception);
    }

    /// <summary>
    /// 値の型変換をサポート
    /// </summary>
    /// <param name="result">変換元のResult</param>
    /// <returns>変換後のResult</returns>
    public static implicit operator Result<T>(Result result)
    {
        if (result.IsSuccess)
        {
            return new Result<T>(true, default!, string.Empty, null);
        }
        else
        {
            return new Result<T>(false, default!, result.ErrorMessage, result.Exception);
        }
    }

    /// <summary>
    /// 値を取得（成功時のみ）
    /// </summary>
    /// <returns>値</returns>
    /// <exception cref="InvalidOperationException">失敗時に呼び出された場合</exception>
    public T GetValue()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException($"Result is in failure state: {ErrorMessage}");
        }
        return Value;
    }

    /// <summary>
    /// 値を取得、失敗時はデフォルト値を返す
    /// </summary>
    /// <param name="defaultValue">デフォルト値</param>
    /// <returns>値またはデフォルト値</returns>
    public T GetValueOrDefault(T defaultValue)
    {
        return IsSuccess ? Value : defaultValue;
    }

    /// <summary>
    /// 成功時にアクションを実行
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <returns>このResult</returns>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Value);
        }
        return this;
    }

    /// <summary>
    /// 失敗時にアクションを実行
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <returns>このResult</returns>
    public Result<T> OnFailure(Action<string, Exception?> action)
    {
        if (IsFailure)
        {
            action(ErrorMessage, Exception);
        }
        return this;
    }

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>文字列表現</returns>
    public override string ToString()
    {
        return IsSuccess 
            ? $"Success: {Value}" 
            : $"Failure: {ErrorMessage}";
    }
}

/// <summary>
/// 値なしのResult型
/// </summary>
public class Result
{
    /// <summary>
    /// 操作が成功したかどうか
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// 操作が失敗したかどうか
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// 例外情報（オプション）
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// プライベートコンストラクタ
    /// </summary>
    /// <param name="isSuccess">成功フラグ</param>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="exception">例外</param>
    private Result(bool isSuccess, string errorMessage, Exception? exception)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage ?? string.Empty;
        Exception = exception;
    }

    /// <summary>
    /// 成功時のResultを作成
    /// </summary>
    /// <returns>成功時のResult</returns>
    public static Result Success()
    {
        return new Result(true, string.Empty, null);
    }

    /// <summary>
    /// 失敗時のResultを作成
    /// </summary>
    /// <param name="errorMessage">エラーメッセージ</param>
    /// <param name="exception">例外（オプション）</param>
    /// <returns>失敗時のResult</returns>
    public static Result Failure(string errorMessage, Exception? exception = null)
    {
        return new Result(false, errorMessage, exception);
    }

    /// <summary>
    /// 成功時にアクションを実行
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <returns>このResult</returns>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }
        return this;
    }

    /// <summary>
    /// 失敗時にアクションを実行
    /// </summary>
    /// <param name="action">実行するアクション</param>
    /// <returns>このResult</returns>
    public Result OnFailure(Action<string, Exception?> action)
    {
        if (IsFailure)
        {
            action(ErrorMessage, Exception);
        }
        return this;
    }

    /// <summary>
    /// 文字列表現
    /// </summary>
    /// <returns>文字列表現</returns>
    public override string ToString()
    {
        return IsSuccess 
            ? "Success" 
            : $"Failure: {ErrorMessage}";
    }
}
