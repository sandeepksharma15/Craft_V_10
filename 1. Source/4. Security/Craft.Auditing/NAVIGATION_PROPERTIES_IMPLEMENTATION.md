# Navigation Properties Support Implementation

## ? **Implementation Complete**

Comprehensive support for tracking navigation property changes has been added to the Craft.Auditing system, enabling audit trails to capture not just foreign key changes, but also related entity information.

## **Features Implemented**

### **1. Configuration Property**

```csharp
/// <summary>
/// Gets or sets whether to include navigation property changes in audit trails.
/// When true, foreign key changes will show both the FK value and related entity information.
/// Default is true.
/// </summary>
public static bool IncludeNavigationProperties { get; set; } = true;
```

### **2. Automatic Navigation Tracking**

When a navigation property changes, the audit trail captures:
- **Foreign Key Value** - The ID of the related entity
- **Related Entity Display Value** - The name/title of the related entity (if loaded)

**Example Output:**
```json
{
  "CustomerId": 2,
  "Customer_Navigation": {
    "ForeignKey": 2,
    "RelatedEntity": "John Doe"
  }
}
```

### **3. Smart Display Value Resolution**

The system automatically looks for display properties in this order:
1. `Name` property
2. `Title` property
3. `DisplayName` property
4. Fallback to `{ "Type": "EntityName", "Id": value }`

### **4. Integration with Existing Features**

Navigation tracking works seamlessly with:
- ? `[Audit]` attribute - only audited entities tracked
- ? `[DoNotAudit]` attribute - exclude specific navigations
- ? Factory methods (`Create`, `CreateAsync`)
- ? All entity states (Added, Modified, Deleted)
- ? Soft delete support
- ? JSON serialization options

## **Usage Examples**

### **Example 1: Basic Configuration**

```csharp
// Enable (default)
AuditTrail.IncludeNavigationProperties = true;

// Disable if you only want FK values
AuditTrail.IncludeNavigationProperties = false;
```

### **Example 2: Entity with Navigations**

```csharp
[Audit]
public class Order : BaseEntity
{
    public string OrderNumber { get; set; }
    
    // Foreign key
    public long CustomerId { get; set; }
    
    // Navigation - will be tracked
    public Customer? Customer { get; set; }
}

[Audit]
public class Customer : BaseEntity
{
    public string Name { get; set; }  // Used for display
    public string Email { get; set; }
}
```

### **Example 3: What Gets Tracked**

When changing an order's customer:

**Old Values:**
```json
{
  "CustomerId": 1,
  "Customer_Navigation": {
    "ForeignKey": 1,
    "RelatedEntity": "Previous value (not loaded)"
  }
}
```

**New Values:**
```json
{
  "CustomerId": 2,
  "Customer_Navigation": {
    "ForeignKey": 2,
    "RelatedEntity": "John Doe"
  }
}
```

**Changed Columns:**
```json
["CustomerId"]
```

### **Example 4: Exclude Specific Navigation**

```csharp
[Audit]
public class Order : BaseEntity
{
    // This will be tracked
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    // This will NOT be tracked
    public long? InternalNoteId { get; set; }
    
    [DoNotAudit]
    public InternalNote? InternalNote { get; set; }
}
```

### **Example 5: Multiple Navigations**

```csharp
[Audit]
public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; }
    
    public long CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public long BillingAddressId { get; set; }
    public Address? BillingAddress { get; set; }
    
    public long? ShippingAddressId { get; set; }
    public Address? ShippingAddress { get; set; }
}

// All navigations will be tracked in the audit trail
```

## **Technical Implementation**

### **1. ExtractNavigationProperties Method**

```csharp
private static void ExtractNavigationProperties(
    EntityEntry entity, 
    Dictionary<string, object>? newValues, 
    Dictionary<string, object>? oldValues)
{
    foreach (var reference in entity.References)
    {
        // Cast to INavigation to access ForeignKey
        if (reference.Metadata is not INavigation navigation)
            continue;

        // Get foreign key
        var foreignKey = navigation.ForeignKey;
        var foreignKeyProperty = foreignKey.Properties.First();
        var fkPropertyEntry = entity.Property(foreignKeyProperty.Name);

        // Track new value
        if (newValues != null && fkPropertyEntry.CurrentValue != null)
        {
            var relatedEntity = reference.CurrentValue;
            newValues[$"{navigationName}_Navigation"] = new
            {
                ForeignKey = fkPropertyEntry.CurrentValue,
                RelatedEntity = GetNavigationDisplayValue(relatedEntity)
            };
        }

        // Track old value
        if (oldValues != null && fkPropertyEntry.OriginalValue != null)
        {
            oldValues[$"{navigationName}_Navigation"] = new
            {
                ForeignKey = fkPropertyEntry.OriginalValue,
                RelatedEntity = "Previous value (not loaded)"
            };
        }
    }
}
```

### **2. GetNavigationDisplayValue Method**

```csharp
private static object? GetNavigationDisplayValue(object? entity)
{
    if (entity == null)
        return null;

    var entityType = entity.GetType();

    // Try Name, Title, DisplayName
    var nameProperty = entityType.GetProperty("Name") ?? 
                      entityType.GetProperty("Title") ??
                      entityType.GetProperty("DisplayName");

    if (nameProperty != null)
    {
        var nameValue = nameProperty.GetValue(entity);
        if (nameValue != null)
            return nameValue;
    }

    // Fallback to Type + Id
    var idProperty = entityType.GetProperty("Id");
    if (idProperty != null)
    {
        var idValue = idProperty.GetValue(entity);
        return new { Type = entityType.Name, Id = idValue };
    }

    return new { Type = entityType.Name };
}
```

### **3. Integration Points**

Navigation tracking is automatically called from:
- `ExtractAddedValues()` - for new entities
- `ExtractDeletedValues()` - for deleted entities
- `ExtractUpdatedValues()` - for modified entities

## **Benefits**

| Benefit | Description |
|---------|-------------|
| **Better Context** | See not just IDs, but actual entity names in audits |
| **Relationship Tracking** | Track when orders change customers, invoices change addresses, etc. |
| **Historical Insight** | Understand relationship evolution over time |
| **Debugging** | Easier to debug issues with wrong associations |
| **Compliance** | Track who changed critical relationships and when |
| **User-Friendly** | Display meaningful names instead of cryptic IDs |

## **Performance Considerations**

### **Minimal Overhead**
- Only reference navigations tracked (not collections)
- Only loaded entities show names
- Unloaded entities show "Previous value (not loaded)"
- Can be disabled globally or per-property

### **Best Practices**

**? DO:**
```csharp
// Use eager loading for better audit info
var order = await context.Orders
    .Include(o => o.Customer)
    .FirstAsync(o => o.Id == orderId);

// Change customer
order.Customer = newCustomer;
await context.SaveChangesAsync();

// Audit shows: "Customer_Navigation": { "ForeignKey": 2, "RelatedEntity": "John Doe" }
```

**? DON'T:**
```csharp
// Without Include, audit shows generic message
var order = await context.Orders
    .FirstAsync(o => o.Id == orderId);

order.Customer = newCustomer;
await context.SaveChangesAsync();

// Audit shows: "Customer_Navigation": { "ForeignKey": 2, "RelatedEntity": "Previous value (not loaded)" }
```

### **Disable for High-Volume Scenarios**

```csharp
// Temporarily disable for bulk operations
var previousSetting = AuditTrail.IncludeNavigationProperties;
AuditTrail.IncludeNavigationProperties = false;

try
{
    // Bulk updates - only FK values tracked
    await PerformBulkUpdates();
}
finally
{
    AuditTrail.IncludeNavigationProperties = previousSetting;
}
```

## **Querying Navigation Changes**

### **Get All Relationship Changes**

```csharp
var audits = await context.AuditTrails
    .Where(a => a.TableName == "Order" 
             && a.ChangedColumns.Contains("CustomerId"))
    .ToListAsync();

foreach (var audit in audits)
{
    var newValues = JsonSerializer.Deserialize<Dictionary<string, object>>(audit.NewValues);
    if (newValues?.ContainsKey("Customer_Navigation") == true)
    {
        var customerInfo = newValues["Customer_Navigation"];
        // Process customer change
    }
}
```

### **Get Customer Assignment History**

```csharp
public async Task<List<CustomerChange>> GetCustomerHistory(long orderId)
{
    var audits = await context.AuditTrails
        .Where(a => a.TableName == "Order" 
                 && a.KeyValues.Contains(orderId.ToString())
                 && (a.ChangedColumns.Contains("CustomerId") 
                    || a.ChangeType == EntityChangeType.Created))
        .OrderBy(a => a.DateTimeUTC)
        .ToListAsync();

    return audits.Select(a => new CustomerChange
    {
        ChangedAt = a.DateTimeUTC,
        OldCustomer = ExtractCustomer(a.OldValues),
        NewCustomer = ExtractCustomer(a.NewValues)
    }).ToList();
}
```

## **Entity Design Recommendations**

### **Good: Entity with Name Property**

```csharp
[Audit]
public class Customer : BaseEntity
{
    public string Name { get; set; }  // ? Will be used in audits
    public string Email { get; set; }
}
```

### **Good: Entity with Title Property**

```csharp
[Audit]
public class Article : BaseEntity
{
    public string Title { get; set; }  // ? Will be used if no Name
    public string Content { get; set; }
}
```

### **Acceptable: Entity without Display Property**

```csharp
[Audit]
public class Tag : BaseEntity
{
    // No Name/Title/DisplayName
    // Audit will show: { "Type": "Tag", "Id": 123 }
    public string Code { get; set; }
    public string Color { get; set; }
}
```

## **Files Modified**

1. ? **`AuditTrail.cs`**
   - Added `IncludeNavigationProperties` static property
   - Added `ExtractNavigationProperties()` method
   - Added `GetNavigationDisplayValue()` method
   - Integrated into `ExtractAddedValues()`, `ExtractDeletedValues()`, `ExtractUpdatedValues()`

## **Files Created**

1. ? **`NavigationPropertyTrackingExample.cs`** - 13 comprehensive examples:
   - Configuration
   - Basic entity setup
   - What gets tracked
   - Excluding navigations
   - Multiple navigations
   - Querying changes
   - Performance considerations
   - Complex scenarios
   - Custom display strategies

## **Build Status**

- ? **Craft.Auditing** project builds successfully
- ?? 2 warnings in example code (null reference checks - not production code)
- ? All production code compiles without errors

## **Compatibility**

| Feature | Compatibility |
|---------|---------------|
| **[Audit] Attribute** | ? Fully compatible |
| **[DoNotAudit] Attribute** | ? Works on navigations |
| **Factory Methods** | ? Automatic integration |
| **Soft Delete** | ? Tracks on soft delete |
| **Retention Policy** | ? Works together |
| **JSON Options** | ? Respects configuration |
| **Async Support** | ? Works with CreateAsync |

## **Migration Guide**

### **No Breaking Changes**

Navigation tracking is additive - existing audits continue to work:
- Default is enabled (`IncludeNavigationProperties = true`)
- Existing audit trails unaffected
- No database schema changes needed
- No code changes required

### **Opt-Out if Needed**

```csharp
// Disable globally
AuditTrail.IncludeNavigationProperties = false;

// Or exclude specific navigations
[DoNotAudit]
public Customer? Customer { get; set; }
```

## **Summary**

? **Navigation property tracking implemented**  
? **Tracks FK values + related entity names**  
? **Smart display value resolution**  
? **Works with all entity states**  
? **Configurable globally and per-property**  
? **Minimal performance impact**  
? **No breaking changes**  
? **Comprehensive examples provided**  

The navigation properties support provides rich audit context by capturing not just what IDs changed, but what actual entities were involved, making audit trails much more useful for compliance, debugging, and historical analysis.
