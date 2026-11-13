using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The lifecycle of one <see cref="ChatAssetRow"/>.
    /// </summary>
    public enum AssetRowState
    {
        /// <summary>
        /// Nothing has happened yet.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The item was successfully added.
        /// </summary>
        Added,

        /// <summary>
        /// Nothing was added, but not because of an error — the item already existed, for instance.
        /// <see cref="ChatAssetRow.statusText"/> explains why.
        /// </summary>
        Skipped,

        /// <summary>
        /// The row's last action failed. <see cref="ChatAssetRow.statusText"/> explains why.
        /// </summary>
        Failed
    }

    /// <summary>
    /// How a <see cref="ChatAssetRow"/>'s status line reads.
    /// </summary>
    public enum AssetRowStatusTone
    {
        /// <summary>
        /// An ordinary note.
        /// </summary>
        Muted = 0,

        /// <summary>
        /// Something went wrong.
        /// </summary>
        Destructive
    }

    /// <summary>
    /// Everything a chat surface shows about one asset.
    /// </summary>
    /// <remarks>
    /// A thumbnail on the left, an identity block on the right: title, source, an optional pack, an
    /// optional details link, an optional creator, an optional two-line description, optional tags, an
    /// optional price, an optional size and an optional status line, then an optional consumer
    /// <see cref="extra"/> element. Every optional line hides itself when it has nothing to say, so the
    /// row never leaves a gap.
    ///
    /// The row deliberately has no list semantics, no click behaviour of its own and no trailing
    /// control. A caller renders it inside its own list item and owns the interaction around it.
    ///
    /// <see cref="statusTone"/> is how the caller says a status counts as a problem, but the row owns
    /// which token expresses that, so the mapping lives in one place and a consumer cannot inject
    /// arbitrary styling. <see cref="state"/> is separate on purpose: it is the row's lifecycle, and
    /// tone is how the caller wants that read.
    /// </remarks>
    /// <example>
    /// <para>A row that failed.</para>
    /// <code lang="csharp">
    /// var row = new ChatAssetRow
    /// {
    ///     title = "Sci-Fi Crate",
    ///     sourceLabel = "Asset Store",
    ///     packLabel = "Sci-Fi Props Vol. 2",
    ///     priceLabel = "$12",
    ///     state = AssetRowState.Failed,
    ///     statusText = "Download failed",
    ///     statusTone = AssetRowStatusTone.Destructive
    /// };
    /// parentElement.Add(row);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatAssetRow title="Sci-Fi Crate" source-label="Asset Store" state="Added" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatAssetRow : BaseVisualElement
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId sourceLabelProperty = nameof(sourceLabel);

        internal static readonly BindingId packLabelProperty = nameof(packLabel);

        internal static readonly BindingId creatorLabelProperty = nameof(creatorLabel);

        internal static readonly BindingId priceLabelProperty = nameof(priceLabel);

        internal static readonly BindingId statusTextProperty = nameof(statusText);

        internal static readonly BindingId statusToneProperty = nameof(statusTone);

        internal static readonly BindingId stateProperty = nameof(state);

        internal static readonly BindingId thumbnailProperty = nameof(thumbnail);

        internal static readonly BindingId fallbackIconProperty = nameof(fallbackIcon);

        internal static readonly BindingId mediaProperty = nameof(media);

        internal static readonly BindingId creatorLinkProperty = nameof(creatorLink);

        internal static readonly BindingId detailsProperty = nameof(details);

        internal static readonly BindingId sizeLabelProperty = nameof(sizeLabel);

        internal static readonly BindingId descriptionProperty = nameof(description);

        internal static readonly BindingId tagsProperty = nameof(tags);

        internal static readonly BindingId extraProperty = nameof(extra);

        /// <summary>
        /// The ChatAssetRow main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-asset-row";

        /// <summary>
        /// The ChatAssetRow lifecycle styling class.
        /// </summary>
        [EnumName("GetStateUssClassName", typeof(AssetRowState))]
        public const string stateUssClassName = ussClassName + "--state-";

        /// <summary>
        /// The ChatAssetRow status tone styling class.
        /// </summary>
        [EnumName("GetStatusToneUssClassName", typeof(AssetRowStatusTone))]
        public const string statusToneUssClassName = ussClassName + "__status--";

        /// <summary>
        /// The styling class of the thumbnail.
        /// </summary>
        public const string thumbnailUssClassName = ussClassName + "__thumbnail";

        /// <summary>
        /// The styling class of a caller-supplied medium standing in for the thumbnail.
        /// </summary>
        public const string mediaUssClassName = ussClassName + "__media";

        /// <summary>
        /// The styling class of the thumbnail's placeholder glyph.
        /// </summary>
        public const string thumbnailPlaceholderUssClassName = ussClassName + "__thumbnail-placeholder";

        /// <summary>
        /// The styling class of the identity block.
        /// </summary>
        public const string identityUssClassName = ussClassName + "__identity";

        /// <summary>
        /// The styling class of the title.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the source line.
        /// </summary>
        public const string sourceUssClassName = ussClassName + "__source";

        /// <summary>
        /// The styling class of the source label.
        /// </summary>
        public const string sourceLabelUssClassName = ussClassName + "__source-label";

        /// <summary>
        /// The styling class of the separator between the source and the pack.
        /// </summary>
        public const string separatorUssClassName = ussClassName + "__separator";

        /// <summary>
        /// The styling class of the pack label.
        /// </summary>
        public const string packLabelUssClassName = ussClassName + "__pack-label";

        /// <summary>
        /// The styling class of the details link.
        /// </summary>
        public const string detailsButtonUssClassName = ussClassName + "__details-button";

        /// <summary>
        /// The styling class of the creator line.
        /// </summary>
        public const string creatorUssClassName = ussClassName + "__creator";

        /// <summary>
        /// The styling class applied to the creator label while it is offered as a link.
        /// </summary>
        public const string creatorLinkUssClassName = creatorUssClassName + "--link";

        /// <summary>
        /// The styling class of the price line.
        /// </summary>
        public const string priceUssClassName = ussClassName + "__price";

        /// <summary>
        /// The styling class of the status line.
        /// </summary>
        public const string statusUssClassName = ussClassName + "__status";

        /// <summary>
        /// The styling class of the size line.
        /// </summary>
        public const string sizeUssClassName = ussClassName + "__size";

        /// <summary>
        /// The styling class of the description, clamped to two lines.
        /// </summary>
        public const string descriptionUssClassName = ussClassName + "__description";

        /// <summary>
        /// The styling class of the strip holding the tags.
        /// </summary>
        public const string tagsUssClassName = ussClassName + "__tags";

        /// <summary>
        /// The styling class of one tag pill.
        /// </summary>
        public const string tagUssClassName = ussClassName + "__tag";

        /// <summary>
        /// The styling class applied to the consumer's <see cref="extra"/> element.
        /// </summary>
        public const string extraUssClassName = ussClassName + "__extra";

        /// <summary>
        /// The separator drawn between the source and the pack.
        /// </summary>
        public const string separator = "·";

        readonly VisualElement m_Thumbnail;

        readonly Icon m_ThumbnailPlaceholder;

        readonly LocalizedTextElement m_Title;

        readonly LocalizedTextElement m_SourceLabel;

        readonly LocalizedTextElement m_Separator;

        readonly LocalizedTextElement m_PackLabel;

        readonly IconButton m_DetailsButton;

        readonly LocalizedTextElement m_CreatorLabel;

        readonly LocalizedTextElement m_PriceLabel;

        readonly LocalizedTextElement m_StatusText;

        AssetRowState m_State;

        AssetRowStatusTone m_StatusTone;

        bool m_Details;

        bool m_CreatorLink;

        VisualElement m_Media;

        Pressable m_CreatorPressable;

        readonly VisualElement m_Identity;

        readonly LocalizedTextElement m_Description;

        readonly VisualElement m_Tags;

        readonly LocalizedTextElement m_SizeLabel;

        List<string> m_TagsData = new List<string>();

        VisualElement m_Extra;

        float m_DescriptionClamp = -1f;

        /// <summary>
        /// Raised when the details link is activated.
        /// </summary>
        public event Action detailsRequested;

        /// <summary>
        /// Raised when the creator label is activated, while <see cref="creatorLink"/> is on.
        /// </summary>
        /// <remarks>
        /// The registry names the creator's page with a <c>creatorUrl</c> and has to check that it
        /// is an absolute http(s) URL before turning it into an <c>href</c>, because a bare catalog
        /// path would resolve against the host app's own origin and lead nowhere. There is no
        /// <c>href</c> here, so there is nothing to guard: the row reports the intent and the host
        /// decides where, and whether, to go. Same shape as
        /// <see cref="detailsRequested"/>.
        /// </remarks>
        public event Action creatorRequested;

        /// <summary>
        /// The details link.
        /// </summary>
        public IconButton detailsButton => m_DetailsButton;

        /// <summary>
        /// The manipulator that turns the creator label into a link, or null while
        /// <see cref="creatorLink"/> is off.
        /// </summary>
        internal Pressable creatorPressable => m_CreatorPressable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatAssetRow()
        {
            AddToClassList(ussClassName);

            m_Thumbnail = new VisualElement { name = thumbnailUssClassName, pickingMode = PickingMode.Ignore };
            m_Thumbnail.AddToClassList(thumbnailUssClassName);

            m_ThumbnailPlaceholder = new Icon
            {
                name = thumbnailPlaceholderUssClassName,
                iconName = "cube",
                size = IconSize.Sm,
                pickingMode = PickingMode.Ignore,
            };
            m_ThumbnailPlaceholder.AddToClassList(thumbnailPlaceholderUssClassName);
            m_Thumbnail.hierarchy.Add(m_ThumbnailPlaceholder);

            var identity = m_Identity = new VisualElement { name = identityUssClassName, pickingMode = PickingMode.Ignore };
            identity.AddToClassList(identityUssClassName);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            var source = new VisualElement { name = sourceUssClassName };
            source.AddToClassList(sourceUssClassName);

            m_SourceLabel = new LocalizedTextElement { name = sourceLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_SourceLabel.AddToClassList(sourceLabelUssClassName);

            m_Separator = new LocalizedTextElement
            {
                name = separatorUssClassName,
                text = separator,
                pickingMode = PickingMode.Ignore,
            };
            m_Separator.AddToClassList(separatorUssClassName);

            m_PackLabel = new LocalizedTextElement { name = packLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_PackLabel.AddToClassList(packLabelUssClassName);

            m_DetailsButton = new IconButton
            {
                name = detailsButtonUssClassName,
                icon = "arrow-up-right",
                quiet = true,
                size = Size.Sm,
            };
            m_DetailsButton.AddToClassList(detailsButtonUssClassName);
            m_DetailsButton.clicked += () => detailsRequested?.Invoke();

            source.hierarchy.Add(m_SourceLabel);
            source.hierarchy.Add(m_Separator);
            source.hierarchy.Add(m_PackLabel);
            source.hierarchy.Add(m_DetailsButton);

            m_CreatorLabel = new LocalizedTextElement { name = creatorUssClassName, pickingMode = PickingMode.Ignore };
            m_CreatorLabel.AddToClassList(creatorUssClassName);

            m_PriceLabel = new LocalizedTextElement { name = priceUssClassName, pickingMode = PickingMode.Ignore };
            m_PriceLabel.AddToClassList(priceUssClassName);

            m_StatusText = new LocalizedTextElement { name = statusUssClassName, pickingMode = PickingMode.Ignore };
            m_StatusText.AddToClassList(statusUssClassName);

            m_Description = new LocalizedTextElement { name = descriptionUssClassName, pickingMode = PickingMode.Ignore };
            m_Description.AddToClassList(descriptionUssClassName);
            m_Description.RegisterCallback<GeometryChangedEvent>(OnDescriptionGeometryChanged);
            // A density or theme change can move the font size without moving the clamped box, which
            // then raises no geometry event; the size is published as a custom property so the change
            // still reaches the clamp.
            m_Description.RegisterCallback<CustomStyleResolvedEvent>(OnDescriptionStyleResolved);

            m_Tags = new VisualElement { name = tagsUssClassName, pickingMode = PickingMode.Ignore };
            m_Tags.AddToClassList(tagsUssClassName);

            m_SizeLabel = new LocalizedTextElement { name = sizeUssClassName, pickingMode = PickingMode.Ignore };
            m_SizeLabel.AddToClassList(sizeUssClassName);

            // The registry's order: title, source, creator, description, tags, price, size, status, and
            // the consumer's extra last.
            identity.hierarchy.Add(m_Title);
            identity.hierarchy.Add(source);
            identity.hierarchy.Add(m_CreatorLabel);
            identity.hierarchy.Add(m_Description);
            identity.hierarchy.Add(m_Tags);
            identity.hierarchy.Add(m_PriceLabel);
            identity.hierarchy.Add(m_SizeLabel);
            identity.hierarchy.Add(m_StatusText);

            hierarchy.Add(m_Thumbnail);
            hierarchy.Add(identity);

            packLabel = null;
            creatorLabel = null;
            priceLabel = null;
            sizeLabel = null;
            description = null;
            RefreshTags();
            statusText = null;
            details = false;
            state = AssetRowState.Pending;
            statusTone = AssetRowStatusTone.Muted;
            thumbnail = default;
        }

        /// <summary>
        /// The asset's name.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title.text;
            set
            {
                var changed = m_Title.text != value;
                m_Title.text = value;

                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// Where the asset came from, for instance a catalog name. Always shown.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string sourceLabel
        {
            get => m_SourceLabel.text;
            set
            {
                var changed = m_SourceLabel.text != value;
                m_SourceLabel.text = value;

                if (changed)
                    NotifyPropertyChanged(in sourceLabelProperty);
            }
        }

        /// <summary>
        /// The pack the asset belongs to. Null or empty hides it, and the separator with it.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string packLabel
        {
            get => m_PackLabel.text;
            set
            {
                var changed = m_PackLabel.text != value;
                m_PackLabel.text = value;
                var empty = string.IsNullOrEmpty(value);
                m_PackLabel.EnableInClassList(Styles.hiddenUssClassName, empty);
                m_Separator.EnableInClassList(Styles.hiddenUssClassName, empty);

                if (changed)
                    NotifyPropertyChanged(in packLabelProperty);
            }
        }

        /// <summary>
        /// Who made the asset. Null or empty hides the line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string creatorLabel
        {
            get => m_CreatorLabel.text;
            set
            {
                var changed = m_CreatorLabel.text != value;
                m_CreatorLabel.text = value;
                m_CreatorLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in creatorLabelProperty);
            }
        }

        /// <summary>
        /// A pre-formatted commercial price, for instance "$12" or "Free". The row does no currency
        /// formatting of its own. Null or empty hides the line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string priceLabel
        {
            get => m_PriceLabel.text;
            set
            {
                var changed = m_PriceLabel.text != value;
                m_PriceLabel.text = value;
                m_PriceLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in priceLabelProperty);
            }
        }

        /// <summary>
        /// Why the row is in the state it is in. Null or empty hides the line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string statusText
        {
            get => m_StatusText.text;
            set
            {
                var changed = m_StatusText.text != value;
                m_StatusText.text = value;
                m_StatusText.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in statusTextProperty);
            }
        }

        /// <summary>
        /// Where the row is in its lifecycle. <see cref="AssetRowState.Pending"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AssetRowState state
        {
            get => m_State;
            set
            {
                var changed = m_State != value;
                RemoveFromClassList(GetStateUssClassName(m_State));
                m_State = value;
                AddToClassList(GetStateUssClassName(m_State));

                if (changed)
                    NotifyPropertyChanged(in stateProperty);
            }
        }

        /// <summary>
        /// How the status line reads. <see cref="AssetRowStatusTone.Muted"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public AssetRowStatusTone statusTone
        {
            get => m_StatusTone;
            set
            {
                var changed = m_StatusTone != value;
                m_StatusText.RemoveFromClassList(GetStatusToneUssClassName(m_StatusTone));
                m_StatusTone = value;
                m_StatusText.AddToClassList(GetStatusToneUssClassName(m_StatusTone));

                if (changed)
                    NotifyPropertyChanged(in statusToneProperty);
            }
        }

        /// <summary>
        /// Whether the details link is offered. False by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool details
        {
            get => m_Details;
            set
            {
                var changed = m_Details != value;
                m_Details = value;
                m_DetailsButton.EnableInClassList(Styles.hiddenUssClassName, !m_Details);

                if (changed)
                    NotifyPropertyChanged(in detailsProperty);
            }
        }

        /// <summary>
        /// Whether the creator label is offered as a link. False by default.
        /// </summary>
        /// <remarks>
        /// Off, the label is plain text, so a source that knows a name but no page still shows the
        /// name. On, activating it raises <see cref="creatorRequested"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool creatorLink
        {
            get => m_CreatorLink;
            set
            {
                var changed = m_CreatorLink != value;
                m_CreatorLink = value;
                RefreshCreatorLink();

                if (changed)
                    NotifyPropertyChanged(in creatorLinkProperty);
            }
        }

        /// <summary>
        /// The name of the symbol shown when there is no thumbnail to draw. Defaults to a generic
        /// asset cube.
        /// </summary>
        /// <remarks>
        /// A consumer that knows what kind of thing the row describes passes its own symbol, so an
        /// item with no picture still reads as what it is. Named, not styled, by the consumer: the
        /// size and colour stay here so every row's placeholder matches.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string fallbackIcon
        {
            get => m_ThumbnailPlaceholder.iconName;
            set
            {
                var next = string.IsNullOrEmpty(value) ? "cube" : value;
                var changed = m_ThumbnailPlaceholder.iconName != next;
                m_ThumbnailPlaceholder.iconName = next;

                if (changed)
                    NotifyPropertyChanged(in fallbackIconProperty);
            }
        }

        /// <summary>
        /// An element rendered in place of the thumbnail.
        /// </summary>
        /// <remarks>
        /// Lets a consumer supply a richer medium for rows whose content is not an image -- an
        /// <see cref="AudioPreview"/> for a sound, say -- without this row knowing what that medium
        /// is. Setting it back to null restores the thumbnail.
        /// </remarks>
        [CreateProperty]
        public VisualElement media
        {
            get => m_Media;
            set
            {
                if (m_Media == value)
                    return;

                m_Media?.RemoveFromHierarchy();
                m_Media = value;

                if (m_Media != null)
                {
                    m_Media.AddToClassList(mediaUssClassName);
                    hierarchy.Insert(0, m_Media);
                }

                m_Thumbnail.EnableInClassList(Styles.hiddenUssClassName, m_Media != null);
                NotifyPropertyChanged(in mediaProperty);
            }
        }

        /// <summary>
        /// The asset's thumbnail. Unset, the row shows a cube placeholder instead of a broken image.
        /// </summary>
        [CreateProperty]
        public Background thumbnail
        {
            get => m_Thumbnail.style.backgroundImage.value;
            set
            {
                var changed = m_Thumbnail.style.backgroundImage.value != value;
                m_Thumbnail.style.backgroundImage = value;

                // An unset thumbnail shows the cube placeholder rather than an empty box.
                var empty = value.texture == null && value.sprite == null &&
                    value.vectorImage == null && value.renderTexture == null;
                m_ThumbnailPlaceholder.EnableInClassList(Styles.hiddenUssClassName, !empty);

                if (changed)
                    NotifyPropertyChanged(in thumbnailProperty);
            }
        }


        /// <summary>
        /// A pre-formatted size or cost of the item, for instance "≈ 48 MB". The row does no formatting
        /// of its own. Null or empty hides the line.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string sizeLabel
        {
            get => m_SizeLabel.text;
            set
            {
                var changed = m_SizeLabel.text != value;
                m_SizeLabel.text = value;
                m_SizeLabel.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in sizeLabelProperty);
            }
        }

        /// <summary>
        /// A short text describing the item, such as what it was made from. Clamped to two lines, so a
        /// long one never pushes the rows below out of view. Null or empty hides the line.
        /// </summary>
        /// <remarks>
        /// USS has neither <c>line-clamp</c> nor <c>line-height</c>, so the row measures two lines of
        /// the description's own text style and caps the element at that height. The cap follows the
        /// font size the tokens resolve to, so it holds at every density.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string description
        {
            get => m_Description.text;
            set
            {
                var changed = m_Description.text != value;
                m_Description.text = value;
                m_Description.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in descriptionProperty);
            }
        }

        /// <summary>
        /// Short facts shown as tags under the description, for instance a format or a length. Null or
        /// empty hides the strip.
        /// </summary>
        /// <remarks>
        /// The registry draws each tag as a secondary <c>Badge</c>. UI Toolkit's <see cref="Badge"/> is a
        /// notification overlay rather than a pill, so the row draws its own pills, styled from the
        /// tokens the registry's secondary badge reads. The setter copies the incoming list, so a caller
        /// mutating its own list afterwards does not silently desynchronise what is on screen.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public List<string> tags
        {
            get => m_TagsData;
            set
            {
                var next = value ?? new List<string>();
                var changed = !m_TagsData.SequenceEqual(next);
                m_TagsData = new List<string>(next);
                RefreshTags();

                if (changed)
                    NotifyPropertyChanged(in tagsProperty);
            }
        }

        /// <summary>
        /// An element rendered under the row's facts, after the status line: a control the consumer adds
        /// for this item, such as an editor for its description. The row stays a plain row; the consumer
        /// owns what the control does. Setting it back to null removes it.
        /// </summary>
        [CreateProperty]
        public VisualElement extra
        {
            get => m_Extra;
            set
            {
                if (m_Extra == value)
                    return;

                if (m_Extra != null)
                {
                    m_Extra.RemoveFromClassList(extraUssClassName);
                    m_Extra.RemoveFromHierarchy();
                }

                m_Extra = value;

                if (m_Extra != null)
                {
                    m_Extra.AddToClassList(extraUssClassName);
                    m_Identity.hierarchy.Add(m_Extra);
                }

                NotifyPropertyChanged(in extraProperty);
            }
        }

        void RefreshTags()
        {
            m_Tags.Clear();
            foreach (var tag in m_TagsData)
            {
                var pill = new LocalizedTextElement { text = tag, pickingMode = PickingMode.Ignore };
                pill.AddToClassList(tagUssClassName);
                m_Tags.Add(pill);
            }

            m_Tags.EnableInClassList(Styles.hiddenUssClassName, m_TagsData.Count == 0);
        }

        void OnDescriptionGeometryChanged(GeometryChangedEvent evt) => UpdateDescriptionClamp();

        void OnDescriptionStyleResolved(CustomStyleResolvedEvent evt) => UpdateDescriptionClamp();

        void UpdateDescriptionClamp()
        {
            // Two lines of this element's own resolved text style: the measure follows the font size the
            // tokens give it, so the clamp holds at every density without a magic height.
            var twoLines = m_Description.MeasureTextSize("Mg\nMg", 0, MeasureMode.Undefined, 0, MeasureMode.Undefined).y;
            if (twoLines <= 0f || float.IsNaN(twoLines))
                return;

            if (UnityEngine.Mathf.Approximately(m_DescriptionClamp, twoLines))
                return;

            m_DescriptionClamp = twoLines;
            m_Description.style.maxHeight = twoLines;
        }

        void RefreshCreatorLink()
        {
            m_CreatorLabel.EnableInClassList(creatorLinkUssClassName, m_CreatorLink);
            m_CreatorLabel.pickingMode = m_CreatorLink ? PickingMode.Position : PickingMode.Ignore;

            if (m_CreatorLink && m_CreatorPressable == null)
            {
                m_CreatorPressable = new Pressable(() => creatorRequested?.Invoke());
                m_CreatorLabel.AddManipulator(m_CreatorPressable);
            }
            else if (!m_CreatorLink && m_CreatorPressable != null)
            {
                m_CreatorLabel.RemoveManipulator(m_CreatorPressable);
                m_CreatorPressable = null;
            }
        }
    }
}
