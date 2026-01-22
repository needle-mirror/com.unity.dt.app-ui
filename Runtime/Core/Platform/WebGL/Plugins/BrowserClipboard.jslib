const BrowserClipboard = {
    /**
     * @namespace browserClipboard
     * @description Provides clipboard interaction methods for WebGL builds.
     */
    $browserClipboard: {
        /**
         * Read text from clipboard.
         * @param {function(string|null, Error|null): void} callback - Called with data (string) on success, or error (Error object) on failure.
         */
        readFromClipboard: function(callback) {
            // Try modern Clipboard API first
            if (navigator.clipboard && navigator.clipboard.readText) {
                navigator.clipboard
                    .readText()
                    .then(data => {
                        callback(data, null);
                    })
                    .catch(function(err) {
                        callback(null, err);
                    });
            } else {
                // Clipboard API not available
                callback(null, new Error('NOT_SUPPORTED'));
            }
        },
        /**
         * Copy text to clipboard.
         * @param {string} text - The text to copy to clipboard.
         * @param {function(boolean): void} callback - Called with true on success, false on failure.
         */
        copyToClipboard: function(text, callback) {
            // Fallback to execCommand
            const fallBack = function() {
                const textArea = document.createElement('textarea');
                textArea.value = text;
                textArea.style.position = 'fixed';
                textArea.style.left = '-999999px';
                textArea.style.top = '-999999px';
                textArea.style.opacity = '0';
                document.body.appendChild(textArea);
                textArea.focus();
                textArea.select();

                try {
                    if (document.execCommand('copy')) {
                        callback(true);
                    } else {
                        callback(false);
                    }
                } catch (e) {
                    callback(false);
                }

                document.body.removeChild(textArea);
            };

            // Try modern Clipboard API first
            if (navigator.clipboard && navigator.clipboard.writeText && window.isSecureContext) {
                navigator.clipboard.writeText(text).then(function() {
                    callback(true);
                }).catch(function(err) {
                    fallBack();
                });
            } else {
                fallBack();
            }
        },
        /**
         * Check if clipboard is accessible.
         * @param {function(boolean): void} callback - Called with true if accessible, false otherwise.
         */
        checkClipboardAccess: function(callback) {
            // Try to check permissions
            if (navigator.permissions && navigator.permissions.query) {
                // navigator.permissions.query exists, but 'clipboard-read' may not be supported in all browsers
                navigator.permissions.query({ name: 'clipboard-read' }).then(function(result) {
                    if (result.state === 'granted' || result.state === 'prompt') {
                        callback(true);
                    } else {
                        callback(false);
                    }
                }).catch(function(err) {
                    // Permission check failed, assume no access unless it's a TypeError (unsupported permission name)
                    if (err instanceof TypeError) {
                        callback(true);
                    } else {
                        callback(false);
                    }
                });
            } else {
                // Permissions API not available, assume access
                callback(true);
            }
        },
    },

    /**
     * Prepare to copy text to clipboard on next user gesture.
     * @param {number} textPtr - Pointer to UTF8 string in WASM memory.
     * @param {number} callback - WASM table entry index. Called with 1 on success, 0 on failure.
     */
    WebGL_PrepareCopyToClipboard: function(textPtr, callback) {
        const text = UTF8ToString(textPtr);
        const callbackEntry = getWasmTableEntry(callback);
        const listener = function(event) {
            document.removeEventListener('pointerup', listener);
            browserClipboard.copyToClipboard(text, success => {
                callbackEntry(success ? 1 : 0);
            });
        };
        document.addEventListener('pointerup', listener);
    },

    /**
     * Copy text to clipboard.
     * @param {number} textPtr - Pointer to UTF8 string in WASM memory.
     * @param {number} callback - WASM table entry index. Called with 1 on success, 0 on failure.
     */
    WebGL_CopyToClipboard: function(textPtr, callback) {
        const text = UTF8ToString(textPtr);
        const callbackEntry = getWasmTableEntry(callback);
        browserClipboard.copyToClipboard(text, success => {
            callbackEntry(success ? 1 : 0);
        });
    },

    /**
     * Prepare to read text from clipboard on next user gesture.
     * @param {number} callback - WASM table entry index. Called with data pointer and length on success.
     */
    WebGL_PrepareReadFromClipboard: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        const listener = function(event) {
            document.removeEventListener('pointerup', listener);
            browserClipboard.readFromClipboard((data, error) => {
                if (error || data === null) {
                    callBackEntry(0, 0);
                    return;
                }
                const lengthBytes = lengthBytesUTF8(data) + 1;
                const dataPtr = _malloc(lengthBytes);
                stringToUTF8(data, dataPtr, lengthBytes);
                callBackEntry(dataPtr, lengthBytes - 1);
                // Memory is freed by C# via WebGL_FreeClipboardData after copying
            });
        };
        document.addEventListener('pointerup', listener);
    },

    /**
     * Read text from clipboard.
     * @param {number} callback - WASM table entry index. Called with dataPtr (pointer to UTF8 string) and length.
     */
    WebGL_ReadFromClipboard: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.readFromClipboard((data, error) => {
            if (error || data === null) {
                callBackEntry(0, 0);
                return;
            }
            const lengthBytes = lengthBytesUTF8(data) + 1;
            const dataPtr = _malloc(lengthBytes);
            stringToUTF8(data, dataPtr, lengthBytes);
            callBackEntry(dataPtr, lengthBytes - 1);
            // Memory is freed by C# via WebGL_FreeClipboardData after copying
        });
    },

    /**
     * Free clipboard data allocated by WebGL_ReadFromClipboard or WebGL_PrepareReadFromClipboard.
     * @param {number} dataPtr - Pointer to the data to free.
     */
    WebGL_FreeClipboardData: function(dataPtr) {
        if (dataPtr !== 0) {
            // Defer free to avoid potential issues if called within the callback stack
            setTimeout(function() {
                _free(dataPtr);
            }, 0);
        }
    },

    /**
     * Check if clipboard has accessible data.
     * @param {number} callback - WASM table entry index. Called with 1 if accessible, 0 otherwise.
     */
    WebGL_CheckClipboardAccess: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.checkClipboardAccess(hasAccess => {
            callBackEntry(hasAccess ? 1 : 0);
        });
    }
};

autoAddDeps(BrowserClipboard, '$browserClipboard');
mergeInto(LibraryManager.library, BrowserClipboard);
