using System.Text;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif
#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
using Unity.AppUI.TextMateLib;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A reusable code-block VisualElement: a header row with an optional language label and
    /// a copy-to-clipboard button (revealed on hover) above a monospaced body. When App UI's
    /// TextMate integration is enabled (<c>APPUI_ENABLE_SYNTAX_HIGHLIGHTING</c>), the active
    /// grammar and theme are resolved from two USS custom properties on this element:
    /// <list type="bullet">
    /// <item><description><c>--codeblock-grammar</c> — GUID string of a <see cref="TextMateGrammarAsset"/>.</description></item>
    /// <item><description><c>--codeblock-theme</c> — GUID string of a <see cref="TextMateThemeAsset"/>.</description></item>
    /// </list>
    /// Setting <see cref="language"/> automatically toggles a
    /// <c>appui-code-block--lang-&lt;value&gt;</c> class so per-language grammar mappings can be
    /// expressed declaratively in USS (see <c>PackageResources/Styles/Components/CodeBlock.uss</c>).
    /// Asset resolution from GUID requires the editor; in built players the body falls back to
    /// plain monospaced text.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class CodeBlock : BaseVisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId codeProperty = nameof(code);
        internal static readonly BindingId languageProperty = nameof(language);
        internal static readonly BindingId showLineNumbersProperty = nameof(showLineNumbers);
#endif

        /// <summary>The CodeBlock main USS class name.</summary>
        public const string ussClassName = "appui-code-block";

        /// <summary>Prefix of the per-language modifier class added when <see cref="language"/> is set.</summary>
        public const string languageVariantUssClassNamePrefix = ussClassName + "--lang-";

        /// <summary>The USS class applied to the header row.</summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>The USS class applied to the language label inside the header.</summary>
        public const string languageUssClassName = ussClassName + "__language";

        /// <summary>The USS class applied to the flexible spacer between the language label and the copy button.</summary>
        public const string headerSpacerUssClassName = ussClassName + "__header-spacer";

        /// <summary>The USS class applied to the copy / checkmark button.</summary>
        public const string copyButtonUssClassName = ussClassName + "__copy-btn";

        /// <summary>The USS class applied to the scrollable area that wraps the line-numbers gutter and the body.</summary>
        public const string scrollUssClassName = ussClassName + "__scroll";

        /// <summary>The USS class applied to the row inside the scroll content (line numbers + body).</summary>
        public const string viewportUssClassName = ussClassName + "__viewport";

        /// <summary>The USS class applied to the optional left-side line-numbers gutter.</summary>
        public const string lineNumbersUssClassName = ussClassName + "__line-numbers";

        /// <summary>The USS class applied to the source-code body element.</summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>The USS class toggled while the pointer is anywhere over this element. Used to reveal the copy button.</summary>
        public const string hoveredUssClassName = ussClassName + "--hovered";

        /// <summary>The USS class toggled when <see cref="showLineNumbers"/> is enabled.</summary>
        public const string withLineNumbersUssClassName = ussClassName + "--with-line-numbers";

        const string k_CopyIcon = "copy";
        const string k_CheckIcon = "check";
        const long k_CopyFeedbackMs = 1500;

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
        static readonly CustomStyleProperty<TextMateGrammarAsset> k_GrammarGuidProperty = new ("--codeblock-grammar");
        static readonly CustomStyleProperty<TextMateThemeAsset> k_ThemeGuidProperty = new ("--codeblock-theme");
#endif

        readonly VisualElement m_Header;
        readonly Text m_LanguageLabel;
        readonly IconButton m_CopyButton;
        readonly ScrollView m_Scroll;
        readonly VisualElement m_Viewport;
        readonly LocalizedTextElement m_LineNumbers;
        readonly LocalizedTextElement m_Body;

        IVisualElementScheduledItem m_CopyFeedback;

        string m_Code = string.Empty;
        string m_Language;
        bool m_ShowLineNumbers;

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
        TextMateGrammarAsset m_Grammar;
        TextMateThemeAsset m_Theme;
#endif

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CodeBlock()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = false;

            m_Header = new VisualElement { pickingMode = PickingMode.Position };
            m_Header.AddToClassList(headerUssClassName);
            hierarchy.Add(m_Header);

            m_LanguageLabel = new Text { pickingMode = PickingMode.Ignore };
            m_LanguageLabel.AddToClassList(languageUssClassName);
            m_LanguageLabel.style.display = DisplayStyle.None;
            m_Header.Add(m_LanguageLabel);

            var spacer = new VisualElement { pickingMode = PickingMode.Ignore };
            spacer.AddToClassList(headerSpacerUssClassName);
            m_Header.Add(spacer);

            m_CopyButton = new IconButton(k_CopyIcon, OnCopyClicked) { quiet = true };
            m_CopyButton.AddToClassList(copyButtonUssClassName);
            m_CopyButton.tooltip = "Copy to clipboard";
            m_Header.Add(m_CopyButton);

            m_Scroll = new ScrollView(ScrollViewMode.VerticalAndHorizontal)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Auto,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
            };
            m_Scroll.AddToClassList(scrollUssClassName);
            hierarchy.Add(m_Scroll);

            m_Viewport = new VisualElement { pickingMode = PickingMode.Position };
            m_Viewport.AddToClassList(viewportUssClassName);
            m_Scroll.Add(m_Viewport);

            m_LineNumbers = new LocalizedTextElement
            {
                enableRichText = false,
                pickingMode = PickingMode.Ignore,
#if UNITY_2022_1_OR_NEWER
                selection = { isSelectable = false },
#endif
            };
            m_LineNumbers.AddToClassList(lineNumbersUssClassName);
            m_LineNumbers.style.display = DisplayStyle.None;
            m_Viewport.Add(m_LineNumbers);

            m_Body = new LocalizedTextElement
            {
                enableRichText = false,
                pickingMode = PickingMode.Position,
#if UNITY_2022_1_OR_NEWER
                selection = { isSelectable = true },
#endif
            };
            m_Body.AddToClassList(bodyUssClassName);
            m_Viewport.Add(m_Body);

            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        void OnPointerEnter(PointerEnterEvent evt) => EnableInClassList(hoveredUssClassName, true);

        void OnPointerLeave(PointerLeaveEvent evt) => EnableInClassList(hoveredUssClassName, false);

        /// <summary>
        /// Children are managed internally — this element exposes no content container.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The source code displayed in the body. Setting this triggers a re-render
        /// (with syntax highlighting if a grammar+theme pair is configured via USS).
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string code
        {
            get => m_Code;
            set
            {
                var changed = m_Code != value;
                m_Code = value ?? string.Empty;
                Refresh();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in codeProperty);
#endif
            }
        }

        /// <summary>
        /// The language identifier shown in the header (e.g. <c>"csharp"</c>, <c>"lua"</c>).
        /// When null or empty the label is hidden but the copy button stays available.
        /// Setting this also adds a <c>appui-code-block--lang-&lt;value&gt;</c> class so USS can
        /// map the language to a grammar via <c>--codeblock-grammar</c>.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string language
        {
            get => m_Language;
            set
            {
                var changed = m_Language != value;
                if (!string.IsNullOrEmpty(m_Language))
                    RemoveFromClassList(languageVariantUssClassNamePrefix + m_Language);
                m_Language = value;
                if (!string.IsNullOrEmpty(m_Language))
                    AddToClassList(languageVariantUssClassNamePrefix + m_Language);

                m_LanguageLabel.text = m_Language ?? string.Empty;
                m_LanguageLabel.style.display = string.IsNullOrEmpty(m_Language) ? DisplayStyle.None : DisplayStyle.Flex;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in languageProperty);
#endif
            }
        }

        /// <summary>
        /// When <c>true</c>, a left-side gutter shows 1-based line numbers next to the source.
        /// The gutter and the body live inside the same <see cref="ScrollView"/>, so vertical
        /// (and horizontal) scrolling keeps them aligned. Disabled by default.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool showLineNumbers
        {
            get => m_ShowLineNumbers;
            set
            {
                var changed = m_ShowLineNumbers != value;
                m_ShowLineNumbers = value;
                m_LineNumbers.style.display = m_ShowLineNumbers ? DisplayStyle.Flex : DisplayStyle.None;
                EnableInClassList(withLineNumbersUssClassName, m_ShowLineNumbers);
                UpdateLineNumbers();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in showLineNumbersProperty);
#endif
            }
        }

        void UpdateLineNumbers()
        {
            if (!m_ShowLineNumbers)
            {
                m_LineNumbers.text = string.Empty;
                return;
            }

            var src = m_Code ?? string.Empty;
            var lineCount = 1;
            for (var i = 0; i < src.Length; i++)
            {
                if (src[i] == '\n')
                    lineCount++;
            }

            var sb = new StringBuilder(lineCount * 4);
            for (var i = 1; i <= lineCount; i++)
            {
                sb.Append(i);
                if (i < lineCount)
                    sb.Append('\n');
            }
            m_LineNumbers.text = sb.ToString();
        }

        void OnCopyClicked()
        {
            if (string.IsNullOrEmpty(m_Code))
                return;

            try
            {
                Platform.SetPasteboardData(PasteboardType.Text, Encoding.UTF8.GetBytes(m_Code));
            }
            catch (System.Exception)
            {
                return;
            }

            m_CopyButton.icon = k_CheckIcon;
            m_CopyFeedback?.Pause();
            m_CopyFeedback = schedule.Execute(() => m_CopyButton.icon = k_CopyIcon).StartingIn(k_CopyFeedbackMs);
        }

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
            evt.customStyle.TryGetValue(k_GrammarGuidProperty, out var newGrammar);
            evt.customStyle.TryGetValue(k_ThemeGuidProperty, out var newTheme);

            var changed = newGrammar != m_Grammar || newTheme != m_Theme;
            m_Grammar = newGrammar;
            m_Theme = newTheme;

            if (changed)
                Refresh();
#endif
        }

        void Refresh()
        {
            var src = m_Code ?? string.Empty;
            UpdateLineNumbers();

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
            if (m_Theme)
            {
                if (m_Grammar)
                {
                    // Theme + grammar → coloured rich-text body.
                    string highlighted = null;
                    using (var hl = new TextMateSyntaxHighlighter(m_Grammar, m_Theme))
                    {
                        ApplyThemeColors(hl.defaultForeground, hl.defaultBackground);
                        try
                        {
                            highlighted = hl.Highlight(src);
                        }
                        catch (System.Exception e)
                        {
                            // Some TextMate grammars (e.g. xml) can throw inside the native
                            // tokenizer on certain inputs. Degrade to plain text rather than
                            // letting the exception bubble up through the style-update pass.
                            Debug.LogWarning(
                                "[CodeBlock] Failed to highlight code with grammar '" +
                                (m_Grammar ? m_Grammar.scopeName : "null") + "': " + e.Message);
                        }
                    }

                    if (highlighted != null)
                    {
                        m_Body.text = highlighted;
                        m_Body.enableRichText = true;
                    }
                    else
                    {
                        m_Body.text = src;
                        m_Body.enableRichText = false;
                    }
                    return;
                }

                // Theme only (no grammar) → plain body, but still inherit the theme bg/fg
                // so the codeblock visually matches the rest of the codeblocks in the doc.
                using (var theme = TextMateLib.Theme.LoadFromJson(m_Theme.jsonContent))
                {
                    ApplyThemeColors(theme.GetDefaultForeground(), theme.GetDefaultBackground());
                }
                m_Body.text = src;
                m_Body.enableRichText = false;
                return;
            }

            // No theme — fall back to the design-system colors set in USS.
            style.backgroundColor = StyleKeyword.Null;
            m_Body.style.color = StyleKeyword.Null;
#endif

            m_Body.text = src;
            m_Body.enableRichText = false;
        }

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
        void ApplyThemeColors(uint foreground, uint background)
        {
            var bg = ColorExtensions.RawColorToColor(background);
            if (bg.a <= 0f)
                bg.a = 1f;
            style.backgroundColor = bg;

            var fg = ColorExtensions.RawColorToColor(foreground);
            if (fg.a <= 0f)
                fg.a = 1f;
            m_Body.style.color = fg;
        }
#endif

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the <see cref="CodeBlock"/>.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<CodeBlock, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="CodeBlock"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Code = new UxmlStringAttributeDescription
            {
                name = "code",
                defaultValue = string.Empty,
            };

            readonly UxmlStringAttributeDescription m_Language = new UxmlStringAttributeDescription
            {
                name = "language",
                defaultValue = null,
            };

            readonly UxmlBoolAttributeDescription m_ShowLineNumbers = new UxmlBoolAttributeDescription
            {
                name = "show-line-numbers",
                defaultValue = false,
            };

            /// <summary>
            /// Initializes the <see cref="CodeBlock"/> from UXML attributes.
            /// </summary>
            /// <param name="ve">The element being initialized.</param>
            /// <param name="bag">Parsed attribute bag.</param>
            /// <param name="cc">Creation context.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (CodeBlock)ve;
                element.code = m_Code.GetValueFromBag(bag, cc);
                element.language = m_Language.GetValueFromBag(bag, cc);
                element.showLineNumbers = m_ShowLineNumbers.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
