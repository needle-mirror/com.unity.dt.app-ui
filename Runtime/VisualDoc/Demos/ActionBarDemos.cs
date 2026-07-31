using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ActionBar documentation page.</summary>
    static class ActionBarDemos
    {
        [VisualDocDemo("actionbar")]
        static VisualElement Basic()
        {
            var actionBar = new ActionBar();
            actionBar.Add(new ActionButton { icon = "pencil", label = "Edit" });
            actionBar.Add(new ActionButton { icon = "trash", label = "Delete" });

            return DemoUtils.Row(actionBar);
        }
    }
}
