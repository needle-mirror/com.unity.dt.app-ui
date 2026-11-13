#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture(Direction.Horizontal)]
    [TestFixture(Direction.Vertical)]
    [TestOf(typeof(SplitView))]
    class SplitViewBehaviourTests : UIComponentTests<SplitView>
    {
        readonly Direction m_Direction;

        public SplitViewBehaviourTests(Direction direction)
        {
            m_Direction = direction;
        }

        SplitView MountSplitView()
        {
            var splitView = new SplitView(m_Direction)
            {
                style = { width = 600, height = 400 },
            };
            splitView.AddPane(NewPane());
            splitView.AddPane(NewPane());

            var mounted = MountElement(splitView);
            simulate.FrameUpdate();
            return mounted;
        }

        Pane NewPane()
        {
            var pane = new Pane();
            if (m_Direction == Direction.Horizontal)
                pane.style.width = new Length(50, LengthUnit.Percent);
            else
                pane.style.height = new Length(50, LengthUnit.Percent);
            return pane;
        }

        /// <summary>
        /// The size of <paramref name="element"/> along the split axis, which is the only dimension
        /// a splitter drag can change.
        /// </summary>
        float SizeAlongAxis(VisualElement element) =>
            m_Direction == Direction.Horizontal ? element.worldBound.width : element.worldBound.height;

        /// <summary>
        /// Drags <paramref name="splitter"/> by <paramref name="distance"/> along the split axis.
        /// </summary>
        void DragSplitter(VisualElement splitter, float distance)
        {
            var from = splitter.worldBound.center;
            var delta = m_Direction == Direction.Horizontal
                ? new Vector2(distance, 0f)
                : new Vector2(0f, distance);
            simulate.DragAndDrop(from, from + delta);
            simulate.FrameUpdate();
        }

        [Test]
        public void SplitView_DraggingSplitter_ShouldResizePanes()
        {
            var splitView = MountSplitView();
            var splitter = splitView.Q<Splitter>();
            Assert.IsNotNull(splitter, "A two-pane SplitView should have a splitter.");

            var first = splitView.PaneAt(0);
            var second = splitView.PaneAt(1);
            var firstBefore = SizeAlongAxis(first);
            var secondBefore = SizeAlongAxis(second);
            Assert.Greater(firstBefore, 0f, "The panes need a resolved layout before dragging.");

            DragSplitter(splitter, 60f);

            Assert.Greater(SizeAlongAxis(first), firstBefore,
                "Dragging the splitter forward should grow the first pane.");
            Assert.Less(SizeAlongAxis(second), secondBefore,
                "...and shrink the second by the same move.");
        }

        [Test]
        public void SplitView_DraggingSplitter_ShouldNotShrinkPaneBelowItsMinimum()
        {
            var splitView = MountSplitView();
            var splitter = splitView.Q<Splitter>();
            var second = splitView.PaneAt(1);

            // A Pane's minimum comes from USS (var(--appui-sizing-250)), so it is read back rather
            // than hardcoded -- a copied constant breaks silently whenever the sizing ramp is
            // retuned.
            var minimum = m_Direction == Direction.Horizontal
                ? second.resolvedStyle.minWidth.value
                : second.resolvedStyle.minHeight.value;

            // Drag far past the point where the second pane would run out of room.
            DragSplitter(splitter, 10000f);

            Assert.GreaterOrEqual(SizeAlongAxis(second), minimum,
                "The splitter should stop at the pane's minimum size.");
        }

        // ------------------------------------------------------------------ indicator

        [Test]
        public void SplitView_DefaultIndicator_ShowsTheLineAndHidesTheGrip()
        {
            var splitView = MountSplitView();
            var splitter = splitView.Q<Splitter>();

            Assert.AreEqual(SplitterIndicator.Stretch, splitView.indicator);
            Assert.AreEqual(DisplayStyle.Flex, splitter.Q(className: Splitter.anchorUssClassName).resolvedStyle.display,
                "The stretch line is the default indicator.");
            Assert.AreEqual(DisplayStyle.None, splitter.Q(className: Splitter.gripUssClassName).resolvedStyle.display,
                "The centered grip stays hidden by default.");
        }

        [Test]
        public void SplitView_CenteredIndicator_ShowsTheGripAndHidesTheLine()
        {
            var splitView = MountSplitView();
            splitView.indicator = SplitterIndicator.Centered;
            simulate.FrameUpdate();
            var splitter = splitView.Q<Splitter>();
            var grip = splitter.Q(className: Splitter.gripUssClassName);

            Assert.AreEqual(DisplayStyle.None, splitter.Q(className: Splitter.anchorUssClassName).resolvedStyle.display,
                "The centered indicator hides the stretch line.");
            Assert.AreEqual(DisplayStyle.Flex, grip.resolvedStyle.display,
                "The centered indicator shows the grip.");

            // The grip spans a quarter of the splitter's length, centered on it.
            var length = SizeAlongCrossAxis(splitter);
            var gripLength = SizeAlongCrossAxis(grip);
            Assert.That(gripLength, Is.EqualTo(length * 0.25f).Within(1f),
                "The grip spans 25% of the splitter's length.");
            var offset = m_Direction == Direction.Horizontal
                ? grip.worldBound.center.y - splitter.worldBound.center.y
                : grip.worldBound.center.x - splitter.worldBound.center.x;
            Assert.That(offset, Is.EqualTo(0f).Within(1f), "The grip is centered on the splitter.");

            splitView.indicator = SplitterIndicator.Stretch;
            simulate.FrameUpdate();
            Assert.AreEqual(DisplayStyle.None, grip.resolvedStyle.display,
                "Switching back to stretch hides the grip again.");
        }

        [Test]
        public void SplitView_Indicator_SwapsItsModifierClass()
        {
            var splitView = new SplitView(m_Direction);
            Assert.IsTrue(splitView.ClassListContains(SplitView.GetIndicatorUssClassName(SplitterIndicator.Stretch)),
                "A new SplitView carries the stretch modifier.");

            splitView.indicator = SplitterIndicator.Centered;
            Assert.AreEqual(SplitterIndicator.Centered, splitView.indicator);
            Assert.IsTrue(splitView.ClassListContains(SplitView.GetIndicatorUssClassName(SplitterIndicator.Centered)));
            Assert.IsFalse(splitView.ClassListContains(SplitView.GetIndicatorUssClassName(SplitterIndicator.Stretch)));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void SplitView_IndicatorDisplay_IsThemeOverridable()
        {
            // A theme picks the indicator through these custom properties; the indicator value is only
            // the fallback. Mirrors the registry's ResizableHandle contract.
            var uss = UssSource.Read("SplitView.uss");
            StringAssert.Contains("display: var(--appui-resizable-handle-stretch-display, flex)", uss);
            StringAssert.Contains("display: var(--appui-resizable-handle-centered-display, none)", uss);
            StringAssert.Contains("display: var(--appui-resizable-handle-stretch-display, none)", uss);
            StringAssert.Contains("display: var(--appui-resizable-handle-centered-display, flex)", uss);
        }

        float SizeAlongCrossAxis(VisualElement element) =>
            m_Direction == Direction.Horizontal ? element.worldBound.height : element.worldBound.width;
    }
}
#endif
