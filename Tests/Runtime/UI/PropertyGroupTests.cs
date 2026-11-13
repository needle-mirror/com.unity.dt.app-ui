using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(PropertyGroup))]
    class PropertyGroupTests : VisualElementTests<PropertyGroup>
    {
        // Handed over to PropertyGroupSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => PropertyGroup.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Section", (ctx) => Build("Transform", PropertyGroupLevel.Section, true));
                yield return new Story("SectionClosed", (ctx) => Build("Transform", PropertyGroupLevel.Section, false));
                yield return new Story("Sub", (ctx) => Build("Advanced", PropertyGroupLevel.Sub, true));
                yield return new Story("SubClosed", (ctx) => Build("Advanced", PropertyGroupLevel.Sub, false));
                yield return new Story("WithTrailing", (ctx) =>
                {
                    var group = Build("Transform", PropertyGroupLevel.Section, true);
                    group.trailingContainer.Add(new IconButton("dots-three-vertical") { quiet = true });
                    group.RefreshTrailing();
                    return group;
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:PropertyGroup />",
            @"<appui:PropertyGroup label=""Transform"" level=""Section"" open=""false"" />",
        };

        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/PropertyGroup.uss";

        static PropertyGroup Build(string label, PropertyGroupLevel level, bool open)
        {
            var group = new PropertyGroup { label = label, level = level, open = open };
            var row = new InputLabel("Position") { rowLayout = InputLabelRowLayout.Grid };
            row.Add(new Unity.AppUI.UI.FloatField { value = 1f });
            group.Add(row);
            return group;
        }

        [Test]
        public void OpenByDefault_AndChildrenLandInTheContent()
        {
            var group = new PropertyGroup();
            Assert.IsTrue(group.open);

            var row = new VisualElement();
            group.Add(row);
            Assert.AreSame(group.Q(className: PropertyGroup.contentUssClassName), row.hierarchy.parent,
                "A row added to the group must land in its content, not beside its header.");
        }

        [Test]
        public void Open_TogglesTheClassAndRaisesOpenChangedOnce()
        {
            var group = Build("Transform", PropertyGroupLevel.Section, true);
            var raised = new List<bool>();
            group.openChanged += raised.Add;

            group.open = false;
            Assert.IsFalse(group.ClassListContains(PropertyGroup.openUssClassName));
            CollectionAssert.AreEqual(new[] { false }, raised);

            group.open = false;
            CollectionAssert.AreEqual(new[] { false }, raised,
                "Setting the state it is already in must not raise the event again.");

            group.Toggle();
            Assert.IsTrue(group.ClassListContains(PropertyGroup.openUssClassName));
            CollectionAssert.AreEqual(new[] { false, true }, raised);
        }

        [Test]
        public void SetOpenWithoutNotify_ChangesTheStateSilently()
        {
            var group = Build("Transform", PropertyGroupLevel.Section, true);
            var raised = 0;
            group.openChanged += _ => raised++;

            group.SetOpenWithoutNotify(false);
            Assert.IsFalse(group.open);
            Assert.AreEqual(0, raised);
        }

        /// <summary>
        /// The ellipsis stands in for the rows a closed group folded away, so it is the only thing that
        /// tells a closed group from an empty one.
        /// </summary>
        [Test]
        public void Ellipsis_ShowsOnlyWhenClosedWithContent()
        {
            var group = Build("Transform", PropertyGroupLevel.Section, true);
            var ellipsis = group.Q(className: PropertyGroup.ellipsisUssClassName);
            Assert.IsNotNull(ellipsis);
            Assert.IsTrue(ellipsis.ClassListContains(Styles.hiddenUssClassName), "An open group shows no ellipsis.");

            group.open = false;
            Assert.IsFalse(ellipsis.ClassListContains(Styles.hiddenUssClassName));

            var empty = new PropertyGroup { open = false };
            Assert.IsTrue(empty.Q(className: PropertyGroup.ellipsisUssClassName)
                    .ClassListContains(Styles.hiddenUssClassName),
                "An empty group has folded nothing away, so it must not claim it has.");
        }

        /// <summary>
        /// An empty group is not a disclosure. Its toggle is disabled rather than pressable, and a press
        /// on it must not flip a state that would show an empty box.
        /// </summary>
        [Test]
        public void AnEmptyGroup_IsNotADisclosure()
        {
            var group = new PropertyGroup { label = "Nothing" };
            Assert.IsFalse(group.hasContent);
            Assert.IsTrue(group.ClassListContains(PropertyGroup.emptyUssClassName));
            Assert.IsFalse(group.toggleElement.enabledSelf);

            var raised = 0;
            group.openChanged += _ => raised++;
            group.clickable.InvokePressed(null);
            Assert.AreEqual(0, raised, "Pressing an empty group's header must do nothing.");
            Assert.IsTrue(group.open);
        }

        [Test]
        public void Level_AppliesOneLevelClassAtATime()
        {
            var group = new PropertyGroup { level = PropertyGroupLevel.Section };
            Assert.AreEqual("appui-property-group--level-section",
                group.GetClasses().Single(c => c.StartsWith(PropertyGroup.levelUssClassName)));

            group.level = PropertyGroupLevel.Sub;
            CollectionAssert.AreEqual(
                new[] { "appui-property-group--level-sub" },
                group.GetClasses().Where(c => c.StartsWith(PropertyGroup.levelUssClassName)).ToList());
        }

        /// <summary>
        /// Content in the trailing slot sits outside the toggle, so a control in it can be clicked
        /// without folding the group.
        /// </summary>
        [Test]
        public void TrailingContent_SitsOutsideTheToggle()
        {
            var group = Build("Transform", PropertyGroupLevel.Section, true);
            Assert.IsFalse(group.ClassListContains(PropertyGroup.withTrailingUssClassName));

            var button = new IconButton("dots-three-vertical");
            group.trailingContainer.Add(button);
            group.RefreshTrailing();

            Assert.IsTrue(group.ClassListContains(PropertyGroup.withTrailingUssClassName));
            Assert.AreSame(group.trailingContainer, button.hierarchy.parent);
            Assert.AreNotSame(group.toggleElement, button.hierarchy.parent);
        }

        /// <summary>
        /// The row gap is a leading margin on every row, so the first one has to be tagged for the
        /// stylesheet to zero it — UI Toolkit has neither <c>gap</c> nor <c>:first-child</c>.
        /// </summary>
        [Test]
        public void TheFirstRow_IsTaggedSoItCarriesNoLeadingGap()
        {
            var group = new PropertyGroup();
            var first = new VisualElement();
            var second = new VisualElement();
            group.Add(first);
            group.Add(second);
            group.RefreshTrailing();

            Assert.IsTrue(first.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(second.ClassListContains(Styles.firstChildUssClassName));
        }

        /// <summary>
        /// The registry hides an empty body (<c>empty:hidden</c>): a zero-height body still takes the
        /// stack's gap, so an open group with nothing in it grew by exactly one row gap.
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator AnOpenEmptyGroup_AddsNoGapBelowItsHeader()
        {
            var group = new PropertyGroup { label = "Empty" };
            yield return ChatTestUtils.Mount(m_TestUI, group);

            var header = group.Q(className: PropertyGroup.headerUssClassName);
            Assert.IsTrue(group.open, "The group under test must be open.");
            Assert.Greater(header.layout.height, 0f, "The header did not lay out.");
            Assert.AreEqual(header.layout.height, group.layout.height, 0.5f,
                "An open group with an empty body must be exactly as tall as its header.");
        }

        /// <summary>
        /// The empty body still has to notice a row arriving. Hiding it with <c>display: none</c> would
        /// stop its layout pass, and with it the refresh that clears the empty state, so the group would
        /// stay stuck empty.
        /// </summary>
        [UnityTest]
        public System.Collections.IEnumerator AnEmptyGroupThatGainsARow_GetsItsGapBack()
        {
            var group = new PropertyGroup { label = "Late" };
            yield return ChatTestUtils.Mount(m_TestUI, group);

            var row = new VisualElement { style = { height = 10f } };
            group.Add(row);
            for (var i = 0; i < 4; i++)
                yield return null;

            var header = group.Q(className: PropertyGroup.headerUssClassName);
            Assert.IsFalse(group.ClassListContains(PropertyGroup.emptyUssClassName),
                "A group that gained a row is no longer empty.");
            Assert.Greater(row.layout.height, 0f, "The row did not lay out.");
            Assert.Greater(group.layout.height, header.layout.height + row.layout.height + 0.5f,
                "The row gap below the header must come back once the body has a row.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_SpacesRowsAndSectionsFromTheInspectorTokens()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));

            StringAssert.Contains("var(--appui-alias-inspector-row-height)", uss,
                "The header row height must come from the inspector row-height token.");
            StringAssert.Contains("var(--appui-alias-inspector-row-gap)", uss,
                "The gap between rows must come from the inspector row-gap token.");
            StringAssert.Contains("var(--appui-alias-inspector-section-gap)", uss,
                "PropertyGroupStack must space sections with the inspector section-gap token.");
            Assert.IsFalse(Regex.IsMatch(uss, @"(height|margin-top|padding-left):\s*\d+(\.\d+)?px"),
                "PropertyGroup.uss inlines a literal dimension where a token belongs.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_UsesTokensOnly_NoLiteralColours()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"),
                "PropertyGroup.uss must not contain a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("),
                "PropertyGroup.uss must not contain a literal rgb()/rgba() colour.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_AvoidsSelectorsUssDoesNotSupport()
        {
            var uss = File.ReadAllText(Path.GetFullPath(k_UssPath));
            Assert.IsFalse(uss.Contains(":nth-child"), "USS rejects :nth-child, taking the file with it.");
            Assert.IsFalse(Regex.IsMatch(uss, @":[^;{}\r\n]*calc\("), "USS has no calc().");
            Assert.IsFalse(uss.Contains("::after"), "USS has no generated content.");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads a package source file, Editor only")]
        public void Uss_IsImportedByTheManifest()
        {
            var manifest = File.ReadAllText(Path.GetFullPath(
                "Packages/com.unity.dt.app-ui/PackageResources/Styles/App UI.uss"));
            StringAssert.Contains("@import \"./Components/PropertyGroup.uss\";", manifest,
                "A stylesheet missing from App UI.uss does nothing, silently.");
        }
    }

    [TestFixture]
    [TestOf(typeof(PropertyGroupStack))]
    class PropertyGroupStackTests : VisualElementTests<PropertyGroupStack>
    {
        protected override string mainUssClassName => PropertyGroupStack.ussClassName;

        protected override bool capturesSnapshots => false;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:PropertyGroupStack />",
        };

        [Test]
        public void TheFirstSection_IsTaggedSoItCarriesNoLeadingGap()
        {
            var stack = new PropertyGroupStack();
            var first = new PropertyGroup { label = "A" };
            var second = new PropertyGroup { label = "B" };
            stack.Add(first);
            stack.Add(second);
            stack.RefreshChildMarkers();

            Assert.IsTrue(first.ClassListContains(Styles.firstChildUssClassName));
            Assert.IsFalse(second.ClassListContains(Styles.firstChildUssClassName));
        }
    }
}
