using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(ColorField))]
    class ColorFieldTests : VisualElementTests<ColorField>
    {
        protected override string mainUssClassName => ColorField.ussClassName;

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:ColorField />",
            @"<appui:ColorField value=""#FF0000"" size=""M"" invalid=""false"" swatch-only=""false"" inline-picker=""false"" />",
            @"<appui:ColorField value=""#FF0000"" swatch-size=""M"" color-picker-type=""UnityEditor"" hdr=""true"" show-alpha=""false"" />",
        };

        [Test]
        public void Value_HdrColorAboveOne_DoesNotThrowAndKeepsValue()
        {
            var field = new ColorField { hdr = true };
            var hdrColor = new Color(2, 0, 0, 1);

            Assert.DoesNotThrow(() => field.value = hdrColor);
            Assert.AreEqual(hdrColor, field.value);
            Assert.DoesNotThrow(() => field.SetValueWithoutNotify(new Color(0, 3, 0, 1)));
        }
    }
}
