using System;

namespace Unity.AppUI.UI
{
    /// <summary>
    ///  <para>Event propagation flags.</para>
    /// </summary>
    [Flags]
    public enum EventPropagations
    {
        /// <summary>
        /// <para>Default propagation flags.</para>
        /// </summary>
        None = 0,
        
        /// <summary>
        /// <para>Event bubbles up the hierarchy.</para>
        /// </summary>
        Bubbles = 1,
        
        /// <summary>
        /// <para>Event trickles down the hierarchy.</para>
        /// </summary>
        TricklesDown = 2,
        
        /// <summary>
        /// <para>Event can be cancelled.</para>
        /// </summary>
        Cancellable = 4,
        
        /// <summary>
        /// <para> Skip disabled elements.</para>
        /// </summary>
        SkipDisabledElements = 8,
        
        /// <summary>
        /// <para>Ignore composite roots.</para>
        /// </summary>
        IgnoreCompositeRoots = 16, // 0x00000010
    }
}
