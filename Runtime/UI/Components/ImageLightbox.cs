using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A modal viewer for a single image, sized to the panel.
    /// </summary>
    /// <remarks>
    /// The image is never scaled beyond its natural size: an asset whose only available file is small
    /// stays crisp at its own resolution rather than being upscaled into a blurry wall. The aspect ratio
    /// is preserved by construction, because the drawn size is the natural size times a single uniform
    /// factor that is only ever a shrink.
    ///
    /// Focus trapping, Esc, the scrim and outside-click dismissal all come from <see cref="Modal"/>
    /// rather than a hand-rolled overlay: <see cref="Show"/> wraps this element in one and returns it, so
    /// the caller can keep configuring or dismiss it.
    ///
    /// **Why it measures.** The registry sizes this in viewport units — `max-w-[min(90vw,64rem)]`,
    /// `max-h-[90vh]`, the image at `max-h-[70vh]` — and UI Toolkit has no viewport unit. Nor is a
    /// percentage usable here: the lightbox's parent is the modal's shrink-to-fit content box, whose own
    /// size is what the lightbox is trying to determine, so a percentage against it is circular. So the
    /// element measures the <see cref="Panel"/> it is shown in on `GeometryChangedEvent` and applies the
    /// same fractions itself. They are viewport fractions, not design tokens, which is why they live in
    /// C# as <see cref="panelWidthFraction"/> / <see cref="panelHeightFraction"/> /
    /// <see cref="imageHeightFraction"/> rather than being invented as `--appui-*` vars. Everything that
    /// *is* design — the surface, the padding, the caption's colour and type — stays in USS on tokens.
    ///
    /// The caption doubles as the viewer's title, so there is no separate heading repeating the same
    /// string. With no <see cref="src"/>, or one whose texture never resolved, the <see cref="fallback"/>
    /// element is shown in the image's place.
    ///
    /// **Navigation.** Set <see cref="navigationCount"/> above 1 to show the image as one of a set:
    /// previous and next buttons either side of the caption, Left / Right arrow keys step too, and the
    /// caption leads with the position ("2 / 5"). Stepping never wraps. Each step updates
    /// <see cref="navigationIndex"/> and raises <see cref="navigationIndexChanged"/>; the caller swaps
    /// <see cref="src"/> and <see cref="caption"/>. When a step reaches an end and disables the button
    /// that had focus, focus moves to the other step button so the arrow keys keep working.
    ///
    /// **Tall images.** A picture more than <see cref="tallRatio"/> times taller than wide is shown at
    /// its full (capped) width and scrolls vertically, rather than being shrunk to the panel's height
    /// until nothing in it reads.
    /// </remarks>
    /// <example>
    /// <para>Opening a viewer over the current panel.</para>
    /// <code lang="csharp"><![CDATA[
    /// var lightbox = new ImageLightbox
    /// {
    ///     src = BackgroundExtensions.FromObject(fullResTexture),
    ///     alt = "Cliff face, tiling",
    ///     caption = "cliff-face-01.png",
    /// };
    /// lightbox.Show(rootVisualElement);
    /// ]]></code>
    /// <para>UXML declaration — usually you let <see cref="ZoomableThumbnail"/> own one instead.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:ImageLightbox caption="cliff-face-01.png" alt="Cliff face, tiling" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("collections", isNew = true)]
    public partial class ImageLightbox : BaseVisualElement
    {
        internal static readonly BindingId srcProperty = nameof(src);

        internal static readonly BindingId altProperty = nameof(alt);

        internal static readonly BindingId captionProperty = nameof(caption);

        internal static readonly BindingId navigationIndexProperty = nameof(navigationIndex);

        internal static readonly BindingId navigationCountProperty = nameof(navigationCount);

        internal static readonly BindingId previousLabelProperty = nameof(previousLabel);

        internal static readonly BindingId nextLabelProperty = nameof(nextLabel);

        const string k_DefaultPreviousLabel = "Previous image";

        const string k_DefaultNextLabel = "Next image";

        /// <summary>
        /// The ImageLightbox main styling class.
        /// </summary>
        public const string ussClassName = "appui-imagelightbox";

        /// <summary>
        /// The styling class of the element the image is painted on.
        /// </summary>
        public const string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The styling class of the caption under the image.
        /// </summary>
        public const string captionUssClassName = ussClassName + "__caption";

        /// <summary>
        /// The styling class of the container holding the fallback content.
        /// </summary>
        public const string fallbackUssClassName = ussClassName + "__fallback";

        /// <summary>
        /// The styling class of the scroller holding the image. It only scrolls for a tall image.
        /// </summary>
        public const string scrollUssClassName = ussClassName + "__scroll";

        /// <summary>
        /// The styling class of the row holding the step buttons and the title.
        /// </summary>
        public const string footerUssClassName = ussClassName + "__footer";

        /// <summary>
        /// The styling class of the title, the position followed by the caption.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the position indicator ("2 / 5").
        /// </summary>
        public const string positionUssClassName = ussClassName + "__position";

        /// <summary>
        /// The styling class of the separator between the position and the caption.
        /// </summary>
        public const string separatorUssClassName = ussClassName + "__separator";

        /// <summary>
        /// The styling class of the previous image button.
        /// </summary>
        public const string previousUssClassName = ussClassName + "__previous";

        /// <summary>
        /// The styling class of the next image button.
        /// </summary>
        public const string nextUssClassName = ussClassName + "__next";

        /// <summary>
        /// The modifier class set while the viewer shows one image of a set.
        /// </summary>
        public const string navigationUssClassName = ussClassName + "--navigation";

        /// <summary>
        /// The modifier class set while the image is tall enough to scroll.
        /// </summary>
        public const string tallUssClassName = ussClassName + "--tall";

        /// <summary>
        /// Taller than wide by more than this, a picture scrolls at full width instead of shrinking.
        /// The registry's `TALL_RATIO`.
        /// </summary>
        public const float tallRatio = 1.25f;

        /// <summary>
        /// The share of the panel width the viewer may occupy. The registry's `90vw`.
        /// </summary>
        public const float panelWidthFraction = 0.9f;

        /// <summary>
        /// The share of the panel height the viewer may occupy. The registry's `90vh`.
        /// </summary>
        public const float panelHeightFraction = 0.9f;

        /// <summary>
        /// The share of the panel height the image itself may occupy, leaving room for the caption.
        /// The registry's `max-h-[70vh]`.
        /// </summary>
        public const float imageHeightFraction = 0.7f;

        readonly VisualElement m_Image;

        readonly VisualElement m_FallbackContainer;

        readonly LocalizedTextElement m_Caption;

        readonly ScrollView m_Scroll;

        readonly ActionButton m_Previous;

        readonly ActionButton m_Next;

        readonly LocalizedTextElement m_Position;

        readonly LocalizedTextElement m_Separator;

        string m_Alt;

        VisualElement m_Fallback;

        int m_NavigationIndex;

        int m_NavigationCount;

        string m_PreviousLabel = k_DefaultPreviousLabel;

        string m_NextLabel = k_DefaultNextLabel;

        bool m_IsTall;

        /// <summary>
        /// Raised when the user steps to another image, with the index to show. The caller swaps
        /// <see cref="src"/> and <see cref="caption"/>.
        /// </summary>
        public event Action<int> navigationIndexChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImageLightbox()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            // The fallback target for focus when a step leaves no step button to hold it, so the arrow
            // keys still reach the viewer.
            focusable = true;

            m_Image = new VisualElement { name = imageUssClassName, pickingMode = PickingMode.Ignore };
            m_Image.AddToClassList(imageUssClassName);

            m_Scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = scrollUssClassName,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_Scroll.AddToClassList(scrollUssClassName);
            m_Scroll.Add(m_Image);

            m_FallbackContainer = new VisualElement { name = fallbackUssClassName };
            m_FallbackContainer.AddToClassList(fallbackUssClassName);

            m_Caption = new LocalizedTextElement { name = captionUssClassName, pickingMode = PickingMode.Ignore };
            m_Caption.AddToClassList(captionUssClassName);

            m_Position = new LocalizedTextElement { name = positionUssClassName, pickingMode = PickingMode.Ignore };
            m_Position.AddToClassList(positionUssClassName);

            m_Separator = new LocalizedTextElement { name = separatorUssClassName, pickingMode = PickingMode.Ignore, text = "·" };
            m_Separator.AddToClassList(separatorUssClassName);

            var title = new VisualElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            title.AddToClassList(titleUssClassName);
            title.Add(m_Position);
            title.Add(m_Separator);
            title.Add(m_Caption);

            m_Previous = new ActionButton(() => Step(-1)) { name = previousUssClassName, icon = "caret-left", quiet = true };
            m_Previous.AddToClassList(previousUssClassName);

            m_Next = new ActionButton(() => Step(1)) { name = nextUssClassName, icon = "caret-right", quiet = true };
            m_Next.AddToClassList(nextUssClassName);

            var footer = new VisualElement { name = footerUssClassName };
            footer.AddToClassList(footerUssClassName);
            footer.Add(m_Previous);
            footer.Add(title);
            footer.Add(m_Next);

            hierarchy.Add(m_Scroll);
            hierarchy.Add(m_FallbackContainer);
            hierarchy.Add(footer);

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);
            RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            RegisterCallback<KeyUpEvent>(OnKeyUp, TrickleDown.TrickleDown);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);

            src = new Background();
            caption = null;
            alt = null;

            UpdateVisibility();
            UpdateNavigation();
        }

        [UxmlAttribute("src")]
        UnityEngine.Object srcTex
        {
            get => src.GetSelectedImage();
            set => src = BackgroundExtensions.FromObject(value);
        }

        /// <summary>
        /// The image to display. Leave it unset while a larger image is still resolving, or when none
        /// exists — the <see cref="fallback"/> stands in.
        /// </summary>
        [CreateProperty]
        public Background src
        {
            get => m_Image.style.backgroundImage.value;
            set
            {
                var changed = m_Image.style.backgroundImage.value != value;
                m_Image.style.backgroundImage = value;
                UpdateVisibility();
                Relayout();
                if (changed)
                    NotifyPropertyChanged(in srcProperty);
            }
        }

        /// <summary>
        /// A textual description of the image, for assistive technology and for the tooltip.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("alt")]
        [Header("Image Lightbox")]
        public string alt
        {
            get => m_Alt;
            set
            {
                var changed = m_Alt != value;
                m_Alt = value;
                m_Image.tooltip = m_Alt;
                if (changed)
                    NotifyPropertyChanged(in altProperty);
            }
        }

        /// <summary>
        /// Shown under the image and used as the viewer's accessible name.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("caption")]
        public string caption
        {
            get => m_Caption.text;
            set
            {
                var changed = m_Caption.text != value;
                m_Caption.text = value;
                if (changed)
                    NotifyPropertyChanged(in captionProperty);
            }
        }

        /// <summary>
        /// Which image of the set is shown, counting from zero. Only meaningful while
        /// <see cref="navigationCount"/> is above 1.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("navigation-index")]
        public int navigationIndex
        {
            get => m_NavigationIndex;
            set
            {
                var changed = m_NavigationIndex != value;
                m_NavigationIndex = value;
                UpdateNavigation();
                if (changed)
                    NotifyPropertyChanged(in navigationIndexProperty);
            }
        }

        /// <summary>
        /// How many images the set holds. Above 1, the viewer shows step buttons, the position and
        /// answers the arrow keys; at 0 or 1 it shows a single image.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("navigation-count")]
        public int navigationCount
        {
            get => m_NavigationCount;
            set
            {
                var changed = m_NavigationCount != value;
                m_NavigationCount = value;
                UpdateNavigation();
                if (changed)
                    NotifyPropertyChanged(in navigationCountProperty);
            }
        }

        /// <summary>
        /// The accessible name of the previous image button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("previous-label")]
        public string previousLabel
        {
            get => m_PreviousLabel;
            set
            {
                var changed = m_PreviousLabel != value;
                m_PreviousLabel = value;
                m_Previous.tooltip = string.IsNullOrEmpty(value) ? k_DefaultPreviousLabel : value;
                if (changed)
                    NotifyPropertyChanged(in previousLabelProperty);
            }
        }

        /// <summary>
        /// The accessible name of the next image button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("next-label")]
        public string nextLabel
        {
            get => m_NextLabel;
            set
            {
                var changed = m_NextLabel != value;
                m_NextLabel = value;
                m_Next.tooltip = string.IsNullOrEmpty(value) ? k_DefaultNextLabel : value;
                if (changed)
                    NotifyPropertyChanged(in nextLabelProperty);
            }
        }

        /// <summary>
        /// Whether the image is tall enough (see <see cref="tallRatio"/>) to be shown at full width
        /// and scrolled.
        /// </summary>
        public bool isTall => m_IsTall;

        bool hasNavigation => m_NavigationCount > 1;

        /// <summary>
        /// Steps to another image of the set. Does nothing without a set, or past either end: it never
        /// wraps.
        /// </summary>
        /// <param name="delta"> -1 for the previous image, 1 for the next. </param>
        internal void Step(int delta)
        {
            if (!hasNavigation)
                return;
            var target = m_NavigationIndex + delta;
            if (target < 0 || target >= m_NavigationCount)
                return;

            var focused = focusController?.focusedElement as VisualElement;
            var steppedFrom = focused == m_Previous || focused == m_Next ? (ActionButton)focused : null;

            navigationIndex = target;
            navigationIndexChanged?.Invoke(target);

            // A step's focus fix-up lives for this step only, so a later count change that disables the
            // button never pulls focus back. A disabled button drops focus, out of reach of the arrow keys.
            if (steppedFrom == null || steppedFrom.enabledSelf)
                return;
            var now = focusController?.focusedElement as VisualElement;
            if (now != null && now != steppedFrom)
                return;
            var other = steppedFrom == m_Previous ? m_Next : m_Previous;
            if (other.enabledSelf)
                other.Focus();
            else
                Focus();
        }

        void UpdateNavigation()
        {
            var on = hasNavigation;
            EnableInClassList(navigationUssClassName, on);
            var display = on ? DisplayStyle.Flex : DisplayStyle.None;
            m_Previous.style.display = display;
            m_Next.style.display = display;
            m_Position.style.display = display;
            m_Separator.style.display = display;
            m_Position.text = on ? $"{m_NavigationIndex + 1} / {m_NavigationCount}" : null;
            m_Previous.SetEnabled(on && m_NavigationIndex > 0);
            m_Next.SetEnabled(on && m_NavigationIndex < m_NavigationCount - 1);
            m_Previous.tooltip = string.IsNullOrEmpty(m_PreviousLabel) ? k_DefaultPreviousLabel : m_PreviousLabel;
            m_Next.tooltip = string.IsNullOrEmpty(m_NextLabel) ? k_DefaultNextLabel : m_NextLabel;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (!hasNavigation)
                return;
            int delta;
            if (evt.keyCode == KeyCode.LeftArrow)
                delta = -1;
            else if (evt.keyCode == KeyCode.RightArrow)
                delta = 1;
            else
                return;
            evt.StopPropagation();

            var firstPress = m_HeldArrow == 0;
            m_HeldArrow = delta;
            // The navigation move this press also produced may have arrived first and stepped already.
            var paired = firstPress && m_LastMoveStep == delta && evt.timestamp - m_LastMoveStepTime <= k_PairedEventWindowMs;
            // One navigation move pairs with one key press at most.
            m_LastMoveStep = 0;
            if (paired)
                return;
            Step(delta);
        }

        void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.keyCode is KeyCode.LeftArrow or KeyCode.RightArrow)
                m_HeldArrow = 0;
        }

        void OnAttachToPanel(AttachToPanelEvent evt) => Application.focusChanged += OnApplicationFocusChanged;

        void OnDetachFromPanel(DetachFromPanelEvent evt) => Application.focusChanged -= OnApplicationFocusChanged;

        /// <summary>
        /// A key released while the application is in the background never sends its KeyUp, and the
        /// element focus may not change either.
        /// </summary>
        internal void OnApplicationFocusChanged(bool hasFocus)
        {
            if (!hasFocus)
                m_HeldArrow = 0;
        }

        void OnFocusOut(FocusOutEvent evt)
        {
            // A key released elsewhere never sends us its KeyUp.
            if (evt.relatedTarget is VisualElement next && Contains(next))
                return;
            m_HeldArrow = 0;
        }

        void OnNavigationMove(NavigationMoveEvent evt)
        {
            // An arrow key sends this alongside its KeyDownEvent, each repeating on its own schedule;
            // a gamepad or D-pad sends only this. So while an arrow key is held the key events do the
            // stepping and these are swallowed, and otherwise these step. Either way Left / Right step
            // rather than move focus.
            if (!hasNavigation)
                return;
            int delta;
            if (evt.direction == NavigationMoveEvent.Direction.Left)
                delta = -1;
            else if (evt.direction == NavigationMoveEvent.Direction.Right)
                delta = 1;
            else
                return;
            evt.StopPropagation();
            if (m_HeldArrow != 0)
                return;
            m_LastMoveStep = delta;
            m_LastMoveStepTime = evt.timestamp;
            Step(delta);
        }

        /// <summary>
        /// How far apart, in milliseconds, a key press and the navigation move it produced may arrive
        /// and still count as one input.
        /// </summary>
        const long k_PairedEventWindowMs = 50;

        int m_HeldArrow;

        int m_LastMoveStep;

        long m_LastMoveStepTime = long.MinValue / 2;

        /// <summary>
        /// Rendered in place of the image when there is no <see cref="src"/>, or its texture never
        /// resolved.
        /// </summary>
        public VisualElement fallback
        {
            get => m_Fallback;
            set
            {
                m_Fallback = value;
                m_FallbackContainer.Clear();
                if (m_Fallback != null)
                    m_FallbackContainer.Add(m_Fallback);
                UpdateVisibility();
            }
        }

        /// <summary>
        /// Whether there is an image to paint.
        /// </summary>
        /// <remarks>
        /// A <see cref="VectorImage"/> counts, even though <see cref="NaturalSize"/> deliberately
        /// reports zero for one: it has no pixel resolution to cap against, which is a sizing
        /// question, not a question of whether an image is there. Asking NaturalSize alone would
        /// hide a perfectly good vector image behind the fallback.
        /// </remarks>
        public bool hasImage =>
            m_Image.style.backgroundImage.value.vectorImage || NaturalSize() is { x: > 0, y: > 0 };

        /// <summary>
        /// Show this viewer as a modal over the given panel.
        /// </summary>
        /// <remarks>
        /// The modal owns the scrim, the focus trap, Esc and outside-click dismissal. The returned
        /// instance is not yet shown, so the caller can keep configuring it; call
        /// <see cref="Popup.Show"/> on it, which this method already does, or
        /// <see cref="Popup.Dismiss()"/> to close it again.
        /// </remarks>
        /// <param name="referenceView"> Any element in the panel the viewer should cover. </param>
        /// <returns> The modal hosting this viewer. </returns>
        public Modal Show(VisualElement referenceView)
        {
            var modal = Modal.Build(referenceView, this);
            // The modal focuses its own content wrapper, an ancestor the arrow keys never reach this
            // viewer from. Take focus once shown so they step straight away.
            modal.shown += _ => Focus();
            modal.Show();
            return modal;
        }

        void UpdateVisibility()
        {
            var showImage = hasImage;
            m_Image.style.display = showImage ? DisplayStyle.Flex : DisplayStyle.None;
            m_Scroll.style.display = showImage ? DisplayStyle.Flex : DisplayStyle.None;
            m_FallbackContainer.style.display = showImage || m_Fallback == null
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }

        Vector2 NaturalSize()
        {
            var background = m_Image.style.backgroundImage.value;
            if (background.texture)
                return new Vector2(background.texture.width, background.texture.height);
            if (background.sprite)
                return background.sprite.rect.size;
            if (background.renderTexture)
                return new Vector2(background.renderTexture.width, background.renderTexture.height);
            // A VectorImage has no natural pixel size to cap against, so it is left to fill whatever the
            // USS box allows rather than being pinned to a made-up resolution.
            return Vector2.zero;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Relayout();
        }

        /// <summary>
        /// Applies the registry's viewport fractions against the panel, then sizes the image to the
        /// largest uniform scale that fits — capped at 1, so a small asset is never upscaled.
        /// </summary>
        void Relayout()
        {
            var host = (VisualElement)GetFirstAncestorOfType<Panel>() ?? panel?.visualTree;
            if (host == null)
                return;

            var available = host.layout.size;
            if (available.x <= 0 || available.y <= 0)
                return;

            style.maxWidth = available.x * panelWidthFraction;
            style.maxHeight = available.y * panelHeightFraction;

            var natural = NaturalSize();
            var tall = natural.x > 0 && natural.y > natural.x * tallRatio;
            m_IsTall = tall;
            EnableInClassList(tallUssClassName, tall);
            if (natural.x <= 0 || natural.y <= 0)
                return;

            var budget = new Vector2(available.x * panelWidthFraction, available.y * imageHeightFraction);
            // A tall picture keeps its (capped) width and scrolls inside the image share of the panel;
            // anything else shrinks uniformly to fit it.
            m_Scroll.style.maxHeight = tall ? budget.y : new StyleLength(StyleKeyword.Null);
            var scale = tall
                ? Mathf.Min(1f, budget.x / natural.x)
                : Mathf.Min(1f, Mathf.Min(budget.x / natural.x, budget.y / natural.y));

            m_Image.style.width = natural.x * scale;
            m_Image.style.height = natural.y * scale;
        }
    }
}
