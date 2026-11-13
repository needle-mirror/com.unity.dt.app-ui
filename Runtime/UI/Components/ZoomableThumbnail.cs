using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Where a <see cref="ZoomableThumbnail"/> puts its zoom control.
    /// </summary>
    public enum ZoomTriggerMode
    {
        /// <summary>
        /// A small control in the top corner, revealed on hover or focus. For thumbnails big enough to
        /// host it without covering the picture.
        /// </summary>
        Corner,

        /// <summary>
        /// The entire thumbnail is the control, with a scrim on hover to say so. For thumbnails too
        /// small to host a corner control.
        /// </summary>
        Whole,
    }

    /// <summary>
    /// Resolves a larger image lazily, for callers that must fetch to find one.
    /// </summary>
    /// <remarks>
    /// The resolver is handed a completion callback to invoke when it has an answer. Answering with a
    /// default <see cref="Background"/> is not an error: the small image simply stays. Answering late is
    /// safe — a resolution that lands after <see cref="ZoomableThumbnail.src"/> has moved on to a
    /// different image, or after the thumbnail has left the panel, is dropped.
    /// </remarks>
    /// <param name="resolved"> Invoke with the larger image, or with `default` if there is none. </param>
    public delegate void FullImageResolver(Action<Background> resolved);

    /// <summary>
    /// A thumbnail that can be opened full-size.
    /// </summary>
    /// <remarks>
    /// The thumbnail owns its own <see cref="ImageLightbox"/> and builds a <see cref="Modal"/> for it only
    /// when the user asks, so a grid of these costs no more than the images themselves. The modal
    /// is dismissed by a click outside the viewer, by Esc, or by <see cref="CloseZoom"/>.
    ///
    /// <see cref="triggerMode"/> chooses the affordance. <see cref="ZoomTriggerMode.Corner"/> puts a
    /// <see cref="ZoomTriggerButton"/> in the top corner, hidden until the thumbnail is hovered or the
    /// control itself takes focus. <see cref="ZoomTriggerMode.Whole"/> stretches that same control over
    /// the whole image and paints a scrim behind its glyph on hover, which is what tells the user the
    /// picture is a button. USS has no `:focus-within`, so the reveal is keyed off the control's own
    /// `:focus` plus a class this element raises while hovered.
    ///
    /// Set <see cref="trigger"/> to false to render just the image, with no control and no viewer, for a
    /// caller whose thumbnail sits inside another interactive element such as a list item or a button.
    /// A nested control there is unreachable, so such a caller hosts the control and the viewer itself,
    /// as a sibling of that element.
    ///
    /// A larger image can be supplied up front through <see cref="fullSrc"/>, or resolved lazily through
    /// <see cref="loadFullSrc"/>, which is called at most once and never on mount, so a grid of these
    /// issues no requests until someone actually zooms.
    ///
    /// **Not ported from the registry.** The web version keeps its dialog mounted at all times, because
    /// unmounting it in the same commit that closes it skips the library's close path and can leave the
    /// document hit-test-dead. UI Toolkit's <see cref="Modal"/> owns no such global state and is
    /// dismissed explicitly, so the viewer is built on demand here and the guard would be dead code.
    /// </remarks>
    /// <example>
    /// <para>A grid tile with a corner control and a lazily resolved full-size image.</para>
    /// <code lang="csharp"><![CDATA[
    /// var thumb = new ZoomableThumbnail
    /// {
    ///     src = BackgroundExtensions.FromObject(previewTexture),
    ///     alt = "Cliff face, tiling",
    ///     caption = "cliff-face-01.png",
    ///     fallback = new Icon { iconName = "image" },
    /// };
    /// thumb.loadFullSrc = resolved => StartCoroutine(FetchFullRes(url, resolved));
    /// ]]></code>
    /// <para>UXML declaration — the whole picture is the control.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:ZoomableThumbnail caption="cliff-face-01.png" alt="Cliff face" trigger-mode="Whole" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("collections", isNew = true)]
    public partial class ZoomableThumbnail : BaseVisualElement
    {
        internal static readonly BindingId srcProperty = nameof(src);

        internal static readonly BindingId fullSrcProperty = nameof(fullSrc);

        internal static readonly BindingId altProperty = nameof(alt);

        internal static readonly BindingId captionProperty = nameof(caption);

        internal static readonly BindingId triggerModeProperty = nameof(triggerMode);

        internal static readonly BindingId triggerProperty = nameof(trigger);

        /// <summary>
        /// The ZoomableThumbnail main styling class.
        /// </summary>
        public const string ussClassName = "appui-zoomablethumbnail";

        /// <summary>
        /// The styling class of the element the thumbnail is painted on.
        /// </summary>
        public const string imageUssClassName = ussClassName + "__image";

        /// <summary>
        /// The styling class of the container holding the fallback content.
        /// </summary>
        public const string fallbackUssClassName = ussClassName + "__fallback";

        /// <summary>
        /// The styling class of the zoom control, whichever mode it is in.
        /// </summary>
        public const string triggerUssClassName = ussClassName + "__trigger";

        /// <summary>
        /// The ZoomableThumbnail trigger-mode styling class.
        /// </summary>
        [EnumName("GetTriggerModeUssClassName", typeof(ZoomTriggerMode))]
        public const string triggerModeUssClassName = ussClassName + "--";

        const ZoomTriggerMode k_DefaultTriggerMode = ZoomTriggerMode.Corner;

        readonly VisualElement m_Image;

        readonly VisualElement m_FallbackContainer;

        readonly ZoomTriggerButton m_Trigger;

        readonly ImageLightbox m_Lightbox;

        Modal m_Modal;

        VisualElement m_Fallback;

        Background m_FullSrc;

        Background m_Resolved;

        ZoomTriggerMode m_TriggerMode;

        bool m_TriggerEnabled;

        bool m_Requested;

        /// <summary>
        /// Bumped whenever <see cref="src"/> changes, so a resolution that lands afterwards can tell it
        /// belongs to a picture nobody is looking at any more.
        /// </summary>
        int m_Generation;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ZoomableThumbnail()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_Image = new VisualElement { name = imageUssClassName, pickingMode = PickingMode.Ignore };
            m_Image.AddToClassList(imageUssClassName);

            m_FallbackContainer = new VisualElement { name = fallbackUssClassName, pickingMode = PickingMode.Ignore };
            m_FallbackContainer.AddToClassList(fallbackUssClassName);

            m_Trigger = new ZoomTriggerButton(null, OpenZoom) { name = triggerUssClassName };
            m_Trigger.AddToClassList(triggerUssClassName);

            m_Lightbox = new ImageLightbox();

            hierarchy.Add(m_Image);
            hierarchy.Add(m_FallbackContainer);
            hierarchy.Add(m_Trigger);

            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);

            src = new Background();
            fullSrc = new Background();
            alt = null;
            caption = null;
            triggerMode = k_DefaultTriggerMode;
            trigger = true;

            UpdateVisibility();
        }

        [UxmlAttribute("src")]
        UnityEngine.Object srcTex
        {
            get => src.GetSelectedImage();
            set => src = BackgroundExtensions.FromObject(value);
        }

        [UxmlAttribute("full-src")]
        UnityEngine.Object fullSrcTex
        {
            get => fullSrc.GetSelectedImage();
            set => fullSrc = BackgroundExtensions.FromObject(value);
        }

        /// <summary>
        /// The small image. Unset means there is nothing to show or zoom, and the
        /// <see cref="fallback"/> stands in.
        /// </summary>
        [CreateProperty]
        public Background src
        {
            get => m_Image.style.backgroundImage.value;
            set
            {
                var changed = m_Image.style.backgroundImage.value != value;
                m_Image.style.backgroundImage = value;
                if (changed)
                {
                    // A resolution already cached or in flight belongs to the previous image. Drop both
                    // so the next open re-resolves for the new one instead of reusing stale data.
                    m_Generation++;
                    m_Resolved = default;
                    m_Requested = false;
                    NotifyPropertyChanged(in srcProperty);
                }
                UpdateVisibility();
            }
        }

        /// <summary>
        /// A larger image already known to the caller. Preferred over <see cref="src"/> when zoomed.
        /// </summary>
        [CreateProperty]
        public Background fullSrc
        {
            get => m_FullSrc;
            set
            {
                var changed = m_FullSrc != value;
                m_FullSrc = value;
                if (changed)
                    NotifyPropertyChanged(in fullSrcProperty);
            }
        }

        /// <summary>
        /// Resolves a larger image lazily. Called at most once, on first open — never on mount.
        /// </summary>
        public FullImageResolver loadFullSrc { get; set; }

        /// <summary>
        /// A textual description of the enlarged image. The thumbnail itself is decorative.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("alt")]
        [Header("Zoomable Thumbnail")]
        public string alt
        {
            get => m_Lightbox.alt;
            set
            {
                var changed = m_Lightbox.alt != value;
                m_Lightbox.alt = value;
                if (changed)
                    NotifyPropertyChanged(in altProperty);
            }
        }

        /// <summary>
        /// Names the asset, in the zoom control's label and under the enlarged image.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("caption")]
        public string caption
        {
            get => m_Lightbox.caption;
            set
            {
                var changed = m_Lightbox.caption != value;
                m_Lightbox.caption = value;
                m_Trigger.assetName = value;
                if (changed)
                    NotifyPropertyChanged(in captionProperty);
            }
        }

        /// <summary>
        /// Where the zoom control sits, and how much of the thumbnail it takes.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("trigger-mode")]
        public ZoomTriggerMode triggerMode
        {
            get => m_TriggerMode;
            set
            {
                var changed = m_TriggerMode != value;
                RemoveFromClassList(GetTriggerModeUssClassName(m_TriggerMode));
                m_TriggerMode = value;
                AddToClassList(GetTriggerModeUssClassName(m_TriggerMode));
                if (changed)
                    NotifyPropertyChanged(in triggerModeProperty);
            }
        }

        /// <summary>
        /// Whether the thumbnail offers the zoom control and the viewer at all.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("trigger")]
        public bool trigger
        {
            get => m_TriggerEnabled;
            set
            {
                var changed = m_TriggerEnabled != value;
                m_TriggerEnabled = value;
                UpdateVisibility();
                if (changed)
                    NotifyPropertyChanged(in triggerProperty);
            }
        }

        /// <summary>
        /// Shown when there is no image, or it fails to resolve. Also handed to the viewer, so an
        /// enlarged image that never arrives shows the same thing.
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
        /// The viewer this thumbnail opens. Exposed so a caller can style it or read what it is showing.
        /// </summary>
        public ImageLightbox lightbox => m_Lightbox;

        /// <summary>
        /// The zoom control. Exposed so a caller can drive it directly, or read the wording it uses.
        /// </summary>
        public ZoomTriggerButton triggerButton => m_Trigger;

        /// <summary>
        /// Whether the thumbnail has an image to paint.
        /// </summary>
        public bool hasImage => m_Image.style.backgroundImage.value.GetSelectedImage() != null;

        /// <summary>
        /// Open the viewer, resolving a larger image first if one has been promised.
        /// </summary>
        public void OpenZoom()
        {
            if (m_Modal != null)
                return;

            m_Lightbox.src = LargestKnownImage();
            m_Modal = BuildZoomModal();
            m_Modal.Show();

            if (m_FullSrc.GetSelectedImage() != null || m_Requested || loadFullSrc == null)
                return;

            m_Requested = true;
            var generation = m_Generation;
            loadFullSrc(resolved =>
            {
                // The thumbnail may have been detached, or `src` may have moved on to a different
                // image, by the time the resolver answers.
                if (panel == null || generation != m_Generation)
                    return;

                m_Resolved = resolved;
                if (m_Modal != null)
                    m_Lightbox.src = LargestKnownImage();
            });
        }

        /// <summary>
        /// Dismiss the viewer, if it is open.
        /// </summary>
        public void CloseZoom()
        {
            var modal = m_Modal;
            m_Modal = null;
            modal?.Dismiss();
        }

        /// <summary>
        /// The modal hosting the viewer while it is open, `null` otherwise.
        /// </summary>
        internal Modal zoomModal => m_Modal;

        Modal BuildZoomModal()
        {
            // The overlay system owns the scrim and the dismissal: a click outside the viewer, Esc, or
            // CloseZoom. Forget the modal however it closes, so the next open builds a fresh one.
            var modal = Modal.Build(this, m_Lightbox)
                .SetOutsideClickDismiss(true)
                .SetKeyboardDismiss(true);
            // The modal focuses its own content wrapper, which the viewer's arrow keys never reach.
            // Hand focus to the viewer once shown so Esc and the arrows work straight away.
            modal.shown += _ => m_Lightbox.Focus();
            modal.dismissed += (dismissedModal, _) =>
            {
                if (m_Modal == dismissedModal)
                    m_Modal = null;
            };
            return modal;
        }

        Background LargestKnownImage()
        {
            if (m_FullSrc.GetSelectedImage() != null)
                return m_FullSrc;
            if (m_Resolved.GetSelectedImage() != null)
                return m_Resolved;
            return src;
        }

        void UpdateVisibility()
        {
            var showImage = hasImage;
            m_Image.style.display = showImage ? DisplayStyle.Flex : DisplayStyle.None;
            m_FallbackContainer.style.display = showImage || m_Fallback == null
                ? DisplayStyle.None
                : DisplayStyle.Flex;
            // With no image there is nothing to enlarge, so the control goes with it.
            m_Trigger.style.display = showImage && m_TriggerEnabled ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void OnPointerEnter(PointerEnterEvent evt)
        {
            m_Trigger.revealed = true;
        }

        void OnPointerLeave(PointerLeaveEvent evt)
        {
            m_Trigger.revealed = false;
        }
    }
}
