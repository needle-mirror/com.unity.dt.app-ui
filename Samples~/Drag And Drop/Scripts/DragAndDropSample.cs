using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Unity.AppUI.Samples
{
    public class DragAndDropSample : MonoBehaviour
    {
        public UIDocument uiDocument;
        
        void Start()
        {
            var root = uiDocument.rootVisualElement;
            var script = new DragAndDropSampleScript();
            script.Start(root);
        }

        public class DragAndDropSampleScript
        {
            GridView m_SrcList;

            GridView m_DstList;

            DropZone m_Dropzone;

            static List<string> s_DraggedObjects;

            public void Start(VisualElement root)
            {
                m_SrcList = root.Q<GridView>("dnd-gridview-src");
                m_DstList = root.Q<GridView>("dnd-gridview-dst");
                m_Dropzone = root.Q<DropZone>("dnd-dropzone");
                
                m_SrcList.columnCount = 1;
                m_DstList.columnCount = 1;

                m_SrcList.makeItem = MakeItem;
                m_DstList.makeItem = MakeItem;

                m_SrcList.bindItem = BindSrcItem;
                m_DstList.bindItem = BindDstItem;
                
                // Populate source list
                var srcItems = new List<string>();
                for (var i = 0; i < 10; ++i)
                {
                    srcItems.Add($"Item {i}");
                }
                m_SrcList.itemsSource = srcItems;
                m_SrcList.selectionType = SelectionType.Multiple;
                
                // Subscribe to the drag event from the source list
                m_SrcList.dragStarted += OnDragStarted;
                m_SrcList.dragFinished += OnDragFinished;
                m_SrcList.dragCanceled += OnDragCanceled;
                
                // Handle drop in the dropzone
                m_Dropzone.tryGetDroppableFromPath = TryGetDroppableFromPath;
                m_Dropzone.tryGetDroppablesFromUnityObjects = TryGetDroppableFromUnityObjects;
                m_Dropzone.dropped += OnDropped;
                m_Dropzone.dragStarted += OnEditorDragStarted;
                m_Dropzone.dragEnded += Reset;
                
                Reset();
            }

            void BindDstItem(VisualElement el, int idx)
            {
                var item = (Text)el.ElementAt(0);
                item.text = (string)m_DstList.itemsSource[idx];
            }

            void BindSrcItem(VisualElement el, int idx)
            {
                var item = (Text)el.ElementAt(0);
                item.text = (string)m_SrcList.itemsSource[idx];
            }

            static VisualElement MakeItem()
            {
                var item = new VisualElement();
                item.AddToClassList("dnd-item");
                var text = new Text();
                text.AddToClassList("dnd-item__label");
                item.Add(text);
                return item;
            }

            void Reset()
            {
                m_Dropzone.state = DropZoneState.Default;
                m_Dropzone.visibleIndicator = m_DstList.itemsSource == null || m_DstList.itemsSource.Count == 0;
                SetDraggedObjects(null);
            }

            void OnDropped(IEnumerable<object> objects)
            {
                var selection = objects.Where(o => o is string).Cast<string>().ToList();
                
                if (selection.Count == 0)
                    return;
                
                // Add dropped items to the destination list
                var dstItems = new List<string>(m_DstList.itemsSource?.Cast<string>() ?? Enumerable.Empty<string>());
                dstItems.AddRange(selection);
                m_DstList.itemsSource = dstItems;
                
                // Remove dropped items from the source list
                var srcItems = new List<string>(m_SrcList.itemsSource?.Cast<string>() ?? Enumerable.Empty<string>());
                foreach (var item in selection)
                {
                    srcItems.Remove(item);
                }
                m_SrcList.itemsSource = srcItems;
                
                // Hide the dropzone if the destination list is not empty
                m_Dropzone.visibleIndicator = m_DstList.itemsSource != null || m_DstList.itemsSource.Count == 0;
            }

            void OnDragCanceled()
            {
                Reset();
            }

            void OnDragFinished(PointerUpEvent evt)
            {
                if (m_Dropzone.state == DropZoneState.AcceptDrag &&
                    m_Dropzone.ContainsPoint(m_Dropzone.WorldToLocal(evt.position)) && 
                    s_DraggedObjects != null)
                    OnDropped(s_DraggedObjects);
                
                Reset();
            }
            
            void OnEditorDragStarted()
            {
                // Accept every path from the editor
                m_Dropzone.visibleIndicator = true;
                m_Dropzone.state = DropZoneState.AcceptDrag;
            }

            void OnDragStarted(PointerMoveEvent evt)
            {
                var selection = new List<int>(m_SrcList.selectedIndices);
                
                // Show the dropzone
                m_Dropzone.visibleIndicator = true;
                
                // Enable dropzone if there is a selection
                m_Dropzone.state = selection.Count > 0 && !selection.Contains(0) ? 
                    DropZoneState.AcceptDrag : DropZoneState.RejectDrag;
                
                // Set the dragged objects
                SetDraggedObjects(selection.Select(i => (string)m_SrcList.itemsSource[i]));
            }

            static void SetDraggedObjects(IEnumerable<string> objects)
            {
                s_DraggedObjects = objects == null ? null : new List<string>(objects);
            }
            
            static bool TryGetDroppableFromPath(string path, out object droppable)
            {
                // Use path directly as droppable item
                droppable = path;
                return true;
            }
            
            static bool TryGetDroppableFromUnityObjects(Object[] objects, out List<object> droppables)
            {
                // We don't want to support Unity objects in this sample
                droppables = null;
                return false;
            }
        }
    }
}
