using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the LocalizedTextElement documentation page.</summary>
    static class LocalizedTextElementDemos
    {
        [VisualDocDemo("localizedtextelement")]
        static VisualElement Basic()
        {
            return DemoUtils.Column(
                new LocalizedTextElement("Hello World"),
                new LocalizedTextElement("Plain text also works without a localization table."));
        }
    }
}
