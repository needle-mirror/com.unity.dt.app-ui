using System.Collections;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextField))]
    class TextFieldTests : VisualElementTests<TextField>
    {
        protected override string mainUssClassName => TextField.ussClassName;

        static UnityEngine.UIElements.TextField GetInputField(TextField field) =>
            field.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName);

        [Test]
        public void IsPassword_SetWhileDetached_DoesNotThrow()
        {
            // Enabling UITK's isPasswordField forces multiline off, which walks text backing
            // that only exists once the field belongs to a panel. Setting isPassword before
            // attaching used to throw a NullReferenceException from deep inside UITK.
            TextField field = null;
            Assert.DoesNotThrow(() => field = new TextField { isPassword = true });
            Assert.IsTrue(field.isPassword);

            Assert.DoesNotThrow(() => field.isPassword = false);
            Assert.IsFalse(field.isPassword);
        }

        [Test]
        public void IsPassword_SetInObjectInitializerWithOtherProperties_DoesNotThrow()
        {
            // Mirrors how the VisualDoc demos build a password field.
            TextField field = null;
            Assert.DoesNotThrow(() => field = new TextField
            {
                placeholder = "Enter password",
                isPassword = true,
                leadingIconName = "gear",
            });
            Assert.IsTrue(field.isPassword);
        }

        [UnityTest]
        public IEnumerator IsPassword_SetWhileDetached_IsAppliedOnceAttached()
        {
            var field = new TextField { isPassword = true };
            var input = GetInputField(field);
            Assert.IsNotNull(input, "Expected to find the underlying UITK TextField.");
            Assert.IsFalse(input.isPasswordField, "UITK field should be left untouched while detached.");

            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            Assert.IsTrue(input.isPasswordField, "isPassword should reach the UITK field on attach.");
            Assert.IsTrue(field.isPassword);
        }

        [UnityTest]
        public IEnumerator IsPassword_ToggledWhileAttached_UpdatesInputField()
        {
            var field = new TextField();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var input = GetInputField(field);
            Assert.IsNotNull(input);
            Assert.IsFalse(input.isPasswordField);

            field.isPassword = true;
            Assert.IsTrue(input.isPasswordField);

            field.isPassword = false;
            Assert.IsFalse(input.isPasswordField);
        }
    }
}
