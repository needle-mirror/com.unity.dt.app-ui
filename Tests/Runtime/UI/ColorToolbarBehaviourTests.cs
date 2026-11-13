#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorToolbar))]
    class ColorToolbarBehaviourTests : UIComponentTests<ColorToolbar>
    {
        ColorToolbar MountToolbar() => MountElement(new ColorToolbar
        {
            previousColor = Color.red,
            currentColor = Color.blue,
        });

        [Test]
        public void ColorToolbar_ClickPreviousSwatch_ShouldInvokePreviousColorSwatchClicked()
        {
            var toolbar = MountToolbar();
            // previousColor/currentColor are set through the object initializer, before the
            // ColorToolbar is ever attached to a panel, and ColorSwatch defers its texture/visual
            // refresh to AttachToPanelEvent. Give the swatch's own layout (its height comes from a
            // percentage that only resolves once its ancestors have stretched to the toolbar's own
            // height) one more pass to settle before trusting its worldBound for a position-based
            // click.
            simulate.FrameUpdate();
            Assert.Greater(toolbar.previousColorSwatch.worldBound.width, 0f,
                "previousColorSwatch has no resolved layout, so simulate.Click would silently miss it.");
            Assert.Greater(toolbar.previousColorSwatch.worldBound.height, 0f,
                "previousColorSwatch has no resolved layout, so simulate.Click would silently miss it.");
            var clickCount = 0;
            toolbar.previousColorSwatchClicked += () => clickCount++;

            simulate.Click(toolbar.previousColorSwatch);

            Assert.AreEqual(1, clickCount, "Clicking the previous color swatch should invoke previousColorSwatchClicked.");
        }

        [Test]
        public void ColorToolbar_EyeDropperButton_Click_ShouldInvokeClicked()
        {
            var toolbar = MountToolbar();
            var clickCount = 0;
            toolbar.eyeDropperButton.clicked += () => clickCount++;

            simulate.Click(toolbar.eyeDropperButton);

            Assert.AreEqual(1, clickCount, "Clicking the eye dropper button should invoke its clicked event.");
        }

        [Test]
        public void ColorToolbar_WhenPreviousSwatchDisabled_Click_ShouldNotInvokeClicked()
        {
            var toolbar = MountToolbar();
            toolbar.previousColorSwatch.SetEnabled(false);
            simulate.FrameUpdate();
            var clickCount = 0;
            toolbar.previousColorSwatchClicked += () => clickCount++;

            simulate.Click(toolbar.previousColorSwatch);

            Assert.AreEqual(0, clickCount, "A disabled previous color swatch should not invoke the click event.");
        }
    }
}
#endif
