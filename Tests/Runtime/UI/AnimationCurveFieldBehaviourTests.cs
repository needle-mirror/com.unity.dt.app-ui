#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AnimationCurveField))]
    class AnimationCurveFieldBehaviourTests : UIComponentTests<AnimationCurveField>
    {
        AnimationCurveField MountField() => MountElement(new AnimationCurveField
        {
            value = AnimationCurve.Linear(0f, 0f, 1f, 1f),
            style = { width = 120, height = 32 },
        });

        [Test]
        public void AnimationCurveField_Click_ShouldRaisePickRequested()
        {
            var field = MountField();
            var requests = 0;
            field.pickRequested += () => requests++;

            simulate.Click(field);

            Assert.AreEqual(1, requests, "Clicking the field should raise pickRequested even with no picker assigned.");
        }

        [Test]
        public void AnimationCurveField_SetValue_ShouldSendChangeEvent()
        {
            var field = MountField();
            AnimationCurve received = null;
            var raised = false;
            field.RegisterCallback<ChangeEvent<AnimationCurve>>(evt =>
            {
                raised = true;
                received = evt.newValue;
            });

            var newCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            field.value = newCurve;

            Assert.IsTrue(raised, "Setting the value should send a ChangeEvent<AnimationCurve>.");
            Assert.AreSame(newCurve, received);
        }

        [Test]
        public void AnimationCurveField_WhenDisabled_Click_ShouldNotRaisePickRequested()
        {
            var field = MountField();
            field.SetEnabled(false);
            simulate.FrameUpdate();
            var requests = 0;
            field.pickRequested += () => requests++;

            simulate.Click(field);

            Assert.AreEqual(0, requests, "A disabled AnimationCurveField should not respond to a click.");
        }
    }
}
#endif
