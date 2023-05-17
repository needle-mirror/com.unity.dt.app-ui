using System;
using System.Reflection;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// An Action has been triggered.
    /// </summary>
    public class ActionTriggeredEvent : EventBase<ActionTriggeredEvent>
    {
        static readonly PropertyInfo k_Propagation =
            typeof(EventBase).GetProperty("propagation", BindingFlags.Instance | BindingFlags.NonPublic);

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
            k_Propagation.SetValue(this,
                (int)(EventPropagations.Bubbles | EventPropagations.TricklesDown | EventPropagations.Cancellable));
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ActionTriggeredEvent() => LocalInit();
    }
}
