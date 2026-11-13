#if APPUI_UITF_PRESENT
using NUnit.Framework;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    [TestFixture]
    [TestOf(typeof(RectAnchorPresets))]
    class RectAnchorPresetsBehaviourTests : UIComponentTests<RectAnchorPresets>
    {
        [Test]
        public void RectAnchorPresets_ClickCell_ShouldInvokePickedWithMatchingAnchors()
        {
            var presets = MountElement(new RectAnchorPresets());
            RectAnchorPickEvent? picked = null;
            presets.picked += evt => picked = evt;

            // Top-left cell: row 0 (top), column 0 (left).
            var cell = presets.GetCell(0, 0);

            simulate.Click(cell);

            Assert.IsTrue(picked.HasValue, "Clicking a cell should invoke the picked event.");
            Assert.AreEqual(RectAnchorPresets.columns[0].x, picked.Value.anchorMin.x, "The picked anchorMin.x should match the top-left cell's column span.");
            Assert.AreEqual(RectAnchorPresets.rows[0].x, picked.Value.anchorMin.y, "The picked anchorMin.y should match the top-left cell's row span.");
        }

        [Test]
        public void RectAnchorPresets_ClickCell_ShouldSelectMatchingCell()
        {
            var presets = MountElement(new RectAnchorPresets());
            presets.picked += evt =>
            {
                presets.anchorMin = evt.anchorMin;
                presets.anchorMax = evt.anchorMax;
            };
            Assert.IsFalse(presets.IsSelected(1, 1), "The centre cell should not start selected.");

            // Centre cell: row 1 (middle), column 1 (centre).
            simulate.Click(presets.GetCell(1, 1));

            Assert.IsTrue(presets.IsSelected(1, 1), "Setting the anchors from the picked event should select the matching cell.");
        }

        [Test]
        public void RectAnchorPresets_ClickWithShift_ShouldReportSetPivot()
        {
            var presets = MountElement(new RectAnchorPresets());
            RectAnchorPickEvent? picked = null;
            presets.picked += evt => picked = evt;

            simulate.Click(presets.GetCell(2, 2), modifiers: UnityEngine.EventModifiers.Shift);

            Assert.IsTrue(picked.HasValue, "Clicking should invoke the picked event.");
            Assert.IsTrue(picked.Value.setPivot, "Holding Shift while picking should report setPivot.");
        }
    }
}
#endif
