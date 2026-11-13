using NUnit.Framework;
using Unity.AppUI.Navigation;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.Navigation
{
    /// <summary>
    /// The template name is an assembly-qualified type name read out of a serialized navigation
    /// graph, so it selects which type gets constructed. These tests lock in that the type is
    /// checked before <c>Activator.CreateInstance</c> runs its constructor, not after.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(DefaultDestinationTemplate))]
    class DefaultDestinationTemplateTests
    {
        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("Not.A.Real.Type, Nope")]
        [TestCase("System.Text.StringBuilder")]
        [TestCase("UnityEngine.UIElements.VisualElement, UnityEngine.UIElementsModule")]
        public void CreateScreen_WithNonScreenTemplate_FallsBackToDefault(string template)
        {
            LogAssert.ignoreFailingMessages = true;
            try
            {
                var destinationTemplate = new DefaultDestinationTemplate { template = template };
                var screen = destinationTemplate.CreateScreen(null);

                Assert.IsNotNull(screen);
                Assert.AreEqual(typeof(NavigationScreen), screen.GetType());
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }

        [Test]
        public void CreateScreen_WithValidScreenTemplate_UsesIt()
        {
            var destinationTemplate = new DefaultDestinationTemplate
            {
                template = typeof(NavigationScreen).AssemblyQualifiedName
            };

            var screen = destinationTemplate.CreateScreen(null);

            Assert.IsNotNull(screen);
            Assert.AreEqual(typeof(NavigationScreen), screen.GetType());
        }
    }
}
