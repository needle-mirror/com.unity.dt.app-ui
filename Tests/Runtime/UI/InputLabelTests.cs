using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(InputLabel))]
    class InputLabelTests : VisualElementTests<InputLabel>
    {
        protected override string mainUssClassName => InputLabel.ussClassName;

#if ENABLE_VALUEFIELD_INTERFACE
        [Test]
        [Order(10)]
        public void CanSetDraggableProperty()
        {
            var label = new InputLabel("Test");
            Assert.IsFalse(label.draggable);

            label.draggable = true;
            Assert.IsTrue(label.draggable);

            label.draggable = false;
            Assert.IsFalse(label.draggable);
        }

        [UnityTest]
        [Order(11)]
        public IEnumerator DraggableProvidesDragContext()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var label = new InputLabel("Test") { draggable = true };
            panel.Add(label);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Simulate providing drag context
            label.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(5f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            // Check that context is provided
            var context = label.GetSelfContext<DragContext>();
            Assert.IsNotNull(context);
            Assert.AreEqual(5f, context.delta.x);
        }
#endif
    }
}
