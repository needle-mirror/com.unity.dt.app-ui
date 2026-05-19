using Markdig;
using UnityEngine.UIElements;
using Unity.AppUI.Markdown;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Renders a markdown string as a tree of App UI UITK components. Parses with
    /// <see cref="Markdig.Markdown"/> using a default pipeline of advanced extensions + autolinks,
    /// then walks the document with a <see cref="UITKMarkdownRenderer"/>.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class MarkdownView : BaseVisualElement
    {
        /// <summary>
        /// The MarkdownView main USS class name.
        /// </summary>
        public const string ussClassName = "appui-markdown-view";

#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId contentProperty = nameof(content);
#endif

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
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string content
        {
            get => m_Content;
            set
            {
                var changed = m_Content != value;
                m_Content = value ?? string.Empty;
                Refresh();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in contentProperty);
#endif
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

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// Factory class to instantiate a <see cref="MarkdownView"/> using the data read from a UXML file.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<MarkdownView, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="MarkdownView"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Content = new UxmlStringAttributeDescription
            {
                name = "content",
                defaultValue = string.Empty,
            };

            /// <summary>
            /// Initializes the <see cref="MarkdownView"/> from the UXML attributes.
            /// </summary>
            /// <param name="ve">The element to initialize.</param>
            /// <param name="bag">The bag of attributes parsed from UXML.</param>
            /// <param name="cc">The creation context.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, UnityEngine.UIElements.CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (MarkdownView)ve;
                element.content = m_Content.GetValueFromBag(bag, cc);
            }
        }

#endif
    }
}
