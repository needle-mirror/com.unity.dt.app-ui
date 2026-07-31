using System.Collections.Generic;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Dropdown documentation page.</summary>
    static class DropdownDemos
    {
        [VisualDocDemo("dropdown")]
        static VisualElement Sizes()
        {
            var items = new List<string> { "Option 1", "Option 2", "Option 3" };

            var small = new Dropdown { sourceItems = items, size = Size.S };
            small.AddToClassList("demo-field");

            var medium = new Dropdown { sourceItems = items, size = Size.M };
            medium.AddToClassList("demo-field");

            var large = new Dropdown { sourceItems = items, size = Size.L };
            large.AddToClassList("demo-field");

            return DemoUtils.Row(small, medium, large);
        }
    }
}
