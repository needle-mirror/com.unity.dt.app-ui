#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using System.Collections;
using NUnit.Framework;
using Unity.AppUI.Core;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ConfirmDialog"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ConfirmDialogTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ConfirmDialog))]
    class ConfirmDialogSnapshotTests : UISnapshotTests<ConfirmDialog>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Default", ctx => new ConfirmDialog
                {
                    title = "Discard changes?",
                    description = "Your edits will be lost.",
                });
                yield return new Story("Destructive", ctx => new ConfirmDialog
                {
                    title = "Delete asset?",
                    description = "This action cannot be undone.",
                    confirmLabel = "Delete",
                    destructive = true,
                });
            }
        }
    }
}
#endif
