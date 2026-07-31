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
    /// A progress indicator that displays progress linearly along a horizontal bar.
    /// </summary>
    /// <remarks>
    /// Linear Progress indicators express an unspecified wait time or display the length of a process. They help
    /// users visualize the progression of an operation or activity in your application.
    ///
    /// The component supports two variants: determinate and indeterminate. Use determinate progress indicators when
    /// the wait time or completion percentage is known. For operations where the wait time is unknown, use the
    /// indeterminate mode which displays a continuous animation.
    ///
    /// Note: Progress indicators inform users about the status of ongoing processes, such as loading an app,
    /// submitting a form, or saving updates.
    ///
    /// The LinearProgress component inherits from the base Progress class and implements additional features
    /// specific to linear representation of progress.
    /// </remarks>
    /// <example>
    /// <para>Basic indeterminate progress: Shows an animated progress bar for operations with unknown duration</para>
    /// <code lang="xml"><![CDATA[
    /// <LinearProgress variant="Indeterminate" />
    /// ]]></code>
    /// <para>Determinate progress with buffer: Shows a progress bar with both primary and buffer progress, useful for
    /// media playback scenarios</para>
    /// <code lang="xml"><![CDATA[
    /// <LinearProgress
    ///     variant="Determinate"
    ///     value="0.4"
    ///     bufferValue="0.6"
    ///     size="M"
    ///     roundedProgressCorners="true"
    /// />
    /// ]]></code>
    /// <para>Programmatic usage example: Creating and configuring a progress bar through code</para>
    /// <code lang="csharp"><![CDATA[
    /// var progress = new LinearProgress();
    /// progress.variant = Variant.Determinate;
    /// progress.value = 0.75f;
    /// progress.size = Size.L;
    /// progress.colorOverride = new Color(0.2f, 0.6f, 1f, 1f);
    /// ]]></code>
    /// <para>Styling with USS: Customizing the appearance using USS styles</para>
    /// <code lang="csharp"><![CDATA[
    /// .custom-progress {
    ///     --progress-color: rgb(25, 118, 210);
    ///     width: 200px;
    ///     margin: 8px;
    /// }
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("feedbacks")]
    public partial class LinearProgress : Progress
    {
        static readonly int k_Start = Shader.PropertyToID("_Start");

        static readonly int k_End = Shader.PropertyToID("_End");

        static readonly int k_Rounded = Shader.PropertyToID("_Rounded");

        static readonly int k_BufferStart = Shader.PropertyToID("_BufferStart");

        static readonly int k_BufferEnd = Shader.PropertyToID("_BufferEnd");

        static readonly int k_Color = Shader.PropertyToID("_Color");

        static readonly int k_AA = Shader.PropertyToID("_AA");

        static readonly int k_Ratio = Shader.PropertyToID("_Ratio");

        static readonly int k_Padding = Shader.PropertyToID("_Padding");

        static readonly int k_BufferOpacity = Shader.PropertyToID("_BufferOpacity");

        static Material s_Material;

        /// <summary>
        /// The Progress main styling class.
        /// </summary>
        public new const string ussClassName = "appui-linear-progress";

        static readonly int k_Phase = Shader.PropertyToID("_Phase");

        /// <summary>
        /// Default constructor.
        /// </summary>
        public LinearProgress()
        {
            AddToClassList(ussClassName);
        }

        /// <summary>
        /// Generates the textures for the progress element.
        /// </summary>
        protected override void GenerateTextures()
        {
            if (!s_Material)
            {
                s_Material = MaterialUtils.CreateMaterial("Hidden/App UI/LinearProgress");
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
                m_RT = RenderTexture.GetTemporary((int)rectSize.x, (int)rectSize.y, 24);

            s_Material.SetColor(k_Color, colorOverride);
            s_Material.SetInt(k_Rounded, roundedProgressCorners ? 1 : 0);
            s_Material.SetFloat(k_Start, 0);
            s_Material.SetFloat(k_End, value);
            s_Material.SetFloat(k_BufferStart, 0);
            s_Material.SetFloat(k_BufferEnd, bufferValue);
            s_Material.SetFloat(k_BufferOpacity, bufferOpacity);
            s_Material.SetFloat(k_AA, 2.0f / rectSize.x);
            s_Material.SetVector(k_Phase, TimeUtils.GetCurrentTimeVector());
            s_Material.SetFloat(k_Ratio, rectSize.x / rectSize.y);
            s_Material.SetFloat(k_Padding, roundedProgressCorners ? rect.height * 0.5f / rect.width : 0);
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
