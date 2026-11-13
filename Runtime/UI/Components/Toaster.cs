using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// One entry in the <see cref="ToastManager"/> stack.
    /// </summary>
    /// <remarks>
    /// An entry is a handle, not a view. It is returned by <see cref="ToastManager.Add(string,NotificationStyle,string)"/>
    /// so the caller can update or dismiss the toast later without holding on to a visual element.
    /// </remarks>
    public sealed class ToastEntry
    {
        internal ToastEntry(int id, string text, NotificationStyle style, string icon)
        {
            this.id = id;
            this.text = text;
            this.style = style;
            this.icon = icon;
        }

        /// <summary>
        /// The unique identifier of this entry, stable for its whole lifetime.
        /// </summary>
        public int id { get; }

        /// <summary>
        /// The message displayed by the toast.
        /// </summary>
        public string text { get; private set; }

        /// <summary>
        /// The semantic style of the toast. See <see cref="NotificationStyle"/>.
        /// </summary>
        public NotificationStyle style { get; private set; }

        /// <summary>
        /// The name of the leading icon, or `null` for no icon.
        /// </summary>
        public string icon { get; private set; }

        /// <summary>
        /// Update this entry in place and refresh every mounted <see cref="Toaster"/>. The entry
        /// keeps its position in the stack, so a long-running task can turn its own toast from
        /// "Uploading…" into "Upload complete" without it jumping to the top.
        /// </summary>
        /// <param name="text">The new message.</param>
        /// <param name="style">The new semantic style.</param>
        /// <param name="icon">The new leading icon name.</param>
        public void Update(string text = null, NotificationStyle? style = null, string icon = null)
        {
            if (text != null)
                this.text = text;
            if (style.HasValue)
                this.style = style.Value;
            if (icon != null)
                this.icon = icon;
            ToastManager.NotifyChanged();
        }

        /// <summary>
        /// Remove this entry from the stack.
        /// </summary>
        public void Dismiss() => ToastManager.Dismiss(this);
    }

    /// <summary>
    /// The global toast stack: a capped, newest-first list of <see cref="ToastEntry"/> that every
    /// mounted <see cref="Toaster"/> renders.
    /// </summary>
    /// <remarks>
    /// This is the stacking counterpart of <see cref="Toast"/>. A <see cref="Toast"/> is a single
    /// <see cref="PopupNotification{T}"/> shown one at a time by the notification manager;
    /// <see cref="ToastManager"/> instead keeps up to <see cref="limit"/> messages visible at once
    /// so several results can be reported without one replacing the next.
    ///
    /// The store is global and view-independent, which is the point: any code — including code with
    /// no <see cref="VisualElement"/> at hand — can post a toast, and whichever
    /// <see cref="Toaster"/> hosts are mounted display it.
    /// </remarks>
    /// <example>
    /// <code lang="csharp"><![CDATA[
    /// // Mount one host, usually next to the app root.
    /// panel.Add(new Toaster { anchor = PopupNotificationPlacement.BottomRight, limit = 3 });
    ///
    /// // Then post from anywhere.
    /// ToastManager.Add("Scene saved", NotificationStyle.Positive, "check");
    ///
    /// // Or keep the handle and update it in place.
    /// var entry = ToastManager.Add("Uploading…");
    /// entry.Update("Upload complete", NotificationStyle.Positive);
    /// ]]></code>
    /// </example>
    public static class ToastManager
    {
        /// <summary>
        /// The default stack depth.
        /// </summary>
        public const int defaultLimit = 3;

        static readonly List<ToastEntry> s_Toasts = new List<ToastEntry>();

        static int s_Limit = defaultLimit;

        static int s_NextId;

        /// <summary>
        /// Raised whenever the stack changes: an entry added, updated, dismissed or cleared.
        /// </summary>
        public static event Action changed;

        /// <summary>
        /// How many toasts stack at once. Clamped to at least 1. Lowering it trims the stack
        /// immediately, dropping the oldest entries.
        /// </summary>
        public static int limit
        {
            get => s_Limit;
            set
            {
                var newValue = Mathf.Max(1, value);
                if (s_Limit == newValue)
                    return;
                s_Limit = newValue;
                if (Trim())
                    NotifyChanged();
            }
        }

        /// <summary>
        /// The live stack, newest first. Never null.
        /// </summary>
        public static IReadOnlyList<ToastEntry> toasts => s_Toasts;

        /// <summary>
        /// Post a new toast at the top of the stack.
        /// </summary>
        /// <param name="text">The raw message, or a Localization dictionary key.</param>
        /// <param name="style">The semantic style of the toast.</param>
        /// <param name="icon">An optional leading icon name.</param>
        /// <returns>The entry, so it can be updated or dismissed later.</returns>
        public static ToastEntry Add(
            string text,
            NotificationStyle style = NotificationStyle.Default,
            string icon = null)
        {
            var entry = new ToastEntry(s_NextId++, text, style, icon);
            s_Toasts.Insert(0, entry);
            Trim();
            NotifyChanged();
            return entry;
        }

        /// <summary>
        /// Remove an entry from the stack. Unknown or already-removed entries are ignored.
        /// </summary>
        /// <param name="entry">The entry to remove.</param>
        public static void Dismiss(ToastEntry entry)
        {
            if (entry == null || !s_Toasts.Remove(entry))
                return;
            NotifyChanged();
        }

        /// <summary>
        /// Remove the entry with the given <see cref="ToastEntry.id"/>. Unknown ids are ignored.
        /// </summary>
        /// <param name="id">The identifier of the entry to remove.</param>
        public static void Dismiss(int id)
        {
            var index = s_Toasts.FindIndex(t => t.id == id);
            if (index < 0)
                return;
            s_Toasts.RemoveAt(index);
            NotifyChanged();
        }

        /// <summary>
        /// Remove every toast immediately. Gives imperative flows — and tests sharing this global
        /// store — a way to start from an empty stack.
        /// </summary>
        public static void Clear()
        {
            if (s_Toasts.Count == 0)
                return;
            s_Toasts.Clear();
            NotifyChanged();
        }

        internal static void NotifyChanged() => changed?.Invoke();

        static bool Trim()
        {
            var trimmed = false;
            while (s_Toasts.Count > s_Limit)
            {
                s_Toasts.RemoveAt(s_Toasts.Count - 1);
                trimmed = true;
            }
            return trimmed;
        }
    }

    /// <summary>
    /// The viewport that renders the <see cref="ToastManager"/> stack, anchored to a corner or an
    /// edge of its parent.
    /// </summary>
    /// <remarks>
    /// Mount one <see cref="Toaster"/> per UI panel that should display toasts, then post from
    /// anywhere with <see cref="ToastManager.Add(string,NotificationStyle,string)"/>. The host only
    /// renders: it owns no queue of its own, so several hosts stay in sync and a host that is
    /// removed from the hierarchy stops tracking the stack.
    ///
    /// Toasts are laid out newest first, growing away from the anchored edge. Each one is the same
    /// visual element a <see cref="Toast"/> popup uses, so the two channels look identical.
    /// </remarks>
    /// <example>
    /// <para>Mounting a host in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// panel.Add(new Toaster { anchor = PopupNotificationPlacement.TopEnd, limit = 5 });
    /// ]]></code>
    /// <para>Declaring one in UXML.</para>
    /// <code lang="xml"><![CDATA[
    /// <UXML>
    /// <appui:Toaster anchor="BottomRight" limit="3" />
    /// </UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups")]
    public partial class Toaster : BaseVisualElement
    {
        internal static readonly BindingId anchorProperty = nameof(anchor);

        internal static readonly BindingId limitProperty = nameof(limit);

        /// <summary>
        /// The main styling class of the Toaster.
        /// </summary>
        public const string ussClassName = "appui-toaster";

        /// <summary>
        /// The prefix of the anchor styling classes, for instance <c>appui-toaster--bottomright</c>.
        /// </summary>
        public const string anchorUssClassName = ussClassName + "--";

        PopupNotificationPlacement m_Anchor = PopupNotificationPlacement.BottomRight;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Toaster()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Ignore;
            AddToClassList(MemoryUtils.Concatenate(anchorUssClassName, m_Anchor.ToLowerCase()));

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
        }

        /// <summary>
        /// Which corner or edge of the parent the toast stack anchors to. Defaults to
        /// <see cref="PopupNotificationPlacement.BottomRight"/>.
        /// </summary>
        [Tooltip("Which corner or edge of the parent the toast stack anchors to.")]
        [CreateProperty]
        [UxmlAttribute]
        [Header("Toaster")]
        public PopupNotificationPlacement anchor
        {
            get => m_Anchor;
            set
            {
                var changed = m_Anchor != value;
                RemoveFromClassList(MemoryUtils.Concatenate(anchorUssClassName, m_Anchor.ToLowerCase()));
                m_Anchor = value;
                AddToClassList(MemoryUtils.Concatenate(anchorUssClassName, m_Anchor.ToLowerCase()));
                if (changed)
                    NotifyPropertyChanged(in anchorProperty);
            }
        }

        /// <summary>
        /// How many toasts stack at once. This is a view over the global
        /// <see cref="ToastManager.limit"/>, so setting it here changes the depth for every host.
        /// </summary>
        [Tooltip("How many toasts stack at once.")]
        [CreateProperty]
        [UxmlAttribute]
        public int limit
        {
            get => ToastManager.limit;
            set
            {
                var changed = ToastManager.limit != Mathf.Max(1, value);
                ToastManager.limit = value;
                if (changed)
                    NotifyPropertyChanged(in limitProperty);
            }
        }

        /// <summary>
        /// The class a toast carries while it plays its exit transition. It is no longer part of
        /// the stack, and is removed from the hierarchy once the transition ends.
        /// </summary>
        public const string exitingUssClassName = ussClassName + "__toast--exiting";

        // Same enter/exit transition classes as a Toast popup (Toast.uss), so both channels animate alike.
        const string k_FadeUssClassName = "appui-popup-notification--fade";

        // Removal normally happens on TransitionEndEvent of the exit. This fallback only
        // covers the case where no transition runs at all (no stylesheet, zero duration).
        const long k_ExitFallbackMs = 1000;

        readonly Dictionary<int, ToastVisualElement> m_Views = new Dictionary<int, ToastVisualElement>();

        void OnAttachToPanel(AttachToPanelEvent evt)
        {
            ToastManager.changed += Refresh;
            Refresh();
        }

        void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            ToastManager.changed -= Refresh;
        }

        /// <summary>
        /// Diff the mounted toasts against the stack by entry id: kept toasts update in place,
        /// new ones fade in, removed ones fade out and then leave the hierarchy.
        /// </summary>
        void Refresh()
        {
            var entries = ToastManager.toasts;

            // Exit the views whose entry left the stack.
            List<int> gone = null;
            foreach (var pair in m_Views)
            {
                var stillThere = false;
                for (var i = 0; i < entries.Count; i++)
                {
                    if (entries[i].id == pair.Key)
                    {
                        stillThere = true;
                        break;
                    }
                }
                if (!stillThere)
                    (gone ??= new List<int>()).Add(pair.Key);
            }
            if (gone != null)
            {
                foreach (var id in gone)
                {
                    var view = m_Views[id];
                    m_Views.Remove(id);
                    BeginExit(view);
                }
            }

            // Create or update the live views, keeping them in stack order.
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!m_Views.TryGetValue(entry.id, out var view))
                {
                    view = new ToastVisualElement();
                    view.AddToClassList(k_FadeUssClassName);
                    view.RegisterCallback<GeometryChangedEvent>(OnEnteringGeometryChanged);
                    m_Views.Add(entry.id, view);
                    Insert(IndexOfNextLiveView(entries, i), view);
                }
                view.text = entry.text;
                view.notificationStyle = entry.style;
                view.icon = string.IsNullOrEmpty(entry.icon) ? null : entry.icon;
                // USS has no :first-child/:last-child, so the edges of the live stack are marked
                // here. Exiting toasts are not part of it.
                view.EnableInClassList(Styles.firstChildUssClassName, i == 0);
                view.EnableInClassList(Styles.lastChildUssClassName, i == entries.Count - 1);
            }
        }

        // Hierarchy index to insert the entry at index i: right before the next older live toast,
        // else right after the next newer one, else at the anchored edge (index 0). Exiting toasts
        // still in the hierarchy are never used as anchors, so a replacement never lands behind one.
        int IndexOfNextLiveView(IReadOnlyList<ToastEntry> entries, int i)
        {
            for (var j = i + 1; j < entries.Count; j++)
            {
                if (m_Views.TryGetValue(entries[j].id, out var next) && next.parent == this)
                    return IndexOf(next);
            }
            for (var j = i - 1; j >= 0; j--)
            {
                if (m_Views.TryGetValue(entries[j].id, out var prev) && prev.parent == this)
                    return IndexOf(prev) + 1;
            }
            return 0;
        }

        static void OnEnteringGeometryChanged(GeometryChangedEvent evt)
        {
            // The first layout has resolved the hidden state, so opening now plays the transition.
            var view = (VisualElement)evt.target;
            view.UnregisterCallback<GeometryChangedEvent>(OnEnteringGeometryChanged);
            if (!view.ClassListContains(exitingUssClassName))
                view.AddToClassList(Styles.openUssClassName);
        }

        static void BeginExit(ToastVisualElement view)
        {
            view.UnregisterCallback<GeometryChangedEvent>(OnEnteringGeometryChanged);
            view.AddToClassList(exitingUssClassName);
            // Picking mode is not inherited, so the toast's inner elements must stop picking too,
            // or the fading toast keeps swallowing clicks meant for what lies underneath.
            view.Query<VisualElement>().ForEach(e => e.pickingMode = PickingMode.Ignore);
            view.RemoveFromClassList(Styles.openUssClassName);
            view.RemoveFromClassList(Styles.firstChildUssClassName);
            view.RemoveFromClassList(Styles.lastChildUssClassName);
            if (view.panel == null)
            {
                view.RemoveFromHierarchy();
                return;
            }
            // Only the end of the exit transition removes the toast. Removing is-open while the enter
            // transition still runs cancels that transition (TransitionCancelEvent) and starts the
            // exit one, so cancellations are deliberately ignored here. An exiting toast is never
            // reopened, so the exit transition itself is not cancelled; the fallback covers the rest.
            view.RegisterCallback<TransitionEndEvent>(OnExitTransitionDone);
            view.schedule.Execute(() => FinishExit(view)).StartingIn(k_ExitFallbackMs);
        }

        static void OnExitTransitionDone(EventBase evt) => FinishExit((VisualElement)evt.currentTarget);

        static void FinishExit(VisualElement view)
        {
            view.UnregisterCallback<TransitionEndEvent>(OnExitTransitionDone);
            view.RemoveFromHierarchy();
        }
    }
}
