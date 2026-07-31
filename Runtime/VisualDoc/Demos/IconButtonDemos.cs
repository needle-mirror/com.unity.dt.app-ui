using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the IconButton documentation page.</summary>
    static class IconButtonDemos
    {
        [VisualDocDemo("iconbutton")]
        static VisualElement Variants()
        {
            return DemoUtils.Row(
                new IconButton("gear"),
                new IconButton("trash") { primary = true },
                new IconButton("copy") { quiet = true },
                new IconButton("star") { size = Size.L });
        }
    }
}
