using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Quote documentation page.</summary>
    static class QuoteDemos
    {
        [VisualDocDemo("quote")]
        static VisualElement Variants()
        {
            var defaultQuote = new Quote();
            defaultQuote.Add(new Text("The best way to predict the future is to invent it."));

            var coloredQuote = new Quote { color = new Color(0.808f, 0.576f, 0.847f) };
            coloredQuote.Add(new Text("Innovation distinguishes between a leader and a follower."));

            return DemoUtils.Row(defaultQuote, coloredQuote);
        }
    }
}
