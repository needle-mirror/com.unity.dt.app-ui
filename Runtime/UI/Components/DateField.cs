using System;
using System.Globalization;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A form input control that allows users to select a date through text input or a calendar picker interface.
    /// </summary>
    /// <remarks>
    /// The DateField component combines a text input with a calendar picker to provide users with flexible date
    /// selection capabilities. Users can either type the date directly in a standardized format or click the
    /// calendar icon to visually select a date from a popup calendar interface.
    ///
    /// The component supports different sizes to fit various layout needs and includes validation capabilities to
    /// ensure date inputs meet specific requirements.
    ///
    /// Key features:
    /// - Direct text input with format validation
    /// - Calendar picker interface
    /// - Keyboard navigation support
    /// - Customizable date formatting
    /// - Size variants
    /// - Value validation
    ///
    /// NOTE: The DateField component follows a standardized date format by default (yyyy-MM-dd) but can be
    /// customized using the formatString property.
    /// </remarks>
    /// <example>
    /// <para>Basic usage in UXML:</para>
    /// <code lang="xml"><![CDATA[
    /// <DateField name="birthDate" />
    /// ]]></code>
    /// <para>Customized DateField with validation:</para>
    /// <code lang="csharp"><![CDATA[
    /// var dateField = new DateField {
    ///     formatString = "MM/dd/yyyy",
    ///     size = Size.L
    /// };
    ///
    /// dateField.validateValue = (date) => {
    ///     var now = Date.now;
    ///     var minAge = new Date(now.Year - 18, now.Month, now.Day);
    ///     return date <= minAge; // Validate age >= 18
    /// };
    ///
    /// dateField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Selected date: {evt.newValue}");
    /// });
    /// ]]></code>
    /// <para>Complete UXML example with various properties:</para>
    /// <code lang="xml"><![CDATA[
    /// <DateField
    ///     name="appointmentDate"
    ///     size="M"
    ///     format-string="yyyy-MM-dd"
    ///     value="2024-01-01"
    ///     class="appointment-picker" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class DateField : ExVisualElement, IInputElement<Date>, INotifyValueChanging<Date>, ISizeableElement
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId formatStringProperty = nameof(formatString);


        /// <summary>
        /// The DateField main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-date-field";

        /// <summary>
        /// The DateField input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The DateField picker button styling class.
        /// </summary>
        public static readonly string pickerButtonUssClassName = ussClassName + "__picker-button";

        /// <summary>
        /// The DateField size styling class.
        /// </summary>
        public static readonly string sizeUssClassName = ussClassName + "--size-";

        readonly UnityEngine.UIElements.TextField m_InputElement;

        readonly VisualElement m_PickerButton;

        Date m_Value;

        Size m_Size;

        Date m_PreviousValue;

        DatePicker m_Picker;

        Func<Date, bool> m_ValidateValue;

        Popover m_Popover;

        string m_FormatString;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DateField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;

            m_InputElement = new UnityEngine.UIElements.TextField()
            {
                name = inputUssClassName,
                pickingMode = PickingMode.Ignore,
                isDelayed = true,
            };
            m_InputElement.AddToClassList(inputUssClassName);
            m_InputElement.RegisterValueChangedCallback(OnInputValueChanged);
            m_InputElement.RegisterCallback<KeyDownEvent>(OnInputKeyDown, TrickleDown.TrickleDown);

            m_PickerButton = new VisualElement()
            {
                focusable = true,
                pickingMode = PickingMode.Position,
                name = pickerButtonUssClassName,
            };
            m_PickerButton.AddToClassList(pickerButtonUssClassName);
            m_PickerButton.AddManipulator(new Pressable(OnClick));
            var icon = new Icon { iconName = "calendar", pickingMode = PickingMode.Ignore };
            m_PickerButton.Add(icon);

            hierarchy.Add(m_InputElement);
            hierarchy.Add(m_PickerButton);

            size = Size.M;
            SetValueWithoutNotify(Date.now);
            formatString = "yyyy-MM-dd";
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnInputKeyDown(KeyDownEvent evt)
        {
            var delta = evt.keyCode switch
            {
                KeyCode.UpArrow => 1,
                KeyCode.DownArrow => -1,
                _ => 0
            };

            if (delta == 0)
                return;

            evt.StopPropagation();
            if (evt.target == m_InputElement.Q<TextElement>())
            {
                var newStartDate = ((DateTime)m_Value).AddDays(delta);
                value = new Date(newStartDate);
            }
        }

        void OnInputValueChanged(ChangeEvent<string> e)
        {
            var newValue = !string.IsNullOrEmpty(e.newValue) && DateTime.TryParse(e.newValue, out var date)
                ? new Date(date)
                : m_PreviousValue;
            value = newValue;
            SetFormattedString();
        }

        void OnClick()
        {
            m_Picker?.parent?.Remove(m_Picker);

            m_PreviousValue = value;
            m_Picker ??= new DatePicker
            {
                //todo add settings
            };
            m_Picker.SetValueWithoutNotify(m_PreviousValue);
            m_Picker.RegisterValueChangedCallback(OnPickerValueChanged);
            if (m_Popover != null)
            {
                m_Popover.Dismiss(DismissType.Consecutive);
                m_Popover.dismissed -= OnPopoverDismissed;
            }
            m_Popover = Popover.Build(this, m_Picker);
            m_Popover.dismissed += OnPopoverDismissed;
            m_Popover.Show();
            AddToClassList(Styles.focusedUssClassName);
        }

        void OnPopoverDismissed(Popover popover, DismissType reason)
        {
            popover.dismissed -= OnPopoverDismissed;
            if (popover == m_Popover)
                m_Popover = null;
            RemoveFromClassList(Styles.focusedUssClassName);
            m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
            if (m_PreviousValue != m_Picker.value)
            {
                using var evt = ChangeEvent<Date>.GetPooled(m_PreviousValue, m_Picker.value);
                SetValueWithoutNotify(m_Picker.value);
                evt.target = this;
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
            Focus();
        }

        void OnPickerValueChanged(ChangeEvent<Date> e)
        {
            if (e.newValue != value)
            {
                SetValueWithoutNotify(e.newValue);
                using var evt = ChangingEvent<Date>.GetPooled();
                evt.previousValue = m_PreviousValue;
                evt.newValue = e.newValue;
                evt.target = this;
                SendEvent(evt);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_PreviousValue = value;
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_PreviousValue = value;
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// The content container of this DateField. This is null for DateField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The DateField size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(sizeUssClassName + m_Size.ToString().ToLower());
                m_Size = value;
                AddToClassList(sizeUssClassName + m_Size.ToString().ToLower());

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The DateField invalid state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = ClassListContains(Styles.invalidUssClassName) != value;
                EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// The DateField validation function.
        /// </summary>
        [CreateProperty]
        public Func<Date, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Sets the DateField value without notifying the DateField.
        /// </summary>
        /// <param name="newValue"> The new DateField value. </param>
        public void SetValueWithoutNotify(Date newValue)
        {
            m_Value = newValue;
            SetFormattedString();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        void SetFormattedString()
        {
            var formattedValue = string.IsNullOrEmpty(m_FormatString)
                ? ((DateTime)m_Value).ToString(CultureInfo.InvariantCulture)
                : ((DateTime)m_Value).ToString(m_FormatString, CultureInfo.InvariantCulture);
            m_InputElement.SetValueWithoutNotify(formattedValue);
        }

        /// <summary>
        /// The DateField value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Date value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;

                using var evt = ChangeEvent<Date>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The DateField string formatting.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string formatString
        {
            get => m_FormatString;
            set
            {
                var changed = m_FormatString != value;
                m_FormatString = value;
                SetFormattedString();

                if (changed)
                    NotifyPropertyChanged(in formatStringProperty);
            }
        }

    }
}
