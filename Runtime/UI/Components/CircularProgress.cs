using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// CircularProgress UI element. This is a circular progress bar.
    /// </summary>
    public class CircularProgress : Progress
    {
        static Material s_Material;

        /// <summary>
        /// The CircularProgress main styling class.
        /// </summary>
        public new static readonly string ussClassName = "appui-circular-progress";

        static readonly int k_InnerRadius = Shader.PropertyToID("_InnerRadius");

        static readonly int k_Start = Shader.PropertyToID("_Start");

        static readonly int k_End = Shader.PropertyToID("_End");

        static readonly int k_BufferStart = Shader.PropertyToID("_BufferStart");

        static readonly int k_BufferEnd = Shader.PropertyToID("_BufferEnd");

        static readonly int k_Color = Shader.PropertyToID("_Color");

        static readonly int k_AA = Shader.PropertyToID("_AA");

        static readonly int k_BufferOpacity = Shader.PropertyToID("_BufferOpacity");

        static readonly int k_Phase = Shader.PropertyToID("_Phase");
        
        /// <summary>
        /// The inner radius of the CircularProgress.
        /// </summary>
        public float innerRadius { get; set; } = 0.38f;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CircularProgress()
        {
            AddToClassList(ussClassName);
        }

        /// <summary>
        /// Generates the textures for the CircularProgress.
        /// </summary>
        protected override void GenerateTextures()
        {
            if (!s_Material)
            {
                s_Material = MaterialUtils.CreateMaterial("Hidden/App UI/CircularProgress");
                if (!s_Material)
                    return;
            }
            
            var rect = contentRect;

            if (!rect.IsValid())
                return;

            var dpi = Mathf.Max(Platform.mainScreenScale, 1f);
            var rectSize = rect.size * dpi;

            if (!rectSize.IsValidForTextureSize())
                return;

            if (m_RT && (Mathf.Abs(m_RT.width - rectSize.x) > 1 || Mathf.Abs(m_RT.height - rectSize.y) > 1))
            {
                m_RT.Release();
                UnityObject.Destroy(m_RT);
                m_RT = null;
            }

            if (!m_RT)
            {
                m_RT = new RenderTexture((int) rectSize.x, (int) rectSize.y, 24)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                m_RT.Create();
            }

            var time = Application.isEditor ?
#if UNITY_EDITOR
                (float)EditorApplication.timeSinceStartup
#else
                Time.time
#endif
                : Time.time;

            s_Material.SetColor(k_Color, colorOverride ?? m_Color);
            s_Material.SetFloat(k_InnerRadius, innerRadius);
            s_Material.SetFloat(k_Start, 0);
            s_Material.SetFloat(k_End, value);
            s_Material.SetFloat(k_BufferStart, 0);
            s_Material.SetFloat(k_BufferEnd, bufferValue);
            s_Material.SetFloat(k_AA, 2.0f / rectSize.x);
            s_Material.SetVector(k_Phase, new Vector4(time / 20, time, time * 2, time * 3));
            s_Material.SetFloat(k_BufferOpacity, bufferOpacity);
            if (variant == Variant.Indeterminate)
                s_Material.EnableKeyword("PROGRESS_INDETERMINATE");
            else
                s_Material.DisableKeyword("PROGRESS_INDETERMINATE");

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;
        }

        /// <summary>
        /// Defines the UxmlFactory for the CircularProgress.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<CircularProgress, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="CircularProgress"/>.
        /// </summary>
        public new class UxmlTraits : Progress.UxmlTraits { }
    }
}
