using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A circular progress indicator that represents loading or processing state.
    /// </summary>
    /// <remarks>
    /// CircularProgress is a visual indicator that displays progress in a circular format. It can be used to
    /// show the status of an operation, like loading content, processing data, or uploading files.
    ///
    /// The component supports two variants: determinate and indeterminate. The determinate variant shows
    /// concrete progress with a specific value, while the indeterminate variant displays a continuous animation
    /// indicating an ongoing process without a specific completion percentage.
    ///
    /// Use CircularProgress when you want to show progress in a compact, circular format. It's particularly
    /// useful in scenarios where space is limited or when you want to maintain visual harmony with circular
    /// design elements.
    ///
    /// The component features customizable properties including inner radius, rounded corners, and color
    /// styling to match your application's design system.
    /// </remarks>
    /// <example>
    /// <para>Basic indeterminate circular progress. Creates a default circular progress with indeterminate animation.</para>
    /// <code lang="xml"><![CDATA[
    /// <CircularProgress />
    /// ]]></code>
    /// <para>Determinate progress with custom styling. Creates a large determinate progress indicator with custom
    /// color and thickness.</para>
    /// <code lang="xml"><![CDATA[
    /// <CircularProgress variant="Determinate" value="0.75" inner-radius="0.4" color-override="#2196F3" size="L" />
    /// ]]></code>
    /// <para>Progress with code-behind interaction. Demonstrates how to create and update a circular progress
    /// indicator programmatically.</para>
    /// <code lang="csharp"><![CDATA[
    /// var progress = new CircularProgress();
    /// progress.variant = Progress.Variant.Determinate;
    /// progress.value = 0.0f;
    ///
    /// // Update progress value over time
    /// float currentValue = 0f;
    /// void UpdateProgress() {
    ///     currentValue = Mathf.Min(currentValue + 0.1f, 1f);
    ///     progress.value = currentValue;
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class CircularProgress : Progress
    {

        internal static readonly BindingId innerRadiusProperty = nameof(innerRadius);


        static Material s_Material;

        /// <summary>
        /// The CircularProgress main styling class.
        /// </summary>
        public new const string ussClassName = "appui-circular-progress";

        static readonly int k_InnerRadius = Shader.PropertyToID("_InnerRadius");

        static readonly int k_Rounded = Shader.PropertyToID("_Rounded");

        static readonly int k_Start = Shader.PropertyToID("_Start");

        static readonly int k_End = Shader.PropertyToID("_End");

        static readonly int k_BufferStart = Shader.PropertyToID("_BufferStart");

        static readonly int k_BufferEnd = Shader.PropertyToID("_BufferEnd");

        static readonly int k_Color = Shader.PropertyToID("_Color");

        static readonly int k_AA = Shader.PropertyToID("_AA");

        static readonly int k_BufferOpacity = Shader.PropertyToID("_BufferOpacity");

        static readonly int k_Phase = Shader.PropertyToID("_Phase");

        float m_InnerRadius = k_DefaultInnerRadius;

        const float k_DefaultInnerRadius = 0.38f;

        /// <summary>
        /// The inner radius of the CircularProgress.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float innerRadius
        {
            get => m_InnerRadius;
            set
            {
                var changed = !Mathf.Approximately(m_InnerRadius, value);
                m_InnerRadius = value;

                if (changed)
                    NotifyPropertyChanged(in innerRadiusProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CircularProgress()
        {
            AddToClassList(ussClassName);

            innerRadius = k_DefaultInnerRadius;
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
                {
                    ReleaseTextures();
                    return;
                }
            }

            var rect = contentRect;

            if (!rect.IsValid())
            {
                ReleaseTextures();
                return;
            }

            var dpi = Mathf.Max(Platform.scaleFactor, 1f);
            var rectSize = rect.size * dpi;

            if (!rectSize.IsValidForTextureSize())
            {
                ReleaseTextures();
                return;
            }

            if (m_RT && (Mathf.Abs(m_RT.width - rectSize.x) > 1 || Mathf.Abs(m_RT.height - rectSize.y) > 1))
                ReleaseTextures();

            if (!m_RT)
                m_RT = RenderTexture.GetTemporary((int) rectSize.x, (int) rectSize.y, 24);

            s_Material.SetColor(k_Color, colorOverride);
            s_Material.SetFloat(k_InnerRadius, innerRadius);
            s_Material.SetInt(k_Rounded, roundedProgressCorners ? 1 : 0);
            s_Material.SetFloat(k_Start, 0);
            s_Material.SetFloat(k_End, value);
            s_Material.SetFloat(k_BufferStart, 0);
            s_Material.SetFloat(k_BufferEnd, bufferValue);
            s_Material.SetFloat(k_AA, 2.0f / rectSize.x);
            s_Material.SetVector(k_Phase, TimeUtils.GetCurrentTimeVector());
            s_Material.SetFloat(k_BufferOpacity, bufferOpacity);
            if (variant == Variant.Indeterminate)
                s_Material.EnableKeyword("PROGRESS_INDETERMINATE");
            else
                s_Material.DisableKeyword("PROGRESS_INDETERMINATE");

            var prevRt = RenderTexture.active;
            Graphics.Blit(null, m_RT, s_Material);
            RenderTexture.active = prevRt;
        }

    }
}
