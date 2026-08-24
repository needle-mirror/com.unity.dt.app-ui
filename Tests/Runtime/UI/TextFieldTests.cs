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
        public void UnderlyingInputField_TextIsNeverNull()
        {
            // Before Unity 6000.0, UITK's multiline setter calls text.Replace("\n", "")
            // with no null check, so a null text turns isPasswordField = true into a
            // NullReferenceException. The default ctor seeds the value with null.
            Assert.IsNotNull(GetInputField(new TextField()).text);
            Assert.IsNotNull(GetInputField(new TextField(null)).text);

            var field = new TextField("abc");
            field.SetValueWithoutNotify(null);
            Assert.IsNotNull(GetInputField(field).text);
        }

        [Test]
        public void IsPassword_SetWhileDetached_DoesNotThrow()
        {
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

        [Test]
        public void IsPassword_SetAfterClearingValue_DoesNotThrow()
        {
            var field = new TextField("secret");
            field.SetValueWithoutNotify(null);

            Assert.DoesNotThrow(() => field.isPassword = true);
            Assert.IsTrue(field.isPassword);
        }

        [UnityTest]
        public IEnumerator IsPassword_SetWhileDetached_SurvivesAttach()
        {
            var field = new TextField { isPassword = true };
            var input = GetInputField(field);
            Assert.IsNotNull(input, "Expected to find the underlying UITK TextField.");
            Assert.IsTrue(input.isPasswordField);

            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            Assert.IsTrue(input.isPasswordField, "isPassword should survive attaching to a panel.");
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

            Assert.DoesNotThrow(() => field.isPassword = true);
            Assert.IsTrue(input.isPasswordField);

            Assert.DoesNotThrow(() => field.isPassword = false);
            Assert.IsFalse(input.isPasswordField);
        }
    }
}
