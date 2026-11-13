using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(TextArea))]
    class TextAreaTests : VisualElementTests<TextArea>
    {
        protected override string mainUssClassName => TextArea.ussClassName;

        // Handed over to TextAreaSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        /// <summary>
        /// The registry's <c>textarea</c> has no size and reads
        /// <c>--appui-alias-field-padding-horizontal-md</c> (#1432). The input and the placeholder
        /// share one rule, so the placeholder sits exactly where the caret starts.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_ComesFromTheFieldPaddingAlias()
        {
            UssSource.AssertHorizontalInset("TextArea.uss", ".appui-textarea__input",
                "--appui-alias-field-padding-horizontal-md");

            var uss = UssSource.Read("TextArea.uss");
            Assert.IsFalse(Regex.IsMatch(uss, @"(?m)^\s*\.appui-textarea__placeholder\s*\{[^}]*\bpadding(-left|-right)?\s*:"),
                "The placeholder-only rule must not set its own horizontal padding; it would drift from the input's.");
        }

        [UnityTest]
        public IEnumerator Placeholder_LinesUpWithTheInput()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var area = new TextArea { placeholder = "Type here" };
            panel.Add(area);
            yield return null;
            yield return null;

            var input = area.Q(className: TextArea.inputUssClassName);
            var placeholder = area.Q(className: TextArea.placeholderUssClassName);
            Assert.Greater(input.resolvedStyle.paddingLeft, 0f, "The input must keep a leading inset.");
            Assert.AreEqual(input.resolvedStyle.paddingLeft, placeholder.resolvedStyle.paddingLeft,
                "The placeholder's leading inset must match the input's.");
            Assert.AreEqual(input.resolvedStyle.paddingRight, placeholder.resolvedStyle.paddingRight,
                "The placeholder's trailing inset must match the input's.");
        }

        /// <summary>
        /// The registry's <c>textarea</c> is <c>text-base md:text-sm</c>: <c>text-sm</c> once the
        /// viewport passes its 768px breakpoint. USS has no viewport query, so UITK pins the desktop
        /// step, for the value and the placeholder alike.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheDesktopStep_AtEveryDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                TextArea filled = null, empty = null;
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    filled = new TextArea("Ag");
                    empty = new TextArea { placeholder = "Ag" };
                    panel.Add(filled);
                    panel.Add(empty);
                }, s => stage = s);

                FieldTextSize.AssertTier(stage, filled.Q(className: TextArea.inputUssClassName).Q<TextElement>(), "sm",
                    "a TextArea's value");
                FieldTextSize.AssertTier(stage, empty.Q(className: TextArea.placeholderUssClassName), "sm",
                    "a TextArea's placeholder");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedSmStep()
        {
            FieldTextSize.AssertFontSize("TextArea.uss", ".appui-textarea__input", 100);
        }
    }
}
