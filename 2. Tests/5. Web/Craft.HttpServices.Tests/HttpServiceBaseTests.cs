using Craft.Core;

namespace Craft.HttpServices.Tests;

public class HttpServiceBaseTests
{
    public record TestItem(int Id, string Name);

    #region GetAllFromPagedAsync Tests

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsEmptyList_WhenResultIsNull()
    {
        // Arrange
        static Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken _) => Task.FromResult<ServiceResult<PageResponse<TestItem>>?>(null);
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("No result returned", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsEmptyList_WhenPageResponseHasNoItems()
    {
        // Arrange
        var pageResponse = new PageResponse<TestItem>([], 0, 1, 10);
        var pagedResult = ServiceResult<PageResponse<TestItem>>.Success(pageResponse);

        Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken _) => Task.FromResult<ServiceResult<PageResponse<TestItem>>?>(pagedResult);
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsItems_WhenPageResponseHasItems()
    {
        // Arrange
        var items = new List<TestItem> { new(1, "Item1"), new(2, "Item2") };
        var pageResponse = new PageResponse<TestItem>(items, 2, 1, 10);
        var pagedResult = ServiceResult<PageResponse<TestItem>>.Success(pageResponse);

        Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken _) => Task.FromResult<ServiceResult<PageResponse<TestItem>>?>(pagedResult);
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_PropagatesErrors_WhenApiReturnsError()
    {
        // Arrange
        var pagedResult = ServiceResult<PageResponse<TestItem>>.Failure(["API Error"], statusCode: 500);

        Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken _) => Task.FromResult<ServiceResult<PageResponse<TestItem>>?>(pagedResult);
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Errors);
        Assert.Contains("API Error", result.Errors);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_ReturnsFailure_WhenExceptionThrown()
    {
        // Arrange
        static Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken _) => throw new InvalidOperationException("Test exception");
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act
        var result = await HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Test exception", result.ErrorMessage);
    }

    [Fact]
    public async Task GetAllFromPagedAsync_RethrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        
        static Task<ServiceResult<PageResponse<TestItem>>?> getPaged(CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult<ServiceResult<PageResponse<TestItem>>?>(null);
        };
        static List<TestItem> extractItems(PageResponse<TestItem> pr) => pr.Items?.ToList() ?? [];

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => HttpServiceBase.GetAllFromPagedAsync(getPaged, extractItems, cts.Token));
    }

    #endregion
}
