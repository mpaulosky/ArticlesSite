# Plan: Split ThemeSelector into Nav-Ready Components

**Date:** March 26, 2026
**Status:** ✅ Implemented
**Scope:** `src/Web/Components/Shared/` · `src/Web/Components/Layout/` · `src/Web/wwwroot/css/input.css`

---

## Goal

Split the existing `ThemeSelector.razor` into two compact, nav-friendly Blazor components:

| New Component | Purpose | Nav placement |
|---|---|---|
| `ThemeBrightnessToggleComponent.razor` | Two buttons — **Light (Pastel)** / **Dark (Rich)** | Right side of nav bar |
| `ThemeColorDropdownComponent.razor` | `<select>` dropdown — Red / Blue / Green / Yellow | Right side of nav bar |

The original `ThemeSelector.razor` (full settings card on the Settings/Preferences page) is **unchanged**. Both new components share the exact same `window.ThemeManager` JS calls — no new JavaScript is required.

---

## Background: How the Theme System Works Today

```
localStorage["tailwind-color-theme"]  ← canonical key (theme-manager.js)
localStorage["theme"]                 ← synced brightness key (ThemeToggle.razor.js)

ThemeManager.selectColorAndUpdateUI(color)       → setColor() → setTheme() → syncUI()
ThemeManager.selectBrightnessAndUpdateUI(mode)   → setBrightness() → setTheme() → syncUI()
ThemeManager.syncUI()                            → updates DOM buttons with .active class
ThemeManager.getCurrentColor()                   → returns "RED" | "BLUE" | "GREEN" | "YELLOW"
ThemeManager.getCurrentBrightness()              → returns "light" | "dark"
```

`ThemeSelector.razor` calls these functions via `JS.InvokeVoidAsync`. The new components will use the same calls — the only difference is the HTML rendered.

---

## Phase 1 — Create `ThemeBrightnessToggleComponent.razor`

**File:** `src/Web/Components/Shared/ThemeBrightnessToggleComponent.razor`

### UI Design

```
[ ☀ Light ]  [ ◐ Dark ]
```

- Two `<button>` elements styled for compact nav use.
- Active button highlighted with `.active` class (matches existing `.brightness-btn.active` CSS).
- Calls `ThemeManager.selectBrightnessAndUpdateUI("light"|"dark")` on click.
- On `OnAfterRenderAsync(firstRender)` calls `ThemeManager.syncUI()` to sync active state on load.

### Component Skeleton

```razor
@inject IJSRuntime JS

<div class="theme-brightness-toggle">
    <button @onclick='() => SelectBrightness("light")' class="btn brightness-btn" id="nav-btn-light">
        ☀ Light
    </button>
    <button @onclick='() => SelectBrightness("dark")' class="btn brightness-btn" id="nav-btn-dark">
        ◐ Dark
    </button>
</div>

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender) { ... syncUI ... }
    private async Task SelectBrightness(string brightness) { ... selectBrightnessAndUpdateUI ... }
}
```

### Notes

- Error handling: `TaskCanceledException` and `JSDisconnectedException` caught and swallowed (matches existing pattern).
- No new JS needed — uses `ThemeManager` already on `window`.
- `.active` toggling is done by `ThemeManager.syncUI()` in the browser DOM — the Blazor component does not need C# state for the active button.

---

## Phase 2 — Create `ThemeColorDropdownComponent.razor`

**File:** `src/Web/Components/Shared/ThemeColorDropdownComponent.razor`

### UI Design

```
[ Color ▼ ]   (dropdown: Red / Blue / Green / Yellow)
```

- Native `<select>` element styled to match nav colours.
- `@onchange` calls `ThemeManager.selectColorAndUpdateUI(value)`.
- On `OnAfterRenderAsync(firstRender)` reads current color via `ThemeManager.getCurrentColor()` and sets `_selectedColor` field to reflect the active selection.

### Component Skeleton

```razor
@inject IJSRuntime JS

<select class="theme-color-dropdown" @onchange="OnColorChanged">
    <option value="RED"    selected="@(_selectedColor == "RED")">🔴 Red</option>
    <option value="BLUE"   selected="@(_selectedColor == "BLUE")">🔵 Blue</option>
    <option value="GREEN"  selected="@(_selectedColor == "GREEN")">🟢 Green</option>
    <option value="YELLOW" selected="@(_selectedColor == "YELLOW")">🟡 Yellow</option>
</select>

@code {
    private string _selectedColor = "BLUE";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _selectedColor = await JS.InvokeAsync<string>("ThemeManager.getCurrentColor");
            StateHasChanged();
        }
    }

    private async Task OnColorChanged(ChangeEventArgs args)
    {
        _selectedColor = args.Value?.ToString() ?? "BLUE";
        await JS.InvokeVoidAsync("ThemeManager.selectColorAndUpdateUI", _selectedColor);
    }
}
```

---

## Phase 3 — Add Nav-Compact CSS Classes

**File:** `src/Web/wwwroot/css/input.css`

Add to the `@layer components` block:

```css
/* Nav-compact brightness toggle */
.theme-brightness-toggle {
    @apply flex gap-1 items-center;
}

/* Nav-compact color dropdown */
.theme-color-dropdown {
    @apply text-sm font-medium rounded-full px-3 py-1.5
    bg-theme-primary-200 dark:bg-theme-primary-800
    text-theme-primary-800 dark:text-theme-primary-100
    border border-theme-primary-500 dark:border-theme-primary-400
    cursor-pointer transition-all duration-200
    hover:bg-theme-primary-300 dark:hover:bg-theme-primary-700
    focus:outline-none focus:ring-2 focus:ring-theme-accent-500;
}
```

Existing `.brightness-btn` and `.brightness-btn.active` classes already handle the button states — no changes needed there.

---

## Phase 4 — Integrate into `NavMenuComponent.razor`

**File:** `src/Web/Components/Layout/NavMenuComponent.razor`

Add the two new components to the right side of the nav `<div>`:

```razor
<div class="flex items-center gap-4">
    <!-- existing nav links -->
    <div class="hidden space-x-2 font-medium sm:block">
        ...
    </div>
    <!-- Theme controls — compact nav versions -->
    <div class="flex items-center gap-2">
        <ThemeBrightnessToggleComponent />
        <ThemeColorDropdownComponent />
    </div>
</div>
```

---

## Phase 5 — Keep ThemeSelector.razor Untouched

The existing `ThemeSelector.razor` (rendered on the Settings/Preferences page as a full card) is **not modified**. It continues to use the button-grid layout with `ThemeManager.syncUI()` to keep its active states in sync.

Since all components share `window.ThemeManager`, state is always consistent — changing the colour from the nav dropdown also reflects in the full ThemeSelector card when the user navigates to that page, and vice versa.

---

## File Change Summary

| File | Action | Notes |
|---|---|---|
| `src/Web/Components/Shared/ThemeBrightnessToggleComponent.razor` | **Create** | Phase 1 |
| `src/Web/Components/Shared/ThemeColorDropdownComponent.razor` | **Create** | Phase 2 |
| `src/Web/wwwroot/css/input.css` | **Edit** — add 2 CSS classes | Phase 3 |
| `src/Web/Components/Layout/NavMenuComponent.razor` | **Edit** — add 2 component refs | Phase 4 |
| `src/Web/Components/Shared/ThemeSelector.razor` | **No change** | Phase 5 |

---

## Testing Checklist

- [ ] `ThemeBrightnessToggleComponent` — Light button applies `theme-*-light` class to `<html>`; Dark applies `theme-*-dark`
- [ ] `ThemeBrightnessToggleComponent` — Active button reflects page load state (persisted from localStorage)
- [ ] `ThemeColorDropdownComponent` — Selecting a colour updates both `localStorage["tailwind-color-theme"]` and `<html>` class
- [ ] `ThemeColorDropdownComponent` — Dropdown pre-selects the current active colour on load
- [ ] Changing colour from nav dropdown is reflected in full `ThemeSelector.razor` when navigating to the settings page
- [ ] Changing brightness from nav buttons is reflected in full `ThemeSelector.razor`
- [ ] Existing `ThemeToggle.razor` (dark-mode toggle) continues to work without conflict
- [ ] No layout overflow in nav bar on mobile (components hidden at small breakpoints or collapsed)
- [ ] bUnit tests — `ThemeBrightnessToggleComponent` renders 2 buttons; `ThemeColorDropdownComponent` renders 4 options

---

## Confirmed Decisions

1. **Mobile responsiveness:** Nav theme controls are wrapped in `hidden sm:flex` — hidden below `sm` breakpoint, matching the behaviour of the existing nav links.
2. **Emoji in dropdown:** ✅ Emoji colour indicators used (🔴🔵🟢🟡).
3. **Dual syncUI behaviour:** ✅ Confirmed — `ThemeManager.syncUI()` updates `.brightness-btn` elements everywhere in the DOM simultaneously (both the nav buttons and the full ThemeSelector card).
4. **`ThemeSelector.razor` long term:** Retire the full card once `ThemeBrightnessToggleComponent` + `ThemeColorDropdownComponent` are proven to cover all use-cases.
