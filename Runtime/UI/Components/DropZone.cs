using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.UI
{
    /// <summary>
    /// The state of the DropZone.
    /// </summary>
    public enum DropZoneState
    {
        /// <summary>
        /// The default state of the DropZone.
        /// </summary>
        Default,

        /// <summary>
        /// The reject drag state of the DropZone.
        /// </summary>
        RejectDrag,

        /// <summary>
        /// The accept drag state of the DropZone.
        /// </summary>
        AcceptDrag
    }

    /// <summary>
    /// A drop zone is a container that can be used to drop content into.
    /// </summary>
#if ENABLE_UXML_SERIALIZED_DATA
    [UxmlElement]
#endif
    public partial class DropZone : BaseVisualElement
    {
        /// <summary>
        /// Delegate used to find any droppable object(s) that can be dropped on the target using the current drag and drop path data.
        /// </summary>
        /// <param name="path">The path data from the current drag and drop operation.</param>
        /// <param name="droppable">The droppable object(s) found.</param>
        /// <returns>True if the target accepts the drag, false otherwise.</returns>
        public delegate bool TryGetDroppableFromPathHandler(string path, out object droppable);

        /// <summary>
        /// Delegate used to find any droppable object(s) that can be dropped on the target using the current drag and drop objects data.
        /// </summary>
        /// <param name="objects">The objects data from the current drag and drop operation.</param>
        /// <param name="droppables">The droppable object(s) found.</param>
        /// <returns>True if the target accepts the drag, false otherwise.</returns>
        public delegate bool TryGetDroppablesFromUnityObjectsHandler(UnityEngine.Object[] objects, out List<object> droppables);

        /// <summary>
        /// Delegate used to find any droppable object(s) that can be dropped on the target.
        /// </summary>
        /// <param name="droppables">The droppable object(s) found.</param>
        /// <returns>True if the target accepts the drag, false otherwise.</returns>
        public delegate bool TryGetDroppablesHandler(out List<object> droppables);

        /// <summary>
        /// The DropZone main styling class.
        /// </summary>
        public const string ussClassName = "appui-dropzone";

        /// <summary>
        /// The DropZone frame styling class.
        /// </summary>
        public const string frameUssClassName = ussClassName + "__frame";

        /// <summary>
        /// The DropZone background styling class.
        /// </summary>
        public const string backgroundUssClassName = ussClassName + "__background";

        /// <summary>
        /// The DropZone state styling class.
        /// </summary>
        [EnumName("GetDropZoneStateUssClassName", typeof(DropZoneState))]
        public const string stateUssClassName = ussClassName + "--";

        /// <summary>
        /// The DropZone visible indicator styling class.
        /// </summary>
        public const string visibleIndicatorUssClassName = ussClassName + "--visible-indicator";

        readonly ExVisualElement m_DropZoneFrame;

        readonly VisualElement m_Background;

        Pressable m_Clickable;

        readonly DropZoneTarget m_DropTarget;

        DropZoneState m_DropZoneState;

        /// <summary>
        /// Event fired when the user drops droppable object(s) on the target.
        /// </summary>
        public event Action<IEnumerable<object>> dropped;

        /// <summary>
        /// <para>
        /// Event fired when the user starts dragging droppable object(s).
        /// </para>
        /// <para>
        /// Use this event to perform validate the drag and drop operation via the <see cref="state"/> property.
        /// </para>
        /// </summary>
        public event Action dragStarted;

        /// <summary>
        /// <para>
        /// Event fired when the user stops dragging droppable object(s).
        /// </para>
        /// <para>
        /// Use this event to perform any cleanup after a drag and drop operation.
        /// </para>
        /// </summary>
        public event Action dragEnded;

        /// <summary>
        /// The container used to display the content.
        /// </summary>
        public override VisualElement contentContainer => m_DropZoneFrame;

        /// <summary>
        /// The Pressable used to handle click events.
        /// </summary>
        public Pressable clickable
        {
            get => m_Clickable;
            private set
            {
                if (m_Clickable != null && m_Clickable.target == this)
                    this.RemoveManipulator(m_Clickable);
                m_Clickable = value;
                if (m_Clickable == null)
                    return;
                this.AddManipulator(m_Clickable);
            }
        }

        /// <summary>
        /// <para>
        /// Method invoked when a drag is started to find any droppable object(s) that can be dropped on the target
        /// using the current drag and drop path data.
        /// </para>
        /// <para>
        /// See <see cref="UnityEditor.DragAndDrop.paths"/> for more information.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This method will only be called in the Unity Editor, since it relies on the <see cref="UnityEditor.DragAndDrop.paths"/>
        /// property which is not available in Unity Runtime.
        /// </remarks>
        public TryGetDroppableFromPathHandler tryGetDroppableFromPath
        {
            get => m_DropTarget.tryGetDroppableFromPath;
            set => m_DropTarget.tryGetDroppableFromPath = value;
        }

        /// <summary>
        /// <para>
        /// Method invoked when a drag is started to find any droppable object(s) that can be dropped on the target
        /// using the current drag and drop objects data.
        /// </para>
        /// <para>
        /// See <see cref="UnityEditor.DragAndDrop.objectReferences"/> for more information.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This method will only be called in the Unity Editor, since it relies on the <see cref="UnityEditor.DragAndDrop.objectReferences"/>
        /// property which is not available in Unity Runtime.
        /// </remarks>
        public TryGetDroppablesFromUnityObjectsHandler tryGetDroppablesFromUnityObjects
        {
            get => m_DropTarget.tryGetDroppablesFromUnityObjects;
            set => m_DropTarget.tryGetDroppablesFromUnityObjects = value;
        }

        /// <summary>
        /// Method invoked when a drag is started to find any droppable object(s) that can be dropped on the target.
        /// </summary>
        /// <remarks>
        /// This method will be called in both the Unity Editor and Unity Runtime.
        /// For Unity Editor specific Drag And Drop system, use <see cref="tryGetDroppableFromPath"/> and <see cref="tryGetDroppablesFromUnityObjects"/>.
        /// </remarks>
        public TryGetDroppablesHandler tryGetDroppables
        {
            get => m_DropTarget.tryGetDroppables;
            set => m_DropTarget.tryGetDroppables = value;
        }

        /// <summary>
        /// The state of the DropZone.
        /// </summary>
        public DropZoneState state
        {
            get => m_DropZoneState;
            set
            {
                RemoveFromClassList(GetDropZoneStateUssClassName(m_DropZoneState));
                m_DropZoneState = value;
                AddToClassList(GetDropZoneStateUssClassName(m_DropZoneState));
            }
        }

        /// <summary>
        /// The visible indicator state of the DropZone.
        /// </summary>
        public bool visibleIndicator
        {
            get => ClassListContains(visibleIndicatorUssClassName);
            set => EnableInClassList(visibleIndicatorUssClassName, value);
        }

        /// <summary>
        /// Create a new DropZone.
        /// </summary>
        public DropZone()
            : this(null)
        {

        }

        /// <summary>
        /// Create a new DropZone.
        /// </summary>
        /// <param name="onClick">The action to perform when the DropZone is clicked.</param>
        public DropZone(Action onClick)
        {
            pickingMode = PickingMode.Position;
            focusable = true;
            clickable = new Pressable(onClick);

            AddToClassList(ussClassName);

            m_Background = new VisualElement { name = backgroundUssClassName, pickingMode = PickingMode.Ignore };
            m_Background.AddToClassList(backgroundUssClassName);
            hierarchy.Add(m_Background);

            m_DropZoneFrame = new ExVisualElement
            {
                name = frameUssClassName,
                pickingMode = PickingMode.Ignore,
                passMask = ExVisualElement.Passes.Clear | ExVisualElement.Passes.Borders,
            };
            m_DropZoneFrame.AddToClassList(frameUssClassName);
            hierarchy.Add(m_DropZoneFrame);

            m_DropTarget = new DropZoneTarget();
            m_DropTarget.dropped += OnDropped;
            m_DropTarget.dragStarted += OnDragStarted;
            m_DropTarget.dragEnded += OnDragEnded;
            this.AddManipulator(m_DropTarget);

            state = DropZoneState.Default;

            schedule.Execute(m_DropZoneFrame.MarkDirtyRepaint).Every(16L);
        }

        void OnDragEnded()
        {
            dragEnded?.Invoke();
        }

        void OnDragStarted()
        {
            dragStarted?.Invoke();
        }

        void OnDropped(IEnumerable<object> objects)
        {
            if (state == DropZoneState.AcceptDrag && dropped != null)
                dropped.Invoke(objects);
            state = DropZoneState.Default;
        }

        class DropZoneTarget : DropTarget<object>
        {
            internal TryGetDroppablesHandler tryGetDroppables { get; set; }

            internal TryGetDroppableFromPathHandler tryGetDroppableFromPath { get; set; }

            internal TryGetDroppablesFromUnityObjectsHandler tryGetDroppablesFromUnityObjects { get; set; }

            protected override List<object> GetDroppableObjects()
            {
                // In GetDroppableObjects, we have to call both the tryGetDroppables and the base method.
                // This insures that Editor specific logic is also called.
                if (tryGetDroppables != null && tryGetDroppables.Invoke(out var droppables))
                    return droppables;

                return base.GetDroppableObjects();
            }

            protected override bool GetDroppableObjectForPath(string path, out object obj)
            {
                return tryGetDroppableFromPath != null ?
                    tryGetDroppableFromPath.Invoke(path, out obj) :
                    base.GetDroppableObjectForPath(path, out obj);
            }

            protected override bool GetDroppableObjectsForUnityObjects(Object[] objects, out List<object> objs)
            {
                return tryGetDroppablesFromUnityObjects != null ?
                    tryGetDroppablesFromUnityObjects.Invoke(objects, out objs) :
                    base.GetDroppableObjectsForUnityObjects(objects, out objs);
            }
        }

#if ENABLE_UXML_TRAITS

        /// <summary>
        /// The UXML factory for the DropZone.
        /// </summary>
        public new class UxmlFactory : UxmlFactory<DropZone, UxmlTraits> { }

        /// <summary>
        /// Class containing the <see cref="UxmlTraits"/> for the <see cref="DropZone"/>.
        /// </summary>
        public new class UxmlTraits : BaseVisualElement.UxmlTraits { }

#endif
    }
}
