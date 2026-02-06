# Debugging AutoIncludeNavigationProperties

## Status
✅ All components updated with AutoIncludeNavigationProperties parameter
✅ BuildQuery methods updated in all grid components
✅ PublicHolidaysList.razor has AutoIncludeNavigationProperties="true"
✅ Build successful

## Debug Output Added

Added debug logging to `AutoIncludeNavigationPropertiesEvaluator.cs` to trace:
1. When the evaluator is called
2. What navigation properties are discovered
3. Whether includes are added or skipped

## How to Debug

1. **Run the application in Debug mode**
2. **Navigate to Public Holidays list** (`/masters/public-holidays`)
3. **Check the Output window** in Visual Studio:
   - View → Output
   - Select "Debug" from the dropdown
4. **Look for lines starting with `[AutoInclude]`**

## Expected Debug Output

If working correctly, you should see something like:
```
[AutoInclude] Entity: PublicHoliday, Found 1 navigation properties
[AutoInclude] - Location (Location)
[AutoInclude] Added Include for Location
```

## Possible Issues & Solutions

### Issue 1: Evaluator Not Called
**Output**: No `[AutoInclude]` messages at all

**Cause**: Query not being processed through evaluators

**Solution**: Check that HttpService is using Repository which uses QueryEvaluator

### Issue 2: No Properties Discovered
**Output**: `[AutoInclude] Entity: PublicHoliday, Found 0 navigation properties`

**Cause**: NavigationPropertyDiscovery not detecting Location

**Possible reasons**:
- Location class doesn't have an `Id` property
- Location is abstract
- PropertyType detection issue

**Solution**: 
```csharp
// Check Location model - ensure it has:
public class Location
{
    public int Id { get; set; }  // or KeyType Id
    // ... other properties
}
```

### Issue 3: Property Discovered But Not Included
**Output**: 
```
[AutoInclude] Entity: PublicHoliday, Found 1 navigation properties
[AutoInclude] - Location (Location)
[AutoInclude] Skipped Location (already included)
```

**Cause**: Location is already explicitly included somewhere

**Solution**: This is actually correct behavior - check if QueryBuilder is adding an explicit Include

### Issue 4: Include Added But Data Still Null
**Output**:
```
[AutoInclude] Entity: PublicHoliday, Found 1 navigation properties
[AutoInclude] - Location (Location)
[AutoInclude] Added Include for Location
```

**Cause**: Include expression created but not applied by EF Core

**Possible reasons**:
1. IncludeEvaluator not processing the expressions correctly
2. Database has no matching Location records
3. Foreign key (LocationId) is null or invalid

**Solution**:
- Check IncludeEvaluator.cs
- Verify database data:
  ```sql
  SELECT ph.*, l.*
  FROM MD_PublicHolidays ph
  LEFT JOIN MD_Locations l ON ph.LocationId = l.Id
  ```

## Next Steps

1. Run the app and collect debug output
2. Share the output to identify the specific issue
3. Apply targeted fix based on the debug information

## Files Modified

### With Debug Output:
- `AutoIncludeNavigationPropertiesEvaluator.cs`

### Previously Updated:
- ✅ `CraftDataGrid.razor.cs`
- ✅ `CraftCardGrid.razor.cs`
- ✅ `CraftGrid.razor.cs`
- ✅ `CraftGrid.razor`
- ✅ `PublicHolidaysList.razor`

## Contact Info

All components are correctly configured. The debug output will tell us exactly what's happening at runtime.
