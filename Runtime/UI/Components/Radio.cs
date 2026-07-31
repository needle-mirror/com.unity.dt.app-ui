using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A toggle control that allows users to select one option from a set of mutually exclusive choices.
    /// </summary>
    /// <remarks>
    /// The Radio component is a form control that allows users to select a single option from a group of related
    /// choices. Radio buttons are typically used when there are two or more mutually exclusive options, and only
    /// one option can be selected at a time.
    ///
    /// Radio buttons should be used within a RadioGroup to manage the selection state and ensure that only one
    /// option can be selected at a time.
    ///
    /// Radio buttons should always be used in groups of two or more options. For single boolean choices, consider
    /// using a Checkbox instead.
    ///
    /// Radio buttons consist of a circular button and a label. When selected, the button displays a filled circle.
    /// The control can be interacted with either by clicking the button or its associated label.
    /// </remarks>
    /// <example>
    /// <para>Basic Radio Group.</para>
    /// <code lang="xml"><![CDATA[
    /// <RadioGroup>
    ///     <Radio key="small" label="Small" size="Size.S" />
    ///     <Radio key="medium" label="Medium" size="Size.M" />
    ///     <Radio key="large" label="Large" size="Size.L" />
    /// </RadioGroup>
    /// ]]></code>
    /// <para>Radio Group with Validation.</para>
    /// <code lang="csharp"><![CDATA[
    /// var radioGroup = new RadioGroup();
    /// radioGroup.validateValue = (value) => !string.IsNullOrEmpty(value);
    ///
    /// radioGroup.Add(new Radio { key = "option1", label = "Option 1" });
    /// radioGroup.Add(new Radio { key = "option2", label = "Option 2" });
    /// ]]></code>
    /// <para>Emphasized Radio Buttons.</para>
    /// <code lang="xml"><![CDATA[
    /// <RadioGroup>
    ///     <Radio key="opt1" label="Important Choice 1" emphasized="true" />
    ///     <Radio key="opt2" label="Important Choice 2" emphasized="true" />
    /// </RadioGroup>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class Radio : BaseVisualElement, IValidatableElement<bool>, IPressable
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId emphasizedProperty = nameof(emphasized);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId keyProperty = nameof(key);

        internal static readonly BindingId clickableProperty = nameof(clickable);


        /// <summary>
        /// The Radio main styling class.
        /// </summary>
        public const string ussClassName = "appui-radio";

        /// <summary>
        /// The Radio size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Radio emphasized mode styling class.
        /// </summary>
        public const string emphasizedUssClassName = ussClassName + "--emphasized";

        /// <summary>
        /// The Radio button styling class.
        /// </summary>
        public const string boxUssClassName = ussClassName + "__button";

        /// <summary>
        /// The Radio checkmark styling class.
        /// </summary>
        public const string checkmarkUssClassName = ussClassName + "__checkmark";

        /// <summary>
        /// The Radio label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        readonly LocalizedTextElement m_Label;

        Size m_Size;

        bool m_Value;

        Pressable m_Clickable;

        readonly ExVisualElement m_Box;

        Func<bool, bool> m_ValidateValue;

        string m_Key;

        RadioGroup m_Group;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Radio()
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(OnClick);
            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;

            var radioIcon = new VisualElement { name = checkmarkUssClassName, pickingMode = PickingMode.Ignore };
            radioIcon.AddToClassList(checkmarkUssClassName);
            m_Box = new ExVisualElement { name = boxUssClassName, pickingMode = PickingMode.Ignore, passMask = 0 };
            m_Box.AddToClassList(boxUssClassName);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Box.hierarchy.Add(radioIcon);
            hierarchy.Add(m_Box);
            hierarchy.Add(m_Label);

            size = Size.M;
            emphasized = false;
            invalid = false;
            key = null;
            SetValueWithoutNotify(false);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Clickable Manipulator for this Radio.
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
        /// The Radio key.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string key
        {
            get => m_Key;
            set
            {
                var changed = m_Key != value;
                m_Key = value;
                TryAddToGroup();

                if (changed)
                    NotifyPropertyChanged(in keyProperty);
            }
        }

        /// <summary>
        /// The Radio size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The Radio emphasized mode.
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
        /// The Radio label.
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
                if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                    key = value;

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The Radio invalid state.
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
        /// The Radio validation function.
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
        /// Sets the Radio value without notifying the listeners.
        /// </summary>
        /// <param name="newValue"> The new value. </param>
        public void SetValueWithoutNotify(bool newValue)
        {
            m_Value = newValue;
            EnableInClassList(Styles.checkedUssClassName, m_Value);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The Radio value.
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

        void OnClick()
        {
            value = true;
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            m_Box.passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Outline;
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            TryAddToGroup();
        }

        void TryAddToGroup()
        {
            m_Group?.RemoveRadio(this);
            var group = GetFirstAncestorOfType<RadioGroup>();
            if (group != null && m_Group != group)
            {
                m_Group = group;
                m_Group.AddRadio(this);
            }
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            m_Group?.RemoveRadio(this);
        }

    }
}
