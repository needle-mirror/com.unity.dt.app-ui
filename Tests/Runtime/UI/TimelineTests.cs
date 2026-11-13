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
    // Declared inside the namespace: a compilation-unit alias loses to the root
    // `Timeline` namespace Unity 6.6 puts in scope, but an alias at this level is
    // found before the lookup walks outward.
    using Timeline = Unity.AppUI.UI.Timeline;

    [TestFixture]
    [TestOf(typeof(Timeline))]
    class TimelineTests : VisualElementTests<Timeline>
    {
        protected override string mainUssClassName => Timeline.ussClassName;

        // Handed over to TimelineSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => BuildTimeline(0));
                yield return new Story("With gutter", (ctx) => BuildTimeline(120));
            }
        }

        static Timeline BuildTimeline(float labelWidth)
        {
            var timeline = new Timeline
            {
                duration = 60f,
                labelWidth = labelWidth,
                value = 12f,
                style = { width = 640, height = 160 },
            };

            var track = new TimelineTrack { id = "position", label = "Position" };
            track.Add(new TimelineClip { start = 4f, duration = 12f, label = "Walk" });
            track.Add(new TimelineKeyframe { time = 24f });
            timeline.Add(track);

            var group = new TimelineTrackGroup { id = "cube", title = "Cube" };
            var rotation = new TimelineTrack { id = "rotation", label = "Rotation" };
            rotation.Add(new TimelineKeyframe { time = 8f });
            group.Add(rotation);
            timeline.Add(group);

            timeline.AddMarker(new TimelineMarker { time = 30f, label = "Cut" });
            return timeline;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Timeline duration=""60"" />",
            @"<appui:Timeline duration=""60"" px-per-unit=""12"" step=""0"" label-width=""120"" control-scheme=""Editor"" />",
            @"<appui:Timeline duration=""60"" label-width=""120"" label-resizable=""true"" min-label-width=""80"" hover-line=""true"" time-suffix=""/ 2s"" />",
            @"<appui:Timeline duration=""60"" lane-extent=""40"" key-size=""Small"" min-tick-step=""0.1"" ticks-at=""Top"">
                 <appui:TimelineTrack id=""a"" lane=""false"" lane-start=""4"" lane-end=""20"">
                   <appui:TimelineClip start=""4"" duration=""12"" variant=""Handle"" resizable=""true"" />
                   <appui:TimelineKeyframe time=""24"" size=""Regular"" />
                   <appui:TimelineHandle time=""20"" min=""4"" max=""40"" key-step=""0.5"" />
                 </appui:TimelineTrack>
               </appui:Timeline>",
            @"<appui:Timeline duration=""60"" label-width=""120"">
                 <appui:TimelineTrack id=""position"" label=""Position"">
                   <appui:TimelineClip start=""4"" duration=""12"" label=""Walk"" />
                   <appui:TimelineKeyframe time=""24"" />
                 </appui:TimelineTrack>
                 <appui:TimelineTrackGroup id=""cube"" title=""Cube"">
                   <appui:TimelineTrack id=""rotation"" label=""Rotation"" />
                 </appui:TimelineTrackGroup>
               </appui:Timeline>",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Timeline.uss";

        static string ReadUss()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            return File.ReadAllText(fullPath);
        }

        /// <summary>
        /// Pull one rule body out of Timeline.uss. The pattern is anchored to the start of a line so
        /// `.appui-timeline__x` never accidentally matches `.appui-timeline--y .appui-timeline__x`
        /// further up the file (PORTING-GUIDE, "A bare class pattern can match the wrong rule").
        /// </summary>
        static string RuleBody(string uss, string selector)
        {
            var rule = Regex.Match(uss, @"^" + Regex.Escape(selector) + @"\s*\{(?<body>[^}]*)\}",
                RegexOptions.Multiline);
            Assert.IsTrue(rule.Success, $"Timeline.uss has no top-level `{selector}` rule.");
            return rule.Groups["body"].Value;
        }

        static string StripComments(string uss) =>
            Regex.Replace(uss, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

        // ------------------------------------------------------------------ defaults

        [Test]
        public void Default_MatchesTheRegistryDefaults()
        {
            var timeline = new Timeline();
            Assert.AreEqual(0f, timeline.duration);
            Assert.AreEqual(8f, timeline.pxPerUnit, "The registry defaults pxPerUnit to 8.");
            Assert.AreEqual(1f, timeline.minorTick);
            Assert.AreEqual(5f, timeline.majorTick);
            Assert.AreEqual(1f, timeline.step);
            Assert.AreEqual(Timeline.defaultTrackHeight, timeline.trackHeight);
            Assert.AreEqual(Timeline.defaultRulerHeight, timeline.rulerHeight);
            Assert.AreEqual(0f, timeline.labelWidth, "The gutter is hidden by default.");
            Assert.IsTrue(timeline.showFrameLines);
            Assert.IsFalse(timeline.infinite);
            Assert.IsFalse(timeline.disabled);
            Assert.AreEqual(TimelineControlScheme.Modern, timeline.controlScheme);
            Assert.AreEqual(1f, timeline.zoom);
        }

        [Test]
        public void LayoutConstants_MatchTheStaticSizingSteps()
        {
            // The static ramp is 0.08px per step and no density class overrides it, so these four
            // constants stay in agreement with the USS that renders them under `.appui--compact`.
            Assert.AreEqual(16f, Timeline.origin);
            Assert.AreEqual(24f, Timeline.endPadding);
            Assert.AreEqual(32f, Timeline.defaultTrackHeight);
            Assert.AreEqual(24f, Timeline.defaultRulerHeight);
        }

        // ------------------------------------------------------------------ time axis

        [Test]
        public void TimeToX_IsOriginPlusTimeTimesEffectivePixelsPerUnit()
        {
            var timeline = new Timeline { duration = 100f, pxPerUnit = 10f };
            Assert.AreEqual(10f, timeline.effectivePxPerUnit);
            Assert.AreEqual(Timeline.origin, timeline.TimeToX(0f), 0.001f);
            Assert.AreEqual(Timeline.origin + 50f, timeline.TimeToX(5f), 0.001f);
            Assert.AreEqual(Timeline.origin + 1000f, timeline.TimeToX(100f), 0.001f);
        }

        [Test]
        public void XToTime_IsTheExactInverseOfTimeToX()
        {
            var timeline = new Timeline { duration = 100f, pxPerUnit = 7f, zoom = 1.35f };
            foreach (var t in new[] { 0f, 0.5f, 13.25f, 99.75f })
                Assert.AreEqual(t, timeline.XToTime(timeline.TimeToX(t)), 0.0001f,
                    $"XToTime/TimeToX round trip drifted at t={t}.");
        }

        [Test]
        public void EffectivePxPerUnit_IsClampedToTheZoomRange()
        {
            var timeline = new Timeline { duration = 100f, pxPerUnit = 8f };

            timeline.zoom = 0.0001f;
            Assert.AreEqual(Timeline.minEffectivePxPerUnit, timeline.effectivePxPerUnit, 0.001f);

            timeline.zoom = 1000f;
            Assert.AreEqual(Timeline.maxEffectivePxPerUnit, timeline.effectivePxPerUnit, 0.001f);
        }

        /// <summary>
        /// The parts project through the axis context, not through the Timeline, so the two
        /// projections have to be the same one. A sabotage that dropped the origin inset from
        /// Timeline.TimeToX left every part rendering correctly — this pins the pair together.
        /// </summary>
        [UnityTest]
        public IEnumerator TimeToX_AgreesBetweenTheTimelineAndTheAxisContext()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, style = { width = 800, height = 160 } };
            var track = new TimelineTrack { id = "a" };
            var keyframe = new TimelineKeyframe { time = 17f };
            track.Add(keyframe);
            timeline.Add(track);
            yield return Mount(timeline);

            Assert.AreEqual(timeline.TimeToX(17f), keyframe.resolvedStyle.left, 0.001f,
                "A part rendered at a different x than Timeline.TimeToX reports for the same time.");
        }

        [Test]
        public void StripWidth_IsOriginPlusSpanPlusEndPadding()
        {
            var timeline = new Timeline { duration = 50f, pxPerUnit = 4f };
            Assert.AreEqual(Timeline.origin + 200f + Timeline.endPadding, timeline.stripWidth, 0.001f);
        }

        // ------------------------------------------------------------------ playhead value

        [Test]
        public void Value_IsSnappedToTheStepAndClampedToTheDuration()
        {
            var timeline = new Timeline { duration = 10f, step = 1f };

            timeline.value = 3.4f;
            Assert.AreEqual(3f, timeline.value, 0.001f);

            timeline.value = 3.6f;
            Assert.AreEqual(4f, timeline.value, 0.001f);

            timeline.value = -5f;
            Assert.AreEqual(0f, timeline.value, 0.001f);

            timeline.value = 100f;
            Assert.AreEqual(10f, timeline.value, 0.001f);
        }

        [Test]
        public void Value_IsContinuousWhenTheStepIsZero()
        {
            var timeline = new Timeline { duration = 10f, step = 0f, value = 3.456f };
            Assert.AreEqual(3.456f, timeline.value, 0.0001f);
        }

        [Test]
        public void Value_NotifiesOnChangeOnlyWhenTheSnappedValueMoves()
        {
            var timeline = new Timeline { duration = 10f, step = 1f };
            var changes = new List<float>();
            timeline.valueChanged += t => changes.Add(t);

            timeline.value = 3.4f;   // -> 3
            timeline.value = 3.2f;   // -> 3, same snapped value, no event
            timeline.value = 4f;     // -> 4

            CollectionAssert.AreEqual(new[] { 3f, 4f }, changes);
        }

        [Test]
        public void Selection_IsExposedAndClearable()
        {
            var timeline = new Timeline { duration = 10f };
            Assert.IsFalse(timeline.hasSelection);

            var seen = 0;
            timeline.selectionChanged += _ => seen++;

            timeline.selection = new TimelineSelection(2f, 6f);
            Assert.IsTrue(timeline.hasSelection);
            Assert.AreEqual(2f, timeline.selection.Value.start, 0.001f);
            Assert.AreEqual(6f, timeline.selection.Value.end, 0.001f);

            timeline.selection = null;
            Assert.IsFalse(timeline.hasSelection);
            Assert.AreEqual(2, seen);
        }

        // ------------------------------------------------------------------ sub-parts

        [Test]
        public void SubElements_CarryTheirBemUssClasses()
        {
            Assert.AreEqual("appui-timeline__track", TimelineTrack.ussClassName);
            Assert.AreEqual("appui-timeline__group", TimelineTrackGroup.ussClassName);
            Assert.AreEqual("appui-timeline__clip", TimelineClip.ussClassName);
            Assert.AreEqual("appui-timeline__keyframe", TimelineKeyframe.ussClassName);
            Assert.AreEqual("appui-timeline__ruler", TimelineRuler.ussClassName);
            Assert.AreEqual("appui-timeline__grid", TimelineGrid.ussClassName);
            Assert.AreEqual("appui-timeline__marker", TimelineMarker.ussClassName);
            Assert.AreEqual("appui-timeline__range", TimelineRange.ussClassName);
            Assert.AreEqual("appui-timeline__playhead", TimelinePlayhead.ussClassName);

            Assert.IsTrue(new TimelineTrack().ClassListContains(TimelineTrack.ussClassName));
            Assert.IsTrue(new TimelineTrackGroup().ClassListContains(TimelineTrackGroup.ussClassName));
            Assert.IsTrue(new TimelineClip().ClassListContains(TimelineClip.ussClassName));
            Assert.IsTrue(new TimelineKeyframe().ClassListContains(TimelineKeyframe.ussClassName));
            Assert.IsTrue(new TimelineRuler().ClassListContains(TimelineRuler.ussClassName));
            Assert.IsTrue(new TimelineGrid().ClassListContains(TimelineGrid.ussClassName));
            Assert.IsTrue(new TimelineMarker().ClassListContains(TimelineMarker.ussClassName));
            Assert.IsTrue(new TimelineRange().ClassListContains(TimelineRange.ussClassName));
            Assert.IsTrue(new TimelinePlayhead().ClassListContains(TimelinePlayhead.ussClassName));
        }

        [Test]
        public void Timeline_AddsLaneChildrenToTheLanesContainer()
        {
            var timeline = new Timeline { duration = 10f };
            var track = new TimelineTrack { id = "a" };
            timeline.Add(track);

            Assert.AreSame(timeline.contentContainer, track.hierarchy.parent,
                "Tracks must land in the lanes container, not directly on the root.");
            Assert.AreEqual(1, timeline.trackCount);
        }

        // ------------------------------------------------------------------ positioning

        [UnityTest]
        public IEnumerator Clip_PositionsItselfOnTheTimeAxis()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, style = { width = 800, height = 160 } };
            var track = new TimelineTrack { id = "a" };
            var clip = new TimelineClip { start = 4f, duration = 12f };
            track.Add(clip);
            timeline.Add(track);
            yield return Mount(timeline);

            Assert.AreEqual(Timeline.origin + 40f, clip.resolvedStyle.left, 0.5f,
                "The clip did not land on the time it represents.");
            Assert.AreEqual(120f, clip.resolvedStyle.width, 0.5f,
                "The clip width must be duration x effective px-per-unit.");
        }

        [UnityTest]
        public IEnumerator Clip_FollowsAZoomChange()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, style = { width = 800, height = 160 } };
            var track = new TimelineTrack { id = "a" };
            var clip = new TimelineClip { start = 4f, duration = 12f };
            track.Add(clip);
            timeline.Add(track);
            yield return Mount(timeline);

            timeline.zoom = 2f;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(20f, timeline.effectivePxPerUnit, 0.001f);
            Assert.AreEqual(Timeline.origin + 80f, clip.resolvedStyle.left, 0.5f,
                "The clip did not re-project after a zoom change.");
            Assert.AreEqual(240f, clip.resolvedStyle.width, 0.5f);
        }

        [UnityTest]
        public IEnumerator Keyframe_PositionsItselfOnTheTimeAxis()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, style = { width = 800, height = 160 } };
            var track = new TimelineTrack { id = "a" };
            var keyframe = new TimelineKeyframe { time = 7f };
            track.Add(keyframe);
            timeline.Add(track);
            yield return Mount(timeline);

            Assert.AreEqual(Timeline.origin + 70f, keyframe.resolvedStyle.left, 0.5f);
        }

        [UnityTest]
        public IEnumerator Playhead_SitsAtTheCurrentTime()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, value = 9f, style = { width = 800, height = 160 } };
            timeline.Add(new TimelineTrack { id = "a" });
            yield return Mount(timeline);

            Assert.AreEqual(Timeline.origin + 90f, timeline.playhead.resolvedStyle.left, 0.5f);

            timeline.value = 20f;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(Timeline.origin + 200f, timeline.playhead.resolvedStyle.left, 0.5f,
                "The playhead did not follow the value.");
        }

        [UnityTest]
        public IEnumerator Marker_SitsAtTheTimeItNames()
        {
            var timeline = new Timeline { duration = 60f, pxPerUnit = 10f, style = { width = 800, height = 160 } };
            var marker = new TimelineMarker { time = 30f, label = "Cut" };
            timeline.AddMarker(marker);
            yield return Mount(timeline);

            Assert.AreEqual(Timeline.origin + 300f, marker.resolvedStyle.left, 0.5f);
        }

        [UnityTest]
        public IEnumerator Range_SpansTheSelection()
        {
            var timeline = new Timeline
            {
                duration = 60f, pxPerUnit = 10f,
                selection = new TimelineSelection(5f, 15f),
                style = { width = 800, height = 160 },
            };
            yield return Mount(timeline);

            Assert.AreEqual(Timeline.origin + 50f, timeline.range.resolvedStyle.left, 0.5f);
            Assert.AreEqual(100f, timeline.range.resolvedStyle.width, 0.5f);
        }

        // ------------------------------------------------------------------ ruler

        [UnityTest]
        public IEnumerator Ruler_BuildsMinorAndMajorTicksAcrossTheDuration()
        {
            var timeline = new Timeline
            {
                duration = 20f, pxPerUnit = 10f, minorTick = 1f, majorTick = 5f,
                style = { width = 800, height = 160 },
            };
            yield return Mount(timeline);

            var ticks = timeline.ruler.Query<VisualElement>(className: TimelineRuler.tickUssClassName).ToList();
            Assert.AreEqual(21, ticks.Count, "One tick per minor step, inclusive of both ends.");

            var majors = ticks.Where(t => t.ClassListContains(TimelineRuler.majorTickUssClassName)).ToList();
            Assert.AreEqual(5, majors.Count, "Major ticks land on 0, 5, 10, 15, 20.");

            var labels = timeline.ruler.Query<VisualElement>(className: TimelineRuler.tickLabelUssClassName).ToList();
            Assert.AreEqual(5, labels.Count, "Only major ticks are labelled.");

            Assert.AreEqual(Timeline.origin + 50f, majors[1].resolvedStyle.left, 0.5f,
                "The tick at t=5 is not at its time.");
        }

        [UnityTest]
        public IEnumerator FitWidth_LeavesRoomForTheFixedInsets()
        {
            var timeline = new Timeline
            {
                duration = 60f, pxPerUnit = 10f, fitWidth = true,
                style = { width = 800, height = 160 },
            };
            timeline.Add(new TimelineTrack { id = "a" });
            yield return Mount(timeline);

            var viewport = timeline.Q<VisualElement>(className: Timeline.viewportUssClassName);
            Assert.LessOrEqual(timeline.stripWidth, viewport.resolvedStyle.width + 0.5f,
                "A fitted strip must fit: the scale has to divide the width left after the origin " +
                "and end padding, not the whole viewport.");
            Assert.AreEqual(viewport.resolvedStyle.width, timeline.stripWidth, 0.5f,
                "And it must still fill it.");
        }

        // ------------------------------------------------------------------ gutter / groups

        [UnityTest]
        public IEnumerator Gutter_IsHiddenWhenLabelWidthIsZeroAndShownOtherwise()
        {
            var timeline = new Timeline { duration = 20f, style = { width = 800, height = 160 } };
            timeline.Add(new TimelineTrack { id = "a", label = "A" });
            yield return Mount(timeline);

            Assert.AreEqual(DisplayStyle.None, timeline.gutter.resolvedStyle.display,
                "labelWidth 0 must collapse the gutter entirely, border included.");

            timeline.labelWidth = 120f;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(120f, timeline.gutter.resolvedStyle.width, 0.5f);
            var rows = timeline.gutter.Query<VisualElement>(className: Timeline.gutterRowUssClassName).ToList();
            Assert.AreEqual(1, rows.Count, "One gutter row per lane.");
        }

        [UnityTest]
        public IEnumerator TrackGroup_HidesItsChildTracksWhenCollapsed()
        {
            var timeline = new Timeline { duration = 20f, labelWidth = 120f, style = { width = 800, height = 200 } };
            var group = new TimelineTrackGroup { id = "cube", title = "Cube" };
            var child = new TimelineTrack { id = "rot", label = "Rotation" };
            group.Add(child);
            timeline.Add(group);
            yield return Mount(timeline);

            Assert.IsTrue(group.expanded, "Groups default to expanded.");
            Assert.Greater(child.resolvedStyle.height, 0f);
            var expandedHeight = group.resolvedStyle.height;
            Assert.AreEqual(Timeline.defaultTrackHeight * 2f, expandedHeight, 0.5f,
                "An expanded group is its header lane plus its one child lane.");

            group.expanded = false;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(Timeline.defaultTrackHeight, group.resolvedStyle.height, 0.5f,
                "A collapsed group must not lay its child tracks out — only its header lane.");
        }

        // ------------------------------------------------------------------ USS tokens

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void RulerAndTrackHeights_ComeFromTheStaticSizingRamp()
        {
            var uss = ReadUss();

            // These two heights are ALSO C# constants used to project time onto pixels, so they must
            // come from the static ramp — a density-varying step would let the constants and the
            // rendering silently disagree under `.appui--compact`.
            var ruler = RuleBody(uss, ".appui-timeline__ruler");
            StringAssert.Contains("height: var(--appui-static-sizing-300)", ruler);

            var track = RuleBody(uss, ".appui-timeline__track");
            StringAssert.Contains("height: var(--appui-static-sizing-400)", track);

            foreach (var body in new[] { ruler, track })
            {
                StringAssert.DoesNotContain("--appui-sizing-", body,
                    "A density-varying sizing step would desync the C# layout constants from the rendering.");
                Assert.IsFalse(Regex.IsMatch(body, @"height:\s*-?\d"), "A magic pixel height.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void GridColours_AreCustomStylePropertiesBoundToTokens()
        {
            var body = RuleBody(ReadUss(), ".appui-timeline__grid");
            StringAssert.Contains("--grid-minor-color: var(--appui-", body,
                "The grid paints in generateVisualContent, so its colours must be resolved custom " +
                "style properties bound to tokens, never literals.");
            StringAssert.Contains("--grid-major-color: var(--appui-", body);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void PlayheadAndClip_BindTheirFillsToTokens()
        {
            var uss = ReadUss();
            StringAssert.Contains("background-color: var(--appui-", RuleBody(uss, ".appui-timeline__playhead-line"));
            StringAssert.Contains("background-color: var(--appui-", RuleBody(uss, ".appui-timeline__clip"));
            StringAssert.Contains("background-color: var(--appui-", RuleBody(uss, ".appui-timeline__range-band"));
        }

        /// <summary>
        /// Tokens are the contract: no literal colours and no magic pixel values anywhere in
        /// Timeline.uss.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Timeline_UsesNoLiteralColourAndNoMagicPixelValue()
        {
            var uss = StripComments(ReadUss());
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "Timeline.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "Timeline.uss contains a literal rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "Timeline.uss contains a magic pixel value.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Timeline_IsRegisteredInTheMainStylesheet()
        {
            var path = Path.GetFullPath("Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss");
            StringAssert.Contains("./Components/Timeline.uss", File.ReadAllText(path),
                "A component stylesheet that is never imported has no effect at all.");
        }

        /// <summary>
        /// Guards against the whole fixture running against another worktree's DLLs
        /// (PORTING-GUIDE, "Run from your own worktree, and prove it").
        /// </summary>
        [Test]
        public void Fixture_RunsAgainstThisWorktree()
        {
            Assert.IsNotNull(typeof(Timeline).Assembly.GetType("Unity.AppUI.UI.TimelinePlayhead"));
        }

        IEnumerator Mount(VisualElement content, string density = "normal")
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
            panel.Add(content);

            for (var i = 0; i < 4; i++)
                yield return null;
        }

        /// <summary>
        /// The bounds used to be compile-time consts, so a host with a different time unit could
        /// not widen them. They keep the old values as defaults.
        /// </summary>
        [Test]
        public void ZoomBounds_DefaultToTheOldConstantsAndAreNowSettable()
        {
            var timeline = new Timeline { pxPerUnit = 8f };

            Assert.AreEqual(Timeline.minEffectivePxPerUnit, timeline.minPxPerUnit, 0.001f);
            Assert.AreEqual(Timeline.maxEffectivePxPerUnit, timeline.maxPxPerUnit, 0.001f);

            timeline.maxPxPerUnit = 400f;
            timeline.zoom = 1000f;
            Assert.AreEqual(400f, timeline.effectivePxPerUnit, 0.001f,
                "A raised cap must let the axis zoom past the old 80.");

            timeline.minPxPerUnit = 0.5f;
            timeline.zoom = 0.0001f;
            Assert.AreEqual(0.5f, timeline.effectivePxPerUnit, 0.001f);
        }

        /// <summary>
        /// A caller that sets the bounds the wrong way round should get a pinned axis, not a clamp
        /// that inverts and reports a scale outside both bounds.
        /// </summary>
        [Test]
        public void InvertedZoomBounds_PinTheAxisRatherThanInverting()
        {
            var timeline = new Timeline { pxPerUnit = 8f, minPxPerUnit = 50f, maxPxPerUnit = 10f };

            timeline.zoom = 1000f;
            Assert.AreEqual(50f, timeline.effectivePxPerUnit, 0.001f);

            timeline.zoom = 0.0001f;
            Assert.AreEqual(50f, timeline.effectivePxPerUnit, 0.001f);
        }

        /// <summary>
        /// Off, the ruler draws whatever minorTick says, which turns to mush once a zoomed-out
        /// axis puts those ticks a pixel apart. On, the interval grows with the pixel density.
        /// </summary>
        [Test]
        public void AdaptiveTicks_WidenTheIntervalAsTheAxisZoomsOut()
        {
            var timeline = new Timeline { duration = 1000f, minorTick = 1f, majorTick = 5f };

            timeline.ResolveTicks(40f, out var minorNear, out _);
            Assert.AreEqual(1f, minorNear, 0.001f,
                "With adaptiveTicks off the configured interval is used as-is.");

            timeline.adaptiveTicks = true;
            timeline.ResolveTicks(40f, out var minorZoomedIn, out _);
            timeline.ResolveTicks(0.5f, out var minorZoomedOut, out _);

            Assert.Greater(minorZoomedOut, minorZoomedIn,
                "Zooming out must widen the tick interval, not keep drawing a pixel apart.");
        }

        /// <summary>
        /// A labelled tick at the same interval as the minor one would label every tick, which is
        /// the thing the major/minor split exists to avoid.
        /// </summary>
        [Test]
        public void AdaptiveTicks_KeepTheMajorIntervalAboveTheMinor()
        {
            var timeline = new Timeline { duration = 1000f, minorTick = 1f, majorTick = 5f, adaptiveTicks = true };

            foreach (var ppu in new[] { 0.1f, 0.5f, 2f, 8f, 40f, 200f })
            {
                timeline.ResolveTicks(ppu, out var minor, out var major);
                Assert.Greater(major, minor, $"Major collapsed onto minor at {ppu} px/unit.");
                Assert.Greater(minor, 0f);
            }
        }

        /// <summary>
        /// An adaptive ruler must still land on the beats the caller asked for. An interval that
        /// is not a whole multiple of minorTick drifts off them as the axis zooms.
        /// </summary>
        [Test]
        public void AdaptiveTicks_StayOnWholeMultiplesOfTheConfiguredInterval()
        {
            var timeline = new Timeline { duration = 1000f, minorTick = 0.25f, majorTick = 1f, adaptiveTicks = true };

            foreach (var ppu in new[] { 0.3f, 1.7f, 9f, 55f })
            {
                timeline.ResolveTicks(ppu, out var minor, out _);
                var multiples = minor / 0.25f;
                Assert.AreEqual(Mathf.Round(multiples), multiples, 0.001f,
                    $"Adaptive minor {minor} is not a whole multiple of the configured 0.25.");
            }
        }

        /// <summary>
        /// infinite implies adaptive: an axis with no end cannot have a fixed tick count.
        /// </summary>
        [Test]
        public void InfiniteAxis_GetsAdaptiveTicksWithoutAskingForThem()
        {
            var timeline = new Timeline { duration = 1000f, minorTick = 1f, majorTick = 5f, infinite = true };

            timeline.ResolveTicks(0.05f, out var minor, out _);
            Assert.Greater(minor, 1f,
                "An infinite axis must thin its ticks even with adaptiveTicks left off.");
        }

        [Test]
        public void NiceInterval_StepsThroughTheOneTwoFiveLadder()
        {
            // 10 px budget at 10 px/unit wants 1 unit exactly.
            Assert.AreEqual(1f, Timeline.NiceInterval(1f, 10f, 10f), 0.001f);
            // Same budget at 4 px/unit wants 2.5, which rounds up the ladder to 5.
            Assert.AreEqual(5f, Timeline.NiceInterval(1f, 4f, 10f), 0.001f);
            // A pixel density so low the interval has to jump an order of magnitude.
            Assert.AreEqual(20f, Timeline.NiceInterval(1f, 0.5f, 10f), 0.001f);
        }

        [Test]
        public void Keyframe_MutedTogglesItsClass()
        {
            var keyframe = new TimelineKeyframe();
            Assert.IsFalse(keyframe.muted);
            Assert.IsFalse(keyframe.ClassListContains(TimelineKeyframe.mutedUssClassName));

            keyframe.muted = true;
            Assert.IsTrue(keyframe.ClassListContains(TimelineKeyframe.mutedUssClassName));
        }

        /// <summary>
        /// A frame marks where something sits without claiming the lane, so the body has to let a
        /// click through and only the handle strip may drag it. A frame that still swallowed
        /// pointers would make the lane behind it unreachable.
        /// </summary>
        [Test]
        public void FrameClip_LetsPointersThroughItsBodyAndDragsOnlyByItsHandle()
        {
            var clip = new TimelineClip { start = 0f, duration = 4f };
            Assert.AreEqual(PickingMode.Position, clip.pickingMode);

            clip.frame = true;

            Assert.IsTrue(clip.ClassListContains(TimelineClip.frameUssClassName));
            Assert.AreEqual(PickingMode.Ignore, clip.pickingMode,
                "A frame's body must not swallow clicks meant for the lane behind it.");

            var handle = clip.Q(className: TimelineClip.dragHandleUssClassName);
            Assert.IsNotNull(handle);
            Assert.IsFalse(handle.ClassListContains(Styles.hiddenUssClassName));

            clip.frame = false;
            Assert.AreEqual(PickingMode.Position, clip.pickingMode,
                "A solid clip is its own drag surface and must take pointers again.");
            Assert.IsTrue(handle.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void FrameClip_DefaultsToPurpleAndSwapsItsHueClass()
        {
            var clip = new TimelineClip { frame = true };
            Assert.AreEqual(TimelineFrameColor.Purple, clip.frameColor);
            Assert.IsTrue(clip.ClassListContains(
                TimelineClip.GetFrameColorUssClassName(TimelineFrameColor.Purple)));

            clip.frameColor = TimelineFrameColor.Amber;
            Assert.IsTrue(clip.ClassListContains(
                TimelineClip.GetFrameColorUssClassName(TimelineFrameColor.Amber)));
            Assert.IsFalse(clip.ClassListContains(
                    TimelineClip.GetFrameColorUssClassName(TimelineFrameColor.Purple)),
                "Two hue classes at once would leave the winner up to import order.");
        }

        /// <summary>
        /// A tall frame must not turn its whole body into a drag surface, which is what an
        /// unclamped handle height would do.
        /// </summary>
        [Test]
        public void FrameHeight_CapsTheHandleStrip()
        {
            var clip = new TimelineClip { frame = true, frameHeight = 200f };
            var handle = clip.Q(className: TimelineClip.dragHandleUssClassName);

            Assert.AreEqual(200f, clip.style.height.value.value, 0.001f);
            Assert.AreEqual(TimelineClip.maxFrameHandleHeight, handle.style.height.value.value, 0.001f,
                "The handle must stop at its cap however tall the frame is.");

            clip.frameHeight = 10f;
            Assert.AreEqual(10f, handle.style.height.value.value, 0.001f,
                "A frame shorter than the cap gets a handle no taller than itself.");
        }

        /// <summary>
        /// The clip colour must come from the editor token, not the generic accent: a clip's
        /// identity should not change when the accent does.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void ClipFill_BindsToTheEditorClipToken()
        {
            var uss = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Timeline.uss"));
            var rule = Regex.Match(uss, @"\.appui-timeline__clip\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(rule.Success);
            StringAssert.Contains("background-color: var(--appui-alias-editor-clip)", rule.Groups["body"].Value,
                "A clip must paint itself with the editor clip token, not the generic accent.");
        }

        // ------------------------------------------------------------------ lane and fine-tick tokens

        /// <summary>
        /// The registry's lane shape reads the lane radius and rim width tokens, so a theme can round
        /// the lanes or drop their rim. The rim used to borrow the generic control border width.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_Lane_ReadsTheLaneShapeTokens()
        {
            var block = UssSource.RuleBlock(UssSource.Read("Timeline.uss"), ".appui-timeline__track-background");
            Assert.IsNotNull(block, "Timeline.uss has no lane rule.");
            StringAssert.Contains("border-radius: var(--appui-timeline-lane-border-radius)", block);
            StringAssert.Contains("border-top-width: var(--appui-timeline-lane-border-width)", block);
            StringAssert.Contains("border-bottom-width: var(--appui-timeline-lane-border-width)", block);
            Assert.IsFalse(block.Contains("--appui-alias-control-border-width"),
                "The lane rim must come from the lane token, not the generic control border width.");
        }

        /// <summary>
        /// UI Toolkit only surfaces a custom property a matched rule declares on the element, so the
        /// root rule has to republish the theme's fine-tick opacity for the C# to read it.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_Root_RepublishesTheFineTickOpacity()
        {
            var block = UssSource.RuleBlock(UssSource.Read("Timeline.uss"), ".appui-timeline");
            Assert.IsNotNull(block, "Timeline.uss has no root rule.");
            StringAssert.Contains("--timeline-tick-fine-opacity: var(--appui-timeline-tick-fine-opacity)", block);
        }

        // At 4 px/unit with minorTick 1, adaptive ticks resolve the minor step to 2, and the fine
        // level below it is 1: 4 px apart against an 8 px budget, so it fades in at
        // (0.5 - 0.35) / 0.65 of the peak.
        const float k_FinePxPerUnit = 4f;
        const float k_FineAlpha = (0.5f - 0.35f) / 0.65f;

        static Timeline BuildAdaptiveTimeline() => new Timeline
        {
            duration = 20f, pxPerUnit = k_FinePxPerUnit, minorTick = 1f, majorTick = 5f, adaptiveTicks = true,
            style = { width = 800, height = 160 },
        };

        List<VisualElement> RulerTicks(Timeline timeline) =>
            timeline.ruler.Query<VisualElement>(className: TimelineRuler.tickUssClassName).ToList();

        /// <summary>
        /// App UI's fine-tick opacity is 0, and the registry then generates no finer level at all.
        /// Only the minor ticks are built, every one fully opaque.
        /// </summary>
        [UnityTest]
        public IEnumerator FineTicks_AreNotGeneratedAtTheDefaultOpacity()
        {
            var timeline = BuildAdaptiveTimeline();
            yield return Mount(timeline);

            Assert.AreEqual(0f, timeline.tickFineOpacity, 1e-4f, "App UI's theme sets no fine tick level.");

            var ticks = RulerTicks(timeline);
            Assert.AreEqual(11, ticks.Count, "Only the minor ticks, every 2 units from 0 to 20.");
            foreach (var tick in ticks)
                Assert.AreEqual(1f, tick.resolvedStyle.opacity, 1e-4f, "A minor tick is never faded.");
        }

        /// <summary>
        /// With a non-zero opacity the level below the minor step joins between the minor ticks,
        /// faded by how far apart its ticks are, while the minor ticks themselves stay opaque.
        /// </summary>
        [UnityTest]
        public IEnumerator FineTicks_FadeInBetweenTheMinorTicks()
        {
            var timeline = BuildAdaptiveTimeline();
            yield return Mount(timeline);

            timeline.tickFineOpacity = 0.6f;
            yield return null;
            yield return null;

            var ticks = RulerTicks(timeline);
            Assert.AreEqual(21, ticks.Count, "11 minor ticks plus 10 fine ones on the odd units.");

            foreach (var tick in ticks)
            {
                var t = Mathf.Round((tick.resolvedStyle.left - Timeline.origin) / k_FinePxPerUnit);
                // Not Mathf.Approximately: on 6000.7 it reports 0 and 0 as different.
                var expected = (int)t % 2 == 0 ? 1f : 0.6f * k_FineAlpha;
                Assert.AreEqual(expected, tick.resolvedStyle.opacity, 1e-3f, $"The tick at t={t} has the wrong opacity.");
            }

            timeline.adaptiveTicks = false;
            yield return null;
            yield return null;

            ticks = RulerTicks(timeline);
            Assert.AreEqual(21, ticks.Count, "Fixed ticks: one per unit, no fine level.");
            Assert.IsTrue(ticks.All(t => Mathf.Approximately(t.resolvedStyle.opacity, 1f)),
                "The fine level exists only on an adaptive axis.");
        }

        static List<float> CollectTickTimes(float duration, float minorTick, float pxPerUnit)
        {
            var timeline = new Timeline
            {
                duration = duration, minorTick = minorTick, majorTick = minorTick * 10f, adaptiveTicks = true,
                tickFineOpacity = 0.6f,
            };
            timeline.ResolveTicks(pxPerUnit, out var minor, out var major);
            var axis = new TimelineAxisContext(timeline, duration, pxPerUnit, Timeline.origin, 0f, 0f,
                minor, major, 24f, 24f, false, null);
            var ticks = new List<TimelineTick>();
            Timeline.CollectTicks(axis, ticks);
            return ticks.Select(t => t.time).ToList();
        }

        /// <summary>
        /// A fine tick that lands on a minor tick is the same tick. Deciding that from the float
        /// quotient lets rounding through on a fractional axis, which built the tick twice and
        /// stroked its grid line twice.
        /// </summary>
        [Test]
        public void FineTicks_NeverDuplicateAMinorTick_OnAFractionalAxis()
        {
            // minorTick 0.1 at 40 px/unit resolves to a 0.2 minor step and a 0.1 fine level.
            var times = CollectTickTimes(350f, 0.1f, 40f);
            var distinct = times.Select(t => Mathf.RoundToInt(t * 1000f)).Distinct().Count();
            Assert.AreEqual(times.Count, distinct, "Some fine ticks repeat a minor tick.");
            Assert.AreEqual(3501, times.Count, "1751 minor ticks plus the 1750 fine ones between them.");
        }

        /// <summary>
        /// The tick limit counts the ticks actually drawn. The fine level's own count includes the
        /// positions it shares with the minor ticks, and counting those dropped a fine level that fits.
        /// </summary>
        [Test]
        public void FineTicks_CountOnlyDistinctTicks_AgainstTheLimit()
        {
            // 4501 minor ticks every 2 units plus 4500 fine ones on the odd units: 9001, under the limit.
            var times = CollectTickTimes(9000f, 1f, k_FinePxPerUnit);
            Assert.AreEqual(9001, times.Count, "The fine level fits under the limit and must be drawn.");
        }

#if UNITY_EDITOR
        /// <summary>
        /// The opacity comes from the stylesheet, not just from the internal setter: a theme that sets
        /// the fine-tick variable on the timeline gets the fine level.
        /// </summary>
        [UnityTest]
        public IEnumerator FineTicks_ReadTheirOpacityFromTheStylesheet()
        {
            const string path = "Assets/W1cTimelineFineTickProbe.uss";
            System.IO.File.WriteAllText(path,
                ".appui-timeline.fine-tick-probe { --timeline-tick-fine-opacity: 0.6; }\n");
            try
            {
                UnityEditor.AssetDatabase.ImportAsset(path, UnityEditor.ImportAssetOptions.ForceSynchronousImport);
                var sheet = UnityEditor.AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
                Assert.IsNotNull(sheet, "The probe stylesheet failed to import.");

                var timeline = BuildAdaptiveTimeline();
                timeline.AddToClassList("fine-tick-probe");
                timeline.styleSheets.Add(sheet);
                yield return Mount(timeline);

                Assert.AreEqual(0.6f, timeline.tickFineOpacity, 1e-3f,
                    "The Timeline must read --timeline-tick-fine-opacity off its resolved style.");
                Assert.AreEqual(21, RulerTicks(timeline).Count);
            }
            finally
            {
                UnityEditor.AssetDatabase.DeleteAsset(path);
            }
        }
#endif

    }
}
