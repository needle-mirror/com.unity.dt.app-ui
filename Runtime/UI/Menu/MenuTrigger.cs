using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A wrapper to display a menu when a trigger has been activated.
    /// </summary>
    [UxmlElement]
    public partial class MenuTrigger : BaseVisualElement
    {

        internal static readonly BindingId triggerProperty = new BindingId(nameof(trigger));

        internal static readonly BindingId anchorProperty = new BindingId(nameof(anchor));

        internal static readonly BindingId menuProperty = new BindingId(nameof(menu));

        internal static readonly BindingId closeOnSelectionProperty = new BindingId(nameof(closeOnSelection));


        string m_AnchorName;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MenuTrigger()
        {
            pickingMode = PickingMode.Ignore;

            anchor = null;
            closeOnSelection = true;

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        VisualElement m_Trigger;

        /// <summary>
        /// The trigger used to determine when to display them <see cref="menu"/>.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public VisualElement trigger
        {
            get => m_Trigger;
            private set
            {
                var changed = m_Trigger != value;
                m_Trigger = value;

                if (changed)
                    NotifyPropertyChanged(in triggerProperty);
            }
        }

        VisualElement m_Anchor;

        /// <summary>
        /// The UI element used as an anchor for the menu's popover.
        /// </summary>
        [CreateProperty]
        public VisualElement anchor
        {
            get => m_Anchor;
            set
            {
                var changed = m_Anchor != value;
                m_Anchor = value;

                if (changed)
                    NotifyPropertyChanged(in anchorProperty);
            }
        }

        Menu m_Menu;

        /// <summary>
        /// The menu to display.
        /// </summary>
        [CreateProperty(ReadOnly = true)]
        public Menu menu
        {
            get => m_Menu;
            private set
            {
                var changed = m_Menu != value;
                m_Menu = value;

                if (changed)
                    NotifyPropertyChanged(in menuProperty);
            }
        }

        bool m_CloseOnSelection = true;

        /// <summary>
        /// Whether the menu should close when a selection is made.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool closeOnSelection
        {
            get => m_CloseOnSelection;
            set
            {
                var changed = m_CloseOnSelection != value;
                m_CloseOnSelection = value;

                if (changed)
                    NotifyPropertyChanged(in closeOnSelectionProperty);
            }
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            Menu childMenu = null;
            VisualElement ve = null;

            foreach (var child in Children())
            {
                if (childMenu == null && child is Menu m)
                    childMenu = m;

                if (ve == null && !(child is Menu))
                    ve = child;

                if (childMenu != null && ve != null)
                    break;
            }

            if (childMenu != null && childMenu != menu)
            {
                // New Dialog attached as child
                menu = childMenu;
                Remove(childMenu);
            }

            if (ve != null && ve != trigger)
            {
                if (trigger is IPressable c1)
                    c1.clickable.clicked -= OnActionTriggered;
                trigger = ve;
                if (trigger is IPressable c2)
                    c2.clickable.clicked += OnActionTriggered;
            }

            // we can also try to find the anchor (if any has been given with the UXML attribute)
            if (anchor != null && !string.IsNullOrEmpty(m_AnchorName) && panel != null)
            {
                var anchorElement = panel.visualTree.Q<VisualElement>(m_AnchorName);
                if (anchorElement != null)
                    anchor = anchorElement;
                else
                    Debug.LogWarning($"Unable to find {m_AnchorName}");
            }
        }

        void OnActionTriggered()
        {
            var popover = MenuBuilder.Build(anchor ?? trigger, menu);
            popover.SetCloseOnSelection(closeOnSelection);
            popover.Show();
        }

    }
}
