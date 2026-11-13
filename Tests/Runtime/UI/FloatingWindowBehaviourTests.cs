#if APPUI_UITF_PRESENT
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatingWindow))]
    class FloatingWindowBehaviourTests : UIComponentTests<FloatingWindow>
    {
        /// <summary>
        /// An in-memory <see cref="IWindowLayoutStorage"/>, so the tests never touch PlayerPrefs.
        /// </summary>
        class MemoryStorage : IWindowLayoutStorage
        {
            readonly Dictionary<string, string> m_Values = new Dictionary<string, string>();

            public string GetItem(string key) => m_Values.TryGetValue(key, out var v) ? v : null;

            public void SetItem(string key, string value) => m_Values[key] = value;

            public void RemoveItem(string key) => m_Values.Remove(key);
        }

        FloatingWindow MountWindow()
        {
            var window = new FloatingWindow
            {
                title = "Inspector",
                storage = new MemoryStorage(),
                storageKey = "test-window",
                defaultWidth = 360f,
                defaultHeight = 240f,
            };
            window.Add(new Text { text = "Content" });
            window.open = true;
            return MountElement(window);
        }

        [Test]
        public void FloatingWindow_DraggingHeader_ShouldMoveWindow()
        {
            var window = MountWindow();
            var header = window.Q(name: FloatingWindow.headerUssClassName);
            Assert.IsNotNull(header, "The FloatingWindow should have a header to drag from.");

            var xBefore = window.layoutGeometry.x;

            var from = header.worldBound.center;
            simulate.DragAndDrop(from, from + new Vector2(50f, 0f));
            simulate.FrameUpdate();

            Assert.Greater(window.layoutGeometry.x, xBefore,
                "Dragging the header to the right should move the window's x position.");
        }

        [Test]
        public void FloatingWindow_FirstDragOfDefaultedWindow_ShouldFollowPointer()
        {
            // A window without a saved layout is re-placed at its default once the container has a
            // size. The first drag must start from where it is drawn, not jump.
            var window = MountWindow();
            simulate.FrameUpdate();
            var header = window.Q(name: FloatingWindow.headerUssClassName);
            var before = window.layout.position;

            var from = header.worldBound.center;
            var delta = new Vector2(-20f, -10f);
            simulate.DragAndDrop(from, from + delta);
            simulate.FrameUpdate();

            var after = window.layout.position;
            Assert.AreEqual(before.x + delta.x, after.x, 1f, "The window should move by the pointer delta on x.");
            Assert.AreEqual(before.y + delta.y, after.y, 1f, "The window should move by the pointer delta on y.");
        }

        [Test]
        public void FloatingWindow_DragWithMargin_ShouldNotShiftByMargin()
        {
            // layout.position includes the margin; the drag must not add it again through style.left/top.
            var window = MountWindow();
            window.style.marginLeft = 12f;
            window.style.marginTop = 8f;
            simulate.FrameUpdate();
            var header = window.Q(name: FloatingWindow.headerUssClassName);
            var before = window.layout.position;

            var from = header.worldBound.center;
            var delta = new Vector2(-20f, -10f);
            simulate.DragAndDrop(from, from + delta);
            simulate.FrameUpdate();

            var after = window.layout.position;
            Assert.AreEqual(before.x + delta.x, after.x, 1f, "A drag should move by the pointer delta only, not the margin.");
            Assert.AreEqual(before.y + delta.y, after.y, 1f, "A drag should move by the pointer delta only, not the margin.");

            // A plain click must not shift the window by the margin either.
            var beforeClick = window.layout.position;
            simulate.Click(header);
            simulate.FrameUpdate();
            Assert.AreEqual(beforeClick.x, window.layout.position.x, 0.5f, "A click should not shift the window by its margin.");
            Assert.AreEqual(beforeClick.y, window.layout.position.y, 0.5f, "A click should not shift the window by its margin.");
        }

        [Test]
        public void FloatingWindow_CloseButtonClick_ShouldClose()
        {
            var window = MountWindow();
            var closed = false;
            window.openChanged += isOpen => closed = !isOpen;
            var closeButton = window.Q<ActionButton>(className: FloatingWindow.closeButtonUssClassName);
            Assert.IsNotNull(closeButton, "The FloatingWindow should have a close button.");

            simulate.Click(closeButton);

            Assert.IsFalse(window.open, "Clicking the close button should close the window.");
            Assert.IsTrue(closed, "Closing should raise openChanged(false).");
        }

        [Test]
        public void FloatingWindow_ClickHeaderWithoutMoving_ShouldNotMoveWindow()
        {
            // OnDragClick is a no-op by design: the title bar is a drag surface, not a button, so a
            // click with no movement should leave the window exactly where it was.
            var window = MountWindow();
            var header = window.Q(name: FloatingWindow.headerUssClassName);
            var xBefore = window.layoutGeometry.x;
            var yBefore = window.layoutGeometry.y;

            simulate.Click(header);
            simulate.FrameUpdate();

            Assert.AreEqual(xBefore, window.layoutGeometry.x, "A plain click on the header should not move the window.");
            Assert.AreEqual(yBefore, window.layoutGeometry.y, "A plain click on the header should not move the window.");
        }
    }
}
#endif
