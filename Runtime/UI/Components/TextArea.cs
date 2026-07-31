using System;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A multi-line text input field that allows users to enter and edit longer text content.
    /// </summary>
    /// <remarks>
    /// TextArea is a versatile input component that enables users to enter and edit multiple lines of text. It's
    /// particularly useful for collecting longer form responses, comments, or any scenario requiring multi-line
    /// text input.
    ///
    /// The component features a resizable input area, placeholder text support, and various customization options
    /// like auto-resize and validation capabilities.
    ///
    /// Key features include:
    ///
    /// - Resizable text area with drag handle
    /// - Auto-resize capability based on content
    /// - Placeholder text support
    /// - Read-only mode
    /// - Maximum length restriction
    /// - Input validation
    /// - Submit on Enter functionality
    /// - Keyboard focus management
    /// </remarks>
    /// <example>
    /// <para>Basic TextArea with placeholder</para>
    ///
    /// <para>Creates a simple TextArea with placeholder text and fixed dimensions.</para>
    /// <code lang="xml"><![CDATA[
    /// <TextArea placeholder="Enter your message here..." style="height: 100px; width: 300px;" />
    /// ]]></code>
    /// <para>Auto-resizing TextArea with validation</para>
    ///
    /// <para>Creates a TextArea that automatically resizes and validates input length.</para>
    /// <code lang="csharp"><![CDATA[
    /// var textArea = new TextArea {
    ///     autoResize = true,
    ///     autoShrink = true,
    ///     placeholder = "Enter at least 10 characters",
    ///     style = { minHeight = 50, width = 300 }
    /// };
    ///
    /// textArea.validateValue = (value) => value.Length >= 10;
    /// textArea.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Text is valid: {!textArea.invalid}");
    /// });
    /// ]]></code>
    /// <para>Comment box with submission handling</para>
    ///
    /// <para>Creates a comment box that submits on Shift+Enter with character limit.</para>
    /// <code lang="xml"><![CDATA[
    /// <TextArea
    ///     placeholder="Write your comment..."
    ///     submit-on-enter="true"
    ///     submit-modifiers="Shift"
    ///     max-length="500"
    ///     style="min-height: 100px; width: 100%" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class TextArea : ExVisualElement, IInputElement<string>, INotifyValueChanging<string>
    {

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId isReadOnlyProperty = nameof(isReadOnly);

        internal static readonly BindingId maxLengthProperty = nameof(maxLength);

        internal static readonly BindingId placeholderProperty = nameof(placeholder);

        internal static readonly BindingId autoResizeProperty = nameof(autoResize);

        internal static readonly BindingId autoShrinkProperty = nameof(autoShrink);

        internal static readonly BindingId submitOnEnterProperty = nameof(submitOnEnter);

        internal static readonly BindingId submitModifiersProperty = nameof(submitModifiers);

        internal static readonly BindingId submitActionKeyModifierProperty = nameof(submitActionKeyModifier);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId invalidProperty = nameof(invalid);



        /// <summary>
        /// The TextArea main styling class.
        /// </summary>
        public const string ussClassName = "appui-textarea";

        /// <summary>
        /// The TextArea input container styling class.
        /// </summary>
        public const string scrollViewUssClassName = ussClassName + "__scrollview";

        /// <summary>
        /// The TextArea resize handle styling class.
        /// </summary>
        public const string resizeHandleUssClassName = ussClassName + "__resize-handle";

        /// <summary>
        /// The TextArea input styling class.
        /// </summary>
        public const string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TextArea placeholder styling class.
        /// </summary>
        public const string placeholderUssClassName = ussClassName + "__placeholder";

        const bool k_IsReadOnlyDefault = false;

        const int k_MaxLengthDefault = -1;

        readonly UnityEngine.UIElements.TextField m_InputField;

        readonly LocalizedTextElement m_Placeholder;


        Size m_Size;

        string m_Value;

        readonly VisualElement m_ResizeHandle;

        string m_PreviousValue;

        bool m_RequestSubmit;

        bool m_RequestTab;

        EventModifiers m_SubmitModifiers;

        bool m_SubmitActionKeyModifier;

        bool m_SubmitOnEnter;

        Func<string, bool> m_ValidateValue;

        bool m_AutoResize;

        bool m_AutoShrink;

        /// <summary>
        /// Event triggered when the user presses the Enter key and <see cref="submitOnEnter"/> is true.
        /// </summary>
        public event Action submitted;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TextArea()
            : this(null) { }

        /// <summary>
        /// Construct a TextArea with a predefined text value.
        /// </summary>
        /// <param name="value">A default text value.</param>
        /// <remarks>
        /// No event will be triggered when setting the text value during construction.
        /// </remarks>
        public TextArea(string value)
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            passMask = 0;
            tabIndex = 0;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            delegatesFocus = true;

            m_Placeholder = new LocalizedTextElement
            {
                name = placeholderUssClassName,
                pickingMode = PickingMode.Ignore,
                focusable = false
            };
            m_Placeholder.AddToClassList(placeholderUssClassName);
            hierarchy.Add(m_Placeholder);

            m_InputField = new UnityEngine.UIElements.TextField {name = inputUssClassName, multiline = true};
            m_InputField.AddToClassList(inputUssClassName);
            m_InputField.verticalScrollerVisibility = ScrollerVisibility.Auto;
            m_InputField.style.position = Position.Absolute;
            m_InputField.style.top = 0;
            m_InputField.style.left = 0;
            m_InputField.style.right = 0;
            m_InputField.style.bottom = 0;
            hierarchy.Add(m_InputField);

            m_ResizeHandle = new VisualElement
            {
                name = resizeHandleUssClassName,
                pickingMode = PickingMode.Position,
            };
            m_ResizeHandle.AddToClassList(resizeHandleUssClassName);
            hierarchy.Add(m_ResizeHandle);
            var dragManipulator = new Draggable(null, OnDrag, null)
            {
                dragDirection = Draggable.DragDirection.Vertical
            };
            m_ResizeHandle.AddManipulator(dragManipulator);
            m_ResizeHandle.RegisterCallback<ClickEvent>(OnResizeHandleClicked);

            isReadOnly = k_IsReadOnlyDefault;
            maxLength = k_MaxLengthDefault;
            autoResize = false;
            autoShrink = false;
            placeholder = string.Empty;
            submitModifiers = EventModifiers.None;
            submitOnEnter = false;

            SetValueWithoutNotify(value);
            m_InputField.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_InputField.AddManipulator(new BlinkingCursor());
            m_InputField.RuntimeContextMenu();
            m_InputField.RegisterValueChangedCallback(OnInputValueChanged);
            m_Placeholder.RegisterValueChangedCallback(OnPlaceholderValueChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Tab && !evt.shiftKey)
            {
                evt.StopPropagation();
                m_RequestTab = true;
                return;
            }

            if (m_RequestTab && evt.keyCode == KeyCode.None)
            {
                evt.StopPropagation();
                focusController.FocusNextInDirectionEx(this, VisualElementFocusChangeDirection.right);
            }

            if (submitOnEnter && evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                var isSubmit = submitActionKeyModifier ? evt.actionKey : evt.modifiers == submitModifiers;
                if (isSubmit)
                {
                    evt.StopPropagation();
                    submitted?.Invoke();

                    // Clamp cursor indices after submit callback, which may have
                    // cleared the text. Prevents UGUI's synthetic OnSubmit event
                    // from crashing in TextEditingUtilities.Insert with a stale index.
                    var len = (m_InputField.value ?? string.Empty).Length;
                    if (m_InputField.cursorIndex > len)
                        m_InputField.cursorIndex = len;
                    if (m_InputField.selectIndex > len)
                        m_InputField.selectIndex = len;
                }
                else
                {
                    // Insert newline explicitly and stop propagation to avoid
                    // UIElements/UGUI cursor sync issues (ArgumentOutOfRangeException).
                    var text = m_InputField.value ?? string.Empty;
                    var cursor = m_InputField.cursorIndex;
                    var select = m_InputField.selectIndex;
                    int start = Math.Max(0, Math.Min(Math.Min(cursor, select), text.Length));
                    int end = Math.Max(0, Math.Min(Math.Max(cursor, select), text.Length));

                    var newText = text.Substring(0, start) + "\n" + text.Substring(end);
                    var newCursor = start + 1;

                    m_Value = newText;
                    m_InputField.SetValueWithoutNotify(newText);
                    RefreshUI();

                    // Re-focus and restore cursor position on next frame to avoid
                    // breaking the TextField's editing state.
                    schedule.Execute(() =>
                    {
                        m_InputField.Focus();
                        m_InputField.SelectRange(newCursor, newCursor);
                    });

                    evt.StopPropagation();
                }
            }

            m_RequestTab = false;
        }

        void OnResizeHandleClicked(ClickEvent evt)
        {
            if (evt.clickCount == 2)
            {
                evt.StopPropagation();
                autoResize = true;
                AutoResize();
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var newHeight = contentRect.height;
            var currentHeight = m_InputField.resolvedStyle.minHeight;

            if (currentHeight.keyword == StyleKeyword.Auto || !Mathf.Approximately(newHeight, currentHeight.value))
                m_InputField.style.minHeight = newHeight;

            AutoResize();
        }

        static void OnPlaceholderValueChanged(ChangeEvent<string> evt)
        {
            evt.StopPropagation();
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

        void AutoResize()
        {
            if (!autoResize || panel == null  || !contentRect.IsValid())
                return;

            var width = m_InputField.resolvedStyle.width -
                m_InputField.resolvedStyle.borderLeftWidth -
                m_InputField.resolvedStyle.borderRightWidth -
                m_InputField.resolvedStyle.paddingLeft -
                m_InputField.resolvedStyle.paddingRight;

            var text = m_InputField.text ?? string.Empty;
            if (text.EndsWith("\n"))
                text += "1";

            var textSize = m_InputField.MeasureTextSize(
                text,
                width, MeasureMode.Exactly,
                0, MeasureMode.Undefined);

            var newHeight = textSize.y +
                resolvedStyle.paddingTop +
                resolvedStyle.paddingBottom +
                resolvedStyle.borderTopWidth +
                resolvedStyle.borderBottomWidth +
                m_InputField.resolvedStyle.borderTopWidth +
                m_InputField.resolvedStyle.borderBottomWidth +
                m_InputField.resolvedStyle.marginTop +
                m_InputField.resolvedStyle.marginBottom +
                m_InputField.resolvedStyle.paddingTop +
                m_InputField.resolvedStyle.paddingBottom;

            newHeight = Mathf.Max(resolvedStyle.minHeight.value, newHeight);

            if (Mathf.Approximately(newHeight, resolvedStyle.height))
                return;

            if (newHeight > resolvedStyle.height || autoShrink)
                style.height = newHeight;
        }

        void OnDrag(Draggable draggable)
        {
            autoResize = false;
            style.height = Mathf.Max(resolvedStyle.minHeight.value, resolvedStyle.height + draggable.deltaPos.y);
        }

        /// <summary>
        /// The content container of the TextArea.
        /// </summary>
        public override VisualElement contentContainer => m_InputField.contentContainer;

        /// <summary>
        /// The TextArea placeholder text.
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
        /// The validation function for the TextArea.
        /// </summary>
        [CreateProperty]
        public Func<string, bool> validateValue
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
        /// The invalid state of the TextArea.
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
        /// Whether the TextArea is read-only.
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
        /// The maximum length of the TextArea.
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
        /// Automatically resize the <see cref="TextArea"/> if the content is larger than the current size.
        /// </summary>
        /// <remarks>
        /// <para>This will only grow the <see cref="TextArea"/>. It will not shrink it.</para>
        /// <para>If the user manually resizes the <see cref="TextArea"/>, the auto resize will be disabled.</para>
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool autoResize
        {
            get => m_AutoResize;
            set
            {
                var changed = m_AutoResize != value;
                m_AutoResize = value;

                if (changed)
                    NotifyPropertyChanged(in autoResizeProperty);
            }
        }

        /// <summary>
        /// Whether the <see cref="TextArea"/> should automatically shrink if the content is smaller than the current size.
        /// </summary>
        /// <remarks>
        /// To enable this feature, <see cref="autoResize"/> must be set to true.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool autoShrink
        {
            get => m_AutoShrink;
            set
            {
                var changed = m_AutoShrink != value;
                m_AutoShrink = value;

                if (changed)
                    NotifyPropertyChanged(in autoShrinkProperty);
            }
        }

        /// <summary>
        /// Set the TextArea value without notifying the change.
        /// </summary>
        /// <param name="newValue"> The new value of the TextArea. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;
            m_InputField.SetValueWithoutNotify(m_Value);
            RefreshUI();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// Whether the TextArea should invoke the <see cref="submitted"/> event when the user presses the Enter key.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool submitOnEnter
        {
            get => m_SubmitOnEnter;
            set
            {
                var changed = m_SubmitOnEnter != value;
                m_SubmitOnEnter = value;

                if (changed)
                    NotifyPropertyChanged(in submitOnEnterProperty);
            }
        }

        /// <summary>
        /// The modifiers required to submit the TextArea.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public EventModifiers submitModifiers
        {
            get => m_SubmitModifiers;
            set
            {
                var changed = m_SubmitModifiers != value;
                m_SubmitModifiers = value;

                if (changed)
                    NotifyPropertyChanged(in submitModifiersProperty);
            }
        }

        /// <summary>
        /// Whether the submit action should be triggered by using the Action key (Ctrl on Windows, Cmd on Mac) modifier.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool submitActionKeyModifier
        {
            get => m_SubmitActionKeyModifier;
            set
            {
                var changed = m_SubmitActionKeyModifier != value;
                m_SubmitActionKeyModifier = value;

                if (changed)
                    NotifyPropertyChanged(in submitActionKeyModifierProperty);
            }
        }

        /// <summary>
        /// The TextArea value.
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
            passMask = 0;
            value = m_InputField.value;
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
            AutoResize();
        }

    }
}
