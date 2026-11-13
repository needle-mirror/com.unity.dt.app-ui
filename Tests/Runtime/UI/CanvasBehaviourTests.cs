#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using UnityEngine;
using Canvas = Unity.AppUI.UI.Canvas;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Canvas))]
    class CanvasBehaviourTests : UIComponentTests<Canvas>
    {
        Canvas MountCanvas()
        {
            var canvas = new Canvas { style = { width = 400, height = 300 } };
            var mounted = MountElement(canvas);
            simulate.FrameUpdate();
            return mounted;
        }

        [Test]
        public void Canvas_SetZoom_ShouldInvokeZoomChanged()
        {
            var canvas = MountCanvas();
            var notifications = 0;
            canvas.zoomChanged += () => notifications++;

            canvas.zoom = canvas.zoom * 2f;
            simulate.FrameUpdate();

            Assert.Greater(notifications, 0, "Changing zoom should raise zoomChanged.");
        }

        [Test]
        public void Canvas_SetScrollOffset_ShouldInvokeScrollOffsetChanged()
        {
            var canvas = MountCanvas();
            var notifications = 0;
            canvas.scrollOffsetChanged += () => notifications++;

            canvas.scrollOffset = new UnityEngine.Vector2(10f, 10f);
            simulate.FrameUpdate();

            Assert.Greater(notifications, 0, "Changing scrollOffset should raise scrollOffsetChanged.");
        }

        [Test]
        public void Canvas_FKeyPress_ShouldFrameContentWithoutThrowing()
        {
            var canvas = MountCanvas();
            var content = new VisualElement { style = { width = 50, height = 50 } };
            canvas.Add(content);
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            canvas.Focus();
            simulate.FrameUpdate();

            // OnKeyDown handles KeyCode.F by calling FrameAll(). There is no dedicated "frame all"
            // event, so this is a smoke test that the shortcut runs to completion.
            Assert.DoesNotThrow(() => simulate.KeyDown(KeyCode.F), "Pressing F should frame the content without throwing.");
        }

        // A pan is taken in the capture phase (#1460): the node under the pointer never sees the
        // press, so a pan neither selects it nor drags it.
        (Canvas canvas, CanvasNode node) MountCanvasWithNode(int[] selections)
        {
            var canvas = MountCanvas();
            var node = new CanvasNode
            {
                title = "Node",
                movable = true,
                x = 40f,
                y = 40f,
                style = { height = 100 },
            };
            node.selectRequested += () => selections[0]++;
            canvas.Add(node);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            Assert.Greater(node.worldBound.width, 0f, "The node needs a layout to be pressed.");
            return (canvas, node);
        }

        [Test]
        public void Canvas_MiddleDragOverNode_PansAndLeavesSelectionUnchanged()
        {
            var selections = new int[1];
            var (canvas, node) = MountCanvasWithNode(selections);
            var offsetBefore = canvas.scrollOffset;

            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(60f, 40f), MouseButton.MiddleMouse);
            simulate.FrameUpdate();

            Assert.AreNotEqual(offsetBefore, canvas.scrollOffset, "A middle-button drag should pan the canvas.");
            Assert.AreEqual(0, selections[0], "A middle-button pan over a node should not select it.");
            Assert.AreEqual(40f, node.x.Value, "A middle-button pan should not move the node.");
        }

        [Test]
        public void Canvas_SpaceDragOverNode_PansAndLeavesSelectionUnchanged()
        {
            var selections = new int[1];
            var (canvas, node) = MountCanvasWithNode(selections);
            var offsetBefore = canvas.scrollOffset;
            canvas.Focus();
            simulate.FrameUpdate();

            simulate.KeyDown(KeyCode.Space);
            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(60f, 40f));
            simulate.KeyUp(KeyCode.Space);
            simulate.FrameUpdate();

            Assert.AreNotEqual(offsetBefore, canvas.scrollOffset, "A Space+drag should pan the canvas.");
            Assert.AreEqual(0, selections[0], "A Space+drag over a node should not select it.");
            Assert.AreEqual(40f, node.x.Value, "A Space+drag should pan, not drag the movable node.");
        }

        [Test]
        public void Canvas_AltDragOverNode_InEditorScheme_PansAndLeavesSelectionUnchanged()
        {
            var selections = new int[1];
            var (canvas, node) = MountCanvasWithNode(selections);
            canvas.controlScheme = CanvasControlScheme.Editor;
            var offsetBefore = canvas.scrollOffset;

            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(60f, 40f), MouseButton.LeftMouse, EventModifiers.Alt);
            simulate.FrameUpdate();

            Assert.AreNotEqual(offsetBefore, canvas.scrollOffset, "An Alt+drag should pan an Editor-scheme canvas.");
            Assert.AreEqual(0, selections[0], "An Alt+drag over a node should not select it.");
        }

        [Test]
        public void Canvas_PrimaryClickOnNode_StillSelects()
        {
            var selections = new int[1];
            var (canvas, node) = MountCanvasWithNode(selections);
            var offsetBefore = canvas.scrollOffset;

            simulate.Click(node);

            Assert.AreEqual(1, selections[0], "A plain primary click on a node should still select it.");
            Assert.AreEqual(offsetBefore, canvas.scrollOffset, "A plain primary click should not pan.");
        }
    }
}
#endif
