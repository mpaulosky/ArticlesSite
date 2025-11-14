// Theme management helper functions for light/dark mode toggle
window.themeHelper = {
    /**
     * Gets the current theme from localStorage
     * @returns {string} 'light' or 'dark', defaults to 'dark'
     */
    get: () => {
        const theme = localStorage.getItem('theme');
        return theme || 'dark';
    },

    /**
     * Sets the theme in localStorage
     * @param {string} theme - 'light' or 'dark'
     */
    set: (theme) => {
        localStorage.setItem('theme', theme);
    },

    /**
     * Applies the theme to the document by adding/removing the 'dark' class
     * @param {string} theme - 'light' or 'dark'
     */
    applyTheme: (theme) => {
        if (theme === 'dark') {
            document.documentElement.classList.add('dark');
        } else {
            document.documentElement.classList.remove('dark');
        }
    },

    /**
     * Initializes theme on page load (called immediately)
     */
    initialize: () => {
        const theme = window.themeHelper.get();
        window.themeHelper.applyTheme(theme);
    }
};

// Apply theme immediately on script load to prevent flash
window.themeHelper.initialize();
