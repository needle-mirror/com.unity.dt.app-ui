using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ActionButton))]
    class ActionButtonTests : VisualElementTests<ActionButton>
    {
        protected override string mainUssClassName => ActionButton.ussClassName;

        // Handed over to ActionButtonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new ActionButton());
                yield return new Story("WithLabel", ctx => new ActionButton { label = "Test" });
                yield return new Story("WithIconAndLabel", ctx => new ActionButton { label = "Test", icon = "info", iconVariant = IconVariant.Regular });
                yield return new Story("IconOnly", ctx => new ActionButton { icon = "info" });
                yield return new Story("WithTrailingIcon", ctx => new ActionButton { label = "My Label", trailingIcon = "info", trailingIconVariant = IconVariant.Regular });
            }
        }

        protected override IEnumerable<string> uxmlTestCases
        {
            get
            {
                yield return @"<appui:ActionButton/>";
                yield return @"<appui:ActionButton label=""Test""/>";
                yield return @"<appui:ActionButton icon=""info""/>";
                yield return @"<appui:ActionButton label=""My Label"" trailing-icon=""info"" trailing-icon-variant=""Regular"" />";
                yield return @"<appui:ActionButton label=""Test"" icon=""info"" icon-variant=""Regular"" size=""Md"" accent=""true"" selected=""false"" quiet=""true""/>";
            }
        }

        static readonly Size[] k_AllSizes = { Size.Sm, Size.Md, Size.Lg };

        /// <summary>
        /// The registry's <c>action-button</c> reads the tighter
        /// <c>--appui-alias-actions-compact-padding-horizontal-{sm,md,lg}</c> by size (#1418).
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_ComesFromTheCompactActionsPaddingAlias_ForEverySize()
        {
            foreach (var suffix in new[] { "sm", "md", "lg" })
                UssSource.AssertHorizontalInset("ActionButton.uss", ".appui-actionbutton--size-" + suffix,
                    "--appui-alias-actions-compact-padding-horizontal-" + suffix);

            var root = UssSource.RuleBlock(UssSource.Read("ActionButton.uss"), ".appui-actionbutton");
            StringAssert.DoesNotContain("static-sizing-100", root,
                "The base rule must not keep the old static inset.");
        }

        IEnumerator Mount(VisualElement element)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, alignItems = Align.FlexStart } };
            panel.Add(row);
            row.Add(element);
            yield return null;
            yield return null;
        }

        /// <summary>
        /// The size rules now set a horizontal padding, so the icon-only reset has to outrank them;
        /// otherwise an icon-only button stops being square.
        /// </summary>
        [UnityTest]
        public IEnumerator IconOnly_HasNoInsetAndStaysSquare([ValueSource(nameof(k_AllSizes))] Size size)
        {
            var button = new ActionButton { icon = "info", size = size };
            yield return Mount(button);

            Assert.AreEqual(0f, button.resolvedStyle.paddingLeft, $"A {size} icon-only ActionButton has no inset.");
            Assert.AreEqual(0f, button.resolvedStyle.paddingRight, $"A {size} icon-only ActionButton has no inset.");
            Assert.AreEqual(button.layout.height, button.layout.width, 0.01f, $"A {size} icon-only ActionButton is square.");
        }

        [UnityTest]
        public IEnumerator Labelled_HasAnInset([ValueSource(nameof(k_AllSizes))] Size size)
        {
            var button = new ActionButton { label = "Action", size = size };
            yield return Mount(button);

            Assert.Greater(button.resolvedStyle.paddingLeft, 0f, $"A {size} labelled ActionButton must have an inset.");
            Assert.AreEqual(button.resolvedStyle.paddingLeft, button.resolvedStyle.paddingRight, 0.01f);
        }
    }
}
