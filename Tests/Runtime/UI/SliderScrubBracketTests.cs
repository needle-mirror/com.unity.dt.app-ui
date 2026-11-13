using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using SliderFloat = Unity.AppUI.UI.SliderFloat;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// The scrub bracket a slider drag opens and closes, which is what a caller needs to open and close
    /// a single undo group while the value streams.
    /// </summary>
    /// <remarks>
    /// The bracket lives on <see cref="BaseSlider{TValue,TScalar}"/>, so a probe subclass is the only
    /// way to exercise it without a synthetic pointer: <c>BeginScrub</c> and <c>EndScrub</c> are what
    /// the track's own pointer handlers call.
    /// </remarks>
    [TestFixture]
    [TestOf(typeof(SliderFloat))]
    class SliderScrubBracketTests : VisualElementTests<SliderScrubBracketTests.ScrubProbeSlider>
    {
        internal class ScrubProbeSlider : SliderFloat
        {
            public void Begin() => BeginScrub();

            public void End() => EndScrub();
        }

        protected override bool uxmlConstructable => false;

        protected override bool capturesSnapshots => false;

        [Test]
        public void ASliderIsNotScrubbingUntilAGestureStarts()
        {
            var slider = new ScrubProbeSlider();
            Assert.IsFalse(slider.scrubbing);
        }

        [Test]
        public void TheBracketRaisesEachEventOnce()
        {
            var slider = new ScrubProbeSlider();
            var started = 0;
            var ended = 0;
            slider.scrubStarted += () => started++;
            slider.scrubEnded += () => ended++;

            slider.Begin();
            Assert.IsTrue(slider.scrubbing);
            Assert.AreEqual(1, started);
            Assert.AreEqual(0, ended);

            slider.Begin();
            Assert.AreEqual(1, started, "A second start inside one gesture is still one gesture.");

            slider.End();
            Assert.IsFalse(slider.scrubbing);
            Assert.AreEqual(1, ended);

            slider.End();
            Assert.AreEqual(1, ended, "Closing a closed bracket must not raise an end without a start.");
        }

        /// <summary>
        /// The whole point of the bracket: an end always follows a start. A slider that leaves the panel
        /// mid-drag fires no pointer up of its own, and a caller's undo group would stay open for the
        /// rest of the session.
        /// </summary>
        [UnityTest]
        public IEnumerator LeavingThePanelMidDragClosesTheBracket()
        {
            var slider = new ScrubProbeSlider();
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(slider);
            yield return null;

            var ended = 0;
            slider.scrubEnded += () => ended++;
            slider.Begin();
            Assert.IsTrue(slider.scrubbing);

            slider.RemoveFromHierarchy();
            yield return null;

            Assert.AreEqual(1, ended, "Detaching mid-drag left the bracket open.");
            Assert.IsFalse(slider.scrubbing);
        }

        /// <summary>
        /// Keyboard steps are not a gesture: there is nothing to bracket, and a step per key press would
        /// otherwise open and close a bracket for each one.
        /// </summary>
        [Test]
        public void SettingTheValueFromCodeIsNotAGesture()
        {
            var slider = new ScrubProbeSlider { lowValue = 0f, highValue = 10f };
            var started = 0;
            slider.scrubStarted += () => started++;

            slider.value = 5f;
            Assert.AreEqual(0, started);
            Assert.IsFalse(slider.scrubbing);
        }
    }
}
