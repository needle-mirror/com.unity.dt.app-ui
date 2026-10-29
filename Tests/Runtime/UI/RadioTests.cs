using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Radio))]
    class RadioTests : VisualElementTests<Radio>
    {
        protected override string mainUssClassName => Radio.ussClassName;
    }

    [TestFixture]
    [TestOf(typeof(RadioGroup))]
    class RadioGroupTests : VisualElementTests<RadioGroup>
    {
        protected override string mainUssClassName => RadioGroup.ussClassName;

        static RadioGroup CreateGroup(out Radio first, out Radio second)
        {
            var group = new RadioGroup();
            first = new Radio { key = "a" };
            second = new Radio { key = "b" };
            group.Add(first);
            group.Add(second);
            return group;
        }

        [UnityTest]
        public IEnumerator RadioGroup_KeepsRadiosRegistered_WhenReattached()
        {
            var doc = Utils.ConstructTestUI();
            var group = CreateGroup(out var first, out var second);
            doc.rootVisualElement.Add(group);
            yield return null;

            group.RemoveFromHierarchy();
            yield return null;
            doc.rootVisualElement.Add(group);
            yield return null;

            second.value = true;
            Assert.AreEqual("b", group.value);
            Assert.IsFalse(first.value, "Selecting another radio should deselect the first one.");
            first.value = true;
            Assert.AreEqual("a", group.value);
            Assert.IsFalse(second.value);
        }

        [UnityTest]
        public IEnumerator RadioGroup_WorksInsidePopover_WhenShownTwice()
        {
            var doc = Utils.ConstructTestUI();
            var anchor = new VisualElement();
            doc.rootVisualElement.Add(anchor);
            var content = new VisualElement();
            var group = CreateGroup(out var first, out var second);
            content.Add(group);
            yield return null;

            for (var i = 0; i < 2; i++)
            {
                var popover = Popover.Build(anchor, content);
                popover.Show();
                yield return null;
                yield return null;

                second.value = true;
                Assert.AreEqual("b", group.value, $"Show #{i + 1}");
                Assert.IsFalse(first.value, $"Show #{i + 1}");
                first.value = true;
                Assert.IsFalse(second.value, $"Show #{i + 1}");

                popover.Dismiss();
                yield return null;
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator RadioGroup_KeepsSelection_WhenReattached()
        {
            var doc = Utils.ConstructTestUI();
            var group = CreateGroup(out var first, out var second);
            doc.rootVisualElement.Add(group);
            yield return null;

            second.value = true;
            Assert.AreEqual("b", group.value);

            var changes = 0;
            group.RegisterCallback<ChangeEvent<string>>(_ => changes++);
            group.RemoveFromHierarchy();
            yield return null;
            Assert.AreEqual("b", group.value, "Detaching the group should not change its selection.");
            doc.rootVisualElement.Add(group);
            yield return null;

            Assert.AreEqual(0, changes, "Detaching and re-attaching the group should not send a change event.");
            Assert.AreEqual("b", group.value);
            Assert.IsFalse(first.value);
            Assert.IsTrue(second.value);
        }

        [UnityTest]
        public IEnumerator RadioGroup_KeepsSelection_WhenPopoverShownAgain()
        {
            var doc = Utils.ConstructTestUI();
            var anchor = new VisualElement();
            doc.rootVisualElement.Add(anchor);
            var content = new VisualElement();
            var group = CreateGroup(out var first, out var second);
            content.Add(group);
            yield return null;

            var popover = Popover.Build(anchor, content);
            popover.Show();
            yield return null;
            yield return null;
            second.value = true;
            popover.Dismiss();
            yield return null;
            yield return null;

            popover = Popover.Build(anchor, content);
            popover.Show();
            yield return null;
            yield return null;

            Assert.AreEqual("b", group.value, "Showing the popover again should keep the previous selection.");
            Assert.IsFalse(first.value);
            Assert.IsTrue(second.value);

            popover.Dismiss();
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator RadioGroup_RegistersRadio_WhenKeyChangedWhileDetached()
        {
            var doc = Utils.ConstructTestUI();
            var group = CreateGroup(out var first, out var second);
            doc.rootVisualElement.Add(group);
            yield return null;

            group.RemoveFromHierarchy();
            yield return null;
            second.key = "c";
            doc.rootVisualElement.Add(group);
            yield return null;

            second.value = true;
            Assert.AreEqual("c", group.value, "A radio whose key changed while detached should select its group once re-attached.");
            Assert.IsFalse(first.value);
            first.value = true;
            Assert.AreEqual("a", group.value);
            Assert.IsFalse(second.value);
        }

        [UnityTest]
        public IEnumerator RadioGroup_SelectsRemainingRadio_WhenSelectedRadioRemoved()
        {
            var doc = Utils.ConstructTestUI();
            var group = CreateGroup(out var first, out var second);
            doc.rootVisualElement.Add(group);
            yield return null;

            second.value = true;
            second.RemoveFromHierarchy();
            yield return null;

            Assert.AreEqual("a", group.value);
            Assert.IsTrue(first.value);
        }
    }
}
