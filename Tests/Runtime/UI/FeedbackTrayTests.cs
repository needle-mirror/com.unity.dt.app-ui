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
    [TestOf(typeof(FeedbackTray))]
    class FeedbackTrayTests : VisualElementTests<FeedbackTray>
    {
        static void Click(IconButton button)
        {
            using var evt = ClickEvent.GetPooled();
            evt.target = button;
            button.clickable.SimulateSingleClickInternal(evt);
        }

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/FeedbackTray.uss";

        protected override string mainUssClassName => FeedbackTray.ussClassName;

        // Handed over to FeedbackTraySnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new FeedbackTray());
                yield return new Story("Good", ctx => new FeedbackTray { value = FeedbackValue.Good });
                yield return new Story("Poor", ctx => new FeedbackTray { value = FeedbackValue.Poor });
                yield return new Story("WithActions", ctx => new FeedbackTray
                {
                    canRefresh = true,
                    canCopy = true,
                });
                yield return new Story("Copied", ctx => new FeedbackTray { canCopy = true, copied = true });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:FeedbackTray />",
            @"<appui:FeedbackTray value=""Good"" can-copy=""true"" can-refresh=""true"" />",
        };

        [Test]
        public void Value_DefaultsToNone()
        {
            var tray = new FeedbackTray();
            Assert.AreEqual(FeedbackValue.None, tray.value);
        }

        [Test]
        public void Value_MarksOnlyTheMatchingButtonSelected()
        {
            var tray = new FeedbackTray { value = FeedbackValue.Good };
            Assert.IsTrue(tray.goodButton.ClassListContains(Styles.selectedUssClassName));
            Assert.IsFalse(tray.poorButton.ClassListContains(Styles.selectedUssClassName));

            tray.value = FeedbackValue.Poor;
            Assert.IsFalse(tray.goodButton.ClassListContains(Styles.selectedUssClassName),
                "The previously selected rating must be cleared, not left behind.");
            Assert.IsTrue(tray.poorButton.ClassListContains(Styles.selectedUssClassName));

            tray.value = FeedbackValue.None;
            Assert.IsFalse(tray.goodButton.ClassListContains(Styles.selectedUssClassName));
            Assert.IsFalse(tray.poorButton.ClassListContains(Styles.selectedUssClassName));
        }

        [UnityTest]
        public IEnumerator Value_NotifiesOnChange()
        {
            var tray = new FeedbackTray();
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(tray);

            var observed = FeedbackValue.None;
            var count = 0;
            tray.RegisterValueChangedCallback(evt =>
            {
                observed = evt.newValue;
                count++;
            });

            for (var i = 0; i < 4; i++)
                yield return null;

            tray.value = FeedbackValue.Good;
            yield return null;
            Assert.AreEqual(FeedbackValue.Good, observed);
            Assert.AreEqual(1, count);

            tray.value = FeedbackValue.Good;
            yield return null;
            Assert.AreEqual(1, count, "Setting the same value again must not raise a change event.");
        }

        [Test]
        public void ClickingTheActiveRating_TogglesBackToNone()
        {
            var tray = new FeedbackTray { value = FeedbackValue.Good };
            Click(tray.goodButton);
            Assert.AreEqual(FeedbackValue.None, tray.value,
                "Re-clicking the active rating must clear it.");

            Click(tray.poorButton);
            Assert.AreEqual(FeedbackValue.Poor, tray.value);
        }

        [Test]
        public void OptionalButtons_AreHiddenUnlessEnabled()
        {
            var tray = new FeedbackTray();
            Assert.IsTrue(tray.refreshButton.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(tray.copyButton.ClassListContains(Styles.hiddenUssClassName));

            tray.canRefresh = true;
            tray.canCopy = true;
            Assert.IsFalse(tray.refreshButton.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(tray.copyButton.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void Copied_SwapsTheCopyGlyph()
        {
            var tray = new FeedbackTray { canCopy = true };
            Assert.AreEqual("copy", tray.copyButton.icon);
            tray.copied = true;
            Assert.AreEqual("check", tray.copyButton.icon);
            tray.copied = false;
            Assert.AreEqual("copy", tray.copyButton.icon);
        }

        [Test]
        public void RefreshAndCopy_RaiseTheirEvents()
        {
            var tray = new FeedbackTray { canRefresh = true, canCopy = true };
            var refreshed = 0;
            var copied = 0;
            tray.refreshRequested += () => refreshed++;
            tray.copyRequested += () => copied++;

            Click(tray.refreshButton);
            Click(tray.copyButton);

            Assert.AreEqual(1, refreshed);
            Assert.AreEqual(1, copied);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensOnly()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);

            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "FeedbackTray.uss uses a literal hex colour instead of an --appui-* token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "FeedbackTray.uss uses a literal rgb()/rgba() colour instead of an --appui-* token.");
            Assert.IsFalse(Regex.IsMatch(uss, @":\s*\d+(\.\d+)?px"),
                "FeedbackTray.uss uses a magic pixel value instead of an --appui-* token.");
        }
    }
}
