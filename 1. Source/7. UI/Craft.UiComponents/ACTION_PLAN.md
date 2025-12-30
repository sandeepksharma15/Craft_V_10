# Action Plan: Production-Ready Semantic Components

## Quick Reference

**Status:** ?? Not Production Ready  
**Est. Time to Production:** 4-6 weeks (1 developer)  
**Priority:** High (accessibility and correctness issues)

---

## Critical Issues Summary

| Issue | Severity | Components Affected | Impact |
|-------|----------|---------------------|--------|
| Incorrect ARIA roles | ?? Critical | 3 (Time, Details, Article) | Accessibility violation |
| Not using BuildCssClass() | ?? Critical | All 14 | Missing framework features |
| Not using BuildStyle() | ?? Critical | All 14 | Missing animations, custom styles |
| Not using GetId() | ?? Critical | All 14 | UserAttributes["id"] ignored |
| Missing click handlers | ?? High | All 14 | Base event not working |
| No component CSS classes | ?? High | All 14 | No styling hooks |
| Missing semantic parameters | ?? Medium | 5 (Time, Details, Figure, Nav, Mark) | Limited functionality |

---

## Phase 1: Critical Fixes (Week 1) - REQUIRED FOR PRODUCTION

### Tasks

#### Task 1.1: Fix All Razor Templates (14 components)
**Time:** 8 hours  
**Files:** All `*.razor` files in Semantic folder

**Change Pattern:**
```razor
<!-- BEFORE -->
<element id="@Id" class="@Class" style="@Style" role="wrong">
    @ChildContent
</element>

<!-- AFTER -->
<element @ref="ElementRef"
         id="@GetId()"
         class="@BuildCssClass()"
         style="@BuildStyle()"
         @attributes="@UserAttributes"
         @onclick="HandleClickAsync">
    @ChildContent
</element>
```

**Components to update:**
1. ? CraftArticle.razor
2. ? CraftSection.razor
3. ? CraftAside.razor
4. ? CraftNav.razor
5. ? CraftHeader.razor
6. ? CraftFooter.razor
7. ? CraftMain.razor
8. ? CraftDiv.razor
9. ? CraftFigure.razor
10. ? CraftFigCaption.razor
11. ? CraftDetails.razor - Remove role="contentinfo"
12. ? CraftSummary.razor
13. ? CraftTime.razor - Remove role="contentinfo"
14. ? CraftMark.razor

#### Task 1.2: Add GetComponentCssClass() to All Code-Behind Files
**Time:** 4 hours  
**Files:** All `*.razor.cs` files

**Pattern:**
```csharp
public partial class CraftElementName : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-elementname";
}
```

#### Task 1.3: Update Existing Tests
**Time:** 8 hours  
**Files:** All test files in `Craft.UiComponents.Tests\Semantic\`

Update tests to verify:
- BuildCssClass() is called
- BuildStyle() is called
- GetId() is used
- Component CSS class is present
- Click events work
- No incorrect ARIA roles

#### Task 1.4: Run Full Test Suite
**Time:** 2 hours

Ensure all 279 existing tests still pass after changes.

**Total Phase 1 Time: 22 hours (~3 days)**

---

## Phase 2: Enhancements (Week 2-3) - RECOMMENDED

### Task 2.1: Add CraftTime Enhancements (3 hours)
- DateTime parameter
- Value parameter
- Format parameter
- ShowRelativeTime parameter
- Culture parameter
- Auto-formatting logic
- Unit tests (8 new tests)

### Task 2.2: Add CraftDetails Enhancements (3 hours)
- IsOpen parameter
- IsOpenChanged event
- OnToggle event
- Toggle handling logic
- Unit tests (8 new tests)

### Task 2.3: Add CraftFigure Enhancements (3 hours)
- ImageSrc parameter
- ImageAlt parameter
- Caption parameter
- CaptionContent parameter
- Smart rendering logic
- Unit tests (8 new tests)

### Task 2.4: Add CraftNav Enhancements (2 hours)
- AriaLabel parameter
- AriaLabelledBy parameter
- Unit tests (4 new tests)

### Task 2.5: Add CraftMark Enhancements (2 hours)
- Highlight parameter (bool)
- HighlightColor parameter
- Unit tests (4 new tests)

### Task 2.6: Add Missing Components (20 hours)
1. CraftAddress (2h)
2. CraftBlockquote (2h)
3. CraftCode (3h)
4. CraftPre (2h)
5. CraftDialog (5h - most complex)
6. CraftSpan (1h)
7. CraftP (1h)
8. CraftHeading (4h - dynamic h1-h6)

Each includes:
- Razor file
- Code-behind
- XML documentation
- Unit tests (8-12 tests per component)

**Total Phase 2 Time: 33 hours (~4-5 days)**

---

## Phase 3: Documentation & Polish (Week 4) - RECOMMENDED

### Task 3.1: XML Documentation Audit (8 hours)
- Review all public APIs
- Add missing <example> tags
- Add <remarks> where helpful
- Verify all parameters documented

### Task 3.2: Create Demo Project (16 hours)
Create `Craft.UiComponents.Demo` Blazor project showing:
- One page per component
- Basic examples
- Advanced examples
- Accessibility examples
- Styling examples
- Interactive playground

### Task 3.3: Accessibility Audit (8 hours)
- Run automated tools (axe, Lighthouse)
- Manual keyboard navigation testing
- Screen reader testing (NVDA/JAWS)
- Document WCAG AA compliance
- Fix any issues found

### Task 3.4: Performance Testing (4 hours)
- Render performance benchmarks
- Memory leak detection
- Large list performance
- Animation performance

### Task 3.5: Create CHANGELOG.md (2 hours)
Document all changes from v1.x to v2.0:
- Breaking changes
- New features
- Bug fixes
- Migration guide

**Total Phase 3 Time: 38 hours (~5 days)**

---

## Phase 4: Release Preparation (Week 5)

### Task 4.1: Code Review (8 hours)
- Peer review all changes
- Security review
- Performance review
- Documentation review

### Task 4.2: Integration Testing (8 hours)
- Test in real application
- Test with different themes
- Test responsive behavior
- Cross-browser testing

### Task 4.3: Package & Publish (4 hours)
- Update version numbers
- Create NuGet package
- Publish to NuGet.org
- Update GitHub release notes

**Total Phase 4 Time: 20 hours (~2-3 days)**

---

## Total Effort Estimate

| Phase | Days | Hours | Priority |
|-------|------|-------|----------|
| Phase 1: Critical Fixes | 3 | 22 | ?? Required |
| Phase 2: Enhancements | 4-5 | 33 | ?? Recommended |
| Phase 3: Documentation | 5 | 38 | ?? Nice to Have |
| Phase 4: Release Prep | 2-3 | 20 | ?? Recommended |
| **TOTAL** | **14-16** | **113** | - |

---

## Minimum Viable Production (MVP)

If time is constrained, **Phase 1 only** makes the library production-ready:

? **MVP Checklist (22 hours)**
1. Fix all ARIA roles
2. Use BuildCssClass(), BuildStyle(), GetId()
3. Add click event handling
4. Add component CSS classes
5. Update and pass all existing tests

**Remaining phases can be done incrementally in minor releases.**

---

## Implementation Order (Phase 1)

### Day 1 (8 hours)
- Fix all Razor templates (1.1)
- Add GetComponentCssClass() to code-behind (1.2)

### Day 2 (8 hours)
- Update test files
- Run test suite
- Fix any failures

### Day 3 (6 hours)
- Final testing
- Documentation updates
- Code review

---

## Verification Checklist

Before declaring "Production Ready", verify:

### Functionality
- [ ] All 14 components render correctly
- [ ] BuildCssClass() used everywhere
- [ ] BuildStyle() used everywhere
- [ ] GetId() used everywhere
- [ ] Click events work on all components
- [ ] Disabled state works
- [ ] Visibility works
- [ ] Size variants work
- [ ] Color variants work
- [ ] Animations work
- [ ] User attributes work

### Accessibility
- [ ] No incorrect ARIA roles
- [ ] Semantic HTML used correctly
- [ ] Keyboard navigation works
- [ ] Screen reader friendly
- [ ] WCAG AA compliance

### Testing
- [ ] All existing tests pass (279+)
- [ ] New tests added for fixes
- [ ] Code coverage > 90%
- [ ] Integration tests pass

### Documentation
- [ ] All public APIs documented
- [ ] Usage examples provided
- [ ] Migration guide exists (if breaking)
- [ ] CHANGELOG.md updated

---

## Risk Assessment

### High Risk
- **Breaking Changes**: Existing users must update their code
  - **Mitigation**: Provide clear migration guide and deprecation warnings

### Medium Risk
- **Test Failures**: Changes may break existing tests
  - **Mitigation**: Update tests incrementally, verify each component

### Low Risk
- **Performance**: Changes should not impact performance
  - **Mitigation**: Benchmark before/after

---

## Dependencies

### Required
- ? CraftComponent base class (already correct)
- ? CssBuilder (already exists)
- ? StyleBuilder (already exists)
- ? Test infrastructure (already exists)

### Optional (for Phase 2+)
- System.Globalization (for CraftTime localization)
- Additional theme support (for enhanced styling)

---

## Success Criteria

### Phase 1 Success
- ? All 14 components use base class methods correctly
- ? No ARIA role violations
- ? All 279+ tests pass
- ? No breaking changes to public API (only implementation fixes)

### Phase 2 Success
- ? Component-specific parameters work correctly
- ? 8 new components added
- ? 60+ new tests passing

### Phase 3 Success
- ? WCAG AA compliant
- ? Demo project showcases all features
- ? Performance benchmarks meet targets

---

## Next Steps

1. **Review this plan** with team/stakeholders
2. **Prioritize phases** based on timeline and resources
3. **Create GitHub issues** for each task
4. **Start with Phase 1** - critical fixes only
5. **Iterate** - release early, release often

---

## Questions to Answer

Before starting, clarify:

1. **Breaking Changes OK?** Can we break the API in v2.0?
   - If YES: Follow plan as-is
   - If NO: Add deprecation warnings, dual support

2. **Timeline?** When is this needed?
   - If URGENT: Phase 1 only
   - If SOON: Phase 1 + 2
   - If FLEXIBLE: All phases

3. **Resources?** How many developers?
   - 1 developer: 4-6 weeks
   - 2 developers: 2-3 weeks (parallel work)

4. **Testing Strategy?** Manual or automated?
   - Automated: Current plan works
   - Manual: Add 20% more time

---

## Conclusion

**Recommendation**: Execute Phase 1 immediately. This fixes critical correctness and accessibility issues that will be harder to fix later after adoption grows.

Phases 2-4 can be done in subsequent releases as enhancements, but Phase 1 is essential for production use.

**Quick Win**: The fixes in Phase 1 are mostly mechanical - search and replace patterns. Can be completed in 3 days with focused effort.
