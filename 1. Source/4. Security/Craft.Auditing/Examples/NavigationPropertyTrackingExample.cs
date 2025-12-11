using Craft.Domain;
using Microsoft.EntityFrameworkCore;

namespace Craft.Auditing.Examples;

/// <summary>
/// Examples demonstrating navigation property tracking in audit trails.
/// </summary>
public static class NavigationPropertyTrackingExample
{
    /// <summary>
    /// Example 1: Enable/disable navigation property tracking globally.
    /// </summary>
    public static void ConfigureNavigationTracking()
    {
        // Enable (default)
        AuditTrail.IncludeNavigationProperties = true;

        // Disable if you only want to track foreign key values
        // AuditTrail.IncludeNavigationProperties = false;
    }

    /// <summary>
    /// Example 2: Entity with navigation properties.
    /// </summary>
    [Audit]
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        
        // Foreign key
        public long CustomerId { get; set; }
        
        // Navigation property - will be tracked in audit trail
        public Customer? Customer { get; set; }
        
        // Another navigation
        public long? ShippingAddressId { get; set; }
        public Address? ShippingAddress { get; set; }
    }

    [Audit]
    public class Customer : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    [Audit]
    public class Address : BaseEntity
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    /// <summary>
    /// Example 3: What gets tracked when order customer changes.
    /// </summary>
    public static class WhatGetsTracked
    {
        /*
         * When changing an order's customer from Customer A to Customer B:
         * 
         * OLD VALUES:
         * {
         *   "CustomerId": 1,
         *   "Customer_Navigation": {
         *     "ForeignKey": 1,
         *     "RelatedEntity": "Previous value (not loaded)"
         *   }
         * }
         * 
         * NEW VALUES:
         * {
         *   "CustomerId": 2,
         *   "Customer_Navigation": {
         *     "ForeignKey": 2,
         *     "RelatedEntity": "John Doe"  // Customer.Name if loaded
         *   }
         * }
         * 
         * CHANGED COLUMNS:
         * ["CustomerId"]
         */
    }

    /// <summary>
    /// Example 4: Exclude specific navigation from auditing.
    /// </summary>
    [Audit]
    public class OrderWithExclusions : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        
        // This navigation will be tracked
        public long CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        // This navigation will NOT be tracked
        public long? InternalNoteId { get; set; }
        
        [DoNotAudit]
        public InternalNote? InternalNote { get; set; }
    }

    public class InternalNote : BaseEntity
    {
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Example 5: Entity with custom display property.
    /// </summary>
    [Audit]
    public class Product : BaseEntity
    {
        // AuditTrail looks for Name, Title, or DisplayName
        public string Name { get; set; } = string.Empty;  // This will be used
        public string SKU { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    [Audit]
    public class OrderItem : BaseEntity
    {
        public long OrderId { get; set; }
        public Order? Order { get; set; }
        
        public long ProductId { get; set; }
        public Product? Product { get; set; }  // Product.Name will show in audit
        
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Example 6: Viewing navigation changes in audit trail.
    /// </summary>
    public static async Task<List<NavigationChange>> GetNavigationChanges(
        DbContext context, 
        string tableName,
        long entityId)
    {
        var audits = await context.Set<AuditTrail>()
            .Where(a => a.TableName == tableName 
                     && a.KeyValues!.Contains(entityId.ToString()))
            .OrderByDescending(a => a.DateTimeUTC)
            .ToListAsync();

        var changes = new List<NavigationChange>();

        foreach (var audit in audits)
        {
            if (audit.NewValues != null)
            {
                var newDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(audit.NewValues);
                if (newDict != null)
                {
                    foreach (var kvp in newDict.Where(k => k.Key.EndsWith("_Navigation")))
                    {
                        changes.Add(new NavigationChange
                        {
                            NavigationName = kvp.Key.Replace("_Navigation", ""),
                            ChangeDate = audit.DateTimeUTC,
                            ChangeType = audit.ChangeType.ToString(),
                            NewValue = kvp.Value?.ToString()
                        });
                    }
                }
            }
        }

        return changes;
    }

    public class NavigationChange
    {
        public string NavigationName { get; set; } = string.Empty;
        public DateTime ChangeDate { get; set; }
        public string ChangeType { get; set; } = string.Empty;
        public string? NewValue { get; set; }
    }

    /// <summary>
    /// Example 7: Entity without Name property - uses Id as fallback.
    /// </summary>
    [Audit]
    public class Tag : BaseEntity
    {
        // No Name/Title/DisplayName property
        public string Code { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }

    [Audit]
    public class EntityTag : BaseEntity
    {
        public long EntityId { get; set; }
        public long TagId { get; set; }
        
        // Audit will show: { "Type": "Tag", "Id": 123 }
        public Tag? Tag { get; set; }
    }

    /// <summary>
    /// Example 8: Multiple navigation properties.
    /// </summary>
    [Audit]
    public class Invoice : BaseEntity
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        
        // Customer navigation
        public long CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        // Billing address navigation
        public long BillingAddressId { get; set; }
        public Address? BillingAddress { get; set; }
        
        // Shipping address navigation
        public long? ShippingAddressId { get; set; }
        public Address? ShippingAddress { get; set; }
        
        // Sales rep navigation
        public long? SalesRepId { get; set; }
        public User? SalesRep { get; set; }
    }

    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Example 9: Querying for relationship changes.
    /// </summary>
    public static async Task<List<CustomerChangeHistory>> GetCustomerChangeHistory(
        DbContext context,
        long orderId)
    {
        var audits = await context.Set<AuditTrail>()
            .Where(a => a.TableName == "Order" 
                     && a.KeyValues!.Contains(orderId.ToString())
                     && (a.ChangedColumns!.Contains("CustomerId") || a.ChangeType == EntityChangeType.Created))
            .OrderBy(a => a.DateTimeUTC)
            .ToListAsync();

        var history = new List<CustomerChangeHistory>();

        foreach (var audit in audits)
        {
            var oldCustomer = ExtractCustomerFromAudit(audit.OldValues);
            var newCustomer = ExtractCustomerFromAudit(audit.NewValues);

            if (oldCustomer != null || newCustomer != null)
            {
                history.Add(new CustomerChangeHistory
                {
                    ChangedAt = audit.DateTimeUTC,
                    ChangedBy = audit.UserId,
                    OldCustomer = oldCustomer,
                    NewCustomer = newCustomer
                });
            }
        }

        return history;
    }

    private static string? ExtractCustomerFromAudit(string? jsonValues)
    {
        if (string.IsNullOrEmpty(jsonValues))
            return null;

        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonValues);
        if (dict?.ContainsKey("Customer_Navigation") == true)
        {
            var navJson = dict["Customer_Navigation"].ToString();
            var navDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(navJson!);
            return navDict?["RelatedEntity"]?.ToString();
        }

        return null;
    }

    public class CustomerChangeHistory
    {
        public DateTime ChangedAt { get; set; }
        public KeyType ChangedBy { get; set; }
        public string? OldCustomer { get; set; }
        public string? NewCustomer { get; set; }
    }

    /// <summary>
    /// Example 10: Performance considerations.
    /// </summary>
    public static class PerformanceConsiderations
    {
        /*
         * Navigation property tracking adds minimal overhead:
         * 
         * 1. Only reference navigations are tracked (not collections)
         * 2. Only loaded navigations show entity info
         * 3. Unloaded navigations show "Previous value (not loaded)"
         * 4. Can be disabled globally: AuditTrail.IncludeNavigationProperties = false
         * 5. Can be excluded per property: [DoNotAudit]
         * 
         * Best Practices:
         * - Include navigations when auditing order/invoice assignments
         * - Exclude navigations for high-volume logging tables
         * - Use eager loading (.Include) if you need entity names in audit
         * - Consider lazy loading impact on audit performance
         */
    }

    /// <summary>
    /// Example 11: Complex scenario - Order reassignment.
    /// </summary>
    public static async Task ReassignOrderToNewCustomer(
        DbContext context,
        long orderId,
        long newCustomerId,
        KeyType userId)
    {
        var order = await context.Set<Order>()
            .Include(o => o.Customer)  // Include for better audit info
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return;

        // Load new customer for audit
        var newCustomer = await context.Set<Customer>()
            .FindAsync(newCustomerId);

        var oldCustomerId = order.CustomerId;
        var oldCustomerName = order.Customer?.Name;

        // Change customer
        order.CustomerId = newCustomerId;
        order.Customer = newCustomer;

        // Save - audit will automatically track:
        // - CustomerId change (1 -> 2)
        // - Customer_Navigation change (showing both names if loaded)
        await context.SaveChangesAsync();

        /*
         * Resulting audit trail:
         * {
         *   "OldValues": {
         *     "CustomerId": 1,
         *     "Customer_Navigation": {
         *       "ForeignKey": 1,
         *       "RelatedEntity": "Old Customer Name"
         *     }
         *   },
         *   "NewValues": {
         *     "CustomerId": 2,
         *     "Customer_Navigation": {
         *       "ForeignKey": 2,
         *       "RelatedEntity": "New Customer Name"
         *     }
         *   },
         *   "ChangedColumns": ["CustomerId"]
         * }
         */
    }

    /// <summary>
    /// Example 12: Disable navigation tracking for specific scenarios.
    /// </summary>
    public static async Task BulkUpdateWithoutNavigationTracking(DbContext context)
    {
        // Disable temporarily for bulk operations
        var previousSetting = AuditTrail.IncludeNavigationProperties;
        AuditTrail.IncludeNavigationProperties = false;

        try
        {
            // Perform bulk updates
            // Only FK values will be tracked, not related entity info
            // await PerformBulkUpdates(context);
        }
        finally
        {
            // Restore setting
            AuditTrail.IncludeNavigationProperties = previousSetting;
        }
    }

    /// <summary>
    /// Example 13: Custom display value strategy.
    /// </summary>
    public static class CustomDisplayStrategy
    {
        /*
         * The audit trail looks for properties in this order:
         * 1. Name
         * 2. Title
         * 3. DisplayName
         * 4. Falls back to { Type: "EntityName", Id: value }
         * 
         * To customize, consider:
         * - Adding a Name property to your entities
         * - Or using Title/DisplayName
         * - Or accept the fallback format
         * 
         * Example entity with good display support:
         */

        [Audit]
        public class WellDesignedEntity : BaseEntity
        {
            public string Name { get; set; } = string.Empty;  // ? Will be used
            public string Code { get; set; } = string.Empty;
        }

        [Audit]
        public class AlternativeEntity : BaseEntity
        {
            public string Title { get; set; } = string.Empty;  // ? Will be used if no Name
            public string Description { get; set; } = string.Empty;
        }

        [Audit]
        public class FallbackEntity : BaseEntity
        {
            // No Name/Title/DisplayName
            // Audit will show: { "Type": "FallbackEntity", "Id": 123 }
            public string Code { get; set; } = string.Empty;
        }
    }
}
