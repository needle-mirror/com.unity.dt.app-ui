using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Interface for the editor a <see cref="ColorGradientField"/> opens when it is pressed.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="ICustomColorFieldPicker"/>. There is no default implementation because
    /// UITK has no <c>GradientEditor</c> yet; until one lands, assign this to host the Editor's
    /// own gradient editor, or leave it null and the field is a read-only gradient well.
    /// </remarks>
    public interface ICustomColorGradientFieldPicker
    {
        /// <summary>
        /// Show the gradient editor.
        /// </summary>
        /// <param name="owner"> The field that requested the editor. </param>
        /// <param name="onValueChanging"> Callback invoked as the user edits. Sends a <see cref="ChangingEvent{T}"/>. </param>
        /// <param name="initialValue"> The gradient assigned when the editor opened. </param>
        /// <param name="hdr"> Whether the editor should allow HDR color keys. </param>
        void Show(ColorGradientField owner, Action<Gradient> onValueChanging, Gradient initialValue, bool hdr);
    }

    /// <summary>
    /// A field that previews a <see cref="Gradient"/> and opens a gradient editor when pressed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The preview is a full-bleed <see cref="ColorSwatch"/>, so the color × alpha composite over
    /// the transparency checkerboard is rendered by the same shader the rest of the color family
    /// uses rather than by a second render path.
    /// </para>
    ///
    /// <para>
    /// Like <see cref="ColorField"/>, the field and its editor are separable: the editor arrives
    /// through <see cref="customPicker"/>. With no picker assigned the control is still a useful,
    /// correct gradient display — which is why it ships ahead of the editor.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var gradient = new Gradient();
    /// gradient.SetKeys(
    ///     new[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.blue, 1f) },
    ///     new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
    /// var field = new ColorGradientField { value = gradient };
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:ColorGradientField size="Md" hdr="false" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    public partial class ColorGradientField : ExVisualElement, IInputElement<Gradient>, INotifyValueChanging<Gradient>, ISizeableElement, IPressable
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId hdrProperty = nameof(hdr);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId customPickerProperty = nameof(customPicker);

        /// <summary>
        /// The ColorGradientField main styling class.
        /// </summary>
        public const string ussClassName = "appui-colorgradientfield";

        /// <summary>
        /// The ColorGradientField swatch styling class.
        /// </summary>
        public const string swatchUssClassName = ussClassName + "__swatch";

        /// <summary>
        /// The ColorGradientField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly ColorSwatch m_SwatchElement;

        Gradient m_Value;

        Gradient m_PreviousValue;

        Size m_Size;

        bool m_Hdr;

        Pressable m_Clickable;

        ICustomColorGradientFieldPicker m_CustomPicker;

        Func<Gradient, bool> m_ValidateValue;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorGradientField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;
            clickable = new Pressable(OpenPicker);

            m_SwatchElement = new ColorSwatch
            {
                name = swatchUssClassName,
                pickingMode = PickingMode.Ignore,
                round = false,
            };
            m_SwatchElement.AddToClassList(swatchUssClassName);
            hierarchy.Add(m_SwatchElement);

            size = Size.Md;
            hdr = false;
            SetValueWithoutNotify(null);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The <see cref="ColorSwatch"/> that renders the preview.
        /// </summary>
        public ColorSwatch swatchElement => m_SwatchElement;

        /// <summary>
        /// Clickable Manipulator for this ColorGradientField.
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
        /// The editor opened when the field is pressed. Null by default.
        /// </summary>
        [CreateProperty]
        public ICustomColorGradientFieldPicker customPicker
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
        /// Raised when the user asks for the editor, whether or not a <see cref="customPicker"/>
        /// is assigned.
        /// </summary>
        public event Action pickRequested;

        /// <summary>
        /// The size of the element.
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
        /// Whether the editor should offer HDR color keys. Forwarded to
        /// <see cref="customPicker"/>; it does not change the preview.
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
        /// Whether the current value is valid, as determined by <see cref="validateValue"/>.
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
        /// A function that validates the value of the ColorGradientField.
        /// </summary>
        [CreateProperty]
        public Func<Gradient, bool> validateValue
        {
            get => m_ValidateValue;
            set
            {
                var changed = m_ValidateValue != value;
                m_ValidateValue = value;
                invalid = !m_ValidateValue?.Invoke(this.value) ?? false;
                if (changed)
                    NotifyPropertyChanged(in validateValueProperty);
            }
        }

        /// <summary>
        /// Sets the value without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new gradient. </param>
        public void SetValueWithoutNotify(Gradient newValue)
        {
            m_Value = newValue;
            m_SwatchElement.SetValueWithoutNotify(newValue);
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The current value of the ColorGradientField.
        /// </summary>
        [CreateProperty]
        public Gradient value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<Gradient>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Opens <see cref="customPicker"/>, as pressing the field does.
        /// </summary>
        public void OpenPicker()
        {
            m_PreviousValue = m_Value;
            pickRequested?.Invoke();
            customPicker?.Show(this, OnPickerValueChanging, m_PreviousValue, m_Hdr);
        }

        void OnPickerValueChanging(Gradient newValue)
        {
            if (m_Value == newValue)
                return;

            SetValueWithoutNotify(newValue);
            using var evt = ChangingEvent<Gradient>.GetPooled();
            evt.previousValue = m_PreviousValue;
            evt.newValue = newValue;
            evt.target = this;
            SendEvent(evt);
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }
    }
}
