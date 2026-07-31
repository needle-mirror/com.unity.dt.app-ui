using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A visual element that applies color masking effects to create visual overlays and highlights.
    /// </summary>
    /// <remarks>
    /// The Mask component is a versatile visual element that allows you to create masked areas with
    /// customizable colors, shapes, and effects. It's particularly useful for creating spotlight effects,
    /// highlighting specific areas, or creating visual overlays in your UI.
    ///
    /// The component provides fine control over the mask's appearance through properties like inner and outer
    /// colors, mask rectangle dimensions, corner radius, and blur effects. You can specify mask dimensions
    /// either in absolute pixels or normalized coordinates (0-1 range).
    ///
    /// Note: The Mask component inherits from Image and uses a custom shader to generate the masking effect.
    /// The mask is rendered using a RenderTexture that automatically adjusts to the component's size.
    /// </remarks>
    /// <example>
    /// <para>Creating a spotlight effect</para>
    /// <code lang="csharp"><![CDATA[
    /// var spotlight = new Mask();
    /// spotlight.style.position = Position.Absolute;
    /// spotlight.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
    /// spotlight.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
    /// spotlight.innerMaskColor = Color.clear;
    /// spotlight.outerMaskColor = new Color(0, 0, 0, 0.7f);
    /// spotlight.maskRect = new Rect(100, 100, 200, 200);
    /// spotlight.radius = 100f; // Circular spotlight
    /// spotlight.blur = 20f; // Soft edges
    /// ]]></code>
    /// <para>Creating a responsive highlight area using normalized coordinates</para>
    /// <code lang="csharp"><![CDATA[
    /// var highlight = new Mask();
    /// highlight.useNormalizedMaskRect = true;
    /// highlight.innerMaskColor = new Color(1, 1, 0, 0.2f); // Semi-transparent yellow
    /// highlight.outerMaskColor = Color.clear;
    /// highlight.maskRect = new Rect(0.1f, 0.1f, 0.8f, 0.2f); // Highlight strip
    /// highlight.radius = 10f;
    /// highlight.blur = 5f;
    /// ]]></code>
    /// <para>UXML definition example</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns="UnityEngine.UIElements">
    ///     <Mask
    ///         inner-mask-color="#FFFFFF"
    ///         outer-mask-color="#000000AA"
    ///         mask-rect-x="50"
    ///         mask-rect-y="50"
    ///         mask-rect-width="300"
    ///         mask-rect-height="200"
    ///         radius="15"
    ///         blur="10"
    ///         use-normalized-mask-rect="false"
    ///         style="position: absolute; width: 100%; height: 100%;" />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class Mask : Image
    {

        internal static readonly BindingId innerMaskColorProperty = nameof(innerMaskColor);

        internal static readonly BindingId outerMaskColorProperty = nameof(outerMaskColor);

        internal static readonly BindingId maskRectProperty = nameof(maskRect);

        internal static readonly BindingId radiusProperty = nameof(radius);

        internal static readonly BindingId blurProperty = nameof(blur);

        internal static readonly BindingId useNormalizedMaskRectProperty = nameof(useNormalizedMaskRect);


        /// <summary>
        /// The Mask main styling class.
        /// </summary>
        public new const string ussClassName = "appui-mask";

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => null;

        RenderTexture m_RT;

        static Material s_Material;

        static readonly int k_MaskRect = Shader.PropertyToID("_MaskRect");

        static readonly int k_Radius = Shader.PropertyToID("_Radius");

        static readonly int k_InnerMaskColor = Shader.PropertyToID("_InnerMaskColor");

        static readonly int k_OuterMaskColor = Shader.PropertyToID("_OuterMaskColor");

        static readonly int k_Ratio = Shader.PropertyToID("_Ratio");

        static readonly int k_Sigma = Shader.PropertyToID("_Sigma");

        Vector2 m_PreviousSize;

        Color m_InnerMaskColor = Color.black;

        Color m_OuterMaskColor = Color.clear;

        Rect m_MaskRect = new Rect(100f, 100f, 100f, 40f);

        Rect m_PreviousMaskRect;

        float m_Radius = 0;

        float m_Blur = 0;

        bool m_UseNormalizedMaskRect;

        static readonly Color k_DefaultInnerMaskColor = Color.white;

        static readonly Color k_DefaultOuterMaskColor = Color.black;

        static readonly Rect k_DefaultMaskRect = new Rect(20f, 20f, 20f, 20f);

        const float k_DefaultRadius = 0f;

        const float k_DefaultBlur = 0f;

        const bool k_DefaultUseNormalizedMaskRect = false;

        /// <summary>
        /// The inner mask color. Sets the color of the inner mask.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Color innerMaskColor
        {
            get => m_InnerMaskColor;
            set
            {
                var changed = m_InnerMaskColor != value;
                m_InnerMaskColor = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in innerMaskColorProperty);
            }
        }

        /// <summary>
        /// The outer mask color. The color of the area outside the mask.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Color outerMaskColor
        {
            get => m_OuterMaskColor;
            set
            {
                var changed = m_OuterMaskColor != value;
                m_OuterMaskColor = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in outerMaskColorProperty);
            }
        }

        /// <summary>
        /// The mask rect. Sets the rect of the mask (in pixels or normalized if <see cref="useNormalizedMaskRect"/> is true).
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Rect maskRect
        {
            get => m_MaskRect;
            set
            {
                var changed = m_MaskRect != value;
                m_MaskRect = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in maskRectProperty);
            }
        }

        /// <summary>
        /// The mask radius. Sets the radius of the rounded corners (in pixels).
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float radius
        {
            get => m_Radius;
            set
            {
                var changed = !Mathf.Approximately(m_Radius, value);
                m_Radius = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in radiusProperty);
            }
        }

        /// <summary>
        /// The mask blur. Sets the blur of the mask (in pixels).
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float blur
        {
            get => m_Blur;
            set
            {
                var changed = !Mathf.Approximately(m_Blur, value);
                m_Blur = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in blurProperty);
            }
        }

        /// <summary>
        /// If true, the mask rect you will provide through <see cref="maskRect"/> must be normalized (0-1) instead of using pixels coordinates.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool useNormalizedMaskRect
        {
            get => m_UseNormalizedMaskRect;
            set
            {
                var changed = m_UseNormalizedMaskRect != value;
                m_UseNormalizedMaskRect = value;
                GenerateTextures();
                MarkDirtyRepaint();

                if (changed)
                    NotifyPropertyChanged(in useNormalizedMaskRectProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Mask()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);

            innerMaskColor = k_DefaultInnerMaskColor;
            outerMaskColor = k_DefaultOuterMaskColor;
            radius = k_DefaultRadius;
            blur = k_DefaultBlur;
            maskRect = k_DefaultMaskRect;
            useNormalizedMaskRect = k_DefaultUseNormalizedMaskRect;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var isNullSize =
                paddingRect.width <= Mathf.Epsilon ||
                paddingRect.height <= Mathf.Epsilon;

            var isSameSize =
                Mathf.Approximately(paddingRect.width, m_PreviousSize.x) &&
                Mathf.Approximately(paddingRect.height, m_PreviousSize.y);

            m_PreviousSize.x = paddingRect.width;
            m_PreviousSize.y = paddingRect.height;

            if (!isNullSize && !isSameSize)
            {
                GenerateTextures();
                MarkDirtyRepaint();
            }
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            ReleaseTextures();
        }

        void GenerateTextures()
        {
            if (!s_Material)
            {
                s_Material = MaterialUtils.CreateMaterial("Hidden/App UI/Mask");
                if (!s_Material)
                {
                    ReleaseTextures();
                    return;
                }
            }

            var rect = paddingRect;

            if (!rect.IsValid())
            {
                ReleaseTextures();
                return;
            }

            var dpi = Mathf.Max(Platform.scaleFactor, 1f);
            var texSize = rect.size * dpi;

            if (!texSize.IsValidForTextureSize())
            {
                ReleaseTextures();
                return;
            }

            if (m_RT && (Mathf.Abs(m_RT.width - texSize.x) > 1 || Mathf.Abs(m_RT.height - texSize.y) > 1))
                ReleaseTextures();

            if (!m_RT)
                m_RT = RenderTexture.GetTemporary((int)texSize.x, (int)texSize.y, 24);

            s_Material.SetColor(k_InnerMaskColor, innerMaskColor);
            s_Material.SetColor(k_OuterMaskColor, outerMaskColor);

            var ratio = rect.width / rect.height;
            var maskRect =
                useNormalizedMaskRect ? new Vector4(m_MaskRect.x, m_MaskRect.y / ratio, m_MaskRect.width, m_MaskRect.height / ratio) :
                new Vector4(m_MaskRect.x / rect.width, (m_MaskRect.y / rect.height) / ratio,
                m_MaskRect.width / rect.width, (m_MaskRect.height / rect.height) / ratio);
            s_Material.SetVector(k_MaskRect, maskRect);
            s_Material.SetFloat(k_Ratio, ratio);
            s_Material.SetFloat(k_Radius, m_Radius / rect.width);
            s_Material.SetFloat(k_Sigma, m_Blur / rect.width);

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;

            if (image != m_RT)
                image = m_RT;
        }

        void ReleaseTextures()
        {
            if (m_RT)
            {
                RenderTexture.ReleaseTemporary(m_RT);
                m_RT = null;
            }
        }

    }
}
