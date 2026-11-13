#if APPUI_UITF_PRESENT
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ChoiceList))]
    class ChoiceListBehaviourTests : UIComponentTests<ChoiceList>
    {
        ChoiceList MountChoiceList()
        {
            var list = new ChoiceList();
            list.Add(new ChoiceListItem { key = "fast", label = "Favor speed" });
            list.Add(new ChoiceListItem { key = "balanced", label = "Balanced" });
            list.Add(new ChoiceListItem { key = "quality", label = "Favor quality" });
            return MountElement(list);
        }

        static ChoiceListItem[] ItemsOf(ChoiceList list) => list.Query<ChoiceListItem>().ToList().ToArray();

        [Test]
        public void ChoiceList_ClickItem_ShouldSetValue()
        {
            var list = MountChoiceList();
            var items = ItemsOf(list);
            Assert.AreEqual(3, items.Length, "Every ChoiceListItem child should be present.");

            simulate.Click(items[1]);

            Assert.AreEqual("balanced", list.value, "Clicking an item should select it by its key.");
        }

        [Test]
        public void ChoiceList_ClickItem_ShouldNotifyValueChanged()
        {
            var list = MountChoiceList();
            var items = ItemsOf(list);
            var notifications = 0;
            list.RegisterValueChangedCallback(_ => notifications++);

            simulate.Click(items[2]);

            Assert.AreEqual(1, notifications, "Selecting an item should send a single ChangeEvent.");
        }

        [Test]
        public void ChoiceList_ClickAnotherItem_ShouldDeselectThePrevious()
        {
            var list = MountChoiceList();
            var items = ItemsOf(list);
            simulate.Click(items[0]);
            Assert.IsTrue(items[0].value, "The first item should be selected after being clicked.");

            simulate.Click(items[1]);

            Assert.IsFalse(items[0].value, "Selecting another item should deselect the previous one.");
            Assert.IsTrue(items[1].value, "The clicked item should become selected.");
        }

        [Test]
        public void ChoiceList_WhenDisabled_Click_ShouldNotChangeValue()
        {
            var list = MountChoiceList();
            var items = ItemsOf(list);

            // RadioGroup.AddRadio defaults the group's value to the first item added as soon as it
            // attaches to the panel, so the list already holds "fast" before any click -- it is
            // never null once mounted.
            Assert.AreEqual("fast", list.value, "The first ChoiceListItem added should be selected by default.");

            list.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(items[1]);

            Assert.AreEqual("fast", list.value, "A disabled ChoiceList should not respond to a click.");
        }
    }
}
#endif
