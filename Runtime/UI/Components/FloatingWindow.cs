using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Where a <see cref="FloatingWindow"/> persists its geometry.
    /// </summary>
    /// <remarks>
    /// Deliberately the same synchronous get / set / remove shape as the registry's
    /// `WindowLayoutStorage`, so a host that already routes preferences somewhere — an editor
    /// preference store, a cloud profile — can drop an adapter in. The default implementation is
    /// <see cref="PlayerPrefsWindowLayoutStorage"/>.
    /// </remarks>
    public interface IWindowLayoutStorage
    {
        /// <summary>
        /// Reads the value stored under <paramref name="key"/>, or `null` when there is none.
        /// </summary>
        /// <param name="key"> The storage key. </param>
        /// <returns> The stored value, or `null`. </returns>
        string GetItem(string key);

        /// <summary>
        /// Writes <paramref name="value"/> under <paramref name="key"/>.
        /// </summary>
        /// <param name="key"> The storage key. </param>
        /// <param name="value"> The value to store. </param>
        void SetItem(string key, string value);

        /// <summary>
        /// Removes whatever is stored under <paramref name="key"/>.
        /// </summary>
        /// <param name="key"> The storage key. </param>
        void RemoveItem(string key);
    }

    /// <summary>
    /// The default <see cref="IWindowLayoutStorage"/>, backed by <see cref="PlayerPrefs"/>.
    /// </summary>
    /// <remarks>
    /// The web component defaults to `window.localStorage`; the platform-native equivalent in a Unity
    /// runtime is PlayerPrefs, which is synchronous, available in the Editor and in a player, and
    /// intended for exactly this kind of small per-user preference.
    ///
    /// `viewDataKey` was the other candidate and is deliberately not used: UI Toolkit view data is
    /// keyed by the element's position in a serialized hierarchy rather than by a stable name the host
    /// chooses, and it is not injectable, so it could not honour the registry's `storageKey` and
    /// `storage` props.
    /// </remarks>
    public class PlayerPrefsWindowLayoutStorage : IWindowLayoutStorage
    {
        /// <summary>
        /// The shared instance used by every <see cref="FloatingWindow"/> that does not set its own
        /// <see cref="FloatingWindow.storage"/>.
        /// </summary>
        public static readonly PlayerPrefsWindowLayoutStorage instance = new PlayerPrefsWindowLayoutStorage();

        /// <inheritdoc />
        public string GetItem(string key) => PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : null;

        /// <inheritdoc />
        public void SetItem(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        /// <inheritdoc />
        public void RemoveItem(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// The serialized geometry of a <see cref="FloatingWindow"/>.
    /// </summary>
    [Serializable]
    public struct WindowLayout
    {
        /// <summary> The distance from the container's left edge, in pixels. </summary>
        public float x;

        /// <summary> The distance from the container's top edge, in pixels. </summary>
        public float y;

        /// <summary> The window width, in pixels. </summary>
        public float w;

        /// <summary> The window height, in pixels. </summary>
        public float h;
    }

    /// <summary>
    /// A draggable, resizable window that floats above the rest of the UI and remembers where the user
    /// left it.
    /// </summary>
    /// <remarks>
    /// The window positions itself absolutely inside its parent, so give it a parent that fills the
    /// area you want it to roam. Its title bar is the drag handle and its bottom-right corner carries a
    /// <see cref="ResizeHandle"/>; both clamp the result so the whole window stays inside the parent
    /// rather than leaving a sliver on screen.
    ///
    /// Geometry is persisted under <see cref="storageKey"/> through <see cref="storage"/>, which
    /// defaults to <see cref="PlayerPrefsWindowLayoutStorage"/> — the platform-native counterpart of the
    /// registry's `window.localStorage` default. Restoring re-clamps against the current container, so a
    /// layout saved on a larger screen still comes back fully visible.
    ///
    /// <see cref="autoSize"/> lets the content size the window and implies it is not resizable, matching
    /// the registry.
    ///
    /// **Not ported:** the registry's Tab focus trap and its shared window stack (`useWindowStack`)
    /// have no UI Toolkit counterpart here. Ordering between several floating windows is whatever their
    /// order in the hierarchy gives you; bring one forward by re-adding it to its parent.
    /// </remarks>
    /// <example>
    /// <para>A tool window in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <appui:FloatingWindow title="Inspector" storage-key="inspector-window" open="true">
    ///     <appui:Text text="Anything you like goes here." />
    /// </appui:FloatingWindow>
    /// ]]></code>
    /// <para>Opened from C#, with an injected store.</para>
    /// <code lang="csharp"><![CDATA[
    /// var window = new FloatingWindow
    /// {
    ///     title = "Inspector",
    ///     storageKey = "inspector-window",
    ///     storage = myPreferencesAdapter,
    ///     defaultWidth = 360f,
    ///     defaultHeight = 480f,
    /// };
    /// window.Add(new Text { text = "Anything you like goes here." });
    /// window.openChanged += isOpen => Debug.Log($"open: {isOpen}");
    /// root.Add(window);
    /// window.open = true;
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("layouts", isNew = true)]
    public partial class FloatingWindow : BaseVisualElement
    {
        internal static readonly BindingId openProperty = nameof(open);

        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId storageKeyProperty = nameof(storageKey);

        internal static readonly BindingId resizableProperty = nameof(resizable);

        internal static readonly BindingId autoSizeProperty = nameof(autoSize);

        internal static readonly BindingId dismissableProperty = nameof(dismissable);

        internal static readonly BindingId defaultWidthProperty = nameof(defaultWidth);

        internal static readonly BindingId defaultHeightProperty = nameof(defaultHeight);

        internal static readonly BindingId minWidthProperty = nameof(minWidth);

        internal static readonly BindingId minHeightProperty = nameof(minHeight);

        /// <summary>
        /// The FloatingWindow main styling class.
        /// </summary>
        public const string ussClassName = "appui-floating-window";

        /// <summary>
        /// The styling class of the title bar, which is also the drag handle.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the title text.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the close button.
        /// </summary>
        public const string closeButtonUssClassName = ussClassName + "__close-button";

        /// <summary>
        /// The styling class of the body the content lives in.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        /// <summary>
        /// The styling class of the bottom-right resize grip.
        /// </summary>
        public const string resizeHandleUssClassName = ussClassName + "__resize-handle";

        /// <summary>
        /// The styling class applied while <see cref="autoSize"/> is on.
        /// </summary>
        public const string autoSizeUssClassName = ussClassName + "--auto-size";

        /// <summary>
        /// The default window width, in pixels. Mirrors the registry.
        /// </summary>
        public const float defaultWindowWidth = 420f;

        /// <summary>
        /// The default window height, in pixels. Mirrors the registry.
        /// </summary>
        public const float defaultWindowHeight = 600f;

        /// <summary>
        /// The default minimum window width, in pixels. Mirrors the registry.
        /// </summary>
        public const float defaultMinWidth = 320f;

        /// <summary>
        /// The default minimum window height, in pixels. Mirrors the registry.
        /// </summary>
        public const float defaultMinHeight = 240f;

        /// <summary>
        /// The margin kept between a defaulted window and the container's bottom-right corner.
        /// </summary>
        public const float defaultMargin = 24f;

        readonly VisualElement m_Header;

        readonly LocalizedTextElement m_Title;

        readonly ActionButton m_CloseButton;

        readonly VisualElement m_Body;

        readonly ResizeHandle m_ResizeHandle;

        readonly Draggable m_Draggable;

        IWindowLayoutStorage m_Storage;

        string m_StorageKey = "floating-window-layout";

        bool m_Open;

        bool m_Resizable = true;

        bool m_AutoSize;

        bool m_Dismissable = true;

        float m_DefaultWidth = defaultWindowWidth;

        float m_DefaultHeight = defaultWindowHeight;

        float m_MinWidth = defaultMinWidth;

        float m_MinHeight = defaultMinHeight;

        float m_DefaultX = float.NaN;

        float m_DefaultY = float.NaN;

        WindowLayout m_Layout;

        bool m_LayoutResolved;

        bool m_PositionIsDefaulted;

        VisualElement m_WatchedContainer;

        Vector2 m_DragStartPosition;

        /// <inheritdoc />
        public override VisualElement contentContainer => m_Body;

        /// <summary>
        /// Whether the window is currently shown.
        /// </summary>
        /// <remarks>
        /// Opening restores the persisted geometry, or falls back to the defaults when there is none.
        /// Closing only hides the window; the geometry stays saved.
        /// </remarks>
        [Tooltip("Whether the window is currently shown.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool open
        {
            get => m_Open;
            set
            {
                var changed = m_Open != value;
                m_Open = value;
                EnableInClassList(Styles.hiddenUssClassName, !value);

                if (value)
                    RestoreLayout();

                if (changed)
                {
                    openChanged?.Invoke(value);
                    NotifyPropertyChanged(in openProperty);
                }
            }
        }

        /// <summary>
        /// The window's title. Accepts a localization key.
        /// </summary>
        [Tooltip("The window's title.")]
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
        /// The key the geometry is persisted under.
        /// </summary>
        /// <remarks>
        /// Give every window in your app its own key, or two windows will fight over one saved layout.
        /// </remarks>
        [Tooltip("The key the window geometry is persisted under.")]
        [CreateProperty]
        [UxmlAttribute]
        public string storageKey
        {
            get => m_StorageKey;
            set
            {
                var changed = m_StorageKey != value;
                m_StorageKey = value;

                if (changed)
                    NotifyPropertyChanged(in storageKeyProperty);
            }
        }

        /// <summary>
        /// The persistence backend. Defaults to <see cref="PlayerPrefsWindowLayoutStorage.instance"/>.
        /// </summary>
        /// <remarks>
        /// Set this to `null` to fall back to the default. Set it to an in-memory implementation in
        /// tests, or to your own adapter to route the layout into an existing preference store.
        /// </remarks>
        public IWindowLayoutStorage storage
        {
            get => m_Storage ?? PlayerPrefsWindowLayoutStorage.instance;
            set => m_Storage = value;
        }

        /// <summary>
        /// Whether the bottom-right resize grip is shown. Ignored while <see cref="autoSize"/> is on.
        /// </summary>
        [Tooltip("Whether the bottom-right resize grip is shown.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool resizable
        {
            get => m_Resizable;
            set
            {
                var changed = m_Resizable != value;
                m_Resizable = value;
                RefreshResizeHandle();

                if (changed)
                    NotifyPropertyChanged(in resizableProperty);
            }
        }

        /// <summary>
        /// Whether the window sizes itself to its content instead of to a fixed box.
        /// </summary>
        /// <remarks>
        /// This implies not resizable, and neither the width nor the height is persisted while it is on.
        /// </remarks>
        [Tooltip("Whether the window sizes itself to its content.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool autoSize
        {
            get => m_AutoSize;
            set
            {
                var changed = m_AutoSize != value;
                m_AutoSize = value;
                EnableInClassList(autoSizeUssClassName, value);
                RefreshResizeHandle();
                ApplyLayout();

                if (changed)
                    NotifyPropertyChanged(in autoSizeProperty);
            }
        }

        /// <summary>
        /// Whether the title bar shows a close button.
        /// </summary>
        [Tooltip("Whether the title bar shows a close button.")]
        [CreateProperty]
        [UxmlAttribute]
        public bool dismissable
        {
            get => m_Dismissable;
            set
            {
                var changed = m_Dismissable != value;
                m_Dismissable = value;
                m_CloseButton.EnableInClassList(Styles.hiddenUssClassName, !value);

                if (changed)
                    NotifyPropertyChanged(in dismissableProperty);
            }
        }

        /// <summary>
        /// The width the window opens at when nothing has been persisted, in pixels.
        /// </summary>
        [Tooltip("The width the window opens at when nothing has been persisted.")]
        [CreateProperty]
        [UxmlAttribute]
        public float defaultWidth
        {
            get => m_DefaultWidth;
            set
            {
                var changed = !Mathf.Approximately(m_DefaultWidth, value);
                m_DefaultWidth = value;

                if (changed)
                    NotifyPropertyChanged(in defaultWidthProperty);
            }
        }

        /// <summary>
        /// The height the window opens at when nothing has been persisted, in pixels.
        /// </summary>
        [Tooltip("The height the window opens at when nothing has been persisted.")]
        [CreateProperty]
        [UxmlAttribute]
        public float defaultHeight
        {
            get => m_DefaultHeight;
            set
            {
                var changed = !Mathf.Approximately(m_DefaultHeight, value);
                m_DefaultHeight = value;

                if (changed)
                    NotifyPropertyChanged(in defaultHeightProperty);
            }
        }

        /// <summary>
        /// The smallest width the resize grip will produce, in pixels.
        /// </summary>
        [Tooltip("The smallest width the resize grip will produce.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minWidth
        {
            get => m_MinWidth;
            set
            {
                var changed = !Mathf.Approximately(m_MinWidth, value);
                m_MinWidth = Mathf.Max(0f, value);

                if (changed)
                    NotifyPropertyChanged(in minWidthProperty);
            }
        }

        /// <summary>
        /// The smallest height the resize grip will produce, in pixels.
        /// </summary>
        [Tooltip("The smallest height the resize grip will produce.")]
        [CreateProperty]
        [UxmlAttribute]
        public float minHeight
        {
            get => m_MinHeight;
            set
            {
                var changed = !Mathf.Approximately(m_MinHeight, value);
                m_MinHeight = Mathf.Max(0f, value);

                if (changed)
                    NotifyPropertyChanged(in minHeightProperty);
            }
        }

        /// <summary>
        /// The x the window opens at when nothing has been persisted. `NaN` (the default) places it
        /// against the container's right edge.
        /// </summary>
        public float defaultX
        {
            get => m_DefaultX;
            set => m_DefaultX = value;
        }

        /// <summary>
        /// The y the window opens at when nothing has been persisted. `NaN` (the default) places it
        /// against the container's bottom edge.
        /// </summary>
        public float defaultY
        {
            get => m_DefaultY;
            set => m_DefaultY = value;
        }

        /// <summary>
        /// The geometry the window is currently at, in pixels relative to its parent.
        /// </summary>
        public WindowLayout layoutGeometry => m_Layout;

        /// <summary>
        /// Raised when <see cref="open"/> changes.
        /// </summary>
        public event Action<bool> openChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FloatingWindow()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;

            m_Header = new VisualElement { name = headerUssClassName };
            m_Header.AddToClassList(headerUssClassName);
            hierarchy.Add(m_Header);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);
            m_Header.Add(m_Title);

            m_CloseButton = new ActionButton(() => open = false)
            {
                name = closeButtonUssClassName,
                icon = "x",
                quiet = true,
            };
            m_CloseButton.AddToClassList(closeButtonUssClassName);
            m_Header.Add(m_CloseButton);

            m_Body = new VisualElement { name = bodyUssClassName };
            m_Body.AddToClassList(bodyUssClassName);
            hierarchy.Add(m_Body);

            m_ResizeHandle = new ResizeHandle(this)
            {
                name = resizeHandleUssClassName,
                dragDirection = Draggable.DragDirection.Free,
                sizeModifier = ClampSize,
            };
            m_ResizeHandle.AddToClassList(resizeHandleUssClassName);
            m_ResizeHandle.resizeEnded += _ => CaptureAndSave();
            hierarchy.Add(m_ResizeHandle);

            // The title bar is the drag handle, which is the UI Toolkit shape of the registry's
            // `dragHandle` render prop.
            m_Draggable = new Draggable(OnDragClick, OnDrag, OnDragUp, OnDragDown)
            {
                dragDirection = Draggable.DragDirection.Free,
            };
            m_Header.AddManipulator(m_Draggable);

            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            title = null;
            storageKey = m_StorageKey;
            dismissable = true;
            resizable = true;
            autoSize = false;
            open = false;
        }

        void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            WatchContainer(hierarchy.parent);
            if (m_Open)
                RestoreLayout();
        }

        void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            WatchContainer(null);
        }

        // A fixed-size window gets no GeometryChangedEvent of its own when only the container
        // resizes, and the container's event does not bubble down, so the window would sit partly
        // off-screen until something else moved it. Watch the container directly instead.
        void WatchContainer(VisualElement container)
        {
            if (m_WatchedContainer == container)
                return;

            m_WatchedContainer?.UnregisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
            m_WatchedContainer = container;
            m_WatchedContainer?.RegisterCallback<GeometryChangedEvent>(OnContainerGeometryChanged);
        }

        void OnContainerGeometryChanged(GeometryChangedEvent evt)
        {
            ReclampToContainer();
        }

        // The container's size is not known until it has been laid out, and neither the default
        // bottom-right placement nor the on-screen clamp mean anything without it. Re-run both once,
        // and re-clamp on every later resize of the container. Only the POSITION is touched here: the
        // size is user-dragged state the ResizeHandle owns mid-drag, and re-applying it would undo the
        // drag in progress.
        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ReclampToContainer();
        }

        void ReclampToContainer()
        {
            if (!m_Open || !m_LayoutResolved)
                return;

            var container = ContainerSize();
            if (container.x <= 0f || container.y <= 0f)
                return;

            if (m_PositionIsDefaulted)
            {
                m_PositionIsDefaulted = false;
                var placed = DefaultLayout();
                m_Layout.x = placed.x;
                m_Layout.y = placed.y;
            }

            ClampPosition();

            // Always write the position back: the re-placed default can differ from what is drawn
            // even when the clamp leaves it untouched, and the drag starts from m_Layout.
            style.left = m_Layout.x;
            style.top = m_Layout.y;
        }

        void RefreshResizeHandle()
        {
            m_ResizeHandle.EnableInClassList(Styles.hiddenUssClassName, !m_Resizable || m_AutoSize);
        }

        Vector2 ClampSize(Vector2 newSize, Vector2 startSize)
        {
            var container = ContainerSize();
            var w = Mathf.Max(m_MinWidth, newSize.x);
            var h = Mathf.Max(m_MinHeight, newSize.y);
            if (container.x > 0f)
                w = Mathf.Min(w, container.x - m_Layout.x);
            if (container.y > 0f)
                h = Mathf.Min(h, container.y - m_Layout.y);
            return new Vector2(w, h);
        }

        Vector2 ContainerSize()
        {
            var parentRect = hierarchy.parent?.contentRect ?? Rect.zero;
            if (float.IsNaN(parentRect.width) || float.IsNaN(parentRect.height))
                return Vector2.zero;
            return parentRect.size;
        }

        static void OnDragClick()
        {
            // The title bar is a drag surface, not a button. A click with no movement does nothing.
        }

        void OnDragDown(Draggable draggable)
        {
            // Start from where the window is actually drawn, so the pointer keeps its offset inside
            // the header, and resync m_Layout in case it drifted from the drawn position.
            // layout.position includes the margin, while style.left/top do not: subtract it so
            // ApplyLayout() does not add the margin a second time.
            var drawn = layout.position - new Vector2(resolvedStyle.marginLeft, resolvedStyle.marginTop);
            if (!float.IsNaN(drawn.x) && !float.IsNaN(drawn.y))
            {
                m_Layout.x = drawn.x;
                m_Layout.y = drawn.y;
            }
            m_DragStartPosition = new Vector2(m_Layout.x, m_Layout.y);
        }

        void OnDrag(Draggable draggable)
        {
            var delta = draggable.position - draggable.startPosition;
            m_Layout.x = m_DragStartPosition.x + delta.x;
            m_Layout.y = m_DragStartPosition.y + delta.y;
            ClampPosition();
            ApplyLayout();
        }

        void OnDragUp(Draggable draggable)
        {
            CaptureAndSave();
        }

        void ClampPosition()
        {
            var container = ContainerSize();
            if (container.x <= 0f || container.y <= 0f)
                return;

            // Keep the WHOLE window inside the container, clamping the bottom-right by the window's own
            // size rather than leaving a sliver, symmetric with the top-left clamp.
            m_Layout.x = Mathf.Clamp(m_Layout.x, 0f, Mathf.Max(0f, container.x - m_Layout.w));
            m_Layout.y = Mathf.Clamp(m_Layout.y, 0f, Mathf.Max(0f, container.y - m_Layout.h));
        }

        void ApplyLayout()
        {
            style.left = m_Layout.x;
            style.top = m_Layout.y;

            if (m_AutoSize)
            {
                style.width = StyleKeyword.Auto;
                style.height = StyleKeyword.Auto;
            }
            else
            {
                style.width = m_Layout.w;
                style.height = m_Layout.h;
            }
        }

        void CaptureAndSave()
        {
            if (!m_AutoSize)
            {
                var w = resolvedStyle.width;
                var h = resolvedStyle.height;
                if (!float.IsNaN(w))
                    m_Layout.w = w;
                if (!float.IsNaN(h))
                    m_Layout.h = h;
            }
            SaveLayout();
        }

        /// <summary>
        /// Writes the current geometry to <see cref="storage"/> under <see cref="storageKey"/>.
        /// </summary>
        /// <remarks>
        /// This runs automatically at the end of a drag or a resize. Call it directly only if you move
        /// the window by setting its style yourself.
        /// </remarks>
        public void SaveLayout()
        {
            if (string.IsNullOrEmpty(m_StorageKey))
                return;

            try
            {
                storage.SetItem(m_StorageKey, JsonUtility.ToJson(m_Layout));
            }
            catch (Exception)
            {
                // A host store may refuse the write (locked-down preferences, quota). Losing the
                // layout must never take the window down with it.
            }
        }

        /// <summary>
        /// Restores the persisted geometry, falling back to the defaults when there is none.
        /// </summary>
        /// <remarks>
        /// The restored geometry is re-clamped against the current container size and the current
        /// <see cref="minWidth"/> / <see cref="minHeight"/>, so a layout saved on a larger screen comes
        /// back fully visible rather than off-screen.
        /// </remarks>
        public void RestoreLayout()
        {
            if (TryLoadLayout(out m_Layout))
            {
                m_PositionIsDefaulted = false;
            }
            else
            {
                m_Layout = DefaultLayout();
                // Only a defaulted position needs re-placing once the container has a size; a restored
                // one is the user's own and is merely clamped.
                m_PositionIsDefaulted = float.IsNaN(m_DefaultX) || float.IsNaN(m_DefaultY);
            }

            m_Layout.w = Mathf.Max(m_MinWidth, m_Layout.w);
            m_Layout.h = Mathf.Max(m_MinHeight, m_Layout.h);
            ClampPosition();
            ApplyLayout();
            m_LayoutResolved = true;
        }

        /// <summary>
        /// Forgets the persisted geometry and returns the window to its defaults.
        /// </summary>
        public void ResetLayout()
        {
            if (!string.IsNullOrEmpty(m_StorageKey))
            {
                try
                {
                    storage.RemoveItem(m_StorageKey);
                }
                catch (Exception)
                {
                    // See SaveLayout.
                }
            }

            m_Layout = DefaultLayout();
            m_PositionIsDefaulted = float.IsNaN(m_DefaultX) || float.IsNaN(m_DefaultY);
            ClampPosition();
            ApplyLayout();
        }

        WindowLayout DefaultLayout()
        {
            var container = ContainerSize();
            var w = Mathf.Max(m_MinWidth, m_DefaultWidth);
            var h = Mathf.Max(m_MinHeight, m_DefaultHeight);

            var x = m_DefaultX;
            var y = m_DefaultY;
            if (float.IsNaN(x))
                x = container.x > 0f ? Mathf.Max(0f, container.x - w - defaultMargin) : defaultMargin;
            if (float.IsNaN(y))
                y = container.y > 0f ? Mathf.Max(0f, container.y - h - defaultMargin) : defaultMargin;

            return new WindowLayout { x = x, y = y, w = w, h = h };
        }

        bool TryLoadLayout(out WindowLayout result)
        {
            result = default;
            if (string.IsNullOrEmpty(m_StorageKey))
                return false;

            try
            {
                var raw = storage.GetItem(m_StorageKey);
                if (string.IsNullOrEmpty(raw))
                    return false;

                var parsed = JsonUtility.FromJson<WindowLayout>(raw);
                if (float.IsNaN(parsed.x) || float.IsNaN(parsed.y) ||
                    float.IsNaN(parsed.w) || float.IsNaN(parsed.h) ||
                    parsed.w <= 0f || parsed.h <= 0f)
                    return false;

                result = parsed;
                return true;
            }
            catch (Exception)
            {
                // Corrupt or foreign payload — fall back to the defaults rather than throwing on open.
                return false;
            }
        }

        /// <summary>
        /// Whether the window has resolved a geometry yet, either restored or defaulted.
        /// </summary>
        public bool layoutResolved => m_LayoutResolved;
    }
}
