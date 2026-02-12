namespace Craft.Domain.Tests.Contracts;

public class AggregateRootTests
{
    #region Test Implementations

    private sealed class Order : BaseEntity, IAggregateRoot
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    private sealed class OrderWithGenericKey : BaseEntity<Guid>, IAggregateRoot<Guid>
    {
        public string CustomerName { get; set; } = string.Empty;
    }

    private sealed class RegularEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Inheritance Tests

    [Fact]
    public void IAggregateRoot_ShouldInheritFromIEntity()
    {
        // Arrange & Act
        var order = new Order { Id = 1, CustomerName = "Test" };

        // Assert
        Assert.IsType<IEntity>(order, exactMatch: false);
        Assert.IsType<IEntity<long>>(order, exactMatch: false);
    }

    [Fact]
    public void IAggregateRootGeneric_ShouldInheritFromIEntityGeneric()
    {
        // Arrange & Act
        var order = new OrderWithGenericKey { Id = Guid.NewGuid(), CustomerName = "Test" };

        // Assert
        Assert.IsType<IEntity<Guid>>(order, exactMatch: false);
        Assert.IsType<IAggregateRoot<Guid>>(order, exactMatch: false);
    }

    #endregion

    #region Marker Interface Tests

    [Fact]
    public void IAggregateRoot_ShouldActAsMarkerInterface()
    {
        // Arrange
        var aggregateRoot = new Order { Id = 1 };
        var regularEntity = new RegularEntity { Id = 1 };

        // Assert
        Assert.IsType<IAggregateRoot>(aggregateRoot, exactMatch: false);
        Assert.False((IEntity)regularEntity is IAggregateRoot);
    }

    [Fact]
    public void CanDistinguishAggregateRootsFromRegularEntities()
    {
        // Arrange
        var entities = new List<IEntity>
        {
            new Order { Id = 1 },
            new RegularEntity { Id = 2 },
            new Order { Id = 3 }
        };

        // Act
        var aggregateRoots = entities.OfType<IAggregateRoot>().ToList();

        // Assert
        Assert.Equal(2, aggregateRoots.Count);
        Assert.All(aggregateRoots, ar => Assert.IsType<Order>(ar));
    }

    #endregion

    #region BaseEntity Properties Available Tests

    [Fact]
    public void AggregateRoot_ShouldHaveIdProperty()
    {
        // Arrange
        var order = new Order { Id = 42 };

        // Assert
        Assert.Equal(42, order.Id);
    }

    [Fact]
    public void AggregateRoot_ShouldHaveConcurrencyStamp()
    {
        // Arrange
        var order = new Order { Id = 1 };

        // Assert
        Assert.NotNull(order.ConcurrencyStamp);
        Assert.True(Guid.TryParse(order.ConcurrencyStamp, out _));
    }

    [Fact]
    public void AggregateRoot_ShouldSupportSoftDelete()
    {
        // Arrange
        // Act
        var order = new Order
        {
            Id = 1,         
            IsDeleted = true
        };

        // Assert
        Assert.True(order.IsDeleted);
    }

    #endregion

    #region Type Checking Utility Tests

    [Fact]
    public void CanUseTypeCheckingForRepositoryPattern()
    {
        // This demonstrates how IAggregateRoot can be used to constrain generic repositories
        
        // Arrange & Act
        bool isAggregateRoot = typeof(IAggregateRoot).IsAssignableFrom(typeof(Order));
        bool isNotAggregateRoot = typeof(IAggregateRoot).IsAssignableFrom(typeof(RegularEntity));

        // Assert
        Assert.True(isAggregateRoot);
        Assert.False(isNotAggregateRoot);
    }

    [Fact]
    public void GenericConstraintPattern_ShouldWorkWithIAggregateRoot()
    {
        // This simulates a repository that only accepts aggregate roots
        
        // Arrange
        static void AcceptOnlyAggregateRoot<T>(T entity) where T : IAggregateRoot
        {
            Assert.NotNull(entity);
        }

        var order = new Order { Id = 1 };

        // Act & Assert (should compile and work)
        AcceptOnlyAggregateRoot(order);
    }

    #endregion
}
