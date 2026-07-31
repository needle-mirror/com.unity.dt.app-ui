using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ActionGroup documentation page.</summary>
    static class ActionGroupDemos
    {
        [VisualDocDemo("actiongroup")]
        static VisualElement Basic()
        {
            var group = new ActionGroup();
            group.Add(new ActionButton { label = "Cut" });
            group.Add(new ActionButton { label = "Copy" });
            group.Add(new ActionButton { label = "Paste" });

            return DemoUtils.Row(group);
        }

        [VisualDocDemo("actiongroup", order = 1)]
        static VisualElement Selectable()
        {
            var group = new ActionGroup { selectionType = SelectionType.Single };
            group.Add(new ActionButton { label = "Left" });
            group.Add(new ActionButton { label = "Center" });
            group.Add(new ActionButton { label = "Right" });
            group.SetSelection(new[] { 0 });

            return DemoUtils.Row(group);
        }
    }
}
