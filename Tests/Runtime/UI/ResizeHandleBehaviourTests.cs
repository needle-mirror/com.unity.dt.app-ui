#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ResizeHandle))]
    class ResizeHandleBehaviourTests : UIComponentTests<ResizeHandle>
    {
        VisualElement m_Target;

        ResizeHandle MountHandle(Draggable.DragDirection direction)
        {
            // The handle resizes a sibling, not a child, so both are mounted directly rather than
            // through MountElement, which would clear the panel out from under the target.
            appUiPanel.Clear();

            m_Target = new VisualElement { style = { width = 200, height = 100 } };
            appUiPanel.Add(m_Target);

            var handle = new ResizeHandle(m_Target)
            {
                dragDirection = direction,
                style = { width = 20, height = 20 },
            };
            appUiPanel.Add(handle);

            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.Greater(handle.worldBound.width, 0f, "The handle needs a resolved layout to be draggable.");
            Assert.Greater(handle.worldBound.height, 0f, "The handle needs a resolved layout to be draggable.");

            return handle;
        }

        [Test]
        public void ResizeHandle_VerticalDrag_ShouldResizeTargetHeight()
        {
            var handle = MountHandle(Draggable.DragDirection.Vertical);
            var heightBefore = m_Target.layout.height;

            var from = handle.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(0f, 40f));
            simulate.FrameUpdate();

            Assert.Greater(m_Target.layout.height, heightBefore, "Dragging down should grow the target's height.");
        }

        [Test]
        public void ResizeHandle_VerticalDrag_ShouldInvokeResizeStartedAndEnded()
        {
            var handle = MountHandle(Draggable.DragDirection.Vertical);
            var started = 0;
            var ended = 0;
            handle.resizeStarted += _ => started++;
            handle.resizeEnded += _ => ended++;

            var from = handle.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(0f, 40f));
            simulate.FrameUpdate();

            Assert.AreEqual(1, started, "A drag should raise resizeStarted exactly once.");
            Assert.AreEqual(1, ended, "Releasing should raise resizeEnded exactly once.");
        }

        [Test]
        public void ResizeHandle_HorizontalDirection_ShouldNotResizeOnVerticalDrag()
        {
            var handle = MountHandle(Draggable.DragDirection.Horizontal);
            var widthBefore = m_Target.layout.width;
            var heightBefore = m_Target.layout.height;

            var from = handle.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(0f, 40f));
            simulate.FrameUpdate();

            Assert.AreEqual(heightBefore, m_Target.layout.height,
                "A handle restricted to horizontal drag should not resize the target's height.");
            Assert.AreEqual(widthBefore, m_Target.layout.width,
                "A purely vertical pointer move should not change the width either.");
        }
    }
}
#endif
