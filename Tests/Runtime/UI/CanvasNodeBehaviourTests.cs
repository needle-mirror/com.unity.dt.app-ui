#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CanvasNode))]
    class CanvasNodeBehaviourTests : UIComponentTests<CanvasNode>
    {
        CanvasNode MountNode(bool movable)
        {
            var node = new CanvasNode
            {
                title = "Node",
                movable = movable,
                x = 0f,
                y = 0f,
                style = { height = 100 },
            };
            var mounted = MountElement(node);
            simulate.FrameUpdate();
            return mounted;
        }

        [Test]
        public void CanvasNode_PointerDown_ShouldInvokeSelectRequested()
        {
            var node = MountNode(false);
            var notifications = 0;
            node.selectRequested += () => notifications++;

            simulate.MouseDown(node);

            Assert.AreEqual(1, notifications, "Pressing the node should raise selectRequested.");
        }

        [Test]
        public void CanvasNode_MiddlePress_ShouldNotInvokeSelectRequested()
        {
            var node = MountNode(false);
            var notifications = 0;
            node.selectRequested += () => notifications++;

            simulate.MouseDown(node, MouseButton.MiddleMouse);
            simulate.MouseUp(node, MouseButton.MiddleMouse);

            Assert.AreEqual(0, notifications, "Only a primary press selects; a middle press is a pan.");
        }

        [Test]
        public void CanvasNode_SecondaryPress_ShouldNotInvokeSelectRequested()
        {
            var node = MountNode(false);
            var notifications = 0;
            node.selectRequested += () => notifications++;

            simulate.MouseDown(node, MouseButton.RightMouse);
            simulate.MouseUp(node, MouseButton.RightMouse);

            Assert.AreEqual(0, notifications, "Only a primary press selects.");
        }

        [Test]
        public void CanvasNode_WhenMovable_MiddleDrag_ShouldNotMove()
        {
            var node = MountNode(true);
            var notifications = 0;
            node.positionChanged += _ => notifications++;

            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(50f, 30f), MouseButton.MiddleMouse);
            simulate.FrameUpdate();

            Assert.AreEqual(0, notifications, "The node's drag answers the primary button only.");
            Assert.AreEqual(0f, node.x.Value, "A middle-button drag should not move the node.");
        }

        // CanvasPort.description is the port row's hover tooltip (#1460), on both sides.
        [TestCase(CanvasPortSide.Input)]
        [TestCase(CanvasPortSide.Output)]
        public void CanvasNode_HoveringAPort_ShowsItsDescriptionAsATooltip(CanvasPortSide side)
        {
            appUiPanel.forceUseTooltipSystem = true;
            appUiPanel.tooltipDelayMs = 0;
            var node = new CanvasNode
            {
                title = "Node",
                x = 0f,
                y = 0f,
                inputPorts = new[] { new CanvasPort("in", "Base", description: "Float input") },
                outputPorts = new[] { new CanvasPort("out", "Result", description: "Float output") },
            };
            MountElement(node);
            simulate.FrameUpdate();

            var row = node.GetPortElement(side, 0);
            Assert.Greater(row.worldBound.width, 0f, "The port row needs a layout to be hovered.");
            simulate.MouseMove(row.worldBound.center);
            for (var i = 0; i < 4; i++)
                simulate.FrameUpdate();

            var expected = side == CanvasPortSide.Input ? "Float input" : "Float output";
            var shown = appUiPanel.tooltipContainer.Q<LocalizedTextElement>(className: "appui-tooltip__content");
            Assert.IsNotNull(shown, "Hovering a port with a description should show a tooltip.");
            Assert.AreEqual(expected, shown.text, "The tooltip should carry the port's description.");
        }

        [Test]
        public void CanvasNode_WhenMovable_Drag_ShouldChangePositionAndNotify()
        {
            var node = MountNode(true);
            var notifications = 0;
            node.positionChanged += _ => notifications++;

            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(50f, 30f));
            simulate.FrameUpdate();

            Assert.Greater(notifications, 0, "Dragging a movable node should raise positionChanged.");
            Assert.IsTrue(node.x.IsSet && node.x.Value > 0f, "Dragging right should increase x.");
            Assert.IsTrue(node.y.IsSet && node.y.Value > 0f, "Dragging down should increase y.");
        }

        [Test]
        public void CanvasNode_WhenNotMovable_Drag_ShouldNotChangePosition()
        {
            var node = MountNode(false);
            var notifications = 0;
            node.positionChanged += _ => notifications++;

            var from = node.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(50f, 30f));
            simulate.FrameUpdate();

            Assert.AreEqual(0, notifications, "A non-movable node should not react to a drag.");
            Assert.AreEqual(0f, node.x.Value, "x should stay unchanged when the node is not movable.");
        }
    }
}
#endif
