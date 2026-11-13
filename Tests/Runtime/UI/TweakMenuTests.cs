using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TweakMenu))]
    class TweakMenuTests : VisualElementTests<TweakMenu>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/TweakMenu.uss";

        protected override string mainUssClassName => TweakMenu.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new TweakMenu());
                yield return new Story("CloudDark", OpenCloudDark);
            }
        }

        /// <summary>
        /// The menu open on cloud-dark and compact, which is the state this story is named for.
        /// </summary>
        /// <remarks>
        /// Both values have to be set after the menu is in the panel. <c>OnAttachedToPanel</c>
        /// adopts whatever the host panel has, so anything assigned at construction is overwritten
        /// on attach and the capture comes out identical to <c>Default</c>. The menu is opened for
        /// the same reason: the closed trigger is already what <c>Default</c> shows.
        /// </remarks>
        static IEnumerator OpenCloudDark(StoryContext ctx)
        {
            var menu = new TweakMenu();
            ctx.container.Add(menu);
            yield return null;

            menu.theme = "cloud-dark";
            menu.density = "compact";
            yield return null;

            MenuBuilder.Build(menu.trigger, menu.menu).SetCloseOnSelection(false).Show();

            // Show() only starts the popover's 150ms fade from opacity 0.0001. Returning on the
            // next frame captures a menu that is attached but not yet on screen.
            yield return WaitUntil(() => PopoverOf(menu) != null, 10f);
            s_Popover = PopoverOf(menu);
            yield return WaitUntil(() => s_Popover.resolvedStyle.opacity >= 0.99f, 10f);
        }

        /// <summary>
        /// The popover the story opened.
        /// </summary>
        /// <remarks>
        /// Held rather than looked up again at assertion time. A panel can be left holding more
        /// than one popover root, and a second lookup picks whichever comes first in the tree --
        /// which is how this assertion came to read another popover's fade as this one's.
        /// </remarks>
        static VisualElement s_Popover;

        /// <summary>
        /// The popover root the open menu lives in, or <c>null</c> while nothing is open.
        /// </summary>
        static VisualElement PopoverOf(TweakMenu tweakMenu)
        {
            for (var element = tweakMenu.menu.hierarchy.parent; element != null; element = element.hierarchy.parent)
            {
                if (element.ClassListContains("appui-popover"))
                    return element;
            }

            return null;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:TweakMenu />",
            @"<appui:TweakMenu theme=""cloud-light"" density=""comfort"" />",
        };

        [Test]
        public void Themes_CoverEverySupportedTheme()
        {
            CollectionAssert.AreEquivalent(
                new[] { "dark", "light", "editor-dark", "editor-light", "cloud-dark", "cloud-light" },
                TweakMenu.themes,
                "TweakMenu must offer every App UI theme.");
        }

        [Test]
        public void Densities_CoverEveryDensityStep()
        {
            CollectionAssert.AreEquivalent(
                new[] { "normal", "compact", "comfort" },
                TweakMenu.densities,
                "TweakMenu must offer every density step.");
        }

        [Test]
        public void Defaults_MatchThePanelDefaults()
        {
            var menu = new TweakMenu();
            Assert.AreEqual("dark", menu.theme);
            Assert.AreEqual("normal", menu.density);
        }

        [UnityTest]
        public IEnumerator CloudDarkStory_EstablishesTheStateItIsNamedFor()
        {
            yield return RunStory("CloudDark", ctx =>
            {
                var menu = ctx.container.Q<TweakMenu>();
                Assert.IsNotNull(menu);
                Assert.AreEqual("cloud-dark", menu.theme,
                    "OnAttachedToPanel adopts the host panel's theme, so a value set before mounting "
                    + "is overwritten and the capture silently duplicates Default.");
                Assert.AreEqual("compact", menu.density);
                Assert.AreEqual("cloud-dark", ctx.panel.theme,
                    "The menu pushes its theme back to the host panel.");
                Assert.IsNotNull(menu.menu.panel,
                    "The story must leave the menu open; a closed trigger is what Default shows.");

                var popover = s_Popover;
                Assert.IsNotNull(popover, "The menu must be in an open popover.");
                // The ease-out asymptotes just short of 1, so this checks the fade has finished
                // rather than that it landed exactly on the end value.
                Assert.Greater(popover.resolvedStyle.opacity, 0.95f,
                    "The popover fades in over 150ms, so an attached menu is not yet a visible one.");
            });
        }

        [UnityTest]
        public IEnumerator Theme_AppliesToTheHostPanel()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new TweakMenu();
            panel.Add(menu);

            for (var i = 0; i < 4; i++)
                yield return null;

            menu.theme = "cloud-light";
            yield return null;

            Assert.AreEqual("cloud-light", panel.theme,
                "Changing the TweakMenu theme must reach the host Panel's theme context.");
        }

        [UnityTest]
        public IEnumerator Density_AppliesToTheHostPanel()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new TweakMenu();
            panel.Add(menu);

            for (var i = 0; i < 4; i++)
                yield return null;

            menu.density = "compact";
            yield return null;

            Assert.AreEqual("compact", panel.density,
                "Changing the TweakMenu density must reach the host Panel's density context.");
        }

        [UnityTest]
        public IEnumerator AttachingToAPanel_AdoptsThePanelState()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "editor-light", density = "comfort" };
            m_TestUI.rootVisualElement.Add(panel);
            var menu = new TweakMenu();
            panel.Add(menu);

            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual("editor-light", menu.theme,
                "The TweakMenu must show the Panel's real theme, not its own default.");
            Assert.AreEqual("comfort", menu.density);
        }

        [Test]
        public void ThemeChanged_Notifies()
        {
            var menu = new TweakMenu();
            string observed = null;
            menu.themeChanged += t => observed = t;
            menu.theme = "editor-dark";
            Assert.AreEqual("editor-dark", observed);
        }

        [Test]
        public void DensityChanged_Notifies()
        {
            var menu = new TweakMenu();
            string observed = null;
            menu.densityChanged += d => observed = d;
            menu.density = "compact";
            Assert.AreEqual("compact", observed);
        }

        [Test]
        public void Menu_OffersOneRadioRowPerThemeAndDensity()
        {
            var menu = new TweakMenu();
            var keys = menu.themeRadioGroup.Query<MenuItem>().ToList().Select(i => i.key).ToArray();
            CollectionAssert.AreEquivalent(TweakMenu.themes, keys);

            var densityKeys = menu.densityRadioGroup.Query<MenuItem>().ToList().Select(i => i.key).ToArray();
            CollectionAssert.AreEquivalent(TweakMenu.densities, densityKeys);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensOnly()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "TweakMenu.uss uses a literal hex colour instead of an --appui-* token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "TweakMenu.uss uses a literal rgb()/rgba() colour instead of an --appui-* token.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*\d+(\.\d+)?px"),
                "TweakMenu.uss uses a magic pixel value instead of an --appui-* token.");
        }
    }
}
