using System;
using Unity.AppUI.Core;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A circular button that promotes a primary action in the application.
    /// </summary>
    /// <remarks>
    /// The Floating Action Button (FAB) represents the primary action in an application. It appears on top of
    /// the main content and stays fixed in a position (usually bottom right) as a circular button with
    /// elevation (shadow).
    ///
    /// FABs are used for positive actions like Create, Favorite, Share, or Start. The action should be
    /// contextual to the screen's main content.
    ///
    /// Only use one FAB per screen to represent the most important action. Multiple FABs can cause confusion
    /// about which action is primary.
    ///
    /// The component supports different sizes, elevation levels, and can be styled with an accent color to
    /// make it more prominent.
    /// </remarks>
    /// <example>
    /// <para>Basic FAB with default settings. Creates a medium-sized FAB with the add icon.</para>
    /// <code lang="xml"><![CDATA[
    /// <FAB>
    ///     <Icon name="add" />
    /// </FAB>
    /// ]]></code>
    /// <para>Accent FAB with custom elevation. Creates an accent-colored FAB with higher elevation.</para>
    /// <code lang="xml"><![CDATA[
    /// <FAB accent="true" elevation="16">
    ///     <Icon name="favorite" />
    /// </FAB>
    /// ]]></code>
    /// <para>Code example showing FAB creation and event handling. Creating and configuring FAB in C# code.</para>
    /// <code lang="csharp"><![CDATA[
    /// var fab = new FloatingActionButton(() => {
    ///     Debug.Log("FAB clicked!");
    /// });
    /// fab.size = Size.L;
    /// fab.accent = true;
    /// fab.Add(new Icon { name = "add" });
    /// parentElement.Add(fab);
    /// ]]></code>
    /// </example>
    [UxmlElement]
    [VisualDocPage("inputs")]
    public partial class FloatingActionButton : ExVisualElement, IPressable
    {

        internal static readonly BindingId sizeProperty = new BindingId(nameof(size));

        internal static readonly BindingId elevationProperty = new BindingId(nameof(elevation));

        internal static readonly BindingId accentProperty = new BindingId(nameof(accent));

        internal static readonly BindingId clickableProperty = new BindingId(nameof(clickable));


        /// <summary>
        /// The Floating Action Button's USS class name.
        /// </summary>
        public const string ussClassName = "appui-fab";

        /// <summary>
        /// The Floating Action Button's elevation USS class name.
        /// </summary>
        public const string elevationUssClassName = Styles.elevationUssClassName;

        /// <summary>
        /// The Floating Action Button's size USS class name.
        /// </summary>
        [EnumName("GetSizeUssClassName", typeof(Size))]
        public const string sizeUssClassName = ussClassName + "--size-";

        /// <summary>
        /// The Floating Action Button's accent USS class name.
        /// </summary>
        public const string accentUssClassName = ussClassName + "--accent";

        Pressable m_Clickable;

        int m_Elevation;

        Size m_Size;

        /// <summary>
        /// The clickable manipulator used by this button.
        /// </summary>
        [CreateProperty]
        public Pressable clickable
        {
            get => m_Clickable;
            private set
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
        /// Event fired when the button is clicked.
        /// </summary>
        public event Action clicked
        {
            add => clickable.clicked += value;
            remove => clickable.clicked -= value;
        }

        /// <summary>
        /// The elevation of this element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int elevation
        {
            get => m_Elevation;
            set
            {
                var changed = m_Elevation != value;
                RemoveFromClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));
                m_Elevation = value;
                AddToClassList(MemoryUtils.Concatenate(elevationUssClassName, m_Elevation.ToString()));

                if (changed)
                    NotifyPropertyChanged(in elevationProperty);
            }
        }

        /// <summary>
        /// The accent variant of this element.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool accent
        {
            get => ClassListContains(accentUssClassName);
            set
            {
                var changed = ClassListContains(accentUssClassName) != value;
                EnableInClassList(accentUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in accentProperty);
            }
        }

        /// <summary>
        /// The size of this element.
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
                AddToClassList(GetSizeUssClassName(m_Size));

                if (changed)
                    NotifyPropertyChanged(in sizeProperty);
            }
        }

        /// <summary>
        /// The content container of this element.
        /// </summary>
        public override VisualElement contentContainer => this;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public FloatingActionButton() : this(null) { }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="clickAction"> The action to perform when the button is clicked. </param>
        public FloatingActionButton(Action clickAction)
        {
            AddToClassList(ussClassName);

            clickable = new Pressable(clickAction);
            pickingMode = PickingMode.Position;
            focusable = true;
            tabIndex = 0;

            passMask = Passes.Clear | Passes.OutsetShadows;
            elevation = 12;
            size = Size.M;

            this.AddManipulator(new KeyboardFocusController(OnKeyboardFocus, OnPointerFocus));
        }

        void OnPointerFocus(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.OutsetShadows;
        }

        void OnKeyboardFocus(FocusInEvent evt)
        {
            passMask = Passes.Clear | Passes.OutsetShadows | Passes.Outline;
        }

    }
}
