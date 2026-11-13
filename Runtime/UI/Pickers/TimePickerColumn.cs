using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A single spinner column of a <see cref="TimePicker"/>, stepping one <see cref="TimeSegment"/>.
    /// </summary>
    /// <remarks>
    /// A column is an increment button, the current value of its segment, and a decrement button. It reads
    /// and writes the value of the <see cref="TimePicker"/> it is nested in, so several columns stay in sync
    /// without holding any state of their own. A column outside a TimePicker renders empty and does nothing.
    ///
    /// Pressing and holding a button auto-repeats the step after a short delay, so a user can spin through a
    /// full hour without clicking sixty times. Enter and Space step once from the keyboard.
    ///
    /// The picker creates the default columns itself. Declare them explicitly only when you want a different
    /// order or a subset of the segments.
    /// </remarks>
    /// <example>
    /// <para>A minutes-only picker in UXML:</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:TimePicker minute-step="15">
    ///         <appui:TimePickerColumn segment="Minutes" />
    ///     </appui:TimePicker>
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TimePickerColumn : BaseVisualElement
    {

        internal static readonly BindingId segmentProperty = nameof(segment);

        /// <summary>
        /// The TimePickerColumn main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-time-picker-column";

        /// <summary>
        /// The TimePickerColumn segment variant styling class.
        /// </summary>
        /// <remarks>
        /// Suffixed with the lowercase segment name, so `appui-time-picker-column--hours`.
        /// </remarks>
        public static readonly string segmentUssClassName = ussClassName + "--";

        /// <summary>
        /// The TimePickerColumn stepper button styling class.
        /// </summary>
        public static readonly string buttonUssClassName = ussClassName + "__button";

        /// <summary>
        /// The TimePickerColumn increment button styling class.
        /// </summary>
        public static readonly string incrementUssClassName = ussClassName + "__increment";

        /// <summary>
        /// The TimePickerColumn decrement button styling class.
        /// </summary>
        public static readonly string decrementUssClassName = ussClassName + "__decrement";

        /// <summary>
        /// The TimePickerColumn value label styling class.
        /// </summary>
        public static readonly string valueUssClassName = ussClassName + "__value";

        /// <summary>
        /// The delay in milliseconds before a held button starts auto-repeating.
        /// </summary>
        internal const long holdDelayMs = 400;

        /// <summary>
        /// The interval in milliseconds between two auto-repeated steps.
        /// </summary>
        internal const long holdIntervalMs = 80;

        readonly VisualElement m_IncrementButton;

        readonly VisualElement m_DecrementButton;

        readonly Text m_ValueLabel;

        TimeSegment m_Segment;

        IVisualElementScheduledItem m_RepeatItem;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimePickerColumn()
        {
            AddToClassList(ussClassName);

            focusable = false;
            pickingMode = PickingMode.Ignore;

            m_IncrementButton = CreateButton(incrementUssClassName, "caret-up", 1);
            m_ValueLabel = new Text { pickingMode = PickingMode.Ignore };
            m_ValueLabel.AddToClassList(valueUssClassName);
            m_DecrementButton = CreateButton(decrementUssClassName, "caret-down", -1);

            hierarchy.Add(m_IncrementButton);
            hierarchy.Add(m_ValueLabel);
            hierarchy.Add(m_DecrementButton);

            segment = TimeSegment.Hours;

            RegisterCallback<AttachToPanelEvent>(_ => Refresh());
            RegisterCallback<DetachFromPanelEvent>(_ => StopRepeat());
        }

        VisualElement CreateButton(string additionalUssClassName, string iconName, int delta)
        {
            var button = new VisualElement
            {
                name = additionalUssClassName,
                focusable = true,
                pickingMode = PickingMode.Position,
            };
            button.AddToClassList(buttonUssClassName);
            button.AddToClassList(additionalUssClassName);

            var icon = new Icon { primary = true, iconName = iconName, pickingMode = PickingMode.Ignore };
            button.hierarchy.Add(icon);

            button.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;
                evt.StopPropagation();
                button.CapturePointer(evt.pointerId);
                Step(delta);
                StartRepeat(delta);
            });
            button.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (button.HasPointerCapture(evt.pointerId))
                    button.ReleasePointer(evt.pointerId);
                StopRepeat();
            });
            button.RegisterCallback<PointerCaptureOutEvent>(_ => StopRepeat());
            button.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode is not (KeyCode.Return or KeyCode.KeypadEnter or KeyCode.Space))
                    return;
                evt.StopPropagation();
                Step(delta);
            });

            return button;
        }

        void StartRepeat(int delta)
        {
            StopRepeat();
            m_RepeatItem = schedule.Execute(() => Step(delta)).StartingIn(holdDelayMs).Every(holdIntervalMs);
        }

        void StopRepeat()
        {
            m_RepeatItem?.Pause();
            m_RepeatItem = null;
        }

        void Step(int delta)
        {
            picker?.StepSegment(m_Segment, delta);
        }

        /// <summary>
        /// The content container of this TimePickerColumn. This is null for TimePickerColumn.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The TimePicker this column belongs to, or null when it stands alone.
        /// </summary>
        public TimePicker picker => GetFirstAncestorOfType<TimePicker>();

        /// <summary>
        /// The segment of the time value this column steps.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public TimeSegment segment
        {
            get => m_Segment;
            set
            {
                var changed = m_Segment != value;
                RemoveFromClassList(segmentUssClassName + m_Segment.ToString().ToLower());
                m_Segment = value;
                AddToClassList(segmentUssClassName + m_Segment.ToString().ToLower());
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in segmentProperty);
            }
        }

        /// <summary>
        /// The text currently displayed by the column.
        /// </summary>
        public string text => m_ValueLabel.text;

        /// <summary>
        /// Re-reads the value from the owning <see cref="TimePicker"/> and refreshes the displayed text.
        /// </summary>
        public void Refresh()
        {
            var owner = picker;
            m_ValueLabel.text = owner == null
                ? string.Empty
                : TimeField.FormatSegment(owner.value, m_Segment, owner.hour12);
        }
    }
}
