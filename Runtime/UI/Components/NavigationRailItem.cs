using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A NavigationRailItem is a selectable item in a NavigationRail.
    /// </summary>
    [UxmlElement]
    public partial class NavigationRailItem : VisualElement, IPressable, ISelectableElement
    {

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId clickableProperty = nameof(clickable);

        /// <summary>
        /// Main styling class for the NavigationRailItem.
        /// </summary>
        public const string ussClassName = "appui-navigation-rail-item";

        /// <summary>
        /// Icon styling class for the NavigationRailItem.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        /// <summary>
        /// Label styling class for the NavigationRailItem.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        Icon m_IconElement;

        LocalizedTextElement m_LabelElement;

        Pressable m_Clickable;

        /// <summary>
        /// Whether the NavigationRailItem is in selected state.
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public bool selected
        {
            get => ClassListContains(Styles.selectedUssClassName);
            set
            {
                var changed = selected != value;
                SetSelectedWithoutNotify(value);

                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// The icon of the NavigationRailItem.
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public string icon
        {
            get => m_IconElement.iconName;
            set
            {
                var changed = m_IconElement.iconName != value;
                m_IconElement.iconName = value;
                m_IconElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_IconElement.iconName));

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The label of the NavigationRailItem.
        /// </summary>
        [UxmlAttribute]
        [CreateProperty]
        public string label
        {
            get => m_LabelElement.text;
            set
            {
                var changed = m_LabelElement.text != value;
                m_LabelElement.text = value;
                m_LabelElement.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_LabelElement.text));

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this NavigationRailItem.
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
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
                if (changed)
                    NotifyPropertyChanged(in clickableProperty);
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NavigationRailItem()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            clickable = new Pressable();
            this.AddManipulator(clickable);

            m_IconElement = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_IconElement.AddToClassList(iconUssClassName);
            hierarchy.Add(m_IconElement);

            m_LabelElement = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_LabelElement.AddToClassList(labelUssClassName);
            hierarchy.Add(m_LabelElement);

            icon = null;
            label = null;
            selected = false;
        }

        /// <inheritdoc/>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }


    }
}
