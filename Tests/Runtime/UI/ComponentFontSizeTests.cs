using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Guards component text sizes against the registry's tiers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The registry reads its text tiers through <c>TEXT_TIERS</c> in <c>build-registry.ts</c>:
    /// <c>text-xs</c> is <c>--appui-font-sizes-75</c>, <c>text-sm</c> is <c>-100</c> and
    /// <c>text-base</c> is <c>-200</c>. The <c>--appui-font-sizes-body-*</c> aliases sit one name
    /// off that scale (<c>body-md</c> is <c>-100</c>, the registry's <c>text-sm</c>), so a rule
    /// that reaches for <c>body-md</c> as "the default body size" renders a tier below the web.
    /// Component stylesheets read the numbered step instead, the way the fields already do.
    /// </para>
    /// <para>
    /// Rendered sizes cannot tell <c>body-md</c> from <c>font-sizes-100</c>, so these checks read
    /// the source.
    /// </para>
    /// </remarks>
    [TestFixture]
    class ComponentFontSizeTests
    {
        /// <summary>
        /// Stylesheets that may still read a <c>body-*</c> alias because they have no registry
        /// counterpart to match. Remove an entry once its stylesheet moves to numbered steps, and
        /// never add one for a component that exists in the registry.
        /// </summary>
        static readonly HashSet<string> k_UnityOnlyExceptions = new HashSet<string>
        {
            // Thread UI (author, content, unresolve text, attachment name) has no registry component.
            "ThreadMessage.uss",
            // The emoji grid sizes its glyphs, not text; the registry has no ReactionPicker.
            "ReactionPicker.uss",
        };

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void ComponentStylesheets_DoNotReadBodyAliases_ExceptUnityOnly()
        {
            var components = Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Components");
            var offenders = Directory.EnumerateFiles(components, "*.uss", SearchOption.TopDirectoryOnly)
                .Where(p => !k_UnityOnlyExceptions.Contains(Path.GetFileName(p)))
                .Where(p => File.ReadAllText(p).Contains("--appui-font-sizes-body-"))
                .Select(Path.GetFileName)
                .OrderBy(n => n)
                .ToList();

            Assert.IsEmpty(offenders,
                "These stylesheets read a --appui-font-sizes-body-* alias. Read the numbered step the "
                + "registry's text tier maps to instead (text-xs 75, text-sm 100, text-base 200): "
                + string.Join(", ", offenders));
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "Reads package source files, Editor only")]
        public void ExceptionList_OnlyNamesStylesheetsThatStillReadBodyAliases()
        {
            var components = Path.Combine(UssSource.PackageResourcesDirectory(), "Styles", "Components");
            foreach (var name in k_UnityOnlyExceptions)
            {
                var path = Path.Combine(components, name);
                Assert.IsTrue(File.Exists(path), $"{name} is listed as an exception but does not exist.");
                StringAssert.Contains("--appui-font-sizes-body-", File.ReadAllText(path),
                    $"{name} no longer reads a body-* alias; remove it from the exception list.");
            }
        }

        static IEnumerable<TestCaseData> RegistryTiers()
        {
            // file, selector, step: the step of the registry class the same text reads.
            yield return new TestCaseData("Button.uss", ".appui-button", 200);                 // button.tsx text-base
            yield return new TestCaseData("Button.uss", ".appui-button--size-sm", 100);        // button.tsx sm text-sm
            yield return new TestCaseData("ActionButton.uss", ".appui-actionbutton--size-sm", 75); // action-button.tsx sm text-xs
            yield return new TestCaseData("SplitButton.uss", ".appui-split-button--size-sm .appui-split-button__label", 100);
            yield return new TestCaseData("SplitButton.uss", ".appui-split-button--size-md .appui-split-button__label", 200);
            yield return new TestCaseData("Tabs.uss", ".appui-tabitem__label", 200);
            yield return new TestCaseData("Accordion.uss", ".appui-accordionitem__header", 200);
            yield return new TestCaseData("HelpBox.uss", ".appui-helpbox__message", 200);
            yield return new TestCaseData("HelpBox.uss", ".appui-helpbox--size-md > .appui-helpbox__message", 200);
            yield return new TestCaseData("TouchSlider.uss", ".appui-touchslider__label", 200);
            yield return new TestCaseData("PropertyGroup.uss", ".appui-property-group--level-section .appui-property-group__label", 200);
            yield return new TestCaseData("PropertyGroup.uss", ".appui-property-group--level-sub .appui-property-group__label", 100);
            yield return new TestCaseData("MarkdownView.uss", ".appui-markdown-view__paragraph", 200);
            yield return new TestCaseData("ObjectPicker.uss", ".appui-objectpicker__row__label", 200);
            yield return new TestCaseData("ObjectPickerPreview.uss", ".appui-objectpickerpreview__name", 200);
            yield return new TestCaseData("ObjectPickerPreview.uss", ".appui-objectpickerpreview__type", 100);
            yield return new TestCaseData("ObjectPickerPreview.uss", ".appui-objectpickerpreview__path", 75);
            yield return new TestCaseData("Attachment.uss", ".appui-attachment__title", 100);
            yield return new TestCaseData("Attachment.uss", ".appui-attachment__subtitle", 75);
            yield return new TestCaseData("Menu.uss", ".appui-menuitem__label", 200);          // menu-item.tsx text-base
            yield return new TestCaseData("Menu.uss", ".appui-menuitem__shortcut", 100);       // menu-item.tsx text-sm
            yield return new TestCaseData("Menu.uss", ".appui-menusection__title", 75);        // menu.tsx MenuSectionHeader text-xs
            yield return new TestCaseData("ChoiceList.uss", ".appui-choicelistitem", 200);
            yield return new TestCaseData("ChoiceList.uss", ".appui-choicelistitem__badge", 75);
            yield return new TestCaseData("ListViewItem.uss", ".appui-listview-item__title", 200); // list-view.tsx text-base
            yield return new TestCaseData("Badge.uss", ".appui-badge__label", 75);
            yield return new TestCaseData("Avatar.uss", ".appui-avatar--size-sm .appui-avatar__container", 75);
            yield return new TestCaseData("Avatar.uss", ".appui-avatar--size-md .appui-avatar__container", 100);
            yield return new TestCaseData("Avatar.uss", ".appui-avatar--size-lg .appui-avatar__container", 200);

            // Text that used to inherit the panel root's step and reads text-sm on the web, pinned so
            // the root's text-base does not grow it.
            yield return new TestCaseData("ActionButton.uss", ".appui-actionbutton", 100);     // action-button.tsx text-sm
            yield return new TestCaseData("Radio.uss", ".appui-radio__label", 100);            // label.tsx text-sm
            yield return new TestCaseData("HelpText.uss", ".appui-help-text", 100);            // help-text.tsx text-sm
            yield return new TestCaseData("Tooltip.uss", ".appui-tooltip__content", 100);      // tooltip.tsx text-sm
            yield return new TestCaseData("Dialog.uss", ".appui-dialog__content", 100);        // dialog.tsx description text-sm
            yield return new TestCaseData("Breadcrumbs.uss", ".appui-breadcrumb-separator", 100); // breadcrumbs.tsx separator text-sm

            // No web counterpart: pinned at the step they rendered at before the root moved.
            yield return new TestCaseData("MentionPicker.uss", ".appui-mention-picker__none", 100);
            yield return new TestCaseData("ThreadMessage.uss", ".appui-thread-reaction", 100);
            yield return new TestCaseData("Thumb.uss", ".appui-thumb__value-label-container", 100);
        }

        /// <summary>
        /// The panel root sets the size every unsized text inherits, and that is the registry's
        /// default body tier, <c>text-base</c>. At <c>font-sizes-100</c> every unsized label in UITK
        /// rendered a tier below the web.
        /// </summary>
        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void PanelRoot_ReadsTextBaseStep()
        {
            FieldTextSize.AssertFontSize("Context.uss", ".appui", 200);
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        [TestCaseSource(nameof(RegistryTiers))]
        public void Text_ReadsTheRegistryTierStep(string fileName, string selector, int step)
        {
            FieldTextSize.AssertFontSize(fileName, selector, step);
        }
    }
}
