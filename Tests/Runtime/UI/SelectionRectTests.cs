using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(SelectionRect))]
    class SelectionRectTests : VisualElementTests<SelectionRect>
    {
        const string k_Uss = "SelectionRect.uss";

        protected override string mainUssClassName => SelectionRect.ussClassName;

        // Handed over to SelectionRectSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Marquee", (ctx) =>
                {
                    var host = new UnityEngine.UIElements.VisualElement
                    {
                        style = { width = 240, height = 160 },
                    };
                    host.Add(new SelectionRect { rect = new Rect(20f, 20f, 160f, 90f) });
                    return host;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:SelectionRect />",
            @"<appui:SelectionRect rect=""20,20,160,90"" />",
        };

        [Test]
        public void Rect_WritesThroughToTheInlineStyle()
        {
            var marquee = new SelectionRect { rect = new Rect(12f, 34f, 56f, 78f) };
            Assert.AreEqual(12f, marquee.style.left.value.value, 0.001f);
            Assert.AreEqual(34f, marquee.style.top.value.value, 0.001f);
            Assert.AreEqual(56f, marquee.style.width.value.value, 0.001f);
            Assert.AreEqual(78f, marquee.style.height.value.value, 0.001f);
        }

        /// <summary>
        /// A marquee dragged up and to the left produces negative extents, which draw as nothing at
        /// all. Normalize is the documented way out; the clamp is the backstop for callers who skip it.
        /// </summary>
        [Test]
        public void Normalize_TurnsAnyTwoDragCornersIntoAPositiveRect()
        {
            var expected = new Rect(10f, 20f, 90f, 80f);
            Assert.AreEqual(expected, SelectionRect.Normalize(new Vector2(100f, 100f), new Vector2(10f, 20f)));
            Assert.AreEqual(expected, SelectionRect.Normalize(new Vector2(10f, 20f), new Vector2(100f, 100f)));
            Assert.AreEqual(expected, SelectionRect.Normalize(new Vector2(10f, 100f), new Vector2(100f, 20f)));
        }

        [Test]
        public void NegativeExtent_ClampsToZeroRatherThanBreakingLayout()
        {
            var marquee = new SelectionRect { rect = new Rect(10f, 10f, -40f, -20f) };
            Assert.AreEqual(0f, marquee.style.width.value.value, 0.001f);
            Assert.AreEqual(0f, marquee.style.height.value.value, 0.001f);
            Assert.AreEqual(new Rect(10f, 10f, -40f, -20f), marquee.rect,
                "The property must report what it was given; only the drawn size is clamped.");
        }

        /// <summary>
        /// The marquee is drawn during a drag, so it must never intercept the release that ends it.
        /// </summary>
        [Test]
        public void Marquee_IsTransparentToThePointer()
        {
            Assert.AreEqual(UnityEngine.UIElements.PickingMode.Ignore, new SelectionRect().pickingMode);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);

            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"^\.appui-selection-rect");
            StringAssert.Contains(
                "background-color: var(--appui-alias-actions-selected-background-color-default)", body);
            StringAssert.Contains(
                "border-color: var(--appui-alias-actions-accent-background-color-default)", body);
        }
    }
}
