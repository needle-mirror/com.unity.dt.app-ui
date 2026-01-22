using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using FloatField = Unity.AppUI.UI.FloatField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(FloatField))]
    class FloatFieldTests : NumericalFieldTests<FloatField, float>
    {
        protected override string mainUssClassName => FloatField.ussClassName;

        static readonly float[] k_CanSetValueCases = new [] { 1f, 2f, 3f };

        [UnityTest]
        [Order(3)]
        public IEnumerator CanSetValue([ValueSource(nameof(k_CanSetValueCases))]float expected)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var field = new FloatField();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(field);

            yield return null;

            var callCount = 0;
            void ValueChanged(ChangeEvent<float> evt)
            {
                Assert.AreEqual(expected, evt.newValue);
                callCount++;
            }

            field.RegisterValueChangedCallback(ValueChanged);

            field.value = expected;
            field.value = expected;

            yield return new WaitUntilOrTimeOut(() => callCount > 0);

            Assert.AreEqual(1, callCount);
        }

        [Test]
        [Order(4)]
        [TestCase(0, 1, 0, 0)]
        [TestCase(0, 1, 1, 1)]
        [TestCase(0, 1, 2, 1)]
        [TestCase(0, 1, -1, 0)]
        [TestCase(0, 1, -2, 0)]
        public void CanClampValue(float min, float max, float val, float expected)
        {
            var field = new FloatField { lowValue = min, highValue = max, value = val };
            Assert.AreEqual(expected, field.value);
        }

        static readonly (string text, int expected)[] k_CanEnterValueCases = new [] { ("1", 1), ("2", 2), ("3", 3) };

        [UnityTest]
        [Order(5)]
        public IEnumerator CanEnterValue([ValueSource(nameof(k_CanEnterValueCases))] (string text, int expected) expected)
        {
            if (!Application.isEditor)
            {
                // skip test and mark as ignored
                Assert.Ignore("Can't run this test outside of the editor");
                yield break;
            }

            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var field = new FloatField();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(field);

            yield return null;

            field.Focus();

            yield return null;

            var changingCount = 0;
            void ValueChanging(ChangingEvent<float> evt)
            {
                Assert.AreEqual(expected.expected, evt.newValue);
                changingCount++;
            }

            var changedCount = 0;
            void ValueChanged(ChangeEvent<float> evt)
            {
                Assert.AreEqual(expected.expected, evt.newValue);
                changedCount++;
            }

            field.RegisterValueChangingCallback(ValueChanging);
            field.RegisterValueChangedCallback(ValueChanged);

            field.Q<UnityEngine.UIElements.TextField>(FloatField.inputUssClassName).value = expected.text;

            yield return new WaitUntilOrTimeOut(() => changingCount > 0);

            Assert.AreEqual(1, changingCount);

            Assert.AreEqual(0, changedCount);

            field.Blur();

            yield return new WaitUntilOrTimeOut(() => changedCount > 0);

            Assert.AreEqual(1, changedCount);
        }

#if ENABLE_VALUEFIELD_INTERFACE
        [UnityTest]
        [Order(6)]
        public IEnumerator CanIncrementValueWithDrag()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var inputLabel = new InputLabel("Test") { draggable = true };
            var field = new FloatField { value = 100f, acceptDragging = true };
            inputLabel.Add(field);
            panel.Add(inputLabel);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            var initialValue = field.value;

            // Simulate drag by providing context
            inputLabel.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(10f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            // Value should have increased
            Assert.Greater(field.value, initialValue);
        }
#endif

        [Test]
        [Order(7)]
        public void GetIncrementFactorScalesWithValue()
        {
            var field = new FloatField();

            // Test that increment factor scales appropriately
            var factor1 = field.InvokeGetIncrementFactor(0.001f);
            var factor2 = field.InvokeGetIncrementFactor(1f);
            var factor3 = field.InvokeGetIncrementFactor(1000f);

            Assert.Greater(factor2, factor1);
            Assert.Greater(factor3, factor2);
        }
    }
}
