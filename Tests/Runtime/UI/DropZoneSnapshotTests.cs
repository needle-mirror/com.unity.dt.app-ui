#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="DropZone"/> on the com.unity.ui.test-framework harness.
    /// A bare DropZone hides its indicator and holds its content in an absolutely positioned frame,
    /// so it has no in-flow size and captures empty. Each story gives it a size, shows the
    /// indicator and adds a label.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(DropZone))]
    class DropZoneSnapshotTests : UISnapshotTests<DropZone>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", _ => Create("Drop files here", DragAndDropState.Default));
                yield return new Story("AcceptDrag", _ => Create("Release to drop", DragAndDropState.AcceptDrag));
                yield return new Story("RejectDrag", _ => Create("File type not supported", DragAndDropState.RejectDrag));
            }
        }

        static DropZone Create(string label, DragAndDropState state)
        {
            var dropZone = new DropZone
            {
                visibleIndicator = true,
                state = state,
                style = { width = 240, height = 120 },
            };
            dropZone.Add(new Text { text = label });
            return dropZone;
        }
    }
}
#endif
