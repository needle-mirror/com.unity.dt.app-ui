using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Tabs))]
    class TabsTests : VisualElementTests<Tabs>
    {
        protected override string mainUssClassName => Tabs.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                var sourceItems = new List<string>();
                for (var i = 0; i < 2; ++i)
                {
                    sourceItems.Add($"Tab {i}");
                }
                yield return new Story("Default", (ctx) => new Tabs
                {
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
                yield return new Story("Justified", (ctx) => new Tabs
                {
                    justified = true,
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
                yield return new Story("Vertical", (ctx) => new Tabs
                {
                    direction = Direction.Vertical,
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.icon = "info";
                    }
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Tabs />",
            @"<appui:Tabs>
                <appui:TabItem label=""Tab 1"" icon=""info"" />
                <appui:TabItem label=""Tab 2"" icon=""info"" enabled=""false"" />
                <appui:TabItem label=""Tab 3"" icon=""info"" />
            </appui:Tabs>",
            @"<appui:Tabs emphasized=""true"" justified=""true"">
                <appui:TabItem label=""Tab 1"" icon=""info"" />
                <appui:TabItem label=""Tab 2"" icon=""info"" enabled=""false"" />
                <appui:TabItem label=""Tab 3"" icon=""info"" />
            </appui:Tabs>",
            @"<appui:Tabs emphasized=""true"" direction=""Vertical"">
                <appui:TabItem label=""Tab 1"" icon=""info"" />
                <appui:TabItem label=""Tab 2"" icon=""info"" enabled=""false"" />
                <appui:TabItem label=""Tab 3"" icon=""info"" />
            </appui:Tabs>",
        };

        [UnityTest]
        public IEnumerator CanBindItems()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            var sourceItems = new List<string>();
            for (var i = 0; i < 2; ++i)
            {
                sourceItems.Add($"Tab {i}");
            }
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) =>
                {
                    tab.label = sourceItems[i];
                    tab.icon = "info";
                }
            };

            panel.Add(tabs);

            yield return null;

            var container = tabs.Q<VisualElement>(Tabs.containerUssClassName);
            Assert.NotNull(container);
            Assert.AreEqual(2, container.childCount);
            Assert.IsTrue(container[0] is TabItem);
            Assert.AreEqual("Tab 0", ((TabItem)container[0]).label);
            Assert.AreEqual(0, tabs.value);
        }

        Tabs m_StaticTabs;

        List<TabItem> m_StaticTabItems;

        // UXML-authored tabs: the children are picked up by Tabs' hierarchy poll, which runs every 50ms.
        IEnumerator MountStaticTabs()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            m_StaticTabs = new Tabs();
            for (var i = 0; i < 3; ++i)
                m_StaticTabs.Add(new TabItem { label = $"Tab {i}" });
            panel.Add(m_StaticTabs);

            yield return new WaitForSeconds(0.2f);
            yield return null;

            m_StaticTabItems = m_StaticTabs.Query<TabItem>().ToList();
            Assert.AreEqual(3, m_StaticTabItems.Count);
        }

        // UI Builder deletes TabItems straight out of the item container (UUM-151402).
        [UnityTest]
        public IEnumerator RemovingAllTabItemsFromItemContainer_ClearsSelectionWithoutThrowing()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            Assert.AreEqual(0, tabs.value);

            tabs.itemContainer.Clear();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(-1, tabs.value, "No tab is left to select.");
            Assert.AreEqual(0, tabs.items.Count, "items should no longer list the removed tabs.");
        }

        [UnityTest]
        public IEnumerator RemovingSelectedTabItemFromItemContainer_SelectsFirstRemainingTab()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 1;

            items[1].RemoveFromHierarchy();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.IsTrue(items[0].selected);
        }

        [UnityTest]
        public IEnumerator RemovingTabItemBeforeSelectedOne_KeepsSameTabSelectedAndNotifies()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 2;
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[0].RemoveFromHierarchy();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(1, tabs.value, "The selected tab moved up one slot.");
            Assert.IsTrue(items[2].selected);
            Assert.AreEqual(new[] { (2, 1) }, notified.ToArray(), "Listeners should learn the selected index moved.");
        }

        [UnityTest]
        public IEnumerator RemovingSelectedTabItem_NotifiesFallbackValueOnce()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 1;
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(0);

            Assert.AreEqual(0, tabs.value);
            Assert.AreEqual(new[] { (1, 0) }, notified.ToArray(), "Only the reconciliation should notify, not SetValueWithoutNotify.");
        }

        [UnityTest]
        public IEnumerator SettingValueRightAfterRemovingTabItem_UsesCurrentIndices()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            var notified = new List<(int, int)>();
            tabs.RegisterValueChangedCallback(evt => notified.Add((evt.previousValue, evt.newValue)));

            items[1].RemoveFromHierarchy();
            tabs.value = 1;

            Assert.AreEqual(1, tabs.value);
            Assert.IsTrue(items[2].selected, "Index 1 is now the third tab.");
            Assert.AreEqual(new[] { (0, 1) }, notified.ToArray());
        }

        [UnityTest]
        public IEnumerator ReaddingRemovedSelectedTabItem_DoesNotLeaveTwoTabsSelected()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 1;

            items[1].RemoveFromHierarchy();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);
            tabs.itemContainer.Insert(1, items[1]);
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.AreEqual(1, items.Count(item => item.selected), "Only the fallback tab should be selected.");
        }

        [UnityTest]
        public IEnumerator ReaddingTabItemAfterRemovingAll_SelectsIt()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;

            tabs.itemContainer.Clear();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);
            tabs.itemContainer.Add(items[0]);
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(0, tabs.value);
            Assert.IsTrue(items[0].selected);
        }

        [UnityTest]
        public IEnumerator AddingTabItemWhileNothingSelected_KeepsNothingSelected()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            tabs.value = -1;

            tabs.itemContainer.Add(new TabItem { label = "Tab 3" });
            yield return null;

            Assert.AreEqual(-1, tabs.value, "An explicit empty selection is kept.");
        }

        [UnityTest]
        public IEnumerator ReselectingFallbackTabAfterRemovingSelectedOne_MarksItSelected()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 1;

            items[1].RemoveFromHierarchy();
            Assert.AreEqual(0, tabs.value);
            tabs.value = 0;

            Assert.IsTrue(items[0].selected, "The fallback tab reported by value should be selected.");
        }

        [UnityTest]
        public IEnumerator AddingAlreadySelectedTabItem_DoesNotLeaveTwoTabsSelected()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var incoming = new TabItem { label = "Tab 3", selected = true };

            tabs.itemContainer.Add(incoming);
            Assert.AreEqual(0, tabs.value);

            Assert.IsFalse(incoming.selected, "An incoming tab should not stay selected when value points elsewhere.");
            Assert.AreEqual(1, tabs.Query<TabItem>().ToList().Count(item => item.selected), "Only the tab at value should be selected.");
        }

        IEnumerator MountStaticTabsClearedByFirstValueChange()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            tabs.value = 2;
            var cleared = false;
            tabs.RegisterValueChangedCallback(_ =>
            {
                if (cleared)
                    return;
                cleared = true;
                tabs.itemContainer.Clear();
            });
            m_StaticTabItems[0].RemoveFromHierarchy();
        }

        [UnityTest]
        public IEnumerator SetValueWithoutNotifyWhenResyncHandlerClearsItemContainer_DoesNotSelectRemovedTab()
        {
            yield return MountStaticTabsClearedByFirstValueChange();
            var tabs = m_StaticTabs;

            Assert.DoesNotThrow(() => tabs.SetValueWithoutNotify(1));

            Assert.AreEqual(-1, tabs.value, "The handler removed every tab.");
            Assert.AreEqual(0, tabs.items.Count);
        }

        [UnityTest]
        public IEnumerator SettingValueWhenResyncHandlerClearsItemContainer_DoesNotSelectRemovedTab()
        {
            yield return MountStaticTabsClearedByFirstValueChange();
            var tabs = m_StaticTabs;

            Assert.DoesNotThrow(() => tabs.value = 0);

            Assert.AreEqual(-1, tabs.value, "The handler removed every tab.");
            Assert.AreEqual(0, tabs.items.Count);
        }

        [UnityTest]
        public IEnumerator ResyncHandlerRemovingSelectedTabOnEveryChange_StopsOnceItemContainerIsEmpty()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.value = 2;
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

        IEnumerator MountFixedSizeStaticTabsWithLastSelected()
        {
            yield return MountStaticTabs();
            m_StaticTabs.style.width = 400;
            m_StaticTabs.style.height = 40;
            m_StaticTabs.value = 2;
            yield return new WaitForSeconds(1f);
        }

        [UnityTest]
        public IEnumerator RemovingSelectedTabItem_MovesIndicatorToFallbackTab()
        {
            yield return MountFixedSizeStaticTabsWithLastSelected();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            var indicator = tabs.Q(className: Tabs.indicatorUssClassName);
            Assert.AreEqual(items[2].layout.x, indicator.resolvedStyle.left, 0.5f);

            items[2].RemoveFromHierarchy();
            Assert.AreEqual(0, tabs.value);
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(items[0].layout.x, indicator.resolvedStyle.left, 0.5f, "The indicator should follow the fallback tab.");
            Assert.AreEqual(items[0].layout.width, indicator.resolvedStyle.width, 0.5f);
        }

        [UnityTest]
        public IEnumerator RemovingAllTabItems_HidesIndicator()
        {
            yield return MountFixedSizeStaticTabsWithLastSelected();
            var tabs = m_StaticTabs;
            var indicator = tabs.Q(className: Tabs.indicatorUssClassName);
            Assert.Greater(indicator.resolvedStyle.width, 0f);

            tabs.itemContainer.Clear();
            Assert.AreEqual(-1, tabs.value);
            yield return new WaitForSeconds(0.5f);

            Assert.AreEqual(0f, indicator.resolvedStyle.width, 0.5f, "No tab is selected, so the indicator should be hidden.");
        }

        [UnityTest]
        public IEnumerator RemovingAllTabItemsInSameFrameAsSelecting_DoesNotThrow()
        {
            yield return MountFixedSizeStaticTabsWithLastSelected();
            var tabs = m_StaticTabs;

            tabs.value = 1;
            tabs.itemContainer.Clear();
            Assert.AreEqual(-1, tabs.value);
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTest]
        public IEnumerator AddingTabItemToEmptyItemContainer_StopsHierarchyPoll()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var tabs = new Tabs();
            panel.Add(tabs);
            yield return new WaitForSeconds(0.2f);

            tabs.itemContainer.Add(new TabItem { label = "Tab 0" });
            Assert.AreEqual(1, tabs.items.Count);

            var poll = (IVisualElementScheduledItem)typeof(Tabs)
                .GetField("m_PollHierarchyItem", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(tabs);
            Assert.IsTrue(poll == null || !poll.isActive, "The hierarchy poll can never fire once the static items are known, so it should be stopped.");
        }

        [UnityTest]
        public IEnumerator AddingTabItemToEmptyItemContainer_ExposesItInItems()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var tabs = new Tabs();
            panel.Add(tabs);
            yield return new WaitForSeconds(0.2f);
            var item = new TabItem { label = "Tab 0" };

            tabs.itemContainer.Add(item);

            Assert.IsNotNull(tabs.items, "items should list a TabItem added straight to the item container.");
            Assert.AreEqual(new[] { item }, tabs.items.Cast<TabItem>().ToArray());
        }

        [UnityTest]
        public IEnumerator ClickingTabAfterNonTabItemChild_SelectsItsTabIndex()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.itemContainer.Insert(0, new VisualElement());

            using (var evt = ActionTriggeredEvent.GetPooled())
            {
                evt.target = items[2];
                items[2].SendEvent(evt);
            }

            Assert.AreEqual(2, tabs.value, "value indexes the tabs, not every child of the item container.");
            Assert.IsTrue(items[2].selected);
        }

        [UnityTest]
        public IEnumerator NonTabItemChildInUxml_IsIgnoredByHierarchyPoll()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var tabs = new Tabs();
            var item = new TabItem { label = "Tab 0" };
            tabs.Add(item);
            tabs.Add(new VisualElement());
            panel.Add(tabs);

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(new[] { item }, tabs.items.Cast<TabItem>().ToArray());
            Assert.AreEqual(0, tabs.value);
        }

        [UnityTest]
        public IEnumerator NonTabItemChildInItemContainer_DoesNotResyncOnEveryRead()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            tabs.itemContainer.Add(new VisualElement());
            Assert.AreEqual(3, tabs.items.Count);

            // A resync re-marks the selected tab, so clearing the flag by hand shows whether one ran.
            items[0].selected = false;
            _ = tabs.items;

            Assert.IsFalse(items[0].selected, "The tabs did not change, so reading them should not resync.");
        }

        [UnityTest]
        public IEnumerator RemovingSourceBoundTabItemFromItemContainer_KeepsValueIndexingSourceItems()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var sourceItems = new List<string> { "A", "B", "C" };
            var unbound = new List<(TabItem, int)>();
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) => tab.label = sourceItems[i],
                unbindItem = (tab, i) => unbound.Add((tab, i)),
            };
            panel.Add(tabs);
            yield return null;
            tabs.value = 2;
            var removed = (TabItem)tabs.itemContainer[1];
            unbound.Clear();

            removed.RemoveFromHierarchy();
            yield return null;
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(2, tabs.value, "The source did not change, so the selection should still index C.");
            Assert.AreEqual("C", tabs.items[tabs.value]);
            var labels = tabs.itemContainer.Children().Cast<TabItem>().Select(t => t.label).ToArray();
            Assert.AreEqual(new[] { "A", "B", "C" }, labels, "The views should be rebuilt from the source.");
            Assert.IsTrue(((TabItem)tabs.itemContainer[2]).selected);
            Assert.IsTrue(unbound.Contains((removed, 1)), "The removed view should be unbound.");
        }

        [UnityTest]
        public IEnumerator RemovingSourceBoundTabItemFromItemContainer_UnbindsRemainingViewsWhileAttached()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var sourceItems = new List<string> { "A", "B", "C" };
            var parents = new Dictionary<TabItem, VisualElement>();
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) => tab.label = sourceItems[i],
                unbindItem = (tab, i) => parents[tab] = tab.parent,
            };
            panel.Add(tabs);
            yield return null;
            var views = tabs.itemContainer.Children().Cast<TabItem>().ToList();

            views[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(0);

            Assert.AreEqual(tabs.itemContainer, parents[views[0]], "A view still in the container should be unbound before it is detached.");
            Assert.AreEqual(tabs.itemContainer, parents[views[2]], "A view still in the container should be unbound before it is detached.");
            Assert.IsNull(parents[views[1]], "The view removed from outside should still be unbound.");
        }

        [UnityTest]
        public IEnumerator UnbindHandlerReadingValueDuringResync_UnbindsEachViewOnce()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var sourceItems = new List<string> { "A", "B", "C" };
            var unbound = new List<(TabItem, int)>();
            var readValues = new List<int>();
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) => tab.label = sourceItems[i],
            };
            tabs.unbindItem = (tab, i) =>
            {
                unbound.Add((tab, i));
                readValues.Add(tabs.value);
                Assert.AreEqual(sourceItems.Count, tabs.items.Count);
            };
            panel.Add(tabs);
            yield return null;
            tabs.value = 2;
            var views = tabs.itemContainer.Children().Cast<TabItem>().ToList();
            unbound.Clear();
            readValues.Clear();

            views[1].RemoveFromHierarchy();
            tabs.SetValueWithoutNotify(tabs.value);

            Assert.AreEqual(new[] { (views[0], 0), (views[1], 1), (views[2], 2) }, unbound.ToArray(), "Each old view should be unbound exactly once.");
            Assert.AreEqual(new[] { 2, 2, 2 }, readValues.ToArray(), "The handler should see the selection it had before the rebuild.");
            Assert.AreEqual(2, tabs.value);
            var labels = tabs.itemContainer.Children().Cast<TabItem>().Select(t => t.label).ToArray();
            Assert.AreEqual(sourceItems.ToArray(), labels);
        }

        [UnityTest]
        public IEnumerator AssigningUnbindItemRightAfterRemovingStaticTabItem_DoesNotReaddIt()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;

            items[1].RemoveFromHierarchy();
            tabs.unbindItem = (tab, i) => { };

            Assert.IsNull(items[1].parent, "The removed tab should stay out of the item container.");
            Assert.AreEqual(new[] { items[0], items[2] }, tabs.items.Cast<TabItem>().ToArray());
        }

        [UnityTest]
        public IEnumerator AssigningBindItemRightAfterAddingStaticTabItem_KeepsIt()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;
            var incoming = new TabItem { label = "Tab 3" };

            tabs.itemContainer.Add(incoming);
            tabs.bindItem = (tab, i) => { };

            Assert.AreEqual(tabs.itemContainer, incoming.parent, "The added tab should stay in the item container.");
            Assert.AreEqual(items.Append(incoming).ToArray(), tabs.items.Cast<TabItem>().ToArray());
        }

        [UnityTest]
        public IEnumerator ClearingSourceItemsAfterRemovingStaticTabItem_DoesNotReaddIt()
        {
            yield return MountStaticTabs();
            var tabs = m_StaticTabs;
            var items = m_StaticTabItems;

            items[1].RemoveFromHierarchy();
            tabs.sourceItems = new List<string> { "A" };
            tabs.sourceItems = null;

            Assert.IsNull(items[1].parent, "The removed tab should stay out of the item container.");
            Assert.AreEqual(new[] { items[0], items[2] }, tabs.items.Cast<TabItem>().ToArray());
        }
    }
}
