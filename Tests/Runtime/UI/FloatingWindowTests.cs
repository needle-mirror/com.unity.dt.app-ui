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
    [TestOf(typeof(FloatingWindow))]
    class FloatingWindowTests : VisualElementTests<FloatingWindow>
    {
        protected override string mainUssClassName => FloatingWindow.ussClassName;

        // Handed over to FloatingWindowSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/FloatingWindow.uss";

        /// <summary>
        /// An in-memory <see cref="IWindowLayoutStorage"/>, so the tests never touch PlayerPrefs.
        /// </summary>
        class MemoryStorage : IWindowLayoutStorage
        {
            readonly Dictionary<string, string> m_Values = new Dictionary<string, string>();

            public int writes { get; private set; }

            public string GetItem(string key) => m_Values.TryGetValue(key, out var v) ? v : null;

            public void SetItem(string key, string value)
            {
                m_Values[key] = value;
                writes++;
            }

            public void RemoveItem(string key) => m_Values.Remove(key);
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Open", (ctx) => Build(true));
                yield return new Story("AutoSize", (ctx) =>
                {
                    var w = Build(true);
                    w.autoSize = true;
                    return w;
                });
            }
        }

        static FloatingWindow Build(bool open)
        {
            var window = new FloatingWindow
            {
                title = "Inspector",
                storage = new MemoryStorage(),
                storageKey = "test-window",
                defaultWidth = 360f,
                defaultHeight = 240f,
            };
            window.Add(new Text { text = "Content" });
            window.open = open;
            return window;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:FloatingWindow />",
            @"<appui:FloatingWindow title=""Inspector"" storage-key=""inspector"" open=""true"" resizable=""false"" />",
            @"<appui:FloatingWindow open=""true""><appui:Text text=""Body"" /></appui:FloatingWindow>",
            // Every attribute the UI Kit sample sets, so a rename cannot break the sample silently.
            @"<appui:FloatingWindow title=""Console"" storage-key=""c"" open=""true"" resizable=""false"" dismissable=""false"" default-width=""260"" default-height=""140"" min-width=""160"" min-height=""120"" auto-size=""false"" />",
        };

        [Test]
        public void Defaults_MatchTheRegistry()
        {
            var window = new FloatingWindow();
            Assert.IsFalse(window.open, "A FloatingWindow must start closed.");
            Assert.AreEqual(FloatingWindow.defaultWindowWidth, window.defaultWidth);
            Assert.AreEqual(FloatingWindow.defaultWindowHeight, window.defaultHeight);
            Assert.AreEqual(FloatingWindow.defaultMinWidth, window.minWidth);
            Assert.AreEqual(FloatingWindow.defaultMinHeight, window.minHeight);
            Assert.IsTrue(window.resizable);
            Assert.IsFalse(window.autoSize);
            Assert.IsTrue(window.dismissable);
            Assert.AreEqual("floating-window-layout", window.storageKey);
        }

        [Test]
        public void Storage_DefaultsToPlayerPrefsAndAcceptsAnInjectedStore()
        {
            var window = new FloatingWindow();
            Assert.AreSame(PlayerPrefsWindowLayoutStorage.instance, window.storage,
                "The default backend must be the platform-native PlayerPrefs store.");

            var memory = new MemoryStorage();
            window.storage = memory;
            Assert.AreSame(memory, window.storage);

            window.storage = null;
            Assert.AreSame(PlayerPrefsWindowLayoutStorage.instance, window.storage,
                "Clearing the store must fall back to the default, not null out persistence.");
        }

        [Test]
        public void Content_LandsInTheBodyNotInTheTitleBar()
        {
            var window = new FloatingWindow();
            var child = new VisualElement();
            window.Add(child);

            Assert.AreEqual(1, window.childCount);
            Assert.IsTrue(child.hierarchy.parent.ClassListContains(FloatingWindow.bodyUssClassName),
                "Content must be added to the window body, not to the root beside the header.");
        }

        [UnityTest]
        public IEnumerator Closed_IsNotDisplayed_AndOpeningShowsIt()
        {
            var window = Build(false);
            yield return Mount(window);

            Assert.AreEqual(DisplayStyle.None, window.resolvedStyle.display);

            var raised = 0;
            var last = false;
            window.openChanged += v => { raised++; last = v; };

            window.open = true;
            for (var i = 0; i < 4; i++)
                yield return null;

            Assert.AreEqual(1, raised);
            Assert.IsTrue(last);
            Assert.AreEqual(DisplayStyle.Flex, window.resolvedStyle.display);
        }

        [UnityTest]
        public IEnumerator Opening_WithNothingPersisted_UsesTheDefaultGeometry()
        {
            var window = Build(true);
            yield return Mount(window);

            Assert.IsTrue(window.layoutResolved);
            Assert.AreEqual(360f, window.layoutGeometry.w, 1f);
            Assert.AreEqual(240f, window.layoutGeometry.h, 1f);
            Assert.AreEqual(360f, window.resolvedStyle.width, 1f);
            Assert.AreEqual(240f, window.resolvedStyle.height, 1f);
            Assert.GreaterOrEqual(window.layoutGeometry.x, 0f);
            Assert.GreaterOrEqual(window.layoutGeometry.y, 0f);
        }

        [UnityTest]
        public IEnumerator SavedGeometry_IsRestoredOnTheNextOpen()
        {
            var storage = new MemoryStorage();
            var first = new FloatingWindow
            {
                storage = storage,
                storageKey = "restore-me",
                defaultWidth = 300f,
                defaultHeight = 200f,
                minWidth = 100f,
                minHeight = 100f,
            };
            first.open = true;
            yield return Mount(first);

            first.SaveLayout();
            Assert.Greater(storage.writes, 0, "SaveLayout wrote nothing to the injected store.");

            var second = new FloatingWindow
            {
                storage = storage,
                storageKey = "restore-me",
                defaultWidth = 999f,
                defaultHeight = 999f,
                minWidth = 100f,
                minHeight = 100f,
            };
            second.open = true;
            yield return Mount(second);

            Assert.AreEqual(first.layoutGeometry.w, second.layoutGeometry.w, 1f,
                "The persisted width was not restored; the defaults won instead.");
            Assert.AreEqual(first.layoutGeometry.h, second.layoutGeometry.h, 1f);
            Assert.AreEqual(first.layoutGeometry.x, second.layoutGeometry.x, 1f);
            Assert.AreEqual(first.layoutGeometry.y, second.layoutGeometry.y, 1f);
        }

        [UnityTest]
        public IEnumerator RestoredGeometry_IsReClampedIntoTheCurrentContainer()
        {
            var storage = new MemoryStorage();
            storage.SetItem("offscreen", JsonUtility.ToJson(new WindowLayout { x = 9000f, y = 9000f, w = 300f, h = 200f }));

            var window = new FloatingWindow { storage = storage, storageKey = "offscreen" };
            window.minWidth = 100f;
            window.minHeight = 100f;
            window.open = true;
            yield return Mount(window);

            var container = window.hierarchy.parent.contentRect;
            Assert.LessOrEqual(window.layoutGeometry.x, container.width,
                "A layout saved on a bigger screen came back off-screen.");
            Assert.LessOrEqual(window.layoutGeometry.y, container.height);
            Assert.AreEqual(container.width - window.layoutGeometry.w, window.layoutGeometry.x, 1f,
                "The window must be clamped by its own size, not left as a sliver at the edge.");
        }

        [UnityTest]
        public IEnumerator FixedSizeWindow_IsReClampedWhenOnlyTheContainerShrinks()
        {
            var storage = new MemoryStorage();
            storage.SetItem("corner", JsonUtility.ToJson(new WindowLayout { x = 400f, y = 300f, w = 200f, h = 150f }));

            var window = new FloatingWindow { storage = storage, storageKey = "corner" };
            window.minWidth = 100f;
            window.minHeight = 100f;
            window.open = true;
            // Big enough to hold the saved 400,300 + 200x150 box without clamping, and pinned so
            // the starting position does not depend on the display's DPI scale.
            yield return Mount(window, hostSize: new Vector2(640f, 480f));

            var container = window.hierarchy.parent;
            Assert.AreEqual(400f, window.layoutGeometry.x, 1f, "The window did not start where it was saved.");
            Assert.AreEqual(300f, window.layoutGeometry.y, 1f);

            // flexGrow is what the host uses to fill the panel, so it has to go before an explicit
            // size means anything.
            container.style.flexGrow = 0f;
            container.style.width = 320f;
            container.style.height = 240f;
            for (var i = 0; i < 6; i++)
                yield return null;

            var size = container.contentRect.size;
            Assert.AreEqual(320f, size.x, 1f, "The container did not actually shrink.");
            // The window's own box never changes here, so it gets no GeometryChangedEvent of its
            // own until the clamp below moves it: only watching the container catches this.
            Assert.LessOrEqual(window.layoutGeometry.x + window.layoutGeometry.w, size.x + 1f,
                "A shrinking container left the window hanging off the right edge.");
            Assert.LessOrEqual(window.layoutGeometry.y + window.layoutGeometry.h, size.y + 1f,
                "A shrinking container left the window hanging off the bottom edge.");
        }

        [UnityTest]
        public IEnumerator RestoredGeometry_IsRaisedToTheMinimumSize()
        {
            var storage = new MemoryStorage();
            storage.SetItem("tiny", JsonUtility.ToJson(new WindowLayout { x = 10f, y = 10f, w = 10f, h = 10f }));

            var window = new FloatingWindow { storage = storage, storageKey = "tiny" };
            window.open = true;
            yield return Mount(window);

            Assert.AreEqual(FloatingWindow.defaultMinWidth, window.layoutGeometry.w, 1f,
                "A persisted size below the minimum must be raised, not honoured.");
            Assert.AreEqual(FloatingWindow.defaultMinHeight, window.layoutGeometry.h, 1f);
        }

        [UnityTest]
        public IEnumerator CorruptPersistedPayload_FallsBackToTheDefaults()
        {
            var storage = new MemoryStorage();
            storage.SetItem("junk", "not json at all");

            var window = new FloatingWindow
            {
                storage = storage,
                storageKey = "junk",
                defaultWidth = 300f,
                defaultHeight = 200f,
                minWidth = 100f,
                minHeight = 100f,
            };
            Assert.DoesNotThrow(() => window.open = true,
                "A corrupt stored payload must never throw on open.");
            yield return Mount(window);

            Assert.AreEqual(300f, window.layoutGeometry.w, 1f);
            Assert.AreEqual(200f, window.layoutGeometry.h, 1f);
        }

        [UnityTest]
        public IEnumerator ResetLayout_ForgetsThePersistedGeometry()
        {
            var storage = new MemoryStorage();
            var window = new FloatingWindow
            {
                storage = storage,
                storageKey = "forget-me",
                defaultWidth = 300f,
                defaultHeight = 200f,
                minWidth = 100f,
                minHeight = 100f,
            };
            window.open = true;
            yield return Mount(window);
            window.SaveLayout();
            Assert.IsNotNull(storage.GetItem("forget-me"));

            window.ResetLayout();
            yield return null;

            Assert.IsNull(storage.GetItem("forget-me"), "ResetLayout must clear the stored payload.");
            Assert.AreEqual(300f, window.layoutGeometry.w, 1f);
        }

        [UnityTest]
        public IEnumerator CloseButton_ClosesTheWindow_AndDismissableHidesIt()
        {
            var window = Build(true);
            yield return Mount(window);

            var close = window.Q<ActionButton>(className: FloatingWindow.closeButtonUssClassName);
            Assert.IsNotNull(close);
            Assert.AreEqual(DisplayStyle.Flex, close.resolvedStyle.display);

            close.clickable.InvokePressed(null);
            yield return null;
            Assert.IsFalse(window.open, "The close button did not close the window.");

            window.dismissable = false;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, close.resolvedStyle.display,
                "dismissable=false must hide the close button.");
        }

        [UnityTest]
        public IEnumerator Resizable_And_AutoSize_DriveTheGrip()
        {
            var window = Build(true);
            yield return Mount(window);

            var grip = window.Q(className: FloatingWindow.resizeHandleUssClassName);
            Assert.IsNotNull(grip);
            Assert.AreEqual(DisplayStyle.Flex, grip.resolvedStyle.display);

            window.resizable = false;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, grip.resolvedStyle.display);

            window.resizable = true;
            window.autoSize = true;
            for (var i = 0; i < 3; i++)
                yield return null;
            Assert.AreEqual(DisplayStyle.None, grip.resolvedStyle.display,
                "autoSize implies not resizable, so the grip must be hidden even when resizable is true.");
            Assert.IsTrue(window.ClassListContains(FloatingWindow.autoSizeUssClassName));
        }

        [UnityTest]
        public IEnumerator HeaderHeight_ShrinksUnderCompactDensity()
        {
            var normal = Build(true);
            yield return Mount(normal, "normal");
            var normalHeight = normal.Q(className: FloatingWindow.headerUssClassName).resolvedStyle.height;

            var compact = Build(true);
            yield return Mount(compact, "compact");
            var compactHeight = compact.Q(className: FloatingWindow.headerUssClassName).resolvedStyle.height;

            Assert.Greater(normalHeight, 0f);
            Assert.Less(compactHeight, normalHeight,
                $"The title bar height did not shrink under compact density (normal {normalHeight}, compact {compactHeight}). " +
                "It is probably a hardcoded pixel value instead of the control-sizing alias.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_BindsSizesToTokensAndCarriesNoLiterals()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = Regex.Replace(File.ReadAllText(fullPath), @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);

            var root = Regex.Match(uss, @"\.appui-floating-window\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(root.Success, "FloatingWindow.uss has no root rule.");
            StringAssert.Contains("position: absolute", root.Groups["body"].Value,
                "The window must be positioned absolutely, or left/top do nothing.");
            Assert.IsFalse(Regex.IsMatch(root.Groups["body"].Value, @"(?<![-\w])(left|top|width|height):"),
                "Geometry is user-dragged state written inline from C#, never a USS constant.");

            var header = Regex.Match(uss, @"\.appui-floating-window__header\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(header.Success);
            StringAssert.Contains("var(--appui-alias-control-sizing-lg-height)", header.Groups["body"].Value,
                "The title bar height must come from the large control-sizing alias.");

            var grip = Regex.Match(uss, @"\.appui-floating-window__resize-handle\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(grip.Success);
            StringAssert.Contains("var(--appui-alias-control-sizing-sm-square)", grip.Groups["body"].Value,
                "The grip must be sized by the square control-sizing alias.");

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"), "FloatingWindow.uss contains a magic pixel value.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "FloatingWindow.uss contains a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "FloatingWindow.uss contains a literal rgb()/rgba() colour.");
        }

        /// <param name="hostSize">
        /// An explicit size for the host the window is clamped against. The host otherwise fills
        /// the panel, whose size is the screen divided by its DPI scale -- so a 1200x600 screen is
        /// a 600x300 host on a 2x display and a 1200x600 one on a 1x display. A test that asserts
        /// an absolute window position has to pin this, or it passes or fails on the display it
        /// happens to run on.
        /// </param>
        IEnumerator Mount(FloatingWindow window, string density = "normal", Dir dir = Dir.Ltr, Vector2? hostSize = null)
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = density, lang = "en", layoutDirection = dir };
            m_Panel = panel;
            m_TestUI.rootVisualElement.Add(panel);
            var host = new VisualElement { style = { flexGrow = 1 } };
            if (hostSize is { } size)
            {
                // flexShrink too: the panel is a flex column sized to the screen over its DPI
                // scale, so on a 2x display it is shorter than the host asked for and the default
                // flex-shrink of 1 pulls the host straight back down to the panel's height.
                host.style.flexGrow = 0f;
                host.style.flexShrink = 0f;
                host.style.width = size.x;
                host.style.height = size.y;
            }
            panel.Add(host);
            host.Add(window);

            for (var i = 0; i < 6; i++)
                yield return null;
        }
    }
}
