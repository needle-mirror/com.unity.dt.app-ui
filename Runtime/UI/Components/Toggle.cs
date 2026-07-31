using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A control that allows users to switch between two states: on (checked) and off (unchecked).
    /// </summary>
    /// <remarks>
    /// The Toggle component is a user interface element that allows users to switch between two states. It's
    /// commonly used for enabling or disabling options, features, or settings within an application.
    ///
    /// Toggles are best used when you need to:
    /// - Present a binary choice, such as on/off or true/false
    /// - Immediately apply state changes
    /// - Show the current state clearly through visual feedback
    ///
    /// The component consists of a box containing a checkmark that indicates the selected state, and an optional
    /// label that describes the toggle's purpose.
    ///
    /// When clicked or activated via keyboard, the toggle switches its state and triggers a change event that can
    /// be handled by the application.
    /// </remarks>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Toggle : BaseVisualElement, IInputElement<bool>, IPressable
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);


        /// <summary>
        /// The Toggle main styling class.
        /// </summary>
        public const string ussClassName = "appui-toggle";

        /// <summary>
        /// The Toggle size styling class.
        /// </summary>
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Toggle box styling class.
        /// </summary>
        public const string boxUssClassName = ussClassName + "__box";

        /// <summary>
        /// The Toggle box padded styling class.
        /// </summary>
        public const string paddedBoxUssClassName = ussClassName + "__boxpadded";

        /// <summary>
        /// The Toggle checkmark container styling class.
        /// </summary>
        public const string checkmarkContainerUssClassName = ussClassName + "__checkmarkcontainer";

        /// <summary>
        /// The Toggle checkmark styling class.
        /// </summary>
        public const string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Toggle label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_Label;

        bool m_Value;

        Pressable m_Clickable;

        readonly ExVisualElement m_Box;

        Func<bool, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Toggle()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            clickable = new Pressable(OnClick);

            var checkmark = new VisualElement
            {
                name = checkmarkUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            checkmark.EnableDynamicTransform(true);
            checkmark.AddToClassList(checkmarkUssClassName);
            var checkmarkContainer = new VisualElement { name = checkmarkContainerUssClassName, pickingMode = PickingMode.Ignore };
            checkmarkContainer.AddToClassList(checkmarkContainerUssClassName);
            var boxPadded = new VisualElement { name = paddedBoxUssClassName, pickingMode = PickingMode.Ignore };
            boxPadded.AddToClassList(paddedBoxUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            checkmarkContainer.hierarchy.Add(checkmark);
            boxPadded.hierarchy.Add(checkmarkContainer);
            m_Box.hierarchy.Add(boxPadded);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            label = null;
            invalid = false;
            SetValueWithoutNotify(false);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <summary>
        /// Clickable Manipulator for this Toggle.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = value != m_Clickable;
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// The Toggle label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The invalid state of the Toggle.
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
        /// The validation function for the Toggle.
        /// </summary>
        [CreateProperty]
        public Func<bool, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !m_ValidateValue?.Invoke(m_Value) ?? false;

                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Set the Toggle value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Toggle value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<bool>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

        void OnClick()
        {
            value = !value;
        }

    }
}
