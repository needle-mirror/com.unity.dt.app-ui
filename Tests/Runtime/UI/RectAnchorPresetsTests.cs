using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectAnchorPresets))]
    class RectAnchorPresetsTests : VisualElementTests<RectAnchorPresets>
    {
        const string k_Uss = "RectAnchorPresets.uss";

        protected override string mainUssClassName => RectAnchorPresets.ussClassName;

        // Handed over to RectAnchorPresetsSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new RectAnchorPresets());
                yield return new Story("Stretch selected", (ctx) => new RectAnchorPresets
                {
                    anchorMin = Vector2.zero,
                    anchorMax = Vector2.one,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:RectAnchorPresets />",
            @"<appui:RectAnchorPresets anchor-min=""0,0"" anchor-max=""1,1"" />",
        };

        [Test]
        public void Grid_IsFourByFour()
        {
            var presets = new RectAnchorPresets();
            Assert.AreEqual(4, RectAnchorPresets.columns.Length);
            Assert.AreEqual(4, RectAnchorPresets.rows.Length);

            for (var r = 0; r < 4; r++)
                for (var c = 0; c < 4; c++)
                    Assert.IsNotNull(presets.GetCell(r, c), $"Cell {r},{c} is missing.");
        }

        /// <summary>
        /// The rows run y-up — top is 1, bottom is 0 — because that is what RectTransform.anchorMin and
        /// anchorMax mean. Getting this upside down is invisible until a preset is applied.
        /// </summary>
        [Test]
        public void Rows_RunYUpSoTheTopRowIsOne()
        {
            Assert.AreEqual(new Vector2(1f, 1f), RectAnchorPresets.rows[0], "Row 0 is the top.");
            Assert.AreEqual(new Vector2(0f, 0f), RectAnchorPresets.rows[2], "Row 2 is the bottom.");
            Assert.AreEqual(new Vector2(0f, 1f), RectAnchorPresets.rows[3], "Row 3 stretches vertically.");

            Assert.AreEqual(new Vector2(0f, 0f), RectAnchorPresets.columns[0], "Column 0 is the left.");
            Assert.AreEqual(new Vector2(1f, 1f), RectAnchorPresets.columns[2], "Column 2 is the right.");
            Assert.AreEqual(new Vector2(0f, 1f), RectAnchorPresets.columns[3], "Column 3 stretches horizontally.");
        }

        [Test]
        public void Selection_HighlightsExactlyTheMatchingPreset()
        {
            var presets = new RectAnchorPresets();
            for (var r = 0; r < 4; r++)
                for (var c = 0; c < 4; c++)
                    Assert.IsFalse(presets.GetCell(r, c).ClassListContains(Styles.selectedUssClassName),
                        "Nothing is selected until anchors are set.");

            // Bottom-left point anchor: column 0, row 2.
            presets.anchorMin = Vector2.zero;
            presets.anchorMax = Vector2.zero;

            var selected = 0;
            for (var r = 0; r < 4; r++)
                for (var c = 0; c < 4; c++)
                    if (presets.GetCell(r, c).ClassListContains(Styles.selectedUssClassName))
                        selected++;

            Assert.AreEqual(1, selected, "Exactly one preset matches a given anchor pair.");
            Assert.IsTrue(presets.IsSelected(2, 0), "Anchor (0,0)-(0,0) is bottom-left.");

            presets.anchorMin = Vector2.zero;
            presets.anchorMax = Vector2.one;
            Assert.IsTrue(presets.IsSelected(3, 3), "Anchor (0,0)-(1,1) is stretch/stretch.");
            Assert.IsFalse(presets.GetCell(2, 0).ClassListContains(Styles.selectedUssClassName),
                "The previous highlight must come off, not accumulate.");
        }

        [UnityTest]
        public IEnumerator Picking_ReportsTheCellsAnchorsAndTheHeldModifiers()
        {
            var presets = new RectAnchorPresets();
            yield return ChatTestUtils.Mount(m_TestUI, presets);

            RectAnchorPickEvent? got = null;
            presets.picked += evt => got = evt;

            using (var click = ClickEvent.GetPooled())
            {
                click.target = presets.GetCell(0, 2);
                presets.GetCell(0, 2).SendEvent(click);
            }

            yield return null;
            Assert.IsTrue(got.HasValue, "Clicking a preset must report it.");
            Assert.AreEqual(new Vector2(1f, 1f), got.Value.anchorMin, "Top-right is anchor (1,1).");
            Assert.AreEqual(new Vector2(1f, 1f), got.Value.anchorMax);
            Assert.IsFalse(got.Value.setPivot);
            Assert.IsFalse(got.Value.setPosition);
        }

        /// <summary>
        /// The glyph's two colours come from custom properties declared on the cell, which is the only
        /// way a Painter2D component can be themed at all. Both fields start transparent so an
        /// unresolved property cannot pass for a real colour.
        /// </summary>
        [UnityTest]
        public IEnumerator GlyphColours_ResolveFromTheStylesheet()
        {
            var presets = new RectAnchorPresets();
            yield return ChatTestUtils.Mount(m_TestUI, presets);

            var glyph = presets.GetCell(0, 0).Q<RectAnchorPresetGlyph>();
            Assert.IsNotNull(glyph);
            Assert.AreNotEqual(Color.clear, glyph.resolvedGlyphColor,
                "--rect-glyph-color must reach the painter.");
            Assert.AreNotEqual(Color.clear, glyph.resolvedAnchorColor,
                "--rect-anchor must reach the painter.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);

            // Declared on the glyph's own rule, not the cell's: UITK only surfaces a custom property
            // to the element a matching rule declared it on.
            var body = ChatTestUtils.RuleBody(ChatTestUtils.ReadUss(k_Uss), @"^\.appui-rect-anchor-preset-glyph");
            StringAssert.Contains("--rect-anchor: var(--appui-alias-editor-anchor)", body);
            StringAssert.Contains("--rect-glyph-color: var(--appui-alias-text-disabled)", body);

            var uss = ChatTestUtils.ReadUss(k_Uss);
            Assert.IsFalse(uss.Contains("--appui-sizing-"),
                "A preset glyph is a fixed-scale diagram; it must stay on the static ramp.");
        }
    }
}
