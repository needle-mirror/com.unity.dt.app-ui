using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
#pragma warning disable CS8524 // The switch expression does not handle some values...

namespace Unity.AppUI.Tests.UI
{
    [TestOf(typeof(SplitView))]
    [TestFixture(Direction.Horizontal, Dir.Ltr)]
    [TestFixture(Direction.Vertical, Dir.Ltr)]
    [TestFixture(Direction.Horizontal, Dir.Rtl)]
    [TestFixture(Direction.Vertical, Dir.Rtl)]
    class SplitViewTests : VisualElementTests<SplitView>
    {

        // An empty SplitView lays out to nothing, and it has no intrinsic size either. The story
        // follows the fixture's own direction, and carries it in the story name: the capture file
        // is keyed on component and story, so a shared name would have the vertical fixtures
        // overwrite the horizontal ones (the Ltr/Rtl axis is already part of the filename).
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story($"Default-{m_Direction}", (ctx) =>
                {
                    var splitView = new SplitView(m_Direction) { style = { width = 320, height = 200 } };
                    splitView.AddPane(NamedPane("One"));
                    splitView.AddPane(NamedPane("Two"));
                    return splitView;
                });
            }
        }

        static Pane NamedPane(string title)
        {
            var pane = new Pane();
            pane.Add(new Text(title));
            return pane;
        }

        const float k_TotalSize = 300;

        readonly Direction m_Direction;

        readonly Dir m_LayoutDirection;

        // A Pane's minimum size comes from USS (min-width/min-height: var(--appui-sizing-250)), so it changes
        // with the active density. Measurement assertions below read the resolved value from a live pane via
        // GetMin(Pane) instead of hardcoding it; a hardcoded copy silently breaks whenever the ramp is retuned.
        // The fixture built by SplitView_CanRemoveAndInsertPane gives panes 0 and 2 an inline 50px minimum,
        // so pane 1 is the pane to read the USS default from.

        protected override string mainUssClassName => SplitView.ussClassName;

        // Handed over to SplitViewSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override bool uxmlConstructable => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:SplitView style=""width: 256px;"">
                <appui:Pane style=""width: 50%;"" />
                <appui:Pane style=""width: 50%;"" />
              </appui:SplitView>",
            @"<appui:SplitView style=""width: 256px;"">
                <appui:Pane style=""width: 50px; min-width: 50px;"" />
                <appui:Pane style=""width: auto; flex-grow: 1;"" />
                <appui:Pane style=""width: 50px; min-width: 50px;"" />
              </appui:SplitView>",
        };

        public SplitViewTests(Direction direction, Dir layoutDirection)
        {
            m_Direction = direction;
            m_LayoutDirection = layoutDirection;
        }

        float GetMin(Rect rect)
        {
            return m_Direction switch
            {
                Direction.Horizontal when m_LayoutDirection == Dir.Ltr => rect.xMin,
                Direction.Horizontal when m_LayoutDirection == Dir.Rtl => rect.xMax,
                Direction.Vertical => rect.yMin,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        float GetMin(Pane pane)
        {
            return m_Direction == Direction.Horizontal ? pane.resolvedStyle.minWidth.value : pane.resolvedStyle.minHeight.value;
        }

        float GetMax(Rect rect)
        {
            return m_Direction switch
            {
                Direction.Horizontal when m_LayoutDirection == Dir.Ltr => rect.xMax,
                Direction.Horizontal when m_LayoutDirection == Dir.Rtl => rect.xMin,
                Direction.Vertical => rect.yMax,
                _ => throw new System.ArgumentOutOfRangeException()
            };
        }

        float GetPos(IResolvedStyle style)
        {
            return m_Direction == Direction.Horizontal ? style.left : style.top;
        }

        Vector2 GetVec2Position(float position)
        {
            return m_Direction == Direction.Horizontal ? new Vector2(position, 0) : new Vector2(0, position);
        }

        void SetPaneSize(VisualElement element, Length width)
        {
            if (m_Direction == Direction.Horizontal)
                element.style.width = width;
            else
                element.style.height = width;
        }

        void SetPaneMinSize(VisualElement element, Length width)
        {
            if (m_Direction == Direction.Horizontal)
                element.style.minWidth = width;
            else
                element.style.minHeight = width;
        }

        [UnityTest]
        [Order(10)]
        public IEnumerator SplitView_CanBeConstructed_WhenEmpty()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel
            {
                layoutDirection = m_LayoutDirection
            };
            var splitView = new SplitView
            {
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    width = k_TotalSize,
                    height = k_TotalSize,
                },
                direction = m_Direction
            };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(splitView);

            yield return null; // should have no exceptions with an empty SplitView

            Assert.AreEqual(0, splitView.paneCount);
            Assert.AreEqual(0, splitView.splitterCount);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.PaneAt(99));
        }

        [UnityTest]
        [Order(11)]
        public IEnumerator SplitView_CanBePopulatedWithOnePane_WhenEmpty()
        {
            yield return SplitView_CanBeConstructed_WhenEmpty();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            var pane = new Pane();
            SetPaneSize(pane, new Length(50, LengthUnit.Percent));
            splitView.AddPane(pane);

            yield return null;

            Assert.AreEqual(0, splitView.splitterCount);
            Assert.AreEqual(1, splitView.paneCount);
        }

        [UnityTest]
        [Order(12)]
        public IEnumerator SplitView_CanBePopulatedWithSecondPane()
        {
            yield return SplitView_CanBePopulatedWithOnePane_WhenEmpty();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            splitView.AddPane(new Pane
            {
                style = { flexGrow = 1 }
            });

            yield return null;

            Assert.AreEqual(1, splitView.splitterCount);
            Assert.AreEqual(2, splitView.paneCount);
        }

        [UnityTest]
        [Order(13)]
        public IEnumerator SplitView_CanRemoveAndInsertPane()
        {
            yield return SplitView_CanBePopulatedWithSecondPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            var pane = splitView.PaneAt(0);
            splitView.RemovePane(pane);

            yield return null;

            Assert.AreEqual(0, splitView.splitterCount);
            Assert.AreEqual(1, splitView.paneCount);

            var newPane = new Pane();
            SetPaneSize(newPane, new Length(25, LengthUnit.Percent));
            SetPaneMinSize(newPane, new Length(50, LengthUnit.Pixel));
            splitView.InsertPane(0, newPane);

            yield return null;

            Assert.AreEqual(1, splitView.splitterCount);
            Assert.AreEqual(2, splitView.paneCount);

            Assert.AreSame(newPane, splitView.PaneAt(0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.PaneAt(2));

            newPane = new Pane();
            SetPaneSize(newPane, new Length(25, LengthUnit.Percent));
            SetPaneMinSize(newPane, new Length(50, LengthUnit.Pixel));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.InsertPane(999, newPane));
            splitView.InsertPane(2, newPane);

            yield return null;

            Assert.AreEqual(2, splitView.splitterCount);
            Assert.AreEqual(3, splitView.paneCount);

            Assert.AreSame(newPane, splitView.PaneAt(2));
            Assert.AreEqual(2, splitView.IndexOfPane(newPane));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.PaneAt(3));
        }

        public struct SplitView_CanGetSplitterRange_TestCase
        {
            public int index;
            public Func<Direction, Dir, float, float> expectedMin;
            public Func<Direction, Dir, float, float> expectedMax;
        }

        static IEnumerable SplitView_CanGetSplitterRange_TestCases()
        {
            yield return new SplitView_CanGetSplitterRange_TestCase
            {
                index = 0,
                expectedMin = (direction, layoutDirection, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDirection == Dir.Ltr => 50f,
                    Direction.Horizontal when layoutDirection == Dir.Rtl => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Vertical => 50,
                },
                expectedMax = (direction, layoutDirection, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDirection == Dir.Ltr => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Horizontal when layoutDirection == Dir.Rtl => k_TotalSize - 50f,
                    Direction.Vertical => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                },
            };
            yield return new SplitView_CanGetSplitterRange_TestCase
            {
                index = 1,
                expectedMin = (direction, layoutDirection, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDirection == Dir.Ltr => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Horizontal when layoutDirection == Dir.Rtl => 50f,
                    Direction.Vertical => k_TotalSize * 0.25f + paneMinSize,
                },
                expectedMax = (direction, layoutDirection, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDirection == Dir.Ltr => k_TotalSize - 50f,
                    Direction.Horizontal when layoutDirection == Dir.Rtl => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Vertical => k_TotalSize - 50f,
                },
            };
        }

        [UnityTest]
        [Order(14)]
        public IEnumerator SplitView_CanGetSplitterRange(
            [ValueSource(nameof(SplitView_CanGetSplitterRange_TestCases))] SplitView_CanGetSplitterRange_TestCase testCase)
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            splitView.GetRange(testCase.index, out var min, out var max);

            // Panes 0 and 2 carry an inline 50px minimum (the hardcoded 50f in the cases above); pane 1 is the
            // only one left on the USS default, and it is the pane whose minimum bounds both splitter ranges.
            var paneMinSize = GetMin(splitView.PaneAt(1));

            Assert.AreEqual(testCase.expectedMin(m_Direction, m_LayoutDirection, paneMinSize), min, 1);
            Assert.AreEqual(testCase.expectedMax(m_Direction, m_LayoutDirection, paneMinSize), max, 1);
        }

        public struct SplitView_CanGetLegalSplitterPosition_TestCase
        {
            public int index;
            public float desiredPosition;
            public Func<Direction, Dir, float, float> expectedResult;
        }

        static IEnumerable SplitView_CanGetLegalSplitterPosition_TestCases()
        {
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 0,
                desiredPosition = 50,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 50,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Vertical => 50,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 0,
                desiredPosition = 0,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 50,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Vertical => 50,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 0,
                desiredPosition = 60,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 50,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Vertical => 50,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 0,
                desiredPosition = 100,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 100,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize * 0.25f + paneMinSize,
                    Direction.Vertical => 100,
                }
            };

            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 1,
                desiredPosition = k_TotalSize,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => k_TotalSize - 50,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Vertical => k_TotalSize - 50,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 1,
                desiredPosition = 250,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 250,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Vertical => 250,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 1,
                desiredPosition = 240,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 250,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Vertical => 250,
                }
            };
            yield return new SplitView_CanGetLegalSplitterPosition_TestCase
            {
                index = 1,
                desiredPosition = 200,
                expectedResult = (direction, layoutDir, paneMinSize) => direction switch
                {
                    Direction.Horizontal when layoutDir == Dir.Ltr => 200,
                    Direction.Horizontal when layoutDir == Dir.Rtl => k_TotalSize - k_TotalSize * 0.25f - paneMinSize,
                    Direction.Vertical => 200,
                }
            };
        }

        [UnityTest]
        [Order(15)]
        public IEnumerator SplitView_CanGetLegalSplitterPosition(
            [ValueSource(nameof(SplitView_CanGetLegalSplitterPosition_TestCases))] SplitView_CanGetLegalSplitterPosition_TestCase testCase)
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            var res = splitView.GetLegalSplitterPosition(testCase.index, testCase.desiredPosition);

            // Pane 1 is the only pane without an inline minimum, so it is the one carrying the USS default.
            var paneMinSize = GetMin(splitView.PaneAt(1));

            Assert.AreEqual(testCase.expectedResult(m_Direction, m_LayoutDirection, paneMinSize), res, 1);
        }

        [UnityTest]
        [Order(16)]
        public IEnumerator SplitView_CanSaveAndRestoreState()
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            var state = splitView.SaveState();

            Assert.IsNotNull(state);
            Assert.AreEqual(m_Direction, state.direction);
            Assert.IsTrue(state.collapsedPanes.All(cp => !cp));
            Assert.IsTrue(state.realtimeResize);
            Assert.AreEqual(k_TotalSize * 0.25f, state.paneSizes[0], 1);
            Assert.AreEqual(-1f, state.paneSizes[1], "Panes with 'auto' size should be saved as -1f");
            Assert.AreEqual(k_TotalSize * 0.25f, state.paneSizes[2], 1);

            Assert.DoesNotThrow(() => splitView.RestoreState(state));
        }

        [UnityTest]
        [Order(17)]
        public IEnumerator SplitView_CanMoveSplitters()
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            Assert.DoesNotThrow(() =>
            {
                splitView.OnSplitterDragged(99, GetWorldPos(149));
            }, "Giving an invalid splitter index should not throw an exception");

            splitView.OnSplitterDragged(0, GetWorldPos(149));

            yield return null;

            Assert.AreEqual(GetExpectedPosition(149), GetMax(splitView.PaneAt(0).layout), 1);

            splitView.OnSplitterDragged(0, GetWorldPos(50 + splitView.PaneAt(0).compactThreshold - 1));

            yield return null;

            Assert.AreEqual(GetExpectedPosition(50), GetMax(splitView.PaneAt(0).layout), 1);
            Assert.IsTrue(splitView.PaneAt(0).compact);

            splitView.OnSplitterDragged(1, GetWorldPos(50));

            yield return null;

            // Splitter 1 snaps to its range minimum, which is pane 0's edge plus pane 1's minimum size,
            // so this measures the pane minimum and not the compact threshold. The two were both 15-16px
            // under the old sizing ramp, which let the +/-1 tolerance hide the mismatch.
            Assert.AreEqual(GetExpectedPosition(50 + GetMin(splitView.PaneAt(1))), GetMax(splitView.PaneAt(1).layout), 1);

            splitView.OnSplitterDragged(1, GetWorldPos(290));

            yield return null;

            splitView.OnSplitterUp(1, GetWorldPos(290));
            splitView.OnSplitterUp(1, GetWorldPos(290)); // should not change anything

            yield return null;

            Assert.AreEqual(GetExpectedPosition(250), GetMax(splitView.PaneAt(1).layout), 1);

            yield break;

            // --------------------------------------------------------------------------------------------

            Vector2 GetWorldPos(float val)
            {
                val = GetExpectedPosition(val);
                return splitView.LocalToWorld(GetVec2Position(val));
            }

            float GetExpectedPosition(float val)
            {
                return m_Direction == Direction.Horizontal && m_LayoutDirection == Dir.Rtl ? k_TotalSize - val : val;
            }
        }

        [UnityTest]
        [Order(18)]
        public IEnumerator SplitView_CanMoveSplitters_WithoutRealtimeResize()
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();
            splitView.realtimeResize = false;

            splitView.OnSplitterDragged(0, splitView.LocalToWorld(GetVec2Position(149)));

            yield return null;

            Assert.DoesNotThrow(
                () => splitView.OnSplitterUp(99, splitView.LocalToWorld(GetVec2Position(149))),
                "Giving an invalid splitter index should not throw an exception");

            splitView.OnSplitterUp(0, splitView.LocalToWorld(GetVec2Position(149)));

            yield return null;

            Assert.AreEqual(149, GetMax(splitView.PaneAt(0).layout), 1);
        }

        [UnityTest]
        [Order(19)]
        public IEnumerator SplitView_CanCollapseAndExpandSplitters()
        {
            yield return SplitView_CanRemoveAndInsertPane();

            var splitView = m_TestUI.rootVisualElement.Q<SplitView>();

            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.CollapseSplitter(-1, CollapseDirection.Backward));

            splitView.CollapseSplitter(0, CollapseDirection.Backward);

            yield return null;

            Assert.AreEqual(0, GetMax(splitView.PaneAt(0).layout), 1);
            Assert.IsTrue(splitView.IsSplitterCollapsed(0));
            Assert.AreEqual(DisplayStyle.None, splitView.PaneAt(0).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(1).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(2).resolvedStyle.display);
            Assert.AreEqual(0, GetPos(splitView.splitters[0].resolvedStyle), 1);

            splitView.CollapseSplitter(0, CollapseDirection.Forward); // should do nothing, already collapsed

            yield return null;

            Assert.AreEqual(0, GetMax(splitView.PaneAt(0).layout), 1);
            Assert.IsTrue(splitView.IsSplitterCollapsed(0));
            Assert.AreEqual(DisplayStyle.None, splitView.PaneAt(0).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(1).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(2).resolvedStyle.display);
            Assert.AreEqual(0, GetPos(splitView.splitters[0].resolvedStyle), 1);

            splitView.CollapseSplitter(1, CollapseDirection.Forward);

            yield return null;

            Assert.AreEqual(GetExpectedPosition(k_TotalSize), GetMax(splitView.PaneAt(1).layout), 1);
            Assert.IsTrue(splitView.IsSplitterCollapsed(1));
            Assert.AreEqual(DisplayStyle.None, splitView.PaneAt(0).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(1).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, splitView.PaneAt(2).resolvedStyle.display);
            Assert.AreEqual(GetExpectedPosition(k_TotalSize), GetPos(splitView.splitters[1].resolvedStyle), 1);

            Assert.AreEqual(GetExpectedPosition(0), GetMin(splitView.PaneAt(1).layout), 1);
            Assert.AreEqual(GetExpectedPosition(k_TotalSize), GetMax(splitView.PaneAt(1).layout), 1);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.ExpandSplitter(-1));

            splitView.ExpandSplitter(0);

            // wait 2 frames for splitter to be correctly positioned
            yield return null;
            yield return null;

            Assert.AreEqual(GetExpectedPosition(GetMin(splitView.PaneAt(0))), GetMax(splitView.PaneAt(0).layout), 1, "Splitter 0 should expand the pane with its minimum size");
            Assert.IsFalse(splitView.IsSplitterCollapsed(0));
            Assert.IsTrue(splitView.IsSplitterCollapsed(1));
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(0).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(1).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.None, splitView.PaneAt(2).resolvedStyle.display);
            Assert.AreEqual(GetExpectedPosition(GetMin(splitView.PaneAt(0))), GetPos(splitView.splitters[0].resolvedStyle), 1);
            Assert.AreEqual(GetExpectedPosition(k_TotalSize), GetPos(splitView.splitters[1].resolvedStyle), 1);

            splitView.ExpandSplitter(1);

            // wait 2 frames for splitter to be correctly positioned
            yield return null;
            yield return null;

            Assert.AreEqual(GetExpectedPosition(k_TotalSize - GetMin(splitView.PaneAt(2))), GetMax(splitView.PaneAt(1).layout), 1, "Splitter 1 should expand the pane with its minimum size");
            Assert.IsFalse(splitView.IsSplitterCollapsed(0));
            Assert.IsFalse(splitView.IsSplitterCollapsed(1));
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(0).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(1).resolvedStyle.display);
            Assert.AreEqual(DisplayStyle.Flex, splitView.PaneAt(2).resolvedStyle.display);
            Assert.AreEqual(GetExpectedPosition(GetMin(splitView.PaneAt(0))), GetPos(splitView.splitters[0].resolvedStyle), 1);
            Assert.AreEqual(GetExpectedPosition(k_TotalSize - GetMin(splitView.PaneAt(2))), GetPos(splitView.splitters[1].resolvedStyle), 1);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => splitView.IsSplitterCollapsed(-1));

            yield break;

            // --------------------------------------------------------------------------------------------

            float GetExpectedPosition(float val)
            {
                return m_Direction == Direction.Horizontal && m_LayoutDirection == Dir.Rtl ? k_TotalSize - val : val;
            }
        }
    }
}
