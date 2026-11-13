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
    [TestOf(typeof(Skeleton))]
    class SkeletonTests : VisualElementTests<Skeleton>
    {
        protected override string mainUssClassName => Skeleton.ussClassName;

        // Handed over to SkeletonSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new Skeleton { style = { width = 160 } });
                yield return new Story("Static", (ctx) => new Skeleton { animated = false, style = { width = 160 } });
                yield return new Story("Stack", (ctx) =>
                {
                    var col = new VisualElement { style = { width = 200 } };
                    col.Add(new Skeleton { style = { height = 12, marginBottom = 6 } });
                    col.Add(new Skeleton { style = { height = 12, marginBottom = 6, width = Length.Percent(80) } });
                    col.Add(new Skeleton { style = { height = 12, width = Length.Percent(60) } });
                    return col;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Skeleton />",
            @"<appui:Skeleton animated=""false"" />",
            @"<appui:Skeleton animated=""true"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Skeleton.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        [Test]
        public void Default_IsAnimated()
        {
            var skeleton = new Skeleton();
            Assert.IsTrue(skeleton.animated, "A Skeleton pulses by default, like the registry's animate-pulse.");
            Assert.IsTrue(skeleton.ClassListContains(Skeleton.animatedUssClassName));
        }

        [Test]
        public void Animated_TogglesTheUssClass()
        {
            var skeleton = new Skeleton { animated = false };
            Assert.IsFalse(skeleton.ClassListContains(Skeleton.animatedUssClassName));
            skeleton.animated = true;
            Assert.IsTrue(skeleton.ClassListContains(Skeleton.animatedUssClassName));
        }

        /// <summary>
        /// The pulse envelope is only right if its constants are. Pinned to the literal values of the
        /// web implementation's animate-pulse: a two second cycle dipping to half opacity. Asserting
        /// against <see cref="Skeleton.minPulseOpacity"/> elsewhere is tautological, so this is the
        /// test that fails when someone retunes the pulse.
        /// </summary>
        [Test]
        public void PulseConstants_MatchTheWebAnimatePulse()
        {
            Assert.AreEqual(2000f, Skeleton.pulsePeriodMs, 0.001f,
                "animate-pulse runs on a 2s cycle.");
            Assert.AreEqual(0.5f, Skeleton.minPulseOpacity, 0.001f,
                "animate-pulse dips to 50% opacity at the middle of the cycle.");
        }

        /// <summary>
        /// The pulse must reproduce Tailwind's <c>animate-pulse</c>: full opacity at the start of the
        /// period, half opacity at the midpoint, back to full at the end.
        /// </summary>
        [Test]
        public void PulseOpacity_MatchesTheWebPulseEnvelope()
        {
            var periodSeconds = Skeleton.pulsePeriodMs / 1000f;

            Assert.AreEqual(1f, Skeleton.ComputePulseOpacity(0f), 0.001f,
                "The pulse must start at full opacity.");
            Assert.AreEqual(Skeleton.minPulseOpacity, Skeleton.ComputePulseOpacity(periodSeconds * 0.5f), 0.001f,
                "The pulse must reach its minimum at the half period.");
            Assert.AreEqual(1f, Skeleton.ComputePulseOpacity(periodSeconds), 0.001f,
                "The pulse must return to full opacity after one period.");
        }

        [Test]
        public void PulseOpacity_IsPeriodicAndStaysInRange()
        {
            var periodSeconds = Skeleton.pulsePeriodMs / 1000f;
            for (var i = 0; i <= 40; i++)
            {
                var t = periodSeconds * i / 40f;
                var value = Skeleton.ComputePulseOpacity(t);
                Assert.GreaterOrEqual(value, Skeleton.minPulseOpacity - 0.001f, $"Pulse dipped below the minimum at t={t}.");
                Assert.LessOrEqual(value, 1f + 0.001f, $"Pulse exceeded full opacity at t={t}.");
                Assert.AreEqual(value, Skeleton.ComputePulseOpacity(t + periodSeconds * 3f), 0.001f,
                    $"Pulse is not periodic at t={t}.");
            }
        }

        /// <summary>
        /// The driver must actually write the computed opacity onto the element. Time is frozen by the
        /// harness, so the tick is driven by hand through the time override.
        /// </summary>
        [UnityTest]
        public IEnumerator Ticking_WritesThePulseOpacityOntoTheElement()
        {
            var skeleton = new Skeleton();
            yield return Mount(skeleton);

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = 0f;
                skeleton.TickPulse();
                Assert.AreEqual(1f, skeleton.resolvedStyle.opacity, 0.01f,
                    "At phase 0 the skeleton must be fully opaque.");

                TimeUtils.timeOverride = Skeleton.pulsePeriodMs / 2000f;
                skeleton.TickPulse();
                Assert.AreEqual(Skeleton.minPulseOpacity, skeleton.resolvedStyle.opacity, 0.01f,
                    "At the half period the skeleton must sit at the minimum opacity.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        /// <summary>
        /// Turning the pulse off must both stop the driver and restore full opacity, otherwise the
        /// skeleton freezes at whatever opacity the last tick happened to write.
        /// </summary>
        [UnityTest]
        public IEnumerator TurningTheAnimationOff_StopsTheDriverAndRestoresFullOpacity()
        {
            var skeleton = new Skeleton();
            yield return Mount(skeleton);
            Assert.IsTrue(skeleton.isPulsing, "An attached, animated Skeleton must be driving its pulse.");

            var previous = TimeUtils.timeOverride;
            try
            {
                TimeUtils.timeOverride = Skeleton.pulsePeriodMs / 2000f;
                skeleton.TickPulse();
                Assert.Less(skeleton.resolvedStyle.opacity, 1f);

                skeleton.animated = false;
                yield return null;
                Assert.IsFalse(skeleton.isPulsing, "Setting animated=false must stop the pulse driver.");
                Assert.AreEqual(1f, skeleton.resolvedStyle.opacity, 0.01f,
                    "Stopping the pulse must restore full opacity.");
            }
            finally
            {
                TimeUtils.timeOverride = previous;
            }
        }

        [UnityTest]
        public IEnumerator DetachingFromThePanel_StopsTheDriver()
        {
            var skeleton = new Skeleton();
            yield return Mount(skeleton);
            Assert.IsTrue(skeleton.isPulsing);

            skeleton.RemoveFromHierarchy();
            yield return null;
            Assert.IsFalse(skeleton.isPulsing, "A detached Skeleton must not keep a scheduled item alive.");
        }

        [UnityTest]
        public IEnumerator AStaticSkeleton_NeverStartsTheDriver()
        {
            var skeleton = new Skeleton { animated = false };
            yield return Mount(skeleton);
            Assert.IsFalse(skeleton.isPulsing);
            Assert.AreEqual(1f, skeleton.resolvedStyle.opacity, 0.01f);
        }

        /// <summary>
        /// The surface must come from the muted surface token, which is what the registry's
        /// <c>bg-muted</c> maps to in COLOR_MAP. A literal colour fails here.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Surface_BindsToTheMutedSurfaceToken()
        {
            var body = RuleBody(ReadUss(), @"\.appui-skeleton");
            StringAssert.Contains("background-color: var(--appui-alias-surface-muted)", body,
                ".appui-skeleton must paint itself with the muted surface token.");
            StringAssert.Contains("border-radius: var(--appui-alias-control-border-radius)", body,
                ".appui-skeleton must take its radius from the control border-radius alias (rounded-md).");
        }

        /// <summary>
        /// Pins the sizing to the token contract: the default box must bind to a control-sizing alias,
        /// never to a raw sizing step and never to a literal pixel value.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void MinHeight_ComesFromTheControlSizingAlias()
        {
            var body = RuleBody(ReadUss(), @"\.appui-skeleton");
            StringAssert.Contains("min-height: var(--appui-alias-control-sizing-md-height)", body,
                ".appui-skeleton must bind its default height to the md control-sizing alias.");
            Assert.IsFalse(Regex.IsMatch(body, @"--appui-sizing-\d"),
                ".appui-skeleton uses a raw --appui-sizing step instead of the alias.");
            Assert.IsFalse(Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                ".appui-skeleton uses a literal pixel value instead of a token.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_ContainsNoLiteralColours()
        {
            var uss = ReadUss();
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Skeleton.uss contains a hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Skeleton.uss contains a literal rgb()/rgba() colour.");
        }

        /// <summary>
        /// The default box must shrink under compact density. That only holds while the height goes
        /// through a density-aware sizing token rather than a hardcoded pixel value.
        /// </summary>
        [UnityTest]
        public IEnumerator DefaultBox_ShrinksUnderCompactDensity()
        {
            var normal = new Skeleton();
            yield return Mount(normal, "normal");
            var normalHeight = normal.resolvedStyle.height;

            var compact = new Skeleton();
            yield return Mount(compact, "compact");
            var compactHeight = compact.resolvedStyle.height;

            Assert.Greater(normalHeight, 0f, "The Skeleton resolved to a zero height; Skeleton.uss is probably not loaded.");
            Assert.Less(compactHeight, normalHeight,
                $"The Skeleton did not shrink under compact density (normal {normalHeight}, compact {compactHeight}). " +
                "Its height is probably a hardcoded pixel value.");
        }

        static string RuleBody(string uss, string selectorPattern)
        {
            var rule = Regex.Match(uss, selectorPattern + @"\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success, $"Skeleton.uss has no {selectorPattern} rule.");
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
