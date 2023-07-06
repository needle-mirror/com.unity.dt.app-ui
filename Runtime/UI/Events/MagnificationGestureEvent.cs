using Unity.AppUI.Core;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Event sent when a magnification gesture is recognized.
    /// </summary>
    public class MagnificationGestureEvent : EventBase<MagnificationGestureEvent>
    {
        /// <summary>
        /// The magnification gesture.
        /// </summary>
        public MagnificationGesture gesture { get; set; }
        
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
        public MagnificationGestureEvent() => LocalInit();    }
}
