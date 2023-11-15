using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Interface used on UI elements which handle a *disabled* state.
    /// </summary>
    public interface IDisableElement
    {
        /// <summary>
        /// **True** of the UI element is in disabled state, **False** otherwise.
        /// </summary>
        bool disabled { get; set; }
    }
}
