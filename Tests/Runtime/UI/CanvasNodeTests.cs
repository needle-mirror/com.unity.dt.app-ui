using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(CanvasNode))]
    class CanvasNodeTests : VisualElementTests<CanvasNode>
    {
        const string k_Uss = "CanvasNode.uss";

        protected override string mainUssClassName => CanvasNode.ussClassName;

        // Handed over to CanvasNodeSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static CanvasNode Graph() => new CanvasNode
        {
            title = "Idle",
            x = 40f,
            y = 24f,
            inputPorts = new List<CanvasPort>
            {
                new CanvasPort("in-a", "Base"),
                new CanvasPort("in-b", "Additive"),
            },
            outputPorts = new List<CanvasPort> { new CanvasPort("out", "Result") },
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new CanvasNode { title = "State" });
                yield return new Story("Ports", (ctx) => Graph());
                yield return new Story("Entry", (ctx) => new CanvasNode
                {
                    title = "Entry",
                    variant = CanvasNodeVariant.Entry,
                });
                yield return new Story("Selected", (ctx) => new CanvasNode
                {
                    title = "Selected",
                    selected = true,
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:CanvasNode />",
            @"<appui:CanvasNode title=""Idle"" variant=""Exit"" selected=""true"" movable=""true"" />",
            @"<appui:CanvasNode title=""Placed"" x=""40"" y=""24"" width=""220"" />",
        };

        [Test]
        public void Variant_SwapsTheClassInsteadOfAccumulating()
        {
            var node = new CanvasNode();
            Assert.IsTrue(node.ClassListContains(CanvasNode.GetVariantUssClassName(CanvasNodeVariant.Default)),
                "The default variant class must be applied by the constructor.");

            node.variant = CanvasNodeVariant.Entry;
            Assert.IsTrue(node.ClassListContains(CanvasNode.GetVariantUssClassName(CanvasNodeVariant.Entry)));
            Assert.IsFalse(node.ClassListContains(CanvasNode.GetVariantUssClassName(CanvasNodeVariant.Default)),
                "The previous variant class must come off, not accumulate.");
        }

        [Test]
        public void Selected_TogglesTheClassAndGoesThroughISelectableElement()
        {
            var node = new CanvasNode();
            Assert.IsInstanceOf<ISelectableElement>(node);

            node.selected = true;
            Assert.IsTrue(node.ClassListContains(Styles.selectedUssClassName));

            node.SetSelectedWithoutNotify(false);
            Assert.IsFalse(node.selected);
            Assert.IsFalse(node.ClassListContains(Styles.selectedUssClassName));
        }

        [Test]
        public void Ports_BuildOneRowPerPortOnEachSide()
        {
            var node = Graph();
            Assert.AreEqual(2, node.GetPortCount(CanvasPortSide.Input));
            Assert.AreEqual(1, node.GetPortCount(CanvasPortSide.Output));
            Assert.IsNotNull(node.GetPortElement(CanvasPortSide.Input, 1));
            Assert.IsNull(node.GetPortElement(CanvasPortSide.Output, 1));

            node.inputPorts = null;
            Assert.AreEqual(0, node.GetPortCount(CanvasPortSide.Input),
                "Clearing the port list must remove the rows, not leave them behind.");
        }

        [Test]
        public void Port_DescriptionTakesPartInEquality()
        {
            var a = new CanvasPort("in", "Base", description: "Float");
            Assert.AreEqual(a, new CanvasPort("in", "Base", description: "Float"));
            Assert.AreEqual(a.GetHashCode(), new CanvasPort("in", "Base", description: "Float").GetHashCode());
            Assert.AreNotEqual(a, new CanvasPort("in", "Base", description: "Vector3"),
                "Two ports that differ only in their description are different ports.");
            Assert.IsNull(new CanvasPort("in").description, "A port has no description by default.");
        }

        /// <summary>
        /// The exported geometry is the contract an edge routes against. This is the registry's
        /// formula, transliterated, so a change to either side shows up here.
        /// </summary>
        [Test]
        public void PortAnchor_MatchesTheRegistryFormula()
        {
            var node = Graph();

            var expectedY = 24f + CanvasNode.headerHeight + CanvasNode.portsPaddingY
                + 1 * CanvasNode.portRowHeight + CanvasNode.portRowHeight * 0.5f;

            Assert.AreEqual(new Vector2(40f, expectedY), node.GetPortAnchor(CanvasPortSide.Input, 1));
            Assert.AreEqual(new Vector2(40f + node.width, 24f + CanvasNode.headerHeight
                + CanvasNode.portsPaddingY + CanvasNode.portRowHeight * 0.5f),
                node.GetPortAnchor(CanvasPortSide.Output, 0));
        }

        /// <summary>
        /// The formula above is only worth anything if it agrees with what actually gets laid out.
        /// An edge that lands one row off is the failure this defends against.
        /// </summary>
        [UnityTest]
        public IEnumerator PortAnchor_AgreesWithTheLaidOutPortRowCentre()
        {
            var node = Graph();
            var host = new VisualElement { style = { position = Position.Relative, height = 400, width = 400 } };
            host.Add(node);
            yield return ChatTestUtils.Mount(m_TestUI, host);

            for (var i = 0; i < 2; i++)
            {
                var row = node.GetPortElement(CanvasPortSide.Input, i);
                var centre = row.ChangeCoordinatesTo(host, new Vector2(0f, row.layout.height * 0.5f));
                var anchor = node.GetPortAnchor(CanvasPortSide.Input, i);
                Assert.AreEqual(anchor.y, centre.y, 0.51f,
                    $"Input port {i}'s computed anchor must land on the laid-out row centre.");
                Assert.AreEqual(anchor.x, node.layout.x + node.resolvedStyle.borderLeftWidth, 0.51f);
            }

            var outRow = node.GetPortElement(CanvasPortSide.Output, 0);
            var outCentre = outRow.ChangeCoordinatesTo(host, new Vector2(outRow.layout.width, outRow.layout.height * 0.5f));
            var outAnchor = node.GetPortAnchor(CanvasPortSide.Output, 0);
            Assert.AreEqual(outAnchor.y, outCentre.y, 0.51f);
            Assert.AreEqual(outAnchor.x, outCentre.x, 0.51f,
                "The output anchor must land on the node's right inner edge, where the dot is centred.");
        }

        [UnityTest]
        public IEnumerator Header_HidesItselfWhenThereIsNoTitle()
        {
            var node = new CanvasNode();
            yield return ChatTestUtils.Mount(m_TestUI, node);
            Assert.IsTrue(node.header.ClassListContains(Styles.hiddenUssClassName));

            node.title = "Named";
            Assert.IsFalse(node.header.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void Position_WritesThroughToTheInlineStyle()
        {
            var node = new CanvasNode { x = 12f, y = 34f };
            Assert.IsTrue(node.ClassListContains(CanvasNode.positionedUssClassName));
            Assert.AreEqual(12f, node.style.left.value.value, 0.01f);
            Assert.AreEqual(34f, node.style.top.value.value, 0.01f);

            var free = new CanvasNode();
            Assert.IsFalse(free.ClassListContains(CanvasNode.positionedUssClassName),
                "A node with no x/y must stay in normal flow.");
        }

        /// <summary>
        /// The node's geometry has to be density-invariant, because <see cref="CanvasNode.headerHeight"/>
        /// and friends are compile-time constants an edge routes against. The static sizing ramp is the
        /// only ramp that does not move under a density class, so the USS must use it and only it.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Geometry_ComesFromTheStaticSizingRamp()
        {
            var uss = ChatTestUtils.ReadUss(k_Uss);

            var header = ChatTestUtils.RuleBody(uss, @"^\.appui-canvas-node__header");
            StringAssert.Contains("height: var(--appui-static-sizing-375)", header);

            var row = ChatTestUtils.RuleBody(uss, @"^\.appui-canvas-node__port-row");
            StringAssert.Contains("height: var(--appui-static-sizing-300)", row);

            var ports = ChatTestUtils.RuleBody(uss, @"^\.appui-canvas-node__ports");
            StringAssert.Contains("padding-top: var(--appui-static-sizing-100)", ports);
            StringAssert.Contains("padding-bottom: var(--appui-static-sizing-100)", ports);

            var root = ChatTestUtils.RuleBody(uss, @"^\.appui-canvas-node");
            StringAssert.Contains("width: var(--appui-static-sizing-2250)", root);

            Assert.IsFalse(uss.Contains("--appui-sizing-"),
                "Node geometry must not use the density-varying ramp: the exported constants would drift.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_StaysOnTokens()
        {
            ChatTestUtils.AssertNoLiteralColours(k_Uss);
            ChatTestUtils.AssertNoMagicPixels(k_Uss);
        }

        /// <summary>
        /// The constants are the published contract; pin their values so a USS edit that changes the
        /// ramp step cannot silently desynchronise them.
        /// </summary>
        [Test]
        public void Constants_MatchTheRegistryValues()
        {
            Assert.AreEqual(30f, CanvasNode.headerHeight);
            Assert.AreEqual(24f, CanvasNode.portRowHeight);
            Assert.AreEqual(8f, CanvasNode.portsPaddingY);
            Assert.AreEqual(180f, CanvasNode.defaultWidth);
        }
    }
}
