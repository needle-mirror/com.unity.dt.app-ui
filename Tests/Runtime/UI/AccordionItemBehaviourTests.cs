#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AccordionItem))]
    class AccordionItemBehaviourTests : UIComponentTests<AccordionItem>
    {
        /// <summary>
        /// The header carries the <c>Pressable</c>, so it is what a click has to land on. It is
        /// found by name rather than by class because <see cref="AccordionItem"/> sets both to
        /// <c>headerUssClassName</c>.
        /// </summary>
        static VisualElement HeaderOf(AccordionItem item) =>
            item.Q(name: AccordionItem.headerUssClassName);

        AccordionItem MountInAccordion(AccordionItem item)
        {
            // An AccordionItem is meant to live inside an Accordion, which owns the exclusivity
            // behaviour, so mounting one bare would not match how it is used.
            var accordion = new Accordion();
            accordion.Add(item);
            appUiPanel.Clear();
            appUiPanel.Add(accordion);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            return item;
        }

        [Test]
        public void AccordionItem_HeaderClick_ShouldToggleValue()
        {
            var item = MountInAccordion(new AccordionItem { title = "Item 1", value = false });
            var header = HeaderOf(item);
            Assert.IsNotNull(header, "The AccordionItem header should be present.");

            simulate.Click(header);
            Assert.IsTrue(item.value, "Clicking the header should expand the item.");

            simulate.Click(header);
            Assert.IsFalse(item.value, "Clicking the header again should collapse the item.");
        }

        [Test]
        public void AccordionItem_HeaderClick_ShouldSendValueChangedEvent()
        {
            var item = MountInAccordion(new AccordionItem { title = "Item 1", value = false });
            var notifications = 0;
            item.RegisterCallback<AccordionItemValueChangedEvent>(_ => notifications++);

            simulate.Click(HeaderOf(item));

            Assert.AreEqual(1, notifications);
        }

        [Test]
        public void AccordionItem_SubmitKey_ShouldToggleValue()
        {
            var item = MountInAccordion(new AccordionItem { title = "Item 1", value = false });

            HeaderOf(item).Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.IsTrue(item.value, "Submitting on the header should expand the item.");
        }

        [Test]
        public void Accordion_WhenExclusive_ExpandingOneItem_ShouldCollapseTheOther()
        {
            var first = new AccordionItem { title = "Item 1", value = true };
            var second = new AccordionItem { title = "Item 2", value = false };
            var accordion = new Accordion { isExclusive = true };
            accordion.Add(first);
            accordion.Add(second);
            appUiPanel.Clear();
            appUiPanel.Add(accordion);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            simulate.Click(HeaderOf(second));

            Assert.IsTrue(second.value, "The clicked item should expand.");
            Assert.IsFalse(first.value, "An exclusive Accordion should collapse the other item.");
        }
    }
}
#endif
