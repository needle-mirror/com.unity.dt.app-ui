using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Kbd))]
    class KbdTests : VisualElementTests<Kbd>
    {
        protected override string mainUssClassName => Kbd.ussClassName;

        // Handed over to KbdSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Kbd { text = "K" });
                yield return new Story("Combo", (ctx) => new Kbd { keys = new List<string> { "Ctrl", "K" } });
                yield return new Story("Row", (ctx) =>
                {
                    var row = new VisualElement { style = { flexDirection = FlexDirection.Row } };
                    row.Add(new Kbd { text = "Esc" });
                    row.Add(new Kbd { keys = new List<string> { "Shift", "Tab" } });
                    return row;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Kbd />",
            @"<appui:Kbd text=""K"" />",
            @"<appui:Kbd keys=""Ctrl,K"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Kbd.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        static List<VisualElement> Caps(Kbd kbd) =>
            kbd.Children().Where(c => c.ClassListContains(Kbd.capUssClassName)).ToList();

        /// <summary>
        /// The registry always renders at least one cap: the `keys` combo form, or the single-child
        /// short form. An empty Kbd must still be a cap, not an empty box.
        /// </summary>
        [Test]
        public void Default_RendersASingleEmptyCap()
        {
            var kbd = new Kbd();
            Assert.AreEqual(1, Caps(kbd).Count);
            Assert.AreEqual(string.Empty, kbd.text);
        }

        [Test]
        public void Text_RendersASingleCapCarryingIt()
        {
            var kbd = new Kbd { text = "Esc" };
            var caps = Caps(kbd);
            Assert.AreEqual(1, caps.Count);
            Assert.AreEqual("Esc", ((TextElement)caps[0]).text);
        }

        [Test]
        public void Keys_RenderOneCapEachInOrder()
        {
            var kbd = new Kbd { keys = new List<string> { "Ctrl", "Shift", "K" } };
            var caps = Caps(kbd);
            CollectionAssert.AreEqual(
                new[] { "Ctrl", "Shift", "K" },
                caps.Select(c => ((TextElement)c).text).ToArray());
        }

        [Test]
        public void Keys_TakePrecedenceOverText()
        {
            var kbd = new Kbd { text = "Esc", keys = new List<string> { "Ctrl", "K" } };
            var caps = Caps(kbd);
            CollectionAssert.AreEqual(new[] { "Ctrl", "K" }, caps.Select(c => ((TextElement)c).text).ToArray());
        }

        /// <summary>
        /// Rebuilding must clear the previous caps rather than appending to them, otherwise every
        /// assignment leaves stale keys on screen.
        /// </summary>
        [Test]
        public void Keys_ReassignedDoNotAccumulate()
        {
            var kbd = new Kbd { keys = new List<string> { "Ctrl", "Shift", "K" } };
            kbd.keys = new List<string> { "Alt" };
            var caps = Caps(kbd);
            CollectionAssert.AreEqual(new[] { "Alt" }, caps.Select(c => ((TextElement)c).text).ToArray());
        }

        [Test]
        public void Keys_ClearedFallBackToTheTextForm()
        {
            var kbd = new Kbd { text = "Esc", keys = new List<string> { "Ctrl", "K" } };
            kbd.keys = null;
            var caps = Caps(kbd);
            Assert.AreEqual(1, caps.Count);
            Assert.AreEqual("Esc", ((TextElement)caps[0]).text);
        }

        [Test]
        public void Text_UpdatesTheCapWhileInTheTextForm()
        {
            var kbd = new Kbd { text = "A" };
            kbd.text = "B";
            var caps = Caps(kbd);
            Assert.AreEqual(1, caps.Count);
            Assert.AreEqual("B", ((TextElement)caps[0]).text);
        }

        /// <summary>
        /// The UXML `keys` attribute has to round-trip through the collection converter, otherwise a
        /// combo authored in UXML silently collapses to a single cap.
        /// </summary>
        [UnityTest]
        [ConditionalIgnore("IgnoreInPlayer", "UXML construction is supported only in Editor")]
        public IEnumerator Keys_RoundTripThroughUxml()
        {
            const string content =
                "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:appui=\"Unity.AppUI.UI\">" +
                "<appui:Kbd name=\"combo\" keys=\"Ctrl,K\" />" +
                "</ui:UXML>";
            m_TestUI.visualTreeAsset = Utils.LoadUxmlTemplateFromString(content);
            yield return null;

            var kbd = m_TestUI.rootVisualElement.Q<Kbd>("combo");
            Assert.IsNotNull(kbd, "The UXML Kbd was not instantiated.");
            CollectionAssert.AreEqual(new[] { "Ctrl", "K" },
                Caps(kbd).Select(c => ((TextElement)c).text).ToArray());
        }

        /// <summary>
        /// Pins the key cap to the token contract. h-5 / min-w-5 in the registry is the sm
        /// control-sizing step, so the cap must bind to that alias, never to a raw sizing step and
        /// never to a literal pixel value.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void CapBox_BindsToTheControlSizingAliases()
        {
            var body = RuleBody(ReadUss(), @"\.appui-kbd__cap");
            StringAssert.Contains("height: var(--appui-alias-control-sizing-sm-height)", body,
                ".appui-kbd__cap must bind its height to the sm control-sizing alias.");
            StringAssert.Contains("min-width: var(--appui-alias-control-sizing-sm-square)", body,
                ".appui-kbd__cap must bind its min-width to the sm control-sizing square alias.");
            // Padding and margin may read a raw sizing step matching the registry's spacing
            // (px-1.5 is --appui-sizing-115, gap-1 is --appui-sizing-75); sizes may not.
            Assert.IsFalse(Regex.IsMatch(body,
                    @"(?<![-\w])(height|width|min-width|min-height)\s*:\s*var\(--appui-sizing-\d+\)"),
                ".appui-kbd__cap sizes itself from a raw --appui-sizing step instead of an alias.");
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                ".appui-kbd__cap uses a literal pixel value instead of a token.");
        }

        /// <summary>
        /// The cap skin: raised surface, strong border, a thicker bottom edge for keycap depth, and
        /// secondary (not disabled) text so the legend stays legible.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void CapSkin_BindsToTheSurfaceBorderAndTextTokens()
        {
            var body = RuleBody(ReadUss(), @"\.appui-kbd__cap");
            StringAssert.Contains("background-color: var(--appui-alias-surface-raised)", body);
            StringAssert.Contains("border-color: var(--appui-base-500)", body);
            StringAssert.Contains("color: var(--appui-alias-text-secondary)", body);
            StringAssert.Contains("border-bottom-width: var(--appui-border-width-sm)", body,
                "The cap needs a thicker bottom edge to read as a physical key.");
            StringAssert.Contains("border-radius: var(--appui-alias-control-border-radius)", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void CapFontSize_BindsToTheXsTypeStep()
        {
            var body = RuleBody(ReadUss(), @"\.appui-kbd__cap");
            StringAssert.Contains("font-size: var(--appui-font-sizes-75)", body,
                "The registry's text-xs maps to --appui-font-sizes-75.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColours()
        {
            var uss = ReadUss();
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Kbd.uss contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Kbd.uss contains a literal rgb()/rgba() colour.");
        }

        [UnityTest]
        public IEnumerator Caps_ShrinkUnderCompactDensity()
        {
            var normal = new Kbd { text = "K" };
            yield return Mount(normal, "normal");
            var normalHeight = Caps(normal)[0].resolvedStyle.height;

            var compact = new Kbd { text = "K" };
            yield return Mount(compact, "compact");
            var compactHeight = Caps(compact)[0].resolvedStyle.height;

            Assert.Greater(normalHeight, 0f, "The cap resolved to a zero height; Kbd.uss is probably not loaded.");
            Assert.Less(compactHeight, normalHeight,
                $"The cap did not shrink under compact density (normal {normalHeight}, compact {compactHeight}). " +
                "Its height is probably a hardcoded pixel value.");
        }

        static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"Kbd.uss has no {selectorPattern} rule.");
            return rule.Groups["body"].Value;
        }

        IEnumerator Mount(VisualElement element, string density = "normal")
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(element);

            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
