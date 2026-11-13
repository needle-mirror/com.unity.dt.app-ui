using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    class DraggableElementTests
    {
        [Test]
        public void Draggable_AcceptsMouseTouchPenAndTrackedPointers()
        {
            Assert.IsTrue(Draggable.IsValidPointerType(PointerId.mousePointerId));
            Assert.IsTrue(Draggable.IsValidPointerType(PointerId.touchPointerIdBase));
            Assert.IsTrue(Draggable.IsValidPointerType(PointerId.touchPointerIdBase + PointerId.touchPointerCount - 1));
            Assert.IsTrue(Draggable.IsValidPointerType(PointerId.penPointerIdBase));
            Assert.IsTrue(Draggable.IsValidPointerType(PointerId.trackedPointerIdBase));
            Assert.IsFalse(Draggable.IsValidPointerType(PointerId.invalidPointerId));
        }
    }
}
