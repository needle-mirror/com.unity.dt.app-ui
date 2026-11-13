using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Unity.AppUI.Tests.UI
{
    [PrebuildSetup(typeof(ThemeResourceSetup))]
    [PostBuildCleanup(typeof(ThemeResourceSetup))]
    class VisualElementTests<T> where T : VisualElement, new()
    {
        VisualElement m_VisualElement;

        protected virtual string componentName => typeof(T).Name;

        protected virtual string mainUssClassName => null;

        protected virtual bool uxmlConstructable => true;

        protected virtual IEnumerable<string> uxmlTestCases
        {
            get { yield return "<" + uxmlNamespaceName + ":" + componentName + " />"; }
        }

        protected virtual string uxmlNamespaceName => "appui";

        /// <summary>
        /// What a story is handed when it runs.
        /// </summary>
        /// <remarks>
        /// <see cref="container"/> is the element the capture frames: it fills the themed
        /// <see cref="panel"/>, so a story whose component is absolutely positioned, capped to
        /// <c>max-height: 100%</c> or clipped against its parent -- Toaster, FloatingWindow,
        /// TweakMenu -- is measured against the panel's real size rather than a zero-height box.
        /// </remarks>
        protected record StoryContext(UIDocument document, Panel panel, VisualElement container)
        {
            public UIDocument document { get; } = document;

            public Panel panel { get; } = panel;

            public VisualElement container { get; } = container;
        }

        /// <summary>
        /// One named capture case.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A story is a coroutine, not a factory, because a good share of the states worth
        /// capturing cannot be built before the element is in a panel: layout-dependent setup,
        /// a generated row to select, an overlay to open, focus, a pointer state. It mounts
        /// whatever it wants into <see cref="StoryContext.container"/>, yields for as many frames
        /// as it needs, establishes the state, and returns once the frame is worth capturing.
        /// </para>
        /// <para>
        /// A story that needs none of that passes a plain factory instead and the wrapper mounts
        /// its return value. Yield <c>null</c> to ask for a frame; that is the one yield the
        /// <c>com.unity.ui.test-framework</c> counterpart in <c>UISnapshotTests</c> can honour,
        /// since a frame there is an explicit updater pass rather than an engine frame.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Yields until <paramref name="settled"/> holds, and fails the test if it never does.
        /// </summary>
        /// <remarks>
        /// The bound is wall-clock rather than a frame count on purpose. A headless frame is a
        /// fraction of a millisecond, so a plausible-looking number of them does not add up to a
        /// 150ms transition -- which is how a story ends up photographing a reveal at 2% opacity.
        /// The timeout is an error rather than a quiet return: returning on it would capture the
        /// unsettled frame and let the story pass anyway.
        /// </remarks>
        protected static IEnumerator WaitUntil(Func<bool> settled, float timeoutSeconds = 2f)
        {
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;
            while (!settled() && Time.realtimeSinceStartup < deadline)
                yield return null;

            Assert.IsTrue(settled(),
                $"The awaited condition still did not hold after {timeoutSeconds}s, so the frame "
                + "about to be captured is not the state this story is named for.");
        }

        /// <summary>
        /// Yields until <paramref name="read"/> has both moved away from <paramref name="from"/>
        /// and stopped changing, or the timeout passes.
        /// </summary>
        /// <remarks>
        /// For a transition whose end value the story has no name for -- a themed colour, say.
        /// Waiting only for the value to differ would return on the transition's first frame,
        /// which is as wrong as not waiting at all. A timeout is an error rather than a quiet
        /// return: a transition that merely runs longer than the bound would otherwise be
        /// photographed mid-flight, and a "the value moved" assertion in the story would still
        /// hold on that frame.
        /// </remarks>
        protected static IEnumerator WaitForTransition<TValue>(
            Func<TValue> read,
            TValue from,
            float timeoutSeconds = 2f)
        {
            const int stableFrames = 3;
            var last = from;
            var stable = 0;
            var deadline = Time.realtimeSinceStartup + timeoutSeconds;

            while (stable < stableFrames && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
                var now = read();
                stable = !EqualityComparer<TValue>.Default.Equals(now, from)
                    && EqualityComparer<TValue>.Default.Equals(now, last)
                        ? stable + 1
                        : 0;
                last = now;
            }

            Assert.GreaterOrEqual(stable, stableFrames,
                $"The transition from `{from}` had not both moved and held still for "
                + $"{stableFrames} frames after {timeoutSeconds}s (last read `{last}`), so the "
                + "frame about to be captured is mid-transition.");
        }

        /// <summary>
        /// Whether this fixture inherits the bare <c>new T()</c> capture as its <c>Default</c> story.
        /// </summary>
        /// <remarks>
        /// Set it to <c>false</c> for a component that paints nothing without content or a host --
        /// an empty container, a layout spacer, a base class, a control that rests at zero opacity.
        /// A blank capture is not a regression test; it is a file that can never fail.
        /// </remarks>
        protected virtual bool hasDefaultStory => true;

        protected virtual IEnumerable<Story> stories
        {
            get
            {
                if (hasDefaultStory)
                    yield return new Story("Default", (ctx) => new T());
            }
        }

        /// <summary>
        /// Set to <c>true</c> by a component whose snapshots are taken by the
        /// com.unity.ui.test-framework harness instead, via a <c>UISnapshotTests</c> fixture.
        /// </summary>
        /// <remarks>
        /// Captures are filed under a folder named for the running editor, so exactly one harness
        /// must capture a given component on a given editor or the two would overwrite each other.
        /// This is what hands a component over, and it only takes effect on an editor where the
        /// framework path compiles at all; below that the framework fixture does not exist and
        /// this harness keeps capturing everything. Migrating the last component lets the whole
        /// story and snapshot half of this class go.
        /// </remarks>
        protected virtual bool capturesSnapshotsElsewhere => false;

        /// <summary>
        /// Whether this fixture contributes captures at all.
        /// </summary>
        /// <remarks>
        /// <c>false</c> for a fixture whose stories exist to be asserted on rather than
        /// photographed -- the harness's own tests, for instance.
        /// </remarks>
        protected virtual bool capturesSnapshots => true;

        protected T element => m_VisualElement as T;

        protected UIDocument m_TestUI;

        protected Panel m_Panel;

        protected bool m_SetupDone;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!m_SetupDone)
            {
                // Load new scene
                var scene = SceneManager.CreateScene("ComponentTestScene-" + Random.Range(1, 1000000));
                while (!SceneManager.SetActiveScene(scene))
                {
                    yield return null;
                }
                yield return Utils.WaitForLocalizationPreloaded();
                m_TestUI = Utils.ConstructTestUI();
                Screen.SetResolution(1200, 600, FullScreenMode.Windowed);
                TimeUtils.timeOverride = 1f;
            }
            m_TestUI.rootVisualElement.Clear();
            m_SetupDone = true;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            TimeUtils.timeOverride = null;
            if (m_TestUI)
                Object.Destroy(m_TestUI.gameObject);

            m_TestUI = null;
            m_SetupDone = false;
#pragma warning disable CS0618
            SceneManager.UnloadScene(SceneManager.GetActiveScene());
#pragma warning restore CS0618
        }

        [Test]
        [Order(1)]
        public void Constructor_ShouldSucceed()
        {
            m_VisualElement = null;
            Assert.DoesNotThrow(() => m_VisualElement = new T());
            Assert.IsNotNull(m_VisualElement);
            if (!string.IsNullOrEmpty(mainUssClassName))
                Assert.IsTrue(m_VisualElement.ClassListContains(mainUssClassName));
        }

        [UnityTest]
        [Order(2)]
        [ConditionalIgnore("IgnoreInPlayer", "UXML construction is supported only in Editor")]
        public IEnumerator UxmlConstruction_ShouldSucceed()
        {
            if (!uxmlConstructable)
            {
                Assert.Ignore("UXML construction not supported for this type");
                yield break;
            }

            var cases = uxmlTestCases;

            if (cases == null)
            {
                Assert.Ignore("No UXML test cases defined");
                yield break;
            }

            foreach (var uxmlTestCase in cases)
            {
                Assert.DoesNotThrow(() =>
                {
                    var content = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:appui=\"Unity.AppUI.UI\" xmlns:nav=\"Unity.AppUI.Navigation\" >" + uxmlTestCase + "</ui:UXML>";
                    var asset = Utils.LoadUxmlTemplateFromString(content);
                    m_TestUI.visualTreeAsset = asset;
                });

                yield return null;
            }
        }

        [UnityTest]
        [Order(3)]
        public IEnumerator CreateSnapshot()
        {
#if APPUI_UITF_PINNED_PANEL
            if (capturesSnapshotsElsewhere)
            {
                Assert.Ignore("Snapshots for this component are taken by the com.unity.ui.test-framework harness on this editor.");
                yield break;
            }
#endif
            if (!capturesSnapshots)
            {
                Assert.Ignore("This fixture's stories exist to be asserted on, not captured.");
                yield break;
            }

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
        /// Mounts one named story exactly the way <see cref="CreateSnapshot"/> does, then hands its
        /// context to <paramref name="assert"/>.
        /// </summary>
        /// <remarks>
        /// A capture asserts nothing -- in the Editor it does not even write a file -- so a story
        /// whose state is established after attachment has no coverage at all unless a test runs it
        /// and checks the result. This is what a fixture uses to counter-check its own stories.
        /// </remarks>
        /// <param name="storyName">The <see cref="Story.name"/> to run.</param>
        /// <param name="assert">Runs once the story has finished and the tree has settled.</param>
        protected IEnumerator RunStory(string storyName, Action<StoryContext> assert)
        {
            Story story = null;
            foreach (var candidate in stories)
            {
                if (candidate.name != storyName)
                    continue;
                story = candidate;
                break;
            }

            Assert.IsNotNull(story, $"No story named `{storyName}` on {GetType().Name}.");

            StoryContext context = null;
            yield return MountStory(story, "dark", "normal", Dir.Ltr, ctx => context = ctx);

            assert(context);
        }

        IEnumerator MountStory(Story story, string theme, string density, Dir dir, Action<StoryContext> mounted)
        {
#if !UNITY_EDITOR
            Screen.SetResolution(1200, 600, FullScreenMode.Windowed);
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            var panel = new Panel
            {
                theme = theme,
                density = density,
                layoutDirection = dir,
                lang = "en",
            };
            m_TestUI.rootVisualElement.Clear();

            m_TestUI.rootVisualElement.Add(panel);
            // The panel, and with it the capture container, has to carry the screen's size. Left to
            // flow it is only as tall as its content, and an absolutely positioned story -- which
            // contributes no layout -- would be framed against nothing.
            panel.StretchToParentSize();
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
            panel.Add(container);
            var context = new StoryContext(m_TestUI, panel, container);
            yield return story.run(context);
            yield return null;
            const int waitFrames = 3;
            for (var i = 0; i < waitFrames; i++)
            {
                yield return null;
            }

            AssertStoryIsNotBlank(story, container);

            mounted?.Invoke(context);
        }

        /// <summary>
        /// Fails a story that lays out to nothing.
        /// </summary>
        /// <remarks>
        /// A blank capture is not a regression test, it is a file that can never fail, and nothing
        /// else in the pipeline would ever tell you: the image is written, the test passes, and the
        /// component goes uncovered. Rather than auditing the fixtures once and letting the next
        /// empty container slip back in, the harness refuses the capture. A component that really
        /// does paint nothing at any size -- a layout spacer, a base class -- says so through
        /// <see cref="hasDefaultStory"/> instead.
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
                + "Give the story content or a host, or set hasDefaultStory to false if the "
                + "component genuinely paints nothing.");
        }

        IEnumerator CreateSnapshotInternal(Story story, string theme, string density, Dir dir, bool capture)
        {
            yield return MountStory(story, theme, density, dir, null);

            if (capture)
            {
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
}
