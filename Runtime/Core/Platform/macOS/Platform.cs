#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.AppUI.Core
{
    public static partial class Platform
    {
        [DllImport("AppUINativePlugin")]
        static extern float _NSAppUIScaleFactor();

        [DllImport("AppUINativePlugin")]
        static extern int _NSCurrentAppearance();

        [DllImport("AppUINativePlugin")]
        static extern bool _NSReadTouchEvent(ref PlatformTouchEvent e);

        [DllImport("AppUINativePlugin")]
        static extern void _NSSetupFocusedTrackingObject();
    }
}
#endif
