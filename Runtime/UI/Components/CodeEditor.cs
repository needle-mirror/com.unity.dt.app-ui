using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// An editable source-code surface with syntax highlighting.
    /// </summary>
    /// <remarks>
    /// This is <see cref="CodeBlock"/>'s highlighting over <see cref="TextArea"/>'s editing. Neither
    /// half is reimplemented: the TextArea owns focus, caret, selection, undo and the runtime context
    /// menu, and the same TextMate grammar/theme pair that colours a <see cref="CodeBlock"/> colours this.
    ///
    /// **How the two layers stay aligned.** UI Toolkit has no editable rich text — a TextField renders
    /// its value raw — so, exactly as on the web, the colours live on a separate element layered against a
    /// text input whose own glyphs are turned transparent. Both layers are styled by one shared USS rule so
    /// their font, size, padding and wrapping cannot drift apart; changing one without the other
    /// misaligns the caret and nothing errors.
    ///
    /// **What is deliberately missing.** The registry keeps the highlight layer's scroll offset in sync
    /// with the textarea's on every scroll event, because a DOM textarea is a fixed box that scrolls its
    /// own content. Here the editor grows with its content instead — <see cref="TextArea.autoResize"/>
    /// is on, both layers are full height, and there is no independent scroll offset to keep in sync.
    /// Put the editor in a <see cref="UnityEngine.UIElements.ScrollView"/> if the document can outgrow
    /// the space. Porting the sync would have been dead code.
    ///
    /// **Degrading.** When no grammar and theme resolve — which includes every player build, since
    /// grammars are looked up from asset GUIDs, and every project that does not define
    /// <c>APPUI_ENABLE_SYNTAX_HIGHLIGHTING</c> — the overlay stays empty and the input keeps its own
    /// visible text. The editor is fully usable either way, which mirrors the registry's behaviour when
    /// its highlighter chunk fails to load.
    ///
    /// Setting <see cref="language"/> adds a <c>appui-code-editor--lang-&lt;value&gt;</c> class, so
    /// language-to-grammar mappings are declared in USS the same way <see cref="CodeBlock"/> declares
    /// them, through the <c>--codeblock-grammar</c> and <c>--codeblock-theme</c> custom properties.
    /// </remarks>
    /// <example>
    /// <para>A Lua script editor.</para>
    /// <code lang="csharp">
    /// var editor = new CodeEditor
    /// {
    ///     language = "lua",
    ///     placeholder = "-- your script here",
    ///     value = "function update(dt)\n  print(dt)\nend\n",
    /// };
    /// editor.RegisterValueChangedCallback(evt =&gt; Save(evt.newValue));
    /// parentElement.Add(editor);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:CodeEditor language="csharp" placeholder="// code" indent-size="4" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class CodeEditor : BaseVisualElement, INotifyValueChanged<string>
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId languageProperty = nameof(language);

        internal static readonly BindingId placeholderProperty = nameof(placeholder);

        internal static readonly BindingId isReadOnlyProperty = nameof(isReadOnly);

        internal static readonly BindingId indentSizeProperty = nameof(indentSize);

        /// <summary>
        /// The CodeEditor main styling class.
        /// </summary>
        public const string ussClassName = "appui-code-editor";

        /// <summary>
        /// Prefix of the per-language modifier class added when <see cref="language"/> is set.
        /// </summary>
        public const string languageVariantUssClassNamePrefix = ussClassName + "--lang-";

        /// <summary>
        /// The styling class toggled while a highlighted rendering is actually being displayed.
        /// </summary>
        public const string highlightedUssClassName = ussClassName + "--highlighted";

        /// <summary>
        /// The styling class of the highlight layer drawn over the input.
        /// </summary>
        public const string highlightUssClassName = ussClassName + "__highlight";

        /// <summary>
        /// The styling class of the editable input layer.
        /// </summary>
        public const string inputUssClassName = ussClassName + "__input";

        /// <summary>
        /// The indent width used when nothing else is asked for.
        /// </summary>
        public const int defaultIndentSize = 2;

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
        static readonly CustomStyleProperty<TextMateGrammarAsset> k_GrammarProperty = new("--codeblock-grammar");

        static readonly CustomStyleProperty<TextMateThemeAsset> k_ThemeProperty = new("--codeblock-theme");

        TextMateGrammarAsset m_Grammar;

        TextMateThemeAsset m_Theme;
#endif

        readonly TextElement m_Highlight;

        readonly TextArea m_Input;

        string m_Language;

        int m_IndentSize = defaultIndentSize;

        /// <summary>
        /// Children are managed internally — this element exposes no content container.
        /// </summary>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The editing surface. Exposed so a caller can reach TextArea features this element does not
        /// re-expose, such as <see cref="TextArea.maxLength"/> or <see cref="TextArea.validateValue"/>.
        /// </summary>
        public TextArea textArea => m_Input;

        /// <summary>
        /// Whether a highlighted rendering is currently being displayed. False in a player build, and
        /// false whenever no grammar and theme pair resolves for the current <see cref="language"/>.
        /// </summary>
        public bool isHighlighted => ClassListContains(highlightedUssClassName);

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CodeEditor()
            : this(null) { }

        /// <summary>
        /// Constructs an editor holding a predefined source. No change event is sent.
        /// </summary>
        /// <param name="value"> The initial source. </param>
        public CodeEditor(string value)
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_Highlight = new TextElement
            {
                name = highlightUssClassName,
                pickingMode = PickingMode.Ignore,
                enableRichText = true,
            };
            m_Highlight.AddToClassList(highlightUssClassName);
            m_Highlight.selection.isSelectable = false;

            m_Input = new TextArea
            {
                name = inputUssClassName,
                autoResize = true,
            };
            m_Input.AddToClassList(inputUssClassName);

            // The input goes in first and the coloured copy sits on top of it, ignoring picking. The
            // other way round — colours behind — would put the TextArea's own opaque field background
            // between the reader and them, and suppressing that background would cost the editor the
            // field chrome, focus ring and invalid state that TextArea already gets right.
            hierarchy.Add(m_Input);
            hierarchy.Add(m_Highlight);

            m_Input.RegisterValueChangedCallback(OnInputValueChanged);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            SetValueWithoutNotify(value ?? string.Empty);
        }

        // -------------------------------------------------------------------
        // Value
        // -------------------------------------------------------------------

        /// <summary>
        /// The source code being edited.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string value
        {
            get => m_Input.value;
            set
            {
                var previous = m_Input.value;
                if (previous == value)
                    return;

                using var evt = ChangeEvent<string>.GetPooled(previous, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Sets the source without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new source. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Input.SetValueWithoutNotify(newValue ?? string.Empty);
            RefreshHighlight();
        }

        void OnInputValueChanged(ChangeEvent<string> evt)
        {
            // The inner TextArea's own event is an implementation detail; this element re-sends one
            // whose target is the editor, so a caller never has to know which element it came from.
            evt.StopPropagation();
            RefreshHighlight();

            using var forwarded = ChangeEvent<string>.GetPooled(evt.previousValue, evt.newValue);
            forwarded.target = this;
            SendEvent(forwarded);
            NotifyPropertyChanged(in valueProperty);
        }

        // -------------------------------------------------------------------
        // Properties
        // -------------------------------------------------------------------

        /// <summary>
        /// The grammar identifier, for instance <c>"csharp"</c> or <c>"lua"</c>. Adds a
        /// <c>appui-code-editor--lang-&lt;value&gt;</c> class so USS can map it to a grammar. An
        /// unmapped identifier renders as plain text.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
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

                if (changed)
                    NotifyPropertyChanged(in languageProperty);
            }
        }

        /// <summary>
        /// The text shown while the editor is empty.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string placeholder
        {
            get => m_Input.placeholder;
            set
            {
                var changed = m_Input.placeholder != value;
                m_Input.placeholder = value;
                if (changed)
                    NotifyPropertyChanged(in placeholderProperty);
            }
        }

        /// <summary>
        /// Whether the source can be edited. A read-only editor still highlights, still scrolls and
        /// still lets the reader select and copy — and it leaves Tab alone, so it does not trap focus.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isReadOnly
        {
            get => m_Input.isReadOnly;
            set
            {
                var changed = m_Input.isReadOnly != value;
                m_Input.isReadOnly = value;
                if (changed)
                    NotifyPropertyChanged(in isReadOnlyProperty);
            }
        }

        /// <summary>
        /// How many spaces the Tab key inserts. Two by default, matching the registry. Zero or less
        /// leaves Tab as a focus-navigation key.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int indentSize
        {
            get => m_IndentSize;
            set
            {
                var changed = m_IndentSize != value;
                m_IndentSize = value;
                if (changed)
                    NotifyPropertyChanged(in indentSizeProperty);
            }
        }

        // -------------------------------------------------------------------
        // Indenting
        // -------------------------------------------------------------------

        /// <summary>
        /// Tab indents rather than moving focus — but only in an editable editor. A read-only viewer
        /// must keep Tab as Tab, or the reader is trapped in it with no way to type anything either.
        /// </summary>
        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Tab || evt.shiftKey || evt.altKey || evt.ctrlKey || evt.commandKey)
                return;
            if (isReadOnly || m_IndentSize <= 0)
                return;

            evt.StopPropagation();
            InsertIndent();
        }

        /// <summary>
        /// Inserts one indent at the caret, replacing the selection if there is one. Does nothing in a
        /// read-only editor, and nothing when <see cref="indentSize"/> is zero or less.
        /// </summary>
        public void InsertIndent()
        {
            if (isReadOnly)
                return;

            var indent = new string(' ', Mathf.Max(0, m_IndentSize));
            if (indent.Length == 0)
                return;

            var text = value ?? string.Empty;
            var field = m_Input.Q<UnityEngine.UIElements.TextField>();
            var cursor = field?.cursorIndex ?? text.Length;
            var select = field?.selectIndex ?? cursor;

            var start = Mathf.Clamp(Mathf.Min(cursor, select), 0, text.Length);
            var end = Mathf.Clamp(Mathf.Max(cursor, select), 0, text.Length);

            value = text.Substring(0, start) + indent + text.Substring(end);

            var caret = start + indent.Length;
            schedule.Execute(() => field?.SelectRange(caret, caret));
        }

        // -------------------------------------------------------------------
        // Highlighting
        // -------------------------------------------------------------------

        void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
            evt.customStyle.TryGetValue(k_GrammarProperty, out var grammar);
            evt.customStyle.TryGetValue(k_ThemeProperty, out var theme);

            var changed = grammar != m_Grammar || theme != m_Theme;
            m_Grammar = grammar;
            m_Theme = theme;

            if (changed)
                RefreshHighlight();
#endif
        }

        void RefreshHighlight()
        {
            var highlighted = false;

#if APPUI_ENABLE_SYNTAX_HIGHLIGHTING
            if (CodeHighlighter.TryHighlight(m_Grammar, m_Theme, m_Input.value,
                    out var richText, out var foreground, out var background))
            {
                m_Highlight.text = richText;
                m_Highlight.style.color = foreground;
                style.backgroundColor = background;
                highlighted = true;
            }
            else
            {
                style.backgroundColor = StyleKeyword.Null;
                m_Highlight.style.color = StyleKeyword.Null;
            }
#endif

            if (!highlighted)
                m_Highlight.text = string.Empty;

            m_Highlight.EnableInClassList(Styles.hiddenUssClassName, !highlighted);
            EnableInClassList(highlightedUssClassName, highlighted);

            // The input's own glyphs are hidden behind the coloured copy by CodeEditor.uss, keyed on
            // the highlighted class above. USS rather than an inline colour on the TextField root:
            // the inner input and its TextElement carry their own colour, so an inline colour on the
            // root never reached them and the text was drawn twice. The class also needs no inner
            // element to exist yet, so it holds even before the TextArea has built its field.
        }
    }
}
