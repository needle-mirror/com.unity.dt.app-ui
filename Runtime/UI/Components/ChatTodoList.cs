using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// How much room a <see cref="ChatTodoList"/> takes.
    /// </summary>
    public enum ChatTodoListSize
    {
        /// <summary>
        /// Tighter spacing, one type step down, and every row forced to a single line. For a list
        /// riding alongside other content rather than owning its surface.
        /// </summary>
        Sm,

        /// <summary>
        /// The default.
        /// </summary>
        Md,
    }

    /// <summary>
    /// The colour of a completed item's check in a <see cref="ChatTodoList"/>.
    /// </summary>
    public enum ChatTodoCompletedTone
    {
        /// <summary>
        /// Reads as "this passed": the semantic positive colour. The default.
        /// </summary>
        Positive,

        /// <summary>
        /// Reads as progress: the accent, for a host whose other progress, a bar or a stepper, is drawn
        /// in the accent.
        /// </summary>
        Accent,
    }

    /// <summary>
    /// Where one <see cref="ChatTodoItem"/> is in its lifecycle.
    /// </summary>
    /// <remarks>
    /// <see cref="Completed"/> and <see cref="Cancelled"/> are the terminal states: both strike the
    /// item through, and only <see cref="Completed"/> counts towards the header's tally.
    /// </remarks>
    public enum ChatTodoStatus
    {
        /// <summary>
        /// Not started. Drawn as an empty outline circle.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Being worked on right now. Drawn as a spinner.
        /// </summary>
        InProgress,

        /// <summary>
        /// Finished successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Abandoned. Terminal, but not a success.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// One line of an agent's checklist.
    /// </summary>
    public readonly struct ChatTodoItem
    {
        /// <summary>
        /// An optional stable identity for the item. Purely informational; the list does not key on it.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// What the item says.
        /// </summary>
        public string text { get; }

        /// <summary>
        /// Where the item is in its lifecycle.
        /// </summary>
        public ChatTodoStatus status { get; }

        /// <summary>
        /// Constructs an item.
        /// </summary>
        /// <param name="text"> What the item says. </param>
        /// <param name="status"> Where the item is in its lifecycle. </param>
        /// <param name="id"> An optional stable identity. </param>
        public ChatTodoItem(string text, ChatTodoStatus status = ChatTodoStatus.Pending, string id = null)
        {
            this.text = text;
            this.status = status;
            this.id = id;
        }
    }

    /// <summary>
    /// A collapsible checklist an agent maintains while it works through a multi-step task.
    /// </summary>
    /// <remarks>
    /// The list is read-only: the caller owns the items and their status, and re-assigns
    /// <see cref="items"/> whenever the agent updates the plan. Only the open/closed state belongs to
    /// the element.
    ///
    /// The header shows the title, how many items are <see cref="ChatTodoStatus.Completed"/> out of the
    /// total, and a caret. Clicking anywhere on it toggles <see cref="open"/>. An empty list hides
    /// itself entirely rather than showing an empty box, matching the web implementation.
    ///
    /// The in-progress glyph is a static ring with a filled centre rather than a spinner. A
    /// checklist shows several rows at once, and a spinning one pulls the eye away from the text,
    /// which is the part that actually changes.
    ///
    /// The body is a <see cref="ScrollView"/>. When a new list arrives, or the list opens, and the
    /// in-progress item is out of view, the body scrolls by the least amount that brings it in, and
    /// only the body: no scrollable ancestor moves. The registry caps the body at a fixed height; no
    /// sizing token matches that cap, so the body grows with its items until a host sets a
    /// <c>max-height</c> on it.
    /// </remarks>
    /// <example>
    /// <para>A checklist mid-run.</para>
    /// <code lang="csharp">
    /// var todo = new ChatTodoList
    /// {
    ///     items = new[]
    ///     {
    ///         new ChatTodoItem("Read the player controller", ChatTodoStatus.Completed),
    ///         new ChatTodoItem("Raise the jump force", ChatTodoStatus.InProgress),
    ///         new ChatTodoItem("Re-run the tests"),
    ///     }
    /// };
    /// parentElement.Add(todo);
    /// </code>
    /// <para>Starting collapsed, under a different heading.</para>
    /// <code lang="xml">
    /// &lt;appui:ChatTodoList title="Plan" open="false" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class ChatTodoList : BaseVisualElement
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId openProperty = nameof(open);

        internal static readonly BindingId collapsibleProperty = nameof(collapsible);

        internal static readonly BindingId showMaximizeProperty = nameof(showMaximize);

        internal static readonly BindingId showCloseProperty = nameof(showClose);

        internal static readonly BindingId sizeProperty = nameof(size);

        internal static readonly BindingId completedToneProperty = nameof(completedTone);

        /// <summary>
        /// The ChatTodoList main styling class.
        /// </summary>
        public const string ussClassName = "appui-chat-todo-list";

        /// <summary>
        /// The styling class of the header.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the header's leading glyph.
        /// </summary>
        public const string headerIconUssClassName = ussClassName + "__header-icon";

        /// <summary>
        /// The styling class of the header's title.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the "done out of total" tally.
        /// </summary>
        public const string tallyUssClassName = ussClassName + "__tally";

        /// <summary>
        /// The styling class of the header's caret.
        /// </summary>
        public const string caretUssClassName = ussClassName + "__caret";

        /// <summary>
        /// The styling class of the maximize action.
        /// </summary>
        public const string maximizeUssClassName = ussClassName + "__maximize";

        /// <summary>
        /// The styling class of the close action.
        /// </summary>
        public const string closeUssClassName = ussClassName + "__close";

        /// <summary>
        /// The styling class prefix carrying the size variant.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(ChatTodoListSize))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The styling class prefix carrying the completed check's tone.
        /// </summary>
        [EnumName("GetCompletedToneUssClassName", typeof(ChatTodoCompletedTone))]
        public const string completedToneUssClassName = ussClassName + "--completed-tone-";

        /// <summary>
        /// The styling class of the item container.
        /// </summary>
        public const string itemsUssClassName = ussClassName + "__items";

        /// <summary>
        /// The styling class of one item row.
        /// </summary>
        public const string itemUssClassName = ussClassName + "__item";

        /// <summary>
        /// The styling class of an item row's status glyph slot.
        /// </summary>
        public const string itemStatusUssClassName = itemUssClassName + "-status";

        /// <summary>
        /// The styling class of an item row's text.
        /// </summary>
        public const string itemTextUssClassName = itemUssClassName + "-text";

        /// <summary>
        /// The styling class carrying an item row's status.
        /// </summary>
        [EnumName("GetStatusUssClassName", typeof(ChatTodoStatus))]
        public const string itemStatusVariantUssClassName = itemUssClassName + "--status-";

        /// <summary>
        /// The styling class of the empty outline circle a pending item is drawn with.
        /// </summary>
        public const string pendingGlyphUssClassName = ussClassName + "__pending-glyph";

        /// <summary>
        /// The styling class of the in-progress glyph: a static ring with a filled centre.
        /// </summary>
        public const string inProgressGlyphUssClassName = ussClassName + "__in-progress-glyph";

        /// <summary>
        /// The default header label.
        /// </summary>
        public const string defaultTitle = "Tasks";

        readonly VisualElement m_Header;

        readonly LocalizedTextElement m_Title;

        readonly LocalizedTextElement m_Tally;

        readonly Icon m_Caret;

        readonly ScrollView m_Items;

        ChatTodoCompletedTone m_CompletedTone = ChatTodoCompletedTone.Positive;

        VisualElement m_PendingScrollRow;

        readonly Pressable m_Clickable;

        readonly List<ChatTodoItem> m_ItemsData = new List<ChatTodoItem>();

        bool m_Open = true;

        bool m_Collapsible = true;

        ChatTodoListSize m_Size = ChatTodoListSize.Md;

        readonly IconButton m_MaximizeButton;

        readonly IconButton m_CloseButton;

        /// <summary>
        /// The header's press handler. Toggles <see cref="open"/>.
        /// </summary>
        public Pressable clickable => m_Clickable;

        /// <summary>
        /// Raised when the maximize action is activated. Shown by <see cref="showMaximize"/>.
        /// </summary>
        public event Action maximizeRequested;

        /// <summary>
        /// Raised when the close action is activated. Shown by <see cref="showClose"/>.
        /// </summary>
        public event Action closeRequested;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ChatTodoList()
        {
            AddToClassList(ussClassName);

            m_Header = new VisualElement { name = headerUssClassName, focusable = true };
            m_Header.AddToClassList(headerUssClassName);

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

            m_Tally = new LocalizedTextElement { name = tallyUssClassName, pickingMode = PickingMode.Ignore };
            m_Tally.AddToClassList(tallyUssClassName);

            m_Caret = new Icon { name = caretUssClassName, size = IconSize.Xs, pickingMode = PickingMode.Ignore };
            m_Caret.AddToClassList(caretUssClassName);

            m_MaximizeButton = new IconButton("arrows-out-simple", () => maximizeRequested?.Invoke())
            {
                name = maximizeUssClassName,
                size = Size.Sm,
                tooltip = "Expand task list",
            };
            m_MaximizeButton.AddToClassList(maximizeUssClassName);

            m_CloseButton = new IconButton("x", () => closeRequested?.Invoke())
            {
                name = closeUssClassName,
                size = Size.Sm,
                tooltip = "Hide task list",
            };
            m_CloseButton.AddToClassList(closeUssClassName);

            m_Header.hierarchy.Add(headerIcon);
            m_Header.hierarchy.Add(m_Title);
            m_Header.hierarchy.Add(m_Tally);
            m_Header.hierarchy.Add(m_Caret);
            // Siblings of the caret, not children of it: the actions are their own targets and
            // must not be swallowed by the header's toggle.
            m_Header.hierarchy.Add(m_MaximizeButton);
            m_Header.hierarchy.Add(m_CloseButton);

            // A ScrollView so a long plan scrolls within the card and the in-progress item can be kept
            // in view, as the registry's overflow-y-auto list does.
            m_Items = new ScrollView(ScrollViewMode.Vertical)
            {
                name = itemsUssClassName,
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
            };
            m_Items.AddToClassList(itemsUssClassName);

            hierarchy.Add(m_Header);
            hierarchy.Add(m_Items);

            // Gated on collapsible, not just on the header's pickingMode: a press that arrives
            // without hit-testing -- a synthesized one, or a binding -- would otherwise still
            // close a list that has no caret to open it again.
            m_Clickable = new Pressable(() =>
            {
                if (m_Collapsible)
                    open = !open;
            });
            m_Header.AddManipulator(m_Clickable);

            title = defaultTitle;
            open = true;
            collapsible = true;
            showMaximize = false;
            showClose = false;
            size = ChatTodoListSize.Md;
            completedTone = ChatTodoCompletedTone.Positive;
            Refresh();
        }

        /// <summary>
        /// The header label. "Tasks" by default.
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
        /// Whether the item list is expanded. True by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool open
        {
            get => m_Open;
            set
            {
                var changed = m_Open != value;
                m_Open = value;
                EnableInClassList(Styles.openUssClassName, m_Open);
                m_Items.EnableInClassList(Styles.hiddenUssClassName, !m_Open);
                m_Caret.iconName = m_Open ? "caret-up" : "caret-down";

                // The registry mounts the list again on expand, which brings the current step into
                // view; a list collapsed while the plan moved on does the same here.
                if (changed && m_Open)
                    ScrollToCurrentOnceLaidOut();

                if (changed)
                    NotifyPropertyChanged(in openProperty);
            }
        }

        /// <summary>
        /// Whether the header collapses the list. True by default.
        /// </summary>
        /// <remarks>
        /// Off, the body stays open, the caret goes away and the header no longer responds to a
        /// press, so <see cref="open"/> is ignored while it is off. For a list that owns its
        /// surface and has nothing to collapse into.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool collapsible
        {
            get => m_Collapsible;
            set
            {
                var changed = m_Collapsible != value;
                m_Collapsible = value;

                m_Caret.EnableInClassList(Styles.hiddenUssClassName, !m_Collapsible);
                m_Header.pickingMode = m_Collapsible ? PickingMode.Position : PickingMode.Ignore;
                m_Header.focusable = m_Collapsible;

                if (!m_Collapsible)
                    open = true;

                if (changed)
                    NotifyPropertyChanged(in collapsibleProperty);
            }
        }

        /// <summary>
        /// Whether the header offers a maximize action. False by default.
        /// </summary>
        /// <remarks>
        /// The registry shows the button whenever an <c>onMaximize</c> handler is passed. C# has no
        /// way to omit a prop, so the control is an explicit flag and
        /// <see cref="maximizeRequested"/> carries the intent.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool showMaximize
        {
            get => !m_MaximizeButton.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = showMaximize != value;
                m_MaximizeButton.EnableInClassList(Styles.hiddenUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in showMaximizeProperty);
            }
        }

        /// <summary>
        /// Whether the header offers a close action. False by default. See
        /// <see cref="showMaximize"/>.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showClose
        {
            get => !m_CloseButton.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = showClose != value;
                m_CloseButton.EnableInClassList(Styles.hiddenUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in showCloseProperty);
            }
        }

        /// <summary>
        /// How much room the list takes. <see cref="ChatTodoListSize.Md"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ChatTodoListSize size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The checklist itself. Assigning re-renders every row.
        /// </summary>
        /// <remarks>
        /// The setter copies the incoming sequence, so a caller mutating its own list afterwards does
        /// not silently desynchronise what is on screen. Null clears the list.
        /// </remarks>
        public IEnumerable<ChatTodoItem> items
        {
            get => m_ItemsData;
            set
            {
                m_ItemsData.Clear();
                if (value != null)
                    m_ItemsData.AddRange(value);
                Refresh();
            }
        }

        /// <summary>
        /// How many items are <see cref="ChatTodoStatus.Completed"/>.
        /// </summary>
        public int completedCount
        {
            get
            {
                var count = 0;
                foreach (var item in m_ItemsData)
                {
                    if (item.status == ChatTodoStatus.Completed)
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// How many items the list holds.
        /// </summary>
        public int itemCount => m_ItemsData.Count;

        /// <summary>
        /// The colour of a completed item's check. <see cref="ChatTodoCompletedTone.Positive"/> by
        /// default.
        /// </summary>
        /// <remarks>
        /// The registry's <c>completedTone</c>, which it puts on <c>ChatTodoItems</c>, the bare list
        /// inside the card. UI Toolkit has no separate bare list, so it lives here.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public ChatTodoCompletedTone completedTone
        {
            get => m_CompletedTone;
            set
            {
                var changed = m_CompletedTone != value;
                RemoveFromClassList(GetCompletedToneUssClassName(m_CompletedTone));
                m_CompletedTone = value;
                AddToClassList(GetCompletedToneUssClassName(m_CompletedTone));

                if (changed)
                    NotifyPropertyChanged(in completedToneProperty);
            }
        }

        /// <summary>
        /// The row of the item currently <see cref="ChatTodoStatus.InProgress"/>, or null.
        /// </summary>
        VisualElement currentRow
        {
            get
            {
                var index = m_ItemsData.FindIndex(i => i.status == ChatTodoStatus.InProgress);
                return index < 0 || index >= m_Items.childCount ? null : m_Items[index];
            }
        }

        void Refresh()
        {
            m_Items.Clear();

            // An empty checklist hides itself rather than showing an empty box.
            EnableInClassList(Styles.hiddenUssClassName, m_ItemsData.Count == 0);

            m_Tally.text = completedCount.ToString(CultureInfo.InvariantCulture) + "/" +
                m_ItemsData.Count.ToString(CultureInfo.InvariantCulture);

            foreach (var item in m_ItemsData)
                m_Items.Add(CreateRow(item));

            // A new list can move the current item, so it is brought into view once it is laid out.
            ScrollToCurrentOnceLaidOut();
        }

        /// <summary>
        /// Brings the in-progress row into view after its next layout pass.
        /// </summary>
        /// <remarks>
        /// Only when a new list arrives or the list opens, not on every layout, so reading further down
        /// the list is not undone. The row's own geometry event is the signal: its bounds are only
        /// meaningful once it has been laid out, and a freshly built or re-shown row always gets one.
        /// </remarks>
        void ScrollToCurrentOnceLaidOut()
        {
            if (m_PendingScrollRow != null)
                m_PendingScrollRow.UnregisterCallback<GeometryChangedEvent>(OnCurrentRowGeometryChanged);

            m_PendingScrollRow = currentRow;
            m_PendingScrollRow?.RegisterCallback<GeometryChangedEvent>(OnCurrentRowGeometryChanged);
        }

        void OnCurrentRowGeometryChanged(GeometryChangedEvent evt)
        {
            var row = (VisualElement)evt.currentTarget;
            // Wait for a real layout: a row inside a hidden body has no extent yet.
            if (row.layout.height <= 0f || m_Items.contentViewport.layout.height <= 0f)
                return;

            row.UnregisterCallback<GeometryChangedEvent>(OnCurrentRowGeometryChanged);
            if (m_PendingScrollRow == row)
                m_PendingScrollRow = null;

            // The ScrollView updates its scroll range from its own geometry event in this same pass, and
            // an offset set before that is clamped to the old range. Scroll on the next tick instead.
            // ScrollTo works in the content's own units, so a scaled ancestor does not throw it off, and
            // it leaves the offset alone when the row is already in view.
            m_Items.schedule.Execute(() =>
            {
                // A newer list may have replaced the row before this ran.
                if (m_Items.Contains(row))
                    m_Items.ScrollTo(row);
            });
        }

        static VisualElement CreateRow(ChatTodoItem item)
        {
            var row = new VisualElement { name = itemUssClassName, pickingMode = PickingMode.Ignore };
            row.AddToClassList(itemUssClassName);
            row.AddToClassList(GetStatusUssClassName(item.status));

            var status = new VisualElement { name = itemStatusUssClassName, pickingMode = PickingMode.Ignore };
            status.AddToClassList(itemStatusUssClassName);
            status.Add(CreateStatusGlyph(item.status));

            var text = new LocalizedTextElement
            {
                name = itemTextUssClassName,
                text = item.text,
                pickingMode = PickingMode.Ignore,
            };
            text.AddToClassList(itemTextUssClassName);

            row.hierarchy.Add(status);
            row.hierarchy.Add(text);

            return row;
        }

        static VisualElement CreateStatusGlyph(ChatTodoStatus status)
        {
            switch (status)
            {
                case ChatTodoStatus.Completed:
                    return new Icon { iconName = "check-circle", size = IconSize.Sm, pickingMode = PickingMode.Ignore };
                case ChatTodoStatus.InProgress:
                    // Deliberately static: a ring with a filled centre, not a spinner. A checklist
                    // shows several rows at once and a spinning one pulls the eye away from the
                    // text, which is the part that changes. The registry made the same call.
                    var ring = new VisualElement
                    {
                        name = inProgressGlyphUssClassName,
                        pickingMode = PickingMode.Ignore,
                    };
                    ring.AddToClassList(inProgressGlyphUssClassName);
                    return ring;
                case ChatTodoStatus.Cancelled:
                    return new Icon { iconName = "x", size = IconSize.Sm, pickingMode = PickingMode.Ignore };
                default:
                    var circle = new VisualElement { name = pendingGlyphUssClassName, pickingMode = PickingMode.Ignore };
                    circle.AddToClassList(pendingGlyphUssClassName);
                    return circle;
            }
        }
    }
}
