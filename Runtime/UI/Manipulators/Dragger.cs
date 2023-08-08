using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Manipulator that allows dragging from a container component.
    /// </summary>
    public class Dragger : PointerManipulator
    {
        /// <summary>
        /// The threshold in pixels after which a drag will start.
        /// </summary>
        public float dragThreshold { get; set; } = 8f;

        Action<PointerMoveEvent> m_DragStarted;

        Action<PointerMoveEvent> m_Dragging;

        Action<PointerUpEvent> m_DragEnded;

        Action m_DragCanceled;

        Vector3 m_StartPosition;
        
        /// <summary>
        /// Whether the drag is currently active.
        /// </summary>
        public bool isActive { get; private set; }
        
        /// <summary>
        /// Delegate that will be called when a drag should be accepted.
        /// </summary>
        /// <remarks>
        /// If this delegate is not set, the drag will be accepted by default.
        /// </remarks>
        public Func<bool> acceptDrag { get; set; }

        int m_PointerId = -1;

        /// <summary>
        /// Creates a new <see cref="Dragger"/> instance.
        /// </summary>
        /// <param name="dragStarted"> The event that will be fired when a drag starts. </param>
        /// <param name="dragging"> The event that will be fired when a drag is in progress. </param>
        /// <param name="dragEnded"> The event that will be fired when a drag ends. </param>
        /// <param name="dragCanceled"> The event that will be fired when a drag is cancelled. </param>
        public Dragger(Action<PointerMoveEvent> dragStarted, Action<PointerMoveEvent> dragging,
            Action<PointerUpEvent> dragEnded, Action dragCanceled)
        {
            m_DragStarted = dragStarted;
            m_Dragging = dragging;
            m_DragEnded = dragEnded;
            m_DragCanceled = dragCanceled;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            activators.Clear();
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            isActive = false;
            m_PointerId = -1;

            if (CanStartManipulation(evt))
            {
                if (!evt.target.HasPointerCapture(evt.pointerId))
                    evt.target.CapturePointer(evt.pointerId);
                m_StartPosition = evt.position;
                m_PointerId = evt.pointerId;
            }
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (m_PointerId != evt.pointerId)
                return;
            
            if (evt.target.HasPointerCapture(evt.pointerId))
            {
                if (!isActive)
                {
                    var delta = evt.position - m_StartPosition;
                    if (delta.sqrMagnitude > dragThreshold * dragThreshold && (acceptDrag == null || acceptDrag.Invoke()))
                    {
                        isActive = true;
                        m_DragStarted?.Invoke(evt);
                    }
                }
                else
                {
                    m_Dragging?.Invoke(evt);
                }
            }
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (m_PointerId != evt.pointerId)
                return;
            
            if (evt.target.HasPointerCapture(evt.pointerId))
                evt.target.ReleasePointer(evt.pointerId);

            if (isActive)
                m_DragEnded?.Invoke(evt);

            isActive = false;
            m_PointerId = -1;
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId == m_PointerId)
                Cancel();
        }

        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId == m_PointerId)
                Cancel();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (m_PointerId == -1)
                return;
            
            if (evt.keyCode == KeyCode.Escape)
            {
                evt.PreventDefault();
                evt.StopImmediatePropagation();
                Cancel();
            }
        }

        /// <summary>
        /// Cancels the drag.
        /// </summary>
        public void Cancel()
        {
            if (target.HasPointerCapture(m_PointerId))
                target.ReleasePointer(m_PointerId);
            
            if (isActive)
            {
                isActive = false;
                m_DragCanceled?.Invoke();
            }
            
            m_PointerId = -1;
        }
    }
}
