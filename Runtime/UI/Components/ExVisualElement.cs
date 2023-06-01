using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Styling properties that can be applied to a <see cref="ExVisualElement"/>.
    /// </summary>
    public struct AdditionalStyle
    {
        /// <summary>
        /// The outline offset.
        /// </summary>
        public float outlineOffset;

        /// <summary>
        /// The outline width.
        /// </summary>
        public float outlineWidth;

        /// <summary>
        /// The outline color.
        /// </summary>
        public Color outlineColor;

        /// <summary>
        /// The shadow offset.
        /// </summary>
        public Vector2 shadowOffset;

        /// <summary>
        /// The shadow spread.
        /// </summary>
        public float shadowSpread;

        /// <summary>
        /// The shadow blur.
        /// </summary>
        public float shadowBlur;

        /// <summary>
        /// The shadow color.
        /// </summary>
        public Color shadowColor;

        /// <summary>
        /// If the shadow is inset or outset.
        /// </summary>
        public bool shadowInset;

        /// <summary>
        /// The border width.
        /// </summary>
        public float borderWidth;

        /// <summary>
        /// The border color.
        /// </summary>
        public Color borderColor;

        /// <summary>
        /// The background color.
        /// </summary>
        public Color backgroundColor;
    }

    /// <summary>
    /// A visual element that can be used as a normal VisualElement but with additional styling options like shadows, borders, outline, etc.
    /// </summary>
    public class ExVisualElement : VisualElement
    {
        /// <summary>
        /// Rendering passes that will be executed. This is used to optimize and fine-tune the rendering.
        /// </summary>
        [Flags]
        public enum Passes
        {
            /// <summary>
            /// The clear pass. This will clear the render texture.
            /// </summary>
            Clear = 0b00000001,
            /// <summary>
            /// The outset shadows pass. This will render the outset shadows.
            /// </summary>
            OutsetShadows = 0b00000010,
            /// <summary>
            /// The background color pass. This will render the background color.
            /// </summary>
            BackgroundColor = 0b00000100,
            /// <summary>
            /// The background image pass. This will render the background image.
            /// </summary>
            BackgroundImage = 0b00001000,
            /// <summary>
            /// The inset shadows pass. This will render the inset shadows.
            /// </summary>
            InsetShadows = 0b00010000,
            /// <summary>
            /// The borders pass. This will render the borders.
            /// </summary>
            Borders = 0b00100000,
            /// <summary>
            /// The outline pass. This will render the outline.
            /// </summary>
            Outline = 0b01000000,
        }
        
        const int k_BlitTextureMessageId = 520;

        static Handler handler
        {
            get
            {
                if (s_Handler == null)
                    s_Handler = new Handler(AppUI.Core.AppUI.mainLooper, HandleMessage);
                
                return s_Handler;
            }
        }
        
        static bool HandleMessage(Message msg)
        {
            if (msg.what == k_BlitTextureMessageId)
            {
                var ve = (ExVisualElement)msg.obj;

                var renderRect = PrepareMaterial(ve.paddingRect, ve.resolvedStyle, ve.m_Style, ve.passMask);
                var rtSize = GetRenderTextureSize(renderRect);

                if (rtSize.x < 1 || rtSize.y < 1)
                    return true;

                if (ve.m_RT && (ve.m_RT.width != rtSize.x || ve.m_RT.height != rtSize.y))
                {
                    ve.m_RT.Release();
                    ve.m_RT = null;
                }

                if (!ve.m_RT)
                {
                    ve.m_RT = new RenderTexture(rtSize.x, rtSize.y, 24);
                    ve.m_RT.Create();
                }
                
                var prevRt = RenderTexture.active;
                if (ve.passMask == (Passes)0xFF)
                {
                    Graphics.Blit(null, ve.m_RT, s_Material);
                }
                else
                {
                    if ((ve.passMask & Passes.Clear) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 0);
                    if ((ve.passMask & Passes.OutsetShadows) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 1);
                    if ((ve.passMask & Passes.BackgroundColor) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 2);
                    if ((ve.passMask & Passes.BackgroundImage) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 3);
                    if ((ve.passMask & Passes.InsetShadows) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 4);
                    if ((ve.passMask & Passes.Borders) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 5);
                    if ((ve.passMask & Passes.Outline) > 0)
                        Graphics.Blit(null, ve.m_RT, s_Material, 6);
                }
                RenderTexture.active = prevRt;
            
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => this;

        RenderTexture m_RT;

        static Material s_Material;

        static readonly Vertex[] k_Vertices = new Vertex[4];
        static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };

        static readonly int k_Rect = Shader.PropertyToID("_Rect");

        static readonly int k_OutlineThickness = Shader.PropertyToID("_OutlineThickness");

        static readonly int k_OutlineColor = Shader.PropertyToID("_OutlineColor");

        static readonly int k_OutlineOffset = Shader.PropertyToID("_OutlineOffset");

        static readonly int k_AASoftness = Shader.PropertyToID("_AA");

        static readonly int k_ShadowOffset = Shader.PropertyToID("_ShadowOffset");

        static readonly int k_Radiuses = Shader.PropertyToID("_Radiuses");

        static readonly int k_Color = Shader.PropertyToID("_Color");

        static readonly int k_BackgroundImage = Shader.PropertyToID("_BackgroundImage");

        static readonly CustomStyleProperty<float> k_UssBorderWidth = new CustomStyleProperty<float>("--border-width");

        static readonly CustomStyleProperty<Color> k_UssBorderColor = new CustomStyleProperty<Color>("--border-color");

        static readonly CustomStyleProperty<Color> k_UssBackgroundColor = new CustomStyleProperty<Color>("--background-color");

        static readonly CustomStyleProperty<float> k_UssOutlineWidth = new CustomStyleProperty<float>("--outline-width");

        static readonly CustomStyleProperty<float> k_UssOutlineOffset = new CustomStyleProperty<float>("--outline-offset");

        static readonly CustomStyleProperty<Color> k_UssOutlineColor = new CustomStyleProperty<Color>("--outline-color");

        static readonly CustomStyleProperty<float> k_UssShadowOffsetX = new CustomStyleProperty<float>("--box-shadow-offset-x");

        static readonly CustomStyleProperty<float> k_UssShadowOffsetY = new CustomStyleProperty<float>("--box-shadow-offset-y");

        static readonly CustomStyleProperty<int> k_UssShadowType = new CustomStyleProperty<int>("--box-shadow-type");

        static readonly CustomStyleProperty<float> k_UssShadowSpread = new CustomStyleProperty<float>("--box-shadow-spread");

        static readonly CustomStyleProperty<float> k_UssShadowBlur = new CustomStyleProperty<float>("--box-shadow-blur");

        static readonly CustomStyleProperty<Color> k_UssShadowColor = new CustomStyleProperty<Color>("--box-shadow-color");

        AdditionalStyle m_Style;

        static readonly int k_BorderThickness = Shader.PropertyToID("_BorderThickness");

        static readonly int k_BorderColor = Shader.PropertyToID("_BorderColor");

        static readonly int k_ShadowColor = Shader.PropertyToID("_ShadowColor");

        static readonly int k_ShadowInset = Shader.PropertyToID("_ShadowInset");

        Color? m_OutlineColorByCode;

        Vector2 m_PreviousSize;

        static readonly int k_Ratio = Shader.PropertyToID("_Ratio");

        static readonly int k_BackgroundImageTransform = Shader.PropertyToID("_BackgroundImageTransform");

        static readonly int k_BackgroundSize = Shader.PropertyToID("_BackgroundSize");

        Color? m_BackgroundColorByCode;

        Passes m_PassMask = (Passes)0xFF;

        static Handler s_Handler;

        static ExVisualElement()
        {
            k_Vertices[0].tint = Color.white;
            k_Vertices[1].tint = Color.white;
            k_Vertices[2].tint = Color.white;
            k_Vertices[3].tint = Color.white;
        }

        /// <summary>
        /// The outline color of this element. Setting this will override the outline color defined in the USS.
        /// </summary>
        public Color? outlineColor
        {
            get => m_OutlineColorByCode;
            set
            {
                if (m_OutlineColorByCode == value)
                    return;

                m_OutlineColorByCode = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The background color of this element. Setting this will override the background color defined in the USS.
        /// </summary>
        public Color? backgroundColor
        {
            get => m_BackgroundColorByCode;
            set
            {
                if (m_BackgroundColorByCode == value)
                    return;

                m_BackgroundColorByCode = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// The mask of passes to render.
        /// </summary>
        public Passes passMask
        {
            get => m_PassMask;
            set
            {
                m_PassMask = value;
                MarkDirtyRepaint();
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ExVisualElement()
        {
            generateVisualContent = GenerateVisualContentInternal;
            RegisterCallback<CustomStyleResolvedEvent>(OnStylesResolved);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            if (m_RT)
                m_RT.Release();

            m_RT = null;
        }

        void GenerateVisualContentInternal(MeshGenerationContext mgc)
        {
            m_Style.outlineColor = m_OutlineColorByCode ?? m_Style.outlineColor;
            m_Style.backgroundColor = m_BackgroundColorByCode ?? m_Style.backgroundColor;
            GenerateVisualContent(mgc);
        }

        void OnStylesResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(k_UssOutlineColor, out var ussOutlineColor))
                m_Style.outlineColor = ussOutlineColor;
            else
                m_Style.outlineColor = Color.clear;

            if (evt.customStyle.TryGetValue(k_UssOutlineOffset, out var ussOutlineOffset))
                m_Style.outlineOffset = ussOutlineOffset;
            else
                m_Style.outlineOffset = 0;

            if (evt.customStyle.TryGetValue(k_UssBorderWidth, out var ussBorderWidth))
                m_Style.borderWidth = ussBorderWidth;
            else
                m_Style.borderWidth = 0;

            if (evt.customStyle.TryGetValue(k_UssBorderColor, out var ussBorderColor))
                m_Style.borderColor = ussBorderColor;
            else
                m_Style.borderColor = Color.clear;

            if (evt.customStyle.TryGetValue(k_UssBackgroundColor, out var ussBackgroundColor))
                m_Style.backgroundColor = ussBackgroundColor;
            else
                m_Style.backgroundColor = Color.clear;

            if (evt.customStyle.TryGetValue(k_UssOutlineWidth, out var ussOutlineWidth))
                m_Style.outlineWidth = ussOutlineWidth;
            else
                m_Style.outlineWidth = 0;

            if (evt.customStyle.TryGetValue(k_UssShadowOffsetX, out var shadowOffsetX))
                m_Style.shadowOffset.x = shadowOffsetX;
            else
                m_Style.shadowOffset.x = 0;

            if (evt.customStyle.TryGetValue(k_UssShadowOffsetY, out var shadowOffsetY))
                m_Style.shadowOffset.y = shadowOffsetY;
            else
                m_Style.shadowOffset.y = 0;

            if (evt.customStyle.TryGetValue(k_UssShadowType, out var shadowType))
                m_Style.shadowInset = shadowType == 1;
            else
                m_Style.shadowInset = false;

            if (evt.customStyle.TryGetValue(k_UssShadowSpread, out var shadowSpread))
                m_Style.shadowSpread = shadowSpread;
            else
                m_Style.shadowSpread = 0;

            if (evt.customStyle.TryGetValue(k_UssShadowBlur, out var shadowBlur))
                m_Style.shadowBlur = shadowBlur;
            else
                m_Style.shadowBlur = 0;

            if (evt.customStyle.TryGetValue(k_UssShadowColor, out var shadowColor))
                m_Style.shadowColor = shadowColor;
            else
                m_Style.shadowColor = Color.clear;

            MarkDirtyRepaint();
        }

        void GenerateVisualContent(MeshGenerationContext mgc)
        {
            if (!s_Material)
                s_Material = new Material(Shader.Find("Hidden/App UI/Box"));

            if (!paddingRect.IsValid() || passMask == 0)
            {
                if (m_RT)
                {
                    m_RT.Release();
                    m_RT = null;
                }

                return;
            }

            var renderRect = ComputeContentRect(paddingRect, m_Style);
            var rtSize = GetRenderTextureSize(renderRect);

            if (rtSize.x < 1 || rtSize.y < 1)
                return;

            if (m_RT && (m_RT.width != rtSize.x || m_RT.height != rtSize.y))
            {
                m_RT.Release();
                m_RT = null;
            }

            if (!m_RT)
            {
                m_RT = new RenderTexture(rtSize.x, rtSize.y, 24);
                m_RT.Create();
            }
            
            handler.SendMessage(Message.Obtain(handler, k_BlitTextureMessageId, this));

            var left = renderRect.xMin;
            var right = renderRect.xMax;
            var top = renderRect.yMin;
            var bottom = renderRect.yMax;

            k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
            k_Vertices[1].position = new Vector3(left, top, Vertex.nearZ);
            k_Vertices[2].position = new Vector3(right, top, Vertex.nearZ);
            k_Vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);

            var mwd = mgc.Allocate(k_Vertices.Length, k_Indices.Length, m_RT);

#if !UNITY_2023_1_OR_NEWER
            // Since the texture may be stored in an atlas, the UV coordinates need to be
            // adjusted. Simply rescale them in the provided uvRegion.
            var uvRegion = mwd.uvRegion;
#else
            var uvRegion = new Rect(0, 0, 1, 1);
#endif
            k_Vertices[0].uv = new Vector2(0, 0) * uvRegion.size + uvRegion.min;
            k_Vertices[1].uv = new Vector2(0, 1) * uvRegion.size + uvRegion.min;
            k_Vertices[2].uv = new Vector2(1, 1) * uvRegion.size + uvRegion.min;
            k_Vertices[3].uv = new Vector2(1, 0) * uvRegion.size + uvRegion.min;

            mwd.SetAllVertices(k_Vertices);
            mwd.SetAllIndices(k_Indices);
        }

        static Vector2Int GetRenderTextureSize(Rect renderRect)
        {
            var dpi = Mathf.Clamp(Platform.mainScreenScale, 1, 2);
            int width, height;

            if (renderRect.height > renderRect.width)
            {
                height = (int)Mathf.Clamp(renderRect.height * dpi, 16, 1024);
                width = (int)(height * (renderRect.width / renderRect.height));
            }
            else
            {
                width = (int)Mathf.Clamp(renderRect.width * dpi, 16, 1024);
                height = (int)(width * (renderRect.height / renderRect.width));
            }
            
            return new Vector2Int(width, height);
        }

        static Rect ComputeContentRect(Rect rect, AdditionalStyle ads)
        {
            // shrink the rect by 1 pixel to avoid bleeding
            rect = new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2);

            // calculate the area that the outline will be drawn in
            var outlineRadius = new Vector2(ads.outlineOffset + ads.outlineWidth + 2f, ads.outlineOffset + ads.outlineWidth + 2f);
            var outlineRect = new Rect(-outlineRadius,
                rect.size + outlineRadius * 2f);

            // calculate the area that the shadow will be drawn in
            var shadowX = rect.position.x + ads.shadowOffset.x - ads.shadowSpread - ads.shadowBlur;
            var shadowY = rect.position.y + ads.shadowOffset.y - ads.shadowSpread - ads.shadowBlur;
            var shadowWidth = rect.width + Mathf.Max(0, ads.shadowSpread) * 2f + Mathf.Max(0, ads.shadowBlur) * 2f;
            var shadowHeight = rect.height + Mathf.Max(0, ads.shadowSpread) * 2f + Mathf.Max(0, ads.shadowBlur) * 2f;
            var shadowRect = new Rect(new Vector2(shadowX, shadowY), new Vector2(shadowWidth, shadowHeight));

            // compute the overall area that will be drawn in
            var xMin = Mathf.Min(rect.xMin, outlineRect.xMin, shadowRect.xMin) - 2f;
            var xMax = Mathf.Max(rect.xMax, outlineRect.xMax, shadowRect.xMax) + 2f;
            var yMin = Mathf.Min(rect.yMin, outlineRect.yMin, shadowRect.yMin) - 2f;
            var yMax = Mathf.Max(rect.yMax, outlineRect.yMax, shadowRect.yMax) + 2f;

            // Compute the size based on the aspect ratio of the rect
            var width = Mathf.Abs(xMax - xMin);
            var height = Mathf.Abs(yMax - yMin);

            var biggestRect = new Rect(new Vector2(xMin, yMin) - Vector2.one, new Vector2(width, height));

            return biggestRect;
        }

        static Rect PrepareMaterial(Rect rect, IResolvedStyle currentStyle, AdditionalStyle ads, Passes passMask)
        {
            var biggestRect = ComputeContentRect(rect, ads);
            var width = biggestRect.width;
            var height = biggestRect.height;

            // Compute the UV coordinates of the rect (the normalized coordinates of the rect in the biggest rect)
            var rectRect = new Vector4(
                (rect.center.x - biggestRect.x) / biggestRect.width,
                (rect.center.y - biggestRect.y) / biggestRect.height,
                rect.width / biggestRect.width,
                rect.height / biggestRect.height);

            s_Material.SetVector(k_Rect, rectRect);
            s_Material.SetFloat(k_OutlineThickness, ads.outlineWidth / width);
            s_Material.SetColor(k_OutlineColor, ads.outlineColor);
            s_Material.SetFloat(k_OutlineOffset, (ads.outlineOffset + ads.outlineWidth) / width);
            s_Material.SetFloat(k_AASoftness, 1f / width);
            s_Material.SetFloat(k_Ratio, height / width);

            if ((passMask & Passes.Borders) > 0)
                s_Material.EnableKeyword("ENABLE_BORDER");
            else
                s_Material.DisableKeyword("ENABLE_BORDER");
            if ((passMask & Passes.InsetShadows) > 0 || (passMask & Passes.OutsetShadows) > 0)
                s_Material.EnableKeyword("ENABLE_SHADOW");
            else
                s_Material.DisableKeyword("ENABLE_SHADOW");
            if ((passMask & Passes.Outline) > 0)
                s_Material.EnableKeyword("ENABLE_OUTLINE");
            else
                s_Material.DisableKeyword("ENABLE_OUTLINE");

            s_Material.SetVector(k_ShadowOffset, new Vector4(
                ads.shadowOffset.x / width,
                ads.shadowOffset.y / height,
                ads.shadowSpread / width, ads.shadowBlur / width));

            s_Material.SetColor(k_ShadowColor, ads.shadowColor);

            s_Material.SetInt(k_ShadowInset, ads.shadowInset ? 1 : 0);

            // Compute the border radiuses and clamp them to the half size of the rect
            var cornerLimit = Mathf.Min(rect.width, rect.height);
            var borderRadius = new Vector4(
                Mathf.Min(currentStyle.borderTopLeftRadius, cornerLimit),
                Mathf.Min(currentStyle.borderTopRightRadius, cornerLimit),
                Mathf.Min(currentStyle.borderBottomRightRadius, cornerLimit),
                Mathf.Min(currentStyle.borderBottomLeftRadius, cornerLimit));
            borderRadius *= 0.5f;

            s_Material.SetVector(k_Radiuses, borderRadius / width);

            s_Material.SetFloat(k_BorderThickness, ads.borderWidth / width);

            s_Material.SetColor(k_Color, ads.backgroundColor);
            var bgTex = currentStyle.backgroundImage.texture;
            s_Material.SetTexture(k_BackgroundImage, bgTex);
            if (bgTex)
            {
                s_Material.SetVector(k_BackgroundImageTransform, new Vector4(0, 0, bgTex.width, bgTex.height));
                s_Material.SetInt(k_BackgroundSize, 0);
            }

            s_Material.SetColor(k_BorderColor, ads.borderColor);

            return biggestRect;
        }

        /// <summary>
        /// Factory class to instantiate a <see cref="ExVisualElement"/> using the data read from a UXML file.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<ExVisualElement, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ExVisualElement"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits { }
    }
}
