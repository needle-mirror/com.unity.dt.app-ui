using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// A <see cref="MonoBehaviour"/> which is responsible for updating the AppUI system every frame.
    /// </summary>
    /// <remarks>
    /// A single instance of this class should be present.
    /// </remarks>
    [DisallowMultipleComponent]
    public class AppUIManagerBehaviour : MonoBehaviour
    {
        void Update()
        {
            if (!Application.isEditor)
            {
                AppUI.EnsureInitialized();
                AppUI.Update();
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void OnAndroidNativeMessageReceived(string message)
        {
            Platform.HandleAndroidMessage(message);
        }
#endif
    }
}
