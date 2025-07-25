using System;
using System.Windows.Input;
using Unity.AppUI.Bridge;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Pressable Manipulator, used on <see cref="Button"/> elements.
    /// </summary>
    public class Pressable : PointerManipulator
#if ENABLE_RUNTIME_DATA_BINDINGS
        , INotifyBindablePropertyChanged
#endif
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId commandProperty = nameof(command);
#endif
        /// <summary>
        /// The event invoked when the element is pressed.
        /// </summary>
        public event Action clicked;

        /// <summary>
        /// The event invoked when the element is pressed.
        /// </summary>
        public event Action<EventBase> clickedWithEventInfo;

        /// <summary>
        /// The event invoked when the element is pressed for a long time.
        /// </summary>
        public event Action longClicked;

        /// <summary>
        /// Check if the element is currently pressed.
        /// </summary>
        public bool active { get; private set; }

        /// <summary>
        /// <para>The duration of a long press in milliseconds.</para>
        /// <para>
        /// The default value is -1.
        /// </para>
        /// </summary>
        /// <remarks>
        /// Using a negative value will disable long press.
        /// </remarks>
        public int longPressDuration { get; set; } = -1;

        /// <summary>
        /// When true, the event propagation will not be stopped when the element is pressed.
        /// </summary>
        public bool keepEventPropagation { get; set; } = true;

        /// <summary>
        /// A command to invoke when the Button is clicked.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public ICommand command
        {
            get => m_Command;
            set
            {
                var changed = m_Command != value;
                if (m_Command != null)
                    m_Command.CanExecuteChanged -= OnCommandCanExecuteChanged;
                m_Command = value;
                if (m_Command != null)
                    m_Command.CanExecuteChanged += OnCommandCanExecuteChanged;
                UpdateEnabledState();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in commandProperty);
#endif
            }
        }

        Event m_MoveEvent;

        Touch m_TouchMoveEvent;

        Event m_UpEvent;

        Touch m_TouchUpEvent;

        IVisualElementScheduledItem m_DeferDeactivate;

        IVisualElementScheduledItem m_DeferLongPress;

        IVisualElementScheduledItem m_PostProcessDisabledState;

        int m_PointerId;

        ICommand m_Command;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Pressable()
        {
            m_MoveEvent = new Event { type = EventType.MouseMove };
            m_TouchMoveEvent = new Touch { phase = TouchPhase.Moved };
            m_UpEvent = new Event { type = EventType.MouseUp };
            m_TouchUpEvent = new Touch { phase = TouchPhase.Ended };

            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler"> The event handler to register with the Pressed event.</param>
        public Pressable(Action handler)
            : this()
        {
            clicked += handler;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler"> The event handler to register with the Pressed event.</param>
        public Pressable(Action<EventBase> handler)
            : this()
        {
            clickedWithEventInfo += handler;
        }

        /// <summary>
        /// Invoke the Pressed event.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the press.</param>
        public void InvokePressed(EventBase evt) => Invoke(evt);

        void Invoke(EventBase evt)
        {
            m_Command?.Execute(target.userData);
            clicked?.Invoke();
            clickedWithEventInfo?.Invoke(evt);
            PostProcessDisabledState();
            m_PostProcessDisabledState?.ExecuteLater(Styles.animationRefreshDelayMs);
        }

        /// <summary>
        /// Invoke the LongPressed event.
        /// </summary>
        public void InvokeLongPressed()
        {
            longClicked?.Invoke();
            PostProcessDisabledState();
            m_PostProcessDisabledState?.ExecuteLater(Styles.animationRefreshDelayMs);
        }

        void OnCommandCanExecuteChanged(object sender, EventArgs e)
        {
            UpdateEnabledState();
        }

        void UpdateEnabledState()
        {
            if (m_Command == null)
                return;
            target.SetEnabled(m_Command.CanExecute(target.userData));
        }

        void PostProcessDisabledState()
        {
            if (!target.enabledInHierarchy)
            {
                // the element is no more enabled, remove the active and hovered states
                Deactivate(m_PointerId);
                RemoveHoverState();
            }
        }

        /// <summary>
        /// Simulate a single click on the target element.
        /// </summary>
        /// <param name="evt">The base event to use to invoke the click.</param>
        internal void SimulateSingleClickInternal(EventBase evt)
        {
            if (target != null)
            {
                var pseudoStates = target.GetPseudoStates();
                target.SetPseudoStates(pseudoStates | PseudoStates.Active);
                target.AddToClassList(Styles.activeUssClassName);
                target.schedule
                    .Execute(() =>
                    {
                        target.SetPseudoStates(target.GetPseudoStates() & ~PseudoStates.Active);
                        target.RemoveFromClassList(Styles.activeUssClassName);
                    })
                    .ExecuteLater(16L);
            }
            InvokePressed(evt);
        }

        /// <summary>
        /// Force the active pseudo state on the target element.
        /// </summary>
        public void ForceActivePseudoState()
        {
            if (target != null)
            {
                var pseudoStates = target.GetPseudoStates();
                target.SetPseudoStates(pseudoStates | PseudoStates.Active);
                target.AddToClassList(Styles.activeUssClassName);
            }
        }

        /// <summary>
        /// Called to register event callbacks from the target element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            m_PostProcessDisabledState = target.schedule.Execute(PostProcessDisabledState);
            m_PostProcessDisabledState.Pause();

            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
#if !UNITY_2023_1_OR_NEWER
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
#endif
            target.RegisterCallback<KeyDownEvent>(OnKeyDown);
            target.RegisterCallback<KeyUpEvent>(OnKeyUp);
        }

        /// <summary>
        /// Called to unregister event callbacks from the target element.
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
            m_PostProcessDisabledState?.Pause();
            m_PostProcessDisabledState = null;

            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            target.UnregisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
#if !UNITY_2023_1_OR_NEWER
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
#endif
            target.UnregisterCallback<KeyDownEvent>(OnKeyDown);
            target.UnregisterCallback<KeyUpEvent>(OnKeyUp);
        }

        /// <summary>
        /// Custom handling of pointer enter events.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPos"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id.</param>
        protected virtual void ProcessDownEvent(EventBase evt, Vector2 localPos, int pointerId)
        {

        }

        /// <summary>
        /// Custom handling of pointer leave events.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPos"> The local position of the pointer.</param>
        /// <param name="pointerId"> The pointer id.</param>
        protected virtual void ProcessUpEvent(EventBase evt, Vector2 localPos, int pointerId)
        {

        }

        /// <summary>
        /// Custom handling of pointer move events.
        /// </summary>
        /// <param name="evt"> The event to process.</param>
        /// <param name="localPos"> The local position of the pointer.</param>
        protected virtual void ProcessMoveEvent(EventBase evt, Vector2 localPos)
        {

        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode.IsSubmitType())
            {
                Activate(PointerId.mousePointerId);
                evt.StopPropagation();
            }
        }

        void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode.IsSubmitType())
            {
                InvokePressed(evt);
                Deactivate(PointerId.mousePointerId);
                evt.StopPropagation();
            }
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            if (!target.enabledInHierarchy)
                return;

            if (evt.pointerId == PointerId.mousePointerId)
                AddHoveredState();
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            RemoveHoverState();
        }

        void AddHoveredState()
        {
            var pseudoStates = target.GetPseudoStates();
            target.SetPseudoStates(pseudoStates | PseudoStates.Hover);
            target.AddToClassList(Styles.hoveredUssClassName);
        }

        void RemoveHoverState()
        {
            var pseudoStates = target.GetPseudoStates();
            target.SetPseudoStates(pseudoStates & ~PseudoStates.Hover);
            target.RemoveFromClassList(Styles.hoveredUssClassName);
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (!CanStartManipulation(evt))
                return;

            Activate(evt.pointerId);
            ProcessDownEvent(evt, evt.localPosition, evt.pointerId);
            evt.StopPropagation();
        }

        void OnMouseDown(MouseDownEvent evt)
        {
            if (active)
            {
                if (!target.HasMouseCapture())
                    target.CaptureMouse();
                evt.StopPropagation();
            }
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (!CanStopManipulation(evt))
                return;

            var parent = target?.parent;
            if (parent == null)
                return;

            ProcessMoveEvent(evt, evt.localPosition);

            if (!active)
                return;

            if (!keepEventPropagation)
                return;

            m_MoveEvent.mousePosition = evt.position;
            m_MoveEvent.delta = evt.deltaPosition;
            m_MoveEvent.button = evt.button;
            m_MoveEvent.modifiers = evt.modifiers;
            m_MoveEvent.pressure = evt.pressure;
            m_MoveEvent.clickCount = evt.clickCount;

            m_TouchMoveEvent.fingerId = evt.pointerId - PointerId.touchPointerIdBase;
            m_TouchMoveEvent.position = evt.position;
            m_TouchMoveEvent.deltaPosition = evt.deltaPosition;
            m_TouchMoveEvent.deltaTime = evt.deltaTime;
            m_TouchMoveEvent.tapCount = evt.clickCount;
            m_TouchMoveEvent.pressure = evt.pressure;
            m_TouchMoveEvent.azimuthAngle = evt.azimuthAngle;
            m_TouchMoveEvent.altitudeAngle = evt.altitudeAngle;
            m_TouchMoveEvent.radius = evt.radius.x;
            m_TouchMoveEvent.radiusVariance = evt.radiusVariance.x;

            using var e = evt.pointerId == PointerId.mousePointerId ?
                PointerMoveEvent.GetPooled(m_MoveEvent) :
                PointerMoveEvent.GetPooled(m_TouchMoveEvent, evt.modifiers);
            e.target = parent;
            parent.SendEvent(e);
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (!CanStopManipulation(evt))
                return;

            ProcessUpEvent(evt, evt.localPosition, evt.pointerId);

            if (!active)
                return;

            if (target.ContainsPoint(evt.localPosition))
                InvokePressed(evt);
            Deactivate(evt.pointerId);

            var parent = target?.parent;
            if (parent == null || !keepEventPropagation)
                return;

            m_UpEvent.mousePosition = evt.position;
            m_UpEvent.delta = evt.deltaPosition;
            m_UpEvent.button = evt.button;
            m_UpEvent.modifiers = evt.modifiers;
            m_UpEvent.pressure = evt.pressure;
            m_UpEvent.clickCount = evt.clickCount;

            m_TouchUpEvent.fingerId = evt.pointerId - PointerId.touchPointerIdBase;
            m_TouchUpEvent.position = evt.position;
            m_TouchUpEvent.deltaPosition = evt.deltaPosition;
            m_TouchUpEvent.deltaTime = evt.deltaTime;
            m_TouchUpEvent.tapCount = evt.clickCount;
            m_TouchUpEvent.pressure = evt.pressure;
            m_TouchUpEvent.azimuthAngle = evt.azimuthAngle;
            m_TouchUpEvent.altitudeAngle = evt.altitudeAngle;
            m_TouchUpEvent.radius = evt.radius.x;
            m_TouchUpEvent.radiusVariance = evt.radiusVariance.x;

            using var e = evt.pointerId == PointerId.mousePointerId ?
                PointerUpEvent.GetPooled(m_UpEvent) :
                PointerUpEvent.GetPooled(m_TouchUpEvent, evt.modifiers);
            e.target = parent;
            parent.SendEvent(e);
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            Deactivate(evt.pointerId);
        }

        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            Deactivate(evt.pointerId);
        }

        void Activate(int pointerId)
        {
            if (!target.HasPointerCapture(pointerId))
            {
                target.CapturePointer(pointerId);
#if !UNITY_2023_1_OR_NEWER
                if (pointerId == PointerId.mousePointerId)
                    target.CaptureMouse();
#endif
            }

            ForceActivePseudoState();
            target.AddToClassList(Styles.activeUssClassName);
            m_PointerId = pointerId;
            m_DeferDeactivate = target.schedule.Execute(DeferDeactivate);
            m_DeferDeactivate.ExecuteLater(50L);
            m_DeferLongPress?.Pause();
            m_DeferLongPress = null;
            if (longPressDuration > 0)
            {
                m_DeferLongPress = target.schedule.Execute(OnLongPress);
                m_DeferLongPress.ExecuteLater(longPressDuration);
            }
            active = true;
        }

        void Deactivate(int pointerId)
        {
            active = false;

            if (target.HasPointerCapture(pointerId))
                target.ReleasePointer(pointerId);

            if (m_DeferDeactivate != null)
                return;

            var pseudoStates = target.GetPseudoStates();
            target.SetPseudoStates(pseudoStates & ~PseudoStates.Active);
            target.RemoveFromClassList(Styles.activeUssClassName);
        }

        void DeferDeactivate()
        {
            m_DeferDeactivate = null;
            if (!active)
                Deactivate(m_PointerId);
        }

        void OnLongPress()
        {
            m_DeferLongPress?.Pause();
            m_DeferLongPress = null;
            if (active)
            {
                InvokeLongPressed();
                Deactivate(m_PointerId);
            }
        }

#if ENABLE_RUNTIME_DATA_BINDINGS
        /// <summary>
        /// Event raised when a Bindable property changes.
        /// </summary>
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

        void NotifyPropertyChanged(in BindingId id)
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(id));
        }
#endif
    }
}
