using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using Unity.Properties;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Dropdown))]
    class DropdownTests : VisualElementTests<Dropdown>
    {
        protected override string mainUssClassName => Dropdown.ussClassName;

        // Handed over to DropdownSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        [UnityTest]
        [Order(10)]
        [TestCase(null, ExpectedResult = null)]
        [TestCase(new int[] {}, ExpectedResult = null)]
        [TestCase(new int[] { 0 }, ExpectedResult = null)]
        public IEnumerator Dropdown_WhenSettingSourceItems_ShouldResetValue(int[] defaultValue)
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);
            var dropdown = new Dropdown
            {
                defaultValue = defaultValue
            };

            var defaultNonNullValue = defaultValue ?? Array.Empty<int>();
            Assert.AreEqual(defaultValue, dropdown.defaultValue, "Default value should be set");

            var valueChangedNotified = 0;
            dropdown.RegisterValueChangedCallback(OnValueChanged);
            panel.Add(dropdown);

            yield return null;

            Assert.IsEmpty(dropdown.value, "Value should be empty if there are no source items");
            Assert.Zero(valueChangedNotified, "Value changed should not be notified by default");

            dropdown.sourceItems = new[] { "Item 1", "Item 2", "Item 3" };

            yield return null;

            CollectionAssert.AreEquivalent(defaultNonNullValue, dropdown.value, "Value should be set to the default value");
            var expectedNotified = defaultNonNullValue.Length == 0 ? 0 : 1;
            Assert.AreEqual(expectedNotified, valueChangedNotified, "Value changed should br notified when source items are changed " +
                "only if the new value is different than the previous one");

            dropdown.value = new[] {2};

            yield return null;

            CollectionAssert.AreEquivalent(new[] {2}, dropdown.value, "Value should be set to the new value");
            Assert.AreEqual(++expectedNotified, valueChangedNotified, "Value changed should be notified when value is changed");

            dropdown.sourceItems = new[] { "Item 1", "Item 2", "Item 3", "Item 4" };

            yield return null;

            CollectionAssert.AreEquivalent(defaultNonNullValue, dropdown.value, "Value should be set to the default value");
            Assert.AreEqual(++expectedNotified, valueChangedNotified);

            yield break;

            // --------------------------------------------------------------------------------------------

            void OnValueChanged(ChangeEvent<IEnumerable<int>> evt)
            {
                valueChangedNotified++;
            }
        }

        /// <summary>
        /// The registry's <c>select</c> trigger reads the field inset, not the Button's actions
        /// inset: sm for <c>sm</c>, md for every other size (#1395). The aliases resolve to the same
        /// step today, so only the source can tell which variant a size chose.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Trigger_HorizontalInset_ComesFromTheFieldPaddingAlias()
        {
            foreach (var (size, token) in new[] { ("sm", "sm"), ("md", "md"), ("lg", "md") })
                UssSource.AssertHorizontalInset("Picker.uss", ".appui-picker--size-" + size,
                    "--appui-alias-field-padding-horizontal-" + token);

            var root = UssSource.RuleBlock(UssSource.Read("Picker.uss"), ".appui-picker");
            StringAssert.DoesNotContain("control-gap-md", root,
                "The base rule must not keep the old control-gap inset.");
        }

        /// <summary>
        /// A disabled row takes the disabled text colour, never opacity, which compounds down a
        /// disabled subtree (#1419). The checkmark paints its own tint, so it needs its own
        /// disabled override or a disabled selected row shows a full-strength check.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void DisabledItem_UsesTheDisabledColourTokens_NotOpacity()
        {
            var picker = UssSource.Read("Picker.uss");
            var item = UssSource.RuleBlock(picker, ".appui-picker-item:disabled");
            Assert.IsNotNull(item, "Expected a .appui-picker-item:disabled rule.");
            StringAssert.DoesNotContain("opacity", item, "A disabled item must not dim with opacity.");
            StringAssert.IsMatch(@"(?m)^\s*color:\s*var\(--appui-alias-text-disabled\);", item);
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-text-disabled);", item);

            // Icon.uss tints `.unity-disabled .appui-icon` directly, which beats the inherited tint,
            // and its foregrounds-25 differs from text-disabled in Cloud Dark and Editor Dark.
            var icon = UssSource.RuleBlock(picker, ".appui-picker-item:disabled .appui-icon");
            Assert.IsNotNull(icon, "Expected a disabled override for a row's icons.");
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-text-disabled);", icon);

            var check = UssSource.RuleBlock(UssSource.Read("Dropdown.uss"),
                ".appui-picker-item:disabled .appui-dropdown-item__checkmark");
            Assert.IsNotNull(check, "Expected a disabled override for the Dropdown checkmark.");
            StringAssert.IsMatch(@"(?m)^\s*color:\s*var\(--appui-alias-text-disabled\);", check);
            StringAssert.Contains("--unity-image-tint-color: var(--appui-alias-text-disabled);", check);
        }

        [UnityTest]
        public IEnumerator DisabledItem_KeepsFullOpacityAndChangesColour()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            m_TestUI.rootVisualElement.Add(panel);

            PickerItem MakeItem()
            {
                var row = new PickerItem();
                row.Add(new DropdownItem { label = "Row" });
                row.AddToClassList("is-selected");
                return row;
            }

            var enabled = MakeItem();
            var disabled = MakeItem();
            disabled.SetEnabled(false);
            panel.Add(enabled);
            panel.Add(disabled);
            yield return null;
            yield return null;

            Assert.AreEqual(1f, disabled.resolvedStyle.opacity, "A disabled item must not dim with opacity.");
            Assert.AreNotEqual(enabled.resolvedStyle.color, disabled.resolvedStyle.color,
                "A disabled item must take the disabled text colour.");

            var enabledCheck = enabled.Q(className: DropdownItem.checkmarkUssClassName);
            var disabledCheck = disabled.Q(className: DropdownItem.checkmarkUssClassName);
            Assert.AreNotEqual(enabledCheck.resolvedStyle.color, disabledCheck.resolvedStyle.color,
                "A disabled selected row's checkmark must go disabled too.");
        }

        /// <summary>
        /// The registry's <c>select</c> trigger is a <c>Button</c>: <c>text-sm</c> at <c>sm</c> and
        /// <c>text-base</c> otherwise. Its rows are <c>MenuItem</c>s, <c>text-base</c> at every size;
        /// they sit in a popover, so they do not inherit the trigger's step.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var dropdowns = new List<(Size size, Dropdown dropdown)>();
                PickerItem row = null;
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var dropdown = new Dropdown(new[] { "Ag" }) { size = size };
                        panel.Add(dropdown);
                        dropdowns.Add((size, dropdown));
                    }
                    row = new PickerItem();
                    row.Add(new DropdownItem { label = "Ag" });
                    panel.Add(row);
                }, s => stage = s);

                foreach (var (size, dropdown) in dropdowns)
                {
                    var title = dropdown.Q<DropdownItem>()?.labelElement;
                    FieldTextSize.AssertTier(stage, title, FieldTextSize.TextFieldTier(size),
                        $"a {size} Dropdown's trigger");
                    FieldTextSize.AssertFits(stage, title, dropdown, $"a {size} Dropdown's trigger");
                }
                FieldTextSize.AssertTier(stage, row.Q<DropdownItem>().labelElement, "base", "a Dropdown row");
            }
        }

        class DescriptionSink
        {
            string m_Text;
            public int writes;

            [CreateProperty]
            public string text
            {
                get => m_Text;
                set
                {
                    m_Text = value;
                    writes++;
                }
            }
        }

        /// <summary>
        /// A to-source binding only hears about a change through NotifyPropertyChanged, so this
        /// covers the round trip, the BindingId and the change guard at once.
        /// </summary>
        [UnityTest]
        public IEnumerator ItemDescription_RoundTripsAndNotifiesOnlyOnChange()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark" };
            m_TestUI.rootVisualElement.Add(panel);
            var item = new DropdownItem();
            panel.Add(item);
            var sink = new DescriptionSink();
            item.SetBinding(DropdownItem.descriptionProperty, new DataBinding
            {
                dataSource = sink,
                dataSourcePath = new PropertyPath(nameof(DescriptionSink.text)),
                bindingMode = BindingMode.ToSource,
            });
            yield return null;
            yield return null;
            var baseline = sink.writes;

            Assert.IsNull(item.description);
            item.description = "Secondary line";
            Assert.AreEqual("Secondary line", item.description);
            yield return null;
            yield return null;
            Assert.AreEqual("Secondary line", sink.text, "A new description should reach a bound source.");
            Assert.AreEqual(baseline + 1, sink.writes, "Setting a new description should notify once.");

            item.description = "Secondary line";
            yield return null;
            yield return null;
            Assert.AreEqual(baseline + 1, sink.writes, "Setting the same description must not notify.");
        }

        [UnityTest]
        public IEnumerator ItemDescription_IsHiddenWhenEmpty_AndShownOtherwise()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel { theme = "dark" };
            m_TestUI.rootVisualElement.Add(panel);
            var row = new PickerItem();
            var item = new DropdownItem { label = "Row" };
            row.Add(item);
            panel.Add(row);
            yield return null;
            yield return null;

            var description = item.Q<TextElement>(className: DropdownItem.descriptionUssClassName);
            Assert.IsNotNull(description, "A DropdownItem should own a description element.");
            Assert.AreEqual(DisplayStyle.None, description.resolvedStyle.display, "No description: the line is hidden.");

            item.description = "Secondary line";
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, description.resolvedStyle.display, "A description shows its line.");
            Assert.AreEqual("Secondary line", description.text);
            Assert.Greater(description.layout.height, 0f, "The description line should take up room.");

            item.description = "";
            yield return null;
            yield return null;
            Assert.AreEqual(DisplayStyle.None, description.resolvedStyle.display, "An empty description hides the line again.");
        }

        /// <summary>
        /// The registry's MenuItem description is <c>text-sm</c> in <c>--appui-alias-text-secondary</c>,
        /// under the row's <c>text-base</c> label.
        /// </summary>
        [UnityTest]
        public IEnumerator ItemDescription_ReadsTextSecondary_AtTheSmStep()
        {
            foreach (var density in FieldTextSize.densities)
            {
                DropdownItem item = null;
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    var row = new PickerItem();
                    item = new DropdownItem { label = "Ag", description = "Ag" };
                    row.Add(item);
                    panel.Add(row);
                }, s => stage = s);

                var description = item.Q<TextElement>(className: DropdownItem.descriptionUssClassName);
                FieldTextSize.AssertTier(stage, description, "sm", "a Dropdown row's description");
                FieldTextSize.AssertTier(stage, item.labelElement, "base", "a Dropdown row's label");

                var vars = UssTheme.LoadVars("App UI - Dark.class.uss");
                Assert.IsTrue(UssTheme.TryResolveColour(vars, "--appui-alias-text-secondary", out var secondary));
                Assert.IsTrue(UssTheme.TryResolveColour(vars, "--appui-alias-text-primary", out var primary));
                Assert.AreNotEqual(primary, secondary, "The probe needs primary and secondary to differ.");
                var actual = description.resolvedStyle.color;
                Assert.AreEqual(secondary.r, actual.r, 0.01f, "description colour (r)");
                Assert.AreEqual(secondary.g, actual.g, 0.01f, "description colour (g)");
                Assert.AreEqual(secondary.b, actual.b, 0.01f, "description colour (b)");
                Assert.AreEqual(secondary.a, actual.a, 0.01f, "description colour (a)");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in FieldTextSize.sizes)
                FieldTextSize.AssertFontSize("Picker.uss", ".appui-picker--size-" + suffix,
                    FieldTextSize.Step(FieldTextSize.TextFieldTier(size)));
            FieldTextSize.AssertFontSize("Picker.uss", ".appui-picker-item", 200);
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("Picker.uss"),
                "Picker reads the numbered steps the registry reads, never a body-* alias.");
        }
    }
}
