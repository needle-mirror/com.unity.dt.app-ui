using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;
using Button = Unity.AppUI.UI.Button;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Button))]
    class ButtonTests : VisualElementTests<Button>
    {
        protected override string mainUssClassName => Button.ussClassName;

        // Handed over to ButtonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                foreach (ButtonVariant variant in Enum.GetValues(typeof(ButtonVariant)))
                {
                    var v = variant;
                    yield return new Story(v.ToString(), (ctx) => new Button
                    {
                        variant = v,
                        title = v.ToString(),
                        leadingIcon = "info",
                    });
                }

                // Disabled coverage from the UUM-108227 audit. Ghost is the variant that
                // replaced the former `quiet` flag, and it is the one whose disabled state
                // has the least contrast to fall back on, so it is the one worth pinning.
                yield return new Story("GhostDisabled", (ctx) =>
                {
                    var button = new Button { title = "Button", variant = ButtonVariant.Ghost };
                    button.SetEnabled(false);
                    return button;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Button />",
            @"<appui:Button title=""Button"" />",
            @"<appui:Button title=""Button"" leading-icon=""info"" />",
            @"<appui:Button title=""Button"" trailing-icon=""info"" />",
            @"<appui:Button title=""Button"" leading-icon=""info"" trailing-icon=""info"" />",
            @"<appui:Button title=""Button"" leading-icon=""info"" trailing-icon=""info"" subtitle=""Subtitle"" />",
            @"<appui:Button title=""Button"" size=""Md"" variant=""Default"" />",
            @"<appui:Button title=""Button"" variant=""Destructive"" />",
            @"<appui:Button title=""Button"" variant=""Outline"" />",
            @"<appui:Button title=""Button"" variant=""Secondary"" />",
            @"<appui:Button title=""Button"" variant=""Ghost"" />",
            @"<appui:Button title=""Button"" variant=""Link"" />",
            @"<appui:Button title=""Button"" variant=""Ghost"" enabled=""false"" />",
        };

        static readonly ButtonVariant[] k_AllVariants =
        {
            ButtonVariant.Default,
            ButtonVariant.Destructive,
            ButtonVariant.Outline,
            ButtonVariant.Secondary,
            ButtonVariant.Ghost,
            ButtonVariant.Link,
        };

        static readonly Size[] k_AllSizes = { Size.Sm, Size.Md, Size.Lg };

        [Test]
        public void VariantEnum_ShouldMatchTheRegistryVocabulary()
        {
            // The shadcn registry `buttonVariants` cva declares exactly these six variants.
            CollectionAssert.AreEquivalent(
                k_AllVariants,
                Enum.GetValues(typeof(ButtonVariant)).Cast<ButtonVariant>().ToArray());
        }

        [Test]
        public void Variant_ShouldDefaultToDefault()
        {
            var button = new Button();
            Assert.AreEqual(ButtonVariant.Default, button.variant);
            Assert.IsTrue(button.ClassListContains(Button.GetVariantUssClassName(ButtonVariant.Default)),
                "A freshly constructed Button must carry the Default variant USS class.");
        }

        [Test]
        public void Variant_ShouldProduceALowercaseUssClassName([ValueSource(nameof(k_AllVariants))] ButtonVariant variant)
        {
            var expected = Button.ussClassName + "--" + variant.ToString().ToLowerInvariant();
            Assert.AreEqual(expected, Button.GetVariantUssClassName(variant));
        }

        [Test]
        public void Variant_ShouldApplyItsUssClass([ValueSource(nameof(k_AllVariants))] ButtonVariant variant)
        {
            var button = new Button { variant = variant };
            Assert.AreEqual(variant, button.variant);
            Assert.IsTrue(button.ClassListContains(Button.GetVariantUssClassName(variant)));
        }

        [Test]
        public void Variant_ShouldRemoveThePreviousVariantClass()
        {
            var button = new Button();
            foreach (var variant in k_AllVariants)
            {
                button.variant = variant;
                foreach (var other in k_AllVariants)
                {
                    var cls = Button.GetVariantUssClassName(other);
                    if (other == variant)
                        Assert.IsTrue(button.ClassListContains(cls), $"Expected {cls} to be set.");
                    else
                        Assert.IsFalse(button.ClassListContains(cls), $"Stale variant class {cls} was not removed.");
                }
            }
        }

        [Test]
        public void Size_ShouldDefaultToMd()
        {
            var button = new Button();
            Assert.AreEqual(Size.Md, button.size);
            Assert.IsTrue(button.ClassListContains(Button.GetSizeUssClassName(Size.Md)));
        }

        [Test]
        public void Size_ShouldApplyItsUssClassAndRemoveTheStaleOne([ValueSource(nameof(k_AllSizes))] Size size)
        {
            var button = new Button { size = size };
            Assert.AreEqual(size, button.size);
            Assert.IsTrue(button.ClassListContains(Button.GetSizeUssClassName(size)));
            foreach (var other in k_AllSizes)
            {
                if (other == size)
                    continue;
                Assert.IsFalse(button.ClassListContains(Button.GetSizeUssClassName(other)),
                    $"Stale size class for {other} was not removed.");
            }
        }

        [Test]
        public void IconOnly_ShouldToggleWithTheTitle()
        {
            // The registry models an icon-only button as its own `icon` size; UITK models it
            // as a shape modifier driven by the absence of a title.
            var button = new Button { leadingIcon = "info" };
            Assert.IsTrue(button.ClassListContains(Button.iconOnlyUssClassName));
            button.title = "Hello";
            Assert.IsFalse(button.ClassListContains(Button.iconOnlyUssClassName));
            button.title = null;
            Assert.IsTrue(button.ClassListContains(Button.iconOnlyUssClassName));
        }

        [Test]
        public void Variant_ShouldNotBeAQuietBooleanAnymore()
        {
            // `quiet` folded into ButtonVariant.Ghost — no bool property, no `--quiet` class.
            Assert.IsNull(typeof(Button).GetProperty("quiet"),
                "Button.quiet must be gone; the quiet treatment is now ButtonVariant.Ghost.");
            var button = new Button { variant = ButtonVariant.Ghost };
            Assert.IsFalse(button.ClassListContains(Button.ussClassName + "--quiet"));
            Assert.IsTrue(button.ClassListContains(Button.GetVariantUssClassName(ButtonVariant.Ghost)));
        }

        // ---------------------------------------------------------------- insets

        /// <summary>
        /// The registry's <c>button</c> reads <c>--appui-alias-actions-padding-horizontal-{sm,md,lg}</c>
        /// by size (#1418).
        /// </summary>
        [Test]
        [UnityEngine.TestTools.ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_ComesFromTheActionsPaddingAlias_ForEverySize()
        {
            foreach (var suffix in new[] { "sm", "md", "lg" })
                UssSource.AssertHorizontalInset("Button.uss", ".appui-button--size-" + suffix,
                    "--appui-alias-actions-padding-horizontal-" + suffix);
        }

        static System.Collections.IEnumerator Mounted(UnityEngine.UIElements.UIDocument ui, Button button)
        {
            ui.rootVisualElement.Clear();
            var panel = new Panel();
            ui.rootVisualElement.Add(panel);
            panel.Add(button);
            yield return null;
            yield return null;
        }

        /// <summary>
        /// The inset is the root's padding alone. The containers used to pad the outer edges with
        /// their own margins, which stacked on top of the padding; now each gap sits only between
        /// two visible parts.
        /// </summary>
        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator OuterEdges_AreThePaddingAlone([ValueSource(nameof(k_AllSizes))] Size size)
        {
            foreach (var (lead, trail) in new[] { ((string)null, (string)null), ("info", null), (null, "info"), ("info", "info") })
            {
                var button = new Button { title = "Button", size = size, leadingIcon = lead, trailingIcon = trail };
                yield return Mounted(m_TestUI, button);

                var border = button.resolvedStyle.borderLeftWidth;
                var padding = button.resolvedStyle.paddingLeft;
                Assert.Greater(padding, 0f, $"A {size} button with a title must have a horizontal inset.");

                var first = lead != null
                    ? button.Q(className: Button.leadingContainerUssClassName)
                    : button.Q(className: Button.titleContainerUssClassName);
                var last = trail != null
                    ? button.Q(className: Button.trailingContainerUssClassName)
                    : button.Q(className: Button.titleContainerUssClassName);

                Assert.AreEqual(border + padding, first.layout.xMin, 0.01f,
                    $"{size} ({lead}/{trail}): the first part must sit right on the leading padding.");
                Assert.AreEqual(button.layout.width - button.resolvedStyle.borderRightWidth - button.resolvedStyle.paddingRight,
                    last.layout.xMax, 0.01f,
                    $"{size} ({lead}/{trail}): the last part must end right on the trailing padding.");
            }
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator IconOnly_StaysSquare([ValueSource(nameof(k_AllSizes))] Size size)
        {
            var button = new Button { leadingIcon = "info", size = size };
            yield return Mounted(m_TestUI, button);

            Assert.AreEqual(0f, button.resolvedStyle.paddingLeft, "An icon-only button has no inset.");
            Assert.AreEqual(button.layout.height, button.layout.width, 0.01f, "An icon-only button is square.");
        }
    }
}
