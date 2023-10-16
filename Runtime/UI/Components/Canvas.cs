using System;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The direction of the scroll.
    /// </summary>
    public enum ScrollDirection
    {
        /// <summary>
        /// The natural scroll direction.
        /// </summary>
        Natural,
        
        /// <summary>
        /// The inversed scroll direction.
        /// </summary>
        Inverse,
    }
    
    /// <summary>
    /// The current Grab Mode.
    /// </summary>
    public enum GrabMode
    {
        /// <summary>
        /// No grab possible for the moment.
        /// </summary>
        None,
        
        /// <summary>
        /// Elements can be grabbed.
        /// </summary>
        Grab,
        
        /// <summary>
        /// Elements are currently grabbed.
        /// </summary>
        Grabbing,
    }

    /// <summary>
    /// The current Canvas control scheme.
    /// </summary>
    public enum CanvasControlScheme
    {
        /// <summary>
        /// The default control scheme, similar to others Unity Editor tools.
        /// </summary>
        Editor,
        
        /// <summary>
        /// The alternate control scheme, similar to Figma, Sketch, etc.
        /// </summary>
        Modern,
    }

    /// <summary>
    /// A Canvas is a VisualElement that can be used to group other VisualElements.
    /// You can use it to create a scrollable area inside a window.
    /// </summary>
    public class Canvas : VisualElement
    {
        /// <summary>
        /// USS class name of elements of this type.
        /// </summary>
        public const string ussClassName = "appui-canvas";
        
        /// <summary>
        /// USS class name of the background element of this type.
        /// </summary>
        public static readonly string backgroundUssClassName = ussClassName + "__background";
        
        /// <summary>
        /// USS class name of the viewport element of this type.
        /// </summary>
        public static readonly string viewportUssClassName = ussClassName + "__viewport";
        
        /// <summary>
        /// USS class name of the viewport container element of this type.
        /// </summary>
        public static readonly string viewportContainerUssClassName = ussClassName + "__viewport-container";
        
        /// <summary>
        /// USS class name of the horizontal scroller element of this type.
        /// </summary>
        public static readonly string horizontalScrollerUssClassName = ussClassName + "__horizontal-scroller";
        
        /// <summary>
        /// USS class name of the vertical scroller element of this type.
        /// </summary>
        public static readonly string verticalScrollerUssClassName = ussClassName + "__vertical-scroller";
        
        const float k_DefaultScrollSpeed = 2f;
        
        const float k_DefaultMinZoom = 0.1f;
        
        const float k_DefaultMaxZoom = 100.0f;

        const float k_DefaultZoomSpeed = 0.075f;
        
        const float k_DefaultZoomMultiplier = 2f;
        
        const float k_DefaultPanMultiplier = 3f;

        const float k_DefaultFrameMargin = 12f;
        
        const bool k_DefaultUseSpaceBar = true;
        
        const ScrollDirection k_DefaultScrollDirection = ScrollDirection.Natural;
        
        const CanvasControlScheme k_DefaultControlScheme = CanvasControlScheme.Modern;

        readonly CanvasBackground m_Background;

        readonly VisualElement m_Viewport;
        
        readonly VisualElement m_ViewportContainer;
        
        readonly Scroller m_HorizontalScroller;
        
        readonly Scroller m_VerticalScroller;

        Vector3 m_PointerPosition;

        GrabMode m_GrabMode = GrabMode.None;

        bool m_UpdatingScrollers;

        Vector2 m_LastScrollersPosition;
        
        int m_PointerId = -1;
        
        bool m_SpaceBarPressed;

        /// <summary>
        /// The content container of the Canvas.
        /// </summary>
        public override VisualElement contentContainer => m_ViewportContainer;
        
        /// <summary>
        /// The container used for framing the Canvas.
        /// </summary>
        /// <remarks>
        /// The container rect value must be defined in the Canvas' local coordinates.
        /// </remarks>
        public Rect frameContainer { get; set; } = Rect.zero;
        
        /// <summary>
        /// The scroll coordinates of the Canvas.
        /// </summary>
        public Vector2 scrollOffset
        {
            get => m_Viewport.transform.position * -1;
            set
            {
                SetScrollOffset(value);
                UpdateScrollers();
            }
        }
        
        /// <summary>
        /// The scroll speed of the Canvas.
        /// </summary>
        public float scrollSpeed { get; set; } = k_DefaultScrollSpeed;
        
        /// <summary>
        /// The minimum zoom factor of the Canvas.
        /// </summary>
        public float minZoom { get; set; } = k_DefaultMinZoom;
        
        /// <summary>
        /// The maximum zoom factor of the Canvas.
        /// </summary>
        public float maxZoom { get; set; } = k_DefaultMaxZoom;
        
        /// <summary>
        /// The zoom speed of the Canvas.
        /// </summary>
        public float zoomSpeed { get; set; } = k_DefaultZoomSpeed;

        /// <summary>
        /// The zoom speed multiplier when Shift key is hold.
        /// </summary>
        public float zoomMultiplier { get; set; } = k_DefaultZoomMultiplier;
        
        /// <summary>
        /// The pan multiplier when Shift key is hold.
        /// </summary>
        public float panMultiplier { get; set; } = k_DefaultPanMultiplier;

        /// <summary>
        /// The scroll direction of the Canvas. See <see cref="ScrollDirection"/> for more information.
        /// </summary>
        public ScrollDirection scrollDirection { get; set; } = k_DefaultScrollDirection;
        
        /// <summary>
        /// The zoom factor of the Canvas.
        /// </summary>
        public float zoom
        {
            get => m_Viewport.transform.scale.x;
            set
            {
                m_Viewport.transform.scale = new Vector3(value, value, 1);
                m_Background.scale = value;
                UpdateScrollers();
            }
        }
        
        /// <summary>
        /// The margin applied when framing the Canvas.
        /// </summary>
        public float frameMargin { get; set; } = k_DefaultFrameMargin;
        
        /// <summary>
        /// Whether the Canvas should use the Space bar to pan.
        /// </summary>
        public bool useSpaceBar { get; set; } = k_DefaultUseSpaceBar;

        /// <summary>
        /// The current grab state of the canvas (to pan).
        /// </summary>
        public GrabMode grabMode
        {
            get => m_GrabMode;
            private set
            {
                if (m_GrabMode == value)
                    return;
                
                RemoveFromClassList("cursor--" + m_GrabMode.ToString().ToLower());
                m_GrabMode = value;
                AddToClassList("cursor--" + m_GrabMode.ToString().ToLower());
            }
        }
        
        /// <summary>
        /// The current control scheme of the canvas.
        /// </summary>
        public CanvasControlScheme controlScheme { get; set; } = k_DefaultControlScheme;

        /// <summary>
        /// Instantiates a <see cref="Canvas"/> element.
        /// </summary>
        public Canvas()
        {
            AddToClassList(ussClassName);
            pickingMode = PickingMode.Position;
            focusable = true;

            m_Background = new CanvasBackground {name = backgroundUssClassName, pickingMode = PickingMode.Ignore};
            m_Background.AddToClassList(backgroundUssClassName);
            hierarchy.Add(m_Background);
            
            m_Viewport = new VisualElement
            {
                name = viewportUssClassName, 
                pickingMode = PickingMode.Ignore, 
                usageHints = UsageHints.DynamicTransform
            };
            m_Viewport.AddToClassList(viewportUssClassName);
            hierarchy.Add(m_Viewport);
            
            m_ViewportContainer = new VisualElement
            {
                name = viewportContainerUssClassName, 
                pickingMode = PickingMode.Ignore, 
                usageHints = UsageHints.GroupTransform
            };
            m_ViewportContainer.AddToClassList(viewportContainerUssClassName);
            m_Viewport.hierarchy.Add(m_ViewportContainer);

            m_VerticalScroller = new Scroller
            {
                name = verticalScrollerUssClassName,
                direction = SliderDirection.Vertical
            };
            m_VerticalScroller.AddToClassList(verticalScrollerUssClassName);
            m_VerticalScroller.slider.RegisterValueChangedCallback(OnVerticalScrollValueChanged);
            m_VerticalScroller.slider.RegisterCallback<PointerCaptureEvent>(OnScrollerPointerCapture);
            m_VerticalScroller.slider.RegisterCallback<PointerCaptureOutEvent>(OnScrollerPointerCaptureOut);
            hierarchy.Add(m_VerticalScroller);
            
            m_HorizontalScroller = new Scroller
            {
                name = horizontalScrollerUssClassName,
                direction = SliderDirection.Horizontal
            };
            m_HorizontalScroller.AddToClassList(horizontalScrollerUssClassName);
            m_HorizontalScroller.slider.RegisterValueChangedCallback(OnHorizontalScrollValueChanged);
            m_HorizontalScroller.slider.RegisterCallback<PointerCaptureEvent>(OnScrollerPointerCapture);
            m_HorizontalScroller.slider.RegisterCallback<PointerCaptureOutEvent>(OnScrollerPointerCaptureOut);
            hierarchy.Add(m_HorizontalScroller);
            
            RegisterCallback<WheelEvent>(OnWheel);
            RegisterCallback<PointerDownEvent>(OnPointerDown);
            RegisterCallback<PointerUpEvent>(OnPointerUp);
            RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
            RegisterCallback<PointerMoveEvent>(OnPointerMove);
            RegisterCallback<KeyDownEvent>(OnKeyDown);
            RegisterCallback<KeyUpEvent>(OnKeyUp);
            RegisterCallback<FocusOutEvent>(OnFocusOut);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        /// <summary>
        /// Frame the Canvas to the given world area. The area is in world coordinates.
        /// </summary>
        /// <param name="worldRect"> The area to frame. </param>
        public void FrameWorldRect(Rect worldRect)
        {
            if (worldRect.size.sqrMagnitude == 0)
                return;
            
            var container = frameContainer == Rect.zero ? contentRect : frameContainer;
            var containerCenter = new Vector2(container.width * 0.5f, container.height * 0.5f) + container.position;
            
            var localRect = this.WorldToLocal(worldRect);
            var zoomRatio = Mathf.Min(
                (container.width - frameMargin * 2f) / localRect.width, 
                (container.height - frameMargin * 2f) / localRect.height);
            var newZoom = zoom * zoomRatio;

            var centerDelta = localRect.center - containerCenter;
            scrollOffset += centerDelta;
            
            var zoomDelta = newZoom - zoom;
            ApplyZoom(containerCenter, zoomDelta);
        }
        
        /// <summary>
        /// Frame the Canvas to the given area. The area is in the Viewport's local coordinates.
        /// </summary>
        /// <param name="viewportArea"> The area to frame. </param>
        public void FrameArea(Rect viewportArea)
        {
            if (viewportArea.size.sqrMagnitude == 0)
                return;

            var worldRect = m_Viewport.LocalToWorld(viewportArea);
            FrameWorldRect(worldRect);
        }

        /// <summary>
        /// Frame the Canvas to the given element. The element is in the Viewport's local coordinates.
        /// </summary>
        /// <param name="element"> The element to frame. </param>
        public void FrameElement(VisualElement element)
        {
            if (element == null)
                return;
            
            var boundingBox = element.GetWorldBoundingBox();
            if (boundingBox.size.sqrMagnitude == 0)
                return;
            
            FrameArea(m_Viewport.WorldToLocal(boundingBox));
        }

        /// <summary>
        /// Frame the Canvas to see all elements.
        /// </summary>
        public void FrameAll()
        {
            FrameElement(m_ViewportContainer);
        }

        void OnKeyUp(KeyUpEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Space:
                {
                    if (controlScheme != CanvasControlScheme.Editor && useSpaceBar)
                    {
                        m_SpaceBarPressed = false;
                        grabMode = GrabMode.None;
                        if (m_PointerId >= 0 && this.HasPointerCapture(m_PointerId))
                            this.ReleasePointer(m_PointerId);
                    }
                    break;
                }
            }
        }
        
        void OnFocusOut(FocusOutEvent evt)
        {
            m_SpaceBarPressed = false;
            grabMode = GrabMode.None;
            m_PointerId = -1;
        }

        void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.F:
                    evt.StopImmediatePropagation();
                    FrameAll();
                    break;
                case KeyCode.Escape:
                {
                    if (m_PointerId >= 0)
                    {
                        evt.StopImmediatePropagation();
                        if (this.HasPointerCapture(m_PointerId))
                            this.ReleasePointer(m_PointerId);
                        m_PointerId = -1;
                    }
                    break;
                }
                case KeyCode.Space:
                    if (controlScheme != CanvasControlScheme.Editor && useSpaceBar)
                    {
                        evt.StopImmediatePropagation();
                        m_SpaceBarPressed = true;
                        grabMode = GrabMode.Grab;
                    }
                    break;
                case KeyCode.Minus when evt.actionKey:
                case KeyCode.KeypadMinus when evt.actionKey:
                    {
                        evt.StopImmediatePropagation();
                        // logarithmic zoom
                        var multiplier = evt.shiftKey ? zoomMultiplier : 1f;
                        var zoomDelta = zoomSpeed * -1f * multiplier;
                        zoomDelta = (zoom * Mathf.Pow(2f, zoomDelta)) - zoom;
                        ApplyZoom(new Vector2(contentRect.width * 0.5f, contentRect.height * 0.5f), zoomDelta);
                    }
                    break;
                case KeyCode.Plus when evt.actionKey:
                case KeyCode.Equals when evt.actionKey:
                case KeyCode.KeypadPlus when evt.actionKey:
                    {
                        evt.StopImmediatePropagation();
                        // logarithmic zoom
                        var multiplier = evt.shiftKey ? zoomMultiplier : 1f; 
                        var zoomDelta = zoomSpeed * 1f * multiplier;
                        zoomDelta = (zoom * Mathf.Pow(2f, zoomDelta)) - zoom;
                        ApplyZoom(new Vector2(contentRect.width * 0.5f, contentRect.height * 0.5f), zoomDelta);
                    }
                    break;
                case KeyCode.Alpha0 when evt.actionKey:
                case KeyCode.Keypad0 when evt.actionKey:
                    {
                        evt.StopImmediatePropagation();
                        ApplyZoom(new Vector2(contentRect.width * 0.5f, contentRect.height * 0.5f), 1f - zoom);
                    }
                    break;
            }
        }

        void OnPointerDown(PointerDownEvent evt)
        {
            if (panel == null || panel.GetCapturingElement(evt.pointerId) != null)
                return;

            var hasModifierPressed = controlScheme switch
            {
                CanvasControlScheme.Modern => m_SpaceBarPressed,
                CanvasControlScheme.Editor => evt.altKey,
                _ => m_SpaceBarPressed
            };
            
            if (evt.button == (int)MouseButton.MiddleMouse || 
                (evt.button == (int)MouseButton.LeftMouse && hasModifierPressed))
            {
                if (!this.HasPointerCapture(evt.pointerId))
                    this.CapturePointer(evt.pointerId);
                m_PointerId = evt.pointerId;
                m_PointerPosition = evt.localPosition;
                grabMode = GrabMode.Grabbing;
            }
        }

        void OnPointerUp(PointerUpEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
        }

        void OnPointerCancel(PointerCancelEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
                this.ReleasePointer(evt.pointerId);
        }

        void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.pointerId == m_PointerId)
            {
                m_PointerId = -1;
                grabMode = m_SpaceBarPressed ? GrabMode.Grab : GrabMode.None;
            }
        }

        void OnPointerMove(PointerMoveEvent evt)
        {
            if (this.HasPointerCapture(evt.pointerId))
            {
                grabMode = GrabMode.Grabbing;
                scrollOffset += (Vector2)(evt.localPosition - m_PointerPosition) *
                                (scrollDirection == ScrollDirection.Natural ? -1f : 1f);
            }
            
            m_PointerPosition = evt.localPosition;
        }

        void OnWheel(WheelEvent evt)
        {
            evt.PreventDefault();
            evt.StopImmediatePropagation();

            // no support of touchpad App UI events in Alternate control scheme
            if (controlScheme == CanvasControlScheme.Editor && evt.button == Core.AppUI.touchPadId)
                return;

            var shouldZoom = controlScheme switch
            {
                CanvasControlScheme.Modern => evt.ctrlKey || evt.commandKey,
                CanvasControlScheme.Editor => true,
                _ => evt.ctrlKey || evt.commandKey
            };
            
            if (shouldZoom)
            {
                var multiplier = evt.shiftKey ? zoomMultiplier : 1f;
                // logarithmic zoom
                var zoomDelta = zoomSpeed * -evt.delta.y * multiplier;
                zoomDelta = (zoom * Mathf.Pow(2f, zoomDelta)) - zoom;
                ApplyZoom(m_PointerPosition, zoomDelta);
            }
            else
            {
                var multiplier = evt.shiftKey ? panMultiplier : 1f;
                ApplyScrollOffset(evt.delta * multiplier * 2f);
            }
        }

        void OnVerticalScrollValueChanged(ChangeEvent<float> evt)
        {
            if (m_UpdatingScrollers)
                return;
            
            var delta = evt.newValue - m_LastScrollersPosition.y;
            SetScrollOffset(new Vector2(scrollOffset.x, scrollOffset.y + delta));
            m_LastScrollersPosition.y = evt.newValue;
        }
        
        void OnHorizontalScrollValueChanged(ChangeEvent<float> evt)
        {
            if (m_UpdatingScrollers)
                return;
            
            var delta = evt.newValue - m_LastScrollersPosition.x;
            SetScrollOffset(new Vector2(scrollOffset.x - delta, scrollOffset.y));
            m_LastScrollersPosition.x = evt.newValue;
        }

        void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (!evt.newRect.IsValid())
                return;

            UpdateScrollers();
        }
        
        void OnScrollerPointerCapture(PointerCaptureEvent evt)
        {
            if (evt.target == m_HorizontalScroller.slider || evt.target == m_VerticalScroller.slider)
            {
                m_LastScrollersPosition = new Vector2(m_HorizontalScroller.slider.value, m_VerticalScroller.slider.value);
            }
        }
        
        void OnScrollerPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (evt.target == m_HorizontalScroller.slider || evt.target == m_VerticalScroller.slider)
            {
                UpdateScrollers();
            }
        }

        void UpdateScrollers()
        {
            var viewportRect = contentRect;
            if (!viewportRect.IsValid())
                return;
            
            var canvasContentRect = this.WorldToLocal(m_ViewportContainer.GetWorldBoundingBox());
            
            // Shrinking the canvas content rect by 1 pixel on each side to avoid rounding issues
            canvasContentRect.xMin += 1;
            canvasContentRect.yMin += 1;
            canvasContentRect.xMax -= 1;
            canvasContentRect.yMax -= 1;
            
            // Encapsulating the canvas content rect by the viewport rect
            canvasContentRect.xMin = Mathf.Min(canvasContentRect.xMin, 0);
            canvasContentRect.yMin = Mathf.Min(canvasContentRect.yMin, 0);
            canvasContentRect.xMax = Mathf.Max(canvasContentRect.xMax, viewportRect.width);
            canvasContentRect.yMax = Mathf.Max(canvasContentRect.yMax, viewportRect.height);

            // Compute the ratio between the viewport and the canvas content size
            var xRatio = canvasContentRect.width > 0 ? viewportRect.width / canvasContentRect.width : 1f;
            xRatio = Mathf.Approximately(1f, xRatio) ? 1f : xRatio;
            var yRatio = canvasContentRect.height > 0 ? viewportRect.height / canvasContentRect.height : 1f;
            yRatio = Mathf.Approximately(1f, yRatio) ? 1f : yRatio;
            
            m_UpdatingScrollers = true;

            if (xRatio < 1f)
            {
                m_HorizontalScroller.lowValue = float.MinValue;
                m_HorizontalScroller.highValue = float.MaxValue;

                var min = 0;
                var max = Mathf.Max(min, canvasContentRect.width - viewportRect.width);
                var newValue = max - Mathf.Clamp(-canvasContentRect.xMin, min, max);
                
                m_HorizontalScroller.slider.SetValueWithoutNotify(newValue);
                m_HorizontalScroller.lowValue = min;
                m_HorizontalScroller.highValue = max;
            }

            if (yRatio < 1f)
            {
                m_VerticalScroller.lowValue = float.MinValue;
                m_VerticalScroller.highValue = float.MaxValue;
                
                var min = 0;
                var max = Mathf.Max(min, canvasContentRect.height - viewportRect.height);
                var newValue = Mathf.Clamp(-canvasContentRect.yMin, min, max);

                m_VerticalScroller.slider.SetValueWithoutNotify(newValue);
                m_VerticalScroller.lowValue = min;
                m_VerticalScroller.highValue = max;
            }
            
            m_HorizontalScroller.Adjust(xRatio);
            m_VerticalScroller.Adjust(yRatio);
            
            m_UpdatingScrollers = false;
        }

        void ApplyScrollOffset(Vector2 delta)
        {
            if (delta == Vector2.zero)
                return;
            
            var newScrollOffset = scrollOffset;
            newScrollOffset += (delta * scrollSpeed) * (scrollDirection == ScrollDirection.Natural ? -1f : 1f);
            scrollOffset = newScrollOffset;
        }

        void ApplyZoom(Vector2 pivot, float delta)
        {
            if (delta == 0)
                return;
            
            var newZoom = zoom;
            newZoom += delta;
            newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
            var zoomRatio = newZoom / zoom;
            zoom = newZoom;
                
            var newScrollOffset = scrollOffset;
            newScrollOffset = (newScrollOffset + pivot) * zoomRatio - pivot;
            scrollOffset = newScrollOffset;
        }

        void SetScrollOffset(Vector2 newValue)
        {
            m_Viewport.transform.position = new Vector3(-newValue.x, -newValue.y, 0);
            m_Background.offset = m_Viewport.transform.position;
        }

        /// <summary>
        /// Defines the UxmlFactory for the Canvas.
        /// </summary>
        [Preserve]
        public new class UxmlFactory : UxmlFactory<Canvas, UxmlTraits> { }
        
        /// <summary>
        /// Class containing the UXML traits for the <see cref="Canvas"/>.
        /// </summary>
        public new class UxmlTraits : VisualElementExtendedUxmlTraits
        {
            readonly UxmlFloatAttributeDescription m_ScrollSpeed = new UxmlFloatAttributeDescription
            {
                name = "scroll-speed", 
                defaultValue = k_DefaultScrollSpeed
            };
            
            readonly UxmlFloatAttributeDescription m_MinZoom = new UxmlFloatAttributeDescription
            {
                name = "min-zoom", 
                defaultValue = k_DefaultMinZoom
            };
            
            readonly UxmlFloatAttributeDescription m_MaxZoom = new UxmlFloatAttributeDescription
            {
                name = "max-zoom", 
                defaultValue = k_DefaultMaxZoom
            };

            readonly UxmlFloatAttributeDescription m_ZoomSpeed = new UxmlFloatAttributeDescription
            {
                name = "zoom-speed", 
                defaultValue = k_DefaultZoomSpeed
            };
            
            readonly UxmlFloatAttributeDescription m_ZoomMultiplier = new UxmlFloatAttributeDescription
            {
                name = "zoom-multiplier", 
                defaultValue = k_DefaultZoomMultiplier
            };
            
            readonly UxmlFloatAttributeDescription m_PanMultiplier = new UxmlFloatAttributeDescription
            {
                name = "pan-multiplier", 
                defaultValue = k_DefaultPanMultiplier
            };
            
            readonly UxmlFloatAttributeDescription m_FrameMargin = new UxmlFloatAttributeDescription
            {
                name = "frame-margin", 
                defaultValue = k_DefaultFrameMargin
            };
            
            readonly UxmlEnumAttributeDescription<ScrollDirection> m_ScrollDirection = new UxmlEnumAttributeDescription<ScrollDirection>
            {
                name = "scroll-direction", 
                defaultValue = k_DefaultScrollDirection
            };
            
            readonly UxmlEnumAttributeDescription<CanvasControlScheme> m_ControlScheme = new UxmlEnumAttributeDescription<CanvasControlScheme>
            {
                name = "control-scheme", 
                defaultValue = k_DefaultControlScheme
            };
            
            readonly UxmlBoolAttributeDescription m_UseSpaceBar = new UxmlBoolAttributeDescription
            {
                name = "use-space-bar", 
                defaultValue = k_DefaultUseSpaceBar
            };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                
                var canvas = (Canvas)ve;
                
                var floatVal = 0f;
                if (m_ScrollSpeed.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.scrollSpeed = floatVal;
                
                if (m_MinZoom.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.minZoom = floatVal;
                
                if (m_MaxZoom.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.maxZoom = floatVal;
                
                if (m_ZoomSpeed.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.zoomSpeed = floatVal;
                
                if (m_ZoomMultiplier.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.zoomMultiplier = floatVal;
                
                if (m_PanMultiplier.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.panMultiplier = floatVal;
                
                if (m_FrameMargin.TryGetValueFromBag(bag, cc, ref floatVal))
                    canvas.frameMargin = floatVal;
                
                var scrollDirectionVal = ScrollDirection.Natural;
                if (m_ScrollDirection.TryGetValueFromBag(bag, cc, ref scrollDirectionVal))
                    canvas.scrollDirection = scrollDirectionVal;
                
                var controlSchemeVal = CanvasControlScheme.Modern;
                if (m_ControlScheme.TryGetValueFromBag(bag, cc, ref controlSchemeVal))
                    canvas.controlScheme = controlSchemeVal;
                
                var boolVal = false;
                if (m_UseSpaceBar.TryGetValueFromBag(bag, cc, ref boolVal))
                    canvas.useSpaceBar = boolVal;
            }
        }
    }
}
