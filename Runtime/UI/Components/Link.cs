using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The size of a <see cref="Link"/>.
    /// </summary>
    /// <remarks>
    /// Mirrors the registry's `linkVariants` size axis, which is `sm | base | lg` — deliberately
    /// narrower than <see cref="TextSize"/>. Every step here has a matching `--size-*` rule in
    /// `Link.uss`, so no value silently renders unstyled.
    /// </remarks>
    public enum LinkSize
    {
        /// <summary>
        /// Small — `--appui-font-sizes-100`.
        /// </summary>
        Sm,

        /// <summary>
        /// The default link size — `--appui-font-sizes-200`.
        /// </summary>
        Base,

        /// <summary>
        /// Large — `--appui-font-sizes-400`.
        /// </summary>
        Lg,
    }

    /// <summary>
    /// A clickable text element that can navigate to a URL or trigger an action.
    /// </summary>
    /// <remarks>
    /// The Link component is a fundamental navigation element that allows users to navigate to different URLs or
    /// trigger actions within your application. Links are interactive text elements that can be clicked, focused,
    /// and activated using keyboard navigation.
    ///
    /// Links can be styled with different text sizes to establish visual hierarchy and improve readability. They
    /// inherit text localization capabilities and support keyboard focus management for better accessibility.
    ///
    /// Links should be visually distinguishable from regular text to indicate their interactive nature. Use
    /// appropriate styling (like underlines or distinct colors) to make links recognizable.
    /// </remarks>
    /// <example>
    /// <para>Basic Usage. Simple link with URL.</para>
    /// <code lang="xml"><![CDATA[
    /// <Link text="Click me" url="https://example.com" />
    /// ]]></code>
    /// <para>Size Variants. Links with different sizes.</para>
    /// <code lang="xml"><![CDATA[
    /// <VerticalStack spacing="10">
    ///     <Link text="Small Link" size="Sm" />
    ///     <Link text="Default Link" size="Base" />
    ///     <Link text="Large Link" size="Lg" />
    /// </VerticalStack>
    /// ]]></code>
    /// <para>Custom Click Handler. Link with custom click behavior.</para>
    /// <code lang="csharp"><![CDATA[
    /// var link = new Link("Custom Action");
    /// link.clickable.clicked += () => {
    ///     // Custom action
    ///     Debug.Log("Link clicked!");
    /// };
    /// ]]></code>
    /// <para>Localized Link. Link using localized text.</para>
    /// <code lang="xml"><![CDATA[
    /// <Link text="@MyLocalizedText" url="https://example.com" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("actions")]
    public partial class Link : LocalizedTextElement, IPressable
    {

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId urlProperty = nameof(url);

        internal static readonly BindingId clickableProperty = nameof(clickable);


        /// <summary>
        /// The Link's USS class name.
        /// </summary>
        public new const string ussClassName = "appui-link";

        /// <summary>
        /// The Link's size USS class name.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(LinkSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        Pressable m_Clickable;

        LinkSize m_Size;

        string m_Url;

        /// <summary>
        /// The clickable manipulator.
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
        /// The size of the link.
        /// </summary>
        /// <remarks>
        /// `Base` by default, matching the registry's `defaultVariants: { size: 'base' }`. Each step
        /// binds to an `--appui-font-sizes-*` token in `Link.uss`, so links rescale with
        /// <see cref="Panel.density"/> like <see cref="Text"/> does.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public LinkSize size
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
        /// The URL of the link.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string url
        {
            get => m_Url;
            set
            {
                var changed = m_Url != value;
                m_Url = value;

                if (changed)
                    NotifyPropertyChanged(in urlProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Link() : this(null)
        {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The text of the link. </param>
        public Link(string text) : this(text, null)
        {}

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="text"> The text of the link. </param>
        /// <param name="url"> The URL of the link. </param>
        public Link(string text, string url) : base(text)
        {
            AddToClassList(ussClassName);

            clickable = new Pressable();
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            this.url = url;
            size = LinkSize.Base;

            this.AddManipulator(new KeyboardFocusController());
        }

    }
}
