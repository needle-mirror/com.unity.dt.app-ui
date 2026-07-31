using System;
using Unity.AppUI.Bridge;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A text input component that allows users to enter and edit text.
    /// </summary>
    /// <remarks>
    /// TextField is a fundamental input component that enables users to enter and edit text content. It provides a
    /// versatile interface for capturing user input in various formats like plain text, passwords, and validated
    /// content.
    ///
    /// The component supports different sizes, placeholder text, leading and trailing icons, and various
    /// customization options to fit different use cases and design requirements.
    ///
    /// When using TextField for sensitive information like passwords, make sure to enable the isPassword property
    /// to mask the input appropriately.
    /// </remarks>
    /// <example>
    /// <para>Basic TextField Example: Simple text input with placeholder.</para>
    /// <code lang="xml"><![CDATA[
    /// <TextField placeholder="Enter your name" size="Size.M" />
    /// ]]></code>
    /// <para>Password Field Example: Secure password input with validation.</para>
    /// <code lang="xml"><![CDATA[
    /// <TextField
    ///     isPassword="true"
    ///     maskChar="*"
    ///     placeholder="Enter password"
    ///     maxLength="20"
    ///     leadingIconName="lock"
    /// />
    /// ]]></code>
    /// <para>Search Field Example: Search input with icons.</para>
    /// <code lang="xml"><![CDATA[
    /// <TextField
    ///     leadingIconName="search"
    ///     trailingIconName="clear"
    ///     placeholder="Search..."
    ///     size="Size.L"
    /// />
    /// ]]></code>
    /// <para>Validated Input Example: Email input with validation.</para>
    /// <code lang="csharp"><![CDATA[
    /// var textField = new TextField();
    /// textField.placeholder = "Enter email";
    /// textField.validateValue = (value) => {
    ///     return value.Contains("@") && value.Contains(".");
    /// };
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TextField : ExVisualElement, IInputElement<string>, INotifyValueChanging<string>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId isReadOnlyProperty = nameof(isReadOnly);

        internal static readonly BindingId maxLengthProperty = nameof(maxLength);

        internal static readonly BindingId placeholderProperty = nameof(placeholder);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId isPasswordProperty = nameof(isPassword);

        internal static readonly BindingId maskCharProperty = nameof(maskChar);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId leadingIconNameProperty = nameof(leadingIconName);

        internal static readonly BindingId trailingIconNameProperty = nameof(trailingIconName);


        /// <summary>
        /// The TextField main styling class.
        /// </summary>
        public const string ussClassName = "appui-textfield";

        /// <summary>
        /// The TextField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The TextField leading container styling class.
        /// </summary>
        public const string leadingContainerUssClassName = ussClassName + "__leadingcontainer";

        /// <summary>
        /// The TextField leading icon styling class.
        /// </summary>
        public const string leadingIconUssClassName = ussClassName + "__leadingicon";

        /// <summary>
        /// The TextField input container styling class.
        /// </summary>
        public const string inputContainerUssClassName = ussClassName + "__inputcontainer";

        /// <summary>
        /// The TextField input styling class.
        /// </summary>
        public const string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TextField placeholder styling class.
        /// </summary>
        public const string placeholderUssClassName = ussClassName + "__placeholder";

        /// <summary>
        /// The TextField trailing container styling class.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailingcontainer";

        /// <summary>
        /// The TextField trailing icon styling class.
        /// </summary>
        public const string trailingIconUssClassName = ussClassName + "__trailingicon";

        const bool k_IsPasswordDefault = false;

        const bool k_IsReadOnlyDefault = false;

        const char k_MaskCharDefault = '*';

        const int k_MaxLengthDefault = -1;

        readonly UnityEngine.UIElements.TextField m_InputField;

        readonly VisualElement m_LeadingContainer;

        readonly LocalizedTextElement m_Placeholder;

        Size m_Size;

        bool m_IsPassword;

        readonly VisualElement m_TrailingContainer;

        string m_Value;

        string m_PreviousValue;

        VisualElement m_LeadingElement;

        VisualElement m_TrailingElement;

        Func<string, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextField()
            : this(null) { }

        /// <summary>
        /// Construct a TextField with a predefined text value.
        /// </summary>
        /// <param name="value">A default text value.</param>
        /// <remarks>
        /// No event will be triggered when setting the text value during construction.
        /// </remarks>
        public TextField(string value)
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            delegatesFocus = true;

            m_LeadingContainer = new VisualElement { name = leadingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_LeadingContainer.AddToClassList(leadingContainerUssClassName);
            hierarchy.Add(m_LeadingContainer);

            var leadingIcon = new Icon { name = leadingIconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            leadingIcon.AddToClassList(leadingIconUssClassName);
            m_LeadingContainer.hierarchy.Add(leadingIcon);

            var inputContainer = new VisualElement { name = inputContainerUssClassName, pickingMode = PickingMode.Ignore };
            inputContainer.AddToClassList(inputContainerUssClassName);
            hierarchy.Add(inputContainer);

            m_Placeholder = new LocalizedTextElement { name = placeholderUssClassName, pickingMode = PickingMode.Ignore, focusable = false };
            m_Placeholder.AddToClassList(placeholderUssClassName);
            inputContainer.hierarchy.Add(m_Placeholder);

            m_InputField = new UnityEngine.UIElements.TextField { name = inputUssClassName };
            m_InputField.AddToClassList(inputUssClassName);
            m_InputField.AddManipulator(new BlinkingCursor());
            inputContainer.hierarchy.Add(m_InputField);

            m_TrailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_TrailingContainer.AddToClassList(trailingContainerUssClassName);
            hierarchy.Add(m_TrailingContainer);

            var trailingIcon = new Icon { name = trailingIconUssClassName, iconName = null, pickingMode = PickingMode.Ignore };
            trailingIcon.AddToClassList(trailingIconUssClassName);
            m_TrailingContainer.hierarchy.Add(trailingIcon);

            SetValueWithoutNotify(value);
            leadingElement = leadingIcon;
            trailingElement = trailingIcon;
            leadingIconName = null;
            trailingIconName = null;
            size = Size.M;
            isPassword = k_IsPasswordDefault;
            isReadOnly = k_IsReadOnlyDefault;
            maskChar = k_MaskCharDefault;
            maxLength = k_MaxLengthDefault;

            m_InputField.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_InputField.RuntimeContextMenu();
            m_Placeholder.RegisterValueChangedCallback(OnPlaceholderValueChanged);
            m_InputField.RegisterValueChangedCallback(OnInputValueChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
        }

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ApplyIsPassword();
        }

        void OnInputValueChanged(ChangeEvent<string> e)
        {
            e.StopPropagation();

            using var evt = ChangingEvent<string>.GetPooled();
            evt.target = this;
            evt.previousValue = m_Value;
            m_Value = e.newValue;
            evt.newValue = m_Value;

            if (validateValue != null) invalid = !validateValue(m_Value);
            RefreshUI();
            SendEvent(evt);
        }

        void OnPlaceholderValueChanged(ChangeEvent<string> evt)
        {
            evt.StopPropagation();
        }

        /// <summary>
        /// The content container of the TextField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The TextField leading element.
        /// </summary>
        public VisualElement leadingElement
        {
            get => m_LeadingElement;
            set
            {
                if (m_LeadingElement == value)
                    return;

                if (m_LeadingElement != null)
                    m_LeadingContainer.Remove(m_LeadingElement);

                m_LeadingElement = value;

                if (m_LeadingElement != null)
                    m_LeadingContainer.Add(m_LeadingElement);
            }
        }

        /// <summary>
        /// The TextField trailing element.
        /// </summary>
        public VisualElement trailingElement
        {
            get => m_TrailingElement;
            set
            {
                if (m_TrailingElement == value)
                    return;

                if (m_TrailingElement != null)
                    m_TrailingContainer.Remove(m_TrailingElement);

                m_TrailingElement = value;

                if (m_TrailingElement != null)
                    m_TrailingContainer.Add(m_TrailingElement);
            }
        }

        /// <summary>
        /// Whether the TextField is a password field.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isPassword
        {
            get => m_IsPassword;
            set
            {
                var changed = m_IsPassword != value;
                m_IsPassword = value;
                ApplyIsPassword();

                if (changed)
                    NotifyPropertyChanged(in isPasswordProperty);
            }
        }

        /// <summary>
        /// Pushes <see cref="isPassword"/> down to the underlying UITK TextField.
        /// </summary>
        /// <remarks>
        /// Enabling UITK's <c>isPasswordField</c> forces multiline off, which walks text
        /// backing that is only wired up once the field belongs to a panel. Setting it
        /// while detached throws a NullReferenceException, so the value is stored here and
        /// applied on attach instead. Disabling it is safe at any time, but is deferred too
        /// so the panel remains the single point where the two stay in sync.
        /// </remarks>
        void ApplyIsPassword()
        {
            if (panel == null)
                return;

            m_InputField.isPasswordField = m_IsPassword;
        }

        /// <summary>
        /// Whether the TextField is read-only.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isReadOnly
        {
            get => m_InputField.isReadOnly;
            set
            {
                var changed = m_InputField.isReadOnly != value;
                m_InputField.isReadOnly = value;

                if (changed)
                    NotifyPropertyChanged(in isReadOnlyProperty);
            }
        }

        /// <summary>
        /// The TextField mask character.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public char maskChar
        {
            get => m_InputField.maskChar;
            set
            {
                var changed = m_InputField.maskChar != value;
                m_InputField.maskChar = value;

                if (changed)
                    NotifyPropertyChanged(in maskCharProperty);
            }
        }

        /// <summary>
        /// The TextField max length.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int maxLength
        {
            get => m_InputField.maxLength;
            set
            {
                var changed = m_InputField.maxLength != value;
                m_InputField.maxLength = value;

                if (changed)
                    NotifyPropertyChanged(in maxLengthProperty);
            }
        }

        /// <summary>
        /// The TextField placeholder text.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string placeholder
        {
            get => m_Placeholder.text;
            set
            {
                var changed = m_Placeholder.text != value;
                m_Placeholder.text = value;

                if (changed)
                    NotifyPropertyChanged(in placeholderProperty);
            }
        }

        /// <summary>
        /// The trailing icon name.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string trailingIconName
        {
            get => (trailingElement as Icon)?.iconName;
            set
            {
                if (trailingElement is not Icon icon)
                    return;

                var changed = icon.iconName != value;
                icon.iconName = value;
                m_TrailingContainer.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon.iconName));

                if (changed)
                    NotifyPropertyChanged(in trailingIconNameProperty);
            }
        }

        /// <summary>
        /// The leading icon name.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string leadingIconName
        {
            get => (leadingElement as Icon)?.iconName;
            set
            {
                if (leadingElement is not Icon icon)
                    return;

                var changed = icon.iconName != value;
                icon.iconName = value;
                m_LeadingContainer.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(icon.iconName));

                if (changed)
                    NotifyPropertyChanged(in leadingIconNameProperty);
            }
        }

        /// <summary>
        /// The TextField size.
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

                switch (leadingElement)
                {
                    case ISizeableElement leadingSizeable:
                        leadingSizeable.size = m_Size;
                        break;
                    case Icon leadingIcon:
                        leadingIcon.size = m_Size switch
                        {
                            Size.S => IconSize.S,
                            Size.M => IconSize.S,
                            Size.L => IconSize.M,
                            _ => IconSize.S
                        };
                        break;
                }

                switch (trailingElement)
                {
                    case ISizeableElement trailingSizeable:
                        trailingSizeable.size = m_Size;
                        break;
                    case Icon trailingIcon:
                        trailingIcon.size = m_Size switch
                        {
                            Size.S => IconSize.S,
                            Size.M => IconSize.S,
                            Size.L => IconSize.M,
                            _ => IconSize.S
                        };
                        break;
                }

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The validation function for the TextField.
        /// </summary>
        [CreateProperty]
        public Func<string, bool> validateValue
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
        /// The invalid state of the TextField.
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
        /// Set the TextField value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the TextField. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;
            m_InputField.SetValueWithoutNotify(m_Value);
            RefreshUI();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The TextField value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string value
        {
            get => m_InputField.value;
            set
            {
                if (m_Value == value && m_PreviousValue == value)
                {
                    RefreshUI();
                    return;
                }

                using var evt = ChangeEvent<string>.GetPooled(m_PreviousValue, value);
                m_PreviousValue = m_Value;
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        void OnFocusedOut(FocusOutEvent e)
        {
            RemoveFromClassList(Styles.focusedUssClassName);
            RemoveFromClassList(Styles.keyboardFocusUssClassName);
            value = m_InputField.value;
            m_InputField.cursorIndex = 0;
        }

        void OnFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            passMask = 0;
            m_PreviousValue = m_Value;
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            AddToClassList(Styles.keyboardFocusUssClassName);
            passMask = Passes.Clear | Passes.Outline;
            m_PreviousValue = m_Value;
        }

        void RefreshUI()
        {
            m_Placeholder.EnableInClassList(Styles.hiddenUssClassName, !string.IsNullOrEmpty(m_Value));
        }

    }
}
