# ShowDetails Property Removal

## ? **Change Completed**

The `ShowDetails` property has been **removed** from the `IAuditTrail` interface and `AuditTrail` class as it was never set and had no clear purpose in the audit trail context.

## **What Was Removed**

### **From IAuditTrail Interface:**
```csharp
// REMOVED:
bool ShowDetails { get; set; }
```

### **From AuditTrail Class:**
```csharp
// REMOVED:
public bool ShowDetails { get; set; }
```

## **Why It Was Removed**

### **1. Never Set**
The property was declared but never assigned a value anywhere in the codebase:
- Not set in constructors
- Not set in factory methods
- Not set in `PopulateFromEntityEntry`
- Always remained at default value (`false`)

### **2. Unclear Purpose**
The property name suggests it controls "detail visibility," but:
- Audit trails are database entities, not UI/API responses
- All audit trail data is already persisted (OldValues, NewValues, ChangedColumns)
- The "detail level" concept doesn't fit the audit trail domain model
- No filtering or conditional logic used this property

### **3. Confusion with API Patterns**
The name conflicts with API request patterns where `IncludeDetails` is used:
- `GetPagedRequest` has an `IncludeDetails` property for controlling API response detail
- `ShowDetails` in an entity suggests filtering at persistence level, which doesn't make sense
- This creates confusion about where details are controlled (API vs. database)

### **4. Test Usage Was Artificial**
Tests only verified get/set functionality, not actual business logic:
```csharp
// Test was only checking property assignment worked
audit.ShowDetails = true;
Assert.True(audit.ShowDetails);
```

## **Alternative Solutions**

If you need to control audit trail detail visibility, consider:

### **Option 1: API-Level Filtering (Recommended)**
Control detail visibility in your API responses, not in the entity:

```csharp
public class AuditTrailDto
{
    public long Id { get; set; }
    public string TableName { get; set; }
    public DateTime DateTimeUTC { get; set; }
    public EntityChangeType ChangeType { get; set; }
    
    // Include detailed changes only when requested
    public string? OldValues { get; set; }  // null when !includeDetails
    public string? NewValues { get; set; }  // null when !includeDetails
    public string? ChangedColumns { get; set; }  // null when !includeDetails
}

// In your API
public async Task<List<AuditTrailDto>> GetAuditTrails(bool includeDetails = false)
{
    var query = _context.AuditTrails.AsQueryable();
    
    return query.Select(a => new AuditTrailDto
    {
        Id = a.Id,
        TableName = a.TableName,
        DateTimeUTC = a.DateTimeUTC,
        ChangeType = a.ChangeType,
        OldValues = includeDetails ? a.OldValues : null,
        NewValues = includeDetails ? a.NewValues : null,
        ChangedColumns = includeDetails ? a.ChangedColumns : null
    }).ToList();
}
```

### **Option 2: Separate Summary Entity**
Create a separate view or entity for audit summaries:

```csharp
public class AuditTrailSummary
{
    public long Id { get; set; }
    public string TableName { get; set; }
    public DateTime DateTimeUTC { get; set; }
    public EntityChangeType ChangeType { get; set; }
    public int ChangedPropertyCount { get; set; }
}
```

### **Option 3: Custom Attribute**
If you need to mark certain audit trails differently:

```csharp
[Audit(DetailLevel = AuditDetailLevel.Summary)]
public class SimpleEntity : BaseEntity { }

[Audit(DetailLevel = AuditDetailLevel.Full)]
public class ComplexEntity : BaseEntity { }
```

## **Impact Assessment**

| Area | Impact | Action Required |
|------|--------|-----------------|
| **Production Code** | Minimal | None - property was never used |
| **IAuditTrail Interface** | Property removed | ? Complete |
| **AuditTrail Class** | Property removed | ? Complete |
| **Database Schema** | Column can be dropped | ?? Migration needed |
| **Tests** | Will fail | ?? Need update (not done yet per request) |
| **API Responses** | None | ? Not used in DTOs |

## **Database Migration**

When you generate a migration, you'll see:

```csharp
migrationBuilder.DropColumn(
    name: "ShowDetails",
    table: "HK_AuditTrail");
```

This is safe because:
- The column was never used
- Always had default value (false)
- No application logic depended on it

## **Files Modified**

1. ? **`IAuditTrail.cs`** - Removed `ShowDetails` property
2. ? **`AuditTrail.cs`** - Removed `ShowDetails` property

## **Test Updates Needed (When Ready)**

The following test file references `ShowDetails`:
- `2. Tests\4. Security\Craft.Auditing.Tests\Contracts\ContractTest.cs`

**Test to update:**
```csharp
// REMOVE these lines:
audit.ShowDetails = true;
Assert.True(audit.ShowDetails);

// And remove from TestAuditTrail class:
public bool ShowDetails { get; set; }
```

## **Build Status**

- ? **Craft.Auditing** builds successfully
- ? No errors in production code
- ?? **Tests** will need updates (not done yet)

## **Recommendation**

The property removal is correct. If detail-level control is needed in the future:
1. Implement it at the API/service layer, not in the entity
2. Use DTOs to control what's returned to clients
3. Keep the audit trail entity focused on data persistence, not presentation

## **Summary**

? **Removed unused property**  
? **Cleaner data model**  
? **Less confusion**  
? **No functional impact**  
?? **Database migration required**  
?? **Test updates needed** (when ready)

The `ShowDetails` property added no value and created confusion. Its removal makes the audit trail model clearer and more focused on its core purpose: persisting entity change history.
