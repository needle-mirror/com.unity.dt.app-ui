using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Event sent when the number of splitters in a <see cref="SplitView"/> changes.
    /// </summary>
    /// <remarks>
    /// This event is dispatched after splitters have been created or removed
    /// following a change in pane count or the first layout pass.
    /// It can be used to defer collapse/expand operations until the splitters are ready.
    /// </remarks>
    public class SplitViewSplitterCountChangedEvent : EventBase<SplitViewSplitterCountChangedEvent>
    {
        /// <summary>
        /// The previous number of splitters.
        /// </summary>
        public int previousCount { get; internal set; }

        /// <summary>
        /// The new number of splitters.
        /// </summary>
        public int newCount { get; internal set; }

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
            tricklesDown = true;
            bubbles = true;
            previousCount = 0;
            newCount = 0;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SplitViewSplitterCountChangedEvent() => LocalInit();
    }
}
