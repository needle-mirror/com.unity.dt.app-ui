using Markdig;
using UnityEngine.UIElements;
using Unity.AppUI.Markdown;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Renders a markdown string as a tree of App UI UITK components. Parses with
    /// <see cref="Markdig.Markdown"/> using a default pipeline of advanced extensions + autolinks,
    /// then walks the document with a <see cref="UITKMarkdownRenderer"/>.
    /// </summary>
    [UxmlElement]
    public partial class MarkdownView : BaseVisualElement
    {
        /// <summary>
        /// The MarkdownView main USS class name.
        /// </summary>
        public const string ussClassName = "appui-markdown-view";

        internal static readonly BindingId contentProperty = nameof(content);

        static readonly MarkdownPipeline s_DefaultPipeline =
            new MarkdownPipelineBuilder().UseAdvancedExtensions().UseAutoLinks().Build();

        string m_Content = string.Empty;

        MarkdownPipeline m_Pipeline = s_DefaultPipeline;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MarkdownView()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
        }

        /// <summary>
        /// The raw markdown source to render.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string content
        {
            get => m_Content;
            set
            {
                var changed = m_Content != value;
                m_Content = value ?? string.Empty;
                Refresh();
                if (changed)
                    NotifyPropertyChanged(in contentProperty);
            }
        }

        /// <summary>
        /// The Markdig pipeline used for parsing. Defaults to advanced extensions + autolinks.
        /// </summary>
        public MarkdownPipeline pipeline
        {
            get => m_Pipeline;
            set
            {
                m_Pipeline = value ?? s_DefaultPipeline;
                Refresh();
            }
        }

        void Refresh()
        {
            hierarchy.Clear();
            if (string.IsNullOrEmpty(m_Content))
                return;

            var doc = global::Markdig.Markdown.Parse(m_Content, m_Pipeline);
            new UITKMarkdownRenderer(this).Render(doc);
        }

    }
}
