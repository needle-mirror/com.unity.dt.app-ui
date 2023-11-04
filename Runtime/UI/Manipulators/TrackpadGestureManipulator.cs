using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A manipulator that can be used to receive trackpad gestures.
    /// </summary>
    public class TrackpadGestureManipulator : Manipulator
    {
        bool m_Inside;
        
        /// <summary>
        /// The callback that will be invoked when a magnification gesture is recognized.
        /// </summary>
        public Action<MagnificationGesture> onMagnify { get; set; }

        /// <summary>
        /// The callback that will be invoked when a pan gesture is recognized.
        /// </summary>
        public Action<PanGesture> onPan { get; set; }
        
        /// <summary>
        /// Creates a new instance of <see cref="TrackpadGestureManipulator"/>.
        /// </summary>
        /// <param name="onPan"> The callback that will be invoked when a pan gesture is recognized.</param>
        /// <param name="onMagnify"> The callback that will be invoked when a magnification gesture is recognized.</param>
        public TrackpadGestureManipulator(Action<PanGesture> onPan = null, Action<MagnificationGesture> onMagnify = null)
        {
            this.onPan = onPan;
            this.onMagnify = onMagnify;
        }

        /// <inheritdoc cref="Manipulator.RegisterCallbacksOnTarget"/>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnLeave);
            target.RegisterCallback<PanGestureEvent>(OnPan);
            target.RegisterCallback<MagnificationGestureEvent>(OnMagnify);
        }

        /// <inheritdoc cref="Manipulator.UnregisterCallbacksFromTarget"/>
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerEnterEvent>(OnEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnLeave);
            target.UnregisterCallback<PanGestureEvent>(OnPan);
            target.UnregisterCallback<MagnificationGestureEvent>(OnMagnify);
        }

        void OnEnter(PointerEnterEvent evt)
        {
            m_Inside = true;
        }

        void OnLeave(PointerLeaveEvent evt)
        {
            m_Inside = false;
        }

        void OnMagnify(MagnificationGestureEvent evt)
        {
            if (!m_Inside)
                return;
            
            onMagnify?.Invoke(evt.gesture);
        }

        void OnPan(PanGestureEvent evt)
        {
            if (!m_Inside)
                return;
            
            onPan?.Invoke(evt.gesture);
        }
    }
}
