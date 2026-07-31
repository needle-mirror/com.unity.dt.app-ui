using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The Avatar variant.
    /// </summary>
    public enum AvatarVariant
    {
        /// <summary>
        /// Display the Avatar component as a square.
        /// </summary>
        Square,

        /// <summary>
        /// Display the Avatar component as a rounded square.
        /// </summary>
        Rounded,

        /// <summary>
        /// Display the Avatar component as a circle.
        /// </summary>
        Circular,
    }

    /// <summary>
    /// Avatars are visual elements that represent a user, entity, or content through images or initials.
    /// </summary>
    /// <remarks>
    /// The Avatar component is a versatile UI element that displays either an image, initials, or fallback icon to
    /// represent a user or entity. It supports different shapes, sizes, and can be customized with outlines and
    /// background colors.
    ///
    /// **Tip:** Avatars can be used in various contexts such as user profiles, comment sections, or anywhere user
    /// representation is needed. They can be displayed individually or grouped together using the AvatarGroup
    /// component.
    ///
    /// **Note:** When using images in avatars, ensure they are properly cropped and centered to maintain visual
    /// consistency across your application.
    /// </remarks>
    /// <example>
    /// <para>Basic Avatar with Image — Creating a circular avatar with a profile image.</para>
    /// <code lang="csharp">
    /// var avatar = new Avatar();
    /// avatar.size = Size.M;
    /// avatar.variant = AvatarVariant.Circular;
    /// avatar.src = BackgroundExtensions.FromObject(Resources.Load("user-profile"));
    /// </code>
    /// <para>Styled Avatar with Outline — Creating a rounded avatar with background color and outline.</para>
    /// <code lang="csharp">
    /// var avatar = new Avatar();
    /// avatar.backgroundColor = Color.blue;
    /// avatar.outlineColor = Color.white;
    /// avatar.outlineWidth = 2f;
    /// avatar.variant = AvatarVariant.Rounded;
    /// </code>
    /// <para>UXML Declaration — Declaring an avatar in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <Avatar size="L" variant="Circular" src="profile-image" outline-color="white" outline-width="2" />
    /// ]]></code>
    /// </example>
    [VisualDocPage("feedbacks")]
    [UxmlElement]
    public partial class Avatar : BaseVisualElement, ISizeableElement
    {
        internal static readonly BindingId backgroundColorProperty = nameof(backgroundColor);

        internal static readonly BindingId outlineColorProperty = nameof(outlineColor);

        internal static readonly BindingId outlineWidthProperty = nameof(outlineWidth);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId srcProperty = nameof(src);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId labelColorProperty = nameof(labelColor);

        internal static readonly BindingId autoLabelColorProperty = nameof(autoLabelColor);

        /// <summary>
        /// The Avatar main styling class.
        /// </summary>
        public const string ussClassName = "appui-avatar";

        /// <summary>
        /// The Avatar container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Avatar size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Avatar variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(AvatarVariant))]
        public const string variantUssClassName = ussClassName + "--";

        const Size k_DefaultSize = Size.M;

        const AvatarVariant k_DefaultVariant = AvatarVariant.Circular;

        Size m_Size = Size.M;

        readonly TextElement m_Container;

        Optional<Color> m_BackgroundColor;

        Optional<Color> m_OutlineColor;

        Optional<float> m_OutlineWidth;

        Optional<Color> m_LabelColor;

        bool m_AutoLabelColor;

        AvatarVariant m_Variant;

        /// <summary>
        /// The content container of the Avatar.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The Avatar size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        [Header("Avatar")]
        public Size size
        {
            get => m_Size;
            set
            {
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));
            }
        }

        /// <summary>
        /// The Avatar variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AvatarVariant variant
        {
            get => m_Variant;
            set
            {
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));
            }
        }

        [UxmlAttribute("src")]
        Object srcTex
        {
            get => src.GetSelectedImage();
            set => src = BackgroundExtensions.FromObject(value);
        }

        /// <summary>
        /// The Avatar source image.
        /// </summary>
        [CreateProperty]
        public Background src
        {
            get => m_Container.style.backgroundImage.value;
            set
            {
                var changed = m_Container.style.backgroundImage.value != value;
                m_Container.style.backgroundImage = value;
                if (changed)
                    NotifyPropertyChanged(srcProperty);
            }
        }

        /// <summary>
        /// The Avatar background color.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> backgroundColor
        {
            get => m_BackgroundColor;
            set
            {
                m_BackgroundColor = value;
                m_Container.style.backgroundColor = m_BackgroundColor.IsSet ?
                    m_BackgroundColor.Value : new StyleColor(StyleKeyword.Null);
                UpdateLabelColor();
            }
        }

        /// <summary>
        /// The Avatar outline width.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> outlineWidth
        {
            get => m_OutlineWidth;
            set
            {
                m_OutlineWidth = value;

                var borderWidthStyle = m_OutlineWidth.IsSet ? m_OutlineWidth.Value : new StyleFloat(StyleKeyword.Null);
                style.borderBottomWidth = borderWidthStyle;
                style.borderLeftWidth = borderWidthStyle;
                style.borderRightWidth = borderWidthStyle;
                style.borderTopWidth = borderWidthStyle;
            }
        }

        /// <summary>
        /// The Avatar outline color.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> outlineColor
        {
            get => m_OutlineColor;
            set
            {
                m_OutlineColor = value;

                var colorStyle = m_OutlineColor.IsSet ? m_OutlineColor.Value : new StyleColor(StyleKeyword.Null);
                style.borderBottomColor = colorStyle;
                style.borderLeftColor = colorStyle;
                style.borderRightColor = colorStyle;
                style.borderTopColor = colorStyle;

                const float paddingValue = 1;

                var paddingStyle = m_OutlineColor.IsSet ? paddingValue : new StyleLength(StyleKeyword.Null);

                style.paddingBottom = paddingStyle;
                style.paddingLeft = paddingStyle;
                style.paddingRight = paddingStyle;
                style.paddingTop = paddingStyle;
            }
        }

        /// <summary>
        /// The text label displayed inside the Avatar container.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Container.text;
            set
            {
                var changed = m_Container.text != value;
                m_Container.text = value;
                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The color of the label text. When set, takes precedence over <see cref="autoLabelColor"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<Color> labelColor
        {
            get => m_LabelColor;
            set
            {
                var changed = !m_LabelColor.Equals(value);
                m_LabelColor = value;
                UpdateLabelColor();
                if (changed)
                    NotifyPropertyChanged(in labelColorProperty);
            }
        }

        /// <summary>
        /// When <c>true</c> and <see cref="labelColor"/> is not set, automatically computes
        /// the label color based on the luminance of <see cref="backgroundColor"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool autoLabelColor
        {
            get => m_AutoLabelColor;
            set
            {
                var changed = m_AutoLabelColor != value;
                m_AutoLabelColor = value;
                UpdateLabelColor();
                if (changed)
                    NotifyPropertyChanged(in autoLabelColorProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Avatar()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_Container = new TextElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            size = k_DefaultSize;
            variant = k_DefaultVariant;
            backgroundColor = Optional<Color>.none;
            outlineColor = Optional<Color>.none;
            outlineWidth = 2;
            src = new Background();
            labelColor = Optional<Color>.none;
            autoLabelColor = false;

            this.RegisterContextChangedCallback<AvatarVariantContext>(OnVariantContextChanged);
            this.RegisterContextChangedCallback<SizeContext>(OnSizeContextChanged);
        }

        void OnSizeContextChanged(ContextChangedEvent<SizeContext> evt)
        {
            if (evt.context != null)
                size = evt.context.size;
        }

        void OnVariantContextChanged(ContextChangedEvent<AvatarVariantContext> evt)
        {
            if (evt.context != null)
                variant = evt.context.variant;
        }

        void UpdateLabelColor()
        {
            if (m_LabelColor.IsSet)
            {
                m_Container.style.color = m_LabelColor.Value;
            }
            else if (m_AutoLabelColor && m_BackgroundColor.IsSet)
            {
                var bg = m_BackgroundColor.Value;
                var luminance = 0.2126f * bg.r + 0.7152f * bg.g + 0.0722f * bg.b;
                m_Container.style.color = luminance > 0.5f ? Color.black : Color.white;
            }
            else
            {
                m_Container.style.color = new StyleColor(StyleKeyword.Null);
            }
        }

    }
}
