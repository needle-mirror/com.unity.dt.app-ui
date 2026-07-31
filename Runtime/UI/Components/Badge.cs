using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The variant of the Badge.
    /// </summary>
    public enum BadgeVariant
    {
        /// <summary>
        /// The default variant. The Badge will contain some text.
        /// </summary>
        Default,

        /// <summary>
        /// The dot variant. The Badge will be a small dot.
        /// </summary>
        Dot
    }

    /// <summary>
    /// The overlap type of the Badge.
    /// </summary>
    public enum BadgeOverlapType
    {
        /// <summary>
        /// The Badge overlap type is rectangular.
        /// </summary>
        Rectangular,

        /// <summary>
        /// The Badge overlap type is circular.
        /// </summary>
        Circular
    }

    /// <summary>
    /// A horizontal anchor.
    /// </summary>
    public enum HorizontalAnchor
    {
        /// <summary>
        /// The element is anchored at the left.
        /// </summary>
        Left,

        /// <summary>
        /// The element is anchored at the right.
        /// </summary>
        Right
    }

    /// <summary>
    /// A vertical anchor.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum VerticalAnchor
    {
        /// <summary>
        /// The element is anchored at the top.
        /// </summary>
        Top,

        /// <summary>
        /// The element is anchored at the bottom.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// A small overlay element that displays numerical or status information.
    /// </summary>
    /// <remarks>
    /// The Badge component generates a small badge element that can be used to display counts, notifications, or
    /// status information. It's commonly used to show unread messages, notifications, or task counts on top of
    /// icons or other UI elements.
    ///
    /// Badges can be customized in various ways, including their position (using anchors), appearance (using
    /// variants), and overlap behavior. They can display numbers with an optional maximum value, or appear as
    /// simple dots for status indication.
    ///
    /// When using badges, ensure they provide meaningful information and don't overwhelm the user interface.
    /// Consider using the dot variant for simple status indicators and the default variant for numerical
    /// information.
    /// </remarks>
    /// <example>
    /// <para>Basic Badge Usage — Simple numerical badge.</para>
    /// <code lang="xml"><![CDATA[
    /// <Badge content="5" />
    /// ]]></code>
    /// <para>Status Indicator — Using dot variant as a status indicator.</para>
    /// <code lang="xml"><![CDATA[
    /// <Badge variant="Dot" background-color="#4CAF50" />
    /// ]]></code>
    /// <para>Notification Badge — Badge overlapping an icon to show notifications.</para>
    /// <code lang="xml"><![CDATA[
    /// <VisualElement>
    ///     <Icon name="notifications" />
    ///     <Badge content="3" overlap-type="Circular" horizontal-anchor="Right" vertical-anchor="Top" />
    /// </VisualElement>
    /// ]]></code>
    /// <para>Maximum Value Badge — Badge showing maximum value with overflow.</para>
    /// <code lang="xml"><![CDATA[
    /// <Badge content="1000" max="999" background-color="#FF4081" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class Badge : BaseVisualElement
    {

        internal static readonly BindingId backgroundColorProperty = nameof(backgroundColor);

        internal static readonly BindingId colorProperty = nameof(color);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId overlapTypeProperty = nameof(overlapType);

        internal static readonly BindingId horizontalAnchorProperty = nameof(horizontalAnchor);

        internal static readonly BindingId verticalAnchorProperty = nameof(verticalAnchor);

        internal static readonly BindingId labelProperty = nameof(label);

        /// <summary>
        /// The Badge main styling class.
        /// </summary>
        public const string ussClassName = "appui-badge";

        /// <summary>
        /// The Badge label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The Badge badge styling class.
        /// </summary>
        public const string badgeUssClassName = ussClassName + "__badge";

        /// <summary>
        /// The Badge Zero content styling class.
        /// </summary>
        public const string zeroUssClassName = ussClassName + "--zero";

        /// <summary>
        /// The Badge variant styling class prefix.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(BadgeVariant))]
        public const string variantClassName = ussClassName + "--";

        /// <summary>
        /// The Badge overlap type styling class prefix.
        /// </summary>
        [EnumName("GetOverlapUssClassName", typeof(BadgeOverlapType))]
        public const string overlapUssClassName = ussClassName + "--overlap-";

        /// <summary>
        /// The Badge horizontal anchor styling class prefix.
        /// </summary>
        [EnumName("GetHorizontalAnchorUssClassName", typeof(HorizontalAnchor))]
        public const string horizontalAnchorUssClassName = ussClassName + "--anchor-horizontal-";

        /// <summary>
        /// The Badge vertical anchor styling class prefix.
        /// </summary>
        [EnumName("GetVerticalAnchorUssClassName", typeof(VerticalAnchor))]
        public const string verticalAnchorUssClassName = ussClassName + "--anchor-vertical-";

        Optional<Color> m_BackgroundColor;

        BadgeVariant m_Variant;

        BadgeOverlapType m_BadgeOverlapType;

        HorizontalAnchor m_HorizontalAnchor;

        VerticalAnchor m_VerticalAnchor;

        readonly TextElement m_LabelElement;

        Optional<Color> m_Color;

        readonly VisualElement m_BadgeElement;

        /// <summary>
        /// The content container of the Badge.
        /// </summary>
        public override VisualElement contentContainer => m_LabelElement;

        /// <summary>
        /// The background color of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Badge")]
        public Optional<Color> backgroundColor
        {
            get => m_BackgroundColor;
            set
            {
                var changed = m_BackgroundColor != value;
                m_BackgroundColor = value;
                m_BadgeElement.style.backgroundColor = m_BackgroundColor.IsSet ?
                    m_BackgroundColor.Value : new StyleColor(StyleKeyword.Null);
                if (changed)
                    NotifyPropertyChanged(in backgroundColorProperty);
            }
        }

        /// <summary>
        /// The content color of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> color
        {
            get => m_Color;
            set
            {
                var changed = m_Color != value;
                m_Color = value;
                m_LabelElement.style.color = m_Color.IsSet ? m_Color.Value : new StyleColor(StyleKeyword.Null);
                if (changed)
                    NotifyPropertyChanged(in colorProperty);
            }
        }

        /// <summary>
        /// The variant of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public BadgeVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));
                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// The overlap type of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public BadgeOverlapType overlapType
        {
            get => m_BadgeOverlapType;
            set
            {
                var changed = m_BadgeOverlapType != value;
                RemoveFromClassList(GetOverlapUssClassName(m_BadgeOverlapType));
                m_BadgeOverlapType = value;
                AddToClassList(GetOverlapUssClassName(m_BadgeOverlapType));
                if (changed)
                    NotifyPropertyChanged(in overlapTypeProperty);
            }
        }

        /// <summary>
        /// The horizontal anchor of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public HorizontalAnchor horizontalAnchor
        {
            get => m_HorizontalAnchor;
            set
            {
                var changed = m_HorizontalAnchor != value;
                RemoveFromClassList(GetHorizontalAnchorUssClassName(m_HorizontalAnchor));
                m_HorizontalAnchor = value;
                AddToClassList(GetHorizontalAnchorUssClassName(m_HorizontalAnchor));
                if (changed)
                    NotifyPropertyChanged(in horizontalAnchorProperty);
            }
        }

        /// <summary>
        /// The vertical anchor of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public VerticalAnchor verticalAnchor
        {
            get => m_VerticalAnchor;
            set
            {
                var changed = m_VerticalAnchor != value;
                RemoveFromClassList(GetVerticalAnchorUssClassName(m_VerticalAnchor));
                m_VerticalAnchor = value;
                AddToClassList(GetVerticalAnchorUssClassName(m_VerticalAnchor));
                if (changed)
                    NotifyPropertyChanged(in verticalAnchorProperty);
            }
        }

        /// <summary>
        /// The textual content of the Badge.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                var changed = m_LabelElement.text != value;
                m_LabelElement.text = value;
                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Badge()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_BadgeElement = new VisualElement {name = badgeUssClassName, pickingMode = PickingMode.Ignore};
            m_BadgeElement.AddToClassList(badgeUssClassName);
            hierarchy.Add(m_BadgeElement);

            m_LabelElement = new TextElement { pickingMode = PickingMode.Ignore, name = labelUssClassName };
            m_LabelElement.AddToClassList(labelUssClassName);
            m_BadgeElement.Add(m_LabelElement);

            backgroundColor = Optional<Color>.none;
            color = Optional<Color>.none;
            variant = BadgeVariant.Default;
            overlapType = BadgeOverlapType.Rectangular;
            horizontalAnchor = HorizontalAnchor.Right;
            verticalAnchor = VerticalAnchor.Top;
        }

    }
}
