#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionBar))]
    class ActionBarBehaviourTests : UIComponentTests<ActionBar>
    {
        static UnityEngine.UIElements.ListView NewListView()
        {
            var items = new List<string> { "A", "B", "C" };
            return new UnityEngine.UIElements.ListView
            {
                itemsSource = items,
                selectionType = SelectionType.Multiple,
                makeItem = () => new Label(),
                bindItem = (el, i) => ((Label)el).text = items[i],
            };
        }

        static Checkbox SelectAllCheckboxOf(ActionBar actionBar) =>
            actionBar.Q<Checkbox>(name: ActionBar.checkboxUssClassName);

        [Test]
        public void ActionBar_CollectionSelectionChanges_ShouldCheckSelectAllOnlyWhenEverythingIsSelected()
        {
            var actionBar = MountElement(new ActionBar { message = "{0} Selected item(s)" });
            var listView = NewListView();
            actionBar.collectionView = listView;
            var checkbox = SelectAllCheckboxOf(actionBar);
            Assert.AreEqual(CheckboxState.Unchecked, checkbox.value,
                "Nothing is selected yet, so the select-all checkbox should be unchecked.");

            listView.SetSelection(new[] { 0 });
            Assert.AreEqual(CheckboxState.Intermediate, checkbox.value,
                "A partial selection should show as intermediate.");

            listView.SetSelection(new[] { 0, 1, 2 });
            Assert.AreEqual(CheckboxState.Checked, checkbox.value,
                "Selecting every item should check the select-all checkbox.");
        }

        [Test]
        public void ActionBar_ClickSelectAllCheckbox_ShouldSelectEveryItemInTheCollectionView()
        {
            var actionBar = MountElement(new ActionBar());
            var listView = NewListView();
            actionBar.collectionView = listView;
            var checkbox = SelectAllCheckboxOf(actionBar);

            simulate.Click(checkbox);

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, listView.selectedIndices,
                "Checking the select-all checkbox should select every source item.");
        }

        [Test]
        public void ActionBar_ClickSelectAllCheckboxWhenFullySelected_ShouldClearSelection()
        {
            var actionBar = MountElement(new ActionBar());
            var listView = NewListView();
            actionBar.collectionView = listView;
            listView.SetSelection(new[] { 0, 1, 2 });
            var checkbox = SelectAllCheckboxOf(actionBar);
            Assert.AreEqual(CheckboxState.Checked, checkbox.value, "Precondition: everything is selected.");

            simulate.Click(checkbox);

            CollectionAssert.IsEmpty(listView.selectedIndices,
                "Unchecking the select-all checkbox should clear the collection view's selection.");
        }
    }
}
#endif
