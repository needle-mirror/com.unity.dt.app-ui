using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A side navigation component that supports side-rail navigation patterns.
    /// </summary>
    /// <remarks>
    /// The Navigation Rail is a responsive alternative to the bottom navigation bar, designed for larger screen
    /// sizes. It provides easy access to top-level destinations in your app when using tablet and desktop
    /// screens.
    ///
    /// The rail is typically displayed on the left or right edge of the screen, containing navigation
    /// destinations represented by icons and labels. It can also include leading and trailing containers for
    /// additional UI elements like logos or settings buttons.
    ///
    /// Navigation Rails are recommended for apps with 3-7 top-level destinations. Using more destinations can
    /// create cluttered navigation that is difficult to scan.
    ///
    /// - Leading container: Optional area at the top for branding, profile, or global actions
    /// - Main container: Houses the navigation destinations
    /// - Trailing container: Optional area at the bottom for secondary actions
    /// </remarks>
    /// <example>
    /// <para>Basic Navigation Rail with items — A simple navigation rail with three destinations.</para>
    /// <code lang="xml"><![CDATA[
    /// <NavigationRail>
    ///     <NavigationRailItem icon="home" label="Home" selected="true" />
    ///     <NavigationRailItem icon="favorite" label="Favorites" />
    ///     <NavigationRailItem icon="settings" label="Settings" />
    /// </NavigationRail>
    /// ]]></code>
    /// <para>Navigation Rail with custom containers — Navigation rail with custom leading and trailing content.</para>
    /// <code lang="xml"><![CDATA[
    /// <NavigationRail>
    ///     <Template name="leadingContainer">
    ///         <Image class="logo" />
    ///     </Template>
    ///     <NavigationRailItem icon="dashboard" label="Dashboard" />
    ///     <NavigationRailItem icon="person" label="Profile" />
    ///     <Template name="trailingContainer">
    ///         <Button text="Logout" />
    ///     </Template>
    /// </NavigationRail>
    /// ]]></code>
    /// <para>Right-aligned Navigation Rail with selected-only labels — A right-aligned navigation rail showing labels
    /// only for selected items.</para>
    /// <code lang="xml"><![CDATA[
    /// <NavigationRail anchor="End" labelType="Selected" groupAlignment="Center">
    ///     <NavigationRailItem icon="mail" label="Mail" />
    ///     <NavigationRailItem icon="chat" label="Chat" selected="true" />
    ///     <NavigationRailItem icon="calendar" label="Calendar" />
    /// </NavigationRail>
    /// ]]></code>
    /// </example>
    [VisualDocPage("nav-components", id = "navigation-rail", displayName = "Navigation Rail")]
    [UxmlElement]
    public partial class NavigationRail : BaseVisualElement
    {

        internal static readonly BindingId anchorProperty = new BindingId(nameof(anchor));

        internal static readonly BindingId labelTypeProperty = new BindingId(nameof(labelType));

        internal static readonly BindingId groupAlignmentProperty = new BindingId(nameof(groupAlignment));


        /// <summary>
        /// The NavigationRail main styling class.
        /// </summary>
        public const string ussClassName = "appui-navigation-rail";

        /// <summary>
        /// The leading container styling class.
        /// </summary>
        public const string leadingContainerUssClassName = ussClassName + "__leading-container";

        /// <summary>
        /// The trailing container styling class.
        /// </summary>
        public const string trailingContainerUssClassName = ussClassName + "__trailing-container";

        /// <summary>
        /// The content container styling class.
        /// </summary>
        public const string contentContainerUssClassName = ussClassName + "__content-container";

        /// <summary>
        /// The NavigationRail variant styling class.
        /// </summary>
        [EnumName("GetAnchorUssClassName", typeof(NavigationRailAnchor))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The NavigationRail label type styling class.
        /// </summary>
        [EnumName("GetLabelTypeUssClassName", typeof(LabelType))]
        public const string labelTypeUssClassName = ussClassName + "-label-type--";

        /// <summary>
        /// The NavigationRail group alignment styling class.
        /// </summary>
        [EnumName("GetGroupAlignmentUssClassName", typeof(GroupAlignment))]
        public const string groupAlignmentUssClassName = ussClassName + "-group-align--";

        NavigationRailAnchor m_Anchor;

        LabelType m_LabelType;

        VisualElement m_ContentContainer;

        GroupAlignment m_GroupAlignment;

        /// <summary>
        /// The content container of the NavigationRail.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// The anchor of the NavigationRail. The NavigationRail will be anchored to the left or right side of the screen.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public NavigationRailAnchor anchor
        {
            get => m_Anchor;
            set
            {
                var changed = m_Anchor != value;
                RemoveFromClassList(GetAnchorUssClassName(m_Anchor));
                m_Anchor = value;
                AddToClassList(GetAnchorUssClassName(m_Anchor));

                if (changed)
                {
                    NotifyPropertyChanged(in anchorProperty);
                }
            }
        }

        /// <summary>
        /// The label type of the NavigationRail.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public LabelType labelType
        {
            get => m_LabelType;
            set
            {
                var changed = m_LabelType != value;
                RemoveFromClassList(GetLabelTypeUssClassName(m_LabelType));
                m_LabelType = value;
                AddToClassList(GetLabelTypeUssClassName(m_LabelType));

                if (changed)
                {
                    NotifyPropertyChanged(in labelTypeProperty);
                }
            }
        }

        /// <summary>
        /// The alignment of the group of items.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public GroupAlignment groupAlignment
        {
            get => m_GroupAlignment;
            set
            {
                var changed = m_GroupAlignment != value;
                RemoveFromClassList(GetGroupAlignmentUssClassName(m_GroupAlignment));
                m_GroupAlignment = value;
                AddToClassList(GetGroupAlignmentUssClassName(m_GroupAlignment));

                if (changed)
                {
                    NotifyPropertyChanged(in groupAlignmentProperty);
                }
            }
        }

        /// <summary>
        /// The leading container of the NavigationRail.
        /// </summary>
        public VisualElement leadingContainer { get; }

        /// <summary>
        /// The trailing container of the NavigationRail.
        /// </summary>
        public VisualElement trailingContainer { get; }

        /// <summary>
        /// The main container of the NavigationRail.
        /// </summary>
        public VisualElement mainContainer => m_ContentContainer;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public NavigationRail()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(ussClassName);

            leadingContainer = new VisualElement { name = leadingContainerUssClassName, pickingMode = PickingMode.Ignore };
            leadingContainer.AddToClassList(leadingContainerUssClassName);
            hierarchy.Add(leadingContainer);

            m_ContentContainer = new VisualElement { name = contentContainerUssClassName, pickingMode = PickingMode.Ignore };
            m_ContentContainer.AddToClassList(contentContainerUssClassName);
            hierarchy.Add(m_ContentContainer);

            trailingContainer = new VisualElement { name = trailingContainerUssClassName, pickingMode = PickingMode.Ignore };
            trailingContainer.AddToClassList(trailingContainerUssClassName);
            hierarchy.Add(trailingContainer);

            anchor = NavigationRailAnchor.Start;
            labelType = LabelType.All;
            groupAlignment = GroupAlignment.Start;
        }

    }

    /// <summary>
    /// The anchor of the Navigation Rail. The Rail will be anchored to the left or right side of the screen.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum NavigationRailAnchor
    {
        /// <summary>
        /// The Rail will be anchored to the left side of the screen.
        /// </summary>
        Start,
        /// <summary>
        /// The Rail will be anchored to the right side of the screen.
        /// </summary>
        End,
    }

    /// <summary>
    /// How to display the label of the NavigationRailItem.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum LabelType
    {
        /// <summary>
        /// No label will be displayed.
        /// </summary>
        None,
        /// <summary>
        /// Every label will be displayed.
        /// </summary>
        All,
        /// <summary>
        /// Only the selected label will be displayed.
        /// </summary>
        Selected,
    }

    /// <summary>
    /// The alignment of the group of items.
    /// </summary>
    [GenerateLowerCaseStrings]
    public enum GroupAlignment
    {
        /// <summary>
        /// Aligns the group to the start of the container.
        /// </summary>
        Start,
        /// <summary>
        /// Aligns the group to the center of the container.
        /// </summary>
        Center,
        /// <summary>
        /// Aligns the group to the end of the container.
        /// </summary>
        End,
    }
}
