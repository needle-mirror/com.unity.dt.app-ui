using System;
using System.Collections.Generic;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// ThreadComposer UI element. Provides a text input area with submit and attach actions.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class ThreadComposer : ExVisualElement
    {
#if ENABLE_RUNTIME_DATA_BINDINGS
        internal static readonly BindingId placeholderProperty = nameof(placeholder);

        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId encodedValueProperty = nameof(encodedValue);

        internal static readonly BindingId sendingProperty = nameof(sending);

        internal static readonly BindingId isEditingProperty = nameof(isEditing);

        internal static readonly BindingId showAvatarProperty = nameof(showAvatar);

        internal static readonly BindingId alwaysShowToolbarProperty = nameof(alwaysShowToolbar);

        internal static readonly BindingId maxTextLengthProperty = nameof(maxTextLength);
#endif

        /// <summary>
        /// The default maximum text length for the composer.
        /// </summary>
        public const int defaultMaxTextLength = 10000;

#if UNITY_LOCALIZATION_PRESENT
        const string k_SaveText = "@AppUI:save";
        const string k_CancelText =  "@AppUI:cancel";
#else
        const string k_SaveText = "Save";
        const string k_CancelText = "Cancel";
#endif

        /// <summary>
        /// The ThreadComposer main styling class.
        /// </summary>
        public const string ussClassName = "appui-thread-composer";

        /// <summary>
        /// The ThreadComposer textarea styling class.
        /// </summary>
        public const string textareaUssClassName = ussClassName + "__textarea";

        /// <summary>
        /// The ThreadComposer toolbar container styling class.
        /// </summary>
        public const string toolbarContainerUssClassName = ussClassName + "__toolbar-container";

        /// <summary>
        /// The ThreadComposer toolbar styling class.
        /// </summary>
        public const string toolbarUssClassName = ussClassName + "__toolbar";

        /// <summary>
        /// The ThreadComposer actionbar styling class.
        /// </summary>
        public const string actionbarUssClassName = ussClassName + "__actionbar";

        /// <summary>
        /// The ThreadComposer attach button styling class.
        /// </summary>
        public const string attachBtnUssClassName = ussClassName + "__attach-btn";

        /// <summary>
        /// The ThreadComposer submit button styling class.
        /// </summary>
        public const string submitBtnUssClassName = ussClassName + "__submit-btn";

        /// <summary>
        /// The ThreadComposer sending state styling class,
        /// which can be used to disable input and show a loading state when a message is being submitted.
        /// </summary>
        public const string sendingUssClassName = ussClassName + "--sending";

        /// <summary>
        /// The ThreadComposer avatar styling class.
        /// </summary>
        public const string avatarContainerUssClassName = ussClassName + "__avatar-container";

        /// <summary>
        /// The ThreadComposer avatar styling class.
        /// </summary>
        public const string avatarUssClassName = ussClassName + "__avatar";

        /// <summary>
        /// The ThreadComposer avatar label styling class.
        /// </summary>
        public const string avatarLabelUssClassName = ussClassName + "__avatar-initials";

        /// <summary>
        /// The ThreadComposer text area container styling class.
        /// </summary>
        public const string textAreaContainerUssClassName = ussClassName + "__textarea-container";

        /// <summary>
        /// The ThreadComposer input box styling class.
        /// </summary>
        public const string inputBoxUssClassName = ussClassName + "__input-box";

        /// <summary>
        /// The ThreadComposer attachments styling class.
        /// </summary>
        public const string attachmentsUssClassName = ussClassName + "__attachments";

        /// <summary>
        /// The ThreadComposer attachments scroll view styling class.
        /// </summary>
        public const string attachmentsScrollUssClassName = ussClassName + "__attachments-scroll";

        /// <summary>
        /// The ThreadComposer dropzone styling class.
        /// </summary>
        public const string dropzoneUssClassName = ussClassName + "__dropzone";

        /// <summary>
        /// The ThreadComposer cancel button styling class.
        /// </summary>
        public const string cancelBtnUssClassName = ussClassName + "__cancel-btn";

        /// <summary>
        /// The ThreadComposer save button styling class.
        /// </summary>
        public const string saveBtnUssClassName = ussClassName + "__save-btn";

        /// <summary>
        /// The ThreadComposer sending progress indicator styling class,
        /// </summary>
        public const string sendingProgressUssClassName = ussClassName + "__sending-progress";

        /// <summary>
        /// The ThreadComposer character counter styling class.
        /// </summary>
        public const string counterUssClassName = ussClassName + "__counter";

        /// <summary>
        /// The ThreadComposer character counter "current count" styling class.
        /// </summary>
        public const string counterCurrentUssClassName = ussClassName + "__counter-current";

        /// <summary>
        /// The ThreadComposer character counter separator styling class.
        /// </summary>
        public const string counterSeparatorUssClassName = ussClassName + "__counter-separator";

        /// <summary>
        /// The ThreadComposer character counter "max count" styling class.
        /// </summary>
        public const string counterMaxUssClassName = ussClassName + "__counter-max";

        /// <summary>
        /// The ThreadComposer empty state styling class,
        /// which is applied when the text area has no content to switch the toolbar to an inline layout.
        /// </summary>
        public const string emptyUssClassName = ussClassName + "--empty";

        /// <summary>
        /// The ThreadComposer force toolbar styling class.
        /// </summary>
        public const string forceToolbarUssClassName = ussClassName + "--force-toolbar";

        /// <summary>
        /// The ThreadComposer editing modifier styling class.
        /// </summary>
        public const string editingUssClassName = ussClassName + "--editing";

        /// <summary>
        /// The ThreadComposer show avatar modifier styling class,
        /// which is applied when the <see cref="showAvatar"/> property is set to <c>true</c>
        /// </summary>
        public const string showAvatarUssClassName = ussClassName + "--with-avatar";

        readonly IconButton m_AttachButton;

        readonly IconButton m_SubmitButton;

        readonly VisualElement m_ToolbarContainer;

        readonly ActionButton m_CancelButton;

        readonly Button m_SaveButton;

        readonly VisualElement m_ActionBar;

        readonly CircularProgress m_SendingProgress;

        readonly VisualElement m_InputBox;

        readonly ScrollView m_AttachmentsScrollView;

        readonly VisualElement m_TextAreaContainer;

        readonly VisualElement m_MentionPopoverAnchor;

        bool m_IsEditing;

        IMentionProvider m_MentionProvider;

        bool m_IsMentioning;

        int m_MentionStartIndex;

        VisualElement m_MentionPopover;

        MentionPicker m_MentionPicker;

        string m_MentionQuery;

        bool m_AlwaysShowToolbar;

        readonly DropZone m_DropZone;

        readonly VisualElement m_Counter;

        readonly LocalizedTextElement m_CounterCurrent;

        readonly LocalizedTextElement m_CounterMax;

        int m_MaxTextLength = defaultMaxTextLength;

        readonly Dictionary<string, string> m_Mentions = new ();

        /// <summary>
        /// Event invoked when a message is submitted.
        /// </summary>
        public event Action<string> submitClicked;

        /// <summary>
        /// Event invoked when the attach button is clicked.
        /// </summary>
        public event Action attachClicked;

        /// <summary>
        /// Event invoked when the save button is clicked in editing mode.
        /// </summary>
        public event Action saveClicked;

        /// <summary>
        /// Event invoked when the cancel button is clicked in editing mode.
        /// </summary>
        public event Action cancelClicked;

        /// <summary>
        /// Event invoked when a paste command is detected in the text area.
        /// </summary>
        /// <remarks>
        /// The consumer receives the <see cref="KeyDownEvent"/> and can call
        /// <see cref="EventBase.StopPropagation"/> to prevent the default text paste behavior
        /// if they want to handle it fully (e.g., pasting an image as an attachment).
        /// If propagation is not stopped, the default text paste still occurs.
        /// </remarks>
        public event Action<KeyDownEvent> pasted;

        /// <summary>
        /// The Avatar element of the ThreadComposer,
        /// which can be used to display the user's avatar next to the composer.
        /// </summary>
        public Avatar avatar { get; }

        /// <summary>
        /// The toolbar element of the ThreadComposer, which contains the attach actions.
        /// </summary>
        public VisualElement toolbar { get; }

        /// <summary>
        /// The TextArea element of the ThreadComposer, which is the main input area for the message content.
        /// </summary>
        public TextArea textArea { get; }

        /// <summary>
        /// The attachments container of the ThreadComposer,
        /// which can be used to display file attachments or previews related to the message being composed.
        /// </summary>
        public VisualElement attachments { get; }

        /// <summary>
        /// The DropZone element of the ThreadComposer,
        /// which can be used to accept drag and drop operations over the composer.
        /// </summary>
        /// <remarks>
        /// The DropZone is positioned absolutely to overlay the entire input area.
        /// Configure drag acceptance and handle drops using the <see cref="DropZone.controller"/> directly,
        /// e.g. <c>composer.dropZone.controller.acceptDrag = ...</c>.
        /// </remarks>
        public DropZone dropZone => m_DropZone;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ThreadComposer()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Position;
            focusable = true;
            delegatesFocus = true;
            this.SetIsCompositeRoot(true);
            this.SetExcludeFromFocusRing(true);
            passMask = 0;

            var avatarContainer = new VisualElement { name = avatarContainerUssClassName, pickingMode = PickingMode.Ignore };
            avatarContainer.AddToClassList(avatarContainerUssClassName);
            hierarchy.Add(avatarContainer);

            avatar = new Avatar { size = Size.S };
            avatar.AddToClassList(avatarUssClassName);
            avatar.autoLabelColor = true;
            avatarContainer.Add(avatar);

            m_TextAreaContainer = new VisualElement { pickingMode = PickingMode.Ignore };
            m_TextAreaContainer.AddToClassList(textAreaContainerUssClassName);
            hierarchy.Add(m_TextAreaContainer);

            textArea = new TextArea
            {
                name = textareaUssClassName,
                autoResize = true,
                autoShrink = true,
            };
#if UNITY_2022_3_OR_NEWER
            var field = textArea.Q<UnityEngine.UIElements.TextField>();
            var textElement = field.Q<TextElement>();
            textElement.enableRichText = true;
#endif
            textArea.AddToClassList(textareaUssClassName);
            textArea.submitted += OnTextAreaSubmitted;
            textArea.RegisterValueChangingCallback(e => OnTextAreaValueChanged(e.newValue));
            textArea.RegisterValueChangedCallback(e => OnTextAreaValueChanged(e.newValue));
            textArea.RegisterCallback<KeyDownEvent>(OnKeyDown, TrickleDown.TrickleDown);

            m_ToolbarContainer = new VisualElement { name = toolbarContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_ToolbarContainer.AddToClassList(toolbarContainerUssClassName);

            toolbar = new  VisualElement { name = toolbarUssClassName, pickingMode = PickingMode.Ignore };
            toolbar.AddToClassList(toolbarUssClassName);

            m_ActionBar = new VisualElement { name = actionbarUssClassName, pickingMode = PickingMode.Ignore };
            m_ActionBar.AddToClassList(actionbarUssClassName);

            m_AttachButton = new IconButton
            {
                name = attachBtnUssClassName,
                icon = "paperclip",
                size = Size.S,
                quiet = true
            };
            m_AttachButton.AddToClassList(attachBtnUssClassName);
            m_AttachButton.clicked += OnAttachClicked;

            m_SubmitButton = new IconButton
            {
                name = submitBtnUssClassName,
                icon = "arrow-up",
                size = Size.S,
                primary = true
            };
            m_SubmitButton.AddToClassList(submitBtnUssClassName);
            m_SubmitButton.clicked += OnSubmitClicked;

            m_CancelButton = new ActionButton { name = cancelBtnUssClassName, label = k_CancelText, quiet = true, size = Size.S };
            m_CancelButton.AddToClassList(cancelBtnUssClassName);
            m_CancelButton.AddToClassList(Styles.hiddenUssClassName);
            m_CancelButton.clicked += OnCancelClicked;

            m_SaveButton = new Button { name = saveBtnUssClassName, title = k_SaveText, size = Size.S, variant = ButtonVariant.Accent };
            m_SaveButton.AddToClassList(saveBtnUssClassName);
            m_SaveButton.AddToClassList(Styles.hiddenUssClassName);
            m_SaveButton.clicked += OnSaveClicked;

            m_Counter = new VisualElement { name = counterUssClassName, pickingMode = PickingMode.Ignore };
            m_Counter.AddToClassList(counterUssClassName);

            m_CounterCurrent = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_CounterCurrent.AddToClassList(counterCurrentUssClassName);

            var counterSeparator = new LocalizedTextElement { text = " / ", pickingMode = PickingMode.Ignore };
            counterSeparator.AddToClassList(counterSeparatorUssClassName);

            m_CounterMax = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_CounterMax.AddToClassList(counterMaxUssClassName);

            m_Counter.hierarchy.Add(m_CounterCurrent);
            m_Counter.hierarchy.Add(counterSeparator);
            m_Counter.hierarchy.Add(m_CounterMax);

            m_ToolbarContainer.hierarchy.Add(toolbar);
            toolbar.hierarchy.Add(m_AttachButton);
            m_ToolbarContainer.hierarchy.Add(m_Counter);
            m_ToolbarContainer.hierarchy.Add(m_ActionBar);
            m_ActionBar.hierarchy.Add(m_SubmitButton);
            m_ActionBar.hierarchy.Add(m_CancelButton);
            m_ActionBar.hierarchy.Add(m_SaveButton);

            m_SendingProgress = new CircularProgress
            {
                name = sendingProgressUssClassName,
                size = Size.S,
                pickingMode = PickingMode.Ignore
            };
            m_SendingProgress.AddToClassList(sendingProgressUssClassName);
            m_ActionBar.hierarchy.Add(m_SendingProgress);

            attachments = new VisualElement { name = attachmentsUssClassName, pickingMode = PickingMode.Ignore };
            attachments.AddToClassList(attachmentsUssClassName);

            m_AttachmentsScrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                name = attachmentsScrollUssClassName,
                pickingMode = PickingMode.Ignore,
                nestedInteractionKind = ScrollView.NestedInteractionKind.ForwardScrolling,
            };
            m_AttachmentsScrollView.AddToClassList(attachmentsScrollUssClassName);
            m_AttachmentsScrollView.Add(attachments);

            m_InputBox = new VisualElement
            {
                pickingMode = PickingMode.Ignore,
            };
            m_InputBox.AddToClassList(inputBoxUssClassName);

            m_InputBox.Add(textArea);
            m_InputBox.Add(m_ToolbarContainer);

            m_MentionPopoverAnchor = new VisualElement();

            m_DropZone = new DropZone { name = dropzoneUssClassName, pickingMode = PickingMode.Ignore };
            m_DropZone.AddToClassList(dropzoneUssClassName);

            m_TextAreaContainer.Add(m_MentionPopoverAnchor);
            m_TextAreaContainer.Add(m_InputBox);
            m_TextAreaContainer.Add(m_AttachmentsScrollView);
            m_TextAreaContainer.Add(m_DropZone);

            value = null;
            showAvatar = false;
            alwaysShowToolbar = false;
            maxTextLength = defaultMaxTextLength;

            this.RegisterContextChangedCallback<ThreadContext>(OnThreadContextChanged);
        }

        /// <summary>
        /// The content container of the ThreadComposer.
        /// </summary>
        public override VisualElement contentContainer => null;

        void OnThreadContextChanged(ContextChangedEvent<ThreadContext> evt)
        {
            m_MentionProvider = evt.context?.mentionProvider;
        }

        /// <summary>
        /// The placeholder text for the text area.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string placeholder
        {
            get => textArea.placeholder;
            set
            {
                var changed = textArea.placeholder != value;
                textArea.placeholder = value;
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in placeholderProperty);
#endif
            }
        }

        /// <summary>
        /// Whether to show the avatar next to the composer.
        /// When <c>true</c>, the avatar element is visible and can be set via the <see cref="avatar"/> property.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool showAvatar
        {
            get => ClassListContains(showAvatarUssClassName);
            set
            {
                var changed = showAvatar != value;
                EnableInClassList(showAvatarUssClassName, value);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in showAvatarProperty);
#endif
            }
        }

        /// <summary>
        /// Whether to always show the toolbar even when the text area is empty.
        /// When <c>false</c>, the toolbar is only shown when there is text in the text area.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool alwaysShowToolbar
        {
            get => m_AlwaysShowToolbar;
            set
            {
                var changed = m_AlwaysShowToolbar != value;
                m_AlwaysShowToolbar = value;
                EnableInClassList(forceToolbarUssClassName, alwaysShowToolbar);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in alwaysShowToolbarProperty);
#endif
            }
        }

        /// <summary>
        /// The maximum number of characters allowed in the text area.
        /// Drives the underlying <see cref="TextArea.maxLength"/> and the visible character counter
        /// (<c>current / max</c>) shown in the toolbar.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public int maxTextLength
        {
            get => m_MaxTextLength;
            set
            {
                var changed = m_MaxTextLength != value;
                m_MaxTextLength = value;
                textArea.maxLength = value;
                m_CounterMax.text = value.ToString();
                RefreshCounter();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in maxTextLengthProperty);
#endif
            }
        }

        /// <summary>
        /// The current text value.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public string value
        {
            get => textArea.value;
            set
            {
                var changed = textArea.value != value;
                textArea.value = value;
                RefreshStyles();
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                {
                    NotifyPropertyChanged(in valueProperty);
                    NotifyPropertyChanged(in encodedValueProperty);
                }
#endif
            }
        }

        /// <summary>
        /// Get the final encoded value of the text area, with any mentions encoded using the mention provider.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty(ReadOnly = true)]
#endif
        public string encodedValue
        {
            get
            {
                var v = value;
                foreach (var kvp in m_Mentions)
                {
                    v = v.Replace(kvp.Key, kvp.Value);
                }
                return v;
            }
        }

        /// <summary>
        /// The sending state of the composer,
        /// which can be used to disable input and show a loading state when a message is being submitted.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
        public bool sending
        {
            get => ClassListContains(sendingUssClassName);
            set
            {
                var changed = sending != value;
                EnableInClassList(sendingUssClassName, value);
                SetEnabled(!value);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in sendingProperty);
#endif
            }
        }

        /// <summary>
        /// Whether the composer is in editing mode.
        /// When <c>true</c>, the submit icon-button is hidden and replaced with Cancel and Save buttons.
        /// </summary>
#if ENABLE_RUNTIME_DATA_BINDINGS
        [CreateProperty]
#endif
#if ENABLE_UXML_SERIALIZED_DATA
        [UxmlAttribute]
#endif
        public bool isEditing
        {
            get => m_IsEditing;
            set
            {
                var changed = m_IsEditing != value;
                m_IsEditing = value;
                EnableInClassList(editingUssClassName, value);
#if ENABLE_RUNTIME_DATA_BINDINGS
                if (changed)
                    NotifyPropertyChanged(in isEditingProperty);
#endif
            }
        }

        /// <summary>
        /// Clears the mentions cache.
        /// This can be used to reset the mapping between mention display names and their encoded values,
        /// </summary>
        public void ClearMentionsCache()
        {
            m_Mentions.Clear();
#if ENABLE_RUNTIME_DATA_BINDINGS
            NotifyPropertyChanged(in encodedValueProperty);
#endif
        }

        void RefreshStyles()
        {
            EnableInClassList(emptyUssClassName, string.IsNullOrEmpty(value));
            m_SubmitButton.SetEnabled(!string.IsNullOrEmpty(value));
            m_SaveButton.SetEnabled(!string.IsNullOrEmpty(value));
            RefreshCounter();
        }

        void RefreshCounter()
        {
            var len = value?.Length ?? 0;
            m_CounterCurrent.text = len.ToString();
        }

        void OnTextAreaValueChanged(string _)
        {
            RefreshStyles();
        }

        void OnTextAreaSubmitted()
        {
            SubmitMessage();
        }

        void OnSubmitClicked()
        {
            SubmitMessage();
        }

        void OnSaveClicked()
        {
            saveClicked?.Invoke();
        }

        void OnAttachClicked()
        {
            attachClicked?.Invoke();
        }

        void OnCancelClicked()
        {
            cancelClicked?.Invoke();
        }

        void SubmitMessage()
        {
            var text = textArea.value;
            if (string.IsNullOrEmpty(text))
                return;

            submitClicked?.Invoke(encodedValue);
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.V && evt.actionKey)
            {
                pasted?.Invoke(evt);
                return;
            }

            if (m_MentionProvider == null)
                return;

            if (!m_IsMentioning)
            {
                if (evt.character == '@')
                {
                    m_IsMentioning = true;
                    m_MentionQuery = string.Empty;
                    var currentText = textArea.value ?? string.Empty;
                    m_MentionStartIndex = currentText.Length + 1;
                    UpdateMentionSuggestions(string.Empty);
                }
                return;
            }

            if (evt.keyCode == KeyCode.Escape)
            {
                DismissMentionPopover();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                if (m_MentionPicker is {suggestionsCount: > 0})
                    m_MentionPicker.AcceptSelection();
                else
                    DismissMentionPopover();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.Backspace)
            {
                if (string.IsNullOrEmpty(m_MentionQuery))
                    DismissMentionPopover();
                else
                {
                    m_MentionQuery = m_MentionQuery.Substring(0, m_MentionQuery.Length - 1);
                    UpdateMentionSuggestions(m_MentionQuery);
                }
                return;
            }

            if (evt.keyCode == KeyCode.UpArrow)
            {
                if (m_MentionPicker is {suggestionsCount: > 0})
                {
                    evt.StopPropagation();
                    m_MentionPicker.SelectPrevious();
                }
                return;
            }

            if (evt.keyCode == KeyCode.DownArrow)
            {
                if (m_MentionPicker is {suggestionsCount: > 0})
                {
                    evt.StopPropagation();
                    m_MentionPicker.SelectNext();
                }
                return;
            }

            if (evt.keyCode is KeyCode.Tab)
            {
                if (m_MentionPicker is {suggestionsCount: > 0})
                {
                    evt.StopPropagation();
                    m_MentionPicker.AcceptSelection();
                }
                return;
            }

            var c = evt.character;
            if (c == 0 || char.IsControl(c))
                return;

            if (c == ' ' && string.IsNullOrEmpty(m_MentionQuery))
            {
                DismissMentionPopover();
                return;
            }

            m_MentionQuery += c;
            UpdateMentionSuggestions(m_MentionQuery);
        }

        void UpdateMentionSuggestions(string query)
        {
            if (m_MentionProvider == null)
            {
                DismissMentionPopover();
                return;
            }

            if (m_MentionPicker == null)
            {
                m_MentionPicker = new MentionPicker();
                m_MentionPicker.mentionProvider = m_MentionProvider;
                m_MentionPicker.mentionSelected += InsertMention;
            }

            m_MentionPicker.query = query;
            m_MentionPicker.style.minWidth = m_MentionPopoverAnchor.layout.width;

            if (m_MentionPopover == null)
            {
                var popover = new Popover.PopoverVisualElement(m_MentionPicker);
                m_MentionPopoverAnchor.AddToClassList(Styles.noArrowUssClassName);
                m_MentionPopover = popover.popoverElement;
                m_MentionPopover.style.alignSelf = Align.Center;
                m_MentionPopover.style.position = Position.Absolute;
                m_MentionPopover.style.bottom = 0;
                m_MentionPopoverAnchor.Add(m_MentionPopover);
            }
        }

        void InsertMention(object mention)
        {
            var text = textArea.value ?? string.Empty;
            var atIndex = m_MentionStartIndex - 1;

            if (atIndex < 0 || atIndex >= text.Length)
            {
                DismissMentionPopover();
                return;
            }

            var before = text.Substring(0, atIndex);
            var after = m_MentionStartIndex < text.Length ? text.Substring(m_MentionStartIndex) : string.Empty;

            // Remove any partial query from after
            var spaceIdx = after.IndexOf(' ');
            if (spaceIdx >= 0)
                after = after.Substring(spaceIdx);
            else
                after = string.Empty;

            var mentionCode = m_MentionProvider.Encode(mention);
            var mentionFmt = "@" + m_MentionProvider.GetDisplayName(mention);
            m_Mentions[mentionFmt] = mentionCode;
            var part0 = before + mentionFmt;
            var l = part0.Length;
            textArea.value = part0 + after;

#if ENABLE_UITK_TEXT_SELECTION
            // place cursor at the end of the inserted mention
            schedule.Execute(() =>
            {
                var field = textArea.Q<UnityEngine.UIElements.TextField>();
                field.textSelection.cursorIndex = l;
                field.textSelection.selectIndex = l;
            });
#endif

            DismissMentionPopover();
        }

        void DismissMentionPopover()
        {
            m_IsMentioning = false;
            if (m_MentionPopover != null)
            {
                m_MentionPopover.RemoveFromHierarchy();
                m_MentionPopover = null;
            }

            if (m_MentionPicker != null)
            {
                m_MentionPicker.mentionSelected -= InsertMention;
                m_MentionPicker = null;
            }
        }

#if ENABLE_UXML_TRAITS
        /// <summary>
        /// Defines the UxmlFactory for the ThreadComposer.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<ThreadComposer, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="ThreadComposer"/>.
        /// </summary>
        public new class UxmlTraits : ExVisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Placeholder = new UxmlStringAttributeDescription
            {
                name = "placeholder",
                defaultValue = null
            };

            readonly UxmlStringAttributeDescription m_Value = new UxmlStringAttributeDescription
            {
                name = "value",
                defaultValue = null
            };

            readonly UxmlBoolAttributeDescription m_IsEditing = new UxmlBoolAttributeDescription
            {
                name = "is-editing",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_Sending = new UxmlBoolAttributeDescription
            {
                name = "sending",
                defaultValue = false,
            };

            readonly UxmlBoolAttributeDescription m_ShowAvatar = new UxmlBoolAttributeDescription
            {
                name = "show-avatar",
                defaultValue = false,
            };

             readonly UxmlBoolAttributeDescription m_AlwaysShowToolbar = new UxmlBoolAttributeDescription
            {
                name = "always-show-toolbar",
                defaultValue = false,
            };

            readonly UxmlIntAttributeDescription m_MaxTextLength = new UxmlIntAttributeDescription
            {
                name = "max-text-length",
                defaultValue = defaultMaxTextLength,
            };

            /// <summary>
            /// Initializes the VisualElement from the UXML attributes.
            /// </summary>
            /// <param name="ve"> The <see cref="VisualElement"/> to initialize.</param>
            /// <param name="bag"> The <see cref="IUxmlAttributes"/> bag to use to initialize the <see cref="VisualElement"/>.</param>
            /// <param name="cc"> The <see cref="CreationContext"/> to use to initialize the <see cref="VisualElement"/>.</param>
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                var el = (ThreadComposer)ve;
                el.placeholder = m_Placeholder.GetValueFromBag(bag, cc);
                el.value = m_Value.GetValueFromBag(bag, cc);
                el.isEditing = m_IsEditing.GetValueFromBag(bag, cc);
                el.sending = m_Sending.GetValueFromBag(bag, cc);
                el.showAvatar = m_ShowAvatar.GetValueFromBag(bag, cc);
                el.alwaysShowToolbar = m_AlwaysShowToolbar.GetValueFromBag(bag, cc);
                el.maxTextLength = m_MaxTextLength.GetValueFromBag(bag, cc);
            }
        }
#endif
    }
}
