using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The possible states for a <see cref="Checkbox"/>.
    /// </summary>
    public enum CheckboxState
    {
        /// <summary>
        /// The <see cref="Checkbox"/> is completely unchecked.
        /// </summary>
        Unchecked,

        /// <summary>
        /// The <see cref="Checkbox"/> is unchecked but at least one of its dependencies is checked.
        /// </summary>
        Intermediate,

        /// <summary>
        /// The <see cref="Checkbox"/> is checked.
        /// </summary>
        Checked
    }

    /// <summary>
    /// A control that lets users make a binary choice between two mutually exclusive options.
    /// </summary>
    /// <remarks>
    /// The Checkbox component is a fundamental user interface control that allows users to select or deselect an
    /// option. It's particularly useful in forms, settings panels, and anywhere users need to make binary choices.
    ///
    /// Checkboxes can exist in three states: checked, unchecked, and intermediate. The intermediate state is useful
    /// when representing a collection of sub-items where only some are selected.
    ///
    /// The intermediate state cannot be directly toggled by user interaction - it can only be set programmatically.
    /// Users can only toggle between checked and unchecked states.
    ///
    /// - Users need to select one or more options from a list
    /// - Users need to toggle a single option on or off
    /// - Multiple independent choices need to be presented
    /// </remarks>
    /// <example>
    /// <para>Basic checkbox usage in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns:appui="Unity.AppUI.UI">
    ///     <appui:Checkbox label="Accept terms and conditions" />
    /// </UXML>
    /// ]]></code>
    /// <para>Creating a checkbox group programmatically.</para>
    /// <code lang="csharp"><![CDATA[
    /// var container = new VisualElement();
    ///
    /// var option1 = new Checkbox { label = "Option 1" };
    /// var option2 = new Checkbox { label = "Option 2" };
    /// var selectAll = new Checkbox { label = "Select All" };
    ///
    /// option1.RegisterValueChangedCallback(evt => UpdateSelectAllState());
    /// option2.RegisterValueChangedCallback(evt => UpdateSelectAllState());
    ///
    /// selectAll.RegisterValueChangedCallback(evt => {
    ///     if (evt.newValue != CheckboxState.Intermediate)
    ///     {
    ///         option1.value = evt.newValue;
    ///         option2.value = evt.newValue;
    ///     }
    /// });
    ///
    /// container.Add(selectAll);
    /// container.Add(option1);
    /// container.Add(option2);
    /// ]]></code>
    /// <para>Creating a validated checkbox.</para>
    /// <code lang="csharp"><![CDATA[
    /// var checkbox = new Checkbox { label = "Must be checked" };
    ///
    /// checkbox.validateValue = (state) => state == CheckboxState.Checked;
    /// checkbox.RegisterValueChangedCallback(evt => {
    ///     // invalid will be automatically updated based on validateValue
    ///     if (checkbox.invalid)
    ///     {
    ///         Debug.Log("Please check the box to continue");
    ///     }
    /// });
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Checkbox : BaseVisualElement, IInputElement<CheckboxState>, IPressable
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId emphasizedProperty = nameof(emphasized);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        /// <summary>
        /// The Checkbox main styling class.
        /// </summary>
        public const string ussClassName = "appui-checkbox";

        /// <summary>
        /// The Checkbox size styling class.
        /// </summary>
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Checkbox emphasized mode styling class.
        /// </summary>
        public const string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Checkbox box styling class.
        /// </summary>
        public const string boxUssClassName = ussClassName + "__box";

        /// <summary>
        /// The Checkbox checkmark styling class.
        /// </summary>
        public const string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Checkbox partial checkmark styling class.
        /// </summary>
        public const string partialCheckmarkUssClassName = ussClassName + "__partialcheckmark";

        /// <summary>
        /// The Checkbox label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_Label;

        CheckboxState m_Value;

        Pressable m_Clickable;

        readonly ExVisualElement m_Box;

        Func<CheckboxState, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Checkbox()
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(OnClicked);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            var checkmark = new Icon { name = checkmarkUssClassName, iconName = "check", variant = IconVariant.Bold, pickingMode = PickingMode.Ignore };
            checkmark.AddToClassList(checkmarkUssClassName);
            var partialCheckmark = new Icon { name = partialCheckmarkUssClassName, iconName = "minus", variant = IconVariant.Bold, pickingMode = PickingMode.Ignore };
            partialCheckmark.AddToClassList(partialCheckmarkUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Box.hierarchy.Add(checkmark);
            m_Box.hierarchy.Add(partialCheckmark);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            emphasized = false;
            invalid = false;
            label = null;
            SetValueWithoutNotify(CheckboxState.Unchecked);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnPointerFocus));
        }

        /// <summary>
        /// Clickable Manipulator for this Checkbox.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = m_Clickable != value;
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
        /// The Checkbox emphasized mode.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool emphasized
        {
            get => ClassListContains(emphasizedUssClassName);
            set
            {
                var changed = emphasized != value;
                EnableInClassList(emphasizedUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in emphasizedProperty);
            }
        }

        /// <summary>
        /// The text displayed in the Checkbox label.
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
        /// The Checkbox invalid state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = invalid != value;
                EnableInClassList(Styles.invalidUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
            }
        }

        /// <summary>
        /// The Checkbox validation function.
        /// </summary>
        [CreateProperty]
        public Func<CheckboxState, bool> validateValue
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
        /// Set the Checkbox value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new Checkbox value. </param>
        public void SetValueWithoutNotify(CheckboxState newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value == CheckboxState.Checked);
            EnableInClassList(Styles.intermediateUssClassName, m_Value == CheckboxState.Intermediate);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Checkbox value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public CheckboxState value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<CheckboxState>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        void OnClicked()
        {
            // when a user click on the UI element, it can't go into an intermediate state
            // intermediate state can be set programatically.
            value = value == CheckboxState.Checked ? CheckboxState.Unchecked : CheckboxState.Checked;
        }

        void OnPointerFocus(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

    }
}
