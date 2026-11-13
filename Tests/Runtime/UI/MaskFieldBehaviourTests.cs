#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using MaskField = Unity.AppUI.UI.MaskField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(MaskField))]
    class MaskFieldBehaviourTests : UIComponentTests<MaskField>
    {
        static readonly List<int> k_Values = new() { 1, 2, 4 };

        static bool IsInside(VisualElement element, VisualElement ancestor)
        {
            for (var parent = element.hierarchy.parent; parent != null; parent = parent.hierarchy.parent)
            {
                if (parent == ancestor)
                    return true;
            }

            return false;
        }

        MaskField MountMaskField() => MountElement(new MaskField(k_Values));

        [Test]
        public void MaskField_Click_ShouldOpenMenu()
        {
            var field = MountMaskField();
            Assert.IsFalse(field.ClassListContains(Styles.openUssClassName));

            simulate.Click(field);

            Assert.IsTrue(field.ClassListContains(Styles.openUssClassName),
                "Clicking the MaskField should open its menu.");
        }

        [Test]
        public void MaskField_ClickOnItem_ShouldSetBitInValue()
        {
            var field = MountMaskField();

            simulate.Click(field);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var items = appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, field))
                .ToList();
            // "Everything" and "Nothing" come first, followed by one item per source value.
            Assert.AreEqual(k_Values.Count + 2, items.Count,
                "The menu should list Everything, Nothing, plus one item per source value.");

            simulate.Click(items[2]);

            Assert.AreEqual(k_Values[0], field.value, "Clicking the first value's item should set its bit.");
        }

        [Test]
        public void MaskField_ClickEverything_ShouldSetAllBits()
        {
            var field = MountMaskField();

            simulate.Click(field);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            var items = appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, field))
                .ToList();

            simulate.Click(items[0]);

            Assert.AreEqual(1 | 2 | 4, field.value, "Clicking Everything should OR every source value together.");
        }

        static readonly List<string> k_Names = new() { "A", "B", "C" };

        // The default texts are localization keys when com.unity.localization is installed.
        static NUnit.Framework.Constraints.Constraint k_Mixed => Is.EqualTo("Mixed...").Or.EqualTo("@AppUI:mixed");

        static NUnit.Framework.Constraints.Constraint k_Everything => Is.EqualTo("Everything").Or.EqualTo("@AppUI:everything");

        static NUnit.Framework.Constraints.Constraint k_Nothing => Is.EqualTo("Nothing").Or.EqualTo("@AppUI:nothing");

        static string TitleText(MaskField field) => field.Q<DropdownItem>().label;

        List<DropdownItem> OpenMenuItems(MaskField field)
        {
            simulate.Click(field);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            return appUiPanel.Query<DropdownItem>().ToList()
                .Where(item => !IsInside(item, field))
                .ToList();
        }

        MaskField MountNamedMaskField(int value = 0)
        {
            var field = new MaskField(k_Names, value, getMaskValueFunc: i => k_Values[i]);
            return MountElement(field);
        }

        [Test]
        public void MaskField_Defaults_ShouldKeepMixedSummaryAndAutoEverything()
        {
            var field = MountNamedMaskField(1 | 2);
            Assert.AreEqual(MaskFieldSummary.Mixed, field.summary);
            Assert.AreEqual(0, field.everythingValue);
            Assert.That(TitleText(field), k_Mixed);
            field.SetValueWithoutNotify(1 | 2 | 4);
            Assert.That(TitleText(field), k_Everything);
            field.SetValueWithoutNotify(0);
            Assert.That(TitleText(field), k_Nothing);
            field.SetValueWithoutNotify(2);
            Assert.AreEqual("B", TitleText(field));
        }

        [Test]
        public void MaskField_SummaryList_ShouldJoinSelectedNames()
        {
            var field = MountNamedMaskField(1 | 4);
            field.summary = MaskFieldSummary.List;
            Assert.AreEqual("A, C", TitleText(field));
        }

        [Test]
        public void MaskField_UnnamedBit_ShouldReadAsMixedAndBeKept()
        {
            var field = MountNamedMaskField();
            field.summary = MaskFieldSummary.List;
            field.SetValueWithoutNotify(1 | 8);
            Assert.AreEqual(1 | 8, field.value, "A bit no item names must survive in the value.");
            Assert.That(TitleText(field), k_Mixed, "An unnamed bit reads as mixed, even in list mode.");
        }

        [Test]
        public void MaskField_EverythingValue_ShouldBeWrittenAndRecognised()
        {
            var field = MountNamedMaskField();
            field.everythingValue = -1;
            field.SetValueWithoutNotify(1 | 2 | 4);
            Assert.That(TitleText(field), k_Mixed, "With a -1 sentinel, the OR of the items is not Everything.");
            field.SetValueWithoutNotify(-1);
            Assert.AreEqual(-1, field.value);
            Assert.That(TitleText(field), k_Everything);

            field.SetValueWithoutNotify(0);
            var items = OpenMenuItems(field);
            simulate.Click(items[0]);
            Assert.AreEqual(-1, field.value, "The Everything row should write the sentinel.");
        }

        [Test]
        public void MaskField_ToggleUnderSentinel_ShouldClearOnlyThatBit()
        {
            var field = MountNamedMaskField();
            field.everythingValue = -1;
            field.SetValueWithoutNotify(-1);
            var items = OpenMenuItems(field);
            simulate.Click(items[3]);
            Assert.AreEqual(-1 ^ 2, field.value);
        }

        [Test]
        public void MaskField_ValueChangingOnlyUnnamedBits_ShouldRefreshTitleAndValidate()
        {
            var field = MountNamedMaskField(1 | 2 | 4);
            field.everythingValue = -1;
            field.validateValue = v => v != -1;
            Assert.IsFalse(field.invalid);
            field.value = -1;
            Assert.That(TitleText(field), k_Everything);
            Assert.IsTrue(field.invalid, "Validation should run when only unnamed bits change.");
            field.value = 1 | 2 | 4;
            Assert.That(TitleText(field), k_Mixed);
            Assert.IsFalse(field.invalid);
        }

        [Test]
        public void MaskField_AddedItem_ShouldKeepTheBitItNowNames()
        {
            var values = new List<int> { 1, 2, 4 };
            var field = MountElement(new MaskField(values));
            field.everythingValue = -1;
            field.SetValueWithoutNotify(-1);
            values.Add(8);
            field.Refresh();
            Assert.AreEqual(-1, field.value);
            Assert.That(TitleText(field), k_Everything);
        }

        [Test]
        public void MaskField_Labels_ShouldReplaceSummaryAndRowText()
        {
            var field = MountNamedMaskField();
            field.nothingLabel = "None";
            field.everythingLabel = "All";
            field.mixedLabel = "Several";
            Assert.AreEqual("None", TitleText(field));
            field.SetValueWithoutNotify(1 | 2);
            Assert.AreEqual("Several", TitleText(field));
            field.SetValueWithoutNotify(1 | 2 | 4);
            Assert.AreEqual("All", TitleText(field));
            var items = OpenMenuItems(field);
            Assert.AreEqual("All", items[0].label);
            Assert.AreEqual("None", items[1].label);
        }

        [Test]
        public void MaskField_WhenDisabled_Click_ShouldNotOpenMenu()
        {
            var field = MountMaskField();
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field);

            Assert.IsFalse(field.ClassListContains(Styles.openUssClassName),
                "A disabled MaskField should not open its menu.");
        }
    }
}
#endif
