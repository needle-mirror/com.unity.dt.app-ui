#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using EnumField = Unity.AppUI.UI.EnumField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(EnumField))]
    class EnumFieldBehaviourTests : UIComponentTests<EnumField>
    {
        static bool IsInside(VisualElement element, VisualElement ancestor)
        {
            for (var parent = element.hierarchy.parent; parent != null; parent = parent.hierarchy.parent)
            {
                if (parent == ancestor)
                    return true;
            }

            return false;
        }

        EnumField MountEnumField() => MountElement(new EnumField(EnumForTest.Value1));

        [Test]
        public void EnumField_Click_ShouldOpenMenu()
        {
            var field = MountEnumField();
            Assert.IsFalse(field.ClassListContains(Styles.openUssClassName));

            simulate.Click(field);

            Assert.IsTrue(field.ClassListContains(Styles.openUssClassName),
                "Clicking the EnumField should open its menu.");
        }

        [Test]
        public void EnumField_ClickOnMenuItem_ShouldSetValue()
        {
            var field = MountEnumField();

            simulate.Click(field);
            // The popover needs a pass to lay out before its items have a worldBound worth clicking.
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var items = appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, field))
                .ToList();
            Assert.AreEqual(3, items.Count, "The EnumForTest enum has 3 values, so the menu should list 3 items.");

            simulate.Click(items[2]);

            Assert.AreEqual(EnumForTest.Value3, field.value, "Clicking the third item should select Value3.");
        }

        [Test]
        public void EnumField_ClickOnMenuItem_ShouldNotifyValueChanged()
        {
            var field = MountEnumField();
            var enumNotified = 0;
            var selectionNotified = 0;
            field.RegisterCallback<ChangeEvent<System.Enum>>(_ => enumNotified++);
            field.RegisterCallback<ChangeEvent<IEnumerable<int>>>(_ => selectionNotified++);

            simulate.Click(field);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var items = appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, field))
                .ToList();
            simulate.Click(items[1]);

            // Picking from the menu goes through the Picker base, which tracks selected indices, so
            // the notification is a ChangeEvent<IEnumerable<int>>. ChangeEvent<Enum> comes from the
            // EnumField.value setter and so only fires on a programmatic assignment -- both are
            // asserted here so a change to either path is caught.
            Assert.AreEqual(1, selectionNotified,
                "Picking a menu item should send exactly one selection ChangeEvent.");
            Assert.AreEqual(0, enumNotified,
                "Picking a menu item notifies through the Picker's selection event, not ChangeEvent<Enum>.");

            field.value = EnumForTest.Value3;

            Assert.AreEqual(1, enumNotified,
                "Assigning EnumField.value should send exactly one ChangeEvent<Enum>.");
        }

        [Test]
        public void EnumField_WhenDisabled_Click_ShouldNotOpenMenu()
        {
            var field = MountEnumField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field);

            Assert.IsFalse(field.ClassListContains(Styles.openUssClassName),
                "A disabled EnumField should not open its menu.");
        }
    }
}
#endif
