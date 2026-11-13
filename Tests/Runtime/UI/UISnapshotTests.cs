#if APPUI_UITF_PINNED_PANEL
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.AppUI.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using UnityEngine.UIElements.TestFramework;
using AppUIPanel = Unity.AppUI.UI.Panel;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// The Unity 7 story and snapshot harness, built on <c>com.unity.ui.test-framework</c> instead
    /// of a hand-managed <c>UIDocument</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This runs alongside <see cref="VisualElementTests{T}"/> rather than replacing it, and only
    /// on an editor whose bundled <c>com.unity.ui.test-framework</c> is 6.7 or newer -- which is
    /// what <c>APPUI_UITF_PINNED_PANEL</c> means. That version is the first to carry
    /// <i>"Fixed runtime panel tests depending on the game view or display of the host machine: the
    /// fixture now pins the panel's resolved size and scale to <c>panelSize</c>"</i> (UUM-122592,
    /// UUM-122593). Before it, a framework-driven runtime panel inherits size and scale from the
    /// host display, which is strictly worse for pixel comparison than the explicit
    /// <c>Screen.SetResolution</c> the old harness does. So the gate is on the package version, not
    /// the editor version, and it opens by itself once the fix ships.
    /// </para>
    /// <para>
    /// Captures go to the folder named for the running editor, under the same story file names
    /// <see cref="VisualElementTests{T}"/> uses, because there is exactly one set of captures per
    /// editor: a component with a fixture here sets
    /// <c>VisualElementTests.capturesSnapshotsElsewhere</c> so the old harness stands down for it
    /// rather than overwriting these. Two editors' folders can then be compared image for image
    /// for the same set of tests.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The component under test.</typeparam>
    class UISnapshotTests<T> : UITestFixture where T : VisualElement, new()
    {
        protected virtual string componentName => typeof(T).Name;

        /// <inheritdoc cref="VisualElementTests{T}.StoryContext"/>
        protected record StoryContext(AppUIPanel panel, VisualElement container)
        {
            public AppUIPanel panel { get; } = panel;

            public VisualElement container { get; } = container;
        }

        /// <inheritdoc cref="VisualElementTests{T}.Story"/>
        protected record Story(string name, Func<StoryContext, IEnumerator> run)
        {
            public Story(string name, Func<StoryContext, VisualElement> setup)
                : this(name, ctx => Mount(ctx, setup)) { }

            public string name { get; } = name;

            public Func<StoryContext, IEnumerator> run { get; } = run;

            static IEnumerator Mount(StoryContext ctx, Func<StoryContext, VisualElement> setup)
            {
                ctx.container.Add(setup(ctx));
                yield break;
            }
        }

        /// <inheritdoc cref="VisualElementTests{T}.hasDefaultStory"/>
        protected virtual bool hasDefaultStory => true;

        protected virtual IEnumerable<Story> stories
        {
            get
            {
                if (hasDefaultStory)
                    yield return new Story("Default", _ => new T());
            }
        }

        /// <summary>
        /// The pinned panel size. Matches the resolution the old harness asks of
        /// <c>Screen.SetResolution</c>, so a capture from either path frames the story the same way.
        /// </summary>
        protected virtual Vector2 snapshotPanelSize => new Vector2(1200, 600);

        /// <inheritdoc cref="AbstractUITestFixture.FixtureUnitySetUp"/>
        public override IEnumerator FixtureUnitySetUp()
        {
            yield return base.FixtureUnitySetUp();
            yield return Utils.WaitForLocalizationPreloaded();
#if !UNITY_EDITOR
            // panelSize pins the UITK panel, not the framebuffer, and ScreenCapture grabs the
            // framebuffer. Without this the player runs at its native resolution and the captures
            // come out a different size from the old harness's, which cannot then be compared
            // image for image. Same call, same size the old harness uses.
            ApplyCaptureResolution();
            yield return null;
#endif
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Sizes the player's framebuffer to <see cref="snapshotPanelSize"/>, so that what
        /// <c>ScreenCapture</c> writes matches the pinned panel and the old harness's captures.
        /// </summary>
        void ApplyCaptureResolution()
        {
            Screen.SetResolution(
                Mathf.RoundToInt(snapshotPanelSize.x),
                Mathf.RoundToInt(snapshotPanelSize.y),
                FullScreenMode.Windowed);
        }
#endif

        /// <inheritdoc cref="AbstractUITestFixture.FixtureSetUp"/>
        /// <remarks>
        /// Panel configuration belongs here, not in <see cref="FixtureUnitySetUp"/>: <c>[UnitySetUp]</c>
        /// runs while the fixture is still merely initialised, and the simulator rejects
        /// during-test operations until <c>FixtureSetUp</c> has run.
        /// </remarks>
        public override void FixtureSetUp()
        {
            base.FixtureSetUp();

            panelSize = snapshotPanelSize;
            pixelsPerPoint = 1f;
            themeStyleSheet = Utils.appUiThemeStyleSheet;
            debugMode = false;

            // A snapshot is worthless if nothing repaints, which is the one case the framework's
            // Editor panel does not cover. In a player run AutoDetect gives a runtime panel, which
            // does render; needsRendering makes the intent explicit either way.
            needsRendering = true;
        }

        [UnityTest]
        public IEnumerator CreateSnapshot()
        {
            var capture = !Application.isEditor && !string.IsNullOrEmpty(Utils.snapshotsOutputDir);

            var any = false;
            foreach (var story in stories)
            {
                any = true;
                foreach (var theme in Utils.themes)
                {
                    foreach (var density in Utils.densities)
                    {
                        foreach (var dir in Enum.GetValues(typeof(Dir)))
                        {
                            yield return CreateSnapshotInternal(story, theme, density, (Dir)dir, capture);
                        }
                    }
                }
            }

            if (!any)
                Assert.Ignore("This fixture declares no story: it opted out of the bare default capture.");
        }

        /// <summary>
        /// Refuses a story that lays out to nothing, the same contract the <c>UIDocument</c>
        /// harness enforces.
        /// </summary>
        /// <remarks>
        /// A blank capture is a file that can never fail, and nothing downstream would report it:
        /// the image is written, the test passes, and the component goes uncovered. Migrating a
        /// fixture to this harness must not lose the guard.
        /// </remarks>
        void AssertStoryIsNotBlank(Story story, VisualElement container)
        {
            var area = 0f;
            foreach (var child in container.Children())
            {
                var bounds = child.worldBound;
                if (float.IsNaN(bounds.width) || float.IsNaN(bounds.height))
                    continue;
                area = Mathf.Max(area, bounds.width * bounds.height);
            }

            Assert.Greater(area, 0f,
                $"`{componentName}.{story.name}` lays out to nothing, so its capture is blank. "
                + "Give the story content or a host, or declare no story at all if the component "
                + "genuinely paints nothing.");
        }

        IEnumerator CreateSnapshotInternal(Story story, string theme, string density, Dir dir, bool capture)
        {
#if !UNITY_EDITOR
            // Re-asserted per story, the way the old harness does: a resolution change is honoured
            // asynchronously and anything else in the run may have moved it since.
            ApplyCaptureResolution();
#endif

            // A fresh panel per story, so nothing from the previous theme or direction leaks in.
            RecreatePanel();
            panelSize = snapshotPanelSize;
            pixelsPerPoint = 1f;
            themeStyleSheet = Utils.appUiThemeStyleSheet;

            var appUiPanel = new AppUIPanel
            {
                theme = theme,
                density = density,
                layoutDirection = dir,
                lang = "en",
            };
            rootVisualElement.Add(appUiPanel);
            // Same reason as the UIDocument harness: an absolutely positioned story is framed
            // against the panel, so the panel has to carry the pinned size rather than hug its
            // content.
            appUiPanel.StretchToParentSize();

            var container = new VisualElement
            {
                style =
                {
                    flexGrow = 1,
                    paddingBottom = 10,
                    paddingLeft = 10,
                    paddingRight = 10,
                    paddingTop = 10,
                }
            };
            appUiPanel.Add(container);

            // A story yields to ask for a frame, and a frame here is an updater pass rather than an
            // engine frame -- which is why a story may only yield `null`.
            var routine = story.run(new StoryContext(appUiPanel, container));
            while (routine.MoveNext())
            {
                Assert.IsNull(routine.Current,
                    $"Story `{story.name}` yielded {routine.Current}. Under this harness a story may "
                    + "only yield null, which asks for one updater pass.");
                simulate.FrameUpdate();
            }

            // FrameUpdate runs the real updater pass, so three of them settle layout and styles
            // deterministically -- no guessing at a frame count, which is what the old harness's
            // `yield return null` loop is doing.
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            AssertStoryIsNotBlank(story, container);

            if (!capture)
                yield break;

            // The framework has no capture of its own, so this stays on ScreenCapture, writing the
            // same filename shape the visual-regression job's artifact glob already collects.
            yield return new WaitForEndOfFrame();
            var outputFilePath = Utils.SnapshotOutputPath(
                $"{componentName}.{story.name}.{theme}.{dir.ToString().ToLower()}.{density}.png");
            ScreenCapture.CaptureScreenshot(outputFilePath);
            yield return null;
            yield return new WaitUntilOrTimeOut(
                () => Utils.FileAvailable(outputFilePath),
                false,
                TimeSpan.FromSeconds(3));
            Assert.IsTrue(Utils.FileAvailable(outputFilePath));
        }
    }
}
#endif
