const DevicePixelRatio = {
    // Private storage for unregistration functions
    $devicePixelRatioWatchers: {},
    WebGL_RegisterCallbackDevicePixelRatioChanged: function (callback) {
        // Clean up any existing watcher for this specific callback pointer
        if (typeof devicePixelRatioWatchers !== 'undefined' && devicePixelRatioWatchers[callback]) {
            devicePixelRatioWatchers[callback]();
        }

        let currentRatio = window.devicePixelRatio;

        const setupListener = () => {
            const mqString = `(resolution: ${currentRatio}dppx)`;
            const media = window.matchMedia(mqString);

            const listener = (event) => {
                if (!event.matches) {
                    currentRatio = window.devicePixelRatio;
                    getWasmTableEntry(callback)(currentRatio);
                    // Re-register with new ratio
                    setupListener();
                }
            };

            media.addEventListener("change", listener, { once: true });

            // Store the unregistration function
            devicePixelRatioWatchers[callback] = () => {
                media.removeEventListener("change", listener);
                delete devicePixelRatioWatchers[callback];
            };
        };

        setupListener();

        // Initial execution to sync current state
        getWasmTableEntry(callback)(currentRatio);
    },
    WebGL_UnregisterCallbackDevicePixelRatioChanged: function (callback) {
        if (devicePixelRatioWatchers[callback]) {
            devicePixelRatioWatchers[callback]();
        }
    },
    WebGL_GetDevicePixelRatio: function () {
        return window.devicePixelRatio;
    },
};

autoAddDeps(DevicePixelRatio, '$devicePixelRatioWatchers');
mergeInto(LibraryManager.library, DevicePixelRatio);
