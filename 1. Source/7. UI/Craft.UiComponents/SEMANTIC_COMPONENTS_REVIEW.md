# Craft.UiComponents.Semantic - Production Readiness Review

## Executive Summary

The Craft.UiComponents.Semantic library provides HTML5 semantic element wrappers. This review identifies areas for improvement to achieve production readiness, following Blazor best practices and modern web standards.

---

## Current State Analysis

### Existing Components (14)
? **Structural Elements:**
- `CraftArticle` - Self-contained composition
- `CraftSection` - Thematic grouping
- `CraftAside` - Tangentially related content
- `CraftNav` - Navigation links
- `CraftHeader` - Introductory content
- `CraftFooter` - Footer content
- `CraftMain` - Main content area
- `CraftDiv` - Generic container

? **Content Elements:**
- `CraftFigure` - Self-contained content (images, diagrams)
- `CraftFigCaption` - Figure caption
- `CraftDetails` - Disclosure widget
- `CraftSummary` - Summary/legend for details
- `CraftTime` - Date/time representation
- `CraftMark` - Highlighted text

---

## Critical Issues & Recommendations

### ?? **Priority 1: Critical Issues**

#### 1.1 Incorrect ARIA Roles
**Issue:** Several components have incorrect or redundant `role` attributes.

**Current Problems:**
```razor
<!-- ? INCORRECT -->
<time role="contentinfo">  <!-- Wrong role for time element -->
<details role="contentinfo">  <!-- Wrong role for details -->
<article role="article">  <!-- Redundant - article already has implicit role -->
```

**Fix:** Remove or correct roles based on HTML5 semantics.

**Recommended Changes:**

```razor
<!-- ? CORRECT - CraftTime.razor -->
@inherits CraftComponent

<time @ref="ElementRef" 
      id="@GetId()" 
      class="@BuildCssClass()" 
      style="@BuildStyle()"
      datetime="@DateTime"
      @attributes="@UserAttributes"
      @onclick="HandleClickAsync">
    @ChildContent
</time>

@code {
    /// <summary>
    /// Gets or sets the machine-readable datetime value.
    /// </summary>
    [Parameter] public string? DateTime { get; set; }
}
```

```razor
<!-- ? CORRECT - CraftDetails.razor -->
@inherits CraftComponent

<details @ref="ElementRef" 
         id="@GetId()" 
         class="@BuildCssClass()" 
         style="@BuildStyle()"
         open="@IsOpen"
         @attributes="@UserAttributes"
         @onclick="HandleClickAsync"
         @ontoggle="HandleToggleAsync">
    @ChildContent
</details>

@code {
    /// <summary>
    /// Gets or sets whether the details are initially open.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Event callback for when details toggle state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnToggle { get; set; }
    
    private async Task HandleToggleAsync(EventArgs args)
    {
        IsOpen = !IsOpen;
        await OnToggle.InvokeAsync(IsOpen);
    }
}
```

```razor
<!-- ? CORRECT - CraftArticle.razor -->
@inherits CraftComponent

<!-- No role needed - article has implicit role="article" -->
<article @ref="ElementRef" 
         id="@GetId()" 
         class="@BuildCssClass()" 
         style="@BuildStyle()"
         @attributes="@UserAttributes"
         @onclick="HandleClickAsync">
    @ChildContent
</article>
```

#### 1.2 Not Using BuildCssClass() and BuildStyle()
**Issue:** Components directly bind `@Class` and `@Style` instead of using the base class methods.

**Current:**
```razor
<article id="@Id" class="@Class" style="@Style">
```

**Should Be:**
```razor
<article id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()">
```

**Why:** This bypasses size, variant, animation, disabled, and visibility CSS classes from `CraftComponent`.

#### 1.3 Missing Click Event Handling
**Issue:** Components don't wire up the `OnClick` event from base class.

**Add to all components:**
```razor
<article @onclick="HandleClickAsync">
```

#### 1.4 Inconsistent ID Handling
**Issue:** Using `@Id` directly instead of `@GetId()` which handles UserAttributes["id"] precedence.

**Fix:** Replace all `id="@Id"` with `id="@GetId()"`.

---

### ?? **Priority 2: Enhancements**

#### 2.1 Missing Semantic-Specific Parameters

**CraftTime Enhancements:**
```csharp
public partial class CraftTime : CraftComponent
{
    /// <summary>
    /// Gets or sets the machine-readable datetime value (ISO 8601 format).
    /// </summary>
    [Parameter] public string? DateTime { get; set; }
    
    /// <summary>
    /// Gets or sets the date/time value to format.
    /// </summary>
    [Parameter] public DateTimeOffset? Value { get; set; }
    
    /// <summary>
    /// Gets or sets the format string for displaying the datetime.
    /// </summary>
    [Parameter] public string? Format { get; set; } = "g";
    
    protected override void OnParametersSet()
    {
        if (Value.HasValue && string.IsNullOrEmpty(DateTime))
        {
            DateTime = Value.Value.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }
    }
    
    protected override string? GetComponentCssClass() => "craft-time";
}
```

**CraftDetails Enhancements:**
```csharp
public partial class CraftDetails : CraftComponent
{
    /// <summary>
    /// Gets or sets whether the details are open.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when open state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when details are toggled.
    /// </summary>
    [Parameter] public EventCallback<bool> OnToggle { get; set; }
    
    private async Task HandleToggleAsync(EventArgs args)
    {
        IsOpen = !IsOpen;
        await IsOpenChanged.InvokeAsync(IsOpen);
        await OnToggle.InvokeAsync(IsOpen);
    }
    
    protected override string? GetComponentCssClass() => "craft-details";
}
```

**CraftFigure Enhancements:**
```csharp
public partial class CraftFigure : CraftComponent
{
    /// <summary>
    /// Gets or sets the image source URL.
    /// </summary>
    [Parameter] public string? ImageSrc { get; set; }
    
    /// <summary>
    /// Gets or sets the image alt text.
    /// </summary>
    [Parameter] public string? ImageAlt { get; set; }
    
    /// <summary>
    /// Gets or sets the caption text.
    /// </summary>
    [Parameter] public string? Caption { get; set; }
    
    /// <summary>
    /// Gets or sets custom caption content.
    /// </summary>
    [Parameter] public RenderFragment? CaptionContent { get; set; }
    
    protected override string? GetComponentCssClass() => "craft-figure";
}
```

#### 2.2 Add CSS Class Overrides
All components should override `GetComponentCssClass()`:

```csharp
// CraftArticle.razor.cs
public partial class CraftArticle : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-article";
}

// CraftSection.razor.cs
public partial class CraftSection : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-section";
}

// etc...
```

#### 2.3 Enhanced CraftNav
```csharp
public partial class CraftNav : CraftComponent
{
    /// <summary>
    /// Gets or sets the ARIA label for the navigation.
    /// </summary>
    [Parameter] public string? AriaLabel { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of element that labels this navigation.
    /// </summary>
    [Parameter] public string? AriaLabelledBy { get; set; }
    
    protected override string? GetComponentCssClass() => "craft-nav";
}
```

```razor
<nav @ref="ElementRef"
     id="@GetId()"
     class="@BuildCssClass()"
     style="@BuildStyle()"
     aria-label="@AriaLabel"
     aria-labelledby="@AriaLabelledBy"
     @attributes="@UserAttributes"
     @onclick="HandleClickAsync">
    @ChildContent
</nav>
```

---

### ?? **Priority 3: Missing Components**

#### 3.1 Core Semantic Elements

**CraftAddress** - Contact information
```razor
@inherits CraftComponent

<address @ref="ElementRef"
         id="@GetId()"
         class="@BuildCssClass()"
         style="@BuildStyle()"
         @attributes="@UserAttributes"
         @onclick="HandleClickAsync">
    @ChildContent
</address>
```

**CraftBlockquote** - Extended quotation
```csharp
public partial class CraftBlockquote : CraftComponent
{
    /// <summary>
    /// Gets or sets the source URL of the quotation.
    /// </summary>
    [Parameter] public string? Cite { get; set; }
    
    protected override string? GetComponentCssClass() => "craft-blockquote";
}
```

```razor
<blockquote @ref="ElementRef"
            id="@GetId()"
            class="@BuildCssClass()"
            style="@BuildStyle()"
            cite="@Cite"
            @attributes="@UserAttributes"
            @onclick="HandleClickAsync">
    @ChildContent
</blockquote>
```

**CraftCode** - Code snippet
```csharp
public partial class CraftCode : CraftComponent
{
    /// <summary>
    /// Gets or sets the programming language for syntax highlighting.
    /// </summary>
    [Parameter] public string? Language { get; set; }
    
    protected override string? GetComponentCssClass() => 
        string.IsNullOrEmpty(Language) ? "craft-code" : $"craft-code language-{Language}";
}
```

**CraftPre** - Preformatted text
```csharp
public partial class CraftPre : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-pre";
}
```

**CraftDialog** - Dialog or modal
```csharp
public partial class CraftDialog : CraftComponent
{
    /// <summary>
    /// Gets or sets whether the dialog is open.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }
    
    /// <summary>
    /// Gets or sets the callback when open state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }
    
    /// <summary>
    /// Gets or sets whether clicking outside closes the dialog.
    /// </summary>
    [Parameter] public bool CloseOnBackdropClick { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the dialog title.
    /// </summary>
    [Parameter] public string? Title { get; set; }
    
    /// <summary>
    /// Gets or sets custom title content.
    /// </summary>
    [Parameter] public RenderFragment? TitleContent { get; set; }
    
    /// <summary>
    /// Gets or sets the dialog footer content.
    /// </summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }
    
    public async Task OpenAsync()
    {
        IsOpen = true;
        await IsOpenChanged.InvokeAsync(true);
        StateHasChanged();
    }
    
    public async Task CloseAsync()
    {
        IsOpen = false;
        await IsOpenChanged.InvokeAsync(false);
        StateHasChanged();
    }
    
    protected override string? GetComponentCssClass() => "craft-dialog";
}
```

**CraftSpan** - Generic inline container
```csharp
public partial class CraftSpan : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-span";
}
```

**CraftP** - Paragraph
```csharp
public partial class CraftP : CraftComponent
{
    protected override string? GetComponentCssClass() => "craft-p";
}
```

**CraftHeading** - Heading levels (h1-h6)
```csharp
public partial class CraftHeading : CraftComponent
{
    /// <summary>
    /// Gets or sets the heading level (1-6).
    /// </summary>
    [Parameter] public int Level { get; set; } = 1;
    
    protected override void OnParametersSet()
    {
        if (Level < 1 || Level > 6)
            throw new ArgumentOutOfRangeException(nameof(Level), "Heading level must be between 1 and 6");
    }
    
    protected override string? GetComponentCssClass() => $"craft-heading craft-h{Level}";
}
```

```razor
@inherits CraftComponent

@if (Level == 1)
{
    <h1 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h1>
}
else if (Level == 2)
{
    <h2 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h2>
}
else if (Level == 3)
{
    <h3 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h3>
}
else if (Level == 4)
{
    <h4 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h4>
}
else if (Level == 5)
{
    <h5 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h5>
}
else
{
    <h6 @ref="ElementRef" id="@GetId()" class="@BuildCssClass()" style="@BuildStyle()" @attributes="@UserAttributes" @onclick="HandleClickAsync">@ChildContent</h6>
}
```

---

## Standardized Component Template

Here's the corrected template all semantic components should follow:

### Razor File
```razor
@inherits CraftComponent

<elementname @ref="ElementRef"
             id="@GetId()"
             class="@BuildCssClass()"
             style="@BuildStyle()"
             @attributes="@UserAttributes"
             @onclick="HandleClickAsync">
    @ChildContent
</elementname>
```

### Code-Behind File
```csharp
using Craft.UiComponents;

namespace Craft.UiComponents.Semantic;

/// <summary>
/// Represents a semantic HTML element component.
/// </summary>
public partial class CraftElementName : CraftComponent
{
    // Add component-specific parameters here
    
    /// <summary>
    /// Returns the base CSS class for this component.
    /// </summary>
    protected override string? GetComponentCssClass() => "craft-elementname";
}
```

---

## Testing Recommendations

### Unit Tests Coverage
Each component should test:
1. ? Renders correct HTML element
2. ? Applies ID correctly (GetId)
3. ? Applies CSS classes (BuildCssClass)
4. ? Applies styles (BuildStyle)
5. ? Renders child content
6. ? Handles user attributes
7. ? Handles click events
8. ? Handles disabled state
9. ? Handles visibility
10. ? Component-specific parameters (datetime, open, etc.)

### Integration Tests
- Test component composition (e.g., Figure + FigCaption)
- Test accessibility with automated tools
- Test keyboard navigation where applicable

---

## Documentation Requirements

### XML Documentation
All public APIs should have:
- `<summary>` - What the component does
- `<param>` - For each parameter
- `<example>` - Usage example
- `<remarks>` - Additional context

### Usage Examples
Create a Demo/Samples project showing:
- Basic usage of each component
- Advanced scenarios
- Accessibility examples
- Responsive layouts
- Theming

---

## Accessibility (A11y) Checklist

### For All Components:
- ? Semantic HTML elements used correctly
- ? No redundant ARIA roles
- ? Support keyboard navigation
- ? Proper focus management
- ? Color contrast meets WCAG AA standards
- ? Screen reader tested

### Component-Specific:
- **CraftNav**: Must have `aria-label` or `aria-labelledby`
- **CraftDialog**: Proper focus trap, ESC key handling
- **CraftTime**: Machine-readable `datetime` attribute
- **CraftDetails/Summary**: Keyboard accessible (Space/Enter)
- **CraftFigure**: Image must have `alt` text

---

## Performance Considerations

1. **Virtualization**: For lists of semantic elements
2. **Lazy Loading**: Images in CraftFigure
3. **Memoization**: Cache BuildCssClass() results if expensive
4. **Tree Shaking**: Ensure unused components don't get bundled

---

## Breaking Changes to Consider

If you decide to implement these recommendations:

### Version 1.x ? 2.0 (Breaking)
1. Change `id="@Id"` to `id="@GetId()"`
2. Change `class="@Class"` to `class="@BuildCssClass()"`
3. Change `style="@Style"` to `style="@BuildStyle()"`
4. Remove incorrect ARIA roles
5. Add component-specific parameters

### Migration Guide
Provide a migration guide showing:
- Before/After examples
- Automated migration script if possible
- Deprecation warnings in 1.x versions

---

## Implementation Priority

### Phase 1 (Critical - Week 1)
1. Fix ARIA roles
2. Implement BuildCssClass() and BuildStyle()
3. Implement GetId()
4. Add click event handling
5. Update all existing components

### Phase 2 (Enhancement - Week 2-3)
1. Add component-specific parameters
2. Add GetComponentCssClass() overrides
3. Implement two-way binding where needed
4. Add missing components (Address, Blockquote, etc.)

### Phase 3 (Polish - Week 4)
1. Complete documentation
2. Create demo/samples project
3. Accessibility audit
4. Performance testing
5. Unit test coverage to 100%

---

## Long-Term Roadmap

### Version 2.0
- All fixes from this review
- Missing components added
- Comprehensive documentation

### Version 2.1
- Advanced components (CraftAccordion, CraftCarousel)
- Layout components (CraftGrid, CraftFlex)
- Form components using semantic HTML

### Version 3.0
- Web Components interop
- Advanced animations
- Drag and drop support
- Responsive utilities

---

## Conclusion

The current Craft.UiComponents.Semantic library provides a solid foundation but requires several critical fixes to be production-ready. The main issues are:

1. **Incorrect ARIA roles** - High impact on accessibility
2. **Not using base class methods** - Missing framework features
3. **Missing component-specific parameters** - Limited functionality
4. **Missing common semantic elements** - Incomplete coverage

**Estimated Effort:**
- Phase 1 (Critical): 40-60 hours
- Phase 2 (Enhancement): 80-100 hours
- Phase 3 (Polish): 40-60 hours
- **Total: 160-220 hours** (~4-6 weeks with 1 developer)

**Recommendation:** Prioritize Phase 1 fixes before any production deployment. These are fundamental correctness issues that will be harder to fix later after developers start using the library.
