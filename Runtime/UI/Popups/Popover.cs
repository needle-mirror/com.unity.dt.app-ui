using System;
using Unity.AppUI.Bridge;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// Possible placements for a Popover.
    /// </summary>
    [Serializable]
    public enum PopoverPlacement
    {
        /// <summary>
        /// The Popover will be placed at the bottom center of the target.
        /// </summary>
        Bottom,

        /// <summary>
        /// The Popover will be placed at the bottom-left of the target.
        /// </summary>
        BottomLeft,

        /// <summary>
        /// The Popover will be placed at the bottom-right of the target.
        /// </summary>
        BottomRight,

        /// <summary>
        /// The Popover will be placed at the bottom-start of the target.
        /// </summary>
        BottomStart,

        /// <summary>
        /// The Popover will be placed at the bottom-end of the target.
        /// </summary>
        BottomEnd,

        /// <summary>
        /// The Popover will be placed at the top center of the target.
        /// </summary>
        Top,

        /// <summary>
        /// The Popover will be placed at the top-left of the target.
        /// </summary>
        TopLeft,

        /// <summary>
        /// The Popover will be placed at the top-right of the target.
        /// </summary>
        TopRight,

        /// <summary>
        /// The Popover will be placed at the top-start of the target.
        /// </summary>
        TopStart,

        /// <summary>
        /// The Popover will be placed at the top-end of the target.
        /// </summary>
        TopEnd,

        /// <summary>
        /// The Popover will be placed at the left center of the target.
        /// </summary>
        Left,

        /// <summary>
        /// The Popover will be placed at the left-top of the target.
        /// </summary>
        LeftTop,

        /// <summary>
        /// The Popover will be placed at the left-bottom of the target.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// The Popover will be placed at the start center of the target.
        /// </summary>
        Start,

        /// <summary>
        /// The Popover will be placed at the start-top of the target.
        /// </summary>
        StartTop,

        /// <summary>
        /// The Popover will be placed at the start-bottom of the target.
        /// </summary>
        StartBottom,

        /// <summary>
        /// The Popover will be placed at the right center of the target.
        /// </summary>
        Right,

        /// <summary>
        /// The Popover will be placed at the right-top of the target.
        /// </summary>
        RightTop,

        /// <summary>
        /// The Popover will be placed at the right-bottom of the target.
        /// </summary>
        RightBottom,

        /// <summary>
        /// The Popover will be placed at the end center of the target.
        /// </summary>
        End,

        /// <summary>
        /// The Popover will be placed at the end-top of the target.
        /// </summary>
        EndTop,

        /// <summary>
        /// The Popover will be placed at the end-bottom of the target.
        /// </summary>
        EndBottom,

        /// <summary>
        /// The Popover will be placed inside the target, at the top left.
        /// </summary>
        InsideTopStart,

        /// <summary>
        /// The Popover will be placed inside the target, at the top left.
        /// </summary>
        InsideTopLeft,

        /// <summary>
        /// The Popover will be placed inside the target, at the top center.
        /// </summary>
        InsideTop,

        /// <summary>
        /// The Popover will be placed inside the target, at the top right.
        /// </summary>
        InsideTopEnd,

        /// <summary>
        /// The Popover will be placed inside the target, at the bottom left.
        /// </summary>
        InsideBottomStart,

        /// <summary>
        /// The Popover will be placed inside the target, at the bottom center.
        /// </summary>
        InsideBottom,

        /// <summary>
        /// The Popover will be placed inside the target, at the bottom right.
        /// </summary>
        InsideBottomEnd,

        /// <summary>
        /// The Popover will be placed inside the target, at the center left.
        /// </summary>
        InsideStart,

        /// <summary>
        /// The Popover will be placed inside the target, at the center right.
        /// </summary>
        InsideEnd,

        /// <summary>
        /// The Popover will be placed inside the target, at the center.
        /// </summary>
        InsideCenter,
    }

    /// <summary>
    /// The position result data structure returned in <see cref="AnchorPopupUtils.ComputePosition"/> utility method.
    /// </summary>
    public struct PositionResult
    {
        /// <summary>
        /// The Y Position from the top, in pixels.
        /// </summary>
        public float top { get; set; }

        /// <summary>
        /// The X Position from the left, in pixels.
        /// </summary>
        public float left { get; set; }

        /// <summary>
        /// The top margin, in pixels.
        /// </summary>
        public float marginTop { get; set; }

        /// <summary>
        /// The left margin, in pixels.
        /// </summary>
        public float marginLeft { get; set; }

        /// <summary>
        /// The computed placement, that may differ from the desired one.
        /// </summary>
        public PopoverPlacement finalPlacement { get; set; }

        /// <summary>
        /// The USS left value for the tip element.
        /// </summary>
        public float tipLeft { get; set; }

        /// <summary>
        /// The USS right value for the tip element.
        /// </summary>
        public float tipRight { get; set; }

        /// <summary>
        /// The USS top value for the tip element.
        /// </summary>
        public float tipTop { get; set; }

        /// <summary>
        /// The USS bottom value for the tip element.
        /// </summary>
        public float tipBottom { get; set; }
    }

    /// <summary>
    /// Options to pass as argument to <see cref="AnchorPopupUtils.ComputePosition"/> utility method.
    /// </summary>
    public struct PositionOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="preferredPlacement"> The preferred placement for the popover.</param>
        /// <param name="offset"> The offset from the anchor element.</param>
        /// <param name="crossOffset"> The cross offset from the anchor element.</param>
        /// <param name="shouldFlip"> Whether the popover should flip if it doesn't fit in the viewport.</param>
        /// <param name="crossSnap"> Whether the popover should snap not to go outside the viewport.</param>
        public PositionOptions(PopoverPlacement preferredPlacement = PopoverPlacement.Top, int offset = 0, int crossOffset = 0, bool shouldFlip = true, bool crossSnap = true)
        {
            favoritePlacement = preferredPlacement;
            this.offset = offset;
            this.crossOffset = crossOffset;
            this.shouldFlip = shouldFlip;
            this.crossSnap = crossSnap;
        }

        /// <summary>
        /// The preferred placement for the popover.
        /// </summary>
        public PopoverPlacement favoritePlacement { get; set; }

        /// <summary>
        /// The offset from the anchor element.
        /// </summary>
        public int offset { get; set; }

        /// <summary>
        /// The cross offset from the anchor element.
        /// </summary>
        public int crossOffset { get; set; }

        /// <summary>
        /// Whether the popover should flip if it doesn't fit in the viewport.
        /// </summary>
        public bool shouldFlip { get; set; }

        /// <summary>
        /// Whether the popover should snap not to go outside the viewport.
        /// </summary>
        public bool crossSnap { get; set; }
    }

    /// <summary>
    /// A popup usually anchored to another UI element.
    /// </summary>
    public sealed class Popover : AnchorPopup<Popover>
    {
        bool m_HasBeenManuallyMoved;

        bool m_HasBeenManuallyResized;

        /// <summary>
        /// Enable or disable the blocking of outside click events.
        /// </summary>
        public bool modalBackdrop
        {
            get => popover.modalBackdrop;
            set => popover.modalBackdrop = value;
        }

        /// <summary>
        /// `True` if the popup is movable by dragging, `False` otherwise. Default is `False`.
        /// </summary>
        /// <remarks>
        /// When the Popup is set to movable, it can be dragged by clicking and dragging the popover element.
        /// </remarks>
        public bool movable
        {
            get => popover.movable;
            set => popover.movable = value;
        }

        /// <summary>
        /// `True` if the popup is resizable, `False` otherwise. Default is `False`.
        /// </summary>
        /// <remarks>
        /// When the Popup is set to resizable, it will be resizable by dragging the bottom right corner.
        /// </remarks>
        public bool resizable
        {
            get => popover.resizable;
            set => popover.resizable = value;
        }

        /// <summary>
        /// The direction of the drag.
        /// </summary>
        public Draggable.DragDirection resizeDirection
        {
            get => popover.resizeHandle.dragDirection;
            set => popover.resizeHandle.dragDirection = value;
        }

        /// <summary>
        /// Optional callback invoked during resize to modify the size before it is applied to the target.
        /// The first parameter is the computed new size, the second is the original size at drag start.
        /// The callback must return the adjusted size to apply.
        /// </summary>
        public Func<Vector2, Vector2, Vector2> resizeSizeModifier
        {
            get => popover.resizeHandle.sizeModifier;
            set => popover.resizeHandle.sizeModifier = value;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="referenceView">The view used as context provider for the Popover.</param>
        /// <param name="popover">The popup visual element itself.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        Popover(VisualElement referenceView, PopoverVisualElement popover, VisualElement contentView)
            : base(referenceView, popover, contentView)
        {
            popover.moveStarted += OnMoveStarted;
            popover.resizeHandle.resizeStarted += OnResizeStarted;
        }

        PopoverVisualElement popover => (PopoverVisualElement)view;

        void OnMoveStarted()
        {
            m_HasBeenManuallyMoved = true;
        }

        void OnResizeStarted(ResizeHandle handle)
        {
            m_HasBeenManuallyResized = true;
        }

        /// <summary>
        /// Build a new <see cref="Popover"/> instance.
        /// </summary>
        /// <param name="referenceView">An arbitrary UI element in the current panel.</param>
        /// <param name="contentView">The content that will appear inside this popup.</param>
        /// <returns>The <see cref="Popover"/> instance.</returns>
        /// <exception cref="ArgumentNullException">If the referenceView is null.</exception>
        public static Popover Build(VisualElement referenceView, VisualElement contentView)
        {
            if (referenceView == null)
                throw new ArgumentNullException(nameof(referenceView));

            var popoverVisualElement = new PopoverVisualElement(contentView);
            var popoverElement = new Popover(referenceView, popoverVisualElement, contentView)
                .SetAnchor(referenceView)
                .SetLastFocusedElement(referenceView);
            return popoverElement;
        }

        void OnWheel(WheelEvent evt)
        {
            if (outsideScrollEnabled)
                return;

            var inside = GetMovableElement().worldBound.Contains((Vector2)evt.mousePosition);
            if (!inside)
                evt.StopImmediatePropagation();
        }

        void OnTreeDown(PointerDownEvent evt)
        {
            if (!outsideClickDismissEnabled || outsideClickStrategy == 0 || view.parent == null)
                return;

            var index = view.parent.IndexOf(view);
            if (index != view.parent.childCount - 1)
                return;

            var shouldDismiss = true;
            if ((outsideClickStrategy & OutsideClickStrategy.Bounds) != 0)
                shouldDismiss = !GetMovableElement().worldBound.Contains((Vector2)evt.position);

            if (shouldDismiss && (outsideClickStrategy & OutsideClickStrategy.Pick) != 0)
            {
                var picked = view.panel.Pick(evt.position);
                var commonAncestor = picked?.FindCommonAncestor(view);
                if (commonAncestor == view) // if the picked element is a child of the popover, don't dismiss
                    shouldDismiss = false;
            }

            if (!shouldDismiss)
                return;

            var insideAnchor = anchor?.worldBound.Contains((Vector2)evt.position) ?? false;
            var insideLastFocusedElement = (m_LastFocusedElement as VisualElement)?.worldBound.Contains((Vector2)evt.position) ?? false;
            if (insideAnchor || insideLastFocusedElement || outsideClickDismissStopsEventPropagation)
            {
                // prevent reopening the same popover again...
                evt.StopImmediatePropagation();
            }
            Dismiss(DismissType.OutOfBounds);
        }

        /// <inheritdoc />
        protected override bool ShouldRefreshPosition()
        {
            return !m_HasBeenManuallyMoved && !m_HasBeenManuallyResized;
        }

        /// <summary>
        /// Enable or disable the blocking of outside click events.
        /// </summary>
        /// <param name="enableModalBackdrop"> Whether to enable the blocking of outside click events.</param>
        /// <returns> The <see cref="Popover"/> instance.</returns>
        public Popover SetModalBackdrop(bool enableModalBackdrop)
        {
            modalBackdrop = enableModalBackdrop;
            return this;
        }

        /// <summary>
        /// Set the popup as movable by dragging.
        /// </summary>
        /// <param name="isMovable"> `True` to activate the feature, `False` otherwise.</param>
        /// <returns> The popover instance.</returns>
        /// <seealso cref="movable"/>
        public Popover SetMovable(bool isMovable)
        {
            if (!isMovable || !movable)
                m_HasBeenManuallyMoved = false;
            movable = isMovable;
            return this;
        }

        /// <summary>
        /// Set the popup as resizable.
        /// </summary>
        /// <param name="isResizable"> `True` to activate the feature, `False` otherwise.</param>
        /// <returns> The popover instance.</returns>
        /// <seealso cref="resizable"/>
        public Popover SetResizable(bool isResizable)
        {
            if (!isResizable || !resizable)
                m_HasBeenManuallyResized = false;
            resizable = isResizable;
            return this;
        }

        /// <summary>
        /// Set the resize direction of the popover.
        /// </summary>
        /// <param name="direction"> The direction of the resize.</param>
        /// <returns> The popover instance.</returns>
        public Popover SetResizeDirection(Draggable.DragDirection direction)
        {
            resizeDirection = direction;
            return this;
        }

        /// <summary>
        /// Set a callback to modify the size during resize before it is applied to the target.
        /// </summary>
        /// <param name="modifier"> A function that receives the computed new size and the original size at drag start,
        /// and returns the adjusted size. Pass <c>null</c> to remove any previously set modifier.</param>
        /// <returns> The popover instance.</returns>
        /// <seealso cref="resizeSizeModifier"/>
        public Popover SetResizeSizeModifier(Func<Vector2, Vector2, Vector2> modifier)
        {
            resizeSizeModifier = modifier;
            return this;
        }

        /// <summary>
        /// Set the position of the popover programmatically.
        /// </summary>
        /// <remarks>
        /// This method only works when <see cref="movable"/> is <c>true</c>.
        /// The position is clamped to the container bounds.
        /// Calling this method sets the manual move flag, which prevents automatic position refresh.
        /// </remarks>
        /// <param name="position"> The desired position (left, top) in pixels.</param>
        /// <returns> The popover instance.</returns>
        public Popover SetPosition(Vector2 position)
        {
            if (!movable)
                return this;

            m_HasBeenManuallyMoved = true;
            popover.SetPosition(position);
            return this;
        }

        /// <summary>
        /// Set the size of the popover content programmatically.
        /// </summary>
        /// <remarks>
        /// This method only works when <see cref="resizable"/> is <c>true</c>.
        /// The size is clamped so the popover fits within the container bounds.
        /// Calling this method sets the manual resize flag, which prevents automatic position refresh.
        /// </remarks>
        /// <param name="size"> The desired size (width, height) in pixels.</param>
        /// <returns> The popover instance.</returns>
        public Popover SetSize(Vector2 size)
        {
            if (!resizable)
                return this;

            m_HasBeenManuallyResized = true;
            popover.SetSize(size);
            return this;
        }

        /// <inheritdoc cref="Popup.GetFocusableElement"/>
        protected override VisualElement GetFocusableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc cref="AnchorPopup{T}.GetMovableElement"/>
        public override VisualElement GetMovableElement()
        {
            return popover.popoverElement;
        }

        /// <inheritdoc />
        protected override bool ShouldAnimate() => base.ShouldAnimate();

        /// <inheritdoc />
        protected override void InvokeShownEventHandlers()
        {
            base.InvokeShownEventHandlers();
            rootView?.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            rootView?.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
        }

        /// <inheritdoc />
        protected override void HideView(DismissType reason)
        {
            rootView?.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            rootView?.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
            base.HideView(reason);
        }

        /// <summary>
        /// Ensure that the event handlers are registered. This is useful when the popover is used in a smir and the root view can change.
        /// </summary>
        public void EnsureEventHandlersRegistered()
        {
            if (rootView == null)
                return;

            rootView.UnregisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            rootView.UnregisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);

            rootView.RegisterCallback<PointerDownEvent>(OnTreeDown, TrickleDown.TrickleDown);
            rootView.RegisterCallback<WheelEvent>(OnWheel, TrickleDown.TrickleDown);
        }

        /// <summary>
        /// The UI element used as a Popover.
        /// </summary>
        internal class PopoverVisualElement : VisualElement, IPlaceableElement
        {
            public const string ussClassName = "appui-popover";

            public const string modalBackdropUssClassName = ussClassName + "--modal-backdrop";

            public const string popoverUssClassName = ussClassName + "__popover";

            public const string containerUssClassName = ussClassName + "__container";

            public const string shadowElementUssClassName = ussClassName + "__shadow-element";

            public const string tipUssClassName = ussClassName + "__tip";

            public const string resizeHandleUssClassName = ussClassName + "__resize-handle";

            public const string upUssClassName = ussClassName + "--up";

            public const string downUssClassName = ussClassName + "--down";

            public const string leftUssClassName = ussClassName + "--left";

            public const string rightUssClassName = ussClassName + "--right";

            public const string resizableUssClassName = ussClassName + "--resizable";

            public const string movableUssClassName = ussClassName + "--movable";

            readonly VisualElement m_ContentContainer;

            readonly VisualElement m_ResizeContent;

            PopoverPlacement m_Placement = PopoverPlacement.Top;

            Dragger m_Dragger;

            Vector2 m_DragStartLeft;

            Vector2 m_DragStartPointer;

            /// <summary>
            /// Event invoked when the user starts moving the popover by dragging.
            /// </summary>
            public event Action moveStarted;

            /// <summary>
            /// The resize handle of the popover.
            /// </summary>
            public ResizeHandle resizeHandle { get; }

            /// <summary>
            /// Default constructor.
            /// </summary>
            /// <param name="content">The content of the popup.</param>
            public PopoverVisualElement(VisualElement content)
            {
                AddToClassList(ussClassName);

                modalBackdrop = false;

                popoverElement = new VisualElement
                {
                    name = popoverUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = true,
                };
                popoverElement.SetIsCompositeRoot(true);
                popoverElement.SetExcludeFromFocusRing(true);
                popoverElement.EnableDynamicTransform(true);
                popoverElement.AddToClassList(popoverUssClassName);
                hierarchy.Add(popoverElement);

                var shadowElement = new ExVisualElement
                {
                    name = shadowElementUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = false,
                    passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.OutsetShadows
                };
                shadowElement.AddToClassList(shadowElementUssClassName);
                popoverElement.hierarchy.Add(shadowElement);

                tipElement = new VisualElement { name = tipUssClassName, pickingMode = PickingMode.Ignore, focusable = false };
                tipElement.AddToClassList(tipUssClassName);
                popoverElement.hierarchy.Add(tipElement);

                m_ContentContainer = new VisualElement
                {
                    name = containerUssClassName,
                    pickingMode = PickingMode.Ignore,
                    focusable = false,
                };
                m_ContentContainer.AddToClassList(containerUssClassName);
                popoverElement.hierarchy.Add(m_ContentContainer);

                m_ContentContainer.Add(content);

                // Reset any inline size styles from a previous resize so the layout is determined by stylesheets
                content.style.width = StyleKeyword.Null;
                content.style.height = StyleKeyword.Null;

                resizeHandle = new ResizeHandle(content)
                {
                    name = resizeHandleUssClassName,
                    pickingMode = PickingMode.Position,
                    focusable = false,
                };
                resizeHandle.AddToClassList(resizeHandleUssClassName);
                m_ContentContainer.hierarchy.Add(resizeHandle);

                m_ResizeContent = content;
                popoverElement.RegisterCallback<GeometryChangedEvent>(OnPopoverGeometryChanged);

                resizable = false;
                movable = false;

                // auto-set delegate focus if there are focusable elements in the content
                foreach (var element in m_ContentContainer.Query().Build())
                {
                    if (element is {focusable: true, delegatesFocus: false} && element.resolvedStyle.display != DisplayStyle.None)
                    {
                        popoverElement.delegatesFocus = true;
                        break;
                    }
                }

                RefreshPlacement();
            }

            /// <summary>
            /// The popover UI element.
            /// <remarks>This is the real popover element that needs to be anchored. Its parent is usually a smir.</remarks>
            /// </summary>
            public VisualElement popoverElement { get; }

            /// <summary>
            /// The tip UI element.
            /// </summary>
            public VisualElement tipElement { get; }

            /// <inheritdoc />
            public override VisualElement contentContainer => m_ContentContainer;

            /// <summary>
            /// The popup placement, used to display the arrow at the right place.
            /// </summary>
            public PopoverPlacement placement
            {
                get => m_Placement;
                set
                {
                    m_Placement = value;
                    RefreshPlacement();
                }
            }

            /// <summary>
            /// Whether the popover has a modal backdrop.
            /// </summary>
            public bool modalBackdrop
            {
                get => ClassListContains(modalBackdropUssClassName);
                set
                {
                    EnableInClassList(modalBackdropUssClassName, value);
                    pickingMode = value ? PickingMode.Position : PickingMode.Ignore;
                }
            }

            /// <summary>
            /// Whether the popover is resizable.
            /// </summary>
            public bool resizable
            {
                get => ClassListContains(resizableUssClassName);
                set => EnableInClassList(resizableUssClassName, value);
            }

            /// <summary>
            /// Whether the popover is movable by dragging.
            /// </summary>
            public bool movable
            {
                get => ClassListContains(movableUssClassName);
                set
                {
                    var changed = movable != value;
                    EnableInClassList(movableUssClassName, value);
                    if (!changed)
                        return;

                    if (value)
                    {
                        m_Dragger = new Dragger(OnDragStarted, OnDragging, OnDragEnded, OnDragCanceled);
                        popoverElement.pickingMode = PickingMode.Position;
                        popoverElement.AddManipulator(m_Dragger);
                    }
                    else if (m_Dragger != null)
                    {
                        popoverElement.RemoveManipulator(m_Dragger);
                        popoverElement.pickingMode = PickingMode.Ignore;
                        m_Dragger = null;
                    }
                }
            }

            void OnDragStarted(PointerMoveEvent evt)
            {
                m_DragStartLeft = new Vector2(popoverElement.resolvedStyle.left, popoverElement.resolvedStyle.top);
                m_DragStartPointer = (Vector2)evt.position;
                moveStarted?.Invoke();
            }

            void OnDragging(PointerMoveEvent evt)
            {
                var delta = (Vector2)evt.position - m_DragStartPointer;
                var newLeft = m_DragStartLeft.x + delta.x;
                var newTop = m_DragStartLeft.y + delta.y;

                ClampToContainerBounds(ref newLeft, ref newTop);

                popoverElement.style.left = newLeft;
                popoverElement.style.top = newTop;
            }

            void OnDragEnded(PointerUpEvent evt)
            {
                // drag ended, nothing special to do
            }

            void OnDragCanceled()
            {
                popoverElement.style.left = m_DragStartLeft.x;
                popoverElement.style.top = m_DragStartLeft.y;
            }

            void ClampToContainerBounds(ref float left, ref float top)
            {
                var container = parent;
                if (container == null)
                    return;

                var containerBounds = container.contentRect;
                var popoverWidth = popoverElement.resolvedStyle.width;
                var popoverHeight = popoverElement.resolvedStyle.height;

                if (float.IsNaN(popoverWidth) || float.IsNaN(popoverHeight))
                    return;

                var minLeft = containerBounds.xMin;
                var maxLeft = containerBounds.xMax - popoverWidth;
                var minTop = containerBounds.yMin;
                var maxTop = containerBounds.yMax - popoverHeight;

                left = Mathf.Clamp(left, minLeft, Mathf.Max(minLeft, maxLeft));
                top = Mathf.Clamp(top, minTop, Mathf.Max(minTop, maxTop));
            }

            /// <summary>
            /// Set the position of the popover element, clamped to the container bounds.
            /// </summary>
            /// <param name="position"> The desired position (left, top) in pixels.</param>
            public void SetPosition(Vector2 position)
            {
                var left = position.x;
                var top = position.y;
                ClampToContainerBounds(ref left, ref top);
                popoverElement.style.left = left;
                popoverElement.style.top = top;
            }

            /// <summary>
            /// Set the size of the resize content element, clamped so the popover fits within the container.
            /// </summary>
            /// <param name="size"> The desired size (width, height) in pixels.</param>
            public void SetSize(Vector2 size)
            {
                if (m_ResizeContent == null)
                    return;

                var container = parent;
                if (container == null)
                {
                    m_ResizeContent.style.width = size.x;
                    m_ResizeContent.style.height = size.y;
                    return;
                }

                m_ResizeContent.style.width = size.x;
                m_ResizeContent.style.height = size.y;

                // Clamping will be handled by OnPopoverGeometryChanged when the layout updates
            }

            void OnPopoverGeometryChanged(GeometryChangedEvent evt)
            {
                var container = parent;
                if (container == null || m_ResizeContent == null)
                    return;

                var containerBounds = container.contentRect;
                var popoverLeft = popoverElement.resolvedStyle.left;
                var popoverTop = popoverElement.resolvedStyle.top;
                var popoverWidth = evt.newRect.width;
                var popoverHeight = evt.newRect.height;

                if (float.IsNaN(popoverWidth) || float.IsNaN(popoverHeight) ||
                    float.IsNaN(popoverLeft) || float.IsNaN(popoverTop))
                    return;

                // Clamp the content size so the popover fits within the container
                var maxWidth = containerBounds.width - popoverLeft;
                var maxHeight = containerBounds.height - popoverTop;

                var needClampW = popoverWidth > maxWidth && maxWidth > 0;
                var needClampH = popoverHeight > maxHeight && maxHeight > 0;

                if (!needClampW && !needClampH)
                    return;

                var contentWidth = m_ResizeContent.resolvedStyle.width;
                var contentHeight = m_ResizeContent.resolvedStyle.height;
                var clampedWidth = needClampW ? contentWidth - (popoverWidth - maxWidth) : contentWidth;
                var clampedHeight = needClampH ? contentHeight - (popoverHeight - maxHeight) : contentHeight;

                if (resizeHandle.sizeModifier != null)
                {
                    var currentSize = new Vector2(contentWidth, contentHeight);
                    var clampedSize = new Vector2(clampedWidth, clampedHeight);
                    var adjusted = resizeHandle.sizeModifier.Invoke(clampedSize, currentSize);
                    m_ResizeContent.style.width = adjusted.x;
                    m_ResizeContent.style.height = adjusted.y;
                }
                else
                {
                    if (needClampW)
                        m_ResizeContent.style.width = clampedWidth;
                    if (needClampH)
                        m_ResizeContent.style.height = clampedHeight;
                }
            }

            void RefreshPlacement()
            {
                bool up = false, down = false, left = false, right = false;

                switch (m_Placement)
                {
                    case PopoverPlacement.Bottom:
                    case PopoverPlacement.BottomLeft:
                    case PopoverPlacement.BottomRight:
                    case PopoverPlacement.BottomStart:
                    case PopoverPlacement.BottomEnd:
                    case PopoverPlacement.InsideTopStart:
                    case PopoverPlacement.InsideTopLeft:
                    case PopoverPlacement.InsideTop:
                    case PopoverPlacement.InsideTopEnd:
                        up = true;
                        break;
                    case PopoverPlacement.Top:
                    case PopoverPlacement.TopLeft:
                    case PopoverPlacement.TopRight:
                    case PopoverPlacement.TopStart:
                    case PopoverPlacement.TopEnd:
                    case PopoverPlacement.InsideBottomStart:
                    case PopoverPlacement.InsideBottom:
                    case PopoverPlacement.InsideBottomEnd:
                        down = true;
                        break;
                    case PopoverPlacement.Left:
                    case PopoverPlacement.LeftTop:
                    case PopoverPlacement.LeftBottom:
                    case PopoverPlacement.Start:
                    case PopoverPlacement.StartTop:
                    case PopoverPlacement.StartBottom:
                    case PopoverPlacement.InsideEnd:
                        right = true;
                        break;
                    case PopoverPlacement.Right:
                    case PopoverPlacement.RightTop:
                    case PopoverPlacement.RightBottom:
                    case PopoverPlacement.End:
                    case PopoverPlacement.EndTop:
                    case PopoverPlacement.EndBottom:
                    case PopoverPlacement.InsideStart:
                        left = true;
                        break;
                    case PopoverPlacement.InsideCenter:
                        break;
                    default:
                        throw new ValueOutOfRangeException(nameof(m_Placement), m_Placement);
                }

                EnableInClassList(upUssClassName, up);
                EnableInClassList(downUssClassName, down);
                EnableInClassList(leftUssClassName, left);
                EnableInClassList(rightUssClassName, right);
            }
        }
    }
}
