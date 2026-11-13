using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    class NumericalFieldTests<T, U> : VisualElementTests<T>
        where T : NumericalField<U>, new()
        where U : struct, IComparable, IComparable<U>, IFormattable
    {
        [UnityTest]
        [Order(10)]
        public IEnumerator CanReceiveDragContextFromInputLabel()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var inputLabel = new InputLabel("Test Label") { draggable = true };
            var field = new T
            {
                acceptDragging = true
            };
            inputLabel.Add(field);
            panel.Add(inputLabel);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            // Simulate drag context
            var initialValue = field.value;
            inputLabel.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(10f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            // Value should have changed from the drag
            Assert.AreNotEqual(initialValue, field.value);
        }

        /// <summary>
        /// The registry's <c>numerical-field</c> reads <c>--appui-alias-field-padding-horizontal-{sm,md,lg}</c>
        /// by size (#1416). The three aliases resolve to the same step today, so only the source can
        /// tell a size that reads the wrong variant.
        /// </summary>
        [Test]
        [Order(12)]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void HorizontalInset_ComesFromTheFieldPaddingAlias_ForEverySize()
        {
            foreach (var suffix in new[] { "sm", "md", "lg" })
                UssSource.AssertHorizontalInset("NumericalField.uss", ".appui-numericalfield--size-" + suffix,
                    "--appui-alias-field-padding-horizontal-" + suffix);

            var root = UssSource.RuleBlock(UssSource.Read("NumericalField.uss"), ".appui-numericalfield");
            StringAssert.DoesNotContain("control-gap-md", root,
                "The base rule must not keep the old control-gap inset.");
        }

        /// <summary>
        /// The registry's <c>numerical-field</c> reads <c>text-sm</c> at <c>sm</c> and <c>text-base</c>
        /// at <c>default</c> and <c>lg</c>. Its suffix, UITK's unit, is <c>text-sm</c> at every size.
        /// </summary>
        [UnityTest]
        [Order(13)]
        public IEnumerator Text_RendersAtTheRegistrysStep_ForEverySizeAndDensity()
        {
            foreach (var density in FieldTextSize.densities)
            {
                var fields = new List<(Size size, T field)>();
                FieldTextSize.Stage stage = null;
                yield return FieldTextSize.Mount(m_TestUI.rootVisualElement, density, panel =>
                {
                    foreach (var (size, _) in FieldTextSize.sizes)
                    {
                        var field = new T { size = size, unit = "px" };
                        panel.Add(field);
                        fields.Add((size, field));
                    }
                }, s => stage = s);

                foreach (var (size, field) in fields)
                {
                    var value = field.Q(className: NumericalField<U>.inputUssClassName).Q<TextElement>();
                    FieldTextSize.AssertTier(stage, value, FieldTextSize.TextFieldTier(size),
                        $"a {size} {typeof(T).Name}'s value");
                    FieldTextSize.AssertTier(stage, field.Q(className: NumericalField<U>.unitUssClassName), "sm",
                        $"a {size} {typeof(T).Name}'s unit");
                    FieldTextSize.AssertFits(stage, value, field, $"a {size} {typeof(T).Name}'s value");
                }
            }
        }

        /// <summary>
        /// <c>body-lg</c> and <c>body-md</c> resolve to the same pixels as <c>font-sizes-200</c> and
        /// <c>-100</c>, so only the source can tell which a rule reads.
        /// </summary>
        [Test]
        [Order(14)]
        [ConditionalIgnore("IgnoreInPlayer", "The USS source is not shipped with a player build")]
        public void Text_ReadsTheNumberedFontSizeSteps()
        {
            foreach (var (size, suffix) in FieldTextSize.sizes)
                FieldTextSize.AssertFontSize("NumericalField.uss",
                    $".appui-numericalfield--size-{suffix} .appui-numericalfield__input",
                    FieldTextSize.Step(FieldTextSize.TextFieldTier(size)));
            FieldTextSize.AssertFontSize("NumericalField.uss", ".appui-numericalfield__unit", 100);
            StringAssert.DoesNotContain("font-sizes-body-", UssSource.Read("NumericalField.uss"),
                "NumericalField reads the numbered steps the registry reads, never a body-* alias.");
        }

        [Test]
        [Order(11)]
        public void GetIncrementFactorReturnsValidValue()
        {
            var field = new T();

            // Test that GetIncrementFactor returns a non-zero value
            var factor = field.InvokeGetIncrementFactor(default(U));
            Assert.Greater(factor, 0f);
        }
    }
}
