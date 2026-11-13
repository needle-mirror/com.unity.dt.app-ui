using System;
using System.Collections.Generic;
using Unity.AppUI.Core;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The colour role of a <see cref="CanvasNode"/>.
    /// </summary>
    /// <remarks>
    /// Mirrors the Animator window's vocabulary: a green Entry state, a red Exit state, an amber
    /// special state, and an ordinary surface node for everything else.
    /// </remarks>
    public enum CanvasNodeVariant
    {
        /// <summary>
        /// A neutral surface node.
        /// </summary>
        Default = 0,

        /// <summary>
        /// The entry point of a graph.
        /// </summary>
        Entry,

        /// <summary>
        /// The exit point of a graph.
        /// </summary>
        Exit,

        /// <summary>
        /// A node that needs to stand out without being an entry or an exit.
        /// </summary>
        Special
    }

    /// <summary>
    /// Which edge of a <see cref="CanvasNode"/> a port sits on.
    /// </summary>
    public enum CanvasPortSide
    {
        /// <summary>
        /// The left edge: connections arrive here.
        /// </summary>
        Input = 0,

        /// <summary>
        /// The right edge: connections leave from here.
        /// </summary>
        Output
    }

    /// <summary>
    /// One connection point on a <see cref="CanvasNode"/>.
    /// </summary>
    /// <remarks>
    /// A port is data, not a visual element: the node builds and recycles the row that draws it. The
    /// dot's colour is left unset by default so the stylesheet decides it; a caller sets
    /// <see cref="color"/> only when the colour carries meaning, such as a data type in a shader graph.
    /// </remarks>
    public readonly struct CanvasPort : IEquatable<CanvasPort>
    {
        /// <summary>
        /// The port's stable identity, used by the caller to match a connection to a port.
        /// </summary>
        public string id { get; }

        /// <summary>
        /// The label shown beside the dot. Null or empty draws the dot alone.
        /// </summary>
        public string name { get; }

        /// <summary>
        /// The dot's colour. Unset, the dot takes its colour from the stylesheet.
        /// </summary>
        public Optional<Color> color { get; }

        /// <summary>
        /// Shown as the port row's tooltip when it is hovered, such as the type the port carries.
        /// Null or empty shows no tooltip.
        /// </summary>
        public string description { get; }

        /// <summary>
        /// Constructs a port.
        /// </summary>
        /// <param name="id"> The port's stable identity. </param>
        /// <param name="name"> The label shown beside the dot. </param>
        /// <param name="color"> The dot's colour, or unset to let the stylesheet decide. </param>
        /// <param name="description"> The row's hover tooltip, or null for none. </param>
        public CanvasPort(string id, string name = null, Optional<Color> color = default, string description = null)
        {
            this.id = id;
            this.name = name;
            this.color = color;
            this.description = description;
        }

        /// <summary>
        /// Determines whether two ports are equal.
        /// </summary>
        /// <param name="other"> The other port. </param>
        /// <returns> Whether the two ports are equal. </returns>
        public bool Equals(CanvasPort other) =>
            id == other.id && name == other.name && color.Equals(other.color) && description == other.description;

        /// <summary>
        /// Determines whether this port equals another object.
        /// </summary>
        /// <param name="obj"> The object to compare against. </param>
        /// <returns> Whether the object is an equal port. </returns>
        public override bool Equals(object obj) => obj is CanvasPort other && Equals(other);

        /// <summary>
        /// A hash code for this port.
        /// </summary>
        /// <returns> The hash code. </returns>
        public override int GetHashCode() => HashCode.Combine(id, name, color, description);
    }

    /// <summary>
    /// What a <see cref="CanvasNode"/> reports when a pointer interacts with one of its ports.
    /// </summary>
    public readonly struct CanvasPortEvent
    {
        /// <summary>
        /// The port under the pointer.
        /// </summary>
        public CanvasPort port { get; }

        /// <summary>
        /// Which edge the port sits on.
        /// </summary>
        public CanvasPortSide side { get; }

        /// <summary>
        /// The port's index within its side.
        /// </summary>
        public int index { get; }

        /// <summary>
        /// The node the port belongs to.
        /// </summary>
        public CanvasNode node { get; }

        /// <summary>
        /// The pointer's position, in panel coordinates.
        /// </summary>
        public Vector2 position { get; }

        /// <summary>
        /// Constructs a port event.
        /// </summary>
        /// <param name="node"> The node the port belongs to. </param>
        /// <param name="port"> The port under the pointer. </param>
        /// <param name="side"> Which edge the port sits on. </param>
        /// <param name="index"> The port's index within its side. </param>
        /// <param name="position"> The pointer's position, in panel coordinates. </param>
        public CanvasPortEvent(CanvasNode node, CanvasPort port, CanvasPortSide side, int index, Vector2 position)
        {
            this.node = node;
            this.port = port;
            this.side = side;
            this.index = index;
            this.position = position;
        }
    }

    /// <summary>
    /// A node-graph box, for Animator-style state machines and blend trees drawn inside a
    /// <see cref="Canvas"/>.
    /// </summary>
    /// <remarks>
    /// A titled header, an optional body and labelled input/output ports down the left and right
    /// edges. Place it with <see cref="x"/> and <see cref="y"/> in the canvas' content space, set
    /// <see cref="movable"/> for drag-to-move, and connect two ports with a <see cref="CanvasEdge"/>.
    ///
    /// The node lives *inside* a <see cref="Canvas"/> and owns none of its pan or zoom: the canvas
    /// applies its zoom as a transform on the viewport container, so a drag delta converted through
    /// the node's parent is already in content space. That is why this component has no zoom property
    /// of its own, unlike the web original, which has to divide its pointer deltas by the zoom by hand.
    ///
    /// <see cref="headerHeight"/>, <see cref="portRowHeight"/> and <see cref="portsPaddingY"/> are the
    /// published geometry an edge routes against, and <see cref="GetPortAnchor"/> turns them into a
    /// content-space point. They are compile-time constants, so the stylesheet expresses the same
    /// geometry through the **static** sizing ramp — the one ramp that does not rescale with density —
    /// and the two cannot drift apart.
    /// </remarks>
    /// <example>
    /// <para>Two connected nodes.</para>
    /// <code lang="csharp">
    /// var from = new CanvasNode
    /// {
    ///     title = "Idle",
    ///     x = 0, y = 0,
    ///     outputPorts = new[] { new CanvasPort("out", "Next") },
    /// };
    /// var to = new CanvasNode
    /// {
    ///     title = "Run",
    ///     x = 260, y = 80,
    ///     inputPorts = new[] { new CanvasPort("in", "Previous") },
    /// };
    /// var edge = new CanvasEdge
    /// {
    ///     from = from.GetPortAnchor(CanvasPortSide.Output, 0),
    ///     to = to.GetPortAnchor(CanvasPortSide.Input, 0),
    ///     isDirectional = true,
    /// };
    /// canvas.Add(edge);  // edges first, so nodes paint on top
    /// canvas.Add(from);
    /// canvas.Add(to);
    /// </code>
    /// <para>In UXML.</para>
    /// <code lang="xml">
    /// &lt;appui:CanvasNode title="Idle" variant="Entry" movable="true" /&gt;
    /// </code>
    /// </example>
    [UxmlElement]
    public partial class CanvasNode : BaseVisualElement, ISelectableElement
    {
        internal static readonly BindingId titleProperty = nameof(title);

        internal static readonly BindingId variantProperty = nameof(variant);

        internal static readonly BindingId selectedProperty = nameof(selected);

        internal static readonly BindingId xProperty = nameof(x);

        internal static readonly BindingId yProperty = nameof(y);

        internal static readonly BindingId widthProperty = nameof(width);

        internal static readonly BindingId movableProperty = nameof(movable);

        internal static readonly BindingId lodProperty = nameof(lod);

        /// <summary>
        /// The height of the node's header, in pixels.
        /// </summary>
        /// <remarks>
        /// Density-invariant: the stylesheet draws it from <c>--appui-static-sizing-375</c>.
        /// </remarks>
        public const float headerHeight = 30f;

        /// <summary>
        /// The height of one port row, in pixels.
        /// </summary>
        /// <remarks>
        /// Density-invariant: the stylesheet draws it from <c>--appui-static-sizing-300</c>.
        /// </remarks>
        public const float portRowHeight = 24f;

        /// <summary>
        /// The vertical padding above the first port row and below the last, in pixels.
        /// </summary>
        /// <remarks>
        /// Density-invariant: the stylesheet draws it from <c>--appui-static-sizing-100</c>.
        /// </remarks>
        public const float portsPaddingY = 8f;

        /// <summary>
        /// The node's default width, in pixels.
        /// </summary>
        /// <remarks>
        /// Density-invariant: the stylesheet draws it from <c>--appui-static-sizing-2250</c>.
        /// </remarks>
        public const float defaultWidth = 180f;

        /// <summary>
        /// The CanvasNode main styling class.
        /// </summary>
        public const string ussClassName = "appui-canvas-node";

        /// <summary>
        /// The CanvasNode variant styling class.
        /// </summary>
        [EnumName("GetVariantUssClassName", typeof(CanvasNodeVariant))]
        public const string variantUssClassName = ussClassName + "--";

        /// <summary>
        /// The CanvasNode level-of-detail styling class.
        /// </summary>
        [EnumName("GetLodUssClassName", typeof(CanvasLevelOfDetail))]
        public const string lodUssClassName = ussClassName + "--lod-";

        /// <summary>
        /// The styling class of a node placed at an explicit position.
        /// </summary>
        public const string positionedUssClassName = ussClassName + "--positioned";

        /// <summary>
        /// The styling class of a node that can be dragged.
        /// </summary>
        public const string movableUssClassName = ussClassName + "--movable";

        /// <summary>
        /// The styling class of a node currently being dragged.
        /// </summary>
        public const string grabbingUssClassName = ussClassName + "--grabbing";

        /// <summary>
        /// The styling class of the header.
        /// </summary>
        public const string headerUssClassName = ussClassName + "__header";

        /// <summary>
        /// The styling class of the header's title.
        /// </summary>
        public const string titleUssClassName = ussClassName + "__title";

        /// <summary>
        /// The styling class of the port area.
        /// </summary>
        public const string portsUssClassName = ussClassName + "__ports";

        /// <summary>
        /// The styling class of a column of ports.
        /// </summary>
        [EnumName("GetPortColumnUssClassName", typeof(CanvasPortSide))]
        public const string portColumnUssClassName = ussClassName + "__ports--";

        /// <summary>
        /// The styling class of one port row.
        /// </summary>
        public const string portRowUssClassName = ussClassName + "__port-row";

        /// <summary>
        /// The styling class of a port's pointer target.
        /// </summary>
        public const string portHitUssClassName = ussClassName + "__port-hit";

        /// <summary>
        /// The styling class of a port's dot.
        /// </summary>
        public const string portDotUssClassName = ussClassName + "__port-dot";

        /// <summary>
        /// The styling class of a port's label.
        /// </summary>
        public const string portLabelUssClassName = ussClassName + "__port-label";

        /// <summary>
        /// The styling class of the body, which holds the node's children.
        /// </summary>
        public const string bodyUssClassName = ussClassName + "__body";

        readonly VisualElement m_Header;

        readonly LocalizedTextElement m_Title;

        readonly VisualElement m_Ports;

        readonly VisualElement m_InputColumn;

        readonly VisualElement m_OutputColumn;

        readonly VisualElement m_Body;

        readonly Draggable m_Draggable;

        readonly List<CanvasPort> m_InputPorts = new List<CanvasPort>();

        readonly List<CanvasPort> m_OutputPorts = new List<CanvasPort>();

        CanvasNodeVariant m_Variant;

        bool m_Selected;

        bool m_Movable;

        Optional<float> m_X;

        Optional<float> m_Y;

        float m_Width = defaultWidth;

        CanvasLevelOfDetail m_Lod;

        Vector2 m_DragOrigin;

        /// <summary>
        /// Raised when the node is pressed with the primary button, so the caller can select it.
        /// </summary>
        /// <remarks>
        /// A pan press inside a <see cref="Canvas"/> -- the middle button, or the primary button with the
        /// pan modifier held -- is taken by the canvas before the node sees it, so it never selects.
        /// </remarks>
        public event Action selectRequested;

        /// <summary>
        /// Raised while a movable node is dragged, with its next content-space position.
        /// </summary>
        /// <remarks>
        /// The node has already moved itself by the time this fires; the event exists so the caller can
        /// persist the position and re-route any edges attached to it.
        /// </remarks>
        public event Action<Vector2> positionChanged;

        /// <summary>
        /// Raised when a pointer goes down on a port — begin dragging a new connection from it.
        /// </summary>
        public event Action<CanvasPortEvent> portPointerDown;

        /// <summary>
        /// Raised when a pointer is released on a port — drop a connection onto it. Validate in the handler.
        /// </summary>
        public event Action<CanvasPortEvent> portPointerUp;

        /// <summary>
        /// Raised when a pointer enters a port, which is where a dragged connection would snap to it.
        /// </summary>
        public event Action<CanvasPortEvent> portPointerEntered;

        /// <summary>
        /// Raised when a pointer leaves a port.
        /// </summary>
        public event Action<CanvasPortEvent> portPointerLeft;

        /// <summary>
        /// The node's body, which is also its content container.
        /// </summary>
        public override VisualElement contentContainer => m_Body;

        /// <summary>
        /// The header element, exposed so a caller can style or decorate it.
        /// </summary>
        public VisualElement header => m_Header;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public CanvasNode()
        {
            AddToClassList(ussClassName);
            focusable = false;

            m_Header = new VisualElement { name = headerUssClassName, pickingMode = PickingMode.Ignore };
            m_Header.AddToClassList(headerUssClassName);

            m_Title = new LocalizedTextElement { name = titleUssClassName, pickingMode = PickingMode.Ignore };
            m_Title.AddToClassList(titleUssClassName);
            m_Header.hierarchy.Add(m_Title);

            m_Ports = new VisualElement { name = portsUssClassName, pickingMode = PickingMode.Ignore };
            m_Ports.AddToClassList(portsUssClassName);

            m_InputColumn = new VisualElement { pickingMode = PickingMode.Ignore };
            m_InputColumn.AddToClassList(GetPortColumnUssClassName(CanvasPortSide.Input));
            m_OutputColumn = new VisualElement { pickingMode = PickingMode.Ignore };
            m_OutputColumn.AddToClassList(GetPortColumnUssClassName(CanvasPortSide.Output));
            m_Ports.hierarchy.Add(m_InputColumn);
            m_Ports.hierarchy.Add(m_OutputColumn);

            m_Body = new VisualElement { name = bodyUssClassName };
            m_Body.AddToClassList(bodyUssClassName);

            hierarchy.Add(m_Header);
            hierarchy.Add(m_Ports);
            hierarchy.Add(m_Body);

            // Only a primary press selects. A middle press is a pan, and a primary press with the pan
            // modifier never gets here: the canvas takes it in the capture phase.
            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == (int)MouseButton.LeftMouse)
                    selectRequested?.Invoke();
            });

            m_Draggable = new Draggable(OnPressed, OnDragged, OnDragEnded, OnDragStarted)
            {
                dragDirection = Draggable.DragDirection.Free,
            };

            title = null;
            variant = CanvasNodeVariant.Default;
            selected = false;
            movable = false;
            lod = CanvasLevelOfDetail.Full;
            width = defaultWidth;
            x = Optional<float>.none;
            y = Optional<float>.none;
            RefreshVisibility();
        }

        /// <summary>
        /// The header label. Null or empty hides the header entirely.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string title
        {
            get => m_Title.text;
            set
            {
                var changed = m_Title.text != value;
                m_Title.text = value;
                RefreshVisibility();

                if (changed)
                    NotifyPropertyChanged(in titleProperty);
            }
        }

        /// <summary>
        /// The node's colour role. <see cref="CanvasNodeVariant.Default"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public CanvasNodeVariant variant
        {
            get => m_Variant;
            set
            {
                var changed = m_Variant != value;
                RemoveFromClassList(GetVariantUssClassName(m_Variant));
                m_Variant = value;
                AddToClassList(GetVariantUssClassName(m_Variant));

                if (changed)
                    NotifyPropertyChanged(in variantProperty);
            }
        }

        /// <summary>
        /// Whether the node is selected. False by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool selected
        {
            get => m_Selected;
            set
            {
                var changed = m_Selected != value;
                SetSelectedWithoutNotify(value);

                if (changed)
                    NotifyPropertyChanged(in selectedProperty);
            }
        }

        /// <summary>
        /// Sets the selected state without notifying.
        /// </summary>
        /// <param name="newValue"> The new selected state. </param>
        public void SetSelectedWithoutNotify(bool newValue)
        {
            m_Selected = newValue;
            EnableInClassList(Styles.selectedUssClassName, newValue);
        }

        /// <summary>
        /// Whether the node can be dragged with the primary pointer. False by default.
        /// </summary>
        /// <remarks>
        /// A movable node moves itself and reports the result through <see cref="positionChanged"/>. It
        /// also stops the press from reaching the canvas, so dragging a node never pans the canvas.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public bool movable
        {
            get => m_Movable;
            set
            {
                var changed = m_Movable != value;
                m_Movable = value;
                EnableInClassList(movableUssClassName, value);
                if (value)
                    this.AddManipulator(m_Draggable);
                else
                    this.RemoveManipulator(m_Draggable);

                if (changed)
                    NotifyPropertyChanged(in movableProperty);
            }
        }

        /// <summary>
        /// How much of the node to draw. <see cref="CanvasLevelOfDetail.Full"/> by default.
        /// </summary>
        /// <remarks>
        /// <see cref="CanvasLevelOfDetail.Compact"/> hides the port labels and the body.
        /// <see cref="CanvasLevelOfDetail.Box"/> also hides the title and the port dots, leaving the
        /// header colour over the port area. Every level keeps the header and port rows' geometry, so
        /// edges anchored with <see cref="GetPortAnchor"/> still meet the node. Pick the level from the
        /// canvas zoom with <see cref="CanvasCulling.LevelOfDetailForZoom"/>.
        /// </remarks>
        [CreateProperty]
        [UxmlAttribute]
        public CanvasLevelOfDetail lod
        {
            get => m_Lod;
            set
            {
                var changed = m_Lod != value;
                RemoveFromClassList(GetLodUssClassName(m_Lod));
                m_Lod = value;
                AddToClassList(GetLodUssClassName(m_Lod));

                if (changed)
                    NotifyPropertyChanged(in lodProperty);
            }
        }

        /// <summary>
        /// The node's X coordinate in the canvas' content space. Unset leaves the node in normal flow.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> x
        {
            get => m_X;
            set
            {
                var changed = !m_X.Equals(value);
                m_X = value;
                RefreshPosition();

                if (changed)
                    NotifyPropertyChanged(in xProperty);
            }
        }

        /// <summary>
        /// The node's Y coordinate in the canvas' content space. Unset leaves the node in normal flow.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Optional<float> y
        {
            get => m_Y;
            set
            {
                var changed = !m_Y.Equals(value);
                m_Y = value;
                RefreshPosition();

                if (changed)
                    NotifyPropertyChanged(in yProperty);
            }
        }

        /// <summary>
        /// The node's width in pixels. <see cref="defaultWidth"/> by default.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float width
        {
            get => m_Width;
            set
            {
                var changed = !Mathf.Approximately(m_Width, value);
                m_Width = value;
                style.width = value;

                if (changed)
                    NotifyPropertyChanged(in widthProperty);
            }
        }

        /// <summary>
        /// The ports down the node's left edge.
        /// </summary>
        /// <remarks>
        /// Setting this rebuilds the input column. Null or empty removes every input row.
        /// </remarks>
        public IEnumerable<CanvasPort> inputPorts
        {
            get => m_InputPorts;
            set => SetPorts(CanvasPortSide.Input, value);
        }

        /// <summary>
        /// The ports down the node's right edge.
        /// </summary>
        /// <remarks>
        /// Setting this rebuilds the output column. Null or empty removes every output row.
        /// </remarks>
        public IEnumerable<CanvasPort> outputPorts
        {
            get => m_OutputPorts;
            set => SetPorts(CanvasPortSide.Output, value);
        }

        /// <summary>
        /// The number of ports on one side.
        /// </summary>
        /// <param name="side"> The side to count. </param>
        /// <returns> The number of ports on that side. </returns>
        public int GetPortCount(CanvasPortSide side) =>
            side == CanvasPortSide.Input ? m_InputPorts.Count : m_OutputPorts.Count;

        /// <summary>
        /// The row element that draws one port, or null when the index is out of range.
        /// </summary>
        /// <param name="side"> The side the port sits on. </param>
        /// <param name="index"> The port's index within that side. </param>
        /// <returns> The row element, or null. </returns>
        public VisualElement GetPortElement(CanvasPortSide side, int index)
        {
            var column = side == CanvasPortSide.Input ? m_InputColumn : m_OutputColumn;
            return index >= 0 && index < column.childCount ? column[index] : null;
        }

        /// <summary>
        /// The point an edge should attach to for one port, in the canvas' content space.
        /// </summary>
        /// <param name="side"> The side the port sits on. </param>
        /// <param name="index"> The port's index within that side. </param>
        /// <returns> The anchor point. </returns>
        /// <remarks>
        /// Computed from <see cref="headerHeight"/>, <see cref="portsPaddingY"/> and
        /// <see cref="portRowHeight"/>, so it is usable while a graph is still being built, before the
        /// node has ever been laid out. It assumes the node is positioned: an unset <see cref="x"/> or
        /// <see cref="y"/> counts as zero, and a hidden header contributes no offset.
        ///
        /// The node's border is part of its box, so the ports are inset by one border width from the
        /// coordinates <see cref="x"/> and <see cref="y"/> name. That inset is added here once the
        /// border has resolved, which is what keeps an edge landing on the dot rather than a pixel
        /// beside it. Before the first layout the border resolves to zero and this returns exactly the
        /// registry's formula.
        /// </remarks>
        public Vector2 GetPortAnchor(CanvasPortSide side, int index)
        {
            var originX = m_X.IsSet ? m_X.Value : 0f;
            var originY = m_Y.IsSet ? m_Y.Value : 0f;
            var anchorX = side == CanvasPortSide.Input
                ? originX + resolvedStyle.borderLeftWidth
                : originX + m_Width - resolvedStyle.borderRightWidth;
            var headerOffset = m_Header.ClassListContains(Styles.hiddenUssClassName) ? 0f : headerHeight;
            var anchorY = originY + resolvedStyle.borderTopWidth + headerOffset + portsPaddingY
                + index * portRowHeight + portRowHeight * 0.5f;
            return new Vector2(anchorX, anchorY);
        }

        /// <summary>
        /// The height of the header and the port area, from the geometry constants, for a node that has
        /// not been laid out yet.
        /// </summary>
        internal float GetPortAreaBottom()
        {
            var rows = Mathf.Max(m_InputPorts.Count, m_OutputPorts.Count);
            var header = string.IsNullOrEmpty(m_Title.text) ? 0f : headerHeight;
            var ports = rows > 0 ? rows * portRowHeight + 2f * portsPaddingY : 0f;
            return header + ports;
        }

        void SetPorts(CanvasPortSide side, IEnumerable<CanvasPort> ports)
        {
            var list = side == CanvasPortSide.Input ? m_InputPorts : m_OutputPorts;
            var column = side == CanvasPortSide.Input ? m_InputColumn : m_OutputColumn;

            list.Clear();
            column.Clear();

            if (ports != null)
            {
                foreach (var port in ports)
                {
                    var index = list.Count;
                    list.Add(port);
                    column.hierarchy.Add(BuildPortRow(port, side, index));
                }
            }

            RefreshVisibility();
        }

        VisualElement BuildPortRow(CanvasPort port, CanvasPortSide side, int index)
        {
            // The tooltip sits on the row, as the registry's title does, so hovering the dot or the
            // label shows it: the tooltip lookup walks up from whichever one is picked.
            var row = new VisualElement { name = portRowUssClassName + "-" + index, tooltip = port.description };
            row.AddToClassList(portRowUssClassName);

            var hit = new VisualElement { name = portHitUssClassName };
            hit.AddToClassList(portHitUssClassName);

            var dot = new VisualElement { pickingMode = PickingMode.Ignore };
            dot.AddToClassList(portDotUssClassName);
            if (port.color.IsSet)
                dot.style.backgroundColor = port.color.Value;
            hit.hierarchy.Add(dot);

            var label = new LocalizedTextElement { text = port.name, pickingMode = PickingMode.Ignore };
            label.AddToClassList(portLabelUssClassName);
            label.EnableInClassList(Styles.hiddenUssClassName, string.IsNullOrEmpty(port.name));

            row.hierarchy.Add(hit);
            row.hierarchy.Add(label);

            hit.RegisterCallback<PointerDownEvent>(evt =>
            {
                evt.StopPropagation();
                portPointerDown?.Invoke(new CanvasPortEvent(this, port, side, index, evt.position));
            });
            hit.RegisterCallback<PointerUpEvent>(evt =>
            {
                evt.StopPropagation();
                portPointerUp?.Invoke(new CanvasPortEvent(this, port, side, index, evt.position));
            });
            hit.RegisterCallback<PointerEnterEvent>(evt =>
                portPointerEntered?.Invoke(new CanvasPortEvent(this, port, side, index, evt.position)));
            hit.RegisterCallback<PointerLeaveEvent>(evt =>
                portPointerLeft?.Invoke(new CanvasPortEvent(this, port, side, index, evt.position)));

            return row;
        }

        void RefreshVisibility()
        {
            var hasTitle = !string.IsNullOrEmpty(m_Title.text);
            var hasPorts = m_InputPorts.Count > 0 || m_OutputPorts.Count > 0;
            m_Header.EnableInClassList(Styles.hiddenUssClassName, !hasTitle);
            m_Ports.EnableInClassList(Styles.hiddenUssClassName, !hasPorts);
        }

        void RefreshPosition()
        {
            var positioned = m_X.IsSet || m_Y.IsSet;
            EnableInClassList(positionedUssClassName, positioned);
            style.left = m_X.IsSet ? m_X.Value : StyleKeyword.Null;
            style.top = m_Y.IsSet ? m_Y.Value : StyleKeyword.Null;
        }

        void OnPressed()
        {
            // A click is a selection request; the caller decides what selection means.
        }

        void OnDragStarted(Draggable draggable)
        {
            EnableInClassList(grabbingUssClassName, true);
            // The origin is captured in the parent's coordinates, which already carry the canvas'
            // zoom transform. That is why no zoom factor appears anywhere in this component.
            var local = ParentLocal(draggable.position);
            m_DragOrigin = local - new Vector2(m_X.IsSet ? m_X.Value : 0f, m_Y.IsSet ? m_Y.Value : 0f);
        }

        void OnDragged(Draggable draggable)
        {
            var next = ParentLocal(draggable.position) - m_DragOrigin;
            x = next.x;
            y = next.y;
            positionChanged?.Invoke(next);
        }

        void OnDragEnded(Draggable draggable)
        {
            EnableInClassList(grabbingUssClassName, false);
        }

        Vector2 ParentLocal(Vector2 worldPosition)
        {
            var reference = hierarchy.parent ?? this;
            return reference.WorldToLocal(worldPosition);
        }
    }
}
