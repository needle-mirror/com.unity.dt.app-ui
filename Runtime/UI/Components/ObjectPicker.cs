using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One candidate row in an <see cref="ObjectPicker"/>.
    /// </summary>
    public class ObjectPickerItem
    {
        /// <summary>
        /// Stable identifier. This is what <see cref="ObjectPicker.value"/> holds.
        /// </summary>
        /// <remarks>
        /// The empty string is reserved: it is the cleared reference, shown as the
        /// <see cref="ObjectPicker.noneLabel"/> row.
        /// </remarks>
        public string id;

        /// <summary>
        /// The name shown in the row and in the preview.
        /// </summary>
        public string label;

        /// <summary>
        /// Leading glyph, used when <see cref="thumbnail"/> is null.
        /// </summary>
        public string iconName;

        /// <summary>
        /// Cover image. Fills the row and preview thumbnail when set, in preference to
        /// <see cref="iconName"/>.
        /// </summary>
        public Texture2D thumbnail;

        /// <summary>
        /// Preview metadata — the asset's type name.
        /// </summary>
        public string type;

        /// <summary>
        /// Preview metadata — the asset's path.
        /// </summary>
        public string path;

        /// <summary>
        /// Which tab this item belongs to, matching an <see cref="ObjectPickerTab.value"/>.
        /// Items with no tab show under every tab.
        /// </summary>
        public string tab;
    }

    /// <summary>
    /// One tab in an <see cref="ObjectPicker"/> toolbar.
    /// </summary>
    public class ObjectPickerTab
    {
        /// <summary>
        /// The tab's identifier, matched against <see cref="ObjectPickerItem.tab"/>.
        /// </summary>
        public string value;

        /// <summary>
        /// The tab's visible label.
        /// </summary>
        public string label;

        /// <summary>
        /// Creates a tab.
        /// </summary>
        /// <param name="value"> The tab identifier. </param>
        /// <param name="label"> The visible label. </param>
        public ObjectPickerTab(string value, string label)
        {
            this.value = value;
            this.label = label;
        }
    }

    /// <summary>
    /// The layout the <see cref="ObjectPickerPreview"/> derives from its own height.
    /// </summary>
    public enum ObjectPickerPreviewContext
    {
        /// <summary>
        /// A single-line strip: small thumbnail, name, type, path.
        /// </summary>
        Compact,

        /// <summary>
        /// Thumbnail on the leading edge, metadata beside it.
        /// </summary>
        Normal,

        /// <summary>
        /// A large centred thumbnail with the metadata stacked beneath.
        /// </summary>
        Large
    }

    /// <summary>
    /// The resizable preview pane of an <see cref="ObjectPicker"/>.
    /// </summary>
    /// <remarks>
    /// It measures its own height and switches between three layouts, exposed as
    /// <see cref="context"/> and reflected as a <c>--context-*</c> styling class. Exported so a
    /// custom picker can host it; <see cref="ObjectPicker"/> builds one for you.
    /// </remarks>
    [UxmlElement]
    public partial class ObjectPickerPreview : BaseVisualElement
    {
        internal static readonly BindingId contextProperty = nameof(context);

        /// <summary>
        /// Height, in pixels, below which the preview collapses to the one-line strip.
        /// </summary>
        /// <remarks>
        /// A layout breakpoint, not a design dimension — it is the height at which the
        /// thumb-beside-metadata layout stops fitting. It therefore lives in C# rather than
        /// being a USS token.
        /// </remarks>
        const float k_CompactMaxHeight = 72f;

        /// <summary>
        /// Height, in pixels, at or above which the preview goes to the large centred layout.
        /// </summary>
        const float k_LargeMinHeight = 200f;

        /// <summary>
        /// The ObjectPickerPreview main styling class.
        /// </summary>
        public const string ussClassName = "appui-objectpickerpreview";

        /// <summary>
        /// The ObjectPickerPreview thumbnail styling class.
        /// </summary>
        public const string thumbnailUssClassName = ussClassName + "__thumbnail";

        /// <summary>
        /// The ObjectPickerPreview thumbnail icon styling class.
        /// </summary>
        public const string thumbnailIconUssClassName = ussClassName + "__thumbnail-icon";

        /// <summary>
        /// The ObjectPickerPreview metadata container styling class.
        /// </summary>
        public const string metaUssClassName = ussClassName + "__meta";

        /// <summary>
        /// The ObjectPickerPreview name styling class.
        /// </summary>
        public const string nameUssClassName = ussClassName + "__name";

        /// <summary>
        /// The ObjectPickerPreview type styling class.
        /// </summary>
        public const string typeUssClassName = ussClassName + "__type";

        /// <summary>
        /// The ObjectPickerPreview path styling class.
        /// </summary>
        public const string pathUssClassName = ussClassName + "__path";

        /// <summary>
        /// The ObjectPickerPreview empty-state styling class.
        /// </summary>
        public const string emptyUssClassName = ussClassName + "__empty";

        /// <summary>
        /// The ObjectPickerPreview context styling class.
        /// </summary>
        [EnumName("GetContextUssClassName", typeof(ObjectPickerPreviewContext))]
        public const string contextUssClassName = ussClassName + "--context-";

        readonly VisualElement m_Thumbnail;

        readonly Icon m_ThumbnailIcon;

        readonly VisualElement m_Meta;

        readonly LocalizedTextElement m_Name;

        readonly LocalizedTextElement m_Type;

        readonly LocalizedTextElement m_Path;

        readonly LocalizedTextElement m_Empty;

        ObjectPickerPreviewContext m_Context;

        ObjectPickerItem m_Item;

        Func<ObjectPickerItem, ObjectPickerPreviewContext, VisualElement> m_RenderPreview;

        VisualElement m_CustomBody;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ObjectPickerPreview()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;

            m_ThumbnailIcon = new Icon { primary = true, pickingMode = PickingMode.Ignore };
            m_ThumbnailIcon.AddToClassList(thumbnailIconUssClassName);

            m_Thumbnail = new VisualElement { pickingMode = PickingMode.Ignore };
            m_Thumbnail.AddToClassList(thumbnailUssClassName);
            m_Thumbnail.Add(m_ThumbnailIcon);

            m_Name = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Name.AddToClassList(nameUssClassName);
            m_Type = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Type.AddToClassList(typeUssClassName);
            m_Path = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Path.AddToClassList(pathUssClassName);

            m_Meta = new VisualElement { pickingMode = PickingMode.Ignore };
            m_Meta.AddToClassList(metaUssClassName);
            m_Meta.Add(m_Name);
            m_Meta.Add(m_Type);
            m_Meta.Add(m_Path);

            m_Empty = new LocalizedTextElement("No selection") { pickingMode = PickingMode.Ignore };
            m_Empty.AddToClassList(emptyUssClassName);

            hierarchy.Add(m_Thumbnail);
            hierarchy.Add(m_Meta);
            hierarchy.Add(m_Empty);

            // The class must be on the element from the start; the geometry callback only fires
            // once the pane has a size, and until then an unclassed preview has no layout at all.
            m_Context = ObjectPickerPreviewContext.Normal;
            AddToClassList(GetContextUssClassName(m_Context));

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            Refresh();
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// The layout the preview currently uses, derived from its height.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public ObjectPickerPreviewContext context
        {
            get => m_Context;
            private set
            {
                if (m_Context == value)
                    return;
                RemoveFromClassList(GetContextUssClassName(m_Context));
                m_Context = value;
                AddToClassList(GetContextUssClassName(m_Context));
                Refresh();
                NotifyPropertyChanged(in contextProperty);
            }
        }

        /// <summary>
        /// The item being previewed, or null for the empty state.
        /// </summary>
        public ObjectPickerItem item
        {
            get => m_Item;
            set
            {
                m_Item = value;
                Refresh();
            }
        }

        /// <summary>
        /// Replaces the preview body. Receives the item and the live layout context, and returns
        /// the element to show. Set to null to go back to the built-in layouts.
        /// </summary>
        public Func<ObjectPickerItem, ObjectPickerPreviewContext, VisualElement> renderPreview
        {
            get => m_RenderPreview;
            set
            {
                m_RenderPreview = value;
                Refresh();
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            var height = evt.newRect.height;
            context = height < k_CompactMaxHeight
                ? ObjectPickerPreviewContext.Compact
                : height < k_LargeMinHeight
                    ? ObjectPickerPreviewContext.Normal
                    : ObjectPickerPreviewContext.Large;
        }

        void Refresh()
        {
            if (m_CustomBody != null)
            {
                m_CustomBody.RemoveFromHierarchy();
                m_CustomBody = null;
            }

            if (m_RenderPreview != null)
            {
                m_Thumbnail.EnableInClassList(Styles.hiddenUssClassName, true);
                m_Meta.EnableInClassList(Styles.hiddenUssClassName, true);
                m_Empty.EnableInClassList(Styles.hiddenUssClassName, true);
                m_CustomBody = m_RenderPreview(m_Item, m_Context);
                if (m_CustomBody != null)
                    hierarchy.Add(m_CustomBody);
                return;
            }

            var empty = m_Item == null;
            m_Empty.EnableInClassList(Styles.hiddenUssClassName, !empty);
            m_Thumbnail.EnableInClassList(Styles.hiddenUssClassName, empty);
            m_Meta.EnableInClassList(Styles.hiddenUssClassName, empty);
            if (empty)
                return;

            if (m_Item.thumbnail)
            {
                m_Thumbnail.style.backgroundImage = Background.FromTexture2D(m_Item.thumbnail);
                m_ThumbnailIcon.EnableInClassList(Styles.hiddenUssClassName, true);
            }
            else
            {
                m_Thumbnail.style.backgroundImage = StyleKeyword.Null;
                m_ThumbnailIcon.iconName = m_Item.iconName;
                m_ThumbnailIcon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Item.iconName));
            }

            m_Name.text = m_Item.label;
            m_Type.text = m_Item.type;
            m_Path.text = m_Item.path;
            m_Type.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Item.type));
            m_Path.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Item.path));
        }
    }

    /// <summary>
    /// The size step used for the thumbnails in an <see cref="ObjectPicker"/>'s list.
    /// </summary>
    public enum ObjectPickerThumbSize
    {
        /// <summary>
        /// Smallest thumbnails — the densest list.
        /// </summary>
        Sm,

        /// <summary>
        /// The default thumbnail size.
        /// </summary>
        Md,

        /// <summary>
        /// Largest thumbnails.
        /// </summary>
        Lg
    }

    /// <summary>
    /// A rich object/asset picker: search, tabs, a thumbnail-size slider and a custom toolbar
    /// slot above a scrollable list, over a resizable preview that adapts to its own height.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The picker is frameless by design — no border and no radius of its own — so the host owns
    /// the frame and there is no clipped double border at a rounded corner. Compose it inside a
    /// <see cref="Popover"/>, a <see cref="Dialog"/> or a <see cref="FloatingWindow"/>, exactly as
    /// <see cref="ColorPicker"/> is composed by <see cref="ColorField"/>. It is the intended
    /// implementation behind <see cref="ICustomAssetTargetFieldPicker"/>.
    /// </para>
    ///
    /// <para>
    /// The value is an item <see cref="ObjectPickerItem.id"/>. The empty string is the cleared
    /// reference and selects the <see cref="noneLabel"/> row.
    /// </para>
    ///
    /// <para>Example</para>
    /// <code>
    /// var picker = new ObjectPicker
    /// {
    ///     title = "Select Material",
    ///     items = new List&lt;ObjectPickerItem&gt;
    ///     {
    ///         new ObjectPickerItem { id = "bronze", label = "Bronze", type = "Material", tab = "assets" },
    ///     },
    /// };
    /// picker.RegisterValueChangedCallback(evt =&gt; Debug.Log(evt.newValue));
    /// </code>
    ///
    /// <para>UXML</para>
    /// <code>
    /// &lt;appui:ObjectPicker title="Select Material" show-size-slider="true" show-none="true" /&gt;
    /// </code>
    /// </remarks>
    [UxmlElement]
    public partial class ObjectPicker : BaseVisualElement, INotifyValueChanged<string>
    {
        internal static readonly BindingId valueProperty = nameof(value);

        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId showSizeSliderProperty = nameof(showSizeSlider);

        internal static readonly BindingId thumbSizeProperty = nameof(thumbSize);

        internal static readonly BindingId showNoneProperty = nameof(showNone);

        internal static readonly BindingId noneLabelProperty = nameof(noneLabel);

        internal static readonly BindingId noneIconNameProperty = nameof(noneIconName);

        internal static readonly BindingId showCloseButtonProperty = nameof(showCloseButton);

        /// <summary>
        /// The id of the cleared reference. Unity's convention, and what the none row selects.
        /// </summary>
        public const string noneId = "";

        /// <summary>
        /// The ObjectPicker main styling class.
        /// </summary>
        public const string ussClassName = "appui-objectpicker";

        /// <summary>
        /// The ObjectPicker header styling class.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The ObjectPicker title styling class.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The ObjectPicker close button styling class.
        /// </summary>
        public const string closeButtonUssClassName = ussClassName + "__close-button";

        /// <summary>
        /// The ObjectPicker search bar styling class.
        /// </summary>
        public const string searchUssClassName = ussClassName + "__search";

        /// <summary>
        /// The ObjectPicker toolbar styling class.
        /// </summary>
        public const string toolbarUssClassName = ussClassName + "__toolbar";

        /// <summary>
        /// The ObjectPicker tabs styling class.
        /// </summary>
        public const string tabsUssClassName = ussClassName + "__tabs";

        /// <summary>
        /// The ObjectPicker thumbnail-size slider styling class.
        /// </summary>
        public const string sizeSliderUssClassName = ussClassName + "__size-slider";

        /// <summary>
        /// The ObjectPicker toolbar slot styling class.
        /// </summary>
        public const string toolbarSlotUssClassName = ussClassName + "__toolbar-slot";

        /// <summary>
        /// The ObjectPicker split view styling class.
        /// </summary>
        public const string splitViewUssClassName = ussClassName + "__split-view";

        /// <summary>
        /// The ObjectPicker list styling class.
        /// </summary>
        public const string listUssClassName = ussClassName + "__list";

        /// <summary>
        /// The ObjectPicker row styling class.
        /// </summary>
        public const string rowUssClassName = ussClassName + "__row";

        /// <summary>
        /// The ObjectPicker none-row styling class.
        /// </summary>
        public const string noneRowUssClassName = rowUssClassName + "--none";

        /// <summary>
        /// The ObjectPicker row thumbnail styling class.
        /// </summary>
        public const string rowThumbnailUssClassName = rowUssClassName + "__thumbnail";

        /// <summary>
        /// The ObjectPicker row label styling class.
        /// </summary>
        public const string rowLabelUssClassName = rowUssClassName + "__label";

        /// <summary>
        /// The ObjectPicker empty-results styling class.
        /// </summary>
        public const string noResultsUssClassName = ussClassName + "__no-results";

        /// <summary>
        /// The ObjectPicker preview styling class.
        /// </summary>
        public const string previewUssClassName = ussClassName + "__preview";

        /// <summary>
        /// The ObjectPicker thumbnail-size styling class.
        /// </summary>
        [EnumName("GetThumbSizeUssClassName", typeof(ObjectPickerThumbSize))]
        public const string thumbSizeUssClassName = ussClassName + "--thumb-";

        readonly VisualElement m_Header;

        readonly LocalizedTextElement m_Title;

        readonly IconButton m_CloseButton;

        readonly SearchBar m_SearchBar;

        readonly VisualElement m_Toolbar;

        readonly Tabs m_Tabs;

        readonly SliderInt m_SizeSlider;

        readonly VisualElement m_ToolbarSlot;

        readonly SplitView m_SplitView;

        readonly ScrollView m_ListScrollView;

        readonly VisualElement m_List;

        readonly LocalizedTextElement m_NoResults;

        readonly ObjectPickerPreview m_Preview;

        readonly List<ObjectPickerItem> m_Items = new List<ObjectPickerItem>();

        readonly List<ObjectPickerTab> m_TabDefs = new List<ObjectPickerTab>();

        string m_Value = noneId;

        string m_Search = string.Empty;

        string m_NoneLabel;

        string m_NoneIconName;

        bool m_ShowNone;

        ObjectPickerThumbSize m_ThumbSize;

        // The active tab index is tracked here rather than read back from Tabs.value. Tabs polls
        // its hierarchy for statically added TabItems, so its value is not settled at the moment
        // SetTabs runs and the very first filter pass would silently see "no tab" and show
        // everything.
        int m_TabIndex = -1;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ObjectPicker()
        {
            AddToClassList(ussClassName);

            m_Title = new LocalizedTextElement { pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);

            m_CloseButton = new IconButton("x", () => closeRequested?.Invoke())
            {
                quiet = true,
                size = Size.Sm,
            };
            m_CloseButton.AddToClassList(closeButtonUssClassName);

            m_Header = new VisualElement();
            m_Header.AddToClassList(headerUssClassName);
            m_Header.Add(m_Title);
            m_Header.Add(m_CloseButton);

            m_SearchBar = new SearchBar { placeholder = "Search" };
            m_SearchBar.AddToClassList(searchUssClassName);
            m_SearchBar.RegisterValueChangingCallback(OnSearchChanging);
            m_SearchBar.RegisterValueChangedCallback(evt => SetSearch(evt.newValue));

            m_Tabs = new Tabs();
            m_Tabs.AddToClassList(tabsUssClassName);
            m_Tabs.RegisterValueChangedCallback(OnTabChanged);

            m_SizeSlider = new SliderInt
            {
                lowValue = 0,
                highValue = 2,
                step = 1,
                showInputField = false,
            };
            m_SizeSlider.AddToClassList(sizeSliderUssClassName);
            m_SizeSlider.RegisterValueChangedCallback(evt => thumbSize = (ObjectPickerThumbSize)evt.newValue);

            m_ToolbarSlot = new VisualElement();
            m_ToolbarSlot.AddToClassList(toolbarSlotUssClassName);

            m_Toolbar = new VisualElement();
            m_Toolbar.AddToClassList(toolbarUssClassName);
            m_Toolbar.Add(m_Tabs);
            m_Toolbar.Add(m_SizeSlider);
            m_Toolbar.Add(m_ToolbarSlot);

            m_List = new VisualElement();
            m_List.AddToClassList(listUssClassName);

            m_NoResults = new LocalizedTextElement("No results") { pickingMode = PickingMode.Ignore };
            m_NoResults.AddToClassList(noResultsUssClassName);

            m_ListScrollView = new ScrollView(ScrollViewMode.Vertical);
            m_ListScrollView.Add(m_List);
            m_ListScrollView.Add(m_NoResults);

            m_Preview = new ObjectPickerPreview();
            m_Preview.AddToClassList(previewUssClassName);

            m_SplitView = new SplitView(Direction.Vertical) { realtimeResize = true };
            m_SplitView.AddToClassList(splitViewUssClassName);
            m_SplitView.Add(m_ListScrollView);
            m_SplitView.Add(m_Preview);

            hierarchy.Add(m_Header);
            hierarchy.Add(m_SearchBar);
            hierarchy.Add(m_Toolbar);
            hierarchy.Add(m_SplitView);

            // Items usually arrive before the picker is hosted, and ScrollTo is a no-op until
            // then, so re-run it once there is a panel to scroll in.
            RegisterCallback<AttachToPanelEvent>(_ => ScrollSelectionIntoView());

            // The picker's own value is an item id, which is a string — and so is what several
            // of its internals report. The search field sends ChangeEvent<string> per keystroke,
            // and every LocalizedTextElement in the preview sends one whenever its text is
            // rewritten. All of those bubble, so without this guard a consumer that called
            // RegisterValueChangedCallback on the picker sees the preview's metadata and the
            // user's typing as selection changes. Immediate, because the picker is not the
            // target of those events and plain StopPropagation would still run the consumer's
            // callback on this element.
            RegisterCallback<ChangeEvent<string>>(StopForeignStringChange);
            RegisterCallback<ChangingEvent<string>>(StopForeignStringChange);

            title = null;
            showCloseButton = false;
            showSizeSlider = true;
            thumbSize = ObjectPickerThumbSize.Md;
            noneLabel = "(None)";
            noneIconName = null;
            showNone = true;
            SetTabs(new[]
            {
                new ObjectPickerTab("assets", "Assets"),
                new ObjectPickerTab("scene", "Scene"),
            });
        }

        /// <inheritdoc/>
        public override VisualElement contentContainer => null;

        /// <summary>
        /// Raised when the header's close button is pressed. The button is only shown when
        /// <see cref="showCloseButton"/> is true.
        /// </summary>
        public event Action closeRequested;

        /// <summary>
        /// Raised when the active tab changes, with the new <see cref="ObjectPickerTab.value"/>.
        /// </summary>
        public event Action<string> tabChanged;

        /// <summary>
        /// The preview pane. Exposed so a host can override its
        /// <see cref="ObjectPickerPreview.renderPreview"/>.
        /// </summary>
        public ObjectPickerPreview previewElement => m_Preview;

        /// <summary>
        /// The search bar.
        /// </summary>
        public SearchBar searchBar => m_SearchBar;

        /// <summary>
        /// The tab strip.
        /// </summary>
        public Tabs tabsElement => m_Tabs;

        /// <summary>
        /// The header slot for extra toolbar content, right-aligned beside the size slider.
        /// </summary>
        public VisualElement toolbarSlot => m_ToolbarSlot;

        /// <summary>
        /// The header title. Null or empty hides the whole header unless
        /// <see cref="showCloseButton"/> is true.
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
                RefreshHeader();
                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// Whether a close button is offered in the header, for a windowed picker.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showCloseButton
        {
            get => !m_CloseButton.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = showCloseButton != value;
                m_CloseButton.EnableInClassList(Styles.hiddenUssClassName, !value);
                RefreshHeader();
                if (changed)
                    NotifyPropertyChanged(in showCloseButtonProperty);
            }
        }

        /// <summary>
        /// Whether the thumbnail-size slider is shown.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showSizeSlider
        {
            get => !m_SizeSlider.ClassListContains(Styles.hiddenUssClassName);
            set
            {
                var changed = showSizeSlider != value;
                m_SizeSlider.EnableInClassList(Styles.hiddenUssClassName, !value);
                if (changed)
                    NotifyPropertyChanged(in showSizeSliderProperty);
            }
        }

        /// <summary>
        /// The thumbnail size step used by the list rows.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ObjectPickerThumbSize thumbSize
        {
            get => m_ThumbSize;
            set
            {
                var changed = m_ThumbSize != value;
                RemoveFromClassList(GetThumbSizeUssClassName(m_ThumbSize));
                m_ThumbSize = value;
                AddToClassList(GetThumbSizeUssClassName(m_ThumbSize));
                m_SizeSlider.SetValueWithoutNotify((int)value);
                if (changed)
                    NotifyPropertyChanged(in thumbSizeProperty);
            }
        }

        /// <summary>
        /// Whether the leading none row is offered.
        /// </summary>
        /// <remarks>
        /// True by default: a Unity-style object field must always offer "no assignment". The row
        /// pins to the top of every tab, ignores the search text, and stays selected while
        /// <see cref="value"/> is null or empty.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool showNone
        {
            get => m_ShowNone;
            set
            {
                var changed = m_ShowNone != value;
                m_ShowNone = value;
                RefreshList();
                if (changed)
                    NotifyPropertyChanged(in showNoneProperty);
            }
        }

        /// <summary>
        /// The label of the none row, and of the cleared selection in the preview.
        /// </summary>
        /// <remarks>
        /// Lets a field say what "no assignment" actually resolves to — a Text font falling back
        /// to the built-in Inter shows "Inter (Default)" rather than the generic placeholder.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string noneLabel
        {
            get => m_NoneLabel;
            set
            {
                var changed = m_NoneLabel != value;
                m_NoneLabel = value;
                RefreshList();
                if (changed)
                    NotifyPropertyChanged(in noneLabelProperty);
            }
        }

        /// <summary>
        /// Icon for the none row.
        /// </summary>
        /// <remarks>
        /// When set, the row renders like a regular item — icon plus upright, unmuted text —
        /// because the field's "none" is a real engine fallback asset rather than an absence.
        /// Leave it null for the classic italic placeholder.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public string noneIconName
        {
            get => m_NoneIconName;
            set
            {
                var changed = m_NoneIconName != value;
                m_NoneIconName = value;
                RefreshList();
                if (changed)
                    NotifyPropertyChanged(in noneIconNameProperty);
            }
        }

        /// <summary>
        /// The selected item's <see cref="ObjectPickerItem.id"/>.
        /// </summary>
        [CreateProperty]
        public string value
        {
            get => m_Value;
            set
            {
                var newValue = value ?? noneId;
                if (m_Value == newValue)
                    return;
                using var evt = ChangeEvent<string>.GetPooled(m_Value, newValue);
                evt.target = this;
                SetValueWithoutNotify(newValue);
                SendEvent(evt);
                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// The active tab's <see cref="ObjectPickerTab.value"/>, or null when there are no tabs.
        /// </summary>
        public string currentTab =>
            m_TabIndex >= 0 && m_TabIndex < m_TabDefs.Count ? m_TabDefs[m_TabIndex].value : null;

        /// <summary>
        /// The candidates offered by the picker.
        /// </summary>
        public IEnumerable<ObjectPickerItem> items => m_Items;

        /// <summary>
        /// Replaces the candidate list.
        /// </summary>
        /// <param name="newItems"> The new candidates. Null clears the list. </param>
        public void SetItems(IEnumerable<ObjectPickerItem> newItems)
        {
            m_Items.Clear();
            if (newItems != null)
                m_Items.AddRange(newItems);
            RefreshList();
        }

        /// <summary>
        /// Replaces the tab strip.
        /// </summary>
        /// <param name="newTabs"> The new tabs. Null or empty hides the strip. </param>
        public void SetTabs(IEnumerable<ObjectPickerTab> newTabs)
        {
            m_TabDefs.Clear();
            if (newTabs != null)
                m_TabDefs.AddRange(newTabs);

            m_Tabs.Clear();
            foreach (var tab in m_TabDefs)
                m_Tabs.Add(new TabItem { label = tab.label });

            m_TabIndex = m_TabDefs.Count > 0 ? 0 : -1;
            m_Tabs.EnableInClassList(Styles.hiddenUssClassName, m_TabDefs.Count == 0);
            m_Tabs.SetValueWithoutNotify(m_TabIndex);
            RefreshList();
        }

        /// <summary>
        /// Sets the value without sending a change event.
        /// </summary>
        /// <param name="newValue"> The new selected id. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue ?? noneId;
            RefreshSelection();
        }

        void StopForeignStringChange(EventBase evt)
        {
            if (evt.target != (object)this)
                evt.StopImmediatePropagation();
        }

        void OnSearchChanging(ChangingEvent<string> evt) => SetSearch(evt.newValue);

        void SetSearch(string search)
        {
            var normalized = search ?? string.Empty;
            if (m_Search == normalized)
                return;
            m_Search = normalized;
            RefreshList();
        }

        void OnTabChanged(ChangeEvent<int> evt)
        {
            m_TabIndex = evt.newValue;
            RefreshList();
            tabChanged?.Invoke(currentTab);
        }

        void RefreshHeader()
        {
            m_Header.EnableInClassList(Styles.hiddenUssClassName,
                string.IsNullOrEmpty(m_Title.text) && !showCloseButton);
        }

        void RefreshList()
        {
            m_List.Clear();

            if (m_ShowNone)
                m_List.Add(BuildRow(new ObjectPickerItem
                {
                    id = noneId,
                    label = m_NoneLabel,
                    iconName = m_NoneIconName,
                }, true));

            var query = (m_Search ?? string.Empty).Trim();
            var tab = currentTab;
            var matches = 0;

            foreach (var item in m_Items)
            {
                if (item == null)
                    continue;
                if (!string.IsNullOrEmpty(item.tab) && !string.IsNullOrEmpty(tab) && item.tab != tab)
                    continue;
                if (query.Length > 0 &&
                    (item.label == null || item.label.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0))
                    continue;

                matches++;
                m_List.Add(BuildRow(item, false));
            }

            m_NoResults.EnableInClassList(Styles.hiddenUssClassName, matches > 0 || m_ShowNone);
            RefreshSelection();
        }

        VisualElement BuildRow(ObjectPickerItem item, bool isNone)
        {
            var thumbnail = new VisualElement { pickingMode = PickingMode.Ignore };
            thumbnail.AddToClassList(rowThumbnailUssClassName);

            if (item.thumbnail)
            {
                thumbnail.style.backgroundImage = Background.FromTexture2D(item.thumbnail);
            }
            else if (!string.IsNullOrEmpty(item.iconName))
            {
                var icon = new Icon { primary = true, iconName = item.iconName, pickingMode = PickingMode.Ignore };
                thumbnail.Add(icon);
            }

            var label = new LocalizedTextElement(item.label) { pickingMode = PickingMode.Ignore };
            label.AddToClassList(rowLabelUssClassName);

            var row = new VisualElement { focusable = true, tabIndex = 0 };
            row.AddToClassList(rowUssClassName);
            // The none row is only styled as a placeholder when it has no icon. With an icon it
            // IS an asset to the user — a named engine fallback — so it reads as a regular row.
            if (isNone && string.IsNullOrEmpty(item.iconName))
                row.AddToClassList(noneRowUssClassName);
            row.userData = item;
            row.Add(thumbnail);
            row.Add(label);
            row.AddManipulator(new Pressable(() => value = item.id));
            return row;
        }

        void RefreshSelection()
        {
            ObjectPickerItem selected = null;

            foreach (var child in m_List.Children())
            {
                var item = child.userData as ObjectPickerItem;
                var isSelected = item != null && item.id == m_Value;
                child.EnableInClassList(Styles.selectedUssClassName, isSelected);
                if (isSelected)
                    selected = item;
            }

            // The none row is a synthetic item, so hand the preview the real one when the value
            // points at a candidate — otherwise the preview would only ever see list rows. Resolve
            // it from the full collection rather than the visible rows: the current search or tab
            // may have filtered the selected row out, and the value is unchanged either way, so the
            // preview must keep showing it.
            if (m_Value != noneId)
            {
                foreach (var item in m_Items)
                {
                    if (item != null && item.id == m_Value)
                    {
                        selected = item;
                        break;
                    }
                }
            }

            m_Preview.item = selected;
            ScrollSelectionIntoView();
        }

        void ScrollSelectionIntoView()
        {
            // Keep the selection visible, including the initial one: items usually arrive after
            // construction, and without this an open-on-long-list picker highlights a row far
            // off-screen, which reads as "nothing selected".
            //
            // Only once attached: ScrollView.ScrollTo dereferences the panel and throws on a
            // detached tree, and a picker is routinely populated before it is added to its host.
            if (panel == null)
                return;

            foreach (var child in m_List.Children())
            {
                if (child.ClassListContains(Styles.selectedUssClassName))
                {
                    m_ListScrollView.ScrollTo(child);
                    return;
                }
            }
        }
    }
}
