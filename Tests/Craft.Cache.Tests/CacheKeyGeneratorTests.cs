using Xunit;

namespace Craft.Cache.Tests;

public class CacheKeyGeneratorTests
{
    private readonly CacheKeyGenerator _generator;

    public CacheKeyGeneratorTests()
    {
        _generator = new CacheKeyGenerator("test", ":");
    }

    #region Generate Tests

    [Fact]
    public void Generate_WithTypeOnly_ReturnsCorrectKey()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var key = _generator.Generate(type);

        // Assert
        Assert.Equal("test:Product", key);
    }

    [Fact]
    public void Generate_WithTypeAndParameters_ReturnsKeyWithHash()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var key = _generator.Generate(type, 1, "test");

        // Assert
        Assert.StartsWith("test:Product:", key);
        Assert.Contains(":", key);
    }

    [Fact]
    public void Generate_WithSameParameters_ReturnsSameKey()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var key1 = _generator.Generate(type, 1, "test");
        var key2 = _generator.Generate(type, 1, "test");

        // Assert
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void Generate_WithDifferentParameters_ReturnsDifferentKeys()
    {
        // Arrange
        var type = typeof(Product);

        // Act
        var key1 = _generator.Generate(type, 1, "test");
        var key2 = _generator.Generate(type, 2, "test");

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void Generate_WithNullType_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _generator.Generate(null!));
    }

    #endregion

    #region GenerateMethodKey Tests

    [Fact]
    public void GenerateMethodKey_WithoutArguments_ReturnsCorrectKey()
    {
        // Arrange & Act
        var key = _generator.GenerateMethodKey("ProductService", "GetAll");

        // Assert
        Assert.Equal("test:ProductService:GetAll", key);
    }

    [Fact]
    public void GenerateMethodKey_WithArguments_ReturnsKeyWithHash()
    {
        // Arrange & Act
        var key = _generator.GenerateMethodKey("ProductService", "GetById", 123);

        // Assert
        Assert.StartsWith("test:ProductService:GetById:", key);
    }

    [Fact]
    public void GenerateMethodKey_WithSameArguments_ReturnsSameKey()
    {
        // Arrange & Act
        var key1 = _generator.GenerateMethodKey("ProductService", "GetById", 123);
        var key2 = _generator.GenerateMethodKey("ProductService", "GetById", 123);

        // Assert
        Assert.Equal(key1, key2);
    }

    [Theory]
    [InlineData("", "Method")]
    [InlineData("Type", "")]
    public void GenerateMethodKey_WithEmptyParameters_ThrowsArgumentException(string? typeName, string? methodName)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentException>(() => 
            _generator.GenerateMethodKey(typeName!, methodName!));
    }

    [Theory]
    [InlineData(null, "Method")]
    [InlineData("Type", null)]
    public void GenerateMethodKey_WithNullParameters_ThrowsArgumentNullException(string? typeName, string? methodName)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            _generator.GenerateMethodKey(typeName!, methodName!));
    }

    #endregion

    #region GenerateEntityKey Tests

    [Fact]
    public void GenerateEntityKey_WithIntId_ReturnsCorrectKey()
    {
        // Arrange & Act
        var key = _generator.GenerateEntityKey<Product>(123);

        // Assert
        Assert.Equal("test:Product:123", key);
    }

    [Fact]
    public void GenerateEntityKey_WithStringId_ReturnsCorrectKey()
    {
        // Arrange & Act
        var key = _generator.GenerateEntityKey<Product>("abc-123");

        // Assert
        Assert.Equal("test:Product:abc-123", key);
    }

    [Fact]
    public void GenerateEntityKey_WithGuidId_ReturnsCorrectKey()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var key = _generator.GenerateEntityKey<Product>(guid);

        // Assert
        Assert.Equal($"test:Product:{guid}", key);
    }

    [Fact]
    public void GenerateEntityKey_WithNullId_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => _generator.GenerateEntityKey<Product>(null!));
    }

    #endregion

    #region GenerateEntityPattern Tests

    [Fact]
    public void GenerateEntityPattern_WithoutPattern_ReturnsWildcardPattern()
    {
        // Arrange & Act
        var pattern = _generator.GenerateEntityPattern<Product>();

        // Assert
        Assert.Equal("test:Product:*", pattern);
    }

    [Fact]
    public void GenerateEntityPattern_WithNullPattern_ReturnsWildcardPattern()
    {
        // Arrange & Act
        var pattern = _generator.GenerateEntityPattern<Product>(null);

        // Assert
        Assert.Equal("test:Product:*", pattern);
    }

    [Fact]
    public void GenerateEntityPattern_WithCustomPattern_ReturnsCorrectPattern()
    {
        // Arrange & Act
        var pattern = _generator.GenerateEntityPattern<Product>("category:*");

        // Assert
        Assert.Equal("test:Product:category:*", pattern);
    }

    [Theory]
    [InlineData("active:*")]
    [InlineData("123")]
    [InlineData("*:pending")]
    public void GenerateEntityPattern_WithVariousPatterns_ReturnsCorrectFormat(string customPattern)
    {
        // Arrange & Act
        var pattern = _generator.GenerateEntityPattern<Product>(customPattern);

        // Assert
        Assert.StartsWith("test:Product:", pattern);
        Assert.EndsWith(customPattern, pattern);
    }

    #endregion

    #region Custom Prefix and Separator Tests

    [Fact]
    public void Constructor_WithCustomPrefix_UsesCustomPrefix()
    {
        // Arrange
        var generator = new CacheKeyGenerator("myapp", ":");

        // Act
        var key = generator.GenerateEntityKey<Product>(1);

        // Assert
        Assert.StartsWith("myapp:", key);
    }

    [Fact]
    public void Constructor_WithCustomSeparator_UsesCustomSeparator()
    {
        // Arrange
        var generator = new CacheKeyGenerator("test", "-");

        // Act
        var key = generator.GenerateEntityKey<Product>(1);

        // Assert
        Assert.Equal("test-Product-1", key);
    }

    [Theory]
    [InlineData(null, ":")]
    [InlineData("test", null)]
    public void Constructor_WithNullParameters_ThrowsArgumentNullException(string? prefix, string? separator)
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CacheKeyGenerator(prefix!, separator!));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Generate_ComplexScenario_ProducesConsistentKeys()
    {
        // Arrange
        var parameters = new object[] { 1, "test", new { Id = 123, Name = "Product" } };

        // Act
        var key1 = _generator.Generate(typeof(Product), parameters);
        var key2 = _generator.Generate(typeof(Product), parameters);

        // Assert
        Assert.Equal(key1, key2);
        Assert.Contains("test:Product:", key1);
    }

    #endregion
}

public class Product
{
    public int Id { get; set; }
    public string? Name { get; set; }
}
