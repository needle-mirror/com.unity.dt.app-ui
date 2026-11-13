using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Panel))]
    class PanelTests : VisualElementTests<Panel>
    {
        protected override string mainUssClassName => Panel.ussClassName;

        // Handed over to PanelSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield break;
            }
        }

        [Test]
        public void Panel_ShouldHaveDefaultContext()
        {
            var panel = new Panel();
            Assert.AreEqual(Panel.defaultLang, panel.lang);
            Assert.AreEqual(Panel.defaultTheme, panel.theme);
            Assert.AreEqual(Panel.defaultDensity, panel.density);
            Assert.AreEqual(Panel.defaultDir, panel.layoutDirection);

            Assert.AreEqual(Panel.defaultLang, panel.GetContext<LangContext>().lang);
            Assert.AreEqual(Panel.defaultTheme, panel.GetContext<ThemeContext>().theme);
            Assert.AreEqual(Panel.defaultDensity, panel.GetContext<DensityContext>().density);
            Assert.AreEqual(Panel.defaultDir, panel.GetContext<DirContext>().dir);

            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + Panel.defaultLang));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + Panel.defaultTheme));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + Panel.defaultDensity));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + Panel.defaultDir.ToString().ToLower()));
        }

        [Test]
        [TestCase("fr", "dark", "compact", Dir.Rtl)]
        [TestCase("de", "light", "normal", Dir.Ltr)]
        [TestCase("en", "light", "comfort", Dir.Rtl)]
        [TestCase("en", "dark", "compact", Dir.Ltr)]
        public void Panel_WithInitializers_ShouldHaveNewContext(string lang, string theme, string density, Dir dir)
        {
            var panel = new Panel()
            {
                lang = lang,
                theme = theme,
                density = density,
                layoutDirection = dir
            };
            Assert.AreEqual(lang, panel.lang);
            Assert.AreEqual(theme, panel.theme);
            Assert.AreEqual(density, panel.density);
            Assert.AreEqual(dir, panel.layoutDirection);

            Assert.AreEqual(lang, panel.GetContext<LangContext>().lang);
            Assert.AreEqual(theme, panel.GetContext<ThemeContext>().theme);
            Assert.AreEqual(density, panel.GetContext<DensityContext>().density);
            Assert.AreEqual(dir, panel.GetContext<DirContext>().dir);

            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + lang));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + theme));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + density));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + dir.ToString().ToLower()));
        }

        [Test]
        [TestCase("dark")]
        [TestCase("editor-dark")]
        [TestCase("cloud-dark")]
        [TestCase("acme-dark")]
        public void Panel_WithDarkTheme_ShouldHaveDarkContextClass(string theme)
        {
            var panel = new Panel { theme = theme };

            Assert.IsTrue(panel.ClassListContains(Panel.darkContextUssClassName),
                $"Expected the '{Panel.darkContextUssClassName}' marker for theme '{theme}'.");
            Assert.IsFalse(panel.ClassListContains(Panel.lightContextUssClassName),
                "The dark and light markers must never both be present.");
        }

        [Test]
        [TestCase("light")]
        [TestCase("editor-light")]
        [TestCase("cloud-light")]
        [TestCase("acme-light")]
        public void Panel_WithLightTheme_ShouldHaveLightContextClass(string theme)
        {
            var panel = new Panel { theme = theme };

            Assert.IsTrue(panel.ClassListContains(Panel.lightContextUssClassName),
                $"Expected the '{Panel.lightContextUssClassName}' marker for theme '{theme}'.");
            Assert.IsFalse(panel.ClassListContains(Panel.darkContextUssClassName),
                "The dark and light markers must never both be present.");
        }

        [Test]
        public void Panel_WhenThemeChanges_ShouldSwapContextMarkerExclusively()
        {
            var panel = new Panel { theme = "dark" };
            Assert.IsTrue(panel.ClassListContains(Panel.darkContextUssClassName));

            panel.theme = "editor-light";

            // The stale marker must go: an element carrying both would make the icon artwork
            // depend on USS declaration order.
            Assert.IsFalse(panel.ClassListContains(Panel.darkContextUssClassName));
            Assert.IsTrue(panel.ClassListContains(Panel.lightContextUssClassName));
            Assert.IsFalse(panel.ClassListContains(Panel.contextPrefix + "dark"));
            Assert.IsTrue(panel.ClassListContains(Panel.contextPrefix + "editor-light"));

            panel.theme = "cloud-dark";

            Assert.IsTrue(panel.ClassListContains(Panel.darkContextUssClassName));
            Assert.IsFalse(panel.ClassListContains(Panel.lightContextUssClassName));
        }
    }
}
