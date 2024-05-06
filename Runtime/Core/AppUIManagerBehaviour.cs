using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// A <see cref="MonoBehaviour"/> which is responsible for updating the AppUI system every frame.
    /// <remarks>
    /// A single instance of this class should be present.
    /// </remarks>
    /// </summary>
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
        
        void OnNativeMessageReceived(string message)
        {
            Platform.HandleNativeMessage(message);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!Application.isEditor)
            {
                AppUI.EnsureInitialized();
                AppUI.OnApplicationFocus(hasFocus);
            }
        }
    }
}
