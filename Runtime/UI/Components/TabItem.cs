using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// An item used in <see cref="Tabs"/> bar.
    /// </summary>
    [UxmlElement]
    public partial class TabItem : BaseVisualElement, ISelectableElement, IPressable
    {

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId labelProperty = nameof(label);

        internal static readonly BindingId iconProperty = nameof(icon);

        internal static readonly BindingId clickableProperty = nameof(clickable);



        /// <summary>
        /// The TabItem main styling class.
        /// </summary>
        public const string ussClassName = "appui-tabitem";

        /// <summary>
        /// The TabItem label styling class.
        /// </summary>
        public const string labelUssClassName = ussClassName + "__label";

        /// <summary>
        /// The TabItem icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__icon";

        readonly Icon m_Icon;

        readonly LocalizedTextElement m_Label;

        Pressable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TabItem()
        {
            focusable = true;
            pickingMode = PickingMode.Position;
            tabIndex = 0;
            clickable = new Pressable(OnPressed);

            AddToClassList(ussClassName);

            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);
            hierarchy.Add(m_Icon);
            m_Label = new LocalizedTextElement { name = labelUssClassName, pickingMode = PickingMode.Ignore };
            m_Label.AddToClassList(labelUssClassName);
            hierarchy.Add(m_Label);

            label = null;
            icon = null;
            selected = false;
        }

        void OnPressed()
        {
            using var evt = ActionTriggeredEvent.GetPooled();
            evt.target = this;
            SendEvent(evt);
        }

        /// <summary>
        /// The TabItem label.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => m_Label.text;
            set
            {
                var changed = m_Label.text != value;
                m_Label.text = value;
                m_Label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Label.text));

                if (changed)
                    NotifyPropertyChanged(in labelProperty);
            }
        }

        /// <summary>
        /// The TabItem icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string icon
        {
            get => m_Icon.iconName;
            set
            {
                var changed = m_Icon.iconName != value;
                m_Icon.iconName = value;
                m_Icon.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Icon.iconName));

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// Clickable Manipulator for this TabItem.
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
        /// The selected state of the TabItem.
        /// </summary>
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
        /// Set the selected state of the TabItem without notifying the selection system.
        /// </summary>
        /// <param name="newValue"> The new selected state.</param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }

    }
}
