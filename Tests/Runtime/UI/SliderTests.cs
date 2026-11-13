using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using SliderInt = Unity.AppUI.UI.SliderInt;
using SliderFloat = Unity.AppUI.UI.SliderFloat;
using FloatField = Unity.AppUI.UI.FloatField;
using IntField = Unity.AppUI.UI.IntField;

namespace Unity.AppUI.Tests.UI
{
    class SliderTests<T,TValue,TScalar,TInputField> : VisualElementTests<T>
        where T : Slider<TValue,TScalar,TInputField>, new()
        where TScalar : unmanaged, IFormattable
        where TInputField : VisualElement, IValidatableElement<TValue>, IFormattable<TScalar>, new()
    {
        protected virtual TScalar highValue { get; }

        protected virtual TScalar lowValue { get; }

        protected virtual TValue value { get; }

        protected virtual TScalar step { get; }

        protected virtual TScalar shiftStep { get; }

        protected virtual List<SliderMark<TScalar>> marks { get; }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", context => new T());
                yield return new Story("With Step Marks", context =>
                {
                    var slider = new T
                    {
                        highValue = highValue,
                        step = step,
                        shiftStep = shiftStep,
                        showMarks = true,
                        formatString = "0.0"
                    };
                    return slider;
                });
                yield return new Story("With Custom Marks And Labels", context =>
                {
                    var slider = new T
                    {
                        highValue = highValue,
                        showMarks = true,
                        showMarksLabel = true,
                        customMarks = marks
                    };
                    return slider;
                });
                yield return new Story("With Input Field", context =>
                {
                    var slider = new T
                    {
                        showInputField = true,
                        formatFunction = v => v.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                    };
                    return slider;
                });
                yield return new Story("With Label Display", context =>
                {
                    var slider = new T
                    {
                        displayValueLabel = ValueDisplayMode.On
                    };
                    return slider;
                });
                yield return new Story("With Track", context =>
                {
                    var slider = new T
                    {
                        highValue = highValue,
                        value = value,
                        track = TrackDisplayType.On
                    };
                    return slider;
                });
                yield return new Story("With Inverted Track", context =>
                {
                    var slider = new T
                    {
                        highValue = highValue,
                        value = value,
                        track = TrackDisplayType.Inverted
                    };
                    return slider;
                });
                yield return new Story("Vertical With Track And Marks And Labels", context =>
                {
                    var slider = new T
                    {
                        highValue = highValue,
                        value = value,
                        track = TrackDisplayType.On,
                        orientation = Direction.Vertical,
                        step = step,
                        showMarks = true,
                        showMarksLabel = true,
                        formatString = "0.0",
                        style = { height = 300 }
                    };
                    return slider;
                });
            }
        }

        protected virtual string highValueToUxml => $"high-value=\"{highValue.ToString()}\"";

        protected virtual string lowValueToUxml => $"low-value=\"{lowValue.ToString()}\"";

        protected virtual string valueToUxml => $"value=\"{value.ToString()}\"";

        protected override IEnumerable<string> uxmlTestCases
        {
            get
            {
                yield return $"<{uxmlNamespaceName}:{componentName} />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" style=\"height:300px;\" orientation=\"Vertical\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-input-field=\"true\" format-string=\"0.0\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} display-value-label=\"On\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} track=\"On\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} track=\"Inverted\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" track=\"On\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" track=\"Inverted\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" style=\"height:300px;\" orientation=\"Vertical\" track=\"On\" />";
                yield return $"<{uxmlNamespaceName}:{componentName} {highValueToUxml} {valueToUxml} show-marks=\"true\" show-marks-label=\"true\" format-string=\"0.0\" style=\"height:300px;\" orientation=\"Vertical\" track=\"Inverted\" />";
            }
        }
    }

    [TestFixture]
    [TestOf(typeof(SliderFloat))]
    class SliderFloatTests : SliderTests<SliderFloat,float,float,FloatField>
    {
        protected override string mainUssClassName => SliderFloat.ussClassName;

        protected override List<SliderMark<float>> marks => new List<SliderMark<float>>
        {
            new() {label = "Low", value = 0f},
            new() {label = "Middle", value = 5f},
            new() {label = "High", value = 10f},
        };

        protected override float highValue => 10f;

        protected override float lowValue => 0f;

        protected override float value => 3f;

        protected override float step => 1f;

        protected override float shiftStep => 2f;

        /// <summary>
        /// The registry's SliderField lets the numeric field reach past the track, so a quantity
        /// with no real ceiling can still offer a comfortable range to drag through. Without the
        /// widening, a typed 500 would be clamped back to the track's 10.
        /// </summary>
        [Test]
        public void FieldHighValue_WidensTheAcceptedValuePastTheTrack()
        {
            var slider = new SliderFloat { lowValue = 0f, highValue = 10f, fieldHighValue = 500f };

            slider.value = 250f;
            Assert.AreEqual(250f, slider.value, 0.001f,
                "A value inside the widened field bounds must survive, even though it is past the track.");

            slider.value = 900f;
            Assert.AreEqual(500f, slider.value, 0.001f,
                "A value past fieldHighValue must still clamp to it.");
        }

        [Test]
        public void FieldLowValue_WidensTheAcceptedValueBelowTheTrack()
        {
            var slider = new SliderFloat { lowValue = 0f, highValue = 10f, fieldLowValue = -50f };

            slider.value = -25f;
            Assert.AreEqual(-25f, slider.value, 0.001f);

            slider.value = -100f;
            Assert.AreEqual(-50f, slider.value, 0.001f);
        }

        /// <summary>
        /// Widening only. A field bound inside the track would let a drag reach a number the field
        /// then refused to take back, so it is ignored.
        /// </summary>
        [Test]
        public void FieldBoundsInsideTheTrack_AreIgnored()
        {
            var slider = new SliderFloat
            {
                lowValue = 0f, highValue = 10f, fieldLowValue = 2f, fieldHighValue = 8f,
            };

            slider.value = 10f;
            Assert.AreEqual(10f, slider.value, 0.001f,
                "A narrowing fieldHighValue must not clamp a value the track can reach.");

            slider.value = 0f;
            Assert.AreEqual(0f, slider.value, 0.001f,
                "A narrowing fieldLowValue must not clamp a value the track can reach.");
        }

        [Test]
        public void WithoutFieldBounds_TheTrackStillClamps()
        {
            var slider = new SliderFloat { lowValue = 0f, highValue = 10f };

            slider.value = 50f;
            Assert.AreEqual(10f, slider.value, 0.001f,
                "With no widening set, the value must clamp to the track exactly as before.");
        }

        /// <summary>
        /// The widened bounds exist for typed input. An arrow key on the track must not walk the
        /// value past the end the thumb can actually reach, which is what would happen if the key
        /// handlers were left to fall through to the widened value clamp.
        /// </summary>
        [UnityTest]
        public IEnumerator ArrowKeyStepping_StaysOnTheTrackEvenWhenTheFieldIsWidened()
        {
            var slider = new SliderFloat
            {
                lowValue = 0f, highValue = 10f, fieldHighValue = 500f, step = 1f, value = 10f,
            };

            m_TestUI.rootVisualElement.Clear();
            m_TestUI.rootVisualElement.Add(slider);
            yield return null;

            var control = slider.Q(className: SliderFloat.sliderControlUssClassName);
            Assert.IsNotNull(control, "The slider control is the element carrying the key handler.");
            control.Focus();
            yield return null;

            using (var evt = KeyDownEvent.GetPooled('\0', KeyCode.RightArrow, EventModifiers.None))
            {
                evt.target = control;
                control.SendEvent(evt);
            }
            yield return null;

            Assert.AreEqual(10f, slider.value, 0.001f,
                "Stepping up from the track's end must stay at the end, not enter the widened range.");
        }
    }

    [TestFixture]
    [TestOf(typeof(SliderInt))]
    class SliderIntTests : SliderTests<SliderInt,int,int,IntField>
    {
        protected override string mainUssClassName => SliderInt.ussClassName;

        protected override List<SliderMark<int>> marks => new List<SliderMark<int>>
        {
            new() {label = "Low", value = 0},
            new() {label = "Middle", value = 5},
            new() {label = "High", value = 10},
        };

        protected override int highValue => 10;

        protected override int lowValue => 0;

        protected override int value => 3;

        protected override int step => 1;

        protected override int shiftStep => 2;
    }

    [TestFixture]
    [TestOf(typeof(SliderFloat))]
    class SliderFloatRestrictedKeysTests
    {
        class KeySliderFloat : SliderFloat
        {
            public void SendKey(KeyCode key, EventModifiers modifiers = EventModifiers.None)
            {
                using var evt = KeyDownEvent.GetPooled('\0', key, modifiers);
                OnKeyDown(evt);
            }
        }

        static KeySliderFloat Create(RestrictedValuesPolicy policy)
        {
            return new KeySliderFloat
            {
                lowValue = 0,
                highValue = 100,
                step = 0.1f,
                shiftStep = 1f,
                restrictedValues = policy,
                value = 50f
            };
        }

        [TestCase(RestrictedValuesPolicy.None, KeyCode.RightArrow, EventModifiers.None, 50.1f)]
        [TestCase(RestrictedValuesPolicy.None, KeyCode.LeftArrow, EventModifiers.Shift, 49f)]
        [TestCase(RestrictedValuesPolicy.Step, KeyCode.RightArrow, EventModifiers.None, 50.1f)]
        [TestCase(RestrictedValuesPolicy.Step, KeyCode.RightArrow, EventModifiers.Shift, 51f)]
        [TestCase(RestrictedValuesPolicy.Step, KeyCode.LeftArrow, EventModifiers.Shift, 49f)]
        [TestCase(RestrictedValuesPolicy.CtrlStep, KeyCode.RightArrow, EventModifiers.None, 50.1f)]
        [TestCase(RestrictedValuesPolicy.CtrlStep, KeyCode.RightArrow, EventModifiers.Control, 50.1f)]
        [TestCase(RestrictedValuesPolicy.CtrlStep, KeyCode.LeftArrow, EventModifiers.Control | EventModifiers.Shift, 49f)]
        public void ArrowKeys_MoveByTheStep_WhenValuesAreRestricted(RestrictedValuesPolicy policy, KeyCode key, EventModifiers modifiers, float expected)
        {
            var slider = Create(policy);
            Assert.DoesNotThrow(() => slider.SendKey(key, modifiers));
            Assert.AreEqual(expected, slider.value, 1e-3f);
        }
    }
}
