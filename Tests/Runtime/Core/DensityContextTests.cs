using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.Core
{
    [TestFixture]
    [TestOf(typeof(DensityContext))]
    class DensityContextTests
    {
        [Test]
        public void DensityContext_ShouldExposeItsValue()
        {
            var ctx = new DensityContext("compact");
            Assert.AreEqual("compact", ctx.density);
        }

        [Test]
        public void DensityContext_ShouldCompareByValue()
        {
            Assert.AreEqual(new DensityContext("normal"), new DensityContext("normal"));
            Assert.AreNotEqual(new DensityContext("normal"), new DensityContext("comfort"));
        }

        [Test]
        public void Panel_ShouldDefaultToNormalDensity()
        {
            var panel = new Panel();
            Assert.AreEqual("normal", Panel.defaultDensity);
            Assert.AreEqual(Panel.defaultDensity, panel.density);
            Assert.AreEqual(Panel.defaultDensity, panel.GetContext<DensityContext>().density);
        }

        [TestCase("normal")]
        [TestCase("compact")]
        [TestCase("comfort")]
        public void Panel_ShouldWriteTheDensityUssClass(string density)
        {
            var panel = new Panel { density = density };
            Assert.AreEqual(density, panel.density);
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + density),
                $"Panel should carry the '{Panel.contextPrefix + density}' class.");
        }

        [Test]
        public void Panel_ShouldSwapTheDensityUssClassOnChange()
        {
            var panel = new Panel { density = "compact" };
            panel.density = "comfort";
            Assert.IsFalse(panel.ClassListContains(Panel.contextPrefix + "compact"),
                "The previous density class should be removed.");
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + "comfort"));
        }

        [Test]
        public void Panel_ShouldRejectAnEmptyDensity()
        {
            var panel = new Panel();
            Assert.Throws<System.ArgumentException>(() => panel.density = null);
            Assert.Throws<System.ArgumentException>(() => panel.density = string.Empty);
        }

        [Test]
        public void Element_ShouldInheritTheDensityFromItsPanel()
        {
            var panel = new Panel { density = "compact" };
            var text = new Text("hello");
            panel.Add(text);
            Assert.AreEqual("compact", text.GetContext<DensityContext>().density);
        }

        [Test]
        public void Element_DensityOverride_ShouldWinOverThePanel()
        {
            var panel = new Panel { density = "compact" };
            var text = new Text("hello");
            panel.Add(text);
            text.densityOverride = "comfort";

            Assert.AreEqual("comfort", text.GetContext<DensityContext>().density);
            Assert.IsTrue(text.ClassListContains(Panel.contextPrefix + "comfort"));
            Assert.AreEqual("compact", panel.GetContext<DensityContext>().density,
                "Overriding a child must not mutate the panel's density.");
        }

        [Test]
        public void Element_ClearingTheDensityOverride_ShouldFallBackToThePanel()
        {
            var panel = new Panel { density = "compact" };
            var text = new Text("hello");
            panel.Add(text);
            text.densityOverride = "comfort";
            text.densityOverride = Optional<string>.none;

            Assert.IsFalse(text.ClassListContains(Panel.contextPrefix + "comfort"));
            Assert.AreEqual("compact", text.GetContext<DensityContext>().density);
        }
    }
}
