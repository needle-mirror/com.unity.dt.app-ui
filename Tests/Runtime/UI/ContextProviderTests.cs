using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ContextProvider))]
    class ContextProviderTests : VisualElementTests<ContextProvider>
    {
        protected override string mainUssClassName => ContextProvider.ussClassName;

        // Handed over to ContextProviderSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield break;
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ContextProvider />",
            @"<appui:ContextProvider theme=""dark"" density=""normal"" dir=""Ltr"" lang=""en-US"" tooltip-delay-ms=""500"" enabled=""true"" preferred-tooltip-placement=""Bottom"" />",
        };

        [Test]
        [TestCase("dark", true)]
        [TestCase("editor-dark", true)]
        [TestCase("cloud-dark", true)]
        [TestCase("light", false)]
        [TestCase("editor-light", false)]
        [TestCase("editor-7-dark", true)]
        [TestCase("editor-7-light", false)]
        public void ContextProvider_WithThemeOverride_ShouldHaveContextMarker(string theme, bool isDark)
        {
            var provider = new ContextProvider { themeOverride = theme };

            var expected = isDark ? Panel.darkContextUssClassName : Panel.lightContextUssClassName;
            var unexpected = isDark ? Panel.lightContextUssClassName : Panel.darkContextUssClassName;

            Assert.IsTrue(provider.ClassListContains(expected),
                $"Expected the '{expected}' marker for theme '{theme}'.");
            Assert.IsFalse(provider.ClassListContains(unexpected),
                "The dark and light markers must never both be present.");
        }

        [Test]
        public void ContextProvider_WhenThemeOverrideCleared_ShouldDropBothMarkers()
        {
            var provider = new ContextProvider { themeOverride = "dark" };
            Assert.IsTrue(provider.ClassListContains(Panel.darkContextUssClassName));

            provider.themeOverride = Optional<string>.none;

            // With no override the subtree inherits its ancestor's artwork, so the provider itself
            // must carry neither marker.
            Assert.IsFalse(provider.ClassListContains(Panel.darkContextUssClassName));
            Assert.IsFalse(provider.ClassListContains(Panel.lightContextUssClassName));
        }

        [Test]
        public void ContextProvider_NestedInOppositeTheme_ShouldCarryItsOwnMarker()
        {
            // The shape ContextProviderDemos.Themes renders: a light island inside a dark panel.
            // Both markers exist, on different elements, so icon vars resolve by inheritance
            // distance rather than by USS declaration order.
            var panel = new Panel { theme = "dark" };
            var provider = new ContextProvider { themeOverride = "light" };
            panel.Add(provider);

            Assert.IsTrue(panel.ClassListContains(Panel.darkContextUssClassName));
            Assert.IsFalse(panel.ClassListContains(Panel.lightContextUssClassName));

            Assert.IsTrue(provider.ClassListContains(Panel.lightContextUssClassName));
            Assert.IsFalse(provider.ClassListContains(Panel.darkContextUssClassName));
        }
    }
}
