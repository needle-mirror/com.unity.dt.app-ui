#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.AppUI.UI;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Tabs))]
    class TabsBehaviourTests : UIComponentTests<Tabs>
    {
        static readonly List<string> k_Labels = new() { "Tab 0", "Tab 1", "Tab 2" };

        Tabs MountTabs() => MountElement(new Tabs
        {
            sourceItems = k_Labels,
            bindItem = (tab, i) => tab.label = k_Labels[i],
        });

        static List<TabItem> TabItemsOf(Tabs tabs) => tabs.Query<TabItem>().ToList();

        [Test]
        public void Tabs_ClickOnTabItem_ShouldChangeValue()
        {
            var tabs = MountTabs();
            var items = TabItemsOf(tabs);
            Assert.AreEqual(k_Labels.Count, items.Count, "Every source item should produce a TabItem.");
            Assert.AreEqual(0, tabs.value, "The first tab should be selected initially.");

            simulate.Click(items[2]);

            Assert.AreEqual(2, tabs.value, "Clicking a tab should select it.");
        }

        [Test]
        public void Tabs_ClickOnTabItem_ShouldNotifyValueChanged()
        {
            var tabs = MountTabs();
            var notified = new List<int>();
            tabs.RegisterValueChangedCallback(evt => notified.Add(evt.newValue));

            simulate.Click(TabItemsOf(tabs)[1]);

            Assert.AreEqual(new[] { 1 }, notified.ToArray());
        }

        [Test]
        public void Tabs_ClickOnSelectedTabItem_ShouldNotNotifyAgain()
        {
            var tabs = MountTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[1]);

            var notifications = 0;
            tabs.RegisterValueChangedCallback(_ => notifications++);

            simulate.Click(items[1]);

            Assert.AreEqual(0, notifications, "Re-clicking the selected tab should not notify.");
            Assert.AreEqual(1, tabs.value);
        }

        [Test]
        public void Tabs_ClickOnDisabledTabItem_ShouldNotChangeValue()
        {
            var tabs = MountTabs();
            var items = TabItemsOf(tabs);
            items[2].SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(items[2]);

            Assert.AreEqual(0, tabs.value, "A disabled tab should not become selected.");
        }

        // UXML-authored tabs: the children are picked up by Tabs' hierarchy poll, which runs every 50ms.
        Tabs MountStaticTabs()
        {
            var tabs = new Tabs();
            foreach (var label in k_Labels)
                tabs.Add(new TabItem { label = label });
            MountElement(tabs);
            simulate.FrameUpdateMs(100);
            simulate.FrameUpdate();
            return tabs;
        }

        // UI Builder deletes TabItems straight out of the item container (UUM-151402).
        [Test]
        public void Tabs_RemovingAllTabItemsFromItemContainer_ShouldClearSelectionWithoutThrowing()
        {
            var tabs = MountStaticTabs();
            Assert.AreEqual(k_Labels.Count, TabItemsOf(tabs).Count);
            Assert.AreEqual(0, tabs.value);

            tabs.itemContainer.Clear();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(-1, tabs.value, "No tab is left to select.");
            Assert.AreEqual(0, tabs.items.Count, "items should no longer list the removed tabs.");
        }

        [Test]
        public void Tabs_RemovingSelectedTabItemFromItemContainer_ShouldSelectFirstRemainingTab()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[1]);

            items[1].RemoveFromHierarchy();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.IsTrue(items[0].selected);
        }

        [Test]
        public void Tabs_RemovingTabItemBeforeSelectedOne_ShouldKeepSameTabSelected()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[2]);

            items[0].RemoveFromHierarchy();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(1, tabs.value, "The selected tab moved up one slot.");
            Assert.IsTrue(items[2].selected);
        }

        class SelectedTabSource
        {
            [CreateProperty]
            public int selected { get; set; } = -2;
        }

        [Test]
        public void Tabs_RemovingTabItemBeforeSelectedOne_ShouldNotifyShiftedValue()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            var source = new SelectedTabSource();
            tabs.SetBinding(nameof(Tabs.value), new DataBinding
            {
                dataSource = source,
                dataSourcePath = new PropertyPath(nameof(SelectedTabSource.selected)),
                bindingMode = BindingMode.ToSource,
            });
            simulate.FrameUpdate();
            simulate.Click(items[2]);
            simulate.FrameUpdate();
            Assert.AreEqual(2, source.selected);
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[0].RemoveFromHierarchy();
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.AreEqual(1, tabs.value);
            Assert.AreEqual(new[] { (2, 1) }, notified.ToArray(), "Listeners should learn the selected index moved.");
            Assert.AreEqual(1, source.selected, "Bindings on value should follow the selected tab.");
        }

        [Test]
        public void Tabs_RemovingSourceBoundTabItemFromItemContainer_ShouldKeepValueIndexingSourceItems()
        {
            var unbound = new List<(TabItem, int)>();
            var tabs = MountElement(new Tabs
            {
                sourceItems = k_Labels,
                bindItem = (tab, i) => tab.label = k_Labels[i],
                unbindItem = (tab, i) => unbound.Add((tab, i)),
            });
            simulate.FrameUpdate();
            tabs.value = 2;
            var removed = TabItemsOf(tabs)[1];
            unbound.Clear();

            removed.RemoveFromHierarchy();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(2, tabs.value, "The source did not change, so the selection should still index Tab 2.");
            Assert.AreEqual("Tab 2", tabs.items[tabs.value]);
            var items = TabItemsOf(tabs);
            Assert.AreEqual(k_Labels.ToArray(), items.Select(t => t.label).ToArray(), "The views should be rebuilt from the source.");
            Assert.IsTrue(items[2].selected);
            Assert.AreEqual(1, items.Count(t => t.selected));
            Assert.IsTrue(unbound.Contains((removed, 1)), "The removed view should be unbound.");
        }

        [Test]
        public void Tabs_RemovingSourceBoundTabItemFromItemContainer_ShouldUnbindRemainingViewsWhileAttached()
        {
            var parents = new Dictionary<TabItem, VisualElement>();
            var tabs = MountElement(new Tabs
            {
                sourceItems = k_Labels,
                bindItem = (tab, i) => tab.label = k_Labels[i],
                unbindItem = (tab, i) => parents[tab] = tab.parent,
            });
            simulate.FrameUpdate();
            var views = TabItemsOf(tabs);

            views[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(0);

            Assert.AreEqual(tabs.itemContainer, parents[views[0]], "A view still in the container should be unbound before it is detached.");
            Assert.AreEqual(tabs.itemContainer, parents[views[2]], "A view still in the container should be unbound before it is detached.");
            Assert.IsNull(parents[views[1]], "The view removed from outside should still be unbound.");
        }

        [Test]
        public void Tabs_UnbindHandlerReadingValueDuringResync_ShouldUnbindEachViewOnce()
        {
            var unbound = new List<(TabItem, int)>();
            var readValues = new List<int>();
            var tabs = MountTabs();
            tabs.unbindItem = (tab, i) =>
            {
                unbound.Add((tab, i));
                readValues.Add(tabs.value);
                Assert.AreEqual(k_Labels.Count, tabs.items.Count);
            };
            simulate.FrameUpdate();
            tabs.value = 2;
            var views = TabItemsOf(tabs);
            unbound.Clear();
            readValues.Clear();

            views[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(new[] { (views[0], 0), (views[1], 1), (views[2], 2) }, unbound.ToArray(), "Each old view should be unbound exactly once.");
            Assert.AreEqual(new[] { 2, 2, 2 }, readValues.ToArray(), "The handler should see the selection it had before the rebuild.");
            Assert.AreEqual(2, tabs.value);
            Assert.AreEqual(k_Labels.ToArray(), TabItemsOf(tabs).Select(t => t.label).ToArray());
        }

        [Test]
        public void Tabs_RemovingSelectedTabItem_ShouldNotifyFallbackValue()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[1]);
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(0);

            Assert.AreEqual(0, tabs.value);
            Assert.AreEqual(new[] { (1, 0) }, notified.ToArray(), "Only the reconciliation should notify, not SetValueWithoutNotify.");
        }

        [Test]
        public void Tabs_SettingValueRightAfterRemovingTabItem_ShouldUseCurrentIndices()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[1].RemoveFromHierarchy();
            tabs.value = 1;

            Assert.AreEqual(1, tabs.value);
            Assert.IsTrue(items[2].selected, "Index 1 is now the third tab.");
            Assert.AreEqual(new[] { (0, 1) }, notified.ToArray());
        }

        Tabs MountStaticTabsClearedByFirstValueChange()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[2]);
            var cleared = false;
            tabs.RegisterValueChangedCallback(_ =>
            {
                if (cleared)
                    return;
                cleared = true;
                tabs.itemContainer.Clear();
            });
            items[0].RemoveFromHierarchy();
            return tabs;
        }

        [Test]
        public void Tabs_SetValueWithoutNotifyWhenResyncHandlerClearsItemContainer_ShouldNotSelectRemovedTab()
        {
            var tabs = MountStaticTabsClearedByFirstValueChange();

            Assert.DoesNotThrow(() => tabs.SetValueWithoutNotify(1));

            Assert.AreEqual(-1, tabs.value, "The handler removed every tab.");
            Assert.AreEqual(0, tabs.items.Count);
        }

        [Test]
        public void Tabs_SettingValueWhenResyncHandlerClearsItemContainer_ShouldNotSelectRemovedTab()
        {
            var tabs = MountStaticTabsClearedByFirstValueChange();

            Assert.DoesNotThrow(() => tabs.value = 0);

            Assert.AreEqual(-1, tabs.value, "The handler removed every tab.");
            Assert.AreEqual(0, tabs.items.Count);
        }

        [Test]
        public void Tabs_ResyncHandlerRemovingSelectedTabOnEveryChange_ShouldStopOnceItemContainerIsEmpty()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[2]);
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt =>
            {
                notified.Add((evt.previousValue, evt.newValue));
                if (tabs.itemContainer.childCount > 0)
                    tabs.itemContainer.RemoveAt(tabs.itemContainer.childCount - 1);
                _ = tabs.value;
            });

            items[0].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(0);

            Assert.AreEqual(-1, tabs.value);
            Assert.AreEqual(new[] { (2, 1), (1, 0), (0, -1) }, notified.ToArray());
        }

        [Test]
        public void Tabs_ReaddingRemovedSelectedTabItem_ShouldNotLeaveTwoTabsSelected()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[1]);

            items[1].RemoveFromHierarchy();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);
            tabs.itemContainer.Insert(1, items[1]);
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.AreEqual(1, items.Count(item => item.selected), "Only the fallback tab should be selected.");
        }

        [Test]
        public void Tabs_ReaddingTabItemAfterRemovingAll_ShouldSelectIt()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);

            tabs.itemContainer.Clear();
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);
            tabs.itemContainer.Add(items[0]);
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.IsTrue(items[0].selected);
        }

        [Test]
        public void Tabs_AddingTabItemWhileNothingSelected_ShouldKeepNothingSelected()
        {
            var tabs = MountStaticTabs();
            tabs.value = -1;

            tabs.itemContainer.Add(new TabItem { label = "Tab 3" });
            simulate.FrameUpdate();

            Assert.AreEqual(-1, tabs.value, "An explicit empty selection is kept.");
        }

        [Test]
        public void Tabs_AddingAlreadySelectedTabItem_ShouldNotLeaveTwoTabsSelected()
        {
            var tabs = MountStaticTabs();
            var incoming = new TabItem { label = "Tab 3", selected = true };

            tabs.itemContainer.Add(incoming);
            Assert.AreEqual(0, tabs.value);

            Assert.IsFalse(incoming.selected, "An incoming tab should not stay selected when value points elsewhere.");
            Assert.AreEqual(1, TabItemsOf(tabs).Count(item => item.selected), "Only the tab at value should be selected.");
        }

        [Test]
        public void Tabs_ReselectingFallbackTabAfterRemovingSelectedOne_ShouldMarkItSelected()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            simulate.Click(items[1]);

            items[1].RemoveFromHierarchy();
            Assert.AreEqual(0, tabs.value);
            tabs.value = 0;

            Assert.IsTrue(items[0].selected, "The fallback tab reported by value should be selected.");
        }

        [Test]
        public void Tabs_AddingTabItemToEmptyItemContainer_ShouldExposeItInItems()
        {
            var tabs = MountElement(new Tabs());
            simulate.FrameUpdateMs(100);
            var item = new TabItem { label = "Tab 0" };

            tabs.itemContainer.Add(item);

            Assert.IsNotNull(tabs.items, "items should list a TabItem added straight to the item container.");
            Assert.AreEqual(new[] { item }, tabs.items.Cast<TabItem>().ToArray());
        }

        [Test]
        public void Tabs_AddingTabItemToEmptyItemContainer_ShouldStopHierarchyPoll()
        {
            var tabs = MountElement(new Tabs());
            simulate.FrameUpdateMs(100);

            tabs.itemContainer.Add(new TabItem { label = "Tab 0" });
            Assert.AreEqual(1, tabs.items.Count);

            var poll = (IVisualElementScheduledItem)typeof(Tabs)
                .GetField("m_PollHierarchyItem", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(tabs);
            Assert.IsTrue(poll == null || !poll.isActive, "The hierarchy poll can never fire once the static items are known, so it should be stopped.");
        }

        [Test]
        public void Tabs_ClickingTabAfterNonTabItemChild_ShouldSelectItsTabIndex()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            tabs.itemContainer.Insert(0, new VisualElement());
            simulate.FrameUpdate();

            simulate.Click(items[2]);

            Assert.AreEqual(2, tabs.value, "value indexes the tabs, not every child of the item container.");
            Assert.IsTrue(items[2].selected);
        }

        [Test]
        public void Tabs_NonTabItemChildInItemContainer_ShouldNotResyncOnEveryRead()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            tabs.itemContainer.Add(new VisualElement());
            Assert.AreEqual(k_Labels.Count, tabs.items.Count);

            // A resync re-marks the selected tab, so clearing the flag by hand shows whether one ran.
            items[0].selected = false;
            _ = tabs.items;

            Assert.IsFalse(items[0].selected, "The tabs did not change, so reading them should not resync.");
        }

        [Test]
        public void Tabs_NonTabItemChildInUxml_ShouldBeIgnoredByHierarchyPoll()
        {
            var tabs = new Tabs();
            var item = new TabItem { label = "Tab 0" };
            tabs.Add(item);
            tabs.Add(new VisualElement());
            MountElement(tabs);

            simulate.FrameUpdateMs(100);
            simulate.FrameUpdate();

            Assert.AreEqual(new[] { item }, tabs.items.Cast<TabItem>().ToArray());
            Assert.AreEqual(0, tabs.value);
        }

        Tabs MountFixedSizeStaticTabsWithLastSelected()
        {
            var tabs = MountStaticTabs();
            tabs.style.width = 400;
            tabs.style.height = 40;
            simulate.FrameUpdate();
            tabs.SetValueWithoutNotify(2);
            simulate.FrameUpdateMs(500);
            simulate.FrameUpdate();
            return tabs;
        }

        [Test]
        public void Tabs_RemovingSelectedTabItemFromItemContainer_ShouldMoveIndicatorToFallbackTab()
        {
            var tabs = MountFixedSizeStaticTabsWithLastSelected();
            var items = TabItemsOf(tabs);
            var indicator = tabs.Q(className: Tabs.indicatorUssClassName);
            Assert.AreEqual(items[2].layout.x, indicator.resolvedStyle.left, 0.5f);

            items[2].RemoveFromHierarchy();
            Assert.AreEqual(0, tabs.value);
            simulate.FrameUpdateMs(500);
            simulate.FrameUpdate();

            Assert.AreEqual(items[0].layout.x, indicator.resolvedStyle.left, 0.5f, "The indicator should follow the fallback tab.");
            Assert.AreEqual(items[0].layout.width, indicator.resolvedStyle.width, 0.5f);
        }

        [Test]
        public void Tabs_RemovingAllTabItemsFromItemContainer_ShouldHideIndicator()
        {
            var tabs = MountFixedSizeStaticTabsWithLastSelected();
            var indicator = tabs.Q(className: Tabs.indicatorUssClassName);
            Assert.Greater(indicator.resolvedStyle.width, 0f);

            tabs.itemContainer.Clear();
            Assert.AreEqual(-1, tabs.value);
            simulate.FrameUpdateMs(500);
            simulate.FrameUpdate();

            Assert.AreEqual(0f, indicator.resolvedStyle.width, 0.5f, "No tab is selected, so the indicator should be hidden.");
        }

        [Test]
        public void Tabs_AssigningUnbindItemRightAfterRemovingStaticTabItem_ShouldNotReaddIt()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);

            items[1].RemoveFromHierarchy();
            tabs.unbindItem = (tab, i) => { };

            Assert.IsNull(items[1].parent, "The removed tab should stay out of the item container.");
            Assert.AreEqual(new[] { items[0], items[2] }, tabs.items.Cast<TabItem>().ToArray());
        }

        [Test]
        public void Tabs_AssigningBindItemRightAfterAddingStaticTabItem_ShouldKeepIt()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);
            var incoming = new TabItem { label = "Tab 3" };

            tabs.itemContainer.Add(incoming);
            tabs.bindItem = (tab, i) => { };

            Assert.AreEqual(tabs.itemContainer, incoming.parent, "The added tab should stay in the item container.");
            Assert.AreEqual(items.Append(incoming).ToArray(), tabs.items.Cast<TabItem>().ToArray());
        }

        [Test]
        public void Tabs_ClearingSourceItemsAfterRemovingStaticTabItem_ShouldNotReaddIt()
        {
            var tabs = MountStaticTabs();
            var items = TabItemsOf(tabs);

            items[1].RemoveFromHierarchy();
            tabs.sourceItems = new List<string> { "A" };
            tabs.sourceItems = null;

            Assert.IsNull(items[1].parent, "The removed tab should stay out of the item container.");
            Assert.AreEqual(new[] { items[0], items[2] }, tabs.items.Cast<TabItem>().ToArray());
        }
    }
}
#endif
