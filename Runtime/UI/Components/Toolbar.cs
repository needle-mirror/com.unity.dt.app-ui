using System;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The dock mode of the Toolbar.
    /// </summary>
    public enum ToolbarDockMode
    {
        /// <summary>
        /// The Toolbar is floating.
        /// </summary>
        Floating = 0,

        /// <summary>
        /// The Toolbar is docked to the top.
        /// </summary>
        Top,

        /// <summary>
        /// The Toolbar is docked to the bottom.
        /// </summary>
        Bottom,

        /// <summary>
        /// The Toolbar is docked to the left.
        /// </summary>
        Left,

        /// <summary>
        /// The Toolbar is docked to the right.
        /// </summary>
        Right
    }

    /// <summary>
    /// A versatile container for organizing command buttons and tools in a horizontal or vertical layout.
    /// </summary>
    /// <remarks>
    /// The Toolbar component provides a container for organizing and displaying a collection of controls,
    /// buttons, or tools in a structured manner. It can be positioned at various locations in your application
    /// and supports both horizontal and vertical orientations.
    ///
    /// Toolbars help maintain a consistent and accessible interface by grouping related actions together. They
    /// can be either fixed in position or made draggable to allow users to customize their workspace layout.
    ///
    /// **Note:** The Toolbar's appearance and behavior can be customized using USS (Unity Style Sheets) classes,
    /// making it adaptable to different visual themes and design requirements.
    /// </remarks>
    /// <example>
    /// <para>Basic Toolbar Example — A basic horizontal toolbar with common file operations.</para>
    /// <code lang="xml"><![CDATA[
    /// <Toolbar dock-mode="Top" direction="Horizontal">
    ///     <Button text="New" />
    ///     <Button text="Open" />
    ///     <Button text="Save" />
    ///     <Separator />
    ///     <Button text="Settings" />
    /// </Toolbar>
    /// ]]></code>
    /// <para>Vertical Toolbar with Tool Options — A vertical toolbar commonly used in design applications.</para>
    /// <code lang="xml"><![CDATA[
    /// <Toolbar dock-mode="Left" direction="Vertical" draggable="true">
    ///     <Button icon="brush" tooltip="Paint Tool" />
    ///     <Button icon="eraser" tooltip="Eraser Tool" />
    ///     <Button icon="select" tooltip="Selection Tool" />
    ///     <Separator />
    ///     <Button icon="settings" tooltip="Tool Settings" />
    /// </Toolbar>
    /// ]]></code>
    /// <para>Programmatic Toolbar Creation — Creating and configuring a toolbar with buttons programmatically.</para>
    /// <code lang="csharp"><![CDATA[
    /// var toolbar = new Toolbar();
    /// toolbar.dockMode = ToolbarDockMode.Top;
    /// toolbar.direction = Direction.Horizontal;
    /// toolbar.draggable = true;
    ///
    /// var newButton = new Button { text = "New" };
    /// var openButton = new Button { text = "Open" };
    /// var saveButton = new Button { text = "Save" };
    ///
    /// toolbar.Add(newButton);
    /// toolbar.Add(openButton);
    /// toolbar.Add(saveButton);
    ///
    /// parentElement.Add(toolbar);
    /// ]]></code>
    /// </example>
    [VisualDocPage("layouts")]
    [UxmlElement]
    public partial class Toolbar : BaseVisualElement
    {

        internal static readonly BindingId dockModeProperty = new BindingId(nameof(dockMode));

        internal static readonly BindingId draggableProperty = new BindingId(nameof(draggable));

        internal static readonly BindingId directionProperty = new BindingId(nameof(direction));



        /// <summary>
        /// The Toolbar's USS class name.
        /// </summary>
        public const string ussClassName = "appui-toolbar";

        /// <summary>
        /// The Toolbar's drag bar USS class name.
        /// </summary>
        public const string dragBarUssClassName = ussClassName + "__drag-bar";

        /// <summary>
        /// The Toolbar's drag bar indicator USS class name.
        /// </summary>
        public const string dragBarIndicatorUssClassName = dragBarUssClassName + "-indicator";

        /// <summary>
        /// The Toolbar's container USS class name.
        /// </summary>
        public const string containerUssClassName = ussClassName + "__container";

        /// <summary>
        /// The Toolbar's variant USS class name.
        /// </summary>
        [EnumName("GetDockModeUssClassName", typeof(ToolbarDockMode))]
        [EnumName("GetDirectionUssClassName", typeof(Direction))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The Toolbar's draggable USS class name.
        /// </summary>
        public const string draggableUssClassName = ussClassName + "--draggable";

        readonly VisualElement m_Container;

        readonly VisualElement m_DragBar;

        readonly VisualElement m_DragBarIndicator;

        readonly Draggable m_DragBarPressable;

        ToolbarDockMode m_DockMode;

        Direction m_Direction;

        /// <summary>
        /// The container of the Toolbar's content.
        /// </summary>
        public override VisualElement contentContainer => m_Container;

        /// <summary>
        /// The dock mode of the Toolbar.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public ToolbarDockMode dockMode
        {
            get => m_DockMode;
            set
            {
                var changed = m_DockMode != value;
                RemoveFromClassList(GetDockModeUssClassName(m_DockMode));
                m_DockMode = value;
                AddToClassList(GetDockModeUssClassName(m_DockMode));

                if (changed)
                    NotifyPropertyChanged(in dockModeProperty);
            }
        }

        /// <summary>
        /// Whether the Toolbar is draggable.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool draggable
        {
            get => ClassListContains(draggableUssClassName);
            set
            {
                var changed = ClassListContains(draggableUssClassName) != value;
                EnableInClassList(draggableUssClassName, value);

                if (changed)
                    NotifyPropertyChanged(in draggableProperty);
            }
        }

        /// <summary>
        /// The direction of the Toolbar.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Direction direction
        {
            get => m_Direction;
            set
            {
                var changed = m_Direction != value;
                RemoveFromClassList(GetDirectionUssClassName(m_Direction));
                m_Direction = value;
                AddToClassList(GetDirectionUssClassName(m_Direction));

                if (changed)
                    NotifyPropertyChanged(in directionProperty);
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Toolbar()
        {
            AddToClassList(ussClassName);

            m_DragBar = new VisualElement
            {
                name = dragBarUssClassName,
                pickingMode = PickingMode.Position
            };
            m_DragBar.AddToClassList(dragBarUssClassName);
            hierarchy.Add(m_DragBar);

            m_DragBarPressable = new Draggable(null, null, null);
            m_DragBar.AddManipulator(m_DragBarPressable);

            m_DragBarIndicator = new VisualElement
            {
                name = dragBarIndicatorUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_DragBarIndicator.AddToClassList(dragBarIndicatorUssClassName);
            m_DragBar.Add(m_DragBarIndicator);

            m_Container = new VisualElement
            {
                name = containerUssClassName,
                pickingMode = PickingMode.Ignore
            };
            m_Container.AddToClassList(containerUssClassName);
            hierarchy.Add(m_Container);

            draggable = false;
            dockMode = ToolbarDockMode.Floating;
            direction = Direction.Horizontal;
        }

    }
}