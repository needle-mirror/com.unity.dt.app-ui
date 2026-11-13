using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How an <see cref="AudioPreview"/> lays out its control, envelope and length.
    /// </summary>
    public enum AudioPreviewLayout
    {
        /// <summary>
        /// The envelope fills the component, with the control and the length laid over its
        /// corners.
        /// </summary>
        Overlay,

        /// <summary>
        /// One compact row: the control first, a short envelope, then the length. For a panel that
        /// shows the clip above its details rather than as the subject.
        /// </summary>
        Strip,
    }

    /// <summary>
    /// An audio envelope with a play/stop control and an optional length readout.
    /// </summary>
    /// <remarks>
    /// <para>
    /// AudioPreview auditions a single clip. It is purely presentational: the caller owns the peak
    /// data, the playback state, the playhead and every string. Nothing here decodes or plays
    /// audio, so the same component serves an asset row, an inspector and a picker tile.
    /// </para>
    /// <para>
    /// The body has a precedence order. <see cref="message"/> replaces the envelope outright and
    /// beats everything; <see cref="loading"/> shows a <see cref="Skeleton"/> while the envelope is
    /// being computed; a null <see cref="peaks"/> with a <see cref="fallbackIcon"/> set draws that
    /// symbol instead; otherwise the envelope is drawn by <see cref="AudioWaveform"/>. A null
    /// <see cref="peaks"/> with no fallback still draws, as a flat baseline — a clip whose envelope
    /// was never computed is still playable, and an empty box would read as a broken component
    /// rather than an unanalysed one.
    /// </para>
    /// <para>
    /// The message, loading and fallback states are all driven from props precisely so a consumer
    /// that cannot compute an envelope — too large, decode refused, still fetching — or has chosen
    /// not to, can say so in its own words or with its own symbol without this component knowing
    /// why.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>A clip with a computed envelope and a playhead.</para>
    /// <code lang="csharp">
    /// var preview = new AudioPreview
    /// {
    ///     peaks = envelope,
    ///     progress = 0.4f,
    ///     durationLabel = "0:12",
    /// };
    /// preview.toggled += () => TogglePlayback(preview);
    /// parentElement.Add(preview);
    /// </code>
    /// <para>A clip too large to analyse, naming the medium instead of drawing a flat line.</para>
    /// <code lang="xml">
    /// &lt;appui:AudioPreview message="Clip too large to preview" duration-label="4:03" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("media")]
    public partial class AudioPreview : BaseVisualElement
    {
        internal static readonly BindingId playingProperty = new BindingId(nameof(playing));

        internal static readonly BindingId progressProperty = new BindingId(nameof(progress));

        internal static readonly BindingId durationLabelProperty = new BindingId(nameof(durationLabel));

        internal static readonly BindingId messageProperty = new BindingId(nameof(message));

        internal static readonly BindingId loadingProperty = new BindingId(nameof(loading));

        internal static readonly BindingId fallbackIconProperty = new BindingId(nameof(fallbackIcon));

        internal static readonly BindingId playLabelProperty = new BindingId(nameof(playLabel));

        internal static readonly BindingId stopLabelProperty = new BindingId(nameof(stopLabel));

        internal static readonly BindingId previewLayoutProperty = new BindingId(nameof(previewLayout));

        /// <summary>
        /// The AudioPreview main styling class.
        /// </summary>
        public const string ussClassName = "appui-audio-preview";

        /// <summary>
        /// The styling class applied in the <see cref="AudioPreviewLayout.Strip"/> layout.
        /// </summary>
        public const string stripUssClassName = ussClassName + "--strip";

        /// <summary>
        /// The styling class of the body, which holds whichever of the four states is showing.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>
        /// The styling class of the envelope.
        /// </summary>
        public const string waveformUssClassName = ussClassName + "__waveform";

        /// <summary>
        /// The styling class applied to the envelope while it is a flat, uncomputed baseline.
        /// </summary>
        public const string flatUssClassName = waveformUssClassName + "--flat";

        /// <summary>
        /// The styling class of the loading placeholder.
        /// </summary>
        public const string skeletonUssClassName = ussClassName + "__skeleton";

        /// <summary>
        /// The styling class of the message shown in place of the envelope.
        /// </summary>
        public const string messageUssClassName = ussClassName + "__message";

        /// <summary>
        /// The styling class of the fallback symbol shown in place of the envelope.
        /// </summary>
        public const string fallbackIconUssClassName = ussClassName + "__fallback-icon";

        /// <summary>
        /// The styling class of the play/stop control.
        /// </summary>
        public const string playButtonUssClassName = ussClassName + "__play-button";

        /// <summary>
        /// The styling class of the length readout.
        /// </summary>
        public const string durationUssClassName = ussClassName + "__duration";

        /// <summary>
        /// The number of bars drawn for a clip with no computed envelope, and the height each one
        /// takes. Mirrors the registry's <c>FLAT_PEAKS</c>.
        /// </summary>
        internal const int flatPeakCount = 64;

        /// <summary>
        /// The amplitude of a flat baseline bar. Not zero, so the line is visible.
        /// </summary>
        internal const float flatPeakAmplitude = 0.02f;

        static readonly List<float> k_FlatPeaks = BuildFlatPeaks();

        readonly VisualElement m_Body;

        readonly AudioWaveform m_Waveform;

        readonly Skeleton m_Skeleton;

        readonly LocalizedTextElement m_Message;

        readonly Icon m_FallbackIconElement;

        readonly IconButton m_PlayButton;

        readonly LocalizedTextElement m_Duration;

        List<float> m_Peaks;

        bool m_Playing;

        bool m_Loading;

        string m_FallbackIcon;

        string m_PlayLabel = "Play preview";

        string m_StopLabel = "Stop preview";

        AudioPreviewLayout m_Layout = AudioPreviewLayout.Overlay;

        /// <summary>
        /// Raised when the play/stop control is activated. The caller owns playback, so this only
        /// reports the intent to toggle; set <see cref="playing"/> to reflect the new state.
        /// </summary>
        public event Action toggled;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public AudioPreview()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_Body = new VisualElement { name = bodyUssClassName, pickingMode = PickingMode.Ignore };
            m_Body.AddToClassList(bodyUssClassName);
            hierarchy.Add(m_Body);

            m_Waveform = new AudioWaveform { name = waveformUssClassName, pickingMode = PickingMode.Ignore };
            m_Waveform.AddToClassList(waveformUssClassName);

            m_Skeleton = new Skeleton { name = skeletonUssClassName };
            m_Skeleton.AddToClassList(skeletonUssClassName);

            m_Message = new LocalizedTextElement { name = messageUssClassName, pickingMode = PickingMode.Ignore };
            m_Message.AddToClassList(messageUssClassName);

            m_FallbackIconElement = new Icon { name = fallbackIconUssClassName, pickingMode = PickingMode.Ignore };
            m_FallbackIconElement.AddToClassList(fallbackIconUssClassName);

            m_PlayButton = new IconButton("play", OnPlayButtonClicked) { name = playButtonUssClassName };
            m_PlayButton.AddToClassList(playButtonUssClassName);
            hierarchy.Add(m_PlayButton);

            m_Duration = new LocalizedTextElement { name = durationUssClassName, pickingMode = PickingMode.Ignore };
            m_Duration.AddToClassList(durationUssClassName);
            hierarchy.Add(m_Duration);

            RefreshBody();
            RefreshPlayButton();
            RefreshDuration();
        }

        /// <summary>
        /// Normalized peak amplitudes, or null when there are none to draw.
        /// </summary>
        /// <remarks>
        /// A null envelope is not an error state. With no <see cref="fallbackIcon"/> set it draws a
        /// flat baseline, because the clip is still playable.
        /// </remarks>
        [CreateProperty]
        public List<float> peaks
        {
            get => m_Peaks;
            set
            {
                m_Peaks = value;
                RefreshBody();
            }
        }

        /// <summary>
        /// Playhead position as a 0..1 fraction. Unset hides it.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> progress
        {
            get => m_Waveform.progress;
            set
            {
                var changed = m_Waveform.progress != value;
                m_Waveform.progress = value;

                if (changed)
                    NotifyPropertyChanged(in progressProperty);
            }
        }

        /// <summary>
        /// Whether the clip is currently playing. Drives which symbol and label the control shows.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool playing
        {
            get => m_Playing;
            set
            {
                var changed = m_Playing != value;
                m_Playing = value;
                RefreshPlayButton();

                if (changed)
                    NotifyPropertyChanged(in playingProperty);
            }
        }

        /// <summary>
        /// A pre-formatted length, rendered beside the control. Empty hides the readout.
        /// </summary>
        /// <remarks>
        /// Pre-formatted on purpose: the host already knows whether it wants <c>0:12</c>,
        /// <c>12s</c> or a sample count, and this component has no clip to measure.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string durationLabel
        {
            get => m_Duration.text;
            set
            {
                var changed = m_Duration.text != value;
                m_Duration.text = value;
                RefreshDuration();

                if (changed)
                    NotifyPropertyChanged(in durationLabelProperty);
            }
        }

        /// <summary>
        /// Text shown in place of the envelope. Takes precedence over <see cref="loading"/> and
        /// over the envelope itself.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string message
        {
            get => m_Message.text;
            set
            {
                var changed = m_Message.text != value;
                m_Message.text = value;
                RefreshBody();

                if (changed)
                    NotifyPropertyChanged(in messageProperty);
            }
        }

        /// <summary>
        /// Whether to show a placeholder while the envelope is being computed.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool loading
        {
            get => m_Loading;
            set
            {
                var changed = m_Loading != value;
                m_Loading = value;
                RefreshBody();

                if (changed)
                    NotifyPropertyChanged(in loadingProperty);
            }
        }

        /// <summary>
        /// The name of a symbol drawn in place of the envelope while <see cref="peaks"/> is null.
        /// </summary>
        /// <remarks>
        /// For a consumer that has no peak data and would rather name the medium than draw a flat
        /// line that reads as a silent recording. Named, not styled, by the consumer: the size and
        /// colour stay here so every such placeholder matches.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string fallbackIcon
        {
            get => m_FallbackIcon;
            set
            {
                var changed = m_FallbackIcon != value;
                m_FallbackIcon = value;
                // Tracked separately from the Icon's own name, which defaults to "info": reading
                // it back would make every null envelope look like a caller-supplied fallback.
                if (!string.IsNullOrEmpty(value))
                    m_FallbackIconElement.iconName = value;
                RefreshBody();

                if (changed)
                    NotifyPropertyChanged(in fallbackIconProperty);
            }
        }

        /// <summary>
        /// The accessible label of the control while the clip is stopped.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string playLabel
        {
            get => m_PlayLabel;
            set
            {
                var changed = m_PlayLabel != value;
                m_PlayLabel = value;
                RefreshPlayButton();

                if (changed)
                    NotifyPropertyChanged(in playLabelProperty);
            }
        }

        /// <summary>
        /// The accessible label of the control while the clip is playing.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string stopLabel
        {
            get => m_StopLabel;
            set
            {
                var changed = m_StopLabel != value;
                m_StopLabel = value;
                RefreshPlayButton();

                if (changed)
                    NotifyPropertyChanged(in stopLabelProperty);
            }
        }

        /// <summary>
        /// How the control, the envelope and the length are laid out.
        /// </summary>
        /// <remarks>
        /// <see cref="AudioPreviewLayout.Strip"/> has no room for a symbol, so a null
        /// <see cref="peaks"/> draws the flat baseline there even when <see cref="fallbackIcon"/>
        /// is set.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute("layout")]
        public AudioPreviewLayout previewLayout
        {
            get => m_Layout;
            set
            {
                var changed = m_Layout != value;
                m_Layout = value;
                var strip = value == AudioPreviewLayout.Strip;
                EnableInClassList(stripUssClassName, strip);
                // The strip reads control, envelope, length; the overlay draws the envelope first
                // and lays the control over its corner.
                if (strip)
                    m_PlayButton.SendToBack();
                else
                    m_Body.SendToBack();
                RefreshBody();

                if (changed)
                    NotifyPropertyChanged(in previewLayoutProperty);
            }
        }

        /// <summary>
        /// Which of the four body states is currently showing.
        /// </summary>
        internal VisualElement currentBody => m_Body.childCount > 0 ? m_Body[0] : null;

        void OnPlayButtonClicked()
        {
            toggled?.Invoke();
        }

        void RefreshPlayButton()
        {
            m_PlayButton.icon = m_Playing ? "stop" : "play";
            m_PlayButton.tooltip = m_Playing ? m_StopLabel : m_PlayLabel;
        }

        void RefreshDuration()
        {
            m_Duration.style.display = string.IsNullOrEmpty(m_Duration.text)
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }

        void RefreshBody()
        {
            m_Body.Clear();

            if (!string.IsNullOrEmpty(m_Message.text))
            {
                m_Body.Add(m_Message);
                return;
            }

            if (m_Loading)
            {
                m_Body.Add(m_Skeleton);
                return;
            }

            // A strip's envelope is too short to hold a symbol, so it keeps the flat baseline.
            if (m_Peaks == null && !string.IsNullOrEmpty(m_FallbackIcon) && m_Layout != AudioPreviewLayout.Strip)
            {
                m_Body.Add(m_FallbackIconElement);
                return;
            }

            var flat = m_Peaks == null;
            m_Waveform.peaks = flat ? k_FlatPeaks : m_Peaks;
            m_Waveform.EnableInClassList(flatUssClassName, flat);
            m_Body.Add(m_Waveform);
        }

        static List<float> BuildFlatPeaks()
        {
            var peaks = new List<float>(flatPeakCount);
            for (var i = 0; i < flatPeakCount; i++)
                peaks.Add(flatPeakAmplitude);
            return peaks;
        }
    }
}
