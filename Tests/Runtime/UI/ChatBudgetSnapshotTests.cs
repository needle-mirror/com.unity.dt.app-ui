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
    /// Snapshot stories for <see cref="ChatBudget"/> on the com.unity.ui.test-framework harness.
    /// The list mirrors <see cref="ChatBudgetTests"/> exactly, which is what makes a capture from
    /// either harness comparable image for image.
    /// </summary>
    [TestFixture]
    [TestOf(typeof(ChatBudget))]
    class ChatBudgetSnapshotTests : UISnapshotTests<ChatBudget>
    {
        protected override IEnumerable<Story> stories
        {
            get
            {
                yield return new Story("Partial", (ctx) => new ChatBudget { spent = 120, total = 200 });
                yield return new Story("Exhausted", (ctx) => new ChatBudget { spent = 200, total = 200 });
            }
        }
    }
}
#endif
