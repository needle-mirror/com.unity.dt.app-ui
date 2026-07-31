using System;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.Properties;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// An element that can be dragged to resize another element.
    /// </summary>
    [UxmlElement]
    public partial class ResizeHandle : VisualElement
    {

        internal static readonly BindingId dragDirectionProperty = nameof(dragDirection);

        internal static readonly BindingId thresholdProperty = nameof(threshold);


        /// <summary>
        /// Event triggered when the resize operation starts.
        /// </summary>
        public event Action<ResizeHandle> resizeStarted;

        /// <summary>
        /// Event triggered when the resize operation ends.
        /// </summary>
        public event Action<ResizeHandle> resizeEnded;

        /// <summary>
        /// The ResizeHandle main styling class.
        /// </summary>
        public const string ussClassName = "appui-resize-handle";

        /// <summary>
        /// The ResizeHandle variant styling class.
        /// </summary>
        [EnumName("GetDirectionUssClassName", typeof(Draggable.DragDirection))]
        public const string variantUssClassName = ussClassName + "--";

        readonly Draggable m_Draggable;

        Vector2 m_StartSize;

        bool m_Resizing;

        /// <summary>
        /// The direction in which the handle can be dragged.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Draggable.DragDirection dragDirection
        {
            get => m_Draggable.dragDirection;
            set
            {
                var changed = m_Draggable.dragDirection != value;
                RemoveFromClassList(GetDirectionUssClassName(m_Draggable.dragDirection));
                m_Draggable.dragDirection = value;
                AddToClassList(GetDirectionUssClassName(value));

                if (changed)
                    NotifyPropertyChanged(in dragDirectionProperty);
            }
        }

        /// <summary>
        /// The threshold in pixels that the handle must be dragged before it starts to move.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public float threshold
        {
            get => m_Draggable.threshold;
            set
            {
                var changed = !Mathf.Approximately(m_Draggable.threshold, value);
                m_Draggable.threshold = value;

                if (changed)
                    NotifyPropertyChanged(in thresholdProperty);
            }
        }

        /// <summary>
        /// The target <see cref="VisualElement"/> that will be resized when the handle is dragged.
        /// </summary>
        public VisualElement target { get; set; }

        /// <summary>
        /// Optional callback invoked during resize to modify the size before it is applied to the target.
        /// The first parameter is the computed new size, the second is the original size at drag start.
        /// The callback must return the adjusted size to apply.
        /// </summary>
        public Func<Vector2, Vector2, Vector2> sizeModifier { get; set; }

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public ResizeHandle()
        {
            pickingMode = PickingMode.Position;
            focusable = false;

            AddToClassList(ussClassName);

            m_Draggable = new Draggable(OnClick, OnDrag, OnUp, OnDown);
            this.AddManipulator(m_Draggable);

            dragDirection = Draggable.DragDirection.Vertical;
            threshold = 1f;
        }

        /// <summary>
        /// Constructor with a target <see cref="VisualElement"/>.
        /// </summary>
        /// <param name="target"> The element that will be resized when the handle is dragged. </param>
        public ResizeHandle(VisualElement target)
            : this()
        {
            this.target = target;
        }

        void OnClick()
        {
            // do nothing
        }

        void OnDown(Draggable draggable)
        {
            if (target == null)
                return;

            m_StartSize = target.layout.size;
        }

        void OnDrag(Draggable draggable)
        {
            if (target == null)
                return;

            if (!m_Resizing)
            {
                m_Resizing = true;
                resizeStarted?.Invoke(this);
            }

            var deltaFromStart = draggable.startPosition - draggable.position;
            var newSize = new Vector2(m_StartSize.x - deltaFromStart.x, m_StartSize.y - deltaFromStart.y);

            if (sizeModifier != null)
            {
                newSize = sizeModifier.Invoke(newSize, m_StartSize);
                target.style.width = newSize.x;
                target.style.height = newSize.y;
            }
            else
            {
                if ((draggable.dragDirection & Draggable.DragDirection.Vertical) != 0)
                    target.style.height = newSize.y;
                if ((draggable.dragDirection & Draggable.DragDirection.Horizontal) != 0)
                    target.style.width = newSize.x;
            }
        }

        void OnUp(Draggable draggable)
        {
            m_Resizing = false;
            resizeEnded?.Invoke(this);
        }

    }
}
