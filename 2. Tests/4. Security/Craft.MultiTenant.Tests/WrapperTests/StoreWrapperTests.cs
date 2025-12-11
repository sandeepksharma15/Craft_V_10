using Craft.MultiTenant.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Craft.MultiTenant.Tests.WrapperTests;

public class StoreWrapperTests
{
    private readonly ILogger<StoreWrapperTests> _logger = NullLogger<StoreWrapperTests>.Instance;

    [Fact]
    public async Task GetByIdentifierAsync_CallsUnderlyingStore()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        mockStore.Setup(s => s.GetByIdentifierAsync("test", false, default))
            .ReturnsAsync(tenant);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        var result = await wrapper.GetByIdentifierAsync("test");
        
        Assert.NotNull(result);
        Assert.Equal("test", result.Identifier);
        mockStore.Verify(s => s.GetByIdentifierAsync("test", false, default), Times.Once);
    }

    [Fact]
    public async Task GetByIdentifierAsync_LogsDebugOnSuccess()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var mockLogger = new Mock<ILogger>();
        var tenant = new Tenant { Id = 1, Identifier = "test", Name = "Test" };
        mockStore.Setup(s => s.GetByIdentifierAsync("test", false, default))
            .ReturnsAsync(tenant);
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, mockLogger.Object);
        await wrapper.GetByIdentifierAsync("test");
        
        mockLogger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Tenant found")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdentifierAsync_ThrowsOnNullIdentifier()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => wrapper.GetByIdentifierAsync(null!));
    }

    [Fact]
    public async Task GetHostAsync_CallsUnderlyingStore()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var tenant = new Tenant { Id = 1, Identifier = "host", Name = "Host", Type = TenantType.Host };
        mockStore.Setup(s => s.GetHostAsync(false, default))
            .ReturnsAsync(tenant);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        var result = await wrapper.GetHostAsync();
        
        Assert.NotNull(result);
        Assert.Equal(TenantType.Host, result.Type);
        mockStore.Verify(s => s.GetHostAsync(false, default), Times.Once);
    }

    [Fact]
    public async Task AddAsync_PreventsHostDuplicates()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var existingHost = new Tenant { Id = 1, Identifier = "host1", Type = TenantType.Host };
        var newHost = new Tenant { Id = 2, Identifier = "host2", Type = TenantType.Host };
        
        mockStore.Setup(s => s.GetAsync(2, false, default)).ReturnsAsync((Tenant?)null);
        mockStore.Setup(s => s.GetByIdentifierAsync("host2", false, default)).ReturnsAsync((Tenant?)null);
        mockStore.Setup(s => s.GetHostAsync(false, default)).ReturnsAsync(existingHost);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        
        await Assert.ThrowsAsync<MultiTenantException>(
            () => wrapper.AddAsync(newHost));
    }

    [Fact]
    public async Task AddAsync_PreventsDuplicateIdentifier()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var existing = new Tenant { Id = 1, Identifier = "test", Name = "Existing" };
        var newTenant = new Tenant { Id = 2, Identifier = "test", Name = "New" };
        
        mockStore.Setup(s => s.GetAsync(2, false, default)).ReturnsAsync((Tenant?)null);
        mockStore.Setup(s => s.GetByIdentifierAsync("test", false, default)).ReturnsAsync(existing);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        
        await Assert.ThrowsAsync<MultiTenantException>(
            () => wrapper.AddAsync(newTenant));
    }

    [Fact]
    public async Task UpdateAsync_PreventsDuplicateIdentifier()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        var existing = new Tenant { Id = 1, Identifier = "test1", Name = "Tenant 1" };
        var other = new Tenant { Id = 2, Identifier = "test2", Name = "Tenant 2" };
        var updated = new Tenant { Id = 1, Identifier = "test2", Name = "Updated" };
        
        mockStore.Setup(s => s.GetByIdentifierAsync("test2", false, default)).ReturnsAsync(other);
        mockStore.Setup(s => s.GetAsync(1, false, default)).ReturnsAsync(existing);
        
        var wrapper = new StoreWrapper<Tenant>(mockStore.Object, _logger);
        
        await Assert.ThrowsAsync<MultiTenantException>(
            () => wrapper.UpdateAsync(updated));
    }

    [Fact]
    public void Constructor_ThrowsOnNullStore()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StoreWrapper<Tenant>(null!, _logger));
    }

    [Fact]
    public void Constructor_ThrowsOnNullLogger()
    {
        var mockStore = new Mock<ITenantStore<Tenant>>();
        
        Assert.Throws<ArgumentNullException>(
            () => new StoreWrapper<Tenant>(mockStore.Object, null!));
    }
}
