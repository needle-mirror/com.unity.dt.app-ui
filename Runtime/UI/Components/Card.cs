using System;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A raised surface that groups a piece of content into a single, self-contained tile.
    /// </summary>
    /// <remarks>
    /// Card is a compound component. It is a plain surface on its own and gets its structure from the
    /// sub-elements you compose into it, in this order:
    /// <see cref="CardMedia"/>, <see cref="CardHeader"/> (holding a <see cref="CardTitle"/> and a
    /// <see cref="CardDescription"/>), <see cref="CardContent"/> and <see cref="CardFooter"/>.
    /// Every part is optional.
    ///
    /// Two state flags drive its appearance:
    /// <see cref="interactive"/> turns the whole card into a focusable, clickable target — the pointer
    /// becomes a hand, hover tints the surface and a submit key press fires <see cref="clicked"/>.
    /// <see cref="selected"/> outlines the card with the accent colour, for single- or multi-select
    /// tile pickers. The two are usually paired.
    ///
    /// <see cref="elevation"/> is off by default so the card reads as a flat raised surface. Raising it
    /// applies the shared App UI elevation ramp (the same `appui-elevation-*` classes and
    /// `--appui-elevations-*` tokens the Drawer and the FAB use) and enables the outset shadow pass.
    ///
    /// **Tip:** to lay several cards out as a responsive gallery, drop them into a
    /// <see cref="CardGrid"/> rather than positioning them by hand.
    /// </remarks>
    /// <example>
    /// <para>A static content card composed in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:Card>
    ///     <appui:CardHeader>
    ///         <appui:CardTitle text="Third Person Template" />
    ///         <appui:CardDescription text="A ready-made character controller." />
    ///     </appui:CardHeader>
    ///     <appui:CardContent>
    ///         <appui:Text text="Universal Render Pipeline - 2.4 GB" />
    ///     </appui:CardContent>
    ///     <appui:CardFooter>
    ///         <appui:Button title="Open" />
    ///     </appui:CardFooter>
    /// </appui:Card>
    /// ]]></code>
    /// <para>A selectable template tile, built in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var card = new Card { interactive = true };
    /// card.Add(new CardMedia { image = thumbnail });
    ///
    /// var header = new CardHeader();
    /// header.Add(new CardTitle { text = "2D Platformer" });
    /// header.Add(new CardDescription { text = "Tilemaps, physics and a sample level." });
    /// card.Add(header);
    ///
    /// card.clicked += () => card.selected = !card.selected;
    /// ]]></code>
    /// <para>A raised, media-topped card.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:Card interactive="true" elevation="2">
    ///     <appui:CardMedia style="height: 120px;" />
    ///     <appui:CardHeader>
    ///         <appui:CardTitle text="Asset Pack" />
    ///     </appui:CardHeader>
    /// </appui:Card>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class Card : ExVisualElement, IPressable
    {
        internal static readonly BindingId interactiveProperty = nameof(interactive);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId elevationProperty = nameof(elevation);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        /// <summary>
        /// The Card main styling class.
        /// </summary>
        public const string ussClassName = "appui-card";

        /// <summary>
        /// The styling class applied to a Card that is <see cref="interactive"/>.
        /// </summary>
        public const string interactiveUssClassName = ussClassName + "--interactive";

        /// <summary>
        /// The elevation styling class prefix. This is the shared App UI elevation ramp.
        /// </summary>
        public const string elevationUssClassName = Styles.elevationUssClassName;

        Pressable m_Clickable;

        bool m_Interactive;

        float m_Elevation;

        bool m_KeyboardFocused;

        /// <summary>
        /// Whether the Card behaves as a single activatable target.
        /// </summary>
        /// <remarks>
        /// An interactive Card is focusable, shows the pointer cursor, tints on hover and fires
        /// <see cref="clicked"/> on a click or on a submit key press. A non-interactive Card is inert
        /// chrome and is skipped by keyboard navigation.
        /// </remarks>
        [Tooltip("Whether the Card behaves as a single activatable target.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool interactive
        {
            get => m_Interactive;
            set
            {
                var changed = m_Interactive != value;
                m_Interactive = value;
                EnableInClassList(interactiveUssClassName, value);
                focusable = value;
                tabIndex = value ? 0 : -1;
                pickingMode = value ? PickingMode.Position : PickingMode.Ignore;

                if (changed)
                    NotifyPropertyChanged(in interactiveProperty);
            }
        }

        /// <summary>
        /// Whether the Card is currently selected.
        /// </summary>
        /// <remarks>
        /// Selection is purely visual here — the Card outlines itself with the accent colour and does
        /// not manage a selection group. Drive it from whatever owns the selection.
        /// </remarks>
        [Tooltip("Whether the Card is currently selected.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = selected != value;
                EnableInClassList(Styles.selectedUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// The elevation level of the Card, from the shared App UI elevation ramp. `0` (the default)
        /// draws no shadow.
        /// </summary>
        [Tooltip("The elevation level of the Card. 0 draws no shadow.")]
        [CreateProperty]
        [UxmlAttribute]
        public float elevation
        {
            get => m_Elevation;
            set
            {
                var changed = !Mathf.Approximately(m_Elevation, value);
                RemoveFromClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));
                m_Elevation = value;
                AddToClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));
                RefreshPasses();

                if (changed)
                    NotifyPropertyChanged(in elevationProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this Card.
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
                if (m_Clickable != null)
                    this.AddManipulator(m_Clickable);

                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// Raised when an <see cref="interactive"/> Card is activated, by click or by submit key.
        /// </summary>
        public event Action clicked
        {
            add => m_Clickable.clicked += value;
            remove => m_Clickable.clicked -= value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Card()
        {
            AddToClassList(ussClassName);

            clickable = new Pressable();
            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnFocus));

            passMask = 0;
            interactive = false;
            selected = false;
            elevation = 0f;
        }

        void OnFocus(FocusInEvent evt)
        {
            m_KeyboardFocused = false;
            RefreshPasses();
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            m_KeyboardFocused = true;
            RefreshPasses();
        }

        void RefreshPasses()
        {
            var passes = (Passes)0;
            if (m_Elevation > 0f)
                passes |= Passes.Clear | Passes.OutsetShadows;
            if (m_KeyboardFocused)
                passes |= Passes.Clear | Passes.Outline;
            passMask = passes;
        }
    }

    /// <summary>
    /// The full-bleed cover slot at the top of a <see cref="Card"/>.
    /// </summary>
    /// <remarks>
    /// Set <see cref="image"/> to show a texture that covers the box, cropping rather than stretching.
    /// With no image the element is an empty inset surface you can fill with any content — a preview
    /// render, an <see cref="Icon"/>, a badge overlay.
    ///
    /// CardMedia has no intrinsic height. Give it one through USS or an inline style, or let the
    /// content you add to it size the box.
    /// </remarks>
    /// <example>
    /// <code lang="csharp"><![CDATA[
    /// var media = new CardMedia { image = thumbnail };
    /// media.style.height = 120;
    /// card.Add(media);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class CardMedia : BaseVisualElement
    {
        internal static readonly BindingId imageProperty = nameof(image);

        /// <summary>
        /// The CardMedia main styling class.
        /// </summary>
        public const string ussClassName = Card.ussClassName + "__media";

        /// <summary>
        /// The styling class of the CardMedia image element.
        /// </summary>
        public const string imageUssClassName = ussClassName + "-image";

        readonly Image m_Image;

        /// <summary>
        /// The cover texture. `null` (the default) hides the image element entirely.
        /// </summary>
        [Tooltip("The cover texture displayed by this CardMedia.")]
        [CreateProperty]
        [UxmlAttribute]
        public Texture2D image
        {
            get => m_Image.image as Texture2D;
            set
            {
                var changed = !ReferenceEquals(m_Image.image, value);
                m_Image.image = value;
                m_Image.EnableInClassList(Styles.hiddenUssClassName, value == null);

                if (changed)
                    NotifyPropertyChanged(in imageProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardMedia()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Image = new Image
            {
                name = imageUssClassName,
                pickingMode = PickingMode.Ignore,
                scaleMode = ScaleMode.ScaleAndCrop,
            };
            m_Image.AddToClassList(imageUssClassName);
            hierarchy.Add(m_Image);

            image = null;
        }
    }

    /// <summary>
    /// Base class for the <see cref="Card"/> slots that space their children evenly.
    /// </summary>
    /// <remarks>
    /// UITK has no `gap` property, so the spacing is emulated: every child carries a leading margin
    /// and the first one is tagged with <see cref="Styles.firstChildUssClassName"/>, which the
    /// stylesheet uses to zero that margin. The tag is refreshed whenever the slot's layout changes,
    /// so adding, inserting and removing children all keep the spacing correct.
    /// </remarks>
    public abstract class CardSlot : BaseVisualElement
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected CardSlot()
        {
            pickingMode = PickingMode.Ignore;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt) => RefreshChildMarkers();

        void OnGeometryChanged(GeometryChangedEvent evt) => RefreshChildMarkers();

        /// <summary>
        /// Re-tags the first child of this slot so it renders without a leading gap.
        /// </summary>
        /// <remarks>
        /// This runs automatically on layout. Call it directly only if you mutate the children and
        /// need the markers before the next layout pass.
        /// </remarks>
        public void RefreshChildMarkers()
        {
            for (var i = 0; i < childCount; i++)
                this[i].EnableInClassList(Styles.firstChildUssClassName, i == 0);
        }
    }

    /// <summary>
    /// The heading block of a <see cref="Card"/>, stacking a <see cref="CardTitle"/> over a
    /// <see cref="CardDescription"/>.
    /// </summary>
    /// <remarks>
    /// The header is a padded vertical stack. Any element can go in it, not just the title and the
    /// description, and consecutive children are separated by the header gap.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <appui:CardHeader>
    ///     <appui:CardTitle text="Third Person Template" />
    ///     <appui:CardDescription text="A ready-made character controller." />
    /// </appui:CardHeader>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class CardHeader : CardSlot
    {
        /// <summary>
        /// The CardHeader main styling class.
        /// </summary>
        public const string ussClassName = Card.ussClassName + "__header";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardHeader()
        {
            AddToClassList(ussClassName);
        }
    }

    /// <summary>
    /// The primary line of a <see cref="CardHeader"/>.
    /// </summary>
    /// <remarks>
    /// A localized text element rendered at the base type step in the semi-bold weight and the primary
    /// text colour. Its `text` accepts a localization key just like any other App UI text element.
    /// </remarks>
    [UxmlElement]
    public partial class CardTitle : LocalizedTextElement
    {
        /// <summary>
        /// The CardTitle main styling class.
        /// </summary>
        public new const string ussClassName = Card.ussClassName + "__title";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardTitle()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
        }
    }

    /// <summary>
    /// The supporting line of a <see cref="CardHeader"/>, under the <see cref="CardTitle"/>.
    /// </summary>
    /// <remarks>
    /// A localized text element rendered one type step down, in the secondary text colour.
    /// </remarks>
    [UxmlElement]
    public partial class CardDescription : LocalizedTextElement
    {
        /// <summary>
        /// The CardDescription main styling class.
        /// </summary>
        public new const string ussClassName = Card.ussClassName + "__description";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardDescription()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
        }
    }

    /// <summary>
    /// The body of a <see cref="Card"/>, below the <see cref="CardHeader"/>.
    /// </summary>
    /// <remarks>
    /// Padded on every side except the top, so it sits flush under the header without doubling the
    /// spacing between them.
    /// </remarks>
    [UxmlElement]
    public partial class CardContent : CardSlot
    {
        /// <summary>
        /// The CardContent main styling class.
        /// </summary>
        public const string ussClassName = Card.ussClassName + "__content";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardContent()
        {
            AddToClassList(ussClassName);
        }
    }

    /// <summary>
    /// The action row at the bottom of a <see cref="Card"/>.
    /// </summary>
    /// <remarks>
    /// A horizontal, vertically centred row — usually holding one or two <see cref="Button"/>s. It
    /// mirrors under a right-to-left layout direction, and consecutive children are separated by the
    /// footer gap.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <appui:CardFooter>
    ///     <appui:Button title="Open" />
    ///     <appui:Button variant="Accent" title="Create" />
    /// </appui:CardFooter>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class CardFooter : CardSlot
    {
        /// <summary>
        /// The CardFooter main styling class.
        /// </summary>
        public const string ussClassName = Card.ussClassName + "__footer";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CardFooter()
        {
            AddToClassList(ussClassName);
        }
    }
}
