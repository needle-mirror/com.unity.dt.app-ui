using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
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

        [Test]
        public void AllDisabledItems_DoesNotThrow()
        {
            var sourceItems = new List<string>();
            for (var i = 0; i < 2; ++i)
            {
                sourceItems.Add($"Tab {i}");
            }

            Tabs tabs = null;
            Assert.DoesNotThrow(() =>
            {
                tabs = new Tabs
                {
                    sourceItems = sourceItems,
                    bindItem = (tab, i) =>
                    {
                        tab.label = sourceItems[i];
                        tab.SetEnabled(false);
                    }
                };
            });

            // No item is selectable, so the value must fall back to -1.
            Assert.AreEqual(-1, tabs.value);
        }

        [UnityTest]
        public IEnumerator ClearingItemsAfterSelection_ResetsWithoutError()
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
                bindItem = (tab, i) => { tab.label = sourceItems[i]; }
            };

            panel.Add(tabs);

            yield return null;

            Assert.AreEqual(0, tabs.value);

            // Clearing the items while an indicator refresh may be scheduled must not throw
            // and must reset the selection.
            Assert.DoesNotThrow(() => tabs.sourceItems = new List<string>());

            yield return null;
            yield return null;

            Assert.AreEqual(-1, tabs.value);
        }

        [UnityTest]
        public IEnumerator Navigation_SkipsDisabledItems()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            var sourceItems = new List<string> { "Tab 0", "Tab 1", "Tab 2" };
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                // disable the middle item
                bindItem = (tab, i) =>
                {
                    tab.label = sourceItems[i];
                    tab.SetEnabled(i != 1);
                }
            };

            panel.Add(tabs);

            yield return null;

            Assert.AreEqual(0, tabs.value);

            // GoToNext must skip the disabled item at index 1 and land on 2.
            Assert.IsTrue(tabs.GoToNext());
            Assert.AreEqual(2, tabs.value);

            // GoToPrevious must skip index 1 on the way back to 0.
            Assert.IsTrue(tabs.GoToPrevious());
            Assert.AreEqual(0, tabs.value);
        }

        [UnityTest]
        public IEnumerator Navigation_AtBoundaries_ReturnsFalse()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            var sourceItems = new List<string> { "Tab 0", "Tab 1" };
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) => { tab.label = sourceItems[i]; }
            };

            panel.Add(tabs);

            yield return null;

            Assert.AreEqual(0, tabs.value);

            // Already at the first item: no previous.
            Assert.IsFalse(tabs.GoToPrevious());
            Assert.AreEqual(0, tabs.value);

            // Move to the last item, then confirm there is no next.
            Assert.IsTrue(tabs.GoToNext());
            Assert.AreEqual(1, tabs.value);
            Assert.IsFalse(tabs.GoToNext());
            Assert.AreEqual(1, tabs.value);
        }

        [UnityTest]
        public IEnumerator Navigation_AllDisabled_DoesNotThrowOrHang()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            var sourceItems = new List<string> { "Tab 0", "Tab 1" };
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) =>
                {
                    tab.label = sourceItems[i];
                    tab.SetEnabled(false);
                }
            };

            panel.Add(tabs);

            yield return null;

            Assert.AreEqual(-1, tabs.value);

            // No enabled item to move to: navigation is a safe no-op, not a throw or hang.
            Assert.IsFalse(tabs.GoToNext());
            Assert.IsFalse(tabs.GoToPrevious());
            Assert.AreEqual(-1, tabs.value);
        }

        [UnityTest]
        public IEnumerator Navigation_FromDeselectedState_CanReselectInBothDirections()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            var sourceItems = new List<string> { "Tab 0", "Tab 1", "Tab 2" };
            var tabs = new Tabs
            {
                sourceItems = sourceItems,
                bindItem = (tab, i) => { tab.label = sourceItems[i]; }
            };

            panel.Add(tabs);

            yield return null;

            // Consumers can legitimately clear the selection while enabled tabs still exist.
            tabs.value = -1;
            Assert.AreEqual(-1, tabs.value);

            // GoToPrevious from the deselected state must land on the last enabled tab,
            // mirroring GoToNext which lands on the first.
            Assert.IsTrue(tabs.GoToPrevious());
            Assert.AreEqual(2, tabs.value);

            // Back to deselected, GoToNext still works too.
            tabs.value = -1;
            Assert.AreEqual(-1, tabs.value);
            Assert.IsTrue(tabs.GoToNext());
            Assert.AreEqual(0, tabs.value);
        }
    }
}
