using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Caps tall content at a maximum height and reveals the rest behind a Show more / Show less
    /// toggle.
    /// </summary>
    /// <remarks>
    /// Add any content to a ClampedContent and it measures it on every layout pass. While the content
    /// is shorter than <see cref="maxHeight"/> the element is completely transparent — no toggle, no
    /// fade, no clipping. Once it overflows, the body is clipped, a fade sits over the cut edge and the
    /// toggle appears.
    ///
    /// **This clamps by height, not by line count.** That mirrors the registry component, which uses a
    /// `max-h-*` cap rather than `-webkit-line-clamp`; UI Toolkit has no line-clamp either, so the two
    /// implementations agree. To cap at roughly N lines, set <see cref="maxHeight"/> to N times the
    /// content's line height.
    ///
    /// The cap lives on the clipping viewport and the measurement reads the *container* inside it,
    /// which keeps its natural height whether or not it is clipped. That is why measurement can run in
    /// both states here, where the web component has to skip it while expanded: with the cap off, a DOM
    /// measurement reads "not overflowing" and the toggle vanishes, stranding the user expanded.
    /// The registry's `contentKey` prop exists for the same reason and is not needed either, since UI
    /// Toolkit re-runs layout on any child change. Call <see cref="Remeasure"/> only after replacing
    /// the content outside a layout pass.
    ///
    /// The fade is a vertical gradient from transparent to the colour set by the
    /// `--clamped-content-fade-color` custom USS property on the fade element (the default surface
    /// colour by default). USS has no `linear-gradient`, so it is drawn as a vertex-coloured mesh.
    /// Override the property when the host surface is not the default background, or set
    /// <see cref="fadeVisible"/> to `false`.
    /// </remarks>
    /// <example>
    /// <para>Clamping a long description in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:ClampedContent max-height="120">
    ///     <appui:Text text="A very long description that runs well past the cap..." />
    /// </appui:ClampedContent>
    /// ]]></code>
    /// <para>Clamping in C# with custom labels.</para>
    /// <code lang="csharp"><![CDATA[
    /// var clamped = new ClampedContent
    /// {
    ///     maxHeight = 200f,
    ///     moreLabel = "Read more",
    ///     lessLabel = "Collapse",
    /// };
    /// clamped.Add(new Text { text = logOutput });
    /// clamped.expandedChanged += expanded => Debug.Log($"expanded: {expanded}");
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class ClampedContent : BaseVisualElement
    {
        internal static readonly BindingId maxHeightProperty = nameof(maxHeight);

        internal static readonly BindingId expandedProperty = nameof(expanded);

        internal static readonly BindingId moreLabelProperty = nameof(moreLabel);

        internal static readonly BindingId lessLabelProperty = nameof(lessLabel);

        internal static readonly BindingId fadeVisibleProperty = nameof(fadeVisible);

        /// <summary>
        /// The ClampedContent main styling class.
        /// </summary>
        public const string ussClassName = "appui-clamped-content";

        /// <summary>
        /// The styling class of the clipping viewport.
        /// </summary>
        public const string viewportUssClassName = ussClassName + "__viewport";

        /// <summary>
        /// The styling class of the container the content actually lives in.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The styling class of the fade drawn over the clipped edge.
        /// </summary>
        public const string fadeUssClassName = ussClassName + "__fade";

        /// <summary>
        /// The styling class of the Show more / Show less toggle.
        /// </summary>
        public const string toggleUssClassName = ussClassName + "__toggle";

        /// <summary>
        /// The styling class applied while the content actually overflows.
        /// </summary>
        public const string overflowingUssClassName = ussClassName + "--overflowing";

        /// <summary>
        /// The styling class applied while the content is expanded.
        /// </summary>
        public const string expandedUssClassName = ussClassName + "--expanded";

        /// <summary>
        /// The default height cap, in pixels.
        /// </summary>
        /// <remarks>
        /// Mirrors the registry's `max-h-80` default. It is a literal because it is a per-use content
        /// budget, not a design token — no App UI sizing step is anywhere near this tall.
        /// </remarks>
        public const float defaultMaxHeight = 320f;

        readonly VisualElement m_Viewport;

        readonly VisualElement m_Container;

        readonly VisualElement m_Fade;

        readonly LocalizedTextElement m_Toggle;

        readonly Pressable m_TogglePressable;

        float m_MaxHeight = defaultMaxHeight;

        bool m_Expanded;

        bool m_Overflowing;

        string m_MoreLabel = "Show more";

        string m_LessLabel = "Show less";

        /// <inheritdoc />
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The height cap applied while collapsed, in pixels.
        /// </summary>
        [Tooltip("The height cap applied while collapsed, in pixels.")]
        [CreateProperty]
        [UxmlAttribute]
        public float maxHeight
        {
            get => m_MaxHeight;
            set
            {
                var changed = !Mathf.Approximately(m_MaxHeight, value);
                m_MaxHeight = Mathf.Max(0f, value);
                ApplyClamp();
                Remeasure();

                if (changed)
                    NotifyPropertyChanged(in maxHeightProperty);
            }
        }

        /// <summary>
        /// Whether the content is currently revealed in full.
        /// </summary>
        /// <remarks>
        /// Setting this drives the same state the toggle does, so a host can expand the block from
        /// elsewhere — a "expand all" command, for instance.
        /// </remarks>
        [Tooltip("Whether the content is currently revealed in full.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool expanded
        {
            get => m_Expanded;
            set
            {
                var changed = m_Expanded != value;
                m_Expanded = value;
                EnableInClassList(expandedUssClassName, value);
                m_Toggle.text = value ? m_LessLabel : m_MoreLabel;
                ApplyClamp();
                UpdateFade();

                if (changed)
                {
                    expandedChanged?.Invoke(value);
                    NotifyPropertyChanged(in expandedProperty);
                }
            }
        }

        /// <summary>
        /// The toggle's label while collapsed. Accepts a localization key.
        /// </summary>
        [Tooltip("The toggle's label while collapsed.")]
        [CreateProperty]
        [UxmlAttribute]
        public string moreLabel
        {
            get => m_MoreLabel;
            set
            {
                var changed = m_MoreLabel != value;
                m_MoreLabel = value;
                if (!m_Expanded)
                    m_Toggle.text = value;

                if (changed)
                    NotifyPropertyChanged(in moreLabelProperty);
            }
        }

        /// <summary>
        /// The toggle's label while expanded. Accepts a localization key.
        /// </summary>
        [Tooltip("The toggle's label while expanded.")]
        [CreateProperty]
        [UxmlAttribute]
        public string lessLabel
        {
            get => m_LessLabel;
            set
            {
                var changed = m_LessLabel != value;
                m_LessLabel = value;
                if (m_Expanded)
                    m_Toggle.text = value;

                if (changed)
                    NotifyPropertyChanged(in lessLabelProperty);
            }
        }

        /// <summary>
        /// Whether the fade is drawn over the clipped edge while collapsed and overflowing.
        /// </summary>
        [Tooltip("Whether the fade is drawn over the clipped edge.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool fadeVisible
        {
            get => m_FadeVisible;
            set
            {
                var changed = m_FadeVisible != value;
                m_FadeVisible = value;
                UpdateFade();

                if (changed)
                    NotifyPropertyChanged(in fadeVisibleProperty);
            }
        }

        bool m_FadeVisible = true;

        /// <summary>
        /// Whether the content is taller than <see cref="maxHeight"/>.
        /// </summary>
        /// <remarks>
        /// Read-only, and re-evaluated on every layout pass while collapsed. While expanded it keeps
        /// the value measured just before expanding.
        /// </remarks>
        public bool overflowing => m_Overflowing;

        /// <summary>
        /// Raised when <see cref="expanded"/> changes, by toggle or programmatically.
        /// </summary>
        public event Action<bool> expandedChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ClampedContent()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Viewport = new VisualElement { name = viewportUssClassName, pickingMode = PickingMode.Ignore };
            m_Viewport.AddToClassList(viewportUssClassName);
            hierarchy.Add(m_Viewport);

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            m_Viewport.Add(m_Container);

            m_Fade = new VisualElement { name = fadeUssClassName, pickingMode = PickingMode.Ignore };
            m_Fade.AddToClassList(fadeUssClassName);
            m_Fade.generateVisualContent += OnGenerateFade;
            m_Fade.RegisterCallback<CustomStyleResolvedEvent>(OnFadeCustomStyleResolved);
            m_Fade.RegisterCallback<GeometryChangedEvent>(OnFadeGeometryChanged);
            m_Viewport.Add(m_Fade);

            m_Toggle = new LocalizedTextElement { name = toggleUssClassName };
            m_Toggle.AddToClassList(toggleUssClassName);
            m_Toggle.focusable = true;
            m_Toggle.tabIndex = 0;
            m_TogglePressable = new Pressable(() => expanded = !expanded);
            m_Toggle.AddManipulator(m_TogglePressable);
            hierarchy.Add(m_Toggle);

            m_Container.RegisterCallback<GeometryChangedEvent>(OnContentGeometryChanged);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);

            fadeVisible = true;
            moreLabel = "Show more";
            lessLabel = "Show less";
            expanded = false;
            maxHeight = defaultMaxHeight;
            SetOverflowing(false);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt) => Remeasure();

        void OnGeometryChanged(GeometryChangedEvent evt) => Remeasure();

        void OnContentGeometryChanged(GeometryChangedEvent evt) => Remeasure();

        void ApplyClamp()
        {
            // The cap lives on the viewport, not on the content, so the content keeps its natural
            // height and stays measurable while it is clipped.
            if (m_Expanded)
                m_Viewport.style.maxHeight = StyleKeyword.None;
            else
                m_Viewport.style.maxHeight = m_MaxHeight;
        }

        /// <summary>
        /// Re-measures the content against <see cref="maxHeight"/> and updates the toggle and the fade.
        /// </summary>
        /// <remarks>
        /// This runs automatically on attach and on every layout change. Call it directly only after
        /// swapping the content outside a layout pass. It is correct in both states: what is measured
        /// is the uncapped container, not the clipping viewport.
        /// </remarks>
        public void Remeasure()
        {
            var contentHeight = m_Container.layout.height;
            if (float.IsNaN(contentHeight))
                return;

            // A one-pixel slack, matching the registry: UI Toolkit snaps resolved layout to whole
            // pixels, so an exactly-fitting block can measure a hair over the cap.
            SetOverflowing(contentHeight > m_MaxHeight + 1f);
        }

        void SetOverflowing(bool value)
        {
            m_Overflowing = value;
            EnableInClassList(overflowingUssClassName, value);
            m_Toggle.EnableInClassList(Styles.hiddenUssClassName, !value);
            UpdateFade();
        }

        static readonly CustomStyleProperty<Color> k_FadeColorProperty =
            new CustomStyleProperty<Color>("--clamped-content-fade-color");

        Color m_FadeColor = Color.clear;

        void OnFadeCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var color = evt.customStyle.TryGetValue(k_FadeColorProperty, out var c) ? c : Color.clear;
            if (color == m_FadeColor)
                return;
            m_FadeColor = color;
            m_Fade.MarkDirtyRepaint();
        }

        void OnFadeGeometryChanged(GeometryChangedEvent evt) => m_Fade.MarkDirtyRepaint();

        // A quad whose top edge is transparent and bottom edge is the fade colour; the vertex
        // colours interpolate into a vertical gradient.
        void OnGenerateFade(MeshGenerationContext mgc)
        {
            var rect = m_Fade.contentRect;
            if (rect.width <= 0 || rect.height <= 0 || m_FadeColor.a <= 0)
                return;

            var mesh = mgc.Allocate(4, 6);
            var top = m_FadeColor;
            top.a = 0;
            Color32 top32 = top;
            Color32 bottom32 = m_FadeColor;
            mesh.SetNextVertex(new Vertex { position = new Vector3(rect.xMin, rect.yMin, Vertex.nearZ), tint = top32 });
            mesh.SetNextVertex(new Vertex { position = new Vector3(rect.xMax, rect.yMin, Vertex.nearZ), tint = top32 });
            mesh.SetNextVertex(new Vertex { position = new Vector3(rect.xMax, rect.yMax, Vertex.nearZ), tint = bottom32 });
            mesh.SetNextVertex(new Vertex { position = new Vector3(rect.xMin, rect.yMax, Vertex.nearZ), tint = bottom32 });
            // Clockwise winding, as UI Toolkit expects.
            mesh.SetNextIndex(0);
            mesh.SetNextIndex(1);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(2);
            mesh.SetNextIndex(3);
            mesh.SetNextIndex(0);
        }

        void UpdateFade()
        {
            var show = m_FadeVisible && m_Overflowing && !m_Expanded;
            m_Fade.EnableInClassList(Styles.hiddenUssClassName, !show);
        }
    }
}
