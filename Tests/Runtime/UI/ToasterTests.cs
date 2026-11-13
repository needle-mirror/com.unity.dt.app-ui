using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Toaster))]
    class ToasterTests : VisualElementTests<Toaster>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/Toaster.uss";

        protected override string mainUssClassName => Toaster.ussClassName;

        // Handed over to ToasterSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        [SetUp]
        public void ResetStack()
        {
            ToastManager.Clear();
            ToastManager.limit = 3;
        }

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx =>
                {
                    ToastManager.Clear();
                    var toaster = new Toaster();
                    ToastManager.Add("Saved");
                    ToastManager.Add("Import failed", NotificationStyle.Negative);
                    return toaster;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:Toaster />",
            @"<appui:Toaster anchor=""TopRight"" limit=""5"" />",
        };

        [Test]
        public void Anchor_DefaultsToBottomRight_AndSwapsTheClass()
        {
            var toaster = new Toaster();
            Assert.AreEqual(PopupNotificationPlacement.BottomRight, toaster.anchor);
            Assert.IsTrue(toaster.ClassListContains(Toaster.anchorUssClassName + "bottomright"));

            toaster.anchor = PopupNotificationPlacement.TopLeft;
            Assert.IsTrue(toaster.ClassListContains(Toaster.anchorUssClassName + "topleft"));
            Assert.IsFalse(toaster.ClassListContains(Toaster.anchorUssClassName + "bottomright"),
                "The previous anchor class must be removed, not accumulated.");
        }

        [Test]
        public void Limit_DefaultsToThree_AndIsClampedToAtLeastOne()
        {
            Assert.AreEqual(3, ToastManager.limit);
            ToastManager.limit = 0;
            Assert.AreEqual(1, ToastManager.limit);
            ToastManager.limit = -5;
            Assert.AreEqual(1, ToastManager.limit);
        }

        [Test]
        public void Add_KeepsNewestFirst_AndDropsBeyondTheLimit()
        {
            ToastManager.limit = 2;
            ToastManager.Add("one");
            ToastManager.Add("two");
            ToastManager.Add("three");

            var texts = ToastManager.toasts.Select(t => t.text).ToArray();
            Assert.AreEqual(new[] { "three", "two" }, texts,
                "Newest first, capped at the limit — the oldest is dropped.");
        }

        [Test]
        public void LoweringTheLimit_TrimsTheExistingStack()
        {
            ToastManager.Add("one");
            ToastManager.Add("two");
            ToastManager.Add("three");
            Assert.AreEqual(3, ToastManager.toasts.Count);

            ToastManager.limit = 1;
            Assert.AreEqual(1, ToastManager.toasts.Count);
            Assert.AreEqual("three", ToastManager.toasts[0].text);
        }

        [Test]
        public void Dismiss_RemovesOnlyThatEntry()
        {
            var a = ToastManager.Add("a");
            var b = ToastManager.Add("b");
            ToastManager.Dismiss(a);
            Assert.AreEqual(1, ToastManager.toasts.Count);
            Assert.AreSame(b, ToastManager.toasts[0]);
        }

        [Test]
        public void Clear_EmptiesTheStack()
        {
            ToastManager.Add("a");
            ToastManager.Add("b");
            ToastManager.Clear();
            Assert.AreEqual(0, ToastManager.toasts.Count);
        }

        [Test]
        public void Update_MutatesInPlaceAndNotifies()
        {
            var changes = 0;
            void OnChanged() => changes++;
            ToastManager.changed += OnChanged;
            try
            {
                var entry = ToastManager.Add("Uploading…");
                Assert.AreEqual(1, changes);
                entry.Update("Upload complete", NotificationStyle.Positive);
                Assert.AreEqual(2, changes);
                Assert.AreEqual("Upload complete", ToastManager.toasts[0].text);
                Assert.AreEqual(NotificationStyle.Positive, ToastManager.toasts[0].style);
            }
            finally
            {
                ToastManager.changed -= OnChanged;
            }
        }

        [UnityTest]
        public IEnumerator Host_RendersOneToastPerEntry_AndTracksTheStack()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var toaster = new Toaster();
            panel.Add(toaster);
            yield return null;

            Assert.AreEqual(0, toaster.childCount);

            ToastManager.Add("one");
            ToastManager.Add("two");
            yield return null;
            Assert.AreEqual(2, toaster.childCount);

            ToastManager.Clear();
            yield return null;
            Assert.AreEqual(0, LiveToasts(toaster).Count,
                "The host must drop its toasts when the stack empties.");
            Assert.IsTrue(toaster.Children().All(c => c.ClassListContains(Toaster.exitingUssClassName)),
                "Removed toasts play their exit transition before leaving.");

            yield return new UnityEngine.WaitForSeconds(1.5f);
            Assert.AreEqual(0, toaster.childCount,
                "Exiting toasts leave the hierarchy once their transition ends.");
        }

        [UnityTest]
        public IEnumerator Host_KeepsTheSameViewWhenAnEntryUpdates()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var toaster = new Toaster();
            panel.Add(toaster);
            yield return null;

            var entry = ToastManager.Add("Uploading");
            yield return null;
            var before = toaster.Children().Single();

            entry.Update("Upload complete", NotificationStyle.Positive);
            ToastManager.Add("newer");
            yield return null;

            var live = LiveToasts(toaster);
            Assert.AreEqual(2, live.Count);
            Assert.AreSame(before, live[1], "An updated entry keeps its view instead of being rebuilt.");
            Assert.IsTrue(live[0].ClassListContains(Styles.openUssClassName),
                "A new toast opens after its first layout, which plays the enter transition.");
        }

        [UnityTest]
        public IEnumerator Host_PlacesAReplacementAtTheAnchoredEdge_WhileTheOldOneExits()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var toaster = new Toaster();
            panel.Add(toaster);
            yield return null;

            ToastManager.Add("old");
            yield return null;
            ToastManager.Clear();
            ToastManager.Add("new");
            yield return null;

            Assert.AreEqual(2, toaster.childCount, "The old toast is still exiting.");
            Assert.IsFalse(toaster[0].ClassListContains(Toaster.exitingUssClassName),
                "The replacement goes to the anchored edge, ahead of the exiting toast.");
        }

        static List<VisualElement> LiveToasts(Toaster toaster) =>
            toaster.Children().Where(c => !c.ClassListContains(Toaster.exitingUssClassName)).ToList();

        /// <summary>
        /// USS has no :first-child/:last-child, so Refresh marks the edges of the stack itself.
        /// Without those classes the anchor rules that key off them are silently dead.
        /// </summary>
        [UnityTest]
        public IEnumerator Host_MarksTheFirstAndLastToast()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var toaster = new Toaster();
            panel.Add(toaster);
            yield return null;

            ToastManager.Add("one");
            ToastManager.Add("two");
            ToastManager.Add("three");
            yield return null;
            Assert.AreEqual(3, toaster.childCount);

            var children = toaster.Children().ToList();
            for (var i = 0; i < children.Count; i++)
            {
                Assert.AreEqual(i == 0, children[i].ClassListContains(Styles.firstChildUssClassName),
                    $"Only the first toast may carry {Styles.firstChildUssClassName} (index {i}).");
                Assert.AreEqual(i == children.Count - 1, children[i].ClassListContains(Styles.lastChildUssClassName),
                    $"Only the last toast may carry {Styles.lastChildUssClassName} (index {i}).");
            }

            // The stack is diffed newest-first on every change, so the marks have to
            // move with it rather than stick to the element that used to be on the edge.
            ToastManager.Add("four");
            yield return null;
            var rebuilt = LiveToasts(toaster);
            Assert.AreEqual(3, rebuilt.Count, "limit is 3, so the oldest toast is dropped.");
            Assert.IsTrue(rebuilt[0].ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(rebuilt[^1].ClassListContains(Styles.firstChildUssClassName));
            Assert.IsTrue(rebuilt[^1].ClassListContains(Styles.lastChildUssClassName));

            // A single toast is both edges at once.
            ToastManager.Clear();
            ToastManager.Add("solo");
            yield return null;
            var solo = LiveToasts(toaster).Single();
            Assert.IsTrue(solo.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsTrue(solo.ClassListContains(Styles.lastChildUssClassName));
        }

        [UnityTest]
        public IEnumerator Host_StopsTrackingOnceDetached()
        {
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var toaster = new Toaster();
            panel.Add(toaster);
            yield return null;

            toaster.RemoveFromHierarchy();
            yield return null;
            ToastManager.Add("orphan");
            yield return null;
            Assert.AreEqual(0, toaster.childCount,
                "A detached Toaster must unsubscribe from the stack.");
        }

        /// <summary>
        /// Pins the styling to the token contract: the toaster viewport must express its inset and
        /// stack gap through <c>--appui-*</c> tokens, never literal pixels or raw colours.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensForInsetAndGap()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            var root = Regex.Match(uss, @"\.appui-toaster\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(root.Success, "Toaster.uss has no .appui-toaster rule.");
            var body = root.Groups["body"].Value;
            StringAssert.Contains("var(--appui-", body,
                ".appui-toaster must size itself from tokens.");

            Assert.IsFalse(Regex.IsMatch(uss, @":\s*-?\d+(\.\d+)?px"),
                "Toaster.uss uses a literal pixel value instead of a token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "Toaster.uss uses a literal colour instead of a token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "Toaster.uss uses a literal colour instead of a token.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheMainStyleSheet()
        {
            var path = Path.GetFullPath("Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss");
            StringAssert.Contains("Components/Toaster.uss", File.ReadAllText(path),
                "Toaster.uss must be imported by App UI.uss or it styles nothing.");
        }
    }
}
