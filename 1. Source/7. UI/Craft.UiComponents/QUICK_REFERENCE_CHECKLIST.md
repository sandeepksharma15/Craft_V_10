# Quick Reference Checklist

## Component Audit Status

### Existing Components (14 total)

| Component | ARIA Role | BuildCss | BuildStyle | GetId | OnClick | CSS Class | Priority |
|-----------|-----------|----------|------------|-------|---------|-----------|----------|
| CraftArticle | ? Redundant | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftSection | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftAside | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftNav | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftHeader | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftFooter | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftMain | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftDiv | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftFigure | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftFigCaption | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftDetails | ? Wrong | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftSummary | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftTime | ? Wrong | ? No | ? No | ? No | ? No | ? No | ?? Fix |
| CraftMark | ? OK | ? No | ? No | ? No | ? No | ? No | ?? Fix |

**Summary:**
- ? Correct: 0/14 (0%)
- ? Issues: 14/14 (100%)
- ?? Critical: All 14 components

---

## Missing Components (8 recommended)

| Component | Priority | Complexity | Est. Time | Reason |
|-----------|----------|------------|-----------|---------|
| CraftAddress | ?? High | Low | 2h | Contact info (very common) |
| CraftBlockquote | ?? High | Low | 2h | Quotations (very common) |
| CraftCode | ?? High | Medium | 3h | Code snippets (developer-focused) |
| CraftPre | ?? High | Low | 2h | Preformatted text (pairs with Code) |
| CraftDialog | ?? High | High | 5h | Modals/dialogs (very common) |
| CraftSpan | ?? Medium | Low | 1h | Inline container (common) |
| CraftP | ?? Medium | Low | 1h | Paragraphs (very common) |
| CraftHeading | ?? High | Medium | 4h | h1-h6 (very common) |

**Total:** 20 hours for all 8 components

---

## Phase 1 Checklist (Critical - 3 Days)

### Day 1: Template Fixes (8 hours)

#### Morning (4 hours)
- [ ] CraftArticle.razor - Remove role, add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftSection.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftAside.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftNav.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftHeader.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftFooter.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftMain.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick

#### Afternoon (4 hours)
- [ ] CraftDiv.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftFigure.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftFigCaption.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftDetails.razor - Remove role, add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftSummary.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftTime.razor - Remove role, add @ref, GetId, BuildCss, BuildStyle, onclick
- [ ] CraftMark.razor - Add @ref, GetId, BuildCss, BuildStyle, onclick

### Day 2: Code-Behind & Tests (8 hours)

#### Morning (2 hours) - Add GetComponentCssClass
- [ ] CraftArticle.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-article";`
- [ ] CraftSection.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-section";`
- [ ] CraftAside.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-aside";`
- [ ] CraftNav.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-nav";`
- [ ] CraftHeader.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-header";`
- [ ] CraftFooter.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-footer";`
- [ ] CraftMain.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-main";`
- [ ] CraftDiv.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-div";`
- [ ] CraftFigure.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-figure";`
- [ ] CraftFigCaption.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-figcaption";`
- [ ] CraftDetails.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-details";`
- [ ] CraftSummary.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-summary";`
- [ ] CraftTime.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-time";`
- [ ] CraftMark.razor.cs - Add `protected override string? GetComponentCssClass() => "craft-mark";`

#### Afternoon (6 hours) - Update Tests
- [ ] Update CraftArticleTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-article class
- [ ] Update CraftSectionTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-section class
- [ ] Update CraftAsideTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-aside class
- [ ] Update CraftNavTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-nav class
- [ ] Update CraftHeaderTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-header class
- [ ] Update CraftFooterTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-footer class
- [ ] Update CraftMainTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-main class
- [ ] Update CraftDivTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-div class
- [ ] Update CraftFigureTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-figure class
- [ ] Update CraftFigCaptionTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-figcaption class
- [ ] Update CraftDetailsTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-details class, NO ROLE
- [ ] Update CraftSummaryTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-summary class
- [ ] Update CraftTimeTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-time class, NO ROLE
- [ ] Update CraftMarkTests.cs - Verify BuildCssClass, BuildStyle, GetId, craft-mark class

### Day 3: Testing & Verification (6 hours)

#### Morning (4 hours)
- [ ] Run full test suite - `dotnet test`
- [ ] Fix any test failures
- [ ] Verify component CSS classes appear in markup
- [ ] Verify Size variants work (ExtraSmall to ExtraLarge)
- [ ] Verify Variant styles work (Primary, Secondary, etc.)
- [ ] Verify Animations work (Fade, Slide, etc.)
- [ ] Verify Disabled state works
- [ ] Verify Visible state works
- [ ] Verify Click events fire
- [ ] Verify UserAttributes work

#### Afternoon (2 hours)
- [ ] Run accessibility audit (automated tools)
- [ ] Verify no ARIA violations
- [ ] Test keyboard navigation
- [ ] Update CHANGELOG.md
- [ ] Git commit with descriptive message
- [ ] Code review (if team process requires)

---

## Test Template Updates

For each test file, add these new tests:

```csharp
[Fact]
public void Component_ShouldUseGetId()
{
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.AddAttribute(1, "Id", "custom-id");
        builder.CloseComponent();
    });
    
    var element = cut.Find("elementname");
    Assert.Equal("custom-id", element.Id);
}

[Fact]
public void Component_ShouldUseBuildCssClass()
{
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.AddAttribute(1, "Size", ComponentSize.Large);
        builder.AddAttribute(2, "Variant", ComponentVariant.Primary);
        builder.CloseComponent();
    });
    
    var element = cut.Find("elementname");
    Assert.Contains("craft-size-lg", element.ClassName);
    Assert.Contains("craft-variant-primary", element.ClassName);
    Assert.Contains("craft-elementname", element.ClassName);
}

[Fact]
public void Component_ShouldUseBuildStyle()
{
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.AddAttribute(1, "Animation", AnimationType.Fade);
        builder.AddAttribute(2, "AnimationDuration", AnimationDuration.Fast);
        builder.CloseComponent();
    });
    
    var element = cut.Find("elementname");
    var style = element.GetAttribute("style");
    Assert.Contains("--craft-animation-duration", style ?? "");
}

[Fact]
public void Component_ShouldHandleClickEvent()
{
    var clicked = false;
    
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.AddAttribute(1, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(
            this, _ => clicked = true));
        builder.CloseComponent();
    });
    
    cut.Find("elementname").Click();
    Assert.True(clicked);
}

[Fact]
public void Component_ShouldApplyComponentCssClass()
{
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.CloseComponent();
    });
    
    Assert.Contains("craft-elementname", cut.Markup);
}

[Fact]
public void Component_ShouldNotHaveIncorrectAriaRole()
{
    var cut = Render(builder =>
    {
        builder.OpenComponent<CraftElementName>(0);
        builder.CloseComponent();
    });
    
    var element = cut.Find("elementname");
    var role = element.GetAttribute("role");
    
    // For most semantic elements, role should be null (implicit role)
    // Only add this test for components that previously had wrong roles
    Assert.Null(role);
}
```

---

## Verification Commands

```bash
# Build project
dotnet build

# Run all tests
dotnet test

# Run specific component tests
dotnet test --filter "FullyQualifiedName~CraftArticleTests"

# Run semantic component tests only
dotnet test --filter "FullyQualifiedName~Semantic"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Check for ARIA violations (requires axe-core)
# Manual test in browser with axe DevTools extension
```

---

## Git Workflow

```bash
# Create feature branch
git checkout -b fix/semantic-components-phase1

# Stage changes
git add .

# Commit with descriptive message
git commit -m "fix: Correct ARIA roles and implement base class methods in semantic components

- Remove incorrect ARIA roles from CraftTime and CraftDetails
- Implement BuildCssClass(), BuildStyle(), GetId() in all 14 components
- Add click event handling to all components
- Add component-specific CSS classes
- Update all test files to verify new behavior
- Add 56 new test cases (4 per component)

BREAKING: None - API unchanged, only implementation improvements

Fixes #123"

# Push to remote
git push origin fix/semantic-components-phase1

# Create pull request
# Review and merge
```

---

## Success Criteria

Phase 1 is complete when:

- [x] All 14 components updated
- [x] All 14 code-behind files updated
- [x] All 14 test files updated
- [x] 295+ tests passing (was 279, added ~16)
- [x] No ARIA violations (automated tools pass)
- [x] Code review approved
- [x] Documentation updated
- [x] CHANGELOG.md updated
- [x] Git commit pushed
- [x] Pull request merged

---

## Quick Wins

These can be done in parallel or first for momentum:

1. **CraftDiv** - Simplest, no special ARIA concerns
2. **CraftSection** - Very similar to CraftDiv
3. **CraftArticle** - Just remove redundant role attribute

Start with these three, then pattern is clear for the rest!

---

## Common Pitfalls

### ? Don't Do This
```razor
<!-- Wrong - still using old pattern -->
<article id="@Id" class="@Class">
```

### ? Do This
```razor
<!-- Correct - using base class methods -->
<article id="@GetId()" class="@BuildCssClass()">
```

### ? Don't Do This
```csharp
// Wrong - empty class
public partial class CraftArticle : CraftComponent
{
}
```

### ? Do This
```csharp
// Correct - with GetComponentCssClass override
public partial class CraftArticle : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-article";
}
```

---

## Time Tracking

Track your progress:

| Task | Estimated | Actual | Notes |
|------|-----------|--------|-------|
| Template fixes (14 files) | 4h | | |
| Code-behind (14 files) | 2h | | |
| Test updates (14 files) | 6h | | |
| Testing & verification | 4h | | |
| Documentation | 2h | | |
| Code review | 2h | | |
| **TOTAL** | **22h** | | |

---

## Resources

- **Detailed Review**: SEMANTIC_COMPONENTS_REVIEW.md
- **Example Implementation**: EXAMPLE_CORRECTED_COMPONENT.md
- **Full Action Plan**: ACTION_PLAN.md
- **Executive Summary**: EXECUTIVE_SUMMARY.md
- **This Checklist**: QUICK_REFERENCE_CHECKLIST.md

---

## Need Help?

Questions about:
- ARIA roles? ? See SEMANTIC_COMPONENTS_REVIEW.md section "Critical Issues"
- Implementation? ? See EXAMPLE_CORRECTED_COMPONENT.md
- Timeline? ? See ACTION_PLAN.md
- Big picture? ? See EXECUTIVE_SUMMARY.md

---

**Last Updated:** 2024
**Status:** Ready for Phase 1 execution
**Next Review:** After Phase 1 completion
