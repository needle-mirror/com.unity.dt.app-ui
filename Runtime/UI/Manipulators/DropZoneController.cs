using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.Core;
using System.Linq;

namespace Unity.AppUI.UI
{

    /// <summary>
    /// A manipulator that enables drag-and-drop functionality on UI elements with customizable accept logic.
    /// </summary>
    /// <remarks>
    /// DropZoneController is a manipulator that adds drag-and-drop functionality to UI elements, allowing them to
    /// receive and respond to drag operations. It provides callbacks for handling drag enter, exit, and drop events
    /// with customizable acceptance logic.
    ///
    /// The controller enables creating interactive drop zones where users can drag files, objects, or other UI
    /// elements from external sources or within the application. It supports visual feedback during drag operations
    /// and flexible filtering of acceptable drag data.
    ///
    /// DropZoneController works with Unity's drag-and-drop system, providing a bridge between native drag operations
    /// and App UI components. It can handle various data types including files, textures, objects, and custom data.
    ///
    /// Use DropZoneController for file upload areas, asset import zones, reorderable lists, or any interface that
    /// benefits from intuitive drag-and-drop interactions. It enhances user experience by providing direct
    /// manipulation capabilities.
    /// </remarks>
    /// <example>
    /// <para>Basic file drop zone. Creating a drop zone that accepts image files.</para>
    /// <code lang="csharp"><![CDATA[
    /// var dropZoneElement = new VisualElement();
    /// dropZoneElement.AddToClassList("drop-zone");
    /// dropZoneElement.Add(new Icon { icon = "upload" });
    /// dropZoneElement.Add(new Text("Drag image files here"));
    ///
    /// var dropController = new DropZoneController((state) => {
    ///     // Visual feedback during drag operations
    ///     dropZoneElement.EnableInClassList("drag-over", state == DragAndDropState.AcceptDrag);
    ///     dropZoneElement.EnableInClassList("drag-reject", state == DragAndDropState.RejectDrag);
    /// });
    ///
    /// // Define what files to accept
    /// dropController.acceptDrag = (objects) => {
    ///     return objects.OfType<string>()
    ///         .Any(path => IsImageFile(path));
    /// };
    ///
    /// // Handle successful drops
    /// dropController.dropped += (objects) => {
    ///     foreach (var filePath in objects.OfType<string>())
    ///     {
    ///         if (IsImageFile(filePath))
    ///         {
    ///             LoadImageFile(filePath);
    ///         }
    ///     }
    /// };
    ///
    /// dropZoneElement.AddManipulator(dropController);
    /// container.Add(dropZoneElement);
    /// ]]></code>
    /// <para>Asset drop zone with multiple types. Creating a drop zone that accepts multiple asset types.</para>
    /// <code lang="csharp"><![CDATA[
    /// var assetDropZone = new VisualElement();
    /// assetDropZone.AddToClassList("asset-drop-zone");
    ///
    /// var dropController = new DropZoneController();
    ///
    /// // Accept textures, audio clips, and materials
    /// dropController.acceptDrag = (objects) => {
    ///     return objects.Any(obj =>
    ///         obj is Texture2D ||
    ///         obj is AudioClip ||
    ///         obj is Material);
    /// };
    ///
    /// // Handle different asset types
    /// dropController.dropped += (objects) => {
    ///     foreach (var obj in objects)
    ///     {
    ///         switch (obj)
    ///         {
    ///             case Texture2D texture:
    ///                 Debug.Log($"Dropped texture: {texture.name}");
    ///                 ProcessTexture(texture);
    ///                 break;
    ///
    ///             case AudioClip audio:
    ///                 Debug.Log($"Dropped audio clip: {audio.name}");
    ///                 ProcessAudioClip(audio);
    ///                 break;
    ///
    ///             case Material material:
    ///                 Debug.Log($"Dropped material: {material.name}");
    ///                 ProcessMaterial(material);
    ///                 break;
    ///         }
    ///     }
    /// };
    ///
    /// assetDropZone.AddManipulator(dropController);
    /// inspectorPanel.Add(assetDropZone);
    /// ]]></code>
    /// <para>Reorderable list with drop zones. Creating drop zones for reordering list items.</para>
    /// <code lang="csharp"><![CDATA[
    /// public class ReorderableListItem : VisualElement
    /// {
    ///     public string ItemData { get; set; }
    ///
    ///     public ReorderableListItem(string data)
    ///     {
    ///         ItemData = data;
    ///         AddToClassList("reorderable-item");
    ///         Add(new Text(data));
    ///
    ///         // Make this item draggable
    ///         var dragManipulator = new DragManipulator(this);
    ///         AddManipulator(dragManipulator);
    ///
    ///         // Make this item a drop zone
    ///         var dropController = new DropZoneController((state) => {
    ///             EnableInClassList("drop-target", state == DragAndDropState.AcceptDrag);
    ///         });
    ///
    ///         dropController.acceptDrag = (objects) => {
    ///             // Accept other ReorderableListItem objects
    ///             return objects.OfType<ReorderableListItem>()
    ///                 .Any(item => item != this);
    ///         };
    ///
    ///         dropController.dropped += (objects) => {
    ///             var draggedItem = objects.OfType<ReorderableListItem>().FirstOrDefault();
    ///             if (draggedItem != null)
    ///             {
    ///                 ReorderItems(draggedItem, this);
    ///             }
    ///         };
    ///
    ///         AddManipulator(dropController);
    ///     }
    /// }
    ///
    /// // Usage
    /// var listContainer = new VisualElement();
    /// var items = new[] { "Item 1", "Item 2", "Item 3" };
    /// foreach (var item in items)
    /// {
    ///     listContainer.Add(new ReorderableListItem(item));
    /// }
    /// ]]></code>
    /// </example>
    [VisualDocPage("drag-and-drop")]
    public class DropZoneController : Manipulator
    {
#pragma warning disable 67
        /// <summary>
        /// Method called to determine if the target can accept the drag.
        /// </summary>
        public Func<IEnumerable<object>, bool> acceptDrag;

        /// <summary>
        /// Event fired either when objecs have been dropped on the target or when the user cancels the drag operation or when the user exits the target.
        /// </summary>
        /// <remarks>
        /// Use this event to clean up any state that was set up when the drag operation started.
        /// </remarks>
        public event Action dragEnded;

        /// <summary>
        /// Event fired when the user drops droppable object(s) on the target.
        /// </summary>
        /// <remarks>
        /// This event is fired only if the target is currently accepting the drag operation.
        /// </remarks>
        public event Action<IEnumerable<object>> dropped;
#pragma warning restore 67

        readonly Action<DragAndDropState> m_StateChanged;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public DropZoneController() : this(null) { }

        /// <summary>
        /// Constructor with a state changed callback.
        /// </summary>
        /// <param name="stateChanged"> The callback to be called when the drag and drop state changes. </param>
        public DropZoneController(Action<DragAndDropState> stateChanged)
        {
            m_StateChanged = stateChanged;
        }

        /// <summary>
        /// Called to register event callbacks on the target element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
#if UNITY_EDITOR
            target.RegisterCallback<DragEnterEvent>(OnDragEnter);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeave);
            target.RegisterCallback<DragPerformEvent>(OnDragPerform);
            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.RegisterCallback<DragExitedEvent>(OnDragExit);
#endif
        }

        /// <summary>
        /// Called to unregister event callbacks from the target element.
        /// </summary>
        protected override void UnregisterCallbacksFromTarget()
        {
#if UNITY_EDITOR
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
#endif
        }

#if UNITY_EDITOR
        void OnDragEnter(DragEnterEvent evt) => TryStartDragOperation();

        void OnDragUpdate(DragUpdatedEvent evt) => TryStartDragOperation();

        void OnDragPerform(DragPerformEvent evt)
        {
            if (acceptDrag == null)
            {
                EndDragOperation();
                return;
            }

            if (acceptDrag(DragAndDrop.objects))
            {
                var objects = DragAndDrop.Drop();
                dropped?.Invoke(objects);
            }

            EndDragOperation();
        }

        void OnDragLeave(DragLeaveEvent evt) => EndDragOperation();

        void OnDragExit(DragExitedEvent evt) => EndDragOperation();
#endif

        void TryStartDragOperation()
        {
            // If there are no objects being dragged, we don't need to do anything.
            var objects = DragAndDrop.objects.ToList();
            if (objects.Count == 0)
                return;

            DragAndDrop.activeDropTarget = this;
            if (acceptDrag == null)
            {
                m_StateChanged?.Invoke(DragAndDropState.Default);
                return;
            }

            var state = acceptDrag(DragAndDrop.objects) ? DragAndDropState.AcceptDrag : DragAndDropState.RejectDrag;
            DragAndDrop.state = state;
            m_StateChanged?.Invoke(state);
        }

        void EndDragOperation()
        {
            m_StateChanged?.Invoke(DragAndDropState.Default);
            if (DragAndDrop.activeDropTarget == this)
                DragAndDrop.activeDropTarget = null;
            dragEnded?.Invoke();
        }
    }
}
