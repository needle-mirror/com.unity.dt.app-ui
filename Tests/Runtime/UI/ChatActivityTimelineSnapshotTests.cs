#if APPUI_UITF_PINNED_PANEL
using System.Collections.Generic;
using NUnit.Framework;
using Unity.AppUI.UI;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Unity.AppUI.Tests.UI
{
    /// <summary>
    /// Snapshot stories for <see cref="ChatActivityTimeline"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatActivityTimelineTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatActivityTimeline))]
    class ChatActivityTimelineSnapshotTests : UISnapshotTests<ChatActivityTimeline>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Collapsed", (ctx) => new ChatActivityTimeline { parts = SampleParts() });
                yield return new Story("Open", (ctx) => new ChatActivityTimeline { parts = SampleParts(), open = true });
                yield return new Story("Streaming", (ctx) => new ChatActivityTimeline
                {
                    parts = SampleParts(),
                    isStreaming = true,
                    open = true,
                    label = "Running tool",
                });
            }
        }


        static ChatActivityPart[] SampleParts() => new[]
        {
            new ChatActivityPart(ChatActivityPartKind.Thinking, "The jump force lives on the controller.", elapsedMs: 800),
            new ChatActivityPart(ChatActivityPartKind.Tool, "Read PlayerController.cs", elapsedMs: 4200),
            new ChatActivityPart(ChatActivityPartKind.Tool, "Write PlayerController.cs", pending: true),
        };
    }
}
#endif
