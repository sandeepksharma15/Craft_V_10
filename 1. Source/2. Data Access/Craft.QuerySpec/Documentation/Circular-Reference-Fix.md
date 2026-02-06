# Circular Reference Fix - Complete Summary

## Problem

After implementing `AutoIncludeNavigationProperties`, a circular reference error occurred during JSON serialization:

```
System.Text.Json.JsonException: A possible object cycle was detected.
Path: $.Items.Location.PublicHolidays.Location.PublicHolidays...
```

### Root Cause

1. **Entity Relationships:**
   - `PublicHoliday` has navigation property → `Location`
   - `Location` has navigation property → `PublicHolidays` (collection)

2. **AutoInclude Behavior:**
   - When `AutoIncludeNavigationProperties = true`, EF Core loads the `Location` navigation property
   - EF Core automatically populates **both sides** of the relationship
   - This creates a circular object graph: `PublicHoliday` → `Location` → `PublicHolidays` → `Location` → ...

3. **Serialization Issue:**
   - When the API tries to serialize the `PageResponse<PublicHoliday>` to JSON
   - JSON serializer follows the object graph infinitely
   - Exceeds maximum depth of 32, causing an exception

## Solution

Added `ReferenceHandler.IgnoreCycles` to the JSON serialization options in the API project.

### File Modified

**`Craft.QuerySpec/Extensions/QuerySpecServiceExtensions.cs`**

```csharp
public static IMvcBuilder AddQuerySpecJsonOptions(this IMvcBuilder builder)
{
    builder.AddJsonOptions(options =>
    {
        // Add the converter factory that handles all Query<T> and Query<T, TResult> types
        options.JsonSerializerOptions.Converters.Add(new QueryJsonConverterFactory());

        // Set common JSON options that align with QuerySerializerOptions
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        
        // Handle circular references in navigation properties
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

    return builder;
}
```

### How It Works

**`ReferenceHandler.IgnoreCycles`:**
- Detects when an object is being serialized that has already been serialized in the current graph
- Instead of following the reference again (causing infinite loop), it **ignores** it
- The circular reference property is omitted from the JSON output
- The parent object is serialized, but the circular child is skipped

### Example

**Before (causes error):**
```json
{
  "items": [
    {
      "id": 1,
      "name": "New Year",
      "location": {
        "id": 1,
        "name": "USA",
        "publicHolidays": [
          {
            "id": 1,
            "location": {
              "id": 1,
              "publicHolidays": [
                // ← Infinite loop!
              ]
            }
          }
        ]
      }
    }
  ]
}
```

**After (with IgnoreCycles):**
```json
{
  "items": [
    {
      "id": 1,
      "name": "New Year",
      "location": {
        "id": 1,
        "name": "USA"
        // publicHolidays is omitted to break the cycle
      }
    }
  ]
}
```

## Alternative Solutions (Not Used)

### 1. ReferenceHandler.Preserve
```csharp
options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
```
- **Pros**: Preserves all object references using `$id` and `$ref` metadata
- **Cons**: Makes JSON harder to read, requires special handling on client side
- **Why not used**: Unnecessary complexity for this use case

### 2. DTOs (Data Transfer Objects)
```csharp
public class PublicHolidayDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public LocationDto Location { get; set; }  // No circular reference
}

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    // No PublicHolidays property
}
```
- **Pros**: Complete control, better separation of concerns
- **Cons**: More code to write and maintain, need mapping logic
- **Why not used**: Overkill for this scenario, would require changes across entire application

### 3. [JsonIgnore] Attribute
```csharp
public class Location
{
    public int Id { get; set; }
    public string Name { get; set; }
    
    [JsonIgnore]  // Prevent serialization of reverse navigation
    public List<PublicHoliday> PublicHolidays { get; set; }
}
```
- **Pros**: Simple, explicit control
- **Cons**: Affects all serialization scenarios, not just this one
- **Why not used**: Too restrictive, might need PublicHolidays in other contexts

### 4. Explicit Include (Instead of AutoInclude)
```csharp
query.Include(ph => ph.Location);  // Only include Location, not reverse navigation
```
- **Pros**: Most control, no circular references
- **Cons**: Defeats the purpose of AutoIncludeNavigationProperties
- **Why not used**: We want the AutoInclude feature to work seamlessly

## Why IgnoreCycles Is Best

✅ **Simple**: One line of configuration  
✅ **Automatic**: Handles all circular references without code changes  
✅ **Flexible**: Works with AutoIncludeNavigationProperties  
✅ **Safe**: Prevents infinite loops while preserving most data  
✅ **Minimal Impact**: Only affects circular references, not normal properties  
✅ **Standard**: Built-in .NET feature, well-tested and supported  

## Testing

After applying this fix:

1. ✅ Run the application
2. ✅ Navigate to Public Holidays list
3. ✅ Verify Location column displays location names
4. ✅ No circular reference error
5. ✅ Data loads correctly with navigation properties

## Impact on Other Entities

This fix applies globally to **all API responses**, which is good because:

- Any entity with circular navigation properties will be handled correctly
- Examples in your application:
  - `Location` ↔ `PublicHolidays`
  - `Location` ↔ `Contractors`
  - `Location` ↔ `Workers`
  - `Location` ↔ `Holidays`
  - `Location` ↔ `Agencies`
  - Any other bidirectional relationships

All of these will now work correctly with `AutoIncludeNavigationProperties = true`!

## Summary

**Problem**: Circular reference error when serializing entities with bidirectional navigation properties  
**Root Cause**: EF Core populates both sides of relationships, JSON serializer follows infinite loop  
**Solution**: Added `ReferenceHandler.IgnoreCycles` to JSON options  
**Result**: Circular references are detected and ignored during serialization  
**Benefit**: AutoIncludeNavigationProperties now works seamlessly with all entities!  

🎉 **Your AutoIncludeNavigationProperties feature is now fully functional!**
