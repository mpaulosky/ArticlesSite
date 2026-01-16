# Product Requirements Document (PRD)

## Tailwind CSS Color Themes Enhancement

**Document Version:** 1.0  
**Created:** January 16, 2026  
**Project:** ArticlesSite  
**Status:** Ready for Implementation  

---

## Executive Summary

This PRD outlines the implementation of a modern, maintainable color theming system for the ArticlesSite project using Tailwind CSS 4.1+. The solution leverages Tailwind's new **Theme Variables** feature (`@theme` directive) to provide a unified, simple approach to managing **8 complete theme combinations**: **4 color themes** (Red, Blue, Green, Yellow) with **2 brightness options each** (Light/Pastel and Dark/Deep).

**Light (Pastel) Variants**: Soft, gentle colors perfect for a friendly, approachable design  
**Dark (Deep) Variants**: Saturated, bold colors for a professional, strong presence

The approach prioritizes:

- **Simplicity**: Minimal changes required to existing code
- **Maintainability**: Centralized color definitions
- **Scalability**: Easy to add or modify themes in the future
- **Performance**: No JavaScript overhead; pure CSS-based theming

---

## Current State Analysis

### Existing Implementation Issues

1. **Hardcoded Color References**
   - Colors are scattered across component classes in [input.css](../src/Web/wwwroot/css/input.css)
   - Blue, green, and red colors are used inconsistently
   - Dark mode variants use default Tailwind colors

2. **Current Color Usage**
   - `.container-card`: `shadow-blue-500`, `bg-gray-100 dark:bg-gray-800`
   - `.btn-secondary`: Blue gradients (`from-blue-600 to-blue-900`)
   - `.btn-primary`: Green gradients (`from-green-600 to-green-900`)
   - `.btn-warning`: Red gradients (`from-red-600 to-red-900`)
   - `.nav-bar`: `border-blue-700`, `shadow-blue-500`

3. **Maintenance Challenges**
   - Changing brand colors requires multiple file edits
   - No centralized theme configuration
   - Inconsistent color depth (50-950 scale) usage
   - Dark mode colors are ad-hoc, not systematized

---

## Solution Overview

### Architecture: Theme Variables with `@theme` Directive

**Technology**: Tailwind CSS 4.1+ `@theme` directive in CSS layer

**Key Benefits Over Alternatives**:

- ✅ No config files to maintain (pure CSS)
- ✅ Generates utility classes automatically (e.g., `bg-theme-primary-500`)
- ✅ Full dark mode support
- ✅ CSS variables for custom component usage
- ✅ Easiest learning curve; aligns with Tailwind 4 direction

---

## Theme Specification

### 8 Complete Theme Combinations

The system provides **4 color themes** with **2 brightness variants each**:

| Theme | Light Variant | Dark Variant | Best For |
|---|---|---|---|
| **🔴 Red** | Pastel Red (soft, gentle) | Deep Red (bold, saturated) | Warnings, alerts, confidence |
| **🔵 Blue** | Pastel Blue (soft, gentle) | Deep Blue (bold, saturated) | Professional, trustworthy, default |
| **🟢 Green** | Pastel Green (soft, gentle) | Deep Green (bold, saturated) | Success, positive, organic |
| **🟡 Yellow** | Pastel Yellow (soft, gentle) | Deep Yellow (bold, saturated) | Caution, highlights, playful |

### Brightness Levels Explained

**Light (Pastel) Variants**:

- Uses colors from the 50-400 range (high lightness: 75-98%)
- Soft, gentle, approachable feel
- Better for welcoming, friendly interfaces
- Lower color saturation for relaxed appearance

**Dark (Deep) Variants**:

- Uses colors from the 400-600 range (mid-to-low lightness: 54-80%)
- Saturated, bold, professional feel
- Better for strong branding, authority
- Higher color saturation for impactful appearance

### User Choice Matrix

```
Users can independently choose:

├── Color Theme (4 options)
│   ├── Red     ├── Light Red    └── Dark Red
│   ├── Blue    ├── Light Blue   └── Dark Blue  
│   ├── Green   ├── Light Green  └── Dark Green
│   └── Yellow  ├── Light Yellow └── Dark Yellow
│
Result: 8 total theme combinations
```

### Phase 1: Define Theme Color Palettes

#### 1.1 Create Theme Definitions in CSS

**File**: `src/Web/wwwroot/css/themes.css` (NEW)

Define eight complete color palettes (4 themes × 2 brightness) using OKLCH color space:

```css
/* Theme Color Palettes - Using OKLCH Color Space */
/* Light (Pastel) and Dark (Rich) variants for each theme */

/* ===== RED THEME ===== */
/* LIGHT VARIANT - Pastel Red (soft, gentle, high lightness 75-98%) */
:root.theme-red-light {
  --color-theme-primary-50: oklch(98.5% 0.006 17.38);
  --color-theme-primary-100: oklch(96.2% 0.015 17.717);
  --color-theme-primary-200: oklch(93.8% 0.03 18.334);
  --color-theme-primary-300: oklch(90.2% 0.048 19.571);
  --color-theme-primary-400: oklch(84.6% 0.08 22.216);
  --color-theme-primary-500: oklch(78.5% 0.12 25.331);
  --color-theme-primary-600: oklch(72.1% 0.14 27.104);
  --color-theme-primary-700: oklch(65.3% 0.15 28.957);
  --color-theme-primary-800: oklch(55.2% 0.12 32.463);
  --color-theme-primary-900: oklch(45.1% 0.10 35.084);
  --color-theme-primary-950: oklch(38.4% 0.08 38.627);
  --color-theme-accent-500: oklch(82.1% 0.108 47.604);  /* pastel orange */
  --color-theme-accent-600: oklch(76.3% 0.14 41.116);
}

/* DARK VARIANT - Deep Red (bold, saturated, low lightness 25-63%) */
:root.theme-red-dark {
  --color-theme-primary-50: oklch(97.1% 0.013 17.38);
  --color-theme-primary-100: oklch(93.6% 0.032 17.717);
  --color-theme-primary-200: oklch(88.5% 0.062 18.334);
  --color-theme-primary-300: oklch(80.8% 0.114 19.571);
  --color-theme-primary-400: oklch(70.4% 0.191 22.216);
  --color-theme-primary-500: oklch(63.7% 0.237 25.331);
  --color-theme-primary-600: oklch(56.2% 0.240 26.832);
  --color-theme-primary-700: oklch(47.3% 0.222 27.654);
  --color-theme-primary-800: oklch(38.1% 0.198 28.012);
  --color-theme-primary-900: oklch(28.4% 0.145 29.234);
  --color-theme-primary-950: oklch(22.1% 0.098 31.045);
  --color-theme-accent-500: oklch(70.5% 0.213 47.604);
  --color-theme-accent-600: oklch(64.6% 0.222 41.116);
}

/* ===== BLUE THEME ===== */
/* LIGHT VARIANT - Pastel Blue */
:root.theme-blue-light {
  --color-theme-primary-50: oklch(98.3% 0.008 264.52);
  --color-theme-primary-100: oklch(96.1% 0.018 266.34);
  --color-theme-primary-200: oklch(93.5% 0.035 267.89);
  --color-theme-primary-300: oklch(89.8% 0.052 269.12);
  --color-theme-primary-400: oklch(83.6% 0.095 270.22);
  --color-theme-primary-500: oklch(77.2% 0.135 271.01);
  --color-theme-primary-600: oklch(70.1% 0.156 272.45);
  --color-theme-primary-700: oklch(62.4% 0.158 273.98);
  --color-theme-primary-800: oklch(52.3% 0.145 274.65);
  --color-theme-primary-900: oklch(42.1% 0.125 275.34);
  --color-theme-primary-950: oklch(35.2% 0.095 276.12);
  --color-theme-accent-500: oklch(85.2% 0.125 52.34);   /* pastel amber */
  --color-theme-accent-600: oklch(78.5% 0.145 49.87);
}

/* DARK VARIANT - Deep Blue */
:root.theme-blue-dark {
  --color-theme-primary-50: oklch(97.2% 0.016 264.52);
  --color-theme-primary-100: oklch(93.5% 0.038 266.34);
  --color-theme-primary-200: oklch(87.8% 0.075 267.89);
  --color-theme-primary-300: oklch(79.4% 0.125 269.12);
  --color-theme-primary-400: oklch(68.5% 0.195 270.22);
  --color-theme-primary-500: oklch(59.7% 0.245 271.01);
  --color-theme-primary-600: oklch(51.8% 0.258 272.45);
  --color-theme-primary-700: oklch(43.2% 0.242 273.98);
  --color-theme-primary-800: oklch(34.5% 0.215 274.65);
  --color-theme-primary-900: oklch(25.8% 0.165 275.34);
  --color-theme-primary-950: oklch(19.2% 0.125 276.12);
  --color-theme-accent-500: oklch(72.1% 0.215 52.34);
  --color-theme-accent-600: oklch(65.3% 0.235 49.87);
}

/* ===== GREEN THEME ===== */
/* LIGHT VARIANT - Pastel Green */
:root.theme-green-light {
  --color-theme-primary-50: oklch(98.2% 0.007 146.71);
  --color-theme-primary-100: oklch(95.9% 0.018 148.45);
  --color-theme-primary-200: oklch(93.1% 0.032 149.87);
  --color-theme-primary-300: oklch(89.2% 0.055 151.23);
  --color-theme-primary-400: oklch(82.5% 0.105 153.12);
  --color-theme-primary-500: oklch(75.8% 0.142 155.34);
  --color-theme-primary-600: oklch(68.3% 0.152 156.89);
  --color-theme-primary-700: oklch(60.2% 0.148 158.12);
  --color-theme-primary-800: oklch(49.8% 0.135 159.45);
  --color-theme-primary-900: oklch(39.5% 0.115 160.67);
  --color-theme-primary-950: oklch(32.1% 0.088 161.23);
  --color-theme-accent-500: oklch(86.4% 0.095 89.34);    /* pastel yellow */
  --color-theme-accent-600: oklch(80.2% 0.125 87.65);
}

/* DARK VARIANT - Deep Green */
:root.theme-green-dark {
  --color-theme-primary-50: oklch(97.1% 0.014 146.71);
  --color-theme-primary-100: oklch(93.4% 0.038 148.45);
  --color-theme-primary-200: oklch(87.5% 0.068 149.87);
  --color-theme-primary-300: oklch(78.9% 0.118 151.23);
  --color-theme-primary-400: oklch(67.2% 0.195 153.12);
  --color-theme-primary-500: oklch(57.5% 0.252 155.34);
  --color-theme-primary-600: oklch(49.1% 0.268 156.89);
  --color-theme-primary-700: oklch(40.3% 0.248 158.12);
  --color-theme-primary-800: oklch(31.5% 0.215 159.45);
  --color-theme-primary-900: oklch(23.8% 0.165 160.67);
  --color-theme-primary-950: oklch(17.9% 0.125 161.23);
  --color-theme-accent-500: oklch(71.5% 0.185 89.34);
  --color-theme-accent-600: oklch(64.8% 0.205 87.65);
}

/* ===== YELLOW THEME ===== */
/* LIGHT VARIANT - Pastel Yellow */
:root.theme-yellow-light {
  --color-theme-primary-50: oklch(98.8% 0.004 104.23);
  --color-theme-primary-100: oklch(97.2% 0.012 107.45);
  --color-theme-primary-200: oklch(94.8% 0.028 109.87);
  --color-theme-primary-300: oklch(91.2% 0.048 112.34);
  --color-theme-primary-400: oklch(85.9% 0.090 115.12);
  --color-theme-primary-500: oklch(79.6% 0.125 117.89);
  --color-theme-primary-600: oklch(72.3% 0.135 119.45);
  --color-theme-primary-700: oklch(64.1% 0.128 121.23);
  --color-theme-primary-800: oklch(54.2% 0.115 123.12);
  --color-theme-primary-900: oklch(43.8% 0.095 124.67);
  --color-theme-primary-950: oklch(36.5% 0.075 125.34);
  --color-theme-accent-500: oklch(70.2% 0.185 25.12);    /* pastel red */
  --color-theme-accent-600: oklch(62.8% 0.215 22.45);
}

/* DARK VARIANT - Deep Yellow */
:root.theme-yellow-dark {
  --color-theme-primary-50: oklch(97.3% 0.008 104.23);
  --color-theme-primary-100: oklch(93.8% 0.028 107.45);
  --color-theme-primary-200: oklch(88.5% 0.062 109.87);
  --color-theme-primary-300: oklch(80.2% 0.108 112.34);
  --color-theme-primary-400: oklch(69.4% 0.185 115.12);
  --color-theme-primary-500: oklch(59.8% 0.245 117.89);
  --color-theme-primary-600: oklch(51.2% 0.258 119.45);
  --color-theme-primary-700: oklch(42.5% 0.242 121.23);
  --color-theme-primary-800: oklch(33.8% 0.215 123.12);
  --color-theme-primary-900: oklch(25.1% 0.165 124.67);
  --color-theme-primary-950: oklch(18.9% 0.125 125.34);
  --color-theme-accent-500: oklch(58.3% 0.245 25.12);
  --color-theme-accent-600: oklch(51.2% 0.265 22.45);
}

/* ===== NEUTRAL COLORS (used in all themes) ===== */
:root {
  --color-theme-neutral-50: oklch(97.1% 0.001 0);
  --color-theme-neutral-100: oklch(94.2% 0.002 0);
  --color-theme-neutral-200: oklch(89.4% 0.005 0);
  --color-theme-neutral-300: oklch(84.1% 0.008 0);
  --color-theme-neutral-400: oklch(76.3% 0.012 0);
  --color-theme-neutral-500: oklch(64.2% 0.015 0);
  --color-theme-neutral-600: oklch(52.8% 0.014 0);
  --color-theme-neutral-700: oklch(42.3% 0.012 0);
  --color-theme-neutral-800: oklch(30.1% 0.010 0);
  --color-theme-neutral-900: oklch(20.2% 0.008 0);
  --color-theme-neutral-950: oklch(10.1% 0.004 0);
}
```

#### 1.2 Register Theme Variables with Tailwind

**File**: `src/Web/wwwroot/css/input.css` (UPDATED)

```css
@import "tailwindcss";
@import "./themes.css";

@theme {
  --color-theme-primary-*: var(--color-theme-primary-*);
  --color-theme-accent-*: var(--color-theme-accent-*);
  --color-theme-neutral-*: var(--color-theme-neutral-*);
}

@custom-variant dark (&:where(.dark, .dark *));

/* ... rest of input.css ... */
```

**Result**: Tailwind automatically generates utility classes:

- `bg-theme-primary-500`, `text-theme-primary-700`, etc.
- `bg-theme-accent-500`, `border-theme-accent-600`, etc.
- `bg-theme-neutral-100`, `text-theme-neutral-900`, etc.

---

### Phase 2: Update Component Classes

**File**: `src/Web/wwwroot/css/input.css` (UPDATE existing layer)

Replace hardcoded colors with theme variable utilities:

```css
@layer components {
  .container-card {
    @apply mx-auto max-w-7xl mb-6 p-6 sm:px-4 md:px-6 lg:px-8 
           rounded-md shadow-md shadow-theme-primary-500 
           bg-theme-neutral-50 dark:bg-theme-neutral-800 
           transition-colors duration-300;
  }

  .btn-secondary {
    @apply inline-block font-semibold px-5 py-2 rounded-lg text-base 
           leading-tight border-none cursor-pointer transition shadow-sm 
           bg-linear-to-r from-theme-primary-600 to-theme-primary-900 
           text-white hover:from-theme-primary-900 hover:to-theme-primary-600 
           hover:shadow-lg focus:outline-none focus:ring-2 
           focus:ring-theme-primary-500 focus:ring-offset-2 
           active:bg-theme-primary-800 
           dark:from-theme-primary-400 dark:to-theme-primary-700 
           dark:hover:from-theme-primary-700 dark:hover:to-theme-primary-400 
           dark:focus:ring-theme-primary-300;
  }

  .btn-primary {
    @apply inline-block font-semibold px-5 py-2 rounded-lg text-base 
           leading-tight border-none cursor-pointer transition shadow-sm 
           bg-linear-to-r from-theme-accent-600 to-theme-accent-900 
           text-white hover:from-theme-accent-900 hover:to-theme-accent-600 
           hover:shadow-lg focus:outline-none focus:ring-2 
           focus:ring-theme-accent-500 focus:ring-offset-2 
           active:bg-theme-accent-800 
           dark:from-theme-accent-400 dark:to-theme-accent-700 
           dark:hover:from-theme-accent-700 dark:hover:to-theme-accent-400 
           dark:focus:ring-theme-accent-300;
  }

  .btn-warning {
    @apply inline-block font-semibold px-5 py-2 rounded-lg text-base 
           leading-tight border-none cursor-pointer transition shadow-sm 
           bg-linear-to-r from-theme-primary-600 to-theme-primary-900 
           text-white hover:from-theme-primary-900 hover:to-theme-primary-600 
           hover:shadow-lg focus:outline-none focus:ring-2 
           focus:ring-theme-primary-500 focus:ring-offset-2 
           active:bg-theme-primary-800 
           dark:from-theme-primary-400 dark:to-theme-primary-700 
           dark:hover:from-theme-primary-700 dark:hover:to-theme-primary-400 
           dark:focus:ring-theme-primary-300;
  }

  .edit-label {
    @apply block text-theme-neutral-800 dark:text-theme-neutral-300 
           font-semibold mb-2;
  }

  .input-textbox {
    @apply bg-white dark:bg-theme-neutral-800 
           text-theme-neutral-900 dark:text-white 
           border-2 border-theme-primary-500 rounded px-3 py-2 
           focus:outline-none focus:ring-2 focus:ring-theme-accent-500 w-full;
  }

  .nav-bar {
    @apply container flex items-center justify-between max-w-7xl px-6 py-2 
           mx-auto rounded-b-md border-b border-theme-primary-700 
           shadow-md shadow-theme-primary-500 transition-colors duration-200;
  }
}

h1,
h2,
h3,
h4,
h5,
h6 {
  @apply font-bold tracking-tight text-theme-neutral-900 dark:text-theme-neutral-50;
}

a {
  @apply p-1 font-semibold text-theme-neutral-900 
         hover:text-theme-primary-900 dark:text-theme-neutral-50 
         dark:hover:text-theme-primary-700 transition-colors duration-300;
}
```

---

### Phase 3: Create Theme Switching Mechanism

#### 3.1 JavaScript Theme Manager

**File**: `src/Web/wwwroot/js/theme-manager.js` (NEW)

```javascript
/**
 * Theme Manager for Tailwind CSS Color Themes
 * Handles switching between 8 theme combinations:
 * 4 colors (Red, Blue, Green, Yellow) × 2 brightness (Light/Pastel, Dark/Rich)
 */
class ThemeManager {
  static readonly THEMES = {
    RED_LIGHT: 'theme-red-light',
    RED_DARK: 'theme-red-dark',
    BLUE_LIGHT: 'theme-blue-light',
    BLUE_DARK: 'theme-blue-dark',
    GREEN_LIGHT: 'theme-green-light',
    GREEN_DARK: 'theme-green-dark',
    YELLOW_LIGHT: 'theme-yellow-light',
    YELLOW_DARK: 'theme-yellow-dark'
  };
  
  static readonly COLOR_FAMILIES = {
    RED: ['theme-red-light', 'theme-red-dark'],
    BLUE: ['theme-blue-light', 'theme-blue-dark'],
    GREEN: ['theme-green-light', 'theme-green-dark'],
    YELLOW: ['theme-yellow-light', 'theme-yellow-dark']
  };
  
  static readonly BRIGHTNESS_OPTIONS = {
    LIGHT: 'light',
    DARK: 'dark'
  };

  static readonly STORAGE_KEY = 'tailwind-color-theme';
  static readonly DEFAULT_THEME = this.THEMES.BLUE_LIGHT;

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
    }
  }

  /**
   * Set brightness for a specific color family
   * @param {string} colorFamily - RED, BLUE, GREEN, or YELLOW
   * @param {string} brightness - 'light' or 'dark'
   */
  static setBrightness(colorFamily, brightness) {
    const themes = this.COLOR_FAMILIES[colorFamily];
    if (!themes) return;
    
    const themeName = brightness === this.BRIGHTNESS_OPTIONS.DARK 
      ? themes[1] 
      : themes[0];
    
    this.setTheme(themeName);
  }

  /**
   * Set color while preserving current brightness
   * @param {string} colorFamily - RED, BLUE, GREEN, or YELLOW
   */
  static setColor(colorFamily) {
    const currentBrightness = this.getCurrentBrightness();
    this.setBrightness(colorFamily, currentBrightness);
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
   * Get current color (without brightness)
   * @returns {string} Color family name
   */
  static getCurrentColor() {
    const current = this.getCurrentTheme();
    for (const [family, themes] of Object.entries(this.COLOR_FAMILIES)) {
      if (themes.includes(current)) return family;
    }
    return 'BLUE';
  }

  /**
   * Get current brightness
   * @returns {string} 'light' or 'dark'
   */
  static getCurrentBrightness() {
    const current = this.getCurrentTheme();
    return current.includes('light') ? this.BRIGHTNESS_OPTIONS.LIGHT : this.BRIGHTNESS_OPTIONS.DARK;
  }

  /**
   * Get all available themes
   * @returns {Object} Themes object
   */
  static getAvailableThemes() {
    return this.THEMES;
  }

  /**
   * Get themes for a specific color family
   * @param {string} colorFamily - RED, BLUE, GREEN, or YELLOW
   * @returns {Array} Array of light and dark theme names
   */
  static getColorFamilyThemes(colorFamily) {
    return this.COLOR_FAMILIES[colorFamily] || [];
  }
}

// Initialize theme on page load
document.addEventListener('DOMContentLoaded', () => {
  ThemeManager.initialize();
});
```

#### 3.2 Reference in Layout

**File**: `src/Web/Components/Layout/MainLayout.razor`

Add to `<head>` section:

```razor
<script src="~/js/theme-manager.js"></script>
```

#### 3.3 Create Theme Selector Component

**File**: `src/Web/Components/ThemeSelector.razor` (NEW)

```razor
@inject IJSRuntime JS

<div class="theme-selector-container">
  <div class="theme-controls">
    <div class="color-selector">
      <label class="theme-selector-label">Color:</label>
      <div class="button-group">
        <button @onclick="() => SelectColor('RED')" class="color-btn">Red</button>
        <button @onclick="() => SelectColor('BLUE')" class="color-btn active">Blue</button>
        <button @onclick="() => SelectColor('GREEN')" class="color-btn">Green</button>
        <button @onclick="() => SelectColor('YELLOW')" class="color-btn">Yellow</button>
      </div>
    </div>
    
    <div class="brightness-selector">
      <label class="theme-selector-label">Brightness:</label>
      <div class="button-group">
        <button @onclick="() => SelectBrightness('light')" class="brightness-btn active">Light (Pastel)</button>
        <button @onclick="() => SelectBrightness('dark')" class="brightness-btn">Dark (Rich)</button>
      </div>
    </div>
  </div>
  
  <div class="theme-info">
    Current: <strong id="current-theme">Blue Light</strong>
  </div>
</div>

@code {
  protected override async Task OnInitializedAsync()
  {
    // Sync UI with current theme on load
    await JS.InvokeVoidAsync("eval", @"
      const currentTheme = ThemeManager.getCurrentTheme();
      const color = ThemeManager.getCurrentColor();
      const brightness = ThemeManager.getCurrentBrightness();
      document.querySelectorAll('.color-btn').forEach(btn => {
        btn.classList.toggle('active', btn.textContent.toUpperCase() === color);
      });
      document.querySelectorAll('.brightness-btn').forEach(btn => {
        btn.classList.toggle('active', btn.textContent.toLowerCase().includes(brightness));
      });
      document.getElementById('current-theme').textContent = color + ' ' + brightness.charAt(0).toUpperCase() + brightness.slice(1);
    ");
  }

  private async Task SelectColor(string color)
  {
    await JS.InvokeVoidAsync("eval", $@"
      ThemeManager.setColor('{color}');
      const brightness = ThemeManager.getCurrentBrightness();
      document.getElementById('current-theme').textContent = '{color}' + ' ' + brightness.charAt(0).toUpperCase() + brightness.slice(1);
      document.querySelectorAll('.color-btn').forEach(btn => {{
        btn.classList.toggle('active', btn.textContent.toUpperCase() === '{color}');
      }});
    ");
  }

  private async Task SelectBrightness(string brightness)
  {
    await JS.InvokeVoidAsync("eval", $@"
      const color = ThemeManager.getCurrentColor();
      ThemeManager.setBrightness(color, '{brightness}');
      document.getElementById('current-theme').textContent = color + ' ' + '{brightness}'.charAt(0).toUpperCase() + '{brightness}'.slice(1);
      document.querySelectorAll('.brightness-btn').forEach(btn => {{
        btn.classList.toggle('active', btn.textContent.toLowerCase().includes('{brightness}'));
      }});
    ");
  }
}
```

**Styling** (add to `input.css` components layer):

```css
.theme-selector-container {
  @apply flex flex-col gap-4 p-4 rounded-lg bg-theme-neutral-100 dark:bg-theme-neutral-800;
}

.theme-controls {
  @apply flex flex-col gap-4 md:flex-row md:gap-6;
}

.color-selector,
.brightness-selector {
  @apply flex flex-col gap-2;
}

.theme-selector-label {
  @apply font-semibold text-theme-neutral-700 dark:text-theme-neutral-300 text-sm uppercase tracking-wider;
}

.button-group {
  @apply flex gap-2;
}

.color-btn,
.brightness-btn {
  @apply px-3 py-2 rounded-md font-medium text-sm transition-all duration-200
         bg-theme-neutral-200 dark:bg-theme-neutral-700
         text-theme-neutral-700 dark:text-theme-neutral-300
         hover:bg-theme-primary-300 dark:hover:bg-theme-primary-700
         border-2 border-transparent
         cursor-pointer;
}

.color-btn.active,
.brightness-btn.active {
  @apply bg-theme-primary-500 text-white border-theme-primary-700 dark:border-theme-primary-300;
}

.theme-info {
  @apply text-sm font-medium text-theme-neutral-600 dark:text-theme-neutral-400;
}
```

---

### Phase 4: Optional - Create Theme Configuration Component

**File**: `src/Web/Components/Settings/ThemeSettings.razor` (OPTIONAL)

For a dedicated settings page:

```razor
@page "/settings/theme"
@inject IJSRuntime JS

<div class="theme-settings">
  <h2>Select Your Color Theme</h2>
  
  <div class="theme-grid">
    <button class="theme-card theme-card--red" @onclick="() => SelectTheme('theme-red')">
      <span class="theme-card-indicator"></span>
      <span>Red</span>
    </button>
    <button class="theme-card theme-card--blue" @onclick="() => SelectTheme('theme-blue')">
      <span class="theme-card-indicator"></span>
      <span>Blue</span>
    </button>
    <button class="theme-card theme-card--green" @onclick="() => SelectTheme('theme-green')">
      <span class="theme-card-indicator"></span>
      <span>Green</span>
    </button>
    <button class="theme-card theme-card--yellow" @onclick="() => SelectTheme('theme-yellow')">
      <span class="theme-card-indicator"></span>
      <span>Yellow</span>
    </button>
  </div>
</div>

@code {
  private async Task SelectTheme(string themeName)
  {
    await JS.InvokeVoidAsync("eval", $"ThemeManager.setTheme('{themeName}');");
    // Optional: Show confirmation
  }
}
```

---

## Technical Specifications

### File Structure

```
src/Web/
├── wwwroot/
│   ├── css/
│   │   ├── input.css          (UPDATED - add @theme directive)
│   │   ├── themes.css          (NEW - theme color definitions)
│   │   └── app.css            (AUTO-GENERATED by Tailwind)
│   └── js/
│       └── theme-manager.js    (NEW - theme switching logic)
├── Components/
│   ├── ThemeSelector.razor     (NEW - theme selector control)
│   ├── Settings/
│   │   └── ThemeSettings.razor (NEW OPTIONAL - dedicated settings page)
│   └── Layout/
│       └── MainLayout.razor    (UPDATED - reference theme-manager.js)
└── [other existing files unchanged]
```

### Color Theme Specifications

| Theme  | Primary | Accent    | Use Case                          |
|--------|---------|-----------|----------------------------------|
| **Red**    | Red     | Orange    | Warnings, critical actions        |
| **Blue**   | Blue    | Amber     | Primary actions, default theme    |
| **Green**  | Green   | Yellow    | Success, affirmative actions      |
| **Yellow** | Yellow  | Red       | Caution, attention-grabbing       |

Each theme includes:

- **Primary Colors** (50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950)
- **Accent Colors** (500, 600)
- **Neutral Colors** (50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950)

---

## Implementation Strategy

### Migration Approach: Non-Breaking, Incremental

1. **Week 1: Foundation**
   - Add `themes.css` file with all color definitions
   - Update `input.css` with `@theme` directive
   - Build CSS and verify new utilities are generated
   - No visual changes yet

2. **Week 2: Component Updates**
   - Replace component class definitions with theme variables
   - Test with default Blue theme (backward compatible look)
   - Update 4-5 most-used component classes first

3. **Week 3: Theme Switching**
   - Add `theme-manager.js`
   - Create `ThemeSelector.razor` component
   - Integrate into `MainLayout.razor`
   - Manual testing of theme switching

4. **Week 4: Polish & Testing**
   - Update all remaining components
   - Test dark mode with all themes
   - Create user documentation
   - Gather feedback

---

## Benefits

### For Developers

- **Single source of truth** for colors (themes.css)
- **One-line changes** to update all theme colors
- **Auto-generated utilities** - no custom color names to memorize
- **Zero JavaScript overhead** - CSS variable based
- **Easy A/B testing** - swap themes instantly

### For Users

- **Consistent experience** across all pages/components
- **Choice** between four carefully-designed color palettes
- **Persistent preference** saved to localStorage
- **Instant visual feedback** when changing themes

### For Design System

- **Scalable**: Add new themes by creating new `:root.theme-*` selectors
- **Maintainable**: Color changes centralized in one file
- **Future-proof**: Uses modern Tailwind 4.1+ standards
- **Accessible**: All colors use OKLCH (perceptually uniform)

---

## Risk Mitigation

| Risk | Mitigation |
|------|-----------|
| Breaking existing styles | Incremental migration; test each component |
| Browser compatibility | OKLCH supported in all modern browsers; fallback to hex if needed |
| Performance impact | CSS-based only; zero runtime overhead vs. config-based approach |
| User preference loss | localStorage provides persistence |

---

## Success Metrics

- ✅ All 8 theme combinations (4 colors × 2 brightness) switchable without errors
- ✅ Light (pastel) variants display soft, gentle colors
- ✅ Dark (rich) variants display bold, saturated colors
- ✅ Theme switching preserves color family (e.g., Red Light → Red Dark)
- ✅ User preference persists across sessions with localStorage
- ✅ No visual regression compared to current design
- ✅ Build time unchanged (CSS generation only)
- ✅ All component classes updated to use theme variables (100% migration)

---

## Appendix: Quick Reference

### Using Theme Colors in Components

**In Razor Components (inline styles):**

```razor
<div style="background-color: var(--color-theme-primary-500);">
  Content
</div>
```

**In Tailwind @apply:**

```css
.my-component {
  @apply bg-theme-primary-500 text-theme-neutral-900;
}
```

**In HTML classes:**

```html
<div class="bg-theme-accent-600 text-theme-neutral-50 p-4 rounded-lg">
  Styled with theme colors
</div>
```

### Adding a New Theme

To add a fifth theme (e.g., Purple):

```css
/* In themes.css */
:root.theme-purple {
  --color-theme-primary-50: oklch(...);
  /* ... define all 11 primary shades ... */
  --color-theme-accent-500: oklch(...);
  --color-theme-accent-600: oklch(...);
}
```

```javascript
// In theme-manager.js - update THEMES object
static readonly THEMES = {
  RED: 'theme-red',
  BLUE: 'theme-blue',
  GREEN: 'theme-green',
  YELLOW: 'theme-yellow',
  PURPLE: 'theme-purple'  // NEW
};
```

Done! No other changes needed.

---

## References

- [Tailwind CSS Theme Variables Documentation](https://tailwindcss.com/docs/theme)
- [Tailwind CSS Dark Mode](https://tailwindcss.com/docs/dark-mode)
- [OKLCH Color Space](https://oklch.com/)
- [CSS Variables MDN](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)

---

**Document End**
