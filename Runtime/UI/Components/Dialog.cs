using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The width of a dialog. Each step reads its --appui-dialog-width-* token.
    /// </summary>
    public enum DialogSize
    {
        /// <summary>
        /// Small, 400px.
        /// </summary>
        Sm = 1,

        /// <summary>
        /// Medium, 500px. This is the default.
        /// </summary>
        Md,

        /// <summary>
        /// Large, 640px.
        /// </summary>
        Lg,

        /// <summary>
        /// Extra large, 768px.
        /// </summary>
        Xl,
    }

    /// <summary>
    /// Base class for Dialogs (<see cref="Dialog"/>, <see cref="AlertDialog"/>, etc).
    /// </summary>
    [UxmlElement]
    public abstract partial class BaseDialog : BaseVisualElement
    {

        internal static readonly BindingId titlePropertyKey = new BindingId(nameof(title));

        internal static readonly BindingId descriptionPropertyKey = new BindingId(nameof(description));

        internal static readonly BindingId sizePropertyKey = new BindingId(nameof(size));


        /// <summary>
        /// The Dialog main styling class.
        /// </summary>
        public const string ussClassName = "appui-dialog";

        /// <summary>
        /// The Dialog variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(AlertSemantic))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The Dialog size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(DialogSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Dialog heading styling class.
        /// </summary>
        public const string headingUssClassName = ussClassName + "__heading";

        /// <summary>
        /// The Dialog header styling class.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The Dialog divider styling class.
        /// </summary>
        public const string dividerUssClassName = ussClassName + "__divider";

        /// <summary>
        /// The Dialog content styling class.
        /// </summary>
        public const string contentUssClassName = ussClassName + "__content";

        /// <summary>
        /// The Dialog button group styling class.
        /// </summary>
        public const string buttonGroupUssClassName = ussClassName + "__buttongroup";

        // The scroll view around the content. Internal so the fix ships without new public API.
        internal const string contentScrollViewUssClassName = ussClassName + "__content-scroll";

        /// <summary>
        /// The container for the Dialog actions (buttons).
        /// </summary>
        protected readonly VisualElement m_ActionContainer;

        /// <summary>
        /// The Dialog content.
        /// </summary>
        protected readonly LocalizedTextElement m_Content;

        /// <summary>
        /// The Dialog header divider.
        /// </summary>
        protected readonly Divider m_Divider;

        /// <summary>
        /// The Dialog header.
        /// </summary>
        protected readonly Heading m_Header;

        /// <summary>
        /// The Dialog heading.
        /// </summary>
        protected readonly VisualElement m_Heading;

        // Holds m_Content, so content taller than the dialog's room scrolls between a pinned heading
        // and a pinned footer instead of pushing the actions out of the dialog.
        readonly ScrollView m_ContentScrollView;

        DialogSize m_Size;

        /// <summary>
        /// Default constructor.
        /// </summary>
        protected BaseDialog()
        {
            AddToClassList(ussClassName);

            m_Heading = new VisualElement { name = headingUssClassName };
            m_Heading.AddToClassList(headingUssClassName);
            m_Header = new Heading { name = headerUssClassName };
            m_Header.AddToClassList(headerUssClassName);
            m_Divider = new Divider { name = dividerUssClassName };
            m_Divider.AddToClassList(dividerUssClassName);
            m_Content = new LocalizedTextElement { name = contentUssClassName };
            m_Content.AddToClassList(contentUssClassName);
            m_ContentScrollView = new ScrollView(ScrollViewMode.Vertical) { name = contentScrollViewUssClassName };
            m_ContentScrollView.AddToClassList(contentScrollViewUssClassName);
            m_ContentScrollView.Add(m_Content);
            // A ScrollView does not reveal a focused descendant on its own, so tabbing through a form
            // taller than the dialog would land on fields scrolled out of sight. Registered on the
            // content, not the scroll view, so focusing the scroll view's own scrollbar never asks it
            // to scroll to an element outside its content (ScrollTo throws on that).
            m_Content.RegisterCallback<FocusInEvent>(OnContentFocusIn);
            m_ActionContainer = new VisualElement { name = buttonGroupUssClassName };
            m_ActionContainer.AddToClassList(buttonGroupUssClassName);

            m_Heading.hierarchy.Add(m_Header);

            hierarchy.Add(m_Heading);
            hierarchy.Add(m_Divider);
            hierarchy.Add(m_ContentScrollView);
            hierarchy.Add(m_ActionContainer);

            size = DialogSize.Md;
            title = null;
        }

        void OnContentFocusIn(FocusInEvent evt)
        {
            if (evt.target is VisualElement focused)
                m_ContentScrollView.ScrollTo(focused);
        }

        /// <summary>
        /// The Dialog content container.
        /// </summary>
        public override VisualElement contentContainer => m_Content;

        /// <summary>
        /// The Dialog action container.
        /// </summary>
        public VisualElement actionContainer => m_ActionContainer;

        /// <summary>
        /// The Dialog title.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Header.text;
            set
            {
                var changed = m_Header.text != value;
                m_Header.text = value;
                RefreshHeading();

                if (changed)
                    NotifyPropertyChanged(in titlePropertyKey);
            }
        }

        /// <summary>
        /// Check if the heading should be hidden. Override this method to change the default behavior.
        /// By default, the heading is hidden if the title is null or empty.
        /// </summary>
        /// <returns> True if the heading should be hidden, false otherwise.</returns>
        protected virtual bool ShouldHideHeading() => string.IsNullOrEmpty(title);

        /// <summary>
        /// Refresh the heading visibility.
        /// </summary>
        protected void RefreshHeading()
        {
            var hidden = ShouldHideHeading();
            m_Heading.EnableInClassList(Styles.hiddenUssClassName, hidden);
            m_Divider.EnableInClassList(Styles.hiddenUssClassName, hidden);
        }

        /// <summary>
        /// The Dialog description. This is the text displayed in the content container.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string description
        {
            get => m_Content.text;
            set
            {
                var changed = m_Content.text != value;
                m_Content.text = value;

                if (changed)
                    NotifyPropertyChanged(in descriptionPropertyKey);
            }
        }

        /// <summary>
        /// The Dialog size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public DialogSize size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizePropertyKey);
            }
        }

    }

    /// <summary>
    /// A reusable dialog component that displays content in a modal window, often used for important information
    /// or actions that require user attention.
    /// </summary>
    /// <remarks>
    /// The Dialog component provides a way to present content in a focused modal window, temporarily interrupting
    /// the user's workflow. It's commonly used for important notifications, gathering user input, or requiring
    /// user decisions.
    ///
    /// Dialogs can contain various types of content including text, form elements, or custom components. They
    /// appear as modal windows overlaying the main content and typically include a title, content area, and
    /// optional action buttons.
    ///
    /// Note: For situations requiring specific user decisions or acknowledgments, consider using the
    /// <see cref="AlertDialog"/> variant which provides pre-configured semantic variants and action buttons.
    ///
    /// A Dialog's visibility is typically controlled by a <see cref="Modal"/> component, which handles the
    /// overlay and focus management.
    /// </remarks>
    /// <example>
    /// <para>Basic Dialog Example</para>
    /// <code lang="csharp"><![CDATA[
    /// var dialog = new Dialog
    /// {
    ///     title = "Welcome",
    ///     description = "Welcome to our application! We hope you enjoy using it.",
    ///     size = DialogSize.Md,
    ///     dismissable = true
    /// };
    ///
    /// // Add the dialog to a modal
    /// var modal = new Modal();
    /// modal.Add(dialog);
    ///
    /// // Add the modal to your UI hierarchy
    /// rootElement.Add(modal);
    /// ]]></code>
    /// <para>UXML Dialog Definition</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML xmlns="UnityEngine.UIElements">
    ///     <Modal>
    ///         <ui:Dialog
    ///             title="Settings"
    ///             description="Configure your application settings below."
    ///             size="Md"
    ///             dismissable="true">
    ///             <!-- Add custom content here -->
    ///         </ui:Dialog>
    ///     </Modal>
    /// </UXML>
    /// ]]></code>
    /// <para>Dialog with Custom Content</para>
    /// <code lang="csharp"><![CDATA[
    /// var dialog = new Dialog();
    /// dialog.title = "User Profile";
    ///
    /// // Add custom content
    /// var customContent = new VisualElement();
    /// customContent.Add(new TextField("Name:"));
    /// customContent.Add(new TextField("Email:"));
    ///
    /// dialog.Add(customContent);
    ///
    /// // Add action buttons
    /// var saveButton = new Button(() => Debug.Log("Save clicked")) { text = "Save" };
    /// var cancelButton = new Button(() => Debug.Log("Cancel clicked")) { text = "Cancel" };
    ///
    /// dialog.actionContainer.Add(cancelButton);
    /// dialog.actionContainer.Add(saveButton);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts")]
    public partial class Dialog : BaseDialog, IDismissInvocator
    {

        internal static readonly BindingId dismissablePropertyKey = new BindingId(nameof(dismissable));


        /// <summary>
        /// The Dialog close button styling class.
        /// </summary>
        public const string closeButtonUssClassName = ussClassName + "__closebutton";

        /// <summary>
        /// The Dialog dismissable mode styling class.
        /// </summary>
        public const string dismissableUssClassName = ussClassName + "--dismissable";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Dialog()
        {
            closeButton = new Button(OnCloseButtonClicked) { name = closeButtonUssClassName, leadingIcon = "x", variant = ButtonVariant.Ghost };
            closeButton.AddToClassList(closeButtonUssClassName);
            m_Heading.hierarchy.Add(closeButton);

            dismissable = false;
        }

        /// <summary>
        /// The close button.
        /// </summary>
        /// <remarks>
        /// The button is only visible if <see cref="dismissable"/> is `True`.
        /// </remarks>
        public Button closeButton { get; }

        /// <summary>
        /// Set the <see cref="Dialog"/> dismissable by itself using a <see cref="closeButton"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool dismissable
        {
            get => ClassListContains(dismissableUssClassName);
            set
            {
                var changed = ClassListContains(dismissableUssClassName) != value;
                EnableInClassList(dismissableUssClassName, value);
                RefreshHeading();

                if (changed)
                    NotifyPropertyChanged(in dismissablePropertyKey);
            }
        }

        /// <inheritdoc cref="BaseDialog.ShouldHideHeading"/>
        protected override bool ShouldHideHeading() => string.IsNullOrEmpty(title) && !dismissable;

        /// <summary>
        /// Event fired when the <see cref="Dialog"/> is dismissed.
        /// </summary>
        public event Action<DismissType> dismissRequested;

        void OnCloseButtonClicked()
        {
            if (dismissable)
                dismissRequested?.Invoke(DismissType.Manual);
        }

    }
}
