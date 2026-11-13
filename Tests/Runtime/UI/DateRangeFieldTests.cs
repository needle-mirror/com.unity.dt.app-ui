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
    [TestOf(typeof(DateRangeField))]
    class DateRangeFieldTests : VisualElementTests<DateRangeField>
    {
        protected override string mainUssClassName => DateRangeField.ussClassName;

        // Handed over to DateRangeFieldSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new DateRangeField
                {
                    value = new DateRange(new Date(2020, 1, 1), new Date(2020, 1, 2))
                });
                yield return new Story("DifferentMonths", (ctx) => new DateRangeField
                {
                    value = new DateRange(new Date(2020, 1, 1), new Date(2020, 2, 2))
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:DateRangeField />",
            @"<appui:DateRangeField value=""1998-07-12,1998-07-20"" />",
        };

        /// <summary>
        /// The registry's <c>daterange-field</c> wraps <c>textFieldVariants</c>: <c>text-sm</c> at
        /// <c>sm</c> and <c>text-base</c> at <c>default</c> and <c>lg</c>, for both ends. It shares
        /// DateField's classes and stylesheet.
        /// </summary>
        [UnityTest]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, DateRangeField field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new DateRangeField { size = size };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var inputs = field.Query(className: DateRangeField.inputUssClassName).ToList();
                    Assert.AreEqual(2, inputs.Count, "Expected a start and an end input.");
                    foreach (var input in inputs)
                    {
                        var value = input.Q<TextElement>();
                        FieldTextSize.AssertTier(stage, value, FieldTextSize.TextFieldTier(size),
                            $"a {size} DateRangeField's input");
                        FieldTextSize.AssertFits(stage, value, field, $"a {size} DateRangeField's input");
                    }
                }
            }
        }
    }
}
