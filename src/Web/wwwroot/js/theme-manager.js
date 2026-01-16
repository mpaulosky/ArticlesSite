/**
 * Theme Manager for Tailwind CSS Color Themes
 * Handles switching between 8 theme combinations:
 * 4 colors (Red, Blue, Green, Yellow) × 2 brightness (Light/Pastel, Dark/Rich)
 */
class ThemeManager {
  static THEMES = {
    RED_LIGHT: 'theme-red-light',
    RED_DARK: 'theme-red-dark',
    BLUE_LIGHT: 'theme-blue-light',
    BLUE_DARK: 'theme-blue-dark',
    GREEN_LIGHT: 'theme-green-light',
    GREEN_DARK: 'theme-green-dark',
    YELLOW_LIGHT: 'theme-yellow-light',
    YELLOW_DARK: 'theme-yellow-dark'
  };
  
  static COLOR_FAMILIES = {
    RED: ['theme-red-light', 'theme-red-dark'],
    BLUE: ['theme-blue-light', 'theme-blue-dark'],
    GREEN: ['theme-green-light', 'theme-green-dark'],
    YELLOW: ['theme-yellow-light', 'theme-yellow-dark']
  };
  
  static BRIGHTNESS_OPTIONS = {
    LIGHT: 'light',
    DARK: 'dark'
  };

  static STORAGE_KEY = 'tailwind-color-theme';
  static DEFAULT_THEME = this.THEMES.BLUE_LIGHT;

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
