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
        readTextFromClipboard: function(callback) {
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
        copyTextToClipboard: function(text, callback) {
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
        /**
         * Check if clipboard contains text data by attempting to read it.
         * @param {function(boolean): void} callback - Called with true if clipboard has text data, false otherwise.
         */
        checkClipboardHasTextData: function(callback) {
            if (navigator.clipboard && navigator.clipboard.readText) {
                navigator.clipboard.readText()
                    .then(function(data) { callback(data !== null && data.length > 0); })
                    .catch(function() { callback(false); });
            } else {
                callback(false);
            }
        },
        /**
         * Check if clipboard contains an image (image/png).
         * @param {function(boolean): void} callback - Called with true if clipboard has image data, false otherwise.
         */
        checkClipboardHasImage: function(callback) {
            if (navigator.clipboard && navigator.clipboard.read) {
                navigator.clipboard.read()
                    .then(function(items) {
                        for (var i = 0; i < items.length; i++) {
                            if (items[i].types.indexOf('image/png') !== -1) {
                                callback(true);
                                return;
                            }
                        }
                        callback(false);
                    })
                    .catch(function() { callback(false); });
            } else {
                callback(false);
            }
        },
        /**
         * Read image (image/png) from clipboard.
         * @param {function(Uint8Array|null, Error|null): void} callback - Called with image data on success, or error on failure.
         */
        readImageFromClipboard: function(callback) {
            if (navigator.clipboard && navigator.clipboard.read) {
                navigator.clipboard.read()
                    .then(function(items) {
                        for (var i = 0; i < items.length; i++) {
                            if (items[i].types.indexOf('image/png') !== -1) {
                                items[i].getType('image/png')
                                    .then(function(blob) {
                                        return blob.arrayBuffer();
                                    })
                                    .then(function(buffer) {
                                        callback(new Uint8Array(buffer), null);
                                    })
                                    .catch(function(err) {
                                        callback(null, err);
                                    });
                                return;
                            }
                        }
                        callback(null, new Error('No image/png found in clipboard'));
                    })
                    .catch(function(err) {
                        callback(null, err);
                    });
            } else {
                callback(null, new Error('NOT_SUPPORTED'));
            }
        },
    },

    /**
     * Prepare to copy text to clipboard on next user gesture.
     * @param {number} textPtr - Pointer to UTF8 string in WASM memory.
     * @param {number} callback - WASM table entry index. Called with 1 on success, 0 on failure.
     */
    WebGL_PrepareCopyTextToClipboard: function(textPtr, callback) {
        const text = UTF8ToString(textPtr);
        const callbackEntry = getWasmTableEntry(callback);
        const listener = function(event) {
            document.removeEventListener('pointerup', listener);
            browserClipboard.copyTextToClipboard(text, success => {
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
    WebGL_CopyTextToClipboard: function(textPtr, callback) {
        const text = UTF8ToString(textPtr);
        const callbackEntry = getWasmTableEntry(callback);
        browserClipboard.copyTextToClipboard(text, success => {
            callbackEntry(success ? 1 : 0);
        });
    },

    /**
     * Prepare to read text from clipboard on next user gesture.
     * @param {number} callback - WASM table entry index. Called with data pointer and length on success.
     */
    WebGL_PrepareReadTextFromClipboard: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        const listener = function(event) {
            document.removeEventListener('pointerup', listener);
            browserClipboard.readTextFromClipboard((data, error) => {
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
    WebGL_ReadTextFromClipboard: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.readTextFromClipboard((data, error) => {
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
     * Free clipboard data allocated by WebGL_ReadTextFromClipboard or WebGL_PrepareReadTextFromClipboard.
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
    },

    /**
     * Check if clipboard contains text data by attempting to read it.
     * @param {number} callback - WASM table entry index. Called with 1 if clipboard has text data, 0 otherwise.
     */
    WebGL_CheckClipboardHasTextData: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.checkClipboardHasTextData(function(hasData) {
            callBackEntry(hasData ? 1 : 0);
        });
    },

    /**
     * Check if clipboard contains image (image/png) data.
     * @param {number} callback - WASM table entry index. Called with 1 if clipboard has image data, 0 otherwise.
     */
    WebGL_CheckClipboardHasImageData: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.checkClipboardHasImage(function(hasImage) {
            callBackEntry(hasImage ? 1 : 0);
        });
    },

    /**
     * Read image (image/png) from clipboard.
     * @param {number} callback - WASM table entry index. Called with dataPtr (pointer to bytes) and length.
     */
    WebGL_ReadImageFromClipboard: function(callback) {
        const callBackEntry = getWasmTableEntry(callback);
        browserClipboard.readImageFromClipboard(function(data, error) {
            if (error || data === null) {
                callBackEntry(0, 0);
                return;
            }
            var dataPtr = _malloc(data.length);
            HEAPU8.set(data, dataPtr);
            callBackEntry(dataPtr, data.length);
            // Memory is freed by C# via WebGL_FreeClipboardData after copying
        });
    }
};

autoAddDeps(BrowserClipboard, '$browserClipboard');
mergeInto(LibraryManager.library, BrowserClipboard);
