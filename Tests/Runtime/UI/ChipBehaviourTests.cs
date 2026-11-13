#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(Chip))]
    class ChipBehaviourTests : UIComponentTests<Chip>
    {
        static VisualElement DeleteButtonOf(Chip chip) =>
            chip.Q(name: Chip.deleteButtonUssClassName);

        [Test]
        public void Chip_Click_ShouldInvokeClickableClicked()
        {
            var chip = MountElement(new Chip { label = "Filter" });
            var notifications = 0;
            chip.clickable.clicked += () => notifications++;

            simulate.Click(chip);

            Assert.AreEqual(1, notifications, "Clicking the chip should invoke its clickable manipulator.");
        }

        [Test]
        public void Chip_SubmitKey_ShouldInvokeClickableClicked()
        {
            var chip = MountElement(new Chip { label = "Filter" });
            var notifications = 0;
            chip.clickable.clicked += () => notifications++;

            chip.Focus();
            simulate.FrameUpdate();
            simulate.ReturnKeyPress();

            Assert.AreEqual(1, notifications, "Submitting on a focused chip should invoke the clickable manipulator.");
        }

        [Test]
        public void Chip_ClickDeleteButton_ShouldInvokeDeleteClicked()
        {
            var chip = MountElement(new Chip { label = "Filter", deletable = true });
            var deleteClicks = 0;
            var chipClicks = 0;
            chip.delete.clicked += () => deleteClicks++;
            chip.clickable.clicked += () => chipClicks++;

            simulate.Click(DeleteButtonOf(chip));

            Assert.AreEqual(1, deleteClicks, "Clicking the delete button should invoke the delete manipulator.");
            Assert.AreEqual(0, chipClicks, "The delete button is a separate target and should not also click the chip.");
        }

        [Test]
        public void Chip_WhenDisabled_Click_ShouldNotInvokeClickableClicked()
        {
            var chip = MountElement(new Chip { label = "Filter" });
            var notifications = 0;
            chip.clickable.clicked += () => notifications++;
            chip.SetEnabled(false);
            simulate.FrameUpdate();

            simulate.Click(chip);

            Assert.AreEqual(0, notifications, "A disabled chip should not respond to a click.");
        }

        // ── Label truncation (registry #1460) ──────────────────────────────

        const string k_LongLabel = "A filter label far too long to fit in a narrow chip row";

        static TextElement LabelOf(Chip chip) => chip.Q<TextElement>(name: Chip.labelUssClassName);

        /// <summary>
        /// Re-parents the mounted chip into a host of the given width and lets it lay out.
        /// </summary>
        VisualElement HostChip(Chip chip, float width, FlexDirection direction)
        {
            var host = new VisualElement
            {
                style =
                {
                    width = width,
                    flexDirection = direction,
                    alignItems = Align.FlexStart,
                }
            };
            appUiPanel.Add(host);
            host.Add(chip);
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            simulate.FrameUpdate();
            return host;
        }

        [Test]
        public void Chip_LongLabel_InANarrowRow_TruncatesOnOneLine()
        {
            var chip = MountElement(new Chip { label = k_LongLabel, deletable = true });
            var host = HostChip(chip, 80f, FlexDirection.Row);
            var label = LabelOf(chip);

            Assert.LessOrEqual(chip.worldBound.xMax, host.worldBound.xMax + 0.5f,
                "The chip should stay inside its narrow parent rather than overflow it.");
            Assert.IsTrue(label.isElided, "A label that does not fit should be cut with an ellipsis.");
            Assert.AreEqual(UnityEngine.UIElements.TextOverflow.Ellipsis, label.resolvedStyle.textOverflow);
            Assert.LessOrEqual(label.layout.height, chip.layout.height + 0.5f,
                "The label should stay on one line inside the chip's fixed height.");
            // The delete button is nudged outward by a translate, so check its centre, not its edge.
            Assert.LessOrEqual(DeleteButtonOf(chip).worldBound.center.x, host.worldBound.xMax,
                "The delete button should stay reachable inside the parent.");
            Assert.LessOrEqual(label.worldBound.xMax, DeleteButtonOf(chip).worldBound.xMin + 0.5f,
                "The label should give way to the delete button rather than run under it.");
        }

        [Test]
        public void Chip_LongLabel_InANarrowColumn_IsBoundedByItsParent()
        {
            var chip = MountElement(new Chip { label = k_LongLabel });
            var host = HostChip(chip, 80f, FlexDirection.Column);

            Assert.LessOrEqual(chip.layout.width, host.layout.width + 0.5f,
                "max-width: 100% should bound the chip to its parent's width.");
            Assert.IsTrue(LabelOf(chip).isElided, "The bounded label should be cut with an ellipsis.");
        }

        [Test]
        public void Chip_InAShortColumn_KeepsItsHeight()
        {
            var reference = MountElement(new Chip { label = "Filter" });
            HostChip(reference, 200f, FlexDirection.Row);
            var height = reference.layout.height;
            Assert.Greater(height, 0f, "Positive control: the chip has a height.");

            var chip = MountElement(new Chip { label = "Filter" });
            var host = HostChip(chip, 200f, FlexDirection.Column);
            host.style.height = 4f;
            simulate.FrameUpdate();
            simulate.FrameUpdate();

            Assert.AreEqual(height, chip.layout.height, 0.5f,
                "Shrinking is for the width; a short column must not squash the chip's height.");
        }

        [Test]
        public void Chip_LongLabel_WithRoom_IsNotTruncated()
        {
            var chip = MountElement(new Chip { label = k_LongLabel });
            HostChip(chip, 1000f, FlexDirection.Row);

            Assert.IsFalse(LabelOf(chip).isElided, "Positive control: a label that fits is not elided.");
            Assert.Greater(chip.layout.width, 80f, "Positive control: the label is wider than the narrow host.");
        }
    }
}
#endif
