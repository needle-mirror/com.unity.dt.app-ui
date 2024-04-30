using System;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Color Field UI element.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ColorField : ExVisualElement, IInputElement<Color>, INotifyValueChanging<Color>, ISizeableElement, IPressable
    {
#if ENABLE_RUNTIME_DATA_BINDINGS

        internal static readonly BindingId sizeProperty = nameof(size);
        
        internal static readonly BindingId swatchOnlyProperty = nameof(swatchOnly);
        
        internal static readonly BindingId inlinePickerProperty = nameof(inlinePicker);
        
        internal static readonly BindingId invalidProperty = nameof(invalid);
        
        internal static readonly BindingId valueProperty = nameof(value);
        
        internal static readonly BindingId validateValueProperty = nameof(validateValue);

#endif
        
        /// <summary>
        /// The ColorField main styling class.
        /// </summary>
        public const string ussClassName = "appui-colorfield";

        /// <summary>
        /// The ColorField color swatch styling class.
        /// </summary>
        public const string colorSwatchUssClassName = ussClassName + "__color-swatch";

        /// <summary>
        /// The ColorField label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The ColorField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ColorField swatch only styling class.
        /// </summary>
        public const string swatchOnlyUssClassName = ussClassName + "--swatch-only";

        readonly ColorSwatch m_SwatchElement;

        readonly LocalizedTextElement m_LabelElement;

        Color m_Value;

        Size m_Size;

        Type m_Type;

        Pressable m_Clickable;

        Color m_PreviousValue;

        ColorPicker m_Picker;

        bool m_InlinePicker;

        Func<Color, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;
            clickable = new Pressable(OnClick);

            m_SwatchElement = new ColorSwatch
            {
                name = colorSwatchUssClassName,
                pickingMode = PickingMode.Ignore,
                round = true,
            };
            m_SwatchElement.AddToClassList(colorSwatchUssClassName);

            m_LabelElement = new LocalizedTextElement
            {
                name = labelUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_LabelElement.AddToClassList(labelUssClassName);

            hierarchy.Add(m_SwatchElement);
            hierarchy.Add(m_LabelElement);

            size = Size.M;
            SetValueWithoutNotify(Color.clear);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnClick()
        {
            var wasInline = m_Picker != null && m_Picker.parent == parent;
            m_Picker?.parent?.Remove(m_Picker);

            if (inlinePicker && wasInline)
            {
                RemoveFromClassList(Styles.focusedUssClassName);
                m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
                using var evt = ChangeEvent<Color>.GetPooled(m_PreviousValue, m_Picker.value);
                SetValueWithoutNotify(m_Picker.value);
                evt.target = this;
                SendEvent(evt);
                return;
            }

            m_PreviousValue = value;
            m_Picker = m_Picker ?? new ColorPicker
            {
                showAlpha = true,
                showHex = true,
                showToolbar = true,
            };
            m_Picker.previousValue = m_PreviousValue;
            m_Picker.SetValueWithoutNotify(m_PreviousValue);
            m_Picker.RegisterValueChangedCallback(OnPickerValueChanged);
            if (inlinePicker)
            {
                var idx = parent.IndexOf(this) + 1;
                parent.Insert(idx, m_Picker);
            }
            else
            {
                var popover = Popover.Build(this, m_Picker);
                popover.dismissed += (_, _) =>
                {
                    RemoveFromClassList(Styles.focusedUssClassName);
                    m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
                    if (m_PreviousValue != m_Picker.value)
                    {
                        using var evt = ChangeEvent<Color>.GetPooled(m_PreviousValue, m_Picker.value);
                        SetValueWithoutNotify(m_Picker.value);
                        evt.target = this;
                        SendEvent(evt);
                    }
                    Focus();
                };
                popover.Show();
            }
            AddToClassList(Styles.focusedUssClassName);
        }

        void OnPickerValueChanged(ChangeEvent<Color> e)
        {
            if (e.newValue != value)
            {
                SetValueWithoutNotify(e.newValue);
                using var evt = ChangingEvent<Color>.GetPooled();
                evt.previousValue = m_PreviousValue;
                evt.newValue = e.newValue;
                evt.target = this;
                SendEvent(evt);
            }
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// The content container of this ColorField. This is null for ColorField.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Clickable Manipulator for this AssetTargetField.
        /// </summary>
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// The ColorField size.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
#endif
            }
        }

        /// <summary>
        /// The ColorField type. When this is true, the ColorField will only show the swatch.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool swatchOnly
        {
            get => ClassListContains(swatchOnlyUssClassName);
            set
            { 
                var changed = ClassListContains(swatchOnlyUssClassName) != value;
                EnableInClassList(swatchOnlyUssClassName, value);
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in swatchOnlyProperty);
#endif
            }
        }

        /// <summary>
        /// The ColorPicker position relative to the ColorField. When this is true, the ColorPicker will be inlined
        /// instead of being displayed in a Popover.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool inlinePicker
        {
            get => m_InlinePicker;
            set
            {
                var changed = m_InlinePicker != value;
                m_InlinePicker = value;
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in inlinePickerProperty);
#endif
            }
        }

        /// <summary>
        /// The ColorField invalid state.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool invalid
        {
            get => ClassListContains(Styles.invalidUssClassName);
            set
            {
                var changed = ClassListContains(Styles.invalidUssClassName) != value;
                EnableInClassList(Styles.invalidUssClassName, value);
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in invalidProperty);
#endif
            }
        }

        /// <summary>
        /// The ColorField validation function.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public Func<Color, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
#endif
            }
        }

        /// <summary>
        /// Sets the ColorField value without notifying the ColorField.
        /// </summary>
        /// <param name="newValue"> The new ColorField value. </param>
        public void SetValueWithoutNotify(Color newValue)
        {
            m_Value = newValue;
            m_LabelElement.text = $"#{ColorExtensions.ColorToRgbaHex(m_Value)}";
            m_SwatchElement.color = m_Value;
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The ColorField value.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public Color value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                
                using var evt = ChangeEvent<Color>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                
#if ENABLE_RUNTIME_DATA_BINDINGS
                NotifyPropertyChanged(in valueProperty);
#endif
            }
        }
        
#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Class to instantiate a <see cref="ColorField"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ColorField, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ColorField"/>.
        /// </summary>
        public new class UxmlTraits : ExVisualElement.UxmlTraits
        {
            
            readonly UxmlBoolAttributeDescription m_Invalid = new UxmlBoolAttributeDescription
            {
                name = "invalid",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_SwatchOnly = new UxmlBoolAttributeDescription
            {
                name = "swatch-only",
                defaultValue = false
            };

            readonly UxmlBoolAttributeDescription m_InlinePicker = new UxmlBoolAttributeDescription
            {
                name = "inline-picker",
                defaultValue = false
            };

            readonly UxmlEnumAttributeDescription<Size> m_Size = new UxmlEnumAttributeDescription<Size>
            {
                name = "size",
                defaultValue = Size.M,
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

                var element = (ColorField)ve;
                element.size = m_Size.GetValueFromBag(bag, cc);
                element.invalid = m_Invalid.GetValueFromBag(bag, cc);
                element.swatchOnly = m_SwatchOnly.GetValueFromBag(bag, cc);
                element.inlinePicker = m_InlinePicker.GetValueFromBag(bag, cc);

            }
        }
#endif
    }
}
