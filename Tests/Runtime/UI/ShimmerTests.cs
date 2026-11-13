using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Shimmer))]
    class ShimmerTests : VisualElementTests<Shimmer>
    {
        protected override string mainUssClassName => Shimmer.ussClassName;

        // Handed over to ShimmerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Shimmer("Thinking"));
                yield return new Story("Static", (ctx) => new Shimmer("Thinking") { active = false });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Shimmer />",
            @"<appui:Shimmer text=""Thinking"" />",
            @"<appui:Shimmer text=""Thinking"" active=""false"" />",
            @"<appui:Shimmer text=""Thinking"" active=""true"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Shimmer.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        [Test]
        public void Default_IsActive()
        {
            var shimmer = new Shimmer();
            Assert.IsTrue(shimmer.active, "A Shimmer loops by default, like the registry's active={true}.");
            Assert.IsTrue(shimmer.ClassListContains(Shimmer.activeUssClassName));
        }

        [Test]
        public void Active_TogglesTheUssClass()
        {
            var shimmer = new Shimmer { active = false };
            Assert.IsFalse(shimmer.ClassListContains(Shimmer.activeUssClassName));
            shimmer.active = true;
            Assert.IsTrue(shimmer.ClassListContains(Shimmer.activeUssClassName));
        }

        [Test]
        public void TextConstructor_SetsTheText()
        {
            Assert.AreEqual("Thinking", new Shimmer("Thinking").text);
        }

        /// <summary>
        /// The loop's period is pinned to the web implementation's literal value. Asserting against
        /// <see cref="Shimmer.sweepPeriodMs"/> elsewhere is tautological, so this is the test that
        /// fails when someone retunes the sweep.
        /// </summary>
        [Test]
        public void SweepPeriod_MatchesTheWebSweep()
        {
            Assert.AreEqual(2000f, Shimmer.sweepPeriodMs, 0.001f,
                "The web shimmer-sweep keyframe animation runs on a 2s cycle.");
        }

        /// <summary>
        /// The envelope must rest at the start of the period, peak at the midpoint and return to rest
        /// at the end, which is what the travelling gradient does as it crosses the glyphs.
        /// </summary>
        [Test]
        public void SweepBlend_RestsAtTheEdgesAndPeaksAtTheMiddle()
        {
            var periodSeconds = Shimmer.sweepPeriodMs / 1000f;

            Assert.AreEqual(0f, Shimmer.ComputeSweepBlend(0f), 0.001f,
                "The sweep must start at the resting color.");
            Assert.AreEqual(1f, Shimmer.ComputeSweepBlend(periodSeconds * 0.5f), 0.001f,
                "The sweep must be fully highlighted at the half period.");
            Assert.AreEqual(0f, Shimmer.ComputeSweepBlend(periodSeconds), 0.001f,
                "The sweep must return to the resting color after one period.");
        }

        [Test]
        public void SweepBlend_IsPeriodicAndStaysInRange()
        {
            var periodSeconds = Shimmer.sweepPeriodMs / 1000f;
            for (var i = 0; i <= 40; i++)
            {
                var t = periodSeconds * i / 40f;
                var value = Shimmer.ComputeSweepBlend(t);
                Assert.GreaterOrEqual(value, -0.001f, $"Sweep blend went negative at t={t}.");
                Assert.LessOrEqual(value, 1.001f, $"Sweep blend exceeded 1 at t={t}.");
                Assert.AreEqual(value, Shimmer.ComputeSweepBlend(t + periodSeconds * 3f), 0.001f,
                    $"Sweep is not periodic at t={t}.");
            }
        }

        /// <summary>
        /// The driver must actually move the text colour, and it must move it towards the highlight
        /// rather than anywhere else. Time is frozen by the harness, so the tick is driven by hand.
        /// </summary>
        [UnityTest]
        public IEnumerator Ticking_MovesTheTextColourTowardsTheHighlight()
        {
            var shimmer = new Shimmer("Thinking");
            yield return Mount(shimmer);

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = 0f;
                shimmer.TickSweep();
                yield return null;
                var resting = shimmer.resolvedStyle.color;

                TimeUtils.timeOverride = Shimmer.sweepPeriodMs / 2000f;
                shimmer.TickSweep();
                yield return null;
                var peak = shimmer.resolvedStyle.color;

                Assert.AreNotEqual(resting, peak,
                    "The sweep must change the text colour between the start and the middle of its period. " +
                    "If these match, the highlight custom property is probably not resolving.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        /// <summary>
        /// Turning the loop off must both stop the driver and restore the resting colour, otherwise the
        /// text freezes at whatever colour the last tick happened to write.
        /// </summary>
        [UnityTest]
        public IEnumerator TurningTheLoopOff_StopsTheDriverAndRestoresTheRestingColour()
        {
            var shimmer = new Shimmer("Thinking");
            yield return Mount(shimmer);
            Assert.IsTrue(shimmer.isSweeping, "An attached, active Shimmer must be driving its sweep.");

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = 0f;
                shimmer.TickSweep();
                yield return null;
                var resting = shimmer.resolvedStyle.color;

                TimeUtils.timeOverride = Shimmer.sweepPeriodMs / 2000f;
                shimmer.TickSweep();
                yield return null;
                Assert.AreNotEqual(resting, shimmer.resolvedStyle.color);

                shimmer.active = false;
                yield return null;
                Assert.IsFalse(shimmer.isSweeping, "Setting active=false must stop the sweep driver.");
                Assert.AreEqual(resting, shimmer.resolvedStyle.color,
                    "Stopping the sweep must restore the resting colour.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator DetachingFromThePanel_StopsTheDriver()
        {
            var shimmer = new Shimmer("Thinking");
            yield return Mount(shimmer);
            Assert.IsTrue(shimmer.isSweeping);

            shimmer.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(shimmer.isSweeping, "A detached Shimmer must not keep a scheduled item alive.");
        }

        [UnityTest]
        public IEnumerator AStaticShimmer_NeverStartsTheDriver()
        {
            var shimmer = new Shimmer("Thinking") { active = false };
            yield return Mount(shimmer);
            Assert.IsFalse(shimmer.isSweeping);
        }

        /// <summary>
        /// The highlight is the one colour the package owns here, and it must come from the text
        /// primary token, which is what the registry's <c>var(--foreground)</c> gradient stop maps to.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Highlight_BindsToTheTextPrimaryToken()
        {
            var body = RuleBody(ReadUss(), @"\.appui-shimmer");
            StringAssert.Contains("--shimmer-highlight-color: var(--appui-alias-text-primary)", body,
                ".appui-shimmer must take its highlight from the text primary token.");
            StringAssert.Contains("--shimmer-base-color: var(--appui-alias-text-secondary)", body,
                ".appui-shimmer must take its resting colour from the text secondary token.");
            StringAssert.Contains("color: var(--shimmer-base-color)", body,
                "The element must paint its resting colour from the same var the driver lerps from, "
                + "or a stopped Shimmer would not match the bottom of its own loop.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColours()
        {
            var uss = ReadUss();
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Shimmer.uss contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Shimmer.uss contains a literal rgb()/rgba() colour.");
        }

        /// <summary>
        /// An orphaned stylesheet is silent: the component renders unstyled, the highlight custom
        /// property never resolves, and the sweep quietly does nothing.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheStyleManifest()
        {
            var manifest = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains(@"@import ""./Components/Shimmer.uss"";", manifest,
                "Shimmer.uss must be imported by App UI.uss or it is never loaded.");
        }

        static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"Shimmer.uss has no {selectorPattern} rule.");
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
