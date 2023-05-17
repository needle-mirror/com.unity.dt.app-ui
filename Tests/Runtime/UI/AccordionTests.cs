using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Accordion))]
    class AccordionTests : VisualElementTests<Accordion>
    {
        protected override string mainUssClassName => Accordion.ussClassName;
    }

    [TestFixture]
    [TestOf(typeof(AccordionItem))]
    class AccordionItemTests : VisualElementTests<AccordionItem>
    {
        protected override string mainUssClassName => AccordionItem.ussClassName;

        [Test, Order(2)]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        [TestCase("A title")]
        public void AccordionItem_Title_ShouldBeSettable(string title)
        {
            var expected = title;
            element.title = title;
            Assert.AreEqual(expected, element.title);
        }

        [Test, Order(2)]
        public void AccordionItem_SubmitEvent_ShouldBeHandled()
        {
            //todo simulate submitevent
        }

        [Test, Order(2)]
        public void AccordionItem_ClickEvent_ShouldBeHandled()
        {
            //todo simulate clickevent
        }
    }
}
