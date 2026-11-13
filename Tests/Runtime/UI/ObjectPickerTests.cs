using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ObjectPicker))]
    class ObjectPickerTests : VisualElementTests<ObjectPicker>
    {
        protected override string mainUssClassName => ObjectPicker.ussClassName;

        static List<ObjectPickerItem> Items() => new List<ObjectPickerItem>
        {
            new ObjectPickerItem { id = "bronze", label = "Bronze", type = "Material", path = "Assets/Bronze.mat", tab = "assets" },
            new ObjectPickerItem { id = "brass", label = "Brass", type = "Material", path = "Assets/Brass.mat", tab = "assets" },
            new ObjectPickerItem { id = "cube", label = "Cube", type = "GameObject", path = "/Cube", tab = "scene" },
            new ObjectPickerItem { id = "shared", label = "Shared", type = "Material" },
        };

        static ObjectPicker Picker()
        {
            var picker = new ObjectPicker();
            picker.SetItems(Items());
            return picker;
        }

        // Tabs and the search bar report through ChangeEvents, which only dispatch through a
        // panel — any test that drives them has to work on an attached picker.
        IEnumerator Attach(ObjectPicker picker)
        {
            m_TestUI.rootVisualElement.Add(picker);
            yield return null;
        }

        static List<VisualElement> Rows(ObjectPicker picker) =>
            picker.Query<VisualElement>(className: ObjectPicker.rowUssClassName).ToList();

        static List<string> RowIds(ObjectPicker picker) =>
            Rows(picker).Select(r => ((ObjectPickerItem)r.userData).id).ToList();

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => Sized(Picker()));
                yield return new Story("Titled", ctx =>
                {
                    var picker = Picker();
                    picker.title = "Select Material";
                    picker.showCloseButton = true;
                    picker.value = "bronze";
                    return Sized(picker);
                });
                yield return new Story("NamedFallback", ctx =>
                {
                    var picker = Picker();
                    picker.noneLabel = "Inter (Default)";
                    picker.noneIconName = "text-aa";
                    return Sized(picker);
                });
                yield return new Story("NoSlider", ctx =>
                {
                    var picker = Picker();
                    picker.showSizeSlider = false;
                    picker.thumbSize = ObjectPickerThumbSize.Sm;
                    return Sized(picker);
                });
            }
        }

        // The picker is frameless and grows into its host, so a story needs a sized box.
        static VisualElement Sized(ObjectPicker picker)
        {
            var host = new VisualElement { style = { width = 320, height = 380 } };
            host.Add(picker);
            return host;
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ObjectPicker />",
            @"<appui:ObjectPicker title=""Select Material"" show-close-button=""true"" />",
            @"<appui:ObjectPicker show-none=""false"" show-size-slider=""false"" />",
            @"<appui:ObjectPicker none-label=""Inter (Default)"" none-icon-name=""text-aa"" />",
        };

        // ------------------------------------------------------------------ none row

        [Test]
        public void NoneRow_IsOfferedByDefault_AndPinsToTheTop()
        {
            var picker = Picker();
            var ids = RowIds(picker);
            Assert.AreEqual(ObjectPicker.noneId, ids[0],
                "A Unity-style object field must always offer 'no assignment', at the top.");
        }

        [Test]
        public void NoneRow_IsSelectedWhenNothingIsAssigned()
        {
            var picker = Picker();
            var none = Rows(picker)[0];
            Assert.IsTrue(none.ClassListContains(Styles.selectedUssClassName));
        }

        [UnityTest]
        public IEnumerator NoneRow_IgnoresTheSearchText()
        {
            var picker = Picker();
            yield return Attach(picker);

            picker.searchBar.value = "zzzz";
            var ids = RowIds(picker);
            Assert.AreEqual(1, ids.Count, "Only the none row should survive a search that matches nothing.");
            Assert.AreEqual(ObjectPicker.noneId, ids[0]);
        }

        [Test]
        public void NoneRow_WithNoIcon_IsStyledAsAPlaceholder_ButWithAnIconItIsARegularRow()
        {
            var picker = Picker();
            Assert.IsTrue(Rows(picker)[0].ClassListContains(ObjectPicker.noneRowUssClassName),
                "Without an icon the none row is an absence and reads as a muted placeholder.");

            picker.noneIconName = "text-aa";
            Assert.IsFalse(Rows(picker)[0].ClassListContains(ObjectPicker.noneRowUssClassName),
                "With an icon the none row IS an asset to the user — a named engine fallback — so "
                + "it must lose the placeholder treatment.");
        }

        [Test]
        public void ShowNone_False_DropsTheRow()
        {
            var picker = Picker();
            picker.showNone = false;
            CollectionAssert.DoesNotContain(RowIds(picker), ObjectPicker.noneId);
        }

        // ------------------------------------------------------------------ filtering

        [UnityTest]
        public IEnumerator Tabs_FilterTheList_AndUntaggedItemsShowEverywhere()
        {
            var picker = Picker();
            yield return Attach(picker);

            // Default tab is the first one, "assets".
            var assets = RowIds(picker);
            CollectionAssert.Contains(assets, "bronze");
            CollectionAssert.Contains(assets, "shared");
            CollectionAssert.DoesNotContain(assets, "cube");

            picker.tabsElement.value = 1; // scene
            var scene = RowIds(picker);
            CollectionAssert.Contains(scene, "cube");
            CollectionAssert.Contains(scene, "shared");
            CollectionAssert.DoesNotContain(scene, "bronze");
        }

        [UnityTest]
        public IEnumerator Search_IsCaseInsensitiveAndSubstring()
        {
            var picker = Picker();
            yield return Attach(picker);

            picker.searchBar.value = "RON";
            CollectionAssert.Contains(RowIds(picker), "bronze");
            CollectionAssert.DoesNotContain(RowIds(picker), "brass");
        }

        [UnityTest]
        public IEnumerator TabChanged_ReportsTheTabValue()
        {
            var picker = Picker();
            yield return Attach(picker);

            string reported = null;
            picker.tabChanged += v => reported = v;

            picker.tabsElement.value = 1;
            Assert.AreEqual("scene", reported);
        }

        // ------------------------------------------------------------------ selection

        [Test]
        public void Value_MarksTheRowSelected_AndClearsThePreviousOne()
        {
            var picker = Picker();
            picker.value = "bronze";

            var selected = Rows(picker).Where(r => r.ClassListContains(Styles.selectedUssClassName)).ToList();
            Assert.AreEqual(1, selected.Count, "Exactly one row must be selected at a time.");
            Assert.AreEqual("bronze", ((ObjectPickerItem)selected[0].userData).id);

            picker.value = "brass";
            selected = Rows(picker).Where(r => r.ClassListContains(Styles.selectedUssClassName)).ToList();
            Assert.AreEqual(1, selected.Count, "A stale selection class survived a value change.");
            Assert.AreEqual("brass", ((ObjectPickerItem)selected[0].userData).id);
        }

        // Events only dispatch through a panel, so anything asserting on one has to be attached.
        [UnityTest]
        public IEnumerator Value_SendsAChangeEvent()
        {
            var picker = Picker();
            m_TestUI.rootVisualElement.Add(picker);
            yield return null;

            var seen = 0;
            string newValue = null;
            var targets = new List<string>();
            picker.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                seen++;
                newValue = evt.newValue;
                targets.Add((evt.target?.GetType().Name ?? "null") + "=" + evt.newValue);
            });

            picker.value = "bronze";
            // Exactly one, from the picker itself. The search field and every
            // LocalizedTextElement in the preview also send ChangeEvent<string>, and they bubble,
            // so a consumer would otherwise read the preview's metadata as a selection change.
            Assert.AreEqual(1, seen, "Foreign ChangeEvent<string> leaked out of the picker: "
                + string.Join(", ", targets));
            Assert.AreEqual("bronze", newValue);
        }

        [Test]
        public void NullValue_NormalisesToTheNoneId()
        {
            var picker = Picker();
            picker.value = "bronze";
            picker.value = null;
            Assert.AreEqual(ObjectPicker.noneId, picker.value);
            Assert.IsTrue(Rows(picker)[0].ClassListContains(Styles.selectedUssClassName));
        }

        [Test]
        public void Preview_FollowsTheSelection_AndCarriesTheRealItem()
        {
            var picker = Picker();
            // The registry previews the cleared reference too — that is what noneLabel is for —
            // so "nothing assigned" shows the none item, not the empty state.
            Assert.AreEqual(ObjectPicker.noneId, picker.previewElement.item.id);
            Assert.AreEqual("(None)", picker.previewElement.item.label);

            picker.value = "bronze";
            Assert.IsNotNull(picker.previewElement.item);
            Assert.AreEqual("Bronze", picker.previewElement.item.label);
            Assert.AreEqual("Assets/Bronze.mat", picker.previewElement.item.path,
                "The preview must receive the real item, not the synthetic row copy — otherwise "
                + "its metadata is silently empty.");
        }

        [UnityTest]
        public IEnumerator SelectionSurvivesAReFilter()
        {
            var picker = Picker();
            yield return Attach(picker);

            picker.value = "shared";
            picker.tabsElement.value = 1; // shared is untagged, so it is on every tab

            var selected = Rows(picker).Where(r => r.ClassListContains(Styles.selectedUssClassName)).ToList();
            Assert.AreEqual(1, selected.Count);
            Assert.AreEqual("shared", ((ObjectPickerItem)selected[0].userData).id);
        }

        [UnityTest]
        public IEnumerator Preview_KeepsTheSelectedItemWhenTheFilterHidesItsRow()
        {
            var picker = Picker();
            yield return Attach(picker);

            picker.value = "bronze";
            picker.searchBar.value = "zzzz";

            Assert.AreEqual("bronze", picker.value, "Filtering does not change the value.");
            Assert.IsNotNull(picker.previewElement.item,
                "The preview must resolve from the full items collection, not the visible rows — "
                + "a search that hides the selected row does not unassign it.");
            Assert.AreEqual("Bronze", picker.previewElement.item.label);
        }

        // ------------------------------------------------------------------ chrome

        [Test]
        public void ThumbSize_DefaultsToMd_AndSwapsItsClassCleanly()
        {
            var picker = new ObjectPicker();
            Assert.AreEqual(ObjectPickerThumbSize.Md, picker.thumbSize);

            foreach (var (size, suffix) in new[]
                     {
                         (ObjectPickerThumbSize.Sm, "sm"),
                         (ObjectPickerThumbSize.Md, "md"),
                         (ObjectPickerThumbSize.Lg, "lg"),
                     })
            {
                picker.thumbSize = size;
                Assert.IsTrue(picker.ClassListContains(ObjectPicker.ussClassName + "--thumb-" + suffix));

                foreach (var other in new[] { "sm", "md", "lg" })
                {
                    if (other == suffix)
                        continue;
                    Assert.IsFalse(picker.ClassListContains(ObjectPicker.ussClassName + "--thumb-" + other),
                        $"Stale thumb class --thumb-{other} survived a switch to --thumb-{suffix}.");
                }
            }
        }

        [Test]
        public void Header_IsHiddenUntilItHasSomethingToShow()
        {
            var picker = new ObjectPicker();
            var header = picker.Q<VisualElement>(className: ObjectPicker.headerUssClassName);
            Assert.IsNotNull(header);
            Assert.IsTrue(header.ClassListContains(Styles.hiddenUssClassName),
                "An empty header must not eat vertical space in a small popover.");

            picker.title = "Select Material";
            Assert.IsFalse(header.ClassListContains(Styles.hiddenUssClassName));
        }

        [UnityTest]
        public IEnumerator CloseButton_RaisesCloseRequested()
        {
            var picker = new ObjectPicker { showCloseButton = true };
            yield return Attach(picker);

            var closed = 0;
            picker.closeRequested += () => closed++;

            var button = picker.Q<IconButton>(className: ObjectPicker.closeButtonUssClassName);
            Assert.IsNotNull(button);
            button.clickable.InvokePressed(null);
            Assert.AreEqual(1, closed);
        }

        [Test]
        public void SetTabs_Empty_HidesTheStrip_AndStopsFilteringByTab()
        {
            var picker = Picker();
            picker.SetTabs(null);

            Assert.IsTrue(picker.tabsElement.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsNull(picker.currentTab);

            var ids = RowIds(picker);
            foreach (var id in new[] { "bronze", "brass", "cube", "shared" })
                CollectionAssert.Contains(ids, id, "With no tabs every item must be reachable.");
        }

        // ------------------------------------------------------------------ preview context

        [Test]
        public void Preview_StartsWithAContextClass()
        {
            var preview = new ObjectPickerPreview();
            Assert.IsTrue(preview.ClassListContains(ObjectPickerPreview.ussClassName + "--context-normal"),
                "The context class must be on the element from construction. The geometry callback "
                + "only fires once the pane has a size, and an unclassed preview has no layout.");
        }

        [Test]
        public void Preview_EmptyState_ShowsThePlaceholderAndHidesTheBody()
        {
            var preview = new ObjectPickerPreview();
            var empty = preview.Q<LocalizedTextElement>(className: ObjectPickerPreview.emptyUssClassName);
            var meta = preview.Q<VisualElement>(className: ObjectPickerPreview.metaUssClassName);

            Assert.IsFalse(empty.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(meta.ClassListContains(Styles.hiddenUssClassName));

            preview.item = new ObjectPickerItem { id = "a", label = "A", type = "Material" };
            Assert.IsTrue(empty.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsFalse(meta.ClassListContains(Styles.hiddenUssClassName));
        }

        [Test]
        public void Preview_HidesMetadataRowsThatHaveNoContent()
        {
            var preview = new ObjectPickerPreview
            {
                item = new ObjectPickerItem { id = "a", label = "A" },
            };
            var type = preview.Q<LocalizedTextElement>(className: ObjectPickerPreview.typeUssClassName);
            var path = preview.Q<LocalizedTextElement>(className: ObjectPickerPreview.pathUssClassName);

            Assert.IsTrue(type.ClassListContains(Styles.hiddenUssClassName));
            Assert.IsTrue(path.ClassListContains(Styles.hiddenUssClassName));
        }

        // ------------------------------------------------------------------ USS source

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void EveryClassAddedFromCSharp_HasAMatchingUssRule()
        {
            var uss = UssSource.Read("ObjectPicker.uss");

            foreach (var selector in new[]
                     {
                         "." + ObjectPicker.ussClassName,
                         "." + ObjectPicker.headerUssClassName,
                         "." + ObjectPicker.titleUssClassName,
                         "." + ObjectPicker.closeButtonUssClassName,
                         "." + ObjectPicker.searchUssClassName,
                         "." + ObjectPicker.toolbarUssClassName,
                         "." + ObjectPicker.tabsUssClassName,
                         "." + ObjectPicker.sizeSliderUssClassName,
                         "." + ObjectPicker.toolbarSlotUssClassName,
                         "." + ObjectPicker.splitViewUssClassName,
                         "." + ObjectPicker.listUssClassName,
                         "." + ObjectPicker.rowUssClassName,
                         "." + ObjectPicker.noneRowUssClassName,
                         "." + ObjectPicker.rowThumbnailUssClassName,
                         "." + ObjectPicker.rowLabelUssClassName,
                         "." + ObjectPicker.noResultsUssClassName,
                         "." + ObjectPicker.previewUssClassName,
                     })
            {
                StringAssert.Contains(selector, uss,
                    $"{selector} is added from C# but ObjectPicker.uss never mentions it, so that "
                    + "element renders unstyled and nothing errors.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void PreviewContexts_AllHaveAUssRule()
        {
            var uss = UssSource.Read("ObjectPickerPreview.uss");

            foreach (var suffix in new[] { "compact", "normal", "large" })
            {
                var selector = "." + ObjectPickerPreview.ussClassName + "--context-" + suffix;
                Assert.IsNotNull(UssSource.RuleBlock(uss, selector),
                    $"{selector} is emitted by the [EnumName] generator from "
                    + $"{nameof(ObjectPickerPreviewContext)} but has no rule, so that context has "
                    + "no layout at all and nothing warns.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void ThumbSizeSteps_AllHaveAUssRule()
        {
            var uss = UssSource.Read("ObjectPicker.uss");

            foreach (var suffix in new[] { "sm", "md", "lg" })
            {
                StringAssert.Contains(
                    "." + ObjectPicker.ussClassName + "--thumb-" + suffix + " ", uss,
                    $"The --thumb-{suffix} step must actually size the row thumbnails.");
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Colors_ComeFromTokens_NotLiterals()
        {
            UssSource.AssertNoLiteralColors("ObjectPicker.uss");
            UssSource.AssertNoLiteralColors("ObjectPickerPreview.uss");
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Dimensions_ComeFromTokens_NotMagicPixels()
        {
            foreach (var file in new[] { "ObjectPicker.uss", "ObjectPickerPreview.uss" })
            {
                var uss = UssSource.Read(file);
                var magic = System.Text.RegularExpressions.Regex.Matches(uss, @"(?<![\w-])\d+px")
                    .Cast<System.Text.RegularExpressions.Match>()
                    .Select(m => m.Value)
                    .ToList();
                Assert.AreEqual(0, magic.Count,
                    $"{file} must not hardcode dimensions: " + string.Join(", ", magic));
            }
        }
    }
}
