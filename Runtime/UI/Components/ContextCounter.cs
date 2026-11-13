using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How full a model's context window is, as a compact pill.
    /// </summary>
    /// <remarks>
    /// A ring fills as the window does, next to a label and the percentage, with the exact
    /// "used / total (pct)" reading in the tooltip. Past <see cref="nearThreshold"/> the readout turns
    /// warning-coloured, which is the point at which a user needs to think about starting a new
    /// conversation.
    ///
    /// The ring is a determinate <see cref="CircularProgress"/> rather than a hand-drawn arc, so it
    /// follows the package's progress styling and density behaviour.
    /// </remarks>
    /// <example>
    /// <para>A window a little over half full.</para>
    /// <code lang="csharp">
    /// var counter = new ContextCounter { used = 120000, total = 200000 };
    /// parentElement.Add(counter);
    /// </code>
    /// <para>In UXML, close to full.</para>
    /// <code lang="xml">
    /// &lt;appui:ContextCounter used="190000" total="200000" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ContextCounter : BaseVisualElement
    {
        internal static readonly BindingId usedProperty = nameof(used);

        internal static readonly BindingId totalProperty = nameof(total);

        internal static readonly BindingId labelProperty = nameof(label);

        /// <summary>
        /// The ContextCounter main styling class.
        /// </summary>
        public const string ussClassName = "appui-context-counter";

        /// <summary>
        /// The styling class applied once the window is nearly full.
        /// </summary>
        public const string nearUssClassName = ussClassName + "--near";

        /// <summary>
        /// The styling class of the ring.
        /// </summary>
        public const string ringUssClassName = ussClassName + "__ring";

        /// <summary>
        /// The styling class of the leading label.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The styling class of the percentage.
        /// </summary>
        public const string percentUssClassName = ussClassName + "__percent";

        /// <summary>
        /// The default leading label.
        /// </summary>
        public const string defaultLabel = "Context";

        /// <summary>
        /// The fill ratio at which the counter starts warning.
        /// </summary>
        public const float nearThreshold = 0.85f;

        readonly CircularProgress m_Ring;

        readonly LocalizedTextElement m_Label;

        readonly LocalizedTextElement m_Percent;

        float m_Used;

        float m_Total = 1f;

        /// <summary>
        /// The ring the counter fills.
        /// </summary>
        public CircularProgress ring => m_Ring;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ContextCounter()
        {
            AddToClassList(ussClassName);

            m_Ring = new CircularProgress
            {
                name = ringUssClassName,
                variant = Progress.Variant.Determinate,
                size = Size.Sm,
                pickingMode = PickingMode.Ignore,
            };
            m_Ring.AddToClassList(ringUssClassName);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);

            m_Percent = new LocalizedTextElement { name = percentUssClassName, pickingMode = PickingMode.Ignore };
            m_Percent.AddToClassList(percentUssClassName);

            hierarchy.Add(m_Ring);
            hierarchy.Add(m_Label);
            hierarchy.Add(m_Percent);

            label = defaultLabel;
            Refresh();

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        // A pill needs a corner radius of exactly half its height. A fixed USS radius larger than
        // that is not reliable: UITK clamps the corners per axis, which draws the counter as an
        // ellipse, and no density-aware token is exactly half the counter's height in every density.
        // So the radius follows the laid-out height instead.
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var radius = new Length(Mathf.Max(0f, evt.newRect.height * 0.5f), LengthUnit.Pixel);
            if (style.borderTopLeftRadius == radius)
                return;

            style.borderTopLeftRadius = radius;
            style.borderTopRightRadius = radius;
            style.borderBottomRightRadius = radius;
            style.borderBottomLeftRadius = radius;
        }

        /// <summary>
        /// How much of the context window is in use.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float used
        {
            get => m_Used;
            set
            {
                var changed = !Mathf.Approximately(m_Used, value);
                m_Used = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in usedProperty);
            }
        }

        /// <summary>
        /// The size of the context window.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float total
        {
            get => m_Total;
            set
            {
                var changed = !Mathf.Approximately(m_Total, value);
                m_Total = value;
                Refresh();

                if (changed)
                    NotifyPropertyChanged(in totalProperty);
            }
        }

        /// <summary>
        /// The leading label. "Context" by default.
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

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// How full the window is, from 0 to 1. A non-positive <see cref="total"/> reads as empty
        /// rather than dividing by zero.
        /// </summary>
        public float ratio => m_Total > 0f ? Mathf.Clamp01(m_Used / m_Total) : 0f;

        /// <summary>
        /// How full the window is, as a whole percentage.
        /// </summary>
        /// <remarks>
        /// Rounded half up, like the registry's <c>Math.round</c>. <c>Mathf.RoundToInt</c> rounds a
        /// midpoint to even, so 20.5% would report 20 there and 21 on the web.
        /// </remarks>
        public int percent => Mathf.FloorToInt(ratio * 100f + 0.5f);

        /// <summary>
        /// Whether the window is close enough to full to warrant a warning.
        /// </summary>
        public bool near => ratio >= nearThreshold;

        void Refresh()
        {
            m_Ring.value = ratio;
            m_Percent.text = percent.ToString(CultureInfo.InvariantCulture) + "%";
            EnableInClassList(nearUssClassName, near);

            tooltip = ChatFormat.Compact(m_Used) + " / " + ChatFormat.Compact(m_Total) +
                " (" + percent.ToString(CultureInfo.InvariantCulture) + "%)";
        }
    }
}
