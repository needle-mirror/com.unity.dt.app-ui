using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The Plan tool's review card: what the agent intends to do, and the two answers to it.
    /// </summary>
    /// <remarks>
    /// The card is three bands. The header carries the plan title, the optional source path and the
    /// optional edit and expand controls. The body scrolls the plan itself, which the caller supplies
    /// as children — typically a rendered markdown view. The footer pairs a status line with Discard
    /// and Run plan.
    ///
    /// Editing is deliberately not owned here. <see cref="editable"/> only offers the control and
    /// <see cref="isEditing"/> only reflects which mode is showing; the caller swaps the body for an
    /// editor, because only it knows what the plan is made of.
    ///
    /// Every control is a package component: <see cref="IconButton"/> for the header affordances and
    /// <see cref="Button"/> for the footer pair, so the card inherits their sizing and focus behaviour
    /// rather than restating it.
    /// </remarks>
    /// <example>
    /// <para>A plan with an expand control and both footer actions wired.</para>
    /// <code lang="csharp">
    /// var plan = new ChatPlan
    /// {
    ///     title = "Megaman clone",
    ///     filePath = "Assets/Plans/megaman-clone.md",
    ///     expandable = true
    /// };
    /// plan.Add(new MarkdownView { content = planMarkdown });
    /// plan.runRequested += RunPlan;
    /// plan.discardRequested += DiscardPlan;
    /// parentElement.Add(plan);
    /// </code>
    /// <para>In UXML. Children land in the scrolling body.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatPlan title="Megaman clone" file-path="Assets/Plans/megaman-clone.md" editable="true"&gt;
    ///     &lt;appui:Text text="1. Add a player controller" /&gt;
    /// &lt;/appui:ChatPlan&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatPlan : BaseVisualElement
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId filePathProperty = nameof(filePath);

        internal static readonly BindingId statusProperty = nameof(status);

        internal static readonly BindingId isEditingProperty = nameof(isEditing);

        internal static readonly BindingId editableProperty = nameof(editable);

        internal static readonly BindingId expandableProperty = nameof(expandable);

        /// <summary>
        /// The ChatPlan main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-plan";

        /// <summary>
        /// The styling class of the header band.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the header's leading glyph.
        /// </summary>
        public const string headerIconUssClassName = ussClassName + "__header-icon";

        /// <summary>
        /// The styling class of the plan title.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the source path.
        /// </summary>
        public const string filePathUssClassName = ussClassName + "__file-path";

        /// <summary>
        /// The styling class of the edit control.
        /// </summary>
        public const string editButtonUssClassName = ussClassName + "__edit-button";

        /// <summary>
        /// The styling class of the expand control.
        /// </summary>
        public const string expandButtonUssClassName = ussClassName + "__expand-button";

        /// <summary>
        /// The styling class of the scrolling body.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>
        /// The styling class of the footer band.
        /// </summary>
        public const string footerUssClassName = ussClassName + "__footer";

        /// <summary>
        /// The styling class of the footer status group.
        /// </summary>
        public const string statusUssClassName = ussClassName + "__status";

        /// <summary>
        /// The styling class of the status glyph.
        /// </summary>
        public const string statusIconUssClassName = ussClassName + "__status-icon";

        /// <summary>
        /// The styling class of the status text.
        /// </summary>
        public const string statusLabelUssClassName = ussClassName + "__status-label";

        /// <summary>
        /// The styling class of the footer's action group.
        /// </summary>
        public const string actionsUssClassName = ussClassName + "__actions";

        /// <summary>
        /// The styling class of the Discard button.
        /// </summary>
        public const string discardButtonUssClassName = ussClassName + "__discard-button";

        /// <summary>
        /// The styling class of the Run plan button.
        /// </summary>
        public const string runButtonUssClassName = ussClassName + "__run-button";

        /// <summary>
        /// The styling class applied while the body is showing an editor.
        /// </summary>
        public const string editingUssClassName = ussClassName + "--editing";

        /// <summary>
        /// The default footer status line.
        /// </summary>
        public const string defaultStatus = "Plan ready to execute";

        readonly LocalizedTextElement m_Title;

        readonly LocalizedTextElement m_FilePath;

        readonly IconButton m_EditButton;

        readonly IconButton m_ExpandButton;

        readonly ScrollView m_Body;

        readonly LocalizedTextElement m_Status;

        readonly Button m_DiscardButton;

        readonly Button m_RunButton;

        bool m_IsEditing;

        bool m_Editable;

        bool m_Expandable;

        /// <summary>
        /// Raised when the user asks to open the plan full screen.
        /// </summary>
        public event Action expandRequested;

        /// <summary>
        /// Raised when the user toggles editing. The caller owns the resulting mode change.
        /// </summary>
        public event Action editRequested;

        /// <summary>
        /// Raised when the user discards the plan.
        /// </summary>
        public event Action discardRequested;

        /// <summary>
        /// Raised when the user runs the plan.
        /// </summary>
        public event Action runRequested;

        /// <summary>
        /// The plan body. This is what <c>Add</c> reaches.
        /// </summary>
        public override VisualElement contentContainer => m_Body.contentContainer;

        /// <summary>
        /// The header's edit control.
        /// </summary>
        public IconButton editButton => m_EditButton;

        /// <summary>
        /// The header's expand control.
        /// </summary>
        public IconButton expandButton => m_ExpandButton;

        /// <summary>
        /// The footer's Discard button.
        /// </summary>
        public Button discardButton => m_DiscardButton;

        /// <summary>
        /// The footer's Run plan button.
        /// </summary>
        public Button runButton => m_RunButton;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatPlan()
        {
            AddToClassList(ussClassName);

            var header = new VisualElement { name = headerUssClassName };
            header.AddToClassList(headerUssClassName);

            var headerIcon = new Icon
            {
                name = headerIconUssClassName,
                iconName = "list-checks",
                size = IconSize.Sm,
                pickingMode = PickingMode.Ignore,
            };
            headerIcon.AddToClassList(headerIconUssClassName);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            m_FilePath = new LocalizedTextElement { name = filePathUssClassName, pickingMode = PickingMode.Ignore };
            m_FilePath.AddToClassList(filePathUssClassName);

            m_EditButton = new IconButton { name = editButtonUssClassName, icon = "pencil", quiet = true, size = Size.Sm };
            m_EditButton.AddToClassList(editButtonUssClassName);
            m_EditButton.clicked += () => editRequested?.Invoke();

            m_ExpandButton = new IconButton { name = expandButtonUssClassName, icon = "arrows-out", quiet = true, size = Size.Sm };
            m_ExpandButton.AddToClassList(expandButtonUssClassName);
            m_ExpandButton.clicked += () => expandRequested?.Invoke();

            header.hierarchy.Add(headerIcon);
            header.hierarchy.Add(m_Title);
            header.hierarchy.Add(m_FilePath);
            header.hierarchy.Add(m_EditButton);
            header.hierarchy.Add(m_ExpandButton);

            m_Body = new ScrollView { name = bodyUssClassName };
            m_Body.AddToClassList(bodyUssClassName);

            var footer = new VisualElement { name = footerUssClassName };
            footer.AddToClassList(footerUssClassName);

            var statusGroup = new VisualElement { name = statusUssClassName, pickingMode = PickingMode.Ignore };
            statusGroup.AddToClassList(statusUssClassName);

            var statusIcon = new Icon
            {
                name = statusIconUssClassName,
                iconName = "lightning",
                size = IconSize.Sm,
                pickingMode = PickingMode.Ignore,
            };
            statusIcon.AddToClassList(statusIconUssClassName);

            m_Status = new LocalizedTextElement { name = statusLabelUssClassName, pickingMode = PickingMode.Ignore };
            m_Status.AddToClassList(statusLabelUssClassName);

            statusGroup.hierarchy.Add(statusIcon);
            statusGroup.hierarchy.Add(m_Status);

            var actions = new VisualElement { name = actionsUssClassName };
            actions.AddToClassList(actionsUssClassName);

            m_DiscardButton = new Button
            {
                name = discardButtonUssClassName,
                title = "Discard",
                variant = ButtonVariant.Ghost,
                size = Size.Sm,
            };
            m_DiscardButton.AddToClassList(discardButtonUssClassName);
            m_DiscardButton.clicked += () => discardRequested?.Invoke();

            m_RunButton = new Button
            {
                name = runButtonUssClassName,
                title = "Run plan",
                variant = ButtonVariant.Default,
                size = Size.Sm,
            };
            m_RunButton.AddToClassList(runButtonUssClassName);
            m_RunButton.clicked += () => runRequested?.Invoke();

            actions.hierarchy.Add(m_DiscardButton);
            actions.hierarchy.Add(m_RunButton);

            footer.hierarchy.Add(statusGroup);
            footer.hierarchy.Add(actions);

            hierarchy.Add(header);
            hierarchy.Add(m_Body);
            hierarchy.Add(footer);

            status = defaultStatus;
            filePath = null;
            editable = false;
            expandable = false;
            isEditing = false;
        }

        /// <summary>
        /// The plan title shown in the header.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title.text;
            set
            {
                var changed = m_Title.text != value;
                m_Title.text = value;

                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// Where the plan lives, for instance "Assets/Plans/megaman-clone.md". Null or empty hides the
        /// element entirely rather than leaving a gap in the header.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string filePath
        {
            get => m_FilePath.text;
            set
            {
                var changed = m_FilePath.text != value;
                m_FilePath.text = value;
                m_FilePath.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in filePathProperty);
            }
        }

        /// <summary>
        /// The footer status line. "Plan ready to execute" by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string status
        {
            get => m_Status.text;
            set
            {
                var changed = m_Status.text != value;
                m_Status.text = value;

                if (changed)
                    NotifyPropertyChanged(in statusProperty);
            }
        }

        /// <summary>
        /// Whether the edit control is offered. False by default, which makes the plan read-only.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool editable
        {
            get => m_Editable;
            set
            {
                var changed = m_Editable != value;
                m_Editable = value;
                m_EditButton.EnableInClassList(Styles.hiddenUssClassName, !m_Editable);

                if (changed)
                    NotifyPropertyChanged(in editableProperty);
            }
        }

        /// <summary>
        /// Whether the expand control is offered. False by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool expandable
        {
            get => m_Expandable;
            set
            {
                var changed = m_Expandable != value;
                m_Expandable = value;
                m_ExpandButton.EnableInClassList(Styles.hiddenUssClassName, !m_Expandable);

                if (changed)
                    NotifyPropertyChanged(in expandableProperty);
            }
        }

        /// <summary>
        /// Whether the body is currently showing an editor rather than the rendered plan. Purely
        /// reflective: setting it does not change the body, it only marks the card and flips the edit
        /// control to its pressed state.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool isEditing
        {
            get => m_IsEditing;
            set
            {
                var changed = m_IsEditing != value;
                m_IsEditing = value;
                EnableInClassList(editingUssClassName, m_IsEditing);
                m_EditButton.EnableInClassList(Styles.selectedUssClassName, m_IsEditing);

                if (changed)
                    NotifyPropertyChanged(in isEditingProperty);
            }
        }
    }
}
