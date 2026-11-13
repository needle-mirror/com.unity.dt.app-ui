using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the CardGrid documentation page.</summary>
    static class CardGridDemos
    {
        static readonly string[] k_Templates =
        {
            "2D Platformer", "First Person", "Third Person", "Top Down", "VR Core", "Empty",
        };

        [VisualDocDemo("cardgrid")]
        static VisualElement Fill()
        {
            return BuildGrid(true);
        }

        [VisualDocDemo("cardgrid", order = 1)]
        static VisualElement FixedColumns()
        {
            return BuildGrid(false);
        }

        static VisualElement BuildGrid(bool fill)
        {
            var grid = new CardGrid { minItemWidth = 160, fill = fill };
            grid.style.width = 560;
            foreach (var template in k_Templates)
            {
                var card = new Card { interactive = true };
                var header = new CardHeader();
                header.Add(new CardTitle { text = template });
                header.Add(new CardDescription { text = "Sample project" });
                card.Add(header);
                grid.Add(card);
            }
            return grid;
        }
    }
}
