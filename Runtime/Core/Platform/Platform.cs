using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Object = UnityEngine.Object;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete

namespace Unity.AppUI.Core
{
    /// <summary>
    /// The type of haptic feedback to trigger.
    /// </summary>
    public enum HapticFeedbackType
    {
        /// <summary>
        /// No haptic feedback will be triggered with this value.
        /// </summary>
        UNDEFINED = 0,
        /// <summary>
        /// A light haptic feedback.
        /// </summary>
        LIGHT = 1,
        /// <summary>
        /// A medium haptic feedback.
        /// </summary>
        MEDIUM,
        /// <summary>
        /// A heavy haptic feedback.
        /// </summary>
        HEAVY,
        /// <summary>
        /// A success haptic feedback.
        /// </summary>
        SUCCESS,
        /// <summary>
        /// An error haptic feedback.
        /// </summary>
        ERROR,
        /// <summary>
        /// A warning haptic feedback.
        /// </summary>
        WARNING,
        /// <summary>
        /// A selection haptic feedback.
        /// </summary>
        SELECTION
    }

    /// <summary>
    /// A touch event received from a magic trackpad.
    /// </summary>
    /// <remarks>
    /// Theses Touch events can be received from a magic trackpad on macOS.
    /// </remarks>
    public struct TrackPadTouch
    {
        /// <summary>
        /// The unique identifier of the touch.
        /// </summary>
        public int fingerId { get; }

        /// <summary>
        /// The position of the touch in normalized coordinates.
        /// </summary>
        public Vector2 position { get; }

        /// <summary>
        /// The number of taps. This is always 1 for a trackpad.
        /// </summary>
        public int tapCount { get; }

        /// <summary>
        /// The delta position of the touch since the last frame.
        /// </summary>
        public Vector2 deltaPos { get; }

        /// <summary>
        /// The delta time since the last frame.
        /// </summary>
        public float deltaTime { get; }

        /// <summary>
        /// The phase of the touch.
        /// </summary>
        public TouchPhase phase { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fingerId"> The unique identifier of the touch.</param>
        /// <param name="position"> The position of the touch in normalized coordinates.</param>
        /// <param name="tapCount"> The number of taps. This is always 1 for a trackpad.</param>
        /// <param name="deltaPos"> The delta position of the touch since the last frame.</param>
        /// <param name="deltaTime"> The delta time since the last frame.</param>
        /// <param name="phase"> The phase of the touch.</param>
        public TrackPadTouch(int fingerId, Vector2 position, int tapCount, Vector2 deltaPos, float deltaTime,
            TouchPhase phase)
        {
            this.fingerId = fingerId;
            this.position = position;
            this.tapCount = tapCount;
            this.deltaPos = deltaPos;
            this.deltaTime = deltaTime;
            this.phase = phase;
        }
    }

    /// <summary>
    /// A magnification gesture received from a magic trackpad.
    /// </summary>
    public struct MagnificationGesture : IEquatable<MagnificationGesture>
    {
        /// <summary>
        /// The magnification delta of the gesture since the last frame.
        /// </summary>
        public float deltaMagnification { get; }

        /// <summary>
        /// The scroll delta of the gesture since the last frame.
        /// </summary>
        /// <remarks>
        /// This is a convenience property to convert the magnification delta to a scroll delta.
        /// </remarks>
        public Vector2 scrollDelta => new Vector2(0, -deltaMagnification * 50f);

        /// <summary>
        /// The phase of the gesture.
        /// </summary>
        public TouchPhase phase { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="deltaMagnification">The magnification delta of the gesture since the last frame.</param>
        /// <param name="phase">The phase of the gesture.</param>
        public MagnificationGesture(float deltaMagnification, TouchPhase phase)
        {
            this.phase = phase;
            this.deltaMagnification = deltaMagnification;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object.</param>
        /// <returns> True if objects are equal, false otherwise.</returns>
        public bool Equals(MagnificationGesture other)
        {
            return Mathf.Approximately(deltaMagnification, other.deltaMagnification) && phase == other.phase;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object.</param>
        /// <returns> True if the first MagnificationGesture is equal to the second MagnificationGesture, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is MagnificationGesture other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(deltaMagnification, (int) phase);
        }

        /// <summary>
        /// Determines whether two specified MagnificationGesture objects are equal.
        /// </summary>
        /// <param name="left"> The first MagnificationGesture to compare.</param>
        /// <param name="right"> The second MagnificationGesture to compare.</param>
        /// <returns> True if the first MagnificationGesture is equal to the second MagnificationGesture, false otherwise.</returns>
        public static bool operator ==(MagnificationGesture left, MagnificationGesture right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified MagnificationGesture objects are not equal.
        /// </summary>
        /// <param name="left"> The first MagnificationGesture to compare.</param>
        /// <param name="right"> The second MagnificationGesture to compare.</param>
        /// <returns> True if the first MagnificationGesture is not equal to the second MagnificationGesture, false otherwise.</returns>
        public static bool operator !=(MagnificationGesture left, MagnificationGesture right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// A pan gesture received from a magic trackpad.
    /// </summary>
    public struct PanGesture : IEquatable<PanGesture>
    {
        /// <summary>
        /// The delta position of the touch since the last frame.
        /// </summary>
        public Vector2 deltaPos { get; }

        /// <summary>
        /// The phase of the gesture.
        /// </summary>
        public TouchPhase phase { get; }

        /// <summary>
        /// The position of the touch in normalized coordinates.
        /// </summary>
        public Vector2 position { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="position">The position of the touch in normalized coordinates.</param>
        /// <param name="deltaPos">The delta position of the touch since the last frame.</param>
        /// <param name="phase">The phase of the gesture.</param>
        public PanGesture(Vector2 position, Vector2 deltaPos, TouchPhase phase)
        {
            this.position = position;
            this.deltaPos = deltaPos;
            this.phase = phase;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object.</param>
        /// <returns> True if objects are equal, false otherwise.</returns>
        public bool Equals(PanGesture other)
        {
            return deltaPos.Equals(other.deltaPos) && phase == other.phase && position.Equals(other.position);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object.</param>
        /// <returns> True if objects are equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return obj is PanGesture other && Equals(other);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(deltaPos, position, (int) phase);
        }

        /// <summary>
        /// Determines whether two specified PanGesture objects are equal.
        /// </summary>
        /// <param name="left"> The first PanGesture to compare.</param>
        /// <param name="right"> The second PanGesture to compare.</param>
        /// <returns> True if the first PanGesture is equal to the second PanGesture, false otherwise.</returns>
        public static bool operator ==(PanGesture left, PanGesture right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified PanGesture objects are not equal.
        /// </summary>
        /// <param name="left"> The first PanGesture to compare.</param>
        /// <param name="right"> The second PanGesture to compare.</param>
        /// <returns> True if the first PanGesture is not equal to the second PanGesture, false otherwise.</returns>
        public static bool operator !=(PanGesture left, PanGesture right)
        {
            return !left.Equals(right);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PlatformTouchEvent
    {
        public byte touchId;
        public byte phase;
        public float normalizedX;
        public float normalizedY;
        public float deviceWidth;
        public float deviceHeight;
    }

    /// <summary>
    /// Utility methods and properties related to the Target Platform.
    /// </summary>
    public static partial class Platform
    {
#if UNITY_EDITOR
        static Object s_LastEditorWindow;
#endif

        /// <summary>
        /// The base DPI value used in <see cref="UnityEngine.UIElements.PanelSettings"/>.
        /// </summary>
        public const float baseDpi = 96f;

        /// <summary>
        /// <para>
        /// The DPI value that should be used in UI-Toolkit PanelSettings
        /// <see cref="UnityEngine.UIElements.PanelSettings.referenceDpi"/>.
        /// </para>
        /// <para>
        /// This value is the value of <see cref="Screen.dpi"/> divided by the main screen scale factor.
        /// </para>
        /// </summary>
        public static float referenceDpi
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            // On Windows we can use a value of 96dpi because UI Toolkit scales correctly the UI based on
            // Operating System's DPI and ScaleFactor changes.
            return baseDpi;
#else
                return Screen.dpi / mainScreenScale;
#endif
            }
        }

        /// <summary>
        /// The main screen scale factor.
        /// </summary>
        /// <remarks>
        /// The "main" screen is the current screen used at highest priority to display the application window.
        /// </remarks>
        public static float mainScreenScale
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
            return _IOSAppUIScaleFactor();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return _NSAppUIScaleFactor();
#elif UNITY_ANDROID && !UNITY_EDITOR
            // Android ScaledDensity: https://developer.android.com/reference/android/util/DisplayMetrics#scaledDensity
            return AndroidAppUI.scaledDensity;
#else
                // On Windows Screen.dpi is already the result of the dpi multiplied by the scale factor.
                // For example on a 27in 4k monitor at 100% scale, the dpi is 96 (really small UI), but the recommended
                // Scale factor with a 4k monitor is 150%, which gives 96 * 1.5 = 144dpi.
                // Unity Engine sets the DPI awareness per monitor, so the UI will scale automatically :
                // https://docs.microsoft.com/en-us/windows/win32/api/windef/ne-windef-dpi_awareness
                return Screen.dpi / baseDpi;
#endif
            }
        }

        static event Action<string> systemThemeChangedInternal;

        /// <summary>
        /// Event triggered when the system theme changes.
        /// </summary>
        public static event Action<string> systemThemeChanged
        {
            add
            {
                systemThemeChangedInternal += value;
                if (value != null)
                    EnableThemePolling();
            }
            remove
            {
                systemThemeChangedInternal -= value;
                if (systemThemeChangedInternal == null)
                    DisableThemePolling();
            }
        }

        static bool s_SystemThemePollingEnabled = false;

        static void EnableThemePolling()
        {
            s_SystemThemePollingEnabled = true;
        }

        static void DisableThemePolling()
        {
            s_SystemThemePollingEnabled = false;
        }

        static string s_PreviousSystemTheme;

        static int s_ThemePollingDelta = 0;

        /// <summary>
        /// Polls the system theme and triggers the <see cref="systemThemeChanged"/> event if the theme has changed.
        /// </summary>
        internal static void PollSystemTheme()
        {
            if (!s_SystemThemePollingEnabled)
                return;

            s_ThemePollingDelta += Time.frameCount;

            if (s_ThemePollingDelta < 60)
                return;

            s_ThemePollingDelta = 0;
            var currentTheme = systemTheme;
            if (currentTheme != s_PreviousSystemTheme)
            {
                systemThemeChangedInternal?.Invoke(currentTheme);
                s_PreviousSystemTheme = currentTheme;
            }
        }

        static TrackPadTouch s_Touch0;
        static TrackPadTouch s_Touch1;

        static bool s_TwoFingersUsed;

        static Vector2 s_StartPos0;
        static Vector2 s_StartPos1;

        static Vector2 s_DeltaPos0;
        static Vector2 s_DeltaPos1;

        internal enum GestureType
        {
            Unknown,
            Pan,
            Mag
        }

        static GestureType s_Gesture;

        static float s_LastMag;

        static Vector2 s_LastPanPos;

        static float s_StartDistance;

        const float k_RecognitionThreshold = 0.05f;

        const float k_PanRecognitionDotThreshold = 0.9f;

        /// <summary>
        /// Polls the gestures and triggers gesture events if a gesture has been received.
        /// </summary>
        [Obsolete]
        internal static void PollGestures()
        {
            panGestureChangedThisFrame = false;
            magnificationGestureChangedThisFrame = false;

            var touches = GetTrackpadTouches();

            isTouchGestureSupported = isTouchGestureSupported || touches.Count > 0;

            foreach (var touch in touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    // check if we already have another touch
                    if (s_Touch0.fingerId == -1 || s_Touch0.fingerId == touch.fingerId)
                    {
                        s_Touch0 = touch;
                    }
                    else if (s_Touch1.fingerId == -1 || s_Touch1.fingerId == touch.fingerId)
                    {
                        if (touch.fingerId == s_Touch0.fingerId)
                            Debug.LogError("The second finger ID can't be the same as the first one");
                        s_Touch1 = touch;
                        s_TwoFingersUsed = true;
                        s_StartPos0 = s_Touch0.position;
                        s_StartPos1 = s_Touch1.position;
                        s_DeltaPos0 = Vector2.zero;
                        s_DeltaPos1 = Vector2.zero;
                    }

                    if (s_Touch0.fingerId != touch.fingerId && s_Touch1.fingerId != touch.fingerId && s_TwoFingersUsed)
                    {
                        // we can deactivate here since more than 2 fingers are used
                        AbortGesture();
                    }
                }
                else if (touch.phase is TouchPhase.Canceled or TouchPhase.Ended)
                {
                    // check if the touch is one of the tracked ones
                    if (touch.fingerId == s_Touch0.fingerId || touch.fingerId == s_Touch1.fingerId)
                        AbortGesture(touch.phase);
                }

                if (s_TwoFingersUsed && touch.phase is TouchPhase.Moved or TouchPhase.Stationary)
                {
                    // store touch data if its one of the tracked ones
                    if (touch.fingerId == s_Touch0.fingerId)
                    {
                        s_Touch0 = touch;
                    }
                    else if (touch.fingerId == s_Touch1.fingerId)
                    {
                        s_Touch1 = touch;
                    }
                }
            }

            if (s_TwoFingersUsed)
            {
                // compute deltas
                s_DeltaPos0 = s_Touch0.position - s_StartPos0;
                s_DeltaPos1 = s_Touch1.position - s_StartPos1;

                if (s_Gesture == GestureType.Unknown)
                {
                    // need to recognize the movement
                    if (s_DeltaPos0.magnitude > k_RecognitionThreshold && s_DeltaPos1.magnitude > k_RecognitionThreshold)
                    {
                        s_LastMag = 1f;
                        s_StartDistance = Vector2.Distance(s_Touch0.position, s_Touch1.position);
                        s_LastPanPos = s_Touch0.position;

                        // can recognize
                        if (Vector2.Dot(s_DeltaPos0.normalized, s_DeltaPos1.normalized) > k_PanRecognitionDotThreshold)
                        {
                            // pan
                            s_Gesture = GestureType.Pan;
                            panGesture = new PanGesture(s_Touch0.position, Vector2.zero, TouchPhase.Began);
                        }
                        else
                        {
                            // mag
                            s_Gesture = GestureType.Mag;
                            magnificationGesture = new MagnificationGesture(0f, TouchPhase.Began);
                        }
                    }
                }
                else if (s_Gesture == GestureType.Mag)
                {
                    var d = Vector2.Distance(s_Touch0.position, s_Touch1.position);
                    var mag = d / s_StartDistance;
                    var magDelta = mag - s_LastMag;
                    s_LastMag = mag;
                    magnificationGesture = new MagnificationGesture(magDelta, TouchPhase.Moved);
                }
                else if (s_Gesture == GestureType.Pan)
                {
                    var pos = s_Touch0.position;
                    var panDelta = pos - s_LastPanPos;
                    s_LastPanPos = pos;
                    panGesture = new PanGesture(pos, panDelta, TouchPhase.Moved);
                }
            }
        }

        static float s_LastTime;
        static int s_LastFrame;

        static readonly Dictionary<int, TrackPadTouch> k_PrevTouches = new Dictionary<int, TrackPadTouch>();
        static readonly List<TrackPadTouch> k_FrameTouches = new List<TrackPadTouch>();

        [Obsolete]
        static List<TrackPadTouch> GetTrackpadTouches()
        {
            s_LastTime = Time.unscaledTime;
            if ((Application.isPlaying && s_LastFrame != Time.frameCount) || !Application.isPlaying)
            {
                s_LastFrame = Time.frameCount;

                k_PrevTouches.Clear();
                foreach (var touch in k_FrameTouches)
                {
                    k_PrevTouches[touch.fingerId] = touch;
                }

                k_FrameTouches.Clear();

                PlatformTouchEvent e;
                e.touchId = 0;
                e.phase = 0;
                e.normalizedX = 0;
                e.normalizedY = 0;
                e.deviceWidth = 0;
                e.deviceHeight = 0;

#if UNITY_EDITOR
                var currentWindow = UnityEditor.EditorWindow.focusedWindow;
                if (currentWindow != s_LastEditorWindow && currentWindow)
                    SetupFocusedTrackingObject();
                s_LastEditorWindow = currentWindow;

                if (currentWindow)
#endif
                {
                    while (ReadTouch(ref e))
                    {
                        var screenPos = new Vector2(e.normalizedX, e.normalizedY);
                        var deltaPos = new Vector2(0, 0);

                        if (k_PrevTouches.TryGetValue(e.touchId, out var prevTouch))
                            deltaPos = screenPos - prevTouch.position;

                        var timeDelta = Time.unscaledTime - s_LastTime;
                        var phase = e.phase switch
                        {
                            0 => TouchPhase.Began,
                            1 => TouchPhase.Moved,
                            2 => TouchPhase.Ended,
                            3 => TouchPhase.Canceled,
                            4 => TouchPhase.Stationary,
                            _ => TouchPhase.Ended
                        };
                        var newTouch = new TrackPadTouch(e.touchId, screenPos, 1, deltaPos, timeDelta, phase);
                        k_FrameTouches.Add(newTouch);
                    }
                }

                s_LastTime = Time.unscaledTime;
            }

            return k_FrameTouches;
        }

        static bool ReadTouch(ref PlatformTouchEvent touch)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            if (AppUI.settings && AppUI.settings.enableMacOSGestureRecognition)
                return _NSReadTouchEvent(ref touch);
            else
                return false;
#else
            return false;
#endif
        }

        static void SetupFocusedTrackingObject()
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            _NSSetupFocusedTrackingObject();
#endif
        }

        static void AbortGesture(TouchPhase phase = TouchPhase.Canceled)
        {
            s_TwoFingersUsed = false;
            s_Gesture = GestureType.Unknown;
            s_Touch0 = new TrackPadTouch(-1, Vector2.zero, 0, Vector2.zero, 0, TouchPhase.Canceled);
            s_Touch1 = new TrackPadTouch(-1, Vector2.zero, 0, Vector2.zero, 0, TouchPhase.Canceled);
            panGesture = new PanGesture(Vector2.zero, Vector2.zero, phase);
            magnificationGesture = new MagnificationGesture(0f, phase);
        }

        /// <summary>
        /// Event triggered when a pan gesture is received.
        /// </summary>
        [Obsolete]
        public static event Action<PanGesture> panGestureChanged;

        /// <summary>
        /// Event triggered when a magnification gesture is received.
        /// </summary>
        [Obsolete]
        public static event Action<MagnificationGesture> magnificationGestureChanged;

        /// <summary>
        /// Whether the pan gesture has changed this frame.
        /// </summary>
        [Obsolete]
        public static bool panGestureChangedThisFrame { get; set; }

        /// <summary>
        /// Whether the magnification gesture has changed this frame.
        /// </summary>
        [Obsolete]
        public static bool magnificationGestureChangedThisFrame { get; set; }

        /// <summary>
        /// The pan gesture data.
        /// </summary>
        [Obsolete]
        public static PanGesture panGesture
        {
            get => s_PanGesture;
            private set
            {
                if (s_PanGesture != value)
                {
                    s_PanGesture = value;
                    panGestureChangedThisFrame = true;
                    panGestureChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// The magnification gesture data.
        /// </summary>
        [Obsolete]
        public static MagnificationGesture magnificationGesture
        {
            get => s_MagnificationGesture;
            private set
            {
                if (s_MagnificationGesture != value)
                {
                    s_MagnificationGesture = value;
                    magnificationGestureChangedThisFrame = true;
                    magnificationGestureChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Whether the current platform supports touch gestures.
        /// </summary>
        public static bool isTouchGestureSupported { get; set; }

        static PanGesture s_PanGesture;

        static MagnificationGesture s_MagnificationGesture;

        /// <summary>
        /// The current system theme.
        /// </summary>
        public static string systemTheme
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return _IOSCurrentAppearance() == 2 ? "dark" : "light";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                return _NSCurrentAppearance() == 2 ? "dark" : "light";
#elif UNITY_ANDROID && !UNITY_EDITOR
                return AndroidAppUI.isNightModeDefined && AndroidAppUI.isNightModeEnabled ? "dark" : "light";
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                return _WINUseLightTheme == 1 ? "light" : "dark";
#else
                return "dark";
#endif
            }
        }

        /// <summary>
        /// Run a haptic feedback on the current platform.
        /// </summary>
        /// <param name="feedbackType">The type of haptic feedback to trigger.</param>
        public static void RunHapticFeedback(HapticFeedbackType feedbackType)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _IOSRunHapticFeedback((int)feedbackType);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AndroidAppUI.RunHapticFeedback(feedbackType);
#else
            if (UnityEngine.Application.isEditor)
                Debug.LogWarning("Haptic Feedbacks are not supported in the Editor.");
            else
                Debug.LogWarning("Haptic Feedbacks are not supported on the current platform.");
#endif
        }

        static float s_PreviousScaleFactor = 1f;
        internal static event Action<float> scaleFactorChanged;

        internal static void Update()
        {
            PollSystemTheme();
            var newScaleFactor = mainScreenScale;
            if (!Mathf.Approximately(newScaleFactor, s_PreviousScaleFactor) && !Mathf.Approximately(newScaleFactor, 0f))
            {
                s_PreviousScaleFactor = newScaleFactor;
                scaleFactorChanged?.Invoke(newScaleFactor);
            }
            ReadNativeTouches();
        }

        static float m_PollEventTime;

        const int k_MaxTouches = 512;

        static readonly AppUITouch[] k_FrameAppUITouches = new AppUITouch[k_MaxTouches];

        static int m_FrameTouchCount;

        static readonly AppUITouch[] k_PrevAppUITouches = new AppUITouch[k_MaxTouches];

        static int m_PrevTouchCount;

        static int m_LastUpdateFrame;

        static PlatformTouchEvent m_NativeTouch;


        static void ReadNativeTouches()
        {
            // be sure to only read touches once per frame
            if ((Application.isPlaying && m_LastUpdateFrame != Time.frameCount) || !Application.isPlaying)
            {
                m_LastUpdateFrame = Time.frameCount;

                for (var i = 0; i < k_MaxTouches; i++)
                {
                    if (i == m_FrameTouchCount)
                        break;
                    k_PrevAppUITouches[i] = k_FrameAppUITouches[i];
                }

                m_PrevTouchCount = m_FrameTouchCount;
                m_FrameTouchCount = 0;

                for (var i = 0; i < k_MaxTouches; i++)
                {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    m_NativeTouch = default;
                    if (!_NSReadTouchEvent(ref m_NativeTouch))
                        break;

                    ref var touch = ref k_FrameAppUITouches[m_FrameTouchCount++];
                    touch.fingerId = m_NativeTouch.touchId;
                    touch.position = new Vector2(m_NativeTouch.normalizedX * m_NativeTouch.deviceWidth,
                        m_NativeTouch.normalizedY * m_NativeTouch.deviceHeight);
                    touch.phase = ConvertTouchPhase(m_NativeTouch.phase);

                    for (var j = m_PrevTouchCount - 1; j >= 0; j--)
                    {
                        if (k_PrevAppUITouches[i].fingerId == m_NativeTouch.touchId)
                        {
                            touch.deltaPos = touch.position - k_PrevAppUITouches[i].position;
                            touch.deltaTime = Time.unscaledTime - m_PollEventTime;
                            break;
                        }
                    }
#endif
                }

                m_PollEventTime = Time.unscaledTime;
            }
        }

        internal static ReadOnlySpan<AppUITouch> touches => k_FrameAppUITouches.AsSpan(0, m_FrameTouchCount);
    }
}
