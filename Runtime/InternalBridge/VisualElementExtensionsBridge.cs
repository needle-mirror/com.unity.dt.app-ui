using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Bridge
{
#if APPUI_USE_INTERNAL_API_BRIDGE
    [Flags]
    enum PseudoStates
    {
        Active    = UnityEngine.UIElements.PseudoStates.Active,
        Hover     = UnityEngine.UIElements.PseudoStates.Hover,
        Checked   = UnityEngine.UIElements.PseudoStates.Checked,
        Disabled  = UnityEngine.UIElements.PseudoStates.Disabled,
        Focus     = UnityEngine.UIElements.PseudoStates.Focus,
        Root      = UnityEngine.UIElements.PseudoStates.Root,
    }
#else // REFLECTION
    [Flags]
    enum PseudoStates
    {
        Active    = 1,
        Hover     = 2,
        Checked   = 8,
        Disabled  = 32,
        Focus     = 64,
        Root      = 128,
    }
#endif
    
    static class VisualElementExtensionsBridge
    {
#if APPUI_USE_INTERNAL_API_BRIDGE

        internal static void SetPseudoStates(this VisualElement element, PseudoStates pseudoStates)
        {
            element.pseudoStates = (UnityEngine.UIElements.PseudoStates)pseudoStates;
        }

        internal static PseudoStates GetPseudoStates(this VisualElement element)
        {
            return (PseudoStates)element.pseudoStates;
        }
            
        internal static Rect GetWorldBoundingBox(this VisualElement element)
        {
            return element.worldBoundingBox;
        }
        
        internal static void SetIsCompositeRoot(this VisualElement element, bool isCompositeRoot)
        {
            element.isCompositeRoot = isCompositeRoot;
        }

        internal static bool GetIsCompositeRoot(this VisualElement element)
        {
            return element.isCompositeRoot;
        }

        internal static void SetExcludeFromFocusRing(this VisualElement element, bool excludeFromFocusRing)
        {
            element.excludeFromFocusRing = excludeFromFocusRing;
        }

        internal static bool GetExcludeFromFocusRing(this VisualElement element)
        {
            return element.excludeFromFocusRing;
        }

#else // REFLECTION
        
        static readonly System.Reflection.PropertyInfo k_PseudoStates = 
            typeof(VisualElement).GetProperty("pseudoStates", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        static readonly System.Reflection.PropertyInfo k_IsCompositeRoot = 
            typeof(VisualElement).GetProperty("isCompositeRoot", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        static readonly System.Reflection.PropertyInfo k_ExcludeFromFocusRing =
            typeof(VisualElement).GetProperty("excludeFromFocusRing", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        static readonly System.Reflection.PropertyInfo k_WorldBoundingBox =
            typeof(VisualElement).GetProperty("worldBoundingBox", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        internal static void SetPseudoStates(this VisualElement element, PseudoStates pseudoStates)
        {
            k_PseudoStates.SetValue(element, (int)pseudoStates);
        }

        internal static PseudoStates GetPseudoStates(this VisualElement element)
        {
            return (PseudoStates)((int)k_PseudoStates.GetValue(element));
        }
            
        internal static Rect GetWorldBoundingBox(this VisualElement element)
        {
            return (Rect)k_WorldBoundingBox.GetValue(element);
        }
        
        internal static void SetIsCompositeRoot(this VisualElement element, bool isCompositeRoot)
        {
            k_IsCompositeRoot.SetValue(element, isCompositeRoot);
        }

        internal static bool GetIsCompositeRoot(this VisualElement element)
        {
            return (bool)k_IsCompositeRoot.GetValue(element);
        }

        internal static void SetExcludeFromFocusRing(this VisualElement element, bool excludeFromFocusRing)
        {
            k_ExcludeFromFocusRing.SetValue(element, excludeFromFocusRing);
        }

        internal static bool GetExcludeFromFocusRing(this VisualElement element)
        {
            return (bool)k_ExcludeFromFocusRing.GetValue(element);
        }
        
#endif
    }
}
