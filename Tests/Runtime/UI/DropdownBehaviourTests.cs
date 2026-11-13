#if APPUI_UITF_PRESENT
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dropdown))]
    class DropdownBehaviourTests : UIComponentTests<Dropdown>
    {
        static readonly string[] k_Items = { "Item 1", "Item 2", "Item 3" };

        Dropdown MountDropdown() => MountElement(new Dropdown { sourceItems = k_Items });

        static bool IsInside(VisualElement element, VisualElement ancestor)
        {
            for (var parent = element.hierarchy.parent; parent != null; parent = parent.hierarchy.parent)
            {
                if (parent == ancestor)
                    return true;
            }

            return false;
        }

        [Test]
        public void Dropdown_Click_ShouldOpenMenu()
        {
            var dropdown = MountDropdown();
            Assert.IsFalse(dropdown.ClassListContains(Styles.openUssClassName));

            simulate.Click(dropdown);

            Assert.IsTrue(dropdown.ClassListContains(Styles.openUssClassName),
                "Clicking the Dropdown should open its menu.");
        }

        [Test]
        public void Dropdown_WithNoSourceItems_Click_ShouldNotOpenMenu()
        {
            // Picker.OnClicked bails out before building a menu when there is nothing to show.
            var dropdown = MountElement(new Dropdown());

            simulate.Click(dropdown);

            Assert.IsFalse(dropdown.ClassListContains(Styles.openUssClassName));
        }

        [Test]
        public void Dropdown_ClickOnMenuItem_ShouldSetValue()
        {
            var dropdown = MountDropdown();

            simulate.Click(dropdown);

            // The popover needs a pass to lay out before its items have a worldBound worth
            // clicking. Without this the click lands at the origin and misses, and the only
            // symptom is an unchanged value.
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            // The menu is shown through a MenuBuilder popover, which parents into the Panel rather
            // than into the Dropdown, so it is queried from the panel -- but the Dropdown's own
            // trigger also holds a DropdownItem for the selected value, and that one has to be
            // filtered out or the count is off by one.
            var items = appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, dropdown))
                .ToList();
            Assert.AreEqual(k_Items.Length, items.Count, "The open menu should list every source item.");

            simulate.Click(items[1]);

            CollectionAssert.AreEquivalent(new[] { 1 }, dropdown.value);
        }

        [Test]
        public void Dropdown_OpenMenu_GrowsPastANarrowFieldForALongDescription()
        {
            var dropdown = MountElement(new Dropdown(k_Items, (item, i) =>
            {
                item.label = k_Items[i];
                item.description = "A much longer secondary line that cannot fit in the field";
            }));
            dropdown.style.width = 80;
            dropdown.style.flexGrow = 0;
            dropdown.style.alignSelf = Align.FlexStart;
            simulate.FrameUpdate();

            simulate.Click(dropdown);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var menu = appUiPanel.Q<Menu>(className: Picker.appuiPickerMenu);
            Assert.IsNotNull(menu, "The open Dropdown should show its menu.");
            var row = menu.Q<DropdownItem>();
            var description = row.Q<TextElement>(className: DropdownItem.descriptionUssClassName);
            var needed = description.MeasureTextSize(description.text, 0, VisualElement.MeasureMode.Undefined, 0,
                VisualElement.MeasureMode.Undefined).x;

            Assert.Greater(menu.worldBound.width, dropdown.worldBound.width,
                "The menu should grow past a narrow field rather than clip its options.");
            Assert.GreaterOrEqual(description.layout.width + 1f, needed,
                "The description should get the width its text needs.");
        }

        [Test]
        public void Dropdown_WhenDisabled_Click_ShouldNotOpenMenu()
        {
            var dropdown = MountDropdown();
            dropdown.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(dropdown);

            Assert.IsFalse(dropdown.ClassListContains(Styles.openUssClassName));
        }
    }
}
#endif
