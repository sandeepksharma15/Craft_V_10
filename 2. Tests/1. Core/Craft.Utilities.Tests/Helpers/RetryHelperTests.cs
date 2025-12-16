using Craft.Utilities.Helpers;

namespace Craft.Utilities.Tests.Helpers;

public class RetryHelperTests
{
    #region Retry<T> Tests

    [Fact]
    public void Retry_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = RetryHelper.Retry(() => expectedValue);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void Retry_FailsOnceSucceedsSecond_ReturnsResult()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = RetryHelper.Retry(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return expectedValue;
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public void Retry_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.Retry<int>(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void Retry_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RetryHelper.Retry<int>(null!, maxAttempts: 3, delayMs: 1000));
    }

    [Fact]
    public void Retry_InvalidMaxAttempts_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RetryHelper.Retry(() => 42, maxAttempts: 0, delayMs: 1000));
    }

    [Fact]
    public void Retry_NegativeDelay_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RetryHelper.Retry(() => 42, maxAttempts: 3, delayMs: -1));
    }

    #endregion

    #region Retry (void) Tests

    [Fact]
    public void Retry_Void_SuccessfulOperation_Executes()
    {
        // Arrange
        var executed = false;

        // Act
        RetryHelper.Retry(() => executed = true);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void Retry_Void_FailsOnceSucceedsSecond_Executes()
    {
        // Arrange
        var attemptCount = 0;

        // Act
        RetryHelper.Retry(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public void Retry_Void_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.Retry(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public void Retry_Void_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RetryHelper.Retry(null!, maxAttempts: 3, delayMs: 1000));
    }

    #endregion

    #region RetryAsync<T> Tests

    [Fact]
    public async Task RetryAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryAsync(() => Task.FromResult(expectedValue));

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task RetryAsync_FailsOnceSucceedsSecond_ReturnsResult()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryAsync(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return Task.FromResult(expectedValue);
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task RetryAsync_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RetryHelper.RetryAsync<int>(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public async Task RetryAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await RetryHelper.RetryAsync(() => Task.FromResult(42), cancellationToken: cts.Token));
    }

    [Fact]
    public async Task RetryAsync_CancellationRequestedDuringRetry_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var attemptCount = 0;

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await RetryHelper.RetryAsync(() =>
            {
                attemptCount++;
                if (attemptCount == 2)
                    cts.Cancel();
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 5, delayMs: 10, cancellationToken: cts.Token));

        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task RetryAsync_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await RetryHelper.RetryAsync<int>(null!, maxAttempts: 3, delayMs: 1000));
    }

    #endregion

    #region RetryAsync (void) Tests

    [Fact]
    public async Task RetryAsync_Void_SuccessfulOperation_Executes()
    {
        // Arrange
        var executed = false;

        // Act
        await RetryHelper.RetryAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public async Task RetryAsync_Void_FailsOnceSucceedsSecond_Executes()
    {
        // Arrange
        var attemptCount = 0;

        // Act
        await RetryHelper.RetryAsync(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return Task.CompletedTask;
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task RetryAsync_Void_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RetryHelper.RetryAsync(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public async Task RetryAsync_Void_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await RetryHelper.RetryAsync(() => Task.CompletedTask, cancellationToken: cts.Token));
    }

    #endregion

    #region RetryOnException<T, TException> Tests

    [Fact]
    public void RetryOnException_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = RetryHelper.RetryOnException<int, InvalidOperationException>(() => expectedValue);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void RetryOnException_ThrowsSpecificException_Retries()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = RetryHelper.RetryOnException<int, InvalidOperationException>(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return expectedValue;
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public void RetryOnException_ThrowsDifferentException_ThrowsImmediately()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RetryHelper.RetryOnException<int, InvalidOperationException>(() =>
            {
                attemptCount++;
                throw new ArgumentException("Different exception");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public void RetryOnException_AllAttemptsFailWithSpecificException_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.RetryOnException<int, ArgumentException>(() =>
            {
                attemptCount++;
                throw new ArgumentException("Operation failed");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public void RetryOnException_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RetryHelper.RetryOnException<int, InvalidOperationException>(null!, maxAttempts: 3, delayMs: 1000));
    }

    #endregion

    #region RetryOnExceptionAsync<T, TException> Tests

    [Fact]
    public async Task RetryOnExceptionAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryOnExceptionAsync<int, InvalidOperationException>(
            () => Task.FromResult(expectedValue));

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task RetryOnExceptionAsync_ThrowsSpecificException_Retries()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryOnExceptionAsync<int, InvalidOperationException>(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return Task.FromResult(expectedValue);
        }, maxAttempts: 3, delayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task RetryOnExceptionAsync_ThrowsDifferentException_ThrowsImmediately()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await RetryHelper.RetryOnExceptionAsync<int, InvalidOperationException>(() =>
            {
                attemptCount++;
                throw new ArgumentException("Different exception");
            }, maxAttempts: 3, delayMs: 10));

        Assert.Equal(1, attemptCount);
    }

    [Fact]
    public async Task RetryOnExceptionAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await RetryHelper.RetryOnExceptionAsync<int, InvalidOperationException>(
                () => Task.FromResult(42),
                cancellationToken: cts.Token));
    }

    [Fact]
    public async Task RetryOnExceptionAsync_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await RetryHelper.RetryOnExceptionAsync<int, InvalidOperationException>(
                null!, maxAttempts: 3, delayMs: 1000));
    }

    #endregion

    #region RetryWithExponentialBackoff<T> Tests

    [Fact]
    public void RetryWithExponentialBackoff_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = RetryHelper.RetryWithExponentialBackoff(() => expectedValue);

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void RetryWithExponentialBackoff_FailsOnceSucceedsSecond_ReturnsResult()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = RetryHelper.RetryWithExponentialBackoff(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return expectedValue;
        }, maxAttempts: 3, initialDelayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public void RetryWithExponentialBackoff_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.RetryWithExponentialBackoff<int>(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, initialDelayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public void RetryWithExponentialBackoff_DelaysIncrease_ExponentiallyGrows()
    {
        // Arrange
        var attemptCount = 0;
        var delays = new List<long>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var lastTime = sw.ElapsedMilliseconds;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.RetryWithExponentialBackoff<int>(() =>
            {
                attemptCount++;
                if (attemptCount > 1)
                {
                    var currentTime = sw.ElapsedMilliseconds;
                    delays.Add(currentTime - lastTime);
                    lastTime = currentTime;
                }
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 4, initialDelayMs: 100, maxDelayMs: 10000));

        // Assert delays are increasing (allowing for some timing variance)
        Assert.Equal(4, attemptCount);
        Assert.Equal(3, delays.Count);
        Assert.True(delays[1] > delays[0], $"Second delay {delays[1]} should be greater than first {delays[0]}");
        Assert.True(delays[2] > delays[1], $"Third delay {delays[2]} should be greater than second {delays[1]}");
    }

    [Fact]
    public void RetryWithExponentialBackoff_MaxDelay_CapsDelay()
    {
        // Arrange
        var attemptCount = 0;
        var delays = new List<long>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var lastTime = sw.ElapsedMilliseconds;

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            RetryHelper.RetryWithExponentialBackoff<int>(() =>
            {
                attemptCount++;
                if (attemptCount > 1)
                {
                    var currentTime = sw.ElapsedMilliseconds;
                    delays.Add(currentTime - lastTime);
                    lastTime = currentTime;
                }
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 10, initialDelayMs: 100, maxDelayMs: 200));

        // Assert all delays respect the max delay cap (with tolerance for timing variance)
        Assert.Equal(10, attemptCount);
        Assert.All(delays, delay => Assert.True(delay <= 300, $"Delay {delay} exceeded max with tolerance"));
    }

    [Fact]
    public void RetryWithExponentialBackoff_InvalidMaxDelayLessThanInitial_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RetryHelper.RetryWithExponentialBackoff(() => 42,
                maxAttempts: 3,
                initialDelayMs: 1000,
                maxDelayMs: 500));
    }

    [Fact]
    public void RetryWithExponentialBackoff_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            RetryHelper.RetryWithExponentialBackoff<int>(null!, maxAttempts: 3, initialDelayMs: 1000));
    }

    #endregion

    #region RetryWithExponentialBackoffAsync<T> Tests

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_SuccessfulOperation_ReturnsResult()
    {
        // Arrange
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryWithExponentialBackoffAsync(() => Task.FromResult(expectedValue));

        // Assert
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_FailsOnceSucceedsSecond_ReturnsResult()
    {
        // Arrange
        var attemptCount = 0;
        var expectedValue = 42;

        // Act
        var result = await RetryHelper.RetryWithExponentialBackoffAsync(() =>
        {
            attemptCount++;
            if (attemptCount < 2)
                throw new InvalidOperationException("First attempt fails");
            return Task.FromResult(expectedValue);
        }, maxAttempts: 3, initialDelayMs: 10);

        // Assert
        Assert.Equal(expectedValue, result);
        Assert.Equal(2, attemptCount);
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_AllAttemptsFail_ThrowsInvalidOperationException()
    {
        // Arrange
        var attemptCount = 0;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync<int>(() =>
            {
                attemptCount++;
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 3, initialDelayMs: 10));

        Assert.Equal(3, attemptCount);
        Assert.Contains("Operation failed after 3 attempts", exception.Message);
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync(
                () => Task.FromResult(42),
                cancellationToken: cts.Token));
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_DelaysIncrease_ExponentiallyGrows()
    {
        // Arrange
        var attemptCount = 0;
        var delays = new List<long>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var lastTime = sw.ElapsedMilliseconds;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync<int>(() =>
            {
                attemptCount++;
                if (attemptCount > 1)
                {
                    var currentTime = sw.ElapsedMilliseconds;
                    delays.Add(currentTime - lastTime);
                    lastTime = currentTime;
                }
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 4, initialDelayMs: 100, maxDelayMs: 10000));

        // Assert delays are increasing (allowing for some timing variance)
        Assert.Equal(4, attemptCount);
        Assert.Equal(3, delays.Count);
        Assert.True(delays[1] > delays[0], $"Second delay {delays[1]} should be greater than first {delays[0]}");
        Assert.True(delays[2] > delays[1], $"Third delay {delays[2]} should be greater than second {delays[1]}");
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_MaxDelay_CapsDelay()
    {
        // Arrange
        var attemptCount = 0;
        var delays = new List<long>();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var lastTime = sw.ElapsedMilliseconds;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync<int>(() =>
            {
                attemptCount++;
                if (attemptCount > 1)
                {
                    var currentTime = sw.ElapsedMilliseconds;
                    delays.Add(currentTime - lastTime);
                    lastTime = currentTime;
                }
                throw new InvalidOperationException("Operation failed");
            }, maxAttempts: 10, initialDelayMs: 100, maxDelayMs: 200));

        // Assert all delays respect the max delay cap (with tolerance for timing variance)
        Assert.Equal(10, attemptCount);
        Assert.All(delays, delay => Assert.True(delay <= 300, $"Delay {delay} exceeded max with tolerance"));
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_InvalidMaxDelayLessThanInitial_ThrowsArgumentOutOfRangeException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync(() => Task.FromResult(42),
                maxAttempts: 3,
                initialDelayMs: 1000,
                maxDelayMs: 500));
    }

    [Fact]
    public async Task RetryWithExponentialBackoffAsync_NullAction_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await RetryHelper.RetryWithExponentialBackoffAsync<int>(
                null!, maxAttempts: 3, initialDelayMs: 1000));
    }

    #endregion
}
