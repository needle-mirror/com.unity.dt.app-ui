#if (UNITY_ANDROID && !UNITY_EDITOR)
using UnityEngine;
using System;

namespace Unity.AppUI.Core
{
    class AndroidPlatformImpl : PlatformImpl, IDisposable
    {
        readonly NativeConfigListener k_Listener;
        readonly AndroidJavaObject k_CurrentActivity;
        readonly AndroidJavaObject k_Vibrator;

        // ReSharper disable once NotAccessedField.Local
        float m_Density;
        // ReSharper disable once NotAccessedField.Local
        int m_DensityDPI;

        float m_ScaledDensity;
        float m_FontScale;
        bool m_IsNightModeEnabled;
        bool m_IsNightModeDefined;
        bool m_HighContrast;
        bool m_ReduceMotion;
        int m_LayoutDirection;

        class NativeConfigListener : AndroidJavaProxy, IDisposable
        {
            readonly Action m_OnChanged;

            Handler m_Handler;

            Handler handler
            {
                get
                {
                    if (m_Handler == null)
                    {
                        m_Handler = new Handler(AppUI.mainLooper, message =>
                        {
                            switch (message.what)
                            {
                                case k_ActionConfigChanged:
                                    Debug.Log("Handler received configuration change message, invoking callback.");
                                    ((NativeConfigListener)message.obj).m_OnChanged.Invoke();
                                    return true;
                                default:
                                    return false;
                            }
                        });
                    }

                    return m_Handler;
                }
            }

            public NativeConfigListener(Action onChanged)
                : base("android.content.ComponentCallbacks2")
            {
                m_OnChanged = onChanged;
            }

            /// <summary>
            /// Called by the Android system when the device configuration changes (e.g., orientation, screen size, locale).
            /// </summary>
            /// <param name="newConfig"> The new configuration object provided by the Android system.</param>
            // ReSharper disable once InconsistentNaming
            public void onConfigurationChanged(AndroidJavaObject newConfig) {
                handler.SendMessage(handler.ObtainMessage(k_ActionConfigChanged, this));
            }

            public void Dispose()
            {
                handler.RemoveCallbacksAndMessages(this);
                m_Handler = null;
            }

            const int k_ActionConfigChanged = 123;

            /// <summary>
            /// No-op
            /// </summary>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once InconsistentNaming
            public void onLowMemory()
            {
                // no-op
            }

            /// <summary>
            /// No-op
            /// </summary>
            /// <param name="level"> The memory trim level. </param>
            // ReSharper disable once UnusedMember.Local
            // ReSharper disable once InconsistentNaming
            public void onTrimMemory(int level)
            {
                // no-op
            }
        }

        void QueryConfiguration()
        {
            using var resources = k_CurrentActivity.Call<AndroidJavaObject>("getResources");
            using var config = resources.Call<AndroidJavaObject>("getConfiguration");
            using var metrics = resources.Call<AndroidJavaObject>("getDisplayMetrics");

            // Metrics & Scales
            m_Density = metrics.Get<float>("density");
            var newScaledDensity = metrics.Get<float>("scaledDensity");
            if (!Mathf.Approximately(newScaledDensity, m_ScaledDensity))
            {
                m_ScaledDensity = newScaledDensity;
                InvokeScaleFactorChanged(scaleFactor);
            }
            m_DensityDPI = metrics.Get<int>("densityDpi");
            var newFontScale = config.Get<float>("fontScale");
            if (!Mathf.Approximately(newFontScale, m_FontScale))
            {
                m_FontScale = newFontScale;
                InvokeTextScaleFactorChanged(textScaleFactor);
            }

            // Night Mode
            var uiMode = config.Get<int>("uiMode");
            const int nightMask = 0x30; // Configuration.UI_MODE_NIGHT_MASK
            const int nightYes = 0x20;  // Configuration.UI_MODE_NIGHT_YES
            const int nightUndefined = 0x00;

            var currentNightMode = uiMode & nightMask;
            var isNightModeDefined = currentNightMode != nightUndefined;
            var isNightModeEnabled = currentNightMode == nightYes;
            if (isNightModeDefined != m_IsNightModeDefined || isNightModeEnabled != m_IsNightModeEnabled)
            {
                m_IsNightModeDefined = isNightModeDefined;
                m_IsNightModeEnabled = isNightModeEnabled;
                InvokeThemeChanged(darkMode);
            }

            // Layout Direction (0 = LTR, 1 = RTL)
            var newLayoutDirection = config.Call<int>("getLayoutDirection");
            if (newLayoutDirection != m_LayoutDirection)
            {
                m_LayoutDirection = newLayoutDirection;
                InvokeLayoutDirectionChanged(layoutDirection);
            }

            // Accessibility: High Contrast
            var newHighContrast = GetHighContrastStatus();
            if (newHighContrast != m_HighContrast)
            {
                m_HighContrast = newHighContrast;
                InvokeHighContrastChanged(highContrast);
            }

            // Accessibility: Reduce Motion (Animations)
            var newReduceMotion = GetReduceMotionStatus();
            if (newReduceMotion != m_ReduceMotion)
            {
                m_ReduceMotion = newReduceMotion;
                InvokeReduceMotionChanged(reduceMotion);
            }
        }

        bool GetHighContrastStatus()
        {
            try {
                using var am = k_CurrentActivity.Call<AndroidJavaObject>("getSystemService", "accessibility");
                return am.Call<bool>("isHighTextContrastEnabled");
            } catch { return false; }
        }

        bool GetReduceMotionStatus()
        {
            try {
                using var resolver = k_CurrentActivity.Call<AndroidJavaObject>("getContentResolver");
                using var settingsGlobal = new AndroidJavaClass("android.provider.Settings$Global");
                var transition = settingsGlobal.CallStatic<float>("getFloat", resolver, "transition_animation_scale", 1.0f);
                return transition == 0f;
            } catch { return false; }
        }

        void Vibrate(long ms, int amplitude)
        {
            using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
            if (buildVersion.GetStatic<int>("SDK_INT") >= 26)
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude);
                k_Vibrator.Call("vibrate", effect);
            }
            else
            {
                k_Vibrator.Call("vibrate", ms);
            }
        }

        void Vibrate(long[] pattern, int repeat)
        {
            using var buildVersion = new AndroidJavaClass("android.os.Build$VERSION");
            if (buildVersion.GetStatic<int>("SDK_INT") >= 26)
            {
                using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                using var effect = effectClass.CallStatic<AndroidJavaObject>("createWaveform", pattern, repeat);
                k_Vibrator.Call("vibrate", effect);
            }
            else
            {
                k_Vibrator.Call("vibrate", pattern, repeat);
            }
        }

        public AndroidPlatformImpl()
        {
            using var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            k_CurrentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            k_Vibrator = k_CurrentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

            QueryConfiguration();

            k_Listener = new NativeConfigListener(QueryConfiguration);
            k_CurrentActivity.Call("registerComponentCallbacks", k_Listener);
        }

        public void Dispose()
        {
            // "unregisterComponentCallbacks" is not guaranteed to work because an app can be just killed.
            // Since this implementation instance lives for the whole app lifecycle, this should not be an issue.
            k_Listener?.Dispose();
            k_Vibrator?.Dispose();
            k_CurrentActivity?.Dispose();
        }

        public override float referenceDpi => Screen.dpi / scaleFactor;
        public override float scaleFactor => m_ScaledDensity;
        public override bool darkMode => m_IsNightModeDefined && m_IsNightModeEnabled;
        public override int layoutDirection => m_LayoutDirection;
        public override float textScaleFactor => m_FontScale;
        public override bool highContrast => m_HighContrast;
        public override bool isHapticFeedbackSupported => true;
        public override bool reduceMotion => m_ReduceMotion;
        public override ReadOnlySpan<AppUITouch> touches => AppUIInput.GetCurrentInputSystemTouches();

        protected override void HighFrequencyUpdate() { }
        protected override void LowFrequencyUpdate() { }

        public override void RunNativeHapticFeedback(HapticFeedbackType feedbackType)
        {
            if (k_Vibrator == null) return;

            // Simple mapping of your Java logic to C#
            switch (feedbackType)
            {
                case HapticFeedbackType.LIGHT:     Vibrate(8, 1); break;
                case HapticFeedbackType.MEDIUM:    Vibrate(25, -1); break; // -1 is DEFAULT_AMPLITUDE
                case HapticFeedbackType.HEAVY:     Vibrate(50, 255); break;
                case HapticFeedbackType.SUCCESS:   Vibrate(new long[] { 0, 12, 120, 16 }, -1); break;
                case HapticFeedbackType.ERROR:     Vibrate(new long[] { 0, 6, 120, 12, 120, 12 }, -1); break;
                case HapticFeedbackType.WARNING:   Vibrate(new long[] { 0, 80, 100, 40 }, -1); break;
                case HapticFeedbackType.SELECTION: Vibrate(2, -1); break;
                case HapticFeedbackType.UNDEFINED:
                default:
                    break;
            }
        }

        public override Color GetSystemColor(SystemColorType colorType) => Color.clear;
    }
}
#endif
