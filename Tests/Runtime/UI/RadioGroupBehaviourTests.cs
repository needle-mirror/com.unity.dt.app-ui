#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RadioGroup))]
    class RadioGroupBehaviourTests : UIComponentTests<RadioGroup>
    {
        RadioGroup MountGroup(out Radio small, out Radio medium, out Radio large)
        {
            var group = new RadioGroup();
            small = new Radio { key = "small", label = "Small" };
            medium = new Radio { key = "medium", label = "Medium" };
            large = new Radio { key = "large", label = "Large" };
            group.Add(small);
            group.Add(medium);
            group.Add(large);
            return MountElement(group);
        }

        [Test]
        public void RadioGroup_ClickOnChildRadio_ShouldSetGroupValue()
        {
            var group = MountGroup(out _, out var medium, out _);
            Assert.AreEqual("small", group.value, "The first Radio added becomes the initial value.");

            simulate.Click(medium);

            Assert.AreEqual("medium", group.value, "Clicking a child Radio should select it in the group.");
        }

        [Test]
        public void RadioGroup_ClickOnChildRadio_ShouldUncheckPreviousRadio()
        {
            var group = MountGroup(out var small, out var medium, out _);
            Assert.IsTrue(small.value, "The first Radio should start checked as the group's initial value.");

            simulate.Click(medium);

            Assert.IsFalse(small.value, "Selecting another Radio should uncheck the previously selected one.");
            Assert.IsTrue(medium.value, "The clicked Radio should become checked.");
        }

        [Test]
        public void RadioGroup_ClickOnChildRadio_ShouldSendValueChangedEvent()
        {
            var group = MountGroup(out _, out _, out var large);
            var notified = new System.Collections.Generic.List<string>();
            group.RegisterValueChangedCallback(evt => notified.Add(evt.newValue));

            simulate.Click(large);

            CollectionAssert.AreEqual(new[] { "large" }, notified);
        }
    }
}
#endif
