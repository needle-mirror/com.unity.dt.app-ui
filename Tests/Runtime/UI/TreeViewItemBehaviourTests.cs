#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TreeViewItem))]
    class TreeViewItemBehaviourTests : UIComponentTests<TreeViewItem>
    {
        static VisualElement HeaderOf(TreeViewItem item) => item.Q(name: TreeViewItem.headerUssClassName);

        static VisualElement CaretButtonOf(TreeViewItem item) => item.Q(name: TreeViewItem.caretButtonUssClassName);

        [Test]
        public void TreeViewItem_HeaderClick_ShouldInvokeClickable()
        {
            var clickCount = 0;
            var item = MountElement(new TreeViewItem { label = "Item" });
            item.clickable = new Pressable(() => clickCount++);

            simulate.Click(HeaderOf(item));

            Assert.AreEqual(1, clickCount, "Clicking the header should invoke the assigned Pressable.");
        }

        [Test]
        public void TreeViewItem_CaretClick_ShouldInvokeCaretClickable()
        {
            var clickCount = 0;
            var item = MountElement(new TreeViewItem { label = "Item", showCaret = true });
            item.caretClickable = new Pressable(() => clickCount++);

            simulate.Click(CaretButtonOf(item));

            Assert.AreEqual(1, clickCount, "Clicking the caret button should invoke the assigned caretClickable.");
        }

        [Test]
        public void TreeViewItem_ToggleExpand_ShouldFlipExpandedState()
        {
            var item = MountElement(new TreeViewItem { label = "Item", showCaret = true, expanded = false });

            item.ToggleExpand();

            Assert.IsTrue(item.expanded, "ToggleExpand should switch a collapsed item to expanded.");

            item.ToggleExpand();

            Assert.IsFalse(item.expanded, "ToggleExpand should switch it back to collapsed.");
        }

        [Test]
        public void TreeViewItem_HeaderFocus_ShouldMoveFocusToHeader()
        {
            var item = MountElement(new TreeViewItem { label = "Item" });
            item.clickable = new Pressable(() => { });

            HeaderOf(item).Focus();
            simulate.FrameUpdate();

            Assert.AreEqual(HeaderOf(item), item.panel.focusController.focusedElement,
                "Focusing the header should make it the panel's focused element.");
        }
    }
}
#endif
