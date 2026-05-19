#if (UNITY_WEBGL && !UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AOT;

namespace Unity.AppUI.Core
{
    class WebGLPlatformImpl : PlatformImpl
    {
        // Delegates for callbacks
        delegate void CopyCallbackDelegate(int success);
        delegate void ReadCallbackDelegate(IntPtr dataPtr, uint length);
        delegate void CheckAccessCallbackDelegate(int hasAccess);
        delegate void CheckHasDataCallbackDelegate(int hasData);
        delegate void ThemeChangedDelegate(int isDarkColorScheme);
        delegate void DevicePixelRatioChangedDelegate(float devicePixelRatio);

        [StructLayout(LayoutKind.Sequential)]
        struct PluginConfigData
        {
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ThemeChangedDelegate ThemeChangedCSharpHandler;

            [MarshalAs(UnmanagedType.FunctionPtr)]
            public DevicePixelRatioChangedDelegate DevicePixelRatioChangedCSharpHandler;
        }

        [MonoPInvokeCallback(typeof(ThemeChangedDelegate))]
        static void InvokeThemeChangedProxy(int darkModeEnabled) =>
            s_Instance?.InvokeThemeChanged(darkModeEnabled == 1);

        [MonoPInvokeCallback(typeof(DevicePixelRatioChangedDelegate))]
        static void InvokeDevicePixelRatioChangedProxy(float devicePixelRatio) =>
            s_Instance?.InvokeScaleFactorChanged(devicePixelRatio);

        // P/Invoke declarations
        [DllImport("__Internal")]
        static extern void WebGL_PrepareCopyTextToClipboard(string text, IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_PrepareReadTextFromClipboard(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_CheckClipboardAccess(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_CheckClipboardHasTextData(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_FreeClipboardData(IntPtr dataPtr);

        [DllImport("__Internal")]
        static extern void WebGL_CheckClipboardHasImageData(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_ReadImageFromClipboard(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_RegisterCallbackPreferredColorSchemeChanged(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_UnregisterCallbackPreferredColorSchemeChanged(IntPtr callback);

        [DllImport("__Internal")]
        static extern int WebGL_IsDarkPreferredColorScheme();

        [DllImport("__Internal")]
        static extern void WebGL_RegisterCallbackDevicePixelRatioChanged(IntPtr callback);

        [DllImport("__Internal")]
        static extern void WebGL_UnregisterCallbackDevicePixelRatioChanged(IntPtr callback);

        [DllImport("__Internal")]
        static extern float WebGL_GetDevicePixelRatio();

        // Cached clipboard data
        static byte[] s_CachedClipboardData;

        // Singleton instance
        static WebGLPlatformImpl s_Instance;

        // Cached delegate instances to prevent GC from collecting them while JS holds function pointers
        static readonly CopyCallbackDelegate s_CopyCallback = OnCopyComplete;
        static readonly ReadCallbackDelegate s_ReadCallback = OnReadComplete;
        static readonly CheckAccessCallbackDelegate s_CheckAccessCallback = OnCheckAccessComplete;
        static readonly CheckHasDataCallbackDelegate s_CheckHasDataCallback = OnCheckHasDataComplete;
        static readonly CheckHasDataCallbackDelegate s_CheckHasImageDataCallback = OnCheckHasImageDataComplete;
        static readonly ReadCallbackDelegate s_ReadImageCallback = OnReadImageComplete;

        // Cached function pointers derived from the delegates
        static readonly IntPtr s_CopyCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_CopyCallback);
        static readonly IntPtr s_ReadCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_ReadCallback);
        static readonly IntPtr s_CheckAccessCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_CheckAccessCallback);
        static readonly IntPtr s_CheckHasDataCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_CheckHasDataCallback);
        static readonly IntPtr s_CheckHasImageDataCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_CheckHasImageDataCallback);
        static readonly IntPtr s_ReadImageCallbackPtr = Marshal.GetFunctionPointerForDelegate(s_ReadImageCallback);

        // Instance-scoped queues for async operations (handles overlapping requests)
        readonly Queue<TaskCompletionSource<bool>> m_CopyQueue = new Queue<TaskCompletionSource<bool>>();
        readonly Queue<TaskCompletionSource<byte[]>> m_ReadQueue = new Queue<TaskCompletionSource<byte[]>>();
        readonly Queue<TaskCompletionSource<bool>> m_CheckAccessQueue = new Queue<TaskCompletionSource<bool>>();
        readonly Queue<TaskCompletionSource<bool>> m_CheckHasDataQueue = new Queue<TaskCompletionSource<bool>>();
        readonly Queue<TaskCompletionSource<bool>> m_CheckHasImageDataQueue = new Queue<TaskCompletionSource<bool>>();
        readonly Queue<TaskCompletionSource<byte[]>> m_ReadImageQueue = new Queue<TaskCompletionSource<byte[]>>();

        PluginConfigData m_ConfigData;

        public WebGLPlatformImpl() => Setup();

        void Setup()
        {
            CleanUp();
            // Start monitoring preferred color scheme and device pixel ratio
            try
            {
                m_ConfigData = new PluginConfigData
                {
                    ThemeChangedCSharpHandler = InvokeThemeChangedProxy,
                    DevicePixelRatioChangedCSharpHandler = InvokeDevicePixelRatioChangedProxy
                };
                var themeChangedCallbackPtr = Marshal.GetFunctionPointerForDelegate(m_ConfigData.ThemeChangedCSharpHandler);
                WebGL_RegisterCallbackPreferredColorSchemeChanged(themeChangedCallbackPtr);

                var devicePixelRatioChangedCallbackPtr = Marshal.GetFunctionPointerForDelegate(m_ConfigData.DevicePixelRatioChangedCSharpHandler);
                WebGL_RegisterCallbackDevicePixelRatioChanged(devicePixelRatioChangedCallbackPtr);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to monitor preferred color scheme or device pixel ratio: {ex.Message}");
            }
            s_Instance = this;
        }

        ~WebGLPlatformImpl() => CleanUp();

        void CleanUp()
        {
            if (s_Instance == null)
                return;

            try
            {
                var themeChangedCallbackPtr = Marshal.GetFunctionPointerForDelegate(m_ConfigData.ThemeChangedCSharpHandler);
                WebGL_UnregisterCallbackPreferredColorSchemeChanged(themeChangedCallbackPtr);

                var devicePixelRatioChangedCallbackPtr = Marshal.GetFunctionPointerForDelegate(m_ConfigData.DevicePixelRatioChangedCSharpHandler);
                WebGL_UnregisterCallbackDevicePixelRatioChanged(devicePixelRatioChangedCallbackPtr);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to clean up WebGLPlatformImpl: {ex.Message}");
            }

            s_Instance = null;
        }

        // Callback methods
        [MonoPInvokeCallback(typeof(CopyCallbackDelegate))]
        static void OnCopyComplete(int success)
        {
            if (s_Instance != null && s_Instance.m_CopyQueue.Count > 0)
            {
                var tcs = s_Instance.m_CopyQueue.Dequeue();
                tcs.TrySetResult(success == 1);
            }
        }

        [MonoPInvokeCallback(typeof(ReadCallbackDelegate))]
        static void OnReadComplete(IntPtr dataPtr, uint length)
        {
            if (s_Instance != null && s_Instance.m_ReadQueue.Count > 0)
            {
                var tcs = s_Instance.m_ReadQueue.Dequeue();
                try
                {
                    if (dataPtr != IntPtr.Zero && length > 0)
                    {
                        // Copy data from unmanaged memory
                        byte[] data = new byte[length];
                        Marshal.Copy(dataPtr, data, 0, (int)length);
                        tcs.TrySetResult(data);
                    }
                    else
                    {
                        tcs.TrySetResult(Array.Empty<byte>());
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    tcs.TrySetResult(Array.Empty<byte>());
                }
                finally
                {
                    // Free the memory allocated by JavaScript
                    if (dataPtr != IntPtr.Zero)
                        WebGL_FreeClipboardData(dataPtr);
                }
            }
        }

        [MonoPInvokeCallback(typeof(CheckAccessCallbackDelegate))]
        static void OnCheckAccessComplete(int hasAccess)
        {
            if (s_Instance != null && s_Instance.m_CheckAccessQueue.Count > 0)
            {
                var tcs = s_Instance.m_CheckAccessQueue.Dequeue();
                tcs.TrySetResult(hasAccess == 1);
            }
        }

        [MonoPInvokeCallback(typeof(CheckHasDataCallbackDelegate))]
        static void OnCheckHasDataComplete(int hasData)
        {
            if (s_Instance != null && s_Instance.m_CheckHasDataQueue.Count > 0)
            {
                var tcs = s_Instance.m_CheckHasDataQueue.Dequeue();
                tcs.TrySetResult(hasData == 1);
            }
        }

        [MonoPInvokeCallback(typeof(CheckHasDataCallbackDelegate))]
        static void OnCheckHasImageDataComplete(int hasData)
        {
            if (s_Instance != null && s_Instance.m_CheckHasImageDataQueue.Count > 0)
            {
                var tcs = s_Instance.m_CheckHasImageDataQueue.Dequeue();
                tcs.TrySetResult(hasData == 1);
            }
        }

        [MonoPInvokeCallback(typeof(ReadCallbackDelegate))]
        static void OnReadImageComplete(IntPtr dataPtr, uint length)
        {
            if (s_Instance != null && s_Instance.m_ReadImageQueue.Count > 0)
            {
                var tcs = s_Instance.m_ReadImageQueue.Dequeue();
                try
                {
                    if (dataPtr != IntPtr.Zero && length > 0)
                    {
                        byte[] data = new byte[length];
                        Marshal.Copy(dataPtr, data, 0, (int)length);
                        tcs.TrySetResult(data);
                    }
                    else
                    {
                        tcs.TrySetResult(Array.Empty<byte>());
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    tcs.TrySetResult(Array.Empty<byte>());
                }
                finally
                {
                    if (dataPtr != IntPtr.Zero)
                        WebGL_FreeClipboardData(dataPtr);
                }
            }
        }

        protected override void PollDarkMode() { /* No-op, handled via callback */ }

        protected override void PollScaleFactor() { /* No-op, handled via callback */ }

        public override bool darkMode => WebGL_IsDarkPreferredColorScheme() == 1;

        public override float scaleFactor => WebGL_GetDevicePixelRatio();

        public override float referenceDpi
        {
            get
            {
                // On WebGL we can use the base value of 96dpi because UI Toolkit scales correctly the UI based on
                // Web Browser's Device Pixel Ratio changes.
                return Platform.baseDpi;
            }
        }

        public override bool HasPasteboardData(PasteboardType type)
        {
            // Return cached result or false if no cache
            if (type == PasteboardType.Text && s_CachedClipboardData != null)
                return s_CachedClipboardData.Length > 0;

            return false;
        }

        public override byte[] GetPasteboardData(PasteboardType type)
        {
            if (type == PasteboardType.Text && s_CachedClipboardData != null)
                return s_CachedClipboardData;

            return Array.Empty<byte>();
        }

        public override void SetPasteboardData(PasteboardType type, byte[] data)
        {
            if (type == PasteboardType.Text && data != null && data.Length > 0)
            {
                // Cache the data
                s_CachedClipboardData = data;

                // Try to copy to system clipboard asynchronously (fire and forget)
                try
                {
                    // Create a completion source and enqueue it (will be dequeued by callback)
                    var tcs = new TaskCompletionSource<bool>();
                    m_CopyQueue.Enqueue(tcs);
                    string text = Encoding.UTF8.GetString(data);
                    WebGL_PrepareCopyTextToClipboard(text, s_CopyCallbackPtr);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to set pasteboard data: {ex.Message}");
                }
            }
        }

        public override Task<bool> HasPasteboardAccessAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                m_CheckAccessQueue.Enqueue(tcs);
                WebGL_CheckClipboardAccess(s_CheckAccessCallbackPtr);
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to check clipboard access: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public override Task<bool> HasPasteboardDataAsync(PasteboardType type)
        {
            var tcs = new TaskCompletionSource<bool>();
            try
            {
                if (type == PasteboardType.Text)
                {
                    m_CheckHasDataQueue.Enqueue(tcs);
                    WebGL_CheckClipboardHasTextData(s_CheckHasDataCallbackPtr);
                }
                else if (type == PasteboardType.PNG)
                {
                    m_CheckHasImageDataQueue.Enqueue(tcs);
                    WebGL_CheckClipboardHasImageData(s_CheckHasImageDataCallbackPtr);
                }
                else
                {
                    return Task.FromResult(false);
                }
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to check clipboard data: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public override Task<byte[]> GetPasteboardDataAsync(PasteboardType type)
        {
            var tcs = new TaskCompletionSource<byte[]>();
            try
            {
                if (type == PasteboardType.Text)
                {
                    m_ReadQueue.Enqueue(tcs);
                    WebGL_PrepareReadTextFromClipboard(s_ReadCallbackPtr);
                }
                else if (type == PasteboardType.PNG)
                {
                    m_ReadImageQueue.Enqueue(tcs);
                    WebGL_ReadImageFromClipboard(s_ReadImageCallbackPtr);
                }
                else
                {
                    return Task.FromResult(Array.Empty<byte>());
                }
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read pasteboard data: {ex.Message}");
                return Task.FromResult(Array.Empty<byte>());
            }
        }

        public override Task SetPasteboardDataAsync(PasteboardType type, byte[] data)
        {
            if (type != PasteboardType.Text || data == null || data.Length == 0)
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();
            try
            {
                // Cache the data
                s_CachedClipboardData = data;

                m_CopyQueue.Enqueue(tcs);
                string text = Encoding.UTF8.GetString(data);
                WebGL_PrepareCopyTextToClipboard(text, s_CopyCallbackPtr);
                return tcs.Task;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to set pasteboard data: {ex.Message}");
                return Task.CompletedTask;
            }
        }

        static string GetMimeTypeString(PasteboardType type)
        {
            return type switch
            {
                PasteboardType.Text => "text/plain",
                PasteboardType.PNG => "image/png",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
    }
}
#endif
