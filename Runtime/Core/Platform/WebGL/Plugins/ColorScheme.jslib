const ColorScheme = {
    // Private storage for unregistration functions
    $colorSchemeWatchers: {},
    WebGL_RegisterCallbackPreferredColorSchemeChanged: function (callback) {
        // Clean up any existing watcher for this specific callback pointer
        if (typeof colorSchemeWatchers !== 'undefined' && colorSchemeWatchers[callback]) {
            colorSchemeWatchers[callback]();
        }

        const mqString = `(prefers-color-scheme: dark)`;
        const media = window.matchMedia(mqString);

        // This function handles the logic and notifies Unity
        const listener = (event) => {
            const isDarkMode = event.matches ? 1 : 0;
            getWasmTableEntry(callback)(isDarkMode);
        };

        // Start listening
        media.addEventListener("change", listener);

        // Store the unregistration function
        colorSchemeWatchers[callback] = () => {
            media.removeEventListener("change", listener);
            delete colorSchemeWatchers[callback];
        };

        // Initial execution to sync current state
        const initialMode = media.matches ? 1 : 0;
        getWasmTableEntry(callback)(initialMode);
    },
    WebGL_UnregisterCallbackPreferredColorSchemeChanged: function (callback) {
        if (colorSchemeWatchers[callback]) {
            colorSchemeWatchers[callback]();
        }
    },
    WebGL_IsDarkPreferredColorScheme: function () {
        const mqString = `(prefers-color-scheme: dark)`;
        const media = matchMedia(mqString);
        return media.matches ? 1 : 0;
    },
};

autoAddDeps(ColorScheme, '$colorSchemeWatchers');
mergeInto(LibraryManager.library, ColorScheme);
