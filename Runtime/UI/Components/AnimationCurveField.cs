using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Interface for the editor a <see cref="AnimationCurveField"/> opens when it is pressed.
    /// </summary>
    /// <remarks>
    /// Mirrors <see cref="ICustomColorFieldPicker"/>. There is no default implementation because
    /// UITK has no <c>CurveEditor</c> yet; until one lands, assign this to host the Editor's own
    /// curve editor, or leave it null and the field is a read-only curve thumbnail.
    /// </remarks>
    public interface ICustomAnimationCurveFieldPicker
    {
        /// <summary>
        /// Show the curve editor.
        /// </summary>
        /// <param name="owner"> The field that requested the editor. </param>
        /// <param name="onValueChanging"> Callback invoked as the user edits. Sends a <see cref="ChangingEvent{T}"/>. </param>
        /// <param name="initialValue"> The curve assigned when the editor opened. </param>
        void Show(AnimationCurveField owner, Action<AnimationCurve> onValueChanging, AnimationCurve initialValue);
    }

    /// <summary>
    /// A field that previews an <see cref="AnimationCurve"/> and opens a curve editor when pressed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The thumbnail is stroked with <see cref="Painter2D"/> straight into the element's mesh, so
    /// it needs neither a shader nor a render texture — unlike <see cref="ColorSwatch"/> and
    /// <see cref="CanvasBackground"/>, whose fills genuinely require one.
    /// </para>
    ///
    /// <para>
    /// The horizontal axis spans the curve's own time range and the vertical axis spans
    /// <c>[0, 1]</c>, widened to the curve's value range when the curve leaves that band, so a
    /// curve is never silently drawn clipped flat against an edge.
    /// </para>
    ///
    /// <para>
    /// Like <see cref="ColorField"/>, the field and its editor are separable: the editor arrives
    /// through <see cref="customPicker"/>. With no picker assigned the control is still a useful,
    /// correct curve display — which is why it ships ahead of the editor.
    /// </para>
    ///
    /// <para>
    /// The stroke color comes from the <c>--curve-color</c> custom property, which
    /// <c>AnimationCurveField.uss</c> binds to <c>--appui-alias-semantic-positive</c>.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var field = new AnimationCurveField { value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f) };
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:AnimationCurveField size="Md" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    public partial class AnimationCurveField : ExVisualElement, IInputElement<AnimationCurve>, INotifyValueChanging<AnimationCurve>, ISizeableElement, IPressable
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId invalidProperty = nameof(invalid);

        internal static readonly BindingId validateValueProperty = nameof(validateValue);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        internal static readonly BindingId customPickerProperty = nameof(customPicker);

        /// <summary>
        /// Number of samples used to stroke the thumbnail. Enough that an ease curve reads as
        /// smooth at field height without paying for a per-pixel walk.
        /// </summary>
        const int k_SampleCount = 64;

        const float k_DefaultStrokeWidth = 1.5f;

        static readonly CustomStyleProperty<Color> k_UssCurveColor = new CustomStyleProperty<Color>("--curve-color");

        static readonly CustomStyleProperty<float> k_UssCurveWidth = new CustomStyleProperty<float>("--curve-width");

        /// <summary>
        /// The AnimationCurveField main styling class.
        /// </summary>
        public const string ussClassName = "appui-animationcurvefield";

        /// <summary>
        /// The AnimationCurveField preview styling class.
        /// </summary>
        public const string previewUssClassName = ussClassName + "__preview";

        /// <summary>
        /// The AnimationCurveField size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly VisualElement m_Preview;

        AnimationCurve m_Value;

        AnimationCurve m_PreviousValue;

        Size m_Size;

        Pressable m_Clickable;

        ICustomAnimationCurveFieldPicker m_CustomPicker;

        Func<AnimationCurve, bool> m_ValidateValue;

        // Defaults are only fallbacks: USS supplies both through --curve-color / --curve-width.
        // --curve-width is deliberately unitless, because a CustomStyleProperty<float> silently
        // resolves a px length to 0 (see PORTING-GUIDE.md, failure mode 4).
        Color m_CurveColor = Color.white;

        float m_CurveWidth = k_DefaultStrokeWidth;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AnimationCurveField()
        {
            AddToClassList(ussClassName);

            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            passMask = 0;
            clickable = new Pressable(OpenPicker);

            m_Preview = new VisualElement
            {
                name = previewUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            m_Preview.AddToClassList(previewUssClassName);
            m_Preview.generateVisualContent += OnGenerateVisualContent;
            hierarchy.Add(m_Preview);

            // --curve-color and --curve-width are declared on the preview, and a custom property
            // only reaches the element whose matched rule declares it, so listen there. On the
            // field itself the stroke never left its white fallback.
            m_Preview.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            size = Size.Md;
            SetValueWithoutNotify(null);

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The element the curve is stroked into.
        /// </summary>
        public VisualElement previewElement => m_Preview;

        /// <summary>
        /// Clickable Manipulator for this AnimationCurveField.
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
        public ICustomAnimationCurveFieldPicker customPicker
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
        /// A function that validates the value of the AnimationCurveField.
        /// </summary>
        [CreateProperty]
        public Func<AnimationCurve, bool> validateValue
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
        /// <param name="newValue"> The new curve. </param>
        public void SetValueWithoutNotify(AnimationCurve newValue)
        {
            m_Value = newValue;
            m_Preview.MarkDirtyRepaint();
            if (validateValue != null) invalid = !validateValue(m_Value);
        }

        /// <summary>
        /// The current value of the AnimationCurveField.
        /// </summary>
        [CreateProperty]
        public AnimationCurve value
        {
            get => m_Value;
            set
            {
                if (m_Value == value)
                    return;
                using var evt = ChangeEvent<AnimationCurve>.GetPooled(m_Value, value);
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
            customPicker?.Show(this, OnPickerValueChanging, m_PreviousValue);
        }

        void OnPickerValueChanging(AnimationCurve newValue)
        {
            if (m_Value == newValue)
                return;

            SetValueWithoutNotify(newValue);
            using var evt = ChangingEvent<AnimationCurve>.GetPooled();
            evt.previousValue = m_PreviousValue;
            evt.newValue = newValue;
            evt.target = this;
            SendEvent(evt);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var changed = false;

            if (evt.customStyle.TryGetValue(k_UssCurveColor, out var color) && color != m_CurveColor)
            {
                m_CurveColor = color;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(k_UssCurveWidth, out var width) && !Mathf.Approximately(width, m_CurveWidth))
            {
                m_CurveWidth = width;
                changed = true;
            }

            if (changed)
                m_Preview.MarkDirtyRepaint();
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            var rect = m_Preview.contentRect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            var curve = m_Value;
            if (curve == null || curve.length == 0)
                return;

            GetValueRange(curve, out var minValue, out var maxValue);
            var span = maxValue - minValue;
            if (span <= Mathf.Epsilon)
                span = 1f;

            var firstTime = curve.keys[0].time;
            var lastTime = curve.keys[curve.length - 1].time;
            var timeSpan = lastTime - firstTime;

            var painter = mgc.painter2D;
            painter.lineWidth = m_CurveWidth;
            painter.strokeColor = m_CurveColor;
            painter.lineJoin = LineJoin.Round;
            painter.lineCap = LineCap.Butt;
            painter.BeginPath();

            for (var i = 0; i < k_SampleCount; i++)
            {
                var t = i / (float)(k_SampleCount - 1);
                var time = timeSpan > Mathf.Epsilon ? firstTime + t * timeSpan : firstTime;
                var v = curve.Evaluate(time);
                var x = rect.xMin + t * rect.width;
                var y = rect.yMax - (v - minValue) / span * rect.height;
                var point = new Vector2(x, y);

                if (i == 0)
                    painter.MoveTo(point);
                else
                    painter.LineTo(point);
            }

            painter.Stroke();
        }

        /// <summary>
        /// The vertical band the thumbnail maps onto: <c>[0, 1]</c>, widened to fit the curve when
        /// it leaves that band. Without the widening a curve of, say, 0..90 degrees would be drawn
        /// as a flat line pinned to the top edge, which reads as "the curve is broken".
        /// </summary>
        static void GetValueRange(AnimationCurve curve, out float min, out float max)
        {
            min = 0f;
            max = 1f;
            for (var i = 0; i < curve.length; i++)
            {
                var v = curve.keys[i].value;
                if (v < min) min = v;
                if (v > max) max = v;
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
    }
}
