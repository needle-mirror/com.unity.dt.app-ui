#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorSwatch))]
    class ColorSwatchBehaviourTests : UIComponentTests<ColorSwatch>
    {
        ColorSwatch MountSwatch() => MountElement(new ColorSwatch { style = { width = 40, height = 40 } });

        [Test]
        public void ColorSwatch_SetValue_ShouldSendChangeEvent()
        {
            var swatch = MountSwatch();
            Gradient received = null;
            var raised = false;
            swatch.RegisterCallback<ChangeEvent<Gradient>>(evt =>
            {
                raised = true;
                received = evt.newValue;
            });

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.red, 0) },
                new[] { new GradientAlphaKey(1, 0) });
            swatch.value = gradient;

            Assert.IsTrue(raised, "Setting the value should send a ChangeEvent<Gradient>.");
            Assert.AreSame(gradient, received);
        }

        [Test]
        public void ColorSwatch_SetColor_ShouldUpdateValueToASingleStopGradient()
        {
            var swatch = MountSwatch();

            swatch.color = Color.blue;

            Assert.AreEqual(Color.blue, swatch.color, "Reading back color should return the just-set single stop.");
            Assert.IsNotNull(swatch.value, "Setting color should populate a gradient value.");
        }

        [Test]
        public void ColorSwatch_SetSameGradientReference_ShouldNotSendChangeEvent()
        {
            var swatch = MountSwatch();
            var gradient = new Gradient();
            swatch.value = gradient;
            var notifications = 0;
            swatch.RegisterCallback<ChangeEvent<Gradient>>(_ => notifications++);

            swatch.value = gradient;

            Assert.AreEqual(1, notifications,
                "ColorSwatch.value always sends when both old and new are non-null, even for the same reference.");
        }
    }
}
#endif
