# Quick Checklist: Applying AutoIncludeNavigationProperties to Your Components

## ⚡ Quick Steps

### 1. CraftDataGrid.razor.cs
- [ ] Add `AutoIncludeNavigationProperties` parameter (line ~270)
- [ ] Update `BuildQuery()` to use it (line ~515)

### 2. CraftCardGrid.razor.cs
- [ ] Add `AutoIncludeNavigationProperties` parameter (line ~270)
- [ ] Update `BuildQuery()` to use it (line ~575)

### 3. CraftGrid.razor.cs
- [ ] Add `AutoIncludeNavigationProperties` parameter (line ~85)

### 4. CraftGrid.razor
- [ ] Pass parameter to `<CraftDataGrid>`
- [ ] Pass parameter to `<CraftCardGrid>`

### 5. PublicHolidaysList.razor
- [ ] Add `AutoIncludeNavigationProperties="true"` to `<CraftGrid>`

### 6. Build & Test
- [ ] Build solution (should succeed)
- [ ] Run application
- [ ] Navigate to Public Holidays list
- [ ] Verify Location column shows names
- [ ] Test pagination, sorting, filtering

## 📝 Code Snippets

### For CraftDataGrid & CraftCardGrid (Step 1 & 2):

**Add parameter:**
```csharp
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

**Update BuildQuery:**
```csharp
var query = new Query<TEntity>
{
    AutoIncludeNavigationProperties = AutoIncludeNavigationProperties
};
```

### For CraftGrid (Step 3):

**Add parameter:**
```csharp
[Parameter] public bool AutoIncludeNavigationProperties { get; set; }
```

### For CraftGrid.razor (Step 4):

**In both CraftDataGrid and CraftCardGrid tags:**
```razor
AutoIncludeNavigationProperties="@AutoIncludeNavigationProperties"
```

### For PublicHolidaysList.razor (Step 5):

**In CraftGrid tag:**
```razor
AutoIncludeNavigationProperties="true"
```

## ✅ Success Criteria

After completing all steps:
- ✅ Solution builds without errors
- ✅ Location column in PublicHolidaysList shows location names
- ✅ No null reference exceptions
- ✅ Pagination and sorting work correctly
- ✅ No console errors in browser

## 🚨 Troubleshooting

| Problem | Solution |
|---------|----------|
| Build errors | Verify you added the parameter in correct section |
| Location still null | Check database has Location data |
| Can't save files | Close files in Visual Studio first |
| Parameter not recognized | Make sure to add `[Parameter]` attribute |

## 📚 Full Documentation

For detailed instructions, see:
- `COMPLETE_PATCH_INSTRUCTIONS.md`
- `IMPLEMENTATION_SUMMARY.md`
- `AutoIncludeNavigationProperties-Guide.md`

## ⏱️ Estimated Time

Total time to apply all changes: **5-10 minutes**

---

**Ready? Start with Step 1!** 🚀
