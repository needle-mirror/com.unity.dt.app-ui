using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The control that opens an image full-size.
    /// </summary>
    /// <remarks>
    /// Shared so that every surface offering this affordance looks and reads the same, and so the
    /// accessible-name wording lives in exactly one place: <see cref="assetName"/> names the thing being
    /// enlarged, and the component turns it into the tooltip *View larger preview of &lt;name&gt;*.
    ///
    /// **It stops propagation.** The control usually sits on top of something whose own click, or
    /// Enter/Space activation, means *select me*, and enlarging is not selecting. The pointer-down is
    /// stopped too: a host that selects on pointer down, to arm a drag gesture say, would otherwise
    /// select the thing underneath before the click even fires. Stopping it does not affect the button's
    /// own activation, because <see cref="IconButton.clickable"/> is a manipulator on this element and
    /// runs before the event reaches an ancestor.
    ///
    /// **It hides until it is wanted.** The button rests at zero opacity so a dense grid of thumbnails
    /// stays calm, and reveals on its own `:focus` or when the host raises <see cref="revealed"/> — which
    /// is what <see cref="ZoomableThumbnail"/> does on hover. USS has no `:focus-within` and no
    /// `group-hover`, so the host owning the hover is the UI Toolkit shape of the registry's
    /// `group-hover/zoom:opacity-100`.
    ///
    /// On a platform with no hover at all, <see cref="alwaysVisible"/> defaults to true and the control
    /// simply stays on screen. A zero-opacity element is still hit-testable, so hiding it on a touch
    /// device would not spare the user the control — it would only make it invisible, and a tap near the
    /// corner of a tile would open a viewer with nothing on screen to explain why.
    /// </remarks>
    /// <example>
    /// <para>A standalone zoom control.</para>
    /// <code lang="csharp"><![CDATA[
    /// var zoom = new ZoomTriggerButton { assetName = "Cliff face, tiling" };
    /// zoom.clicked += () => lightbox.Show(rootVisualElement);
    /// ]]></code>
    /// <para>UXML declaration.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:ZoomTriggerButton asset-name="Cliff face, tiling" />
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("collections", isNew = true)]
    public partial class ZoomTriggerButton : IconButton
    {
        internal static readonly BindingId assetNameProperty = nameof(assetName);

        internal static readonly BindingId revealedProperty = nameof(revealed);

        internal static readonly BindingId alwaysVisibleProperty = nameof(alwaysVisible);

        /// <summary>
        /// The ZoomTriggerButton main styling class.
        /// </summary>
        public new const string ussClassName = "appui-zoomtriggerbutton";

        /// <summary>
        /// The styling class carried while the control is revealed by its host.
        /// </summary>
        public const string revealedUssClassName = ussClassName + "--revealed";

        /// <summary>
        /// The styling class carried while the control never hides.
        /// </summary>
        public const string alwaysVisibleUssClassName = ussClassName + "--always-visible";

        /// <summary>
        /// The icon this control always shows.
        /// </summary>
        public const string zoomIconName = "magnifying-glass";

        /// <summary>
        /// The tooltip wording, with the asset name substituted in.
        /// </summary>
        public const string accessibleNameFormat = "View larger preview of {0}";

        string m_AssetName;

        bool m_Revealed;

        bool m_AlwaysVisible;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ZoomTriggerButton()
            : this(null)
        {
        }

        /// <summary>
        /// Construct a ZoomTriggerButton naming the asset it enlarges.
        /// </summary>
        /// <param name="assetName"> Names the thing being enlarged; becomes the accessible label. </param>
        /// <param name="clickEvent"> Invoked when the control is activated. </param>
        public ZoomTriggerButton(string assetName, Action clickEvent = null)
            : base(zoomIconName, clickEvent)
        {
            AddToClassList(ussClassName);

            quiet = true;
            size = Size.Sm;

            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<KeyDownEvent>(OnKeyDown);

            this.assetName = assetName;
            revealed = false;
            // Mirrors the registry's `[@media(hover:none)]:opacity-100`.
            alwaysVisible = UnityEngine.Device.Application.isMobilePlatform;
        }

        /// <summary>
        /// Names the thing being enlarged. Becomes the accessible label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute("asset-name")]
        [Header("Zoom Trigger Button")]
        public string assetName
        {
            get => m_AssetName;
            set
            {
                var changed = m_AssetName != value;
                m_AssetName = value;
                tooltip = string.IsNullOrEmpty(m_AssetName)
                    ? null
                    : string.Format(accessibleNameFormat, m_AssetName);
                if (changed)
                    NotifyPropertyChanged(in assetNameProperty);
            }
        }

        /// <summary>
        /// Whether the host is currently revealing the control.
        /// </summary>
        /// <remarks>
        /// USS has no `:focus-within` and no `group-hover`, so a host that reveals this control on its
        /// own hover raises this flag instead. The control's own `:focus` reveals it without any help.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("revealed")]
        public bool revealed
        {
            get => m_Revealed;
            set
            {
                var changed = m_Revealed != value;
                m_Revealed = value;
                EnableInClassList(revealedUssClassName, m_Revealed);
                if (changed)
                    NotifyPropertyChanged(in revealedProperty);
            }
        }

        /// <summary>
        /// Whether the control stays visible instead of hiding until hover or focus.
        /// </summary>
        /// <remarks>
        /// Defaults to true on a platform with no hover, for the reason spelled out on the type.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("always-visible")]
        public bool alwaysVisible
        {
            get => m_AlwaysVisible;
            set
            {
                var changed = m_AlwaysVisible != value;
                m_AlwaysVisible = value;
                EnableInClassList(alwaysVisibleUssClassName, m_AlwaysVisible);
                if (changed)
                    NotifyPropertyChanged(in alwaysVisibleProperty);
            }
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            // The host underneath may select on pointer down. Enlarging is not selecting.
            evt.StopPropagation();
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.target != this)
                return;

            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter or KeyCode.Space)
                evt.StopPropagation();
        }
    }
}
