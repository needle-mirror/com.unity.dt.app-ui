#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace Unity.AppUI.Core
{
    class OSXPlatformImpl : PlatformImpl
    {
        static OSXPlatformImpl s_Instance;

        delegate void DebugLogDelegate(IntPtr messagePtr, uint len);
        delegate void LayoutDirectionChangedDelegate(byte layoutDirection);
        delegate void HighContrastChangedDelegate(bool highContrastEnabled);
        delegate void ReduceMotionChangedDelegate(bool reduceMotionEnabled);
        delegate void ThemeChangedDelegate(bool darkModeEnabled);
        delegate void TextScaleFactorChangedDelegate(float textScaleFactor);
        delegate void ScaleFactorChangedDelegate(float scaleFactor);
        delegate void MagnifyGestureEventDelegate(float magnification, TouchPhase phase);
        delegate void RotateGestureEventDelegate(float rotation, TouchPhase phase);
        delegate void SmartMagnifyEventDelegate();

        [StructLayout(LayoutKind.Sequential)]
        struct NativeTouch
        {
            public byte fingerId;
            public float positionX;
            public float positionY;
            public float deviceWidth;
            public float deviceHeight;
            public double timestamp;
            public TouchPhase phase;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PluginConfigData
        {
            public bool isEditor;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public DebugLogDelegate DebugLogCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public LayoutDirectionChangedDelegate LayoutDirectionChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public HighContrastChangedDelegate HighContrastChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ReduceMotionChangedDelegate ReduceMotionChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ThemeChangedDelegate ThemeChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public TextScaleFactorChangedDelegate TextScaleFactorChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public ScaleFactorChangedDelegate ScaleFactorChangedCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public MagnifyGestureEventDelegate MagnifyGestureEventCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public RotateGestureEventDelegate RotateGestureEventCSharpHandler;
            [MarshalAs(UnmanagedType.FunctionPtr)]
            public SmartMagnifyEventDelegate SmartMagnifyEventCSharpHandler;
        }

        [MonoPInvokeCallback(typeof(DebugLogDelegate))]
        static void DebugLogProxy(IntPtr message, uint len)
        {
            var messageStr = Marshal.PtrToStringAnsi(message, (int)len);
            Debug.Log(messageStr);
        }

        [MonoPInvokeCallback(typeof(LayoutDirectionChangedDelegate))]
        static void InvokeLayoutDirectionChangedProxy(byte layoutDirection) =>
            s_Instance?.InvokeLayoutDirectionChanged(layoutDirection);

        [MonoPInvokeCallback(typeof(HighContrastChangedDelegate))]
        static void InvokeHighContrastChangedProxy(bool highContrastEnabled) =>
            s_Instance?.InvokeHighContrastChanged(highContrastEnabled);

        [MonoPInvokeCallback(typeof(ReduceMotionChangedDelegate))]
        static void InvokeReduceMotionChangedProxy(bool reduceMotionEnabled) =>
            s_Instance?.InvokeReduceMotionChanged(reduceMotionEnabled);

        [MonoPInvokeCallback(typeof(ThemeChangedDelegate))]
        static void InvokeThemeChangedProxy(bool darkModeEnabled) =>
            s_Instance?.InvokeThemeChanged(darkModeEnabled);

        [MonoPInvokeCallback(typeof(TextScaleFactorChangedDelegate))]
        static void InvokeTextScaleFactorChangedProxy(float textScaleFactor) =>
            s_Instance?.InvokeTextScaleFactorChanged(textScaleFactor);

        [MonoPInvokeCallback(typeof(ScaleFactorChangedDelegate))]
        static void InvokeScaleFactorChangedProxy(float scaleFactor) =>
            s_Instance?.InvokeScaleFactorChanged(scaleFactor);

        [MonoPInvokeCallback(typeof(MagnifyGestureEventDelegate))]
        static void InvokeMagnifyGestureEventProxy(float magnification, TouchPhase phase) { }

        [MonoPInvokeCallback(typeof(RotateGestureEventDelegate))]
        static void InvokeRotateGestureEventProxy(float rotation, TouchPhase phase) { }

        [MonoPInvokeCallback(typeof(SmartMagnifyEventDelegate))]
        static void InvokeSmartMagnifyEventProxy() { }

        [DllImport("AppUINativePlugin")]
        static extern bool NativeAppUI_Initialize(ref PluginConfigData configData);

        [DllImport("AppUINativePlugin")]
        static extern void NativeAppUI_Uninitialize();

        [DllImport("AppUINativePlugin")]
        static extern bool NativeAppUI_ReadTouch(ref NativeTouch touch);

        [DllImport("AppUINativePlugin")]
        static extern float NativeAppUI_ScaleFactor();

        [DllImport("AppUINativePlugin")]
        static extern bool NativeAppUI_DarkMode();

        [DllImport("AppUINativePlugin")]
        static extern bool NativeAppUI_HighContrast();

        [DllImport("AppUINativePlugin")]
        static extern bool NativeAppUI_ReduceMotion();

        [DllImport("AppUINativePlugin")]
        static extern float NativeAppUI_TextScaleFactor();

        [DllImport("AppUINativePlugin")]
        static extern int NativeAppUI_LayoutDirection();

        [DllImport("AppUINativePlugin")]
        static extern Color NativeAppUI_GetSystemColor(SystemColorType colorType);

        [DllImport("AppUINativePlugin")]
        static extern UIntPtr NativeAppUI_GetPasteBoardDataLength(PasteboardType type);

        [DllImport("AppUINativePlugin")]
        static extern void NativeAppUI_GetPasteBoardData(PasteboardType type, UIntPtr size, IntPtr data);

        [DllImport("AppUINativePlugin")]
        static extern void NativeAppUI_SetPasteBoardData(PasteboardType type, UIntPtr size, IntPtr data);

        float m_PollEventTime;

        readonly List<AppUITouch> k_FrameTouches = new List<AppUITouch>();

        readonly Dictionary<int, AppUITouch> k_PrevTouches = new Dictionary<int, AppUITouch>();

        AppUITouch[] m_AppUITouches = Array.Empty<AppUITouch>();

        int m_LastUpdateFrame;

        PluginConfigData m_ConfigData;

        public OSXPlatformImpl() => Setup();

        void Setup()
        {
            CleanUp();

            m_ConfigData = new PluginConfigData
            {
                isEditor = Application.isEditor,
                DebugLogCSharpHandler = DebugLogProxy,
                LayoutDirectionChangedCSharpHandler = InvokeLayoutDirectionChangedProxy,
                HighContrastChangedCSharpHandler = InvokeHighContrastChangedProxy,
                ReduceMotionChangedCSharpHandler = InvokeReduceMotionChangedProxy,
                ThemeChangedCSharpHandler = InvokeThemeChangedProxy,
                TextScaleFactorChangedCSharpHandler = InvokeTextScaleFactorChangedProxy,
                ScaleFactorChangedCSharpHandler = InvokeScaleFactorChangedProxy,
                MagnifyGestureEventCSharpHandler = InvokeMagnifyGestureEventProxy,
                RotateGestureEventCSharpHandler = InvokeRotateGestureEventProxy,
                SmartMagnifyEventCSharpHandler = InvokeSmartMagnifyEventProxy,
            };
            NativeAppUI_Initialize(ref m_ConfigData);
            s_Instance = this;
        }

        ~OSXPlatformImpl() => CleanUp();

        void CleanUp()
        {
            if (s_Instance == null)
                return;
            try
            {
                NativeAppUI_Uninitialize();
            }
            catch (Exception)
            {
                // ignored
            }
            s_Instance = null;
        }

        public override float referenceDpi => Screen.dpi / scaleFactor;

        public override float scaleFactor => NativeAppUI_ScaleFactor();

        public override bool darkMode => NativeAppUI_DarkMode();

        public override bool highContrast => NativeAppUI_HighContrast();

        public override bool reduceMotion => NativeAppUI_ReduceMotion();

        public override float textScaleFactor => NativeAppUI_TextScaleFactor();

        public override int layoutDirection => NativeAppUI_LayoutDirection();

        public override Color GetSystemColor(SystemColorType colorType) => NativeAppUI_GetSystemColor(colorType);

        public override bool HasPasteboardData(PasteboardType type) => NativeAppUI_GetPasteBoardDataLength(type).ToUInt64() > 0;

        public override byte[] GetPasteboardData(PasteboardType type)
        {
            var length = NativeAppUI_GetPasteBoardDataLength(type);
            var size = length.ToUInt64();
            if (size <= 0)
                return Array.Empty<byte>();

            var dataBuffer = new byte[size];
            var handle = GCHandle.Alloc(dataBuffer, GCHandleType.Pinned);
            try
            {
                var dataPtr = handle.AddrOfPinnedObject();
                NativeAppUI_GetPasteBoardData(type, length, dataPtr);
            }
            finally
            {
                handle.Free();
            }

            return dataBuffer;
        }

        public override void SetPasteboardData(PasteboardType type, byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            var size = new UIntPtr((ulong)data.Length);
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataPtr = handle.AddrOfPinnedObject();
                NativeAppUI_SetPasteBoardData(type, size, dataPtr);
            }
            finally
            {
                handle.Free();
            }
        }

        protected override void HighFrequencyUpdate()
        {
            ReadNativeTouches();
        }

        void ReadNativeTouches()
        {
            // be sure to only read touches once per frame
            if ((Application.isPlaying && m_LastUpdateFrame != Time.frameCount) || !Application.isPlaying)
            {
                m_LastUpdateFrame = Time.frameCount;

                k_PrevTouches.Clear();
                foreach (var touch in k_FrameTouches)
                {
                    k_PrevTouches[touch.fingerId] = touch;
                }

                k_FrameTouches.Clear();

                NativeTouch nativeTouch;
                nativeTouch.fingerId = 0;
                nativeTouch.positionX = 0;
                nativeTouch.positionY = 0;
                nativeTouch.deviceWidth = 0;
                nativeTouch.deviceHeight = 0;
                nativeTouch.timestamp = 0;
                nativeTouch.phase = 0;

                while (NativeAppUI_ReadTouch(ref nativeTouch))
                {
                    var touch = new AppUITouch
                    {
                        fingerId = nativeTouch.fingerId,
                        position = new Vector2(nativeTouch.positionX * nativeTouch.deviceWidth,
                            nativeTouch.positionY * nativeTouch.deviceHeight),
                        phase = nativeTouch.phase
                    };

                    if (k_PrevTouches.TryGetValue(nativeTouch.fingerId, out var prev))
                    {
                        touch.deltaPos = touch.position - prev.position;
                        touch.deltaTime = Time.unscaledTime - m_PollEventTime;
                    }

                    k_FrameTouches.Add(touch);
                }

                m_AppUITouches = k_FrameTouches.ToArray();
                m_PollEventTime = Time.unscaledTime;
            }
        }

        public override AppUITouch[] touches => m_AppUITouches;

        protected override void LowFrequencyUpdate()
        {
            // nothing to do
        }
    }
}
#endif
