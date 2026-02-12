namespace Craft.Domain.Tests.Contracts;

public class DataTransferObjectTests
{
    #region Test Implementations

    private sealed class ProductDto : BaseDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private sealed class ProductVm : BaseVm
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string FormattedPrice => Price.ToString("C");
    }

    private sealed class ProductModel : BaseModel
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    private sealed class ProductDtoWithGuid : BaseDto<Guid>
    {
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Interface Implementation Tests

    [Fact]
    public void BaseDto_ShouldImplementIDataTransferObject()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        Assert.IsType<IDataTransferObject>(dto, exactMatch: false);
        Assert.IsType<IDataTransferObject<long>>(dto, exactMatch: false);
    }

    [Fact]
    public void BaseVm_ShouldImplementIDataTransferObject()
    {
        // Arrange & Act
        var vm = new ProductVm();

        // Assert
        Assert.IsType<IDataTransferObject>(vm, exactMatch: false);
        Assert.IsType<IDataTransferObject<long>>(vm, exactMatch: false);
    }

    [Fact]
    public void BaseModel_ShouldImplementIDataTransferObject()
    {
        // Arrange & Act
        var model = new ProductModel();

        // Assert
        Assert.IsType<IDataTransferObject>(model, exactMatch: false);
        Assert.IsType<IDataTransferObject<long>>(model, exactMatch: false);
    }

    [Fact]
    public void GenericDto_ShouldImplementGenericIDataTransferObject()
    {
        // Arrange & Act
        var dto = new ProductDtoWithGuid();

        // Assert
        Assert.IsType<IDataTransferObject<Guid>>(dto, exactMatch: false);
    }

    #endregion

    #region Interface Inheritance Chain Tests

    [Fact]
    public void IDataTransferObject_ShouldInheritFromIModel()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        Assert.IsType<IModel>(dto, exactMatch: false);
        Assert.IsType<IModel<long>>(dto, exactMatch: false);
    }

    [Fact]
    public void IDataTransferObject_ShouldInheritFromIHasConcurrency()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        Assert.IsType<IHasConcurrency>(dto, exactMatch: false);
    }

    [Fact]
    public void IDataTransferObject_ShouldInheritFromISoftDelete()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        Assert.IsType<ISoftDelete>(dto, exactMatch: false);
    }

    [Fact]
    public void IDataTransferObject_ShouldInheritFromIHasId()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert - IDataTransferObject inherits from IModel<TKey> which inherits from IHasId<TKey>
        Assert.IsType<IHasId<long>>(dto, exactMatch: false);
    }

    #endregion

    #region Required Properties Tests

    [Fact]
    public void AllDtoTypes_ShouldHaveIdProperty()
    {
        // Arrange
        var dto = new ProductDto { Id = 42 };
        var vm = new ProductVm { Id = 43 };
        var model = new ProductModel { Id = 44 };

        // Assert
        Assert.Equal(42, dto.Id);
        Assert.Equal(43, vm.Id);
        Assert.Equal(44, model.Id);
    }

    [Fact]
    public void AllDtoTypes_ShouldHaveConcurrencyStampProperty()
    {
        // Arrange
        var stamp = "test-stamp";
        var dto = new ProductDto { ConcurrencyStamp = stamp };
        var vm = new ProductVm { ConcurrencyStamp = stamp };
        var model = new ProductModel { ConcurrencyStamp = stamp };

        // Assert
        Assert.Equal(stamp, dto.ConcurrencyStamp);
        Assert.Equal(stamp, vm.ConcurrencyStamp);
        Assert.Equal(stamp, model.ConcurrencyStamp);
    }

    [Fact]
    public void AllDtoTypes_ShouldHaveIsDeletedProperty()
    {
        // Arrange
        var dto = new ProductDto { IsDeleted = true };
        var vm = new ProductVm { IsDeleted = true };
        var model = new ProductModel { IsDeleted = true };

        // Assert
        Assert.True(dto.IsDeleted);
        Assert.True(vm.IsDeleted);
        Assert.True(model.IsDeleted);
    }

    #endregion

    #region Polymorphism Tests

    [Fact]
    public void CanTreatAllTypesAsIDataTransferObject()
    {
        // Arrange
        var dtos = new List<IDataTransferObject>
        {
            new ProductDto { Id = 1, Name = "DTO" },
            new ProductVm { Id = 2, Name = "VM" },
            new ProductModel { Id = 3, Name = "Model" }
        };

        // Act & Assert
        Assert.Equal(3, dtos.Count);
        Assert.All(dtos, dto =>
        {
            Assert.NotEqual(default, dto.Id);
            Assert.False(dto.IsDeleted);
        });
    }

    [Fact]
    public void CanFilterByIDataTransferObject()
    {
        // Arrange
        var objects = new List<object>
        {
            new ProductDto { Id = 1 },
            new ProductVm { Id = 2 },
            "not a dto",
            42,
            new ProductModel { Id = 3 }
        };

        // Act
        var dtos = objects.OfType<IDataTransferObject>().ToList();

        // Assert
        Assert.Equal(3, dtos.Count);
    }

    #endregion

    #region Generic Constraint Pattern Tests

    [Fact]
    public void GenericMethod_CanConstrainToIDataTransferObject()
    {
        // Arrange
        static T CloneDto<T>(T source) where T : IDataTransferObject<long>, new()
        {
            return new T
            {
                Id = source.Id,
                ConcurrencyStamp = source.ConcurrencyStamp,
                IsDeleted = source.IsDeleted
            };
        }

        var original = new ProductDto { Id = 1, ConcurrencyStamp = "stamp", IsDeleted = false };

        // Act
        var clone = CloneDto(original);

        // Assert
        Assert.Equal(original.Id, clone.Id);
        Assert.Equal(original.ConcurrencyStamp, clone.ConcurrencyStamp);
        Assert.Equal(original.IsDeleted, clone.IsDeleted);
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void BaseDto_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var dto = new ProductDto();

        // Assert
        Assert.Equal(default, dto.Id);
        Assert.Null(dto.ConcurrencyStamp);
        Assert.False(dto.IsDeleted);
    }

    [Fact]
    public void BaseVm_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var vm = new ProductVm();

        // Assert
        Assert.Equal(default, vm.Id);
        Assert.Null(vm.ConcurrencyStamp);
        Assert.False(vm.IsDeleted);
    }

    [Fact]
    public void BaseModel_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var model = new ProductModel();

        // Assert
        Assert.Equal(default, model.Id);
        Assert.Null(model.ConcurrencyStamp);
        Assert.False(model.IsDeleted);
    }

    #endregion
}
