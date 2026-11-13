using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MenuRadioGroup))]
    class MenuRadioGroupTests : VisualElementTests<MenuRadioGroup>
    {
        protected override string mainUssClassName => MenuRadioGroup.ussClassName;

        // Handed over to MenuRadioGroupSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static MenuRadioGroup MakeGroup(string value = "md")
        {
            var group = new MenuRadioGroup { value = value };
            group.Add(new MenuItem { key = "sm", label = "Small" });
            group.Add(new MenuItem { key = "md", label = "Medium" });
            group.Add(new MenuItem { key = "lg", label = "Large" });
            return group;
        }

        protected override IEnumerable<Story> stories
        {
            get { yield return new Story("Default", ctx => MakeGroup()); }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:MenuRadioGroup />",
            @"<appui:MenuRadioGroup value=""md""><appui:MenuItem key=""sm"" label=""Small"" /><appui:MenuItem key=""md"" label=""Medium"" /></appui:MenuRadioGroup>",
        };

        [UnityTest]
        public IEnumerator Items_BecomeRadioRows()
        {
            var group = MakeGroup();
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new Menu();
            panel.Add(menu);
            menu.Add(group);

            for (var i = 0; i < 4; i++)
                yield return null;

            foreach (var item in group.Query<MenuItem>().ToList())
            {
                Assert.AreEqual(MenuItemIndicator.Radio, item.indicator,
                    $"'{item.key}' must render as a radio row inside a MenuRadioGroup.");
                Assert.IsTrue(item.selectable,
                    $"'{item.key}' must be selectable so it can show its selection.");
            }
        }

        [UnityTest]
        public IEnumerator Value_SelectsExactlyOneItem()
        {
            var group = MakeGroup("md");
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new Menu();
            panel.Add(menu);
            menu.Add(group);

            for (var i = 0; i < 4; i++)
                yield return null;

            var selected = new List<string>();
            foreach (var item in group.Query<MenuItem>().ToList())
            {
                if (item.value)
                    selected.Add(item.key);
            }
            CollectionAssert.AreEqual(new[] { "md" }, selected);

            group.value = "lg";
            yield return null;

            selected.Clear();
            foreach (var item in group.Query<MenuItem>().ToList())
            {
                if (item.value)
                    selected.Add(item.key);
            }
            CollectionAssert.AreEqual(new[] { "lg" }, selected,
                "Changing the group value must move the selection, not add to it.");
        }

        [UnityTest]
        public IEnumerator ChoosingAnItem_UpdatesTheGroupValue_AndNotifies()
        {
            var group = MakeGroup("md");
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new Menu();
            panel.Add(menu);
            menu.Add(group);

            string observed = null;
            group.RegisterValueChangedCallback(evt => observed = evt.newValue);

            for (var i = 0; i < 4; i++)
                yield return null;

            MenuItem large = null;
            foreach (var item in group.Query<MenuItem>().ToList())
            {
                if (item.key == "lg")
                    large = item;
            }
            large.value = true;
            yield return null;

            Assert.AreEqual("lg", group.value);
            Assert.AreEqual("lg", observed, "The group must raise a ChangeEvent<string> for its new value.");
        }

        [UnityTest]
        public IEnumerator SelectedItem_CannotBeToggledOff()
        {
            var group = MakeGroup("md");
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new Menu();
            panel.Add(menu);
            menu.Add(group);

            for (var i = 0; i < 4; i++)
                yield return null;

            MenuItem medium = null;
            foreach (var item in group.Query<MenuItem>().ToList())
            {
                if (item.key == "md")
                    medium = item;
            }

            medium.value = false;
            yield return null;

            Assert.AreEqual("md", group.value, "Deselecting the active radio must not clear the group.");
            Assert.IsTrue(medium.value, "The active radio must stay selected — radios are not toggles.");
        }
    }
}
