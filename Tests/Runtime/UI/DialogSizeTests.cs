#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(BaseDialog))]
    class DialogSizeTests : UIComponentTests<Dialog>
    {
        // The test panel is clamped to the batchmode screen (640px wide), and the dialog's
        // max-width: 100% would clamp Lg and Xl to it. A fixed 1200px App UI panel lets every size
        // resolve to its own token.
        Dialog MountWide(Dialog dialog)
        {
            appUiPanel.style.width = 1200f;
            appUiPanel.style.flexShrink = 0f;
            return MountElement(dialog);
        }

        static readonly DialogSize[] k_Sizes = { DialogSize.Sm, DialogSize.Md, DialogSize.Lg, DialogSize.Xl };

        [Test]
        public void Dialog_DefaultSize_IsMd()
        {
            var dialog = new Dialog();
            Assert.AreEqual(DialogSize.Md, dialog.size);
            Assert.IsTrue(dialog.ClassListContains(BaseDialog.sizeUssClassName + "md"));
        }

        [Test]
        public void Dialog_SettingSize_TogglesExactlyOneSizeClass([ValueSource(nameof(k_Sizes))] DialogSize size)
        {
            var dialog = new Dialog();
            dialog.size = DialogSize.Sm;
            dialog.size = size;

            Assert.AreEqual(size, dialog.size);
            foreach (var other in k_Sizes)
            {
                var className = BaseDialog.sizeUssClassName + other.ToString().ToLowerInvariant();
                Assert.AreEqual(other == size, dialog.ClassListContains(className),
                    $"Class {className} with size {size}.");
            }
        }

        [TestCase("Sm", DialogSize.Sm)]
        [TestCase("Md", DialogSize.Md)]
        [TestCase("Lg", DialogSize.Lg)]
        [TestCase("Xl", DialogSize.Xl)]
        [ConditionalIgnore("IgnoreInPlayer", "UXML construction is supported only in Editor")]
        public void Dialog_UxmlSize_RoundTrips(string attribute, DialogSize expected)
        {
            var content = "<ui:UXML xmlns:ui=\"UnityEngine.UIElements\" xmlns:appui=\"Unity.AppUI.UI\">"
                + $"<appui:Dialog name=\"d\" size=\"{attribute}\" /></ui:UXML>";
            var tree = Utils.LoadUxmlTemplateFromString(content).Instantiate();
            var dialog = tree.Q<Dialog>("d");

            Assert.IsNotNull(dialog);
            Assert.AreEqual(expected, dialog.size);
        }

        // The --appui-dialog-width-* tokens in App UI - Components.uss.
        [TestCase(DialogSize.Sm, 400f)]
        [TestCase(DialogSize.Md, 500f)]
        [TestCase(DialogSize.Lg, 640f)]
        [TestCase(DialogSize.Xl, 768f)]
        public void Dialog_ResolvedWidth_MatchesTheSizeToken(DialogSize size, float expected)
        {
            var dialog = MountWide(new Dialog { title = "Title", description = "Description", size = size });
            simulate.FrameUpdate();

            Assert.That(dialog.resolvedStyle.width, Is.EqualTo(expected).Within(0.5f),
                $"Resolved width for {size}; parent {dialog.parent.layout.width}.");
        }

        [Test]
        public void Dialog_Xl_IsWiderThanLg()
        {
            var dialog = MountWide(new Dialog { title = "Title", size = DialogSize.Lg });
            simulate.FrameUpdate();
            var lg = dialog.resolvedStyle.width;
            dialog.size = DialogSize.Xl;
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            var xl = dialog.resolvedStyle.width;

            Assert.Greater(lg, 100f, "Lg needs a real layout.");
            Assert.Greater(xl, lg + 1f, "Xl must be wider than Lg.");
        }
    }
}
#endif
