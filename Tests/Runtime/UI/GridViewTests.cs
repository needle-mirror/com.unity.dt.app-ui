using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#if ENABLE_RUNTIME_DATA_BINDINGS
using Unity.Properties;
#endif

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(GridView))]
    class GridViewTests : VisualElementTests<GridView>
    {
        protected override string mainUssClassName => BaseGridView.ussClassName;

        protected override bool uxmlConstructable => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new GridView
                {
                    style =
                    {
                        height = 400,
                    },
                    makeItem = () => new Text(),
                    bindItem = (e, i) => ((Text)e).text = $"Item {i}",
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });

                yield return new Story("ThreeColumns", _ => new GridView
                {
                    style =
                    {
                        height = 400,
                    },
                    columnCount = 3,
                    makeItem = () => new Text(),
                    bindItem = (e, i) => ((Text)e).text = $"Item {i}",
                    itemsSource = Enumerable.Range(0, 50).ToList(),
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:GridView />",
            @"<appui:GridView item-height=""100"" selection-type=""Multiple"" allow-no-selection=""true"" prevent-scroll-with-modifiers=""true""  />",
        };

        [UnityTest, Order(10)]
        public IEnumerator CanConstructGridView()
        {
            if (!Application.isEditor)
            {
                // skip test and mark as ignored
                Assert.Ignore("Can't run this test outside of the editor");
                yield break;
            }

            GridView gridView = null;

            Assert.DoesNotThrow(() =>
            {
                gridView = new GridView(itemsSource: new List<int>() {1, 2, 3, 4, 5}, makeItem: () => new Text(),
                    bindItem: (e, i) => ((Text) e).text = i.ToString());
            });

            Assert.DoesNotThrow(() =>
            {
                gridView = new GridView
                {
                    makeItem = () => new Text(),
                    bindItem = (e, i) => ((Text)e).text = i.ToString(),
                    itemHeight = 100,
                    columnCount = 5,
                    selectionType = SelectionType.Multiple,
                    allowNoSelection = true,
                    preventScrollWithModifiers = true
                };
                gridView.selectionChanged += _ => { };
                gridView.itemsSource = Enumerable.Range(1,1000).ToList();

            });

            Assert.NotNull(gridView);

            m_TestUI.rootVisualElement.Clear();
            m_Panel = new Panel();
            m_Panel.Add(gridView);
            m_TestUI.rootVisualElement.Add(m_Panel);
            gridView.StretchToParentSize();
            m_Panel.StretchToParentSize();

            yield return null;

            gridView.selectedIndex = 500;

            yield return null;

            Assert.AreEqual(500, gridView.selectedIndex);

            gridView.ScrollToItem(500);

            yield return new WaitForSeconds(0.2f);

            gridView.scrollView.verticalScroller.value = gridView.scrollView.verticalScroller.lowValue;

            yield return new WaitForSeconds(0.2f);

            gridView.scrollView.verticalScroller.value = gridView.scrollView.verticalScroller.highValue;

            yield return new WaitForSeconds(0.2f);

            gridView.selectionType = SelectionType.None;

            yield return null;

            Assert.AreEqual(SelectionType.None, gridView.selectionType);
            Assert.AreEqual(-1, gridView.selectedIndex);

            m_Panel = null;
            m_TestUI.rootVisualElement.Clear();
        }

#if ENABLE_RUNTIME_DATA_BINDINGS
        [UnityTest, Order(30)]
        public IEnumerator DataBinding_ColumnCount()
        {
            var dataSource = ScriptableObject.CreateInstance<GridViewDataSource>();
            dataSource.columnCountValue = 2;

            var gridView = new GridView
            {
                makeItem = () => new Text(),
                bindItem = (e, i) => ((Text)e).text = i.ToString(),
                itemsSource = Enumerable.Range(0, 10).ToList(),
                style = { height = 400 }
            };

            m_TestUI.rootVisualElement.Add(gridView);
            gridView.StretchToParentSize();

            // Set data source
            gridView.dataSource = dataSource;

            // Bind columnCount to data source
            var binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(GridViewDataSource.columnCountValue)),
                bindingMode = BindingMode.ToTarget
            };
            gridView.SetBinding(nameof(gridView.columnCount), binding);

            yield return null;

            // Verify initial binding
            Assert.AreEqual(2, gridView.columnCount);

            // Change data source value
            dataSource.columnCountValue = 3;
            yield return null;

            // Verify binding updated the property
            Assert.AreEqual(3, gridView.columnCount);

            m_TestUI.rootVisualElement.Clear();
            Object.Destroy(dataSource);
        }

        [UnityTest, Order(31)]
        public IEnumerator DataBinding_SelectedIndex()
        {
            var dataSource = ScriptableObject.CreateInstance<GridViewDataSource>();
            dataSource.selectedIndexValue = 0;

            var gridView = new GridView
            {
                makeItem = () => new Text(),
                bindItem = (e, i) => ((Text)e).text = i.ToString(),
                itemsSource = Enumerable.Range(0, 50).ToList(),
                style = { height = 400 }
            };

            m_TestUI.rootVisualElement.Add(gridView);
            gridView.StretchToParentSize();

            // Set data source
            gridView.dataSource = dataSource;

            // Bind selectedIndex bidirectionally
            var binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(GridViewDataSource.selectedIndexValue)),
                bindingMode = BindingMode.TwoWay
            };
            gridView.SetBinding(nameof(gridView.selectedIndex), binding);

            yield return null;

            // Verify initial binding
            Assert.AreEqual(0, gridView.selectedIndex);

            // Change data source value
            dataSource.selectedIndexValue = 10;
            yield return null;

            // Verify binding updated the property
            Assert.AreEqual(10, gridView.selectedIndex);

            // Change UI and verify data source updates
            gridView.selectedIndex = 20;
            yield return null;

            Assert.AreEqual(20, dataSource.selectedIndexValue);

            m_TestUI.rootVisualElement.Clear();
            Object.Destroy(dataSource);
        }

        [UnityTest, Order(32)]
        public IEnumerator DataBinding_SelectionCount_ReadOnly()
        {
            var dataSource = ScriptableObject.CreateInstance<GridViewDataSource>();

            var gridView = new GridView
            {
                makeItem = () => new Text(),
                bindItem = (e, i) => ((Text)e).text = i.ToString(),
                itemsSource = Enumerable.Range(0, 50).ToList(),
                selectionType = SelectionType.Multiple,
                style = { height = 400 }
            };

            m_TestUI.rootVisualElement.Add(gridView);
            gridView.StretchToParentSize();

            // Set data source
            gridView.dataSource = dataSource;

            // Bind selectionCount (read-only property)
            var binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(GridViewDataSource.selectionCountValue)),
                bindingMode = BindingMode.ToTarget
            };
            gridView.SetBinding(nameof(gridView.selectionCount), binding);

            yield return null;

            // Initial value should be 0
            Assert.AreEqual(0, gridView.selectionCount);

            // Add selection
            gridView.AddToSelection(5);
            yield return null;

            // Verify read-only binding reflects the change
            Assert.AreEqual(1, gridView.selectionCount);

            // Add more selections
            gridView.AddToSelection(10);
            gridView.AddToSelection(15);
            yield return null;

            Assert.AreEqual(3, gridView.selectionCount);

            m_TestUI.rootVisualElement.Clear();
            Object.Destroy(dataSource);
        }

        [UnityTest, Order(33)]
        public IEnumerator DataBinding_ItemWidth_ReadOnly()
        {
            var dataSource = ScriptableObject.CreateInstance<GridViewDataSource>();

            var gridView = new GridView
            {
                makeItem = () => new Text(),
                bindItem = (e, i) => ((Text)e).text = i.ToString(),
                itemsSource = Enumerable.Range(0, 10).ToList(),
                columnCount = 2,
                style = { height = 400, width = 400 }
            };

            m_TestUI.rootVisualElement.Add(gridView);
            gridView.StretchToParentSize();

            // Set data source
            gridView.dataSource = dataSource;

            // Bind itemWidth (read-only property)
            var binding = new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(GridViewDataSource.itemWidthValue)),
                bindingMode = BindingMode.ToTarget
            };
            gridView.SetBinding(nameof(gridView.itemWidth), binding);

            yield return null;

            // Verify initial value
            Assert.Greater(gridView.itemWidth, 0);

            var initialWidth = gridView.itemWidth;

            // Change columnCount which affects itemWidth
            gridView.columnCount = 4;
            yield return null;

            // Verify width changed
            Assert.AreNotEqual(initialWidth, gridView.itemWidth);
            Assert.IsTrue(gridView.itemWidth < initialWidth);

            m_TestUI.rootVisualElement.Clear();
            Object.Destroy(dataSource);
        }
#endif
    }
}
