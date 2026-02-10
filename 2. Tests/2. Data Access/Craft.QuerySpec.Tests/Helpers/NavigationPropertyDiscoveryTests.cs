using System.Reflection;
using Craft.Testing.Fixtures;

namespace Craft.QuerySpec.Tests.Helpers;

/// <summary>
/// Comprehensive tests for NavigationPropertyDiscovery helper.
/// Tests internal methods using reflection where necessary.
/// </summary>
public class NavigationPropertyDiscoveryTests
{
    // Test entities for navigation property discovery
    private class EntityWithNoNavigations
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public decimal Price { get; set; }
    }

    private class EntityWithSingleNavigation
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public RelatedEntity? Related { get; set; }
    }

    private class EntityWithMultipleNavigations
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public RelatedEntity? Related1 { get; set; }
        public RelatedEntity? Related2 { get; set; }
        public List<ChildEntity>? Children { get; set; }
    }

    private class EntityWithCollectionNavigation
    {
        public int Id { get; set; }
        public ICollection<ChildEntity>? Children { get; set; }
        public IList<ChildEntity>? ChildList { get; set; }
        public List<ChildEntity>? ChildListConcrete { get; set; }
        public IEnumerable<ChildEntity>? ChildEnumerable { get; set; }
        public HashSet<ChildEntity>? ChildHashSet { get; set; }
    }

    private class EntityWithNonNavigationProperties
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int[] Numbers { get; set; } = [];
        public List<string>? StringList { get; set; }
        public List<int>? IntList { get; set; }
    }

    private class EntityWithNoIdProperty
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    private class RelatedEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class ChildEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    private class EntityWithReadOnlyProperty
    {
        public int Id { get; set; }
        public RelatedEntity? WriteOnly { set { } }
        public RelatedEntity? ReadOnly => null;
    }

    public NavigationPropertyDiscoveryTests()
    {
        // Clear cache before each test to ensure clean state
        NavigationPropertyDiscovery.ClearCache();
    }

    #region DiscoverNavigationProperties Tests

    [Fact]
    public void DiscoverNavigationProperties_EntityWithNoNavigations_ReturnsEmptyList()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNoNavigations>();

        // Assert
        Assert.NotNull(properties);
        Assert.Empty(properties);
    }

    [Fact]
    public void DiscoverNavigationProperties_EntityWithSingleNavigation_ReturnsOneProperty()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithSingleNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Single(properties);
        Assert.Equal("Related", properties[0].Name);
        Assert.Equal(typeof(RelatedEntity), properties[0].PropertyType);
    }

    [Fact]
    public void DiscoverNavigationProperties_EntityWithMultipleNavigations_ReturnsAllProperties()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithMultipleNavigations>();

        // Assert
        Assert.NotNull(properties);
        Assert.Equal(3, properties.Count);
        Assert.Contains(properties, p => p.Name == "Related1");
        Assert.Contains(properties, p => p.Name == "Related2");
        Assert.Contains(properties, p => p.Name == "Children");
    }

    [Fact]
    public void DiscoverNavigationProperties_Company_ReturnsCountryAndStores()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();

        // Assert
        Assert.NotNull(properties);
        Assert.True(properties.Count >= 2);
        Assert.Contains(properties, p => p.Name == "Country");
        Assert.Contains(properties, p => p.Name == "Stores");
    }

    [Fact]
    public void DiscoverNavigationProperties_Store_ReturnsCompany()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Store>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "Company");
    }

    [Fact]
    public void DiscoverNavigationProperties_Country_ReturnsCompaniesNavigation()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Country>();

        // Assert
        Assert.NotNull(properties);
        // Country has Companies navigation property
        Assert.Contains(properties, p => p.Name == "Companies");
        Assert.True(properties.Count >= 1);
    }

    #endregion

    #region Collection Navigation Tests

    [Fact]
    public void DiscoverNavigationProperties_ICollectionProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithCollectionNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "Children");
    }

    [Fact]
    public void DiscoverNavigationProperties_IListProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithCollectionNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "ChildList");
    }

    [Fact]
    public void DiscoverNavigationProperties_ListProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithCollectionNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "ChildListConcrete");
    }

    [Fact]
    public void DiscoverNavigationProperties_IEnumerableProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithCollectionNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "ChildEnumerable");
    }

    [Fact]
    public void DiscoverNavigationProperties_HashSetProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithCollectionNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "ChildHashSet");
    }

    #endregion

    #region Non-Navigation Property Filtering Tests

    [Fact]
    public void DiscoverNavigationProperties_StringProperties_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNoNavigations>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "Name");
    }

    [Fact]
    public void DiscoverNavigationProperties_ValueTypeProperties_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNoNavigations>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "Id");
        Assert.DoesNotContain(properties, p => p.Name == "Age");
        Assert.DoesNotContain(properties, p => p.Name == "Price");
    }

    [Fact]
    public void DiscoverNavigationProperties_ArrayProperties_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNonNavigationProperties>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "Numbers");
    }

    [Fact]
    public void DiscoverNavigationProperties_StringCollections_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNonNavigationProperties>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "StringList");
    }

    [Fact]
    public void DiscoverNavigationProperties_ValueTypeCollections_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNonNavigationProperties>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "IntList");
    }

    [Fact]
    public void DiscoverNavigationProperties_WriteOnlyProperties_AreNotIncluded()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithReadOnlyProperty>();

        // Assert
        Assert.NotNull(properties);
        Assert.DoesNotContain(properties, p => p.Name == "WriteOnly");
    }

    #endregion

    #region Caching Tests

    [Fact]
    public void DiscoverNavigationProperties_CalledTwice_ReturnsSameList()
    {
        // Act
        var properties1 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();
        var properties2 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();

        // Assert
        Assert.Same(properties1, properties2);
    }

    [Fact]
    public void DiscoverNavigationProperties_DifferentTypes_ReturnsDifferentLists()
    {
        // Act
        var companyProperties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();
        var storeProperties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Store>();

        // Assert
        Assert.NotSame(companyProperties, storeProperties);
    }

    [Fact]
    public void ClearCache_AfterDiscovery_AllowsRediscovery()
    {
        // Arrange
        var properties1 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();

        // Act
        NavigationPropertyDiscovery.ClearCache();
        var properties2 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();

        // Assert
        Assert.NotSame(properties1, properties2);
        Assert.Equal(properties1.Count, properties2.Count);
    }

    [Fact]
    public void ClearCache_WithMultipleTypes_ClearsAll()
    {
        // Arrange
        var companyProps1 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();
        var storeProps1 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Store>();

        // Act
        NavigationPropertyDiscovery.ClearCache();
        var companyProps2 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();
        var storeProps2 = NavigationPropertyDiscovery.DiscoverNavigationProperties<Store>();

        // Assert
        Assert.NotSame(companyProps1, companyProps2);
        Assert.NotSame(storeProps1, storeProps2);
    }

    #endregion

    #region CreateIncludeExpression Tests

    [Fact]
    public void CreateIncludeExpression_ValidProperty_CreatesExpression()
    {
        // Arrange
        var property = typeof(Company).GetProperty("Country")!;

        // Act
        var includeExpression = NavigationPropertyDiscovery.CreateIncludeExpression<Company>(property);

        // Assert
        Assert.NotNull(includeExpression);
        Assert.False(includeExpression.IsThenInclude);
        Assert.Null(includeExpression.PreviousInclude);
        Assert.Equal(typeof(Company), includeExpression.EntityType);
        Assert.Equal(typeof(Country), includeExpression.PropertyType);
    }

    [Fact]
    public void CreateIncludeExpression_CollectionProperty_CreatesExpression()
    {
        // Arrange
        var property = typeof(Company).GetProperty("Stores")!;

        // Act
        var includeExpression = NavigationPropertyDiscovery.CreateIncludeExpression<Company>(property);

        // Assert
        Assert.NotNull(includeExpression);
        Assert.False(includeExpression.IsThenInclude);
        Assert.Null(includeExpression.PreviousInclude);
        Assert.Equal(typeof(Company), includeExpression.EntityType);
        Assert.Equal(typeof(List<Store>), includeExpression.PropertyType);
    }

    [Fact]
    public void CreateIncludeExpression_ExpressionIsValid_CanBeEvaluated()
    {
        // Arrange
        var property = typeof(Company).GetProperty("Country")!;
        var company = new Company { Id = 1, Country = new Country { Id = 1, Name = "USA" } };

        // Act
        var includeExpression = NavigationPropertyDiscovery.CreateIncludeExpression<Company>(property);
        var compiledExpression = includeExpression.Expression.Compile();
        var result = compiledExpression.DynamicInvoke(company);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<Country>(result);
        Assert.Equal("USA", ((Country)result).Name);
    }

    [Fact]
    public void CreateIncludeExpression_MultipleProperties_CreatesDistinctExpressions()
    {
        // Arrange
        var countryProperty = typeof(Company).GetProperty("Country")!;
        var storesProperty = typeof(Company).GetProperty("Stores")!;

        // Act
        var countryExpression = NavigationPropertyDiscovery.CreateIncludeExpression<Company>(countryProperty);
        var storesExpression = NavigationPropertyDiscovery.CreateIncludeExpression<Company>(storesProperty);

        // Assert
        Assert.NotSame(countryExpression, storesExpression);
        Assert.NotEqual(countryExpression.PropertyType, storesExpression.PropertyType);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DiscoverNavigationProperties_EntityWithoutIdProperty_HandlesGracefully()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithNoIdProperty>();

        // Assert
        Assert.NotNull(properties);
        // Should return empty since entities without Id property are not considered navigation properties
        Assert.Empty(properties);
    }

    [Fact]
    public void DiscoverNavigationProperties_NullableNavigationProperty_IsDiscovered()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<EntityWithSingleNavigation>();

        // Assert
        Assert.NotNull(properties);
        Assert.Contains(properties, p => p.Name == "Related" && p.PropertyType == typeof(RelatedEntity));
    }

    [Fact]
    public void DiscoverNavigationProperties_PublicPropertiesOnly_IgnoresPrivate()
    {
        // Act
        var properties = NavigationPropertyDiscovery.DiscoverNavigationProperties<Company>();

        // Assert
        Assert.NotNull(properties);
        // Verify all returned properties are public
        Assert.All(properties, p => 
        {
            Assert.NotNull(p.GetMethod);
            Assert.True(p.GetMethod.IsPublic);
        });
    }

    #endregion
}
