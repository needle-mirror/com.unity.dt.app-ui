using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BoundsIntField))]
    class BoundsIntFieldTests : VisualElementTests<BoundsIntField>
    {
        protected override string mainUssClassName => BoundsIntField.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", DefaultStory);
            }
        }

        static UnityEngine.UIElements.VisualElement DefaultStory(StoryContext context)
        {
            var field = new BoundsIntField();
            return field;
        }
    }
}
