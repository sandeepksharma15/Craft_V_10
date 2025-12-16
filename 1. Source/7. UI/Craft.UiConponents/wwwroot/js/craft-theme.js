/**
 * Craft UI Components - Theme Management Module
 * Handles theme persistence and system preference detection.
 */

const STORAGE_KEY = 'craft-theme';

/**
 * Gets the stored theme preference from local storage.
 * @returns {string|null} The stored theme name or null if not set.
 */
export function getStoredTheme() {
    try {
        return localStorage.getItem(STORAGE_KEY);
    } catch {
        return null;
    }
}

/**
 * Stores the theme preference in local storage.
 * @param {string} theme - The theme name to store.
 */
export function storeTheme(theme) {
    try {
        localStorage.setItem(STORAGE_KEY, theme);
    } catch {
        // Storage not available
    }
}

/**
 * Gets the system's color scheme preference.
 * @returns {boolean} True if the system prefers dark mode.
 */
export function getSystemPreference() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches;
}

/**
 * Applies the specified theme to the document.
 * @param {string} theme - The theme to apply ('light' or 'dark').
 */
export function applyTheme(theme) {
    const root = document.documentElement;
    
    root.setAttribute('data-craft-theme', theme);
    root.classList.remove('craft-theme-light', 'craft-theme-dark');
    root.classList.add(`craft-theme-${theme}`);
    
    // Update meta theme-color for mobile browsers
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
        metaThemeColor.setAttribute('content', theme === 'dark' ? '#1a1a2e' : '#ffffff');
    }
}

/**
 * Registers a callback for system theme changes.
 * @param {Function} callback - The callback to invoke when system theme changes.
 * @returns {Function} A function to remove the listener.
 */
export function onSystemThemeChange(callback) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');
    
    const handler = (e) => {
        callback(e.matches);
    };
    
    mediaQuery.addEventListener('change', handler);
    
    return () => mediaQuery.removeEventListener('change', handler);
}
