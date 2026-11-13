#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatStreamingIndicator"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatStreamingIndicatorTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatStreamingIndicator))]
    class ChatStreamingIndicatorSnapshotTests : UISnapshotTests<ChatStreamingIndicator>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Streaming", (ctx) => new ChatStreamingIndicator { verbIndex = 0 });
                yield return new Story("WithStats", (ctx) => new ChatStreamingIndicator
                {
                    verbIndex = 0,
                    stats = "2 tools • 1.2k tokens",
                });
                yield return new Story("Completed", (ctx) => new ChatStreamingIndicator
                {
                    verbIndex = 0,
                    durationMs = 6400,
                    completed = true,
                });
            }
        }
    }
}
#endif
