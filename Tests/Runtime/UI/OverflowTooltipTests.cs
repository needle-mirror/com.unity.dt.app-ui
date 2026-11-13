using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(OverflowTooltip))]
    class OverflowTooltipTests : VisualElementTests<OverflowTooltip>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/OverflowTooltip.uss";

        const string k_Long = "A very long asset name that will certainly not fit in a narrow column";

        protected override string mainUssClassName => OverflowTooltip.ussClassName;

        // Handed over to OverflowTooltipSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new OverflowTooltip { text = "Fits" });
                yield return new Story("Truncated", ctx =>
                {
                    var host = new VisualElement { style = { width = 120 } };
                    host.Add(new OverflowTooltip { text = k_Long });
                    return host;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:OverflowTooltip />",
            @"<appui:OverflowTooltip text=""Some label"" placement=""Top"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var element = new OverflowTooltip();
            Assert.AreEqual(PopoverPlacement.Top, element.placement);
            Assert.IsFalse(element.isOverflowing);
            Assert.IsTrue(string.IsNullOrEmpty(element.tooltip),
                "An element that has not been measured must not claim a tooltip.");
        }

        [UnityTest]
        public IEnumerator Overflowing_TextGetsATooltip_AndTheOverflowClass()
        {
            var host = new VisualElement { style = { width = 100 } };
            var element = new OverflowTooltip { text = k_Long };
            host.Add(element);
            yield return MountInPanel(host);

            Assert.IsTrue(element.isOverflowing,
                "A label wider than its box must report itself as overflowing.");
            Assert.AreEqual(k_Long, element.tooltip,
                "The tooltip must carry the full, untruncated text.");
            Assert.IsTrue(element.ClassListContains(OverflowTooltip.overflowingUssClassName));
        }

        [UnityTest]
        public IEnumerator FittingText_GetsNoTooltip()
        {
            var host = new VisualElement { style = { width = 600 } };
            var element = new OverflowTooltip { text = "Fits" };
            host.Add(element);
            yield return MountInPanel(host);

            Assert.IsFalse(element.isOverflowing);
            Assert.IsTrue(string.IsNullOrEmpty(element.tooltip),
                "Text that fits must not carry a tooltip — that is the whole point of the component.");
            Assert.IsFalse(element.ClassListContains(OverflowTooltip.overflowingUssClassName));
        }

        [UnityTest]
        public IEnumerator Overflow_IsRemeasured_WhenTheBoxNarrows()
        {
            var host = new VisualElement { style = { width = 600 } };
            var element = new OverflowTooltip { text = k_Long };
            host.Add(element);
            yield return MountInPanel(host);
            Assert.IsFalse(element.isOverflowing, "600px is wide enough for the sample label.");

            host.style.width = 80;
            yield return null;
            yield return null;

            Assert.IsTrue(element.isOverflowing,
                "Narrowing the container must re-measure and turn the tooltip on.");
            Assert.AreEqual(k_Long, element.tooltip);
        }

        [UnityTest]
        public IEnumerator Overflow_IsRemeasured_WhenTheTextChanges()
        {
            var host = new VisualElement { style = { width = 100 } };
            var element = new OverflowTooltip { text = "Fits" };
            host.Add(element);
            yield return MountInPanel(host);
            Assert.IsFalse(element.isOverflowing);

            element.text = k_Long;
            yield return null;
            yield return null;

            Assert.IsTrue(element.isOverflowing,
                "A rename that overflows the same box must turn the tooltip on.");
            Assert.AreEqual(k_Long, element.tooltip);
        }

        [UnityTest]
        public IEnumerator OverflowChanged_FiresOnlyOnTransitions()
        {
            var host = new VisualElement { style = { width = 600 } };
            var element = new OverflowTooltip { text = k_Long };
            var events = new List<bool>();
            element.overflowChanged += v => events.Add(v);
            host.Add(element);
            yield return MountInPanel(host);
            Assert.AreEqual(0, events.Count, "No transition yet — the label fits.");

            host.style.width = 80;
            yield return null;
            yield return null;
            Assert.AreEqual(new[] { true }, events);

            host.style.width = 600;
            yield return null;
            yield return null;
            Assert.AreEqual(new[] { true, false }, events);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_TruncatesWithAnEllipsis()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            var rule = Regex.Match(uss, @"\.appui-overflow-tooltip\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, "OverflowTooltip.uss has no .appui-overflow-tooltip rule.");
            var body = rule.Groups["body"].Value;
            StringAssert.Contains("text-overflow: ellipsis", body,
                "The label must ellipsize — without it nothing ever visually truncates.");
            StringAssert.Contains("white-space: nowrap", body,
                "Wrapping text never overflows horizontally, so the tooltip would never appear.");
            StringAssert.Contains("overflow: hidden", body);

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"),
                "OverflowTooltip.uss uses a literal pixel value instead of a token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b|\brgba?\("),
                "OverflowTooltip.uss uses a literal colour instead of a token.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheMainStyleSheet()
        {
            var path = Path.GetFullPath("Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss");
            StringAssert.Contains("Components/OverflowTooltip.uss", File.ReadAllText(path),
                "OverflowTooltip.uss must be imported by App UI.uss or it styles nothing.");
        }

        IEnumerator MountInPanel(VisualElement content)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                    "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(content);
            for (var i = 0; i < 4; i++)
                yield return null;
        }
    }
}
