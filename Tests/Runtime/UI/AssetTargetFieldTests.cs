using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(AssetTargetField))]
    class AssetTargetFieldTests : VisualElementTests<AssetTargetField>
    {
        protected override string mainUssClassName => AssetTargetField.ussClassName;

        // Handed over to AssetTargetFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        static readonly (Size size, string suffix)[] k_Sizes =
        {
            (Size.Sm, "sm"),
            (Size.Md, "md"),
            (Size.Lg, "lg"),
        };

        static AssetReference Bronze() => new AssetReference
        {
            name = "Bronze",
            type = "Material",
            typeIcon = "sphere",
        };

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new AssetTargetField());
                yield return new Story("Assigned", ctx => new AssetTargetField { value = Bronze() });
                yield return new Story("Sizes", ctx =>
                {
                    var container = new VisualElement();
                    foreach (var (size, _) in k_Sizes)
                        container.Add(new AssetTargetField { size = size, value = Bronze() });
                    return container;
                });
                yield return new Story("NoClear", ctx => new AssetTargetField
                {
                    value = Bronze(),
                    showClearButton = false,
                });
                yield return new Story("Invalid", ctx => new AssetTargetField
                {
                    value = Bronze(),
                    validateValue = v => v != null && v.name == "Gold",
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:AssetTargetField />",
            @"<appui:AssetTargetField size=""Sm"" />",
            @"<appui:AssetTargetField size=""Md"" />",
            @"<appui:AssetTargetField size=""Lg"" />",
            @"<appui:AssetTargetField empty-label=""Nothing"" show-clear-button=""false"" />",
        };

        static IconButton Clear(AssetTargetField f) => f.clearButton;

        static IconButton Pick(AssetTargetField f) => f.pickButton;

        // ------------------------------------------------------------------ size

        [Test]
        public void Size_DefaultsToMd()
        {
            var field = new AssetTargetField();
            Assert.AreEqual(Size.Md, field.size);
            Assert.IsTrue(field.ClassListContains(AssetTargetField.ussClassName + "--size-md"),
                "The default control size must be md, and it must be on the class list.");
        }

        [Test]
        public void Size_EmitsItsClass_AndDropsTheStaleOne()
        {
            var field = new AssetTargetField();

            foreach (var (size, suffix) in k_Sizes)
            {
                field.size = size;
                Assert.IsTrue(field.ClassListContains(AssetTargetField.ussClassName + "--size-" + suffix),
                    $"Setting size to {size} must add the --size-{suffix} class.");

                foreach (var (_, other) in k_Sizes)
                {
                    if (other == suffix)
                        continue;
                    Assert.IsFalse(field.ClassListContains(AssetTargetField.ussClassName + "--size-" + other),
                        $"Stale size class --size-{other} survived a switch to --size-{suffix}.");
                }
            }
        }

        // ------------------------------------------------------------------ value

        [Test]
        public void EmptyValue_ShowsTheEmptyLabel()
        {
            var field = new AssetTargetField();
            var label = field.Q<LocalizedTextElement>(className: AssetTargetField.labelUssClassName);
            Assert.IsNotNull(label);
            Assert.AreEqual("None", label.text, "The default empty label must match the registry's 'None'.");

            field.emptyLabel = "Nothing";
            Assert.AreEqual("Nothing", label.text);
        }

        [Test]
        public void AssignedValue_ShowsTheAssetName()
        {
            var field = new AssetTargetField { value = Bronze() };
            var label = field.Q<LocalizedTextElement>(className: AssetTargetField.labelUssClassName);
            Assert.AreEqual("Bronze", label.text);
        }

        // Events only dispatch through a panel, so anything asserting on one has to be attached.
        [UnityTest]
        public IEnumerator Value_SendsAChangeEvent()
        {
            var field = new AssetTargetField();
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var seen = 0;
            AssetReference newValue = null;
            field.RegisterCallback<ChangeEvent<AssetReference>>(evt =>
            {
                seen++;
                newValue = evt.newValue;
            });

            var bronze = Bronze();
            field.value = bronze;
            Assert.AreEqual(1, seen);
            Assert.AreSame(bronze, newValue);
        }

        // ------------------------------------------------------------------ icon

        [Test]
        public void Icon_PrefersTheValuesOwnGlyph_ThenTheTypeName_ThenTheDefault()
        {
            var field = new AssetTargetField();
            Assert.AreEqual("cube", field.iconElement.iconName,
                "With no value and no type the leading icon must still resolve to something; an "
                + "empty iconName renders nothing and the label jumps left.");

            field.type = typeof(UnityEngine.Material);
            Assert.AreEqual("material", field.iconElement.iconName);

            field.value = Bronze();
            Assert.AreEqual("sphere", field.iconElement.iconName,
                "The value's own typeIcon must win over the accepted type's name.");
        }

        // ------------------------------------------------------------------ buttons

        [Test]
        public void ClearButton_IsHiddenWhileEmpty_AndShownOnceAssigned()
        {
            var field = new AssetTargetField();
            Assert.IsTrue(Clear(field).ClassListContains(Styles.hiddenUssClassName),
                "There is nothing to clear while the field is empty, so the button must not take "
                + "layout space.");

            field.value = Bronze();
            Assert.IsFalse(Clear(field).ClassListContains(Styles.hiddenUssClassName));

            field.value = null;
            Assert.IsTrue(Clear(field).ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void ShowClearButton_False_KeepsTheButtonHidden_EvenWhenAssigned()
        {
            var field = new AssetTargetField { showClearButton = false, value = Bronze() };
            Assert.IsTrue(Clear(field).ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void Clear_ResetsTheValue_AndRaisesCleared()
        {
            var field = new AssetTargetField { value = Bronze() };
            var cleared = 0;
            field.cleared += () => cleared++;

            field.ClearValue();
            Assert.IsNull(field.value);
            Assert.AreEqual(1, cleared);

            // Already empty — nothing to report.
            field.ClearValue();
            Assert.AreEqual(1, cleared);
        }

        [Test]
        public void PickButton_Exists_AndIsAlwaysVisible()
        {
            var field = new AssetTargetField();
            Assert.IsNotNull(Pick(field));
            Assert.IsFalse(Pick(field).ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void OpenPicker_RaisesPickRequested_EvenWithNoPickerAssigned()
        {
            var field = new AssetTargetField();
            var asked = 0;
            field.pickRequested += () => asked++;

            field.OpenPicker();
            Assert.AreEqual(1, asked,
                "The field must report the intent to pick even when no picker host is wired up; "
                + "that is what lets it ship before ObjectPicker is hosted.");
        }

        [Test]
        public void OpenPicker_ForwardsToTheCustomPicker_WithTheTypeFilter()
        {
            var picker = new RecordingPicker();
            var field = new AssetTargetField { type = typeof(UnityEngine.Material), customPicker = picker };

            field.OpenPicker();
            Assert.AreEqual(1, picker.shown);
            Assert.AreSame(field, picker.lastOwner);
            Assert.AreEqual(typeof(UnityEngine.Material), picker.lastType);
        }

        [UnityTest]
        public IEnumerator CustomPicker_ValueChanging_UpdatesTheFieldAndSendsAChangingEvent()
        {
            var picker = new RecordingPicker();
            var field = new AssetTargetField { customPicker = picker };
            m_TestUI.rootVisualElement.Add(field);
            yield return null;

            var changing = 0;
            field.RegisterCallback<ChangingEvent<AssetReference>>(_ => changing++);

            field.OpenPicker();
            picker.Emit(Bronze());

            Assert.AreEqual("Bronze", field.value.name);
            Assert.AreEqual(1, changing);
        }

        class RecordingPicker : ICustomAssetTargetFieldPicker
        {
            public int shown;
            public AssetTargetField lastOwner;
            public System.Type lastType;
            System.Action<AssetReference> m_OnChanging;

            public void Show(AssetTargetField owner, System.Action<AssetReference> onValueChanging,
                AssetReference initialValue, System.Type type)
            {
                shown++;
                lastOwner = owner;
                lastType = type;
                m_OnChanging = onValueChanging;
            }

            public void Emit(AssetReference value) => m_OnChanging?.Invoke(value);
        }

        // ------------------------------------------------------------------ USS source

        /// <summary>
        /// The leading icon's C# class and its USS rule have to agree. They did not: C# added
        /// <c>__icon</c> while the stylesheet only ever styled <c>__leading-icon</c>, so the icon
        /// had no styling at all and nothing warned.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void EveryClassAddedFromCSharp_HasAMatchingUssRule()
        {
            var uss = UssSource.Read("AssetTargetField.uss");

            foreach (var selector in new[]
                     {
                         "." + AssetTargetField.ussClassName,
                         "." + AssetTargetField.iconUssClassName,
                         "." + AssetTargetField.labelUssClassName,
                         "." + AssetTargetField.clearButtonUssClassName,
                         "." + AssetTargetField.pickButtonUssClassName,
                     })
            {
                StringAssert.Contains(selector, uss,
                    $"{selector} is added from C# but AssetTargetField.uss never mentions it, so "
                    + "that element renders unstyled and nothing errors.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Height_ComesFromTheControlSizingAlias_ForEverySize()
        {
            UssSource.AssertHeightsComeFromControlSizingAliases(
                "AssetTargetField.uss", "." + AssetTargetField.ussClassName);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("AssetTargetField.uss");
        }

        /// <summary>
        /// The registry's <c>asset-target-field</c> root is <c>text-base</c> and its size classes carry
        /// no text class, so it keeps the base step at every size. UITK takes it at <c>Md</c> and
        /// <c>Lg</c>, and deliberately steps <c>Sm</c> down to <c>text-sm</c>: a <c>text-base</c> line
        /// overruns the compact <c>Sm</c> box and the label clips it.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheFieldStep_AndFits_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, AssetTargetField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new AssetTargetField { size = size };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var label = field.Q<TextElement>(className: AssetTargetField.labelUssClassName);
                    FieldTextSize.AssertTier(stage, label, FieldTextSize.TextFieldTier(size),
                        $"a {size} AssetTargetField's label");
                    FieldTextSize.AssertFits(stage, label, field, $"a {size} AssetTargetField's label");
                }
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            FieldTextSize.AssertFontSize("AssetTargetField.uss", ".appui-assettargetfield__label", 200);
            FieldTextSize.AssertFontSize("AssetTargetField.uss",
                ".appui-assettargetfield--size-sm .appui-assettargetfield__label", 100);
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("AssetTargetField.uss"),
                "AssetTargetField reads the numbered step the registry reads, never a body-* alias.");
        }
    }
}
