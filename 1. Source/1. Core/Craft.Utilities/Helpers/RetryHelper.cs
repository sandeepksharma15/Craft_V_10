using System;
using System.Collections.Generic;
using System.Text;

namespace Craft.Utilities.Helpers;

/// <summary>
/// Provides helper methods for retrying operations with various strategies.
/// </summary>
public static class RetryHelper
{
    /// <summary>
    /// Retries an operation that returns a value with a fixed delay between attempts.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    public static T Retry<T>(Func<T> action, int maxAttempts = 3, int delayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Retries an operation that does not return a value with a fixed delay between attempts.
    /// </summary>
    /// <param name="action">The operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    public static void Retry(Action action, int maxAttempts = 3, int delayMs = 1000)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                action();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Asynchronously retries an operation that returns a value with a fixed delay between attempts.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The asynchronous operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public static async Task<T> RetryAsync<T>(Func<Task<T>> action, int maxAttempts = 3, int delayMs = 1000, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await action();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Asynchronously retries an operation that does not return a value with a fixed delay between attempts.
    /// </summary>
    /// <param name="action">The asynchronous operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public static async Task RetryAsync(Func<Task> action, int maxAttempts = 3, int delayMs = 1000, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await action();
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Retries an operation that returns a value only when a specific exception type is thrown.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <typeparam name="TException">The type of exception to retry on.</typeparam>
    /// <param name="action">The operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail with the specified exception type.</exception>
    public static T RetryOnException<T, TException>(Func<T> action, int maxAttempts = 3, int delayMs = 1000)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (TException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    Thread.Sleep(delayMs);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Asynchronously retries an operation that returns a value only when a specific exception type is thrown.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <typeparam name="TException">The type of exception to retry on.</typeparam>
    /// <param name="action">The asynchronous operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="delayMs">The delay in milliseconds between attempts (default: 1000).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1 or delayMs is negative.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail with the specified exception type.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public static async Task<T> RetryOnExceptionAsync<T, TException>(Func<Task<T>> action, int maxAttempts = 3, int delayMs = 1000, CancellationToken cancellationToken = default)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(delayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await action();
            }
            catch (TException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                    await Task.Delay(delayMs, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Retries an operation that returns a value with exponential backoff delay between attempts.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="initialDelayMs">The initial delay in milliseconds (default: 1000).</param>
    /// <param name="maxDelayMs">The maximum delay in milliseconds (default: 30000).</param>
    /// <returns>The result of the operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1, initialDelayMs is negative, or maxDelayMs is less than initialDelayMs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    public static T RetryWithExponentialBackoff<T>(Func<T> action, int maxAttempts = 3, int initialDelayMs = 1000, int maxDelayMs = 30000)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(initialDelayMs);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDelayMs, initialDelayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = Math.Min(
                        (long)initialDelayMs * (long)Math.Pow(2, attempt - 1),
                        maxDelayMs);
                    Thread.Sleep((int)delayMs);
                }
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }

    /// <summary>
    /// Asynchronously retries an operation that returns a value with exponential backoff delay between attempts.
    /// </summary>
    /// <typeparam name="T">The return type of the operation.</typeparam>
    /// <param name="action">The asynchronous operation to retry.</param>
    /// <param name="maxAttempts">The maximum number of attempts (default: 3).</param>
    /// <param name="initialDelayMs">The initial delay in milliseconds (default: 1000).</param>
    /// <param name="maxDelayMs">The maximum delay in milliseconds (default: 30000).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when maxAttempts is less than 1, initialDelayMs is negative, or maxDelayMs is less than initialDelayMs.</exception>
    /// <exception cref="InvalidOperationException">Thrown when all retry attempts fail.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public static async Task<T> RetryWithExponentialBackoffAsync<T>(Func<Task<T>> action, int maxAttempts = 3, int initialDelayMs = 1000, 
        int maxDelayMs = 30000, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfNegative(initialDelayMs);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDelayMs, initialDelayMs);

        Exception? lastException = null;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < maxAttempts)
                {
                    var delayMs = Math.Min(
                        (long)initialDelayMs * (long)Math.Pow(2, attempt - 1),
                        maxDelayMs);
                    await Task.Delay((int)delayMs, cancellationToken);
                }
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {maxAttempts} attempts.", lastException);
    }
}
