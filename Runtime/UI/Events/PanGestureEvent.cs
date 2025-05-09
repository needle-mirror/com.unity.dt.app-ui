using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using EventPropagation = Unity.AppUI.Bridge.EventBaseExtensionsBridge.EventPropagation;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Event sent when a pan gesture is recognized.
    /// </summary>
    public class PanGestureEvent : EventBase<PanGestureEvent>
    {
        /// <summary>
        /// The pan gesture.
        /// </summary>
        public PanGesture gesture { get; set; }
        
        /// <summary>
        /// Resets all event members to their initial values.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            LocalInit();
        }

        void LocalInit()
        {
            this.SetPropagation(EventPropagation.Bubbles | EventPropagation.TricklesDown
#if !UNITY_2023_2_OR_NEWER
                | EventPropagation.Cancellable
#endif
            );
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PanGestureEvent() => LocalInit();
    }
}
