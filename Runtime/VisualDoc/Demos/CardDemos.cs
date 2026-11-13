using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the Card documentation page.</summary>
    static class CardDemos
    {
        [VisualDocDemo("card")]
        static VisualElement States()
        {
            return DemoUtils.Row(
                BuildCard("Static", "Plain grouped content.", false, false),
                BuildCard("Interactive", "Hover and click the whole tile.", true, false),
                BuildCard("Selected", "Outlined with the accent colour.", true, true));
        }

        [VisualDocDemo("card", order = 1)]
        static VisualElement WithFooter()
        {
            var card = new Card { interactive = true, elevation = 2, style = { width = 240 } };

            var header = new CardHeader();
            header.Add(new CardTitle { text = "Third Person Template" });
            header.Add(new CardDescription { text = "A ready-made character controller." });
            card.Add(header);

            var content = new CardContent();
            content.Add(new Text("Universal Render Pipeline - 2.4 GB"));
            card.Add(content);

            var footer = new CardFooter();
            footer.Add(new UI.Button { variant = ButtonVariant.Secondary, title = "Details" });
            footer.Add(new UI.Button { variant = ButtonVariant.Default, title = "Create" });
            card.Add(footer);

            return DemoUtils.Row(card);
        }

        static Card BuildCard(string title, string description, bool interactive, bool selected)
        {
            var card = new Card { interactive = interactive, selected = selected, style = { width = 200 } };
            var header = new CardHeader();
            header.Add(new CardTitle { text = title });
            header.Add(new CardDescription { text = description });
            card.Add(header);
            return card;
        }
    }
}
