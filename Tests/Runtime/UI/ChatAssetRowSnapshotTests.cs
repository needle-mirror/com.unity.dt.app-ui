#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatAssetRow"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatAssetRowTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatAssetRow))]
    class ChatAssetRowSnapshotTests : UISnapshotTests<ChatAssetRow>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Minimal", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                });
                yield return new Story("Full", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                    packLabel = "Sci-Fi Props Vol. 2",
                    creatorLabel = "Nebula Studios",
                    priceLabel = "$12",
                    details = true,
                    state = AssetRowState.Added,
                    statusText = "Added to the scene",
                });
                yield return new Story("Failed", (ctx) => new ChatAssetRow
                {
                    title = "Sci-Fi Crate",
                    sourceLabel = "Asset Store",
                    state = AssetRowState.Failed,
                    statusText = "Download failed",
                    statusTone = AssetRowStatusTone.Destructive,
                });
            }
        }
    }
}
#endif
