using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using FloatField = Unity.AppUI.UI.FloatField;
using IntField = Unity.AppUI.UI.IntField;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(NumericPropertyFloat))]
    class NumericPropertyFloatTests : VisualElementTests<NumericPropertyFloat>
    {
        const string k_UssPath = "Packages/com.unity.dt.app-ui/PackageResources/Styles/Components/NumericProperty.uss";

        protected override string mainUssClassName => NumericPropertyFloat.ussClassName;

        // Handed over to NumericPropertyFloatSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new NumericPropertyFloat
                {
                    label = "Opacity",
                    value = 0.5f,
                    lowValue = 0f,
                    highValue = 1f
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:NumericPropertyFloat />",
            @"<appui:NumericPropertyFloat label=""Opacity"" value=""0.5"" low-value=""0"" high-value=""1"" cap-value=""true"" />",
            @"<appui:NumericPropertyFloat label=""Scale"" icon=""ruler"" size=""Lg"" unit=""m"" />",
        };

        [Test]
        [Order(10)]
        public void NumericProperty_IsAnInputLabel_AndOwnsANumericalField()
        {
            var row = new NumericPropertyFloat();
            Assert.IsInstanceOf<InputLabel>(row, "NumericProperty must reuse InputLabel's label/input layout.");
            Assert.IsNotNull(row.field);
            Assert.IsInstanceOf<FloatField>(row.field);
            Assert.IsTrue(row.ClassListContains(InputLabel.ussClassName));
            Assert.IsTrue(row.ClassListContains(NumericPropertyFloat.ussClassName));
        }

        [Test]
        [Order(11)]
        public void Draggable_IsOnByDefault()
        {
            // The whole reason the type exists: the scrub handle cannot be forgotten. Both halves
            // matter — InputLabel.draggable publishes the DragContext, NumericalField.acceptDragging
            // (off by default) is what makes the field listen to it.
            var row = new NumericPropertyFloat();
            Assert.IsTrue(row.draggable, "The label must be draggable out of the box.");
            Assert.IsTrue(row.field.acceptDragging, "The field must accept the label's drag out of the box.");
        }

        [Test]
        [Order(12)]
        public void Value_RoundTripsThroughTheField()
        {
            var row = new NumericPropertyFloat { value = 3.5f };
            Assert.AreEqual(3.5f, row.value);
            Assert.AreEqual(3.5f, row.field.value);

            row.field.value = 7f;
            Assert.AreEqual(7f, row.value);
        }

        [Test]
        [Order(13)]
        public void Bounds_ClampTheValue()
        {
            var row = new NumericPropertyFloat { lowValue = 0f, highValue = 1f };
            row.value = 5f;
            Assert.AreEqual(1f, row.value);
            row.value = -5f;
            Assert.AreEqual(0f, row.value);
        }

        [UnityTest]
        [Order(14)]
        public IEnumerator ChangeEvent_IsRetargetedAtTheRow()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var row = new NumericPropertyFloat { label = "Opacity" };
            panel.Add(row);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            var received = 0;
            VisualElement target = null;
            row.RegisterValueChangedCallback(evt =>
            {
                received++;
                target = evt.target as VisualElement;
            });

            row.value = 2f;

            yield return null;

            Assert.AreEqual(1, received, "The row must raise exactly one change event.");
            Assert.AreSame(row, target, "The change event must be retargeted at the row, not its inner field.");
        }

        [UnityTest]
        [Order(15)]
        public IEnumerator LabelDrag_ScrubsTheValue()
        {
            m_TestUI.rootVisualElement.Clear();
            var panel = new Panel();
            var row = new NumericPropertyFloat { label = "Opacity", value = 0f };
            panel.Add(row);
            m_TestUI.rootVisualElement.Add(panel);

            yield return null;

            row.ProvideContext(new DragContext(DragPhase.Started, Vector3.zero, DeltaSpeed.Normal));

            yield return null;

            row.ProvideContext(new DragContext(DragPhase.Dragging, new Vector3(40f, 0, 0), DeltaSpeed.Normal));

            yield return null;

            Assert.AreNotEqual(0f, row.value, "Dragging the label must scrub the field's value.");
        }

        [Test]
        [Order(16)]
        public void Size_ForwardsToTheField_AndDefaultsToMd()
        {
            var row = new NumericPropertyFloat();
            Assert.AreEqual(Size.Md, row.size);
            Assert.IsTrue(row.field.ClassListContains(NumericalField<float>.ussClassName + "--size-md"));

            row.size = Size.Sm;
            Assert.AreEqual(Size.Sm, row.field.size);
            Assert.IsTrue(row.field.ClassListContains(NumericalField<float>.ussClassName + "--size-sm"));
            Assert.IsFalse(row.field.ClassListContains(NumericalField<float>.ussClassName + "--size-md"),
                "The stale size class must be removed.");
        }

        [Test]
        [Order(17)]
        [ConditionalIgnore("IgnoreInPlayer", "USS source is not available in a player build")]
        public void Uss_UsesTokensOnly_NoLiteralColoursOrPixels()
        {
            var fullPath = Path.GetFullPath(k_UssPath);
            var uss = File.ReadAllText(fullPath);
            Assert.IsFalse(Regex.IsMatch(uss, @"#[0-9a-fA-F]{3,8}\b"), "NumericProperty.uss must not contain a literal hex colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"\brgba?\("), "NumericProperty.uss must not contain a literal rgb()/rgba() colour.");
            Assert.IsFalse(Regex.IsMatch(uss, @"--appui-sizing-\d"), "NumericProperty.uss must not grab a raw --appui-sizing step.");
        }
    }

    [TestFixture]
    [TestOf(typeof(NumericPropertyInt))]
    class NumericPropertyIntTests : VisualElementTests<NumericPropertyInt>
    {
        // Handed over to NumericPropertyIntSnapshotTests where the framework harness compiles; see
        // VisualElementTests.capturesSnapshotsElsewhere.
        protected override bool capturesSnapshotsElsewhere => true;

        protected override string mainUssClassName => NumericPropertyInt.ussClassName;

        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", (ctx) => new NumericPropertyInt
                {
                    label = "Subdivisions",
                    value = 4,
                    lowValue = 1
                });
            }
        }

        protected override IEnumerable<string> uxmlTestCases => new[]
        {
            @"<appui:NumericPropertyInt />",
            @"<appui:NumericPropertyInt label=""Subdivisions"" value=""4"" low-value=""1"" />",
        };

        [Test]
        [Order(10)]
        public void IntFlavour_UsesAnIntField_AndIsDraggable()
        {
            var row = new NumericPropertyInt { value = 4 };
            Assert.IsInstanceOf<IntField>(row.field);
            Assert.AreEqual(4, row.value);
            Assert.IsTrue(row.draggable);
            Assert.IsTrue(row.field.acceptDragging);
        }
    }
}
