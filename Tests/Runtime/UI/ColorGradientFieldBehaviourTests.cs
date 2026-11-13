#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorGradientField))]
    class ColorGradientFieldBehaviourTests : UIComponentTests<ColorGradientField>
    {
        ColorGradientField MountGradientField() => MountElement(new ColorGradientField { style = { width = 100, height = 24 } });

        [Test]
        public void ColorGradientField_Click_ShouldInvokePickRequested()
        {
            var field = MountGradientField();
            var requested = 0;
            field.pickRequested += () => requested++;

            simulate.Click(field);

            Assert.AreEqual(1, requested, "Clicking the ColorGradientField should raise pickRequested.");
        }

        [Test]
        public void ColorGradientField_SubmitKey_ShouldInvokePickRequested()
        {
            var field = MountGradientField();
            var requested = 0;
            field.pickRequested += () => requested++;

            field.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, requested, "Submitting on a focused ColorGradientField should raise pickRequested.");
        }

        [Test]
        public void ColorGradientField_SetValue_ShouldNotifyValueChanged()
        {
            var field = MountGradientField();
            var notified = 0;
            field.RegisterCallback<ChangeEvent<Gradient>>(_ => notified++);
            var gradient = new Gradient();

            field.value = gradient;

            Assert.AreEqual(1, notified, "Setting a new gradient value should send exactly one ChangeEvent.");
            Assert.AreEqual(gradient, field.value, "The field should report back the gradient it was given.");
        }

        [Test]
        public void ColorGradientField_WhenDisabled_Click_ShouldNotInvokePickRequested()
        {
            var field = MountGradientField();
            var requested = 0;
            field.pickRequested += () => requested++;
            field.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(field);

            Assert.AreEqual(0, requested, "A disabled ColorGradientField should not raise pickRequested.");
        }
    }
}
#endif
