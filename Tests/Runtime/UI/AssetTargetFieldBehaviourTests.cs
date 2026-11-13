#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Unity.AppUI.Core;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AssetTargetField))]
    class AssetTargetFieldBehaviourTests : UIComponentTests<AssetTargetField>
    {
        static AssetReference Bronze() => new AssetReference
        {
            name = "Bronze",
            type = "Material",
            typeIcon = "sphere",
        };

        [Test]
        public void AssetTargetField_Click_ShouldInvokePickRequested()
        {
            var field = MountElement(new AssetTargetField());
            var notified = 0;
            field.pickRequested += () => notified++;

            simulate.Click(field);

            Assert.AreEqual(1, notified, "Clicking the field should request the picker to open.");
        }

        [Test]
        public void AssetTargetField_ClickClearButton_ShouldClearValueAndNotify()
        {
            var field = MountElement(new AssetTargetField { value = Bronze() });
            var cleared = 0;
            field.cleared += () => cleared++;
            var changedCount = 0;
            field.RegisterValueChangedCallback(_ => changedCount++);

            simulate.Click(field.clearButton);

            Assert.IsNull(field.value, "Clicking the clear button should drop the assigned reference.");
            Assert.AreEqual(1, cleared, "Clearing the value should invoke the cleared event.");
            Assert.AreEqual(1, changedCount, "Clearing the value should also raise the value changed event.");
        }

        [Test]
        public void AssetTargetField_WhenDisabled_Click_ShouldNotInvokePickRequested()
        {
            var field = MountElement(new AssetTargetField());
            field.SetEnabled(false);
            simulate.FrameUpdate();
            var notified = 0;
            field.pickRequested += () => notified++;

            simulate.Click(field);

            Assert.AreEqual(0, notified, "A disabled AssetTargetField should not react to clicks.");
        }
    }
}
#endif
