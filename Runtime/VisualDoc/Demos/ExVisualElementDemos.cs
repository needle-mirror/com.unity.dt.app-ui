using UnityEngine;
using UnityEngine.UIElements;
using Unity.AppUI.UI;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the ExVisualElement documentation page.</summary>
    static class ExVisualElementDemos
    {
        [VisualDocDemo("exvisualelement")]
        static VisualElement Passes()
        {
            var card = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear
                    | ExVisualElement.Passes.OutsetShadows
                    | ExVisualElement.Passes.BackgroundColor
                    | ExVisualElement.Passes.Borders,
                backgroundColor = new Color(0.16f, 0.16f, 0.18f),
                outlineColor = new Color(0.3f, 0.5f, 1f)
            };
            card.Add(new Text("Card with shadow"));

            var outlined = new ExVisualElement
            {
                passMask = ExVisualElement.Passes.Clear
                    | ExVisualElement.Passes.BackgroundColor
                    | ExVisualElement.Passes.Outline,
                backgroundColor = new Color(0.16f, 0.16f, 0.18f),
                outlineColor = new Color(1f, 0.6f, 0.2f)
            };
            outlined.Add(new Text("Outlined element"));

            return DemoUtils.Row(card, outlined);
        }
    }
}
