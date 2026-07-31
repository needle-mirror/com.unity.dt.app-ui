using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Preloader documentation page.</summary>
    static class PreloaderDemos
    {
        [VisualDocDemo("preloader")]
        static VisualElement Basic()
        {
            return DemoUtils.Row(new Preloader());
        }
    }
}
