using System;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Text Area UI element.
    /// </summary>
    public class TextArea : ExVisualElement, IValidatableElement<string>, INotifyValueChanging<string>
    {
        /// <summary>
        /// The TextArea main styling class.
        /// </summary>
        public static readonly string ussClassName = "appui-textarea";

        /// <summary>
        /// The TextArea input container styling class.
        /// </summary>
        public static readonly string scrollViewUssClassName = ussClassName + "__scrollview";

        /// <summary>
        /// The TextArea resize handle styling class.
        /// </summary>
        public static readonly string resizeHandleUssClassName = ussClassName + "__resize-handle";

        /// <summary>
        /// The TextArea input styling class.
        /// </summary>
        public static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The TextArea placeholder styling class.
        /// </summary>
        public static readonly string placeholderUssClassName = ussClassName + "__placeholder";
        
        const bool k_IsReadOnlyDefault = false;
        
        const int k_MaxLengthDefault = -1;

        readonly UnityEngine.UIElements.TextField m_InputField;

        readonly LocalizedTextElement m_Placeholder;

#if !UNITY_2022_1_OR_NEWER
        readonly ScrollView m_ScrollView;
#endif

        Size m_Size;

        string m_Value;
        
        readonly VisualElement m_ResizeHandle;

        string m_PreviousValue;

        bool m_RequestSubmit;

        bool m_RequestTab;

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
        /// <remarks>
        /// No event will be triggered when setting the text value during construction.
        /// </remarks>
        /// </summary>
        /// <param name="value">A default text value.</param>
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

            m_InputField = new UnityEngine.UIElements.TextField { name = inputUssClassName, multiline = true };
            m_InputField.AddToClassList(inputUssClassName);
            m_InputField.BlinkingCursor();
#if UNITY_2022_1_OR_NEWER
#if UNITY_2023_1_OR_NEWER
            m_InputField.verticalScrollerVisibility = ScrollerVisibility.Auto;
#else
            m_InputField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
#endif
            m_InputField.style.position = Position.Absolute;
            m_InputField.style.top = 0;
            m_InputField.style.left = 0;
            m_InputField.style.right = 0;
            m_InputField.style.bottom = 0;
            hierarchy.Add(m_InputField);
#else
            m_ScrollView = new ScrollView
            {
                name = scrollViewUssClassName,
                elasticity = 0,
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
#if (UNITY_2021_3 && UNITY_2021_3_NIK) || (UNITY_2022_1 && UNITY_2022_1_NIK) || (UNITY_2022_2 && UNITY_2022_2_NIK) || UNITY_2022_3 || (UNITY_2023_1 && UNITY_2023_1_NIK) || UNITY_2023_2_OR_NEWER
                nestedInteractionKind = ScrollView.NestedInteractionKind.StopScrolling,
#endif
            };
            m_ScrollView.AddToClassList(scrollViewUssClassName);
            hierarchy.Add(m_ScrollView);
            m_ScrollView.Add(m_InputField);
#endif

            m_ResizeHandle = new VisualElement
            {
                name = resizeHandleUssClassName,
                pickingMode = PickingMode.Position,
            };
            m_ResizeHandle.AddToClassList(resizeHandleUssClassName);
            hierarchy.Add(m_ResizeHandle);
            var dragManipulator = new Draggable(null, OnDrag, null);
            m_ResizeHandle.AddManipulator(dragManipulator);
            m_ResizeHandle.RegisterCallback<ClickEvent>(OnResizeHandleClicked);
            
            isReadOnly = k_IsReadOnlyDefault;

            SetValueWithoutNotify(value);
            m_InputField.AddManipulator(new KeyboardFocusController(OnKeyboardFocusedIn, OnFocusedIn, OnFocusedOut));
            m_InputField.RegisterValueChangedCallback(OnInputValueChanged);
            m_Placeholder.RegisterValueChangedCallback(OnPlaceholderValueChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Tab && !evt.shiftKey)
            {
                m_RequestTab = true;
                return;
            }

            if (m_RequestTab && evt.keyCode == KeyCode.None)
            {
                evt.StopPropagation();
                evt.PreventDefault();
                focusController.FocusNextInDirectionEx(this, VisualElementFocusChangeDirection.right);
            }

            if (submitOnEnter && evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter && evt.modifiers == submitModifiers)
            {
                m_RequestSubmit = true;
                return;
            }

            if (m_RequestSubmit && evt.keyCode == KeyCode.None)
            {
                evt.StopPropagation();
                evt.PreventDefault();
                submitted?.Invoke();
            }
            
            m_RequestSubmit = false;
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
        }

        void OnPlaceholderValueChanged(ChangeEvent<string> evt)
        {
            
            evt.StopPropagation();
        }

        void OnInputValueChanged(ChangeEvent<string> e)
        {
            
            e.StopPropagation();

            if (autoResize)
                AutoResize();
            
            using var evt = ChangingEvent<string>.GetPooled();
            evt.target = this;
            evt.previousValue = m_Value;
            m_Value = e.newValue;
            evt.newValue = m_Value;
            
            if (validateValue != null) invalid = !validateValue(m_Value);
            SendEvent(evt);
        }

        void AutoResize()
        {
            if (panel == null || !contentRect.IsValid())
                return;
            
            var width = m_InputField.resolvedStyle.width -
                m_InputField.resolvedStyle.borderLeftWidth -
                m_InputField.resolvedStyle.borderRightWidth -
                m_InputField.resolvedStyle.paddingLeft -
                m_InputField.resolvedStyle.paddingRight;

            var textSize = m_InputField.MeasureTextSize(
                m_InputField.text, 
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
            
            if (newHeight > resolvedStyle.height)
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
        public string placeholder
        {
            get => m_Placeholder.text;
            set => m_Placeholder.text = value;
        }

        /// <summary>
        /// The validation function for the TextArea.
        /// </summary>
        public Func<string, bool> validateValue { get; set; }

        /// <summary>
        /// The invalid state of the TextArea.
        /// </summary>
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set => EnableInClassList(Styles.invalidUssClassName, value);
        }
        
        /// <summary>
        /// Whether the TextArea is read-only.
        /// </summary>
        public bool isReadOnly
        {
            get => m_InputField.isReadOnly;
            set => m_InputField.isReadOnly = value;
        }
        
        /// <summary>
        /// The maximum length of the TextArea.
        /// </summary>
        public int maxLength
        {
            get => m_InputField.maxLength;
            set => m_InputField.maxLength = value;
        }

        /// <summary>
        /// Automatically resize the <see cref="TextArea"/> if the content is larger than the current size.
        /// </summary>
        /// <remarks>
        /// This will only grow the <see cref="TextArea"/>. It will not shrink it.
        /// </remarks>
        /// <remarks>
        /// If the user manually resizes the <see cref="TextArea"/>, the auto resize will be disabled.
        /// </remarks>
        public bool autoResize { get; set; } = false;

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
        public bool submitOnEnter { get; set; } = false;
        
        /// <summary>
        /// The modifiers required to submit the TextArea.
        /// </summary>
        public EventModifiers submitModifiers { get; set; } = EventModifiers.None;

        /// <summary>
        /// The TextArea value.
        /// </summary>
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
            m_Placeholder.AddToClassList(Styles.hiddenUssClassName);
            passMask = 0;
            m_PreviousValue = m_Value;
        }

        void OnKeyboardFocusedIn(FocusInEvent evt)
        {
            AddToClassList(Styles.focusedUssClassName);
            AddToClassList(Styles.keyboardFocusUssClassName);
            m_Placeholder.AddToClassList(Styles.hiddenUssClassName);
            passMask = Passes.Clear | Passes.Outline;
            m_PreviousValue = m_Value;
        }

        void RefreshUI()
        {
            m_Placeholder.EnableInClassList(Styles.hiddenUssClassName, !string.IsNullOrEmpty(m_Value));
        }
        
        /// <summary>
        /// Whether the element is disabled.
        /// </summary>
        public bool disabled
        {
            get => !enabledSelf;
            set => SetEnabled(!value);
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="TextArea"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<TextArea, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="TextArea"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlBoolAttributeDescription m_Disabled = new()
            {
                name = "disabled",
                defaultValue = false
            };

            readonly UxmlStringAttributeDescription m_Placeholder = new()
            {
                name = "placeholder",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Value = new()
            {
                name = "value",
                defaultValue = null
            };
            
            readonly UxmlBoolAttributeDescription m_AutoResize = new()
            {
                name = "auto-resize",
                defaultValue = false
            };
            
            readonly UxmlBoolAttributeDescription m_SubmitOnEnter = new()
            {
                name = "submit-on-enter",
                defaultValue = false
            };
            
            readonly UxmlEnumAttributeDescription<EventModifiers> m_SubmitModifiers = new()
            {
                name = "submit-modifiers",
                defaultValue = EventModifiers.None
            };
            
            readonly UxmlBoolAttributeDescription m_IsReadOnly = new()
            {
                name = "is-read-only",
                defaultValue = k_IsReadOnlyDefault
            };
            
            readonly UxmlIntAttributeDescription m_MaxLength = new()
            {
                name = "max-length",
                defaultValue = k_MaxLengthDefault
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (TextArea)ve;

                el.placeholder = m_Placeholder.GetValueFromBag(bag, cc);
                el.autoResize = m_AutoResize.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
                el.disabled = m_Disabled.GetValueFromBag(bag, cc);
                el.submitOnEnter = m_SubmitOnEnter.GetValueFromBag(bag, cc);
                el.submitModifiers = m_SubmitModifiers.GetValueFromBag(bag, cc);
                el.isReadOnly = m_IsReadOnly.GetValueFromBag(bag, cc);
                el.maxLength = m_MaxLength.GetValueFromBag(bag, cc);
            }
        }
    }
}
