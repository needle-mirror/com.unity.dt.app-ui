using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Accordion documentation page.</summary>
    static class AccordionDemos
    {
        [VisualDocDemo("accordion")]
        static VisualElement Basic()
        {
            var accordion = new Accordion { isExclusive = true };

            var item1 = new AccordionItem { title = "Getting Started", value = true };
            item1.Add(new Text("Read the documentation to learn the basics of App UI."));

            var item2 = new AccordionItem { title = "Advanced Usage" };
            item2.Add(new Text("Explore advanced configuration options and theming."));

            accordion.Add(item1);
            accordion.Add(item2);

            return DemoUtils.Row(accordion);
        }
    }
}
