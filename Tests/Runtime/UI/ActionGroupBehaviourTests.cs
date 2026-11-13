#if APPUI_UITF_PRESENT
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionGroup))]
    class ActionGroupBehaviourTests : UIComponentTests<ActionGroup>
    {
        ActionButton m_Button0;
        ActionButton m_Button1;
        ActionButton m_Button2;

        ActionGroup MountActionGroup(SelectionType selectionType)
        {
            m_Button0 = new ActionButton { icon = "info" };
            m_Button1 = new ActionButton { icon = "info" };
            m_Button2 = new ActionButton { icon = "info" };
            var group = new ActionGroup { selectionType = selectionType };
            group.Add(m_Button0);
            group.Add(m_Button1);
            group.Add(m_Button2);
            return MountElement(group);
        }

        [Test]
        public void ActionGroup_ClickOnItem_WithSingleSelection_ShouldSelectOnlyThatItem()
        {
            MountActionGroup(SelectionType.Single);

            simulate.Click(m_Button1);

            CollectionAssert.AreEquivalent(new[] { 1 }, element.selectedIndices,
                "Clicking an item in Single selection mode should select only that item.");
        }

        [Test]
        public void ActionGroup_ClickOnAnotherItem_WithSingleSelection_ShouldReplaceSelection()
        {
            MountActionGroup(SelectionType.Single);
            simulate.Click(m_Button0);
            Assert.AreEqual(1, element.selectedIndices.Count(), "Precondition: one item should be selected.");

            simulate.Click(m_Button2);

            CollectionAssert.AreEquivalent(new[] { 2 }, element.selectedIndices,
                "A new click should replace the previous single selection.");
        }

        [Test]
        public void ActionGroup_ClickOnItem_WithMultipleSelection_ShouldAccumulateSelection()
        {
            MountActionGroup(SelectionType.Multiple);

            simulate.Click(m_Button0);
            simulate.Click(m_Button2);

            CollectionAssert.AreEquivalent(new[] { 0, 2 }, element.selectedIndices,
                "Clicking items in Multiple selection mode should accumulate the selection.");
        }

        [Test]
        public void ActionGroup_ClickOnItem_ShouldInvokeSelectionChangedEvent()
        {
            MountActionGroup(SelectionType.Single);
            var notifiedIndices = System.Array.Empty<int>();
            element.selectionChanged += indices => notifiedIndices = indices.ToArray();

            simulate.Click(m_Button1);

            CollectionAssert.AreEquivalent(new[] { 1 }, notifiedIndices);
        }

        [Test]
        public void ActionGroup_ClickOnItem_WithNoSelectionType_ShouldNotSelectAnything()
        {
            MountActionGroup(SelectionType.None);

            simulate.Click(m_Button0);

            CollectionAssert.IsEmpty(element.selectedIndices,
                "An ActionGroup with no selection type should never track a selected item.");
        }
    }
}
#endif
