# Tailwind CSS Color Themes - Implementation Guide

## Quick Start Checklist

- [ ] Create `themes.css` with 8 color theme definitions (4 colors × 2 brightness)
- [ ] Update `input.css` with `@theme` directive
- [ ] Create `theme-manager.js` for theme switching (handles light/dark selection)
- [ ] Create `ThemeSelector.razor` component with color & brightness controls
- [ ] Update `MainLayout.razor` to reference theme manager
- [ ] Replace hardcoded colors in `input.css` components layer
- [ ] Test all 8 theme combinations (light & dark variants)
- [ ] Test dark mode styling with all themes
- [ ] Update any inline component colors
- [ ] Verify brightness preference persists in localStorage

---

## Step-by-Step Implementation

### Step 1: Create themes.css

**Location**: `src/Web/wwwroot/css/themes.css`

This file contains all color definitions for the eight theme combinations (4 colors × 2 brightness levels).

**Key Points:**
- Uses OKLCH color space (perceptually uniform across all hues)
- Defines primary, accent, and neutral color scales for each variant
- Each scale goes from 50 (lightest) to 950 (darkest)
- Theme classes applied to `:root` element:
  - Light variants (pastel): `.theme-red-light`, `.theme-blue-light`, `.theme-green-light`, `.theme-yellow-light`
  - Dark variants (rich): `.theme-red-dark`, `.theme-blue-dark`, `.theme-green-dark`, `.theme-yellow-dark`
- Brightness level determines color saturation and intensity

**Implementation:** See the themes.css content in the PRD document Phase 2, Section 1.1 (includes all 8 color palettes)

---

### Step 2: Update input.css

**Location**: `src/Web/wwwroot/css/input.css`

**Add at the top:**
```css
@import "tailwindcss";
@import "./themes.css";

@theme {
  --color-theme-primary-*: var(--color-theme-primary-*);
  --color-theme-accent-*: var(--color-theme-accent-*);
  --color-theme-neutral-*: var(--color-theme-neutral-*);
}

@custom-variant dark (&:where(.dark, .dark *));
```

**Update components layer** with theme-aware classes (see PRD Phase 2, Section 2)

---

### Step 3: Create theme-manager.js

**Location**: `src/Web/wwwroot/js/theme-manager.js`

This JavaScript module handles:
- Theme switching logic
- localStorage persistence
- Initialization on page load

```javascript
/**
 * Theme Manager for Tailwind CSS Color Themes
 * Manages switching between Red, Blue, Green, and Yellow themes
 */
class ThemeManager {
  static readonly THEMES = {
    RED: 'theme-red',
    BLUE: 'theme-blue',
    GREEN: 'theme-green',
    YELLOW: 'theme-yellow'
  };

  static readonly STORAGE_KEY = 'tailwind-color-theme';
  static readonly DEFAULT_THEME = this.THEMES.BLUE;

  /**
   * Initialize theme manager and apply saved or default theme
   */
  static initialize() {
    const savedTheme = localStorage.getItem(this.STORAGE_KEY) || this.DEFAULT_THEME;
    this.setTheme(savedTheme);
  }

  /**
   * Set the active theme by adding class to <html> element
   * @param {string} themeName - Theme name (must be in THEMES object values)
   */
  static setTheme(themeName) {
    // Remove all theme classes
    Object.values(this.THEMES).forEach(theme => {
      document.documentElement.classList.remove(theme);
    });

    // Add the selected theme class
    if (Object.values(this.THEMES).includes(themeName)) {
      document.documentElement.classList.add(themeName);
      localStorage.setItem(this.STORAGE_KEY, themeName);
      console.log(`Theme changed to: ${themeName}`);
    } else {
      console.warn(`Unknown theme: ${themeName}`);
    }
  }

  /**
   * Get the currently active theme
   * @returns {string} Current theme name
   */
  static getCurrentTheme() {
    return Object.values(this.THEMES).find(theme =>
      document.documentElement.classList.contains(theme)
    ) || this.DEFAULT_THEME;
  }

  /**
   * Get all available themes
   * @returns {Object} Themes object
   */
  static getAvailableThemes() {
    return this.THEMES;
  }

  /**
   * Get theme display name
   * @param {string} themeName
   * @returns {string} Display name (e.g., 'theme-red' -> 'Red')
   */
  static getThemeDisplayName(themeName) {
    const names = {
      'theme-red': 'Red',
      'theme-blue': 'Blue',
      'theme-green': 'Green',
      'theme-yellow': 'Yellow'
    };
    return names[themeName] || themeName;
  }
}

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', () => {
  ThemeManager.initialize();
});
```

---

### Step 4: Create ThemeSelector.razor Component

**Location**: `src/Web/Components/ThemeSelector.razor`

```razor
@inject IJSRuntime JS

<div class="theme-selector-container">
  <label for="theme-select" class="theme-selector-label">Color Theme:</label>
  <select id="theme-select" class="theme-selector-select" @onchange="OnThemeChange">
    <option value="theme-red">Red</option>
    <option value="theme-blue" selected>Blue</option>
    <option value="theme-green">Green</option>
    <option value="theme-yellow">Yellow</option>
  </select>
</div>

@code {
  protected override async Task OnInitializedAsync()
  {
    // Sync select with current theme on load
    await JS.InvokeVoidAsync("eval", 
      "document.getElementById('theme-select').value = ThemeManager.getCurrentTheme();");
  }

  private async Task OnThemeChange(ChangeEventArgs e)
  {
    var selectedTheme = e.Value?.ToString();
    if (!string.IsNullOrEmpty(selectedTheme))
    {
      await JS.InvokeVoidAsync("eval", $"ThemeManager.setTheme('{selectedTheme}');");
    }
  }
}
```

**Add styles to input.css:**
```css
.theme-selector-container {
  @apply flex items-center gap-3;
}

.theme-selector-label {
  @apply font-semibold text-theme-neutral-700 dark:text-theme-neutral-300;
}

.theme-selector-select {
  @apply px-3 py-2 rounded border-2 border-theme-primary-500 
         bg-theme-neutral-50 dark:bg-theme-neutral-800 
         text-theme-neutral-900 dark:text-white 
         focus:outline-none focus:ring-2 focus:ring-theme-primary-500 
         cursor-pointer transition-colors duration-200;
}
```

---

### Step 5: Update MainLayout.razor

**Location**: `src/Web/Components/Layout/MainLayout.razor`

Add to the `<head>` section:
```razor
<script src="~/js/theme-manager.js"></script>
```

Optionally add the theme selector to your navbar:
```razor
<ThemeSelector />
```

---

### Step 6: Test Theme Application

**Build CSS:**
```bash
npm run build:css
```

**Expected result:**
- New utility classes like `bg-theme-primary-500` should be available
- All four themes switchable via ThemeSelector dropdown
- Preference persists across page reloads
- Dark mode works with all themes

---

## Migration Guide: Converting Existing Components

### Before (Hardcoded Colors)
```css
.btn-primary {
  @apply bg-linear-to-r from-green-600 to-green-900 
         text-white hover:from-green-900 hover:to-green-600 
         dark:from-green-400 dark:to-green-700;
}
```

### After (Theme Variables)
```css
.btn-primary {
  @apply bg-linear-to-r from-theme-accent-600 to-theme-accent-900 
         text-white hover:from-theme-accent-900 hover:to-theme-accent-600 
         dark:from-theme-accent-400 dark:to-theme-accent-700;
}
```

### Component Color Mapping Reference

| Component      | Old Colors           | New Colors                    | Notes |
|---|---|---|---|
| `.container-card` | `shadow-blue-500`, `bg-gray-100` | `shadow-theme-primary-500`, `bg-theme-neutral-50` | Primary shadow, neutral background |
| `.btn-secondary` | `from-blue-600 to-blue-900` | `from-theme-primary-600 to-theme-primary-900` | Primary gradient |
| `.btn-primary` | `from-green-600 to-green-900` | `from-theme-accent-600 to-theme-accent-900` | Accent gradient (for affirmative) |
| `.btn-warning` | `from-red-600 to-red-900` | `from-theme-primary-600 to-theme-primary-900` | Primary (when primary is red) |
| `.nav-bar` | `border-blue-700`, `shadow-blue-500` | `border-theme-primary-700`, `shadow-theme-primary-500` | Primary accent |
| `.input-textbox` | `border-blue-500`, `focus:ring-green-500` | `border-theme-primary-500`, `focus:ring-theme-accent-500` | Primary border, accent focus |
| `h1-h6` | `text-gray-900`, `dark:text-gray-50` | `text-theme-neutral-900`, `dark:text-theme-neutral-50` | Neutral text |
| `a` | `text-gray-900`, `hover:text-blue-900` | `text-theme-neutral-900`, `hover:text-theme-primary-900` | Neutral text, primary hover |

---

## Testing Checklist

### Functionality Tests
- [ ] Theme selector renders correctly
- [ ] Clicking each theme changes the page appearance
- [ ] Preference persists after page reload
- [ ] Console shows no errors

### Visual Tests (Red Theme)
- [ ] Primary color is red (shadows, borders, gradients)
- [ ] Accent color is orange (highlights, CTAs)
- [ ] All components render correctly

### Visual Tests (Blue Theme)
- [ ] Primary color is blue (should match original look)
- [ ] Accent color is amber
- [ ] Backward compatible with original design

### Visual Tests (Green Theme)
- [ ] Primary color is green
- [ ] Accent color is yellow
- [ ] High contrast maintained

### Visual Tests (Yellow Theme)
- [ ] Primary color is yellow
- [ ] Accent color is red
- [ ] Readable in light and dark modes

### Dark Mode Tests
- [ ] Dark mode classes applied correctly with class-based toggle
- [ ] All four themes work in dark mode
- [ ] Sufficient contrast for accessibility

### Browser Compatibility
- [ ] Chrome/Edge 88+
- [ ] Firefox 89+
- [ ] Safari 14+
- [ ] Mobile browsers

---

## Troubleshooting

### Theme Classes Not Applied
**Problem:** Selected theme class not being added to `<html>`

**Solutions:**
1. Verify `theme-manager.js` is loaded: Check DevTools Console
2. Verify localStorage is enabled
3. Check that theme-manager.js is referenced in MainLayout.razor
4. Rebuild CSS: `npm run build:css`

### New Utilities Not Available
**Problem:** `bg-theme-primary-500` shows as invalid utility

**Solutions:**
1. Verify `@theme` directive is in `input.css`
2. Verify `themes.css` is imported before `@theme` directive
3. Rebuild: `npm run build:css`
4. Clear browser cache (Ctrl+Shift+Delete)

### Dark Mode Not Working
**Problem:** Dark mode not applying with theme changes

**Solutions:**
1. Verify `@custom-variant dark` is defined in `input.css`
2. Verify dark mode classes include `dark:` prefix
3. Check that `.dark` class is applied to `<html>` element
4. Ensure no conflicting dark mode configuration

### Colors Incorrect in Dark Mode
**Problem:** Dark mode colors don't contrast properly

**Solutions:**
1. Review theme definitions in `themes.css`
2. Ensure dark mode overrides use lighter variants (e.g., `dark:from-theme-primary-400`)
3. Verify OKLCH values are perceptually appropriate
4. Test with WCAG contrast checker

---

## Performance Considerations

- **CSS Variables**: Zero runtime overhead; all resolved at compile time
- **localStorage**: Minimal impact (~200 bytes for theme preference)
- **JavaScript**: Minimal (only 2KB for theme-manager.js)
- **Build Time**: No impact; Tailwind generates utilities during build

---

## Future Enhancements

1. **System Theme Detection**
   - Detect system preference (light/dark) and apply automatically
   - Allow user override

2. **Custom Color Picker**
   - Allow users to create custom themes
   - Save to localStorage or database

3. **Theme Preview**
   - Show live preview of all themes before applying
   - Quick preview on hover

4. **API Integration**
   - Save user theme preference to database
   - Sync across devices

5. **Accessibility Options**
   - High contrast themes
   - Reduced motion options per theme

---

## Maintenance

### Adding a New Theme

1. Add new color variables to `themes.css`:
```css
:root.theme-purple {
  --color-theme-primary-50: oklch(...);
  /* ... 11 shades ... */
  --color-theme-accent-500: oklch(...);
  --color-theme-accent-600: oklch(...);
}
```

2. Update `theme-manager.js`:
```javascript
static readonly THEMES = {
  // ... existing
  PURPLE: 'theme-purple'
};
```

3. Update `ThemeSelector.razor`:
```razor
<option value="theme-purple">Purple</option>
```

### Updating Color Values

All color definitions are in a single location: `themes.css`

Change any value and rebuild:
```bash
npm run build:css
```

All utility classes using that color will instantly update across the entire site.

---

## Documentation for Users

### End User Guide

**How to Change Your Color Theme:**

1. Look for the "Color Theme" selector (usually in navigation or settings)
2. Click the dropdown and choose your preferred theme:
   - **Red**: For a warm, energetic look
   - **Blue**: For a professional, classic appearance
   - **Green**: For an organic, calming feel
   - **Yellow**: For a bright, cheerful mood
3. Your selection is saved automatically and persists across visits

**Themes work with both light and dark modes** - switch modes independently of color theme selection.

---

## References

- [Tailwind CSS 4.1 Release Notes](https://tailwindcss.com/blog/tailwindcss-4-1)
- [CSS Custom Properties (Variables)](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [OKLCH Color Space](https://oklch.com/)
- [Web Storage API](https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API)

---

**End of Implementation Guide**
