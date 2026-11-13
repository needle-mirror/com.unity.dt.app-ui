using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A labelled, sectioned vertical navigation surface — the left nav of an app shell.
    /// </summary>
    /// <remarks>
    /// Sidebar is a compound component. It is a bordered column on its own and gets its structure from
    /// the sub-elements you compose into it: <see cref="SidebarHeader"/>, <see cref="SidebarContent"/>
    /// (holding <see cref="SidebarSection"/>s of <see cref="SidebarItem"/>s) and
    /// <see cref="SidebarFooter"/>. Every part is optional.
    ///
    /// It is wider and text-first, unlike the icon-only <see cref="BottomNavBar"/>, and it owns a
    /// single-selection group: any <see cref="SidebarItem"/> carrying a <see cref="SidebarItem.value"/>
    /// participates, and activating one sets <see cref="selectedId"/> and raises
    /// <see cref="selectionChanged"/>. Items with no value are plain action buttons and never change
    /// the selection.
    ///
    /// **Width is not a design token.** The registry's sidebar is `w-60` (240px) and the App UI sizing
    /// ramp tops out well below that, so <see cref="width"/> is a plain pixel property with the
    /// registry's default rather than a fabricated token. Everything else — padding, gaps, colours,
    /// radii — comes from `--appui-*` tokens and rescales with `Panel.density`.
    ///
    /// UI Toolkit has no `gap`, so the vertical rhythm is a leading margin on every child with the
    /// first one tagged <see cref="Styles.firstChildUssClassName"/>; <see cref="SidebarSlot"/> keeps
    /// those markers fresh on every layout pass.
    /// </remarks>
    /// <example>
    /// <para>An app shell nav composed in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:Sidebar selected-id="assets">
    ///     <appui:SidebarHeader>
    ///         <appui:Text text="My Project" />
    ///     </appui:SidebarHeader>
    ///     <appui:SidebarContent>
    ///         <appui:SidebarSection heading="Workspace">
    ///             <appui:SidebarItem value="assets" label="Assets" icon="folder" />
    ///             <appui:SidebarItem value="scenes" label="Scenes" icon="stack" shortcut="Ctrl+2" />
    ///         </appui:SidebarSection>
    ///     </appui:SidebarContent>
    ///     <appui:SidebarFooter>
    ///         <appui:SidebarItem label="Settings" icon="gear" />
    ///     </appui:SidebarFooter>
    /// </appui:Sidebar>
    /// ]]></code>
    /// <para>Reacting to the selection in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var sidebar = new Sidebar { selectedId = "assets" };
    /// sidebar.selectionChanged += id => Debug.Log($"navigate to {id}");
    ///
    /// var content = new SidebarContent();
    /// var section = new SidebarSection { heading = "Workspace" };
    /// section.Add(new SidebarItem { value = "assets", label = "Assets", icon = "folder" });
    /// section.Add(new SidebarItem { value = "scenes", label = "Scenes", icon = "stack" });
    /// content.Add(section);
    /// sidebar.Add(content);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class Sidebar : SidebarSlot
    {
        internal static readonly BindingId selectedIdProperty = nameof(selectedId);

        internal static readonly BindingId widthProperty = nameof(width);

        /// <summary>
        /// The Sidebar main styling class.
        /// </summary>
        public const string ussClassName = "appui-sidebar";

        /// <summary>
        /// The default width of a Sidebar, in pixels.
        /// </summary>
        /// <remarks>
        /// This mirrors the registry's `w-60`. It is deliberately a literal and not a token: the App UI
        /// sizing ramp has no step this wide.
        /// </remarks>
        public const float defaultWidth = 240f;

        string m_SelectedId;

        float m_Width = defaultWidth;

        /// <summary>
        /// The <see cref="SidebarItem.value"/> of the currently selected item, or `null` when nothing
        /// is selected.
        /// </summary>
        /// <remarks>
        /// Setting this re-tags every descendant item, so it is safe to set before or after the items
        /// have been added.
        /// </remarks>
        [Tooltip("The value of the currently selected SidebarItem.")]
        [CreateProperty]
        [UxmlAttribute]
        public string selectedId
        {
            get => m_SelectedId;
            set
            {
                var changed = m_SelectedId != value;
                m_SelectedId = value;
                RefreshSelection();

                if (changed)
                    NotifyPropertyChanged(in selectedIdProperty);
            }
        }

        /// <summary>
        /// The width of the Sidebar, in pixels. Defaults to <see cref="defaultWidth"/>.
        /// </summary>
        [Tooltip("The width of the Sidebar, in pixels.")]
        [CreateProperty]
        [UxmlAttribute]
        public float width
        {
            get => m_Width;
            set
            {
                var changed = !Mathf.Approximately(m_Width, value);
                m_Width = Mathf.Max(0f, value);
                style.width = m_Width;

                if (changed)
                    NotifyPropertyChanged(in widthProperty);
            }
        }

        /// <summary>
        /// Raised with the <see cref="SidebarItem.value"/> of an item the user has just activated.
        /// </summary>
        /// <remarks>
        /// Only items carrying a value raise this. It fires on user activation, not when
        /// <see cref="selectedId"/> is set programmatically.
        /// </remarks>
        public event Action<string> selectionChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Sidebar()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            width = defaultWidth;
            selectedId = null;
        }

        internal void OnItemActivated(SidebarItem item)
        {
            if (item?.value == null)
                return;

            selectedId = item.value;
            selectionChanged?.Invoke(item.value);
        }

        /// <summary>
        /// Re-applies the active state to every <see cref="SidebarItem"/> in this Sidebar.
        /// </summary>
        /// <remarks>
        /// This runs automatically when <see cref="selectedId"/> changes and on every layout pass. Call
        /// it directly only if you mutate the item tree and need the state before the next layout.
        /// </remarks>
        public void RefreshSelection()
        {
            RefreshSelection(this);
        }

        void RefreshSelection(VisualElement root)
        {
            for (var i = 0; i < root.hierarchy.childCount; i++)
            {
                var child = root.hierarchy[i];
                if (child is SidebarItem item)
                    item.SyncActiveFromSelection(m_SelectedId);
                RefreshSelection(child);
            }
        }

        /// <inheritdoc />
        protected override void OnSlotLayout()
        {
            RefreshSelection();
        }
    }

    /// <summary>
    /// Base class for the <see cref="Sidebar"/> containers that space their children evenly.
    /// </summary>
    /// <remarks>
    /// UI Toolkit has no `gap` property, so the spacing is emulated: every child carries a leading
    /// margin and the first one is tagged with <see cref="Styles.firstChildUssClassName"/>, which the
    /// stylesheet uses to zero that margin. The tag is refreshed whenever the slot's layout changes, so
    /// adding, inserting and removing children all keep the spacing correct.
    /// </remarks>
    public abstract class SidebarSlot : BaseVisualElement
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        protected SidebarSlot()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            RefreshChildMarkers();
            OnSlotLayout();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshChildMarkers();
            OnSlotLayout();
        }

        /// <summary>
        /// Called after the child markers have been refreshed, on attach and on every layout change.
        /// </summary>
        protected virtual void OnSlotLayout() { }

        /// <summary>
        /// Re-tags the first child of this slot so it renders without a leading gap.
        /// </summary>
        /// <remarks>
        /// This runs automatically on layout. Call it directly only if you mutate the children and need
        /// the markers before the next layout pass.
        /// </remarks>
        public void RefreshChildMarkers()
        {
            for (var i = 0; i < childCount; i++)
                this[i].EnableInClassList(Styles.firstChildUssClassName, i == 0);
        }
    }

    /// <summary>
    /// The top block of a <see cref="Sidebar"/>, above the scrolling <see cref="SidebarContent"/>.
    /// </summary>
    /// <remarks>
    /// A tight vertical stack for the project name, a workspace switcher or a search field. It never
    /// scrolls and never shrinks.
    /// </remarks>
    [UxmlElement]
    public partial class SidebarHeader : SidebarSlot
    {
        /// <summary>
        /// The SidebarHeader main styling class.
        /// </summary>
        public const string ussClassName = Sidebar.ussClassName + "__header";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarHeader()
        {
            AddToClassList(ussClassName);
        }
    }

    /// <summary>
    /// The scrolling middle of a <see cref="Sidebar"/>, holding the <see cref="SidebarSection"/>s.
    /// </summary>
    /// <remarks>
    /// This is the only part of the Sidebar that grows, so the header and footer stay pinned while a
    /// long nav scrolls between them.
    /// </remarks>
    [UxmlElement]
    public partial class SidebarContent : SidebarSlot
    {
        /// <summary>
        /// The SidebarContent main styling class.
        /// </summary>
        public const string ussClassName = Sidebar.ussClassName + "__content";

        /// <summary>
        /// The styling class of the inner scroll view.
        /// </summary>
        public const string scrollViewUssClassName = ussClassName + "-scroll-view";

        readonly ScrollView m_ScrollView;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_ScrollView.contentContainer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarContent()
        {
            AddToClassList(ussClassName);

            m_ScrollView = new ScrollView { mode = ScrollViewMode.Vertical };
            m_ScrollView.AddToClassList(scrollViewUssClassName);
            hierarchy.Add(m_ScrollView);
        }
    }

    /// <summary>
    /// The bottom block of a <see cref="Sidebar"/>, pushed against its lower edge.
    /// </summary>
    /// <remarks>
    /// Usually holds the account switcher, a settings entry or a help link. It is pinned to the bottom
    /// regardless of how short the <see cref="SidebarContent"/> above it is.
    /// </remarks>
    [UxmlElement]
    public partial class SidebarFooter : SidebarSlot
    {
        /// <summary>
        /// The SidebarFooter main styling class.
        /// </summary>
        public const string ussClassName = Sidebar.ussClassName + "__footer";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarFooter()
        {
            AddToClassList(ussClassName);
        }
    }

    /// <summary>
    /// A titled group of <see cref="SidebarItem"/>s inside a <see cref="SidebarContent"/>.
    /// </summary>
    /// <remarks>
    /// Setting <see cref="heading"/> inserts a <see cref="SidebarSectionHeading"/> above the items;
    /// clearing it removes the heading again. A section with no heading is just a tightly spaced group.
    /// </remarks>
    /// <example>
    /// <code lang="xml"><![CDATA[
    /// <appui:SidebarSection heading="Workspace">
    ///     <appui:SidebarItem value="assets" label="Assets" />
    ///     <appui:SidebarItem value="scenes" label="Scenes" />
    /// </appui:SidebarSection>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class SidebarSection : SidebarSlot
    {
        internal static readonly BindingId headingProperty = nameof(heading);

        /// <summary>
        /// The SidebarSection main styling class.
        /// </summary>
        public const string ussClassName = Sidebar.ussClassName + "__section";

        /// <summary>
        /// The styling class of the container the section's items live in.
        /// </summary>
        public const string containerUssClassName = ussClassName + "-container";

        readonly VisualElement m_Container;

        SidebarSectionHeading m_Heading;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The section heading text, or `null` for no heading.
        /// </summary>
        /// <remarks>
        /// The value is a localization key, like any other App UI text element.
        /// </remarks>
        [Tooltip("The section heading text. Leave empty for no heading.")]
        [CreateProperty]
        [UxmlAttribute]
        public string heading
        {
            get => m_Heading?.text;
            set
            {
                var changed = heading != value;

                if (string.IsNullOrEmpty(value))
                {
                    if (m_Heading != null)
                    {
                        hierarchy.Remove(m_Heading);
                        m_Heading = null;
                    }
                }
                else
                {
                    if (m_Heading == null)
                    {
                        m_Heading = new SidebarSectionHeading();
                        hierarchy.Insert(0, m_Heading);
                    }
                    m_Heading.text = value;
                }

                if (changed)
                    NotifyPropertyChanged(in headingProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarSection()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            heading = null;
        }
    }

    /// <summary>
    /// The small, muted label above a <see cref="SidebarSection"/>'s items.
    /// </summary>
    /// <remarks>
    /// A localized text element at the smallest type step in the disabled text colour. You rarely
    /// construct one directly — set <see cref="SidebarSection.heading"/> instead.
    /// </remarks>
    [UxmlElement]
    public partial class SidebarSectionHeading : LocalizedTextElement
    {
        /// <summary>
        /// The SidebarSectionHeading main styling class.
        /// </summary>
        public new const string ussClassName = Sidebar.ussClassName + "__section-heading";

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarSectionHeading()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
        }
    }

    /// <summary>
    /// A single row of a <see cref="Sidebar"/>: an optional leading icon, a label and an optional
    /// trailing shortcut hint.
    /// </summary>
    /// <remarks>
    /// An item carrying a <see cref="value"/> joins the enclosing <see cref="Sidebar"/>'s
    /// single-selection group — activating it sets <see cref="Sidebar.selectedId"/> and the item paints
    /// itself active. An item with no value is a plain action button: it still raises
    /// <see cref="clicked"/> but never changes the selection, which is what the footer entries want.
    ///
    /// <see cref="active"/> can also be set directly to force the state, for a nav whose current route
    /// is owned by something other than the Sidebar. A forced value on a valued item is overwritten the
    /// next time the selection is refreshed.
    /// </remarks>
    /// <example>
    /// <code lang="csharp"><![CDATA[
    /// var item = new SidebarItem { value = "assets", label = "Assets", icon = "folder", shortcut = "Ctrl+1" };
    /// item.clicked += () => Debug.Log("Assets clicked");
    /// ]]></code>
    /// </example>
    [UxmlElement]
    public partial class SidebarItem : BaseVisualElement, IPressable
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId shortcutProperty = nameof(shortcut);

        internal static readonly BindingId activeProperty = nameof(active);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        /// <summary>
        /// The SidebarItem main styling class.
        /// </summary>
        public const string ussClassName = Sidebar.ussClassName + "__item";

        /// <summary>
        /// The styling class of the item's leading icon.
        /// </summary>
        public const string iconUssClassName = ussClassName + "-icon";

        /// <summary>
        /// The styling class of the item's label.
        /// </summary>
        public const string labelUssClassName = ussClassName + "-label";

        /// <summary>
        /// The styling class of the item's trailing shortcut hint.
        /// </summary>
        public const string shortcutUssClassName = ussClassName + "-shortcut";

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_Label;

        readonly LocalizedTextElement m_Shortcut;

        Pressable m_Clickable;

        string m_Value;

        /// <summary>
        /// The identity of this item within its <see cref="Sidebar"/>'s selection group.
        /// </summary>
        /// <remarks>
        /// `null` (the default) makes the item a plain action button that never changes the selection.
        /// </remarks>
        [Tooltip("The selection value of this item. Leave empty for a plain action item.")]
        [CreateProperty]
        [UxmlAttribute]
        public string value
        {
            get => m_Value;
            set
            {
                var changed = m_Value != value;
                m_Value = value;

                if (changed)
                    NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The item's text. Accepts a localization key.
        /// </summary>
        [Tooltip("The item's text.")]
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The name of the leading icon, or `null` for no icon.
        /// </summary>
        [Tooltip("The name of the leading icon.")]
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                var changed = m_Icon.iconName != value;
                m_Icon.iconName = value;
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The trailing hint, typically a keyboard shortcut. `null` hides the element entirely.
        /// </summary>
        [Tooltip("The trailing shortcut hint.")]
        [CreateProperty]
        [UxmlAttribute]
        public string shortcut
        {
            get => m_Shortcut.text;
            set
            {
                var changed = m_Shortcut.text != value;
                m_Shortcut.text = value;
                m_Shortcut.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(value));

                if (changed)
                    NotifyPropertyChanged(in shortcutProperty);
            }
        }

        /// <summary>
        /// Whether the item paints itself as the current one.
        /// </summary>
        /// <remarks>
        /// For an item with a <see cref="value"/> the enclosing <see cref="Sidebar"/> owns this and
        /// overwrites it on every selection refresh. For an item without a value it is yours to set.
        /// </remarks>
        [Tooltip("Whether the item paints itself as the current one.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool active
        {
            get => ClassListContains(Styles.activeUssClassName);
            set
            {
                var changed = active != value;
                EnableInClassList(Styles.activeUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in activeProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this SidebarItem.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = m_Clickable != value;
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable != null)
                    this.AddManipulator(m_Clickable);

                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// Raised when the item is activated, by click or by submit key.
        /// </summary>
        public event Action clicked
        {
            add => m_Clickable.clicked += value;
            remove => m_Clickable.clicked -= value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SidebarItem()
        {
            AddToClassList(ussClassName);
            focusable = true;
            tabIndex = 0;
            pickingMode = PickingMode.Position;

            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);

            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            m_Shortcut = new LocalizedTextElement { name = shortcutUssClassName, pickingMode = PickingMode.Ignore };
            m_Shortcut.AddToClassList(shortcutUssClassName);
            hierarchy.Add(m_Shortcut);

            clickable = new Pressable(OnClicked);

            value = null;
            label = null;
            icon = null;
            shortcut = null;
            active = false;
        }

        void OnClicked()
        {
            GetFirstAncestorOfType<Sidebar>()?.OnItemActivated(this);
        }

        internal void SyncActiveFromSelection(string selectedId)
        {
            if (m_Value == null)
                return;

            active = m_Value == selectedId;
        }
    }
}
