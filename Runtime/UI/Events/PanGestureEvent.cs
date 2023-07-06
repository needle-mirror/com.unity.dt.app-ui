using Unity.AppUI.Core;
using UnityEngine.UIElements;

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
            EventUtils.propagationProp.SetValue(this,
                (int)(EventPropagations.Bubbles | EventPropagations.TricklesDown | EventPropagations.Cancellable));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PanGestureEvent() => LocalInit();
    }
}
