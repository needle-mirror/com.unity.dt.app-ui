using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(HelpBox))]
    class HelpBoxTests : VisualElementTests<HelpBox>
    {
        protected override string mainUssClassName => HelpBox.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:HelpBox />",
            @"<appui:HelpBox message=""Hello"" variant=""Information"" icon-name=""info"" />",
            @"<appui:HelpBox message=""Warning"" variant=""Warning"" icon-name=""warning"" size=""M"" />",
            @"<appui:HelpBox message=""Error"" variant=""Error"" icon-name=""x-circle"" />",
            @"<appui:HelpBox message=""No icon"" variant=""Default"" icon-name="""" />",
        };

        [Test]
        public void VariantProperty_ShouldUpdateClassList()
        {
            var helpBox = new HelpBox();

            helpBox.variant = AlertSemantic.Warning;
            Assert.AreEqual(AlertSemantic.Warning, helpBox.variant);
            Assert.IsTrue(helpBox.ClassListContains(HelpBox.variantUssClassName + "warning"));

            helpBox.variant = AlertSemantic.Error;
            Assert.AreEqual(AlertSemantic.Error, helpBox.variant);
            Assert.IsTrue(helpBox.ClassListContains(HelpBox.variantUssClassName + "error"));
            Assert.IsFalse(helpBox.ClassListContains(HelpBox.variantUssClassName + "warning"));
        }

        [Test]
        public void IconNameProperty_ShouldUpdate()
        {
            var helpBox = new HelpBox();

            helpBox.iconName = "warning";
            Assert.AreEqual("warning", helpBox.iconName);

            helpBox.iconName = null;
            Assert.IsNull(helpBox.iconName);
        }

        [Test]
        public void MessageProperty_ShouldUpdate()
        {
            var helpBox = new HelpBox();

            helpBox.message = "Test message";
            Assert.AreEqual("Test message", helpBox.message);

            helpBox.message = null;
            Assert.IsNull(helpBox.message);
        }

        [Test]
        public void SizeProperty_ShouldUpdateClassList()
        {
            var helpBox = new HelpBox();

            helpBox.size = Size.M;
            Assert.AreEqual(Size.M, helpBox.size);
            Assert.IsTrue(helpBox.ClassListContains(HelpBox.sizeUssClassName + "m"));

            helpBox.size = Size.L;
            Assert.AreEqual(Size.L, helpBox.size);
            Assert.IsTrue(helpBox.ClassListContains(HelpBox.sizeUssClassName + "l"));
            Assert.IsFalse(helpBox.ClassListContains(HelpBox.sizeUssClassName + "m"));
        }

        [Test]
        public void Constructor_WithParameters_ShouldSetProperties()
        {
            var helpBox = new HelpBox("My message", AlertSemantic.Destructive, "x-circle");

            Assert.AreEqual("My message", helpBox.message);
            Assert.AreEqual(AlertSemantic.Destructive, helpBox.variant);
            Assert.AreEqual("x-circle", helpBox.iconName);
            Assert.AreEqual(Size.S, helpBox.size);
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => new HelpBox());
                yield return new Story("Information", _ => new HelpBox("Informational message", AlertSemantic.Information, "info"));
                yield return new Story("Warning", _ => new HelpBox("Warning message", AlertSemantic.Warning, "warning"));
                yield return new Story("Error", _ => new HelpBox("Error message", AlertSemantic.Error, "x-circle"));
                yield return new Story("Destructive", _ => new HelpBox("Destructive message", AlertSemantic.Destructive, "x-circle"));
                yield return new Story("Confirmation", _ => new HelpBox("Confirmation message", AlertSemantic.Confirmation, "check-circle"));
                yield return new Story("NoIcon", _ => new HelpBox("No icon", AlertSemantic.Default, null));
            }
        }
    }
}
