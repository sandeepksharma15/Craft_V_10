using System.ComponentModel.DataAnnotations.Schema;
using Craft.Auditing;
using Craft.Domain;

namespace Craft.IntegrationTests.Entities;

/// <summary>
/// Test product entity with multi-tenant support.
/// </summary>
public class Product : BaseEntity, IHasTenant
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }

    public KeyType? CategoryId { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }

    public KeyType TenantId { get; set; }
}

/// <summary>
/// Test category entity.
/// </summary>
public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Product>? Products { get; set; }
}

/// <summary>
/// Test customer entity with multi-tenant support.
/// </summary>
public class Customer : BaseEntity, IHasTenant
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    [DoNotAudit]
    public string? SensitiveData { get; set; }

    public KeyType TenantId { get; set; }

    public ICollection<Order>? Orders { get; set; }
}

/// <summary>
/// Test order entity with multi-tenant support.
/// </summary>
public class Order : BaseEntity, IHasTenant
{
    public KeyType CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public Customer? Customer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public KeyType TenantId { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
}

/// <summary>
/// Test order item entity.
/// </summary>
public class OrderItem : BaseEntity, IHasTenant
{
    public KeyType OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public KeyType ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public KeyType TenantId { get; set; }
}

/// <summary>
/// Order status enumeration.
/// </summary>
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
