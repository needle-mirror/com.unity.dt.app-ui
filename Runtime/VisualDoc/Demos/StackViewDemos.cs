using UnityEngine.UIElements;
using Unity.AppUI.UI;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.VisualDoc
{
    /// <summary>Live examples for the StackView documentation page.</summary>
    static class StackViewDemos
    {
        [VisualDocDemo("stackview")]
        static VisualElement Basic()
        {
            var stackView = new StackView();
            stackView.AddToClassList("demo-block");

            var page1 = DemoUtils.Column(new Text("Page 1"));
            var nextButton = new Button { title = "Next" };
            page1.Add(nextButton);

            nextButton.clicked += () =>
            {
                var page2 = DemoUtils.Column(new Text("Page 2"));
                var backButton = new Button { title = "Back" };
                page2.Add(backButton);

                stackView.Push(page2);

                backButton.clicked += () => stackView.Pop();
            };

            stackView.initialItem = page1;

            return stackView;
        }
    }
}
