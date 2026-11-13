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
    [TestOf(typeof(DateField))]
    class DateFieldTests : VisualElementTests<DateField>
    {
        protected override string mainUssClassName => DateField.ussClassName;

        // Handed over to DateFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DateField
                {
                    value = new Date(2020, 1, 1)
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:DateField />",
            @"<appui:DateField value=""1998-07-12"" />",
        };

        /// <summary>
        /// The registry's <c>date-field</c> wraps <c>textFieldVariants</c>: <c>text-sm</c> at
        /// <c>sm</c> and <c>text-base</c> at <c>default</c> and <c>lg</c>.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, DateField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new DateField { size = size };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var value = field.Q(className: DateField.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, value, FieldTextSize.TextFieldTier(size), $"a {size} DateField's value");
                    FieldTextSize.AssertFits(stage, value, field, $"a {size} DateField's value");
                }
            }
        }

        [Test]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in FieldTextSize.sizes)
                FieldTextSize.AssertFontSize("DateField.uss",
                    $".appui-date-field--size-{suffix} .appui-date-field__input",
                    FieldTextSize.Step(FieldTextSize.TextFieldTier(size)));
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("DateField.uss"),
                "DateField reads the numbered steps the registry reads, never a body-* alias.");
        }
    }
}
