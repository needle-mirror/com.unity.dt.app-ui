using System;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    class NumericalFieldTests<T, U> : VisualElementTests<T>
        where T : NumericalField<U>, new()
        where U : struct, IComparable, IComparable<U>, IFormattable
    {
#if ENABLE_VALUEFIELD_INTERFACE
        [UnityTest]
        [Order(10)]
        public IEnumerator CanReceiveDragContextFromInputLabel()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var inputLabel = new InputLabel("Test Label") { draggable = true };
            var field = new T
            {
                acceptDragging = true
            };
            inputLabel.Add(field);
            panel.Add(inputLabel);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Simulate drag context
            var initialValue = field.value;
            inputLabel.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(10f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            // Value should have changed from the drag
            Assert.AreNotEqual(initialValue, field.value);
        }
#endif

        [Test]
        [Order(11)]
        public void GetIncrementFactorReturnsValidValue()
        {
            var field = new T();

            // Test that GetIncrementFactor returns a non-zero value
            var factor = field.InvokeGetIncrementFactor(default(U));
            Assert.Greater(factor, 0f);
        }
    }
}
