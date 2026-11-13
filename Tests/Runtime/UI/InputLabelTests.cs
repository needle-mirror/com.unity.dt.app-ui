using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(InputLabel))]
    class InputLabelTests : VisualElementTests<InputLabel>
    {

        // A textless InputLabel lays out to nothing, and its whole job is to sit beside a control.
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) =>
                {
                    var label = new InputLabel("Project name");
                    label.Add(new Unity.AppUI.UI.TextField { value = "My Project" });
                    return label;
                });
            }
        }

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/InputLabel.uss";

        protected override string mainUssClassName => InputLabel.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:InputLabel />",
            @"<appui:InputLabel label=""Metallic"" row-layout=""Grid"" columns=""Pair"" tall=""true"" label-for=""metallic"" />",
        };

        // Handed over to InputLabelSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        [Test]
        [Order(10)]
        public void CanSetDraggableProperty()
        {
            var label = new InputLabel("Test");
            Assert.IsFalse(label.draggable);

            label.draggable = true;
            Assert.IsTrue(label.draggable);

            label.draggable = false;
            Assert.IsFalse(label.draggable);
        }

        [UnityTest]
        [Order(11)]
        public IEnumerator DraggableProvidesDragContext()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var label = new InputLabel("Test") { draggable = true };
            panel.Add(label);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Simulate providing drag context
            label.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(5f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            // Check that context is provided
            var context = label.GetSelfContext<DragContext>();
            Assert.IsNotNull(context);
            Assert.AreEqual(5f, context.delta.x);
        }

        [Test]
        [Order(12)]
        public void Icon_IsOptional_AndTogglesTheWithIconClass()
        {
            var label = new InputLabel("Test");
            var icon = label.Q<Icon>(className: InputLabel.iconUssClassName);
            Assert.IsNotNull(icon, "InputLabel must build a leading icon element.");
            Assert.IsTrue(icon.ClassListContains(Styles.hiddenUssClassName),
                "The leading icon must be hidden when no icon is set.");
            Assert.IsFalse(label.ClassListContains(InputLabel.withIconUssClassName));

            label.icon = "info";

            Assert.AreEqual("info", label.icon);
            Assert.IsFalse(icon.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(label.ClassListContains(InputLabel.withIconUssClassName));

            label.icon = null;

            Assert.IsTrue(icon.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(label.ClassListContains(InputLabel.withIconUssClassName),
                "The stale --with-icon class must be removed.");
        }

        [Test]
        [Order(13)]
        public void CapValue_TogglesTheCapValueClass()
        {
            var label = new InputLabel("Test");
            Assert.IsFalse(label.capValue);
            Assert.IsFalse(label.ClassListContains(InputLabel.capValueUssClassName));

            label.capValue = true;

            Assert.IsTrue(label.capValue);
            Assert.IsTrue(label.ClassListContains(InputLabel.capValueUssClassName));

            label.capValue = false;

            Assert.IsFalse(label.ClassListContains(InputLabel.capValueUssClassName));
        }

        [Test]
        [Order(14)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void CapValue_CapsTheInputAndLetsTheLabelGrow()
        {
            var fullPath = System.IO.Path.GetFullPath(k_UssPath);
            var uss = System.IO.File.ReadAllText(fullPath);

            var labelRule = System.Text.RegularExpressions.Regex.Match(uss,
                @"\.appui-inputlabel--cap-value\.appui-inputlabel--horizontal > \.appui-inputlabel__label-container\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(labelRule.Success, "InputLabel.uss must widen the label container under --cap-value.");
            StringAssert.Contains("flex-grow: 1", labelRule.Groups["body"].Value);

            var inputRule = System.Text.RegularExpressions.Regex.Match(uss,
                @"\.appui-inputlabel--cap-value\.appui-inputlabel--horizontal > \.appui-inputlabel__input-container\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(inputRule.Success, "InputLabel.uss must cap the input container under --cap-value.");
            var body = inputRule.Groups["body"].Value;
            StringAssert.Contains("max-width: var(--appui-inputlabel-input-max-size)", body,
                "The cap must go through the component variable, not a hardcoded value.");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(body, @":\s*\d+(\.\d+)?px"),
                "The cap rule must not inline a literal pixel value.");
        }

        [Test]
        [Order(15)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_UsesTokensOnly_NoLiteralColours()
        {
            var uss = System.IO.File.ReadAllText(System.IO.Path.GetFullPath(k_UssPath));
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "InputLabel.uss must not contain a literal hex colour.");
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(uss, @"\brgba?\("),
                "InputLabel.uss must not contain a literal rgb()/rgba() colour.");
        }
        [Test]
        [Order(16)]
        public void RowLayout_DefaultsToFlex()
        {
            var label = new InputLabel("Opacity");
            Assert.AreEqual(InputLabelRowLayout.Flex, label.rowLayout);
            Assert.IsTrue(label.ClassListContains("appui-inputlabel--layout-flex"));
        }

        [Test]
        [Order(16)]
        public void RowLayout_AppliesOneLayoutClassAtATime()
        {
            var label = new InputLabel("Opacity") { rowLayout = InputLabelRowLayout.Grid };
            Assert.IsTrue(label.ClassListContains("appui-inputlabel--layout-grid"));
            Assert.IsFalse(label.ClassListContains("appui-inputlabel--layout-flex"));

            label.rowLayout = InputLabelRowLayout.Flex;
            Assert.IsFalse(label.ClassListContains("appui-inputlabel--layout-grid"));
        }

        /// <summary>
        /// On the grid the icon belongs inside the label cell. Beside it, the icon would eat into the row
        /// before the columns are measured, and a row with an icon would put its control one icon-width
        /// off the column the row above uses.
        /// </summary>
        [Test]
        [Order(16)]
        public void RowLayout_Grid_MovesTheIconIntoTheLabelCell()
        {
            var label = new InputLabel("Opacity") { icon = "drop" };
            var icon = label.Q<Icon>(className: InputLabel.iconUssClassName);
            var labelCell = label.Q(className: InputLabel.labelContainerUssClassName);

            Assert.AreSame(label, icon.hierarchy.parent, "In the flex row the icon sits before the label cell.");

            label.rowLayout = InputLabelRowLayout.Grid;
            Assert.AreSame(labelCell, icon.hierarchy.parent);
            Assert.AreEqual(0, labelCell.IndexOf(icon), "The icon must lead the label cell.");

            label.rowLayout = InputLabelRowLayout.Flex;
            Assert.AreSame(label, icon.hierarchy.parent, "Going back to flex must restore the row's anatomy.");
        }

        /// <summary>
        /// USS has no grid and no calc(), so the label cell's share of the row is a percentage basis the
        /// component works out from the two column-count tokens. Every grid row resolves the same
        /// tokens, which is what makes their column edges coincide.
        /// </summary>
        [UnityTest]
        [Order(16)]
        public IEnumerator RowLayout_Grid_GivesTheLabelCellItsColumnShare()
        {
            var label = new InputLabel("Opacity") { rowLayout = InputLabelRowLayout.Grid };
            label.Add(new Unity.AppUI.UI.FloatField { value = 1f });
            label.style.width = 350f;

            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            panel.Add(label);
            yield return null;
            yield return null;

            var labelCell = label.Q(className: InputLabel.labelContainerUssClassName);

            // A share of the row, not a fixed width: the basis is what the component computed from the
            // two column-count tokens.
            var basis = labelCell.style.flexBasis;
            Assert.AreEqual(LengthUnit.Percent, basis.value.unit);
            // 3 of 7 columns, the shipped token values.
            Assert.AreEqual(3f / 7f * 100f, basis.value.value, 0.5f);
            Assert.AreEqual(3f / 7f, labelCell.resolvedStyle.width / label.resolvedStyle.width, 0.02f,
                "The label cell must resolve to its column share of the row.");
        }

        [Test]
        [Order(16)]
        public void Columns_TagsTheFirstChildSoItCarriesNoLeadingGap()
        {
            var label = new InputLabel("Opacity") { rowLayout = InputLabelRowLayout.Grid, columns = InputLabelColumns.Even };
            var slider = new Unity.AppUI.UI.SliderFloat();
            var field = new Unity.AppUI.UI.FloatField();
            label.Add(slider);
            label.Add(field);
            label.RefreshColumnMarkers();

            Assert.IsTrue(label.ClassListContains(InputLabel.columnsUssClassName));
            Assert.IsTrue(slider.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(field.ClassListContains(Styles.firstChildUssClassName));

            // `> *` does not reliably reach the caller's children, so each one is tagged as a column.
            Assert.IsTrue(slider.ClassListContains(InputLabel.columnUssClassName));
            Assert.IsTrue(field.ClassListContains(InputLabel.columnUssClassName));

            label.columns = InputLabelColumns.None;
            Assert.IsFalse(slider.ClassListContains(InputLabel.columnUssClassName),
                "A cell no longer handed out as columns must not keep styling its children as columns.");
        }

        [Test]
        [Order(16)]
        public void Tall_IsItsOwnClass_SoTheRowCanGrow()
        {
            var label = new InputLabel("Notes") { rowLayout = InputLabelRowLayout.Grid };
            Assert.IsFalse(label.tall);

            label.tall = true;
            Assert.IsTrue(label.ClassListContains(InputLabel.tallUssClassName));

            label.tall = false;
            Assert.IsFalse(label.ClassListContains(InputLabel.tallUssClassName));
        }

        [Test]
        [Order(16)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_GridRow_TakesItsHeightAndGapFromTheInspectorTokens()
        {
            var uss = System.IO.File.ReadAllText(System.IO.Path.GetFullPath(k_UssPath));

            var row = System.Text.RegularExpressions.Regex.Match(uss,
                @"\.appui-inputlabel--layout-grid\s*\{(?<body>[^}]*)\}");
            Assert.IsTrue(row.Success, "InputLabel.uss has no grid row rule.");
            StringAssert.Contains("height: var(--appui-alias-inspector-row-height)", row.Groups["body"].Value,
                "A grid row's height must come from the inspector row-height token.");

            StringAssert.Contains("var(--appui-alias-inspector-grid-gap)", uss,
                "The gap between the label and control cells must come from the inspector grid-gap token.");
            // A declaration, not the prose: the file explains in a comment that USS has no calc().
            Assert.IsFalse(System.Text.RegularExpressions.Regex.IsMatch(uss, @":[^;{}\r\n]*calc\("),
                "USS has no calc(): the column share is computed in C#.");
        }

        /// <summary>
        /// The registry's property-field label reads the inspector label tokens in both of its
        /// layouts, while a bare form label keeps the plain FieldLabel ones. InputLabel rebinds the
        /// FieldLabel vars on its own label element rather than changing FieldLabel's defaults, so
        /// the rule must be scoped there.
        /// </summary>
        [Test]
        [Order(17)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_FieldLabel_ReadsTheInspectorLabelTokens()
        {
            var block = UssSource.RuleBlock(UssSource.Read("InputLabel.uss"), ".appui-inputlabel__field-label");
            Assert.IsNotNull(block, "InputLabel.uss has no .appui-inputlabel__field-label rule.");
            StringAssert.Contains("--appui-field-label-size: var(--appui-alias-inspector-label-size)", block,
                "The InputLabel's label must take its size from the inspector label token.");
            StringAssert.Contains("--appui-field-label-color-default: var(--appui-alias-inspector-label-color)", block,
                "The InputLabel's label must take its colour from the inspector label token.");

            var fieldLabelRoot = UssSource.RuleBlock(UssSource.Read("FieldLabel.uss"), ":root");
            Assert.IsNotNull(fieldLabelRoot, "FieldLabel.uss has no :root rule.");
            Assert.IsFalse(fieldLabelRoot.Contains("--appui-alias-inspector-label-"),
                "A standalone FieldLabel is the registry's form label, not an inspector label. The "
                + "inspector tokens belong on the InputLabel's label only.");
        }

        /// <summary>
        /// Resolved, not just written: the label inside an InputLabel lands on the inspector label
        /// size, a step above the standalone FieldLabel beside it.
        /// </summary>
        [UnityTest]
        [Order(17)]
        public IEnumerator FieldLabel_InsideAnInputLabel_ResolvesLargerThanAStandaloneOne()
        {
#if !UNITY_EDITOR
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEngine.Resources.Load<ThemeStyleSheet>("Themes/App UI"));
#else
            m_TestUI.rootVisualElement.styleSheets.Add(
                UnityEditor.AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                    "Packages/com.unity.dt.app-ui/PackageResources/Styles/Themes/App UI.tss"));
#endif
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark", density = "normal", lang = "en" };
            m_TestUI.rootVisualElement.Add(panel);

            var flex = new InputLabel("Opacity");
            var grid = new InputLabel("Opacity") { rowLayout = InputLabelRowLayout.Grid };
            var standalone = new FieldLabel("Opacity");
            panel.Add(flex);
            panel.Add(grid);
            panel.Add(standalone);
            yield return null;
            yield return null;
            yield return null;

            var standaloneSize = standalone.Q(className: FieldLabel.labelUssClassName).resolvedStyle.fontSize;
            Assert.Greater(standaloneSize, 0f, "The standalone FieldLabel resolved no font size at all.");

            foreach (var (name, row) in new[] { ("flex", flex), ("grid", grid) })
            {
                var size = row.Q(className: FieldLabel.labelUssClassName).resolvedStyle.fontSize;
                Assert.Greater(size, standaloneSize,
                    $"The {name} InputLabel's label must resolve to --appui-alias-inspector-label-size, a "
                    + "step above the standalone FieldLabel's --appui-font-sizes-100.");
            }
        }

        [Test]
        [Order(18)]
        public void Columns_AppliesOneModeClassAtATime()
        {
            var label = new InputLabel("Opacity") { rowLayout = InputLabelRowLayout.Grid };
            Assert.AreEqual(InputLabelColumns.None, label.columns);
            Assert.IsFalse(label.ClassListContains(InputLabel.columnsUssClassName));

            label.columns = InputLabelColumns.Pair;
            Assert.IsTrue(label.ClassListContains(InputLabel.columnsUssClassName));
            Assert.IsTrue(label.ClassListContains("appui-inputlabel--columns-pair"));
            Assert.IsFalse(label.ClassListContains("appui-inputlabel--columns-even"));

            label.columns = InputLabelColumns.Even;
            Assert.IsTrue(label.ClassListContains("appui-inputlabel--columns-even"));
            Assert.IsFalse(label.ClassListContains("appui-inputlabel--columns-pair"));

            label.columns = InputLabelColumns.None;
            Assert.IsFalse(label.ClassListContains(InputLabel.columnsUssClassName));
        }

        static IEnumerator MountGridRow(UIDocument document, InputLabel row)
        {
            row.rowLayout = InputLabelRowLayout.Grid;
            row.style.width = 420f;
            return ChatTestUtils.Mount(document, row);
        }

        /// <summary>
        /// The registry's <c>columns="pair"</c> is two equal tracks with the grid gap between them, so
        /// a slider and its number field each take a half and the seam lines up row after row.
        /// </summary>
        [UnityTest]
        [Order(18)]
        public IEnumerator Columns_Pair_SplitsTheControlCellIntoTwoEqualHalves()
        {
            var row = new InputLabel("Metallic") { columns = InputLabelColumns.Pair };
            var slider = new SliderFloat { lowValue = 0f, highValue = 1f };
            var field = new Unity.AppUI.UI.FloatField();
            row.Add(slider);
            row.Add(field);
            yield return MountGridRow(m_TestUI, row);

            var cell = row.Q(className: InputLabel.containerUssClassName);
            Assert.Greater(cell.layout.width, 0f, "The control cell did not lay out.");
            Assert.AreEqual(slider.layout.width, field.layout.width, 1f,
                $"A pair splits the cell into two equal halves, whatever the two controls would want (cell {cell.layout}, slider {slider.layout}, field {field.layout}).");
            Assert.Greater(field.layout.x, slider.layout.xMax, "The grid gap must sit between the halves.");
            Assert.AreEqual(cell.layout.width, field.layout.xMax, 1f, "The second half must end on the cell's edge.");
        }

        /// <summary>
        /// The tracks are the pair's, not the children's: a lone control takes the first half and leaves
        /// the second empty, so it still lines up with the slider of the pair row above it. That is what
        /// tells <see cref="InputLabelColumns.Pair"/> apart from <see cref="InputLabelColumns.Even"/>,
        /// which would hand the lone control the whole cell.
        /// </summary>
        [UnityTest]
        [Order(18)]
        public IEnumerator Columns_Pair_LeavesALoneControlInTheFirstHalf()
        {
            var full = new InputLabel("Metallic") { columns = InputLabelColumns.Pair, rowLayout = InputLabelRowLayout.Grid };
            var slider = new SliderFloat { lowValue = 0f, highValue = 1f };
            full.Add(slider);
            full.Add(new Unity.AppUI.UI.FloatField());
            full.style.width = 420f;

            var pair = new InputLabel("Roughness") { columns = InputLabelColumns.Pair };
            var lone = new Unity.AppUI.UI.FloatField();
            pair.Add(lone);
            yield return MountGridRow(m_TestUI, pair);
            pair.parent.Insert(0, full);
            for (var i = 0; i < 4; i++)
                yield return null;

            var cell = pair.Q(className: InputLabel.containerUssClassName);
            Assert.Greater(cell.layout.width, 0f, "The control cell did not lay out.");
            Assert.Less(lone.layout.width, cell.layout.width * 0.55f,
                $"A lone control in a pair row must take one half of the cell, not all of it (cell {cell.layout}, control {lone.layout}).");
            Assert.AreEqual(slider.layout.width, lone.layout.width, 1f,
                "The lone control must line up with the first control of a full pair row, gap reserved.");
            Assert.AreEqual(0f, lone.layout.x, 1f, "The lone control takes the first half, at the cell's leading edge.");

            pair.columns = InputLabelColumns.Even;
            for (var i = 0; i < 3; i++)
                yield return null;
            Assert.AreEqual(cell.layout.width, lone.layout.width, 1f,
                $"Even shares the cell between the children, so a lone one takes all of it (cell {cell.layout}, control {lone.layout}).");
        }

        /// <summary>
        /// A control added to a mounted pair row whose cell keeps its size raises no layout event on the
        /// cell, and must still be styled as a column.
        /// </summary>
        [UnityTest]
        [Order(18)]
        public IEnumerator Columns_AControlAddedAfterMount_IsStyledAsAColumn()
        {
            var row = new InputLabel("Metallic") { columns = InputLabelColumns.Pair };
            var first = new Unity.AppUI.UI.FloatField();
            row.Add(first);
            yield return MountGridRow(m_TestUI, row);

            var second = new Unity.AppUI.UI.FloatField();
            row.Add(second);
            yield return new WaitForSeconds(0.3f);
            for (var i = 0; i < 3; i++)
                yield return null;

            Assert.IsTrue(second.ClassListContains(InputLabel.columnUssClassName), "The late control was never tagged.");
            Assert.IsFalse(row.ClassListContains(InputLabel.pairLoneUssClassName), "The pair is no longer lone.");
            Assert.AreEqual(first.layout.width, second.layout.width, 1f, "The two controls split the cell evenly.");
        }

        /// <summary>
        /// Swapping a later control for another keeps the child count and the first child, and must
        /// still tag the newcomer as a column.
        /// </summary>
        [UnityTest]
        [Order(18)]
        public IEnumerator Columns_ALaterControlSwappedAtTheSameCount_IsStyledAsAColumn()
        {
            var row = new InputLabel("Metallic") { columns = InputLabelColumns.Pair };
            var first = new Unity.AppUI.UI.FloatField();
            var second = new Unity.AppUI.UI.FloatField();
            row.Add(first);
            row.Add(second);
            yield return MountGridRow(m_TestUI, row);
            Assert.IsTrue(second.ClassListContains(InputLabel.columnUssClassName));

            var third = new Unity.AppUI.UI.FloatField();
            row.Remove(second);
            row.Add(third);
            yield return new WaitForSeconds(0.3f);
            for (var i = 0; i < 3; i++)
                yield return null;

            Assert.IsTrue(third.ClassListContains(InputLabel.columnUssClassName), "The swapped-in control was never tagged.");
        }

        /// <summary>
        /// The registry re-points the medium control height at the inspector row height on a grid row
        /// and on a PropertyGroup body, so a host that dials the row height keeps its controls on the
        /// row. Only there: a flex row is a form row, not an inspector row.
        /// </summary>
        [Test]
        [Order(19)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_MediumControlHeight_FollowsTheInspectorRowHeight_OnGridRowsAndGroupBodiesOnly()
        {
            const string repoint = "--appui-alias-control-sizing-md-height: var(--appui-alias-inspector-row-height)";

            var inputLabel = UssSource.Read("InputLabel.uss");
            var grid = UssSource.RuleBlock(inputLabel, ".appui-inputlabel--layout-grid");
            Assert.IsNotNull(grid, "InputLabel.uss has no grid row rule.");
            StringAssert.Contains(repoint, grid,
                "A grid row must re-point the medium control height at the inspector row height.");

            var baseRule = UssSource.RuleBlock(inputLabel, ".appui-inputlabel");
            Assert.IsNotNull(baseRule, "InputLabel.uss has no base rule.");
            StringAssert.DoesNotContain("--appui-alias-control-sizing-md-height", baseRule,
                "Only the grid row is an inspector row; a flex InputLabel keeps the control ramp.");
            var flex = UssSource.RuleBlock(inputLabel, ".appui-inputlabel--horizontal");
            StringAssert.DoesNotContain("--appui-alias-control-sizing-md-height", flex ?? string.Empty,
                "Only the grid row is an inspector row; a flex InputLabel keeps the control ramp.");

            var body = UssSource.RuleBlock(UssSource.Read("PropertyGroup.uss"), ".appui-property-group__content");
            Assert.IsNotNull(body, "PropertyGroup.uss has no content rule.");
            StringAssert.Contains(repoint, body,
                "A PropertyGroup body must re-point the medium control height at the inspector row height.");
        }

        /// <summary>
        /// Resolved, not just written: the re-point resolves to a real height, so a typo in the var name
        /// or a cycle back through the control ramp would leave the control with none.
        /// </summary>
        [UnityTest]
        [Order(19)]
        public IEnumerator GridRow_AControlResolvesToTheInspectorRowHeight()
        {
            var row = new InputLabel("Name");
            var field = new Unity.AppUI.UI.TextField();
            row.Add(field);
            yield return MountGridRow(m_TestUI, row);

            var standalone = new Unity.AppUI.UI.TextField();
            row.parent.Add(standalone);
            for (var i = 0; i < 3; i++)
                yield return null;

            Assert.Greater(standalone.resolvedStyle.height, 0f, "The standalone field resolved no height.");
            Assert.AreEqual(row.resolvedStyle.height, field.resolvedStyle.height, 0.5f,
                "A control in a grid row must be as tall as the row.");
            Assert.AreEqual(standalone.resolvedStyle.height, field.resolvedStyle.height, 0.5f,
                "Row height and the control ramp resolve to the same token by default.");
        }

    }
}
