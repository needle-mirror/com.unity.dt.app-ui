using System;
using Unity.AppUI.Bridge;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The type of ColorPicker to open when the ColorField is clicked.
    /// </summary>
    public enum ColorPickerType
    {
        /// <summary>
        /// The default ColorPicker from AppUI.
        /// </summary>
        Default,

        /// <summary>
        /// The ColorPicker from Unity's Editor.
        /// </summary>
        UnityEditor
    }

    /// <summary>
    /// Interface for custom ColorField pickers.
    /// </summary>
    public interface ICustomColorFieldPicker
    {
        /// <summary>
        /// Delegate method to show the custom ColorPicker.
        /// </summary>
        /// <param name="owner"> The owner ColorField. </param>
        /// <param name="onValueChanging"> Callback when the color value has changed. It will send a value changing event. </param>
        /// <param name="initialColor"> The initial color to show in the ColorPicker. </param>
        /// <param name="showAlpha"> Whether to show the alpha channel in the ColorPicker. </param>
        /// <param name="hdr"> Whether to show HDR colors in the ColorPicker. </param>
        void Show(ColorField owner, Action<Color> onValueChanging, Color initialColor, bool showAlpha, bool hdr);
    }

    /// <summary>
    /// A field that allows users to select and display colors.
    /// </summary>
    /// <remarks>
    /// The ColorField component provides an interface for users to view, select, and modify colors. It displays the
    /// current color value through a color swatch and optionally shows the color's hex value. When clicked, it
    /// opens a color picker that allows detailed color selection.
    ///
    /// The component supports two different color picker types: the default AppUI color picker and Unity's editor
    /// color picker. The color picker provides multiple ways to select colors including a color wheel, RGB
    /// sliders, HSV sliders, and hex input.
    ///
    /// Key features:
    /// - Color visualization through a swatch
    /// - Optional text display of color value
    /// - Support for alpha channel
    /// - HDR color support
    /// - Multiple size variants
    /// - Inline or popover color picker
    /// </remarks>
    /// <example>
    /// <para>Basic ColorField with default settings. Creates a ColorField with medium size and all default settings.</para>
    /// <code lang="xml"><![CDATA[
    /// <ColorField />
    /// ]]></code>
    /// <para>ColorField with custom configuration. Creates a large ColorField with medium swatch, alpha support, HDR
    /// colors, and inline picker.</para>
    /// <code lang="xml"><![CDATA[
    /// <ColorField
    ///     size="L"
    ///     swatch-size="M"
    ///     show-alpha="true"
    ///     hdr="true"
    ///     inline-picker="true"
    ///     show-text="true" />
    /// ]]></code>
    /// <para>Minimal ColorField showing only the swatch. Creates a minimal ColorField showing only the color swatch.</para>
    /// <code lang="xml"><![CDATA[
    /// <ColorField
    ///     swatch-only="true"
    ///     size="S"
    ///     show-text="false" />
    /// ]]></code>
    /// <para>Code example showing how to handle color changes. Demonstrates how to create a ColorField, register for
    /// value changes, and set its value programmatically.</para>
    /// <code lang="csharp">
    /// var colorField = new ColorField();
    /// colorField.RegisterValueChangedCallback(evt => {
    ///     Debug.Log($"Color changed from {evt.previousValue} to {evt.newValue}");
    /// });
    /// colorField.value = Color.blue;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class ColorField : ExVisualElement, IInputElement<Color>, INotifyValueChanging<Color>, ISizeableElement, IPressable
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId swatchSizeProperty = nameof(swatchSize);

        internal static readonly BindingId swatchOnlyProperty = nameof(swatchOnly);

        internal static readonly BindingId showTextProperty = nameof(showText);

        internal static readonly BindingId inlinePickerProperty = nameof(inlinePicker);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId colorPickerTypeProperty = nameof(colorPickerType);

        internal static readonly BindingId showAlphaProperty = nameof(showAlpha);

        internal static readonly BindingId hdrProperty = nameof(hdr);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId customPickerProperty = nameof(customPicker);


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
        /// The ColorField color picker icon styling class.
        /// </summary>
        public const string colorPickerIconUssClassName = ussClassName + "__color-picker-icon";

        /// <summary>
        /// The ColorField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The ColorField swatch only styling class.
        /// </summary>
        public const string swatchOnlyUssClassName = ussClassName + "--swatch-only";

        /// <summary>
        /// The ColorField showText styling class.
        /// </summary>
        public const string showTextUssClassName = ussClassName + "--show-text";

        /// <summary>
        /// The ColorField inline picker opened styling class.
        /// </summary>
        public const string inlinePickerOpenedUssClassName = ussClassName + "--inline-picker-opened";

        readonly ColorSwatch m_SwatchElement;

        readonly UnityEngine.UIElements.TextField m_LabelElement;

        readonly Icon m_ColorPickerIcon;

        Color m_Value;

        Size m_Size;

        Type m_Type;

        Pressable m_Clickable;

        Color m_PreviousValue;

        ColorPicker m_InlineColorPicker;

        bool m_InlinePicker;

        ICustomColorFieldPicker m_CustomPicker;

        Func<Color, bool> m_ValidateValue;

        ColorPickerType m_ColorPickerType;

        bool m_ShowAlpha;

        bool m_Hdr;

        Popover m_Popover;

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

            m_LabelElement = new UnityEngine.UIElements.TextField
            {
                name = labelUssClassName,
                isReadOnly = true
            };
            m_LabelElement.RegisterCallback<PointerDownEvent>(OnTextFieldPointerDown);
            m_LabelElement.AddToClassList(labelUssClassName);

            m_ColorPickerIcon = new Icon
            {
                name = colorPickerIconUssClassName,
                iconName = "color-picker",
                pickingMode = PickingMode.Ignore,
            };
            m_ColorPickerIcon.AddToClassList(colorPickerIconUssClassName);

            hierarchy.Add(m_SwatchElement);
            hierarchy.Add(m_LabelElement);
            hierarchy.Add(m_ColorPickerIcon);

            size = Size.M;
            swatchSize = Size.S;
            showText = true;
            showAlpha = true;
            hdr = false;
            inlinePicker = false;
            customPicker = new DefaultColorFieldPicker();
            SetValueWithoutNotify(Color.clear);
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnTextFieldPointerDown(PointerDownEvent evt)
        {
            evt.StopPropagation();
        }

        void OnClick()
        {
            m_PreviousValue = m_Value;
            if (Application.isEditor && colorPickerType == ColorPickerType.UnityEditor)
            {
                AddToClassList(Styles.focusedUssClassName);
                OpenUnityEditorPicker();
            }
            else if (inlinePicker)
            {
                AddToClassList(Styles.focusedUssClassName);
                AddToClassList(inlinePickerOpenedUssClassName);
                ToggleInlinePicker();
            }
            else if (customPicker != null)
            {
                customPicker.Show(this, OnPickerValueChanged, m_PreviousValue, showAlpha, hdr);
            }
            else
                Debug.LogWarning("No ColorPicker available to open.");
        }

        void ToggleInlinePicker()
        {
            var wasOpened = m_InlineColorPicker != null && m_InlineColorPicker.parent == parent;
            m_InlineColorPicker?.parent?.Remove(m_InlineColorPicker);

            if (wasOpened)
            {
                RemoveFromClassList(Styles.focusedUssClassName);
                RemoveFromClassList(inlinePickerOpenedUssClassName);
                m_InlineColorPicker.UnregisterValueChangedCallback(OnPickerValueChanged);
                using var evt = ChangeEvent<Color>.GetPooled(m_PreviousValue, m_InlineColorPicker.value);
                SetValueWithoutNotify(m_InlineColorPicker.value);
                evt.target = this;
                SendEvent(evt);
                return;
            }

            m_InlineColorPicker ??= new ColorPicker
            {
                showAlpha = showAlpha,
                showHex = true,
                showToolbar = true,
                hdr = hdr,
            };
            m_InlineColorPicker.showAlpha = showAlpha;
            m_InlineColorPicker.hdr = hdr;
            m_InlineColorPicker.previousValue = m_PreviousValue;
            m_InlineColorPicker.SetValueWithoutNotify(m_PreviousValue);
            m_InlineColorPicker.RegisterValueChangedCallback(OnPickerValueChanged);

            m_InlineColorPicker.eyeDropperButton.clickable = new Pressable(m_InlineColorPicker.OnEyeDropperClicked);
            var idx = parent.IndexOf(this) + 1;
            parent.Insert(idx, m_InlineColorPicker);
        }

        void OpenUnityEditorPicker()
        {
            if (panel == null)
                return;

            if (panel.contextType == ContextType.Player)
            {
                Debug.LogWarning("UnityEditor ColorPicker is not available in Player context.");
                return;
            }

            ColorPickerExtensionsBridge.Show(OnPickerValueChanged, m_PreviousValue, showAlpha, hdr);
        }

        void OnPickerValueChanged(ChangeEvent<Color> e) => OnPickerValueChanged(e.newValue);

        void OnPickerValueChanged(Color color)
        {
            if (color != value)
            {
                SetValueWithoutNotify(color);
                using var evt = ChangingEvent<Color>.GetPooled();
                evt.previousValue = m_PreviousValue;
                evt.newValue = color;
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
        /// Clickable Manipulator for this ColorField.
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
        /// The ColorField color picker type.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ColorPickerType colorPickerType
        {
            get => m_ColorPickerType;
            set
            {
                var changed = m_ColorPickerType != value;
                m_ColorPickerType = value;

                if (changed)
                    NotifyPropertyChanged(in colorPickerTypeProperty);
            }
        }

        /// <summary>
        /// The ColorField size.
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
                m_ColorPickerIcon.size = m_Size.ToIconSize();

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The ColorField swatch size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size swatchSize
        {
            get => m_SwatchElement.size;
            set
            {
                var changed = m_SwatchElement.size != value;
                m_SwatchElement.size = value;

                if (changed)
                    NotifyPropertyChanged(in swatchSizeProperty);
            }
        }

        /// <summary>
        /// The ColorField type. When this is true, the ColorField will only show the swatch.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool swatchOnly
        {
            get => ClassListContains(swatchOnlyUssClassName);
            set
            {
                var changed = ClassListContains(swatchOnlyUssClassName) != value;
                EnableInClassList(swatchOnlyUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in swatchOnlyProperty);
            }
        }

        /// <summary>
        /// Whether to show the text label for the ColorField.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showText
        {
            get => ClassListContains(showTextUssClassName);
            set
            {
                var changed = ClassListContains(showTextUssClassName) != value;
                EnableInClassList(showTextUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in showTextProperty);
            }
        }

        /// <summary>
        /// The ColorPicker position relative to the ColorField. When this is true, the ColorPicker will be inlined
        /// instead of being displayed in a Popover.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool inlinePicker
        {
            get => m_InlinePicker;
            set
            {
                var changed = m_InlinePicker != value;
                m_InlinePicker = value;
                if (changed)
                    NotifyPropertyChanged(in inlinePickerProperty);
            }
        }

        /// <summary>
        /// The custom color picker for the ColorField.
        /// </summary>
        /// <example>
        /// <code>
        /// // Example implementation of a custom color picker
        /// public class CustomColorPicker : ICustomColorFieldPicker
        /// {
        ///     private VisualElement m_PickerWindow;
        ///     private Action&lt;Color&gt; m_OnValueChanging;
        ///     private ColorField m_Owner;
        ///     private Color m_InitialColor;
        ///
        ///     public void Show(ColorField owner, Action&lt;Color&gt; onValueChanging, Color initialColor, bool showAlpha, bool hdr)
        ///     {
        ///         m_Owner = owner;
        ///         m_OnValueChanging = onValueChanging;
        ///         m_InitialColor = initialColor;
        ///
        ///         // Create custom picker UI
        ///         m_PickerWindow = new VisualElement { name = "custom-picker" };
        ///
        ///         // Setup picker with callbacks
        ///         var picker = new MyCustomPickerUI(initialColor, showAlpha, hdr);
        ///
        ///         picker.onColorChanged += (color) =>
        ///         {
        ///             // Send ChangingEvent while picker is still open
        ///             m_OnValueChanging?.Invoke(color);
        ///         };
        ///
        ///         picker.onColorAccepted += (color) =>
        ///         {
        ///             // Send final ChangeEvent like the default implementation
        ///             using var evt = ChangeEvent&lt;Color&gt;.GetPooled(m_InitialColor, color);
        ///             evt.target = m_Owner;
        ///             m_Owner.SetValueWithoutNotify(color);
        ///             m_Owner.SendEvent(evt);
        ///             ClosePickerWindow();
        ///         };
        ///
        ///         picker.onCanceled += () =>
        ///         {
        ///             // Revert to initial color
        ///             m_OnValueChanging?.Invoke(m_InitialColor);
        ///             ClosePickerWindow();
        ///         };
        ///
        ///         m_PickerWindow.Add(picker);
        ///         owner.rootVisualElement.Add(m_PickerWindow);
        ///     }
        ///
        ///     private void ClosePickerWindow()
        ///     {
        ///         m_PickerWindow?.RemoveFromHierarchy();
        ///         m_OnValueChanging = null;
        ///         m_Owner?.Focus();
        ///     }
        /// }
        ///
        /// // Usage:
        /// var colorField = new ColorField();
        /// colorField.customPicker = new CustomColorPicker();
        /// </code>
        /// </example>
        [CreateProperty]
        public ICustomColorFieldPicker customPicker
        {
            get => m_CustomPicker;
            set
            {
                var changed = m_CustomPicker != value;
                m_CustomPicker = value;
                if (changed)
                    NotifyPropertyChanged(in customPickerProperty);
            }
        }

        /// <summary>
        /// The ColorField invalid state.
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
        /// The ColorField validation function.
        /// </summary>
        [CreateProperty]
        public Func<Color, bool> validateValue
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
        /// Sets the ColorField value without notifying the ColorField.
        /// </summary>
        /// <param name="newValue"> The new ColorField value. </param>
        public void SetValueWithoutNotify(Color newValue)
        {
            m_Value = newValue;
            m_LabelElement.SetValueWithoutNotify($"#{ColorExtensions.ColorToRgbaHex(m_Value, showAlpha)}");
            m_SwatchElement.color = m_Value;
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The ColorField value.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
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

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Whether to show the alpha channel in the ColorPicker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showAlpha
        {
            get => m_ShowAlpha;
            set
            {
                var changed = m_ShowAlpha != value;
                m_ShowAlpha = value;

                if (changed)
                    NotifyPropertyChanged(in showAlphaProperty);

                if (!m_ShowAlpha)
                    this.value = new Color(m_Value.r, m_Value.g, m_Value.b, 1);
            }
        }

        /// <summary>
        /// Whether to show the HDR colors in the ColorPicker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool hdr
        {
            get => m_Hdr;
            set
            {
                var changed = m_Hdr != value;
                m_Hdr = value;

                if (changed)
                    NotifyPropertyChanged(in hdrProperty);
            }
        }

        /// <summary>
        /// The default implementation of a custom ColorField picker.
        /// </summary>
        class DefaultColorFieldPicker : ICustomColorFieldPicker
        {
            ColorPicker m_Picker;
            Popover m_Popover;
            Action<Color> m_OnValueChanging;

            public void Show(ColorField owner, Action<Color> onValueChanging, Color initialColor, bool showAlpha, bool hdr)
            {
                owner.AddToClassList(Styles.focusedUssClassName);

                m_OnValueChanging = onValueChanging;
                m_Picker ??= new ColorPicker
                {
                    showAlpha = showAlpha,
                    showHex = true,
                    showToolbar = true,
                    hdr = hdr,
                };
                m_Picker.previousValue = initialColor;
                m_Picker.SetValueWithoutNotify(initialColor);
                m_Picker.RegisterValueChangedCallback(OnPickerValueChanged);
                m_Picker.eyeDropperButton.clickable = new Pressable(OnEyeDropperClicked);

                m_Popover = Popover.Build(owner, m_Picker);
                m_Popover.SetAnchor(owner);
                m_Popover.SetArrowVisible(false);
                m_Popover.SetMovable(true);
                m_Popover.SetLastFocusedElement(owner);
                m_Popover.SetPlacement(PopoverPlacement.End);
                m_Popover.SetShouldFlip(true);
                m_Popover.dismissed += (_, _) =>
                {
                    m_OnValueChanging = null;
                    owner.RemoveFromClassList(Styles.focusedUssClassName);
                    m_Picker.UnregisterValueChangedCallback(OnPickerValueChanged);
                    m_Picker.eyeDropperButton.clickable = null;
                    if (initialColor != m_Picker.value)
                    {
                        using var evt = ChangeEvent<Color>.GetPooled(initialColor, m_Picker.value);
                        evt.target = owner;
                        owner.SetValueWithoutNotify(m_Picker.value);
                        owner.SendEvent(evt);
                    }
                    m_Popover = null;
                    owner.Focus();
                };
                m_Popover.shown += popup =>
                {
                    popup.SetAnchor(null);
                };
                m_Popover.Show();
            }

            void OnEyeDropperClicked(EventBase evt)
            {
                if (m_Popover == null)
                    return;

                // hide the ColorPicker if it is open
                m_Popover.view.style.visibility= Visibility.Hidden;
                m_Picker.OnEyeDropperClicked(evt);
            }

            void OnPickerValueChanged(ChangeEvent<Color> e)
            {
                if (m_Popover != null && m_Popover.view.resolvedStyle.visibility != Visibility.Visible)
                {
                    m_Popover.view.style.visibility = Visibility.Visible;
                    // Ensure the popover is still listening for outside clicks
                    m_Popover.EnsureEventHandlersRegistered();
                }
                m_OnValueChanging?.Invoke(e.newValue);
            }
        }


    }
}
