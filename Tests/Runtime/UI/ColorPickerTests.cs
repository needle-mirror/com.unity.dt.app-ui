using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using TextField = Unity.AppUI.UI.TextField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorPicker))]
    class ColorPickerTests : VisualElementTests<ColorPicker>
    {
        protected override string mainUssClassName => ColorPicker.ussClassName;

        // Handed over to ColorPickerSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ColorPicker />",
            @"<appui:ColorPicker value=""#FF0000"" previous-value=""#00FF00"" show-alpha=""true"" show-hex=""true"" show-toolbar=""true"" />",
        };

        /// <summary>
        /// The registry's <c>color-picker</c> renders its value field as an <c>sm</c>
        /// <c>TextField</c>, so the hex field is <c>Sm</c> and reads <c>text-sm</c>.
        /// </summary>
        [UnityTest]
        public IEnumerator HexField_IsSmall_AsTheRegistrysValueField()
        {
            foreach (var density in FieldTextSize.densities)
            {
                ColorPicker picker = null;
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    picker = new ColorPicker { value = Color.white, showHex = true };
                    panel.Add(picker);
                }, s => stage = s);

                var hex = picker.Q<TextField>(className: ColorPicker.hexFieldUssClassName);
                Assert.AreEqual(Size.Sm, hex.size, "The hex field must be Sm, as the registry's value field is.");
                var text = hex.Q<UnityEngine.UIElements.TextField>(className: TextField.inputUssClassName)
                    .Q<TextElement>();
                FieldTextSize.AssertTier(stage, text, "sm", "the ColorPicker hex field");
                FieldTextSize.AssertFitsWidth(stage, text, text.parent, "the ColorPicker hex field");
            }
        }

        /// <summary>
        /// The channel sliders' value fields sit in a fixed share of a fixed-width picker, so their
        /// text must still fit sideways at the field text size.
        /// </summary>
        [UnityTest]
        public IEnumerator ChannelValueFields_FitTheirText()
        {
            foreach (var density in FieldTextSize.densities)
            {
                ColorPicker picker = null;
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    picker = new ColorPicker { value = Color.white };
                    panel.Add(picker);
                }, s => stage = s);

                var checkedAny = false;
                foreach (var field in picker.Query(className: Unity.AppUI.UI.SliderInt.inputFieldUssClassName).ToList())
                {
                    if (field.resolvedStyle.display == DisplayStyle.None || field.layout.width <= 0f)
                        continue;
                    var text = field.Q(className: NumericalField<int>.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertFitsWidth(stage, text, text.parent, "a ColorPicker channel value");
                    checkedAny = true;
                }
                Assert.IsTrue(checkedAny, $"{density}: no visible channel value field was found.");
            }
        }
    }
}
