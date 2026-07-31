using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A button component that displays only an icon, ideal for compact UI actions.
    /// </summary>
    /// <remarks>
    /// IconButton is a specialized button component that displays an icon without text. It's commonly used for
    /// toolbar actions, navigation items, or any UI element where space is limited and the icon clearly
    /// communicates the action.
    ///
    /// The component supports different sizes, variants, and states to accommodate various design requirements
    /// and user interactions.
    ///
    /// IconButtons can be styled with primary and quiet variants, making them versatile for different visual
    /// hierarchies in your interface.
    ///
    /// For accessibility, IconButtons are keyboard focusable and support keyboard navigation with a tabIndex of 0
    /// by default.
    /// </remarks>
    /// <example>
    /// <para>**Basic IconButton Usage.** Different variations of IconButton usage in C#.</para>
    /// <code lang="csharp">
    /// // Create a simple icon button
    /// var deleteButton = new IconButton("delete");
    /// deleteButton.clicked += () => HandleDelete();
    ///
    /// // Create a primary action button
    /// var addButton = new IconButton("add") { primary = true, size = Size.L };
    ///
    /// // Create a quiet variant for secondary actions
    /// var menuButton = new IconButton("menu") { quiet = true };
    /// </code>
    /// <para>**UXML Integration.** Using IconButton in UXML markup.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <appui:IconButton icon="settings"
    ///                      size="M"
    ///                      primary="false"
    ///                      quiet="false"
    ///                      variant="Regular" />
    /// </ui:UXML>
    /// ]]></code>
    /// <para>**Toolbar Example.** Creating a toolbar with multiple IconButtons.</para>
    /// <code lang="xml"><![CDATA[
    /// <ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:appui="Unity.AppUI.UI">
    ///     <ui:VisualElement class="toolbar">
    ///         <appui:IconButton icon="undo" quiet="true" size="S" />
    ///         <appui:IconButton icon="redo" quiet="true" size="S" />
    ///         <appui:IconButton icon="save" primary="true" size="S" />
    ///         <appui:IconButton icon="delete" size="S" />
    ///     </ui:VisualElement>
    /// </ui:UXML>
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("actions")]
    public partial class IconButton : ExVisualElement, ISizeableElement, IPressable
    {

        internal static readonly BindingId iconProperty = new BindingId(nameof(icon));

        internal static readonly BindingId primaryProperty = new BindingId(nameof(primary));

        internal static readonly BindingId quietProperty = new BindingId(nameof(quiet));

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId variantProperty = new BindingId(nameof(variant));

        internal static readonly BindingId clickableProperty = new BindingId(nameof(clickable));


        /// <summary>
        /// The IconButton main styling class.
        /// </summary>
        public const string ussClassName = "appui-button";

        /// <summary>
        /// The IconButton primary variant styling class.
        /// </summary>
        public const string primaryUssClassName = ussClassName + "--primary";

        /// <summary>
        /// The IconButton quiet mode styling class.
        /// </summary>
        public const string quietUssClassName = ussClassName + "--quiet";

        /// <summary>
        /// The IconButton leading container styling class.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__leadingcontainer";

        /// <summary>
        /// The IconButton leading icon styling class.
        /// </summary>
        public const string iconUssClassName = ussClassName + "__leadingicon";

        /// <summary>
        /// The IconButton size styling class.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        readonly VisualElement m_Container;

        readonly Icon m_Icon;

        Size m_Size;

        Pressable m_Clickable;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IconButton()
            : this(null)
        {

        }

        /// <summary>
        /// Construct an IconButton with a given icon.
        /// </summary>
        /// <param name="iconName">The name of the icon.</param>
        /// <param name="clickEvent">The click event callback.</param>
        public IconButton(string iconName, Action clickEvent = null)
        {
            AddToClassList(ussClassName);
            AddToClassList(Button.iconOnlyUssClassName);

            clickable = new Pressable(clickEvent);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;
            passMask = 0;

            m_Container = new VisualElement { name = containerUssClassName, pickingMode = PickingMode.Ignore };
            m_Container.AddToClassList(containerUssClassName);
            m_Icon = new Icon { name = iconUssClassName, pickingMode = PickingMode.Ignore };
            m_Icon.AddToClassList(iconUssClassName);

            m_Container.hierarchy.Add(m_Icon);
            hierarchy.Add(m_Container);

            primary = false;
            quiet = false;
            icon = iconName;
            size = Size.M;
            variant = IconVariant.Regular;

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocusIn, OnPointerFocusIn));
        }

        void OnPointerFocusIn(FocusInEvent evt)
        {
            passMask = 0;
        }

        void OnKeyboardFocusIn(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.Outline;
        }

        /// <summary>
        /// Event triggered when the Button has been clicked.
        /// </summary>
        public event Action clicked
        {
            add => m_Clickable.clicked += value;
            remove => m_Clickable.clicked -= value;
        }

        /// <summary>
        /// Clickable Manipulator for this Button.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            set
            {
                var changed = value != m_Clickable;
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
        /// Use the primary variant of the Button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool primary
        {
            get => ClassListContains(primaryUssClassName);
            set
            {
                var changed = ClassListContains(primaryUssClassName) != value;
                EnableInClassList(primaryUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in primaryProperty);
            }
        }

        /// <summary>
        /// The quiet state of the Button.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool quiet
        {
            get => ClassListContains(quietUssClassName);
            set
            {
                var changed = ClassListContains(quietUssClassName) != value;
                EnableInClassList(quietUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in quietProperty);
            }
        }

        /// <summary>
        /// The IconButton icon.
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
                m_Container.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(m_Icon.iconName));

                if (changed)
                    NotifyPropertyChanged(in iconProperty);
            }
        }

        /// <summary>
        /// The IconButton icon variant.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public IconVariant variant
        {
            get => m_Icon.variant;
            set
            {
                var changed = m_Icon.variant != value;
                m_Icon.variant = value;

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// The Button size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => m_Size;
            set
            {
                var changed = m_Size != value;
                RemoveFromClassList(GetSizeUssClassName(m_Size));
                m_Size = value;
                m_Icon.size = m_Size switch
                {
                    Size.S => IconSize.S,
                    Size.M => IconSize.M,
                    Size.L => IconSize.L,
                    _ => IconSize.M
                };
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }


    }
}
