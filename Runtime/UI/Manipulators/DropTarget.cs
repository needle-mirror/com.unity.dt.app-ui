using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.AppUI.UI
{
    /// <summary>
    /// A droppable is a container that can be used to drop content into.
    /// </summary>
    /// <typeparam name="T"> The type of the object that can be dropped on the target. </typeparam>
    public class DropTarget<T> : Manipulator
    {
        bool m_Entered;
        
        readonly List<T> m_Objects = new List<T>();
        
        static readonly Dictionary<string, T> k_CacheForPath = new Dictionary<string, T>();
        
        /// <summary>
        /// Event fired when the user starts dragging droppable object(s) over the target.
        /// </summary>
        public event Action dragStarted;
        
        /// <summary>
        /// Event fired when the user stops dragging droppable object(s) over the target.
        /// </summary>
        public event Action dragEnded;
        
        /// <summary>
        /// Event fired when the user drops droppable object(s) on the target.
        /// </summary>
        public event Action<IEnumerable<T>> dropped;

        /// <summary>
        /// True if the user is currently dragging droppable object(s) over the target, false otherwise.
        /// </summary>
        public bool isDragging => m_Objects.Count > 0;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public DropTarget()
        {
            Reset();
        }

        /// <summary>
        /// Called to register event callbacks on the target element.
        /// </summary>
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
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
            target.UnregisterCallback<PointerEnterEvent>(OnPointerEnter);
            target.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
#if UNITY_EDITOR
            target.UnregisterCallback<DragEnterEvent>(OnDragEnter);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeave);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerform);
            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdate);
            target.UnregisterCallback<DragExitedEvent>(OnDragExit);
#endif
        }
        
        /// <summary>
        /// This method is called when the user drops a Unity object on the target.
        /// </summary>
        /// <param name="objects"> The objects dropped on the target. </param>
        /// <param name="objs"> The objects to be dropped. </param>
        /// <returns> True if the object is valid and can be dropped, false otherwise. </returns>
        protected virtual bool GetDroppableObjectsForUnityObjects(Object[] objects, out List<T> objs)
        {
            objs = new List<T>();
            
            foreach (var obj in objects)
            {
                if (obj is T droppable)
                    objs.Add(droppable);
            }
            
            return objs.Count > 0;
        }
        
        /// <summary>
        /// This method is called when the user drops a file on the target.
        /// </summary>
        /// <remarks>
        /// The result will be cached and this cache will be invalidated when the current drag operation ends.
        /// </remarks>
        /// <param name="path"> The path of the file dropped on the target. </param>
        /// <param name="obj"> The object to be dropped. </param>
        /// <returns> True if the object is valid and can be dropped, false otherwise. </returns>
        protected virtual bool GetDroppableObjectForPath(string path, out T obj)
        {
            obj = default;
            return false;
        }
        
        /// <summary>
        /// This method is called to get the objects that can be dropped on the target.
        /// </summary>
        /// <returns> The objects that can be dropped on the target. </returns>
        protected virtual List<T> GetDroppableObjects()
        {
            var res = new List<T>();
            
#if UNITY_EDITOR
            if (GetDroppableObjectsForUnityObjects(UnityEditor.DragAndDrop.objectReferences, out var unityObjs))
                return unityObjs;
            
            foreach (var path in UnityEditor.DragAndDrop.paths)
            {
                if (k_CacheForPath.TryGetValue(path, out var cached))
                {
                    res.Add(cached);
                    continue;
                }

                if (GetDroppableObjectForPath(path, out var pathObj))
                {
                    res.Add(pathObj);
                    k_CacheForPath.Add(path, pathObj);
                }
            }
#endif
            return res;
        }
        
        /// <summary>
        /// Reset the DropTarget.
        /// </summary>
        protected virtual void Reset()
        {
            m_Objects.Clear();
            k_CacheForPath.Clear();
            m_Entered = false;
#if UNITY_EDITOR
            UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.None;
#endif
        }
        
        void OnPointerEnter(PointerEnterEvent evt) => OnEnter();
        void OnPointerLeave(PointerLeaveEvent evt) => OnExit();
        void OnPointerCancel(PointerCancelEvent evt) => OnExit();
#if UNITY_EDITOR
        void OnDragEnter(DragEnterEvent evt)
        {
            OnEnter();
            // We have to update the visual mode here in order for the DragPerformEvent to be fired later.
            if (isDragging)
            {
                dragStarted?.Invoke();
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Generic;
            }
        }

        void OnDragLeave(DragLeaveEvent evt)
        {
            OnExit();
        }

        void OnDragPerform(DragPerformEvent evt)
        {
            OnPerform();
        }
        void OnDragUpdate(DragUpdatedEvent evt)
        {
            // We have to update the visual mode here in order for the DragPerformEvent to be fired later.
            if (isDragging)
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Generic;
        }

        void OnDragExit(DragExitedEvent evt)
        {
            OnExit();
        }
#endif

        void OnEnter()
        {
            if (m_Entered)
                return;
            
            m_Entered = true;
            var objects = GetDroppableObjects();
            if (objects.Count > 0)
            {
                m_Objects.Clear();
                m_Objects.AddRange(objects);
                
                dragStarted?.Invoke();
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Generic;
#endif
            }
            else
            {
                m_Objects.Clear();
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Rejected;
#endif
            }
        }

        void OnExit()
        {
            if (!m_Entered)
                return;
            
            var wasDragging = isDragging;
            Reset();
            
            if (!wasDragging)
                return;

            dragEnded?.Invoke();
        }

        void OnPerform()
        {
            if (!isDragging)
                return;

            var objects = GetDroppableObjects();
            if (objects.Count > 0)
            {
                m_Objects.Clear();
                m_Objects.AddRange(objects);
#if UNITY_EDITOR
                UnityEditor.DragAndDrop.AcceptDrag();
#endif
                dropped?.Invoke(m_Objects);
            }
            
            OnExit();
        }
    }
}
