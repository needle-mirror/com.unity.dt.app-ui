using System;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// App UI v4 renamed some public API (Size.S to Size.Sm, Button.quiet to ButtonVariant.Ghost,
    /// Panel.scale to Panel.density). The old names stay as [Obsolete] shims so code written against
    /// v3, such as com.unity.behavior, still compiles. These tests pin that the shims keep their
    /// meaning and that enum aliases never leak into a USS class name or an EnumField menu.
    /// </summary>
    [TestFixture]
    class ObsoleteApiTests
    {
#pragma warning disable CS0618 // the aliases under test are obsolete on purpose
        [Test]
        public void Aliases_ShouldEqualTheirCurrentMembers()
        {
            Assert.AreEqual(Size.Sm, Size.S);
            Assert.AreEqual(Size.Md, Size.M);
            Assert.AreEqual(Size.Lg, Size.L);
            Assert.AreEqual(IconSize.Xxs, IconSize.XXS);
            Assert.AreEqual(IconSize.Xs, IconSize.XS);
            Assert.AreEqual(IconSize.Sm, IconSize.S);
            Assert.AreEqual(IconSize.Md, IconSize.M);
            Assert.AreEqual(IconSize.Lg, IconSize.L);
            Assert.AreEqual(TextSize.XS, TextSize.XXS);
            Assert.AreEqual(ButtonVariant.Default, ButtonVariant.Accent);
        }

        [Test]
        public void GeneratedUssClassNames_ShouldUseTheCurrentNames()
        {
            Assert.AreEqual(Button.sizeUssClassName + "sm", Button.GetSizeUssClassName(Size.S));
            Assert.AreEqual(Icon.sizeUssClassName + "md", Icon.GetSizeUssClassName(IconSize.M));
            Assert.AreEqual(Text.sizeUssClassName + "xs", Text.GetSizeUssClassName(TextSize.XXS));
            Assert.AreEqual(Button.ussClassName + "--default", Button.GetVariantUssClassName(ButtonVariant.Accent));
        }

        [Test]
        public void DateField_SizeClass_ShouldUseTheCurrentName()
        {
            var field = new DateField { size = Size.L };
            Assert.IsTrue(field.ClassListContains(DateField.sizeUssClassName + "lg"));
        }
        [Test]
        public void ButtonQuiet_ShouldMapToTheGhostVariant()
        {
            var button = new Button { quiet = true };
            Assert.AreEqual(ButtonVariant.Ghost, button.variant);
            Assert.IsTrue(button.quiet);

            button.quiet = false;
            Assert.AreEqual(ButtonVariant.Default, button.variant);

            var destructive = new Button { variant = ButtonVariant.Destructive, quiet = false };
            Assert.AreEqual(ButtonVariant.Destructive, destructive.variant);
        }

        [TestCase("small", "compact")]
        [TestCase("medium", "normal")]
        [TestCase("large", "comfort")]
        [TestCase("custom", "custom")]
        public void PanelScale_ShouldMapToDensity(string scale, string density)
        {
            var panel = new Panel { scale = scale };
            Assert.AreEqual(density, panel.density);
            Assert.AreEqual(scale, panel.scale);
        }
#pragma warning restore CS0618

        [Test]
        public void EnumField_ShouldNotListObsoleteAliases()
        {
            var field = new EnumField(Size.Md);
            CollectionAssert.AreEqual(
                new[] { Size.Sm, Size.Md, Size.Lg },
                field.sourceItems.Cast<Size>().ToArray());
        }

#pragma warning disable CS0618
        enum AllObsoleteEnum
        {
            [Obsolete("Test fixture")] First,
            [Obsolete("Test fixture")] Second,
        }

        enum PartlyObsoleteEnum
        {
            Current,
            [Obsolete("Test fixture")] Alias = Current,
            [Obsolete("Test fixture")] Legacy,
        }

        [Test]
        public void EnumField_EnumWithOnlyObsoleteMembers_ShouldStillListThem()
        {
            var field = new EnumField();
            Assert.DoesNotThrow(() => field.enumType = typeof(AllObsoleteEnum));
            CollectionAssert.AreEqual(
                new[] { AllObsoleteEnum.First, AllObsoleteEnum.Second },
                field.sourceItems.Cast<AllObsoleteEnum>().ToArray());
            Assert.AreEqual(AllObsoleteEnum.First, field.value);
        }

        [Test]
        public void EnumField_ObsoleteMemberWithItsOwnValue_ShouldStayListed()
        {
            var field = new EnumField(PartlyObsoleteEnum.Legacy);
            CollectionAssert.AreEqual(
                new[] { PartlyObsoleteEnum.Current, PartlyObsoleteEnum.Legacy },
                field.sourceItems.Cast<PartlyObsoleteEnum>().ToArray());
            Assert.AreEqual(PartlyObsoleteEnum.Legacy, field.value);
        }
#pragma warning restore CS0618
    }
}
