using UnityEngine.UIElements;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the DropZone documentation page.</summary>
    static class DropZoneDemos
    {
        [VisualDocDemo("dropzone")]
        static VisualElement States()
        {
            var idle = new DropZone { visibleIndicator = true };
            idle.Add(new Text { text = "Drop files here" });
            idle.AddToClassList("demo-block");

            var accept = new DropZone { visibleIndicator = true, state = DragAndDropState.AcceptDrag };
            accept.Add(new Text { text = "Release to drop" });
            accept.AddToClassList("demo-block");

            var reject = new DropZone { visibleIndicator = true, state = DragAndDropState.RejectDrag };
            reject.Add(new Text { text = "File type not supported" });
            reject.AddToClassList("demo-block");

            return DemoUtils.Row(idle, accept, reject);
        }
    }
}
