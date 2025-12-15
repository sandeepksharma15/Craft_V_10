using System.ComponentModel.DataAnnotations.Schema;
using Craft.Domain;

namespace Craft.TestHost.Entities;

/// <summary>
/// Test product entity for HTTP integration tests.
/// </summary>
public class TestProduct : BaseEntity, IHasTenant
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public KeyType TenantId { get; set; }
}

/// <summary>
/// DTO for TestProduct.
/// </summary>
public class TestProductDto : IModel
{
    public KeyType Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
}
