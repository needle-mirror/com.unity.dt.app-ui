using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A set of mutually exclusive <see cref="MenuItem"/> rows inside a <see cref="Menu"/>.
    /// </summary>
    /// <remarks>
    /// This is the UI Toolkit counterpart of the web registry's <c>DropdownMenuRadioGroup</c> /
    /// <c>ContextMenuRadioGroup</c>. It reuses the existing menu machinery rather than introducing a
    /// second kind of row: every <see cref="MenuItem"/> it contains is turned into a
    /// <see cref="MenuItem.selectable"/> row with a <see cref="MenuItemIndicator.Radio"/> bullet, and
    /// the group guarantees exactly one of them is selected at a time.
    ///
    /// The group's <see cref="value"/> is the <see cref="MenuItem.key"/> of the selected row, which
    /// mirrors <see cref="RadioGroup"/> and keeps the selection stable when rows are rebuilt or
    /// reordered. A row with no key is left alone.
    ///
    /// Unlike a checkbox row, an already-selected radio row cannot be toggled off: clicking it again
    /// leaves the selection where it is, so the group is never empty once a value has been set.
    /// </remarks>
    /// <example>
    /// <para>A density picker inside a menu.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:Menu>
    ///     <ui:MenuSection title="Density">
    ///         <ui:MenuRadioGroup value="normal">
    ///             <ui:MenuItem key="compact" label="Compact" />
    ///             <ui:MenuItem key="normal" label="Normal" />
    ///             <ui:MenuItem key="comfort" label="Comfort" />
    ///         </ui:MenuRadioGroup>
    ///     </ui:MenuSection>
    /// </ui:Menu>
    /// ]]></code>
    /// <para>Reacting to the selection in C#.</para>
    /// <code lang="csharp"><![CDATA[
    /// var group = new MenuRadioGroup { value = "normal" };
    /// group.Add(new MenuItem { key = "compact", label = "Compact" });
    /// group.Add(new MenuItem { key = "normal", label = "Normal" });
    /// group.Add(new MenuItem { key = "comfort", label = "Comfort" });
    /// group.RegisterValueChangedCallback(evt => SetDensity(evt.newValue));
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("popups", id = "menu-radio-group", displayName = "Menu Radio Group")]
    public partial class MenuRadioGroup : BaseVisualElement, INotifyValueChanged<string>
    {
        internal static readonly BindingId valueProperty = new BindingId(nameof(value));

        /// <summary>
        /// The MenuRadioGroup main styling class.
        /// </summary>
        public const string ussClassName = "appui-menuradiogroup";

        string m_Value;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuRadioGroup()
        {
            AddToClassList(ussClassName);

            pickingMode = PickingMode.Ignore;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<ChangeEvent<bool>>(OnItemChosen);
        }

        /// <summary>
        /// The MenuRadioGroup content container. Rows are added directly to the group.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The <see cref="MenuItem.key"/> of the currently selected row.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string value
        {
            get => m_Value;
            set
            {
                if (value == m_Value)
                    return;
                using var evt = ChangeEvent<string>.GetPooled(m_Value, value);
                evt.target = this;
                SetValueWithoutNotify(value);
                SendEvent(evt);

                NotifyPropertyChanged(in valueProperty);
            }
        }

        /// <summary>
        /// Set the selected key without raising a <see cref="ChangeEvent{T}"/>.
        /// </summary>
        /// <param name="newValue"> The key of the row to select. </param>
        public void SetValueWithoutNotify(string newValue)
        {
            m_Value = newValue;
            RefreshItems();
        }

        /// <summary>
        /// The rows of this group.
        /// </summary>
        /// <returns> The rows, in document order. </returns>
        public IEnumerable<MenuItem> GetMenuItems()
        {
            return this.Query<MenuItem>().ToList();
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            RefreshItems();
        }

        void RefreshItems()
        {
            foreach (var item in this.Query<MenuItem>().ToList())
            {
                item.indicator = MenuItemIndicator.Radio;
                item.selectable = true;
                item.SetValueWithoutNotify(!string.IsNullOrEmpty(item.key) && item.key == m_Value);
            }
        }

        void OnItemChosen(ChangeEvent<bool> evt)
        {
            if (evt.target is not MenuItem item || string.IsNullOrEmpty(item.key))
                return;

            evt.StopPropagation();

            if (evt.newValue)
            {
                value = item.key;
            }
            else if (item.key == m_Value)
            {
                // Radios are not toggles: the active row stays active.
                item.SetValueWithoutNotify(true);
            }
        }
    }
}
