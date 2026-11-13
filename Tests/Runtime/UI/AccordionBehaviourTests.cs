#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Accordion))]
    class AccordionBehaviourTests : UIComponentTests<Accordion>
    {
        static VisualElement HeaderOf(AccordionItem item) =>
            item.Q(name: AccordionItem.headerUssClassName);

        Accordion MountAccordion(bool isExclusive, out AccordionItem first, out AccordionItem second)
        {
            first = new AccordionItem { title = "Item 1", value = false };
            second = new AccordionItem { title = "Item 2", value = false };
            var accordion = new Accordion { isExclusive = isExclusive };
            accordion.Add(first);
            accordion.Add(second);
            return MountElement(accordion);
        }

        [Test]
        public void Accordion_WhenNotExclusive_ExpandingBothItems_ShouldKeepBothExpanded()
        {
            var accordion = MountAccordion(false, out var first, out var second);

            simulate.Click(HeaderOf(first));
            simulate.Click(HeaderOf(second));

            Assert.IsTrue(first.value, "The first item should stay expanded.");
            Assert.IsTrue(second.value,
                "A non-exclusive Accordion should allow multiple items to be expanded at once.");
        }

        [Test]
        public void Accordion_WhenExclusive_ExpandingOneItem_ShouldCollapseTheOther()
        {
            // Not covered as a bare AccordionItem interaction -- this is the Accordion container's
            // own coordination behaviour on OnAccordionItemValueChanged.
            var accordion = MountAccordion(true, out var first, out var second);
            simulate.Click(HeaderOf(first));
            Assert.IsTrue(first.value, "Precondition: the first item should be expanded.");

            simulate.Click(HeaderOf(second));

            Assert.IsTrue(second.value, "The clicked item should expand.");
            Assert.IsFalse(first.value,
                "An exclusive Accordion should collapse every other item without notifying them.");
        }

        [Test]
        public void Accordion_IsExclusiveToggledAtRuntime_ShouldApplyToSubsequentClicks()
        {
            var accordion = MountAccordion(false, out var first, out var second);
            simulate.Click(HeaderOf(first));
            Assert.IsTrue(first.value, "Precondition: the first item should be expanded.");

            accordion.isExclusive = true;
            simulate.Click(HeaderOf(second));

            Assert.IsFalse(first.value,
                "Switching isExclusive on should apply to the very next selection change.");
            Assert.IsTrue(second.value);
        }
    }
}
#endif
